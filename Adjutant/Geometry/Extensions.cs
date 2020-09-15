using Adjutant.Spatial;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Endian;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Geometry
{
    public static class Extensions
    {
        public static IEnumerable<int> Unstrip(this IEnumerable<int> strip)
        {
            int position = 0;
            int i0 = 0, i1 = 0, i2 = 0;

            foreach (int index in strip)
            {
                i0 = i1;
                i1 = i2;
                i2 = index;

                if (position++ < 2)
                    continue;

                if (i0 != i1 && i0 != i2 && i1 != i2)
                {
                    yield return i0;

                    if (position % 2 == 1)
                    {
                        yield return i1;
                        yield return i2;
                    }
                    else
                    {
                        yield return i2;
                        yield return i1;
                    }
                }
            }
        }

        public static IEnumerable<float> AsEnumerable(this IXMVector vector)
        {
            yield return vector.X;
            yield return vector.Y;
            yield return vector.Z;
            yield return vector.W;
        }

        public static Matrix4x4 AsTransform(this IRealBounds5D bounds)
        {
            return new Matrix4x4
            {
                M11 = bounds.XBounds.Length,
                M22 = bounds.YBounds.Length,
                M33 = bounds.ZBounds.Length,
                M41 = bounds.XBounds.Min,
                M42 = bounds.YBounds.Min,
                M43 = bounds.ZBounds.Min
            };
        }

        public static Matrix4x4 AsTextureTransform(this IRealBounds5D bounds)
        {
            return new Matrix4x4
            {
                M11 = bounds.UBounds.Length,
                M22 = bounds.VBounds.Length,
                M41 = bounds.UBounds.Min,
                M42 = bounds.VBounds.Min
            };
        }

        private class MultiMesh : IGeometryMesh
        {
            private readonly IGeometryModel model;
            private readonly int meshIndex;
            private readonly int meshCount;

            private IEnumerable<IGeometryMesh> AllMeshes => model.Meshes.Skip(meshIndex).Take(meshCount);

            public MultiMesh(IGeometryModel model, int meshIndex, int meshCount)
            {
                if (model == null)
                    throw new ArgumentNullException(nameof(model));

                if (meshIndex < 0 || meshIndex >= model.Meshes.Count)
                    throw new ArgumentOutOfRangeException(nameof(meshIndex));

                if (meshCount < 1 || meshIndex + meshCount > model.Meshes.Count)
                    throw new ArgumentOutOfRangeException(nameof(meshCount));

                this.model = model;
                this.meshIndex = meshIndex;
                this.meshCount = meshCount;
            }

            public bool IsInstancing => false;

            public short? BoundsIndex => model.Meshes[meshIndex].BoundsIndex;

            public byte? NodeIndex => model.Meshes[meshIndex].NodeIndex;

            public IndexFormat IndexFormat => meshCount > 1 ? IndexFormat.TriangleList : model.Meshes[meshIndex].IndexFormat;

            public VertexWeights VertexWeights => model.Meshes[meshIndex].VertexWeights;

            private List<GeometrySubmesh> mergedSubmeshes;
            public IReadOnlyList<IGeometrySubmesh> Submeshes
            {
                get
                {
                    if (meshCount == 1)
                        return model.Meshes[meshIndex].Submeshes;

                    if (mergedSubmeshes == null)
                    {
                        mergedSubmeshes = new List<GeometrySubmesh>();

                        var offset = 0;
                        foreach (var mesh in AllMeshes)
                        {
                            foreach (var sm in mesh.Submeshes)
                            {
                                IEnumerable<int> indices = mesh.Indicies.Skip(sm.IndexStart).Take(sm.IndexLength);
                                if (mesh.IndexFormat == IndexFormat.TriangleStrip)
                                    indices = indices.Unstrip();

                                var newSubmesh = new GeometrySubmesh
                                {
                                    MaterialIndex = sm.MaterialIndex,
                                    IndexStart = offset,
                                    IndexLength = indices.Count()
                                };

                                mergedSubmeshes.Add(newSubmesh);
                                offset += newSubmesh.IndexLength;
                            }
                        }
                    }

                    return mergedSubmeshes;
                }
            }

            private List<int> mergedIndices;
            public IReadOnlyList<int> Indicies
            {
                get
                {
                    if (meshCount == 1)
                        return model.Meshes[meshIndex].Indicies;

                    if (mergedIndices == null)
                    {
                        mergedIndices = new List<int>();

                        var offset = 0;
                        foreach (var mesh in AllMeshes)
                        {
                            IEnumerable<int> indices = mesh.IndexFormat == IndexFormat.TriangleStrip
                                ? mesh.Indicies.Unstrip()
                                : mesh.Indicies;

                            mergedIndices.AddRange(indices.Select(i => offset + i));
                            offset += mesh.Vertices.Count;
                        }
                    }

                    return mergedIndices;
                }
            }

            private List<IVertex> mergedVertices;
            public IReadOnlyList<IVertex> Vertices
            {
                get
                {
                    if (meshCount == 1)
                        return model.Meshes[meshIndex].Vertices;

                    if (mergedVertices == null)
                        mergedVertices = AllMeshes.SelectMany(m => m.Vertices).ToList();

                    return mergedVertices;
                }
            }

            public void Dispose()
            {
                mergedIndices.Clear();
                mergedVertices.Clear();
                mergedSubmeshes.Clear();
            }
        }

        public static void WriteAMF(this IGeometryModel model, string fileName, float scale)
        {
            if (!Directory.GetParent(fileName).Exists) Directory.GetParent(fileName).Create();
            if (!fileName.EndsWith(".amf", StringComparison.CurrentCultureIgnoreCase)) fileName += ".amf";

            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            using (var bw = new EndianWriter(fs, ByteOrder.LittleEndian))
            {
                var dupeDic = new Dictionary<int, long>();

                var validRegions = model.Regions
                    .Select(r => new { r.Name, Permutations = r.Permutations.Where(p => model.Meshes[p.MeshIndex].Submeshes.Count > 0).ToList() })
                    .Where(r => r.Permutations.Count > 0)
                    .ToList();

                var fauxMeshes = validRegions.SelectMany(r => r.Permutations)
                    .Where(p => p.MeshCount > 1)
                    .GroupBy(p => p.MeshIndex)
                    .ToDictionary(g => g.Key, g => new MultiMesh(model, g.Key, g.First().MeshCount));

                #region Address Lists
                var headerAddressList = new List<long>();
                var headerValueList = new List<long>();

                var markerAddressList = new List<long>();
                var markerValueList = new List<long>();

                var permAddressList = new List<long>();
                var permValueList = new List<long>();

                var vertAddressList = new List<long>();
                var vertValueList = new List<long>();

                var indxAddressList = new List<long>();
                var indxValueList = new List<long>();

                var meshAddressList = new List<long>();
                var meshValueList = new List<long>();
                #endregion

                #region Header
                bw.Write("AMF!".ToCharArray());
                bw.Write(2.0f);
                bw.WriteStringNullTerminated(model.Name ?? string.Empty);

                bw.Write(model.Nodes.Count);
                headerAddressList.Add(bw.BaseStream.Position);
                bw.Write(0);

                bw.Write(model.MarkerGroups.Count);
                headerAddressList.Add(bw.BaseStream.Position);
                bw.Write(0);

                bw.Write(validRegions.Count);
                headerAddressList.Add(bw.BaseStream.Position);
                bw.Write(0);

                bw.Write(model.Materials.Count);
                headerAddressList.Add(bw.BaseStream.Position);
                bw.Write(0);
                #endregion

                #region Nodes
                headerValueList.Add(bw.BaseStream.Position);
                foreach (var node in model.Nodes)
                {
                    bw.WriteStringNullTerminated(node.Name);
                    bw.Write(node.ParentIndex);
                    bw.Write(node.FirstChildIndex);
                    bw.Write(node.NextSiblingIndex);
                    bw.Write(node.Position.X * scale);
                    bw.Write(node.Position.Y * scale);
                    bw.Write(node.Position.Z * scale);
                    bw.Write(node.Rotation.X);
                    bw.Write(node.Rotation.Y);
                    bw.Write(node.Rotation.Z);
                    bw.Write(node.Rotation.W);
                }
                #endregion

                #region Marker Groups
                headerValueList.Add(bw.BaseStream.Position);
                foreach (var group in model.MarkerGroups)
                {
                    bw.WriteStringNullTerminated(group.Name);
                    bw.Write(group.Markers.Count);
                    markerAddressList.Add(bw.BaseStream.Position);
                    bw.Write(0);
                }
                #endregion

                #region Markers
                foreach (var group in model.MarkerGroups)
                {
                    markerValueList.Add(bw.BaseStream.Position);
                    foreach (var marker in group.Markers)
                    {
                        bw.Write(marker.RegionIndex);
                        bw.Write(marker.PermutationIndex);
                        bw.Write((short)marker.NodeIndex);
                        bw.Write(marker.Position.X * scale);
                        bw.Write(marker.Position.Y * scale);
                        bw.Write(marker.Position.Z * scale);
                        bw.Write(marker.Rotation.X);
                        bw.Write(marker.Rotation.Y);
                        bw.Write(marker.Rotation.Z);
                        bw.Write(marker.Rotation.W);
                    }
                }
                #endregion

                #region Regions
                headerValueList.Add(bw.BaseStream.Position);
                foreach (var region in validRegions)
                {
                    bw.WriteStringNullTerminated(region.Name);
                    bw.Write(region.Permutations.Count);
                    permAddressList.Add(bw.BaseStream.Position);
                    bw.Write(0);
                }
                #endregion

                #region Permutations
                foreach (var region in validRegions)
                {
                    permValueList.Add(bw.BaseStream.Position);
                    foreach (var perm in region.Permutations)
                    {
                        var part = fauxMeshes.ContainsKey(perm.MeshIndex)
                            ? fauxMeshes[perm.MeshIndex]
                            : model.Meshes[perm.MeshIndex];

                        bw.WriteStringNullTerminated(perm.Name);
                        bw.Write((byte)part.VertexWeights);
                        bw.Write(part.NodeIndex ?? byte.MaxValue);

                        bw.Write(part.Vertices.Count);
                        vertAddressList.Add(bw.BaseStream.Position);
                        bw.Write(0);

                        int count = 0;
                        foreach (var submesh in part.Submeshes)
                        {
                            var indices = part.Indicies.Skip(submesh.IndexStart).Take(submesh.IndexLength);
                            if (part.IndexFormat == IndexFormat.TriangleStrip)
                                indices = Unstrip(indices);

                            count += indices.Count() / 3;
                        }

                        bw.Write(count);
                        indxAddressList.Add(bw.BaseStream.Position);
                        bw.Write(0);

                        bw.Write(part.Submeshes.Count);
                        meshAddressList.Add(bw.BaseStream.Position);
                        bw.Write(0);

                        if (perm.Transform.IsIdentity && perm.TransformScale == 1)
                            bw.Write(float.NaN);
                        else
                        {
                            bw.Write(perm.TransformScale);
                            bw.Write(perm.Transform.M11);
                            bw.Write(perm.Transform.M12);
                            bw.Write(perm.Transform.M13);
                            bw.Write(perm.Transform.M21);
                            bw.Write(perm.Transform.M22);
                            bw.Write(perm.Transform.M23);
                            bw.Write(perm.Transform.M31);
                            bw.Write(perm.Transform.M32);
                            bw.Write(perm.Transform.M33);
                            bw.Write(perm.Transform.M41);
                            bw.Write(perm.Transform.M42);
                            bw.Write(perm.Transform.M43);
                        }
                    }
                }
                #endregion

                #region Vertices
                var emptyVector = new RealVector3D();

                foreach (var region in validRegions)
                {
                    foreach (var perm in region.Permutations)
                    {
                        var part = fauxMeshes.ContainsKey(perm.MeshIndex)
                            ? fauxMeshes[perm.MeshIndex]
                            : model.Meshes[perm.MeshIndex];

                        var scale1 = perm.Transform.IsIdentity && perm.TransformScale == 1 ? scale : 1;

                        long address;
                        if (dupeDic.TryGetValue(perm.MeshIndex, out address))
                        {
                            vertValueList.Add(address);
                            continue;
                        }
                        else dupeDic.Add(perm.MeshIndex, bw.BaseStream.Position);

                        vertValueList.Add(bw.BaseStream.Position);

                        IXMVector vector;
                        var vertices = part.BoundsIndex >= 0 ? part.Vertices.Select(v => (IVertex)new CompressedVertex(v, model.Bounds[part.BoundsIndex.Value])) : part.Vertices;
                        foreach (var vert in vertices)
                        {
                            vector = vert.Position.Count > 0 ? vert.Position[0] : emptyVector;
                            bw.Write(vector.X * scale1);
                            bw.Write(vector.Y * scale1);
                            bw.Write(vector.Z * scale1);

                            vector = vert.Normal.Count > 0 ? vert.Normal[0] : emptyVector;
                            bw.Write(vector.X);
                            bw.Write(vector.Y);
                            bw.Write(vector.Z);

                            vector = vert.TexCoords.Count > 0 ? vert.TexCoords[0] : emptyVector;
                            bw.Write(vector.X);
                            bw.Write(1 - vector.Y);

                            if (part.VertexWeights == VertexWeights.Rigid)
                            {
                                IXMVector i;
                                var indices = new List<int>();
                                i = vert.BlendIndices.Count > 0 ? vert.BlendIndices[0] : emptyVector;

                                if (!indices.Contains((int)i.X) && i.X != 0) indices.Add((int)i.X);
                                if (!indices.Contains((int)i.Y) && i.X != 0) indices.Add((int)i.Y);
                                if (!indices.Contains((int)i.Z) && i.X != 0) indices.Add((int)i.Z);
                                if (!indices.Contains((int)i.W) && i.X != 0) indices.Add((int)i.W);

                                if (indices.Count == 0) indices.Add(0);

                                foreach (int index in indices) bw.Write((byte)index);

                                if (indices.Count < 4) bw.Write(byte.MaxValue);
                            }
                            else if (part.VertexWeights == VertexWeights.Skinned)
                            {
                                var indices = (vert.BlendIndices.Count > 0 ? vert.BlendIndices[0] : emptyVector).AsEnumerable().ToArray();
                                var weights = (vert.BlendWeight.Count > 0 ? vert.BlendWeight[0] : emptyVector).AsEnumerable().ToArray();

                                var count = weights.Count(w => w > 0);

                                if (count == 0)
                                {
                                    bw.Write((byte)0);
                                    bw.Write((byte)255);
                                    bw.Write(0);
                                    continue;
                                    //throw new Exception("no weights on a weighted node. report this.");
                                }

                                for (int i = 0; i < 4; i++)
                                {
                                    if (weights[i] > 0)
                                        bw.Write((byte)indices[i]);
                                }

                                if (count != 4) bw.Write(byte.MaxValue);

                                foreach (var w in weights.Where(w => w > 0))
                                    bw.Write(w);
                            }
                        }
                    }
                }
                #endregion

                #region Indices
                dupeDic.Clear();
                foreach (var region in validRegions)
                {
                    foreach (var perm in region.Permutations)
                    {
                        var part = fauxMeshes.ContainsKey(perm.MeshIndex)
                            ? fauxMeshes[perm.MeshIndex]
                            : model.Meshes[perm.MeshIndex];

                        long address;
                        if (dupeDic.TryGetValue(perm.MeshIndex, out address))
                        {
                            indxValueList.Add(address);
                            continue;
                        }
                        else dupeDic.Add(perm.MeshIndex, bw.BaseStream.Position);

                        indxValueList.Add(bw.BaseStream.Position);

                        foreach (var submesh in part.Submeshes)
                        {
                            var indices = part.Indicies.Skip(submesh.IndexStart).Take(submesh.IndexLength);
                            if (part.IndexFormat == IndexFormat.TriangleStrip)
                                indices = Unstrip(indices);

                            foreach (var index in indices)
                            {
                                if (part.Vertices.Count > ushort.MaxValue) bw.Write(index);
                                else bw.Write((ushort)index);
                            }
                        }
                    }
                }
                #endregion

                #region Submeshes
                foreach (var region in validRegions)
                {
                    foreach (var perm in region.Permutations)
                    {
                        meshValueList.Add(bw.BaseStream.Position);

                        var part = fauxMeshes.ContainsKey(perm.MeshIndex)
                            ? fauxMeshes[perm.MeshIndex]
                            : model.Meshes[perm.MeshIndex];

                        int currentPosition = 0;
                        foreach (var mesh in part.Submeshes)
                        {
                            var indices = part.Indicies.Skip(mesh.IndexStart).Take(mesh.IndexLength);
                            if (part.IndexFormat == IndexFormat.TriangleStrip)
                                indices = Unstrip(indices);

                            var faceCount = indices.Count() / 3;

                            bw.Write(mesh.MaterialIndex);
                            bw.Write(currentPosition);
                            bw.Write(faceCount);

                            currentPosition += faceCount;
                        }
                    }
                }
                #endregion

                #region Shaders
                headerValueList.Add(bw.BaseStream.Position);
                foreach (var material in model.Materials)
                {
                    const string nullPath = "null";

                    //skip null shaders
                    if (material == null)
                    {
                        bw.WriteStringNullTerminated(nullPath);
                        for (int i = 0; i < 8; i++)
                            bw.WriteStringNullTerminated(nullPath);

                        for (int i = 0; i < 4; i++)
                            bw.Write(0);

                        bw.Write(Convert.ToByte(false));
                        bw.Write(Convert.ToByte(false));

                        continue;
                    }

                    if (material.Flags.HasFlag(MaterialFlags.TerrainBlend))
                    {
                        bw.WriteStringNullTerminated("*" + material.Name);

                        var blendInfo = material.Submaterials.FirstOrDefault(s => s.Usage == MaterialUsage.BlendMap);
                        var baseInfo = material.Submaterials.Where(s => s.Usage == MaterialUsage.Diffuse).ToList();
                        var bumpInfo = material.Submaterials.Where(s => s.Usage == MaterialUsage.Normal).ToList();
                        var detailInfo = material.Submaterials.Where(s => s.Usage == MaterialUsage.DiffuseDetail).ToList();

                        if (blendInfo == null)
                            bw.WriteStringNullTerminated(nullPath);
                        else
                        {
                            bw.WriteStringNullTerminated(blendInfo.Bitmap.Name);
                            bw.Write(blendInfo.Tiling.X);
                            bw.Write(blendInfo.Tiling.Y);
                        }

                        bw.Write((byte)baseInfo.Count);
                        bw.Write((byte)bumpInfo.Count);
                        bw.Write((byte)detailInfo.Count);

                        foreach (var info in baseInfo.Concat(bumpInfo).Concat(detailInfo))
                        {
                            bw.WriteStringNullTerminated(info.Bitmap.Name);
                            bw.Write(info.Tiling.X);
                            bw.Write(info.Tiling.Y);
                        }
                    }
                    else
                    {
                        bw.WriteStringNullTerminated(material.Name);
                        for (int i = 0; i < 8; i++)
                        {
                            var submat = material.Submaterials.FirstOrDefault(s => s.Usage == (MaterialUsage)i);
                            bw.WriteStringNullTerminated(submat?.Bitmap.Name ?? nullPath);
                            if (submat != null)
                            {
                                bw.Write(submat.Tiling.X);
                                bw.Write(submat.Tiling.Y);
                            }
                        }

                        for (int i = 0; i < 4; i++)
                        {
                            var tint = material.TintColours.FirstOrDefault(t => t.Usage == (TintUsage)i);
                            if (tint == null)
                            {
                                bw.Write(0);
                                continue;
                            }

                            bw.Write(tint.R);
                            bw.Write(tint.G);
                            bw.Write(tint.B);
                            bw.Write(tint.A);
                        }

                        bw.Write(Convert.ToByte(material.Flags.HasFlag(MaterialFlags.Transparent)));
                        bw.Write(Convert.ToByte(material.Flags.HasFlag(MaterialFlags.ColourChange)));
                    }
                }
                #endregion

                #region Write Addresses
                for (int i = 0; i < headerAddressList.Count; i++)
                {
                    bw.BaseStream.Position = headerAddressList[i];
                    bw.Write((int)headerValueList[i]);
                }

                for (int i = 0; i < markerAddressList.Count; i++)
                {
                    bw.BaseStream.Position = markerAddressList[i];
                    bw.Write((int)markerValueList[i]);
                }

                for (int i = 0; i < permAddressList.Count; i++)
                {
                    bw.BaseStream.Position = permAddressList[i];
                    bw.Write((int)permValueList[i]);
                }

                for (int i = 0; i < vertAddressList.Count; i++)
                {
                    bw.BaseStream.Position = vertAddressList[i];
                    bw.Write((int)vertValueList[i]);
                }

                for (int i = 0; i < indxAddressList.Count; i++)
                {
                    bw.BaseStream.Position = indxAddressList[i];
                    bw.Write((int)indxValueList[i]);
                }

                for (int i = 0; i < meshAddressList.Count; i++)
                {
                    bw.BaseStream.Position = meshAddressList[i];
                    bw.Write((int)meshValueList[i]);
                }
                #endregion
            }
        }

        public static void WriteJMS(this IGeometryModel model, string fileName, float scale)
        {
            const string float4 = "{0:F6}\t{1:F6}\t{2:F6}\t{3:F6}";
            const string float3 = "{0:F6}\t{1:F6}\t{2:F6}";
            const string float1 = "{0:F6}";

            var modelName = Path.GetFileNameWithoutExtension(fileName);
            var directory = Path.Combine(Directory.GetParent(fileName).FullName, "models");
            Directory.CreateDirectory(directory);

            var permNames = model.Regions.SelectMany(r => r.Permutations)
                .Select(p => p.Name)
                .Distinct();

            foreach (var permName in permNames)
            {
                var allRegions = model.Regions.Where(r => r.Permutations.Any(p => p.Name == permName)).ToList();
                var allPerms = model.Regions.SelectMany(r => r.Permutations).Where(p => p.Name == permName).ToList();

                using (var sw = new StreamWriter(Path.Combine(directory, permName + ".jms")))
                {
                    sw.WriteLine("8200");
                    sw.WriteLine("14689795");

                    sw.WriteLine(model.Nodes.Count);
                    foreach (var node in model.Nodes)
                    {
                        sw.WriteLine(node.Name);
                        sw.WriteLine(node.FirstChildIndex);
                        sw.WriteLine(node.NextSiblingIndex);
                        sw.WriteLine("{0}\t{1}\t{2}\t{3}", -node.Rotation.X, -node.Rotation.Y, -node.Rotation.Z, node.Rotation.W);
                        sw.WriteLine(float3, node.Position.X * scale, node.Position.Y * scale, node.Position.Z * scale);
                    }

                    sw.WriteLine(model.Materials.Count);
                    foreach (var mat in model.Materials)
                    {
                        sw.WriteLine(mat?.Name ?? "unused");
                        sw.WriteLine("<none>"); //unknown
                    }

                    sw.WriteLine(model.MarkerGroups.Sum(g => g.Markers.Count));
                    foreach (var group in model.MarkerGroups)
                    {
                        foreach (var marker in group.Markers)
                        {
                            sw.WriteLine(group.Name);
                            sw.WriteLine(-1); //unknown
                            sw.WriteLine(marker.NodeIndex);
                            sw.WriteLine(float4, marker.Rotation.X, marker.Rotation.Y, marker.Rotation.Z, marker.Rotation.W);
                            sw.WriteLine(float3, marker.Position.X * scale, marker.Position.Y * scale, marker.Position.Z * scale);
                            sw.WriteLine(1); //radius
                        }
                    }

                    sw.WriteLine(allRegions.Count);
                    foreach (var region in allRegions)
                        sw.WriteLine(region.Name);

                    #region Vertices
                    var emptyVector = new RealVector3D();

                    sw.WriteLine(allPerms.SelectMany(p => model.Meshes.Skip(p.MeshIndex).Take(p.MeshCount)).Sum(m => m.Vertices.Count));
                    foreach (var perm in allPerms)
                    {
                        var mesh = model.Meshes[perm.MeshIndex];
                        foreach (var vert in mesh.Vertices)
                        {
                            var decompressed = vert;
                            if (mesh.BoundsIndex >= 0)
                                decompressed = new CompressedVertex(vert, model.Bounds[mesh.BoundsIndex.Value]);

                            var pos = decompressed.Position.FirstOrDefault() ?? emptyVector;
                            var norm = vert.Normal.FirstOrDefault() ?? emptyVector;
                            var tex = decompressed.TexCoords.FirstOrDefault() ?? emptyVector;
                            var weights = decompressed.BlendWeight.FirstOrDefault() ?? emptyVector;
                            var nodes = decompressed.BlendIndices.FirstOrDefault() ?? emptyVector;

                            var node1 = nodes.X;
                            if (mesh.NodeIndex < byte.MaxValue)
                                node1 = mesh.NodeIndex.Value;

                            sw.WriteLine("{0:F0}", node1);

                            sw.WriteLine(float3, pos.X * scale, pos.Y * scale, pos.Z * scale);
                            sw.WriteLine(float3, norm.X, norm.Y, norm.Z);

                            sw.WriteLine(nodes.Y);
                            sw.WriteLine(float1, weights.Y);

                            sw.WriteLine(float1, tex.X);
                            sw.WriteLine(float1, 1 - tex.Y);
                            sw.WriteLine(float1, 0f);
                        }
                    }
                    #endregion

                    #region Triangles
                    var totalEdges = allPerms.SelectMany(p =>
                    {
                        var mesh = model.Meshes[p.MeshIndex];
                        if (mesh.IndexFormat == IndexFormat.TriangleList)
                            return mesh.Indicies;
                        else return mesh.Submeshes.SelectMany(s => mesh.Indicies.Skip(s.IndexStart).Take(s.IndexLength).Unstrip());
                    }).Count();

                    sw.WriteLine(totalEdges / 3);
                    int offset = 0;
                    foreach (var perm in allPerms)
                    {
                        var regIndex = allRegions.TakeWhile(r => !r.Permutations.Contains(perm)).Count();
                        var mesh = model.Meshes[perm.MeshIndex];
                        foreach (var sub in mesh.Submeshes)
                        {
                            var temp = mesh.Indicies.Skip(sub.IndexStart).Take(sub.IndexLength);
                            if (mesh.IndexFormat == IndexFormat.TriangleStrip)
                                temp = temp.Unstrip();

                            var indices = temp.ToList();
                            for (int i = 0; i < indices.Count; i += 3)
                            {
                                sw.WriteLine(regIndex);
                                sw.WriteLine(sub.MaterialIndex);
                                sw.WriteLine("{0}\t{1}\t{2}", offset + indices[i], offset + indices[i + 1], offset + indices[i + 2]);
                            }
                        }
                        offset += mesh.Vertices.Count;
                    }
                    #endregion
                }
            }
        }
    }
}
