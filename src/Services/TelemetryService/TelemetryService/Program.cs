using Contracts.Constants;
using Contracts.Middlewares;
using Microsoft.EntityFrameworkCore;
using Telemetry.API.Extensions;
using Telemetry.Infrastructure.Persistence;
using Telemetry.Infrastructure.SignalR;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

WebApplication app = builder.Build();

app.UseGlobalExceptionHandler();

using (IServiceScope scope = app.Services.CreateScope())
{
    SystemDbContext context = scope.ServiceProvider.GetRequiredService<SystemDbContext>();
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
