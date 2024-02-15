﻿using System;
using LibVLCSharp.Shared;
using WallProjections.Helper;
using WallProjections.Models;
using WallProjections.Models.Interfaces;
using WallProjections.ViewModels.Display;
using WallProjections.ViewModels.Editor;
using WallProjections.ViewModels.Interfaces;
using WallProjections.ViewModels.Interfaces.Display;
using WallProjections.ViewModels.Interfaces.Editor;

namespace WallProjections.ViewModels;

public sealed class ViewModelProvider : IViewModelProvider, IDisposable
{
    /// <summary>
    /// The backing field for the <see cref="ViewModelProvider" /> property
    /// </summary>
    private static ViewModelProvider? _viewModelProvider;

    /// <summary>
    /// The backing field for the <see cref="LibVlc" /> property
    /// </summary>
    /// <remarks>Reset when <see cref="Dispose" /> is called so that a new instance can be created if needed</remarks>
    private LibVLC? _libVlc;

    private ViewModelProvider()
    {
    }

    /// <summary>
    /// A global instance of <see cref="ViewModelProvider" />
    /// </summary>
    public static ViewModelProvider Instance => _viewModelProvider ??= new ViewModelProvider();

    /// <summary>
    /// A global instance of <see cref="LibVLC" /> to use for <see cref="LibVLCSharp" /> library
    /// </summary>
    /// <remarks>Only instantiated if needed</remarks>
    private LibVLC LibVlc => _libVlc ??= new LibVLC();

    #region Display

    /// <summary>
    /// Creates a new <see cref="DisplayViewModel" /> instance
    /// </summary>
    /// <param name="config">The <see cref="IConfig" /> containing data about the hotspots</param>
    /// <returns>A new <see cref="DisplayViewModel" /> instance</returns>
    public IDisplayViewModel GetDisplayViewModel(IConfig config) =>
        new DisplayViewModel(this, new ContentProvider(config), PythonEventHandler.Instance);

    /// <summary>
    /// Creates a new <see cref="ImageViewModel" /> instance
    /// </summary>
    /// <returns>A new <see cref="ImageViewModel" /> instance</returns>
    public IImageViewModel GetImageViewModel() => new ImageViewModel();

    /// <summary>
    /// Creates a new <see cref="VideoViewModel" /> instance
    /// </summary>
    /// <returns>A new <see cref="VideoViewModel" /> instance</returns>
    public IVideoViewModel GetVideoViewModel() => new VideoViewModel(LibVlc, new VLCMediaPlayer(LibVlc));

    #endregion

    #region Editor

    /// <summary>
    /// Creates a new <see cref="EditorViewModel" /> instance based on the given <see cref="IConfig" />
    /// </summary>
    /// <inheritdoc />
    /// <returns>A new <see cref="EditorViewModel" /> instance</returns>
    public IEditorViewModel GetEditorViewModel(IConfig config, IFileHandler fileHandler) =>
        new EditorViewModel(config, fileHandler, this);

    /// <summary>
    /// Creates a new empty <see cref="EditorViewModel" /> instance
    /// </summary>
    /// <inheritdoc />
    /// <returns>A new <see cref="EditorViewModel" /> instance</returns>
    public IEditorViewModel GetEditorViewModel(IFileHandler fileHandler) =>
        new EditorViewModel(fileHandler, this);

    /// <summary>
    /// Creates a new <see cref="EditorHotspotViewModel" /> instance
    /// </summary>
    /// <inheritdoc />
    /// <returns>A new <see cref="EditorHotspotViewModel" /> instance</returns>
    public IEditorHotspotViewModel GetEditorHotspotViewModel(int id) =>
        new EditorHotspotViewModel(id, this);

    /// <summary>
    /// Creates a new <see cref="EditorHotspotViewModel" /> instance
    /// </summary>
    /// <inheritdoc />
    /// <returns>A new <see cref="EditorHotspotViewModel" /> instance</returns>
    public IEditorHotspotViewModel GetEditorHotspotViewModel(Hotspot hotspot) =>
        new EditorHotspotViewModel(hotspot, this);

    /// <summary>
    /// Creates a new <see cref="DescriptionEditorViewModel" /> instance
    /// </summary>
    /// <inheritdoc />
    /// <returns>A new <see cref="DescriptionEditorViewModel" /> instance</returns>
    public IDescriptionEditorViewModel GetDescriptionEditorViewModel() => new DescriptionEditorViewModel(this);

    /// <summary>
    /// Creates a new <see cref="MediaEditorViewModel" /> instance
    /// </summary>
    /// <inheritdoc />
    /// <returns>A new <see cref="MediaEditorViewModel" /> instance</returns>
    public IMediaEditorViewModel GetMediaEditorViewModel(MediaEditorType type) => new MediaEditorViewModel(type switch
    {
        MediaEditorType.Images => "Images",
        MediaEditorType.Videos => "Videos",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown media type")
    });

    /// <inheritdoc />
    /// <seealso cref="ImageThumbnailViewModel" />
    /// <seealso cref="VideoThumbnailViewModel" />
    public IThumbnailViewModel GetThumbnailViewModel(MediaEditorType type, string filePath) => type switch
    {
        MediaEditorType.Images => new ImageThumbnailViewModel(filePath, new ProcessProxy()),
        MediaEditorType.Videos => new VideoThumbnailViewModel(filePath, new ProcessProxy()),
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown media type")
    };

    /// <summary>
    /// Creates a new <see cref="ImportViewModel" /> instance
    /// </summary>
    /// <inheritdoc />
    /// <returns>A new <see cref="ImportViewModel" /> instance</returns>
    public IImportViewModel GetImportViewModel(IDescriptionEditorViewModel descVm) => new ImportViewModel(descVm);

    #endregion

    /// <summary>
    /// Disposes of the <see cref="LibVlc" /> instance on resets the backing field to <i>null</i>,
    /// so that a new instance can be created if needed
    /// </summary>
    public void Dispose()
    {
        _libVlc?.Dispose();
        _libVlc = null;
    }
}
