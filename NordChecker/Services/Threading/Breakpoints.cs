using System;
using System.Diagnostics;

namespace NordChecker.Services.Threading
{
    public interface IBreakpointHandler
    {
        public void HandleBreakpointIfNeeded();
    }

    public record BreakpointContext(MasterToken MasterToken);

    public class BreakpointHandler : IBreakpointHandler
    {
        private readonly BreakpointContext _Context;

        public BreakpointHandler(BreakpointContext context) => _Context = context;

        public virtual void HandleBreakpointIfNeeded()
            => _Context.MasterToken.ThrowOrWaitIfRequested();
    }
    
    public record TimeoutBreakpointContext(
        MasterToken MasterToken,
        Stopwatch Watch,
        TimeSpan Timeout)
        : BreakpointContext(MasterToken);
    
    public class TimeoutBreakpointHandler : BreakpointHandler
    {
        private readonly TimeoutBreakpointContext _Context;

        public TimeoutBreakpointHandler(TimeoutBreakpointContext context)
            : base(context) => _Context = context;

        public override void HandleBreakpointIfNeeded()
        {
            _Context.Watch.Stop();

            base.HandleBreakpointIfNeeded();
            if (_Context.Watch.Elapsed > _Context.Timeout)
            {
                _Context.MasterToken.Cancel();
                _Context.MasterToken.ThrowIfCancellationRequested();
            }

            _Context.Watch.Start();
        }
    }
}
