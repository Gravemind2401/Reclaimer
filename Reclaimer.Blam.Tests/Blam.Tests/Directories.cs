using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Blam.Tests
{
    internal static class Directories
    {
        public const string PCHalo1 = ConsoleRoot + @"Y:\Halo\Halo1PC\MAPS";

        #region Console
        private const string ConsoleRoot = @"Y:\Halo";

        public const string ConsoleHalo2 = ConsoleRoot + @"\Halo2Xbox\maps";

        public const string ConsoleHalo3Beta = ConsoleRoot + @"\Halo3Beta\maps";
        public const string ConsoleHalo3 = ConsoleRoot + @"\Halo3Retail\Campaign\maps";
        public const string ConsoleHalo3Multiplayer = ConsoleRoot + @"\Halo3Retail\Multiplayer\maps";
        public const string ConsoleHalo3ODST = ConsoleRoot + @"\Halo3ODST\maps";

        public const string ConsoleHaloReachBeta = ConsoleRoot + @"\HaloReachBeta\maps";
        public const string ConsoleHaloReach = ConsoleRoot + @"\HaloReachRetail\maps";
        public const string ConsoleHaloCEX = ConsoleRoot + @"\HaloCEX\maps";

        public const string ConsoleHalo4Beta = ConsoleRoot + @"\Halo4Beta\maps";
        public const string ConsoleHalo4 = ConsoleRoot + @"\Halo4Retail\Campaign\maps";
        public const string ConsoleHalo4Multiplayer = ConsoleRoot + @"\Halo4Retail\Multiplayer\maps";
        public const string ConsoleHalo4DLC = ConsoleRoot + @"\Halo4Retail\DLC";
        #endregion

        #region MCC
        private const string MccRoot = @"D:\Program Files (x86)\Steam\steamapps\common\Halo The Master Chief Collection";

        public const string MccHalo3 = MccRoot + @"\halo3\maps";
        public const string MccHaloReach = MccRoot + @"\haloreach\maps";
        public const string MccHalo2X = MccRoot + @"\groundhog\maps";
        #endregion

        public const string Halo5Server = @"Y:\Halo\Halo5Server\deploy\any\levels";
        public const string Halo5Forge = @"Y:\Halo\Halo5Forge\deploy\any\levels";
    }
}
