using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using NordChecker.Models;
using NordChecker.Models.Settings;
using NordChecker.Services.Threading;
using Serilog;

namespace NordChecker.Services.Checker
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

            throw new DivideByZeroException();
            var states = Enum.GetValues<AccountState>().ToList();
            states.Remove(AccountState.Unchecked);
            states.Remove(AccountState.Reserved);
            account.State = states[_Random.Next(states.Count)];
        }

        void IChecker.HandleFailure(Account account, Exception exception)
        {
            Log.Warning(exception, "Exception thrown while checking {0}", account.ToString());
            account.State = AccountState.Invalid;
        }
    }
}
