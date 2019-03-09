using Adjutant.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Geometry
{
    public class Model
    {
        public Matrix4x4 CoordinateSystem { get; set; }

        public string Name { get; set; }
        public List<Node> Nodes { get; set; }
        public List<MarkerGroup> MarkerGroups { get; set; }
        public List<Region> Regions { get; set; }
        //public List<Shader> Shaders { get; set; }
        public List<IRealBounds5D> Bounds { get; set; }
        public List<Mesh> Meshes { get; set; }

        public Model()
        {
            Nodes = new List<Node>();
            MarkerGroups = new List<MarkerGroup>();
            Regions = new List<Region>();
            Bounds = new List<IRealBounds5D>();
            Meshes = new List<Mesh>();
        }
    }

    public class Node
    {
        public string Name { get; set; }
        public short ParentIndex { get; set; }
        public short FirstChildIndex { get; set; }
        public short NextSiblingIndex { get; set; }
        public IRealVector3D Position { get; set; }
        public IRealVector4D Rotation { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    public class MarkerGroup : List<Marker>
    {
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    public class Region
    {
        public string Name { get; set; }
        public List<Permutation> Permutations { get; set; }

        public Region()
        {
            Permutations = new List<Permutation>();
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class Permutation
    {
        public string Name { get; set; }
        public byte NodeIndex { get; set; }
        public short BoundsIndex { get; set; }
        public int MeshIndex { get; set; }

        public float TransformScale { get; set; }
        public Matrix4x4 Transform { get; set; }
        public List<Submesh> Submeshes { get; set; }

        public Permutation()
        {
            Submeshes = new List<Submesh>();
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class Submesh
    {
        public short MaterialIndex { get; set; }
        public int IndexStart { get; set; }
        public int IndexLength { get; set; }
        public int VertexStart { get; set; }
        public int VertexLength { get; set; }
    }

    public class Marker
    {
        public byte RegionIndex { get; set; }
        public byte PermutationIndex { get; set; }
        public byte NodeIndex { get; set; }
        public IRealVector3D Position { get; set; }
        public IRealVector4D Rotation { get; set; }
    }

    public class Mesh
    {
        public VertexWeights VertexWeights { get; set; }
        public IndexFormat IndexFormat { get; set; }

        public IXMVector[] Vertices { get; set; }
        public int[] Indicies { get; set; }
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
