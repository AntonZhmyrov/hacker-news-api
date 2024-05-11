using HackerNewsAPI.HackerNews;
using HackerNewsAPI.Helpers;
using HackerNewsAPI.Models;

namespace HackerNewsAPI.Services
{
    public class BestStoriesService : IBestStoriesService
    {
        private readonly IHackerNewsApiService _hackerNewsApiService;

        public BestStoriesService(IHackerNewsApiService hackerNewsApiService)
        {

            _hackerNewsApiService = hackerNewsApiService;
        }

        public async Task<IEnumerable<BestStory>> GetBestStoriesAsync(int numberOfStories)
        {
            var bestStoriesIds = await _hackerNewsApiService.GetBestStoriesIdsAsync(numberOfStories);
            var bestStoriesResponses = await _hackerNewsApiService.GetStoriesDetailsAsync(bestStoriesIds);
            var bestStories = new List<BestStory>();

            foreach (var bestStoryResponse in bestStoriesResponses)
            {
                if (bestStoryResponse is null)
                {
                    continue;
                }

                var bestStory = new BestStory
                {
                    CommentCount = bestStoryResponse.Descendants,
                    PostedBy = bestStoryResponse.By,
                    Score = bestStoryResponse.Score,
                    Time = DateTimeHelper.FromUnixSeconds(bestStoryResponse.Time),
                    Title = bestStoryResponse.Title,
                    Uri = bestStoryResponse.Url
                };

                bestStories.Add(bestStory);
            }

            return bestStories.OrderByDescending(x => x.Score);
        }
    }
}
