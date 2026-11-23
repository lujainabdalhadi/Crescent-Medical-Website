
using System.ComponentModel.DataAnnotations;

namespace MedicalCompany.Models
{
    public class Enquiry
    {
        public int EnquiryId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Phone { get; set; } = string.Empty;

        public string? Message { get; set; }

        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; }
        public string? AdminReply { get; set; }

        public DateTime? RepliedAt { get; set; }
    }
}
