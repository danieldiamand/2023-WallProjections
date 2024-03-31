﻿using System;
using System.Collections.Immutable;
using Avalonia;
using WallProjections.Helper.Interfaces;
#if !DEBUGSKIPPYTHON
using WallProjections.Models.Interfaces;
using System.Diagnostics;
using System.IO;
using Python.Runtime;

#else
using System.Diagnostics.CodeAnalysis;
using WallProjections.Models.Interfaces;
#endif

namespace WallProjections.Helper;

#if !DEBUGSKIPPYTHON
/// <inheritdoc />
public sealed class PythonProxy : IPythonProxy
{
    /// <summary>
    /// Path to VirtualEnv if it exists.
    /// </summary>
    private static string _virtualEnvPath => Path.Combine(IFileHandler.ConfigFolderPath, "VirtualEnv");

    /// <summary>
    /// A handle to Python threads
    /// </summary>
    private readonly IntPtr _threadsPtr;

    /// <summary>
    /// The currently running Python module for hotspot detection
    /// </summary>
    private readonly AtomicPythonModule _currentModule = new();

    /// <summary>
    /// Initializes the Python runtime
    /// </summary>
    public PythonProxy()
    {
        // Only use VirtualEnv if directory for VirtualEnv exists.
        if (Directory.Exists(_virtualEnvPath))
        {
            Debug.WriteLine("Getting Python information from VirtualEnv...");

            string pathVeScripts;
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                pathVeScripts = _virtualEnvPath + @"\Scripts";
            else
                pathVeScripts = _virtualEnvPath + @"/bin";
            Environment.SetEnvironmentVariable("PATH", pathVeScripts, EnvironmentVariableTarget.Process);

            var pythonPath = string.Empty;

            // Process gets the location of the Python DLL and the full path for the VirtualEnv.
            var proc = new Process();
            proc.StartInfo.FileName = pathVeScripts + Path.DirectorySeparatorChar + "python";
            proc.StartInfo.Arguments = $"-c \"import sys; import find_libpython; print(find_libpython.find_libpython() + ',' +'{Path.PathSeparator}'.join(sys.path))\"";
            proc.StartInfo.RedirectStandardOutput = true;
            if (!proc.Start())
                throw new Exception("Couldn't initialize Python in virtual environment");
            proc.WaitForExit();

            var pythonOutput = proc.StandardOutput.ReadToEnd().Split(',');

            pythonPath = pythonOutput[1].Replace(Environment.NewLine, "");
            if (string.IsNullOrEmpty(pythonPath))
                throw new Exception("Couldn't initialize Python.NET");

            Runtime.PythonDLL = pythonOutput[0];

            Environment.SetEnvironmentVariable("PYTHONPATH", pythonPath, EnvironmentVariableTarget.Process);
            PythonEngine.PythonPath = pythonPath;
        }
        else
        {
            Runtime.PythonDLL = Environment.GetEnvironmentVariable("PYTHON_DLL");
        }

        Debug.WriteLine("Initializing Python...    ");
        PythonEngine.Initialize();
        _threadsPtr = PythonEngine.BeginAllowThreads();
        Debug.WriteLine("Done");
    }

    /// <inheritdoc />
    public void StartHotspotDetection(IPythonHandler eventListener, IConfig config)
    {
        RunPythonAction(PythonModule.HotspotDetection, module => { module.StartDetection(eventListener, config); });
    }

    /// <inheritdoc />
    public void StopCurrentAction()
    {
        var currentModule = _currentModule.Take();
        if (currentModule is not PythonModule.HotspotDetectionModule module) return;

        using (Py.GIL())
        {
            module.StopDetection();
        }
    }

    /// <inheritdoc />
    public double[,]? CalibrateCamera(ImmutableDictionary<int, Point> arucoPositions) =>
        RunPythonAction(PythonModule.Calibration, module => module.CalibrateCamera(arucoPositions));

    /// <summary>
    /// Runs the given action after acquiring the Python GIL importing the given module
    /// </summary>
    /// <param name="moduleFactory">A factory for creating the Python module</param>
    /// <param name="action">The action to run using the Python module</param>
    /// <typeparam name="T">The type of the Python module</typeparam>
    /// <remarks><see cref="AtomicPythonModule.Set">Sets</see> <see cref="_currentModule" /> to the imported module</remarks>
    private void RunPythonAction<T>(Func<T> moduleFactory, Action<T> action) where T : PythonModule
    {
        using (Py.GIL())
        {
            //TODO Maybe Import a module once and reuse it?
            var module = moduleFactory();
            _currentModule.Set(module);
            action(module);
        }
    }

    /// <summary>
    /// Runs the given action after acquiring the Python GIL importing the given module and returns the result
    /// </summary>
    /// <param name="moduleFactory">A factory for creating the Python module</param>
    /// <param name="action">The action to run using the Python module</param>
    /// <typeparam name="T">The type of the Python module</typeparam>
    /// <typeparam name="TR">The return type of the action</typeparam>
    /// <remarks><see cref="AtomicPythonModule.Set">Sets</see> <see cref="_currentModule" /> to the imported module</remarks>
    private TR RunPythonAction<T, TR>(Func<T> moduleFactory, Func<T, TR> action) where T : PythonModule
    {
        using (Py.GIL())
        {
            //TODO Maybe Import a module once and reuse it?
            var module = moduleFactory();
            _currentModule.Set(module);
            return action(module);
        }
    }

    public void Dispose()
    {
        PythonEngine.EndAllowThreads(_threadsPtr);
        PythonEngine.Shutdown();
    }

    /// <summary>
    /// A thread-safe wrapper around a Python module
    /// </summary>
    private class AtomicPythonModule
    {
        /// <summary>
        /// The internal module reference
        /// </summary>
        /// <remarks>
        /// Remember to lock on <i>this</i> when accessing this field
        /// </remarks>
        private PythonModule? _module;

        /// <summary>
        /// Sets the module, replacing the previous one (if any)
        /// </summary>
        public void Set(PythonModule module)
        {
            lock (this)
            {
                _module = module;
            }
        }

        /// <summary>
        /// Takes the module, i.e. returns it and sets the internal reference to <i>null</i>
        /// </summary>
        /// <returns>The previously held module, or <i>null</i> if there was none</returns>
        public PythonModule? Take()
        {
            lock (this)
            {
                var module = _module;
                _module = null;
                return module;
            }
        }
    }
}
#else
/// <summary>
/// A mock of <see cref="IPythonProxy" /> in an environment without Python
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Mock Python proxy for manual testing")]
public sealed class PythonProxy : IPythonProxy
{
    /// <summary>
    /// Prints a message to the console
    /// </summary>
    public void StartHotspotDetection(IPythonHandler eventListener, IConfig config)
    {
        Console.WriteLine("Starting hotspot detection");
    }

    /// <summary>
    /// Prints a message to the console
    /// </summary>
    public void StopCurrentAction()
    {
        Console.WriteLine("Stopping currently running action");
    }

    /// <summary>
    /// Prints a message to the console and returns an identity matrix
    /// </summary>
    public double[,] CalibrateCamera(ImmutableDictionary<int, Point> arucoPositions)
    {
        Console.WriteLine("Calibrating camera");
        return new double[,]
        {
            { 1, 0, 0 },
            { 0, 1, 0 },
            { 0, 0, 1 }
        };
    }

    /// <summary>
    /// Prints a message to the console
    /// </summary>
    public void Dispose()
    {
        Console.WriteLine("Disposing PythonProxy");
    }
}
#endif
