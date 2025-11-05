using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EBookDashboard.Models
{
    [Table("planfeatures")]
    public class PlanFeatures
    {
        [Key]
        [Column("FeatureId")]
        public int FeatureId { get; set; }

        [Column("PlanId")]
        public int? PlanId { get; set; }

        [Column("FeatureName")]
        [StringLength(45)]
        public string? FeatureName { get; set; }

        [Column("Description")]
        [StringLength(245)]
        public string? Description { get; set; }

        [Column("FeatureRate", TypeName = "decimal(10,2)")]
        public decimal FeatureRate { get; set; } = 0.00m;

        [Column("Currency")]
        [StringLength(12)]
        public string? Currency { get; set; }

        [Column("Status")]
        [StringLength(12)]
        public string? Status { get; set; }

        [Column("isActive")]
        public int IsActive { get; set; } = 1;

        // ✅ Navigation Property (One-to-Many)
       // public ICollection<AuthorPlanFeatures.AuthorPlanFeatures>? AuthorPlanFeatures { get; set; }
    }
}
