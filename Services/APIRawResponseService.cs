using EBookDashboard.Interfaces;
using EBookDashboard.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace EBookDashboard.Services
{
    public class APIRawResponseService : IAPIRawResponseService
    {
        private readonly ApplicationDbContext _context;
        private readonly CommonMethodsService _commonMethodsService;
        public APIRawResponseService(ApplicationDbContext context, CommonMethodsService commonMethodsService)
        {
            _context = context;
            _commonMethodsService = commonMethodsService;
        }

        // On Press ⚙️ Generate method Add New Record in raw response
        //==================================================
        //    Save Raw Response with generated BookId
        //==================================================
        public async Task<int> SaveRawResponseAsync(AIBookRequest request, string responseData, string endpoint, string statusCode, string? errorMessage = null)
        {
            int userId = int.TryParse(request.UserId, out int uid) ? uid : 0;
            // Generate BookId using the async method
            int generatedBookId = 1; //await _commonMethodsService.GenerateBookIdAsync(userId);
            var rawResponse = new APIRawResponse
            {
                Endpoint = endpoint,
                Chapter = request.Chapter,
                Title = request.Title,
                RequestData = JsonConvert.SerializeObject(request),
                ResponseData = responseData,
                UserId = userId,
                BookId = request.BookId != null && int.TryParse(request.BookId, out int bid) ? bid : generatedBookId,
                StatusCode = statusCode,
                ErrorMessage = errorMessage,
                CreatedAt = DateTime.UtcNow
            };

            _context.APIRawResponse.Add(rawResponse);
            await _context.SaveChangesAsync();

            Console.WriteLine($"💾 Raw response saved with ID: {rawResponse.ResponseId}");
            return rawResponse.ResponseId;
        }
        // Fist we will generate BookId then save raw response
        public async Task<int> SaveRawResponseAsync2(AIBookRequest request, string responseData, string endpoint, string statusCode, string? errorMessage = null)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Step 1: Create and save a new Book first
                var newBook = new Books
                {
                    AuthorId = int.TryParse(request.UserId, out int aid) ? aid : 0, // Adjust based on your AIBookRequest properties
                    UserId = int.TryParse(request.UserId, out int uid) ? uid : 0,
                    CategoryId = 1, // Set default or get from request
                    Title = request.Title ?? "Untitled Book",
                    Subtitle = string.Empty,
                    AuthorCode = "AUTHOR_CODE", // Set appropriate value
                    LanguageId = 1, // Set default language
                    CoverImagePath = string.Empty,
                    ManuscriptPath = string.Empty,
                    Genre = "Default Genre", // Set appropriate genre
                    Description = string.Empty,
                    WordCount = 0,
                    CreatedAt = DateTime.UtcNow,
                    Status = BookStatus.Draft.ToString()
                };

                _context.Books.Add(newBook);
                await _context.SaveChangesAsync(); // This generates the BookId

                Console.WriteLine($"📚 New Book saved with ID: {newBook.BookId}");

                // Step 2: Save raw response with the generated BookId
                var rawResponse = new APIRawResponse
                {
                    Endpoint = endpoint,
                    Chapter = request.Chapter,
                    Title = request.Title,
                    RequestData = JsonConvert.SerializeObject(request),
                    ResponseData = responseData,
                    UserId = newBook.UserId, // Use from the created book
                    BookId = newBook.BookId, // Use the generated BookId from the new book
                    StatusCode = statusCode,
                    ErrorMessage = errorMessage,
                    CreatedAt = DateTime.UtcNow,
                    ParsedBookId = newBook.BookId.ToString() // Store as string for reference
                };

                _context.APIRawResponse.Add(rawResponse);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                Console.WriteLine($"💾 Raw response saved with ID: {rawResponse.ResponseId} for Book ID: {newBook.BookId}");
                return rawResponse.ResponseId;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        // On press 💾 Update/Save button Updated method to handle both insert and update logic
        public async Task<int> UpdateRawResponseAsync(AIBookRequest request, string responseData, string endpoint, string statusCode, string? errorMessage = null)
        {
            // Parse IDs from request
            int? userId = int.TryParse(request.UserId, out int uid) ? uid : (int?)null;
            int? bookId = int.TryParse(request.BookId, out int bid) ? bid : (int?)null;
            int? responseId = int.TryParse(request.ResponseId, out int rid) ? rid : (int?)null;

            // Check if we should update existing record
            APIRawResponse existingResponse = null;

            // For update, we should primarily use ResponseId to find the existing record
            if (responseId.HasValue)
            {
                existingResponse = await _context.APIRawResponse
                    .FirstOrDefaultAsync(r => r.ResponseId == responseId.Value);
            }

            // If ResponseId not found, try to find by other criteria
            if (existingResponse == null && userId.HasValue && bookId.HasValue && request.Chapter > 0)
            {
                existingResponse = await _context.APIRawResponse
                    .FirstOrDefaultAsync(r => r.UserId == userId.Value
                                           && r.BookId == bookId.Value
                                           && r.Chapter == request.Chapter);
            }

            if (existingResponse != null)
            {
                // UPDATE existing record
                existingResponse.Endpoint = endpoint;
                existingResponse.Title = request.Title;
                existingResponse.RequestData = JsonConvert.SerializeObject(request);
                existingResponse.ResponseData = responseData;
                existingResponse.StatusCode = statusCode;
                existingResponse.ErrorMessage = errorMessage;
                existingResponse.UpdatedAt = DateTime.UtcNow; // Add update timestamp

                _context.APIRawResponse.Update(existingResponse);
                await _context.SaveChangesAsync();

                Console.WriteLine($"📝 Raw response updated with ID: {existingResponse.ResponseId}");
                return existingResponse.ResponseId;
            }
            else
            {
                // ADD new record (if no existing record found)
                var rawResponse = new APIRawResponse
                {
                    Endpoint = endpoint,
                    Chapter = request.Chapter,
                    Title = request.Title,
                    RequestData = JsonConvert.SerializeObject(request),
                    ResponseData = responseData,
                    UserId = userId,
                    BookId = bookId,
                    StatusCode = statusCode,
                    ErrorMessage = errorMessage,
                    CreatedAt = DateTime.UtcNow
                };

                _context.APIRawResponse.Add(rawResponse);
                await _context.SaveChangesAsync();

                Console.WriteLine($"💾 Raw response saved with ID: {rawResponse.ResponseId}");
                return rawResponse.ResponseId;
            }
        }

        // We will save data in Books table when Finalize button pressed from APIRawResponse table

        public async Task<APIRawResponse> GetRawResponseAsync(int responseId)
        {
            return await _context.APIRawResponse.FirstOrDefaultAsync(r => r.ResponseId == responseId);
        }

        // NEW: Method to save raw response with a Book object
        public async Task<int> SaveRawResponseWithBookAsync(AIBookRequest request, string responseData, string endpoint, string statusCode, Books book, string? errorMessage = null)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Step 1: Save the Book first
                _context.Books.Add(book);
                await _context.SaveChangesAsync();

                Console.WriteLine($"📚 Book saved with ID: {book.BookId}");

                // Step 2: Save the raw response with the BookId
                var rawResponse = new APIRawResponse
                {
                    Endpoint = endpoint,
                    Chapter = request.Chapter,
                    Title = request.Title,
                    RequestData = JsonConvert.SerializeObject(request),
                    ResponseData = responseData,
                    UserId = int.TryParse(request.UserId, out int uid) ? uid : (int?)null,
                    BookId = book.BookId, // Use the generated BookId
                    StatusCode = statusCode,
                    ErrorMessage = errorMessage,
                    CreatedAt = DateTime.UtcNow,
                    ParsedBookId = book.BookId.ToString()
                };

                _context.APIRawResponse.Add(rawResponse);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                Console.WriteLine($"💾 Raw response saved with ID: {rawResponse.ResponseId} for Book ID: {book.BookId}");
                return rawResponse.ResponseId;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        Task<Models.APIRawResponse> IAPIRawResponseService.GetRawResponseAsync(int responseId)
        {
            throw new NotImplementedException();
        }
        //================================================
        // Get all API raw responses for a user and book
        //================================================
        public async Task<IEnumerable<APIRawResponse>> GetRawResponsesByUserAndBookAsync(int userId, int bookId, int responseId)
        {
            var query = _context.APIRawResponse
                .Where(r => r.UserId == userId && r.BookId == bookId);

            if (responseId > 0)
            {
                // If responseId is provided and > 0, get that specific response
                query = query.Where(r => r.ResponseId == responseId);
                Console.WriteLine($"🔍 Getting specific response ID: {responseId} for user {userId}, book {bookId}");
            }
            else
            {
                // If responseId is 0 or not provided, get the latest response
                query = query.OrderByDescending(r => r.CreatedAt);
                Console.WriteLine($"🔍 Getting latest response for user {userId}, book {bookId}");
            }

            return await query.ToListAsync();
        }

        // Get latest API raw response for a user and book
        public async Task<APIRawResponse?> GetLatestRawResponseAsync(int userId, int bookId)
        {
            return await _context.APIRawResponse
                .Where(r => r.UserId == userId && r.BookId == bookId)
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefaultAsync();
        }

        // Get API raw responses by chapter for a user and book
        public async Task<APIRawResponse?> GetRawResponseByChapterAsync(int userId, int bookId, int chapter)
        {
            return await _context.APIRawResponse
                .Where(r => r.UserId == userId && r.BookId == bookId && r.Chapter == chapter)
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefaultAsync();
        }

        // Get all chapters for a user and book from raw responses
        public async Task<IEnumerable<APIRawResponse>> GetChaptersFromRawResponsesAsync(int userId, int bookId)
        {
            return await _context.APIRawResponse
                .Where(r => r.UserId == userId && r.BookId == bookId)
                .OrderBy(r => r.Chapter)
                .ToListAsync();
        }

        // Check if raw responses exist for a user and book
        public async Task<bool> RawResponsesExistAsync(int userId, int bookId)
        {
            return await _context.APIRawResponse
                .AnyAsync(r => r.UserId == userId && r.BookId == bookId);
        }

        // Get raw response count for a user and book
        public async Task<int> GetRawResponseCountAsync(int userId, int bookId)
        {
            return await _context.APIRawResponse
                .CountAsync(r => r.UserId == userId && r.BookId == bookId);
        }
        // Get latest ResponseId for a user and book
        public async Task<int?> GetLatestResponseIdAsync(int userId, int bookId)
        {
            return await _context.APIRawResponse
                .Where(r => r.UserId == userId && r.BookId == bookId)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => r.ResponseId)
                .FirstOrDefaultAsync();
        }
    }
}
