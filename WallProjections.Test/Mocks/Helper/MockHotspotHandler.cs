﻿using WallProjections.Helper.Interfaces;

namespace WallProjections.Test.Mocks.Helper;

public class MockHotspotHandler : IHotspotHandler
{
    public event EventHandler<IHotspotHandler.HotspotChangingArgs>? HotspotActivating;
    public event EventHandler<IHotspotHandler.HotspotArgs>? HotspotActivated;
    public event EventHandler<IHotspotHandler.HotspotChangingArgs>? HotspotDeactivating;
    public event EventHandler<IHotspotHandler.HotspotArgs>? HotspotForcefullyDeactivated;

    /// <summary>
    /// Whether <see cref="Dispose" /> has been called.
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Returns if there are any subscribers to <see cref="HotspotActivating" />
    /// </summary>
    public bool HasActivatingSubscribers =>
        HotspotActivating is not null && HotspotActivating.GetInvocationList().Length > 0;

    /// <summary>
    /// Returns if there are any subscribers to <see cref="HotspotActivated" />
    /// </summary>
    public bool HasActivatedSubscribers =>
        HotspotActivated is not null && HotspotActivated.GetInvocationList().Length > 0;

    /// <summary>
    /// Returns if there are any subscribers to <see cref="HotspotDeactivating" />
    /// </summary>
    public bool HasDeactivatingSubscribers =>
        HotspotDeactivating is not null && HotspotDeactivating.GetInvocationList().Length > 0;

    /// <summary>
    /// Returns if there are any subscribers to <see cref="HotspotForcefullyDeactivated" />
    /// </summary>
    public bool HasForcefullyDeactivatedSubscribers =>
        HotspotForcefullyDeactivated is not null && HotspotForcefullyDeactivated.GetInvocationList().Length > 0;

    /// <summary>
    /// Invokes the <see cref="HotspotActivating" /> event.
    /// </summary>
    /// <param name="id">The id of the hotspot passed to <see cref="IHotspotHandler.HotspotArgs" />.</param>
    public void StartHotspotActivation(int id) =>
        HotspotActivating?.Invoke(this, new IHotspotHandler.HotspotChangingArgs(id, TimeSpan.Zero));

    /// <summary>
    /// Invokes the <see cref="HotspotActivated" /> event.
    /// </summary>
    /// <param name="id">The id of the hotspot passed to <see cref="IHotspotHandler.HotspotArgs" />.</param>
    public void ActivateHotspot(int id) =>
        HotspotActivated?.Invoke(this, new IHotspotHandler.HotspotArgs(id));

    /// <summary>
    /// Invokes the <see cref="HotspotDeactivating" /> event.
    /// </summary>
    /// <param name="id">The id of the hotspot passed to <see cref="IHotspotHandler.HotspotArgs" />.</param>
    public void DeactivateHotspot(int id) => HotspotDeactivating?.Invoke(
        this,
        new IHotspotHandler.HotspotChangingArgs(id, IHotspotHandler.ActivationTime * 0.5)
    );

    /// <summary>
    /// Invokes the <see cref="HotspotForcefullyDeactivated" /> event.
    /// </summary>
    /// <param name="id">The id of the hotspot passed to <see cref="IHotspotHandler.HotspotArgs" />.</param>
    public void ForcefullyDeactivateHotspot(int id) =>
        HotspotForcefullyDeactivated?.Invoke(this, new IHotspotHandler.HotspotArgs(id));

    public void Dispose()
    {
        IsDisposed = true;
        GC.SuppressFinalize(this);
    }
}
