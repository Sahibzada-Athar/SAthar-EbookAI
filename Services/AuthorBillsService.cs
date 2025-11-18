using EBookDashboard.Interfaces;
using EBookDashboard.Models;
using EBookDashboard.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace EBookDashboard.Services
{
    public class AuthorBillsService : IAuthorBillsService
    {
        private readonly ApplicationDbContext _context;

        public AuthorBillsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AuthorBills?> GetBillByIdAsync(int billId)
        {
            return await _context.AuthorBills
                .Include(b => b.AuthorPlanFeatures)
                    .ThenInclude(apf => apf.PlanFeature)
                .FirstOrDefaultAsync(b => b.BillId == billId && b.IsActive==1);
        }

        public async Task<AuthorBills?> GetRecentBillByUserAsync(int userId, string userEmail)
        {
            var today = DateTime.Today;

            return await _context.AuthorBills
                .Include(b => b.AuthorPlanFeatures)
                    .ThenInclude(apf => apf.PlanFeature)
                .Where(b => b.UserId == userId &&
                           b.UserEmail == userEmail &&
                           b.CreatedAt.Date == today &&
                           b.IsActive == 1)
                .OrderByDescending(b => b.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<AuthorBills>> GetBillsByAuthorAsync(int authorId)
        {
            return await _context.AuthorBills
                .Include(b => b.AuthorPlanFeatures)
                    .ThenInclude(apf => apf.PlanFeature)
                .Where(b => b.AuthorId == authorId && b.IsActive == 1)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        // Optional: Get all bills for an author
        public async Task<IEnumerable<AuthorBills>> GetAuthorBillsAsync(int authorId)
        {
            return await _context.AuthorBills
                .Where(b => b.AuthorId == authorId && b.IsActive == 1)
                .Include(b => b.AuthorPlanFeatures)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }
        // Optional: Get bill with features
        public async Task<AuthorBills> GetAuthorBillWithFeaturesAsync(int billId)
        {
            var bill = await _context.AuthorBills
             .Include(b => b.AuthorPlanFeatures)
             .ThenInclude(apf => apf.PlanFeature)
             .FirstOrDefaultAsync(b => b.BillId == billId && b.IsActive == 1);

            return bill ?? throw new Exception($"Bill with ID {billId} not found");
        }


        public async Task<bool> UpdateBillPaymentStatusAsync(int billId, string paymentReference, string status)
        {
            var bill = await GetBillByIdAsync(billId);
            if (bill == null) return false;

            bill.PaymentReference = paymentReference;
            bill.Status = status;

            if (status == "Paid")
            {
                bill.ClosingDate = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelBillAsync(int billId, string cancellationReason)
        {
            var bill = await GetBillByIdAsync(billId);
            if (bill == null) return false;

            bill.Status = "Cancelled";
            bill.CancellationReason = cancellationReason;
            bill.CancelledAt = DateTime.Now;
            bill.IsActive = 0;

            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<UserPlansViewModel> GetUserPlansWithFeaturesAsync(int userId, string userEmail)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId && u.UserEmail == userEmail);

            if (user == null)
                throw new Exception("User not found");

            var bills = await _context.AuthorBills
                .Where(b => b.UserId == userId && b.UserEmail == userEmail && b.IsActive == 1)
                .Include(b => b.AuthorPlanFeatures)
                    .ThenInclude(apf => apf.PlanFeature)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            var viewModel = new UserPlansViewModel
            {
                UserId = user.UserId,
                UserName = user.FullName,
                UserEmail = user.UserEmail,
                LastLoginAt = user.LastLoginAt,
                Status = user.Status
            };

            foreach (var bill in bills)
            {
                var userBill = new UserPlanBill
                {
                    BillId = bill.BillId,
                    Description = bill.Description,
                    CreatedAt = bill.CreatedAt,
                    ClosingDate = bill.ClosingDate,
                    Currency = bill.Currency ?? "usd",
                    TotalAmount = bill.TotalAmount,
                    TaxAmount = bill.TaxAmount,
                    Discount = bill.Discount,
                    PaymentReference = bill.PaymentReference ?? "",
                    Status = bill.Status ?? "Pending"
                };

                // Add features from AuthorPlanFeatures
                foreach (var authorFeature in bill.AuthorPlanFeatures)
                {
                    if (authorFeature.PlanFeature != null)
                    {
                        userBill.Features.Add(new PlanFeatureDetail
                        {
                            FeatureId = authorFeature.PlanFeature.FeatureId,
                            FeatureName = authorFeature.PlanFeature.FeatureName,
                            FeatureDescription = authorFeature.PlanFeature.Description,
                            FeatureType = authorFeature.PlanFeature.FeatureType,
                           // Value = authorFeature.PlanFeature.Value,
                            IsUnlimited = authorFeature.PlanFeature.IsUnlimited,
                            ExpiryDate = authorFeature.ExpiryDate
                        });
                    }
                }

                viewModel.Bills.Add(userBill);
            }

            // Calculate statistics
            viewModel.Statistics = CalculatePlanStatistics(viewModel.Bills);

            return viewModel;
        }

        private PlanStatistics CalculatePlanStatistics(List<UserPlanBill> bills)
        {
            var stats = new PlanStatistics
            {
                TotalBills = bills.Count,
                ActiveBills = bills.Count(b => b.IsActive),
                TotalFeatures = bills.Sum(b => b.Features.Count),
                ActiveFeatures = bills.Sum(b => b.Features.Count(f => f.IsActive)),
                TotalSpent = bills.Where(b => b.Status == "Paid").Sum(b => b.NetAmount),
                MonthlySpent = bills
                    .Where(b => b.Status == "Paid" && b.CreatedAt >= DateTime.Now.AddMonths(-1))
                    .Sum(b => b.NetAmount)
            };

            // Find most used feature
            var featureUsage = bills
                .SelectMany(b => b.Features)
                .GroupBy(f => f.FeatureName)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault();

            stats.MostUsedFeature = featureUsage?.Key ?? "None";

            return stats;
        }


    }
}