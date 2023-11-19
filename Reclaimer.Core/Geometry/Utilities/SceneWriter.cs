using Reclaimer.IO;

namespace Reclaimer.Geometry.Utilities
{
    internal class SceneWriter
    {
        private static readonly Version version = new Version();

        //TODO: treat ID 0 as always unique? ie dont force creator to provide unique ids

        private readonly EndianWriter writer;

        private readonly LazyList<int, Material> materialPool = new(m => m.Id);
        private readonly LazyList<int, Texture> texturePool = new(t => t.Id);
        private readonly LazyList<Model> modelPool = new();

        private readonly LazyList<VertexBuffer> vertexBufferPool = new(VertexBuffer.EqualityComparer);
        private readonly LazyList<IIndexBuffer> indexBufferPool = new(IIndexBuffer.EqualityComparer);
        private readonly LazyList<Mesh> meshPool = new();

        private Model currentModel;

        public SceneWriter(EndianWriter writer)
        {
            this.writer = writer;
        }

        private BlockMarker BlockMarker(BlockCode code) => new BlockMarker(writer, code);
        private BlockMarker BlockMarker(int identifier) => new BlockMarker(writer, new BlockCode("NULL", "Unknown"));

        private void WriteList<TItem>(IList<TItem> list, Action<TItem> writeFunc, BlockCode code)
        {
            using (new ListBlockMarker(writer, code, list.Count))
            {
                foreach (var item in list)
                    writeFunc(item);
            }
        }

        private void WriteString(string value) => writer.Write(value ?? string.Empty);

        public void Write(Scene scene)
        {
            materialPool.Clear();
            texturePool.Clear();
            modelPool.Clear();

            using (BlockMarker(SceneCodes.FileHeader))
            {
                writer.Write((byte)version.Major);
                writer.Write((byte)version.Minor);
                writer.Write((byte)Math.Max(0, version.Build));
                writer.Write((byte)Math.Max(0, version.Revision));
                writer.Write(scene.CoordinateSystem.UnitScale);
                writer.WriteMatrix3x3(scene.CoordinateSystem.WorldMatrix);
                WriteString(scene.Name);

                //everything from here on must be a block

                Write(scene.RootNode);

                //TODO (global markers such as bsp markers)
                using (new ListBlockMarker(writer, SceneCodes.Marker, 0))
                { }

                //note that the order here is important because writing each pool will trigger the lazy list population for subsequent pools
                WriteList(modelPool, Write, SceneCodes.Model);
                WriteList(vertexBufferPool, Write, SceneCodes.VertexBuffer);
                WriteList(indexBufferPool, Write, SceneCodes.IndexBuffer);
                WriteList(materialPool, Write, SceneCodes.Material);
                WriteList(texturePool, Write, SceneCodes.Texture);
            }
        }

