using NordChecker.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace NordChecker.Models
{
    public enum AccountState
    {
        Premium,
        Free,
        Invalid,
        Reserved,
        Unchecked
    }

    public class Account : INotifyPropertyChangedAdvanced
    {
        public event PropertyChangedEventHandler PropertyChanged;

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

        public (string, string) Credentials
        {
            get => (Email, Password);
            set => (Email, Password) = value;
        }

        public Account(string email, string password)
        {
            Email = email;
            Password = password;
        }
    }
}
