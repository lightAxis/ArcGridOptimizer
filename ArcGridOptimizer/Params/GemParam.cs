using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcGridOptimizer.Params
{
    static public class GemParam
    {
        static public int MaxRequiredWill = 10;
        static public int MaxRewardPoint = 5;
        static public int MaxPropertyValue = 5;

        static public Dictionary<int, double> AtkValues = new Dictionary<int, double>()
        {
            { 1, 0.029 },
            { 2, 0.067 },
            { 3, 0.105 },
            { 4, 0.134 },
            { 5, 0.172 },
        };

        static public Dictionary<int, double> ExtraDmgValues = new Dictionary<int, double>()
        {
            { 1, 0.060 },
            { 2, 0.119 },
            { 3, 0.187 },
            { 4, 0.239 },
            { 5, 0.299 },
        };

        static public Dictionary<int, double> BossDmgValues = new Dictionary<int, double>()
        {
            { 1, 0.078 },
            { 2, 0.156},
            { 3, 0.244 },
            { 4, 0.313 },
            { 5, 0.391 },
        };
    }

    static public class GemParamDefault
    {
        static public int MaxRequiredWill = 10;
        static public int MaxRewardPoint = 5;
        static public int MaxPropertyValue = 5;

        // 포셔 유튜브 참고 : https://www.youtube.com/watch?v=1EHrPm50_Ig&t=418s
        static public Dictionary<int, double> AtkValues = new Dictionary<int, double>()
        {
            { 1, 0.029 },
            { 2, 0.067 },
            { 3, 0.105 },
            { 4, 0.134 },
            { 5, 0.172 },
        };

        static public Dictionary<int, double> ExtraDmgValues = new Dictionary<int, double>()
        {
            { 1, 0.060 },
            { 2, 0.119 },
            { 3, 0.187 },
            { 4, 0.239 },
            { 5, 0.299 },
        };

        static public Dictionary<int, double> BossDmgValues = new Dictionary<int, double>()
        {
            { 1, 0.078 },
            { 2, 0.156},
            { 3, 0.244 },
            { 4, 0.313 },
            { 5, 0.391 },
        };
    }
}
