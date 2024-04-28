using System.Net.Http.Headers;
using System.Text.Json;
using Chronos.Chain.Api.Model.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace Chronos.Chain.Api.Controllers
{
    [Route("api/github")]
    [ApiController]
    public class GitHubController : ControllerBase
    {
        private readonly ILogger<GitHubController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public GitHubController(
            ILogger<GitHubController> logger,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("repositories")]
        public async Task<IActionResult> Get()
        {
            var client = _httpClientFactory.CreateClient();
            var githubToken = _configuration["GitHub:Token"];
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", githubToken);
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Chronos.Chain.Api", "1.0"));
            var response = await client.GetAsync("https://api.github.com/user/repos");
            var content = await response.Content.ReadAsStringAsync();
            var repositories = JsonSerializer.Deserialize<List<Repository>>(content);

            return Ok(repositories.Select(repository => new RepositoryInfo
            {
                Id = repository.Id,
                Name = repository.Name,
                Description = repository.Description,
                ForksCount = repository.ForksCount,
                StargazersCount = repository.StargazersCount,
                Language = repository.Language,
                Url = repository.Url,
            }));
        }
    }
}
