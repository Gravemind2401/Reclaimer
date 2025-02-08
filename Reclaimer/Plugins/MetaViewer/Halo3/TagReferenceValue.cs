using Newtonsoft.Json.Linq;
using Reclaimer.Blam.Common;
using Reclaimer.IO;
using Reclaimer.Utilities;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;

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
            set => SetProperty(ref selectedClass, value, OnClassChanged);
        }

        public override string EntryString => Utils.GetFileName(SelectedItem?.Context?.TagName);

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
                            where t.ClassName == SelectedClass?.Label
                            orderby t.TagName
                            select t;

            foreach (var t in classTags)
                TagOptions.Add(new ComboBoxItem<IIndexItem>(t.TagName, t));

            SelectedItem = TagOptions.First();
        }

        public override void ReadValue(EndianReader reader)
        {
            IsBusy = true;
            IsEnabled = true;

            try
            {
                reader.Seek(ValueAddress, SeekOrigin.Begin);

                if (bool.TryParse(Node.Attributes["withGroup"]?.Value, out var withGroup) && !withGroup)
                {
                    var tagId = reader.ReadInt32();
                    var tagIndex = (short)(tagId & ushort.MaxValue);
                    if (tagIndex < 0)
                        referenceValue = TagReference.NullReference;
                    else
                    {
                        var classId = context.Cache.TagIndex[tagIndex].ClassId;
                        referenceValue = new TagReference(context.Cache, classId, tagId);
                    }
                }
                else
                {
                    referenceValue = new TagReference(context.Cache, reader);
                }

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

        public override JToken GetJValue() => SelectedItem == null ? null : new JValue($"{SelectedItem.Context.TagName}.{SelectedItem.Context.ClassName}");
    }
}
