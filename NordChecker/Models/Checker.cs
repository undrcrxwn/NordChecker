using Newtonsoft.Json.Linq;
using NordChecker.Shared;
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
    internal class Checker : IChecker
    {
        private AppSettings appSettings;

        public Checker(AppSettings appSettings) => this.appSettings = appSettings;

        public async void ProcessAccount(Account account)
        {
            Action<MasterToken> OnAccountProcessingCanceled = x => account.State = AccountState.Invalid;
            account.MasterToken.Canceled += OnAccountProcessingCanceled;

            var context = new TimeoutBreakpointContext(account.MasterToken, Stopwatch.StartNew(), appSettings.Timeout);
            IBreakpointHandler breakpointHandler = new TimeoutBreakpointHandler(context);

            var content = new FormUrlEncodedContent(new Dictionary<string, string>{
                { "username", account.Email },
                { "password", account.Password }
            });

            breakpointHandler.HandleBreakpointIfNeeded();

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/65.0.3325.181 Safari/537.36");
            //client.DefaultRequestHeaders.Add("Accept", "*/*");

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

            account.MasterToken.Canceled -= OnAccountProcessingCanceled;
            return;
        }
    }
}
