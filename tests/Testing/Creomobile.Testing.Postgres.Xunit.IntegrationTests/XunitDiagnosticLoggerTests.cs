using Microsoft.Extensions.Logging;

namespace Creomobile.Testing.Postgres.IntegrationTests;

// The logger is what makes a container that refuses to start explain itself. Nothing else in
// this suite would go red if it stopped working, so its policy is pinned here.
public sealed class XunitDiagnosticLoggerTests
{
    readonly XunitDiagnosticLogger _logger = new();

    [Theory]
    [InlineData(LogLevel.Information)]
    [InlineData(LogLevel.Warning)]
    [InlineData(LogLevel.Error)]
    [InlineData(LogLevel.Critical)]
    public void ForwardsInformationAndAbove(LogLevel logLevel)
        => _logger.IsEnabled(logLevel).Should().BeTrue();

    [Theory]
    [InlineData(LogLevel.Trace)]
    [InlineData(LogLevel.Debug)]
    public void DropsTheStepByStepChatterOfAHealthyStartup(LogLevel logLevel)
        => _logger.IsEnabled(logLevel).Should().BeFalse();

    [Fact]
    public void LoggingAnExceptionDoesNotThrow()
        => _logger.Invoking(logger => logger.Log(
                LogLevel.Error,
                default,
                "could not start",
                new InvalidOperationException("docker is not reachable"),
                (state, exception) => $"{state}: {exception?.Message}"))
            .Should().NotThrow();

    [Fact]
    public void BeginScopeIsAcceptedAndUnused()
        => _logger.BeginScope("scope").Should().BeNull();

    // The point of the level check is that a dropped message costs nothing, so the observable
    // promise is not "no output" — it is that the formatter is never even invoked.
    [Fact]
    public void DoesNotFormatAMessageItWillDrop()
    {
        var formatted = false;

        _logger.Log(
            LogLevel.Debug,
            default,
            "state",
            exception: null,
            (state, _) =>
            {
                formatted = true;
                return state;
            });

        formatted.Should().BeFalse();
    }
}
