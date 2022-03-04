using Reclaimer.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Blam.Halo5
{
    [FixedSize(32)]
    public struct TagReference
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
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            this.module = module ?? throw new ArgumentNullException(nameof(module));

            reader.BaseStream.Position += 8; //padding
            reader.ReadInt32(); //index?
            globalTagId = reader.ReadInt32();
            globalAssetId = reader.ReadInt64();
            classId = reader.ReadInt32(ByteOrder.BigEndian);
            reader.BaseStream.Position += 4; //padding
        }

        public override string ToString() => Tag?.ToString();

        #region Equality Operators

        public static bool operator ==(TagReference value1, TagReference value2)
        {
            return value1.module != null && value2.module != null && value1.module == value2.module && value1.globalTagId == value2.globalTagId;
        }

        public static bool operator !=(TagReference value1, TagReference value2)
        {
            return !(value1 == value2);
        }

        public static bool Equals(TagReference value1, TagReference value2)
        {
            return value1.module != null && value2.module != null && value1.module.Equals(value2.module) && value1.globalTagId.Equals(value2.globalTagId);
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !(obj is TagReference))
                return false;

            return TagReference.Equals(this, (TagReference)obj);
        }

        public bool Equals(TagReference value)
        {
            return TagReference.Equals(this, value);
        }

        public override int GetHashCode()
        {
            return module?.GetHashCode() ?? 0 ^ globalTagId.GetHashCode();
        }

        #endregion
    }
}
