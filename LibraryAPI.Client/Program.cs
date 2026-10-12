using LibraryAPI.Client.Components;
using LibraryAPI.Client.Configuration;
using LibraryAPI.Client.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var apiSettings = builder.Configuration
    .GetSection(ApiSettings.SectionName)
    .Get<ApiSettings>()
    ?? throw new InvalidOperationException($"Missing '{ApiSettings.SectionName}' configuration section.");

if (!Uri.TryCreate(apiSettings.BaseUrl, UriKind.Absolute, out var apiBaseUri))
{
    throw new InvalidOperationException($"'{ApiSettings.SectionName}:BaseUrl' must be an absolute URL.");
}

builder.Services.AddHttpClient<IAuthorService, AuthorService>(client => client.BaseAddress = apiBaseUri);
builder.Services.AddHttpClient<ICourseService, CourseService>(client => client.BaseAddress = apiBaseUri);
builder.Services.AddScoped<INotificationService, NotificationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
