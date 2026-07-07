using Control.Application.Features.VacationModes.Commands.CreateVacationMode;
using Control.Application.Features.VacationModes.Commands.UpdateVacationMode;
using Control.Application.Features.VacationModes.Queries.Shared;
using Control.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Control.API.E2ETests.Controllers;

public class VacationModesControllerTests(E2ETestWebAppFactory factory) : BaseE2ETest(factory)
{
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetAll_WithoutAuthorizationHeader_Returns401Unauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        HttpResponseMessage response = await Client.GetAsync(
            ApiConstants.Routes.VacationModes);

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

        VacationMode vacation = new VacationModeBuilder()
            .WithEcosystemId(ecosystem.Id)
            .Build();

        DbContext.Aquariums.Add(ecosystem);
        DbContext.Vacations.Add(vacation);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await Client.GetAsync(
            $"{ApiConstants.Routes.VacationModes}?ecosystemId={ecosystem.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        IReadOnlyList<VacationModeDto>? content = await response.Content
            .ReadFromJsonAsync<IReadOnlyList<VacationModeDto>>();
        content.Should().NotBeNull();
        content.Should().ContainSingle(v => v.Id == vacation.Id);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetAll_TenantIsolation_Returns409Conflict()
    {
        // Arrange
        Ecosystem ecosystem = new EcosystemBuilder()
            .WithUserId(Guid.NewGuid())
            .Build();

        VacationMode vacation = new VacationModeBuilder()
            .WithEcosystemId(ecosystem.Id)
            .Build();

        DbContext.Aquariums.Add(ecosystem);
        DbContext.Vacations.Add(vacation);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await Client.GetAsync(
            $"{ApiConstants.Routes.VacationModes}?ecosystemId={ecosystem.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetById_WithoutAuthorizationHeader_Returns401Unauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        HttpResponseMessage response = await Client.GetAsync(
            $"{ApiConstants.Routes.VacationModes}/{Guid.NewGuid()}");

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

        VacationMode vacation = new VacationModeBuilder()
            .WithEcosystemId(ecosystem.Id)
            .Build();

        DbContext.Aquariums.Add(ecosystem);
        DbContext.Vacations.Add(vacation);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await Client.GetAsync(
            $"{ApiConstants.Routes.VacationModes}/{vacation.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        VacationModeDto? content = await response.Content.ReadFromJsonAsync<VacationModeDto>();
        content.Should().NotBeNull();
        content!.Id.Should().Be(vacation.Id);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetById_WhenDoesNotExist_Returns404NotFound()
    {
        // Act
        HttpResponseMessage response = await Client.GetAsync(
            $"{ApiConstants.Routes.VacationModes}/{Guid.NewGuid()}");

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

        VacationMode vacation = new VacationModeBuilder()
            .WithEcosystemId(ecosystem.Id)
            .Build();

        DbContext.Aquariums.Add(ecosystem);
        DbContext.Vacations.Add(vacation);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await Client.GetAsync(
            $"{ApiConstants.Routes.VacationModes}/{vacation.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Create_WithoutAuthorizationHeader_Returns401Unauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;
        var command = new CreateVacationModeCommand
        {
            EcosystemId = Guid.NewGuid(),
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(5),
            IsActive = false,
            CalculatedFeed = 100.0
        };

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            ApiConstants.Routes.VacationModes, command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Create_WithValidData_Returns201CreatedAndLocationHeader()
    {
        // Arrange
        Ecosystem ecosystem = new EcosystemBuilder()
            .WithUserId(ControlTestConstants.UserId)
            .Build();

        DbContext.Aquariums.Add(ecosystem);
        await DbContext.SaveChangesAsync();

        var command = new CreateVacationModeCommand
        {
            EcosystemId = ecosystem.Id,
            StartDate = DateTime.UtcNow.AddDays(2),
            EndDate = DateTime.UtcNow.AddDays(8),
            IsActive = true,
            CalculatedFeed = 150.0
        };

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            ApiConstants.Routes.VacationModes, command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain(ApiConstants.Routes.VacationModes);

        Guid createdId = await response.Content.ReadFromJsonAsync<Guid>();
        createdId.Should().NotBeEmpty();

        VacationMode? dbVacation = await DbContext.Vacations
            .AsNoTracking().FirstOrDefaultAsync(v => v.Id == createdId);
        dbVacation.Should().NotBeNull();
        dbVacation!.CalculatedFeed.Should().Be(command.CalculatedFeed);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Create_WithInvalidDates_Returns400BadRequest()
    {
        // Arrange
        Ecosystem ecosystem = new EcosystemBuilder()
            .WithUserId(ControlTestConstants.UserId)
            .Build();

        DbContext.Aquariums.Add(ecosystem);
        await DbContext.SaveChangesAsync();

        var command = new CreateVacationModeCommand
        {
            EcosystemId = ecosystem.Id,
            StartDate = DateTime.UtcNow.AddDays(5),
            EndDate = DateTime.UtcNow.AddDays(2),
            IsActive = true,
            CalculatedFeed = 150.0
        };

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            ApiConstants.Routes.VacationModes, command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Create_ForAnotherUserEcosystem_Returns409Conflict()
    {
        // Arrange
        Ecosystem ecosystem = new EcosystemBuilder()
            .WithUserId(Guid.NewGuid())
            .Build();

        DbContext.Aquariums.Add(ecosystem);
        await DbContext.SaveChangesAsync();

        var command = new CreateVacationModeCommand
        {
            EcosystemId = ecosystem.Id,
            StartDate = DateTime.UtcNow.AddDays(2),
            EndDate = DateTime.UtcNow.AddDays(8),
            IsActive = true,
            CalculatedFeed = 150.0
        };

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            ApiConstants.Routes.VacationModes, command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Update_WithoutAuthorizationHeader_Returns401Unauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;
        var command = new UpdateVacationModeCommand
        {
            StartDate = DateTime.UtcNow.AddDays(3),
            EndDate = DateTime.UtcNow.AddDays(7),
            CalculatedFeed = 200.0
        };

        // Act
        HttpResponseMessage response = await Client.PutAsJsonAsync(
            $"{ApiConstants.Routes.VacationModes}/{Guid.NewGuid()}", command);

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

        VacationMode vacation = new VacationModeBuilder()
            .WithEcosystemId(ecosystem.Id)
            .Build();

        DbContext.Aquariums.Add(ecosystem);
        DbContext.Vacations.Add(vacation);
        await DbContext.SaveChangesAsync();

        var command = new UpdateVacationModeCommand
        {
            StartDate = DateTime.UtcNow.AddDays(3),
            EndDate = DateTime.UtcNow.AddDays(9),
            CalculatedFeed = 180.0
        };

        // Act
        HttpResponseMessage response = await Client.PutAsJsonAsync(
            $"{ApiConstants.Routes.VacationModes}/{vacation.Id}", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        VacationMode? dbVacation = await DbContext.Vacations
            .AsNoTracking().FirstOrDefaultAsync(v => v.Id == vacation.Id);
        dbVacation.Should().NotBeNull();
        dbVacation!.CalculatedFeed.Should().Be(command.CalculatedFeed);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Update_WhenDoesNotExist_Returns404NotFound()
    {
        // Arrange
        var command = new UpdateVacationModeCommand
        {
            StartDate = DateTime.UtcNow.AddDays(3),
            EndDate = DateTime.UtcNow.AddDays(9),
            CalculatedFeed = 180.0
        };

        // Act
        HttpResponseMessage response = await Client.PutAsJsonAsync(
            $"{ApiConstants.Routes.VacationModes}/{Guid.NewGuid()}", command);

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

        VacationMode vacation = new VacationModeBuilder()
            .WithEcosystemId(ecosystem.Id)
            .Build();

        DbContext.Aquariums.Add(ecosystem);
        DbContext.Vacations.Add(vacation);
        await DbContext.SaveChangesAsync();

        var command = new UpdateVacationModeCommand
        {
            StartDate = DateTime.UtcNow.AddDays(3),
            EndDate = DateTime.UtcNow.AddDays(9),
            CalculatedFeed = 180.0
        };

        // Act
        HttpResponseMessage response = await Client.PutAsJsonAsync(
            $"{ApiConstants.Routes.VacationModes}/{vacation.Id}", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Toggle_WithoutAuthorizationHeader_Returns401Unauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        HttpResponseMessage response = await Client.PutAsync(
            $"{ApiConstants.Routes.VacationModes}/{Guid.NewGuid()}/toggle", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Toggle_WithValidRequest_Returns204NoContent()
    {
        // Arrange
        Ecosystem ecosystem = new EcosystemBuilder()
            .WithUserId(ControlTestConstants.UserId)
            .Build();

        VacationMode vacation = new VacationModeBuilder()
            .WithEcosystemId(ecosystem.Id)
            .WithIsActive(false)
            .Build();

        DbContext.Aquariums.Add(ecosystem);
        DbContext.Vacations.Add(vacation);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await Client.PutAsync(
            $"{ApiConstants.Routes.VacationModes}/{vacation.Id}/toggle", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        VacationMode? dbVacation = await DbContext.Vacations
            .AsNoTracking().FirstOrDefaultAsync(v => v.Id == vacation.Id);
        dbVacation!.IsActive.Should().BeTrue();
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Toggle_WhenDoesNotExist_Returns404NotFound()
    {
        // Act
        HttpResponseMessage response = await Client.PutAsync(
            $"{ApiConstants.Routes.VacationModes}/{Guid.NewGuid()}/toggle", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Toggle_WhenBelongsToAnotherUser_Returns409Conflict()
    {
        // Arrange
        Ecosystem ecosystem = new EcosystemBuilder()
            .WithUserId(Guid.NewGuid())
            .Build();

        VacationMode vacation = new VacationModeBuilder()
            .WithEcosystemId(ecosystem.Id)
            .Build();

        DbContext.Aquariums.Add(ecosystem);
        DbContext.Vacations.Add(vacation);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await Client.PutAsync(
            $"{ApiConstants.Routes.VacationModes}/{vacation.Id}/toggle", null);

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
            $"{ApiConstants.Routes.VacationModes}/{Guid.NewGuid()}");

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

        VacationMode vacation = new VacationModeBuilder()
            .WithEcosystemId(ecosystem.Id)
            .Build();

        DbContext.Aquariums.Add(ecosystem);
        DbContext.Vacations.Add(vacation);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await Client.DeleteAsync(
            $"{ApiConstants.Routes.VacationModes}/{vacation.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        VacationMode? dbVacation = await DbContext.Vacations
            .AsNoTracking().FirstOrDefaultAsync(v => v.Id == vacation.Id);
        dbVacation.Should().BeNull();
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Delete_WhenDoesNotExist_Returns404NotFound()
    {
        // Act
        HttpResponseMessage response = await Client.DeleteAsync(
            $"{ApiConstants.Routes.VacationModes}/{Guid.NewGuid()}");

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

        VacationMode vacation = new VacationModeBuilder()
            .WithEcosystemId(ecosystem.Id)
            .Build();

        DbContext.Aquariums.Add(ecosystem);
        DbContext.Vacations.Add(vacation);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await Client.DeleteAsync(
            $"{ApiConstants.Routes.VacationModes}/{vacation.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
