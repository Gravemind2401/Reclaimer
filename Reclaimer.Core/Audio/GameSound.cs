namespace Reclaimer.Audio
{
    public class GameSound
    {
        public string Name { get; set; }

        public string DefaultExtension { get; set; }

        public IFormatHeader FormatHeader { get; set; }

        public List<GameSoundPermutation> Permutations { get; set; }

        public GameSound()
        {
            Permutations = new List<GameSoundPermutation>();
        }
    }

    public class GameSoundPermutation
    {
        public string Name { get; set; }

        public byte[] SoundData { get; set; }
    }
}
