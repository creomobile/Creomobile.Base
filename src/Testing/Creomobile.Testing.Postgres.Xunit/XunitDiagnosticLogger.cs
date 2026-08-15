using Microsoft.Extensions.Logging;
using Xunit;

namespace Creomobile.Testing.Postgres;

/// <summary>
/// Forwards Testcontainers' log output to xunit's diagnostic messages.
/// </summary>
/// <remarks>
/// Testcontainers reports through <see cref="ILogger"/>, and a container that fails to start
/// says why only there. xunit v3 hands a fixture no logger; the diagnostic sink is reachable
/// from the ambient <see cref="TestContext"/> instead. Routing one to the other is what keeps
/// those messages visible without taking an <c>IMessageSink</c> constructor parameter, which
/// xunit v4 no longer allows a fixture to have.
/// <para>
/// Messages appear only when the test project switches diagnostic messages on
/// (<c>diagnosticMessages</c> in <c>xunit.runner.json</c>).
/// </para>
/// </remarks>
sealed class XunitDiagnosticLogger : ILogger
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    // Debug and Trace describe every step of a healthy startup; what a failure needs is
    // Information and above.
    public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var message = formatter(state, exception);

        if (exception is not null)
            message = $"{message}{Environment.NewLine}{exception}";

        TestContext.Current.SendDiagnosticMessage("[testcontainers] {0}: {1}", logLevel, message);
    }
}
