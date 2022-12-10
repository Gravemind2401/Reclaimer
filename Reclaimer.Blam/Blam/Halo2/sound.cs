using Reclaimer.Audio;
using Reclaimer.Blam.Common;
using Reclaimer.Blam.Utilities;
using Reclaimer.IO;
using System;
using System.Linq;

namespace Reclaimer.Blam.Halo2
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
        public Encoding Encoding { get; set; }

        [Offset(5)]
        public CompressionCodec CompressionCodec { get; set; }

        [Offset(6)]
        public short PlaybackIndex { get; set; }

        [Offset(8)]
        public short PitchRangeIndex { get; set; }

        [Offset(11)]
        public byte ScaleIndex { get; set; }

        [Offset(12)]
        public byte PromotionIndex { get; set; }

        [Offset(13)]
        public byte CustomPlaybackIndex { get; set; }

        [Offset(14)]
        public short ExtraInfoIndex { get; set; }

        [Offset(16)]
        public int MaxPlaytime { get; set; }

        public int SampleRateInt
        {
            get
            {
                return SampleRate switch
                {
                    SampleRate.x22050Hz => 22050,
                    SampleRate.x32000Hz => 32000,
                    SampleRate.x44100Hz => 44100,
                    _ => throw new NotSupportedException("Sample Rate not supported")
                };
            }
        }

        #region ISoundContainer

        string ISoundContainer.Name => item.FullPath;

        string ISoundContainer.Class => item.ClassName;

        public GameSound ReadData()
        {
            if (CompressionCodec != CompressionCodec.WMA && CompressionCodec != CompressionCodec.XboxAdpcm)
                throw new NotSupportedException("Unsupported Codec/Encoding");

            var resourceGestalt = cache.TagIndex.GetGlobalTag("ugh!").ReadMetadata<sound_cache_file_gestalt>();
            var pitchRange = resourceGestalt.PitchRanges[PitchRangeIndex];

            var result = new GameSound { Name = item.FileName };
            if (CompressionCodec == CompressionCodec.XboxAdpcm)
            {
                if (Encoding != Encoding.Codec)
                    result.FormatHeader = new XboxAdpcmHeader(SampleRateInt, (byte)(Encoding + 1));

                result.DefaultExtension = "wav";
            }
            else if (CompressionCodec == CompressionCodec.WMA)
                result.DefaultExtension = "wma";

            for (var i = 0; i < pitchRange.PermutationCount; i++)
            {
                var perm = resourceGestalt.SoundPermutations[pitchRange.FirstPermutationIndex + i];
                var name = resourceGestalt.SoundNames[perm.NameIndex].Name;

                byte[] permData;

                if (perm.BlockCount == 1)
                {
                    //skip the array copy
                    var block = resourceGestalt.SoundPermutationChunks[perm.BlockIndex];
                    permData = block.DataPointer.ReadData(block.DataSize);
                }
                else
                {
                    var blocks = Enumerable.Range(perm.BlockIndex, perm.BlockCount)
                        .Select(x => resourceGestalt.SoundPermutationChunks[x])
                        .ToList();

                    permData = new byte[blocks.Sum(b => b.DataSize)];
                    var offset = 0;
                    foreach (var block in blocks)
                    {
                        var sourceData = block.DataPointer.ReadData(block.DataSize);

                        Array.Copy(sourceData, 0, permData, offset, sourceData.Length);
                        offset += sourceData.Length;
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

    public enum SampleRate : byte
    {
        x22050Hz = 0,
        x44100Hz = 1,
        x32000Hz = 2
    }

    public enum Encoding : byte
    {
        Mono = 0,
        Stereo = 1,
        Codec = 2,
    }

    public enum CompressionCodec : byte
    {
        BigEndian = 0,
        XboxAdpcm = 1,
        ImaAdpcm = 2,
        LittleEndian = 3,
        WMA = 4
    }
}
