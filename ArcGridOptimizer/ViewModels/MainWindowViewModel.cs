using ArcGridOptimizer.Enums;
using ArcGridOptimizer.ViewModels;
using ArcGridOptimizer.ViewModels.Items;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using ArcGridOptimizer.Models;
using Microsoft.Win32;
using System.IO;
using System.Runtime.InteropServices.Marshalling;
using Google.OrTools.ConstraintSolver;
using Google.OrTools.ModelBuilder;

namespace ArcGridOptimizer.ViewModels
{
    class MainWindowMockViewModel : MainWindowViewModel
    {
        public MainWindowMockViewModel() : base() { }
    }

    partial class MainWindowViewModel : ViewModelBase
    {
        protected MainWindowViewModel()
        {
            Title = "아크 그리드 젬 배치 최적화 (모의 데이터)";

            Gems.Add(new Items.Gem("더미 젬1", 1, 4, new GemProperty(eGemProperty.Attack, 1), new GemProperty(eGemProperty.None, 0)));
            Gems.Add(new Items.Gem("더미 젬2", 2, 3, new GemProperty(eGemProperty.BossDamage, 2), new GemProperty(eGemProperty.BossDamage, 1)));
            Gems.Add(new Items.Gem("더미 젬3", 3, 2, new GemProperty(eGemProperty.ExtraDamage, 3), new GemProperty(eGemProperty.ExtraDamage, 2)));
            Gems.Add(new Items.Gem("더미 젬4", 4, 1, new GemProperty(eGemProperty.None, 0), new GemProperty(eGemProperty.Attack, 3)));

            for (int i = 0; i < 3; i++)
            {
                Cores.Add(new Items.Core("더미 코어1", eCoreGrade.Hero));
                Cores.Add(new Items.Core("더미 코어2", eCoreGrade.Legend));
                Cores.Add(new Items.Core("더미 코어3", eCoreGrade.Relic));
            }

            var gems1 = new List<GemOptResult>();
            gems1.Add(new GemOptResult() { Name = "더미 젬11", RequiredWill = 1, RewardPoint = 3, Property1 = new GemProperty(eGemProperty.Attack, 1), Property2 = new GemProperty(), Score = 0.111 });
            gems1.Add(new GemOptResult() { Name = "더미 젬22", RequiredWill = 2, RewardPoint = 2, Property1 = new GemProperty(eGemProperty.BossDamage, 5), Property2 = new GemProperty(eGemProperty.None, 0), Score = 0.111 });
            gems1.Add(new GemOptResult() { Name = "더미 젬33", RequiredWill = 3, RewardPoint = 1, Property1 = new GemProperty(eGemProperty.ExtraDamage, 3), Property2 = new GemProperty(eGemProperty.BossDamage,1), Score = 0.1111 });
            var cores1 = new List<CoreOptResult>();
            cores1.Add(new CoreOptResult(new Core("가짜코어 1", eCoreGrade.Hero), gems1));
            cores1.Add(new CoreOptResult(new Core("가짜코어 2", eCoreGrade.Legend), gems1));
            cores1.Add(new CoreOptResult(new Core("가짜코어 3", eCoreGrade.Relic), gems1));
            OptResult = new OptResult(cores1);



            Cores.CollectionChanged += (_, __) =>
            {
                onAddCoreCommand.NotifyCanExecuteChanged();
                onRemoveCoreCommand.NotifyCanExecuteChanged();
            };
        }

        public MainWindowViewModel(Models.Opt.ArcgridCpSat optimizer)
        {
            Title = "아크 그리드 최적화 v1.1";
            _optimizer = optimizer;

            Models.JsonFileIO.CheckAndCreateDataDirs();

            for (int i = 0; i < 3; i++)
            {
                Cores.Add(new Items.Core());
            }

            Cores.CollectionChanged += (_, __) =>
            {
                onAddCoreCommand.NotifyCanExecuteChanged();
                onRemoveCoreCommand.NotifyCanExecuteChanged();
            };
        }

        Models.Opt.ArcgridCpSat _optimizer;

        // TAB : Gems

        [ObservableProperty]
        ObservableCollection<Items.Gem> _gems = new ObservableCollection<Items.Gem>();

        private GemListDTO GemsToDTOList()
        {
            return new GemListDTO()
            {
                Gems = Gems.Select(gem => gem.ToDTO()).ToList()
            };
        }

