using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGridOptimizer.Enums;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.Windows.Navigation;
using System.Windows;

using ArcGridOptimizer.Models;

namespace ArcGridOptimizer.ViewModels.Items
{
    public partial class CoreLevelValue : ObservableObject
    {
        public CoreLevelValue(int level = 10, double value = 0.0,
            bool special_level = false, eCoreEffectLevelGainType effLevelType = eCoreEffectLevelGainType.Medium,
            bool enabled = false)
        {
            Level = level;
            Value = value;
            SpecialLevel = special_level;
            EffLevelType = effLevelType;
            Enabled = enabled;
        }

        [ObservableProperty]
        private int _level = 10;

        [ObservableProperty]
        private double _value = 0.0;

        [ObservableProperty]
        private bool _specialLevel = false;
        partial void OnSpecialLevelChanged(bool oldValue, bool newValue) { update_value(); }

        [ObservableProperty]
        private eCoreEffectLevelGainType _effLevelType = eCoreEffectLevelGainType.Medium;
        partial void OnEffLevelTypeChanged(eCoreEffectLevelGainType oldValue, eCoreEffectLevelGainType newValue) { update_value(); }

        [ObservableProperty]
        private bool _enabled = false;

        private void update_value()
        {
            if (SpecialLevel)
                Value = ArcGridOptimizer.Params.CoreParam.CoreEffLevelGain[EffLevelType];
        }

        public CoreLevelValueDTO ToDTO()
        {
            return new CoreLevelValueDTO()
            {
                Level = Level,
                Value = Value,
                SpecialLevel = SpecialLevel,
                EffectLevelGainType = EffLevelType,
                Enabled = Enabled,
            };
        }

        public void FromDTO(CoreLevelValueDTO dto)
        {
            Level = dto.Level;
            Value = dto.Value;
            SpecialLevel = dto.SpecialLevel;
            EffLevelType = dto.EffectLevelGainType;
            Enabled = dto.Enabled;
        }
    }


    public partial class Core : ObservableObject
    {
        public Core() : this("", eCoreGrade.Hero, false) { }
        public Core(string name, eCoreGrade core_grade) : this(name, core_grade, true) { }

        private Core(string name, eCoreGrade core_grade, bool isInitialized)
        {
            _isInitialized = isInitialized;
            Name = name;

            CoreValues.Clear();
            foreach (var idx_level in ArcGridOptimizer.Params.CoreParam.CoreEffectLevels)
            {
                CoreValues.Add(new CoreLevelValue(idx_level.Value, 0.0, false));
            }
            CoreValues[0].SpecialLevel = true;
            CoreValues[1].SpecialLevel = true;
            CoreValues[2].SpecialLevel = true;

            Grade = core_grade;
        }

        [ObservableProperty]
        private bool _isInitialized = false;


        [ObservableProperty]
        private string _name = "";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SupplyWill))]
        private eCoreGrade _grade;
        partial void OnGradeChanged(eCoreGrade oldValue, eCoreGrade newValue)
        {
            int nowMax = ArcGridOptimizer.Params.CoreParam.CoreGradeMaxEffectLevel[Grade];
            nowMax = ArcGridOptimizer.Params.CoreParam.CoreEffectLevels[nowMax];
            foreach(var coreValue in CoreValues)
            {
                if (coreValue.Level > nowMax) coreValue.Enabled = false;
                else coreValue.Enabled = true;

            }
        }

        public int SupplyWill => ArcGridOptimizer.Params.CoreParam.MaxSupplyWill[Grade];

        [ObservableProperty]
        ObservableCollection<CoreLevelValue> _coreValues = new ObservableCollection<CoreLevelValue>();

        public CoreDTO ToDTO()
        {
            return new CoreDTO
            {
                IsInitialized = IsInitialized,
                Name =  Name,
                Grade = Grade,
                //SupplyWill = SupplyWill,
                CoreValues = CoreValues.Select(cv => cv.ToDTO()).ToList(),
            };
        }

        public void FromDTO(CoreDTO dto)
        {
            IsInitialized = dto.IsInitialized;
            Name = dto.Name;
            Grade = dto.Grade;
            //SupplyWill = dto.SupplyWill;
            CoreValues.Clear();
            foreach(var coreValueDTO in dto.CoreValues)
            {
                CoreLevelValue clv = new CoreLevelValue();
                clv.FromDTO(coreValueDTO);
                CoreValues.Add(clv);
            }
        }

    }
}
