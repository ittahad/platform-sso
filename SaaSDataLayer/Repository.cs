using MongoDB.Driver;

namespace SaaSDataLayer
{
    public class Repository(IUserContextProvider userContextProvider) : IRepository
    {
        public Task<LoanProposal> GetLoanProposal()
        {
            UserContext userContext = userContextProvider.GetUserContext();

            var database = MongoDbConnectionCache.GetVerticalDataContext(userContext.TenantId, userContext.VerticalId);

            var filter = Builders<LoanProposal>.Filter.Empty;

            var sort = Builders<LoanProposal>.Sort.Descending(lp => lp.ApplicationDate);

            return database.GetCollection<LoanProposal>("LoanProposals").Find(filter).Sort(sort).Limit(1).FirstAsync();
        }

        private static readonly string[] FirstNames =
        [
            "Elias", "Julian", "Mateo", "Arthur", "Silas",
            "Clara", "Isla", "Nadia", "Vivian", "Maeve",
            "Rowan", "Quinn", "Avery", "Morgan", "Riley",
            "Kenji", "Amara", "Leila", "Dimitri", "Priya"
        ];

        private static readonly string[] LastNames =
        [
            "Vance", "Thorne", "Reyes", "Finch", "Mercer",
            "Bennett", "Montgomery", "Chen", "Cross", "Sullivan",
            "Hayes", "Delaney", "Sinclair", "Tate", "Sato",
            "Okafor", "Al-Farsi", "Volkov", "Sharma", "Novak"
        ];


        public static string Generate()
        {
            string first = FirstNames[Random.Shared.Next(FirstNames.Length)];
            string last = LastNames[Random.Shared.Next(LastNames.Length)];
            return $"{first} {last}";
        }

        public Task SaveLoanProposal()
        {
            UserContext userContext = userContextProvider.GetUserContext();

            var loanProposal = new LoanProposal
            {
                Id = Guid.NewGuid(),
                ApplicantName = Generate(),
                Amount = Random.Shared.Next(),
                ApplicationDate = DateTime.UtcNow,
                TenantId = userContext.TenantId,
                VerticalId = userContext.VerticalId,
                ClientId = userContext.ClientId,
                LastModifiedBy = userContext.Email
            };

            return MongoDbConnectionCache
                 .GetVerticalDataContext(userContext.TenantId, userContext.VerticalId)
                 .GetCollection<LoanProposal>("LoanProposals")
                 .InsertOneAsync(loanProposal);
        }
    }
}