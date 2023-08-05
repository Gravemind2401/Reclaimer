using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Reclaimer.IO.Dynamic
{
    public class StructureDefinition<TClass>
    {
        private readonly List<VersionDefinition> versions = new();

        [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
        internal class VersionDefinition
        {
            private readonly List<FieldDefinition<TClass>> fields = new();

            private readonly double? minVersion;
            private readonly double? maxVersion;
            private readonly ByteOrder? byteOrder;
            private readonly long? size;

            public VersionDefinition(double? minVersion, double? maxVersion, ByteOrder? byteOrder, long? size)
            {
                this.minVersion = minVersion;
                this.maxVersion = maxVersion;
                this.byteOrder = byteOrder;
                this.size = size;
            }

            public void AddField(FieldDefinition<TClass> definition) => fields.Add(definition);

            public TClass ReadValue(EndianReader reader)
            {
                var result = Activator.CreateInstance<TClass>();

                var origin = reader.Position;
                foreach (var field in fields)
                {
                    reader.Seek(origin + field.Offset, SeekOrigin.Begin);
                    field.ReadValue(result, reader, field.ByteOrder ?? byteOrder ?? reader.ByteOrder);
                }

                if (size.HasValue)
                    reader.Seek(origin + size.Value, SeekOrigin.Begin);

                return result;
            }

            private string GetDebuggerDisplay() => new { Min = minVersion, Max = maxVersion }.ToString();
        }
    }
}
