
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MedicalCompany.Data;

namespace MedicalCompany.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? category, string? search)
        {
            var products = _context.Products
                .Include(p => p.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(category))
            {
                products = products.Where(p => p.Category != null &&
                    p.Category.Name == category);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                products = products.Where(p =>
                    p.Name.Contains(search) ||
                    p.ProductCode.Contains(search) ||
                    (p.Description != null && p.Description.Contains(search)));
            }

            ViewBag.Search = search;
            ViewBag.Category = category;

            return View(await products.ToListAsync());
        }
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }
    }
}
