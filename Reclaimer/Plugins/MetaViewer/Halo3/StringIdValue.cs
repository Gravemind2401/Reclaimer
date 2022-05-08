using Reclaimer.Blam.Common;
using Reclaimer.Controls;
using Reclaimer.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Reclaimer.Plugins.MetaViewer.Halo3
{
    public class StringIdValue : MetaValue, AutoCompleteTextBox.ISuggestionProvider
    {
        public override string EntryString => Value;

        private string _value;
        public string Value
        {
            get => _value;
            set => SetMetaProperty(ref _value, value);
        }

        public StringIdValue(XmlNode node, MetaContext context, EndianReader reader, long baseAddress)
            : base(node, context, reader, baseAddress)
        {
            ReadValue(reader);
        }

        public override void ReadValue(EndianReader reader)
        {
            IsBusy = true;
            IsEnabled = true;

            try
            {
                reader.Seek(ValueAddress, SeekOrigin.Begin);
                Value = new StringId(reader, context.Cache);

                IsDirty = false;
            }
            catch { IsEnabled = false; }

            IsBusy = false;
        }

        public override void WriteValue(EndianWriter writer)
        {
            writer.Seek(ValueAddress, SeekOrigin.Begin);

            var intValue = GetStringId(Value);
            if (string.IsNullOrEmpty(Value) || intValue > 0)
                writer.Write(intValue);

            IsDirty = false;
        }

        private int GetStringId(string value)
        {
            return string.IsNullOrEmpty(value) ? 0 : context.Cache.StringIndex.GetStringId(value);
        }

        protected internal override bool HasCustomValidation => true;

        protected internal override bool ValidateValue(object value)
        {
            var str = value?.ToString();
            return string.IsNullOrEmpty(str) || GetStringId(str) > 0;
        }

        IEnumerable<string> AutoCompleteTextBox.ISuggestionProvider.GetSuggestions(string text)
        {
            if (string.IsNullOrEmpty(text))
                return Enumerable.Empty<string>();
            else if (text.Length < 3)
                return context.Cache.StringIndex.Where(s => s.Length < 3 && s?.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0).OrderBy(s => s);
            else
                return context.Cache.StringIndex.Where(s => s.Length >= 3 && s?.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0).OrderBy(s => s);
        }
    }
}
