using Adjutant.Spatial;
using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Geometry
{
    public interface IGeometryModel
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
        IRealVector3D Position { get; }
        IRealVector4D Rotation { get; }
    }

    public interface IGeometryMarkerGroup
    {
        string Name { get; }
        IReadOnlyList<IGeometryMarker> Markers { get; }
    }

    public interface IGeometryRegion
    {
        string Name { get; }
        IReadOnlyList<IGeometryPermutation> Permutations { get; }
    }

    public interface IGeometryPermutation
    {
        string Name { get; }
        byte NodeIndex { get; }
        short BoundsIndex { get; }
        int MeshIndex { get; }
        int MeshCount { get; }

        float TransformScale { get; }
        Matrix4x4 Transform { get; }
    }

    public interface IGeometrySubmesh
    {
        short MaterialIndex { get; }
        int IndexStart { get; }
        int IndexLength { get; }
    }

    public interface IGeometryMarker
    {
        byte RegionIndex { get; }
        byte PermutationIndex { get; }
        byte NodeIndex { get; }
        IRealVector3D Position { get; }
        IRealVector4D Rotation { get; }
    }

    public interface IGeometryMesh
    {
        VertexWeights VertexWeights { get; }
        IndexFormat IndexFormat { get; }

        IReadOnlyList<IVertex> Vertices { get; }
        IReadOnlyList<int> Indicies { get; }

        IReadOnlyList<IGeometrySubmesh> Submeshes { get; }
    }

    public interface IGeometryMaterial
    {
        string Name { get; }
        IBitmap Diffuse { get; }
        IRealVector2D Tiling { get; }
    }
}
