---
trigger: model_decision
description: >-
  Testing rules and conventions for the Domain layer (*.Domain.UnitTests, BuildingBlocks.UnitTests/Domain,
  *.ArchitectureTests, *.TestShared) across all AquaAPI microservices: AAA pattern, FluentAssertions,
  validation errors, version/concurrency, TestData Builders, Value Objects, and Domain Events.
  Apply whenever creating, modifying, or reviewing tests in any *.Domain.UnitTests or *.ArchitectureTests project.
---

# Domain Layer Testing Standards (`*.Domain.UnitTests`, `*.ArchitectureTests`)

1. **Frameworks & Isolation:**
   - Use **xUnit** (`[Fact]`, `[Theory]`, `[InlineData]`), **FluentAssertions**, and **NetArchTest.Rules**.
   - **Never mock inside domain unit tests** (no Moq, NSubstitute). Test all domain logic, entities, value objects, and factories strictly in-memory.

2. **Test Method Naming & Structure:**
   - Follow `<MethodOrAction>_<ConditionOrStateUnderTest>_<ExpectedOutcome>` with explicit `// Arrange`, `// Act`, `// Assert` sections.
   - Examples: `Create_WithEmptyUserId_ReturnsFailure`, `UpdateProfile_WithInvalidName_ReturnsFailureDoesNotMutateStateOrRaiseEvent`, `SetActive_WithSameStatus_DoesNotIncrementVersion`.

3. **Entity Creation Tests (`Create`):**
   - **Negative cases:** verify each invalid field produces `result.IsFailure.Should().BeTrue()` with aggregated error code `result.Error.Code.Should().Be("<Entity>.Invalid")` and `ErrorType.Validation`.
   - **Positive cases:** verify `result.IsSuccess.Should().BeTrue()`, all properties mapped, dates verified with `Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5))`, `Version.Should().NotBeEmpty()`, and created domain events verified via `.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<TEvent>()`.

4. **Mutation & State Change Tests:**
   - **Use TestData Builders:** instantiate entities using `new <Entity>Builder().Build()` from `*.TestShared/Builders` to isolate tests from constructor/factory changes.
   - **Positive mutation:** verify state updated, `Version.Should().NotBe(initialVersion)` (or `ConcurrencyStamp`), and expected domain events raised.
   - **Idempotent / No-Op:** verify when same state is passed, `Version.Should().Be(initialVersion)` and `DomainEvents.Should().BeEmpty()`.
   - **Failure mutation:** verify `result.IsFailure.Should().BeTrue()` with code `<Entity>.Invalid`, state unchanged, `Version` unchanged, and `DomainEvents.Should().BeEmpty()`.

5. **Value Objects Tests (`ValueObjects/`):**
   - Use `[Theory]` for boundary checks (`null`, `""`, whitespace `"   "`), length limits (`new string('a', MaxLength + 1)`), invalid regex formats, and negative ranges.
   - Test success normalization, `ToString()`, and structural record equality (`vo1.Should().Be(vo2)`).
   - Tests for shared `BuildingBlocks.Domain` Value Objects, `DomainGuard`, and `Result` belong in `BuildingBlocks.UnitTests/Domain/`, not inside individual services.

6. **Architecture Tests (`*.ArchitectureTests`):**
   - Inherit from shared base classes in `BuildingBlocks.ArchitectureTestShared` (`DomainArchitectureTestsBase`, `ApplicationArchitectureTestsBase`, `DomainLayerArchitectureTestsBase`, `ServiceLayerArchitectureTestsBase`) and provide only service assemblies/namespaces + service-specific rules; do not duplicate standard checks across services.
