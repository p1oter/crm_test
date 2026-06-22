using Microsoft.AspNetCore.Mvc;
using CRM.Data;
using CRM.Models;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Microsoft.AspNetCore.Authorization;

namespace CRM.Controllers
{
    [Authorize(Policy = "CanManageEmployees")]
    public class EmployeesController : Controller
    {
        private readonly CrmDbContext _context;
        private readonly IConfiguration _configuration;

        public EmployeesController(CrmDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: Employees
        public async Task<IActionResult> Index()
        {
            var employees = await _context.Employees
                .Include(e => e.Supervisor)
                .ToListAsync();
            return View(employees);
        }

        // GET: Employees/Details/5
        public async Task<IActionResult> Details(ulong? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.Supervisor)
                .Include(e => e.Subordinates)
                .Include(e => e.GuiUser)
                .Include(e => e.Skills)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // GET: Employees/Create
        public async Task<IActionResult> Create()
        {
            ViewData["Supervisors"] = await _context.Employees
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync();
            return View();
        }

        // POST: Employees/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FirstName,LastName,Position,SupervisorId")] Employee employee)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(employee);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Błąd przy dodawaniu pracownika: {ex.Message}");
                }
            }

            ViewData["Supervisors"] = await _context.Employees
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync();
            return View(employee);
        }

        // GET: Employees/Edit/5
        public async Task<IActionResult> Edit(ulong? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.Supervisor)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null)
            {
                return NotFound();
            }

            ViewData["Supervisors"] = await _context.Employees
                .Where(e => e.Id != id)
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync();

            return View(employee);
        }

        // POST: Employees/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ulong id, [Bind("Id,FirstName,LastName,Position,SupervisorId")] Employee employee)
        {
            if (id != employee.Id)
            {
                return NotFound();
            }

            // Usuń errors dla navigation properties
            ModelState.Remove("Supervisor");
            ModelState.Remove("Subordinates");
            ModelState.Remove("GuiUser");
            ModelState.Remove("Skills");
            ModelState.Remove("InvoiceLines");
            ModelState.Remove("Reservations");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(employee);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employee.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewData["Supervisors"] = await _context.Employees
                .Where(e => e.Id != id)
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync();

            return View(employee);
        }

        // GET: Employees/Delete/5
        public async Task<IActionResult> Delete(ulong? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.Supervisor)
                .Include(e => e.Subordinates)
                .Include(e => e.GuiUser)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(ulong id)
        {
            try
            {
                // Wyłącz foreign key checks
                await _context.Database.ExecuteSqlRawAsync("SET FOREIGN_KEY_CHECKS=0");

                // Załaduj pracownika z powiązanymi danymi
                var employee = await _context.Employees
                    .Include(e => e.Subordinates)
                    .Include(e => e.GuiUser)
                    .Include(e => e.Skills)
                    .Include(e => e.InvoiceLines)
                    .Include(e => e.Reservations)
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (employee != null)
                {
                    Console.WriteLine($"Employee found: {employee.FirstName} {employee.LastName}");

                    // 1. Usuń Skills
                    if (employee.Skills?.Any() == true)
                    {
                        Console.WriteLine("Removing Skills...");
                        _context.EmployeeSkills.RemoveRange(employee.Skills);
                        await _context.SaveChangesAsync();
                    }

                    // 2. Usuń InvoiceLines
                    if (employee.InvoiceLines?.Any() == true)
                    {
                        Console.WriteLine("Removing InvoiceLines...");
                        _context.InvoiceLines.RemoveRange(employee.InvoiceLines);
                        await _context.SaveChangesAsync();
                    }

                    // 3. Usuń Reservations
                    if (employee.Reservations?.Any() == true)
                    {
                        Console.WriteLine("Removing Reservations...");
                        _context.Reservations.RemoveRange(employee.Reservations);
                        await _context.SaveChangesAsync();
                    }

                    // 4. Usuń GuiUser
                    if (employee.GuiUser != null)
                    {
                        Console.WriteLine("Removing GuiUser...");
                        _context.GuiUsers.Remove(employee.GuiUser);
                        await _context.SaveChangesAsync();
                    }

                    // 5. Dla podwładnych - ustaw ich supervisor na null
                    if (employee.Subordinates?.Any() == true)
                    {
                        Console.WriteLine("Updating Subordinates...");
                        foreach (var subordinate in employee.Subordinates)
                        {
                            subordinate.SupervisorId = null;
                        }
                        await _context.SaveChangesAsync();
                    }

                    // 6. OSTATECZNIE usuń Employee
                    Console.WriteLine("Removing Employee...");
                    _context.Employees.Remove(employee);
                    await _context.SaveChangesAsync();

                    Console.WriteLine("Employee deleted successfully!");
                }
                else
                {
                    Console.WriteLine($"Employee with id {id} not found");
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

        private bool EmployeeExists(ulong id)
        {
            return _context.Employees.Any(e => e.Id == id);
        }
    }
}