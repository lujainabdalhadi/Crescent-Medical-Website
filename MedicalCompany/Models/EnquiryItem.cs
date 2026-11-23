
namespace MedicalCompany.Models
{
    public class EnquiryItem
    {
        public int EnquiryItemId { get; set; }

        public int EnquiryId { get; set; }

        public int ProductId { get; set; }

        public int Quantity { get; set; } = 1;

        public Enquiry Enquiry { get; set; } = null!;

        public Product Product { get; set; } = null!;
    }
}
