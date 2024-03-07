using System;
using System.Collections.Generic;
using WallProjections.Models;
using WallProjections.ViewModels.Interfaces;
using WallProjections.ViewModels.Interfaces.Display;
using WallProjections.ViewModels.Interfaces.Display.Layouts;

namespace WallProjections.ViewModels.Display.Layouts;

/// <summary>
/// A viewmodel for a view that displays a video with a title and description.
/// </summary>
public class VideoWithDescriptionViewModel : Layout, IDisposable
{
    /// <inheritdoc cref="IVideoViewModel" />
    public IVideoViewModel VideoViewModel { get; }

    /// <summary>
    /// The title of the hotspot.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// The description of the hotspot.
    /// </summary>
    public string Description { get; }

    // ReSharper disable once MemberCanBePrivate.Global
    /// <summary>
    /// Creates a new <see cref="VideoWithDescriptionViewModel"/>
    /// with the given <paramref name="title" />, <paramref name="description" />,
    /// and paths to the videos that <see cref="VideoViewModel" /> will play.
    /// </summary>
    /// <param name="vmProvider">The <see cref="IViewModelProvider" /> to get the <see cref="IVideoViewModel" />.</param>
    /// <param name="title">The title of the hotspot.</param>
    /// <param name="description">The description of the hotspot.</param>
    /// <param name="videoPaths">The paths to the videos to play.</param>
    public VideoWithDescriptionViewModel(
        IViewModelProvider vmProvider,
        string title,
        string description,
        IEnumerable<string> videoPaths)
    {
        VideoViewModel = vmProvider.GetVideoViewModel();
        Title = title;
        Description = description;
        VideoViewModel.PlayVideos(videoPaths);
    }

    public void Dispose()
    {
        VideoViewModel.Dispose();
        GC.SuppressFinalize(this);
    }

    // ReSharper disable once UnusedType.Global
    /// <summary>
    /// A factory for creating <see cref="VideoWithDescriptionViewModel" />s.
    /// </summary>
    public class Factory : LayoutFactory
    {
        /// <inheritdoc />
        public override bool IsCompatibleData(Hotspot.Media hotspot)
        {
            var imagesCompatible = hotspot.ImagePaths.Count == 0;
            var videosCompatible = hotspot.VideoPaths.Count == 1;

            return imagesCompatible && videosCompatible;
        }

        /// <inheritdoc />
        protected override Layout ConstructLayout(IViewModelProvider vmProvider, Hotspot.Media hotspot) =>
            new VideoWithDescriptionViewModel(vmProvider, hotspot.Title, hotspot.Description, hotspot.VideoPaths);
    }
}
