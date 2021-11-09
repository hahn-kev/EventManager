using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Dialogs;
using Avalonia.Logging;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.Logging;
using ReactiveUI;

namespace EventManager
{
    class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static void Main(string[] args)
        {

            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddFile("Logs/log-{Date}.txt", LogLevel.Error, new Dictionary<string, LogLevel>()
                {
                    {"OpenGL", LogLevel.None},
                    {"Binding", LogLevel.None}
                });
                builder.AddDebug();
            });
            var logger = loggerFactory.CreateLogger("main");
            try
            {
                Logger.Sink = new CustomLogSink(loggerFactory);
                logger.LogInformation("application start");
                BuildAvaloniaApp(loggerFactory)
                    .StartWithClassicDesktopLifetime(args);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Main application error");
                throw;
            }
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp(ILoggerFactory loggerFactory) =>
            AppBuilder.Configure(() => new App(loggerFactory))
                .UsePlatformDetect()
                .UseReactiveUI();
    }
}
