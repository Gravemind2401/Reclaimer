using Newtonsoft.Json.Linq;
using Reclaimer.Blam.HaloInfinite;
using Reclaimer.IO;
using Reclaimer.Utilities;
using System.Collections.ObjectModel;
using System.Xml;

namespace Reclaimer.Plugins.MetaViewer.HaloInfinite
{
    public class ArrayValue : MetaValue, IExpandable
    {
        private bool isExpanded;
        public bool IsExpanded
        {
            get => isExpanded;
            set => SetProperty(ref isExpanded, value);
        }

        public bool HasChildren => Children.Any();
        public ObservableCollection<MetaValueBase> Children { get; }

        IEnumerable<MetaValueBase> IExpandable.Children => Children;

        public ArrayValue(XmlNode node, ModuleItem item, MetadataHeader header, DataBlock host, EndianReader reader, long baseAddress, int offset)
            : base(node, item, header, host, reader, baseAddress, offset)
        {
            Children = new ObservableCollection<MetaValueBase>();
            IsExpanded = true;
            ReadValue(reader);
        }

        public override void ReadValue(EndianReader reader)
        {
            IsEnabled = true;

            try
            {
                Children.Clear();

                var offset = Offset;
                foreach (var n in node.GetChildElements())
                {
                    var def = FieldDefinition.GetHaloInfiniteDefinition(n);
                    Children.Add(GetMetaValue(n, item, header, host, reader, BaseAddress, offset));
                    offset += def.Size;
                }

                foreach (var c in Children)
                    c.ReadValue(reader);

                RaisePropertyChanged(nameof(HasChildren));
            }
            catch { IsEnabled = false; }
        }

        public override void WriteValue(EndianWriter writer)
        {
            throw new NotImplementedException();
        }

        public override JToken GetJValue()
        {
            var result = new JObject();
            foreach (var item in Children.Where(c => c.FieldDefinition.ValueType != MetaValueType.Comment))
            {
                if (result.ContainsKey(item.Name))
                    result.Add(item.Name + item.Offset, item.GetJValue());
                else
                    result.Add(item.Name, item.GetJValue());
            }
            return result;
        }
    }
}
