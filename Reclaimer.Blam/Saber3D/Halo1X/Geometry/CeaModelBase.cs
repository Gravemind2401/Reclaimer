﻿using Reclaimer.Drawing;
using Reclaimer.Geometry;
using Reclaimer.IO;
using Reclaimer.Saber3D.Common;
using Reclaimer.Utilities;
using System.Numerics;

namespace Reclaimer.Saber3D.Halo1X.Geometry
{
    public abstract class CeaModelBase : ContentItemDefinition, INodeGraph, IContentProvider<Reclaimer.Geometry.Scene>, IContentProvider<Model>
    {
        public List<DataBlock> Blocks { get; protected init; }
        public Dictionary<int, NodeGraphBlock0xF000> NodeLookup { get; protected init; }

        public NodeGraphBlock0xF000 NodeGraph => Blocks.OfType<NodeGraphBlock0xF000>().SingleOrDefault();
        public List<MaterialReferenceBlock> Materials => Blocks.OfType<MaterialListBlock>().SingleOrDefault()?.Materials;

        protected CeaModelBase(PakItem item)
            : base(item)
        { }

        protected EndianReader CreateReader()
        {
            var reader = Container.CreateReader();
            reader.RegisterInstance(this);
            reader.RegisterInstance(Item);
            return reader;
        }

        public IEnumerable<IContentProvider<IBitmap>> GetAllBitmaps()
        {
            return from m in Materials
                   let i = Container.FindItem(PakItemType.Textures, m.Value, true)
                   where i != null
                   select new Texture(i);
        }

        public IEnumerable<IContentProvider<IBitmap>> GetBitmaps(IEnumerable<int> shaderIndexes)
        {
            return from m in shaderIndexes.Select(i => Materials[i])
                   let i = Container.FindItem(PakItemType.Textures, m.Value, true)
                   where i != null
                   select new Texture(i);
        }

        protected List<Material> GetMaterials()
        {
            return Materials.Select((m, i) =>
            {
                var material = new Material { Id = i, Name = m.Value };

                material.TextureMappings.Add(new TextureMapping
                {
                    Usage = TextureUsage.Diffuse,
                    Tiling = Vector2.One,
                    Texture = new Reclaimer.Geometry.Texture
                    {
                        Id = i,
                        ContentProvider = GetBitmaps(Enumerable.Repeat(i, 1)).FirstOrDefault()
                    }
                });

                return material;
            }).ToList();
        }

        protected static Mesh GetMesh(NodeGraphBlock0xF000 block, List<Material> materials)
        {
            var mesh = new Mesh
            {
                VertexBuffer = new VertexBuffer(),
                IndexBuffer = block.Faces.IndexBuffer
            };

            mesh.VertexBuffer.PositionChannels.Add(block.Positions.PositionBuffer);

            if (block.Positions.NormalBuffer != null)
                mesh.VertexBuffer.NormalChannels.Add(block.Positions.NormalBuffer);

            if (block.VertexData?.Count > 0)
                mesh.VertexBuffer.TextureCoordinateChannels.Add(block.VertexData.TexCoordsBuffer);

            if (block.BlendData == null)
                mesh.BoneIndex = (byte?)block.GetAncestorBoneIndex();
            else
            {
                var indexBuffer = block.BlendData.IndexData.CreateMappedIndexBuffer();
                mesh.VertexBuffer.BlendIndexChannels.Add(indexBuffer);
                mesh.VertexBuffer.BlendWeightChannels.Add(block.BlendData.WeightData.BlendWeightBuffer);
            }

            foreach (var submesh in block.SubmeshData.Submeshes)
            {
                mesh.Segments.Add(new MeshSegment
                {
                    Material = materials.ElementAtOrDefault(submesh.Materials[0].MaterialIndex),
                    IndexStart = submesh.FaceRange.Offset * 3,
                    IndexLength = submesh.FaceRange.Count * 3
                });
            }

            return mesh;
        }

        PakItem INodeGraph.Item => Item;

        #region IContentProvider

        Model IContentProvider<Model>.GetContent() => GetModelContent();

        Reclaimer.Geometry.Scene IContentProvider<Reclaimer.Geometry.Scene>.GetContent() => Reclaimer.Geometry.Scene.WrapSingleModel(GetModelContent(), CoordinateSystem.HaloCEX, 0.1f * StandardUnits.Feet);

        protected abstract Model GetModelContent();

        #endregion
    }
}
