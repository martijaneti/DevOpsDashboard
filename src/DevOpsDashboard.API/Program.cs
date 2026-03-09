using DevOpsDashboard.API.Hubs;
using DevOpsDashboard.API.Workers;
using DevOpsDashboard.Core.Interfaces;
using DevOpsDashboard.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("DashboardUI", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Required for SignalR
    });
});

// Config
builder.Services.Configure<AzureDevOpsOptions>(
    builder.Configuration.GetSection(AzureDevOpsOptions.Section));
builder.Services.Configure<AzureMonitorOptions>(
    builder.Configuration.GetSection(AzureMonitorOptions.Section));

// Caching
builder.Services.AddMemoryCache();

// Our services
builder.Services.AddSingleton<IAzureDevOpsService, AzureDevOpsService>();
builder.Services.AddSingleton<IAzureMonitorService, MockAzureMonitorService>();
builder.Services.AddSingleton<IHardwareMonitorService, HardwareMonitorService>();
builder.Services.AddSignalR();
builder.Services.AddHostedService<DashboardWorker>();

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

app.UseCors("DashboardUI");
app.MapHub<DashboardHub>("/hubs/dashboard");
app.UseHttpsRedirection();
app.MapControllers();
app.Run();