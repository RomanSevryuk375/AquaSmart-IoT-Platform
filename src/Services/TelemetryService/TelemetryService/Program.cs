using Contracts.Constants;
using Contracts.Middlewares;
using Telemetry.API.Extensions;
using Telemetry.Infrastructure.SignalR;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddIConfiguration(builder.Configuration);

WebApplication app = builder.Build();

app.UseGlobalExceptionHandler();

app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks(ApiConstants.HealthRoute);
app.MapControllers();

app.MapHub<TelemetryHub>(SignalRRoutes.RawTelemetry);

await app.RunAsync();

public partial class Program { }

