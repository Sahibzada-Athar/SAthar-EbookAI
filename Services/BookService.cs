using EBookDashboard.Interfaces;
using EBookDashboard.Models;
using Microsoft.EntityFrameworkCore;
using EBookDashboard.Models.DTO;
namespace EBookDashboard.Services
{
    public class BookService : IBookService
    {
        private readonly ApplicationDbContext _context;

        public BookService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ----- Books -----
        public async Task<IEnumerable<Books>> GetAllBooksAsync()
        {
            return await _context.Books
                                 .Include(b => b.Chapters)
                                 .ToListAsync();
        }

        public async Task<Books?> GetBookByIdAsync(int bookId)
        {
            return await _context.Books
                                 .Include(b => b.Chapters)
                                 .FirstOrDefaultAsync(b => b.BookId == bookId);
        }
        //============================================
        // ----- Get Book Details with Chapters -----
        //============================================
        public async Task<BookDetailsDto?> GetBookDetailsAsync(int userId, int bookId)
        {
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
            if (!string.IsNullOrEmpty(rawResponse.ChapterNumber) && int.TryParse(rawResponse.ChapterNumber, out int parsedChapter))
            {
                chapterNumber = parsedChapter;
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
                    CreatedAt = rawResponse.CreatedAt
                };
        }
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

        Task<IEnumerable<Categories>> IBookService.GetAllCategoriesAsync()
        {
            throw new NotImplementedException();
        }
    }
}