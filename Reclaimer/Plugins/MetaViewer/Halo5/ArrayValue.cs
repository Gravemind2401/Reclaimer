using Newtonsoft.Json.Linq;
using Reclaimer.Blam.Common.Gen5;
using Reclaimer.IO;
using Reclaimer.Utilities;
using System.Collections.ObjectModel;
using System.Xml;

namespace Reclaimer.Plugins.MetaViewer.Halo5
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

        public ArrayValue(XmlNode node, IModuleItem item, IMetadataHeader header, DataBlock host, EndianReader reader, long baseAddress, int offset)
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
                    var def = FieldDefinition.GetHalo5Definition(n);
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
                var propName = result.ContainsKey(item.Name) ? $"{item.Name}_{item.Offset}" : item.Name;
                result.Add(propName, item.GetJValue());
            }

            return result;
        }
    }
}
