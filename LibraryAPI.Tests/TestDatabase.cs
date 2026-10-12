using CourseLibrary.API.DbContexts;
using CourseLibrary.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections.Generic;

namespace LibraryAPI.Tests
{
    /// <summary>
    /// Single shared in-memory database, filled once with generated data,
    /// used by every test (unit and integration).
    /// </summary>
    public static class TestDatabase
    {
        public const string Name = "LibraryAPI-Shared-TestDb";
        public const int AuthorCount = 30;
        public const int CoursesPerAuthor = 5;

        public static readonly InMemoryDatabaseRoot Root = new();

        private static readonly object SyncLock = new();
        private static List<Author> _authors;

        public static DbContextOptions<CourseLibraryContext> Options =>
            new DbContextOptionsBuilder<CourseLibraryContext>()
                .UseInMemoryDatabase(Name, Root)
                .Options;

        public static IReadOnlyList<Author> Authors
        {
            get
            {
                EnsureSeeded();
                return _authors;
            }
        }

        public static CourseLibraryContext CreateContext()
        {
            EnsureSeeded();
            return new CourseLibraryContext(Options);
        }

        public static void EnsureSeeded()
        {
            lock (SyncLock)
            {
                if (_authors != null)
                {
                    return;
                }

                using var context = new CourseLibraryContext(Options);
                context.Database.EnsureCreated();
                _authors = TestDataSeeder.Seed(context, AuthorCount, CoursesPerAuthor);
            }
        }
    }
}
