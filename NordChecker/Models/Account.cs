using NordChecker.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace NordChecker.Models
{
    internal enum AccountState
    {
        Premium,
        Free,
        Invalid,
        Reserved,
        Unchecked
    }

    internal class Account
    {
        public AccountState State { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }
        public string RenewToken { get; set; }
        public int UserId { get; set; }
        public DateTime ExpiresAt { get; set; }

        public (string, string) Credentials
        {
            get => (Email, Password);
            set => (Email, Password) = value;
        }

        public Account(string email, string password, AccountState state = AccountState.Unchecked)
        {
            Email = email;
            Password = password;
            State = state;
        }
    }
}
