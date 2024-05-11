namespace HackerNewsAPI.HackerNews
{
    public interface IHackerNewsApiService
    {
        Task<IEnumerable<string>> GetBestStoriesIdsAsync(int numberOfBestStories);

        Task<IEnumerable<BestStoryResponse?>> GetStoriesDetailsAsync(IEnumerable<string> ids);
    }
}
