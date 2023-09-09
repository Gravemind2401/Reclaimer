namespace Reclaimer.Blam.Common
{
    using static CacheMetadataFlags;
    using static CachePlatform;
    using static CacheResourceCodec;

    public enum CacheGeneration
    {
        Gen1,
        Gen2,
        Gen3,
        Gen4
    }

    public enum HaloGame
    {
        Unknown = -1,
        Halo1,
        Halo2,
        Halo3,
        Halo3ODST,
        HaloReach,
        Halo4,
        Halo2X
    }

    public enum CachePlatform
    {
        Xbox,
        Xbox360,
        XboxOne,
        NintendoDS,
        PC
    }

    public enum PlatformArchitecture
    {
        x86,
        PowerPC
    }

    public enum CacheResourceCodec
    {
        Unknown = -1,
        Uncompressed, //default for Gen1/2
        Deflate, //default for Gen3+
        LZX,
        UnknownDeflate
    }

    public enum CacheMetadataFlags
    {
        None = 0,
        PreBeta = 1,
        Beta = 2,
        Flight = 4,
        Anniversary = 8,
        Mcc = 16,

        MccFlight = Mcc | Flight
    }

    public enum CacheType
    {
        Unknown = -1,

        [CacheMetadata(HaloGame.Halo1, Xbox)]
        [BuildString("01.01.14.2342")]
        Halo1Xbox,

        [CacheMetadata(HaloGame.Halo1, PC)]
        [BuildString("01.07.30.0452", Beta)]
        [BuildString("01.00.00.0564")] //release
        Halo1PC,

        [CacheMetadata(HaloGame.Halo1, PC)]
        [BuildString("01.00.00.0609")]
        Halo1CE,

        [CacheMetadata(HaloGame.Halo1, Xbox360, Anniversary)]
        [BuildString("01.00.01.0563")]
        Halo1AE,

        [CacheMetadata(HaloGame.Halo1, PC, Anniversary | Mcc)]
        //[BuildString("01.00.01.0563")]
        [BuildString("01.03.43.0000")] //update 1
        MccHalo1,

        [CacheMetadata(HaloGame.Halo2, Xbox, Beta)]
        [BuildString("02.06.28.07902")]
        Halo2Beta,

        [CacheMetadata(HaloGame.Halo2, Xbox)]
        [BuildString("02.09.27.09809")]
        Halo2Xbox,

        [CacheMetadata(HaloGame.Halo2, PC)]
        [BuildString("11081.07.04.30.0934.main")]
        Halo2Vista,

        [CacheMetadata(HaloGame.Halo2, PC, Anniversary | Mcc)]
        MccHalo2,

        [CacheMetadata(HaloGame.Halo3, Xbox360, Uncompressed, PreBeta)]
        [BuildString("06481.06.11.17.1330.alpha_relea", "alpha_1117")]
        Halo3Alpha,

        [CacheMetadata(HaloGame.Halo3, Xbox360, Uncompressed, PreBeta)]
        [BuildString("08117.07.03.07.1702.delta", "alpha_0307")]
        [BuildString("08172.07.03.08.2240.delta", "alpha_0308")]
        Halo3Delta,

        [CacheMetadata(HaloGame.Halo3, Xbox360, Uncompressed, Beta)]
        [BuildString("Mar  9 2007 22:22:32", "alpha_0308", PreBeta)] //not really beta but matches for the tags we need
        //[BuildString("Mar 10 2007 16:16:44", "alpha_0308", PreBeta)] //no tag names
        [BuildString("09699.07.05.01.1534.delta", "beta")]
        Halo3Beta,

        [CacheMetadata(HaloGame.Halo3, Xbox360)]
        [BuildString("11855.07.08.20.2317.halo3_ship", "retail")]
        [BuildString("11856.07.08.20.2332.release", "retail")] //espilon (unknown support)
        [BuildString("11729.07.08.10.0021.main", "retail")] //expo (unknown support)
        [BuildString("12065.08.08.26.0819.halo3_ship", "retail")] //mythic map pack
        Halo3Retail,

        [CacheMetadata(HaloGame.Halo3, PC, Mcc)]
        [BuildString("Jun  4 2020 20:29:31", "U0", MccFlight)] //flight 1
        [BuildString("Jun 21 2020 16:34:20", "U0", MccFlight)] //flight 1 update
        [BuildString("Jun 25 2020 15:02:49", "U0")] //release
        [BuildString("Aug 11 2020 23:34:41", "U0", MccFlight)] //flight 2 (odst test)
        [BuildString("Aug 26 2020 21:24:11", "U1")] //update 1
        [BuildString("Oct 21 2020 09:24:30", "U1")] //update 2
        [BuildString("Nov 24 2020 15:47:48", "U1")] //update 3
        MccHalo3,

        [CacheMetadata(HaloGame.Halo3, PC, Mcc)]
        [BuildString("Oct  7 2020 03:55:07", "U1", MccFlight)] //flight 3 (mainmenu)
        [BuildString("Feb 19 2021 11:19:43", "U1", MccFlight)] //flight 3
        [BuildString("Mar  5 2021 08:45:13", "U1", MccFlight)] //flight 4
        [BuildString("Mar 14 2021 03:19:55", "U1")] //update 4
        [BuildString("May 19 2021 16:19:54", "U1", MccFlight)] //flight 5
        [BuildString("Jun  9 2021 09:25:41", "U1")] //update 5
        MccHalo3U4,

        [CacheMetadata(HaloGame.Halo3, PC, Mcc | MccFlight)]
        [BuildString("Aug 25 2021 03:43:53", "U1")] //flight 6
        MccHalo3F6,

        [CacheMetadata(HaloGame.Halo3, PC, Mcc)]
        [BuildString("Sep 29 2021 09:17:56", "U6")] //update 6
        [BuildString("Oct 11 2021 04:58:42", "U6")] //U7 (U6 custom maps)
        [BuildString("Nov  1 2021 06:16:14", "U6")] //U8
        MccHalo3U6,

        [CacheMetadata(HaloGame.Halo3, PC, Mcc)]
        [BuildString("Feb 15 2022 01:35:34", "U6")] //U9
        [BuildString("Apr  6 2022 10:48:27", "U6")] //U9 custom maps 1
        [BuildString("Apr  6 2022 10:48:46", "U6")] //U9 custom maps 2
        [BuildString("Apr 20 2022 11:55:21", "U6")] //U9 custom maps 3
        [BuildString("Apr 20 2022 11:56:23", "U6")] //U9 custom maps 4
        [BuildString("May  3 2022 12:38:41", "U6")] //U10
        [BuildString("Jun  8 2022 16:05:04", "U6")] //U10 custom maps 1
        [BuildString("Jun  8 2022 16:06:30", "U6")] //U10 custom maps 2
        [BuildString("Jul 21 2022 21:15:27", "U6")] //U11
        [BuildString("Aug 21 2022 17:23:29", "U6")] //U11 custom maps 1
        [BuildString("Aug 21 2022 17:24:39", "U6")] //U11 custom maps 2
        MccHalo3U9,

        [CacheMetadata(HaloGame.Halo3, PC, Mcc)]
        [BuildString("Oct 11 2022 04:34:53", "U6")] //U12
        [BuildString("Nov 16 2022 21:16:57", "U6")] //U12 custom maps 1
        [BuildString("Nov 16 2022 21:18:10", "U6")] //U12 custom maps 2
        MccHalo3U12,

        [CacheMetadata(HaloGame.Halo3, PC, Mcc)]
        [BuildString("May 22 2023 20:10:02", "U13")] //U13
        [BuildString("Jun 27 2023 08:39:36", "U13")] //U13 custom maps
        MccHalo3U13,

        [CacheMetadata(HaloGame.Halo3ODST, Xbox360)]
        [BuildString("13895.09.04.27.2201.atlas_relea", "odst")]
        Halo3ODST,

        [CacheMetadata(HaloGame.Halo3ODST, PC, Mcc)]
        [BuildString("Aug 11 2020 06:58:27", "ODST U0", MccFlight)] //flight 1
        [BuildString("Aug 17 2020 01:12:27", "ODST U0", MccFlight)] //flight 1 update 1
        [BuildString("Aug 24 2020 08:37:26", "ODST U0", MccFlight)] //flight 1 update 2
        [BuildString("Aug 28 2020 08:43:36", "ODST U0")] //release
        [BuildString("Sep 29 2020 10:59:04", "ODST U0")] //update 1
        [BuildString("Dec  4 2020 18:24:06", "ODST U0")] //update 2
        MccHalo3ODST,

        [CacheMetadata(HaloGame.Halo3ODST, PC, MccFlight)]
        [BuildString("Aug 25 2021 03:35:05", "ODST U0")] //flight 3
        MccHalo3ODSTF3,

        [CacheMetadata(HaloGame.Halo3ODST, PC, Mcc)]
        [BuildString("Sep 18 2021 21:15:55", "ODST U3")] //update 3
        MccHalo3ODSTU3,

        [CacheMetadata(HaloGame.Halo3ODST, PC, Mcc)]
        [BuildString("Feb 15 2022 01:59:47", "ODST U3")] //update 4
        [BuildString("Apr  5 2022 14:16:52", "ODST U3")] //update 4 custom maps 1
        [BuildString("Apr  5 2022 14:16:35", "ODST U3")] //update 4 custom maps 2
        [BuildString("Apr 20 2022 11:53:15", "ODST U3")] //update 5
        [BuildString("May  3 2022 12:27:08", "ODST U3")] //update 5 custom maps 1
        [BuildString("May  3 2022 12:27:56", "ODST U3")] //update 5 custom maps 2
        [BuildString("Jul 22 2022 05:55:30", "ODST U3")] //update 6
        [BuildString("Aug 21 2022 17:29:59", "ODST U3")] //update 6 custom maps 1
        [BuildString("Aug 21 2022 17:30:28", "ODST U3")] //update 6 custom maps 2
        MccHalo3ODSTU4,

        [CacheMetadata(HaloGame.Halo3ODST, PC, Mcc)]
        [BuildString("Oct 24 2022 03:07:19", "ODST U3")] //update 7
        [BuildString("Nov 16 2022 20:57:46", "ODST U3")] //update 7 custom maps 1
        [BuildString("Nov 16 2022 20:59:16", "ODST U3")] //update 7 custom maps 2
        MccHalo3ODSTU7,

        [CacheMetadata(HaloGame.Halo3ODST, PC, Mcc)]
        [BuildString("May 16 2023 11:44:41", "ODST U8")] //update 8
        [BuildString("Jun 27 2023 08:39:51", "ODST U8")] //update 8 custom maps 1
        [BuildString("Jun 27 2023 08:39:50", "ODST U8")] //update 8 custom maps 2
        MccHalo3ODSTU8,

        [CacheMetadata(HaloGame.HaloReach, Xbox360, Beta)]
        [BuildString("09449.10.03.25.1545.omaha_beta", "beta", PreBeta)]
        [BuildString("09730.10.04.09.1309.omaha_delta", "beta")]
        HaloReachBeta,

        [CacheMetadata(HaloGame.HaloReach, Xbox360)]
        [BuildString("11860.10.07.24.0147.omaha_relea", "retail")]
        HaloReachRetail,

        [CacheMetadata(HaloGame.HaloReach, PC, Mcc)]
        [BuildString("Jun 24 2019 00:36:03", "U0", MccFlight)] //flight 1
        [BuildString("Jul 30 2019 14:17:16", "U0", MccFlight)] //flight 2
        [BuildString("Oct 24 2019 15:56:32", "U0")] //release
        [BuildString("Jan 30 2020 16:55:25", "U2")] //update 1
        [BuildString("Mar 24 2020 12:10:36", "U2")] //update 2
        MccHaloReach,

        [CacheMetadata(HaloGame.HaloReach, PC, Mcc)]
        [BuildString("Jun  5 2020 10:40:14", "U2")] //update 3
        [BuildString("Oct 15 2020 18:23:50", "U2")] //update 4
        [BuildString("Nov 24 2020 18:32:37", "U2")] //update 5
        [BuildString("Mar  4 2021 13:14:28", "U6")] //update 6
        [BuildString("May 26 2021 10:02:45", "U6")] //update 7
        MccHaloReachU3,

        [CacheMetadata(HaloGame.HaloReach, PC, Mcc)]
        [BuildString("Aug 11 2021 15:50:30", "U8", MccFlight)] //flight 4
        [BuildString("Sep 13 2021 09:49:52", "U8")] //update 8
        [BuildString("Sep 17 2021 13:25:40", "U8")] //update 9
        MccHaloReachU8,

        [CacheMetadata(HaloGame.HaloReach, PC, Mcc)]
        [BuildString("Jan 13 2022 00:54:50", "U8")] //update 10
        [BuildString("Aug  5 2022 20:35:02", "U8")] //update 11
        [BuildString("Aug 31 2022 11:53:49", "U8")] //update 11 custom maps 1
        [BuildString("Aug 31 2022 11:53:19", "U8")] //update 11 custom maps 2
        [BuildString("Oct 12 2022 01:55:01", "U8")] //update 12
        [BuildString("Nov 16 2022 21:11:25", "U8")] //update 12 custom maps 1
        [BuildString("Nov 16 2022 21:13:04", "U8")] //update 12 custom maps 2
        MccHaloReachU10,

        [CacheMetadata(HaloGame.HaloReach, PC, Mcc)]
        [BuildString("Jun 21 2023 15:35:31", "U13")] //update 13
        [BuildString("Jun 27 2023 08:55:51", "U13")] //update 13 custom maps 1
        [BuildString("Jun 27 2023 08:55:19", "U13")] //update 13 custom maps 2
        [BuildString("Jul 16 2023 16:12:13", "U13")] //update 13 custom maps 3
        [BuildString("Jul 16 2023 16:08:14", "U13")] //update 13 custom maps 4
        MccHaloReachU13,

        [CacheMetadata(HaloGame.Halo4, Xbox360, Beta)]
        [BuildString("14064.12.05.05.1011.beta", "beta")]
        Halo4Beta,

        [CacheMetadata(HaloGame.Halo4, Xbox360, LZX)]
        [BuildString("20810.12.09.22.1647.main", "retail")] //retail
        [BuildString("16531.12.07.05.0200.main", "retail")] //DLC1
        [BuildString("17539.12.07.24.0200.main", "retail")] //DLC3
        [BuildString("18223.12.08.06.0200.main", "retail")] //????
        [BuildString("18845.12.08.16.0200.main", "retail")] //DLC4
        [BuildString("20190.12.09.05.0200.main", "retail")] //DLC5
        [BuildString("20703.12.09.16.0400.main", "retail")] //DLC6
        [BuildString("20744.12.09.18.0100.main", "retail")] //DLC7
        [BuildString("20975.12.10.25.1337.main", "retail")] //????
        [BuildString("21122.12.11.21.0101.main", "retail")] //TU02
        [BuildString("21165.12.12.12.0112.main", "retail")] //TU03
        [BuildString("21339.13.02.05.0117.main", "retail")] //TU04
        [BuildString("21391.13.03.13.1711.main", "retail")] //TU05
        [BuildString("21401.13.04.23.1849.main", "retail")] //TU06
        [BuildString("21501.13.08.06.2311.main", "retail")] //TU07
        [BuildString("21522.13.10.17.1936.main", "retail")] //TU08
        Halo4Retail,

        [CacheMetadata(HaloGame.Halo4, PC, UnknownDeflate, Mcc)]
        [BuildString("Oct 12 2020 08:13:40", "U0", MccFlight)] //flight 1
        [BuildString("Oct 26 2020 11:43:08", "U0")] //release
        [BuildString("Mar 20 2021 04:23:02", "U1", Deflate)] //update 1
        [BuildString("May 16 2021 10:41:44", "U1", Deflate, MccFlight)] //flight 2
        [BuildString("May 27 2021 15:23:34", "U1", Deflate)] //update 2
        [BuildString("Sep 10 2021 06:54:23", "U3", Deflate)] //update 3
        MccHalo4,

        [CacheMetadata(HaloGame.Halo4, PC, Deflate, Mcc)]
        [BuildString("Feb  3 2022 19:02:31", "U3")] //update 4
        [BuildString("Aug  8 2022 17:28:57", "U3")] //update 5
        MccHalo4U4,

        [CacheMetadata(HaloGame.Halo4, PC, Deflate, Mcc)]
        [BuildString("Oct 17 2022 21:26:14", "U3")] //update 6
        [BuildString("Dec  6 2022 11:23:50", "U3")] //update 6 custom maps 1
        [BuildString("Dec  6 2022 11:26:37", "U3")] //update 6 custom maps 2
        [BuildString("Apr  1 2023 17:35:22", "U3")] //update 7
        [BuildString("Jun 27 2023 08:46:47", "U3")] //update 7 custom maps 1
        [BuildString("Jun 27 2023 08:47:24", "U3")] //update 7 custom maps 2
        MccHalo4U6,

        [CacheMetadata(HaloGame.Halo2X, PC, UnknownDeflate, Mcc)]
        [BuildString("Apr  9 2020 01:36:04", "U0", MccFlight)] //flight 1
        [BuildString("Apr 13 2020 02:24:30", "U0")] //release
        [BuildString("May 10 2020 21:14:00", "U1")] //update 1
        [BuildString("May 12 2020 12:18:21", "U2")] //update 2
        [BuildString("Jul 25 2020 22:24:58", "U2")] //update 3
        [BuildString("Sep 30 2020 20:30:41", "U2")] //update 4
        [BuildString("Dec 25 2020 16:05:40", "U2", Deflate)] //update 5
        [BuildString("Sep 23 2021 02:46:19", "U6", Deflate)] //update 6
        [BuildString("Sep 17 2021 14:26:28", "U6", Deflate)] //update 7 (time travel?)
        MccHalo2X,

        [CacheMetadata(HaloGame.Halo2X, PC, Deflate, Mcc)]
        [BuildString("Feb 14 2022 19:18:28", "U6")] //update 8
        [BuildString("Jun 13 2022 13:44:24", "U6")] //update 9
        MccHalo2XU8,

        [CacheMetadata(HaloGame.Halo2X, PC, Deflate, Mcc)]
        [BuildString("Oct 14 2022 21:11:35", "U6")] //update 10
        [BuildString("Dec  6 2022 11:13:38", "U6")] //update 10 custom maps 1
        [BuildString("Dec  6 2022 11:13:46", "U6")] //update 10 custom maps 2
        [BuildString("Jun 13 2023 20:21:18", "U6")] //update 11
        [BuildString("Jun 27 2023 08:47:43", "U6")] //update 11 custom maps 1
        [BuildString("Jun 27 2023 08:47:52", "U6")] //update 11 custom maps 2
        MccHalo2XU10,
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
        ChineseTrad = 8,
        ChineseSimp = 9,
        Portuguese = 10,
        Polish = 11,

        // Halo 4
        Russian = 12,
        Danish = 13,
        Finnish = 14,
        Dutch = 15,
        Norwegian = 16
    }

    public enum MipmapLayout
    {
        None,
        Fragmented,
        Contiguous
    }
}
