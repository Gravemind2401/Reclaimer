using Reclaimer.Geometry.Vectors;
using Reclaimer.IO;
using System.IO;

namespace Reclaimer.Geometry.Utilities
{
    internal class SceneWriter
    {
        private static readonly Version version = new Version();

        //TODO: treat ID 0 as always unique? ie dont force creator to provide unique ids

        private readonly EndianWriter writer;

        private readonly LazyList<string> stringPool = new();
        private readonly LazyList<int, Material> materialPool = new(m => m.Id);
        private readonly LazyList<int, Texture> texturePool = new(t => t.Id);
        private readonly LazyList<Model> modelPool = new();

        private readonly LazyList<VertexBuffer> vertexBufferPool = new(VertexBuffer.EqualityComparer);
        private readonly LazyList<IIndexBuffer> indexBufferPool = new(IIndexBuffer.EqualityComparer);
        private readonly LazyList<VectorDescriptor> vectorDescriptorPool = new();
        private readonly LazyList<Mesh> meshPool = new();

        private Model currentModel;

        public bool EmbedTextures { get; set; }

        public SceneWriter(EndianWriter writer)
        {
            this.writer = writer;
        }

        private BlockMarker BlockMarker(BlockCode code) => new BlockMarker(writer, code);

        private void WriteList<TItem>(IList<TItem> list, Action<TItem> writeFunc, BlockCode code)
        {
            using (new ListBlockMarker(writer, code, list.Count))
            {
                foreach (var item in list)
                    writeFunc(item);
            }
        }

        public void Write(Scene scene)
        {
            stringPool.Clear();
            materialPool.Clear();
            texturePool.Clear();
            modelPool.Clear();
            vertexBufferPool.Clear();
            indexBufferPool.Clear();
            vectorDescriptorPool.Clear();
            meshPool.Clear();

            using (BlockMarker(SceneCodes.FileHeader))
            {
                writer.Write((byte)version.Major);
                writer.Write((byte)version.Minor);
                writer.Write((byte)Math.Max(0, version.Build));
                writer.Write((byte)Math.Max(0, version.Revision));
                
                //everything from here on must be a block
                
                using (BlockMarker(SceneCodes.AttributeData))
                {
                    writer.Write(scene.CoordinateSystem.UnitScale);
                    writer.WriteMatrix3x3(scene.CoordinateSystem.WorldMatrix);
                    writer.Write(stringPool.IndexOf(scene.Name));
                }

                Write(scene.RootNode);

                //TODO (global markers such as bsp markers)
                using (new ListBlockMarker(writer, SceneCodes.Marker, 0))
                { }

                //note that the order here is important because writing each pool will trigger the lazy list population for subsequent pools
                WriteList(modelPool, Write, SceneCodes.Model);
                WriteList(vertexBufferPool, Write, SceneCodes.VertexBuffer);
                WriteList(indexBufferPool, Write, SceneCodes.IndexBuffer);
                WriteList(vectorDescriptorPool, Write, SceneCodes.VectorDescriptor);
                WriteList(materialPool, Write, SceneCodes.Material);
                WriteList(texturePool, Write, SceneCodes.Texture);

                using (BlockMarker(SceneCodes.StringList))
                {
                    writer.Write(stringPool.Count);
                    foreach (var str in stringPool)
                        writer.Write(str ?? string.Empty);
                }
            }
        }

        private void Write(SceneGroup sceneGroup)
        {
            using (BlockMarker(SceneCodes.SceneGroup))
            {
                var exportedGroups = sceneGroup.ChildGroups.Where(g => g.Export).ToList();
                var exportedObjects = sceneGroup.ChildObjects.Where(o => o.Export).ToList();

                using (BlockMarker(SceneCodes.AttributeData))
                    writer.Write(stringPool.IndexOf(sceneGroup.Name));

                WriteList(exportedGroups, Write, SceneCodes.SceneGroup);
                WriteList(exportedObjects, Write, SceneCodes.SceneObject);
            }
        }

        private void Write(SceneObject sceneObject)
        {
            if (sceneObject is Model model)
            {
                using (BlockMarker(SceneCodes.ModelReference))
                    writer.Write(modelPool.IndexOf(model));
            }
            else if (sceneObject is ObjectPlacement placement)
                Write(placement);
            else
                throw new NotImplementedException();
        }

