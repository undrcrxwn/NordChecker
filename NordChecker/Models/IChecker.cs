using Serilog;
using System;

namespace NordChecker.Models
{
    public interface IChecker
    {
        public sealed void ProcessAccount(Account account)
        {
            try
            {
                Log.Verbose("Account processing for {0} has started", account);
                Check(account);
            }
            catch (OperationCanceledException e)
            {
                Log.Debug("Account processing operation for {0} has been cancelled", account);
                HandleFailure(account, e);
            }
            catch (Exception e)
            {
                Log.Error(e, "Account processing operation for {0} has thrown an unexpected exception", account);
                HandleFailure(account, e);
                throw;
            }
        }

        protected void Check(Account account);
        protected void HandleFailure(Account account, Exception exception);
    }
}
