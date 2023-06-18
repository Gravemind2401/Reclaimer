using Reclaimer.IO;
using System.Text;

namespace Reclaimer.Blam.Halo5
{
    [FixedSize(32)]
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public readonly record struct TagReference
    {
        private readonly Module module;
        private readonly int globalTagId;
        private readonly long globalAssetId;
        private readonly int classId;

        public int TagId => globalTagId;
        public ModuleItem Tag => module.GetItemById(globalTagId);

        public string ClassCode => (classId == -1) ? null : Encoding.UTF8.GetString(BitConverter.GetBytes(classId));

        public TagReference(Module module, EndianReader reader)
        {
            this.module = module ?? throw new ArgumentNullException(nameof(module));
            ArgumentNullException.ThrowIfNull(reader);

            reader.BaseStream.Position += 8; //padding
            reader.ReadInt32(); //index?
            globalTagId = reader.ReadInt32();
            globalAssetId = reader.ReadInt64();
            classId = reader.ReadInt32(ByteOrder.BigEndian);
            reader.BaseStream.Position += 4; //padding
        }

        private string GetDebuggerDisplay()
        {
            var tag = Tag;
            return tag == null ? "{null reference}" : $"{{[{tag.ClassCode}] {tag.TagName}}}";
        }
    }
}
