using System.Diagnostics;
using System.Numerics;

namespace Reclaimer.Geometry
{
    [DebuggerDisplay($"{{{nameof(Name)},nq}}")]
    public class Scene
    {
        public CoordinateSystem2 CoordinateSystem { get; set; } = CoordinateSystem2.Default;

        public string Name { get; set; }
        public List<Marker> Markers { get; } = new();
        public SceneGroup RootNode { get; } = new() { Name = "Root" };

        public List<SceneGroup> ChildGroups => RootNode.ChildGroups;
        public List<SceneObject> ChildObjects => RootNode.ChildObjects;

        public static Scene WrapSingleModel(Model model, float unitScale = 1)
        {
            var scene = new Scene { Name = model.Name, CoordinateSystem = CoordinateSystem2.Default.WithScale(unitScale) };
            scene.ChildObjects.Add(model);
            return scene;
        }

        private IEnumerable<SceneGroup> EnumerateDescendants(SceneGroup group) => group.ChildGroups.Concat(group.ChildGroups.SelectMany(EnumerateDescendants));

        public IEnumerable<SceneGroup> EnumerateGroupHierarchy() => ChildGroups.Prepend(RootNode).Concat(ChildGroups.SelectMany(EnumerateDescendants));

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
        public SceneFlags Flags { get; set; }
    }

    [Flags]
    public enum SceneFlags
    {
        None = 0,
        SkyFlag = 1,
    }
}
