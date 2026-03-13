using Apllication.DTOs;
using Apllication.DTOs.CongViec;
using Apllication.IRepositories;
using Apllication.IService;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apllication.Service
{
    public class GiaoViecAIService : IGiaoViecAIService
    {
        private readonly ICongViecRepository _congViecRepo;
        private readonly INguoiDungRepository _nguoiDungRepo;
        private readonly IQuyTacGiaoViecAIRepository _ruleRepo;
        private readonly IDuAnRepository _duAnRepo;
        private readonly ISprintRepository _sprintRepo;

        public GiaoViecAIService(
            ICongViecRepository congViecRepo,
            INguoiDungRepository nguoiDungRepo,
            IQuyTacGiaoViecAIRepository ruleRepo,
            IDuAnRepository duAnRepo,
            ISprintRepository sprintRepo)
        {
            _congViecRepo = congViecRepo;
            _nguoiDungRepo = nguoiDungRepo;
            _ruleRepo = ruleRepo;
            _duAnRepo = duAnRepo;
            _sprintRepo = sprintRepo;
        }

        public async Task<IEnumerable<GoiYGiaoViecDto>> GoiYAssigneeAsync(int congViecId)
        {
            var task = await _congViecRepo.GetByIdAsync(congViecId);
            if (task == null) return new List<GoiYGiaoViecDto>();

            // 1. Lấy danh sách quy tắc AI
            var rules = await _ruleRepo.GetAllActiveRulesAsync();
            double skillWeight = GetRuleValue(rules, "SKILL_MATCH_WEIGHT", 0.6);
            double experienceWeight = GetRuleValue(rules, "EXPERIENCE_WEIGHT", 0.4);
            double workloadPenalty = GetRuleValue(rules, "WORKLOAD_PENALTY", 0.1);
            double minScore = GetRuleValue(rules, "MIN_MATCH_SCORE", 0.3);

            // 2. Lấy danh sách ứng viên (Tất cả nhân viên đang hoạt động)
            // Lưu ý: Trong thực tế nên filter theo team hoặc phòng ban của dự án
            var query = new NguoiDungQueryDto { PageSize = 100 };
            var pagedUsers = await _nguoiDungRepo.LayDanhSachNguoiDungAsync(query);
            var candidates = pagedUsers.Items;

            var recommendations = new List<GoiYGiaoViecDto>();

            foreach (var userDto in candidates)
            {
                var user = await _nguoiDungRepo.LayTheoIdAsync(userDto.Id);
                if (user == null) continue;

                var matchResult = CalculateMatchScore(task, user, skillWeight, experienceWeight, workloadPenalty);
                
                if (matchResult.Score >= minScore)
                {
                    recommendations.Add(new GoiYGiaoViecDto
                    {
                        UserId = user.Id,
                        HoTen = user.FullName,
                        DiemPhuHop = Math.Round(matchResult.Score, 2),
                        LyDo = matchResult.Reason,
                        KyNangPhuHop = matchResult.MatchedSkills
                    });
                }
            }

            return recommendations.OrderByDescending(x => x.DiemPhuHop).Take(5);
        }

        public async Task<bool> TuDongGiaoViecDuAnAsync(int duAnId)
        {
            // 1. Lấy danh sách quy tắc AI để biết Capacity của Sprint (Mặc định 30 SP)
            var rules = await _ruleRepo.GetAllActiveRulesAsync();
            int sprintCapacity = (int)GetRuleValue(rules, "SPRINT_CAPACITY", 30);

            // 2. Lấy danh sách task chưa được giao của dự án
            var query = new CongViecQueryDto { DuAnId = duAnId, PageSize = 1000 };
            var pagedTasks = await _congViecRepo.LayDanhSachCongViecAsync(query);
            var tasks = pagedTasks.Items.Where(t => t.AssigneeId == null || t.SprintId == null).ToList();

            if (!tasks.Any()) return true;

            // 3. Khởi tạo Sprint đầu tiên
            var currentSprint = await GetOrCreateActiveSprintAsync(duAnId, sprintCapacity);
            int currentSprintSp = currentSprint.CongViecs?.Sum(c => c.StoryPoints) ?? 0;

            foreach (var tDto in tasks)
            {
                // Giao việc cho User (Match Score)
                var suggestions = await GoiYAssigneeAsync(tDto.Id);
                var bestMatch = suggestions.FirstOrDefault();

                var task = await _congViecRepo.GetByIdAsync(tDto.Id);
                if (task == null) continue;

                if (bestMatch != null)
                {
                    task.AssigneeId = bestMatch.UserId;
                    task.PhuongThucGiaoViec = PhuongThucGiaoViec.AI;
                    task.AiMatchScore = bestMatch.DiemPhuHop;
                    task.AiReasoning = bestMatch.LyDo;
                }

                // Phân chia nhiệm vụ vào Sprint
                if (currentSprintSp + task.StoryPoints > sprintCapacity)
                {
                    // Sprint hiện tại đầy -> Tạo Sprint mới
                    currentSprint = await CreateNextSprintAsync(duAnId, currentSprint.TenSprint, sprintCapacity);
                    currentSprintSp = 0;
                }

                task.SprintId = currentSprint.Id;
                currentSprintSp += task.StoryPoints;

                await _congViecRepo.UpdateAsync(task);
            }

            return true;
        }

        private async Task<Sprint> GetOrCreateActiveSprintAsync(int duAnId, int capacity)
        {
            var Sprints = await _sprintRepo.GetByProjectIdAsync(duAnId);
            var activeSprint = Sprints.FirstOrDefault(s => s.TrangThai == TrangThaiSprint.New || s.TrangThai == TrangThaiSprint.InProgress);
            
            if (activeSprint != null) return activeSprint;

            return await _sprintRepo.AddAsync(new Sprint
            {
                DuAnId = duAnId,
                TenSprint = "Sprint 1",
                NgayBatDau = DateTime.UtcNow,
                NgayKetThuc = DateTime.UtcNow.AddDays(14),
                MucTieuStoryPoints = capacity,
                TrangThai = TrangThaiSprint.New
            });
        }

        private async Task<Sprint> CreateNextSprintAsync(int duAnId, string lastSprintName, int capacity)
        {
            int nextNumber = 1;
            if (lastSprintName.Contains("Sprint"))
            {
                int.TryParse(lastSprintName.Replace("Sprint", "").Trim(), out nextNumber);
                nextNumber++;
            }

            return await _sprintRepo.AddAsync(new Sprint
            {
                DuAnId = duAnId,
                TenSprint = $"Sprint {nextNumber}",
                NgayBatDau = DateTime.UtcNow,
                NgayKetThuc = DateTime.UtcNow.AddDays(14),
                MucTieuStoryPoints = capacity,
                TrangThai = TrangThaiSprint.New
            });
        }

        private (double Score, string Reason, List<string> MatchedSkills) CalculateMatchScore(
            CongViec task, User user, double sWeight, double eWeight, double wPenalty)
        {
            double skillScore = 0;
            double expScore = 0;
            var matchedSkills = new List<string>();
            var reason = new StringBuilder();

            var userSkills = user.KyNangNguoiDungs ?? new List<KyNangNguoiDung>();
            var taskRequirements = task.YeuCauCongViecs ?? new List<YeuCauCongViec>();

            if (taskRequirements.Any())
            {
                foreach (var req in taskRequirements)
                {
                    var userSkill = userSkills.FirstOrDefault(us => us.KyNangId == req.KyNangId);
                    if (userSkill != null)
                    {
                        // 1. Diem Ky nang: Level user >= Muc do yeu cau -> 1.0, nguoc lai -> LevelUser / MucDoYeuCau
                        double skillRatio = userSkill.Level >= req.MucDoYeuCau ? 1.0 : (double)userSkill.Level / req.MucDoYeuCau;
                        skillScore += skillRatio;
                        
                        matchedSkills.Add(userSkill.KyNang.TenKyNang);

                        // 2. Diem Kinh nghiem: Tinh tren so nam (Quy uoc 5 nam la diem tuyet doi)
                        expScore += Math.Min(userSkill.SoNamKinhNghiem / 5.0, 1.0);
                    }
                }

                // Trung binh cong theo so luong yeu cau
                skillScore /= taskRequirements.Count;
                expScore /= taskRequirements.Count;
            }
            else
            {
                // Neu task khong co yeu cau thi fallback ve keyword matching (Logic tam nhu truoc)
                foreach (var sk in userSkills)
                {
                    if (task.TieuDe.Contains(sk.KyNang.TenKyNang, StringComparison.OrdinalIgnoreCase) ||
                        (task.MoTa != null && task.MoTa.Contains(sk.KyNang.TenKyNang, StringComparison.OrdinalIgnoreCase)))
                    {
                        skillScore += (sk.Level / 5.0);
                        matchedSkills.Add(sk.KyNang.TenKyNang);
                        expScore += Math.Min(sk.SoNamKinhNghiem / 5.0, 1.0);
                    }
                }
                if (matchedSkills.Any())
                {
                    skillScore /= matchedSkills.Count;
                    expScore /= matchedSkills.Count;
                }
            }

            // Tính điểm tổng hợp (Weighted Average)
            double totalScore = (skillScore * sWeight) + (expScore * eWeight);

            // Phạt quá tải (Workload Penalty)
            // Lấy số lượng task đang làm (InProgress) của user
            // Giả lập: User datdd (ID 1) đang bận, các user khác rảnh hơn
            if (user.Id == 1) totalScore -= wPenalty;

            // Xây dựng lý do
            if (matchedSkills.Any())
            {
                reason.Append($"Phù hợp các kỹ năng: {string.Join(", ", matchedSkills)}. ");
                if (expScore > 0.5) reason.Append("Có kinh nghiệm dày dặn trong lĩnh vực này. ");
            }
            else
            {
                reason.Append("Không tìm thấy kỹ năng tương ứng trực tiếp nhưng có thể đào tạo thêm.");
            }

            return (Math.Max(0, totalScore), reason.ToString().Trim(), matchedSkills);
        }

        private double GetRuleValue(IEnumerable<QuyTacGiaoViecAI> rules, string code, double defaultValue)
        {
            var rule = rules.FirstOrDefault(r => r.MaQuyTac == code);
            if (rule != null && double.TryParse(rule.GiaTri, out double val)) return val;
            return defaultValue;
        }
    }
}
