using Reclaimer.IO;
using System.Xml;

namespace Reclaimer.Plugins.MetaViewer.Halo3
{
    public abstract class MetaValue : MetaValueBase
    {
        protected readonly MetaContext context;

        protected MetaValue(XmlNode node, MetaContext context, EndianReader reader, long baseAddress)
            : base(node, baseAddress)
        {
            this.context = context;
            FieldDefinition = FieldDefinition.GetHalo3Definition(node);
            context.AddValue(node, this);
        }

        protected override void OnMetaPropertyChanged(string propertyName)
        {
            using (var writer = new EndianWriter(context.DataSource, context.Cache.ByteOrder, true))
                WriteValue(writer);
        }
    }
}
