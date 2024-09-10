using Newtonsoft.Json.Linq;
using Reclaimer.Blam.HaloInfinite;
using Reclaimer.IO;
using Reclaimer.Utilities;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;

namespace Reclaimer.Plugins.MetaViewer.HaloInfinite
{
    public class TagReferenceValue : MetaValue
    {
        private static readonly ComboBoxItem<TagClass> externalClassOption = new ComboBoxItem<TagClass>("external", new TagClass(null, null), false);
        private static readonly ComboBoxItem<ModuleItem> externalTagOption = new ComboBoxItem<ModuleItem>("[[external reference]]", null, false);

        private Reclaimer.Blam.HaloInfinite.TagReference referenceValue;

        private ComboBoxItem<ModuleItem> selectedItem;
        public ComboBoxItem<ModuleItem> SelectedItem
        {
            get => selectedItem;
            set => SetMetaProperty(ref selectedItem, value);
        }

        private ComboBoxItem<TagClass> selectedClass;
        public ComboBoxItem<TagClass> SelectedClass
        {
            get => selectedClass;
            set
            {
                if (SetProperty(ref selectedClass, value))
                    OnClassChanged();
            }
        }

        public override string EntryString => Utils.GetFileName(SelectedItem?.Label);

        public ObservableCollection<ComboBoxItem<TagClass>> ClassOptions { get; }
        public ObservableCollection<ComboBoxItem<ModuleItem>> TagOptions { get; }

        public TagReferenceValue(XmlNode node, ModuleItem item, MetadataHeader header, DataBlock host, EndianReader reader, long baseAddress, int offset)
            : base(node, item, header, host, reader, baseAddress, offset)
        {
            var allClasses = item.Module.GetTagClasses()
                .Select(t => new ComboBoxItem<TagClass>(t.ClassName, t))
                .OrderBy(s => s.Label);

            ClassOptions = new ObservableCollection<ComboBoxItem<TagClass>>(allClasses);
            TagOptions = new ObservableCollection<ComboBoxItem<ModuleItem>>();

            ClassOptions.Insert(0, externalClassOption);
            TagOptions.Insert(0, externalTagOption);

            ReadValue(reader);
        }

        private void OnClassChanged()
        {
            TagOptions.Clear();
            TagOptions.Insert(0, externalTagOption);

            var classTags = from t in item.Module.GetItemsByClass(SelectedClass.Context.ClassCode)
                            orderby t.TagName
                            select t;

            foreach (var t in classTags)
                TagOptions.Add(new ComboBoxItem<ModuleItem>(t.TagName, t));

            SelectedItem = TagOptions.Skip(1).FirstOrDefault();
        }

        public override void ReadValue(EndianReader reader)
        {
            IsEnabled = true;

            try
            {
                reader.Seek(ValueAddress, SeekOrigin.Begin);

                referenceValue = new TagReference(item.Module, reader);

                if (referenceValue.TagId != -1 && referenceValue.Tag == null)
                {
                    SelectedClass = ClassOptions[0];
                    SelectedItem = TagOptions[0];
                }   
                else
                {
                    SelectedClass = ClassOptions.FirstOrDefault(i => i.Label == referenceValue.Tag?.ClassName);
                    SelectedItem = TagOptions.FirstOrDefault(i => i.Context != null && i.Context == referenceValue.Tag);
                }

                IsDirty = false;
            }
            catch { IsEnabled = false; }
        }

        public override void WriteValue(EndianWriter writer)
        {
            throw new NotImplementedException();
        }

        public override JToken GetJValue() => SelectedItem.Context == null ? null : new JValue($"{SelectedItem.Context.TagName}.{SelectedItem.Context.ClassName}");
    }
}
