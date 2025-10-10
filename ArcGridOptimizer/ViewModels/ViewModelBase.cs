using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;

namespace ArcGridOptimizer.ViewModels
{
    /// <summary>
    /// viewmodel base
    /// </summary>
    public abstract class ViewModelBase : ObservableObject
    {
        private string _title = "";

        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }
    }
}
