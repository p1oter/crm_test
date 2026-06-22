using Microsoft.AspNetCore.Mvc;
using CRM.Data;
using CRM.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace CRM.Controllers
{
    [Authorize(Policy = "CanManageServices")]
    public class ServiceGroupsController : Controller
    {
        private readonly CrmDbContext _context;

        public ServiceGroupsController(CrmDbContext context)
        {
            _context = context;
        }

        // GET: ServiceGroups
        public async Task<IActionResult> Index()
        {
            var serviceGroups = await _context.ServiceGroups
                .Include(sg => sg.Services)
                .Include(sg => sg.EmployeeSkills)
                .ToListAsync();
            return View(serviceGroups);
        }

        // GET: ServiceGroups/Details/5
        public async Task<IActionResult> Details(ulong? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceGroup = await _context.ServiceGroups
                .Include(sg => sg.Services)
                .Include(sg => sg.EmployeeSkills)
                .ThenInclude(es => es.Employee)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (serviceGroup == null)
            {
                return NotFound();
            }

            return View(serviceGroup);
        }

        // GET: ServiceGroups/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ServiceGroups/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name")] ServiceGroup serviceGroup)
        {
            ModelState.Remove("Services");
            ModelState.Remove("EmployeeSkills");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(serviceGroup);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Błąd przy dodawaniu kategorii: {ex.Message}");
                }
            }
            return View(serviceGroup);
        }

        // GET: ServiceGroups/Edit/5
        public async Task<IActionResult> Edit(ulong? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceGroup = await _context.ServiceGroups.FindAsync(id);
            if (serviceGroup == null)
            {
                return NotFound();
            }

            return View(serviceGroup);
        }

        // POST: ServiceGroups/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ulong id, [Bind("Id,Name")] ServiceGroup serviceGroup)
        {
            if (id != serviceGroup.Id)
            {
                return NotFound();
            }

            ModelState.Remove("Services");
            ModelState.Remove("EmployeeSkills");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(serviceGroup);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceGroupExists(serviceGroup.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(serviceGroup);
        }

        // GET: ServiceGroups/Delete/5
        public async Task<IActionResult> Delete(ulong? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceGroup = await _context.ServiceGroups
                .Include(sg => sg.Services)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (serviceGroup == null)
            {
                return NotFound();
            }

            return View(serviceGroup);
        }

        // POST: ServiceGroups/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(ulong id)
        {
            try
            {
                var serviceGroup = await _context.ServiceGroups
                    .Include(sg => sg.Services)
                    .FirstOrDefaultAsync(sg => sg.Id == id);

                if (serviceGroup != null)
                {
                    // Sprawdź czy kategoria ma powiązane usługi
                    if (serviceGroup.Services.Any())
                    {
                        return RedirectToAction(nameof(Delete), new { id = id, error = "Nie można usunąć kategorii, która ma powiązane usługi." });
                    }

                    _context.ServiceGroups.Remove(serviceGroup);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Błąd: {ex.Message}");
                return RedirectToAction(nameof(Delete), new { id = id });
            }
        }

        private bool ServiceGroupExists(ulong id)
        {
            return _context.ServiceGroups.Any(e => e.Id == id);
        }
    }
}