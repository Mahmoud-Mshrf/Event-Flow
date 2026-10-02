using EventFlow.Api.OpenApi.Transformers;
using EventFlow.Application;
using EventFlow.Infrastructure;
using EventFlow.Infrastructure.Hubs;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration,builder.Environment);

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddSingleton<BearerSecuritySchemeTransformer>();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
    options.AddOperationTransformer<BearerSecuritySchemeTransformer>();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "EventFlow API";
        options.Theme = ScalarTheme.DeepSpace;
        options.DefaultHttpClient = new(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

app.UseAuthentication();
app.UseAuthorization();
app.MapHub<EventDashboardHub>("/hubs/event-dashboard");
app.MapControllers();

app.Run();