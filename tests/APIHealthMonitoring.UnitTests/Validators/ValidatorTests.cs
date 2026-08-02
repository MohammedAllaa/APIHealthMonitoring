using System.ComponentModel.DataAnnotations;
using APIHealthMonitoring.Application.DTOs.Auth;
using APIHealthMonitoring.Application.DTOs.MonitoringConfig;
using FluentAssertions;

namespace APIHealthMonitoring.UnitTests.Validators;

public class ValidatorTests
{
    private static List<ValidationResult> ValidateModel(object model)
    {
        var context = new ValidationContext(model, serviceProvider: null, items: null);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, context, results, validateAllProperties: true);
        return results;
    }

    // -------------------------------------------------------------------------
    // CreateMonitoringConfigDto Tests
    // -------------------------------------------------------------------------

    [Fact]
    public void CreateMonitoringConfigDto_ValidModel_ShouldHaveNoValidationErrors()
    {
        // Arrange
        var model = new CreateMonitoringConfigDto
        {
            ApiEndpointId = 1,
            SlowThresholdMs = 1000,
            CriticalThresholdMs = 2000,
            FailureCountLimit = 3,
            AvailabilityThreshold = 99.0m
        };

        // Act
        var results = ValidateModel(model);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void CreateMonitoringConfigDto_FailureCountLimitOutsideBoundary_ShouldHaveValidationError()
    {
        // Arrange
        var model = new CreateMonitoringConfigDto
        {
            ApiEndpointId = 1,
            FailureCountLimit = 11 // Max is 10
        };

        // Act
        var results = ValidateModel(model);

        // Assert
        results.Should().ContainSingle();
        results[0].ErrorMessage.Should().Contain("FailureCountLimit must be between");
    }

    [Fact]
    public void CreateMonitoringConfigDto_SlowThresholdGreaterThanCritical_ShouldHaveValidationError()
    {
        // Arrange
        var model = new CreateMonitoringConfigDto
        {
            ApiEndpointId = 1,
            SlowThresholdMs = 1500,
            CriticalThresholdMs = 1000 // Slow threshold >= Critical threshold
        };

        // Act
        var results = ValidateModel(model);

        // Assert
        results.Should().ContainSingle();
        results[0].ErrorMessage.Should().Be("SlowThresholdMs must be less than CriticalThresholdMs.");
    }

    // -------------------------------------------------------------------------
    // RegisterRequestDto Tests
    // -------------------------------------------------------------------------

    [Fact]
    public void RegisterRequestDto_ValidModel_ShouldHaveNoValidationErrors()
    {
        // Arrange
        var model = new RegisterRequestDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@test.com",
            Password = "SecurePassword123!",
            ConfirmPassword = "SecurePassword123!",
            Role = "Viewer"
        };

        // Act
        var results = ValidateModel(model);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void RegisterRequestDto_InvalidEmailPattern_ShouldHaveValidationError()
    {
        // Arrange
        var model = new RegisterRequestDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "invalid-email",
            Password = "SecurePassword123!",
            ConfirmPassword = "SecurePassword123!",
            Role = "Viewer"
        };

        // Act
        var results = ValidateModel(model);

        // Assert
        results.Should().ContainSingle();
        results[0].MemberNames.Should().Contain(nameof(RegisterRequestDto.Email));
    }

    [Fact]
    public void RegisterRequestDto_PasswordMismatch_ShouldHaveValidationError()
    {
        // Arrange
        var model = new RegisterRequestDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@test.com",
            Password = "SecurePassword123!",
            ConfirmPassword = "DifferentPassword123!",
            Role = "Viewer"
        };

        // Act
        var results = ValidateModel(model);

        // Assert
        results.Should().ContainSingle();
        results[0].ErrorMessage.Should().Be("Passwords do not match.");
    }
}
