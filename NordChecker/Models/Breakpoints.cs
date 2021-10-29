using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NordChecker.Models
{
    public interface IBreakpointHandler
    {
        public void HandleBreakpointIfNeeded();
    }

    public interface IBreakpointContext
    {
        public MasterToken Token { get; init; }
        public void OnBreakpointStarted() { }
        public void OnBreakpointFinished() { }
    }

    public class BreakpointHandler : IBreakpointHandler
    {
        private IBreakpointContext context;

        public BreakpointHandler(IBreakpointContext context)
            => this.context = context;

        public virtual void HandleBreakpointIfNeeded()
        {
            if (context.Token.IsCancellationRequested ||
                context.Token.IsPauseRequested)
            {
                context.OnBreakpointStarted();
                context.Token.ThrowOrWaitIfRequested();
                context.OnBreakpointFinished();
            }
        }
    }

    public class TimeoutBreakpointContext : IBreakpointContext
    {
        public MasterToken Token { get; init; }
        public Stopwatch Watch { get; init; }
        public TimeSpan Timeout { get; init; }

        public TimeoutBreakpointContext(
            MasterToken token,
            Stopwatch watch,
            TimeSpan timeout)
        {
            Token = token;
            Watch = watch;
            Timeout = timeout;
        }
    }

    public class TimeoutBreakpointHandler : BreakpointHandler
    {
        private TimeoutBreakpointContext context;

        public TimeoutBreakpointHandler(TimeoutBreakpointContext context)
            : base(context) => this.context = context;

        public override void HandleBreakpointIfNeeded()
        {
            Log.Error("TIMEOUT CHECK {0} / {1}", context.Watch.Elapsed.TotalSeconds, context.Timeout.TotalSeconds);
            base.HandleBreakpointIfNeeded();
            if (context.Watch.Elapsed > context.Timeout)
            {
                Log.Error("TIMEOUT REACHED");
                context.Token.Cancel();
                context.Token.ThrowIfCancellationRequested();
            }
        }
    }
}
