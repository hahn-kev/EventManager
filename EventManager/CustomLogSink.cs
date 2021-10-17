using System;
using Avalonia.Logging;
using Microsoft.Extensions.Logging;

namespace EventManager
{
    public class CustomLogSink : ILogSink
    {
        private readonly ILoggerFactory _loggerFactory;

        public CustomLogSink(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public bool IsEnabled(LogEventLevel level, string area)
        {
            return _loggerFactory.CreateLogger(area).IsEnabled(ToLogLevel(level));
        }

        private LogLevel ToLogLevel(LogEventLevel level)
        {
            return level switch
            {
                LogEventLevel.Error => LogLevel.Error,
                LogEventLevel.Verbose => LogLevel.Trace,
                LogEventLevel.Debug => LogLevel.Debug,
                LogEventLevel.Information => LogLevel.Information,
                LogEventLevel.Warning => LogLevel.Warning,
                LogEventLevel.Fatal => LogLevel.Critical,
                _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
            };
        }

        private string ModifyMessageTemplate(string template)
        {
            return template.Replace("{$", "{");
        }

        public void Log(LogEventLevel level, string area, object source, string messageTemplate)
        {
            _loggerFactory.CreateLogger(area).Log(ToLogLevel(level), ModifyMessageTemplate(messageTemplate));
        }

        public void Log<T0>(LogEventLevel level, string area, object source, string messageTemplate, T0 propertyValue0)
        {
            _loggerFactory.CreateLogger(area)
                .Log(ToLogLevel(level), ModifyMessageTemplate(messageTemplate), propertyValue0);
        }

        public void Log<T0, T1>(LogEventLevel level,
            string area,
            object source,
            string messageTemplate,
            T0 propertyValue0,
            T1 propertyValue1)
        {
            _loggerFactory.CreateLogger(area).Log(ToLogLevel(level),
                ModifyMessageTemplate(messageTemplate),
                propertyValue0,
                propertyValue1);
        }

        public void Log<T0, T1, T2>(LogEventLevel level,
            string area,
            object source,
            string messageTemplate,
            T0 propertyValue0,
            T1 propertyValue1,
            T2 propertyValue2)
        {
            _loggerFactory.CreateLogger(area).Log(ToLogLevel(level),
                ModifyMessageTemplate(messageTemplate),
                propertyValue0,
                propertyValue1,
                propertyValue2);
        }

        public void Log(LogEventLevel level,
            string area,
            object source,
            string messageTemplate,
            params object[] propertyValues)
        {
            _loggerFactory.CreateLogger(area)
                .Log(ToLogLevel(level), ModifyMessageTemplate(messageTemplate), propertyValues);
        }
    }
}
