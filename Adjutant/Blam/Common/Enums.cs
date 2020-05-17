using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Common
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    internal sealed class BuildStringAttribute : Attribute
    {
        public string BuildString { get; }

        public BuildStringAttribute(string buildString)
        {
            BuildString = buildString;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    internal sealed class CacheGenerationAttribute : Attribute
    {
        public int Generation { get; }

        public CacheGenerationAttribute(int generation)
        {
            Generation = generation;
        }
    }

    public enum CacheType
    {
        Unknown = -1,

        [CacheGeneration(1)]
        [BuildString("01.01.14.2342")]
        Halo1Xbox,

        [CacheGeneration(1)]
        [BuildString("01.00.00.0564")]
        Halo1PC,

        [CacheGeneration(1)]
        [BuildString("01.00.00.0609")]
        Halo1CE,

        [CacheGeneration(1)]
        [BuildString("01.00.01.0563")]
        Halo1AE,

        [CacheGeneration(2)]
        [BuildString("02.09.27.09809")]
        Halo2Xbox,

        [CacheGeneration(2)]
        [BuildString("11081.07.04.30.0934.main")]
        Halo2Vista,
        
        [CacheGeneration(3)]
        [BuildString("09699.07.05.01.1534.delta")]
        Halo3Beta,

        [CacheGeneration(3)]
        [BuildString("11855.07.08.20.2317.halo3_ship")]
        [BuildString("12065.08.08.26.0819.halo3_ship")] //multiplayer map pack
        Halo3Retail,

        [CacheGeneration(3)]
        [BuildString("13895.09.04.27.2201.atlas_relea")]
        Halo3ODST,

        [CacheGeneration(3)]
        [BuildString("09730.10.04.09.1309.omaha_delta")]
        HaloReachBeta,

        [CacheGeneration(3)]
        [BuildString("11860.10.07.24.0147.omaha_relea")]
        HaloReachRetail,

        [CacheGeneration(3)]
        [BuildString("Jun 24 2019 00:36:03")] //flight 1
        [BuildString("Jul 30 2019 14:17:16")] //flight 2
        [BuildString("Oct 24 2019 15:56:32")] //release
        [BuildString("Jan 30 2020 16:55:25")] //update 1
        [BuildString("Mar 24 2020 12:10:36")] //update 2
        MccHaloReach,

        [CacheGeneration(4)]
        [BuildString("14064.12.05.05.1011.beta")]
        Halo4Beta,

        [CacheGeneration(4)]
        [BuildString("20810.12.09.22.1647.main")] //retail
        [BuildString("16531.12.07.05.0200.main")] //DLC1
        [BuildString("17539.12.07.24.0200.main")] //DLC3
        [BuildString("18223.12.08.06.0200.main")] //????
        [BuildString("18845.12.08.16.0200.main")] //DLC4
        [BuildString("20190.12.09.05.0200.main")] //DLC5
        [BuildString("20703.12.09.16.0400.main")] //DLC6
        [BuildString("20744.12.09.18.0100.main")] //DLC7
        [BuildString("20975.12.10.25.1337.main")] //????
        [BuildString("21122.12.11.21.0101.main")] //TU02
        [BuildString("21165.12.12.12.0112.main")] //TU03
        [BuildString("21339.13.02.05.0117.main")] //TU04
        [BuildString("21391.13.03.13.1711.main")] //TU05
        [BuildString("21401.13.04.23.1849.main")] //TU06
        [BuildString("21501.13.08.06.2311.main")] //TU07
        [BuildString("21522.13.10.17.1936.main")] //TU08
        Halo4Retail
    }

    public enum Language
    {
        English = 0,
        Japanese = 1,
        German = 2,
        French = 3,
        Spanish = 4,
        LatinAmericanSpanish = 5,
        Italian = 6,
        Korean = 7,
        Chinese = 8,
        Unknown0 = 9,
        Portuguese = 10,
        Unknown1 = 11
    }
}
