using Apllication.DTOs.TaiLieuDuAn;
using Apllication.IRepositories;
using Apllication.IService;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Apllication.Service
{
    public class TaiLieuDuAnService : ITaiLieuDuAnService
    {
        private readonly ITaiLieuDuAnRepository _repository;
        private readonly IHostEnvironment _env;
        private readonly IGiaoViecAIService _giaoViecAiService;
        private readonly ICongViecRepository _congViecRepo;
        private readonly IKyNangRepository _kyNangRepo;
        private readonly IQuyTacGiaoViecAIRepository _ruleRepo;

        public TaiLieuDuAnService(
            ITaiLieuDuAnRepository repository, 
            IHostEnvironment env,
            IGiaoViecAIService giaoViecAiService,
            ICongViecRepository congViecRepo,
            IKyNangRepository kyNangRepo,
            IQuyTacGiaoViecAIRepository ruleRepo)
        {
            _repository = repository;
            _env = env;
            _giaoViecAiService = giaoViecAiService;
            _congViecRepo = congViecRepo;
            _kyNangRepo = kyNangRepo;
            _ruleRepo = ruleRepo;
        }

        public async Task<TaiLieuDuAnDto> UploadAsync(int duAnId, IFormFile file, int userId)
        {
            string uploadPath = Path.Combine(_env.ContentRootPath, "uploads", "documents");
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            string fileExtension = Path.GetExtension(file.FileName);
            string uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            string filePath = Path.Combine(uploadPath, uniqueFileName);

            using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var taiLieu = new TaiLieuDuAn
            {
                DuAnId = duAnId,
                FileName = file.FileName,
                FilePath = filePath,
                FileType = fileExtension,
                UploadedBy = userId,
                UploadedAt = DateTime.UtcNow,
                IsProcessed = false
            };

            var ketQua = await _repository.AddAsync(taiLieu);

            return MapToDto(ketQua);
        }

        public async Task<bool> ProcessWithAiAsync(int taiLieuId)
        {
            var taiLieu = await _repository.GetByIdAsync(taiLieuId);
            if (taiLieu == null || taiLieu.IsProcessed) return false;

            try
            {
                if (!File.Exists(taiLieu.FilePath)) return false;

                using (var stream = new System.IO.FileStream(taiLieu.FilePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    var doc = new NPOI.XWPF.UserModel.XWPFDocument(stream);
                    var tables = doc.Tables;
                    var moduleMapping = new Dictionary<string, string>();
                    var moduleDescMapping = new Dictionary<string, string>();

                    // LƯỢT 1: Tìm và nạp Quy định Module (Bảng 1)
                    foreach (var table in tables)
                    {
                        var firstRow = table.GetRow(0);
                        if (firstRow == null || firstRow.GetTableCells().Count < 2) continue;
                        var headers = firstRow.GetTableCells().Select(c => NormalizeText(c.GetText())).ToList();

                        int mCodeIdx = headers.FindIndex(h => h == "module" || h.Contains("mã module"));
                        int mNameIdx = headers.FindIndex(h => h.Contains("tên module") || h.Contains("ý nghĩa"));
                        int mDescIdx = headers.FindIndex(h => h.Contains("mô tả") || h.Contains("description") || h.Contains("chi tiết"));

                        if (mCodeIdx >= 0 && mNameIdx >= 0 && mCodeIdx != mNameIdx)
                        {
                            for (int i = 1; i < table.Rows.Count; i++)
                            {
                                var row = table.GetRow(i);
                                var cells = row.GetTableCells();
                                if (cells.Count <= Math.Max(mCodeIdx, mNameIdx)) continue;
                                
                                string code = cells[mCodeIdx].GetText();
                                string name = cells[mNameIdx].GetText().Trim();
                                string desc = (mDescIdx >= 0 && mDescIdx < cells.Count) ? cells[mDescIdx].GetText().Trim() : "";
                                
                                if (!string.IsNullOrEmpty(code))
                                {
                                    string normalizedCode = NormalizeModuleCode(code);
                                    moduleMapping[normalizedCode] = name;
                                    if (!string.IsNullOrEmpty(desc))
                                    {
                                        moduleDescMapping[normalizedCode] = desc;
                                    }
                                }
                            }
                        }
                    }

                    // LƯỢT 2: Bóc tách danh sách công việc (Bảng 2)
                    foreach (var table in tables)
                    {
                        var firstRow = table.GetRow(0);
                        if (firstRow == null || firstRow.GetTableCells().Count < 3) continue;

                        var headers = firstRow.GetTableCells().Select(c => NormalizeText(c.GetText())).ToList();

                        bool isTaskTable = (headers.Any(h => h.Contains("stt") || h.Contains("module"))) && 
                                           (headers.Any(h => h.Contains("tên công việc") || h.Contains("title") || h.Contains("nhiệm vụ")));

                        if (isTaskTable)
                        {
                            string currentModuleName = "";
                            var sprintCache = new Dictionary<string, int>();
                            var tasksInTable = new List<(CongViec Task, string DepStr)>(); 

                            // Xác định chỉ mục cột dựa trên tiêu đề (Headers)
                            int sttIdx = headers.FindIndex(h => h.Contains("stt") || h.Contains("id") || h == "no.");
                            int moduleIdx = headers.FindIndex(h => h == "module" || h.Contains("mã module") || h.Contains("sprint"));
                            int titleIdx = headers.FindIndex(h => h.Contains("tên công việc") || h.Contains("title") || h.Contains("nhiệm vụ") || h.Contains("công việc"));
                            int descIdx = headers.FindIndex(h => h.Contains("mô tả") || h.Contains("description"));
                            int typeIdx = headers.FindIndex(h => h.Contains("loại") || h.Contains("type"));
                            int skillIdx = headers.FindIndex(h => h.Contains("kỹ năng") || h.Contains("skill") || h.Contains("công nghệ"));
                            int priorityIdx = headers.FindIndex(h => h.Contains("ưu tiên") || h.Contains("priority"));
                            int depIdx = headers.FindIndex(h => h.Contains("phụ thuộc") || h.Contains("dependency") || h.Contains("tiền đề"));

                            // Lấy các tham số cấu hình AI cho thời gian ước tính
                            var rules = await _ruleRepo.GetAllActiveRulesAsync();
                            double hHigh = GetRuleValue(rules, "DEFAULT_HOURS_HIGH", 16);
                            double hMed = GetRuleValue(rules, "DEFAULT_HOURS_MEDIUM", 8);
                            double hLow = GetRuleValue(rules, "DEFAULT_HOURS_LOW", 4);

                            for (int i = 1; i < table.Rows.Count; i++)
                            {
                                var row = table.GetRow(i);
                                var cells = row.GetTableCells();
                                if (cells.Count < 2) continue;

                                // Lấy Module Name
                                string moduleName = (moduleIdx >= 0 && moduleIdx < cells.Count) ? cells[moduleIdx].GetText().Trim() : "";
                                if (!string.IsNullOrEmpty(moduleName)) currentModuleName = moduleName;

                                // Lấy thông tin khác dựa trên index đã tìm thấy
                                var title = (titleIdx >= 0 && titleIdx < cells.Count) ? cells[titleIdx].GetText().Trim() : "";
                                var desc = (descIdx >= 0 && descIdx < cells.Count) ? cells[descIdx].GetText().Trim() : "";
                                var typeStr = (typeIdx >= 0 && typeIdx < cells.Count) ? cells[typeIdx].GetText().Trim() : "";
                                var skillStr = (skillIdx >= 0 && skillIdx < cells.Count) ? cells[skillIdx].GetText().Trim() : "";
                                var priorityStr = (priorityIdx >= 0 && priorityIdx < cells.Count) ? cells[priorityIdx].GetText().Trim() : "";
                                var dependencyStr = (depIdx >= 0 && depIdx < cells.Count) ? cells[depIdx].GetText().Trim() : "";
                                var positionStr = (sttIdx >= 0 && sttIdx < cells.Count) ? cells[sttIdx].GetText().Trim() : "0";

                                if (string.IsNullOrEmpty(title)) continue;

                                double estimatedHours = hMed;
                                var normalizedPriority = priorityStr.ToLower();
                                if (normalizedPriority.Contains("high") || normalizedPriority.Contains("cao") || normalizedPriority.Contains("urgent") || normalizedPriority.Contains("khẩn")) {
                                    estimatedHours = hHigh;
                                } else if (normalizedPriority.Contains("low") || normalizedPriority.Contains("thấp") || normalizedPriority.Contains("small")) {
                                    estimatedHours = hLow; 
                                } else {
                                    estimatedHours = hMed;
                                }

                                int.TryParse(new string(positionStr.Where(char.IsDigit).ToArray()), out int viTri);

                                int? sprintId = null;
                                if (!string.IsNullOrEmpty(currentModuleName))
                                {
                                    string normalizedCurrent = NormalizeModuleCode(currentModuleName);
                                    string fullSprintName = moduleMapping.ContainsKey(normalizedCurrent) 
                                        ? moduleMapping[normalizedCurrent] : currentModuleName;
                                    string? sprintDesc = moduleDescMapping.ContainsKey(normalizedCurrent)
                                        ? moduleDescMapping[normalizedCurrent] : null;
 
                                    if (!sprintCache.TryGetValue(fullSprintName, out int sId))
                                    {
                                        var Sprints = await _giaoViecAiService.GetOrCreateSprintByModuleNameAsync(taiLieu.DuAnId, fullSprintName, sprintDesc);
                                        sId = Sprints.Id;
                                        sprintCache[fullSprintName] = sId;
                                    }
                                    sprintId = sId;
                                }

                                var loai = MapLoaiCongViec(typeStr);
                                var task = new CongViec
                                {
                                    DuAnId = taiLieu.DuAnId,
                                    SprintId = sprintId,
                                    ViTri = viTri,
                                    TieuDe = title,
                                    MoTa = desc,
                                    LoaiCongViec = loai,
                                    DoUuTien = MapDoUuTien(priorityStr),
                                    ThoiGianUocTinh = estimatedHours,
                                    TrangThai = TrangThaiCongViec.Todo,
                                    PhuongThucGiaoViec = PhuongThucGiaoViec.AI,
                                    CreatedAt = DateTime.UtcNow,
                                    CreatedBy = taiLieu.UploadedBy,
                                    AiReasoning = !string.IsNullOrEmpty(currentModuleName) ? $"Module: {currentModuleName}. " : ""
                                };

                                if (!string.IsNullOrEmpty(skillStr))
                                {
                                    var skillNames = skillStr.Split(',').Select(s => s.Trim());
                                    foreach (var sName in skillNames)
                                    {
                                        var kynang = await _kyNangRepo.GetByNameAsync(sName);
                                        if (kynang != null)
                                        {
                                            task.YeuCauCongViecs.Add(new YeuCauCongViec { KyNangId = kynang.Id, MucDoYeuCau = 3 });
                                        }
                                    }
                                }
                                tasksInTable.Add((task, dependencyStr));
                            }

                            foreach (var item in tasksInTable) await _congViecRepo.AddAsync(item.Task);

                            for (int idx = 0; idx < tasksInTable.Count; idx++)
                            {
                                var depStr = tasksInTable[idx].DepStr;

                                // Phụ thuộc = "0", rỗng, hoặc "-" nghĩa là task ĐỘC LẬP (không phụ thuộc ai).
                                // → Không tạo dependency record. AI sẽ ưu tiên giao các task này trước.
                                if (string.IsNullOrEmpty(depStr) 
                                    || depStr.Trim() == "0" 
                                    || depStr.Trim() == "-") continue;

                                CongViec? predecessor = null;
                                string sttOnly = new string(depStr.Where(char.IsDigit).ToArray());

                                if (!string.IsNullOrEmpty(sttOnly) && (depStr.ToLower().Contains("task") || int.TryParse(depStr.Trim(), out _)))
                                {
                                    if (int.TryParse(sttOnly, out int stt) && stt > 0 && stt <= tasksInTable.Count)
                                        predecessor = tasksInTable[stt - 1].Task;
                                }

                                if (predecessor == null)
                                {
                                    predecessor = tasksInTable.FirstOrDefault(t => t.Task.TieuDe.Equals(depStr, StringComparison.OrdinalIgnoreCase)).Task;
                                }

                                if (predecessor != null && predecessor != tasksInTable[idx].Task)
                                {
                                    tasksInTable[idx].Task.Dependencies.Add(new PhuThuocCongViec { TaskId = tasksInTable[idx].Task.Id, DependsOnTaskId = predecessor.Id });
                                    await _congViecRepo.UpdateAsync(tasksInTable[idx].Task);
                                }
                            }
                        }
                    }
                }

                await _giaoViecAiService.TuDongGiaoViecDuAnAsync(taiLieu.DuAnId);
                taiLieu.IsProcessed = true;
                return await _repository.UpdateAsync(taiLieu);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private string NormalizeText(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            // Xử lý ký tự khoảng trắng đặc biệt trong Word (Non-breaking space, v.v.)
            return text.Replace("\u00A0", " ").Replace("\u200B", "").Trim().ToLower();
        }

        private string NormalizeModuleCode(string code)
        {
            if (string.IsNullOrEmpty(code)) return "";
            string normalized = NormalizeText(code);
            // Bỏ phần trong ngoặc (Gộp), (Merge)... 
            normalized = normalized.Split('(')[0].Trim();
            return normalized;
        }

        private LoaiCongViec MapLoaiCongViec(string text)
        {
            text = text.ToLower();
            if (text.Contains("back")) return LoaiCongViec.Backend;
            if (text.Contains("front")) return LoaiCongViec.Frontend;
            if (text.Contains("test")) return LoaiCongViec.Tester;
            if (text.Contains("ux") || text.Contains("ui")) return LoaiCongViec.UIUX;
            if (text.Contains("dev")) return LoaiCongViec.DevOps;
            return LoaiCongViec.Fullstack;
        }

        private DoUuTien MapDoUuTien(string text)
        {
            text = text.ToLower();
            if (text.Contains("high") || text.Contains("cao") || text.Contains("urgent") || text.Contains("khẩn")) return DoUuTien.High;
            if (text.Contains("low") || text.Contains("thấp") || text.Contains("nhỏ") || text.Contains("small")) return DoUuTien.Low;
            return DoUuTien.Medium;
        }

        private double GetRuleValue(IEnumerable<QuyTacGiaoViecAI> rules, string code, double defaultValue)
        {
            var rule = rules.FirstOrDefault(r => r.MaQuyTac == code);
            if (rule != null && double.TryParse(rule.GiaTri, out double val)) return val;
            return defaultValue;
        }

        public async Task<IEnumerable<TaiLieuDuAnDto>> GetByProjectIdAsync(int projectId)
        {
            var ds = await _repository.GetByProjectIdAsync(projectId);
            return ds.Select(MapToDto);
        }

        private TaiLieuDuAnDto MapToDto(TaiLieuDuAn t)
        {
            return new TaiLieuDuAnDto
            {
                Id = t.Id,
                DuAnId = t.DuAnId,
                FileName = t.FileName,
                FilePath = t.FilePath,
                FileType = t.FileType,
                UploadedBy = t.UploadedBy,
                UploadAt = t.UploadedAt,
                IsProcessed = t.IsProcessed
            };
        }
    }
}
