using Microsoft.AspNetCore.Mvc;
using CRM.Data;
using CRM.Models;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;

namespace CRM.Controllers
{
    public class InvoicePrintController : Controller
    {
        private readonly CrmDbContext _context;

        public InvoicePrintController(CrmDbContext context)
        {
            _context = context;
        }

        // GET: /InvoicePrint/Print/5  -> HTML view for preview/printing
        public async Task<IActionResult> Print(ulong? id)
        {
            if (id == null) return NotFound();

            var invoice = await _context.Invoices
                .Include(i => i.Client)
                .Include(i => i.Lines)
                    .ThenInclude(l => l.Service)
                .Include(i => i.Lines)
                    .ThenInclude(l => l.Employee)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null) return NotFound();

            return View("Print", invoice);
        }

        // GET: /InvoicePrint/Pdf/5 -> returns PDF file
        public async Task<IActionResult> Pdf(ulong? id)
        {
            if (id == null) return NotFound();

            var invoice = await _context.Invoices
                .Include(i => i.Client)
                .Include(i => i.Lines)
                    .ThenInclude(l => l.Service)
                .Include(i => i.Lines)
                    .ThenInclude(l => l.Employee)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null) return NotFound();

            // ViewAsPdf automatycznie renderuje widok do PDF
            var pdfResult = new ViewAsPdf("Print", invoice)
            {
                FileName = $"Invoice_{invoice.Id}.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                // Dopasowanie marginesów
                PageMargins = new Rotativa.AspNetCore.Options.Margins(10, 10, 10, 10)
            };

            return pdfResult;
        }
    }
}