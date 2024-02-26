﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ReactiveUI;
using WallProjections.Models;
using WallProjections.ViewModels.Interfaces.Editor;

namespace WallProjections.ViewModels.Editor;

/// <inheritdoc cref="IPositionEditorViewModel" />
public class PositionEditorViewModel : ViewModelBase, IPositionEditorViewModel
{
    /// <summary>
    /// A mutex ensuring sequential access to the position and radius of the hotspot.
    /// </summary>
    private readonly Mutex _mutex = new();

    /// <summary>
    /// The backing field for <see cref="SelectedHotspot" />.
    /// </summary>
    private IEditorHotspotViewModel? _selectedHotspot;

    /// <summary>
    /// The backing field for <see cref="X" />.
    /// </summary>
    private double _x;

    /// <summary>
    /// The backing field for <see cref="Y" />.
    /// </summary>
    private double _y;

    /// <summary>
    /// The backing field for <see cref="R" />.
    /// </summary>
    private double _r;

    /// <summary>
    /// The backing field for <see cref="UnselectedHotspots" />.
    /// </summary>
    private IEnumerable<Coord> _unselectedHotspots = Enumerable.Empty<Coord>();

    public bool IsInEditMode { get; set; }

    /// <inheritdoc />
    public IEditorHotspotViewModel? SelectedHotspot
    {
        set
        {
            _selectedHotspot = value;
            _x = _selectedHotspot?.Position.X ?? 0;
            _y = _selectedHotspot?.Position.Y ?? 0;
            _r = _selectedHotspot?.Position.R ?? 0;

            this.RaisePropertyChanged(nameof(X));
            this.RaisePropertyChanged(nameof(Y));
            this.RaisePropertyChanged(nameof(R));
        }
    }

    /// <inheritdoc />
    public IEnumerable<Coord> UnselectedHotspots
    {
        get => _unselectedHotspots;
        set => this.RaiseAndSetIfChanged(ref _unselectedHotspots, value);
    }

    /// <inheritdoc />
    public double X
    {
        get => _x;
        private set => this.RaiseAndSetIfChanged(ref _x, value);
    }

    /// <inheritdoc />
    public double Y
    {
        get => _y;
        private set => this.RaiseAndSetIfChanged(ref _y, value);
    }

    /// <inheritdoc />
    public double R
    {
        get => _r;
        private set => this.RaiseAndSetIfChanged(ref _r, _r + value);
    }

    /// <inheritdoc />
    public void SetPosition(double x, double y)
    {
        if (!IsInEditMode || _selectedHotspot is null) return;

        _mutex.WaitOne();
        X = x;
        Y = y;
        _mutex.ReleaseMutex();
    }

    /// <inheritdoc />
    public void ChangeRadius(double delta)
    {
        if (!IsInEditMode || _selectedHotspot is null) return;

        _mutex.WaitOne();
        R += delta;
        _mutex.ReleaseMutex();
    }

    /// <inheritdoc />
    public void UpdateSelectedHotspot()
    {
        if (!IsInEditMode || _selectedHotspot is null) return;

        _mutex.WaitOne();
        _selectedHotspot.Position = new Coord(X, Y, R);
        _mutex.ReleaseMutex();
    }
}
