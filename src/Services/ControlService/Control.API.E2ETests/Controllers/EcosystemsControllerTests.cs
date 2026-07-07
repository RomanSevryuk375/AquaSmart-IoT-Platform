using Contracts.Enums;
using Control.Application.DTOs.Ecosystem;
using Control.Application.Features.Ecosystems.Queries;
using Control.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Control.API.E2ETests.Controllers;

public class EcosystemsControllerTests(E2ETestWebAppFactory factory) : BaseE2ETest(factory)
{
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetAll_WithoutAuthorizationHeader_Returns401Unauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        HttpResponseMessage response = await Client.GetAsync(
            ApiConstants.Routes.Ecosystems);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetAll_WithValidRequest_Returns200OKAndCorrectData()
    {
        // Arrange
        Ecosystem ecosystem = new EcosystemBuilder()
            .WithUserId(ControlTestConstants.UserId)
            .Build();

        DbContext.Aquariums.Add(ecosystem);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await Client.GetAsync(
            ApiConstants.Routes.Ecosystems);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        IReadOnlyList<EcosystemDto>? content = await response.Content
            .ReadFromJsonAsync<IReadOnlyList<EcosystemDto>>();
        content.Should().NotBeNull();
        content.Should().ContainSingle(e => e.Id == ecosystem.Id);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetById_WithoutAuthorizationHeader_Returns401Unauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        HttpResponseMessage response = await Client.GetAsync(
            $"{ApiConstants.Routes.Ecosystems}/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetById_WhenExists_Returns200OK()
    {
        // Arrange
        Ecosystem ecosystem = new EcosystemBuilder()
            .WithUserId(ControlTestConstants.UserId)
            .Build();

        DbContext.Aquariums.Add(ecosystem);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await Client.GetAsync(
            $"{ApiConstants.Routes.Ecosystems}/{ecosystem.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        EcosystemDto? content = await response.Content.ReadFromJsonAsync<EcosystemDto>();
        content.Should().NotBeNull();
        content!.Id.Should().Be(ecosystem.Id);
        content.Name.Should().Be(ecosystem.Name.Value);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetById_WhenDoesNotExist_Returns404NotFound()
    {
        // Act
        HttpResponseMessage response = await Client.GetAsync(
            $"{ApiConstants.Routes.Ecosystems}/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetById_WhenBelongsToAnotherUser_Returns409Conflict()
    {
        // Arrange
        Ecosystem ecosystem = new EcosystemBuilder()
            .WithUserId(Guid.NewGuid())
            .Build();

        DbContext.Aquariums.Add(ecosystem);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await Client.GetAsync(
            $"{ApiConstants.Routes.Ecosystems}/{ecosystem.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Create_WithoutAuthorizationHeader_Returns401Unauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;
        var request = new EcosystemRequestDto
        {
            Type = EcosystemType.Aquarium,
            Name = "New Aqua",
            Volume = 50.0,
            ControllerId = Guid.NewGuid()
        };

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            ApiConstants.Routes.Ecosystems, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Create_WithValidData_Returns201CreatedAndLocationHeader()
    {
        // Arrange
        var request = new EcosystemRequestDto
        {
            Type = EcosystemType.Aquarium,
            Name = "New Aquarium Test",
            Volume = 120.0,
            ControllerId = Guid.NewGuid()
        };

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            ApiConstants.Routes.Ecosystems, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain(ApiConstants.Routes.Ecosystems);

        Guid createdId = await response.Content.ReadFromJsonAsync<Guid>();
        createdId.Should().NotBeEmpty();

        Ecosystem? dbEcosystem = await DbContext.Aquariums
            .AsNoTracking().FirstOrDefaultAsync(e => e.Id == createdId);
        dbEcosystem.Should().NotBeNull();
        dbEcosystem!.Name.Value.Should().Be(request.Name);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Create_WithInvalidData_Returns400BadRequest()
    {
        // Arrange
        var request = new EcosystemRequestDto
        {
            Type = EcosystemType.Aquarium,
            Name = "",
            Volume = -10.0,
            ControllerId = Guid.Empty
        };

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            ApiConstants.Routes.Ecosystems, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Update_WithoutAuthorizationHeader_Returns401Unauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;
        var request = new EcosystemUpdateRequestDto
        {
            Name = "Updated Aqua",
            Volume = 60.0
        };

        // Act
        HttpResponseMessage response = await Client.PutAsJsonAsync(
            $"{ApiConstants.Routes.Ecosystems}/{Guid.NewGuid()}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Update_WithValidData_Returns204NoContent()
    {
        // Arrange
        Ecosystem ecosystem = new EcosystemBuilder()
            .WithUserId(ControlTestConstants.UserId)
            .Build();

        DbContext.Aquariums.Add(ecosystem);
        await DbContext.SaveChangesAsync();

        var request = new EcosystemUpdateRequestDto
        {
            Name = "Fully Updated Aqua",
            Volume = 150.0
        };

        // Act
        HttpResponseMessage response = await Client.PutAsJsonAsync(
            $"{ApiConstants.Routes.Ecosystems}/{ecosystem.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        Ecosystem? dbEcosystem = await DbContext.Aquariums
            .AsNoTracking().FirstOrDefaultAsync(e => e.Id == ecosystem.Id);
        dbEcosystem!.Name.Value.Should().Be(request.Name);
        dbEcosystem.Volume!.Value.Should().Be(request.Volume);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Update_WithInvalidData_Returns400BadRequest()
    {
        // Arrange
        Ecosystem ecosystem = new EcosystemBuilder()
            .WithUserId(ControlTestConstants.UserId)
            .Build();

        DbContext.Aquariums.Add(ecosystem);
        await DbContext.SaveChangesAsync();

        var request = new EcosystemUpdateRequestDto
        {
            Name = "", // Invalid
            Volume = -5.0 // Invalid
        };

        // Act
        HttpResponseMessage response = await Client.PutAsJsonAsync(
            $"{ApiConstants.Routes.Ecosystems}/{ecosystem.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Update_WhenDoesNotExist_Returns404NotFound()
    {
        // Arrange
        var request = new EcosystemUpdateRequestDto
        {
            Name = "Updated Non-existent",
            Volume = 100.0
        };

        // Act
        HttpResponseMessage response = await Client.PutAsJsonAsync(
            $"{ApiConstants.Routes.Ecosystems}/{Guid.NewGuid()}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Update_WhenBelongsToAnotherUser_Returns409Conflict()
    {
        // Arrange
        Ecosystem ecosystem = new EcosystemBuilder()
            .WithUserId(Guid.NewGuid())
            .Build();

        DbContext.Aquariums.Add(ecosystem);
        await DbContext.SaveChangesAsync();

        var request = new EcosystemUpdateRequestDto
        {
            Name = "Hacked Aqua",
            Volume = 200.0
        };

        // Act
        HttpResponseMessage response = await Client.PutAsJsonAsync(
            $"{ApiConstants.Routes.Ecosystems}/{ecosystem.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Delete_WithoutAuthorizationHeader_Returns401Unauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        HttpResponseMessage response = await Client.DeleteAsync(
            $"{ApiConstants.Routes.Ecosystems}/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Delete_WhenExists_Returns204NoContent()
    {
        // Arrange
        Ecosystem ecosystem = new EcosystemBuilder()
            .WithUserId(ControlTestConstants.UserId)
            .Build();

        DbContext.Aquariums.Add(ecosystem);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await Client.DeleteAsync(
            $"{ApiConstants.Routes.Ecosystems}/{ecosystem.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        Ecosystem? dbEcosystem = await DbContext.Aquariums
            .AsNoTracking().FirstOrDefaultAsync(e => e.Id == ecosystem.Id);
        dbEcosystem.Should().BeNull();
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Delete_WhenDoesNotExist_Returns404NotFound()
    {
        // Act
        HttpResponseMessage response = await Client.DeleteAsync(
            $"{ApiConstants.Routes.Ecosystems}/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Delete_WhenBelongsToAnotherUser_Returns409Conflict()
    {
        // Arrange
        Ecosystem ecosystem = new EcosystemBuilder()
            .WithUserId(Guid.NewGuid())
            .Build();

        DbContext.Aquariums.Add(ecosystem);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await Client.DeleteAsync(
            $"{ApiConstants.Routes.Ecosystems}/{ecosystem.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
