using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace NordChecker.Models
{
    internal class MockChecker : IChecker
    {
        private AppSettings appSettings;

        public MockChecker(AppSettings appSettings) => this.appSettings = appSettings;

        void IChecker.Check(Account account)
        {
            var context = new TimeoutBreakpointContext(account.MasterToken, Stopwatch.StartNew(), appSettings.Timeout);
            IBreakpointHandler breakpointHandler = new TimeoutBreakpointHandler(context);

            for (int i = 0; i < 10; i++)
            {
                Thread.Sleep(500);
                breakpointHandler.HandleBreakpointIfNeeded();
            }

            account.State = new Random().Next(11) switch
            {
                <= 1 => AccountState.Premium,
                <= 3 => AccountState.Free,
                _ => AccountState.Invalid
            };
        }

        void IChecker.HandleFailure(Account account, Exception exception)
            => account.State = AccountState.Invalid;
    }
}
