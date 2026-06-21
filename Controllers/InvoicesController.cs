using Microsoft.AspNetCore.Mvc;
using CRM.Data;
using CRM.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Controllers
{
    public class InvoicesController : Controller
    {
        private readonly CrmDbContext _context;
        private readonly ILogger<InvoicesController> _logger;

        public InvoicesController(CrmDbContext context, ILogger<InvoicesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Invoices
        public async Task<IActionResult> Index()
        {
            var invoices = await _context.Invoices
                .Include(i => i.Client)
                .Include(i => i.Lines)
                .ToListAsync();
            return View(invoices);
        }

        // GET: Invoices/Details/5
        public async Task<IActionResult> Details(ulong? id)
        {
            if (id == null) return NotFound();

            var invoice = await _context.Invoices
                .Include(i => i.Client)
                    .ThenInclude(c => c.BillInfo)
                .Include(i => i.Lines)
                    .ThenInclude(l => l.Service)
                .Include(i => i.Lines)
                    .ThenInclude(l => l.Employee)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null) return NotFound();

            return View(invoice);
        }

        // GET: Invoices/Create
        public async Task<IActionResult> Create()
        {
            ViewData["Clients"] = await _context.Clients.ToListAsync();
            ViewData["Services"] = await _context.Services.ToListAsync();
            ViewData["Employees"] = await _context.Employees.ToListAsync();

            var invoice = new Invoice
            {
                CreatedT = DateTime.Now,
                // Wygeneruj dwie puste linie domyślnie
                Lines = new List<InvoiceLine> { new InvoiceLine(), new InvoiceLine() }
            };

            return View(invoice);
        }

        // POST: Invoices/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Invoice invoice)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Clients"] = await _context.Clients.ToListAsync();
                ViewData["Services"] = await _context.Services.ToListAsync();
                ViewData["Employees"] = await _context.Employees.ToListAsync();
                return View(invoice);
            }

            try
            {
                // Ustaw ClientId dla każdej linii (DB wymaga)
                if (invoice.Lines != null)
                {
                    foreach (var line in invoice.Lines)
                    {
                        line.ClientId = invoice.ClientId;
                        if (line.Amount == 0) line.Amount = 1;
                    }
                }

                _context.Invoices.Add(invoice);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Błąd przy zapisie faktury: {ex.Message}");
                ViewData["Clients"] = await _context.Clients.ToListAsync();
                ViewData["Services"] = await _context.Services.ToListAsync();
                ViewData["Employees"] = await _context.Employees.ToListAsync();
                return View(invoice);
            }
        }

        // GET: Invoices/Edit/5
        public async Task<IActionResult> Edit(ulong? id)
        {
            if (id == null) return NotFound();

            var invoice = await _context.Invoices
                .Include(i => i.Lines)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null) return NotFound();

            ViewData["Clients"] = await _context.Clients.ToListAsync();
            ViewData["Services"] = await _context.Services.ToListAsync();
            ViewData["Employees"] = await _context.Employees.ToListAsync();

            // ensure we have a List (model should use List<InvoiceLine>) and at least one line for editor template
            if (invoice.Lines == null || invoice.Lines.Count == 0)
            {
                invoice.Lines = new List<InvoiceLine> { new InvoiceLine() };
            }

            return View(invoice);
        }

        // POST: Invoices/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ulong id, Invoice invoice)
        {
            if (id != invoice.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewData["Clients"] = await _context.Clients.ToListAsync();
                ViewData["Services"] = await _context.Services.ToListAsync();
                ViewData["Employees"] = await _context.Employees.ToListAsync();
                return View(invoice);
            }

            try
            {
                var dbInvoice = await _context.Invoices.FindAsync(id);
                if (dbInvoice == null) return NotFound();

                // update header
                dbInvoice.ClientId = invoice.ClientId;
                dbInvoice.CreatedT = invoice.CreatedT;
                dbInvoice.DueT = invoice.DueT;
                dbInvoice.SentToClient = invoice.SentToClient;

                // remove existing lines
                var existingLines = _context.InvoiceLines.Where(l => l.InvoiceId == id);
                _context.InvoiceLines.RemoveRange(existingLines);

                // add posted lines (set InvoiceId and ClientId)
                if (invoice.Lines != null)
                {
                    foreach (var line in invoice.Lines)
                    {
                        // defensywne ustawienia
                        if (line.Amount == 0) line.Amount = 1;
                        line.InvoiceId = id;
                        line.ClientId = invoice.ClientId;
                        line.Id = 0; // ensure EF treats as new
                        line.Service = null;
                        line.Employee = null;
                        line.Client = null;

                        _context.InvoiceLines.Add(line);
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Invoices.Any(e => e.Id == id)) return NotFound();
                throw;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Błąd przy zapisie faktury: {ex.Message}");
                ViewData["Clients"] = await _context.Clients.ToListAsync();
                ViewData["Services"] = await _context.Services.ToListAsync();
                ViewData["Employees"] = await _context.Employees.ToListAsync();
                return View(invoice);
            }
        }

        // GET: Invoices/Delete/5
        public async Task<IActionResult> Delete(ulong? id)
        {
            if (id == null) return NotFound();

            var invoice = await _context.Invoices
                .Include(i => i.Client)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null) return NotFound();

            return View(invoice);
        }

        // POST: Invoices/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(ulong id)
        {
            try
            {
                // Remove lines explicitly
                var lines = _context.InvoiceLines.Where(l => l.InvoiceId == id);
                _context.InvoiceLines.RemoveRange(lines);

                var invoice = await _context.Invoices.FindAsync(id);
                if (invoice != null)
                {
                    _context.Invoices.Remove(invoice);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Błąd: {ex.Message}");
                return RedirectToAction(nameof(Delete), new { id });
            }
        }
    }
}