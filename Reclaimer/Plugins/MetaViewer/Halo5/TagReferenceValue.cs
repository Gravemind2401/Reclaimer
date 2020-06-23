using Adjutant.Blam.Halo5;
using Reclaimer.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Reclaimer.Plugins.MetaViewer.Halo5
{
    public class TagReferenceValue : MetaValue
    {
        private readonly ComboBoxItem externalClassOption = new ComboBoxItem("external", false);
        private readonly ComboBoxItem<ModuleItem> externalTagOption = new ComboBoxItem<ModuleItem>("[[external reference]]", null, false);

        private TagReference referenceValue;

        private ComboBoxItem<ModuleItem> selectedItem;
        public ComboBoxItem<ModuleItem> SelectedItem
        {
            get { return selectedItem; }
            set { SetMetaProperty(ref selectedItem, value); }
        }

        private ComboBoxItem selectedClass;
        public ComboBoxItem SelectedClass
        {
            get { return selectedClass; }
            set
            {
                if (SetProperty(ref selectedClass, value))
                    OnClassChanged();
            }
        }

        public override string EntryString => Utils.GetFileName(SelectedItem?.Label);

        public ObservableCollection<ComboBoxItem> ClassOptions { get; }
        public ObservableCollection<ComboBoxItem<ModuleItem>> TagOptions { get; }

        public TagReferenceValue(XmlNode node, ModuleItem item, MetadataHeader header, DataBlock host, EndianReader reader, long baseAddress, int offset)
            : base(node, item, header, host, reader, baseAddress, offset)
        {
            var allClasses = item.Module.Items
                .Select(i => new ComboBoxItem(i.ClassName))
                .Distinct()
                .OrderBy(s => s.Label);

            ClassOptions = new ObservableCollection<ComboBoxItem>(allClasses);
            TagOptions = new ObservableCollection<ComboBoxItem<ModuleItem>>();

            ClassOptions.Insert(0, externalClassOption);
            TagOptions.Insert(0, externalTagOption);

            ReadValue(reader);
        }

        private void OnClassChanged()
        {
            TagOptions.Clear();
            TagOptions.Insert(0, externalTagOption);

            var classTags = from t in item.Module.Items
                            where t.ClassName == SelectedClass.Label
                            orderby t.FullPath
                            select t;

            foreach (var t in classTags)
                TagOptions.Add(new ComboBoxItem<ModuleItem>(t.FullPath, t));

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
    }
}
