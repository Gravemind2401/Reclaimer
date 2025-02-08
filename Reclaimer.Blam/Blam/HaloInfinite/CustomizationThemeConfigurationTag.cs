using Reclaimer.Blam.Common.Gen5;
using Reclaimer.IO;

namespace Reclaimer.Blam.HaloInfinite
{
    public class CustomizationThemeConfigurationTag
    {
        [Offset(16)]
        public BlockCollection<ObjectRegion> Regions { get; set; }

        [Offset(36)]
        public BlockCollection<TagReferenceGen5> Attachments { get; set; }

        [Offset(56)]
        public BlockCollection<ObjectRegion> Prosthetics { get; set; }

        [Offset(76)]
        public BlockCollection<ObjectRegion> BodyTypes { get; set; }
    }

    [FixedSize(44)]
    public class ObjectRegion
    {
        [Offset(0)]
        public StringHashGen5 RegionName { get; set; }

        [Offset(4)]
        public BlockCollection<StringHashGen5> PermutationRegions { get; set; }

        [Offset(24)]
        public BlockCollection<PermutationSetting> PermutationSettings { get; set; }
    }

    [FixedSize(60)]
    public class PermutationSetting
    {
        [Offset(0)]
        public StringHashGen5 Name { get; set; }

        [Offset(4)]
        public TagReferenceGen5 Style { get; set; }

        [Offset(32)]
        public TagReferenceGen5 Attachment { get; set; }
    }
}
