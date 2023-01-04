namespace Reclaimer.Geometry
{
    public interface IMeshCompat
    {
        sealed IndexFormat IndexFormat => IndexBuffer.Layout;

        VertexBuffer VertexBuffer { get; }
        IIndexBuffer IndexBuffer { get; }

        int VertexCount => VertexBuffer?.Count ?? 0;
        int IndexCount => IndexBuffer?.Count ?? 0;
    }

    public interface ISubmeshCompat
    {
        int IndexStart { get; }
        int IndexLength { get; }
    }
}
