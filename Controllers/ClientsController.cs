using Microsoft.AspNetCore.Mvc;
using CRM.Data;
using CRM.Models;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Microsoft.AspNetCore.Authorization;

namespace CRM.Controllers
{
    [Authorize(Policy = "CanManageClients")]
    public class ClientsController : Controller
    {
        private readonly CrmDbContext _context;
        private readonly IConfiguration _configuration;

        public ClientsController(CrmDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: Clients
        public async Task<IActionResult> Index()
        {
            var clients = await _context.Clients.ToListAsync();
            return View(clients);
        }

        // GET: Clients/Details/5
        public async Task<IActionResult> Details(ulong? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _context.Clients
                .Include(c => c.BillInfo)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        // GET: Clients/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Clients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FirstName,LastName,Email,Street,HouseNumber,ApartmentNumber,PostalCode,City,Country")] Client client, string bankAccount)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    using (var connection = new MySqlConnection(_configuration.GetConnectionString("CrmDatabase")))
                    {
                        await connection.OpenAsync();

                        using (var command = new MySqlCommand("CREATE_CLIENT", connection))
                        {
                            command.CommandType = System.Data.CommandType.StoredProcedure;

                            command.Parameters.AddWithValue("@P_FIRST_NAME", client.FirstName);
                            command.Parameters.AddWithValue("@P_LAST_NAME", client.LastName);
                            command.Parameters.AddWithValue("@P_STREET", client.Street);
                            command.Parameters.AddWithValue("@P_HOUSE_NUMBER", client.HouseNumber);
                            command.Parameters.AddWithValue("@P_APARTMENT_NUMBER", client.ApartmentNumber ?? "");
                            command.Parameters.AddWithValue("@P_POSTAL_CODE", client.PostalCode);
                            command.Parameters.AddWithValue("@P_CITY", client.City);
                            command.Parameters.AddWithValue("@P_EMAIL", client.Email);
                            command.Parameters.AddWithValue("@P_COUNTRY", client.Country);
                            command.Parameters.AddWithValue("@P_BANK_ACCOUNT", bankAccount);

                            var outParam = new MySqlParameter("@P_ID", System.Data.DbType.UInt64)
                            {
                                Direction = System.Data.ParameterDirection.Output
                            };
                            command.Parameters.Add(outParam);

                            await command.ExecuteNonQueryAsync();
                        }
                    }

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Błąd przy dodawaniu klienta: {ex.Message}");
                }
            }
            return View(client);
        }

        // GET: Clients/Edit/5
        public async Task<IActionResult> Edit(ulong? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _context.Clients
                .Include(c => c.BillInfo)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        // POST: Clients/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ulong id, [Bind("Id,FirstName,LastName,Email,Street,HouseNumber,ApartmentNumber,PostalCode,City,Country")] Client client)
        {
            if (id != client.Id)
            {
                return NotFound();
            }

            // Usuń errors dla navigation properties
            ModelState.Remove("BillInfo");
            ModelState.Remove("Invoices");
            ModelState.Remove("Reservations");
            ModelState.Remove("InvoiceLines");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(client);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClientExists(client.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(client);
        }

        // GET: Clients/Delete/5
        public async Task<IActionResult> Delete(ulong? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _context.Clients
                .Include(c => c.BillInfo)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        // POST: Clients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(ulong id)
        {
            try
            {
                // Wyłącz foreign key checks
                await _context.Database.ExecuteSqlRawAsync("SET FOREIGN_KEY_CHECKS=0");

                // Załaduj klienta z powiązanymi danymi
                var client = await _context.Clients
                    .Include(c => c.BillInfo)
                    .Include(c => c.Invoices)
                    .Include(c => c.Reservations)
                    .Include(c => c.InvoiceLines)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (client != null)
                {
                    Console.WriteLine($"Client found: {client.FirstName} {client.LastName}");

                    // 1. NAJPIERW usuń PAYMENT (bo ma FK do BILLINFO)
                    if (client.BillInfo != null)
                    {
                        Console.WriteLine("Removing Payments...");
                        var payments = await _context.Payments
                            .Where(p => p.BankAccount == client.BillInfo.BankAccount)
                            .ToListAsync();

                        if (payments.Any())
                        {
                            _context.Payments.RemoveRange(payments);
                            await _context.SaveChangesAsync();
                        }
                    }

                    // 2. Potem usuń BillInfo
                    if (client.BillInfo != null)
                    {
                        Console.WriteLine("Removing BillInfo...");
                        _context.BillInfos.Remove(client.BillInfo);
                        await _context.SaveChangesAsync();
                    }

                    // 3. Usuń InvoiceLines
                    if (client.InvoiceLines?.Any() == true)
                    {
                        Console.WriteLine("Removing InvoiceLines...");
                        _context.InvoiceLines.RemoveRange(client.InvoiceLines);
                        await _context.SaveChangesAsync();
                    }

                    // 4. Usuń Invoices
                    if (client.Invoices?.Any() == true)
                    {
                        Console.WriteLine("Removing Invoices...");
                        _context.Invoices.RemoveRange(client.Invoices);
                        await _context.SaveChangesAsync();
                    }

                    // 5. Usuń Reservations
                    if (client.Reservations?.Any() == true)
                    {
                        Console.WriteLine("Removing Reservations...");
                        _context.Reservations.RemoveRange(client.Reservations);
                        await _context.SaveChangesAsync();
                    }

                    // 6. OSTATECZNIE usuń Client
                    Console.WriteLine("Removing Client...");
                    _context.Clients.Remove(client);
                    await _context.SaveChangesAsync();

                    Console.WriteLine("Client deleted successfully!");
                }
                else
                {
                    Console.WriteLine($"Client with id {id} not found");
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

        private bool ClientExists(ulong id)
        {
            return _context.Clients.Any(e => e.Id == id);
        }
    }
}