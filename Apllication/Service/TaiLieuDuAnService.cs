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

        public TaiLieuDuAnService(
            ITaiLieuDuAnRepository repository, 
            IHostEnvironment env,
            IGiaoViecAIService giaoViecAiService,
            ICongViecRepository congViecRepo,
            IKyNangRepository kyNangRepo)
        {
            _repository = repository;
            _env = env;
            _giaoViecAiService = giaoViecAiService;
            _congViecRepo = congViecRepo;
            _kyNangRepo = kyNangRepo;
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

            using (var stream = new FileStream(filePath, FileMode.Create))
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

                using (var stream = new FileStream(taiLieu.FilePath, FileMode.Open, FileAccess.Read))
                {
                    var doc = new NPOI.XWPF.UserModel.XWPFDocument(stream);
                    var tables = doc.Tables;
                    var moduleMapping = new Dictionary<string, string>();

                    foreach (var table in tables)
                    {
                        var firstRow = table.GetRow(0);
                        if (firstRow == null || firstRow.GetTableCells().Count < 3) continue;

                        var headers = firstRow.GetTableCells().Select(c => c.GetText().Trim().ToLower()).ToList();

                        if (headers.Contains("mã module") && (headers.Contains("tên module") || headers.Contains("tên đầy đủ")))
                        {
                            for (int i = 1; i < table.Rows.Count; i++)
                            {
                                var row = table.GetRow(i);
                                var cells = row.GetTableCells();
                                if (cells.Count < 2) continue;
                                moduleMapping[cells[0].GetText().Trim()] = cells[1].GetText().Trim();
                            }
                            continue;
                        }

                        bool isTaskTable = (headers.Any(h => h.Contains("stt") || h.Contains("module"))) && 
                                           (headers.Any(h => h.Contains("tên công việc") || h.Contains("title") || h.Contains("nhiệm vụ")));

                        if (isTaskTable)
                        {
                            string currentModuleName = "";
                            var sprintCache = new Dictionary<string, int>();
                            var tasksInTable = new List<(CongViec Task, string DepStr)>(); 

                            for (int i = 1; i < table.Rows.Count; i++)
                            {
                                var row = table.GetRow(i);
                                var cells = row.GetTableCells();
                                if (cells.Count < 3) continue;

                                var moduleName = cells[0].GetText().Trim();
                                if (!string.IsNullOrEmpty(moduleName)) currentModuleName = moduleName;

                                var title = cells[1].GetText().Trim();
                                var desc = cells[2].GetText().Trim();
                                var typeStr = cells.Count > 3 ? cells[3].GetText().Trim() : "";
                                var skillStr = cells.Count > 4 ? cells[4].GetText().Trim() : "";
                                var priorityStr = cells.Count > 5 ? cells[5].GetText().Trim() : "";
                                var dependencyStr = cells.Count > 6 ? cells[6].GetText().Trim() : "";
                                var positionStr = cells.Count > 0 ? cells[0].GetText().Trim() : "0";

                                if (string.IsNullOrEmpty(title)) continue;

                                double estimatedHours = 8;
                                int storyPoints = 2;

                                var normalizedPriority = priorityStr.ToLower();
                                if (normalizedPriority.Contains("high") || normalizedPriority.Contains("cao")) {
                                    estimatedHours = 16;
                                    storyPoints = 5;
                                } else if (normalizedPriority.Contains("small") || normalizedPriority.Contains("thấp")) {
                                    estimatedHours = 4;
                                    storyPoints = 1;
                                } else {
                                    estimatedHours = 8;
                                    storyPoints = 3;
                                }

                                int.TryParse(new string(positionStr.Where(char.IsDigit).ToArray()), out int viTri);

                                int? sprintId = null;
                                if (!string.IsNullOrEmpty(currentModuleName))
                                {
                                    string fullSprintName = moduleMapping.ContainsKey(currentModuleName) 
                                        ? moduleMapping[currentModuleName] : currentModuleName;

                                    if (!sprintCache.TryGetValue(fullSprintName, out int sId))
                                    {
                                        var Sprints = await _giaoViecAiService.GetOrCreateSprintByModuleNameAsync(taiLieu.DuAnId, fullSprintName);
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
                                    StoryPoints = storyPoints,
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
                                if (string.IsNullOrEmpty(depStr)) continue;

                                CongViec predecessor = null;
                                if (depStr.ToLower().Contains("task"))
                                {
                                    var sttStr = new string(depStr.Where(char.IsDigit).ToArray());
                                    if (int.TryParse(sttStr, out int stt) && stt > 0 && stt <= tasksInTable.Count)
                                        predecessor = tasksInTable[stt - 1].Task;
                                }
                                else predecessor = tasksInTable.FirstOrDefault(t => t.Task.TieuDe.Equals(depStr, StringComparison.OrdinalIgnoreCase)).Task;

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
            if (text.Contains("urgent") || text.Contains("khẩn")) return DoUuTien.Urgent;
            if (text.Contains("high") || text.Contains("cao")) return DoUuTien.High;
            if (text.Contains("small") || text.Contains("thấp") || text.Contains("nhỏ")) return DoUuTien.Low;
            return DoUuTien.Medium;
        }

        private int UocLuongStoryPointsAI(string title, string typeStr)
        {
            title = title.ToLower();
            typeStr = typeStr.ToLower();

            if (title.Contains("kiến trúc") || title.Contains("architecture") || title.Contains("tích hợp") || title.Contains("payment") || title.Contains("thanh toán"))
                return 8;

            if (title.Contains("api") || title.Contains("database") || title.Contains("logic") || title.Contains("xử lý"))
                return 5;

            if (typeStr.Contains("back")) return 5;
            if (typeStr.Contains("front")) return 3;
            if (typeStr.Contains("ux") || typeStr.Contains("ui")) return 3;
            if (typeStr.Contains("test")) return 2;

            return 3;
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
