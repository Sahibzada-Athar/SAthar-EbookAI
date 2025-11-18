using System.ComponentModel.DataAnnotations;

namespace EBookDashboard.Models
{
    public class DashboardIndexViewModel
    {
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public int TotalBooksPublished { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public int TotalDownloads { get; set; }
        public decimal AverageRating { get; set; }
        public List<ProjectViewModel> CurrentProjects { get; set; } = new List<ProjectViewModel>();
        public List<ActivityViewModel> RecentActivities { get; set; } = new List<ActivityViewModel>();
        public BookViewModel CurrentWorkingBook { get; set; } = new BookViewModel();
    }

    public class DashboardProfileViewModel
    {
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
        public DateTime MemberSince { get; set; }
        public string Country { get; set; } = string.Empty;
        public int TotalBooks { get; set; }
        public int BooksReading { get; set; }
        public int Reviews { get; set; }
        public int BooksRead { get; set; }
        public int ReadingHours { get; set; }
        public int PagesRead { get; set; }
        public int ReadingStreak { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public decimal PlanPrice { get; set; }
        public DateTime NextBillingDate { get; set; }
        public string BillingCycle { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public List<PlanFeatureViewModel> PlanFeatures { get; set; } = new List<PlanFeatureViewModel>();
    }

    public class ProjectViewModel
    {
        public int BookId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int ProgressPercentage { get; set; }
        public string ProgressText { get; set; } = string.Empty;
    }

    public class ActivityViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string TimeAgo { get; set; } = string.Empty;
        public string IconClass { get; set; } = string.Empty;
    }

    public class BookViewModel
    {
        public int BookId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string BookIdText { get; set; } = string.Empty;
        public int ProgressPercentage { get; set; }
        public string ProgressText { get; set; } = string.Empty;
    }

    public class PlanFeatureViewModel
    {
        public string Name { get; set; } = string.Empty;
        public bool IsIncluded { get; set; }
    }

    public class DashboardProjectViewModel
    {
        public int BookId { get; set; }
        public string? Title { get; set; }
        public string? Status { get; set; }
        public int ProgressPercentage { get; set; }
        public string? ProgressText { get; set; }
    }

    // New ViewModel for Payment Summary
    public class PaymentSummaryViewModel
    {
        public List<CartItemViewModel> CartItems { get; set; } = new List<CartItemViewModel>();
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }
    }

    public class CartItemViewModel
    {
        public int FeatureId { get; set; }
        public string FeatureName { get; set; } = "";
        public string Description { get; set; } = "";
        public decimal FeatureRate { get; set; }
    }

    // Admin Dashboard ViewModels
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalBooks { get; set; }
        public decimal TotalRevenue { get; set; }
        public int ActiveUsers { get; set; }
        public List<UserManagementViewModel> RecentUsers { get; set; } = new List<UserManagementViewModel>();
        public List<BookManagementViewModel> RecentBooks { get; set; } = new List<BookManagementViewModel>();
        public AnalyticsViewModel Analytics { get; set; } = new AnalyticsViewModel();
    }

    public class UserManagementViewModel
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public string SignupMethod { get; set; } = "Manual"; // Manual, Google, Facebook
        public int ProfileCompletionPercentage { get; set; }
        public int BooksCreated { get; set; }
    }

    public class BookManagementViewModel
    {
        public int BookId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // Draft, Styled, Previewed, Generated, Published
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? CoverImagePath { get; set; }
        public int WordCount { get; set; }
        public string Genre { get; set; } = string.Empty;
        public int UserId { get; set; }
        public int AuthorId { get; set; }
    }

    public class ProgressTrackerViewModel
    {
        public Dictionary<string, int> UsersByPhase { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> BooksByPhase { get; set; } = new Dictionary<string, int>();
        public List<UserJourneyViewModel> UserJourneys { get; set; } = new List<UserJourneyViewModel>();
        public List<BookProgressViewModel> BookProgresses { get; set; } = new List<BookProgressViewModel>();
    }

    public class UserJourneyViewModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string CurrentPhase { get; set; } = string.Empty;
        public DateTime SignupDate { get; set; }
        public DateTime? DraftDate { get; set; }
        public DateTime? GeneratedDate { get; set; }
        public DateTime? PublishedDate { get; set; }
        public int DaysInCurrentPhase { get; set; }
    }

    public class BookProgressViewModel
    {
        public int BookId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string CurrentPhase { get; set; } = string.Empty;
        public int ProgressPercentage { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUpdated { get; set; }
    }

    public class AnalyticsViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalBooks { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalActivity { get; set; }
        public List<TimeSeriesData> SignupsOverTime { get; set; } = new List<TimeSeriesData>();
        public List<TimeSeriesData> BookCreationsOverTime { get; set; } = new List<TimeSeriesData>();
        public List<TimeSeriesData> PurchasesOverTime { get; set; } = new List<TimeSeriesData>();
        public ConversionFunnelViewModel ConversionFunnel { get; set; } = new ConversionFunnelViewModel();
    }

    public class TimeSeriesData
    {
        public string Date { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    public class ConversionFunnelViewModel
    {
        public int Signups { get; set; }
        public int Drafts { get; set; }
        public int Styled { get; set; }
        public int Previewed { get; set; }
        public int Generated { get; set; }
        public int Published { get; set; }
        public decimal SignupToDraftRate { get; set; }
        public decimal DraftToPublishedRate { get; set; }
    }
}