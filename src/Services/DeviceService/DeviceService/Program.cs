using Contracts.Middlewares;
using Device.API.Extensions;
using Device.Application.Extesions;
using Device.Infrastructure.Extensions;
using Device.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration)
                .AddApplication(builder.Configuration)
                .AddApi(builder.Configuration);

WebApplication app = builder.Build();

app.UseGlobalExceptionHandler();

using (IServiceScope scope = app.Services.CreateScope())
{
    SystemDbContext context = scope.ServiceProvider.GetRequiredService<SystemDbContext>();
    await context.Database.MigrateAsync();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

await app.RunAsync();
