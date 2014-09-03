using System;
using System.Configuration.Install;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace DuckDnsUpdateService
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
                Layout = "${longdate} ${logger:shortName=true} - ${message}${onexception:${newline}${exception:format=ToString}}",
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
            var parameter = string.Concat(args);

            if (0 == string.Compare(parameter, "-i", StringComparison.OrdinalIgnoreCase))
            {
                ManagedInstallerClass.InstallHelper(new[] {Assembly.GetExecutingAssembly().Location});
            }
            else if (0 == string.Compare(parameter, "-u", StringComparison.OrdinalIgnoreCase))
            {
                ManagedInstallerClass.InstallHelper(new[] {"/u", Assembly.GetExecutingAssembly().Location});
            }
            else if (0 == string.Compare(parameter, "--console", StringComparison.OrdinalIgnoreCase))
            {
                RunAsService<T>();
            }
            else
            {
                Logger.Warn("Usage: {0} -i | -u | --console", AppDomain.CurrentDomain.FriendlyName);
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