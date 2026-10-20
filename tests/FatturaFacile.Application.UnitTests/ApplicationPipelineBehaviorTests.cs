using FatturaFacile.Application.Common.Behaviors;
using FatturaFacile.Application.Common.Exceptions;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Domain.Repositories;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using ValidationException = FatturaFacile.Application.Common.Exceptions.ValidationException;

namespace FatturaFacile.Application.UnitTests;

/// <summary>
/// Dummy test command for behavior tests.
/// </summary>
public sealed record TestSampleCommand(string Name) : IRequest<string>;

/// <summary>
/// Dummy test query for behavior tests.
/// </summary>
public sealed record TestSampleQuery(int Id) : IRequest<string>;

/// <summary>
/// Unit tests for MediatR pipeline behaviors and exceptions.
/// </summary>
public sealed class ApplicationPipelineBehaviorTests
{
    /// <summary>
    /// Tests ValidationBehavior passes through when no validators are registered.
    /// </summary>
    [Fact]
    public async Task ValidationBehavior_NoValidators_ShouldCallNext()
    {
        // Arrange
        var behavior = new ValidationBehavior<TestSampleCommand, string>([]);
        var request = new TestSampleCommand("Valid");

        // Act
        var result = await behavior.Handle(request, (ct) => Task.FromResult("Success"), CancellationToken.None);

        // Assert
        _ = result.Should().Be("Success");
    }

    /// <summary>
    /// Tests ValidationBehavior throws ValidationException when validation failures exist.
    /// </summary>
    [Fact]
    public async Task ValidationBehavior_ValidationFailure_ShouldThrowValidationException()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<TestSampleCommand>>();
        var failure = new ValidationFailure("Name", "Name is required");
        _ = validatorMock.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestSampleCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([failure]));

        var behavior = new ValidationBehavior<TestSampleCommand, string>([validatorMock.Object]);
        var request = new TestSampleCommand(string.Empty);

        // Act
        var act = async () => await behavior.Handle(request, (ct) => Task.FromResult("Success"), CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<ValidationException>();
        _ = ex.Which.Errors.Should().ContainKey("Name");
        _ = ex.Which.Errors["Name"].Should().Contain("Name is required");
    }

    /// <summary>
    /// Tests LoggingBehavior logs execution and returns response.
    /// </summary>
    [Fact]
    public async Task LoggingBehavior_SuccessfulExecution_ShouldReturnResponse()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<LoggingBehavior<TestSampleCommand, string>>>();
        var behavior = new LoggingBehavior<TestSampleCommand, string>(loggerMock.Object);
        var request = new TestSampleCommand("Hello");

        // Act
        var result = await behavior.Handle(request, (ct) => Task.FromResult("Done"), CancellationToken.None);

        // Assert
        _ = result.Should().Be("Done");
    }

    /// <summary>
    /// Tests LoggingBehavior logs error and rethrows when delegate fails.
    /// </summary>
    [Fact]
    public async Task LoggingBehavior_ExceptionThrown_ShouldLogAndRethrow()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<LoggingBehavior<TestSampleCommand, string>>>();
        var behavior = new LoggingBehavior<TestSampleCommand, string>(loggerMock.Object);
        var request = new TestSampleCommand("ErrorRequest");

        // Act
        var act = async () => await behavior.Handle(request, (ct) => throw new InvalidOperationException("Pipeline error"), CancellationToken.None);

        // Assert
        _ = await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Pipeline error");
    }

    /// <summary>
    /// Tests TenantBehavior when tenant provider has active tenant.
    /// </summary>
    [Fact]
    public async Task TenantBehavior_WithActiveTenant_ShouldCallNext()
    {
        // Arrange
        var tenantMock = new Mock<ITenantProvider>();
        _ = tenantMock.SetupGet(t => t.HasTenant).Returns(true);
        _ = tenantMock.SetupGet(t => t.CurrentTenantId).Returns(Guid.NewGuid());

        var loggerMock = new Mock<ILogger<TenantBehavior<TestSampleCommand, string>>>();
        var behavior = new TenantBehavior<TestSampleCommand, string>(tenantMock.Object, loggerMock.Object);

        // Act
        var result = await behavior.Handle(new TestSampleCommand("Test"), (ct) => Task.FromResult("OK"), CancellationToken.None);

        // Assert
        _ = result.Should().Be("OK");
    }

    /// <summary>
    /// Tests TransactionBehavior auto-commits unit of work when request name ends with Command.
    /// </summary>
    [Fact]
    public async Task TransactionBehavior_CommandRequest_ShouldCommitUnitOfWork()
    {
        // Arrange
        var uowMock = new Mock<IUnitOfWork>();
        _ = uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var loggerMock = new Mock<ILogger<TransactionBehavior<TestSampleCommand, string>>>();
        var behavior = new TransactionBehavior<TestSampleCommand, string>(uowMock.Object, loggerMock.Object);

        // Act
        var result = await behavior.Handle(new TestSampleCommand("Save"), (ct) => Task.FromResult("Saved"), CancellationToken.None);

        // Assert
        _ = result.Should().Be("Saved");
        uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests TransactionBehavior does not commit unit of work when request is a query.
    /// </summary>
    [Fact]
    public async Task TransactionBehavior_QueryRequest_ShouldNotCommitUnitOfWork()
    {
        // Arrange
        var uowMock = new Mock<IUnitOfWork>();
        var loggerMock = new Mock<ILogger<TransactionBehavior<TestSampleQuery, string>>>();
        var behavior = new TransactionBehavior<TestSampleQuery, string>(uowMock.Object, loggerMock.Object);

        // Act
        var result = await behavior.Handle(new TestSampleQuery(42), (ct) => Task.FromResult("QueryResults"), CancellationToken.None);

        // Assert
        _ = result.Should().Be("QueryResults");
        uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Tests ValidationException constructors and error formatting.
    /// </summary>
    [Fact]
    public void ValidationException_Constructors_ShouldInitializeProperly()
    {
        // Act & Assert
        var exDefault = new ValidationException();
        _ = exDefault.Message.Should().Contain("One or more validation failures have occurred");
        _ = exDefault.Errors.Should().BeEmpty();

        var exMsg = new ValidationException("Custom error message");
        _ = exMsg.Message.Should().Be("Custom error message");

        var inner = new FormatException("Inner error");
        var exInner = new ValidationException("Message with inner", inner);
        _ = exInner.InnerException.Should().Be(inner);

        var failures = new List<ValidationFailure>
        {
            new("Code", "Code is required"),
            new("Code", "Code must be uppercase"),
            new("Email", "Invalid email format"),
        };
        var exFailures = new ValidationException(failures);
        _ = exFailures.Errors.Should().HaveCount(2);
        _ = exFailures.Errors["Code"].Should().HaveCount(2);
        _ = exFailures.Errors["Email"].Should().ContainSingle();
    }
}
