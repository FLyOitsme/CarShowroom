using CarShowroom.Interfaces;

namespace CarShowroom.Services
{
    public class ImageSearchService : IImageSearchService
    {
        public ImageSearchService()
        {
        }

        public Task<List<ImageSearchResult>> SearchImagesAsync(string query, int count = 10)
        {
            return Task.FromResult(new List<ImageSearchResult>());
        }
    }
}
