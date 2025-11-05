using System.ComponentModel.DataAnnotations;

namespace EBookDashboard.Models.ViewModels
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
}