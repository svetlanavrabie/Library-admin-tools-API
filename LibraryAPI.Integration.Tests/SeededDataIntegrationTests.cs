using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace LibraryAPI.Integration.Tests
{
    public class SeededDataIntegrationTests : IClassFixture<LibraryApiFactory>
    {
        private readonly HttpClient _client;
        private readonly LibraryApiFactory _factory;

        public SeededDataIntegrationTests(LibraryApiFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        [Fact]
        public async Task GetCourse_Seeded_ReturnsOk()
        {
            var author = _factory.SeededAuthors[1];
            var course = author.Courses.First();

            var response = await _client.GetAsync($"api/authors/{author.Id}/courses/{course.Id}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetCourse_UnknownCourse_ReturnsNotFound()
        {
            var author = _factory.SeededAuthors[1];

            var response = await _client.GetAsync($"api/authors/{author.Id}/courses/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetCourses_UnknownAuthor_ReturnsNotFound()
        {
            var response = await _client.GetAsync($"api/authors/{Guid.NewGuid()}/courses");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetAuthors_SecondPage_HasPreviousLink()
        {
            var response = await _client.GetAsync("api/authors?pageNumber=2&pageSize=5");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var pagination = response.Headers.GetValues("X-Pagination").Single();
            using var doc = JsonDocument.Parse(pagination);
            Assert.Equal(2, doc.RootElement.GetProperty("currentPage").GetInt32());
            Assert.True(doc.RootElement.GetProperty("totalCount").GetInt32() >= 30);
        }

        [Fact]
        public async Task GetAuthors_OrderByName_ReturnsOk()
        {
            var response = await _client.GetAsync("api/authors?orderBy=name desc,age&pageSize=5");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetAuthors_SearchQuery_ReturnsMatches()
        {
            var author = _factory.SeededAuthors[2];

            var response = await _client.GetAsync($"api/authors?searchQuery={author.FirstName}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            Assert.True(doc.RootElement.GetProperty("value").GetArrayLength() >= 1);
        }

        [Fact]
        public async Task GetAuthor_WithFields_ReturnsOnlyRequestedFields()
        {
            var author = _factory.SeededAuthors[0];

            var response = await _client.GetAsync($"api/authors/{author.Id}?fields=id,name");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            Assert.True(doc.RootElement.TryGetProperty("name", out _));
            Assert.False(doc.RootElement.TryGetProperty("mainCategory", out _));
        }

        [Fact]
        public async Task DeleteCourse_Seeded_ReturnsNoContentThenNotFound()
        {
            var author = _factory.SeededAuthors[3];
            var course = author.Courses.First();

            var first = await _client.DeleteAsync($"api/authors/{author.Id}/courses/{course.Id}");
            var second = await _client.GetAsync($"api/authors/{author.Id}/courses/{course.Id}");

            Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, second.StatusCode);
        }

        [Fact]
        public async Task CreateCourse_UnknownAuthor_ReturnsNotFound()
        {
            var response = await _client.PostAsJsonAsync(
                $"api/authors/{Guid.NewGuid()}/courses",
                new { Title = "Title", Description = "Description" });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
