using Microsoft.EntityFrameworkCore;
using Apllication.DTOs;

namespace Infrastructure.Helpers
{
    // Phuong thuc mo rong giup phan trang de dang tu IQueryable
    public static class QueryableExtensions
    {
        public static async Task<KetQuaPhanTrangDto<T>> ToPagedListAsync<T>(
            this IQueryable<T> source, 
            int pageIndex, 
            int pageSize)
        {
            var count = await source.CountAsync();
            var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            
            return new KetQuaPhanTrangDto<T>(items, count, pageIndex, pageSize);
        }
    }
}
