using System;
using System.Configuration.Install;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace EmailIpAddressChange
{
    internal abstract class ServiceMainHelper
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static readonly EventWaitHandle Signal = new ManualResetEvent(false);

        static ServiceMainHelper()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, args) => Logger.FatalException("Unhandled exception", (Exception) args.ExceptionObject);
            TaskScheduler.UnobservedTaskException += (sender, args) => Logger.FatalException("Unhandled exception", args.Exception);
        }

        public static void Run<T>(string[] args) where T : ServiceBase, new()
        {
            try
            {
                if (Environment.UserInteractive)
                {
                    InitConsoleLogger();
                    Logger.Debug("Started in user mode");
                    ConsoleMain<T>(args);
                }
                else
                {
                    Logger.Debug("Started in service mode");
                    ServiceMain<T>();
                }
                Logger.Debug("Finished");
            }
            catch (Exception ex)
            {
                Logger.FatalException("Exception is caught", ex);
            }
        }

        private static void InitConsoleLogger()
        {
            var configuration = LogManager.Configuration;
            var target = new ColoredConsoleTarget
            {
                Layout = "${longdate} ${threadid:padding=3:padCharacter=0} ${logger} - ${message}${onexception:${newline}${exception:format=ToString}}",
            };
            configuration.AddTarget("stdout", target);
            configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, target));
            LogManager.Configuration = configuration;
        }

        private static void ServiceMain<T>() where T : ServiceBase, new()
        {
            ServiceBase.Run(new T());
        }

        private static void ConsoleMain<T>(string[] args) where T : ServiceBase, new()
        {
            var installKeys = new[] {"-i", "--install"};
            var uninstallKeys = new[] {"-u", "--uninstall"};
            var consoleKeys = new[] {"--console"};
            var parameter = string.Concat(args);

            if (installKeys.Contains(parameter))
            {
                ManagedInstallerClass.InstallHelper(new[] {Assembly.GetExecutingAssembly().Location});
            }
            else if (uninstallKeys.Contains(parameter))
            {
                ManagedInstallerClass.InstallHelper(new[] {"/u", Assembly.GetExecutingAssembly().Location});
            }
            else if (consoleKeys.Contains(parameter))
            {
                RunAsService<T>();
            }
            else
            {
                Logger.Warn("Usage: {0} {1}",
                            AppDomain.CurrentDomain.FriendlyName,
                            string.Join("|", installKeys.Union(uninstallKeys).Union(consoleKeys)));
                Environment.Exit(1);
            }
        }

        private static void RunAsService<T>() where T : ServiceBase, new()
        {
            Logger.Info("Executing the Service interactively. Press Ctrl+C to stop...");

            Console.CancelKeyPress += OnCancelKeyPress;
            var service = new T();
            Logger.Debug("Attempting to start service...");
            typeof(T).GetMethod("OnStart", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(service, new object[] {new string[0]});
            Logger.Debug("Service started");

            Signal.WaitOne();
            Logger.Debug("Attempting to stop service...");
            service.Stop();
            Logger.Debug("Service stopped");
        }

        private static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs args)
        {
            args.Cancel = true;
            Logger.Info("Ctrl+C detected.");
            Signal.Set();
        }
    }
}