using HackerNewsAPI.Models;

namespace HackerNewsAPI.Services
{
    public interface IBestStoriesService
    {
        Task<IEnumerable<BestStory>> GetBestStoriesAsync(int numberOfStories);
    }
}
