using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using NordChecker.Infrastructure;
using NordChecker.Models;
using NordChecker.Models.Settings;
using NordChecker.Services.Threading;
using NordChecker.Shared;

namespace NordChecker.Services.Checker
{
    internal class Checker : IChecker
    {
        private readonly Wrapped<AppSettings> _AppSettingsWrapped;

        public Checker(Wrapped<AppSettings> appSettingsWrapped) => _AppSettingsWrapped = appSettingsWrapped;

        public async void ProcessAccount(Account account)
        {
            Action<MasterToken> accountProcessingCancellationHandler = x => account.State = AccountState.Invalid;
            account.MasterToken.Canceled += accountProcessingCancellationHandler;

            var context = new TimeoutBreakpointContext(account.MasterToken, Stopwatch.StartNew(), _AppSettingsWrapped.Instance.Timeout);
            IBreakpointHandler breakpointHandler = new TimeoutBreakpointHandler(context);
            
            breakpointHandler.HandleBreakpointIfNeeded();

            var content = new FormUrlEncodedContent(new Dictionary<string, string>{
                { "username", account.Email },
                { "password", account.Password }
            });

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/65.0.3325.181 Safari/537.36");
            
            var responseMessage = await client.PostAsync("https://api.nordvpn.com/v1/users/tokens", content);
            string response = await responseMessage.Content.ReadAsStringAsync();

            breakpointHandler.HandleBreakpointIfNeeded();

            try
            {
                JToken root = JArray.Parse(response).First;
                account.Token = root.Value<string>("token");
                account.RenewToken = root.Value<string>("renew_token");
                account.UserId = root.Value<int>("user_id");
                account.ExpiresAt = root.Value<DateTime>("expires_at");
            }
            catch
            {
                account.State = AccountState.Invalid;
                return;
            }

            breakpointHandler.HandleBreakpointIfNeeded();

            string authString = ("token:" + account.Token).ToBase64();
            client.DefaultRequestHeaders.Add("Authorization", $"Basic {authString}");
            responseMessage = await client.GetAsync("https://api.nordvpn.com/v1/users/services");
            response = await responseMessage.Content.ReadAsStringAsync();

            breakpointHandler.HandleBreakpointIfNeeded();

            try
            {
                JToken root = JArray.Parse(response).First;
                account.Token = root.Value<string>("token");
                account.RenewToken = root.Value<string>("renew_token");
                account.UserId = root.Value<int>("user_id");
                account.ExpiresAt = root.Value<DateTime>("expires_at");
            }
            catch
            {
                account.State = AccountState.Invalid;
                return;
            }

            account.State = AccountState.Premium;
            account.MasterToken.Canceled -= accountProcessingCancellationHandler;
        }

        void IChecker.Check(Account account)
        {
            throw new NotImplementedException();
        }

        void IChecker.HandleFailure(Account account, Exception exception)
        {
            throw new NotImplementedException();
        }
    }
}
