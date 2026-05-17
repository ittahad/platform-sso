namespace SaaSDataLayer
{
    public class LoanProposal
    {
        public Guid Id { get; set; }
        public string ApplicantName { get; set; }
        public decimal Amount { get; set; }
        public DateTime ApplicationDate { get; set; }
        public string TenantId { get; set; }
        public string VerticalId { get; set; }
        public string ClientId { get; set; }
    }

}