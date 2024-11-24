using Reclaimer.Blam.Common;
using Reclaimer.IO;
using Reclaimer.IO.Dynamic;

namespace Reclaimer.Blam.Halo2
{
    [StructureDefinition<CacheHeader, DefinitionBuilder>]
    public partial class CacheHeader
    {
        private sealed class DefinitionBuilder : DefinitionBuilder<CacheHeader>
        {
            public DefinitionBuilder()
            {
                var builder = AddVersion(CacheType.Halo2Beta);
                builder.Property(x => x.Head).HasOffset(0);
                builder.Property(x => x.FileSize).HasOffset(8);
                builder.Property(x => x.IndexAddress).HasOffset(16);
                builder.Property(x => x.IndexSize).HasOffset(20);
                builder.Property(x => x.BuildString).HasOffset(288).IsNullTerminated(32);
                builder.Property(x => x.StringCount).HasOffset(356);
                builder.Property(x => x.StringTableSize).HasOffset(360);
                builder.Property(x => x.StringTableIndexAddress).HasOffset(364);
                builder.Property(x => x.StringTableAddress).HasOffset(368);

                builder = AddVersion(CacheType.Halo2Xbox);
                builder.Property(x => x.Head).HasOffset(0);
                builder.Property(x => x.FileSize).HasOffset(8);
                builder.Property(x => x.IndexAddress).HasOffset(16);
                builder.Property(x => x.IndexSize).HasOffset(20);
                builder.Property(x => x.BuildString).HasOffset(288).IsNullTerminated(32);
                builder.Property(x => x.StringCount).HasOffset(356);
                builder.Property(x => x.StringTableSize).HasOffset(360);
                builder.Property(x => x.StringTableIndexAddress).HasOffset(364);
                builder.Property(x => x.StringTableAddress).HasOffset(368);
                builder.Property(x => x.ScenarioName).HasOffset(444).IsNullTerminated(256);
                builder.Property(x => x.FileCount).HasOffset(704);
                builder.Property(x => x.FileTableAddress).HasOffset(708);
                builder.Property(x => x.FileTableSize).HasOffset(712);
                builder.Property(x => x.FileTableIndexAddress).HasOffset(716);

                builder = AddVersion(CacheType.Halo2Vista);
                builder.Property(x => x.Head).HasOffset(0);
                builder.Property(x => x.FileSize).HasOffset(8);
                builder.Property(x => x.IndexAddress).HasOffset(16);
                builder.Property(x => x.IndexSize).HasOffset(20);
                builder.Property(x => x.BuildString).HasOffset(300).IsNullTerminated(32);
                builder.Property(x => x.StringCount).HasOffset(0x170);
                builder.Property(x => x.StringTableSize).HasOffset(0x174);
                builder.Property(x => x.StringTableIndexAddress).HasOffset(0x178);
                builder.Property(x => x.StringTableAddress).HasOffset(0x17C);
                builder.Property(x => x.ScenarioName).HasOffset(456).IsNullTerminated(256);
                builder.Property(x => x.FileCount).HasOffset(716);
                builder.Property(x => x.FileTableAddress).HasOffset(720);
                builder.Property(x => x.FileTableSize).HasOffset(724);
                builder.Property(x => x.FileTableIndexAddress).HasOffset(728);
            }
        }
    }
}
