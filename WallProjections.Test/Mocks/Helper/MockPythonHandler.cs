﻿using WallProjections.Helper;
using WallProjections.Helper.Interfaces;

namespace WallProjections.Test.Mocks.Helper;

/// <summary>
/// A mock of <see cref="PythonHandler" />
/// </summary>
/// <remarks>
/// This is basically the same thing as <see cref="PythonHandler" />,
/// but with a public constructor instead of being a singleton
/// </remarks>
public class MockPythonHandler : IPythonHandler
{
    public PythonScript? CurrentScript { get; private set; }

    public bool IsDisposed { get; private set; }

    /// <inheritdoc />
    public event EventHandler<IPythonHandler.HotspotSelectedArgs>? HotspotSelected;

    public Task RunHotspotDetection()
    {
        CurrentScript = PythonScript.HotspotDetection;
        return Task.CompletedTask;
    }

    public Task RunCalibration()
    {
        CurrentScript = PythonScript.Calibration;
        return Task.CompletedTask;
    }

    public void CancelCurrentTask()
    {
        CurrentScript = null;
    }

    /// <inheritdoc />
    public void OnPressDetected(int id)
    {
        HotspotSelected?.Invoke(this, new IPythonHandler.HotspotSelectedArgs(id));
    }

    /// <summary>
    /// Returns if there are any subscribers to <see cref="HotspotSelected" />
    /// </summary>
    public bool HasSubscribers => HotspotSelected is not null && HotspotSelected.GetInvocationList().Length > 0;

    public void Dispose()
    {
        IsDisposed = true;
        CancelCurrentTask();
        GC.SuppressFinalize(this);
    }

    public enum PythonScript
    {
        HotspotDetection,
        Calibration
    }
}
