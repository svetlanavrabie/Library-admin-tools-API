using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Services;
using LibraryAPI.Controllers;
using LibraryAPI.Dtos;
using LibraryAPI.Profiles;
using LibraryAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace LibraryAPI.Tests
{
    public class AuthorsControllerTests
    {
        private readonly Fixture _fixture = new();
        private readonly Mock<ICourseLibraryRepository> _repo = new();
        private readonly AuthorsController _sut;
        private readonly AuthorCollectionsController _collections;

        public AuthorsControllerTests()
        {
            var mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<AuthorProfile>();
                cfg.AddProfile<CoursesProfile>();
            }, NullLoggerFactory.Instance).CreateMapper();

            var urlHelper = new Mock<IUrlHelper>();

            urlHelper.Setup(u => u.Link(It.IsAny<string>(), It.IsAny<object>())).Returns("http://localhost/x");

            _sut = new AuthorsController(_repo.Object, mapper,
                new PropertyMappingService(), new PropertyCheckerService())
            {
                Url = urlHelper.Object,
                ControllerContext = new ControllerContext
                {
                    HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext()
                }
            };

            _collections = new AuthorCollectionsController(_repo.Object, mapper);
        }

        private Author NewAuthor() =>
            _fixture.Build<Author>()
                .With(a => a.DateOfBirth, new DateTimeOffset(1980, 1, 1, 0, 0, 0, TimeSpan.Zero))
                .With(a => a.DateOfDeath, (DateTimeOffset?)null)
                .Without(a => a.Courses)
                .Create();

        [Fact]
        public void Ctor_NullDependencies_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => new AuthorsController(null, Mock.Of<IMapper>(),
                Mock.Of<IPropertyMappingService>(), Mock.Of<IPropertyCheckerService>()));
            Assert.Throws<ArgumentNullException>(() => new AuthorsController(_repo.Object, null,
                Mock.Of<IPropertyMappingService>(), Mock.Of<IPropertyCheckerService>()));
        }

        [Fact]
        public void GetAuthors_InvalidOrderBy_ReturnsBadRequest()
        {
            var result = _sut.GetAuthors(new LibraryAPI.ResourcesParameters.AuthorResourcesParameters { OrderBy = "nope" });

            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public void GetAuthors_InvalidFields_ReturnsBadRequest()
        {
            var result = _sut.GetAuthors(new LibraryAPI.ResourcesParameters.AuthorResourcesParameters { Fields = "nope" });

            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public void GetAuthor_InvalidMediaType_ReturnsBadRequest()
        {
            var result = _sut.GetAuthor(Guid.NewGuid(), null, "not a media type");

            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public void GetAuthor_Missing_ReturnsNotFound()
        {
            _repo.Setup(r => r.GetAuthor(It.IsAny<Guid>())).Returns((Author)null);

            var result = _sut.GetAuthor(Guid.NewGuid(), null, "application/json");

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void GetAuthor_Existing_ReturnsOk()
        {
            var author = NewAuthor();

            _repo.Setup(r => r.GetAuthor(author.Id)).Returns(author);

            var result = _sut.GetAuthor(author.Id, null, "application/json");

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public void GetAuthor_HateoasMediaType_IncludesLinks()
        {
            var author = NewAuthor();

            _repo.Setup(r => r.GetAuthor(author.Id)).Returns(author);

            var result = _sut.GetAuthor(author.Id, null, "application/vdn.marvin.author.full.hateoas+json");

            var ok = Assert.IsType<OkObjectResult>(result);

            Assert.True(((IDictionary<string, object>)ok.Value).ContainsKey("links"));
        }

        [Fact]
        public void CreateAuthor_Valid_AddsAndReturnsCreated()
        {
            var dto = _fixture.Build<CreateAuthorDto>()
                .With(a => a.Courses, new List<CreateCourseDto>()).Create();

            var result = _sut.CreateAuthor(dto);

            Assert.IsType<CreatedAtRouteResult>(result.Result);
            _repo.Verify(r => r.AddAuthor(It.IsAny<Author>()), Times.Once);
            _repo.Verify(r => r.Save(), Times.Once);
        }

        [Fact]
        public void CreateAuthor_Null_ReturnsBadRequest()
        {
            Assert.IsType<BadRequestResult>(_sut.CreateAuthor(null).Result);
        }

        [Fact]
        public void DeleteAuthor_Existing_ReturnsNoContent()
        {
            var author = NewAuthor();

            _repo.Setup(r => r.GetAuthor(author.Id)).Returns(author);

            var result = _sut.DeleteAuthor(author.Id);

            Assert.IsType<NoContentResult>(result);
            _repo.Verify(r => r.DeleteAuthor(author), Times.Once);
            _repo.Verify(r => r.Save(), Times.Once);
        }

        [Fact]
        public void DeleteAuthor_Missing_ReturnsNotFound()
        {
            _repo.Setup(r => r.GetAuthor(It.IsAny<Guid>())).Returns((Author)null);

            Assert.IsType<NotFoundResult>(_sut.DeleteAuthor(Guid.NewGuid()));
        }

        [Fact]
        public void GetAuthorsOptions_SetsAllowHeader()
        {
            var result = _sut.GetAuthorsOptions();

            Assert.IsType<OkResult>(result);
            Assert.Equal("GET,OPTIONS,POST", _sut.Response.Headers["Allow"].ToString());
        }

        [Fact]
        public void AuthorCollection_Get_MismatchedCount_ReturnsNotFound()
        {
            var ids = new[] { Guid.NewGuid(), Guid.NewGuid() };

            _repo.Setup(r => r.GetAuthors(ids)).Returns(new List<Author> { NewAuthor() });

            Assert.IsType<NotFoundResult>(_collections.GetAuthorCollection(ids));
        }

        [Fact]
        public void AuthorCollection_Get_AllFound_ReturnsOk()
        {
            var authors = Enumerable.Range(0, 2).Select(_ => NewAuthor()).ToList();
            var ids = authors.Select(a => a.Id).ToList();

            _repo.Setup(r => r.GetAuthors(ids)).Returns(authors);

            var result = _collections.GetAuthorCollection(ids);

            var ok = Assert.IsType<OkObjectResult>(result);

            Assert.Equal(2, Assert.IsAssignableFrom<IEnumerable<AuthorDto>>(ok.Value).Count());
        }

        [Fact]
        public void AuthorCollection_Create_ReturnsCreated()
        {
            var dtos = _fixture.Build<CreateAuthorDto>()
                .With(a => a.Courses, new List<CreateCourseDto>()).CreateMany(2).ToList();

            var result = _collections.CreateAuthorCollection(dtos);

            Assert.IsType<CreatedAtRouteResult>(result.Result);
            _repo.Verify(r => r.AddAuthor(It.IsAny<Author>()), Times.Exactly(2));
            _repo.Verify(r => r.Save(), Times.Once);
        }

        [Fact]
        public void AuthorCollection_Create_Null_ReturnsBadRequest()
        {
            Assert.IsType<BadRequestResult>(_collections.CreateAuthorCollection(null).Result);
        }

        [Fact]
        public void Root_ReturnsThreeLinks()
        {
            var urlHelper = new Mock<IUrlHelper>();

            urlHelper.Setup(u => u.Link(It.IsAny<string>(), It.IsAny<object>())).Returns("http://localhost/x");

            var root = new RootController { Url = urlHelper.Object };

            var ok = Assert.IsType<OkObjectResult>(root.GetRoot());

            Assert.Equal(3, Assert.IsAssignableFrom<IEnumerable<LinkDto>>(ok.Value).Count());
        }
    }
}
