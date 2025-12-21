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
            // Поиск изображений отключен, используйте готовые URL
            return Task.FromResult(new List<ImageSearchResult>());
        }
    }
}
