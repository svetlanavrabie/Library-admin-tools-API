using AutoFixture;
using LibraryAPI.Dtos;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace LibraryAPI.Integration.Tests
{
    public class ApiIntegrationTests : IClassFixture<LibraryApiFactory>
    {
        private const string SeededAuthorId = "d28888e9-2ba9-473a-a40f-e38cb54f9b35";
        private readonly HttpClient _client;
        private readonly LibraryApiFactory _factory;
        private readonly Fixture _fixture = new();

        public ApiIntegrationTests(LibraryApiFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private CreateAuthorDto NewAuthor()
        {
            var dto = _fixture.Build<CreateAuthorDto>()
                .With(a => a.FirstName, "John")
                .With(a => a.LastName, "Doe")
                .With(a => a.MainCategory, "Tests")
                .With(a => a.DateOfBirth, new DateTimeOffset(1980, 1, 1, 0, 0, 0, TimeSpan.Zero))
                .With(a => a.Courses, new List<CreateCourseDto>())
                .Create();
            return dto;
        }

        private CreateCourseDto NewCourse() =>
            new() { Title = _fixture.Create<string>()[..20], Description = "Description " + Guid.NewGuid() };

        private static JsonContent Json(object value, string mediaType = "application/json") =>
            JsonContent.Create(value, value.GetType(), new MediaTypeHeaderValue(mediaType));

        private async Task<Guid> CreateAuthorAsync()
        {
            var response = await _client.PostAsync("api/authors", Json(NewAuthor()));

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var segments = response.Headers.Location!.AbsolutePath.Split('/');
            return Guid.Parse(segments[^1]);
        }

        private async Task<Guid> CreateCourseAsync(Guid authorId)
        {
            var response = await _client.PostAsJsonAsync($"api/authors/{authorId}/courses", NewCourse());

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            return Guid.Parse(response.Headers.Location!.AbsolutePath.Split('/')[^1]);
        }

        [Fact]
        public async Task GeneratedData_IsAvailableThroughApi()
        {
            var author = _factory.SeededAuthors[0];

            var authorResponse = await _client.GetAsync($"api/authors/{author.Id}");
            var coursesResponse = await _client.GetAsync($"api/authors/{author.Id}/courses");

            Assert.Equal(HttpStatusCode.OK, authorResponse.StatusCode);
            Assert.Equal(HttpStatusCode.OK, coursesResponse.StatusCode);

            using var doc = JsonDocument.Parse(await coursesResponse.Content.ReadAsStringAsync());

            Assert.Equal(author.Courses.Count, doc.RootElement.GetArrayLength());
        }

        [Fact]
        public async Task GetRoot_ReturnsLinks()
        {
            var response = await _client.GetAsync("api");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

            Assert.Equal(3, doc.RootElement.GetArrayLength());
        }

        [Fact]
        public async Task GetAuthors_ReturnsPagedSeedData()
        {
            var response = await _client.GetAsync("api/authors?pageSize=3");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(response.Headers.Contains("X-Pagination"));

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

            Assert.Equal(3, doc.RootElement.GetProperty("value").GetArrayLength());
        }

        [Fact]
        public async Task GetAuthors_InvalidOrderBy_ReturnsBadRequest()
        {
            var response = await _client.GetAsync("api/authors?orderBy=unknown");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetAuthors_InvalidFields_ReturnsBadRequest()
        {
            var response = await _client.GetAsync("api/authors?fields=unknown");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetAuthors_FilterByCategory_ReturnsOnlyMatching()
        {
            var response = await _client.GetAsync("api/authors?mainCategory=Rum");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

            Assert.True(doc.RootElement.GetProperty("value").GetArrayLength() >= 2);
        }

        [Fact]
        public async Task HeadAuthors_ReturnsOk()
        {
            var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Head, "api/authors"));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task OptionsAuthors_ReturnsAllowHeader()
        {
            var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Options, "api/authors"));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("POST", response.Content.Headers.Allow.Count > 0
                ? string.Join(",", response.Content.Headers.Allow)
                : string.Join(",", response.Headers.GetValues("Allow")));
        }

        [Fact]
        public async Task GetAuthor_Seeded_ReturnsOk()
        {
            var response = await _client.GetAsync($"api/authors/{SeededAuthorId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetAuthor_Unknown_ReturnsNotFound()
        {
            var response = await _client.GetAsync($"api/authors/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetAuthor_FullHateoasMediaType_ReturnsLinks()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/authors/{SeededAuthorId}");
            request.Headers.Accept.Clear();
            request.Headers.Accept.ParseAdd("application/vdn.marvin.author.full.hateoas+json");

            var response = await _client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("links", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task CreateAuthor_Invalid_ReturnsUnprocessableEntity()
        {
            var dto = NewAuthor();
            dto.FirstName = null;

            var response = await _client.PostAsync("api/authors", Json(dto));

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Fact]
        public async Task CreateAuthorWithDateOfDeath_ReturnsCreated()
        {
            var body = new
            {
                firstName = "Jane",
                lastName = "Roe",
                dateOfBirth = "1900-01-01T00:00:00+00:00",
                dateOfDeath = "1970-01-01T00:00:00+00:00",
                mainCategory = "History"
            };
            var content = Json(body, "application/vdn.marvin.authorforcreationwithdateofdeath+json");

            var response = await _client.PostAsync("api/authors", content);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task AuthorFlow_CreateGetDelete()
        {
            var id = await CreateAuthorAsync();

            var get = await _client.GetAsync($"api/authors/{id}");

            Assert.Equal(HttpStatusCode.OK, get.StatusCode);

            var delete = await _client.DeleteAsync($"api/authors/{id}");

            Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

            var getAfter = await _client.GetAsync($"api/authors/{id}");

            Assert.Equal(HttpStatusCode.NotFound, getAfter.StatusCode);

            var deleteAgain = await _client.DeleteAsync($"api/authors/{id}");

            Assert.Equal(HttpStatusCode.NotFound, deleteAgain.StatusCode);
        }

        [Fact]
        public async Task AuthorCollectionsFlow_CreateThenGet()
        {
            var authors = new List<CreateAuthorDto> { NewAuthor(), NewAuthor() };

            var create = await _client.PostAsJsonAsync("api/authorcollections", authors);

            Assert.Equal(HttpStatusCode.Created, create.StatusCode);

            var get = await _client.GetAsync(create.Headers.Location);

            Assert.Equal(HttpStatusCode.OK, get.StatusCode);

            using var doc = JsonDocument.Parse(await get.Content.ReadAsStringAsync());

            Assert.Equal(2, doc.RootElement.GetArrayLength());
        }

        [Fact]
        public async Task GetAuthorCollection_WithUnknownId_ReturnsNotFound()
        {
            var response = await _client.GetAsync($"api/authorcollections({Guid.NewGuid()})");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CoursesFlow_CreateGetUpdatePatchDelete()
        {
            var authorId = await CreateAuthorAsync();
            var courseId = await CreateCourseAsync(authorId);

            var list = await _client.GetAsync($"api/authors/{authorId}/courses");

            Assert.Equal(HttpStatusCode.OK, list.StatusCode);

            using (var doc = JsonDocument.Parse(await list.Content.ReadAsStringAsync()))
            {
                Assert.Equal(1, doc.RootElement.GetArrayLength());
            }

            var get = await _client.GetAsync($"api/authors/{authorId}/courses/{courseId}");

            Assert.Equal(HttpStatusCode.OK, get.StatusCode);

            var put = await _client.PutAsJsonAsync($"api/authors/{authorId}/courses/{courseId}",
                new UpdateCourseDto { Title = "Updated", Description = "Updated description" });

            Assert.Equal(HttpStatusCode.NoContent, put.StatusCode);

            var patchBody = "[{\"op\":\"replace\",\"path\":\"/title\",\"value\":\"Patched\"}]";
            var patch = await _client.PatchAsync($"api/authors/{authorId}/courses/{courseId}",
                new StringContent(patchBody, Encoding.UTF8, "application/json-patch+json"));

            Assert.Equal(HttpStatusCode.NoContent, patch.StatusCode);

            var afterPatch = await _client.GetAsync($"api/authors/{authorId}/courses/{courseId}");

            Assert.Contains("Patched", await afterPatch.Content.ReadAsStringAsync());

            var delete = await _client.DeleteAsync($"api/authors/{authorId}/courses/{courseId}");

            Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

            var getAfter = await _client.GetAsync($"api/authors/{authorId}/courses/{courseId}");

            Assert.Equal(HttpStatusCode.NotFound, getAfter.StatusCode);
        }

        [Fact]
        public async Task PutCourse_NonExisting_Upserts()
        {
            var authorId = await CreateAuthorAsync();
            var courseId = Guid.NewGuid();

            var put = await _client.PutAsJsonAsync($"api/authors/{authorId}/courses/{courseId}",
                new UpdateCourseDto { Title = "Upserted", Description = "Upserted description" });

            Assert.Equal(HttpStatusCode.Created, put.StatusCode);
        }

        [Fact]
        public async Task PatchCourse_NonExisting_Upserts()
        {
            var authorId = await CreateAuthorAsync();
            var courseId = Guid.NewGuid();
            var patchBody = "[{\"op\":\"add\",\"path\":\"/title\",\"value\":\"T\"},{\"op\":\"add\",\"path\":\"/description\",\"value\":\"D\"}]";

            var patch = await _client.PatchAsync($"api/authors/{authorId}/courses/{courseId}",
                new StringContent(patchBody, Encoding.UTF8, "application/json-patch+json"));

            Assert.Equal(HttpStatusCode.Created, patch.StatusCode);
        }

        [Fact]
        public async Task CreateCourse_TitleEqualsDescription_ReturnsUnprocessableEntity()
        {
            var authorId = await CreateAuthorAsync();

            var response = await _client.PostAsJsonAsync($"api/authors/{authorId}/courses",
                new CreateCourseDto { Title = "Same", Description = "Same" });

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Fact]
        public async Task Courses_UnknownAuthor_ReturnsNotFound()
        {
            var authorId = Guid.NewGuid();

            Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"api/authors/{authorId}/courses")).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"api/authors/{authorId}/courses/{Guid.NewGuid()}")).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await _client.PostAsJsonAsync($"api/authors/{authorId}/courses", NewCourse())).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await _client.DeleteAsync($"api/authors/{authorId}/courses/{Guid.NewGuid()}")).StatusCode);
        }

        [Fact]
        public async Task DeleteAuthor_RemovesItsCourses()
        {
            var authorId = await CreateAuthorAsync();
            await CreateCourseAsync(authorId);

            await _client.DeleteAsync($"api/authors/{authorId}");

            var list = await _client.GetAsync($"api/authors/{authorId}/courses");

            Assert.Equal(HttpStatusCode.NotFound, list.StatusCode);
        }
    }
}
