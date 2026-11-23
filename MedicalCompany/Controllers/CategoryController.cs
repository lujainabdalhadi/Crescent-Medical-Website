using Microsoft.AspNetCore.Mvc;
using MedicalCompany.Data;
using Microsoft.EntityFrameworkCore;

namespace MedicalCompany.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // صفحة إدارة التصنيفات - تبقى كما هي
        public IActionResult Index()
        {
            var categories = _context.Categories.ToList();
            return View(categories);
        }

        // صفحة عرض تصنيف محدد للزوار
        public IActionResult Products(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return NotFound();

            var category = _context.Categories
                .FirstOrDefault(c => c.Name == name);

            if (category == null)
                return NotFound();

            return View(category);
        }
    }
}