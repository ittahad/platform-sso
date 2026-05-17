namespace SaaSDataLayer
{
    public interface IRepository
    {
        Task SaveLoanProposal();
        Task<LoanProposal> GetLoanProposal();
    }
}