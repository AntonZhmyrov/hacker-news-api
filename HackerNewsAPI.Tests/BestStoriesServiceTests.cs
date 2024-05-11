using HackerNewsAPI.HackerNews;
using HackerNewsAPI.Helpers;
using HackerNewsAPI.Services;
using Moq;
using Xunit;

namespace HackerNewsAPI.Tests
{
    public class BestStoriesServiceTests
    {
        private readonly Mock<IHackerNewsApiService> _hackerNewsApiService = new();

        [Fact]
        public async Task GetBestStoriesAsync_OneItem_Success()
        {
            // Arrange
            int numberOfStories = 1;
            var bestStoryIds = new[] { "7689654" };
            var bestStoryResponse = new BestStoryResponse
            {
                By = "simonunai",
                Descendants = 749,
                Score = 367,
                Time = 1715036614,
                Title = "Caniemail.com – like caniuse but for email content",
                Url = "https://www.caniemail.com/"
            };

            var bestStoryResponses = new[]
            {
                bestStoryResponse
            };

            _hackerNewsApiService.Setup(x => x.GetBestStoriesIdsAsync(numberOfStories)).ReturnsAsync(bestStoryIds);
            _hackerNewsApiService.Setup(x => x.GetStoriesDetailsAsync(bestStoryIds)).ReturnsAsync(bestStoryResponses);

            var bestStoriesService = new BestStoriesService(_hackerNewsApiService.Object);

            // Act
            var result = (await bestStoriesService.GetBestStoriesAsync(numberOfStories))?.ToArray();

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.True(result!.Length == 1);

            var bestStoryResult = result.First();
            Assert.NotNull(bestStoryResult);

            Assert.Equal(bestStoryResponse.Title, bestStoryResult.Title);
            Assert.Equal(bestStoryResponse.Url, bestStoryResult.Uri);
            Assert.Equal(DateTimeHelper.FromUnixSeconds(bestStoryResponse.Time), bestStoryResult.Time);
            Assert.Equal(bestStoryResponse.Descendants, bestStoryResult.CommentCount);
            Assert.Equal(bestStoryResponse.By, bestStoryResult.PostedBy);
            Assert.Equal(bestStoryResponse.Score, bestStoryResult.Score);
        }

        [Fact]
        public async Task GetBestStoriesAsync_MultipleItems_CheckSortingByScore_Success()
        {
            // Arrange
            int numberOfStories = 3;
            var bestStoryIds = new[] { "8765678", "7689654", "9654673" };

            var bestStoryResponses = new[]
            {
                new BestStoryResponse
                {
                    By = "simonunai",
                    Descendants = 749,
                    Score = 367,
                    Time = 1715036614,
                    Title = "Caniemail.com – like caniuse but for email content",
                    Url = "https://www.caniemail.com/"
                },
                new BestStoryResponse
                {
                    By = "johndoe",
                    Descendants = 623,
                    Score = 754,
                    Time = 1715036617,
                    Title = "Cold brew coffee in 3 minutes using acoustic cavitation",
                    Url = "https://www.unsw.edu.au/newsroom/news/2024/05/Ultrasonic_cold_brew_coffee_ready_under_three_minutes"
                },
                new BestStoryResponse
                {
                    By = "tarasvelychko",
                    Descendants = 3421,
                    Score = 1254,
                    Time = 1715036619,
                    Title = "Attackers can decloak routing-based VPNs",
                    Url = "https://www.leviathansecurity.com/blog/tunnelvision"
                },
            };

            _hackerNewsApiService.Setup(x => x.GetBestStoriesIdsAsync(numberOfStories)).ReturnsAsync(bestStoryIds);
            _hackerNewsApiService.Setup(x => x.GetStoriesDetailsAsync(bestStoryIds)).ReturnsAsync(bestStoryResponses);

            var bestStoriesService = new BestStoriesService(_hackerNewsApiService.Object);

            // Act
            var result = (await bestStoriesService.GetBestStoriesAsync(numberOfStories))?.ToArray();

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.True(result!.Length == 3);

            Assert.Equal(1254, result[0].Score);
            Assert.Equal(754, result[1].Score);
            Assert.Equal(367, result[2].Score);
        }
    }
}
