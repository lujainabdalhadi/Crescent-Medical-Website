
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using MedicalCompany.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using MedicalCompany.Models;
using System.IO;
using System.Linq;

namespace MedicalCompany.Controllers
{
 
    public class AdminController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly MedicalCompany.Services.EmailService _emailService;

        public AdminController(
            ApplicationDbContext context,
            MedicalCompany.Services.EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            // Temporary demo credentials.
            // We will move these to secure configuration next.
            if (username == "Admin" && password == "admin1986")
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Role, "Admin")
                };

                var identity = new ClaimsIdentity(
                    claims, "AdminCookie");

                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(
                    "AdminCookie", principal);

                return RedirectToAction("Dashboard");
            }

            ViewBag.Error = "Invalid username or password.";
            return View();
        }

        [Authorize(Roles = "Admin", AuthenticationSchemes = "AdminCookie")]
        public IActionResult Dashboard()
        {
            return View();
        }

        [Authorize(Roles = "Admin", AuthenticationSchemes = "AdminCookie")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("AdminCookie");
            return RedirectToAction("Login");
        }

        [Microsoft.AspNetCore.Authorization.Authorize(
            Roles = "Admin",
            AuthenticationSchemes = "AdminCookie")]
        public async Task<IActionResult> Products()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .OrderBy(p => p.Name)
                .ToListAsync();

            return View(products);
        }

        [Authorize(
            Roles = "Admin",
            AuthenticationSchemes = "AdminCookie")]
        [HttpGet]
        public async Task<IActionResult> CreateProduct()
        {
            ViewBag.Categories = await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View();
        }

        [Authorize(
            Roles = "Admin",
            AuthenticationSchemes = "AdminCookie")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(
            Product product,
            IFormFile? imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                var extension = Path.GetExtension(imageFile.FileName)
                    .ToLowerInvariant();

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(
                        "imageFile",
                        "Only JPG, PNG, and WEBP images are allowed.");
                }
            }
            product.ImageUrl ??= "";
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName).ToLowerInvariant()}";
                    var folder = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        "products");

                    Directory.CreateDirectory(folder);

                    var filePath = Path.Combine(folder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    product.ImageUrl = "/products/" + fileName;
                }

                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Product added successfully!";
                return RedirectToAction("Products");
            }


            ViewBag.Categories = await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();

            var errors = ModelState
                .Where(x => x.Value != null && x.Value.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors)
                .Select(x => x.ErrorMessage);

            ViewBag.ErrorMessage = string.Join(" | ", errors);

            return View(product);

        }

        [Authorize(Roles = "Admin", AuthenticationSchemes = "AdminCookie")]
        [HttpGet]
        public async Task<IActionResult> EditProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            ViewBag.Categories = await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(product);
        }

        [Authorize(Roles = "Admin", AuthenticationSchemes = "AdminCookie")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(
            int id, Product product, IFormFile? imageFile)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            var existingProduct = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (existingProduct == null)
            {
                return NotFound();
            }

            product.ImageUrl = existingProduct.ImageUrl ?? "";

            if (imageFile != null && imageFile.Length > 0)
            {
                var extension = Path.GetExtension(imageFile.FileName)
                    .ToLowerInvariant();

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(
                        "imageFile",
                        "Only JPG, PNG, and WEBP images are allowed.");
                }
            }

            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    var extension = Path.GetExtension(imageFile.FileName)
                        .ToLowerInvariant();

                    var fileName = $"{Guid.NewGuid()}{extension}";
                    var folder = Path.Combine(
                        Directory.GetCurrentDirectory(), "wwwroot", "products");

                    Directory.CreateDirectory(folder);

                    var filePath = Path.Combine(folder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    product.ImageUrl = "/products/" + fileName;
                }

                _context.Products.Update(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Product updated successfully!";
                return RedirectToAction("Products");
            }

            ViewBag.Categories = await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(product);
        }

        [Authorize(Roles = "Admin", AuthenticationSchemes = "AdminCookie")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Product deleted successfully!";
            return RedirectToAction("Products");
        }


        [Authorize(Roles = "Admin", AuthenticationSchemes = "AdminCookie")]
        public async Task<IActionResult> Categories()
        {
            var categories = await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(categories);
        }

        [Authorize(Roles = "Admin", AuthenticationSchemes = "AdminCookie")]
        [HttpGet]
        public IActionResult CreateCategory()
        {
            return View();
        }


        [Authorize(Roles = "Admin", AuthenticationSchemes = "AdminCookie")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(Category category)
        {
            category.ImageUrl ??= "";
            category.Description ??= "";

            ModelState.Remove(nameof(Category.ImageUrl));
            ModelState.Remove(nameof(Category.Products));

            if (ModelState.IsValid)
            {
                category.CreatedAt = DateTime.Now;
                category.IsActive = true;

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Category added successfully!";
                return RedirectToAction("Categories");
            }

            return View(category);
        }

        [Authorize(Roles = "Admin", AuthenticationSchemes = "AdminCookie")]
        [HttpGet]
        public async Task<IActionResult> EditCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [Authorize(Roles = "Admin", AuthenticationSchemes = "AdminCookie")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(int id, Category category)
        {
            if (id != category.CategoryId)
            {
                return NotFound();
            }

            var existingCategory = await _context.Categories.FindAsync(id);

            if (existingCategory == null)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(Category.Products));
            ModelState.Remove(nameof(Category.ImageUrl));

            if (ModelState.IsValid)
            {
                existingCategory.Name = category.Name;
                existingCategory.Description = category.Description;
                existingCategory.ImageUrl = category.ImageUrl ?? "";
                existingCategory.IsActive = category.IsActive;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Category updated successfully!";
                return RedirectToAction("Categories");
            }

            return View(category);
        }


        [Authorize(Roles = "Admin", AuthenticationSchemes = "AdminCookie")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
            {
                return NotFound();
            }

            if (category.Products != null && category.Products.Any())
            {
                TempData["ErrorMessage"] =
                    "This category cannot be deleted because it contains products.";

                return RedirectToAction("Categories");
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Category deleted successfully!";

            return RedirectToAction("Categories");
        }


        [Authorize(Roles = "Admin", AuthenticationSchemes = "AdminCookie")]
        [HttpGet]
        public async Task<IActionResult> Enquiries()
        {
            var enquiries = await _context.Enquiries
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();

            ViewBag.EnquiryItems = await _context.EnquiryItems
                .Include(i => i.Product)
                .ToListAsync();

            return View(enquiries);
        }


        [Authorize(Roles = "Admin", AuthenticationSchemes = "AdminCookie")]
        [HttpGet]
        public async Task<IActionResult> ReplyEnquiry(int id)
        {
            var enquiry = await _context.Enquiries
                .FirstOrDefaultAsync(e => e.EnquiryId == id);

            if (enquiry == null)
            {
                return NotFound();
            }

            return View(enquiry);
        }


        [Authorize(Roles = "Admin", AuthenticationSchemes = "AdminCookie")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReplyEnquiry(int id, string reply)
        {
            var enquiry = await _context.Enquiries
                .FirstOrDefaultAsync(e => e.EnquiryId == id);

            if (enquiry == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(reply))
            {
                ViewBag.ErrorMessage = "Please enter your reply.";
                return View(enquiry);
            }

            if (reply.Length > 5000)
            {
                ViewBag.ErrorMessage = "The reply cannot exceed 5000 characters.";
                return View(enquiry);
            }

            enquiry.AdminReply = reply.Trim();
            enquiry.RepliedAt = DateTime.UtcNow;
            enquiry.Status = "Replied";

            await _context.SaveChangesAsync();

            try
            {
                await _emailService.SendReplyAsync(
                    enquiry.Email,
                    enquiry.Name,
                    reply.Trim());

                TempData["SuccessMessage"] =
                    "Reply saved and email sent successfully!";
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] =
                    "Reply saved, but the email could not be sent. Please check your email settings.";
            }

            return RedirectToAction(nameof(Enquiries));
        }


    }
}
