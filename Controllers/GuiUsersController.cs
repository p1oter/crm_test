using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRM.Data; // dostosuj namespace do Twojego projektu
using CRM.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using System;
using Microsoft.AspNetCore.Authorization;

namespace CRM.Controllers
{
    [Authorize(Policy = "CanManageGuiUsers")] 
    public class GuiUsersController : Controller
    {
        private readonly CrmDbContext _context;
        private const int BcryptDesiredCost = 10; // zmień jeśli chcesz inny cost

        public GuiUsersController(CrmDbContext context)
        {
            _context = context;
        }

        // GET: GuiUsers
        public async Task<IActionResult> Index()
        {
            var users = await _context.GuiUsers
                .AsNoTracking()
                .OrderBy(u => u.Login)
                .ToListAsync();
            return View(users);
        }

        // GET: GuiUsers/Details/{login}
        // GET: GuiUsers/Details/{id}
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            // Pobierz użytkownika
            var user = await _context.GuiUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Login == id);

            if (user == null) return NotFound();

            // Pobierz czytelne nazwy (jeśli istnieją). Używamy FirstOrDefaultAsync aby tolerować brak powiązania.
            var employeeName = await _context.Employees
                .Where(e => e.Id == user.EmployeeId)
                .Select(e => (e.FirstName + " " + e.LastName))
                .FirstOrDefaultAsync();

            var roleName = await _context.Roles
                .Where(r => r.Id == user.RoleId)
                .Select(r => r.Name)
                .FirstOrDefaultAsync();

            var vm = new CRM.Models.GuiUserDetailsViewModel
            {
                Login = user.Login,
                PasswordHash = user.PasswordHash,
                EmployeeId = user.EmployeeId,
                RoleId = user.RoleId,
                EmployeeName = employeeName,
                RoleName = roleName
            };

            return View(vm);
        }

        // GET: GuiUsers/Create
        public async Task<IActionResult> Create()
        {
            await PopulateEmployeesAndRoles();
            return View(new GuiUserViewModel());
        }

        // POST: GuiUsers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GuiUserViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                await PopulateEmployeesAndRoles();
                return View(vm);
            }

            // sprawdź, czy wybrany employee istnieje
            var employeeExists = await _context.Employees.AnyAsync(e => e.Id == vm.EmployeeId);
            if (!employeeExists)
            {
                ModelState.AddModelError(nameof(vm.EmployeeId), "Wybrany pracownik nie istnieje.");
            }
            else
            {
                // sprawdź czy pracownik nie ma już konta (EMPLOYEE_ID jest UNIQUE)
                var already = await _context.GuiUsers.AnyAsync(u => u.EmployeeId == vm.EmployeeId);
                if (already)
                {
                    ModelState.AddModelError(nameof(vm.EmployeeId), "Ten pracownik ma już przypisane konto.");
                }
            }

            // sprawdź, czy wybrana rola istnieje
            var roleExists = await _context.Roles.AnyAsync(r => r.Id == vm.RoleId);
            if (!roleExists)
            {
                ModelState.AddModelError(nameof(vm.RoleId), "Wybrana rola nie istnieje.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateEmployeesAndRoles();
                return View(vm);
            }

            if (await _context.GuiUsers.AnyAsync(u => u.Login == vm.Login))
            {
                ModelState.AddModelError(nameof(vm.Login), "Użytkownik z tym loginem już istnieje.");
                await PopulateEmployeesAndRoles();
                return View(vm);
            }

            if (string.IsNullOrEmpty(vm.Password))
            {
                ModelState.AddModelError(nameof(vm.Password), "Hasło jest wymagane.");
                await PopulateEmployeesAndRoles();
                return View(vm);
            }

            var hash = BCrypt.Net.BCrypt.HashPassword(vm.Password, workFactor: 10);

            var entity = new GuiUser
            {
                Login = vm.Login,
                PasswordHash = hash,
                EmployeeId = vm.EmployeeId,
                RoleId = vm.RoleId
            };

            _context.GuiUsers.Add(entity);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: GuiUsers/Edit/{login}
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _context.GuiUsers.FindAsync(id);
            if (user == null) return NotFound();

            var vm = new GuiUserViewModel
            {
                Login = user.Login,
                EmployeeId = user.EmployeeId,
                RoleId = user.RoleId
            };

            await PopulateEmployeesAndRoles();
            return View(vm);
        }

        // POST: GuiUsers/Edit/{login}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, GuiUserViewModel vm)
        {
            if (id != vm.Login) return BadRequest();

            if (!ModelState.IsValid)
            {
                await PopulateEmployeesAndRoles();
                return View(vm);
            }

            var user = await _context.GuiUsers.FindAsync(id);
            if (user == null) return NotFound();

            // update Employee/Role
            user.EmployeeId = vm.EmployeeId;
            user.RoleId = vm.RoleId;

            // if Password provided -> rehash and save
            if (!string.IsNullOrEmpty(vm.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(vm.Password, workFactor: BcryptDesiredCost);
            }

            _context.GuiUsers.Update(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: GuiUsers/Delete/{login}
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _context.GuiUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Login == id);
            if (user == null) return NotFound();

            return View(user);
        }

        // POST: GuiUsers/Delete/{login}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _context.GuiUsers.FindAsync(id);
            if (user == null) return NotFound();

            _context.GuiUsers.Remove(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateEmployeesAndRoles()
        {
            var employees = await _context.Employees
                .AsNoTracking()
                .OrderBy(e => e.FirstName)
                .Select(e => new SelectListItem { Value = e.Id.ToString(), Text = e.FirstName + " " + e.LastName })
                .ToListAsync();

            var roles = await _context.Roles
                .AsNoTracking()
                .OrderBy(r => r.Name)
                .Select(r => new SelectListItem { Value = r.Id.ToString(), Text = r.Name })
                .ToListAsync();

            ViewData["Employees"] = employees;
            ViewData["Roles"] = roles;
        }
    }
}