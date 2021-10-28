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
    internal class MockChecker : IChecker, IBreakable<TimeoutBreakpointContext<Account>>
    {
        public int Timeout { get; set; }

        public MockChecker(int timeout) => Timeout = timeout;

        public void ProcessAccount(Account account)
        {
            var context = new TimeoutBreakpointContext<Account>(
                account,
                account.MasterToken,
                Stopwatch.StartNew());

            for (int i = 0; i < 4; i++)
            {
                Thread.Sleep(500);
                (this as IBreakable<TimeoutBreakpointContext<Account>>).HandleBreakpointIfNeeded(context);
            }

            account.State = new Random().Next(11) switch
            {
                <= 1 => AccountState.Premium,
                <= 3 => AccountState.Free,
                _ => AccountState.Invalid
            };
            return;
        }

        bool IBreakable<TimeoutBreakpointContext<Account>>.IsCancelationNeededFor(TimeoutBreakpointContext<Account> context)
            => context.Watch.ElapsedMilliseconds > Timeout;
    }
}
