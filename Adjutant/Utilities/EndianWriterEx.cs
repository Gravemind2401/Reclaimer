using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Endian;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Utilities
{
    public class EndianWriterEx : EndianWriter
    {
        public EndianWriterEx(Stream input, ByteOrder byteOrder)
            : this(input, byteOrder, false)
        {

        }

        public EndianWriterEx(Stream input, ByteOrder byteOrder, bool leaveOpen)
            : base(input, byteOrder, leaveOpen)
        {

        }

        protected EndianWriterEx(EndianWriterEx parent, long virtualOrigin)
            : base(parent, virtualOrigin)
        {

        }

        public override EndianWriter CreateVirtualWriter()
        {
            return CreateVirtualWriter(BaseStream.Position);
        }

        public override EndianWriter CreateVirtualWriter(long origin)
        {
            return new EndianWriterEx(this, origin);
        }

        protected override void WriteObject(object value, double? version)
        {
            var writeable = value as IWriteable;
            if (writeable != null)
                writeable.Write(this, version);
            else base.WriteObject(value, version);
        }
    }
}
