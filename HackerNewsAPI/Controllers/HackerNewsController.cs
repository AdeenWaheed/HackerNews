using HackerNewsAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Newtonsoft.Json;

namespace HackerNewsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HackerNewsController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public HackerNewsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Get best n stories from Hacker News API
        /// </summary>
        /// <param name="n">Number of stories to be returned</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [EnableRateLimiting("fixed")]
        [HttpGet("getBestStories")]
        public async Task<IEnumerable<StoryDetails>> GetBestStories(int n)
        {
            try
            {
                var bestStories = new List<StoryDetails>();
                using (HttpClient httpClient = new HttpClient())
                {
                    string bestStoriesAPIUrl = $"{_configuration["HackerNewsAPIS:BestStoriesAPIUrl"]}.json";
                    HttpRequestMessage bestStoriesRequest = new HttpRequestMessage(HttpMethod.Get, bestStoriesAPIUrl);
                    HttpResponseMessage bestStoriesResponse = await httpClient.SendAsync(bestStoriesRequest);

                    if (bestStoriesResponse.IsSuccessStatusCode)
                    {
                        string bestStoriesResponseBody = await bestStoriesResponse.Content.ReadAsStringAsync();
                        var bestStoryIds = JsonConvert.DeserializeObject<List<int>>(bestStoriesResponseBody);

                        foreach (var storyId in bestStoryIds.Take(n))
                        {
                            string storyDetailsAPIUrl = $"{_configuration["HackerNewsAPIS:StoryDetailsAPIUrl"]}{storyId}.json";
                            HttpRequestMessage storyDetailsRequest = new HttpRequestMessage(HttpMethod.Get, storyDetailsAPIUrl);
                            HttpResponseMessage storyDetailsResponse = await httpClient.SendAsync(storyDetailsRequest);

                            if (storyDetailsResponse.IsSuccessStatusCode)
                            {
                                string storyDetailsResponseBody = await storyDetailsResponse.Content.ReadAsStringAsync();
                                var storyDetails = JsonConvert.DeserializeObject<StoryDetails>(storyDetailsResponseBody);
                                bestStories.Add(storyDetails);
                            }
                        }
                    }
                    else
                    {
                        throw new Exception($"Error from Hacker News API: {bestStoriesResponse.StatusCode}");
                    }
                }

                return bestStories.OrderByDescending(x => x.Score);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
