
using MedicalCompany.Data;
using MedicalCompany.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace MedicalCompany.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(
            string Name,
            string Email,
            string? Phone,
            string Message)
        {
            if (string.IsNullOrWhiteSpace(Name) ||
                string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(Message))
            {
                TempData["ContactError"] =
                    "Please fill in your name, email and message.";

                return RedirectToAction(nameof(Contact));
            }

            var enquiry = new Enquiry
            {
                Name = Name.Trim(),
                Email = Email.Trim(),
                Phone = Phone?.Trim() ?? "",
                Message = Message.Trim(),
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.Enquiries.Add(enquiry);
            await _context.SaveChangesAsync();

            TempData["ContactSuccess"] =
                "Thank you! Your message has been submitted successfully.";

            return RedirectToAction(nameof(Contact));
        }

        public IActionResult ProductRange()
        {
            return View();
        }

        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id
                    ?? HttpContext.TraceIdentifier
            });
        }
        [HttpGet]
        public IActionResult GetInTouch()
        {
            return View();
        }
    }
}
