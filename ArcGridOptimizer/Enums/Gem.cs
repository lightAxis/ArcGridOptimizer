using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;

namespace ArcGridOptimizer.Enums
{ 
    public enum eGemProperty
    {
        [Description("없음")]
        None = 0,
        [Description("공격력")]
        Attack = 1,
        [Description("추가 피해")]
        ExtraDamage = 2,
        [Description("보스 피해")]
        BossDamage = 3,
    }
}
