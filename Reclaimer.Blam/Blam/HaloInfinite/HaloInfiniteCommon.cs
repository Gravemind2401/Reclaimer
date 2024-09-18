using Reclaimer.Blam.Common;
using Reclaimer.Geometry;
using Reclaimer.Geometry.Vectors;
using Reclaimer.IO;
using System.Numerics;
using System.Reflection;

namespace Reclaimer.Blam.HaloInfinite
{
    [FixedSize(80)]
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public struct VertexBufferInfo
    {
        [Offset(4)]
        public int VertexCount { get; set; }

        private readonly string GetDebuggerDisplay() => new { VertexCount }.ToString();
    }

    [FixedSize(72)]
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public struct IndexBufferInfo
    {
        [Offset(4)]
        public int IndexCount { get; set; }

        private readonly string GetDebuggerDisplay() => new { IndexCount }.ToString();
    }

    public class HaloInfiniteGeometryArgs
    {
        public Module Module { get; init; }
        public ResourcePackingPolicy ResourcePolicy { get; init; }
        public IReadOnlyList<RegionBlock> Regions { get; init; }
        public IReadOnlyList<MaterialBlock> Materials { get; init; }
        public IReadOnlyList<SectionBlock> Sections { get; init; }
        public IReadOnlyList<NodeMapBlock> NodeMaps { get; init; }
        public IReadOnlyList<MeshResourceGroupBlock> MeshResourceGroups { get; init; }
        public int ResourceIndex { get; init; }
        public int ResourceCount { get; init; }
    }

    internal static class HaloInfiniteCommon
    {
        public static IEnumerable<Material> GetMaterials(IReadOnlyList<MaterialBlock> materials)
        {
            for (var i = 0; i < materials?.Count; i++)
            {
                var tag = materials[i].MaterialReference.Tag;
                if (tag == null)
                {
                    yield return null;
                    continue;
                }

                var material = new Material
                {
                    Id = tag.GlobalTagId,
                    Name = tag.FileName
                };

                material.CustomProperties.Add(BlamConstants.SourceTagPropertyName, tag.TagName);

                var mat = tag?.ReadMetadata<material>();
                if (mat == null)
                {
                    yield return material;
                    continue;
                }

                var map = mat.PostprocessDefinitions.FirstOrDefault()?.Textures.FirstOrDefault();
                var bitmTag = map?.BitmapReference.Tag;
                if (bitmTag == null)
                {
                    yield return material;
                    continue;
                }

                try
                {
                    var texture = new Texture
                    {
                        Id = bitmTag.GlobalTagId,
                        ContentProvider = bitmTag.ReadMetadata<bitmap>()
                    };

                    texture.CustomProperties.Add(BlamConstants.SourceTagPropertyName, bitmTag.TagName);

                    material.TextureMappings.Add(new TextureMapping
                    {
                        Usage = TextureUsage.Diffuse,
                        Tiling = Vector2.One,
                        Texture = texture
                    });
                }
                catch { }

                yield return material;
            }
        }

