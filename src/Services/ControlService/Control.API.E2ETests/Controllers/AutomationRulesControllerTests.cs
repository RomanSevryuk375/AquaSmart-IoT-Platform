using Contracts.Enums;
using Control.Application.DTOs.AutomationRule;
using Control.Application.Features.AutomationRules.Commands.CreateRule;
using Control.Application.Features.AutomationRules.Queries;
using Control.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Control.API.E2ETests.Controllers;

public class AutomationRulesControllerTests(E2ETestWebAppFactory factory) : BaseE2ETest(factory)
{
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetAll_WithoutAuthorizationHeader_Returns401Unauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        HttpResponseMessage response = await Client.GetAsync(ApiConstants.Routes.AutomationRules);

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

        AutomationRule rule = new AutomationRuleBuilder()
            .WithEcosystemId(ecosystem.Id)
            .Build();

        DbContext.Aquariums.Add(ecosystem);
        DbContext.Rules.Add(rule);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await Client.GetAsync($"{ApiConstants.Routes.AutomationRules}?ecosystemId={ecosystem.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        IReadOnlyList<AutomationRuleDto>? content = await response.Content.ReadFromJsonAsync<IReadOnlyList<AutomationRuleDto>>();
        content.Should().NotBeNull();
        content.Should().ContainSingle(r => r.Id == rule.Id);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetAll_TenantIsolation_Returns409Conflict()
    {
        // Arrange
        Ecosystem ecosystem = new EcosystemBuilder()
            .WithUserId(Guid.NewGuid())
            .Build();

        AutomationRule rule = new AutomationRuleBuilder()
            .WithEcosystemId(ecosystem.Id)
            .Build();

        DbContext.Aquariums.Add(ecosystem);
        DbContext.Rules.Add(rule);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await Client.GetAsync(
            $"{ApiConstants.Routes.AutomationRules}?ecosystemId={ecosystem.Id}");

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
            $"{ApiConstants.Routes.AutomationRules}/{Guid.NewGuid()}");

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

        AutomationRule rule = new AutomationRuleBuilder()
            .WithEcosystemId(ecosystem.Id)
            .Build();

        DbContext.Aquariums.Add(ecosystem);
        DbContext.Rules.Add(rule);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await Client.GetAsync(
            $"{ApiConstants.Routes.AutomationRules}/{rule.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        AutomationRuleDto? content = await response.Content.ReadFromJsonAsync<AutomationRuleDto>();
        content.Should().NotBeNull();
        content!.Id.Should().Be(rule.Id);
        content.Name.Should().Be(rule.Name.Value);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetById_WhenDoesNotExist_Returns404NotFound()
    {
        // Act
        HttpResponseMessage response = await Client.GetAsync(
            $"{ApiConstants.Routes.AutomationRules}/{Guid.NewGuid()}");

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

        AutomationRule rule = new AutomationRuleBuilder()
            .WithEcosystemId(ecosystem.Id)
            .Build();

        DbContext.Aquariums.Add(ecosystem);
        DbContext.Rules.Add(rule);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await Client.GetAsync(
            $"{ApiConstants.Routes.AutomationRules}/{rule.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Create_WithoutAuthorizationHeader_Returns401Unauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;
        var command = new CreateRuleCommand
        {
            EcosystemId = Guid.NewGuid(),
            RelayId = Guid.NewGuid(),
            Name = "Rule 1",
            Operator = Operator.AND,
            Action = RuleAction.SwitchOn,
            IsActive = true
        };

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            ApiConstants.Routes.AutomationRules, command);

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

        var command = new CreateRuleCommand
        {
            EcosystemId = ecosystem.Id,
            RelayId = Guid.NewGuid(),
            Name = "Valid Rule Name",
            Operator = Operator.AND,
            Action = RuleAction.SwitchOn,
            IsActive = true
        };

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            ApiConstants.Routes.AutomationRules, command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain(ApiConstants.Routes.AutomationRules);

        Guid createdId = await response.Content.ReadFromJsonAsync<Guid>();
        createdId.Should().NotBeEmpty();

        AutomationRule? dbRule = await DbContext.Rules.AsNoTracking().FirstOrDefaultAsync(r => r.Id == createdId);
        dbRule.Should().NotBeNull();
        dbRule!.Name.Value.Should().Be(command.Name);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Create_WithInvalidData_Returns400BadRequest()
    {
        // Arrange
        var command = new CreateRuleCommand
        {
            EcosystemId = Guid.NewGuid(),
            RelayId = Guid.NewGuid(),
            Name = "", // Invalid
            Operator = Operator.AND,
            Action = RuleAction.SwitchOn,
            IsActive = true
        };

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            ApiConstants.Routes.AutomationRules, command);

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

        var command = new CreateRuleCommand
        {
            EcosystemId = ecosystem.Id,
            RelayId = Guid.NewGuid(),
            Name = "Hacker Rule Attempt",
            Operator = Operator.AND,
            Action = RuleAction.SwitchOn,
            IsActive = true
        };

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            ApiConstants.Routes.AutomationRules, command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Update_WithoutAuthorizationHeader_Returns401Unauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;
        var request = new AutomationRuleUpdateRequestDto
        {
            Name = "New Name",
            RelayId = Guid.NewGuid(),
            Operator = Operator.OR,
            Action = RuleAction.SwitchOff
        };

        // Act
        HttpResponseMessage response = await Client.PutAsJsonAsync(
            $"{ApiConstants.Routes.AutomationRules}/{Guid.NewGuid()}", request);

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

        AutomationRule rule = new AutomationRuleBuilder()
            .WithEcosystemId(ecosystem.Id)
            .Build();

        DbContext.Aquariums.Add(ecosystem);
        DbContext.Rules.Add(rule);
        await DbContext.SaveChangesAsync();

        var request = new AutomationRuleUpdateRequestDto
        {
            Name = "Updated Rule Name",
            RelayId = Guid.NewGuid(),
            Operator = Operator.OR,
            Action = RuleAction.SwitchOff
        };

        // Act
        HttpResponseMessage response = await Client.PutAsJsonAsync(
            $"{ApiConstants.Routes.AutomationRules}/{rule.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        AutomationRule? dbRule = await DbContext.Rules.AsNoTracking().FirstOrDefaultAsync(r => r.Id == rule.Id);
        dbRule.Should().NotBeNull();
        dbRule!.Name.Value.Should().Be(request.Name);
        dbRule.Operator.Should().Be(request.Operator);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Update_WithInvalidData_Returns400BadRequest()
    {
        // Arrange
        Ecosystem ecosystem = new EcosystemBuilder()
            .WithUserId(ControlTestConstants.UserId)
            .Build();

        AutomationRule rule = new AutomationRuleBuilder()
            .WithEcosystemId(ecosystem.Id)
            .Build();

        DbContext.Aquariums.Add(ecosystem);
        DbContext.Rules.Add(rule);
        await DbContext.SaveChangesAsync();

        var request = new AutomationRuleUpdateRequestDto
        {
            Name = "", // Invalid
            RelayId = Guid.NewGuid(),
            Operator = Operator.AND,
            Action = RuleAction.SwitchOn
        };

        // Act
        HttpResponseMessage response = await Client.PutAsJsonAsync(
            $"{ApiConstants.Routes.AutomationRules}/{rule.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Update_WhenDoesNotExist_Returns404NotFound()
    {
        // Arrange
        var request = new AutomationRuleUpdateRequestDto
        {
            Name = "Valid Name",
            RelayId = Guid.NewGuid(),
            Operator = Operator.AND,
            Action = RuleAction.SwitchOn
        };

        // Act
        HttpResponseMessage response = await Client.PutAsJsonAsync(
            $"{ApiConstants.Routes.AutomationRules}/{Guid.NewGuid()}", request);

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

        AutomationRule rule = new AutomationRuleBuilder()
            .WithEcosystemId(ecosystem.Id)
            .Build();

        DbContext.Aquariums.Add(ecosystem);
        DbContext.Rules.Add(rule);
        await DbContext.SaveChangesAsync();

        var request = new AutomationRuleUpdateRequestDto
        {
            Name = "Valid Name",
            RelayId = Guid.NewGuid(),
            Operator = Operator.AND,
            Action = RuleAction.SwitchOn
        };

        // Act
        HttpResponseMessage response = await Client.PutAsJsonAsync(
            $"{ApiConstants.Routes.AutomationRules}/{rule.Id}", request);

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
            $"{ApiConstants.Routes.AutomationRules}/{Guid.NewGuid()}");

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

        AutomationRule rule = new AutomationRuleBuilder()
            .WithEcosystemId(ecosystem.Id)
            .Build();

        DbContext.Aquariums.Add(ecosystem);
        DbContext.Rules.Add(rule);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await Client.DeleteAsync(
            $"{ApiConstants.Routes.AutomationRules}/{rule.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        AutomationRule? dbRule = await DbContext.Rules.AsNoTracking().FirstOrDefaultAsync(r => r.Id == rule.Id);
        dbRule.Should().BeNull();
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Delete_WhenDoesNotExist_Returns404NotFound()
    {
        // Act
        HttpResponseMessage response = await Client.DeleteAsync(
            $"{ApiConstants.Routes.AutomationRules}/{Guid.NewGuid()}");

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

        AutomationRule rule = new AutomationRuleBuilder()
            .WithEcosystemId(ecosystem.Id)
            .Build();

        DbContext.Aquariums.Add(ecosystem);
        DbContext.Rules.Add(rule);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await Client.DeleteAsync(
            $"{ApiConstants.Routes.AutomationRules}/{rule.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task AddCondition_WithoutAuthorizationHeader_Returns401Unauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;
        var request = new RuleConditionRequestDto
        {
            SensorId = Guid.NewGuid(),
            Condition = Condition.Greater,
            Threshold = 25.0,
            Hysteresis = 0.5
        };

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            $"{ApiConstants.Routes.AutomationRules}/{Guid.NewGuid()}/conditions", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task AddCondition_WithValidData_Returns200OKAndConditionId()
    {
        // Arrange
        Ecosystem ecosystem = new EcosystemBuilder()
            .WithUserId(ControlTestConstants.UserId)
            .Build();

        AutomationRule rule = new AutomationRuleBuilder()
            .WithEcosystemId(ecosystem.Id)
            .Build();

        Sensor sensor = new SensorBuilder()
            .WithEcosystemId(ecosystem.Id)
            .Build();

        DbContext.Aquariums.Add(ecosystem);
        DbContext.Rules.Add(rule);
        DbContext.Sensors.Add(sensor);
        await DbContext.SaveChangesAsync();

        var request = new RuleConditionRequestDto
        {
            SensorId = sensor.Id,
            Condition = Condition.Greater,
            Threshold = 28.5,
            Hysteresis = 0.8
        };

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            $"{ApiConstants.Routes.AutomationRules}/{rule.Id}/conditions", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        Guid conditionId = await response.Content.ReadFromJsonAsync<Guid>();
        conditionId.Should().NotBeEmpty();

        RuleCondition? dbCondition = await DbContext.RuleConditions.AsNoTracking().FirstOrDefaultAsync(c => c.Id == conditionId);
        dbCondition.Should().NotBeNull();
        dbCondition!.ConditionThreshold.Threshold.Should().Be(request.Threshold);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task AddCondition_WithSensorBelongingToDifferentEcosystem_Returns400BadRequest()
    {
        // Arrange
        Ecosystem ecosystemRule = new EcosystemBuilder()
            .WithId(Guid.NewGuid())
            .WithUserId(ControlTestConstants.UserId)
            .Build();

        Ecosystem ecosystemSensor = new EcosystemBuilder()
            .WithId(Guid.NewGuid())
            .WithUserId(ControlTestConstants.UserId)
            .WithControllerId(Guid.NewGuid())
            .Build();

        AutomationRule rule = new AutomationRuleBuilder()
            .WithEcosystemId(ecosystemRule.Id)
            .Build();

        Sensor sensor = new SensorBuilder()
            .WithEcosystemId(ecosystemSensor.Id)
            .Build();

        DbContext.Aquariums.AddRange(ecosystemRule, ecosystemSensor);
        DbContext.Rules.Add(rule);
        DbContext.Sensors.Add(sensor);
        await DbContext.SaveChangesAsync();

        var request = new RuleConditionRequestDto
        {
            SensorId = sensor.Id,
            Condition = Condition.Greater,
            Threshold = 28.5,
            Hysteresis = 0.8
        };

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            $"{ApiConstants.Routes.AutomationRules}/{rule.Id}/conditions", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task AddCondition_WithInvalidData_Returns400BadRequest()
    {
        // Arrange
        var request = new RuleConditionRequestDto
        {
            SensorId = Guid.Empty,
            Condition = Condition.Greater,
            Threshold = 25.0,
            Hysteresis = 0.5
        };

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            $"{ApiConstants.Routes.AutomationRules}/{Guid.NewGuid()}/conditions", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task AddCondition_WhenRuleDoesNotExist_Returns404NotFound()
    {
        // Arrange
        var request = new RuleConditionRequestDto
        {
            SensorId = Guid.NewGuid(),
            Condition = Condition.Greater,
            Threshold = 25.0,
            Hysteresis = 0.5
        };

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            $"{ApiConstants.Routes.AutomationRules}/{Guid.NewGuid()}/conditions", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task AddCondition_WhenRuleBelongsToAnotherUser_Returns409Conflict()
    {
        // Arrange
        Ecosystem ecosystem = new EcosystemBuilder()
            .WithUserId(Guid.NewGuid())
            .Build();

        AutomationRule rule = new AutomationRuleBuilder()
            .WithEcosystemId(ecosystem.Id)
            .Build();

        Sensor sensor = new SensorBuilder()
            .WithEcosystemId(ecosystem.Id)
            .Build();

        DbContext.Aquariums.Add(ecosystem);
        DbContext.Rules.Add(rule);
        DbContext.Sensors.Add(sensor);
        await DbContext.SaveChangesAsync();

        var request = new RuleConditionRequestDto
        {
            SensorId = sensor.Id,
            Condition = Condition.Greater,
            Threshold = 25.0,
            Hysteresis = 0.5
        };

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            $"{ApiConstants.Routes.AutomationRules}/{rule.Id}/conditions", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task UpdateCondition_WithoutAuthorizationHeader_Returns401Unauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;
        var request = new RuleConditionRequestDto
        {
            SensorId = Guid.NewGuid(),
            Condition = Condition.Less,
            Threshold = 20.0,
            Hysteresis = 0.2
        };

        // Act
        HttpResponseMessage response = await Client.PutAsJsonAsync(
            $"{ApiConstants.Routes.AutomationRules}/{Guid.NewGuid()}/conditions/{Guid.NewGuid()}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task UpdateCondition_WithValidData_Returns204NoContent()
    {
        // Arrange
        Ecosystem ecosystem = new EcosystemBuilder()
            .WithUserId(ControlTestConstants.UserId)
            .Build();

        AutomationRule rule = new AutomationRuleBuilder()
            .WithEcosystemId(ecosystem.Id)
            .Build();

        Sensor sensor = new SensorBuilder()
            .WithEcosystemId(ecosystem.Id)
            .Build();

        RuleCondition condition = new RuleConditionBuilder()
            .WithAutomationRuleId(rule.Id)
            .WithSensorId(sensor.Id)
            .Build();

        DbContext.Aquariums.Add(ecosystem);
        DbContext.Rules.Add(rule);
        DbContext.Sensors.Add(sensor);
        DbContext.RuleConditions.Add(condition);
        await DbContext.SaveChangesAsync();

        var request = new RuleConditionRequestDto
        {
            SensorId = sensor.Id,
            Condition = Condition.Less,
            Threshold = 22.0,
            Hysteresis = 0.4
        };

        // Act
        HttpResponseMessage response = await Client.PutAsJsonAsync(
            $"{ApiConstants.Routes.AutomationRules}/{rule.Id}/conditions/{condition.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        RuleCondition? dbCondition = await DbContext.RuleConditions.AsNoTracking().FirstOrDefaultAsync(c => c.Id == condition.Id);
        dbCondition.Should().NotBeNull();
        dbCondition!.ConditionThreshold.Threshold.Should().Be(request.Threshold);
        dbCondition.Condition.Should().Be(request.Condition);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task UpdateCondition_WhenRuleOrConditionDoesNotExist_Returns404NotFound()
    {
        // Arrange
        var request = new RuleConditionRequestDto
        {
            SensorId = Guid.NewGuid(),
            Condition = Condition.Less,
            Threshold = 20.0,
            Hysteresis = 0.2
        };

        // Act
        HttpResponseMessage response = await Client.PutAsJsonAsync(
            $"{ApiConstants.Routes.AutomationRules}/{Guid.NewGuid()}/conditions/{Guid.NewGuid()}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task UpdateCondition_WhenBelongsToAnotherUser_Returns409Conflict()
    {
        // Arrange
        Ecosystem ecosystem = new EcosystemBuilder()
            .WithUserId(Guid.NewGuid())
            .Build();

        AutomationRule rule = new AutomationRuleBuilder()
            .WithEcosystemId(ecosystem.Id)
            .Build();

        Sensor sensor = new SensorBuilder()
            .WithEcosystemId(ecosystem.Id)
            .Build();

        RuleCondition condition = new RuleConditionBuilder()
            .WithAutomationRuleId(rule.Id)
            .WithSensorId(sensor.Id)
            .Build();

        DbContext.Aquariums.Add(ecosystem);
        DbContext.Rules.Add(rule);
        DbContext.Sensors.Add(sensor);
        DbContext.RuleConditions.Add(condition);
        await DbContext.SaveChangesAsync();

        var request = new RuleConditionRequestDto
        {
            SensorId = sensor.Id,
            Condition = Condition.Less,
            Threshold = 22.0,
            Hysteresis = 0.4
        };

        // Act
        HttpResponseMessage response = await Client.PutAsJsonAsync(
            $"{ApiConstants.Routes.AutomationRules}/{rule.Id}/conditions/{condition.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task DeleteCondition_WithoutAuthorizationHeader_Returns401Unauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        HttpResponseMessage response = await Client.DeleteAsync(
            $"{ApiConstants.Routes.AutomationRules}/{Guid.NewGuid()}/conditions/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task DeleteCondition_WhenExists_Returns204NoContent()
    {
        // Arrange
        Ecosystem ecosystem = new EcosystemBuilder()
            .WithUserId(ControlTestConstants.UserId)
            .Build();

        AutomationRule rule = new AutomationRuleBuilder()
            .WithEcosystemId(ecosystem.Id)
            .Build();

        Sensor sensor = new SensorBuilder()
            .WithEcosystemId(ecosystem.Id)
            .Build();

        RuleCondition condition = new RuleConditionBuilder()
            .WithAutomationRuleId(rule.Id)
            .WithSensorId(sensor.Id)
            .Build();

        DbContext.Aquariums.Add(ecosystem);
        DbContext.Rules.Add(rule);
        DbContext.Sensors.Add(sensor);
        DbContext.RuleConditions.Add(condition);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await Client.DeleteAsync(
            $"{ApiConstants.Routes.AutomationRules}/{rule.Id}/conditions/{condition.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        RuleCondition? dbCondition = await DbContext.RuleConditions
            .AsNoTracking().FirstOrDefaultAsync(c => c.Id == condition.Id);
        dbCondition.Should().BeNull();
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task DeleteCondition_WhenRuleOrConditionDoesNotExist_Returns404NotFound()
    {
        // Act
        HttpResponseMessage response = await Client.DeleteAsync(
            $"{ApiConstants.Routes.AutomationRules}/{Guid.NewGuid()}/conditions/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task DeleteCondition_WhenBelongsToAnotherUser_Returns409Conflict()
    {
        // Arrange
        Ecosystem ecosystem = new EcosystemBuilder()
            .WithUserId(Guid.NewGuid())
            .Build();

        AutomationRule rule = new AutomationRuleBuilder()
            .WithEcosystemId(ecosystem.Id)
            .Build();

        Sensor sensor = new SensorBuilder()
            .WithEcosystemId(ecosystem.Id)
            .Build();

        RuleCondition condition = new RuleConditionBuilder()
            .WithAutomationRuleId(rule.Id)
            .WithSensorId(sensor.Id)
            .Build();

        DbContext.Aquariums.Add(ecosystem);
        DbContext.Rules.Add(rule);
        DbContext.Sensors.Add(sensor);
        DbContext.RuleConditions.Add(condition);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await Client.DeleteAsync(
            $"{ApiConstants.Routes.AutomationRules}/{rule.Id}/conditions/{condition.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
