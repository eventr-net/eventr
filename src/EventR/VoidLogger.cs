namespace EventR
{
    using System;
    using Microsoft.Extensions.Logging;

    public sealed class VoidLogger : ILogger
    {
        private VoidLogger()
        { }

        public static readonly ILogger Current = new VoidLogger();

        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotSupportedException();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return false;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
        }
    }
}
