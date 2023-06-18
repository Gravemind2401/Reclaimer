using Newtonsoft.Json.Linq;
using Reclaimer.IO;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;

namespace Reclaimer.Plugins.MetaViewer.Halo3
{
    public class BlockIndexValue : MetaValue
    {
        private readonly string targetId;

        public override string EntryString => Value.ToString();

        private object _value;
        public object Value
        {
            get => _value;
            set => SetMetaProperty(ref _value, value);
        }

        public ObservableCollection<Tuple<int, string>> Options { get; }

        public BlockIndexValue(XmlNode node, MetaContext context, EndianReader reader, long baseAddress)
            : base(node, context, reader, baseAddress)
        {
            targetId = node.GetStringAttribute("targetGuid", "targetId", "targetName");

            Options = new ObservableCollection<Tuple<int, string>>();
            ReadValue(reader);
        }

        private void Context_BlockAdded(object sender, EventArgs e) => ReadOptions();

        internal void ReadOptions()
        {
            Options.Clear();
            var target = (context.GetValue($"//*[@srcGuid='{targetId}']")
                ?? context.GetValue($"//*[@srcId='{targetId}']")
                ?? context.GetValue($"//*[@srcName='{targetId}']")) as StructureValue;

            if (target == null)
                return;

            var defaultIndex = FieldDefinition.ValueType == MetaValueType.BlockIndex8 ? 255 : -1;
            Options.Add(Tuple.Create(defaultIndex, "<none>"));
            Options.AddRange(target.BlockLabels.Select((s, i) => Tuple.Create(i, s)));
        }

        public override void ReadValue(EndianReader reader)
        {
            IsBusy = true;
            IsEnabled = true;

            try
            {
                if (string.IsNullOrEmpty(targetId))
                    throw new InvalidOperationException();

                reader.Seek(ValueAddress, SeekOrigin.Begin);

                ReadOptions();

                switch (FieldDefinition.ValueType)
                {
                    case MetaValueType.BlockIndex8:
                        Value = reader.ReadByte();
                        break;
                    case MetaValueType.BlockIndex16:
                        Value = reader.ReadInt16();
                        break;
                    case MetaValueType.BlockIndex32:
                        Value = reader.ReadInt32();
                        break;
                }

                IsDirty = false;
            }
            catch { IsEnabled = false; }

            IsBusy = false;
        }

        public override void WriteValue(EndianWriter writer)
        {
            writer.Seek(ValueAddress, SeekOrigin.Begin);

            var parsed = int.Parse(Value.ToString());

            switch (FieldDefinition.ValueType)
            {
                case MetaValueType.BlockIndex8:
                    writer.Write((byte)parsed);
                    break;
                case MetaValueType.BlockIndex16:
                    writer.Write((short)parsed);
                    break;
                case MetaValueType.BlockIndex32:
                    writer.Write(parsed);
                    break;
            }

            IsDirty = false;
        }

        public override JToken GetJValue() => new JValue(Value);
    }
}
