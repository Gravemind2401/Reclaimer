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

        public IEnumerable<SceneGroup> EnumerateGroupHierarchy() => RootNode.EnumerateHierarchy();

        public IEnumerable<Material> EnumerateMaterials()
        {
            return EnumerateGroupHierarchy()
                .SelectMany(g => g.ChildObjects)
                .Select(o => (o as ObjectPlacement)?.Object ?? o)
                .OfType<Model>()
                .Distinct()
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

        public bool HasItems => EnumerateHierarchy().Any(g => g.ChildObjects.Count > 0);

        public IEnumerable<SceneGroup> EnumerateHierarchy() => ChildGroups.SelectMany(g => g.EnumerateHierarchy()).Prepend(this);
    }

    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public class ObjectPlacement : SceneObject
    {
        public SceneObject Object { get; }
        public Matrix4x4 Transform { get; set; } = Matrix4x4.Identity;

        public ObjectPlacement(SceneObject sceneObject)
        {
            ArgumentNullException.ThrowIfNull(sceneObject);
            Object = sceneObject;
        }

        public void SetTransform(float scale, Vector3 translation, Quaternion rotation) => SetTransform(new Vector3(scale == 0 ? 1 : scale), translation, rotation);
        public void SetTransform(Vector3 scale3d, Vector3 translation, Quaternion rotation)
        {
            Transform = Matrix4x4.CreateScale(scale3d) * Matrix4x4.CreateFromQuaternion(rotation) * Matrix4x4.CreateTranslation(translation);
        }
        
        private string GetDebuggerDisplay() => string.IsNullOrWhiteSpace(Name) ? $"[[{Object.Name}]]" : Name;
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
