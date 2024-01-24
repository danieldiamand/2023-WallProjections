﻿using System.Collections.Immutable;
using System.IO.Compression;
using WallProjections.Models;
using WallProjections.Models.Interfaces;
using WallProjections.Test.Mocks.Models;
using static WallProjections.Test.TestExtensions;

namespace WallProjections.Test.Models;

[TestFixture]
public class ContentProviderTest
{
    private const string ValidConfigPath = "Valid";
    private const string InvalidConfigPath = "Invalid";

    private static string TestZipPath =>
        Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "test.zip");

    private static string TestInvalidZipPath =>
        Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "test_invalid.zip");

    private string _configPath = null!;

    private readonly IConfig _mockConfig = new Config(Enumerable.Range(0, 5).Select(id => new Hotspot(
        id,
        new Coord(1, 1, 1),
        "0.txt",
        ImmutableList.Create("1.png"),
        ImmutableList.Create("1.mp4")
    )));

    private string MediaPath => Path.Combine(_configPath, ValidConfigPath, "Media");
    private string InvalidMediaPath => Path.Combine(_configPath, InvalidConfigPath, "Media");

    private string GetFullPath(int id, string file) => Path.Combine(MediaPath, id.ToString(), file);

    private string GetDescription(int id, string descFile) => File.ReadAllText(GetFullPath(id, descFile));

    private static IEnumerable<TestCaseData<(int, string, string[], string[])>> TestCases()
    {
        yield return MakeTestData(
            (0, "0.txt", Array.Empty<string>(), Array.Empty<string>()),
            "TextOnly"
        );
        yield return MakeTestData(
            (1, "1.txt", new[] { "1.png" }, new[] { "1.mp4" }),
            "FilenamesAreIDs"
        );
        yield return MakeTestData(
            (2, "test2.txt", new[] { "1_2.jpg", "random.jpeg" }, new[] { "2.mkv", "2_1.mov" }),
            "RandomFilenames"
        );
    }

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        // Create a temporary directory for the test
        _configPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var valid = Path.Combine(_configPath, ValidConfigPath);
        Directory.CreateDirectory(valid);
        ZipFile.ExtractToDirectory(TestZipPath, valid);

        var invalid = Path.Combine(_configPath, InvalidConfigPath);
        Directory.CreateDirectory(invalid);
        ZipFile.ExtractToDirectory(TestInvalidZipPath, invalid);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        // Clean up the temporary directory
        Directory.Delete(_configPath, true);
    }

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void GetMediaTest((int, string, string[], string[]) testCase)
    {
        var (id, descPath, imagePaths, videoPaths) = testCase;
        var provider = new ContentProvider(_mockConfig);

        var media = provider.GetMedia(id);
        var expectedDescription = GetDescription(id, descPath);
        Assert.Multiple(() =>
        {
            Assert.That(media.Description, Is.EqualTo(expectedDescription));
            Assert.That(
                imagePaths.Select(path => GetFullPath(id, path)),
                media.ImagePath is not null ? Has.Member(GetFullPath(id, media.ImagePath)) : Is.Empty
            );
            Assert.That(
                videoPaths.Select(path => GetFullPath(id, path)),
                media.VideoPath is not null ? Has.Member(GetFullPath(id, media.VideoPath)) : Is.Empty
            );
        });
    }

    [Test]
    public void GetMediaNoHotspotTest()
    {
        var provider = new ContentProvider(_mockConfig);

        Assert.Throws<IConfig.HotspotNotFoundException>(() => provider.GetMedia(-1));
    }

    [Test]
    public void GetMediaNoDescriptionTest()
    {
        var provider = new ContentProvider(_mockConfig);

        Assert.Throws<FileNotFoundException>(() => provider.GetMedia(1));
    }
}
