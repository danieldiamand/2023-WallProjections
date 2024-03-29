using System;

namespace WallProjections.Models;

/// <summary>
/// Base <see cref="Exception"/> for any expected issues with the config/file management.
/// </summary>
public class ConfigException : Exception
{
    public override string Message => "Something went wrong managing the config";
}

/// <summary>
/// Thrown if the config package has invalid format.
/// </summary>
public class ConfigPackageFormatException : ConfigException
{
    /// <summary>
    /// Name of the config package which has invalid format.
    /// </summary>
    private string _fileName { get; }

    public override string Message => $".wall package {_fileName} not valid";

    public ConfigPackageFormatException(string fileName)
    {
        _fileName = fileName;
    }
}

/// <summary>
/// Thrown if the config that is imported is invalid
/// </summary>
public class ConfigInvalidException : ConfigException
{
    public override string Message => "Current config is invalid/corrupt";
}

/// <summary>
/// Thrown if there is not a config imported to the application path when required.
/// </summary>
public class ConfigNotImportedException : ConfigException
{
    public override string Message => "No config is currently loaded into the program";
}

/// <summary>
/// Thrown if there is an issue reading/writing to the required locations in the imported config.
/// </summary>
public class ConfigIOException : ConfigException
{
    public override string Message => "Unable to access loaded config files";
}

/// <summary>
/// Thrown if an external file could not be read.
/// </summary>
public class ExternalFileReadException : ConfigException
{
    /// <summary>
    /// Name of file which could not be read.
    /// </summary>
    private string _fileName { get; }

    public override string Message => $"Could not find/access {_fileName}";

    public ExternalFileReadException(string fileName)
    {
        _fileName = fileName;
    }
}