        private void Write(SceneGroup sceneGroup)
        {
            using (BlockMarker(SceneCodes.SceneGroup))
            {
                WriteString(sceneGroup.Name);
                writer.Write(sceneGroup.ChildGroups.Count + sceneGroup.ChildObjects.Count);

                foreach (var child in sceneGroup.ChildGroups)
                    Write(child);

                foreach (var child in sceneGroup.ChildObjects)
                    Write(child);
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

        #region Materials

        private void Write(Material material)
        {
            using (BlockMarker(SceneCodes.Material))
            {
                WriteString(material.Name);
                WriteList(material.TextureMappings, Write, SceneCodes.TextureMapping);
                WriteList(material.Tints, Write, SceneCodes.Tint);
            }
        }

        private void Write(TextureMapping mapping)
        {
            using (BlockMarker(mapping.Usage))
            {
                writer.Write(texturePool.IndexOf(mapping.Texture));
                writer.Write(mapping.Tiling);
                writer.Write((int)mapping.ChannelMask);
            }
        }

        private void Write(MaterialTint tint)
        {
            using (BlockMarker(tint.Usage))
            {
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
                WriteString(texture.Name);
                writer.Write(0); //binary size if embedded
            }
        }

        #endregion

        #region Models

        private void WriteBaseProps(SceneObject obj)
        {
            WriteString(obj.Name);
            writer.Write((int)obj.Flags);
        }

        private void Write(ObjectPlacement placement)
        {
            using (BlockMarker(SceneCodes.Placement))
            {
                WriteBaseProps(placement);
                writer.WriteMatrix3x4(placement.Transform);
                Write(placement.Object);
            }
        }

        private void Write(Model model)
        {
            meshPool.Clear();
            currentModel = model;

            using (BlockMarker(SceneCodes.Model))
            {
                WriteBaseProps(model);
                WriteList(model.Regions, Write, SceneCodes.Region);
                WriteList(model.Markers, Write, SceneCodes.Marker);
                WriteList(model.Bones, Write, SceneCodes.Bone);
                WriteList(meshPool, Write, SceneCodes.Mesh);
            }
        }

        private void Write(ModelRegion region)
        {
            using (BlockMarker(SceneCodes.Region))
            {
                WriteString(region.Name);
                WriteList(region.Permutations, Write, SceneCodes.Permutation);
            }
        }

        private void Write(ModelPermutation permutation)
        {
            var meshes = permutation.MeshIndices.Select(i => currentModel.Meshes.ElementAtOrDefault(i));

            var meshRange = permutation.MeshRange;
            if (!meshes.Any() || meshes.Any(m => m == null))
                meshRange = (0, 0); //normalize to 0,0 in case of negatives or bad indices
            else
            {
                meshPool.AddRange(meshes);
                meshRange.Index = meshPool.IndexOf(meshes.First());
            }

            using (BlockMarker(SceneCodes.Permutation))
            {
                WriteString(permutation.Name);
                writer.Write(permutation.IsInstanced);
                writer.Write(meshRange.Index);
                writer.Write(meshRange.Count);
                writer.WriteMatrix3x4(permutation.GetFinalTransform());
            }
        }

        private void Write(Marker marker)
        {
            using (BlockMarker(SceneCodes.Marker))
            {
                WriteString(marker.Name);
                WriteList(marker.Instances, Write, SceneCodes.MarkerInstance);
            }
        }

        private void Write(MarkerInstance instance)
        {
            using (BlockMarker(SceneCodes.MarkerInstance))
            {
                writer.Write(instance.RegionIndex);
                writer.Write(instance.PermutationIndex);
                writer.Write(instance.BoneIndex);
                writer.Write(instance.Position);
                writer.Write(instance.Rotation);
            }
        }

        private void Write(Bone bone)
        {
            using (BlockMarker(SceneCodes.Bone))
            {
                WriteString(bone.Name);
                writer.Write(bone.ParentIndex);
                writer.WriteMatrix4x4(bone.Transform);
            }
        }

        private void Write(Mesh mesh)
        {
            using (BlockMarker(SceneCodes.Mesh))
            {
                writer.Write(vertexBufferPool.IndexOf(mesh.VertexBuffer));
                writer.Write(indexBufferPool.IndexOf(mesh.IndexBuffer));
                writer.Write(mesh.BoneIndex ?? -1);
                writer.WriteMatrix3x4(mesh.PositionBounds.CreateExpansionMatrix());
                writer.WriteMatrix3x4(mesh.TextureBounds.CreateExpansionMatrix());

                WriteList(mesh.Segments, Write, SceneCodes.MeshSegment);
            }
        }

        private void Write(MeshSegment segment)
        {
            using (BlockMarker(SceneCodes.MeshSegment))
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
            }

            void WriteChannels(IList<IReadOnlyList<IVector>> vertexChannel, BlockCode code)
            {
                foreach (var vectorBuffer in vertexChannel)
                {
                    using (BlockMarker(code))
                        WriteBuffer(vectorBuffer);
                }
            }

            void WriteBuffer(IReadOnlyList<IVector> vectorBuffer)
            {
                var vb = vectorBuffer as IDataBuffer;
                var typeCode = VectorTypeCodes.FromType(vb?.DataType);
                if (typeCode == null)
                {
                    //no choice but to assume float4
                    writer.Write(VectorTypeCodes.Float4.Value);
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
                    writer.Write(typeCode.Value);
                    for (var i = 0; i < vb.Count; i++)
                        writer.Write(vb.GetBytes(i));
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

        #endregion
    }
}
