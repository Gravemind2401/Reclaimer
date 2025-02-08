using Reclaimer.Blam.Common.Gen5;
using Reclaimer.IO;
using System.Numerics;

namespace Reclaimer.Blam.HaloInfinite
{
    public class CustomizationAttachmentConfigurationTag
    {
        [Offset(16)]
        public BlockCollection<ModelAttachment> ModelAttachments { get; set; }
    }

    [FixedSize(56)]
    public class ModelAttachment
    {
        [Offset(0)]
        public TagReferenceGen5 AttachmentModel { get; set; }

        [Offset(32)]
        public BlockCollection<AttachmentMarker> Markers { get; set; }
    }

    [FixedSize(40)]
    public class AttachmentMarker
    {
        [Offset(0)]
        public StringHashGen5 MarkerName { get; set; }

        [Offset(4)]
        public Vector3 Translation { get; set; }

        [Offset(16)]
        public Vector3 Rotation { get; set; }

        [Offset(28)]
        public Vector3 Scale { get; set; }
    }
}