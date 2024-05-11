using System.Text.Json;

namespace HackerNewsAPI.HackerNews
{
    public class HackerNewsApiService : IHackerNewsApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<HackerNewsApiService> _logger;

        public HackerNewsApiService(HttpClient httpClient, ILogger<HackerNewsApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<IEnumerable<string>> GetBestStoriesIdsAsync(int numberOfBestStories)
        {
            const string requestUri = "beststories.json";

            try
            {
                using var responseMessage = await _httpClient.GetAsync(requestUri);
                responseMessage.EnsureSuccessStatusCode();

                var responseString = await responseMessage.Content.ReadAsStringAsync();
                var ids = responseString.Trim('[', ']').Split(',', StringSplitOptions.RemoveEmptyEntries);

                return ids.Take(numberOfBestStories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not retrieve ids of best stories");
                throw;
            }
        }

        public async Task<IEnumerable<BestStoryResponse?>> GetStoriesDetailsAsync(IEnumerable<string> ids)
        {
            const string requestUri = "item/{0}.json";
            const int batchSize = 50;

            var bestStories = new List<BestStoryResponse?>();
            var requestUris = ids.Select(id => string.Format(requestUri, id)).ToArray();
            int numberOfBatches = (int)Math.Ceiling((double)requestUris.Length / batchSize);

            for (int i = 0; i < numberOfBatches; i++)
            {
                var currentUris = requestUris.Skip(i * batchSize).Take(batchSize);
                var requests = currentUris.Select(x => GetStoryDetailsAsync(x));
                bestStories.AddRange(await Task.WhenAll(requests));
            }

            return bestStories;
        }

        private async Task<BestStoryResponse?> GetStoryDetailsAsync(string requestUri)
        {
            try
            {
                using var responseMessage = await _httpClient.GetAsync(requestUri);
                responseMessage.EnsureSuccessStatusCode();

                var responseString = await responseMessage.Content.ReadAsStringAsync();

                return JsonSerializer.Deserialize<BestStoryResponse>(
                    responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not retrieve story details");
                throw;
            }
        }
    }
}
