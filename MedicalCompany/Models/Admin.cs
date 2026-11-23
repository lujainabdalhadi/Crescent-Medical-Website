using System.ComponentModel.DataAnnotations;

namespace MedicalCompany.Models
{
    public class Admin
    {
        [Key]
        public int AdminId { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public string PasswordHash { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
    }

}
