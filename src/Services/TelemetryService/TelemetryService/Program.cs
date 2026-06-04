using Contracts.Constants;
using Contracts.Middlewares;
using Microsoft.EntityFrameworkCore;
using Telemetry.API.Extensions;
using Telemetry.Infrastructure.Persistence;
using Telemetry.Infrastructure.SignalR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseGlobalExceptionHandler();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SystemDbContext>();
    context.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

app.MapHub<TelemetryHub>(SignalRRoutes.RawTelemetry);

app.Run();
