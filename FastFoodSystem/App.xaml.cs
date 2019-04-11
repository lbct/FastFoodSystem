using FastFoodSystem.Database;
using FastFoodSystem.PopUps;
using FastFoodSystem.Scripts;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FastFoodSystem
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Dictionary<Type, UserControl> instances;
        public static DatabaseEntities Database { get; private set; }
        public static MainWindow MainWin { get; private set; }
        private static BackgroundProcess process;


        public App()
        {
            instances = new Dictionary<Type, UserControl>();
            Database = new DatabaseEntities();
            var currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += CurrentDomain_UnhandledException;
            process = new BackgroundProcess();
            process.Start();
        }

        public static void CloseAll()
        {
            process.Kill();
        }

        [STAThread]
        public static void Main(string[] args)
        {
            Init(args);
        }

        public static async Task<bool> RunAsync(Action action)
        {
            Task<object> task = new Task<object>(() =>
            {
                bool correct = true;
                try
                {
                    lock (Database)
                    {
                        action.Invoke();
                    }
                }
                catch (Exception e)
                {
                    Current.Dispatcher.Invoke(() => ShowMessage(e.Message, false, () =>
                    {
                        Database = new DatabaseEntities();
                    }));
                    correct = false;
                }
                return correct;
            });
            process.AddTask(task);
            return (bool)await task;
        }

        public static async Task<T> RunAsync<T>(Func<T> action)
        {
            Task<object> task = new Task<object>(() =>
            {
                try
                {
                    T result = default(T);
                    lock (Database)
                    {
                        result = action.Invoke();
                    }
                    return result;
                }
                catch (Exception e)
                {
                    Current.Dispatcher.Invoke(() => ShowMessage(e.Message, false, () =>
                    {
                        Database = new DatabaseEntities();
                    }));
                    return default(T);
                }
            });
            process.AddTask(task);
            return (T)await task;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Debug.WriteLine("**** FATAL ERROR ****");
            Shutdown();
        }

        public static void RunOnMain<T>(Action<T> action, T parameter)
        {
            Current.Dispatcher.Invoke(() => action.Invoke(parameter));
        }

        public static void RunOnMain(Action action)
        {
            Current.Dispatcher.Invoke(action);
        }

        public static T OpenSystemPage<T>() where T : SystemPageClass
        {
            var page = GetSystemPage<T>();
            MainWin.container.SetPage(page);
            page.Refresh();
            return page;
        }

        public static T GetSystemPage<T>() where T : SystemPageClass
        {
            T page;
            if (instances.ContainsKey(typeof(T)))
                page = instances[typeof(T)] as T;
            else
            {
                page = Activator.CreateInstance<T>();
                instances.Add(typeof(T), page);
            }
            return page;
        }

        public static T OpenSystemPopUp<T>() where T : SystemPopUpClass
        {
            T page = GetSystemPopUp<T>();
            MainWin.container.SetPopUp(page);
            MainWin.container.ShowPopUp();
            return page;
        }

        public static SystemPageClass GetCurrentPage()
        {
            return MainWin.container.GetPage<SystemPageClass>();
        }

        public static T GetSystemPopUp<T>() where T : SystemPopUpClass
        {
            T page;
            if (instances.ContainsKey(typeof(T)))
                page = instances[typeof(T)] as T;
            else
            {
                page = Activator.CreateInstance<T>();
                instances.Add(typeof(T), page);
            }
            return page;
        }

        public static void Remove<T>()
        {
            if (instances.ContainsKey(typeof(T)))
                instances.Remove(typeof(T));
        }

        public static void ShowLoad()
        {
            Remove<LoadPopUp>();
            OpenSystemPopUp<LoadPopUp>();
        }

        public static void ShowMessage(string txt, bool correct = true, Action action = null)
        {
            if (correct)
                OpenSystemPopUp<CorrectPopUp>().SetMsg(txt, action);
            else
                OpenSystemPopUp<ErrorPopUp>().SetMsg(txt, action);
        }

        public static void CloseSystemPopUp()
        {
            MainWin.container.HidePopUp();
        }

        private static void Init(string[] args)
        {
            var proc = Process.GetCurrentProcess();
            var processName = proc.ProcessName.Replace(".vshost", "");
            var runningProcess = Process.GetProcesses()
                .FirstOrDefault(x => (x.ProcessName == processName ||
                                x.ProcessName == proc.ProcessName ||
                                x.ProcessName == proc.ProcessName + ".vshost") && x.Id != proc.Id);

            if (runningProcess == null)
            {
                var app = new App();
                app.InitializeComponent();
                MainWin = new MainWindow();

                FastFoodSystem.MainWindow.HandleParameter(args);
                app.Run(MainWin);
                return;
            }

            if (args.Length > 0)
                UnsafeNative.SendMessage(runningProcess.MainWindowHandle, string.Join(" ", args));
        }
    }
}
