using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.Logging;
using WallProjections.Helper;
using WallProjections.Helper.Interfaces;

[assembly: InternalsVisibleTo("WallProjections.Test")]

namespace WallProjections;

// ReSharper disable once ClassNeverInstantiated.Global
internal class Program
{
    /// <summary>
    /// Initialization code. Don't use any Avalonia, third-party APIs or any
    /// SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    /// yet and stuff might break.
    /// </summary>
    /// <param name="args">Application arguments</param>
    [STAThread]
    [ExcludeFromCodeCoverage]
    public static void Main(string[] args)
    {
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            var logPath = Path.Combine(
                FileLoggerProvider.DefaultLogFolderPath,
                $"WallProjections_{DateTime.Now:yyyy-MM-dd}.log"
            );

            if (args.Contains("--trace"))
                builder.AddFilter(level => level >= LogLevel.Trace);

            builder.AddSimpleConsole(options => options.TimestampFormat = "HH:mm:ss ");
            builder.AddProvider(new FileLoggerProvider(logPath));
        });
        var logger = loggerFactory.CreateLogger("Program");
        logger.LogInformation("Starting application");

        using var pythonProxy = new PythonProxy(new ProcessProxy(loggerFactory), loggerFactory);
        var cameras = pythonProxy.GetAvailableCameras();

        //TODO Allow user to select camera
        if (cameras.Count == 0)
        {
            logger.LogError("No cameras detected. Exiting application.");
            return;
        }
        var (cameraIndex, _) = cameras.First();

        using var pythonHandler = new PythonHandler(cameraIndex, pythonProxy, loggerFactory);
        BuildAvaloniaApp(pythonHandler, loggerFactory).StartWithClassicDesktopLifetime(args);

        logger.LogInformation("Closing application");
    }

    /// <summary>
    /// Avalonia configuration, don't remove; also used by visual designer.
    /// </summary>
    [ExcludeFromCodeCoverage]
    private static AppBuilder BuildAvaloniaApp(IPythonHandler pythonHandler, ILoggerFactory loggerFactory)
    {
        return AppBuilder.Configure(() => new App(pythonHandler, loggerFactory))
            .UsePlatformDetect()
            .LogToTrace()
            .UseReactiveUI();
    }

    // ReSharper disable once UnusedMember.Local
    /// <summary>
    /// Don't use this method. It is only used by the visual designer.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [Obsolete("This method is only used by the visual designer.", true)]
    private static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure(() => new App(null!, null!))
            .UsePlatformDetect()
            .LogToTrace()
            .UseReactiveUI();
    }
}
