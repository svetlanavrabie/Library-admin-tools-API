using AutoFixture;
using CourseLibrary.API.DbContexts;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Services;
using LibraryAPI.ResourcesParameters;
using LibraryAPI.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Linq;
using Xunit;

namespace LibraryAPI.Tests
{
    public class CourseLibraryRepositoryTests
    {
        private readonly Fixture _fixture = new();

        private static CourseLibraryContext NewContext() => TestDatabase.CreateContext();

        private static CourseLibraryRepository NewRepository(CourseLibraryContext context)
        {
            var mapping = new Mock<IPropertyMappingService>();
            return new CourseLibraryRepository(context, mapping.Object);
        }

        private Author NewAuthor() =>
            _fixture.Build<Author>()
                .With(a => a.FirstName, "First")
                .With(a => a.LastName, "Last")
                .With(a => a.MainCategory, "Cat")
                .With(a => a.DateOfDeath, (DateTimeOffset?)null)
                .Without(a => a.Courses)
                .Create();

        [Fact]
        public void Ctor_NullContext_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new CourseLibraryRepository(null, Mock.Of<IPropertyMappingService>()));
        }

        [Fact]
        public void AddAuthor_Null_Throws()
        {
            using var context = NewContext();

            Assert.Throws<ArgumentNullException>(() => NewRepository(context).AddAuthor(null));
        }

        [Fact]
        public void AddAuthor_AssignsIdAndPersists()
        {
            using var context = NewContext();

            var repo = NewRepository(context);
            var author = NewAuthor();

            repo.AddAuthor(author);
            repo.Save();

            Assert.NotEqual(Guid.Empty, author.Id);
            Assert.True(repo.AuthorExists(author.Id));
        }

        [Fact]
        public void GetAuthor_EmptyGuid_Throws()
        {
            using var context = NewContext();

            Assert.Throws<ArgumentNullException>(() => NewRepository(context).GetAuthor(Guid.Empty));
        }

        [Fact]
        public void GetAuthors_ByIds_ReturnsMatchingOnly()
        {
            using var context = NewContext();

            var repo = NewRepository(context);
            var a = NewAuthor();
            var b = NewAuthor();
            repo.AddAuthor(a);
            repo.AddAuthor(b);
            repo.Save();

            var result = repo.GetAuthors(new[] { a.Id });

            Assert.Single(result);
        }

        [Fact]
        public void DeleteAuthor_RemovesAuthor()
        {
            using var context = NewContext();

            var repo = NewRepository(context);
            var author = NewAuthor();
            repo.AddAuthor(author);
            repo.Save();

            repo.DeleteAuthor(author);
            repo.Save();

            Assert.False(repo.AuthorExists(author.Id));
        }

        [Fact]
        public void CourseLifecycle_AddGetDelete()
        {
            using var context = NewContext();

            var repo = NewRepository(context);
            var author = NewAuthor();
            repo.AddAuthor(author);
            repo.Save();
            var course = _fixture.Build<Course>().Without(c => c.Author).Create();

            repo.AddCourse(author.Id, course);
            repo.Save();

            Assert.Single(repo.GetCourses(author.Id));
            Assert.NotNull(repo.GetCourse(author.Id, course.Id));

            repo.DeleteCourse(course);
            repo.Save();

            Assert.Empty(repo.GetCourses(author.Id));
        }

        [Fact]
        public void AddCourse_NullCourse_Throws()
        {
            using var context = NewContext();

            Assert.Throws<ArgumentNullException>(() => NewRepository(context).AddCourse(Guid.NewGuid(), null));
        }

        [Fact]
        public void GeneratedData_AuthorsAndCoursesAreQueryable()
        {
            using var context = NewContext();

            var seeded = TestDatabase.Authors;
            var repo = NewRepository(context);
            var first = seeded[0];

            Assert.True(repo.AuthorExists(first.Id));
            Assert.Equal(TestDatabase.CoursesPerAuthor, repo.GetCourses(first.Id).Count());
            Assert.Equal(seeded.Count, repo.GetAuthors(seeded.Select(a => a.Id)).Count());
        }

        [Fact]
        public void GetAuthors_NoParameters_ReturnsSeedData()
        {
            using var context = NewContext();

            Assert.NotEmpty(NewRepository(context).GetAuthors());
        }
    }
}