        private void WriteData(byte[] data)
        {
            using (BlockMarker(SceneCodes.Data))
            {
                writer.Write(data.Length);
                writer.Write(data);
            }
        }

        private void Write(CustomProperties properties)
        {
            if (properties.Count == 0)
                return;

            using (BlockMarker(SceneCodes.CustomProperties))
            {
                writer.Write(properties.Count);
                foreach (var (key, value) in properties)
                {
                    writer.Write(stringPool.IndexOf(key));

                    if (value is bool b)
                    {
                        writer.Write((byte)0);
                        writer.Write(b);
                    }
                    else if (value is bool[] ba)
                    {
                        writer.Write((byte)1);
                        writer.Write(ba.Length);
                        foreach (var x in ba)
                            writer.Write(x);
                    }
                    else if (value is int i)
                    {
                        writer.Write((byte)2);
                        writer.Write(i);
                    }
                    else if (value is int[] ia)
                    {
                        writer.Write((byte)3);
                        writer.Write(ia.Length);
                        foreach (var x in ia)
                            writer.Write(x);
                    }
                    else if (value is float f)
                    {
                        writer.Write((byte)4);
                        writer.Write(f);
                    }
                    else if (value is float[] fa)
                    {
                        writer.Write((byte)5);
                        writer.Write(fa.Length);
                        foreach (var x in fa)
                            writer.Write(x);
                    }
                    else if (value is string s)
                    {
                        writer.Write((byte)6);
                        writer.Write(stringPool.IndexOf(s));
                    }
                    else if (value is string[] sa)
                    {
                        writer.Write((byte)7);
                        writer.Write(sa.Length);
                        foreach (var x in sa)
                            writer.Write(stringPool.IndexOf(x));
                    }
                    else
                        throw new NotSupportedException();
                }
            }
        }

        #region Materials

        private void Write(Material material)
        {
            using (BlockMarker(SceneCodes.Material))
            {
                using (BlockMarker(SceneCodes.AttributeData))
                {
                    writer.Write(stringPool.IndexOf(material.Name));
                    writer.Write(stringPool.IndexOf(material.AlphaMode));
                }

                WriteList(material.TextureMappings, Write, SceneCodes.TextureMapping);
                WriteList(material.Tints, Write, SceneCodes.Tint);
                Write(material.CustomProperties);
            }
        }

        private void Write(TextureMapping mapping)
        {
            using (BlockMarker(SceneCodes.TextureMapping))
            using (BlockMarker(SceneCodes.AttributeData))
            {
                writer.Write(stringPool.IndexOf(mapping.Usage));
                writer.Write((int)mapping.BlendChannel);
                writer.Write(texturePool.IndexOf(mapping.Texture));
                writer.Write((int)mapping.ChannelMask);
                writer.Write(mapping.Tiling);
            }
        }

        private void Write(MaterialTint tint)
        {
            using (BlockMarker(SceneCodes.Tint))
            using (BlockMarker(SceneCodes.AttributeData))
            {
                writer.Write(stringPool.IndexOf(tint.Usage));
                writer.Write((int)tint.BlendChannel);
                writer.Write(tint.Color.R);
                writer.Write(tint.Color.G);
                writer.Write(tint.Color.B);
                writer.Write(tint.Color.A);
            }
        }

        private void Write(Texture texture)
        {
            using (BlockMarker(SceneCodes.Texture))
            {
                using (BlockMarker(SceneCodes.AttributeData))
                {
                    writer.Write(stringPool.IndexOf(texture.Name));
                    writer.Write(texture.Gamma);
                }

                Write(texture.CustomProperties);

                if (EmbedTextures)
                {
                    try
                    {
                        var dds = texture.GetDds();
                        using (var ms = new MemoryStream())
                        {
                            dds.WriteToStream(ms, System.Drawing.Imaging.ImageFormat.Png);
                            WriteData(ms.ToArray());
                        }
                    }
                    catch
                    {
                        return;
                    }
                }
            }
        }

        #endregion

        #region Models

        private void WriteBaseProps(SceneObject obj)
        {
            writer.Write(stringPool.IndexOf(obj.Name));
            writer.Write((int)obj.Flags);
        }

