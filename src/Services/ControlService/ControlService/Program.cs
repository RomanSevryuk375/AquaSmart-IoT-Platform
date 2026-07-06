using Contracts.Constants;
using Contracts.Middlewares;
using Control.API.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddConfiguration(builder.Configuration);

WebApplication app = builder.Build();

app.UseGlobalExceptionHandler();
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks(ApiConstants.HealthRoute);
app.MapControllers();

await app.RunAsync();

#pragma warning disable S1118 
public partial class Program { }
#pragma warning restore S1118 
