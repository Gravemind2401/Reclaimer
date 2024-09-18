using Reclaimer.Blam.Common;
using Reclaimer.Geometry;
using Reclaimer.Geometry.Vectors;
using Reclaimer.IO;
using Reclaimer.Utilities;
using System.Numerics;

namespace Reclaimer.Blam.HaloInfinite
{
    public class scenario_structure_bsp : ContentTagDefinition<Scene>, IContentProvider<Scene>
    {
        public scenario_structure_bsp(ModuleItem item, MetadataHeader header)
            : base(item, header)
        { }

        [Offset(300)]
        public BlockCollection<ClusterBlock> Clusters { get; set; }

        [Offset(420)]
        public BlockCollection<InstancedGeometry> InstancedGeometryInstances { get; set; }


        #region IContentProvider

        public override Scene GetContent()
        {
            var scene = new Scene
            {
                Name = Item.FileName,
                OriginalPath = Item.TagName,
                CoordinateSystem = CoordinateSystem.Default.WithScale(BlamConstants.WorldUnitScale)
            };

            var bspGroup = new SceneGroup { Name = BlamConstants.ScenarioBspGroupName };

            foreach (var instance in InstancedGeometryInstances)
            {
                if (instance.RuntimeGeoMeshReference.Tag == null)
                    continue;
                if (instance.FlagsOverride.HasFlag(MeshFlags.MeshIsCustomShadowCaster))
                    continue;
                try
                {
                    var tag = instance.RuntimeGeoMeshReference.Tag.ReadMetadata<runtime_geo>();
                    var model = tag.GetModelContent(instance.Material);

                    var sceneItem = new ObjectPlacement(model);

                    var rotMat = new Matrix4x4(
                        instance.Transform.M11, instance.Transform.M12, instance.Transform.M13, 0,
                        instance.Transform.M21, instance.Transform.M22, instance.Transform.M23, 0,
                        instance.Transform.M31, instance.Transform.M32, instance.Transform.M33, 0,
                        0, 0, 0, 1
                    );

                    var rotation = Quaternion.CreateFromRotationMatrix(rotMat);
                    sceneItem.SetTransform((Vector3)instance.Scale, instance.Transform.Translation, rotation);
                    bspGroup.ChildObjects.Add(sceneItem);
                }
                catch
                {
                    
                }
            }

            if (bspGroup.HasItems)
                scene.ChildGroups.Add(bspGroup);

            return scene;
        }
        #endregion
    }


    [FixedSize(320)]
    public class InstancedGeometry
    {
        [Offset(0)]
        public RealVector3 Scale { get; set; }

        [Offset(12)]
        public Matrix4x4 Transform { get; set; }

        [Offset(60)]
        public TagReference RuntimeGeoMeshReference { get; set; }

        [Offset(116)]
        public short MeshIndex { get; set; }

        [Offset(118)]
        public short BoundsIndex { get; set; }

        [Offset(240)]
        public BlockCollection<MaterialBlock> Material { get; set; }

        [Offset(272)]
        public MeshFlags FlagsOverride { get; set; }

        [Offset(300)]
        public long Guid { get; set; }
    }

    [FixedSize(100)]
    public class ClusterBlock
    {
        [Offset(0)]
        public RealBounds XBounds { get; set; }

        [Offset(8)]
        public RealBounds YBounds { get; set; }

        [Offset(16)]
        public RealBounds ZBounds { get; set; }

        [Offset(44)]
        public short SectionIndex { get; set; }
    }
}