        private void Write(ObjectPlacement placement)
        {
            using (BlockMarker(SceneCodes.Placement))
            {
                using (BlockMarker(SceneCodes.AttributeData))
                {
                    WriteBaseProps(placement);
                    writer.WriteMatrix3x4(placement.Transform);
                }

                using (BlockMarker(SceneCodes.SceneObject))
                    Write(placement.Object);

                Write(placement.CustomProperties);
            }
        }

        private void Write(Model model)
        {
            meshPool.Clear();
            currentModel = model;

            var exportedRegions = model.Regions.Where(r => r.Export && r.Permutations.Any(p => p.Export)).ToList();
            using (BlockMarker(SceneCodes.Model))
            {
                using (BlockMarker(SceneCodes.AttributeData))
                    WriteBaseProps(model);

                WriteList(exportedRegions, Write, SceneCodes.Region);
                WriteList(model.Markers, Write, SceneCodes.Marker);
                WriteList(model.Bones, Write, SceneCodes.Bone);
                WriteList(meshPool, Write, SceneCodes.Mesh);
                Write(model.CustomProperties);
            }
        }

        private void Write(ModelRegion region)
        {
            var exportedPermutations = region.Permutations.Where(p => p.Export).ToList();
            using (BlockMarker(SceneCodes.Region))
            {
                using (BlockMarker(SceneCodes.AttributeData))
                    writer.Write(stringPool.IndexOf(region.Name));

                WriteList(exportedPermutations, Write, SceneCodes.Permutation);
                Write(region.CustomProperties);
            }
        }

        private void Write(ModelPermutation permutation)
        {
            var meshes = permutation.MeshIndices
                .Select(i => currentModel.Meshes.ElementAtOrDefault(i))
                .Where(m => m != null)
                .ToList();

            //default to 0,0 in case of negatives or bad indices
            var meshRange = (Index: 0, Count: 0);

            if (meshes.Any())
            {
                meshPool.AddRange(meshes);
                meshRange = (meshPool.IndexOf(meshes[0]), meshes.Count);
            }

            using (BlockMarker(SceneCodes.Permutation))
            {
                using (BlockMarker(SceneCodes.AttributeData))
                {
                    writer.Write(stringPool.IndexOf(permutation.Name));
                    writer.Write(permutation.IsInstanced);
                    writer.Write(meshRange.Index);
                    writer.Write(meshRange.Count);
                    writer.WriteMatrix3x4(permutation.GetFinalTransform());
                }

                Write(permutation.CustomProperties);
            }
        }

        private void Write(Marker marker)
        {
            using (BlockMarker(SceneCodes.Marker))
            {
                using (BlockMarker(SceneCodes.AttributeData))
                    writer.Write(stringPool.IndexOf(marker.Name));

                WriteList(marker.Instances, Write, SceneCodes.MarkerInstance);
                Write(marker.CustomProperties);
            }
        }

        private void Write(MarkerInstance instance)
        {
            using (BlockMarker(SceneCodes.MarkerInstance))
            {
                using (BlockMarker(SceneCodes.AttributeData))
                {
                    writer.Write(instance.RegionIndex);
                    writer.Write(instance.PermutationIndex);
                    writer.Write(instance.BoneIndex);
                    writer.Write(instance.Position);
                    writer.Write(instance.Rotation);
                }
                
                Write(instance.CustomProperties);
            }
        }

        private void Write(Bone bone)
        {
            using (BlockMarker(SceneCodes.Bone))
            {
                using (BlockMarker(SceneCodes.AttributeData))
                {
                    writer.Write(stringPool.IndexOf(bone.Name));
                    writer.Write(bone.ParentIndex);
                    writer.WriteMatrix4x4(bone.LocalTransform);
                    //TODO: write/read world transform
                }

                Write(bone.CustomProperties);
            }
        }

        private void Write(Mesh mesh)
        {
            using (BlockMarker(SceneCodes.Mesh))
            {
                using (BlockMarker(SceneCodes.AttributeData))
                {
                    writer.Write(vertexBufferPool.IndexOf(mesh.VertexBuffer));
                    writer.Write(indexBufferPool.IndexOf(mesh.IndexBuffer));
                    writer.Write(mesh.BoneIndex ?? -1);
                    writer.WriteMatrix3x4(mesh.PositionBounds.CreateExpansionMatrix());
                    writer.WriteMatrix3x4(mesh.TextureBounds.CreateExpansionMatrix());
                }

                WriteList(mesh.Segments, Write, SceneCodes.MeshSegment);
            }
        }

