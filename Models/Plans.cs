using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EBookDashboard.Models
{
    public class Plans
    {
        [Required]
        [Key]
        public int PlanId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public string PlanDescription { get; set; } = string.Empty;
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal PlanRate { get; set; }
        [Required]
        public int PlanDays { get; set; }  // DurationInDays
        public int PlanHours { get; set; }
        
        public int MaxEBooks { get; set; }
        public bool AllowDownloads { get; set; }
        public bool AllowFullDashboard { get; set; }
        public bool AllowAnalytics { get; set; }
        public bool AllowPublishing { get; set; }
        public int IsActive { get; set; } = 1;

        public DateTime CreateddAt { get; set; } = DateTime.UtcNow;
        public int MaxPages { get; set; }
        public int MaxChapters { get; set; }

        public string Currency { get; set; } = "usd";
        // Navigation
        public ICollection<AuthorPlans> AuthorPlans { get; set; } = new List<AuthorPlans>();


    }

}
