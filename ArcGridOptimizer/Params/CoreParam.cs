using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ArcGridOptimizer.Enums;

namespace ArcGridOptimizer.Params
{
    static public class CoreParam
    {
        public static Dictionary<eCoreGrade, int> MaxSupplyWill = new Dictionary<eCoreGrade, int>()
        {
            { eCoreGrade.Hero, 9 },
            { eCoreGrade.Legend, 12 },
            { eCoreGrade.Relic, 15 },
            { eCoreGrade.Ancient, 17 },
        };

        public static Dictionary<int, int> CoreEffectLevels = new Dictionary<int, int>()
        {
            { 1, 10 },
            { 2, 14 },
            { 3, 17 },
            { 4, 18 },
            { 5, 19 },
            { 6, 20 },
        };

        public static Dictionary<eCoreGrade, int> CoreGradeMaxEffectLevel = new Dictionary<eCoreGrade, int>()
        { 
            { eCoreGrade.Hero, 1 },
            { eCoreGrade.Legend, 2 },
            { eCoreGrade.Relic, 6 },
            { eCoreGrade.Ancient, 6 },
        };

        public static Dictionary<eCoreEffectLevelGainType, double> CoreEffLevelGain = new Dictionary<eCoreEffectLevelGainType, double>()
        {
            {eCoreEffectLevelGainType.Low, 100.0 },
            {eCoreEffectLevelGainType.Medium, 110.0 },
            {eCoreEffectLevelGainType.High, 120.0 },
        };
    }
}
