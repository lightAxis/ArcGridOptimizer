using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGridOptimizer.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ArcGridOptimizer.ViewModels.Items
{
    public partial class GemOptResult : Gem
    {
        public GemOptResult() { }

        [ObservableProperty]
        private double _score = 0;

        public void FromDTO(GemOptResultDTO dto)
        {
            base.FromDTO(dto);
            Score = dto.Score;
        }

        public new GemOptResultDTO ToDTO()
        {
            var dto = base.ToDTO();
            return new GemOptResultDTO()
            {
                Name = dto.Name,
                RequiredWill = dto.RequiredWill,
                RewardPoint = dto.RewardPoint,
                Property1 = dto.Property1,
                Property2 = dto.Property2,
                Score = this.Score,
            };
        }
    }

    public partial class CoreOptResult : ObservableObject
    {
        public CoreOptResult() { }

        public CoreOptResult(Core core, List<GemOptResult> gemOptResults)
        {
            Core = core;
            GemOptResults = new ObservableCollection<GemOptResult>(gemOptResults);
            updateProps();
        }

        private void updateProps()
        {
            TotalWill = GemOptResults.Sum(g => g.RequiredWill);
            TotalPoint = GemOptResults.Sum(g => g.RewardPoint);
            TotalAtkSum = GemOptResults.Sum(g => g.Property1.Type == Enums.eGemProperty.Attack ? g.Property1.Value : 0) +
                GemOptResults.Sum(g => g.Property2.Type == Enums.eGemProperty.Attack ? g.Property2.Value : 0);
            TotalExtraSum = GemOptResults.Sum(g => g.Property1.Type == Enums.eGemProperty.ExtraDamage ? g.Property1.Value : 0) +
                GemOptResults.Sum(g => g.Property2.Type == Enums.eGemProperty.ExtraDamage ? g.Property2.Value : 0);
            TotalBossSum = GemOptResults.Sum(g => g.Property1.Type == Enums.eGemProperty.BossDamage ? g.Property1.Value : 0) +
                GemOptResults.Sum(g => g.Property2.Type == Enums.eGemProperty.BossDamage ? g.Property2.Value : 0);

            TotalScore = GemOptResults.Sum(g => g.Score);
        }

        public CoreOptResultDTO ToDTO()
        {
            return new CoreOptResultDTO()
            {
                Core = this.Core.ToDTO(),
                GemOptResults = this.GemOptResults.Select(g => g.ToDTO()).ToList(),
                TotalWill = this.TotalWill,
                TotalPoint = this.TotalPoint,
                TotalAtkSum = this.TotalAtkSum,
                TotalExtraSum = this.TotalExtraSum,
                TotalBossSum = this.TotalBossSum,
                TotalScore = this.TotalScore,
            };
        }

        public void FromDTO(CoreOptResultDTO dto)
        {
            Core.FromDTO(dto.Core);
            GemOptResults = new ObservableCollection<GemOptResult>(dto.GemOptResults.Select(g =>
            {
                var gem = new GemOptResult();
                gem.FromDTO(g);
                return gem;
            }));
            updateProps();
            // These properties are read-only and set in the constructor, so we can't set them here.
            // TotalWill = dto.TotalWill;
            // TotalPoint = dto.TotalPoint;
            // TotalAtkSum = dto.TotalAtkSum;
            // TotalExtraSum = dto.TotalExtraSum;
            // TotalBossSum = dto.TotalBossSum;
            // TotalScore = dto.TotalScore;
        }

        [ObservableProperty]
        private Core _core = new Core();

        [ObservableProperty]
        private ObservableCollection<GemOptResult> _gemOptResults = new ObservableCollection<GemOptResult>();


        public int TotalWill { get; private set; }
        public int TotalPoint { get; private set; }
        public int TotalAtkSum { get; private set; }
        public int TotalExtraSum { get; private set; }
        public int TotalBossSum { get; private set; }
        public double TotalScore { get; private set; }
    }

    public partial class OptResult : ObservableObject
    {
        public OptResult() { }

        public OptResult(List<CoreOptResult> coreOptResults)
        {
            CoreResults = new ObservableCollection<CoreOptResult>(coreOptResults);
            updateProps();
        }

        private void updateProps()
        {
            TotalAtkSum = CoreResults.Sum(c => c.TotalAtkSum);
            TotalExtraSum = CoreResults.Sum(c => c.TotalExtraSum);
            TotalBossSum = CoreResults.Sum(c => c.TotalBossSum);
            TotalScore = CoreResults.Sum(c => c.TotalScore);
        }

        public OptResultDTO ToDTO()
        {
            return new OptResultDTO()
            {
                CoreResults = this.CoreResults.Select(c => c.ToDTO()).ToList(),
            };
        }

        public void FromDTO(OptResultDTO dto)
        {
            CoreResults = new ObservableCollection<CoreOptResult>(dto.CoreResults.Select(c =>
            {
                var coreOpt = new CoreOptResult();
                coreOpt.FromDTO(c);
                return coreOpt;
            }));
            updateProps();
        }

        [ObservableProperty]
        ObservableCollection<CoreOptResult> _coreResults = new ObservableCollection<CoreOptResult>();

        public int TotalAtkSum { get; private set; }
        public int TotalExtraSum { get; private set; }
        public int TotalBossSum { get; private set; }
        public double TotalScore { get; private set; }
    }
}
