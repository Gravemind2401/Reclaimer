using Reclaimer.Blam.Common;
using Reclaimer.IO;
using Reclaimer.IO.Dynamic;

namespace Reclaimer.Blam.Halo3
{
    [StructureDefinition<cache_file_resource_gestalt, DefinitionBuilder>]
    public partial class cache_file_resource_gestalt
    {
        private sealed class DefinitionBuilder : DefinitionBuilder<cache_file_resource_gestalt>
        {
            public DefinitionBuilder()
            {
                PreRelease();
                Halo3();
                Odst();
            }

            private void PreRelease()
            {
                var builder = AddVersion(null, CacheType.Halo3Delta);
                builder.Property(x => x.ResourceEntries).HasOffset(24);
                builder.Property(x => x.FixupDataSize).HasOffset(48);
                builder.Property(x => x.FixupDataPointer).HasOffset(60);

                builder = AddVersion(CacheType.Halo3Delta, null);
                builder.Property(x => x.ResourceEntries).HasOffset(36);
                builder.Property(x => x.FixupDataSize).HasOffset(132);
                builder.Property(x => x.FixupDataPointer).HasOffset(144);
            }

            private void Halo3()
            {
                var builder = AddVersion(CacheType.Halo3Retail, null);
                builder.Property(x => x.ResourceEntries).HasOffset(88);
                builder.Property(x => x.FixupDataSize).HasOffset(316);
                builder.Property(x => x.FixupDataPointer).HasOffset(328);

                builder = AddVersion(CacheType.MccHalo3, null);
                builder.Property(x => x.ResourceEntries).HasOffset(100);
                builder.Property(x => x.FixupDataSize).HasOffset(328);
                builder.Property(x => x.FixupDataPointer).HasOffset(340);
            }

            private void Odst()
            {
                var builder = AddVersion(CacheType.Halo3ODST, null);
                builder.Property(x => x.ResourceEntries).HasOffset(88);
                builder.Property(x => x.FixupDataSize).HasOffset(316);
                builder.Property(x => x.FixupDataPointer).HasOffset(328);

                builder = AddVersion(CacheType.MccHalo3ODST, null);
                builder.Property(x => x.ResourceEntries).HasOffset(100);
                builder.Property(x => x.FixupDataSize).HasOffset(328);
                builder.Property(x => x.FixupDataPointer).HasOffset(340);
            }
        }
    }

    [StructureDefinition<ResourceEntryBlock, DefinitionBuilder>]
    public partial class ResourceEntryBlock
    {
        private sealed class DefinitionBuilder : DefinitionBuilder<ResourceEntryBlock>
        {
            public DefinitionBuilder()
            {
                var builder = AddVersion(CacheType.Halo3Alpha).HasFixedSize(88);
                builder.Property(x => x.OwnerReference).HasOffset(0);
                builder.Property(x => x.ResourcePointer).HasOffset(16);
                builder.Property(x => x.FixupOffset).HasOffset(24);
                builder.Property(x => x.FixupSize).HasOffset(28);
                builder.Property(x => x.PrimaryOffset).HasOffset(36);
                builder.Property(x => x.PrimarySize).HasOffset(40);
                builder.Property(x => x.SecondaryOffset).HasOffset(48);
                builder.Property(x => x.SecondarySize).HasOffset(52);
                builder.Property(x => x.ResourceFixups).HasOffset(64);
                builder.Property(x => x.ResourceDefinitionFixups).HasOffset(76);

                builder = AddVersion(CacheType.Halo3Delta, null).HasFixedSize(96);
                builder.Property(x => x.OwnerReference).HasOffset(0);
                builder.Property(x => x.ResourcePointer).HasOffset(16);
                builder.Property(x => x.FixupOffset).HasOffset(24);
                builder.Property(x => x.FixupSize).HasOffset(28);
                builder.Property(x => x.CacheIndex).HasOffset(36);
                builder.Property(x => x.PrimaryOffset).HasOffset(40);
                builder.Property(x => x.PrimarySize).HasOffset(44);
                builder.Property(x => x.CacheIndex2).HasOffset(52);
                builder.Property(x => x.SecondaryOffset).HasOffset(56);
                builder.Property(x => x.SecondarySize).HasOffset(60);
                builder.Property(x => x.ResourceFixups).HasOffset(72);
                builder.Property(x => x.ResourceDefinitionFixups).HasOffset(84);

                builder = AddVersion(CacheType.Halo3Retail, null).HasFixedSize(64);
                builder.Property(x => x.OwnerReference).HasOffset(0);
                builder.Property(x => x.ResourcePointer).HasOffset(16);
                builder.Property(x => x.FixupOffset).HasOffset(20);
                builder.Property(x => x.FixupSize).HasOffset(24);
                builder.Property(x => x.LocationType).HasOffset(32);
                builder.Property(x => x.SegmentIndex).HasOffset(34);
                builder.Property(x => x.ResourceFixups).HasOffset(40);
                builder.Property(x => x.ResourceDefinitionFixups).HasOffset(52);
            }
        }
    }

    [StructureDefinition<ResourceFixupBlock, DefinitionBuilder>]
    public partial class ResourceFixupBlock
    {
        private sealed class DefinitionBuilder : DefinitionBuilder<ResourceFixupBlock>
        {
            public DefinitionBuilder()
            {
                var builder = AddDefaultVersion().HasFixedSize(8);
                builder.Property(x => x.Unknown).HasOffset(0);
                builder.Property(x => x.Offset).HasOffset(4);
            }
        }
    }

    [StructureDefinition<ResourceDefinitionFixupBlock, DefinitionBuilder>]
    public partial class ResourceDefinitionFixupBlock
    {
        private sealed class DefinitionBuilder : DefinitionBuilder<ResourceDefinitionFixupBlock>
        {
            public DefinitionBuilder()
            {
                var builder = AddDefaultVersion().HasFixedSize(8);
                builder.Property(x => x.Offset).HasOffset(0);
                builder.Property(x => x.Unknown).HasOffset(4);
            }
        }
    }
}
