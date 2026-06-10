using Microsoft.AspNetCore.Mvc;
using CRM.Data;
using CRM.Models;
using Microsoft.EntityFrameworkCore;

namespace CRM.Controllers
{
    public class EmployeeSkillsController : Controller
    {
        private readonly CrmDbContext _context;

        public EmployeeSkillsController(CrmDbContext context)
        {
            _context = context;
        }

        // GET: EmployeeSkills
        public async Task<IActionResult> Index()
        {
            var employeeSkills = await _context.EmployeeSkills
                .Include(es => es.Employee)
                .Include(es => es.ServiceGroup)
                .ToListAsync();
            return View(employeeSkills);
        }

        // GET: EmployeeSkills/Details/5/5
        public async Task<IActionResult> Details(ulong? employeeId, ulong? serviceGroupId)
        {
            if (employeeId == null || serviceGroupId == null)
            {
                return NotFound();
            }

            var employeeSkill = await _context.EmployeeSkills
                .Include(es => es.Employee)
                .Include(es => es.ServiceGroup)
                .FirstOrDefaultAsync(m => m.EmployeeId == employeeId && m.ServiceGroupId == serviceGroupId);

            if (employeeSkill == null)
            {
                return NotFound();
            }

            return View(employeeSkill);
        }

        // GET: EmployeeSkills/Create
        public async Task<IActionResult> Create()
        {
            var employees = await _context.Employees.ToListAsync();
            var serviceGroups = await _context.ServiceGroups.ToListAsync();

            ViewData["Employees"] = employees;
            ViewData["ServiceGroups"] = serviceGroups;

            return View();
        }

        // POST: EmployeeSkills/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EmployeeId,ServiceGroupId")] EmployeeSkills employeeSkill)
        {
            // Sprawdź czy taka umiejętność już istnieje
            var existingSkill = await _context.EmployeeSkills
                .FirstOrDefaultAsync(es => es.EmployeeId == employeeSkill.EmployeeId && es.ServiceGroupId == employeeSkill.ServiceGroupId);

            if (existingSkill != null)
            {
                ModelState.AddModelError("", "Ten pracownik już ma taką umiejętność.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(employeeSkill);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Błąd przy dodawaniu umiejętności: {ex.Message}");
                }
            }

            var employees = await _context.Employees.ToListAsync();
            var serviceGroups = await _context.ServiceGroups.ToListAsync();

            ViewData["Employees"] = employees;
            ViewData["ServiceGroups"] = serviceGroups;

            return View(employeeSkill);
        }

        // GET: EmployeeSkills/Edit/5/5
        public async Task<IActionResult> Edit(ulong? employeeId, ulong? serviceGroupId)
        {
            if (employeeId == null || serviceGroupId == null)
            {
                return NotFound();
            }

            var employeeSkill = await _context.EmployeeSkills
                .FirstOrDefaultAsync(es => es.EmployeeId == employeeId && es.ServiceGroupId == serviceGroupId);

            if (employeeSkill == null)
            {
                return NotFound();
            }

            var employees = await _context.Employees.ToListAsync();
            var serviceGroups = await _context.ServiceGroups.ToListAsync();

            ViewData["Employees"] = employees;
            ViewData["ServiceGroups"] = serviceGroups;

            return View(employeeSkill);
        }

        // POST: EmployeeSkills/Edit/5/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ulong employeeId, ulong serviceGroupId, [Bind("EmployeeId,ServiceGroupId")] EmployeeSkills employeeSkill)
        {
            if (employeeId != employeeSkill.EmployeeId || serviceGroupId != employeeSkill.ServiceGroupId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(employeeSkill);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeSkillExists(employeeSkill.EmployeeId, employeeSkill.ServiceGroupId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            var employees = await _context.Employees.ToListAsync();
            var serviceGroups = await _context.ServiceGroups.ToListAsync();

            ViewData["Employees"] = employees;
            ViewData["ServiceGroups"] = serviceGroups;

            return View(employeeSkill);
        }

        // GET: EmployeeSkills/Delete/5/5
        public async Task<IActionResult> Delete(ulong? employeeId, ulong? serviceGroupId)
        {
            if (employeeId == null || serviceGroupId == null)
            {
                return NotFound();
            }

            var employeeSkill = await _context.EmployeeSkills
                .Include(es => es.Employee)
                .Include(es => es.ServiceGroup)
                .FirstOrDefaultAsync(m => m.EmployeeId == employeeId && m.ServiceGroupId == serviceGroupId);

            if (employeeSkill == null)
            {
                return NotFound();
            }

            return View(employeeSkill);
        }

        // POST: EmployeeSkills/Delete/5/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(ulong employeeId, ulong serviceGroupId)
        {
            try
            {
                var employeeSkill = await _context.EmployeeSkills
                    .FirstOrDefaultAsync(es => es.EmployeeId == employeeId && es.ServiceGroupId == serviceGroupId);

                if (employeeSkill != null)
                {
                    _context.EmployeeSkills.Remove(employeeSkill);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Błąd: {ex.Message}");
                return RedirectToAction(nameof(Delete), new { employeeId = employeeId, serviceGroupId = serviceGroupId });
            }
        }

        private bool EmployeeSkillExists(ulong employeeId, ulong serviceGroupId)
        {
            return _context.EmployeeSkills.Any(e => e.EmployeeId == employeeId && e.ServiceGroupId == serviceGroupId);
        }
    }
}