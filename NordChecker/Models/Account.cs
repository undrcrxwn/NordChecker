using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace NordChecker.Models
{
    internal enum AccountState
    {
        Unchecked,
        Invalid,
        Free,
        Premium
    }

    internal class Account : INotifyPropertyChanged
    {
        public string Email { get; }
        public string Password { get; }
        public string Token { get; set; }
        public string RenewToken { get; set; }
        public int UserId { get; set; }
        public DateTime ExpiresAt { get; set; }

        private AccountState _State;
        public AccountState State
        {
            get => _State;
            set
            {
                _State = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Account(string email, string password, AccountState state = AccountState.Unchecked)
        {
            Email = email;
            Password = password;
            State = state;
        }
    }
}