        private void LoadGemsFromDTOList(GemListDTO dto)
        {
            Gems.Clear();
            foreach(var gemDTO in dto.Gems)
            {
                var gem = new Items.Gem();
                gem.FromDTO(gemDTO);
                Gems.Add(gem);
            }
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SelectedGemExists))]
        [NotifyCanExecuteChangedFor(nameof(onCopyGemCommand))]
        [NotifyCanExecuteChangedFor(nameof(onRemoveGemCommand))]
        [NotifyCanExecuteChangedFor(nameof(onMoveGemUpCommand))]
        [NotifyCanExecuteChangedFor(nameof(onMoveGemDownCommand))]
        Items.Gem? _selectedGem;

        [ObservableProperty]
        int _selectedGemIndex;

        public bool SelectedGemExists => SelectedGem != null;

        [RelayCommand]
        private void onAddGem()
        {
            if (SelectedGem is null)
            {
                Gems.Add(new Items.Gem());
                SelectedGemIndex = Gems.Count - 1;
            }
            else
            {
                Gems.Insert(SelectedGemIndex, new Items.Gem());
            }
        }
        [RelayCommand(CanExecute = nameof(SelectedGemExists))]
        private void onCopyGem()
        {
            if (SelectedGem is null) return;
            Gems.Insert(SelectedGemIndex, SelectedGem.Clone());
        }
        [RelayCommand(CanExecute = nameof(SelectedGemExists))]
        private void onRemoveGem()
        {
            if (SelectedGem is null) return;

            int last_selected_index = SelectedGemIndex;
            Gems.Remove(SelectedGem);

            if (Gems.Count != 0) { 
                SelectedGemIndex = Math.Clamp(last_selected_index, 0, Gems.Count-1); 
            }
        }
        [RelayCommand(CanExecute = nameof(SelectedGemExists))]
        private void onMoveGemUp()
        {
            if (SelectedGem is null) return;
            int idx = SelectedGemIndex;
            if (idx <= 0) return;
            Gems.Move(idx, idx - 1);
            SelectedGemIndex = idx - 1;
        }
        [RelayCommand(CanExecute = nameof(SelectedGemExists))]
        private void onMoveGemDown()
        {
            if (SelectedGem is null) return;
            int idx = SelectedGemIndex;
            if (idx >= Gems.Count - 1) return;
            Gems.Move(idx, idx + 1);
            SelectedGemIndex = idx + 1;
        }
        [RelayCommand]
        private void onGemOptionConfig(Window obj)
        {
            var dlg = new Views.GemOption()
            {
                Owner = obj, // 부모 윈도우 지정
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            dlg.ShowDialog();
        }
        [RelayCommand]
        private async Task onGemSetSave()
        {
            var dlg = new SaveFileDialog()
            {
                Title = "젬 목록 파일 열기",
                DefaultExt = ".json",
                Filter = "JSON 파일 (*.json)|*.json|모든 파일 (*.*)|*.*",
                InitialDirectory = Path.GetFullPath(Models.JsonFileIO.GetGemListDefaultPath()),
                CheckFileExists = false,
                CheckPathExists = true,
            };
            var res = dlg.ShowDialog();
            if (res != true) return;

            var ok = await Models.JsonFileIO.SaveGemListAsync(dlg.FileName, GemsToDTOList());
            if (!ok)
            {
                MessageBox.Show(Title, "젬 목록 파일 저장에 실패했습니다.", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        [RelayCommand]
        private async Task onGemSetLoad()
        {
            var dlg = new OpenFileDialog()
            {
                Title = "젬 목록 파일 열기",
                DefaultExt = ".json",
                Filter = "JSON 파일 (*.json)|*.json|모든 파일 (*.*)|*.*",
                InitialDirectory = Path.GetFullPath(Models.JsonFileIO.GetGemListDefaultPath()),
                CheckFileExists = true,
                CheckPathExists = true,
            };
            var res = dlg.ShowDialog();
            if (res != true) return;

            var dto = await Models.JsonFileIO.LoadGemListAsync(dlg.FileName);
            if (dto is null)
            {
                MessageBox.Show(Title, "젬 목록 파일 로드에 실패했습니다.", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            LoadGemsFromDTOList(dto);
        }

        // TAB : Cores

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(onAddCoreCommand))]
        [NotifyCanExecuteChangedFor(nameof(onRemoveCoreCommand))]
        private ObservableCollection<Items.Core> _cores = new ObservableCollection<Items.Core>();

        private CoreListDTO CoresToDTOList()
        {
            return new CoreListDTO()
            {
                Cores = Cores.Select(core => core.ToDTO()).ToList(),
            };
        }
        private void LoadCoresFromDTOList(CoreListDTO dto)
        {
            for(int i=0;i<dto.Cores.Count;i++)
            {
                if (Cores.Count <= i) break;

                var core = new Items.Core();
                core.FromDTO(dto.Cores[i]);
                Cores[i] = core;
            }
        }

        [RelayCommand(CanExecute=nameof(onAddCoreCanExecute))]
        private void onAddCore(string idx)
        {
            if (idx == null) return;
            Cores[Int32.Parse(idx)] = new Items.Core("새 코어", eCoreGrade.Hero);
        }
        private bool onAddCoreCanExecute(string idx)
        {
            if (idx == null) return false;
            if (Cores[Int32.Parse(idx)].IsInitialized == false) return true;
            return false;
        }

        [RelayCommand(CanExecute=nameof(onRemoveCoreCanExecute))]
        private void onRemoveCore(string idx)
        {
            if (idx == null) return;
            Cores[Int32.Parse(idx)] = new Items.Core();
        }
        private bool onRemoveCoreCanExecute(string idx)
        {
            if (idx == null) return false;
            if (Cores[Int32.Parse(idx)].IsInitialized == false) return false;
            return true;
        }

        [RelayCommand]
        private async Task onCoreSetSave()
        {
            var dlg = new SaveFileDialog()
            {
                Title = "코어 목록 파일 저장",
                DefaultExt = ".json",
                Filter = "JSON 파일 (*.json)|*.json|모든 파일 (*.*)|*.*",
                InitialDirectory = Path.GetFullPath(Models.JsonFileIO.GetCoreListDefaultPath()),
                CheckFileExists = false,
                CheckPathExists = true,
            };
            var res = dlg.ShowDialog();
            if (res != true) return;

            var ok = await Models.JsonFileIO.SaveCoreListAsync(dlg.FileName, CoresToDTOList());
            if (!ok)
            {
                MessageBox.Show(Title, "코어 목록 파일 저장에 실패했습니다.", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task onCoreSetLoad()
        {
            var dlg = new OpenFileDialog()
            {
                Title = "코어 목록 파일 열기",
                DefaultExt = ".json",
                Filter = "JSON 파일 (*.json)|*.json|모든 파일 (*.*)|*.*",
                InitialDirectory = Path.GetFullPath(Models.JsonFileIO.GetCoreListDefaultPath()),
                CheckFileExists = true,
                CheckPathExists = true,
            };
            var res = dlg.ShowDialog();
            if (res != true) return;

            var dto = await Models.JsonFileIO.LoadCoreListAsync(dlg.FileName);
            if (dto is null)
            {
                MessageBox.Show(Title, "젬 목록 파일 로드에 실패했습니다.", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            LoadCoresFromDTOList(dto);
        }

        // 최적화 관련

        [ObservableProperty]
        OptResult _optResult = new OptResult();

        private OptResultDTO OptResultToDTO()
        {
            return OptResult.ToDTO();
        }

        private void LoadOptResultFromDTO(OptResultDTO dto)
        {
            var res = new OptResult();
            res.FromDTO(dto);
            OptResult = res;
        }

        [RelayCommand]
        private void onOptimizeStart()
        {
            var res = Models.Opt.ArcgridCpSat.Solve(CoresToDTOList(), GemsToDTOList());
            LoadOptResultFromDTO(res);
        }

        [RelayCommand]
        private async Task onOptResultSave()
        {
            var dlg = new SaveFileDialog()
            {
                Title = "최적화 결과 파일 저장",
                DefaultExt = ".json",
                Filter = "JSON 파일 (*.json)|*.json|모든 파일 (*.*)|*.*",
                InitialDirectory = Path.GetFullPath(Models.JsonFileIO.getOptResultDefaultPath()),
                CheckFileExists = false,
                CheckPathExists = true,
            };
            var res = dlg.ShowDialog();
            if (res != true) return;
            var ok = await Models.JsonFileIO.SaveOptResultAsync(dlg.FileName, OptResultToDTO());
            if (!ok)
            {
                MessageBox.Show(Title, "최적화 결과 파일 저장에 실패했습니다.", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task onOptResultLoad()
        {
            var dlg = new OpenFileDialog()
            {
                Title = "최적화 결과 파일 열기",
                DefaultExt = ".json",
                Filter = "JSON 파일 (*.json)|*.json|모든 파일 (*.*)|*.*",
                InitialDirectory = Path.GetFullPath(Models.JsonFileIO.getOptResultDefaultPath()),
                CheckFileExists = true,
                CheckPathExists = true,
            };
            var res = dlg.ShowDialog();
            if (res != true) return;
            var dto = await Models.JsonFileIO.LoadOptResultAsync(dlg.FileName);
            if (dto is null)
            {
                MessageBox.Show(Title, "최적화 결과 파일 로드에 실패했습니다.", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            LoadOptResultFromDTO(dto);
        }

        [RelayCommand]
        private void onDebug1()
        {
            Gems.Add(new Items.Gem("더미 젬1", 1, 4, new GemProperty(eGemProperty.Attack, 1), new GemProperty(eGemProperty.None, 0)));
            Gems.Add(new Items.Gem("더미 젬2", 2, 3, new GemProperty(eGemProperty.BossDamage, 2), new GemProperty(eGemProperty.BossDamage, 1)));
            Gems.Add(new Items.Gem("더미 젬3", 3, 2, new GemProperty(eGemProperty.ExtraDamage, 3), new GemProperty(eGemProperty.ExtraDamage, 2)));
            Gems.Add(new Items.Gem("더미 젬4", 4, 1, new GemProperty(eGemProperty.None, 0), new GemProperty(eGemProperty.Attack, 3)));
        }
        [RelayCommand]
        private void onDebug2()
        {
            Cores[0] = new Items.Core("더미 코어1", eCoreGrade.Hero);
            Cores[1] = new Items.Core("더미 코어2", eCoreGrade.Legend);
            Cores[2] = new Items.Core("더미 코어3", eCoreGrade.Hero);
        }

    }
}
