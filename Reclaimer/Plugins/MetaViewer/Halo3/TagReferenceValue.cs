using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Reclaimer.Blam.Common;
using Reclaimer.IO;
using Reclaimer.Utilities;

namespace Reclaimer.Plugins.MetaViewer.Halo3
{
    public class TagReferenceValue : MetaValue
    {
        private TagReference referenceValue;

        private ComboBoxItem<IIndexItem> selectedItem;
        public ComboBoxItem<IIndexItem> SelectedItem
        {
            get => selectedItem;
            set => SetMetaProperty(ref selectedItem, value);
        }

        private ComboBoxItem selectedClass;
        public ComboBoxItem SelectedClass
        {
            get => selectedClass;
            set
            {
                if (SetProperty(ref selectedClass, value))
                    OnClassChanged();
            }
        }

        public override string EntryString => Utils.GetFileName(SelectedItem?.Context?.FullPath);

        public ObservableCollection<ComboBoxItem> ClassOptions { get; }
        public ObservableCollection<ComboBoxItem<IIndexItem>> TagOptions { get; }

        public TagReferenceValue(XmlNode node, MetaContext context, EndianReader reader, long baseAddress)
            : base(node, context, reader, baseAddress)
        {
            var allClasses = context.Cache.TagIndex
                .Select(i => i.ClassName)
                .Distinct()
                .OrderBy(s => s)
                .Select(s => new ComboBoxItem(s));

            ClassOptions = new ObservableCollection<ComboBoxItem>(allClasses);
            TagOptions = new ObservableCollection<ComboBoxItem<IIndexItem>>();

            ReadValue(reader);
        }

        private void OnClassChanged()
        {
            TagOptions.Clear();
            var classTags = from t in context.Cache.TagIndex
                            where t.ClassName == SelectedClass.Label
                            orderby t.FullPath
                            select t;

            foreach (var t in classTags)
                TagOptions.Add(new ComboBoxItem<IIndexItem>(t.FullPath, t));

            SelectedItem = TagOptions.First();
        }

        public override void ReadValue(EndianReader reader)
        {
            IsBusy = true;
            IsEnabled = true;

            try
            {
                reader.Seek(ValueAddress, SeekOrigin.Begin);

                referenceValue = new TagReference(context.Cache, reader);
                SelectedClass = ClassOptions.FirstOrDefault(i => i.Label == referenceValue.Tag?.ClassName);
                SelectedItem = TagOptions.FirstOrDefault(i => i.Context != null && i.Context == referenceValue.Tag);

                IsDirty = false;
            }
            catch { IsEnabled = false; }

            IsBusy = false;
        }

        public override void WriteValue(EndianWriter writer)
        {
            if (SelectedItem == null) //null during class transition
                return;

            var tag = SelectedItem.Context;
            referenceValue = new TagReference(context.Cache, tag.ClassId, tag.Id);

            writer.Seek(ValueAddress, SeekOrigin.Begin);
            referenceValue.Write(writer);
        }
    }
}
