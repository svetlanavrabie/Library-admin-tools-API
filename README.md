# Library Admin Tools API

A REST Web API built with **ASP.NET Core on .NET 10** for managing library authors and the courses they have written.

## Summary

The API exposes endpoints to create, read, update, partially update and delete authors and their courses. It demonstrates a range of production-style REST features:

- **Authors and courses**: CRUD operations, with courses nested under authors (`api/authors/{authorId}/courses`).
- **Author collections**: create and fetch multiple authors in a single request.
- **Paging, filtering, searching and sorting** on author lists, with pagination metadata returned in headers.
- **Data shaping**: clients choose which fields to return.
- **HATEOAS**: links to related actions are included in responses, with a root document describing the API.
- **Content negotiation and vendor media types**: custom media types select between DTO shapes (for example, authors with or without date of death, or full vs. basic author data).
- **JSON Patch** support for partial updates.
- **HTTP caching** with cache headers and ETags (Marvin.Cache.Headers).
- **Validation** with RFC 7807 problem details responses (400 / 422).
- **Swagger / OpenAPI** UI available in the Development environment at `/swagger`.

## Tech stack

- .NET 10 / ASP.NET Core controllers
- Entity Framework Core 10 with SQL Server and migrations (including seed data)
- AutoMapper for entity/DTO mapping
- Newtonsoft.Json and Microsoft.AspNetCore.JsonPatch
- System.Linq.Dynamic.Core for dynamic sorting
- Swashbuckle.AspNetCore for API documentation

## Project structure

| Folder | Purpose |
| --- | --- |
| `Controllers` | Authors, author collections, courses and root endpoints |
| `Entities` | `Author` and `Course` entities |
| `Dtos` | Data transfer objects for requests and responses |
| `DbContexts` | `CourseLibraryContext` (EF Core) |
| `Services` | Repository, property mapping and property checker services |
| `Profiles` | AutoMapper profiles |
| `Helpers` | Paged list, extensions, model binders |
| `ActionConstrains` | Media type based routing constraint |
| `ResourcesParameters` | Query parameter models for filtering/paging |
| `ValidationTools` | Course validation helpers |
| `Migrations` | EF Core migrations |

## Getting started

1. Install the [.NET 10 SDK](https://dotnet.microsoft.com/download) and SQL Server (or LocalDB).
2. Set the `CourseLibraryDB` connection string in `LibraryAPI/appsettings.json`.
3. Run the API:

   ```bash
   dotnet run --project LibraryAPI
   ```

4. Open `/swagger` in a browser to explore the endpoints.

> **Note:** For demo purposes, the database is deleted and re-created with migrations on every startup. Remove this behavior in `Program.cs` before using real data.
