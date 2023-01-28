using Reclaimer.IO;

namespace Reclaimer.Geometry.Utilities
{
    internal abstract class SceneWriter<T>
    {
        protected readonly EndianWriter Writer;

        protected SceneWriter(EndianWriter writer)
        {
            Writer = writer;
        }

        public abstract void Write(T obj);

        protected BlockMarker BlockMarker(string code) => throw new NotImplementedException();
        protected BlockMarker BlockMarker(int identifier) => new BlockMarker(Writer, identifier);

        protected void WriteList<TItem>(IList<TItem> list, Action<TItem> writeFunc, string code)
        {
            using (BlockMarker(code))
            {
                Writer.Write(list.Count);
                foreach (var item in list)
                    writeFunc(item);
            }
        }
    }

    internal class SceneWriter : SceneWriter<Scene>
    {
        private static readonly Version version = new Version(0, 0, 0, 0);

        //TODO: treat ID 0 as always unique? ie dont force creator to provide unique ids

        private readonly LazyList<int, Material> materialPool = new(m => m.Id);
        private readonly LazyList<int, Texture> texturePool = new(t => t.Id);
        private readonly LazyList<Model> modelPool = new();

        public SceneWriter(EndianWriter writer)
            : base(writer)
        { }

        public override void Write(Scene scene)
        {
            materialPool.Clear();
            texturePool.Clear();
            modelPool.Clear();

            using (BlockMarker("RMF!"))
            {
                Writer.Write((byte)version.Major);
                Writer.Write((byte)version.Minor);
                Writer.Write((byte)version.Build);
                Writer.Write((byte)version.Revision);
                Write(scene.CoordinateSystem);
                Writer.Write(scene.Name);
                //write markers
                //write hierarchy

                var modelWriter = new ModelWriter(Writer, materialPool);
                WriteList(modelPool, modelWriter.Write, "model list");
                WriteList(materialPool, Write, "material list");
                WriteList(texturePool, Write, "texture list");
            }
        }

        private void Write(CoordinateSystem2 coordsys)
        {
            throw new NotImplementedException();
        }

        private void Write(SceneGroup sceneGroup)
        {
            throw new NotImplementedException();
        }

        private void Write(SceneObject sceneObject)
        {
            throw new NotImplementedException();
        }

        private void Write(Material material)
        {
            using (BlockMarker("material"))
            {
                Writer.Write(material.Name);
                WriteList(material.TextureMappings, Write, "tex map list");
                WriteList(material.Tints, Write, "tint list");
            }
        }

        private void Write(TextureMapping mapping)
        {
            using (BlockMarker(mapping.Usage))
            {
                Writer.Write(texturePool.IndexOf(mapping.Texture));
                Writer.Write(mapping.Tiling);
                Writer.Write((int)mapping.ChannelMask);
            }
        }

        private void Write(MaterialTint tint)
        {
            using (BlockMarker(tint.Usage))
            {
                Writer.Write(tint.Color.R);
                Writer.Write(tint.Color.G);
                Writer.Write(tint.Color.B);
                Writer.Write(tint.Color.A);
            }
        }

        private void Write(Texture texture)
        {
            throw new NotImplementedException();
        }
    }

    internal class ModelWriter : SceneWriter<Model>
    {
        private readonly List<VertexBuffer> vertexBuffers = new();
        private readonly List<IIndexBuffer> indexBuffers = new();

        private readonly LazyList<int, Material> materialPool;

        public ModelWriter(EndianWriter writer, LazyList<int, Material> materialPool)
            : base(writer)
        {
            this.materialPool = materialPool;
        }

        public override void Write(Model model)
        {
            vertexBuffers.Clear();
            indexBuffers.Clear();

            vertexBuffers.AddRange(model.Meshes.Select(m => m.VertexBuffer).Distinct());
            indexBuffers.AddRange(model.Meshes.Select(m => m.IndexBuffer).Distinct());

            using (BlockMarker("model"))
            {
                Writer.Write(model.Name);
                WriteList(model.Regions, Write, "region list");
                WriteList(model.Markers, Write, "marker list");
                WriteList(model.Bones, Write, "bone list");
                WriteList(model.Meshes, Write, "mesh list");
                WriteList(vertexBuffers, Write, "vertex buffer list");
                WriteList(indexBuffers, Write, "index buffer list");
            }
        }

        private void Write(ModelRegion region)
        {
            using (BlockMarker("region"))
            {
                Writer.Write(region.Name);
                WriteList(region.Permutations, Write, "permutation list");
            }
        }

        private void Write(ModelPermutation permutation)
        {
            using (BlockMarker("permutation"))
            {
                Writer.Write(permutation.Name);
                Writer.Write(permutation.IsInstanced);
                Writer.Write(permutation.MeshRange.Index);
                Writer.Write(permutation.MeshRange.Count);
                Writer.WriteMatrix4x4(permutation.GetFinalTransform());
            }
        }

        private void Write(Marker marker)
        {
            using (BlockMarker("marker"))
            {
                Writer.Write(marker.Name);
                WriteList(marker.Instances, Write, "marker instance list");
            }
        }

        private void Write(MarkerInstance instance)
        {
            using (BlockMarker("marker instance"))
            {
                Writer.Write(instance.RegionIndex);
                Writer.Write(instance.PermutationIndex);
                Writer.Write(instance.BoneIndex);
                Writer.Write(instance.Position);
                Writer.Write(instance.Rotation);
            }
        }

        private void Write(Bone bone)
        {
            using (BlockMarker("marker instance"))
            {
                Writer.Write(bone.Name);
                Writer.Write(bone.ParentIndex);
                Writer.WriteMatrix4x4(bone.Transform);
            }
        }

        private void Write(Mesh mesh)
        {
            using (BlockMarker("mesh"))
            {
                Writer.Write(vertexBuffers.IndexOf(mesh.VertexBuffer));
                Writer.Write(indexBuffers.IndexOf(mesh.IndexBuffer));
                Writer.Write(mesh.BoneIndex ?? -1);
                Writer.WriteMatrix3x4(mesh.PositionBounds.CreateExpansionMatrix());
                Writer.WriteMatrix3x4(mesh.TextureBounds.CreateExpansionMatrix());

                WriteList(mesh.Segments, Write, "mesh segment list");
            }
        }

        private void Write(VertexBuffer vertexBuffer)
        {
            throw new NotImplementedException();
        }

        private void Write(IIndexBuffer indexBuffer)
        {
            throw new NotImplementedException();
        }

        private void Write(MeshSegment segment)
        {
            using (BlockMarker("mesh segment"))
            {
                Writer.Write(segment.IndexStart);
                Writer.Write(segment.IndexLength);
                Writer.Write(materialPool.IndexOf(segment.Material));
            }
        }
    }
}
