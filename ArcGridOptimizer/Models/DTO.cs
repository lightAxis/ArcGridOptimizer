using ArcGridOptimizer.Enums;
using ArcGridOptimizer.ViewModels.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcGridOptimizer.Models
{
    public class CoreLevelValueDTO
    {
        public int Level { get; set; }
        public double Value { get; set; }
        public bool SpecialLevel { get; set; }
        public eCoreEffectLevelGainType EffectLevelGainType { get; set; }
        public bool Enabled { get; set; }
    }

    public class CoreDTO
    {
        public bool IsInitialized { get; set; }
        public string Name { get; set; } = "";
        public eCoreGrade Grade { get; set; }
        //public int SupplyWill { get; set; }
        public List<CoreLevelValueDTO> CoreValues { get; set; } = new List<CoreLevelValueDTO>();
    }

    public class CoreListDTO
    {
        public List<CoreDTO> Cores { get; set; } = new List<CoreDTO>();
    }

    public class GemPropertyDTO
    {
        public eGemProperty Type { get; set; } = eGemProperty.None;
        public int Value { get; set; } = 0;
    }

    public class GemDTO
    {
        public string Name { get; set; } = "";
        public int RequiredWill { get; set; }
        public int RewardPoint { get; set; }
        public GemPropertyDTO Property1 { get; set; } = new GemPropertyDTO();
        public GemPropertyDTO Property2 { get; set; } = new GemPropertyDTO();
    }

    public class GemListDTO
    {
        public List<GemDTO> Gems { get; set; } = new List<GemDTO>();
    }

    public class GemOptResultDTO : GemDTO
    {
        public double Score { get; set; }
    }

    public class CoreOptResultDTO
    {
        public CoreDTO Core { get; set; } = new CoreDTO();
        public List<GemOptResultDTO> GemOptResults { get; set; } = new List<GemOptResultDTO>();
        public int TotalWill { get; set; }
        public int TotalPoint { get; set; }
        public int TotalAtkSum { get; set; }
        public int TotalExtraSum { get; set; }
        public int TotalBossSum { get; set; }
        public double TotalScore { get; set; }
    }

    public class OptResultDTO
    {
        public List<CoreOptResultDTO> CoreResults { get; set; } = new List<CoreOptResultDTO>();
        public int TotalAtkSum { get; set; }
        public int TotalExtraSum { get; set; }
        public int TotalBossSum { get; set; }
        public double TotalScore { get; set; }
    }
}
