using Reclaimer.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Blam.Utilities
{
    public class EndianWriterEx : EndianWriter
    {
        private readonly Dictionary<Type, Action<object, double?>> registeredTypes;

        public EndianWriterEx(Stream input, ByteOrder byteOrder)
            : this(input, byteOrder, false)
        {

        }

        public EndianWriterEx(Stream input, ByteOrder byteOrder, bool leaveOpen)
            : base(input, byteOrder, leaveOpen)
        {
            registeredTypes = new Dictionary<Type, Action<object, double?>>();
        }

        protected EndianWriterEx(EndianWriterEx parent, long virtualOrigin)
            : base(parent, virtualOrigin)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));

            registeredTypes = parent.registeredTypes;
        }

        public void RegisterType<T>(Action<T, double?> writer)
        {
            if (registeredTypes.ContainsKey(typeof(T)))
                throw new ArgumentException(Utils.CurrentCulture($"{typeof(T).Name} has already been registered."));

            registeredTypes.Add(typeof(T), (obj, ver) => writer((T)obj, ver));
        }

        public override EndianWriter CreateVirtualWriter() => CreateVirtualWriter(BaseStream.Position);

        public override EndianWriter CreateVirtualWriter(long origin) => new EndianWriterEx(this, origin);

        protected override void WriteObject(object value, double? version)
        {
            if (value is IWriteable writeable)
                writeable.Write(this, version);
            else if (registeredTypes.ContainsKey(value.GetType()))
                registeredTypes[value.GetType()](value, version);
            else
                base.WriteObject(value, version);
        }
    }
}
