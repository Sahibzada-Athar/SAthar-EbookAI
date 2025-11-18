using EBookDashboard.Interfaces;
using EBookDashboard.Models;
using EBookDashboard.Models.DTO;
using EBookDashboard.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
namespace EBookDashboard.Services
{
    public class BookService : IBookService
    {
        private readonly ApplicationDbContext _context;

        public BookService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Books?> GetBookByIdAsync(int bookId)
        {
            return await _context.Books.FindAsync(bookId);
        }
        // ----- Books -----
        public async Task<IEnumerable<Books>> GetAllBooksAsync()
        {
            return await _context.Books
                                 .Include(b => b.Chapters)
                                 .ToListAsync();
        }

        //public async Task<Books?> GetBookByIdAsync(int bookId)
        //{
        //    return await _context.Books
        //                         .Include(b => b.Chapters)
        //                         .FirstOrDefaultAsync(b => b.BookId == bookId);
        //}
        //public Task<BookDetailsDto?> GetBookDetailsAsync(int userId, int bookId)
        //{
        //    throw new NotImplementedException();
        //}
        //============================================
        // ----- Get Book Details with Chapters -----
        //============================================
        public async Task<BookDetailsDto?> GetBookDetailsAsync(int userId, int bookId, int responseId)
        {
            // It should be from APIRawResponse table
            return await _context.Books
                .Where(b => b.UserId == userId && b.BookId == bookId)
                .Select(b => new BookDetailsDto
                {
                    BookId = b.BookId,
                    Title = b.Title,
                    Description = b.Description,
                    Genre = b.Genre,
                    TotalChapters = b.Chapters.Count,
                    Chapters = b.Chapters
                        .OrderBy(c => c.ChapterNumber)
                        .Select(c => new ChapterDto
                        {
                            ChapterNumber = c.ChapterNumber,
                            Title = c.Title,
                            Content = c.Content,
                            Status = c.Status
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();
        }

        //==================================================
        // ----- Get Book Details from APIRawResponse -----
        //==================================================
        public async Task<BookDetailsDto?> GetBookDetailsFromRawDataAsync(int userId, int bookId)
        {
            var rawResponse = await _context.APIRawResponse
        .FirstOrDefaultAsync(b => b.UserId == userId && b.BookId == bookId);

            if (rawResponse == null)
                return null;

            // Parse chapter number safely
            int chapterNumber = 1;
            if (rawResponse.Chapter > 0)
            {
                chapterNumber = rawResponse.Chapter;
            }

            return new BookDetailsDto
            {
                BookId = rawResponse.BookId ?? 0,
                Title = rawResponse.Title ?? "Untitled Book",
                Description = "",
                Genre = "",
                TotalChapters = 1,
                Chapters = new List<ChapterDto>
                    {
                        new ChapterDto
                            {
                                ChapterNumber = chapterNumber,
                                Title = rawResponse.Title ?? "Untitled Chapter",
                                Content = rawResponse.ResponseData ?? "",
                                Status = "Generated"
                            }
                    },
                RawResponseId = rawResponse.ResponseId,
                Endpoint = rawResponse.Endpoint ?? "",
                CreatedAt = rawResponse.CreatedAt,
            };
        }

        // ----- to make BookId relationship Key -----
        public async Task<Books> CreateBookAsync(Books book)
        {
            // Set default values if not provided
            book.CreatedAt = DateTime.UtcNow;
            book.UpdatedAt = DateTime.UtcNow;
            book.Status = book.Status ?? BookStatus.Draft.ToString();

            _context.Books.Add(book);
            await _context.SaveChangesAsync();
            return book;
        }

        public async Task<Books> CreateBookFromRequestAsync(CreateBookRequest request)
        {
            var book = new Books
            {
                Title = request.Title.Trim(),
                Description = request.Description?.Trim(),
                CategoryId = request.CategoryId,
                AuthorId = request.AuthorId,
                UserId = request.UserId,
                Genre = request.Genre?.Trim() ?? "",
                Dedication = request.Dedication?.Trim() ?? "",
                Ghostwriting = request.Ghostwriting?.Trim() ?? "",
                Epigraph = request.Epigraph?.Trim() ?? "",
                LanguageId = request.LanguageId,
                CoverImagePath = request.CoverImagePath ?? "",
                ManuscriptPath = request.ManuscriptPath ?? "",
                WordCount = request.WordCount,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Status = BookStatus.Draft.ToString()
            };

            return await CreateBookAsync(book);
        }

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

        // ----- Categories -----
        public async Task<IEnumerable<Categories>> GetAllCategoriesAsync()
        {
            return await _context.Categories
                                 .OrderBy(c => c.CategoryName)
                                 .ToListAsync();
        }

        // ----- Book Prices -----
        public async Task<BookPrice?> GetPriceByBookIdAsync(int bookId)
        {
            return await _context.BookPrices
                                 .FirstOrDefaultAsync(p => p.BookId == bookId);
        }

        public async Task<BookPrice> SetBookPriceAsync(BookPrice price)
        {
            _context.BookPrices.Add(price);
            await _context.SaveChangesAsync();
            return price;
        }

        public async Task<bool> UpdateBookPriceAsync(BookPrice price)
        {
            _context.BookPrices.Update(price);
            return await _context.SaveChangesAsync() > 0;
        }

        // ----- Book Versions -----
        public async Task<IEnumerable<BookVersion>> GetVersionsByBookIdAsync(int bookId)
        {
            return await _context.BookVersions
                                 .Where(v => v.BookId == bookId)
                                 .ToListAsync();
        }

        public async Task<BookVersion> AddBookVersionAsync(BookVersion version)
        {
            _context.BookVersions.Add(version);
            await _context.SaveChangesAsync();
            return version;
        }

        public async Task<BookVersion?> GetVersionByIdAsync(int versionId)
        {
            return await _context.BookVersions
                                 .FirstOrDefaultAsync(v => v.BookVersionId == versionId);
        }

        //Task<IEnumerable<Categories>> IBookService.GetAllCategoriesAsync()
        //{
        //    throw new NotImplementedException();
        //}
        public async Task<UserBooksViewModel> GetUserBooksWithChaptersAsync(int userId)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);

            var books = await _context.Books
                .Where(b => b.UserId == userId)
                .Include(b => b.Chapters)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new UserBook
                {
                    BookId = b.BookId,
                    Title = b.Title,
                    Description = b.Description ?? "No description available",
                    Genre = b.Genre ?? "Uncategorized",
                    Status = b.Status,
                    CreatedAt = b.CreatedAt,
                    UpdatedAt = b.UpdatedAt,
                    TotalChapters = b.Chapters.Count,
                    TotalWords = b.Chapters.Sum(c => c.Content != null ? CalculateWordCount(c.Content) : 0),
                    CoverImagePath = b.CoverImagePath ?? "/images/default-book-cover.jpg",
                    Chapters = b.Chapters
                        .OrderBy(c => c.ChapterNumber)
                        .Select(c => new UserChapter
                        {
                            ChapterId = c.ChapterId,
                            ChapterNumber = c.ChapterNumber,
                            Title = c.Title,
                            Content = c.Content,
                            Status = c.Status,
                            CreatedAt = c.CreatedAt,
                            UpdatedAt = c.UpdatedAt,
                            WordCount = c.Content != null ? CalculateWordCount(c.Content) : 0,
                            PreviewContent = c.Content != null ? GetContentPreview(c.Content, CalculateWordCount(c.Content)) : "No content available"
                        })
                        .ToList()
                })
                .ToListAsync();

            return new UserBooksViewModel
            {
                UserId = userId,
                UserName = user?.FullName ?? "User",
                Books = books
            };
        }

        public async Task<UserBook?> GetUserBookDetailsAsync(int userId, int bookId)
        {
            return await _context.Books
                .Where(b => b.UserId == userId && b.BookId == bookId)
                .Include(b => b.Chapters)
                .Select(b => new UserBook
                {
                    BookId = b.BookId,
                    Title = b.Title,
                    Description = b.Description ?? "No description available",
                    Genre = b.Genre ?? "Uncategorized",
                    Status = b.Status,
                    CreatedAt = b.CreatedAt,
                    UpdatedAt = b.UpdatedAt,
                    TotalChapters = b.Chapters.Count,
                    TotalWords = b.Chapters.Sum(c => CalculateWordCount(c.Content)),
                    CoverImagePath = b.CoverImagePath ?? "/images/default-book-cover.jpg",
                    Chapters = b.Chapters
                        .OrderBy(c => c.ChapterNumber)
                        .Select(c => new UserChapter
                        {
                            ChapterId = c.ChapterId,
                            ChapterNumber = c.ChapterNumber,
                            Title = c.Title,
                            Content = c.Content,
                            Status = c.Status,
                            CreatedAt = c.CreatedAt,
                            UpdatedAt = c.UpdatedAt,
                            WordCount = c.Content != null ? CalculateWordCount(c.Content) : 0,
                            PreviewContent = GetContentPreview(c.Content, CalculateWordCount(c.Content))
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();
        }
        public async Task<List<UserBook>> GetUserBooksSummaryAsync(int userId)
        {
            return await _context.Books
                .Where(b => b.UserId == userId)
                .Include(b => b.Chapters)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new UserBook
                {
                    BookId = b.BookId,
                    Title = b.Title,
                    Description = b.Description ?? "No description available",
                    Genre = b.Genre ?? "Uncategorized",
                    Status = b.Status,
                    CreatedAt = b.CreatedAt,
                    UpdatedAt = b.UpdatedAt,
                    TotalChapters = b.Chapters.Count,
                    TotalWords = b.Chapters.Sum(c => CalculateWordCount(c.Content)),
                    CoverImagePath = b.CoverImagePath ?? "/images/default-book-cover.jpg"
                })
                .ToListAsync();
        }
        private int CalculateWordCount(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return 0;

            // Remove HTML tags for accurate word count
            var plainText = System.Text.RegularExpressions.Regex.Replace(
                content, "<.*?>", string.Empty);

            // Split by multiple whitespace characters and count non-empty words
            var wordCount = 0;
            var wordPattern = new System.Text.RegularExpressions.Regex(@"\b\w+\b");
            wordCount = wordPattern.Matches(plainText).Count;

            return wordCount;
        }

        private string GetContentPreview(string? content, int maxLength = 150)
        {
            if (string.IsNullOrEmpty(content))
                return "No content available";

            // Remove HTML tags for preview
            var plainText = System.Text.RegularExpressions.Regex.Replace(
                content, "<.*?>", string.Empty);

            return plainText.Length <= maxLength
                ? plainText
                : plainText.Substring(0, maxLength) + "...";
        }

        //=======================================
        // Get a single saved API response for a user and book
        //====================================
        public async Task<APIRawResponse?> GetLatestBookResponseAsync(int userId, int bookId)
        {
            // APIRawResponse table
            return await _context.APIRawResponse
                .Where(r => r.UserId == userId && r.BookId == bookId)
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefaultAsync();
        }
        //==========================================
        //   Get all saved books for a user
        //==========================================
        public async Task<IEnumerable<SavedBookDto>> GetSavedBooksForDropdownAsync(int userId)
        {
            if (userId == 0)
                throw new ArgumentException("UserId is required.");

            return await _context.Books
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new SavedBookDto
                {
                    //ResponseId=b.ResponseId,
                    UserId = b.UserId,
                    BookId = b.BookId,
                    BookTitle = b.Title,
                    CreatedDate = b.CreatedAt
                })
                .ToListAsync();
        }

        // Get latest BookId for a user
        public async Task<int?> GetLatestBookIdAsync(int userId)
        {
            if (userId == 0)
                throw new ArgumentException("UserId is required.");

            var latestBook = await _context.Books
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new { b.BookId })
                .FirstOrDefaultAsync();

            return latestBook?.BookId;
        }

        // Get latest book with details
        public async Task<SavedBookDto?> GetLatestBookAsync(int userId)
        {
            if (userId == 0)
                throw new ArgumentException("UserId is required.");

            return await _context.Books
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new SavedBookDto
                {
                    BookId = b.BookId,
                    BookTitle = b.Title,
                    CreatedDate = b.CreatedAt
                })
                .FirstOrDefaultAsync();
        }
    }
}