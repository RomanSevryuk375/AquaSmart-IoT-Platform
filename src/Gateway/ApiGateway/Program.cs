using ApiGateway;
using Contracts.Authorization;
using Contracts.Middlewares;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCustomSwagger();

builder.Services.AddApiAuthentication(builder.Configuration);
builder.Services.AddAquaAuthorizationPolicies();
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseGlobalExceptionHandler();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger-docs/telemetry/swagger/v1/swagger.json", "Telemetry API");
    options.SwaggerEndpoint("/swagger-docs/device/swagger/v1/swagger.json", "Device API");
    options.SwaggerEndpoint("/swagger-docs/control/swagger/v1/swagger.json", "Control API");
    options.SwaggerEndpoint("/swagger-docs/identity/swagger/v1/swagger.json", "Identity API");
    options.SwaggerEndpoint("/swagger-docs/notification/swagger/v1/swagger.json", "Notification API");
});

app.UseAuthentication(); 
app.UseAuthorization();  

app.MapReverseProxy();

app.Run();
