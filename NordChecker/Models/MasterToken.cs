using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NordChecker.Models
{
    public interface ICancelable
    {
        public abstract void Cancel();
    }

    public interface IPausable
    {
        public abstract void Pause();
        public abstract void Continue();
    }

    public class MasterToken : ICancelable, IPausable
    {
        public bool IsCancellationRequested { get; private set; }
        public bool IsPauseRequested { get; private set; }

        public event Action<MasterToken> Canceled;
        public event Action<MasterToken> Paused;
        public event Action<MasterToken> Continued;

        public void Cancel()
        {
            IsCancellationRequested = true;
            Canceled?.Invoke(this);
        }

        public void Pause()
        {
            IsPauseRequested = true;
            Paused?.Invoke(this);
        }

        public void Continue()
        {
            IsPauseRequested = false;
            Continued?.Invoke(this);
        }

        public void ThrowIfCancellationRequested()
        {
            if (IsCancellationRequested)
                throw new OperationCanceledException();
        }

        public void WaitIfPauseRequested()
        {
            while (IsPauseRequested)
            {
                Thread.Sleep(50);
                ThrowIfCancellationRequested();
            }
        }

        public void ThrowOrWaitIfRequested()
        {
            ThrowIfCancellationRequested();
            WaitIfPauseRequested();
        }
    }

    public class MasterTokenSource
    {
        public List<MasterToken> Tokens = new List<MasterToken>();

        public MasterToken MakeToken()
        {
            var token = new MasterToken();
            Tokens.Add(token);
            return token;
        }

        public void Cancel() => Tokens.ToList().ForEach(x => x.Cancel());
        public void Pause() => Tokens.ToList().ForEach(x => x.Pause());
        public void Continue() => Tokens.ToList().ForEach(x => x.Continue());
    }
}
