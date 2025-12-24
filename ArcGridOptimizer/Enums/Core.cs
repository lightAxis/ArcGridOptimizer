using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcGridOptimizer.Enums
{
    public enum eCoreGrade
    {
        [Description("영웅")]
        Hero = 1,
        [Description("전설")]
        Legend = 2,
        [Description("유물")]
        Relic = 3,
        [Description("고대")]
        Ancient = 4,
    }

    public enum eCoreEffectLevelGainType
    {
        [Description("없음")]
        None = 0,
        [Description("낮음")]
        Low = 1,
        [Description("중간")]
        Medium = 2,
        [Description("높음")]
        High = 3,
    }
}
