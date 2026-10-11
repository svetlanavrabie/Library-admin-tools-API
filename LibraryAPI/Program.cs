using CourseLibrary.API.DbContexts;
using CourseLibrary.API.Services;
using LibraryAPI.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpCacheHeaders(
    expirationModelOptions =>
    {
        expirationModelOptions.MaxAge = 60;
        expirationModelOptions.CacheLocation = Marvin.Cache.Headers.CacheLocation.Private;
    },
    validationModelOptions =>
    {
        validationModelOptions.MustRevalidate = true;
    });

builder.Services.AddResponseCaching();

builder.Services.AddControllers(setupAction =>
{
    setupAction.ReturnHttpNotAcceptable = true;
    setupAction.CacheProfiles.Add("240SecondsCacheProfile",
        new CacheProfile() { Duration = 240 });
})
.AddNewtonsoftJson(setupAction =>
{
    setupAction.SerializerSettings.ContractResolver =
        new CamelCasePropertyNamesContractResolver();
})
.AddXmlDataContractSerializerFormatters()
.ConfigureApiBehaviorOptions(setupAction =>
{
    setupAction.InvalidModelStateResponseFactory = context =>
    {
        // create a problem details object
        var problemDetailsFactory = context.HttpContext.RequestServices
            .GetRequiredService<ProblemDetailsFactory>();
        var problemDetails = problemDetailsFactory.CreateValidationProblemDetails(
            context.HttpContext,
            context.ModelState);

        // add additional info not added by default
        problemDetails.Detail = "See the errors field for details.";
        problemDetails.Instance = context.HttpContext.Request.Path;

        // find out which status code to use
        var actionExecutingContext = context as ActionExecutingContext;

        // if there are modelstate errors & all keys were correctly
        // found/parsed we're dealing with validation errors
        if ((context.ModelState.ErrorCount > 0) &&
            (actionExecutingContext?.ActionArguments.Count == context.ActionDescriptor.Parameters.Count))
        {
            problemDetails.Type = "https://courselibrary.com/modelvalidationproblem";
            problemDetails.Status = StatusCodes.Status422UnprocessableEntity;
            problemDetails.Title = "One or more validation errors occurred.";

            return new UnprocessableEntityObjectResult(problemDetails)
            {
                ContentTypes = { "application/problem+json" }
            };
        }

        // if one of the keys wasn't correctly found / couldn't be parsed
        // we're dealing with null/unparsable input
        problemDetails.Status = StatusCodes.Status400BadRequest;
        problemDetails.Title = "One or more errors on input occurred.";
        return new BadRequestObjectResult(problemDetails)
        {
            ContentTypes = { "application/problem+json" }
        };
    };
});

builder.Services.Configure<MvcOptions>(config =>
{
    var newtonsoftJsonOutputFormatter = config.OutputFormatters
        .OfType<NewtonsoftJsonOutputFormatter>()?.FirstOrDefault();

    if (newtonsoftJsonOutputFormatter != null)
    {
        newtonsoftJsonOutputFormatter.SupportedMediaTypes.Add("application/vdn.marvin.hateoas+json");
    }
});

builder.Services.AddTransient<IPropertyMappingService, PropertyMappingService>();
builder.Services.AddTransient<IPropertyCheckerService, PropertyCheckerService>();

builder.Services.AddAutoMapper(cfg => { }, AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddScoped<ICourseLibraryRepository, CourseLibraryRepository>();

builder.Services.AddDbContext<CourseLibraryContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("CourseLibraryDB"));
});

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Library API",
        Version = "v1",
        Description = "Library admin tools REST API built with ASP.NET Core on .NET 10. " +
                      "Manage authors and their courses, with paging, sorting, filtering, " +
                      "data shaping and HATEOAS links."
    });

    // Some actions share the same verb/route and are selected by content type
    // (e.g. POST api/authors). Swagger needs a unique verb/route pair, so the
    // first matching action is documented.
    options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
});

var app = builder.Build();

// ---------------------------------------------------------------------
// Database migration (best practice: at startup, using a service scope)
// ---------------------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<CourseLibraryContext>();
        // for demo purposes, delete the database & migrate on startup so
        // we can start with a clean slate
        context.Database.EnsureDeleted();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

// ---------------------------------------------------------------------
// HTTP request pipeline
// ---------------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Library API v1");
        options.RoutePrefix = "swagger";
    });
}
else
{
    app.UseExceptionHandler(appBuilder =>
    {
        appBuilder.Run(async context =>
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("An unexpected fault happened. Try again later.");
        });
    });
}

//app.UseResponseCaching();

app.UseHttpCacheHeaders();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();
