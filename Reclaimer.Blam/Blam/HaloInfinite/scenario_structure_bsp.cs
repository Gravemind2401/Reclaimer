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
                    var model = tag.GetModelContent();

                    var sceneItem = new ObjectPlacement(model);
                    var transformMatrix = instance.Transform;

                    var forward = new Vector3(transformMatrix.M31, transformMatrix.M32, transformMatrix.M33);
                    var left = new Vector3(transformMatrix.M11, transformMatrix.M12, transformMatrix.M13);
                    var up = new Vector3(transformMatrix.M21, transformMatrix.M22, transformMatrix.M23);

                    var rotMat = new Matrix4x4(
                        left.X, left.Y, left.Z, 0,
                        up.X, up.Y, up.Z, 0,
                        forward.X, forward.Y, forward.Z, 0,
                        0, 0, 0, 1
                    );

                    var rotation = Quaternion.CreateFromRotationMatrix(rotMat);

                    var eulerAngles = QuaternionToEuler(rotation);
                    eulerAngles = Vector3.Transform(eulerAngles, Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathF.PI / 180 * -90));

                    var position = instance.Transform.Translation;
                    var rotatedPos = Vector4.Transform(new Vector4(position, 1.0f), rotMat);

                    sceneItem.SetTransform((Vector3)(instance.Scale), position, rotation);
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

        private static Vector3 QuaternionToEuler(Quaternion q)
        {
            float roll = MathF.Atan2(2.0f * (q.Y * q.Z + q.W * q.X), q.W * q.W - q.X * q.X - q.Y * q.Y + q.Z * q.Z);
            float pitch = MathF.Asin(-2.0f * (q.X * q.Z - q.W * q.Y));
            float yaw = MathF.Atan2(2.0f * (q.X * q.Y + q.W * q.Z), q.W * q.W + q.X * q.X - q.Y * q.Y - q.Z * q.Z);

            return new Vector3(roll, pitch, yaw);
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