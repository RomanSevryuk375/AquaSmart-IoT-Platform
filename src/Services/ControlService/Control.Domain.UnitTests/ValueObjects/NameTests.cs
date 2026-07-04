namespace Control.Domain.UnitTests.ValueObjects;

public class NameTests
{
    [Fact]
    public void Create_WithValidName_ReturnsSuccessAndTrimsValue()
    {
        // Arrange
        string rawName = "  My Ecosystem Name   ";

        // Act
        Result<Name> result = Name.Create(rawName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("My Ecosystem Name");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyOrWhitespace_ReturnsFailure(string invalidName)
    {
        // Act
        Result<Name> result = Name.Create(invalidName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Name.Invalid");
        result.Error.Message.Should().Contain("Name cannot be empty");
    }

    [Fact]
    public void Create_WithNameExceedingLengthLimit_ReturnsFailure()
    {
        // Arrange
        string tooLongName = new('A', CommonConstants.NameLength + 1);

        // Act
        Result<Name> result = Name.Create(tooLongName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Name.Invalid");
        result.Error.Message.Should().Contain($"Name cannot exceed {CommonConstants.NameLength} characters");
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        // Arrange
        Name name = Name.Create("Test Name").Value;

        // Act
        string str = name.ToString();

        // Assert
        str.Should().Be("Test Name");
    }
}
