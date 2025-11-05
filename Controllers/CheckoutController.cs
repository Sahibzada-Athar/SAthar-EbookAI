using EBookDashboard.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;

namespace EBookDashboard.Controllers
{
    [Route("[controller]/[action]")]
    public class CheckoutController : Controller
    {
        private readonly ICheckoutService _checkoutService;
        private readonly IConfiguration _config;
        private readonly IAuthorBillsService _authorBillsService;
        private readonly IPlansService _plansService;
        private readonly IAuthorPlansService _authorPlansService;

        public CheckoutController(ICheckoutService checkoutService, IConfiguration config)
        {
            _checkoutService = checkoutService;
            _config = config;
        }

        // FIXED: Changed route to match what JavaScript is calling
        [HttpGet]
        public IActionResult GetPublishableKey()
        {
            var key = _config["Stripe:PublishableKey"];
            if (string.IsNullOrEmpty(key))
                return BadRequest(new { message = "Stripe publishable key not configured." });

            return Ok(new { publishableKey = key });
        }

        [HttpPost]
        public IActionResult CreateSession([FromBody] CheckoutRequest request)
        {
            try
            {
                var secretKey = _config["Stripe:SecretKey"];
                if (string.IsNullOrEmpty(secretKey))
                    return BadRequest(new { message = "Stripe secret key not configured." });

                StripeConfiguration.ApiKey = secretKey;

                if (request.Amount <= 0)
                {
                    return BadRequest(new { message = "Invalid amount." });
                }

                var domain = $"{Request.Scheme}://{Request.Host.Value}";

                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = new List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                UnitAmount = request.Amount,
                                Currency = request.Currency,
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = request.ProductName,
                                    Description = request.ProductDescription
                                }
                            },
                            Quantity = 1
                        }
                    },
                    Mode = "payment",
                    SuccessUrl = $"{domain}/Checkout/Success?session_id={{CHECKOUT_SESSION_ID}}",
                    CancelUrl = $"{domain}/Checkout/Cancel",
                    ClientReferenceId = request.AuthorPlanId?.ToString() // Track which plan is being purchased
                };

                var service = new SessionService();
                var session = service.Create(options);

                return Ok(new { sessionId = session.Id });
            }
            catch (StripeException ex)
            {
                return BadRequest(new { message = $"Stripe error: {ex.Message}" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Server error: {ex.Message}" });
            }
        }

        [HttpGet]
        public IActionResult Success(string session_id)
        {
            ViewBag.SessionId = session_id;
            return View();
        }

        [HttpGet]
        public IActionResult Cancel()
        {
            return View();
        }
    }

    public class CheckoutRequest
    {
        public string ProductName { get; set; } = string.Empty;
        public string ProductDescription { get; set; } = string.Empty;
        public long Amount { get; set; }
        public string Currency { get; set; } = "usd";
        public int? AuthorPlanId { get; set; }
    }
}

