using Adjutant.Blam.Common;
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

namespace Reclaimer.Plugins.MetaViewer.Halo3
{
    public class TagReferenceValue : MetaValue
    {
        private int _value;
        public int Value
        {
            get { return _value; }
            set { SetMetaProperty(ref _value, value); }
        }

        private string selectedClass;
        public string SelectedClass
        {
            get { return selectedClass; }
            set
            {
                if (SetProperty(ref selectedClass, value))
                    OnClassChanged();
            }
        }

        public override string EntryString => Utils.GetFileName(cache.TagIndex[Value].FullPath);

        public ObservableCollection<string> ClassOptions { get; }
        public ObservableCollection<IIndexItem> TagOptions { get; }

        public TagReferenceValue(XmlNode node, ICacheFile cache, EndianReader reader, long baseAddress)
            : base(node, cache, reader, baseAddress)
        {
            var allClasses = cache.TagIndex
                .Select(i => i.ClassName)
                .Distinct()
                .OrderBy(s => s);

            ClassOptions = new ObservableCollection<string>(allClasses);
            TagOptions = new ObservableCollection<IIndexItem>();

            ReadValue(reader);
        }

        private void OnClassChanged()
        {
            TagOptions.Clear();
            var classTags = from t in cache.TagIndex
                            where t.ClassName == SelectedClass
                            orderby t.FullPath
                            select t;

            foreach (var t in classTags)
                TagOptions.Add(t);

            Value = (short)TagOptions.First().Id;
        }

        public override void ReadValue(EndianReader reader)
        {
            IsEnabled = true;

            try
            {
                reader.Seek(ValueAddress, SeekOrigin.Begin);

                if (cache.CacheType >= CacheType.Halo3Beta)
                    reader.Seek(14, SeekOrigin.Current);
                else if (cache.CacheType >= CacheType.Halo2Xbox)
                    reader.Seek(4, SeekOrigin.Current);
                else
                    reader.Seek(12, SeekOrigin.Current);

                var tagId = reader.ReadInt16();
                SelectedClass = cache.TagIndex[tagId].ClassName;
                Value = tagId;

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
