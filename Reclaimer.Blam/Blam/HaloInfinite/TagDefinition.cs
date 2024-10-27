namespace Reclaimer.Blam.HaloInfinite
{
    public abstract class TagDefinition
    {
        protected readonly ModuleItem Item;
        protected readonly Module Module;

        public MetadataHeader Header { get; }

        public TagDefinition(ModuleItem item, MetadataHeader header)
        {
            Item = item;
            Module = item.Module;
            Header = header;
        }
    }
}
