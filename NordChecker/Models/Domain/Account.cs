using NordChecker.Shared;
using System;
using System.ComponentModel;
using Newtonsoft.Json;
using NordChecker.Infrastructure;
using NordChecker.Threading;

namespace NordChecker.Models.Domain
{
    public enum AccountState
    {
        Unchecked,
        Reserved,
        Invalid,
        Free,
        Premium
    }

    public class Account : INotifyPropertyChangedAdvanced
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public MasterToken MasterToken;

        private AccountState _State = AccountState.Unchecked;
        public AccountState State
        {
            get => _State;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _State, value, PropertyChanged);
        }

        public string Email { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }
        public string RenewToken { get; set; }
        public int UserId { get; set; }
        public DateTime ExpiresAt { get; set; }
        public Proxy Proxy { get; set; }
        
        [JsonIgnore]
        public (string, string) Credentials => (Email, Password);

        public Account(string email, string password)
        {
            Email = email;
            Password = password;
        }
    }
}
