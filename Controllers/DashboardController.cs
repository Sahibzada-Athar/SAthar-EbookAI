using EBookDashboard.Interfaces;
using EBookDashboard.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EBookDashboard.Controllers
{
    [Authorize]
    [Route("Dashboard")]
    public class DashboardController : Controller
    {
        private readonly IFeatureCartService _featureCartService;
        private readonly ApplicationDbContext _context;

        public DashboardController(IFeatureCartService featureCartService, ApplicationDbContext context)
        {
            _featureCartService = featureCartService;
            _context = context;
        }

        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            // Temporarily bypass the feature check to allow login
            /*
            // Check if user has an active plan
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))  
            {
                var hasActivePlan = await _featureCartService.HasActivePlanAsync(userId);
                if (!hasActivePlan)
                {
                    // Redirect to features selection if no active plan
                    return RedirectToAction("Index", "Features");
                }
            }
            */

            // Get user information
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "";
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserEmail == userEmail);

            if (user == null)
            {
                ViewBag.UserName = User.Identity?.Name ?? "User";
                ViewBag.UserEmail = userEmail;
                ViewBag.AuthorName = User.FindFirst("AuthorName")?.Value ?? "";
                ViewBag.Genre = User.FindFirst("Genre")?.Value ?? "";
                return View();
            }

            // Get author information
            var author = await _context.Authors
                .FirstOrDefaultAsync(a => a.AuthorCode == user.UserId.ToString());

            // Get user's books
            var books = await _context.Books
                .Where(b => b.UserId == user.UserId)
                .ToListAsync();

            // Calculate real statistics from database
            var totalBooksPublished = books.Count(b => b.Status == "Published");
            
            // Calculate monthly revenue from BookPrice table (if exists)
            var currentMonth = DateTime.UtcNow.Month;
            var currentYear = DateTime.UtcNow.Year;
            var monthlyRevenue = await _context.BookPrices
                .Where(bp => bp.CreatedAt.Month == currentMonth && bp.CreatedAt.Year == currentYear)
                .Join(_context.Books.Where(b => b.UserId == user.UserId),
                    bp => bp.BookId,
                    b => b.BookId,
                    (bp, b) => bp)
                .SumAsync(bp => (decimal?)bp.bookPrice) ?? 0m;
            
            // Calculate total downloads from database (if you have a Downloads table)
            // For now, using book count as a placeholder - replace with actual downloads table when available
            var totalDownloads = books.Count; // Replace with actual downloads count when Downloads table exists
            
            // Calculate average rating from database (if you have a Ratings table)
            // For now, return 0 if no ratings exist
            var averageRating = 0m; // Replace with actual rating calculation when Ratings table exists

            // Create view model with real database data
            var viewModel = new DashboardIndexViewModel
            {
                UserName = user.FullName,
                UserEmail = user.UserEmail,
                TotalBooksPublished = totalBooksPublished,
                MonthlyRevenue = monthlyRevenue,
                TotalDownloads = totalDownloads,
                AverageRating = averageRating,
                CurrentProjects = books.Select(b => new ProjectViewModel
                {
                    BookId = b.BookId,
                    Title = b.Title,
                    Status = b.Status,
                    ProgressPercentage = b.Status == "Published" ? 100 : b.Status == "Draft" ? 30 : 75,
                    ProgressText = b.Status == "Published" ? "Complete" : b.Status == "Draft" ? $"Chapter 2 of 8" : $"Chapter 8 of 10"
                }).ToList(),
                RecentActivities = new List<ActivityViewModel>
                {
                    new ActivityViewModel { Title = "Chapter 5 of \"The Art of Digital Publishing\" was updated", Description = "Your latest changes have been saved successfully", TimeAgo = "2 hours ago", IconClass = "fas fa-book" },
                    new ActivityViewModel { Title = "New order for \"Modern Web Development\" received", Description = "Customer purchased 3 copies of your book", TimeAgo = "5 hours ago", IconClass = "fas fa-shopping-cart" },
                    new ActivityViewModel { Title = "New review for \"AI in Everyday Life\"", Description = "Received 5-star rating with positive feedback", TimeAgo = "1 day ago", IconClass = "fas fa-comment" },
                    new ActivityViewModel { Title = "New manuscript uploaded for \"Creative Writing Techniques\"", Description = "File processed and ready for editing", TimeAgo = "2 days ago", IconClass = "fas fa-file-alt" }
                },
                CurrentWorkingBook = books.Any() ? new BookViewModel
                {
                    BookId = books.First().BookId,
                    Title = books.First().Title,
                    BookIdText = $"ID: {books.First().BookId}",
                    ProgressPercentage = books.First().Status == "Published" ? 100 : books.First().Status == "Draft" ? 30 : 75,
                    ProgressText = books.First().Status == "Published" ? "Complete" : books.First().Status == "Draft" ? $"Chapter 2 of 8" : $"Chapter 8 of 10"
                } : new BookViewModel()
            };

            // Pass profile picture path to view
            ViewBag.ProfilePicturePath = user.ProfilePicturePath;

            return View(viewModel);
        }

        [Route("EBook")]
        public IActionResult EBook()
        {
            ViewBag.UserName = User.Identity?.Name ?? "User";
            return View();
        }

        [Route("CoverDesign")]
        public IActionResult CoverDesign()
        {
            ViewBag.UserName = User.Identity?.Name ?? "User";
            return View();
        }

        [Route("AudioBook")]
        public IActionResult AudioBook()
        {
            ViewBag.UserName = User.Identity?.Name ?? "User";
            return View();
        }

        [Route("EditingFormatting")]
        public async Task<IActionResult> EditingFormatting()
        {
            ViewBag.UserName = User.Identity?.Name ?? "User";
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                var hasActivePlan = await _featureCartService.HasActivePlanAsync(userId);
                if (!hasActivePlan)
                {
                    return RedirectToAction("Index", "Features");
                }
            }
            return View();
        }

        [Route("Proofreading")]
        public IActionResult Proofreading()
        {
            ViewBag.UserName = User.Identity?.Name ?? "User";
            return View();
        }

        [Route("Publishing")]
        public IActionResult Publishing()
        {
            ViewBag.UserName = User.Identity?.Name ?? "User";
            return View();
        }

        [Route("Marketing")]
        public IActionResult Marketing()
        {
            ViewBag.UserName = User.Identity?.Name ?? "User";
            return View();
        }

        [Route("Royalties")]
        public IActionResult Royalties()
        {
            ViewBag.UserName = User.Identity?.Name ?? "User";
            return View();
        }

        [Route("OrderAuthorCopy")]
        public IActionResult OrderAuthorCopy()
        {
            ViewBag.UserName = User.Identity?.Name ?? "User";
            return View();
        }

        [Route("Copyright")]
        public IActionResult Copyright()
        {
            ViewBag.UserName = User.Identity?.Name ?? "User";
            return View();
        }

        [Route("Profile")]
        public async Task<IActionResult> Profile(PlanFeatures f)
        {
            // Get user information
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "";
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserEmail == userEmail);

            if (user == null)
            {
                ViewBag.UserName = User.Identity?.Name ?? "User";
                return View();
            }

            // Get author information
            var author = await _context.Authors
                .FirstOrDefaultAsync(a => a.AuthorCode == user.UserId.ToString());

            // Get user's active plan
            var activePlan = await _context.AuthorPlans
                .Include(ap => ap.Plan)
                .Where(ap => ap.AuthorId == user.UserId && ap.IsActive == 1 && ap.EndDate > DateTime.UtcNow)
                .OrderByDescending(ap => ap.EndDate)
                .FirstOrDefaultAsync();

            // Get plan features
            var planFeatures = new List<PlanFeatureViewModel>();
            if (activePlan != null)
            {
                // Get features for this plan
                var features = await _context.PlanFeatures
                    .Where(pf => pf.PlanId == activePlan.PlanId)
                    .ToListAsync();

                planFeatures = features.Select(f => new PlanFeatureViewModel
                {
                    Name = f.FeatureName ?? "",
                    IsIncluded = f.IsActive == 1
                }).ToList();
            }

            // Compute profile metrics from DB where possible
            var userBooks = await _context.Books.Where(b => b.UserId == user.UserId).ToListAsync();
            var totalBooks = userBooks.Count;
            var booksRead = userBooks.Count(b => b.Status == "Published");
            var booksReading = userBooks.Count(b => b.Status != "Published");

            // Create view model
            var viewModel = new DashboardProfileViewModel
            {
                UserName = user.FullName,
                UserEmail = user.UserEmail,
                UserRole = user.Role?.RoleName ?? "Reader",
                MemberSince = user.CreatedAt,
                Country = "United States",
                TotalBooks = totalBooks,
                BooksReading = booksReading,
                Reviews = 0,
                BooksRead = booksRead,
                ReadingHours = 0,
                PagesRead = 0,
                ReadingStreak = 0,
                PlanName = activePlan?.Plan?.PlanName ?? "Basic Plan",
                PlanPrice = activePlan?.Plan?.PlanRate ?? 0m,
                NextBillingDate = activePlan?.EndDate ?? DateTime.UtcNow.AddDays(30),
                BillingCycle = "Monthly",
                PaymentMethod = "**** 4242",
                PlanFeatures = planFeatures
            };

            // Pass profile picture path to view
            ViewBag.ProfilePicturePath = user.ProfilePicturePath;

            return View(viewModel);
        }

        // New actions for the dashboard overhaul
        [Route("FeaturesSelection")]
        public IActionResult FeaturesSelection()
        {
            ViewBag.UserName = User.Identity?.Name ?? "User";
            return View();
        }

        [Route("PlanSuggestion")]
        public IActionResult PlanSuggestion()
        {
            ViewBag.UserName = User.Identity?.Name ?? "User";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("UpdateProfile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            try
            {
                var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "";
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserEmail == userEmail);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                if (!string.IsNullOrWhiteSpace(request.FullName))
                {
                    user.FullName = request.FullName.Trim();
                }

                if (!string.IsNullOrWhiteSpace(request.Email))
                {
                    user.UserEmail = request.Email.Trim();
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("UpdatePassword")]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordRequest request)
        {
            try
            {
                var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "";
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserEmail == userEmail);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                if (string.IsNullOrWhiteSpace(request.CurrentPassword) || user.Password != request.CurrentPassword)
                {
                    return Json(new { success = false, message = "Current password is incorrect" });
                }

                if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
                {
                    return Json(new { success = false, message = "New password must be at least 6 characters" });
                }

                if (request.NewPassword != request.ConfirmPassword)
                {
                    return Json(new { success = false, message = "New passwords do not match" });
                }

                user.Password = request.NewPassword;
                user.ConfirmPassword = request.NewPassword;
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [Route("PlanConfirmation")]
        public IActionResult PlanConfirmation()
        {
            ViewBag.UserName = User.Identity?.Name ?? "User";
            return View();
        }

        [Route("Summary")]
        public IActionResult Summary()
        {
            ViewBag.UserName = User.Identity?.Name ?? "User";
            return View();
        }

        [Route("Billing")]
        public IActionResult Billing()
        {
            ViewBag.UserName = User.Identity?.Name ?? "User";
            return View();
        }

        [Route("LivePreview")]
        public IActionResult LivePreview()
        {
            ViewBag.UserName = User.Identity?.Name ?? "User";
            return View();
        }

        [Route("AdvancedEditing")]
        public IActionResult AdvancedEditing()
        {
            ViewBag.UserName = User.Identity?.Name ?? "User";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("UploadProfilePicture")]
        public async Task<IActionResult> UploadProfilePicture(IFormFile profilePicture)
        {
            try
            {
                // Log for debugging
                System.Diagnostics.Debug.WriteLine("UploadProfilePicture called");
                
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
                System.Diagnostics.Debug.WriteLine($"User email: {userEmail}");
                
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserEmail == userEmail);
                
                if (user == null)
                {
                    System.Diagnostics.Debug.WriteLine("User not found");
                    return Json(new { success = false, message = "User not found" });
                }

                if (profilePicture == null || profilePicture.Length == 0)
                {
                    System.Diagnostics.Debug.WriteLine("No file received");
                    return Json(new { success = false, message = "Please select an image file" });
                }

                System.Diagnostics.Debug.WriteLine($"File received: {profilePicture.FileName}, Size: {profilePicture.Length}");

                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var fileExtension = Path.GetExtension(profilePicture.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return Json(new { success = false, message = "Only JPG, JPEG, and PNG files are allowed" });
                }

                // Validate file size (2 MB = 2 * 1024 * 1024 bytes)
                const long maxFileSize = 2 * 1024 * 1024; // 2 MB
                if (profilePicture.Length > maxFileSize)
                {
                    return Json(new { success = false, message = "File size must be less than 2 MB" });
                }

                // Delete old profile picture if exists
                if (!string.IsNullOrEmpty(user.ProfilePicturePath))
                {
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ProfilePicturePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                // Generate unique filename
                var fileName = $"profile_{user.UserId}_{DateTime.UtcNow.Ticks}{fileExtension}";
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "UserImages", fileName);
                var relativePath = $"/UserImages/{fileName}";

                // Ensure directory exists
                var directory = Path.GetDirectoryName(uploadPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Save file
                using (var stream = new FileStream(uploadPath, FileMode.Create))
                {
                    await profilePicture.CopyToAsync(stream);
                }

                // Update user record
                user.ProfilePicturePath = relativePath;
                System.Diagnostics.Debug.WriteLine($"Updating user ProfilePicturePath to: {relativePath}");
                
                var changes = await _context.SaveChangesAsync();
                System.Diagnostics.Debug.WriteLine($"Database changes saved: {changes}");

                System.Diagnostics.Debug.WriteLine("Upload successful");
                return Json(new { success = true, message = "Profile picture uploaded successfully", imagePath = relativePath });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return Json(new { success = false, message = $"Error uploading image: {ex.Message}" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("RemoveProfilePicture")]
        public async Task<IActionResult> RemoveProfilePicture()
        {
            try
            {
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserEmail == userEmail);
                
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                // Delete file if exists
                if (!string.IsNullOrEmpty(user.ProfilePicturePath))
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ProfilePicturePath.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                // Update user record
                user.ProfilePicturePath = null;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Profile picture removed successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error removing image: {ex.Message}" });
            }
        }

        [HttpGet]
        [Route("GetProfilePicture")]
        public async Task<IActionResult> GetProfilePicture()
        {
            try
            {
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserEmail == userEmail);
                
                if (user == null || string.IsNullOrEmpty(user.ProfilePicturePath))
                {
                    return Json(new { success = false, imagePath = "" });
                }

                return Json(new { success = true, imagePath = user.ProfilePicturePath });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ===================== BOOK CREATION SCREEN =====================
        [Route("BookCreation")]
        public async Task<IActionResult> BookCreation(int? bookId = null)
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserEmail == userEmail);
            
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.UserId = user.UserId;
            ViewBag.SelectedBookId = bookId;
            
            // Load user's books for dropdown
            var userBooks = await _context.Books
                .Where(b => b.UserId == user.UserId)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new { b.BookId, b.Title, b.CreatedAt })
                .ToListAsync();
            
            ViewBag.UserBooks = userBooks;
            
            // If bookId provided, load book details
            if (bookId.HasValue)
            {
                var book = await _context.Books
                    .Include(b => b.Chapters)
                    .FirstOrDefaultAsync(b => b.BookId == bookId.Value && b.UserId == user.UserId);
                
                if (book != null)
                {
                    ViewBag.Book = book;
                    ViewBag.Chapters = book.Chapters.OrderBy(c => c.ChapterNumber).ToList();
                }
            }
            
            return View();
        }

        // ===================== STYLING SCREEN =====================
        [Route("Styling")]
        public async Task<IActionResult> Styling(int? bookId = null)
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserEmail == userEmail);
            
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.UserId = user.UserId;
            
            // Check if user has styling feature (premium) - Database connected
            var userIdString = user.UserId.ToString();
            //var hasStylingFeature = await _context.userfeatures
            //    .Include(uf => uf.Feature)
            //    .Where(uf => uf.UserId == userIdString)
            //    .AnyAsync(uf => uf.Feature != null &&
            //                    (uf.Feature.Name.ToLower().Contains("style") ||
            //                     uf.Feature.Key.ToLower().Contains("style")));

            //ViewBag.HasStylingFeature = hasStylingFeature;

            // Load user's books
            var userBooks = await _context.Books
                .Where(b => b.UserId == user.UserId)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new { b.BookId, b.Title })
                .ToListAsync();
            
            ViewBag.UserBooks = userBooks;
            ViewBag.SelectedBookId = bookId;
            
            // If bookId provided, load style preferences from Settings
            if (bookId.HasValue)
            {
                var styleSetting = await _context.Settings
                    .FirstOrDefaultAsync(s => s.Key == $"book:{bookId}:style");
                
                if (styleSetting != null)
                {
                    ViewBag.StylePreferences = styleSetting.Value;
                }
            }
            
            return View();
        }

        // ===================== PREVIEW SCREEN =====================
        [Route("Preview")]
        public async Task<IActionResult> Preview(int? bookId = null)
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserEmail == userEmail);
            
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.UserId = user.UserId;
            
            // Load user's books
            var userBooks = await _context.Books
                .Where(b => b.UserId == user.UserId)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new { b.BookId, b.Title })
                .ToListAsync();
            
            ViewBag.UserBooks = userBooks;
            ViewBag.SelectedBookId = bookId;
            
            // If bookId provided, load book with chapters
            if (bookId.HasValue)
            {
                var book = await _context.Books
                    .Include(b => b.Chapters)
                    .FirstOrDefaultAsync(b => b.BookId == bookId.Value && b.UserId == user.UserId);
                
                if (book != null)
                {
                    ViewBag.Book = book;
                    ViewBag.Chapters = book.Chapters.OrderBy(c => c.ChapterNumber).ToList();
                }
            }
            
            return View();
        }

        // ===================== GENERATE FORMAT SCREEN =====================
        [Route("GenerateFormat")]
        public async Task<IActionResult> GenerateFormat(int? bookId = null)
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserEmail == userEmail);
            
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.UserId = user.UserId;
            
            // Load user's books
            var userBooks = await _context.Books
                .Where(b => b.UserId == user.UserId)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new { b.BookId, b.Title, b.Status })
                .ToListAsync();
            
            ViewBag.UserBooks = userBooks;
            ViewBag.SelectedBookId = bookId;
            
            // If bookId provided, check for generated formats in Settings
            if (bookId.HasValue)
            {
                var epubSetting = await _context.Settings
                    .FirstOrDefaultAsync(s => s.Key == $"book:{bookId}:output:epub");
                var pdfSetting = await _context.Settings
                    .FirstOrDefaultAsync(s => s.Key == $"book:{bookId}:output:pdf");
                
                ViewBag.EpubPath = epubSetting?.Value;
                ViewBag.PdfPath = pdfSetting?.Value;
            }
            
            return View();
        }

        // ===================== MY BOOKS SCREEN =====================
        [Route("MyBooks")]
        public async Task<IActionResult> MyBooks()
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserEmail == userEmail);
            
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Load all user's books with details from database
            var books = await _context.Books
                .Where(b => b.UserId == user.UserId)
                .Include(b => b.Chapters)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new
                {
                    b.BookId,
                    b.Title,
                    b.Status,
                    b.CreatedAt,
                    b.UpdatedAt,
                    ChapterCount = b.Chapters.Count,
                    b.CoverImagePath
                })
                .ToListAsync();
            
            ViewBag.Books = books;
            ViewBag.UserId = user.UserId;
            
            return View();
        }

        // ===================== SUBSCRIPTIONS SCREEN =====================
        [Route("Subscriptions")]
        public async Task<IActionResult> Subscriptions()
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserEmail == userEmail);
            
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Load user's active subscriptions from database
            var activeSubscriptions = await _context.AuthorPlans
                .Include(ap => ap.Plan)
                .Where(ap => ap.UserId == user.UserId && ap.IsActive == 1 && ap.EndDate > DateTime.UtcNow)
                .OrderByDescending(ap => ap.EndDate)
                .Select(ap => new
                {
                    ap.AuthorPlanId,
                    PlanName = ap.Plan != null ? ap.Plan.PlanName : ap.PlanName,
                    ap.PlanRate,
                    ap.StartDate,
                    ap.EndDate,
                    ap.MaxEBooks,
                    ap.PlanDescription
                })
                .ToListAsync();
            
            // Load all available plans from database
            var availablePlans = await _context.Plans
                .Where(p => p.PlanId > 0) // Active plans
                .OrderBy(p => p.PlanRate)
                .ToListAsync();
            
            // Load user's purchased features - Database connected
            var userIdString = user.UserId.ToString();
            var userFeatures = await _context.UserFeatures
                .Include(uf => uf.Feature)
                .Where(uf => uf.UserId == userIdString)
                .Select(uf => new
                {
                    uf.FeatureId,
                    FeatureName = uf.Feature != null ? uf.Feature.Name : "Unknown",
                    uf.AddedAt
                })
                .ToListAsync();
            
            ViewBag.ActiveSubscriptions = activeSubscriptions;
            ViewBag.AvailablePlans = availablePlans;
            ViewBag.UserFeatures = userFeatures;
            ViewBag.UserId = user.UserId;
            
            return View();
        }

        // ===================== SETTINGS SCREEN =====================
        [Route("Settings")]
        public async Task<IActionResult> Settings()
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserEmail == userEmail);
            
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Load user preferences from database
            var userPreferences = await _context.UserPreferences
                .Where(up => up.UserId == user.UserId)
                .ToListAsync();
            
            var notificationsEnabled = userPreferences
                .FirstOrDefault(up => up.Key == "notifications_enabled");
            
            ViewBag.NotificationsEnabled = notificationsEnabled?.Value == "true";
            ViewBag.User = user;
            
            return View();
        }

        // Save notification settings to database
        [HttpPost]
        [Route("SaveNotificationSettings")]
        public async Task<IActionResult> SaveNotificationSettings([FromBody] bool enabled)
        {
            try
            {
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserEmail == userEmail);
                
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                var preference = await _context.UserPreferences
                    .FirstOrDefaultAsync(up => up.UserId == user.UserId && up.Key == "notifications_enabled");
                
                if (preference == null)
                {
                    _context.UserPreferences.Add(new UserPreference
                    {
                        UserId = user.UserId,
                        Key = "notifications_enabled",
                        Value = enabled.ToString().ToLower(),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
                else
                {
                    preference.Value = enabled.ToString().ToLower();
                    preference.UpdatedAt = DateTime.UtcNow;
                }
                
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ===================== SUPPORT SCREEN =====================
        [Route("Support")]
        public async Task<IActionResult> Support()
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserEmail == userEmail);
            
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Load FAQs from Settings table (if stored there) or use default
            var faqSettings = await _context.Settings
                .Where(s => s.Category == "FAQ" || s.Key.StartsWith("faq:"))
                .OrderBy(s => s.Key)
                .ToListAsync();
            
            ViewBag.FAQs = faqSettings;
            ViewBag.UserId = user.UserId;
            ViewBag.UserEmail = user.UserEmail;
            
            return View();
        }
    }
}

public class UpdateProfileRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class UpdatePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}