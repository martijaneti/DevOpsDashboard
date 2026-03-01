using DevOpsDashboard.Core.Interfaces;
using DevOpsDashboard.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Config
builder.Services.Configure<AzureDevOpsOptions>(
    builder.Configuration.GetSection(AzureDevOpsOptions.Section));
builder.Services.Configure<AzureMonitorOptions>(
    builder.Configuration.GetSection(AzureMonitorOptions.Section));

// Caching
builder.Services.AddMemoryCache();

// Our services
builder.Services.AddSingleton<IAzureDevOpsService, MockAzureDevOpsService>();
builder.Services.AddSingleton<IAzureMonitorService, MockAzureMonitorService>();

// API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();