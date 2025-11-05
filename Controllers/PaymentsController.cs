using EBookDashboard.Interfaces;
using EBookDashboard.Models;
using EBookDashboard.Models.ViewModels;
using EBookDashboard.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Stripe;
using Stripe.Checkout;
using Stripe.Forwarding;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Metadata;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace EBookDashboard.Controllers
{
    public class PaymentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPlanFeaturesService _planFeaturesService;
        private readonly IPlansService _plansService;
        private readonly IAuthorPlansService _authorPlansService;
        private readonly IAuthorBillsService _authorBillsService;
        public PaymentsController(ApplicationDbContext context, IAuthorBillsService authorBillsService)
        {
            _context = context;
            _authorBillsService = authorBillsService;
        }
        // Confirm Payment
        [HttpPost]
        public IActionResult ProcessPayment(int authorPlanId)
        {
            var plan = _context.AuthorPlans.Find(authorPlanId);
            if (plan == null) return NotFound();

            // TODO: Replace with real Stripe/PayPal payment logic
            plan.PaymentReference = Guid.NewGuid().ToString();
            plan.IsActive = 1;

            _context.SaveChanges();

            return RedirectToAction("Index", "Dashboard");
        }
        //=============================================
        //       Checkout Payment via AuthorBills
        //============================================
        [HttpGet]
        public async Task<IActionResult> CheckoutPayment(int? billId = null, string featureIds = null)
        {
            if (!billId.HasValue || billId.Value <= 0)
            {
                return BadRequest("Invalid bill ID");
            }
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.FullName == username);
            if (user == null)
            {
                return Unauthorized();
            }
            int userId = user.UserId;
            int authorId = user.UserId;

            // Get the bill from service
            var authorBill = await _authorBillsService.GetBillByIdAsync(billId.Value);
            if (authorBill == null)
            {
                return NotFound("Bill not found");
            }
            // Verify ownership
            if (authorBill.UserId != userId)
            {
                return Unauthorized("This bill does not belong to you");
            }
            // Return the view with the bill
            return View("CheckoutPayment", authorBill);
         }
        //=======================================
        //           Start Checkout
        //=======================================
        public async Task<IActionResult> Checkout1(int planId = 0, int? billId = null)
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.FullName == username);
            if (user == null)
            {
                return Unauthorized();
            }

            int userId = user.UserId;
            int authorId = user.UserId;

            // If billId is provided, use AuthorBills (for features checkout)
            if (billId.HasValue && billId.Value > 0)
            {
                var authorBill = await _authorBillsService.GetBillByIdAsync(billId.Value);
                if (authorBill == null)
                {
                    return NotFound("Bill not found");
                }

                // Verify ownership
                if (authorBill.UserId != userId || authorBill.AuthorId != authorId)
                {
                    return Unauthorized("This bill does not belong to you");
                }

                return View("Checkout", new CheckoutViewModel
                {
                    Bill = authorBill,
                    Plan = null,
                    Type = "features"
                });
            }
            // If no billId but we have user info, try to find recent bill
            else if (planId == 0)
            {
                var recentBill = await _authorBillsService.GetRecentBillByUserAsync(userId, user.UserEmail);
                if (recentBill != null)
                {
                    return View("Checkout", new CheckoutViewModel
                    {
                        Bill = recentBill,
                        Plan = null,
                        Type = "features"
                    });
                }
                else
                {
                    return BadRequest("No bill found. Please select features first.");
                }
            }
            // Otherwise, use Plans (for plan checkout)
            else
            {
                var planDetails = await _plansService.GetPlanByIdAsync(planId);
                if (planDetails == null)
                {
                    return NotFound("Plan not found");
                }

                var authorPlanId = await _authorPlansService.CreateAuthorPlanAsync(authorId, userId, planId);
                var authorPlan = await _authorPlansService.GetAuthorPlanByIdAsync(authorPlanId);

                if (authorPlan == null)
                {
                    return StatusCode(500, "Failed to create author plan");
                }

                return View("Checkout", new CheckoutViewModel
                {
                    Bill = null,
                    Plan = authorPlan,
                    Type = "plan"
                });
            }
        }

        // Start Checkout
       
        // Start Checkout
        public async Task<IActionResult> Checkout(int planId)
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }

            var user = _context.Users.FirstOrDefault(u => u.FullName == username);
            if (user == null)
            {
                return Unauthorized();
            }

            int userId = user.UserId;
            int authorId = user.UserId;
            // Fetch plan from DB instead of helper method
            var planDetails = await _context.Plans.FirstOrDefaultAsync(p => p.PlanId == planId);
            //var planDetails = await _context.Plans.FirstOrDefault(p => p.PlanId == planId);
            if (planDetails == null)
            {
                return NotFound();
            }

            // Create AuthorPlan record
            var authorPlan = new AuthorPlans
            {
                AuthorId = authorId,
                UserId = userId,
                PlanId = planDetails.PlanId,
                PlanName = planDetails.PlanName,
                PlanDescription = planDetails.PlanDescription,
                PlanRate = planDetails.PlanRate,
                PlanDays = planDetails.PlanDays,
                PlanHours = 0,
                MaxEBooks = planDetails.MaxEBooks,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(planDetails.PlanDays),
                IsActive = 1,
                TrialUsed = false,
                PaymentReference = null
            };

            _context.AuthorPlans.Add(authorPlan);
            _context.SaveChanges();

            // Send to checkout view
            return View("Checkout", authorPlan);
        }
        // Show all active plan features as cards
        public async Task<IActionResult> Index()
        {
            // This is the normal page view (if you visit /Payments/Index directly)
            // Fetch all features from your PlanFeatures table
            var features = await _context.PlanFeatures.ToListAsync();

            // Pass the data to the Razor view
            return View(features);
        }

        [HttpGet]
        public async Task<IActionResult> LoadFeatureLocks()
        {
            // Return your Razor partial view (or the same Index view if you prefer)
            var features = await _context.PlanFeatures.ToListAsync();
            return PartialView("_FeatureLocks", features); // ✅ loads the new partial view
        }

        [HttpPost]
        public IActionResult GenerateInvoice(List<int> SelectedFeatureIds, decimal TotalAmount, string SelectedFeaturesJson)
        {
            try
            {
                // Deserialize the selected features
                var selectedFeatures = JsonSerializer.Deserialize<List<SelectedFeature>>(SelectedFeaturesJson);

                // Calculate tax and grand total (example: 10% tax)
                decimal taxRate = 0.10m;
                decimal taxAmount = TotalAmount * taxRate;
                decimal grandTotal = TotalAmount + taxAmount;

                var invoiceModel = new InvoiceViewModel
                {
                    InvoiceId = "INV-" + DateTime.Now.ToString("yyyyMMdd") + "-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                    InvoiceDate = DateTime.Now,
                    SelectedFeatures = selectedFeatures,
                    TotalAmount = TotalAmount,
                    TaxAmount = taxAmount,
                    GrandTotal = grandTotal
                };

                return View(invoiceModel);
            }
            catch (Exception ex)
            {
                // Log error
                //_logger.LogError(ex, "Error generating invoice");

                TempData["Error"] = "Error generating invoice. Please try again.";
                return RedirectToAction("LoadFeatureLocks");
            }
        }
    }

}