using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRM.Models;
using CRM.Data;

namespace CRM.Controllers
{
    public class HomeController : Controller
    {
        private readonly CrmDbContext _context;

        public HomeController(CrmDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}