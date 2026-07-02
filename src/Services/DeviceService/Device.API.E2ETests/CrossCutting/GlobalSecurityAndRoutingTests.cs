using Device.Application.Features.Controllers.Command.AddController;
using Device.Domain.Entities;

namespace Device.API.E2ETests.CrossCutting;

public class GlobalSecurityAndRoutingTests(E2ETestWebAppFactory factory) : BaseE2ETest(factory)
{
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Authentication_WhenNoTokenProvided_Returns401Unauthorized()
    {
        // Arrange
        HttpClient unauthenticatedClient = Factory.CreateClient();

        // Act
        HttpResponseMessage response = await unauthenticatedClient.GetAsync(ApiConstants.Routes.Controllers);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task ValidationPipeline_WithInvalidData_Returns400BadRequestAndErrorMessage()
    {
        var command = new AddControllerCommand
        {
            MacAddress = "XX-XX-XX-XX-XX-XX",
            Name = "",
            IsOnline = true
        };

        HttpResponseMessage response = await Client.PostAsJsonAsync(ApiConstants.Routes.Controllers, command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        string content = await response.Content.ReadAsStringAsync();

        content.Should().Contain("'Mac Address' is not in the correct format.");
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task TenantIsolation_WhenRequestingAnotherUsersDevice_Returns404NotFound()
    {
        // Arrange
        var anotherUserId = Guid.NewGuid();

        Controller hackerController = new ControllerBuilder()
            .WithId(Guid.NewGuid())
            .WithUserId(anotherUserId)
            .Build();

        DbContext.Controllers.Add(hackerController);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await Client.GetAsync($"{ApiConstants.Routes.Controllers}{hackerController.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
