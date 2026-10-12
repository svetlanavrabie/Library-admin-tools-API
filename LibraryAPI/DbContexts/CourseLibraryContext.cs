using CourseLibrary.API.Entities;
using Microsoft.EntityFrameworkCore;
using System;

namespace CourseLibrary.API.DbContexts
{
    public class CourseLibraryContext : DbContext
    {
        public CourseLibraryContext(DbContextOptions<CourseLibraryContext> options)
           : base(options)
        {
        }

        public DbSet<Author> Authors { get; set; }
        public DbSet<Course> Courses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // seed the database with dummy data
            modelBuilder.Entity<Author>().HasData(
                new Author()
                {
                    Id = Guid.Parse("d28888e9-2ba9-473a-a40f-e38cb54f9b35"),
                    FirstName = "Berry",
                    LastName = "Griffin Beak Eldritch",
                    DateOfBirth = new DateTime(1960, 7, 23),
                    MainCategory = "Ships"
                },
                new Author()
                {
                    Id = Guid.Parse("da2fd609-d754-4feb-8acd-c4f9ff13ba96"),
                    FirstName = "Nancy",
                    LastName = "Swashbuckler Rye",
                    DateOfBirth = new DateTime(1963, 5, 21),
                    MainCategory = "Rum"
                },
                new Author()
                {
                    Id = Guid.Parse("2902b665-1190-4c70-9915-b9c2d7680450"),
                    FirstName = "Eli",
                    LastName = "Ivory Bones Sweet",
                    DateOfBirth = new DateTime(1968, 12, 16),
                    MainCategory = "Singing"
                },
                new Author()
                {
                    Id = Guid.Parse("102b566b-ba1f-404c-b2df-e2cde39ade09"),
                    FirstName = "Arnold",
                    LastName = "The Unseen Stafford",
                    DateOfBirth = new DateTime(1971, 3, 6),
                    MainCategory = "Singing"
                },
                new Author()
                {
                    Id = Guid.Parse("5b3621c0-7b12-4e80-9c8b-3398cba7ee05"),
                    FirstName = "Seabury",
                    LastName = "Toxic Reyson",
                    DateOfBirth = new DateTime(1974, 11, 23),
                    MainCategory = "Maps"
                },
                new Author()
                {
                    Id = Guid.Parse("2aadd2df-7caf-45ab-9355-7f6332985a87"),
                    FirstName = "Rutherford",
                    LastName = "Fearless Cloven",
                    DateOfBirth = new DateTime(1978, 4, 5),
                    MainCategory = "General debauchery"
                },
                new Author()
                {
                    Id = Guid.Parse("2ee49fe3-edf2-4f91-8409-3eb25ce6ca51"),
                    FirstName = "Atherton",
                    LastName = "Crow Ridley",
                    DateOfBirth = new DateTime(1981, 10, 11),
                    MainCategory = "Rum"
                },
                new Author()
                {
                    Id = Guid.Parse("a1f4c3d2-6b7e-4c8a-9d10-1e2f3a4b5c6d"),
                    FirstName = "Josephine",
                    LastName = "Silver Tongue Marlowe",
                    DateOfBirth = new DateTime(1984, 2, 14),
                    MainCategory = "Navigation"
                },
                new Author()
                {
                    Id = Guid.Parse("b2e5d4c3-7c8f-4d9b-8e21-2f3a4b5c6d7e"),
                    FirstName = "Bartholomew",
                    LastName = "Iron Hook Dunmore",
                    DateOfBirth = new DateTime(1987, 9, 2),
                    MainCategory = "Treasure"
                }
                );

            modelBuilder.Entity<Course>().HasData(
               new Course
               {
                   Id = Guid.Parse("5b1c2b4d-48c7-402a-80c3-cc796ad49c6b"),
                   AuthorId = Guid.Parse("d28888e9-2ba9-473a-a40f-e38cb54f9b35"),
                   Title = "Commandeering a Ship Without Getting Caught",
                   Description = "Commandeering a ship in rough waters isn't easy.  Commandeering it without getting caught is even harder.  In this course you'll learn how to sail away and avoid those pesky musketeers."
               },
               new Course
               {
                   Id = Guid.Parse("d8663e5e-7494-4f81-8739-6e0de1bea7ee"),
                   AuthorId = Guid.Parse("d28888e9-2ba9-473a-a40f-e38cb54f9b35"),
                   Title = "Overthrowing Mutiny",
                   Description = "In this course, the author provides tips to avoid, or, if needed, overthrow pirate mutiny."
               },
               new Course
               {
                   Id = Guid.Parse("d173e20d-159e-4127-9ce9-b0ac2564ad97"),
                   AuthorId = Guid.Parse("da2fd609-d754-4feb-8acd-c4f9ff13ba96"),
                   Title = "Avoiding Brawls While Drinking as Much Rum as You Desire",
                   Description = "Every good pirate loves rum, but it also has a tendency to get you into trouble.  In this course you'll learn how to avoid that.  This new exclusive edition includes an additional chapter on how to run fast without falling while drunk."
               },
               new Course
               {
                   Id = Guid.Parse("40ff5488-fdab-45b5-bc3a-14302d59869a"),
                   AuthorId = Guid.Parse("2902b665-1190-4c70-9915-b9c2d7680450"),
                   Title = "Singalong Pirate Hits",
                   Description = "In this course you'll learn how to sing all-time favourite pirate songs without sounding like you actually know the words or how to hold a note."
               },
               new Course
               {
                   Id = Guid.Parse("c3f6e5d4-8d90-4eac-9f32-3a4b5c6d7e8f"),
                   AuthorId = Guid.Parse("a1f4c3d2-6b7e-4c8a-9d10-1e2f3a4b5c6d"),
                   Title = "Reading the Stars Without a Compass",
                   Description = "Learn to navigate open seas using only the night sky."
               },
               new Course
               {
                   Id = Guid.Parse("d4a7f6e5-9ea1-4fbd-8a43-4b5c6d7e8f90"),
                   AuthorId = Guid.Parse("b2e5d4c3-7c8f-4d9b-8e21-2f3a4b5c6d7e"),
                   Title = "Burying Treasure Like a Professional",
                   Description = "Techniques for hiding your loot and drawing maps only you can read."
               },
               new Course
               {
                   Id = Guid.Parse("e5b8a7f6-afb2-40ce-9b54-5c6d7e8f9a01"),
                   AuthorId = Guid.Parse("5b3621c0-7b12-4e80-9c8b-3398cba7ee05"),
                   Title = "Mapping Uncharted Islands",
                   Description = "A practical guide to charting coastlines and hidden coves."
               },
               new Course
               {
                   Id = Guid.Parse("f6c9b8a7-b0c3-41df-8c65-6d7e8f9a0b12"),
                   AuthorId = Guid.Parse("2ee49fe3-edf2-4f91-8409-3eb25ce6ca51"),
                   Title = "Brewing Your Own Rum at Sea",
                   Description = "Make rum from ship supplies when the barrels run dry."
               }
               );

            base.OnModelCreating(modelBuilder);
        }
    }
}
