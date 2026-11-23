using Microsoft.EntityFrameworkCore;
using MedicalCompany.Models;

namespace MedicalCompany.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Admin> Admins { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Product> Products { get; set; }

        public DbSet<Enquiry> Enquiries { get; set; }

        public DbSet<EnquiryItem> EnquiryItems { get; set; }
    }
}