namespace EBookDashboard.Models.ViewModels
{
    public class InvoiceViewModel
    {
        public string InvoiceId { get; set; }
        public DateTime InvoiceDate { get; set; }
        public List<SelectedFeature> SelectedFeatures { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal GrandTotal { get; set; }
    }
}
