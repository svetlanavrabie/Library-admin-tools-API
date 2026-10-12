using AutoFixture;
using CourseLibrary.API.DbContexts;
using CourseLibrary.API.Entities;
using System;
using System.Collections.Generic;

namespace LibraryAPI.Tests
{
    public static class TestDataSeeder
    {
        public static List<Author> Seed(CourseLibraryContext context, int authors = 5, int coursesPerAuthor = 3)
        {
            var fixture = new Fixture();
            var generated = new List<Author>();

            for (var i = 0; i < authors; i++)
            {
                var author = new Author
                {
                    Id = Guid.NewGuid(),
                    FirstName = $"First{i}",
                    LastName = $"Last{i}",
                    MainCategory = $"TestCategory{i % 2}",
                    DateOfBirth = new DateTimeOffset(1970 + i, 1, 1, 0, 0, 0, TimeSpan.Zero)
                };

                for (var j = 0; j < coursesPerAuthor; j++)
                {
                    author.Courses.Add(new Course
                    {
                        Id = Guid.NewGuid(),
                        AuthorId = author.Id,
                        Title = $"Title-{fixture.Create<Guid>():N}",
                        Description = $"Description-{fixture.Create<Guid>():N}"
                    });
                }

                generated.Add(author);
            }

            context.Authors.AddRange(generated);
            context.SaveChanges();
            return generated;
        }
    }
}
