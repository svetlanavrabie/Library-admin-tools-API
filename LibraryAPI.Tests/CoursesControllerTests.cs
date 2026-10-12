using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Services;
using LibraryAPI.Controllers;
using LibraryAPI.Dtos;
using LibraryAPI.Profiles;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace LibraryAPI.Tests
{
    public class CoursesControllerTests
    {
        private readonly IFixture _fixture;
        private readonly Mock<ICourseLibraryRepository> _repo;
        private readonly CoursesController _sut;

        public CoursesControllerTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            var mapper = new MapperConfiguration(
                cfg => cfg.AddProfile<CoursesProfile>(), NullLoggerFactory.Instance).CreateMapper();

            _repo = new Mock<ICourseLibraryRepository>();
            _sut = new CoursesController(_repo.Object, mapper);
        }

        private Course NewCourse(Guid authorId) =>
            _fixture.Build<Course>().With(c => c.AuthorId, authorId).Without(c => c.Author).Create();

        [Fact]
        public void Ctor_NullRepository_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new CoursesController(null, Mock.Of<IMapper>()));
        }

        [Fact]
        public void Ctor_NullMapper_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new CoursesController(_repo.Object, null));
        }

        [Fact]
        public void GetCourses_UnknownAuthor_ReturnsNotFound()
        {
            _repo.Setup(r => r.AuthorExists(It.IsAny<Guid>())).Returns(false);

            var result = _sut.GetCoursesForAuhtor(Guid.NewGuid());

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public void GetCourses_ReturnsMappedCourses()
        {
            var authorId = Guid.NewGuid();
            var courses = Enumerable.Range(0, 3).Select(_ => NewCourse(authorId)).ToList();

            _repo.Setup(r => r.AuthorExists(authorId)).Returns(true);
            _repo.Setup(r => r.GetCourses(authorId)).Returns(courses);

            var result = _sut.GetCoursesForAuhtor(authorId);

            var ok = Assert.IsType<OkObjectResult>(result.Result);

            Assert.Equal(3, Assert.IsAssignableFrom<IEnumerable<CoursDto>>(ok.Value).Count());
        }

        [Fact]
        public void GetCourse_Missing_ReturnsNotFound()
        {
            var authorId = Guid.NewGuid();

            _repo.Setup(r => r.AuthorExists(authorId)).Returns(true);
            _repo.Setup(r => r.GetCourse(authorId, It.IsAny<Guid>())).Returns((Course)null);

            var result = _sut.GetCourseForAuhtor(authorId, Guid.NewGuid());

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public void GetCourse_Existing_ReturnsDto()
        {
            var authorId = Guid.NewGuid();
            var course = NewCourse(authorId);

            _repo.Setup(r => r.AuthorExists(authorId)).Returns(true);
            _repo.Setup(r => r.GetCourse(authorId, course.Id)).Returns(course);

            var result = _sut.GetCourseForAuhtor(authorId, course.Id);

            var ok = Assert.IsType<OkObjectResult>(result.Result);

            Assert.Equal(course.Title, Assert.IsType<CoursDto>(ok.Value).Title);
        }

        [Fact]
        public void CreateCourse_Valid_AddsAndSaves()
        {
            var authorId = Guid.NewGuid();

            _repo.Setup(r => r.AuthorExists(authorId)).Returns(true);

            var dto = _fixture.Create<CreateCourseDto>();

            var result = _sut.CreateCourseForAthor(authorId, dto);

            Assert.IsType<CreatedAtRouteResult>(result.Result);
            _repo.Verify(r => r.AddCourse(authorId, It.IsAny<Course>()), Times.Once);
            _repo.Verify(r => r.Save(), Times.Once);
        }

        [Fact]
        public void CreateCourse_UnknownAuthor_ReturnsNotFound()
        {
            _repo.Setup(r => r.AuthorExists(It.IsAny<Guid>())).Returns(false);

            var result = _sut.CreateCourseForAthor(Guid.NewGuid(), _fixture.Create<CreateCourseDto>());

            Assert.IsType<NotFoundResult>(result.Result);
            _repo.Verify(r => r.Save(), Times.Never);
        }

        [Fact]
        public void UpdateCourse_Existing_ReturnsNoContent()
        {
            var authorId = Guid.NewGuid();
            var course = NewCourse(authorId);

            _repo.Setup(r => r.AuthorExists(authorId)).Returns(true);
            _repo.Setup(r => r.GetCourse(authorId, course.Id)).Returns(course);

            var dto = _fixture.Create<UpdateCourseDto>();

            var result = _sut.UpdateCourseForAthor(authorId, course.Id, dto);

            Assert.IsType<NoContentResult>(result);
            Assert.Equal(dto.Title, course.Title);
            _repo.Verify(r => r.UpdateCourse(course), Times.Once);
            _repo.Verify(r => r.Save(), Times.Once);
        }

        [Fact]
        public void UpdateCourse_Missing_Upserts()
        {
            var authorId = Guid.NewGuid();
            var courseId = Guid.NewGuid();

            _repo.Setup(r => r.AuthorExists(authorId)).Returns(true);
            _repo.Setup(r => r.GetCourse(authorId, courseId)).Returns((Course)null);

            var result = _sut.UpdateCourseForAthor(authorId, courseId, _fixture.Create<UpdateCourseDto>());

            Assert.IsType<CreatedAtRouteResult>(result);
            _repo.Verify(r => r.AddCourse(authorId, It.Is<Course>(c => c.Id == courseId)), Times.Once);
        }

        [Fact]
        public void UpdateCourse_UnknownAuthor_ReturnsNotFound()
        {
            _repo.Setup(r => r.AuthorExists(It.IsAny<Guid>())).Returns(false);

            var result = _sut.UpdateCourseForAthor(Guid.NewGuid(), Guid.NewGuid(), _fixture.Create<UpdateCourseDto>());

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void PatchCourse_UnknownAuthor_ReturnsNotFound()
        {
            _repo.Setup(r => r.AuthorExists(It.IsAny<Guid>())).Returns(false);

            var result = _sut.PartiallyUpdateCourseForAthor(Guid.NewGuid(), Guid.NewGuid(),
                new JsonPatchDocument<UpdateCourseDto>());

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void DeleteCourse_Existing_ReturnsNoContent()
        {
            var authorId = Guid.NewGuid();
            var course = NewCourse(authorId);

            _repo.Setup(r => r.AuthorExists(authorId)).Returns(true);
            _repo.Setup(r => r.GetCourse(authorId, course.Id)).Returns(course);

            var result = _sut.DeleteCourseForAthor(authorId, course.Id);

            Assert.IsType<NoContentResult>(result);
            _repo.Verify(r => r.DeleteCourse(course), Times.Once);
            _repo.Verify(r => r.Save(), Times.Once);
        }

        [Fact]
        public void DeleteCourse_Missing_ReturnsNotFound()
        {
            var authorId = Guid.NewGuid();

            _repo.Setup(r => r.AuthorExists(authorId)).Returns(true);
            _repo.Setup(r => r.GetCourse(authorId, It.IsAny<Guid>())).Returns((Course)null);

            var result = _sut.DeleteCourseForAthor(authorId, Guid.NewGuid());

            Assert.IsType<NotFoundResult>(result);
        }
    }
}
