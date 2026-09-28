using BuildingBlocks.Presentation.Constants;
using Device.API.Filters;
using Microsoft.AspNetCore.Routing;

namespace Device.Application.UnitTests.Filters;

public class DeviceAuthenticationFilterHelpersTests
{
    // --------------- TryGetDeviceToken ---------------

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public void TryGetDeviceToken_WhenHeaderPresent_ReturnsTrueAndToken()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers[ApiConstants.Headers.DeviceToken] = "  my_token  ";

        // Act
        bool result = DeviceAuthenticationFilterHelpers.TryGetDeviceToken(context.Request, out string token);

        // Assert
        result.Should().BeTrue();
        token.Should().Be("my_token");
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public void TryGetDeviceToken_WhenHeaderMissing_ReturnsFalse()
    {
        var context = new DefaultHttpContext();

        bool result = DeviceAuthenticationFilterHelpers.TryGetDeviceToken(context.Request, out string token);

        result.Should().BeFalse();
        token.Should().BeEmpty();
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public void TryGetDeviceToken_WhenHeaderWhitespaceOnly_ReturnsFalse()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers[ApiConstants.Headers.DeviceToken] = "   ";

        bool result = DeviceAuthenticationFilterHelpers.TryGetDeviceToken(context.Request, out string token);

        result.Should().BeFalse();
        token.Should().BeEmpty();
    }

    // --------------- TryGetMacAddress ---------------

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public void TryGetMacAddress_WhenHeaderPresent_ReturnsTrueAndNormalizedMac()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers[ApiConstants.Headers.MacAddress] = "  aa:bb:cc:dd:ee:ff  ";

        bool result = DeviceAuthenticationFilterHelpers.TryGetMacAddress(context, out string mac);

        result.Should().BeTrue();
        mac.Should().Be("AA:BB:CC:DD:EE:FF");
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public void TryGetMacAddress_WhenHeaderMissing_ReturnsFalse()
    {
        var context = new DefaultHttpContext();

        bool result = DeviceAuthenticationFilterHelpers.TryGetMacAddress(context, out string mac);

        result.Should().BeFalse();
        mac.Should().BeEmpty();
    }

    // --------------- TryGetControllerId ---------------

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public void TryGetControllerId_WithControllerIdKey_ReturnsGuid()
    {
        var controllerId = Guid.NewGuid();
        var routeValues = new RouteValueDictionary
        {
            ["controllerId"] = controllerId.ToString()
        };

        bool result = DeviceAuthenticationFilterHelpers.TryGetControllerId(routeValues, out Guid parsed);

        result.Should().BeTrue();
        parsed.Should().Be(controllerId);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public void TryGetControllerId_WithIdKey_ReturnsGuid()
    {
        var controllerId = Guid.NewGuid();
        var routeValues = new RouteValueDictionary
        {
            ["id"] = controllerId.ToString()
        };

        bool result = DeviceAuthenticationFilterHelpers.TryGetControllerId(routeValues, out Guid parsed);

        result.Should().BeTrue();
        parsed.Should().Be(controllerId);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public void TryGetControllerId_WhenNeitherKeyPresent_ReturnsFalse()
    {
        var routeValues = new RouteValueDictionary();

        bool result = DeviceAuthenticationFilterHelpers.TryGetControllerId(routeValues, out Guid parsed);

        result.Should().BeFalse();
        parsed.Should().Be(Guid.Empty);
    }

    // --------------- TryGetGuid ---------------

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public void TryGetGuid_WithValidGuidString_ReturnsTrue()
    {
        var expected = Guid.NewGuid();
        var routeValues = new RouteValueDictionary { ["commandId"] = expected.ToString() };

        bool result = DeviceAuthenticationFilterHelpers.TryGetGuid(routeValues, "commandId", out Guid parsed);

        result.Should().BeTrue();
        parsed.Should().Be(expected);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public void TryGetGuid_WhenKeyMissing_ReturnsFalseAndEmpty()
    {
        var routeValues = new RouteValueDictionary();

        bool result = DeviceAuthenticationFilterHelpers.TryGetGuid(routeValues, "commandId", out Guid parsed);

        result.Should().BeFalse();
        parsed.Should().Be(Guid.Empty);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public void TryGetGuid_WhenValueIsNotGuid_ReturnsFalseAndEmpty()
    {
        var routeValues = new RouteValueDictionary { ["commandId"] = "not-a-guid" };

        bool result = DeviceAuthenticationFilterHelpers.TryGetGuid(routeValues, "commandId", out Guid parsed);

        result.Should().BeFalse();
        parsed.Should().Be(Guid.Empty);
    }
}
