using CourseLibrary.API.DbContexts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using CourseLibrary.API.Entities;
using LibraryAPI.Tests;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LibraryAPI.Integration.Tests
{
    public class LibraryApiFactory : WebApplicationFactory<Program>
    {
        public IReadOnlyList<Author> SeededAuthors => TestDatabase.Authors;

        protected override IHost CreateHost(IHostBuilder builder)
        {
            TestDatabase.EnsureSeeded();
            return base.CreateHost(builder);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                var toRemove = services
                    .Where(d => d.ServiceType == typeof(DbContextOptions<CourseLibraryContext>)
                             || d.ServiceType == typeof(DbContextOptions)
                             || d.ServiceType.IsGenericType
                                && d.ServiceType.GetGenericArguments().Contains(typeof(CourseLibraryContext)))
                    .ToList();

                foreach (var descriptor in toRemove)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<CourseLibraryContext>(options =>
                    options.UseInMemoryDatabase(TestDatabase.Name, TestDatabase.Root));
            });
        }
    }
}
