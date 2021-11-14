using System;
using System.Diagnostics;
using System.Threading;
using NordChecker.Models.Settings;
using NordChecker.Threading;
using Serilog;

namespace NordChecker.Models.Domain.Checker
{
    internal class MockChecker : IChecker
    {
        private const int ElapsedMilliseconds = 5000;
        private const int BreakpointCount = 5;
        
        private readonly AppSettings _AppSettings;
        private readonly Random _Random = new();

        public MockChecker(AppSettings appSettings) => _AppSettings = appSettings;

        void IChecker.Check(Account account)
        {
            var context = new TimeoutBreakpointContext(account.MasterToken, Stopwatch.StartNew(), _AppSettings.Timeout);
            IBreakpointHandler breakpointHandler = new TimeoutBreakpointHandler(context);
            
            for (int i = 0; i < BreakpointCount; i++)
            {
                Thread.Sleep(ElapsedMilliseconds / BreakpointCount);
                breakpointHandler.HandleBreakpointIfNeeded();
            }

            var states = Enum.GetValues<AccountState>();
            account.State = states[_Random.Next(states.Length)];
        }

        void IChecker.HandleFailure(Account account, Exception exception)
        {
            Log.Warning(exception, "Exception thrown while checking {0}", account.ToString());
            account.State = AccountState.Invalid;
        }
    }
}
