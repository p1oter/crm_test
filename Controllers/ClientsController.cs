using Microsoft.AspNetCore.Mvc;
using CRM.Data;
using CRM.Models;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Microsoft.AspNetCore.Authorization;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;

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
            if (id == null) return NotFound();

            var client = await _context.Clients
                .Include(c => c.BillInfo)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (client == null) return NotFound();

            return View(client);
        }

        // GET: Clients/Create
        public IActionResult Create() => View();

        // POST: Clients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FirstName,LastName,Email,Street,HouseNumber,ApartmentNumber,PostalCode,City,Country")] Client client)
        {
            if (!ModelState.IsValid) return View(client);

            // Generate unique 26-digit bank account number (ensure uniqueness against BILLINFO.BANK_ACCOUNT)
            string bankAccount;
            const int desiredLength = 26;
            var attempts = 0;
            do
            {
                if (attempts++ > 10)
                {
                    ModelState.AddModelError("", "Nie udało się wygenerować unikalnego numeru konta (zbyt wiele kolizji). Spróbuj ponownie.");
                    return View(client);
                }
                bankAccount = GenerateAccountNumber(desiredLength);
            } while (await _context.BillInfos.AnyAsync(b => b.BankAccount == bankAccount));

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
                        // opcjonalnie: var newId = Convert.ToUInt64(outParam.Value);
                    }
                }

                // Try to send the generated account number by email. Do not fail client creation if email sending fails.
                try
                {
                    if (!string.IsNullOrWhiteSpace(client.Email))
                    {
                        await SendAccountNumberEmailAsync(client.Email, bankAccount, client.FirstName, client.LastName);
                    }
                }
                catch (Exception emailEx)
                {
                    // Log the email error; client creation already succeeded
                    Console.WriteLine($"Failed to send account number email: {emailEx.Message}");
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Błąd przy dodawaniu klienta: {ex.Message}");
            }

            return View(client);
        }

        // (pozostałe akcje Edit/Delete - niezmienione)
        // GET: Clients/Edit/5
        public async Task<IActionResult> Edit(ulong? id)
        {
            if (id == null) return NotFound();

            var client = await _context.Clients
                .Include(c => c.BillInfo)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (client == null) return NotFound();

            return View(client);
        }

        // POST: Clients/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ulong id, [Bind("Id,FirstName,LastName,Email,Street,HouseNumber,ApartmentNumber,PostalCode,City,Country")] Client client)
        {
            if (id != client.Id) return NotFound();

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
                    if (!ClientExists(client.Id)) return NotFound();
                    throw;
                }
            }
            return View(client);
        }

        // GET: Clients/Delete/5
        public async Task<IActionResult> Delete(ulong? id)
        {
            if (id == null) return NotFound();

            var client = await _context.Clients
                .Include(c => c.BillInfo)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (client == null) return NotFound();

            return View(client);
        }

        // POST: Clients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(ulong id)
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync("SET FOREIGN_KEY_CHECKS=0");

                var client = await _context.Clients
                    .Include(c => c.BillInfo)
                    .Include(c => c.Invoices)
                    .Include(c => c.Reservations)
                    .Include(c => c.InvoiceLines)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (client != null)
                {
                    Console.WriteLine($"Client found: {client.FirstName} {client.LastName}");

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

                    if (client.BillInfo != null)
                    {
                        Console.WriteLine("Removing BillInfo...");
                        _context.BillInfos.Remove(client.BillInfo);
                        await _context.SaveChangesAsync();
                    }

                    if (client.InvoiceLines?.Any() == true)
                    {
                        Console.WriteLine("Removing InvoiceLines...");
                        _context.InvoiceLines.RemoveRange(client.InvoiceLines);
                        await _context.SaveChangesAsync();
                    }

                    if (client.Invoices?.Any() == true)
                    {
                        Console.WriteLine("Removing Invoices...");
                        _context.Invoices.RemoveRange(client.Invoices);
                        await _context.SaveChangesAsync();
                    }

                    if (client.Reservations?.Any() == true)
                    {
                        Console.WriteLine("Removing Reservations...");
                        _context.Reservations.RemoveRange(client.Reservations);
                        await _context.SaveChangesAsync();
                    }

                    Console.WriteLine("Removing Client...");
                    _context.Clients.Remove(client);
                    await _context.SaveChangesAsync();

                    Console.WriteLine("Client deleted successfully!");
                }
                else
                {
                    Console.WriteLine($"Client with id {id} not found");
                }

                await _context.Database.ExecuteSqlRawAsync("SET FOREIGN_KEY_CHECKS=1");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
                try { await _context.Database.ExecuteSqlRawAsync("SET FOREIGN_KEY_CHECKS=1"); } catch { }
                ModelState.AddModelError("", $"Błąd: {ex.Message}");
                return RedirectToAction(nameof(Delete), new { id = id });
            }
        }

        private bool ClientExists(ulong id) => _context.Clients.Any(e => e.Id == id);

        private static string GenerateAccountNumber(int length = 26)
        {
            var result = new StringBuilder(length);
            using (var rng = RandomNumberGenerator.Create())
            {
                var buffer = new byte[8];
                while (result.Length < length)
                {
                    rng.GetBytes(buffer);
                    for (int i = 0; i < buffer.Length && result.Length < length; i++)
                    {
                        result.Append((buffer[i] % 10).ToString());
                    }
                }
            }
            return result.ToString();
        }

        private async Task SendAccountNumberEmailAsync(string toEmail, string accountNumber, string? firstName, string? lastName)
        {
            var smtpSection = _configuration.GetSection("Smtp");
            var host = smtpSection["Host"];
            var portString = smtpSection["Port"];
            var user = smtpSection["User"];
            var pass = smtpSection["Pass"];
            var enableSslString = smtpSection["EnableSsl"];
            var from = smtpSection["From"];

            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(portString) || string.IsNullOrWhiteSpace(from))
            {
                Console.WriteLine("SMTP not configured properly; skipping sending account number email.");
                return;
            }

            if (!int.TryParse(portString, out var port)) port = 25;
            if (!bool.TryParse(enableSslString, out var enableSsl)) enableSsl = true;

            using (var smtpClient = new SmtpClient(host, port))
            {
                if (!string.IsNullOrWhiteSpace(user))
                {
                    smtpClient.Credentials = new System.Net.NetworkCredential(user, pass);
                }
                smtpClient.EnableSsl = enableSsl;

                var subject = "Twój numer konta płatniczego";
                var body = new StringBuilder();
                body.AppendLine($"Dzień dobry {firstName} {lastName},");
                body.AppendLine();
                body.AppendLine("Wygenerowaliśmy dla Ciebie numer konta płatniczego:");
                body.AppendLine();
                body.AppendLine(accountNumber);
                body.AppendLine();
                body.AppendLine("Pozdrawiamy,");
                body.AppendLine("Zespół CRM");

                using (var message = new MailMessage(from, toEmail)
                {
                    Subject = subject,
                    Body = body.ToString()
                })
                {
                    await smtpClient.SendMailAsync(message);
                }
            }
        }
    }
}