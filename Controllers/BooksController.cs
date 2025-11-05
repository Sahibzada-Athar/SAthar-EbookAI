using EBookDashboard.Interfaces;
using EBookDashboard.Models;
using EBookDashboard.Models.ViewModels;
using EBookDashboard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Recommendations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.Cmp;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EBookDashboard.Controllers
{
    [AllowAnonymous]
    //[Route("[controller]/[action]")]
    public class BooksController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IBookService _bookService;
        private readonly IAPIRawResponseService _rawResponseService;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<BooksController> _logger;

        public BooksController(IBookService bookService, ApplicationDbContext context, IHttpClientFactory httpClientFactory, IAPIRawResponseService rawResponseService, IConfiguration configuration, ILogger<BooksController> logger)
        {
            _httpClient = httpClientFactory.CreateClient();
            _bookService = bookService;
            _rawResponseService = rawResponseService;
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }
        //=====================================
        //       At the time of Page Load
        //=====================================
        // ✅ 1️⃣ — GET: Show the Razor view page
        [AllowAnonymous]
        [HttpGet]
        public IActionResult AIGenerateBook()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                // not logged in → redirect to login
                return RedirectToAction("Login", "Account");
            }
            ViewBag.UserId = userId; // ✅ send to Razor view
            //==== Using 2 tables in Razor View ==========
            var model = new AIGenerateBookViewModel
            {
                BookRequest = new AIBookRequest(),
                AvailablePlans = _context.Plans.ToList() // or your service
            };
            return View(model); // this will look for Views/Books/AIGenerateBook.cshtml
        }
        //=====================================
        //   When user clicks "Generate Book" button
        //=====================================
        // ✅ 2️⃣ — POST: Call external API and return book data as JSON
        // Generate Book via API
        [AllowAnonymous]
        [HttpPost]
        [Route("Books/AIGenerateBook")]
        public async Task<IActionResult> AIGenerateBook([FromBody] AIBookRequest model)
        {
            if (model == null)
            {
                return BadRequest("Invalid request data");
            }

            // ✅ Load from appsettings.json
            //var apiUrl = _configuration["ExternalApi:generate_chapter"];
            string apiUrl = "http://162.229.248.26:8001/api/generate_chapter";
            string apiKey = "X-API-Key";
            string password = "AK-proj-c8r15p0EYc1B0SKi5_hP58HEyL6xP0ywmZ2hEpvpvU5y-i7yZ8IiyLv1K7cGSkyNh";
            var responseData = string.Empty;
            // var apiKey = _configuration["ExternalApi:ApiKey"];
            //string apiUrl = "http://162.229.248.26:8001/api/generate_chapter";
            var apiHeaderName = "X-API-Key";


            int? rawResponseId = null;
            try
            {
                using var client = new HttpClient();
                {
                    // Set timeout(optional but recommended)
                    client.Timeout = TimeSpan.FromMinutes(5);

                    client.DefaultRequestHeaders.Add(apiKey, password);

                    var json = JsonConvert.SerializeObject(model);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    Console.WriteLine($"📤 Sending request to API - Title: {model.Title}, Chapter: {model.ChapterNo}");
                    // Console.WriteLine($"📤Sending request to API: {json}");
                    //-------- Error occurs here ------
                    var response = await client.PostAsync(apiUrl, content);
                    responseData = await response.Content.ReadAsStringAsync();

                    // Log the API response in VS Output or console
                    Console.WriteLine($"📥API Response Status: {response.StatusCode}");
                    Console.WriteLine($"📥API Response Data: {responseData}");

                    // ✅ SAVE RAW RESPONSE FIRST
                    rawResponseId = await _rawResponseService.SaveRawResponseAsync(
                        model,
                        responseData,
                        apiUrl,
                        response.StatusCode.ToString()
                    );

                    Console.WriteLine($"💾 Raw Response saved - ID: {rawResponseId}, Chapter: {model.ChapterNo}, Title: {model.Title}");
                    Console.WriteLine($"📥 API Response Status: {response.StatusCode}");

                    if (!response.IsSuccessStatusCode)
                    {
                        return StatusCode((int)response.StatusCode,
                            $"API error: {response.StatusCode} - {responseData}");
                    }
                    // Parse and save to database
                    try
                    {
                        // ✅ NEW: Parse the response and save to database
                        var apiResponse = JsonConvert.DeserializeObject<AIBookResponse>(responseData);
                        if (apiResponse != null)
                        {
                            Console.WriteLine($"🔍 API Response has content, proceeding to save...");
                            await SaveBookToDatabase(model, apiResponse, rawResponseId);
                            Console.WriteLine("✅ Book successfully saved to database");
                        }
                        else
                        {
                            Console.WriteLine("⚠️ No chapters to save to database");
                        }
                    }
                    catch (Exception dbEx)
                    {
                        Console.WriteLine($"⚠️ Database save failed but returning API response: {dbEx.Message}");
                        // Continue to return the API response even if DB save fails
                    }
                    return Content(responseData, "application/json");
                }
            }
            catch (HttpRequestException ex)
            {
                // Save error response
                if (rawResponseId == null)
                {
                    await _rawResponseService.SaveRawResponseAsync(
                        model,
                        responseData,
                        apiUrl,
                        "500",
                        $"Network error: {ex.Message}"
                    );
                }

                Console.WriteLine($"❌ Network error: {ex.Message}");
                return StatusCode(500, $"Network error: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Save error response
                if (rawResponseId == null)
                {
                    await _rawResponseService.SaveRawResponseAsync(
                        model,
                        responseData,
                        apiUrl,
                        "500",
                        $"Unexpected error: {ex.Message}"
                    );
                }

                // Log database errors but still return the book data to user
                Console.WriteLine($"Database save error: {ex.Message}");
                // You might want to return the API response even if saving fails
                return Content(responseData, "application/json");
            }
        }
        // ✅ 1️⃣ — GET: Show the Razor view page
        [AllowAnonymous]
        [HttpGet]
        public IActionResult AIGenerateBook2()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                // not logged in → redirect to login
                return RedirectToAction("Login", "Account");
            }
            ViewBag.UserId = userId; // ✅ send to Razor view
            return View(); // this will look for Views/Books/AIGenerateBook.cshtml
        }
        //=======================================
        // Get a single saved API response for a user and book
        //====================================
        [HttpGet]
        public async Task<IActionResult> GetBookResponse(int userId, int bookId)
        {
            var response = await _context.APIRawResponse
                .Where(r => r.UserId == userId && r.BookId == bookId)
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefaultAsync();

            if (response == null)
                return Json(new { success = false, message = "No response found." });

            return Json(new
            {
                success = true,
                data = response.ResponseData
            });
        }
        //=======================================
        // Get a single saved API response for a user and book
        //====================================
        // 🟢 Get categories and load the create book view
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.Categories
                .Select(c => new { c.CategoryId, c.CategoryName })
                .ToListAsync();

            ViewBag.Categories = categories;
            return View();
        }
        //==============================================
        //    Create a new book entry in the database
        //==============================================
        [HttpGet]
        public async Task<IActionResult> CreateBook(int userId)
        {
            try
            {
                // Load categories using service
                var categories = await _bookService.GetAllCategoriesAsync();
                ViewBag.Categories = categories;

                // Get user info from session/claims
                ViewBag.UserId = userId;
                //ViewBag.AuthorId = GetCurrentAuthorId();

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading CreateBook page");
                ViewBag.Error = "Unable to load categories. Please try again.";
                return View();
            }
        }
        // POST: /Books/CreateBook
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBook([FromBody] CreateBookRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new { success = false, message = "Invalid request data." });
                }

                // Basic validation
                if (string.IsNullOrWhiteSpace(request.Title))
                {
                    return BadRequest(new { success = false, message = "Book title is required." });
                }

                if (request.CategoryId <= 0)
                {
                    return BadRequest(new { success = false, message = "Please select a valid category." });
                }

                // Use service to create book - this should now work
                var book = await _bookService.CreateBookFromRequestAsync(request);

                return Ok(new
                {
                    success = true,
                    message = "Book created successfully!",
                    bookId = book.BookId
                });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database error creating book for user {UserId}", request?.UserId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Database error occurred while creating book."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating book for user {UserId}", request?.UserId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "An unexpected error occurred while creating book."
                });
            }
        }
        // Helper methods to get current user/author from session or claims
        public async Task<bool> UpdateBookAsync(Books book)
        {
            book.UpdatedAt = DateTime.UtcNow;
            _context.Books.Update(book);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteBookAsync(int bookId)
        {
            var book = await _context.Books.FindAsync(bookId);
            if (book == null) return false;

            _context.Books.Remove(book);
            return await _context.SaveChangesAsync() > 0;
        }
        
        // Default method to get next IDs
        [HttpGet]
        public async Task<IActionResult> GetNextIds()
        {
            var lastUserId = await _context.Users.OrderByDescending(u => u.UserId).Select(u => u.UserId).FirstOrDefaultAsync();
            var lastBookId = await _context.Books.OrderByDescending(b => b.BookId).Select(b => b.BookId).FirstOrDefaultAsync();

            var nextUserId = lastUserId + 1;
            var nextBookId = lastBookId + 1;

            return Json(new { nextUserId, nextBookId });
        }
        // ✅ NEW: Method to save book to database
        private async Task<bool> SaveBookToDatabase(AIBookRequest request, AIBookResponse apiResponse, int? rawResponseId = null)
        {
            Console.WriteLine("✅ Trying to save Book to database");

            if (apiResponse?.Data == null || string.IsNullOrEmpty(apiResponse.Data.Content))
            {
                Console.WriteLine("❌ No content data in API response to save");
                return false;
            }


            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Convert UserId from string to int safely
                if (!int.TryParse(request.UserId, out int userId))
                {
                    userId = 1; // Default fallback
                }
                // === ADD THESE DEBUG LINES ===
                Console.WriteLine($"🔍 DEBUG: Raw UserId from request: '{request.UserId}'");
                var sessionUserId = HttpContext.Session.GetInt32("UserId");
                Console.WriteLine($"🔍 DEBUG: Session UserId: {sessionUserId}");
                // =============================

                    // === ADD THIS COMPARISON ===
                    if (sessionUserId.HasValue && userId != sessionUserId.Value)
                    {
                        Console.WriteLine($"⚠️ DEBUG: USER ID MISMATCH! Session: {sessionUserId.Value}, Using: {userId}");
                    }
                    // ===========================

                    Console.WriteLine($"💾 Saving book to database - UserId: {userId} (converted from: {request.UserId})");
                    Console.WriteLine($"💾 Saving book to database - UserId: {userId} (converted from: {request.UserId})");

                // Create new Book
                var book = new Books
                {
                    Title = apiResponse.Title ?? $"Book - {request.UserInput}",
                    UserId = userId,
                    Status = "Generated",
                    CreatedAt = DateTime.UtcNow
                };
                Console.WriteLine($"🔍 Adding book to context: {book.Title}");
                _context.Books.Add(book);
                await _context.SaveChangesAsync();
                Console.WriteLine($"🔍 Book saved with ID: {book.BookId}");

                // Update raw response with the parsed book ID
                if (rawResponseId.HasValue)
                {
                    var rawResponse = await _context.APIRawResponse.FindAsync(rawResponseId.Value);
                    if (rawResponse != null)
                    {
                        rawResponse.ParsedBookId = book.BookId.ToString();
                        await _context.SaveChangesAsync();
                        Console.WriteLine($"🔍 Updated raw response {rawResponseId} with book ID: {book.BookId}");
                    }
                }
                // Save the raw content as a single chapter (for now)
                var chapter = new Chapters
                {
                    BookId = book.BookId,
                    Title = "Full Book Content",
                    Content = apiResponse.Data.Content, // Save the entire raw content
                    ChapterNumber = 1
                };

                _context.Chapters.Add(chapter);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                Console.WriteLine($"✅ SUCCESS: Book saved to database with ID: {book.BookId}");


                //Console.WriteLine($"✅ Book saved to database with ID: {book.BookId}");
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"❌ Error saving book to database: {ex.Message}");
                return false;
            }
        }
        // ✅ 3️⃣ — POST: Save book data from API response (called from frontend)
        [AllowAnonymous]
        [HttpPost]
        [Route("Books/SaveBookFromAPI")]
        public async Task<IActionResult> SaveBookFromAPI([FromBody] APISaveBookRequest model)
        {
            try
            {
                if (model == null || string.IsNullOrEmpty(model.ApiRaw))
                    return BadRequest("No API response data found.");

                // Parse JSON string from hidden field
                var apiResponse = JsonConvert.DeserializeObject<AIBookResponse>(model.ApiRaw);

                if (apiResponse == null || apiResponse.Data == null || string.IsNullOrEmpty(apiResponse.Data.Content))
                    return BadRequest("Invalid or empty book data.");

                // ✅ Parse chapters from the HTML content (<h2> marks title boundaries)
                var chapters = ParseChaptersFromHtml(apiResponse.Data.Content);

                // ✅ Create new book entry
                var book = new Books
                {
                    Title = apiResponse.Title ?? "Untitled Book",
                    UserId = int.TryParse(model.UserId, out int uid) ? uid : 1,
                    Status = "Saved",
                    CreatedAt = DateTime.UtcNow
                };
                _context.Books.Add(book);
                await _context.SaveChangesAsync();

                int chapterNo = 1;
                foreach (var ch in chapters)
                {
                    _context.Chapters.Add(new Chapters
                    {
                        BookId = book.BookId,
                        Title = ch.Title,
                        Content = ch.Content,
                        ChapterNumber = chapterNo++,
                        LanguageId = 1,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        Status = "Saved"
                    });
                }

                await _context.SaveChangesAsync();

                return Ok($"Book '{book.Title}' saved with {chapters.Count} chapters (BookId={book.BookId}).");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ SaveBookFromAPI failed: " + ex.Message);
                return StatusCode(500, ex.Message);
            }
        }
        // ✅ Helper method to parse chapters from HTML content
        private List<(string Title, string Content)> ParseChaptersFromHtml(string htmlContent)
        {
            var chapters = new List<(string Title, string Content)>();

            // Split content based on <h2> tags (assuming each chapter starts with <h2>)
            var parts = System.Text.RegularExpressions.Regex.Split(htmlContent, @"<h2>|</h2>",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            string currentTitle = null;
            foreach (var part in parts)
            {
                if (string.IsNullOrWhiteSpace(part))
                    continue;

                if (currentTitle == null)
                {
                    currentTitle = System.Net.WebUtility.HtmlDecode(part.Trim());
                }
                else
                {
                    chapters.Add((currentTitle, System.Net.WebUtility.HtmlDecode(part.Trim())));
                    currentTitle = null;
                }
            }

            return chapters;
        }
        /// <summary>
        /// Get list of saved responses for a given UserId and BookId
        /// </summary>
   /*     [HttpGet]
        public async Task<IActionResult> GetSavedResponsesByBook(int userId, int bookId)
        {
            var responses = await _context.APIRawResponse
                .Where(r => r.UserId == userId && r.BookId == bookId)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new
                {
                    responseId = r.ResponseId,
                    createdAt = r.CreatedAt.ToString("yyyy-MM-dd HH:mm")
                })
                .ToListAsync();

            return Json(responses);
        }*/
        /// <summary>
        /// Get a single saved Book by its ResponseId
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetResponseById(int id)
        {
            var response = await _context.APIRawResponse
                .FirstOrDefaultAsync(r => r.ResponseId == id);

            if (response == null)
                return NotFound(new { message = "Response not found" });

            return Json(new
            {
                responseId = response.ResponseId,
                userId = response.UserId ?? 0,
                bookId = response.BookId ?? 0,
                apiRaw = response.ResponseData ?? "{}",
                createdAt = response.CreatedAt.ToString("yyyy-MM-dd HH:mm") ?? "(no date)"
            });
        }
        // Load all saved Books for a user
        [HttpGet]
        public async Task<IActionResult> GetAllResponses(int userId)
        {
            var responses = await _context.APIRawResponse
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new
                {
                    r.ResponseId,
                    r.BookId,
                    r.UserId,
                    r.CreatedAt,
                    Title = _context.Books
                        .Where(b => b.BookId == r.BookId)
                        .Select(b => b.Title)
                        .FirstOrDefault() ?? "(Untitled Book)"
                })
                .ToListAsync();

            if (responses == null || !responses.Any())
                return Json(new { success = false, message = "No saved responses found." });

            return Json(new { success = true, data = responses });
        }
        //==========================================================
        //============ Step 1: Load Books in DropDown ==============
        //==========================================================
        [HttpGet]
        public async Task<IActionResult> GetSavedResponses(int userId)
        {
            if (userId==0)
                return BadRequest("UserId is required.");
            try
            {
                var savedBooks = await _context.Books
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new {
                    bookId = b.BookId,
                    bookTitle = b.Title,
                    createdDate = b.CreatedAt.ToString("MMM dd, yyyy")
                })
                .ToListAsync();
                Console.WriteLine($"🔍 GetSavedResponses: Found {savedBooks.Count} books for user {userId}");
                return Json(savedBooks);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ GetSavedResponses error: {ex.Message}");
                return Json(new { error = ex.Message });
            }
        }
        //==================================================
        //       Load Book Data according to Selection
        //=================================================
        [HttpGet]
        public async Task<IActionResult> GetBookDetails(int userId, int bookId)
        {
            try
            {
                var bookDetails = await _bookService.GetBookDetailsAsync(userId, bookId);
                
                if (bookDetails == null)
                    return Json(new { success = false, message = "Book not found" });

                return Json(new
                {
                    success = true,
                    bookId = bookDetails.BookId,
                    title = bookDetails.Title,
                    description = bookDetails.Description,
                    genre = bookDetails.Genre,
                    totalChapters = bookDetails.TotalChapters,
                    chapters = bookDetails.Chapters
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        //==================================================
        //       Load Book Data according to Selection
        //=================================================
        [HttpGet]
        public async Task<IActionResult> GetBookFromRawData(int userId, int bookId)
        {
            try
            {
                var bookDetails = await _bookService.GetBookDetailsFromRawDataAsync(userId, bookId);

                if (bookDetails == null)
                    return Json(new { success = false, message = "Book not found" });

                return Json(new
                {
                    success = true,
                    bookId = bookDetails.BookId,
                    title = bookDetails.Title,
                    description = bookDetails.Description,
                    genre = bookDetails.Genre,
                    totalChapters = bookDetails.TotalChapters,
                    chapters = bookDetails.Chapters
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        // GET: Books
        public async Task<IActionResult> Index()
        {
            var books = await _context.Books
                .Include(b => b.Chapters)
                .Include(b => b.BookPrice)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
            return View(books);
        }

        // GET: Books/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var book = await _context.Books
                .Include(b => b.Chapters)
                .Include(b => b.BookPrice)
                .FirstOrDefaultAsync(b => b.BookId == id);
            if (book == null) return NotFound();
            return View(book);
        }

        // GET: Books/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Books/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Books book)
        {
            if (ModelState.IsValid)
            {
                book.CreatedAt = DateTime.UtcNow;
                book.Status = "Draft";
                _context.Books.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(book);
        }

        // GET: Books/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();
            return View(book);
        }

        // POST: Books/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Books book)
        {
            if (id != book.BookId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    book.UpdatedAt = DateTime.UtcNow;
                    _context.Update(book);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(book.BookId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(book);
        }

        // GET: Books/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var book = await _context.Books
                .Include(b => b.Chapters)
                .Include(b => b.BookPrice)
                .FirstOrDefaultAsync(b => b.BookId == id);
            if (book == null) return NotFound();
            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Books/Publish/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Publish(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                book.Status = "Published";
                book.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Books/Archive/5 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Archive(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                book.Status = "Archived";
                book.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.BookId == id);
        }
        // Additional actions for managing chapters can be added here
        [AllowAnonymous]
        [HttpPost]
        [Route("Books/EditChapter")]
        public async Task<IActionResult> EditChapter([FromBody] APIEditChapterRequest model)
        {
            if (model == null)
                return BadRequest("Invalid request payload.");
            // ✅ Read from appsettings.json
            var apiUrl = _configuration["ExternalApi:EditUrl"];
            var apiKey = _configuration["ExternalApi:ApiKey"];
            var apiHeaderName = "X-API-Key";

            string responseData = string.Empty;
            int? rawResponseId = null;

            try
            {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromMinutes(2);
                if (!string.IsNullOrEmpty(apiKey))
                    client.DefaultRequestHeaders.Add(apiHeaderName, apiKey);

                var json = JsonConvert.SerializeObject(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                Console.WriteLine($"📤Forwarding edit request to API: {json}");

                var response = await client.PostAsync(apiUrl, content);
                responseData = await response.Content.ReadAsStringAsync();

                // Save raw response for audit
                rawResponseId = await _rawResponseService.SaveRawResponseAsync(
                    // reuse AIBookRequest-like object for logging; create minimal AIBookRequest
                    new AIBookRequest
                    {
                        UserId = model.UserId,
                        BookId = model.BookId,
                        Chapter = int.TryParse(model.Chapter, out var c) ? c : 0,
                        UserInput = model.Changes
                    },
                    responseData,
                    apiUrl,
                    response.StatusCode.ToString()
                );

                Console.WriteLine($"📥 Edit API response status: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    // return the raw content and code so frontend can show error
                    return StatusCode((int)response.StatusCode, responseData);
                }
                // Optionally parse and persist edited content into Chapters table
                try
                {
                    dynamic parsed = JsonConvert.DeserializeObject(responseData);
                    string newContent = parsed?.data?.content ?? parsed?.content ?? null;

                    if (!string.IsNullOrEmpty(newContent) && int.TryParse(model.BookId, out int bookId))
                    {
                        int chapterNum = int.TryParse(model.Chapter, out var ch) ? ch : 0;
                        var chapter = await _context.Chapters.FirstOrDefaultAsync(c => c.BookId == bookId && c.ChapterNumber == chapterNum);

                        if (chapter != null)
                        {
                            chapter.Content = System.Net.WebUtility.HtmlDecode(newContent);
                            chapter.UpdatedAt = DateTime.UtcNow;
                            _context.Chapters.Update(chapter);
                            await _context.SaveChangesAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log parsing/persistence error but still return api response
                    Console.WriteLine($"⚠️ Unable to persist edited chapter: {ex.Message}");
                }
                // Optionally parse response as JSON to easily return it. We'll return raw content with application/json content type.
                return Content(responseData, "application/json");
            }
            catch (Exception ex)
            {
                // Save error (if not saved already)
                if (rawResponseId == null)
                {
                    await _rawResponseService.SaveRawResponseAsync(
                        new AIBookRequest { UserId = model.UserId, BookId = model.BookId, Chapter = int.TryParse(model.Chapter, out var c) ? c : 0, UserInput = model.Changes },
                        responseData,
                        apiUrl,
                        "500",
                        $"Forwarding error: {ex.Message}"
                    );
                }

                Console.WriteLine($"❌ EditChapter Exception: {ex.Message}");
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }

        //====================== Change Chapter Content ======================
        [HttpPost]
        public async Task<IActionResult> ChangeChapterContent([FromBody] APIChangeChapterModel model)
        {
            if (model == null)
                return BadRequest("Invalid data.");

            // Step 1: Send user request to your external API
            // ✅ Read from appsettings.json
            //var apiUrl = "http://162.229.248.26:8001/api/changecontent";

            var apiUrl = _configuration["ExternalApi:EditUrl"];
            var apiKey = _configuration["ExternalApi:ApiKey"];
            var apiHeaderName = "X-API-Key";

            var payload = new
            {
                user_id = model.UserId,
                book_id = model.BookId,
                chapter = model.Chapter,
                user_input = model.NewContent
            };

            using var httpClient = new HttpClient();
            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(apiUrl, content);
            var responseData = await response.Content.ReadAsStringAsync();

            // Step 2: Parse response JSON
            var parsedJson = JsonConvert.DeserializeObject<JObject>(responseData);

            // ✅ Step 3: Insert your code snippet HERE
            var newContent = parsedJson?["data"]?["content"]?.ToString() ?? parsedJson?["content"]?.ToString();

            int bid = int.TryParse(model.BookId, out var bookIdVal) ? bookIdVal : 0;
            int chapterNum = int.TryParse(model.Chapter, out var chVal) ? chVal : 0;

            var chapter = await _context.Chapters.FirstOrDefaultAsync(c =>
                    c.BookId == bid && c.ChapterNumber == chapterNum);

            if (chapter != null)
            {
                chapter.Content = newContent;
                chapter.UpdatedAt = DateTime.UtcNow;
                _context.Chapters.Update(chapter);
                await _context.SaveChangesAsync();
            }

            // Step 4: Return success response to UI
            return Json(new { success = true, data = parsedJson });
        }
        //================== Load Books on Dropdown ==================
       

        [HttpGet]
        public async Task<IActionResult> DebugBooks(int userId)
        {
            var books = await _context.Books
                .Where(b => b.UserId == userId)
                .ToListAsync();

            return Json(new
            {
                userId = userId,
                bookCount = books.Count,
                books = books
            });
        }
        
        //====================== Finalize Chapter ======================
        [AllowAnonymous]
        [HttpPost]
        [Route("Books/FinalizeChapter")]
        public async Task<IActionResult> FinalizeChapter([FromBody] APIFinalizeChapterRequest model)
        {
            if (model == null)
                return BadRequest("Invalid request payload.");

            // ✅ Load from appsettings.json
            var apiUrl = _configuration["ExternalApi:ApproveUrl"];
            var apiKey = _configuration["ExternalApi:ApiKey"];
            var apiHeaderName = "X-API-Key";

            string responseData = string.Empty;
            int? rawResponseId = null;

            try
            {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromMinutes(2);
                if (!string.IsNullOrEmpty(apiKey))
                    client.DefaultRequestHeaders.Add(apiHeaderName, apiKey);

                var json = JsonConvert.SerializeObject(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                Console.WriteLine($"📤 Sending finalize request to API: {json}");

                var response = await client.PostAsync(apiUrl, content);
                responseData = await response.Content.ReadAsStringAsync();

                // 🧾 Save raw API response
                rawResponseId = await _rawResponseService.SaveRawResponseAsync(
                    new AIBookRequest
                    {
                        UserId = model.UserId,
                        BookId = model.BookId,
                        Chapter = int.TryParse(model.Chapter, out var c) ? c : 0,
                        UserInput = "Finalize"
                    },
                    responseData,
                    apiUrl,
                    response.StatusCode.ToString()
                );

                Console.WriteLine($"📥 Finalize API Response Status: {response.StatusCode}");
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, responseData);
                }
                // If success, update DB: Book.Status = "Final", Chapter.Status = "Final"
                // ✅ Update DB if success
                try
                {
                    if (int.TryParse(model.BookId, out int bookId))
                    {
                        var book = await _context.Books.FirstOrDefaultAsync(b => b.BookId == bookId);
                        if (book != null)
                        {
                            book.Status = "Final";
                            book.UpdatedAt = DateTime.UtcNow;
                            _context.Books.Update(book);
                        }
                        int chapterNum = int.TryParse(model.Chapter, out var ch) ? ch : 0;
                        // ✅ Save or update Book and Chapter
                        var chapter = await _context.Chapters.FirstOrDefaultAsync(c => c.BookId == bookId && c.ChapterNumber == chapterNum);

                        if (chapter != null)
                        {
                            chapter.Status = "Final";
                            chapter.UpdatedAt = DateTime.UtcNow;
                            _context.Chapters.Update(chapter);
                        }
                        else
                        {
                            // create stub chapter if not exists (optional)
                            _context.Chapters.Add(new Chapters
                            {
                                BookId = bookId,
                                ChapterNumber = chapterNum,
                                Title = $"Chapter {chapterNum}",
                                Content = "", // unchanged
                                Status = "Final",
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            });
                        }

                        await _context.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ DB update after finalize failed: {ex.Message}");
                }

                return Content(responseData, "application/json");
            }
            catch (Exception ex)
            {
                if (rawResponseId == null)
                {
                    await _rawResponseService.SaveRawResponseAsync(
                        new AIBookRequest
                        {
                            UserId = model.UserId,
                            BookId = model.BookId,
                            Chapter = int.TryParse(model.Chapter, out var c) ? c : 0,
                            UserInput = "Finalize"
                        },
                        responseData,
                        apiUrl,
                        "500",
                        $"Finalize error: {ex.Message}"
                    );
                }

                Console.WriteLine($"❌ FinalizeChapter Exception: {ex.Message}");
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }

    }
}
