using CourseLibrary.API.Entities;
using LibraryAPI.Dtos;
using LibraryAPI.Helpers;
using LibraryAPI.Services;
using System;
using System.Linq;
using Xunit;

namespace LibraryAPI.Tests
{
    public class HelpersAndServicesTests
    {
        [Fact]
        public void PagedList_Create_ComputesPagingInfo()
        {
            using var context = TestDatabase.CreateContext();

            var page = PagedList<Author>.Create(context.Authors.OrderBy(a => a.LastName), 2, 10);

            Assert.Equal(10, page.Count);
            Assert.Equal(2, page.CurrentPage);
            Assert.Equal(10, page.PageSize);
            Assert.True(page.TotalCount >= TestDatabase.AuthorCount);
            Assert.True(page.HasPrevious);
            Assert.Equal(page.CurrentPage < page.TotalPages, page.HasNext);
        }

        [Fact]
        public void PagedList_FirstPage_HasNoPrevious()
        {
            using var context = TestDatabase.CreateContext();

            var page = PagedList<Author>.Create(context.Authors.OrderBy(a => a.LastName), 1, 5);

            Assert.False(page.HasPrevious);
            Assert.True(page.HasNext);
        }

        [Fact]
        public void GetCurrentAge_Alive_UsesToday()
        {
            var birth = DateTimeOffset.UtcNow.AddYears(-30).AddDays(-1);

            Assert.Equal(30, birth.GetCurrentAge(null));
        }

        [Fact]
        public void GetCurrentAge_Deceased_UsesDateOfDeath()
        {
            var birth = new DateTimeOffset(1900, 6, 1, 0, 0, 0, TimeSpan.Zero);
            var death = new DateTimeOffset(1950, 6, 2, 0, 0, 0, TimeSpan.Zero);

            Assert.Equal(50, birth.GetCurrentAge(death));
        }

        [Fact]
        public void GetCurrentAge_BirthdayNotReached_SubtractsOneYear()
        {
            var birth = new DateTimeOffset(1900, 12, 31, 0, 0, 0, TimeSpan.Zero);
            var death = new DateTimeOffset(1950, 6, 1, 0, 0, 0, TimeSpan.Zero);

            Assert.Equal(49, birth.GetCurrentAge(death));
        }

        [Theory]
        [InlineData(null, true)]
        [InlineData("", true)]
        [InlineData("Id", true)]
        [InlineData("name", true)]
        [InlineData("name desc, age", true)]
        [InlineData("Unknown", false)]
        public void PropertyMappingService_ValidMappingExistsFor_Works(string fields, bool expected)
        {
            var service = new PropertyMappingService();

            Assert.Equal(expected, service.ValidMappingExistsFor<AuthorDto, Author>(fields));
        }

        [Fact]
        public void PropertyMappingService_UnknownMapping_Throws()
        {
            var service = new PropertyMappingService();

            Assert.Throws<Exception>(() => service.GetPropertyMapping<AuthorDto, Course>());
        }

        [Theory]
        [InlineData(null, true)]
        [InlineData("Id", true)]
        [InlineData("id, name", true)]
        [InlineData("Id,Unknown", false)]
        public void PropertyCheckerService_TypeHasProperties_Works(string fields, bool expected)
        {
            var service = new PropertyCheckerService();

            Assert.Equal(expected, service.TypeHasProperties<AuthorDto>(fields));
        }
    }
}
