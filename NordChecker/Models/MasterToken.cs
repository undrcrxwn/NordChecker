using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NordChecker.Models
{
    public class MasterToken
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

        public async Task WaitIfPauseRequestedAsync()
        {
            while (IsPauseRequested)
            {
                await Task.Delay(50);
                ThrowIfCancellationRequested();
            }
        }

        public void ThrowOrWaitIfRequested()
        {
            ThrowIfCancellationRequested();
            WaitIfPauseRequested();
            MasterTokenSource x = new();
        }
    }

    public class MasterTokenSource
    {
        private readonly List<MasterToken> _Tokens = new();
        public IEnumerable<MasterToken> Tokens => _Tokens;

        public MasterToken MakeToken()
        {
            var token = new MasterToken();
            _Tokens.Add(token);
            return token;
        }

        public void Cancel() =>
            _Tokens.ToList().ForEach(x => x.Cancel());

        public void Pause() =>
            _Tokens.ToList().ForEach(x => x.Pause());

        public void Continue() =>
            _Tokens.ToList().ForEach(x => x.Continue());
    }
}
