using Microsoft.AspNetCore.Mvc;
using CRM.Data;
using CRM.Models;
using Microsoft.EntityFrameworkCore;

namespace CRM.Controllers
{
    public class BillInfosController : Controller
    {
        private readonly CrmDbContext _context;

        public BillInfosController(CrmDbContext context)
        {
            _context = context;
        }

        // GET: BillInfos
        public async Task<IActionResult> Index()
        {
            var billInfos = await _context.BillInfos
                .Include(b => b.Client)
                .Include(b => b.Payments)
                .ToListAsync();
            return View(billInfos);
        }

        // GET: BillInfos/Details/5
        public async Task<IActionResult> Details(ulong? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var billInfo = await _context.BillInfos
                .Include(b => b.Client)
                .Include(b => b.Payments)
                .FirstOrDefaultAsync(m => m.ClientId == id);

            if (billInfo == null)
            {
                return NotFound();
            }

            return View(billInfo);
        }
    }
}