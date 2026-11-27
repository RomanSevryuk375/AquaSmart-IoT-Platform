---
trigger: model_decision
description: >-
  Testing rules and conventions for the Application layer (*.Application.UnitTests,
  TelegramBot.Application.UnitTests, BuildingBlocks.UnitTests/Application) across all AquaAPI microservices:
  AAA pattern, FluentAssertions, NSubstitute mocks, handlers, validators, and behaviors.
  Apply whenever creating, modifying, or reviewing tests in any Application unit test project.
---

# Application Layer Testing Standards (`*.Application.UnitTests`)

1. **Frameworks, Naming & Mocking:**
   - Use **xUnit**, **FluentAssertions**, and **NSubstitute** (`Substitute.For<T>()`), following `<MethodOrAction>_<ConditionOrStateUnderTest>_<ExpectedOutcome>` with `// Arrange`, `// Act`, `// Assert`.
   - Mock external ports (`I*Repository`, `IUnitOfWork`, security/domain services, `IFusionCache`, `ILogger<T>`); never connect to real databases or brokers in unit tests.

2. **Handler Tests (`Features/`):**
   - Instantiate entities for mock returns using `*.TestShared/Builders` (`new <Entity>Builder().Build()`).
   - **Success path:** verify `result.IsSuccess.Should().BeTrue()`, returned payload (`result.Value`), and repository/port calls via `.Received(1)` with `Arg.Is<...>(...)`.
   - **Failure / Short-circuit path:** verify `result.IsFailure.Should().BeTrue()`, expected `result.Error.Code` / `ErrorType`, and that write/side-effect methods (`AddAsync`, `Update`, `DeleteAsync`, `Publish`) were `.DidNotReceive()`.

3. **Validator, Security & Shared Behavior Tests:**
   - Test boundary inputs, `Guid.Empty` (including `UserId` on `IUserBoundRequest`), enum checks, and custom `Must(...)` rules on `*Validator` classes.
   - Test service-specific ownership/security behaviors for both allowed owner and unauthorized/other-user rejection.
   - Tests for shared `BuildingBlocks.Application.Behaviors` (`TransactionBehavior`, `ValidationBehavior`, `CacheBehavior`, `LoggingBehavior`, `BehaviorHelpers`) belong in `BuildingBlocks.UnitTests/Application/Behaviors/`, not inside individual services.
