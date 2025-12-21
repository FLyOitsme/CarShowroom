namespace CarShowroom.Interfaces
{
    public interface IImageSearchService
    {
        Task<List<ImageSearchResult>> SearchImagesAsync(string query, int count = 10);
    }

    public class ImageSearchResult
    {
        public string Url { get; set; } = string.Empty;
        public string ThumbnailUrl { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}

