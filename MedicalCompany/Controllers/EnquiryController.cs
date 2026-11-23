
using System.Text.Json;
using MedicalCompany.Data;
using MedicalCompany.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicalCompany.Controllers
{
    public class EnquiryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string CartKey = "EnquiryCart";

        public EnquiryController(ApplicationDbContext context)
        {
            _context = context;
        }

        private List<EnquiryItem> GetCart()
        {
            return JsonSerializer.Deserialize<List<EnquiryItem>>(
                HttpContext.Session.GetString(CartKey) ?? "[]"
            ) ?? new List<EnquiryItem>();
        }

        private void SaveCart(List<EnquiryItem> cart)
        {
            HttpContext.Session.SetString(
                CartKey, JsonSerializer.Serialize(cart));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.ProductId == id);

            if (item == null)
            {
                cart.Add(new EnquiryItem
                {
                    ProductId = id,
                    Quantity = 1
                });
            }
            else
            {
                item.Quantity++;
            }

            SaveCart(cart);
            return Json(new
            {
                success = true,
                count = GetCart().Sum(item => item.Quantity)
            });
        }

        public async Task<IActionResult> Index()
        {
            var cart = GetCart();
            var ids = cart.Select(x => x.ProductId).ToList();

            var products = await _context.Products
                .Where(p => ids.Contains(p.ProductId))
                .ToListAsync();

            ViewBag.Cart = cart;
            return View(products);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(int id, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.ProductId == id);

            if (item != null)
            {
                if (quantity <= 0)
                    cart.Remove(item);
                else
                    item.Quantity = quantity;

                SaveCart(cart);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(int id)
        {
            var cart = GetCart();
            cart.RemoveAll(x => x.ProductId == id);
            SaveCart(cart);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Send()
        {
            if (!GetCart().Any())
                return RedirectToAction(nameof(Index));

            return View(new Enquiry());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(Enquiry enquiry)
        {
            var cart = GetCart();

            if (!cart.Any())
                return RedirectToAction(nameof(Index));
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .SelectMany(x => x.Value!.Errors)
                    .Select(x => x.ErrorMessage);

                ViewBag.Errors = string.Join(" | ", errors);
                return View(enquiry);
            }
            enquiry.Status = "Pending";
            enquiry.CreatedAt = DateTime.UtcNow;

            _context.Enquiries.Add(enquiry);
            await _context.SaveChangesAsync();

            foreach (var item in cart)
            {
                _context.EnquiryItems.Add(new EnquiryItem
                {
                    EnquiryId = enquiry.EnquiryId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                });
            }

            await _context.SaveChangesAsync();

            HttpContext.Session.Remove(CartKey);

            return RedirectToAction(nameof(Success));
        }

        public IActionResult Success()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Preview()
        {
            var cart = GetCart();
            var ids = cart.Select(x => x.ProductId).ToList();

            var products = await _context.Products
                .Where(p => ids.Contains(p.ProductId))
                .ToListAsync();

            var result = products.Select(p => new
            {
                name = p.Name,
                productCode = p.ProductCode,
                imageUrl = p.ImageUrl,
                quantity = cart.First(x => x.ProductId == p.ProductId).Quantity
            });

            return Json(result);
        }

    }
}
