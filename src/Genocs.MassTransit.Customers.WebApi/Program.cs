using Genocs.MassTransit.Customers.WebApi;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using Serilog.Events;


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc
    .WriteTo.Console());


// ***********************************************
// Azure Application Insight configuration - START
builder.Services.AddApplicationInsightsTelemetry();

var connectionString = builder.Configuration.GetConnectionString(Constants.ApplicationInsightsConnectionString);
TelemetryAndLogging.Initialize(connectionString);
// Azure Application Insight configuration - END
// ***********************************************


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<HealthCheckPublisherOptions>(options =>
{
    options.Delay = TimeSpan.FromSeconds(2);
    options.Predicate = check => check.Tags.Contains("ready");

});

//builder.Services.Configure<JsonOptions>(options =>
//{
//    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
//    options.JsonSerializerOptions.PropertyNamingPolicy = null;
//});



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

await TelemetryAndLogging.FlushAndCloseAsync();

Log.CloseAndFlush();
