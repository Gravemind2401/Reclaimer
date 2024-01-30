using Adjutant.Spatial;
using Reclaimer.Drawing;
using Reclaimer.Geometry;
using System.Numerics;

namespace Adjutant.Geometry
{
    public interface IGeometryModel : IDisposable
    {
        Matrix4x4 CoordinateSystem { get; }

        string Name { get; }
        IReadOnlyList<IGeometryNode> Nodes { get; }
        IReadOnlyList<IGeometryMarkerGroup> MarkerGroups { get; }
        IReadOnlyList<IGeometryRegion> Regions { get; }
        IReadOnlyList<IGeometryMaterial> Materials { get; }
        IReadOnlyList<IRealBounds5D> Bounds { get; }
        IReadOnlyList<IGeometryMesh> Meshes { get; }
    }

    public interface IGeometryNode
    {
        string Name { get; }
        short ParentIndex { get; }
        short FirstChildIndex { get; }
        short NextSiblingIndex { get; }
        IVector3 Position { get; }
        IVector4 Rotation { get; }
        Matrix4x4 OffsetTransform { get; }
    }

    public interface IGeometryMarkerGroup
    {
        string Name { get; }
        IReadOnlyList<IGeometryMarker> Markers { get; }
    }

    public interface IGeometryRegion
    {
        int SourceIndex { get; }
        string Name { get; }
        IReadOnlyList<IGeometryPermutation> Permutations { get; }
    }

    public interface IGeometryPermutation
    {
        int SourceIndex { get; }
        string Name { get; }
        int MeshIndex { get; }
        int MeshCount { get; }

        float TransformScale { get; }
        Matrix4x4 Transform { get; }
    }

    public interface IGeometryMarker
    {
        byte RegionIndex { get; }
        byte PermutationIndex { get; }
        byte NodeIndex { get; }
        IVector3 Position { get; }
        IVector4 Rotation { get; }
    }

    public interface IGeometryMesh : IMeshCompat, IDisposable
    {
        bool IsInstancing { get; }

        VertexWeights VertexWeights { get; }

        byte? NodeIndex { get; }
        short? BoundsIndex { get; }
        IReadOnlyList<IGeometrySubmesh> Submeshes { get; }
    }

    public interface IGeometrySubmesh : ISubmeshCompat
    {
        short MaterialIndex { get; }
    }

    public interface IGeometryMaterial
    {
        string Name { get; }
        MaterialFlags Flags { get; }
        IReadOnlyList<ISubmaterial> Submaterials { get; }
        //IReadOnlyList<TintColour> TintColours { get; }
    }

    public interface ISubmaterial
    {
        int Usage { get; }
        IBitmap Bitmap { get; }
        IVector2 Tiling { get; }
    }
}
