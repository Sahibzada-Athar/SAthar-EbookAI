using EBookDashboard.Interfaces;
using EBookDashboard.Models;
using Microsoft.EntityFrameworkCore;

namespace EBookDashboard.Services
{
    public class PlanFeaturesService : IPlanFeaturesService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthorBillsService _authorBillsService;
        public PlanFeaturesService(ApplicationDbContext context, IAuthorBillsService authorBillsService)
        {
            _context = context;
            _authorBillsService = authorBillsService;
        }
        // ----- Features -----
        public async Task<IEnumerable<PlanFeatures>> GetAllFeaturesAsync()
        {
            // Return all features (active and inactive) for complete admin oversight
            return await _context.PlanFeatures
                .OrderBy(f => f.FeatureId)
                .ToListAsync();
        }
        public async Task AddFeatureAsync(PlanFeatures feature)
        {
            _context.PlanFeatures.Add(feature);
            await _context.SaveChangesAsync();  // this actually commits to DB
        }
        // ----- Author Plan Features -----
        public async Task<int> SaveAuthorPlanFeaturesAsync(int authorId, int userId, string userEmail, List<int> featureIds)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Get feature details from database
                var features = await _context.PlanFeatures
                    .Where(f => featureIds.Contains(f.FeatureId) && f.IsActive == 1)
                    .ToListAsync();

                if (!features.Any())
                {
                    throw new Exception("No valid features found");
                }

                // Calculate total amount
                var totalAmount = features.Sum(f => f.FeatureRate);
                var currency = features.First().Currency ?? "usd";
                
                // 1. Create AuthorBill (Master record)
                var authorBill = new AuthorBills
                {
                    AuthorId = authorId,
                    UserId = userId,
                    UserEmail = userEmail,
                    Description = $"Plan Features Purchase - {features.Count} features",
                    CreatedAt = DateTime.Now,
                    Currency = currency,
                    Discount = 0,
                    TotalAmount = totalAmount,
                    PaymentReference = string.Empty,
                    CancelledAt = new DateTime(1990, 1, 1),
                    CancellationReason = null,
                    Status = "Pending",
                    ClosingDate = DateTime.Now.AddDays(30), // 30 days validity
                    IsActive = 1
                };

                await _context.AuthorBills.AddAsync(authorBill);
                await _context.SaveChangesAsync(); // Save to get BillId

                // 2. Create AuthorPlanFeatures records (Detail records)
                var authorPlanFeatures = new List<AuthorPlanFeatures>();

                foreach (var feature in features)
                {
                    var authorPlanFeature = new AuthorPlanFeatures
                    {
                        BillId = authorBill.BillId, // Set the foreign key
                        AuthorId = authorId,
                        UserId = userId,
                        UserEmail = userEmail,
                        FeatureId = feature.FeatureId,
                        PlanId = feature.PlanId,
                        FeatureName = feature.FeatureName,
                        Description = feature.Description,
                        FeatureRate = feature.FeatureRate,
                        Currency = feature.Currency,
                        TotalAmount = totalAmount,
                        CreatedAt = DateTime.Now,  
                        Status = "Pending",
                        IsActive = 1
                    };

                    authorPlanFeatures.Add(authorPlanFeature);
                   // totalAmount += feature.FeatureRate;
                }

                // If you want to store total amount in each record
                //foreach (var feature in authorPlanFeatures)
                //{
                //    feature.TotalAmount = totalAmount;
                //}

                await _context.AuthorPlanFeaturesSet.AddRangeAsync(authorPlanFeatures);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return authorBill.BillId; // Return the generated BillId
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // Optional: Get saved author features
        public async Task<IEnumerable<AuthorPlanFeatures>> GetAuthorPlanFeaturesAsync(int authorId)
        {
            return await _context.AuthorPlanFeaturesSet
                .Where(apf => apf.AuthorId == authorId && apf.IsActive == 1)
                .Include(apf => apf.PlanFeature)
                .Include(apf => apf.AuthorBill) // Include the bill details
                .OrderByDescending(apf => apf.CreatedAt)
                .ToListAsync();
        }

        public Task<IEnumerable<AuthorBills>> GetAuthorBillsAsync(int authorId)
        {
            throw new NotImplementedException();
        }

        public Task<AuthorBills?> GetAuthorBillWithFeaturesAsync(int billId)
        {
            throw new NotImplementedException();
        }
    }
}
