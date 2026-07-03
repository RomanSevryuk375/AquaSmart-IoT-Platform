using Contracts.Middlewares;
using Control.API.Extensions;
using Control.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

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

app.Run();
