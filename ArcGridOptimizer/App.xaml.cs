using System.Configuration;
using System.Data;
using System.Windows;


using Microsoft.Extensions.DependencyInjection;
using ArcGridOptimizer.ViewModels;
using ArcGridOptimizer.Models;
using System.Windows.Input;

namespace ArcGridOptimizer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Services = ConfigureServices();
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets the current application instance as <see cref="App"/>.
        /// </summary>
        public new static App Current => (App)Application.Current;

        /// <summary>
        /// Gets the <see cref="IServiceProvider"/> instance to resolve application services
        /// </summary>
        public IServiceProvider Services { get; }

        /// <summary>
        /// Configures the services for the application
        /// </summary>
        /// <returns></returns>
        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            services.AddSingleton(typeof(Models.Opt.ArcgridCpSat));
            services.AddTransient(typeof(MainWindowViewModel));
            services.AddTransient(typeof(GemOptionViewModel));
            return services.BuildServiceProvider();
        }

    }

}
