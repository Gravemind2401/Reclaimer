﻿using Reclaimer.Audio;
using Reclaimer.Blam.Common;
using Reclaimer.IO;

namespace Reclaimer.Blam.HaloReach
{
    public class SoundTag : ContentTagDefinition<GameSound>
    {
        public SoundTag(IIndexItem item)
            : base(item)
        { }

        [Offset(0)]
        public short Flags { get; set; }

        [Offset(2)]
        public byte SoundClass { get; set; }

        [Offset(3)]
        public SampleRate SampleRate { get; set; }

        [Offset(4)]
        public byte Encoding { get; set; }

        [Offset(5)]
        public byte CodecIndex { get; set; }

        [Offset(6)]
        public short PlaybackIndex { get; set; }

        [Offset(28)]
        public ResourceIdentifier ResourceIdentifier { get; set; }

        #region IContentProvider

        public override GameSound GetContent()
        {
            var resourceGestalt = Cache.TagIndex.GetGlobalTag("ugh!").ReadMetadata<SoundCacheFileGestaltTag>();
            var playback = resourceGestalt.Playbacks[PlaybackIndex];
            var codec = resourceGestalt.Codecs[CodecIndex];
            var sourceData = ResourceIdentifier.ReadSoundData();

            var result = new GameSound
            {
                Name = Item.FileName,
                FormatHeader = new XmaHeader(codec.SampleRateInt, codec.ChannelCounts),
                DefaultExtension = "xma"
            };

            for (var i = 0; i < playback.PermutationCount; i++)
            {
                var perm = resourceGestalt.SoundPermutations[playback.FirstPermutationIndex + i];
                var name = resourceGestalt.SoundNames[perm.NameIndex].Name;

                byte[] permData;

                if (playback.PermutationCount == 1)
                {
                    //skip the array copy
                    permData = sourceData;
                }
                else
                {
                    var blocks = Enumerable.Range(perm.BlockIndex, perm.BlockCount)
                        .Select(x => resourceGestalt.DataBlocks[x])
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
