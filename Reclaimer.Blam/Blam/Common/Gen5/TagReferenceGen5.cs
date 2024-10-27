using Reclaimer.IO;
using System.Text;

namespace Reclaimer.Blam.Common.Gen5
{
    [FixedSize(32, MaxVersion = (int)ModuleType.HaloInfinite)]
    [FixedSize(28, MinVersion = (int)ModuleType.HaloInfinite)]
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public readonly record struct TagReferenceGen5
    {
        private readonly IModule module;
        private readonly int globalTagId;
        private readonly long globalAssetId;
        private readonly int classId;

        public int TagId => globalTagId;
        public IModuleItem Tag => module.GetItemById(globalTagId);

        public string ClassCode => classId == -1 ? null : Encoding.UTF8.GetString(BitConverter.GetBytes(classId));

        public TagReferenceGen5(IModule module, EndianReader reader)
        {
            this.module = module ?? throw new ArgumentNullException(nameof(module));
            ArgumentNullException.ThrowIfNull(reader);

            reader.BaseStream.Position += 8; //padding
            if (module.ModuleType < ModuleType.HaloInfinite)
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
