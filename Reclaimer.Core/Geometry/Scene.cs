using System.Diagnostics;
using System.Numerics;

namespace Reclaimer.Geometry
{
    [DebuggerDisplay($"{{{nameof(Name)},nq}}")]
    public class Scene : SceneGroup
    {
        public CoordinateSystem2 CoordinateSystem { get; set; } = CoordinateSystem2.Default;

        public List<Marker> Markers { get; } = new();

        public static Scene WrapSingleModel(Model model)
        {
            var scene = new Scene { Name = model.Name };
            scene.ChildObjects.Add(model);

            return scene;
        }

        private IEnumerable<SceneGroup> EnumerateDescendants(SceneGroup group) => group.ChildGroups.Concat(group.ChildGroups.SelectMany(EnumerateDescendants));

        public IEnumerable<SceneGroup> EnumerateGroupHierarchy() => ChildGroups.Prepend(this).Concat(ChildGroups.SelectMany(EnumerateDescendants));

        public IEnumerable<Material> EnumerateMaterials()
        {
            return EnumerateGroupHierarchy()
                .SelectMany(g => g.ChildObjects)
                .OfType<Model>()
                .SelectMany(m => m.EnumerateMaterials())
                .DistinctBy(m => m.Id);
        }
    }

    [DebuggerDisplay($"{{{nameof(Name)},nq}}")]
    public class SceneGroup
    {
        public string Name { get; set; }
        public List<SceneGroup> ChildGroups { get; } = new();
        public List<SceneObject> ChildObjects { get; } = new();
    }

    [DebuggerDisplay($"{{{nameof(Name)},nq}}")]
    public class ObjectPlacement
    {
        public string Name { get; set; }
        public Matrix4x4 Transform { get; set; } = Matrix4x4.Identity;
    }

    [DebuggerDisplay($"{{{nameof(Name)},nq}}")]
    public abstract class SceneObject
    {
        public string Name { get; set; }
    }
}
