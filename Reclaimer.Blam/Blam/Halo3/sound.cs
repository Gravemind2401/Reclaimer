using Reclaimer.Audio;
using Reclaimer.Blam.Common;
using Reclaimer.Blam.Utilities;
using Reclaimer.IO;

namespace Reclaimer.Blam.Halo3
{
    public class sound : ISoundContainer
    {
        private readonly ICacheFile cache;
        private readonly IIndexItem item;

        public sound(ICacheFile cache, IIndexItem item)
        {
            this.cache = cache;
            this.item = item;
        }

        [Offset(0)]
        public short Flags { get; set; }

        [Offset(2)]
        public byte SoundClass { get; set; }

        [Offset(3)]
        public SampleRate SampleRate { get; set; }

        [Offset(4)]
        public short CodecIndex { get; set; }

        [Offset(6)]
        public short PitchRangeIndex { get; set; }

        [Offset(8)]
        public short DialogueUnknown { get; set; }

        [Offset(10)]
        public short Unknown0 { get; set; }

        [Offset(12)]
        public short PlaybackParamsIndex { get; set; }

        [Offset(14)]
        public short ScaleIndex { get; set; }

        [Offset(16)]
        public byte PromotionIndex { get; set; }

        [Offset(17)]
        public byte CustomPlaybackIndex { get; set; }

        [Offset(18)]
        public short ExtraInfoIndex { get; set; }

        [Offset(20)]
        public int MaxPlaytime { get; set; }

        [Offset(24)]
        public ResourceIdentifier ResourceIdentifier { get; set; }

        #region ISoundContainer

        string ISoundContainer.Name => item.FullPath;

        string ISoundContainer.Class => item.ClassName;

        public GameSound ReadData()
        {
            var resourceGestalt = cache.TagIndex.GetGlobalTag("ugh!").ReadMetadata<sound_cache_file_gestalt>();
            var pitchRange = resourceGestalt.PitchRanges[PitchRangeIndex];
            var codec = resourceGestalt.Codecs[CodecIndex];
            var sourceData = ResourceIdentifier.ReadSoundData();

            var result = new GameSound
            {
                Name = item.FileName,
                FormatHeader = new XmaHeader(codec.SampleRateInt, codec.ChannelCounts),
                DefaultExtension = "xma"
            };

            for (var i = 0; i < pitchRange.PermutationCount; i++)
            {
                var perm = resourceGestalt.SoundPermutations[pitchRange.FirstPermutationIndex + i];
                var name = resourceGestalt.SoundNames[perm.NameIndex].Name;

                byte[] permData;

                if (pitchRange.PermutationCount == 1)
                {
                    //skip the array copy
                    permData = sourceData;
                }
                else
                {
                    var blocks = Enumerable.Range(perm.BlockIndex, perm.BlockCount)
                        .Select(x => resourceGestalt.SoundPermutationChunk[x])
                        .ToList();

                    permData = new byte[blocks.Sum(b => b.Size)];
                    var offset = 0;
                    foreach (var block in blocks)
                    {
                        Array.Copy(sourceData, block.FileOffset, permData, offset, block.Size);
                        offset += block.Size;
                    }
                }

                result.Permutations.Add(new GameSoundPermutation
                {
                    Name = name,
                    SoundData = permData
                });
            }

            return result;
        }

        #endregion
    }
}
