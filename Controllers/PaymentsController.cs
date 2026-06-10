using Microsoft.AspNetCore.Mvc;
using CRM.Data;
using Microsoft.EntityFrameworkCore;

namespace CRM.Controllers
{
    public class PaymentsController : Controller
    {
        private readonly CrmDbContext _context;

        public PaymentsController(CrmDbContext context)
        {
            _context = context;
        }

        // GET: Payments
        public async Task<IActionResult> Index()
        {
            var payments = await _context.Payments
                .Include(p => p.BillInfo)
                .ThenInclude(b => b.Client)
                .OrderByDescending(p => p.CreatedT)
                .ToListAsync();
            return View(payments);
        }
    }
}