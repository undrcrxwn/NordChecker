using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;
using Serilog;

namespace NordChecker.Services.Threading
{
    public partial class ThreadDistributor<TPayload>
    {
        public class Builder
        {
            [Required] public int ThreadCount { get; private set; }
            public MasterToken Token { get; private set; }
            [Required] public ObservableCollection<TPayload> Payloads { get; private set; }
            [Required] public Func<TPayload, bool> Filter { get; private set; }
            [Required] public Action<TPayload> Handler { get; private set; }
            
            public Builder SetThreadCount(int threadsCount)
            {
                ThreadCount = threadsCount;
                return this;
            }

            public Builder SetPayloads(ObservableCollection<TPayload> payloads)
            {
                Payloads = payloads;
                return this;
            }

            public Builder SetFilter(Func<TPayload, bool> filter)
            {
                Filter = filter;
                return this;
            }

            public Builder SetHandler(Action<TPayload> handler)
            {
                Handler = handler;
                return this;
            }

            public Builder SetToken(MasterToken token)
            {
                Token = token;
                return this;
            }

            public ThreadDistributor<TPayload> Build()
            {
                ValidateMembers();

                ThreadDistributor<TPayload> distributor = new(this);
                return distributor;
            }

            private void ValidateMembers()
            {
                var context = new ValidationContext(this);
                Validator.ValidateObject(this, context);
            }
        }
    }
}
