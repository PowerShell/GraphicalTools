using System;
using Avalonia;
using System.Collections.Generic;
using System.Management.Automation;
using Avalonia.Logging.Serilog;
using OutGridView.ViewModels;
using OutGridView.Views;
using OutGridView.Services;
using System.Threading;
using OutGridView.Models;
using System.Linq;
using ReactiveUI;
using Avalonia.Threading;
using Avalonia.Controls;

namespace OutGridView
{
    public static class AvaloniaAppRunner
    {
        public static App App;
        public static AppBuilder Builder;
        private static ApplicationData _applicationData;
        private static Window _mainWindow;
        private static CancellationTokenSource _source;
        static AvaloniaAppRunner()
        {
            new CustomAssemblyLoadContext().LoadNativeLibraries();
            new CustomAssemblyLoadContext().LoadLibs();
            App = new App();
            Builder = BuildAvaloniaApp();
        }

        public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure(App)
               .UseReactiveUI()
               .UsePlatformDetect()
               .UseDataGrid()
               .LogToDebug()
               .SetupWithoutStarting();
        public static void RunApp(ApplicationData applicationData)
        {
            _applicationData = applicationData;
            AppMain(App);
        }
        private static void AppMain(Application app)
        {
            var db = new Database(_applicationData);
            _mainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(db),
            };

            _source = new CancellationTokenSource();

            _mainWindow.Show();
            _mainWindow.Closing += Window_Closing;

            App.Run(_source.Token);

            _source.Dispose();

        }
        private static void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _source.Cancel();
        }

        public static void CloseProgram()
        {
            _mainWindow.Close();
        }

        public static List<PSObject> GetPassThruObjects()
        {
            var mainWindowDataContext = _mainWindow.DataContext as MainWindowViewModel;
            return mainWindowDataContext.OutputObjects;
        }
    }
}
