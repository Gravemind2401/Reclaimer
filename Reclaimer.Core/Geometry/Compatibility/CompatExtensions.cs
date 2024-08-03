using Reclaimer.IO;
using System.IO;

namespace Reclaimer.Geometry.Compatibility
{
    [Obsolete("Backwards compatibility for AMF")]
    public static class CompatExtensions
    {
        public static void WriteAMF(this Scene scene, string fileName, float scale)
        {
            var model = scene.EnumerateGroupHierarchy().SelectMany(g => g.ChildObjects.OfType<Model>()).First();
            model.WriteAMF(fileName, scale);
        }

        public static void WriteJMS(this Scene scene, string fileName, float scale)
        {
            var model = scene.EnumerateGroupHierarchy().SelectMany(g => g.ChildObjects.OfType<Model>()).First();
            model.WriteJMS(fileName, scale);
        }

        public static void WriteAMF(this Model model, string fileName, float scale)
        {
            if (!Directory.GetParent(fileName).Exists)
                Directory.GetParent(fileName).Create();
            if (!fileName.EndsWith(".amf", StringComparison.CurrentCultureIgnoreCase))
                fileName += ".amf";

            IEnumerable<(string, ModelPermutation, Mesh, int)> ExpandPermutation(ModelPermutation perm)
            {
                var (index, count) = perm.MeshRange;
                for (var i = 0; i < count; i++)
                {
                    if (model.Meshes[index + i] == null)
                        continue;

                    var name = perm.Name;
                    if (count > 1)
                        name += i.ToString("D2");
                    yield return (name, perm, model.Meshes[index + i], index + i);
                }
            }

            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            using (var bw = new EndianWriter(fs, ByteOrder.LittleEndian))
            {
                var dupeDic = new Dictionary<int, long>();

                var allMaterials = model.EnumerateMaterials().ToList();

                var validRegions = model.Regions
                    .Where(r => r.Export)
                    .Select(r => new
                    {
                        r.Name,
                        Permutations = r.Permutations.Where(p => p.Export && p.MeshRange.Count > 0 && model.Meshes.ElementAtOrDefault(p.MeshRange.Index)?.Segments.Count > 0).ToList()
                    })
                    .Where(r => r.Permutations.Count > 0)
                    .ToList();

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

                bw.Write(model.Bones.Count);
                headerAddressList.Add(bw.BaseStream.Position);
                bw.Write(0);

                bw.Write(model.Markers.Count);
                headerAddressList.Add(bw.BaseStream.Position);
                bw.Write(0);

                bw.Write(validRegions.Count);
                headerAddressList.Add(bw.BaseStream.Position);
                bw.Write(0);

                bw.Write(allMaterials.Count);
                headerAddressList.Add(bw.BaseStream.Position);
                bw.Write(0);
                #endregion

                #region Nodes
                headerValueList.Add(bw.BaseStream.Position);
                foreach (var bone in model.Bones)
                {
                    bw.WriteStringNullTerminated(bone.Name);
                    bw.Write((short)bone.ParentIndex);
                    bw.Write((short)bone.FirstChildIndex);
                    bw.Write((short)bone.NextSiblingIndex);
                    bw.Write(bone.Position.X * scale);
                    bw.Write(bone.Position.Y * scale);
                    bw.Write(bone.Position.Z * scale);
                    bw.Write(bone.Rotation.X);
                    bw.Write(bone.Rotation.Y);
                    bw.Write(bone.Rotation.Z);
                    bw.Write(bone.Rotation.W);
                }
                #endregion

                #region Marker Groups
                headerValueList.Add(bw.BaseStream.Position);
                foreach (var group in model.Markers)
                {
                    bw.WriteStringNullTerminated(group.Name);
                    bw.Write(group.Instances.Count);
                    markerAddressList.Add(bw.BaseStream.Position);
                    bw.Write(0);
                }
                #endregion

                #region Markers
                foreach (var group in model.Markers)
                {
                    markerValueList.Add(bw.BaseStream.Position);
                    foreach (var marker in group.Instances)
                    {
                        bw.Write((byte)marker.RegionIndex);
                        bw.Write((byte)marker.PermutationIndex);
                        bw.Write((short)marker.BoneIndex);
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
                    bw.Write(region.Permutations.Sum(p => p.MeshRange.Count));
                    permAddressList.Add(bw.BaseStream.Position);
                    bw.Write(0);
                }
                #endregion

                #region Permutations
                foreach (var region in validRegions)
                {
                    permValueList.Add(bw.BaseStream.Position);
                    foreach (var (permName, perm, part, _) in region.Permutations.SelectMany(ExpandPermutation))
                    {
                        bw.WriteStringNullTerminated(permName);
                        bw.Write((byte)part.VertexWeights);
                        bw.Write(part.BoneIndex ?? byte.MaxValue);

                        bw.Write(part.VertexBuffer.Count);
                        vertAddressList.Add(bw.BaseStream.Position);
                        bw.Write(0);

                        var count = 0;
                        foreach (var submesh in part.Segments)
                        {
                            var indices = part.GetTriangleIndicies(submesh);
                            count += indices.Count() / 3;
                        }

                        bw.Write(count);
                        indxAddressList.Add(bw.BaseStream.Position);
                        bw.Write(0);

                        bw.Write(part.Segments.Count);
                        meshAddressList.Add(bw.BaseStream.Position);
                        bw.Write(0);

                        if (perm.Transform.IsIdentity)
                            bw.Write(float.NaN);
                        else
                        {
                            bw.Write(1f);
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
                foreach (var region in validRegions)
                {
                    foreach (var (permName, perm, part, meshIndex) in region.Permutations.SelectMany(ExpandPermutation))
                    {
                        var scale1 = perm.Transform.IsIdentity ? scale : 1;

                        if (dupeDic.TryGetValue(meshIndex, out var address))
                        {
                            vertValueList.Add(address);
                            continue;
                        }
                        else
                            dupeDic.Add(meshIndex, bw.BaseStream.Position);

                        vertValueList.Add(bw.BaseStream.Position);

                        var posTransform = part.PositionBounds.CreateExpansionMatrix();
                        var texTransform = part.TextureBounds.CreateExpansionMatrix();

                        var positions = part.GetPositions()?.Select(v => Vector3.Transform(v, posTransform) * scale1).ToList();
                        var texcoords = part.GetTexCoords()?.Select(v => Vector2.Transform(v, texTransform)).ToList();
                        var normals = part.GetNormals()?.ToList();
                        var blendIndices = part.GetBlendIndices()?.ToList();
                        var blendWeights = part.GetBlendWeights()?.ToList();

                        Vector3 vector;
                        Vector2 vector2;
                        for (var i = 0; i < part.VertexBuffer.Count; i++)
                        {
                            vector = positions?[i] ?? default;
                            bw.Write(vector.X);
                            bw.Write(vector.Y);
                            bw.Write(vector.Z);

                            vector = normals?[i] ?? default;
                            bw.Write(vector.X);
                            bw.Write(vector.Y);
                            bw.Write(vector.Z);

                            vector2 = texcoords?[i] ?? default;
                            bw.Write(vector2.X);
                            bw.Write(1 - vector2.Y);

                            if (part.VertexWeights == VertexWeightsCompat.None)
                                continue;

                            if (part.VertexWeights == VertexWeightsCompat.Rigid)
                            {
                                var indices = new List<int>();
                                var bi = blendIndices?[i] ?? default;

                                if (!indices.Contains((int)bi.X) && bi.X != 0)
                                    indices.Add((int)bi.X);
                                if (!indices.Contains((int)bi.Y) && bi.X != 0)
                                    indices.Add((int)bi.Y);
                                if (!indices.Contains((int)bi.Z) && bi.X != 0)
                                    indices.Add((int)bi.Z);
                                if (!indices.Contains((int)bi.W) && bi.X != 0)
                                    indices.Add((int)bi.W);

                                if (indices.Count == 0)
                                    indices.Add(0);

                                foreach (var index in indices)
                                    bw.Write((byte)index);

                                if (indices.Count < 4)
                                    bw.Write(byte.MaxValue);
                            }
                            else if (part.VertexWeights == VertexWeightsCompat.Skinned)
                            {
                                var temp = blendIndices?[i] ?? default;
                                var indices = new[] { temp.X, temp.Y, temp.Z, temp.W };

                                temp = blendWeights?[i] ?? default;
                                var weights = new[] { temp.X, temp.Y, temp.Z, temp.W };

                                var count = weights.Count(w => w > 0);

                                if (part.VertexBuffer.HasImpliedBlendWeights)
                                {
                                    //set the last weight to 1f then normalise
                                    weights[3] = 1;
                                    count++;
                                    var sum = weights.Sum();
                                    for (var bi = 0; bi < 4; bi++)
                                        weights[bi] /= sum;
                                }

                                if (count == 0)
                                {
                                    bw.Write((byte)0);
                                    bw.Write((byte)255);
                                    bw.Write(0);
                                    continue;
                                    //throw new Exception("no weights on a weighted node. report this.");
                                }

                                for (var bi = 0; bi < 4; bi++)
                                {
                                    if (weights[bi] > 0)
                                        bw.Write((byte)indices[bi]);
                                }

                                if (count != 4)
                                    bw.Write(byte.MaxValue);

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
                    foreach (var (permName, perm, part, meshIndex) in region.Permutations.SelectMany(ExpandPermutation))
                    {
                        if (dupeDic.TryGetValue(meshIndex, out var address))
                        {
                            indxValueList.Add(address);
                            continue;
                        }
                        else
                            dupeDic.Add(meshIndex, bw.BaseStream.Position);

                        indxValueList.Add(bw.BaseStream.Position);

                        foreach (var submesh in part.Segments)
                        {
                            var indices = part.GetTriangleIndicies(submesh);
                            foreach (var index in indices)
                            {
                                if (part.VertexBuffer.Count > ushort.MaxValue)
                                    bw.Write(index);
                                else
                                    bw.Write((ushort)index);
                            }
                        }
                    }
                }
                #endregion

                #region Submeshes
                foreach (var region in validRegions)
                {
                    foreach (var (permName, perm, part, _) in region.Permutations.SelectMany(ExpandPermutation))
                    {
                        meshValueList.Add(bw.BaseStream.Position);

                        var currentPosition = 0;
                        foreach (var mesh in part.Segments)
                        {
                            var indices = part.GetTriangleIndicies(mesh);
                            var faceCount = indices.Count() / 3;

                            bw.Write((short)allMaterials.IndexOf(mesh.Material));
                            bw.Write(currentPosition);
                            bw.Write(faceCount);

                            currentPosition += faceCount;
                        }
                    }
                }
                #endregion

                #region Shaders
                headerValueList.Add(bw.BaseStream.Position);
                foreach (var material in allMaterials)
                {
                    const string nullPath = "null";

                    //skip null shaders
                    if (material == null)
                    {
                        bw.WriteStringNullTerminated(nullPath);
                        for (var i = 0; i < 8; i++)
                            bw.WriteStringNullTerminated(nullPath);

                        for (var i = 0; i < 4; i++)
                            bw.Write(0);

                        bw.Write(Convert.ToByte(false));
                        bw.Write(Convert.ToByte(false));

                        continue;
                    }

                    if (material.LegacyFlags.HasFlag(MaterialFlagsCompat.TerrainBlend))
                    {
                        bw.WriteStringNullTerminated("*" + material.Name);

                        var blendInfo = material.TextureMappings.FirstOrDefault(s => MaterialUsageCompat.GetValue(s.Usage) == MaterialUsageCompat.BlendMap);
                        var baseInfo = material.TextureMappings.Where(s => MaterialUsageCompat.GetValue(s.Usage) == MaterialUsageCompat.Diffuse).ToList();
                        var bumpInfo = material.TextureMappings.Where(s => MaterialUsageCompat.GetValue(s.Usage) == MaterialUsageCompat.Normal).ToList();
                        var detailInfo = material.TextureMappings.Where(s => MaterialUsageCompat.GetValue(s.Usage) == MaterialUsageCompat.DiffuseDetail).ToList();

                        if (blendInfo == null)
                            bw.WriteStringNullTerminated(nullPath);
                        else
                        {
                            bw.WriteStringNullTerminated(blendInfo.Texture.Name);
                            bw.Write(blendInfo.Tiling.X);
                            bw.Write(blendInfo.Tiling.Y);
                        }

                        bw.Write((byte)baseInfo.Count);
                        bw.Write((byte)bumpInfo.Count);
                        bw.Write((byte)detailInfo.Count);

                        foreach (var info in baseInfo.Concat(bumpInfo).Concat(detailInfo))
                        {
                            bw.WriteStringNullTerminated(info.Texture.Name);
                            bw.Write(info.Tiling.X);
                            bw.Write(info.Tiling.Y);
                        }
                    }
                    else
                    {
                        bw.WriteStringNullTerminated(material.Name);
                        for (var i = 0; i < 8; i++)
                        {
                            var submat = material.TextureMappings.FirstOrDefault(s => MaterialUsageCompat.GetValue(s.Usage) == i);
                            bw.WriteStringNullTerminated(submat?.Texture.Name ?? nullPath);
                            if (submat != null)
                            {
                                bw.Write(submat.Tiling.X);
                                bw.Write(submat.Tiling.Y);
                            }
                        }

                        for (var i = 0; i < 4; i++)
                        {
                            var tint = material.Tints.FirstOrDefault(t => TintUsageCompat.GetValue(t.Usage) == i);
                            if (tint == null)
                            {
                                bw.Write(0);
                                continue;
                            }

                            bw.Write(tint.Color.R);
                            bw.Write(tint.Color.G);
                            bw.Write(tint.Color.B);
                            bw.Write(tint.Color.A);
                        }

                        bw.Write(Convert.ToByte(material.LegacyFlags.HasFlag(MaterialFlagsCompat.Transparent)));
                        bw.Write(Convert.ToByte(material.LegacyFlags.HasFlag(MaterialFlagsCompat.ColourChange)));
                    }
                }
                #endregion

                #region Write Addresses
                for (var i = 0; i < headerAddressList.Count; i++)
                {
                    bw.BaseStream.Position = headerAddressList[i];
                    bw.Write((int)headerValueList[i]);
                }

                for (var i = 0; i < markerAddressList.Count; i++)
                {
                    bw.BaseStream.Position = markerAddressList[i];
                    bw.Write((int)markerValueList[i]);
                }

                for (var i = 0; i < permAddressList.Count; i++)
                {
                    bw.BaseStream.Position = permAddressList[i];
                    bw.Write((int)permValueList[i]);
                }

                for (var i = 0; i < vertAddressList.Count; i++)
                {
                    bw.BaseStream.Position = vertAddressList[i];
                    bw.Write((int)vertValueList[i]);
                }

                for (var i = 0; i < indxAddressList.Count; i++)
                {
                    bw.BaseStream.Position = indxAddressList[i];
                    bw.Write((int)indxValueList[i]);
                }

                for (var i = 0; i < meshAddressList.Count; i++)
                {
                    bw.BaseStream.Position = meshAddressList[i];
                    bw.Write((int)meshValueList[i]);
                }
                #endregion
            }
        }

        public static void WriteJMS(this Model model, string fileName, float scale)
        {
            const string float4 = "{0:F6}\t{1:F6}\t{2:F6}\t{3:F6}";
            const string float3 = "{0:F6}\t{1:F6}\t{2:F6}";
            const string float1 = "{0:F6}";

            var modelName = Path.GetFileNameWithoutExtension(fileName);
            var directory = Path.Combine(Directory.GetParent(fileName).FullName, "models");
            Directory.CreateDirectory(directory);

            var permNames = model.Regions
                .Where(r => r.Export)
                .SelectMany(r => r.Permutations)
                .Where(p => p.Export)
                .Select(p => p.Name)
                .Distinct();

            var allMaterials = model.Meshes
                .SelectMany(m => m.Segments)
                .Select(s => s.Material)
                .Distinct().ToList();

            foreach (var permName in permNames)
            {
                var allRegions = model.Regions.Where(r => r.Export && r.Permutations.Any(p => p.Export && p.Name == permName)).ToList();
                var allPerms = model.Regions.Where(r => r.Export).SelectMany(r => r.Permutations).Where(p => p.Export && p.Name == permName).ToList();

                using (var sw = new StreamWriter(Path.Combine(directory, permName + ".jms")))
                {
                    sw.WriteLine("8200");
                    sw.WriteLine("14689795");

                    sw.WriteLine(model.Bones.Count);
                    foreach (var node in model.Bones)
                    {
                        sw.WriteLine(node.Name);
                        sw.WriteLine(node.FirstChildIndex);
                        sw.WriteLine(node.NextSiblingIndex);
                        sw.WriteLine("{0}\t{1}\t{2}\t{3}", -node.Rotation.X, -node.Rotation.Y, -node.Rotation.Z, node.Rotation.W);
                        sw.WriteLine(float3, node.Position.X * scale, node.Position.Y * scale, node.Position.Z * scale);
                    }

                    sw.WriteLine(allMaterials.Count);
                    foreach (var mat in allMaterials)
                    {
                        sw.WriteLine(mat?.Name ?? "unused");
                        sw.WriteLine("<none>"); //unknown
                    }

                    sw.WriteLine(model.Markers.Sum(g => g.Instances.Count));
                    foreach (var group in model.Markers)
                    {
                        foreach (var marker in group.Instances)
                        {
                            sw.WriteLine(group.Name);
                            sw.WriteLine(-1); //unknown
                            sw.WriteLine(marker.BoneIndex);
                            sw.WriteLine(float4, marker.Rotation.X, marker.Rotation.Y, marker.Rotation.Z, marker.Rotation.W);
                            sw.WriteLine(float3, marker.Position.X * scale, marker.Position.Y * scale, marker.Position.Z * scale);
                            sw.WriteLine(1); //radius
                        }
                    }

                    sw.WriteLine(allRegions.Count);
                    foreach (var region in allRegions)
                        sw.WriteLine(region.Name);

                    #region Vertices
                    sw.WriteLine(allPerms.SelectMany(p => p.MeshIndices.Select(i => model.Meshes[i])).Sum(m => m.VertexBuffer.Count));
                    foreach (var perm in allPerms)
                    {
                        var mesh = model.Meshes[perm.MeshRange.Index];

                        var posTransform = mesh.PositionBounds.CreateExpansionMatrix();
                        var texTransform = mesh.TextureBounds.CreateExpansionMatrix();

                        var positions = mesh.GetPositions()?.Select(v => Vector3.Transform(v, posTransform) * scale).ToList();
                        var texcoords = mesh.GetTexCoords()?.Select(v => Vector2.Transform(v, texTransform)).ToList();
                        var normals = mesh.GetNormals()?.ToList();
                        var blendIndices = mesh.GetBlendIndices()?.ToList();
                        var blendWeights = mesh.GetBlendWeights()?.ToList();

                        for (var i = 0; i < mesh.VertexBuffer.Count; i++)
                        {
                            var pos = positions?[i] ?? default;
                            var norm = normals?[i] ?? default;
                            var tex = texcoords?[i] ?? default;
                            var weights = blendIndices?[i] ?? default;
                            var nodes = blendWeights?[i] ?? default;

                            var node1 = nodes.X;
                            if (mesh.BoneIndex < byte.MaxValue)
                                node1 = mesh.BoneIndex.Value;

                            sw.WriteLine("{0:F0}", node1);

                            sw.WriteLine(float3, pos.X, pos.Y, pos.Z);
                            sw.WriteLine(float3, norm.X, norm.Y, norm.Z);

                            sw.WriteLine(nodes.Y);
                            sw.WriteLine(float1, weights.Y);

                            sw.WriteLine(float1, tex.X);
                            sw.WriteLine(float1, 1 - tex.Y);
                            sw.WriteLine(0);
                        }
                    }
                    #endregion

                    #region Triangles
                    var totalEdges = allPerms.SelectMany(p =>
                    {
                        var mesh = model.Meshes[p.MeshRange.Index];
                        return mesh.IndexBuffer.Layout == IndexFormat.TriangleList
                            ? mesh.IndexBuffer
                            : mesh.Segments.SelectMany(s => mesh.IndexBuffer.GetSubset(s.IndexStart, s.IndexLength).Unstrip());
                    }).Count();

                    sw.WriteLine(totalEdges / 3);
                    var offset = 0;
                    foreach (var perm in allPerms)
                    {
                        var regIndex = allRegions.TakeWhile(r => !r.Permutations.Contains(perm)).Count();
                        var mesh = model.Meshes[perm.MeshRange.Index];
                        foreach (var sub in mesh.Segments)
                        {
                            var indices = mesh.GetTriangleIndicies(sub).ToList();
                            for (var i = 0; i < indices.Count; i += 3)
                            {
                                sw.WriteLine(regIndex);
                                sw.WriteLine(allMaterials.IndexOf(sub.Material));
                                sw.WriteLine("{0}\t{1}\t{2}", offset + indices[i], offset + indices[i + 1], offset + indices[i + 2]);
                            }
                        }
                        offset += mesh.VertexBuffer.Count;
                    }
                    #endregion
                }
            }
        }
    }
}
