using Adjutant.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Adjutant.Utilities;

namespace Adjutant.Geometry
{
    public class GeometryModel : IGeometryModel
    {
        public Matrix4x4 CoordinateSystem { get; set; }

        public string Name { get; set; }
        public List<IGeometryNode> Nodes { get; set; }
        public List<IGeometryMarkerGroup> MarkerGroups { get; set; }
        public List<IGeometryRegion> Regions { get; set; }
        public List<IGeometryMaterial> Materials { get; set; }
        public List<IRealBounds5D> Bounds { get; set; }
        public List<IGeometryMesh> Meshes { get; set; }

        public GeometryModel()
        {
            Nodes = new List<IGeometryNode>();
            MarkerGroups = new List<IGeometryMarkerGroup>();
            Regions = new List<IGeometryRegion>();
            Materials = new List<IGeometryMaterial>();
            Bounds = new List<IRealBounds5D>();
            Meshes = new List<IGeometryMesh>();
        }

        public override string ToString() => Name;

        #region IGeometryModel

        IReadOnlyList<IGeometryNode> IGeometryModel.Nodes => Nodes;

        IReadOnlyList<IGeometryMarkerGroup> IGeometryModel.MarkerGroups => MarkerGroups;

        IReadOnlyList<IGeometryRegion> IGeometryModel.Regions => Regions;

        IReadOnlyList<IGeometryMaterial> IGeometryModel.Materials => Materials;

        IReadOnlyList<IRealBounds5D> IGeometryModel.Bounds => Bounds;

        IReadOnlyList<IGeometryMesh> IGeometryModel.Meshes => Meshes;

        #endregion
    }

    public class GeometryNode : IGeometryNode
    {
        public string Name { get; set; }
        public short ParentIndex { get; set; }
        public short FirstChildIndex { get; set; }
        public short NextSiblingIndex { get; set; }
        public IRealVector3D Position { get; set; }
        public IRealVector4D Rotation { get; set; }

        public override string ToString() => Name;
    }

    public class GeometryMarkerGroup : IGeometryMarkerGroup
    {
        public string Name { get; set; }
        public List<IGeometryMarker> Markers { get; set; }

        public GeometryMarkerGroup()
        {
            Markers = new List<IGeometryMarker>();
        }

        public override string ToString() => Name;

        #region IGeometryMarkerGroup

        IReadOnlyList<IGeometryMarker> IGeometryMarkerGroup.Markers => Markers; 

        #endregion
    }

    public class GeometryRegion : IGeometryRegion
    {
        public string Name { get; set; }
        public List<IGeometryPermutation> Permutations { get; set; }

        public GeometryRegion()
        {
            Permutations = new List<IGeometryPermutation>();
        }

        public override string ToString() => Name;

        #region IGeometryRegion

        IReadOnlyList<IGeometryPermutation> IGeometryRegion.Permutations => Permutations; 

        #endregion
    }

    public class GeometryPermutation : IGeometryPermutation
    {
        public string Name { get; set; }
        public byte NodeIndex { get; set; }
        public int MeshIndex { get; set; }
        public int MeshCount { get; set; }

        public float TransformScale { get; set; }
        public Matrix4x4 Transform { get; set; }

        public override string ToString() => Name;
    }

    public class GeometryMarker : IGeometryMarker
    {
        public byte RegionIndex { get; set; }
        public byte PermutationIndex { get; set; }
        public byte NodeIndex { get; set; }
        public IRealVector3D Position { get; set; }
        public IRealVector4D Rotation { get; set; }
    }

    public class GeometryMesh : IGeometryMesh
    {
        public VertexWeights VertexWeights { get; set; }
        public IndexFormat IndexFormat { get; set; }

        public IVertex[] Vertices { get; set; }
        public int[] Indicies { get; set; }

        public short BoundsIndex { get; set; }
        public List<IGeometrySubmesh> Submeshes { get; set; }

        public GeometryMesh()
        {
            Submeshes = new List<IGeometrySubmesh>();
        }

        #region IGeometryMesh

        IReadOnlyList<IVertex> IGeometryMesh.Vertices => Vertices;

        IReadOnlyList<int> IGeometryMesh.Indicies => Indicies; 

        IReadOnlyList<IGeometrySubmesh> IGeometryMesh.Submeshes => Submeshes; 

        #endregion
    }

    public class GeometrySubmesh : IGeometrySubmesh
    {
        public short MaterialIndex { get; set; }
        public int IndexStart { get; set; }
        public int IndexLength { get; set; }
    }

    public class GeometryMaterial : IGeometryMaterial
    {
        public string Name { get; set; }

        public IBitmap Diffuse { get; set; }

        public IRealVector2D Tiling { get; set; }
    }

    public enum VertexWeights
    {
        None,
        Rigid,
        Skinned
    }

    public enum IndexFormat
    {
        Triangles = 3,
        Stripped = 5
    }
}
