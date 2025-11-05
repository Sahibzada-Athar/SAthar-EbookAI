using EBookDashboard.Interfaces;
using EBookDashboard.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace EBookDashboard.Services
{
    public class APIRawResponseService : IAPIRawResponseService
    {
        private readonly ApplicationDbContext _context;

        public APIRawResponseService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> SaveRawResponseAsync(AIBookRequest request, string responseData, string endpoint, string statusCode, string errorMessage = null)
        {
            var rawResponse = new APIRawResponse
            {
                Endpoint = endpoint,
                ChapterNumber = request.ChapterNo,
                Title = request.Title,
                RequestData = JsonConvert.SerializeObject(request),
                ResponseData = responseData,
                UserId = int.TryParse(request.UserId, out int uid) ? uid : (int?)null,
                BookId = int.TryParse(request.BookId, out int bid) ? bid : (int?)null,
                StatusCode = statusCode,
                ErrorMessage = errorMessage,
                CreatedAt = DateTime.UtcNow
            };

            _context.APIRawResponse.Add(rawResponse);
            await _context.SaveChangesAsync();

            Console.WriteLine($"💾 Raw response saved with ID: {rawResponse.ResponseId}");
            return rawResponse.ResponseId;
        }

        public async Task<APIRawResponse> GetRawResponseAsync(int responseId)
        {
            return await _context.APIRawResponse.FirstOrDefaultAsync(r => r.ResponseId == responseId);
        }

        Task<Models.APIRawResponse> IAPIRawResponseService.GetRawResponseAsync(int responseId)
        {
            throw new NotImplementedException();
        }
    }
}
