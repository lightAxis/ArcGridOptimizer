using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;

using ArcGridOptimizer.Params;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;

namespace ArcGridOptimizer.ViewModels
{

    public class GemOptionMockViewModel : GemOptionViewModel
    {
        public GemOptionMockViewModel() : base() { }
    }

    public partial class GemOptionViewModel : ViewModelBase
    {
        public GemOptionViewModel()
        {
            Title = "젬 특옵 가중치 설정";

            List<double> zeros = Enumerable.Repeat(0.0, GemParam.MaxPropertyValue).ToList();
            AtkGains = new ObservableCollection<double>(zeros);
            ExtraDmgGains = new ObservableCollection<double>(zeros);
            BossDmgGains = new ObservableCollection<double>(zeros);

            pull_params();
        }

        private void pull_params()
        {
            for (int i = 0; i < GemParam.MaxPropertyValue; i++)
            {
                AtkGains[i] = GemParam.AtkValues[i + 1];
                ExtraDmgGains[i] = GemParam.ExtraDmgValues[i + 1];
                BossDmgGains[i] = GemParam.BossDmgValues[i + 1];
            }
        }

        private void push_params()
        {
            for (int i = 0; i < GemParam.MaxPropertyValue; i++)
            {
                GemParam.AtkValues[i + 1] = AtkGains[i];
                GemParam.ExtraDmgValues[i + 1] = ExtraDmgGains[i];
                GemParam.BossDmgValues[i + 1] = BossDmgGains[i];
            }
        }

        private void reset_params()
        {             
            for (int i = 0; i < GemParam.MaxPropertyValue; i++)
            {
                AtkGains[i] = GemParamDefault.AtkValues[i + 1];
                ExtraDmgGains[i] = GemParamDefault.ExtraDmgValues[i + 1];
                BossDmgGains[i] = GemParamDefault.BossDmgValues[i + 1];
            }
        }

        [ObservableProperty]
        ObservableCollection<double> _atkGains = new ObservableCollection<double>();

        [ObservableProperty]
        ObservableCollection<double> _extraDmgGains = new ObservableCollection<double>();

        [ObservableProperty]
        ObservableCollection<double> _bossDmgGains = new ObservableCollection<double>();

        [RelayCommand]
        private void onReset(Window obj)
        {
            var res = MessageBox.Show(obj, "포셔 유튜브에 나오는 기본값으로 초기화됩니다", "초기화 하시겠습니까?", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
            if (res != MessageBoxResult.OK)
                return;
            reset_params();
        }

        [RelayCommand]
        private void onApply(Window obj)
        {
            push_params();
            MessageBox.Show(obj, "설정이 적용되었습니다", "굿", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
