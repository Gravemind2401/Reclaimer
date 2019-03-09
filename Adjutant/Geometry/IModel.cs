using Adjutant.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Geometry
{
    public interface IModel
    {
        Matrix4x4 CoordinateSystem { get; }

        string Name { get; }
        IReadOnlyList<INode> Nodes { get; }
        IReadOnlyList<IMarkerGroup> MarkerGroups { get; }
        IReadOnlyList<IRegion> Regions { get; }
        //IReadOnlyList<Shader> Shaders { get; }
        IReadOnlyList<IRealBounds5D> Bounds { get; }
        IReadOnlyList<IMesh> Meshes { get; }
    }

    public interface INode
    {
        string Name { get; }
        short ParentIndex { get; }
        short FirstChildIndex { get; }
        short NextSiblingIndex { get; }
        IRealVector3D Position { get; }
        IRealVector4D Rotation { get; }
    }

    public interface IMarkerGroup
    {
        string Name { get; }
        IReadOnlyList<IMarker> Markers { get; }
    }

    public interface IRegion
    {
        string Name { get; }
        IReadOnlyList<IPermutation> Permutations { get; }
    }

    public interface IPermutation
    {
        string Name { get; }
        byte NodeIndex { get; }
        short BoundsIndex { get; }
        int MeshIndex { get; }

        float TransformScale { get; }
        Matrix4x4 Transform { get; }
        IReadOnlyList<ISubmesh> Submeshes { get; }
    }

    public interface ISubmesh
    {
        short MaterialIndex { get; }
        int IndexStart { get; }
        int IndexLength { get; }
        int VertexStart { get; }
        int VertexLength { get; }
    }

    public interface IMarker
    {
        byte RegionIndex { get; }
        byte PermutationIndex { get; }
        byte NodeIndex { get; }
        IRealVector3D Position { get; }
        IRealVector4D Rotation { get; }
    }

    public interface IMesh
    {
        VertexWeights VertexWeights { get; }
        IndexFormat IndexFormat { get; }

        IReadOnlyList<IXMVector> Vertices { get; }
        IReadOnlyList<int> Indicies { get; }
    }
}
