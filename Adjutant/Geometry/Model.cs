using Adjutant.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Geometry
{
    public class Model : IModel
    {
        public Matrix4x4 CoordinateSystem { get; set; }

        public string Name { get; set; }
        public List<INode> Nodes { get; set; }
        public List<IMarkerGroup> MarkerGroups { get; set; }
        public List<IRegion> Regions { get; set; }
        //public List<Shader> Shaders { get; set; }
        public List<IRealBounds5D> Bounds { get; set; }
        public List<IMesh> Meshes { get; set; }

        public Model()
        {
            Nodes = new List<INode>();
            MarkerGroups = new List<IMarkerGroup>();
            Regions = new List<IRegion>();
            Bounds = new List<IRealBounds5D>();
            Meshes = new List<IMesh>();
        }

        public override string ToString() => Name;

        #region IModel

        IReadOnlyList<INode> IModel.Nodes => Nodes;

        IReadOnlyList<IMarkerGroup> IModel.MarkerGroups => MarkerGroups;

        IReadOnlyList<IRegion> IModel.Regions => Regions;

        IReadOnlyList<IRealBounds5D> IModel.Bounds => Bounds;

        IReadOnlyList<IMesh> IModel.Meshes => Meshes;

        #endregion
    }

    public class Node : INode
    {
        public string Name { get; set; }
        public short ParentIndex { get; set; }
        public short FirstChildIndex { get; set; }
        public short NextSiblingIndex { get; set; }
        public IRealVector3D Position { get; set; }
        public IRealVector4D Rotation { get; set; }

        public override string ToString() => Name;
    }

    public class MarkerGroup : IMarkerGroup
    {
        public string Name { get; set; }
        public List<IMarker> Markers { get; set; }

        public MarkerGroup()
        {
            Markers = new List<IMarker>();
        }

        public override string ToString() => Name;

        #region IMarkerGroup

        IReadOnlyList<IMarker> IMarkerGroup.Markers => Markers; 

        #endregion
    }

    public class Region : IRegion
    {
        public string Name { get; set; }
        public List<IPermutation> Permutations { get; set; }

        public Region()
        {
            Permutations = new List<IPermutation>();
        }

        public override string ToString() => Name;

        #region IRegion

        IReadOnlyList<IPermutation> IRegion.Permutations => Permutations; 

        #endregion
    }

    public class Permutation : IPermutation
    {
        public string Name { get; set; }
        public byte NodeIndex { get; set; }
        public short BoundsIndex { get; set; }
        public int MeshIndex { get; set; }

        public float TransformScale { get; set; }
        public Matrix4x4 Transform { get; set; }
        public List<ISubmesh> Submeshes { get; set; }

        public Permutation()
        {
            Submeshes = new List<ISubmesh>();
        }

        public override string ToString() => Name;

        IReadOnlyList<ISubmesh> IPermutation.Submeshes => Submeshes;
    }

    public class Submesh : ISubmesh
    {
        public short MaterialIndex { get; set; }
        public int IndexStart { get; set; }
        public int IndexLength { get; set; }
        public int VertexStart { get; set; }
        public int VertexLength { get; set; }
    }

    public class Marker : IMarker
    {
        public byte RegionIndex { get; set; }
        public byte PermutationIndex { get; set; }
        public byte NodeIndex { get; set; }
        public IRealVector3D Position { get; set; }
        public IRealVector4D Rotation { get; set; }
    }

    public class Mesh : IMesh
    {
        public VertexWeights VertexWeights { get; set; }
        public IndexFormat IndexFormat { get; set; }

        public IXMVector[] Vertices { get; set; }
        public int[] Indicies { get; set; }

        #region IMesh

        IReadOnlyList<IXMVector> IMesh.Vertices => Vertices;

        IReadOnlyList<int> IMesh.Indicies => Indicies; 

        #endregion
    }

    public enum VertexWeights
    {
        None,
        Single,
        Multiple
    }

    public enum IndexFormat
    {
        Triangles = 3,
        Stripped = 5
    }
}
