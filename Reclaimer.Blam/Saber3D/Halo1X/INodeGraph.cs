﻿using Reclaimer.Saber3D.Halo1X.Geometry;

namespace Reclaimer.Saber3D.Halo1X
{
    public interface INodeGraph
    {
        internal PakItem Item { get; }

        List<MaterialReferenceBlock> Materials { get; }
        NodeGraphBlock0xF000 NodeGraph { get; }
        Dictionary<int, NodeGraphBlock0xF000> NodeLookup { get; }
        List<BoneBlock> Bones => null;
        MatrixListBlock0x0D03 MatrixList => null;

        internal sealed string GetDebugObjectName(int objectId) => NodeLookup.TryGetValue(objectId, out var obj) ? obj.MeshName : null;
        internal sealed string GetDebugObjectNames(int startId, int count) => string.Join(" + ", Enumerable.Range(startId, count).Select(GetDebugObjectName));
    }
}
