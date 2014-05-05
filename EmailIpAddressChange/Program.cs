using System;
using NLog;

namespace EmailIpAddressChange
{
    internal class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static void Main(string[] args)
        {
            ServiceMainHelper.Run<Service>(args, CustomMain);
        }

        private static bool CustomMain(string[] args)
        {
            if (args.Length == 3 && 0 == string.Compare(args[0], "-credentials", StringComparison.OrdinalIgnoreCase))
            {
                Logger.Info("Username: {0} => {1}", args[1], StringProtector.ProtectString(args[1]));
                Logger.Info("Password: {0} => {1}", args[2], StringProtector.ProtectString(args[2]));
                return true;
            }
            Logger.Warn("Usage: {0} -credentials username password", AppDomain.CurrentDomain.FriendlyName);
            return false;
        }
    }
}