        private void Write(MeshSegment segment)
        {
            using (BlockMarker(SceneCodes.MeshSegment))
            using (BlockMarker(SceneCodes.AttributeData))
            {
                writer.Write(segment.IndexStart);
                writer.Write(segment.IndexLength);
                writer.Write(materialPool.IndexOf(segment.Material));
            }
        }

        private void Write(VertexBuffer vertexBuffer)
        {
            using (BlockMarker(SceneCodes.VertexBuffer))
            {
                writer.Write(vertexBuffer.Count);
                WriteChannels(vertexBuffer.PositionChannels, VertexChannelCodes.Position);
                WriteChannels(vertexBuffer.TextureCoordinateChannels, VertexChannelCodes.TextureCoordinate);
                WriteChannels(vertexBuffer.NormalChannels, VertexChannelCodes.Normal);
                WriteChannels(vertexBuffer.BlendIndexChannels, VertexChannelCodes.BlendIndex);
                WriteChannels(vertexBuffer.BlendWeightChannels, VertexChannelCodes.BlendWeight);
                WriteChannels(vertexBuffer.ColorChannels, VertexChannelCodes.Color);
            }

            void WriteChannels(IList<IReadOnlyList<IVector>> vertexChannel, BlockCode code)
            {
                if (code == VertexChannelCodes.BlendWeight && vertexBuffer.HasImpliedBlendWeights)
                {
                    foreach (var vectorBuffer in vertexChannel)
                    {
                        var buffer = new List<IVector>(vectorBuffer.Count);
                        foreach (var vec in vectorBuffer)
                        {
                            var replacement = new RealVector4(vec.X, vec.Y, vec.Z, 1);
                            var len = vec.X + vec.Y + vec.Z + 1;
                            replacement.X /= len;
                            replacement.Y /= len;
                            replacement.Z /= len;
                            replacement.W /= len;

                            buffer.Add(replacement);
                        }

                        using (BlockMarker(code))
                            WriteBuffer(buffer);
                    }

                    return;
                }

                foreach (var vectorBuffer in vertexChannel)
                {
                    using (BlockMarker(code))
                        WriteBuffer(vectorBuffer);
                }
            }

            void WriteBuffer(IReadOnlyList<IVector> vectorBuffer)
            {
                var dataBuffer = vectorBuffer as IDataBuffer;
                var descriptor = VectorDescriptor.FromType(dataBuffer?.DataType);
                if (descriptor == null)
                {
                    //no choice but to assume float4
                    descriptor = VectorDescriptor.FromType(typeof(RealVector4));
                    writer.Write(vectorDescriptorPool.IndexOf(descriptor));
                    foreach (var vec in vectorBuffer)
                    {
                        writer.Write(vec.X);
                        writer.Write(vec.Y);
                        writer.Write(vec.Z);
                        writer.Write(vec.W);
                    }
                }
                else
                {
                    writer.Write(vectorDescriptorPool.IndexOf(descriptor));
                    for (var i = 0; i < dataBuffer.Count; i++)
                        writer.Write(dataBuffer.GetBytes(i));
                }
            }
        }

        private void Write(IIndexBuffer indexBuffer)
        {
            using (BlockMarker(SceneCodes.IndexBuffer))
            {
                var max = indexBuffer.DefaultIfEmpty().Max();
                var width = max <= byte.MaxValue ? sizeof(byte) : max <= ushort.MaxValue ? sizeof(ushort) : sizeof(int);
                Action<int> writeFunc = width switch
                {
                    sizeof(byte) => i => writer.Write((byte)i),
                    sizeof(ushort) => i => writer.Write((ushort)i),
                    _ => writer.Write
                };

                writer.Write((byte)indexBuffer.Layout);
                writer.Write((byte)width);
                writer.Write(indexBuffer.Count);

                foreach (var index in indexBuffer)
                    writeFunc(index);
            }
        }

        private void Write(VectorDescriptor descriptor)
        {
            using (BlockMarker(SceneCodes.VectorDescriptor))
            {
                writer.Write((byte)descriptor.DataType);
                writer.Write(descriptor.Size);
                writer.Write(descriptor.Configuration.Length);
                foreach (var dimens in descriptor.Configuration)
                {
                    writer.Write((byte)dimens.Flags);
                    writer.Write(dimens.BitCount);
                }
            }
        }

        #endregion
    }
}