        public static List<Mesh> GetMeshes(HaloInfiniteGeometryArgs args, out List<Material> materials)
        {
            //TODO: implement an LOD selector one day
            const int lod = 0;
            var lodFlag = (LodFlags)(1 << lod);

            var resourceBuffers = new byte[args.ResourceCount][];
            for (var i = 0; i < args.ResourceCount; i++)
            {
                var itemIndex = args.Module.Resources[args.ResourceIndex + i];
                var resource = args.Module.Items[itemIndex];
                using (var reader = resource.CreateReader())
                    resourceBuffers[i] = reader.ReadBytes(resource.TotalUncompressedSize);
            }

            byte[] GetResourceData(int offset, int length)
            {
                var output = new byte[length];
                var resourceIndex = 0;
                var resourcePosition = 0;
                var outputPosition = 0;

                while (outputPosition < length && resourceIndex < resourceBuffers.Length)
                {
                    var resourceBuffer = resourceBuffers[resourceIndex];

                    if (offset >= resourcePosition + resourceBuffer.Length)
                    {
                        resourcePosition += resourceBuffer.Length;
                        resourceIndex++;
                        continue;
                    }

                    var offsetInBlock = 0;
                    if (offset > resourcePosition)
                    {
                        offsetInBlock += offset - resourcePosition;
                        resourcePosition += offsetInBlock;
                    }

                    //the data can end up being split across the end of one resource tag and the beginning of the next
                    //so we need to copy whats left and move to the next resource tag if theres still more to copy
                    var bytesToCopy = Math.Min(length - outputPosition, resourceBuffer.Length - offsetInBlock);
                    Array.Copy(resourceBuffer, offsetInBlock, output, outputPosition, bytesToCopy);

                    outputPosition += bytesToCopy;
                    resourcePosition += bytesToCopy;
                    resourceIndex++;
                }

                return output;
            }

            //TODO: will there ever be more than one MeshResourceGroup?
            var apiResource = args.MeshResourceGroups[0].RenderGeometryApiResource;

            var vectorBuffers = new Dictionary<int, IVectorBuffer>(apiResource.PcVertexBuffers.Count);
            var indexBuffers = new Dictionary<int, IndexBuffer>(apiResource.PcIndexBuffers.Count);

            materials = new List<Material>(args.Materials?.Count ?? default);
            var meshList = new List<Mesh>(args.Sections.Count);
            meshList.AddRange(Enumerable.Repeat(default(Mesh), args.Sections.Count));

            var matLookup = materials;
            materials.AddRange(GetMaterials(args.Materials));

            foreach (var (section, sectionIndex) in args.Sections.Select((s, i) => (s, i)))
            {
                if (section.SectionLods[0].LodFlags > 0 && (section.SectionLods[0].LodFlags & lodFlag) == 0 || section.Flags.HasFlag(MeshFlags.MeshIsCustomShadowCaster) || section.SectionLods[0].LODHasShadowProxies != 0)
                    continue;

                var lodIndex = Math.Min(lod, section.SectionLods.Count - 1);
                var lodData = section.SectionLods[lodIndex];

                var vertexBuffer = new VertexBuffer();

                if (!indexBuffers.TryGetValue(lodData.IndexBufferIndex, out var indexBuffer))
                {
                    var indexBufferInfo = apiResource.PcIndexBuffers[lodData.IndexBufferIndex];
                    var resourceBuffer = GetResourceData(indexBufferInfo.Offset, indexBufferInfo.DataLength);

                    indexBuffer = new IndexBuffer(resourceBuffer, indexBufferInfo.Count, 0, indexBufferInfo.Stride, 0, indexBufferInfo.Stride);
                    indexBuffer.Layout = section.IndexFormat;
                    indexBuffers.Add(lodData.IndexBufferIndex, indexBuffer);
                }

                foreach (var vectorBufferIndex in lodData.VertexBufferIndicies.ValidIndicies)
                {
                    var vertexBufferInfo = apiResource.PcVertexBuffers[vectorBufferIndex];
                    var resourceBuffer = GetResourceData(vertexBufferInfo.Offset, vertexBufferInfo.DataLength);

                    var vectorChannel = GetVectorChannel(vertexBufferInfo.Usage, vertexBuffer);
                    if (vectorChannel == null)
                        continue;

                    if (!vectorBuffers.TryGetValue(vectorBufferIndex, out var vectorBuffer))
                    {
                        var vectorType = GetFieldType(vertexBufferInfo.Format);
                        if (vectorType == null)
                            continue;

                        var method = typeof(HaloInfiniteCommon).GetMethod(nameof(CreateBuffer), BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(vectorType);
                        vectorBuffer = (IVectorBuffer)method.Invoke(null, new object[] { resourceBuffer, vertexBufferInfo.Count, 0, vertexBufferInfo.Stride, 0 });
                        vectorBuffers.Add(vectorBufferIndex, vectorBuffer);
                    }

                    vectorChannel.Add(vectorBuffer);
                }

                if (vertexBuffer.HasBlendWeights && section.VertexFormat != VertexType.Skinned8Weights)
                    vertexBuffer.HasImpliedBlendWeights = true;

                var mesh = new Mesh
                {
                    BoneIndex = section.NodeIndex == byte.MaxValue ? null : section.NodeIndex,
                    VertexBuffer = vertexBuffer,
                    IndexBuffer = indexBuffers[lodData.IndexBufferIndex]
                };

                if (mesh.VertexBuffer.HasBlendWeights && mesh.VertexBuffer.HasImpliedBlendWeights)
                    mesh.Flags |= Geometry.MeshFlags.UseImpliedBlendWeights;

                if (section.UseDualQuat)
                    mesh.Flags |= Geometry.MeshFlags.UseDualQuat;

                // have alternate function for if there are no defined parts, where we just do the whole buffer
                if (lodData.Submeshes.Count > 0)
                {
                    mesh.Segments.AddRange(
                        lodData.Submeshes.Select(s => new MeshSegment
                        {
                            Material = matLookup.ElementAtOrDefault(s.ShaderIndex),
                            IndexStart = s.IndexStart,
                            IndexLength = s.IndexLength
                        })
                    );
                }
                else
                {
                    //create an implied submesh that covers all indices
                    mesh.Segments.Add(
                        new MeshSegment
                        {
                            Material = matLookup.ElementAtOrDefault(0),
                            IndexStart = 0,
                            IndexLength = mesh.IndexBuffer.Count
                        }
                    );
                }

                meshList[sectionIndex] = mesh;
            }

            return meshList;
        }

        private static Type GetFieldType(RasterizerVertexFormat vectorType)
        {
            return vectorType switch
            {
                RasterizerVertexFormat.Real16Vector2D => typeof(HalfVector2),
                RasterizerVertexFormat.Real16Vector4D => typeof(HalfVector4),
                RasterizerVertexFormat.RealVector2D => typeof(RealVector2),
                RasterizerVertexFormat.RealVector3D => typeof(RealVector3),
                RasterizerVertexFormat.RealVector4D => typeof(RealVector4),
                RasterizerVertexFormat.ByteVector4D => typeof(UByte4),
                RasterizerVertexFormat.WordVector2DNormalized => typeof(UInt16N2),
                RasterizerVertexFormat.WordVector4DNormalized => typeof(UInt16N4),
                RasterizerVertexFormat._10_10_10_2_SignedNormalizedPackedAsUnorm => typeof(NxAAA2),
                RasterizerVertexFormat._10_10_10_Normalized => typeof(UxAAA0),
                RasterizerVertexFormat.Real => typeof(RealVector1),
                _ => null
            };
        }

        private static IList<IReadOnlyList<IVector>> GetVectorChannel(VertexBufferUsage usage, VertexBuffer vertexBuffer)
        {
            return usage switch
            {
                VertexBufferUsage.Position => vertexBuffer.PositionChannels,
                VertexBufferUsage.UV0 or VertexBufferUsage.UV1 or VertexBufferUsage.UV2 => vertexBuffer.TextureCoordinateChannels,
                VertexBufferUsage.Normal => vertexBuffer.NormalChannels,
                VertexBufferUsage.Tangent => vertexBuffer.TangentChannels,
                VertexBufferUsage.BlendWeights0 or VertexBufferUsage.BlendWeights1 => vertexBuffer.BlendWeightChannels,
                VertexBufferUsage.BlendIndices0 or VertexBufferUsage.BlendIndices1 => vertexBuffer.BlendIndexChannels,
                VertexBufferUsage.Color => vertexBuffer.ColorChannels,
                _ => null
            };
        }

        private static IVectorBuffer CreateBuffer<T>(byte[] buffer, int count, int start, int stride, int offset)
            where T : struct, IBufferableVector<T>
        {
            return new VectorBuffer<T>(buffer, count, start, stride, offset);
        }
    }
}
