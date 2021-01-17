using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Common
{
    public static class ContentFactory
    {
        private const string bitmap = "bitm";
        private const string gbxmodel = "mod2";
        private const string render_model = "mode";
        private const string scenario_structure_bsp = "sbsp";
        private const string sound = "snd!";

        #region Standard Halo Maps

        public static bool TryGetPrimaryContent(IIndexItem item, out object content)
        {
            switch (item.ClassCode)
            {
                case bitmap:
                    IBitmap bitmapContent;
                    if (TryGetBitmapContent(item, out bitmapContent))
                    {
                        content = bitmapContent;
                        return true;
                    }
                    break;
                case gbxmodel:
                case render_model:
                case scenario_structure_bsp:
                    IRenderGeometry geometryContent;
                    if (TryGetGeometryContent(item, out geometryContent))
                    {
                        content = geometryContent;
                        return true;
                    }
                    break;
                case sound:
                    ISoundContainer soundContent;
                    if (TryGetSoundContent(item, out soundContent))
                    {
                        content = soundContent;
                        return true;
                    }
                    break;
            }

            content = null;
            return false;
        }

        public static bool TryGetBitmapContent(IIndexItem item, out IBitmap content)
        {
            content = null;

            if (item == null)
                return false;

            if (item.ClassCode != bitmap)
                return false;

            switch (item.CacheFile.CacheType)
            {
                case CacheType.Halo1Xbox:
                case CacheType.Halo1CE:
                case CacheType.Halo1PC:
                    content = item.ReadMetadata<Halo1.bitmap>();
                    break;
                case CacheType.Halo2Beta:
                    content = item.ReadMetadata<Halo2Beta.bitmap>();
                    break;
                case CacheType.Halo2Xbox:
                    content = item.ReadMetadata<Halo2.bitmap>();
                    break;
                case CacheType.Halo3Alpha:
                case CacheType.Halo3Beta:
                case CacheType.Halo3Retail:
                case CacheType.MccHalo3:
                case CacheType.Halo3ODST:
                case CacheType.MccHalo3ODST:
                    content = item.ReadMetadata<Halo3.bitmap>();
                    break;
                case CacheType.HaloReachBeta:
                case CacheType.HaloReachRetail:
                case CacheType.MccHaloReach:
                case CacheType.MccHaloReachU3:
                    content = item.ReadMetadata<HaloReach.bitmap>();
                    break;
                case CacheType.Halo4Beta:
                case CacheType.Halo4Retail:
#if DEBUG
                case CacheType.MccHalo4:
                case CacheType.MccHalo2X:
#endif
                    content = item.ReadMetadata<Halo4.bitmap>();
                    break;
            }

            return content != null;
        }

        public static bool TryGetGeometryContent(IIndexItem item, out IRenderGeometry content)
        {
            content = null;

            if (item == null)
                return false;

            if (item.ClassCode == gbxmodel || item.ClassCode == render_model)
            {
                switch (item.CacheFile.CacheType)
                {
                    case CacheType.Halo1Xbox:
                    case CacheType.Halo1CE:
                    case CacheType.Halo1PC:
                        content = item.ReadMetadata<Halo1.gbxmodel>();
                        break;
                    case CacheType.Halo2Xbox:
                        content = item.ReadMetadata<Halo2.render_model>();
                        break;
                    case CacheType.Halo3Alpha:
                    case CacheType.Halo3Beta:
                    case CacheType.Halo3Retail:
                    case CacheType.MccHalo3:
                    case CacheType.Halo3ODST:
                    case CacheType.MccHalo3ODST:
                        content = item.ReadMetadata<Halo3.render_model>();
                        break;
                    case CacheType.HaloReachBeta:
                    case CacheType.HaloReachRetail:
                    case CacheType.MccHaloReach:
                    case CacheType.MccHaloReachU3:
                        content = item.ReadMetadata<HaloReach.render_model>();
                        break;
                    case CacheType.Halo4Beta:
                    case CacheType.Halo4Retail:
#if DEBUG
                    case CacheType.MccHalo4:
                    case CacheType.MccHalo2X:
#endif
                        content = item.ReadMetadata<Halo4.render_model>();
                        break;
                }
            }
            else if (item.ClassCode == scenario_structure_bsp)
            {
                switch (item.CacheFile.CacheType)
                {
                    case CacheType.Halo1CE:
                    case CacheType.Halo1PC:
                        content = item.ReadMetadata<Halo1.scenario_structure_bsp>();
                        break;
                    case CacheType.Halo2Xbox:
                        content = item.ReadMetadata<Halo2.scenario_structure_bsp>();
                        break;
                    case CacheType.Halo3Alpha:
                    case CacheType.Halo3Beta:
                    case CacheType.Halo3Retail:
                    case CacheType.MccHalo3:
                    case CacheType.Halo3ODST:
                    case CacheType.MccHalo3ODST:
                        content = item.ReadMetadata<Halo3.scenario_structure_bsp>();
                        break;
                    case CacheType.HaloReachBeta:
                    case CacheType.HaloReachRetail:
                    case CacheType.MccHaloReach:
                    case CacheType.MccHaloReachU3:
                        content = item.ReadMetadata<HaloReach.scenario_structure_bsp>();
                        break;
                    case CacheType.Halo4Beta:
                    case CacheType.Halo4Retail:
#if DEBUG
                    case CacheType.MccHalo4:
                    case CacheType.MccHalo2X:
#endif
                        content = item.ReadMetadata<Halo4.scenario_structure_bsp>();
                        break;
                }
            }

            return content != null;
        }

        public static bool TryGetSoundContent(IIndexItem item, out ISoundContainer content)
        {
            content = null;

            if (item == null)
                return false;

            if (item.ClassCode != sound)
                return false;

            switch (item.CacheFile.CacheType)
            {
                case CacheType.Halo2Xbox:
                    content = item.ReadMetadata<Halo2.sound>();
                    break;
                //case CacheType.Halo3Beta:
                case CacheType.Halo3Retail:
                case CacheType.Halo3ODST:
                    content = item.ReadMetadata<Halo3.sound>();
                    break;
                case CacheType.HaloReachBeta:
                case CacheType.HaloReachRetail:
                    content = item.ReadMetadata<HaloReach.sound>();
                    break;
            }

            return content != null;
        }

        #endregion

        #region Halo 5

        public static bool TryGetPrimaryContent(Halo5.ModuleItem item, out object content)
        {
            switch (item.ClassCode)
            {
                case bitmap:
                    IBitmap bitmapContent;
                    if (TryGetBitmapContent(item, out bitmapContent))
                    {
                        content = bitmapContent;
                        return true;
                    }
                    break;
                case render_model:
                    IRenderGeometry geometryContent;
                    if (TryGetGeometryContent(item, out geometryContent))
                    {
                        content = geometryContent;
                        return true;
                    }
                    break;
            }

            content = null;
            return false;
        }

        public static bool TryGetBitmapContent(Halo5.ModuleItem item, out IBitmap content)
        {
            content = null;

            if (item.ClassCode != bitmap)
                return false;

            content = item.ReadMetadata<Halo5.bitmap>();

            return content != null;
        }

        public static bool TryGetGeometryContent(Halo5.ModuleItem item, out IRenderGeometry content)
        {
            content = null;

            if (item.ClassCode != render_model)
                return false;

            content = item.ReadMetadata<Halo5.render_model>();

            return content != null;
        }

        #endregion
    }
}
