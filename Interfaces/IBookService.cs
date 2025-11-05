using EBookDashboard.Models;
using EBookDashboard.Models.DTO;

namespace EBookDashboard.Interfaces
{
    public interface IBookService
    {
        // Books
        Task<IEnumerable<Books>> GetAllBooksAsync();
        Task<Books?> GetBookByIdAsync(int bookId);
        Task<Books> CreateBookAsync(Books book);
        Task<Books> CreateBookFromRequestAsync(CreateBookRequest request); // Updated parameter type
        Task<bool> UpdateBookAsync(Books book);
        Task<bool> DeleteBookAsync(int bookId);

        // Categories
        Task<IEnumerable<Categories>> GetAllCategoriesAsync();

        // Book Prices
        Task<BookPrice?> GetPriceByBookIdAsync(int bookId);
        Task<BookPrice> SetBookPriceAsync(BookPrice price);
        Task<bool> UpdateBookPriceAsync(BookPrice price);

        // Book Versions
        Task<IEnumerable<BookVersion>> GetVersionsByBookIdAsync(int bookId);
        Task<BookVersion> AddBookVersionAsync(BookVersion version);
        Task<BookVersion?> GetVersionByIdAsync(int versionId);
        Task<BookDetailsDto?> GetBookDetailsAsync(int userId, int bookId);
        Task<BookDetailsDto?> GetBookDetailsFromRawDataAsync(int userId, int bookId);
    }
}
