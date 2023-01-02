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
