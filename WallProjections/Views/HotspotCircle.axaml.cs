﻿using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.Shapes;

namespace WallProjections.Views;

/// <summary>
/// A circle representing a hotspot
/// </summary>
public partial class HotspotCircle : Ellipse, IDisposable
{
    /// <summary>
    /// The time required to pulse the hotspot (one direction only)
    /// </summary>
    public static readonly TimeSpan PulseTime = TimeSpan.FromSeconds(1);

    #region Styled/Direct properties

    /// <summary>
    /// A <see cref="StyledProperty{T}">StyledProperty</see> that defines the <see cref="Diameter" /> property.
    /// </summary>
    public static readonly StyledProperty<double> DiameterProperty =
        AvaloniaProperty.Register<HotspotCircle, double>(nameof(Diameter));

    /// <summary>
    /// A <see cref="DirectProperty{T, TP}">DirectProperty</see> that defines the <see cref="IsActivating" /> property.
    /// </summary>
    public static readonly DirectProperty<HotspotCircle, bool> IsActivatingProperty =
        AvaloniaProperty.RegisterDirect<HotspotCircle, bool>(
            nameof(IsActivating),
            o => o.IsActivating,
            (o, v) => o.IsActivating = v
        );

    /// <summary>
    /// A <see cref="DirectProperty{T, TP}">DirectProperty</see> that defines the <see cref="IsActivated" /> property.
    /// </summary>
    public static readonly DirectProperty<HotspotCircle, bool> IsActivatedProperty =
        AvaloniaProperty.RegisterDirect<HotspotCircle, bool>(
            nameof(IsActivated),
            o => o.IsActivated,
            (o, v) => o.IsActivated = v
        );

    /// <summary>
    /// A <see cref="DirectProperty{T, TP}">DirectProperty</see> that defines the <see cref="Pulse" /> property.
    /// </summary>
    public static readonly DirectProperty<HotspotCircle, bool> PulseProperty =
        AvaloniaProperty.RegisterDirect<HotspotCircle, bool>(
            nameof(Pulse),
            o => o.Pulse,
            (o, v) => o.Pulse = v
        );

    #endregion

    #region Backing fields

    /// <summary>
    /// The subject that holds the value of <see cref="IsActivating" />
    /// </summary>
    private readonly BehaviorSubject<bool> _isActivating = new(false);

    /// <summary>
    /// The subject that holds the value of <see cref="IsActivated" />
    /// </summary>
    private readonly BehaviorSubject<bool> _isActivated = new(false);

    /// <summary>
    /// The subject that holds the value of <see cref="Pulse" />
    /// </summary>
    private readonly BehaviorSubject<bool> _pulse = new(false);

    #endregion

    /// <summary>
    /// The diameter of the hotspot
    /// </summary>
    public double Diameter
    {
        get => GetValue(DiameterProperty);
        set => SetValue(DiameterProperty, value);
    }

    /// <summary>
    /// Whether the hotspot is activating
    /// </summary>
    public bool IsActivating
    {
        get => _isActivating.Value;
        set
        {
            var oldValue = _isActivating.Value;
            _isActivating.OnNext(value);
            RaisePropertyChanged(IsActivatingProperty, oldValue, value);
        }
    }

    /// <summary>
    /// Whether the hotspot is fully activated
    /// </summary>
    public bool IsActivated
    {
        get => _isActivated.Value;
        set
        {
            var oldValue = _isActivated.Value;
            _isActivated.OnNext(value);
            RaisePropertyChanged(IsActivatedProperty, oldValue, value);
            if (value)
                StartPulsing();
        }
    }

    /// <summary>
    /// Whether the hotspot should be scaled up
    /// </summary>
    private bool Pulse
    {
        get => _pulse.Value && IsActivated;
        set
        {
            var oldValue = _pulse.Value;
            _pulse.OnNext(value);
            RaisePropertyChanged(PulseProperty, oldValue, value);
        }
    }

    public HotspotCircle()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Starts pulsing the hotspot
    /// </summary>
    /// <remarks>Stops pulsing if the hotspot is <see cref="IsActivated">deactivated</see></remarks>
    private async void StartPulsing()
    {
        while (true)
        {
            if (!IsActivated) return;
            Pulse = false;

            await Task.Delay(PulseTime);

            if (!IsActivated) return;
            Pulse = true;

            await Task.Delay(PulseTime);
        }
    }

    public void Dispose()
    {
        _isActivating.Dispose();
        _isActivated.Dispose();
        GC.SuppressFinalize(this);
    }
}
