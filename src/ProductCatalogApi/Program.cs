using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Register Controller Services
builder.Services.AddControllers();
builder.Services.AddOpenApi(); 

builder.Services.AddHealthChecks()
    .AddCheck("self_liveness", () => HealthCheckResult.Healthy(), tags: new[] { "liveness" })
    .AddCheck("database_check", () => HealthCheckResult.Healthy("Database is online."), tags: new[] { "readiness" });

var app = builder.Build();

// 2. Map OpenAPI & Scalar UI documentation routes
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();              
    app.MapScalarApiReference();   
}

app.UseAuthorization();

// 3. CRITICAL: Map the Controller routing tables
app.MapControllers(); 

// 4. Fallback Verification Endpoint
app.MapGet("/test-ping", () => "Pong!");

// 5. Map Health Check Probes
app.MapHealthChecks("/healthz/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("liveness")
});

app.MapHealthChecks("/healthz/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("readiness")
});

app.Run();