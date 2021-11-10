using System;
using System.Diagnostics;
using System.Threading;

namespace NordChecker.Models
{
    internal class MockChecker : IChecker
    {
        private readonly AppSettings _AppSettings;
        private readonly Random _Random = new();

        public MockChecker(AppSettings appSettings) => _AppSettings = appSettings;

        void IChecker.Check(Account account)
        {
            var context = new TimeoutBreakpointContext(account.MasterToken, Stopwatch.StartNew(), _AppSettings.Timeout);
            IBreakpointHandler breakpointHandler = new TimeoutBreakpointHandler(context);

            for (int i = 0; i < 10; i++)
            {
                Thread.Sleep(500);
                breakpointHandler.HandleBreakpointIfNeeded();
            }

            account.State = _Random.Next(11) switch
            {
                <= 1 => AccountState.Premium,
                <= 3 => AccountState.Free,
                _ => AccountState.Invalid
            };
        }

        void IChecker.HandleFailure(Account account, Exception exception) =>
            account.State = AccountState.Invalid;
    }
}
