using System.Diagnostics;
using System.Numerics;

namespace Reclaimer.Geometry
{
    [DebuggerDisplay($"{{{nameof(Name)},nq}}")]
    public class Scene
    {
        public CoordinateSystem2 CoordinateSystem { get; set; } = CoordinateSystem2.Default;

        public string Name { get; set; }
        public List<SceneGroup> ObjectGroups { get; } = new();
        public List<Marker> Markers { get; } = new();

        public static Scene WrapSingleModel(Model model)
        {
            var scene = new Scene { Name = model.Name };
            var group = new SceneGroup { Name = model.Name };
            var obj = new SceneObject { Name = model.Name };

            obj.Model = model;
            group.ChildObjects.Add(obj);
            scene.ObjectGroups.Add(group);

            return scene;
        }

        private IEnumerable<SceneGroup> EnumerateDescendants(SceneGroup group) => group.ChildGroups.Concat(group.ChildGroups.SelectMany(EnumerateDescendants));

        public IEnumerable<SceneGroup> EnumerateGroupHierarchy() => ObjectGroups.Concat(ObjectGroups.SelectMany(EnumerateDescendants));

        public IEnumerable<Material> EnumerateMaterials()
        {
            return EnumerateGroupHierarchy()
                .SelectMany(g => g.ChildObjects)
                .Select(o => o.Model)
                .SelectMany(m => m.Meshes)
                .Where(m => m != null)
                .SelectMany(m => m.Segments)
                .Select(s => s.Material)
                .Where(m => m != null)
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
    public class SceneObject
    {
        public string Name { get; set; }
        public Model Model { get; set; }
    }
}
