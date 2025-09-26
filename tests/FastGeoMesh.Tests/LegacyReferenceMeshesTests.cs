using FastGeoMesh.Meshing;
using Xunit;

namespace FastGeoMesh.Tests;

public sealed class LegacyFilesLoadTests
{
    private static string Res(string folder) => Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Resources", folder, "0_maill.txt");

    [Theory]
    [InlineData("1")]
    [InlineData("2")]
    [InlineData("3")]
    [InlineData("4")]
    [InlineData("5")]
    public void LegacyFileParsesAndBasicConsistency(string folder)
    {
        string path = Res(folder);
        Assert.True(File.Exists(path), $"Missing legacy file {path}");
        var legacy = IndexedMesh.ReadCustomTxt(path);

        Assert.NotEmpty(legacy.Vertices);
        Assert.NotEmpty(legacy.Edges);
        Assert.NotEmpty(legacy.Quads);

        foreach (var e in legacy.Edges)
        {
            Assert.InRange(e.a, 0, legacy.Vertices.Count - 1);
            Assert.InRange(e.b, 0, legacy.Vertices.Count - 1);
            Assert.NotEqual(e.a, e.b);
        }

        foreach (var q in legacy.Quads)
        {
            Assert.InRange(q.v0, 0, legacy.Vertices.Count - 1);
            Assert.InRange(q.v1, 0, legacy.Vertices.Count - 1);
            Assert.InRange(q.v2, 0, legacy.Vertices.Count - 1);
            Assert.InRange(q.v3, 0, legacy.Vertices.Count - 1);
        }

        double minZ = double.MaxValue, maxZ = double.MinValue;
        foreach (var v in legacy.Vertices)
        {
            if (v.Z < minZ) minZ = v.Z;
            if (v.Z > maxZ) maxZ = v.Z;
        }
        Assert.True(maxZ > minZ);
    }
}
