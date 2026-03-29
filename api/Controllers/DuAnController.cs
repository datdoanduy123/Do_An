using Microsoft.AspNetCore.Authorization;
using api.Attributes;
using Apllication.DTOs.DuAn;
using Apllication.IService;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DuAnController : BaseController
    {
        private readonly IDuAnService _duAnService;

        public DuAnController(IDuAnService duAnService)
        {
            _duAnService = duAnService;
        }

        [QuyenHan("PROJECT_VIEW")]
        [HttpGet("danh-sach")]
        public async Task<IActionResult> LayDanhSach()
        {
            try
            {
                var result = await _duAnService.GetAllAsync();
                return SuccessResponse(result, "Lay danh sach du an thanh cong.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }

        [QuyenHan("PROJECT_VIEW")]
        [HttpGet("{id}")]
        public async Task<IActionResult> ChiTiet(int id)
        {
            try
            {
                var result = await _duAnService.GetByIdAsync(id);
                if (result == null) return ErrorResponse(404, "Khong tim thay du an.");
                return SuccessResponse(result, "Lay thong tin du an thanh cong.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }

        [QuyenHan("PROJECT_CREATE")]
        [HttpPost("tao-du-an")]
        public async Task<IActionResult> TaoDuAn([FromBody] TaoDuAnDto dto)
        {
            try
            {
                var result = await _duAnService.CreateAsync(dto, CurrentUserId);
                return SuccessResponse(result, "Tao du an thanh cong.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }

        [QuyenHan("PROJECT_UPDATE")]
        [HttpPut("{id}")]
        public async Task<IActionResult> CapNhat(int id, [FromBody] CapNhatDuAnDto dto)
        {
            try
            {
                var result = await _duAnService.UpdateAsync(id, dto);
                if (result) return SuccessResponse(null!, "Cap nhat du an thanh cong.");
                return ErrorResponse(400, "Khong the cap nhat du an.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }

        [QuyenHan("PROJECT_DELETE")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Xoa(int id)
        {
            try
            {
                var result = await _duAnService.DeleteAsync(id);
                if (result) return SuccessResponse(null!, "Xoa du an thanh cong.");
                return ErrorResponse(400, "Khong the xoa du an.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }

        [QuyenHan("PROJECT_VIEW")]
        [HttpGet("{id}/members")]
        public async Task<IActionResult> LayThanhVien(int id)
        {
            try
            {
                var result = await _duAnService.GetMembersAsync(id);
                return SuccessResponse(result, "Lay danh sach thanh vien thanh cong.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }

        [QuyenHan("PROJECT_UPDATE")]
        [HttpPost("{id}/members/{userId}")]
        public async Task<IActionResult> ThemThanhVien(int id, int userId)
        {
            try
            {
                var result = await _duAnService.AddMemberAsync(id, userId);
                if (result) return SuccessResponse(null!, "Them thanh vien thanh cong.");
                return ErrorResponse(400, "Khong the them thanh vien.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }

        [QuyenHan("PROJECT_UPDATE")]
        [HttpDelete("{id}/members/{userId}")]
        public async Task<IActionResult> XoaThanhVien(int id, int userId)
        {
            try
            {
                var result = await _duAnService.RemoveMemberAsync(id, userId);
                if (result) return SuccessResponse(null!, "Xoa thanh vien thanh cong.");
                return ErrorResponse(400, "Khong the xoa thanh vien.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }

        [QuyenHan("PROJECT_VIEW")]
        [HttpGet("{id}/skill-coverage")]
        public async Task<IActionResult> LayDoPhuKyNang(int id)
        {
            try
            {
                var result = await _duAnService.GetSkillCoverageAsync(id);
                return SuccessResponse(result, "Lay do phu ky nang thanh cong.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }
    }
}
