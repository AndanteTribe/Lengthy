#nullable enable

namespace Lengthy.Editor
{
    /// <summary>
    /// The kind of package manager to use for generating rights notation.
    /// </summary>
    public enum PackageManagerKind : byte
    {
        Invalid = 0,
        Upm = 1,
        NuGet = 2
    }
}