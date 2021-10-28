using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NordChecker.Models
{
    public interface IBreakpointContext
    {
        public MasterToken Token { get; init; }
        public void OnBreakpointStarted() { }
        public void OnBreakpointFinished() { }
    }

    public interface IBreakpointContext<TPayload> : IBreakpointContext
    {
        public TPayload Payload { get; init; }
    }

    public class TimeoutBreakpointContext<TPayload> : IBreakpointContext<TPayload>
    {
        public TPayload Payload { get; init; }
        public MasterToken Token { get; init; }
        public Stopwatch Watch { get; init; }

        public TimeoutBreakpointContext(
            TPayload payload,
            MasterToken token,
            Stopwatch watch)
        {
            Payload = payload;
            Token = token;
            Watch = watch;
        }

        public void OnBreakpointStarted() => Watch.Stop();
        public void OnBreakpointFinished() => Watch.Start();
    }

    public interface IBreakable<TContext> where TContext : IBreakpointContext
    {
        public void HandleBreakpointIfNeeded(TContext context)
        {
            if (IsCancelationNeededFor(context) ||
                context.Token.IsCancellationRequested ||
                context.Token.IsPauseRequested)
            {
                context.OnBreakpointStarted();
                if (IsCancelationNeededFor(context))
                    context.Token.Cancel();
                if (IsPauseNeededFor(context))
                    context.Token.Pause();
                context.Token.ThrowOrWaitIfRequested();
                context.OnBreakpointFinished();
            }
        }

        protected bool IsCancelationNeededFor(TContext context) => false;
        protected bool IsPauseNeededFor(TContext context) => false;
    }
}
