using HackerNewsAPI.Configuration;
using HackerNewsAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HackerNewsAPI.Controllers.V1
{
    [ApiController]
    [Route("api/v1/best-stories")]
    public class BestStoriesController : ControllerBase
    {
        private readonly IBestStoriesService _bestStoriesService;

        public BestStoriesController(IBestStoriesService bestStoriesService)
        {
            _bestStoriesService = bestStoriesService;
        }

        [EnableRateLimiting(RateLimitOptions.PolicyName)]
        [HttpGet("{numberOfStories}/details")]
        public async Task<IActionResult> GetBestStoriesDetails(int numberOfStories)
        {
            var bestStories = await _bestStoriesService.GetBestStoriesAsync(numberOfStories);

            return Ok(bestStories);
        }
    }
}
