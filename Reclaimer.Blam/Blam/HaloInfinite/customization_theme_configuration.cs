using Reclaimer.Blam.Common;
using Reclaimer.Blam.Utilities;
using Reclaimer.Geometry;
using Reclaimer.IO;
using Reclaimer.Utilities;
using System.Numerics;

namespace Reclaimer.Blam.HaloInfinite
{
    public class customization_theme_configuration
    {
        [Offset(16)]
        public BlockCollection<ObjectRegion> Regions { get; set; }
        [Offset(36)]
        public BlockCollection<TagReference> Attachments {  get; set; }
        [Offset(56)]
        public BlockCollection<ObjectRegion> Prosthetics { get; set; }
        [Offset(76)]
        public BlockCollection<ObjectRegion> BodyTypes { get; set; }
    }

    [FixedSize(44)]
    public class ObjectRegion
    {
        [Offset(0)]
        public StringHash RegionName { get; set; }
        [Offset(4)]
        public BlockCollection<StringHash> PermutationRegions { get; set; }
        [Offset(24)]
        public BlockCollection<PermutationSetting> PermutationSettings { get; set; }
    }

    [FixedSize(60)]
    public class PermutationSetting
    {
        [Offset(0)]
        public StringHash Name { get; set; }
        [Offset(4)]
        public TagReference Style { get; set; }
        [Offset(32)]
        public TagReference Attachment { get; set; }
    }
}
