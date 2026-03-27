using System;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;

namespace Temp
{
    public class CheckDeps
    {
        public static async Task Run(AppDbContext context)
        {
            var tasks = await context.CongViecs
                .Include(c => c.Dependencies)
                .OrderBy(c => c.Id)
                .ToListAsync();

            Console.WriteLine("ID | Title | Status | Dependencies");
            Console.WriteLine("---|---|---|---");
            foreach (var t in tasks)
            {
                var depIds = string.Join(", ", t.Dependencies.Select(d => d.DependsOnTaskId));
                Console.WriteLine($"{t.Id} | {t.TieuDe} | {t.TrangThai} | [{depIds}]");
            }
        }
    }
}
