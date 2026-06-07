using Microsoft.AspNetCore.Mvc;
using CRM.Data;
using CRM.Models;
using Microsoft.EntityFrameworkCore;

namespace CRM.Controllers
{
    public class ServicesController : Controller
    {
        private readonly CrmDbContext _context;

        public ServicesController(CrmDbContext context)
        {
            _context = context;
        }

        // GET: Services
        public async Task<IActionResult> Index()
        {
            var services = await _context.Services
                .Include(s => s.ServiceGroup)
                .ToListAsync();
            return View(services);
        }

        // GET: Services/Details/5
        public async Task<IActionResult> Details(ulong? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var service = await _context.Services
                .Include(s => s.ServiceGroup)
                .Include(s => s.InvoiceLines)
                .Include(s => s.Reservations)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (service == null)
            {
                return NotFound();
            }

            return View(service);
        }

        // GET: Services/Create
        public async Task<IActionResult> Create()
        {
            ViewData["ServiceGroups"] = await _context.ServiceGroups
                .OrderBy(sg => sg.Name)
                .ToListAsync();
            return View();
        }

        // POST: Services/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Price,ServiceGroupId")] Service service)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(service);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Błąd przy dodawaniu usługi: {ex.Message}");
                }
            }

            ViewData["ServiceGroups"] = await _context.ServiceGroups
                .OrderBy(sg => sg.Name)
                .ToListAsync();
            return View(service);
        }

        // GET: Services/Edit/5
        public async Task<IActionResult> Edit(ulong? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var service = await _context.Services
                .Include(s => s.ServiceGroup)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (service == null)
            {
                return NotFound();
            }

            ViewData["ServiceGroups"] = await _context.ServiceGroups
                .OrderBy(sg => sg.Name)
                .ToListAsync();

            return View(service);
        }

        // POST: Services/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ulong id, [Bind("Id,Name,Price,ServiceGroupId")] Service service)
        {
            if (id != service.Id)
            {
                return NotFound();
            }

            // Usuń errors dla navigation properties
            ModelState.Remove("ServiceGroup");
            ModelState.Remove("InvoiceLines");
            ModelState.Remove("Reservations");
            ModelState.Remove("EmployeeSkills");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(service);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceExists(service.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewData["ServiceGroups"] = await _context.ServiceGroups
                .OrderBy(sg => sg.Name)
                .ToListAsync();

            return View(service);
        }

        // GET: Services/Delete/5
        public async Task<IActionResult> Delete(ulong? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var service = await _context.Services
                .Include(s => s.ServiceGroup)
                .Include(s => s.InvoiceLines)
                .Include(s => s.Reservations)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (service == null)
            {
                return NotFound();
            }

            return View(service);
        }

        // POST: Services/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(ulong id)
        {
            try
            {
                // Wyłącz foreign key checks
                await _context.Database.ExecuteSqlRawAsync("SET FOREIGN_KEY_CHECKS=0");

                // Załaduj usługę z powiązanymi danymi
                var service = await _context.Services
                    .Include(s => s.InvoiceLines)
                    .Include(s => s.Reservations)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (service != null)
                {
                    Console.WriteLine($"Service found: {service.Name}");

                    // 1. Usuń InvoiceLines
                    if (service.InvoiceLines?.Any() == true)
                    {
                        Console.WriteLine("Removing InvoiceLines...");
                        _context.InvoiceLines.RemoveRange(service.InvoiceLines);
                        await _context.SaveChangesAsync();
                    }

                    // 2. Usuń Reservations
                    if (service.Reservations?.Any() == true)
                    {
                        Console.WriteLine("Removing Reservations...");
                        _context.Reservations.RemoveRange(service.Reservations);
                        await _context.SaveChangesAsync();
                    }

                    // 3. OSTATECZNIE usuń Service
                    Console.WriteLine("Removing Service...");
                    _context.Services.Remove(service);
                    await _context.SaveChangesAsync();

                    Console.WriteLine("Service deleted successfully!");
                }
                else
                {
                    Console.WriteLine($"Service with id {id} not found");
                }

                // Włącz foreign key checks
                await _context.Database.ExecuteSqlRawAsync("SET FOREIGN_KEY_CHECKS=1");

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");

                try
                {
                    await _context.Database.ExecuteSqlRawAsync("SET FOREIGN_KEY_CHECKS=1");
                }
                catch { }

                ModelState.AddModelError("", $"Błąd: {ex.Message}");
                return RedirectToAction(nameof(Delete), new { id = id });
            }
        }

        private bool ServiceExists(ulong id)
        {
            return _context.Services.Any(e => e.Id == id);
        }
    }
}