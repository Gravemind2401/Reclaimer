using Reclaimer.Geometry;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Reclaimer.Blam.Common
{
    internal static class BlamUtils
    {
        //for sorting, ignore leading symbol chars
        //also pad numbers to 3 digits so abc_9 comes before abc_10 etc
        private static readonly Regex RxInstanceNameSort = new Regex(@"(?<=^[^a-z0-9]*)[a-z0-9].*", RegexOptions.IgnoreCase);
        private static readonly Regex RxInstanceNumberSort = new Regex(@"(?<!\d)\d{1,2}(?!\d)", RegexOptions.IgnoreCase);

        //take the longest string of one or more words separated by underscores (ie "word_word_word", final word must be 2+ chars)
        private static readonly Regex RxInstanceGroupName = new Regex(@"[a-z]+(?:_[a-z]+)*(?<=[a-z]{2,})", RegexOptions.IgnoreCase);

        public static IEnumerable<IGrouping<string, TInstance>> GroupGeometryInstances<TInstance>(IEnumerable<TInstance> instances, Func<TInstance, string> nameSelector)
        {
            return from i in instances
                   let name = nameSelector(i)
                   orderby GetSortValue(name), name
                   group i by GetGroupName(name) into g
                   orderby g.Key
                   select g;

            static string GetSortValue(string value)
            {
                var m = RxInstanceNameSort.Match(value);
                return m.Success ? RxInstanceNumberSort.Replace(m.Value, x => x.Value.PadLeft(3, '0')) : value;
            }

            static string GetGroupName(string value)
            {
                //note the matches are ranked by word count, not string length
                //this ensures that if only singular words were found, then the leftmost match is the one that gets used
                var m = RxInstanceGroupName.Matches(value);
                return m.Count == 0 ? value : m.OfType<Match>().MaxBy(m => m.Value.Count(c => c == '_')).Value;
            }
        }

        public static IndexBuffer CreateImpliedIndexBuffer(int vertexCount, IndexFormat indexFormat)
        {
            //decorator models have no explicit index buffer.
            //instead, the index buffer is implied to be a triangle strip ranging from zero to N-1 where N is the vertex count.

            Type indexType;
            byte[] buffer;

            if (vertexCount > ushort.MaxValue)
            {
                indexType = typeof(int);
                buffer = new byte[vertexCount * sizeof(int)];
                var indices = MemoryMarshal.Cast<byte, int>(buffer);
                for (var i = 0; i < indices.Length; i++)
                    indices[i] = i;
            }
            else
            {
                indexType = typeof(ushort);
                buffer = new byte[vertexCount * sizeof(ushort)];
                var indices = MemoryMarshal.Cast<byte, ushort>(buffer);
                for (var i = 0; i < indices.Length; i++)
                    indices[i] = (ushort)i;
            }

            return new IndexBuffer(buffer, indexType) { Layout = indexFormat };
        }
    }
}
