using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ArcGridOptimizer.Enums;
using ArcGridOptimizer.Params;
using System.Windows;
using System.Linq.Expressions;

using ArcGridOptimizer.Models;

namespace ArcGridOptimizer.ViewModels.Items
{
    public partial class GemProperty : ObservableObject
    {

        public GemProperty(eGemProperty type = eGemProperty.None, int value = 0)
        {
            Type = type;
            Value = value;
        }

        public GemProperty Clone()
        {
            return new GemProperty() { 
                Type = this.Type, 
                Value = this.Value 
            };
        }

        [ObservableProperty]
        private eGemProperty _type = eGemProperty.None;

        private int _value = 0;
        public int Value
        {
            get => _value;
            set => SetProperty(ref _value, Math.Clamp(value, 0, Params.GemParam.MaxPropertyValue));
        }

        public GemPropertyDTO ToDTO()
        {
            return new GemPropertyDTO()
            {
                Type = this.Type,
                Value = this.Value,
            };
        }

        public void FromDTO(GemPropertyDTO dto)
        {
            Type = dto.Type;
            Value = dto.Value;
        }
    }



    public partial class Gem : ObservableObject
    {
        public Gem(string name = "", int requiredWill = 0, int rewardPoint = 0, GemProperty? property1 = null, GemProperty? property2 = null)
        {
            Name = name;
            RequiredWill = requiredWill;
            RewardPoint = rewardPoint;
            Property1 = property1 ?? new GemProperty();
            Property2 = property2 ?? new GemProperty();
        }

        public Gem Clone()
        {
            return new Gem()
            {
                Name = this.Name,
                RequiredWill = this.RequiredWill,
                RewardPoint = this.RewardPoint,
                Property1 = this.Property1.Clone(),
                Property2 = this.Property2.Clone(),
            };
        }

        [ObservableProperty]
        private string _name = "";

        private int _requiredWill = 0;
        public int RequiredWill
        {
            get => _requiredWill;
            set => SetProperty(ref _requiredWill, Math.Clamp(value, 0, GemParam.MaxRequiredWill));
        }

        private int _rewardPoint = 0;
        public int RewardPoint
        {
            get => _rewardPoint;
            set => SetProperty(ref _rewardPoint, Math.Clamp(value, 0, GemParam.MaxRewardPoint));
        }

        [ObservableProperty]
        private GemProperty _property1 = new GemProperty();

        [ObservableProperty]
        private GemProperty _property2 = new GemProperty();

        public GemDTO ToDTO()
        {
            return new GemDTO()
            {
                Name = this.Name,
                RequiredWill = this.RequiredWill,
                RewardPoint = this.RewardPoint,
                Property1 = this.Property1.ToDTO(),
                Property2 = this.Property2.ToDTO(),
            };
        }

        public void FromDTO(GemDTO dto)
        {
            Name = dto.Name;
            RequiredWill = dto.RequiredWill;
            RewardPoint = dto.RewardPoint;
            Property1.FromDTO(dto.Property1);
            Property2.FromDTO(dto.Property2);
        }
    }


}
