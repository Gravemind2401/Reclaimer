using System.Numerics;

namespace Reclaimer.Geometry
{
    public static class GeometryExtensions
    {
        public static IEnumerable<int> Unstrip(this IEnumerable<int> strip)
        {
            var position = 0;
            int i0, i1 = 0, i2 = 0;

            foreach (var index in strip)
            {
                i0 = i1;
                i1 = i2;
                i2 = index;

                if (position++ < 2)
                    continue;

                if (i0 != i1 && i0 != i2 && i1 != i2)
                {
                    yield return i0;

                    if (position % 2 == 1)
                    {
                        yield return i1;
                        yield return i2;
                    }
                    else
                    {
                        yield return i2;
                        yield return i1;
                    }
                }
            }
        }

        public static IEnumerable<int> GetTriangleIndicies(this IMeshCompat mesh, ISubmeshCompat submesh)
        {
            var indices = mesh.IndexBuffer.GetSubset(submesh.IndexStart, submesh.IndexLength);

            if (mesh.IndexFormat == IndexFormat.TriangleStrip)
                indices = Unstrip(indices);

            return indices;
        }

        public static IEnumerable<Vector3> GetPositions(this IMeshCompat mesh) => GetPositions(mesh, 0, mesh.VertexCount);
        public static IEnumerable<Vector3> GetPositions(this IMeshCompat mesh, int index, int count)
        {
            return mesh.VertexBuffer.HasPositions ? mesh.VertexBuffer.PositionChannels[0].GetSubset(index, count).Select(v => new Vector3(v.X, v.Y, v.Z)) : null;
        }

        public static IEnumerable<Vector2> GetTexCoords(this IMeshCompat mesh) => GetTexCoords(mesh, 0, mesh.VertexCount);
        public static IEnumerable<Vector2> GetTexCoords(this IMeshCompat mesh, int index, int count)
        {
            return mesh.VertexBuffer.HasTextureCoordinates ? mesh.VertexBuffer.TextureCoordinateChannels[0].GetSubset(index, count).Select(v => new Vector2(v.X, v.Y)) : null;
        }

        public static IEnumerable<Vector3> GetNormals(this IMeshCompat mesh) => GetNormals(mesh, 0, mesh.VertexCount);
        public static IEnumerable<Vector3> GetNormals(this IMeshCompat mesh, int index, int count)
        {
            return mesh.VertexBuffer.HasNormals ? mesh.VertexBuffer.NormalChannels[0].GetSubset(index, count).Select(v => new Vector3(v.X, v.Y, v.Z)) : null;
        }

        public static IEnumerable<Vector4> GetBlendIndices(this IMeshCompat mesh) => GetBlendIndices(mesh, 0, mesh.VertexCount);
        public static IEnumerable<Vector4> GetBlendIndices(this IMeshCompat mesh, int index, int count)
        {
            return mesh.VertexBuffer.HasBlendIndices ? mesh.VertexBuffer.BlendIndexChannels[0].GetSubset(index, count).Select(v => new Vector4(v.X, v.Y, v.Z, v.W)) : null;
        }

        public static IEnumerable<Vector4> GetBlendWeights(this IMeshCompat mesh) => GetBlendWeights(mesh, 0, mesh.VertexCount);
        public static IEnumerable<Vector4> GetBlendWeights(this IMeshCompat mesh, int index, int count)
        {
            return mesh.VertexBuffer.HasBlendWeights ? mesh.VertexBuffer.BlendWeightChannels[0].GetSubset(index, count).Select(v => new Vector4(v.X, v.Y, v.Z, v.W)) : null;
        }
    }
}
