using Microsoft.AspNetCore.Mvc;
using CRM.Data;
using CRM.Models;
using Microsoft.EntityFrameworkCore;

namespace CRM.Controllers
{
    public class ReservationsController : Controller
    {
        private readonly CrmDbContext _context;

        public ReservationsController(CrmDbContext context)
        {
            _context = context;
        }

        // GET: Reservations
        public async Task<IActionResult> Index()
        {
            var reservations = await _context.Reservations
                .Include(r => r.Employee)
                .Include(r => r.Client)
                .Include(r => r.Service)
                .ToListAsync();
            return View(reservations);
        }

        // GET: Reservations/Details/5
        public async Task<IActionResult> Details(ulong? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations
                .Include(r => r.Employee)
                .Include(r => r.Client)
                .Include(r => r.Service)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

        // GET: Reservations/Create
        public async Task<IActionResult> Create()
        {
            ViewData["Employees"] = await _context.Employees.ToListAsync();
            ViewData["Clients"] = await _context.Clients.ToListAsync();
            ViewData["Services"] = await _context.Services.ToListAsync();
            return View();
        }

        // POST: Reservations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("StartT,EmployeeId,ClientId,ServiceId")] Reservation reservation)
        {
            // Usuń validation errors dla navigation properties
            ModelState.Remove("Employee");
            ModelState.Remove("Client");
            ModelState.Remove("Service");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(reservation);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Błąd przy dodawaniu rezerwacji: {ex.Message}");
                }
            }

            ViewData["EmployeeId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                await _context.Employees.ToListAsync(), "Id", "FirstName", reservation.EmployeeId);
            ViewData["ClientId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                await _context.Clients.ToListAsync(), "Id", "FirstName", reservation.ClientId);
            ViewData["ServiceId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                await _context.Services.ToListAsync(), "Id", "Name", reservation.ServiceId);
            return View(reservation);
        }

        // GET: Reservations/Edit/5
        public async Task<IActionResult> Edit(ulong? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }

            ViewData["Employees"] = await _context.Employees.ToListAsync();
            ViewData["Clients"] = await _context.Clients.ToListAsync();
            ViewData["Services"] = await _context.Services.ToListAsync();

            return View(reservation);
        }

        // POST: Reservations/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ulong id, [Bind("Id,StartT,EmployeeId,ClientId,ServiceId")] Reservation reservation)
        {
            if (id != reservation.Id)
            {
                return NotFound();
            }

            // Usuń validation errors dla navigation properties
            ModelState.Remove("Employee");
            ModelState.Remove("Client");
            ModelState.Remove("Service");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(reservation);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReservationExists(reservation.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewData["EmployeeId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                await _context.Employees.ToListAsync(), "Id", "FirstName", reservation.EmployeeId);
            ViewData["ClientId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                await _context.Clients.ToListAsync(), "Id", "FirstName", reservation.ClientId);
            ViewData["ServiceId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                await _context.Services.ToListAsync(), "Id", "Name", reservation.ServiceId);

            return View(reservation);
        }

        // GET: Reservations/Delete/5
        public async Task<IActionResult> Delete(ulong? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations
                .Include(r => r.Employee)
                .Include(r => r.Client)
                .Include(r => r.Service)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

        // POST: Reservations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(ulong id)
        {
            try
            {
                var reservation = await _context.Reservations.FindAsync(id);
                if (reservation != null)
                {
                    _context.Reservations.Remove(reservation);
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

        private bool ReservationExists(ulong id)
        {
            return _context.Reservations.Any(e => e.Id == id);
        }
    }
}