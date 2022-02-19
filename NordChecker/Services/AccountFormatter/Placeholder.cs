using System;
using System.Collections.Generic;
using NordChecker.Models;

namespace NordChecker.Services.AccountFormatter
{
    public class Placeholder
    {
        public string Key;
        public List<string> Aliases;
        public Func<Account, string> Binding;

        public Placeholder()
        {
            Aliases = new List<string>();
        }

        public Placeholder(string key, List<string> aliases, Func<Account, string> binding)
        {
            Aliases = aliases;
            Binding = binding;
        }
    }

    public class PlaceholderListBuilder
    {
        private List<Placeholder> Placeholders = new();

        public AliasingStagePlaceholderBuilder AddPlaceholder(string key)
        {
            Placeholder placeholder = new();
            Placeholders.Add(placeholder);
            return new AliasingStagePlaceholderBuilder(this, placeholder);
        }

        public List<Placeholder> Build() => Placeholders;
    }

    public class AliasingStagePlaceholderBuilder
    {
        private Placeholder Placeholder;
        private readonly PlaceholderListBuilder _PlaceholderListBuilder;

        public AliasingStagePlaceholderBuilder(
            PlaceholderListBuilder placeholderListBuilder,
            Placeholder placeholder)
        {
            _PlaceholderListBuilder = placeholderListBuilder;
            Placeholder = placeholder;
        }

        public BindingStagePlaceholderBuilder KnownAs(params string[] aliases)
        {
            Placeholder.Aliases.AddRange(aliases);
            return new BindingStagePlaceholderBuilder(_PlaceholderListBuilder, Placeholder);
        }
    }

    public class BindingStagePlaceholderBuilder
    {
        private Placeholder Placeholder;
        private readonly PlaceholderListBuilder _PlaceholderListBuilder;

        public BindingStagePlaceholderBuilder(
            PlaceholderListBuilder placeholderListBuilder,
            Placeholder placeholder)
        {
            _PlaceholderListBuilder = placeholderListBuilder;
            Placeholder = placeholder;
        }

        public PlaceholderListBuilder BoundTo(Func<Account, string> binding)
        {
            Placeholder.Binding = binding;
            return _PlaceholderListBuilder;
        }
    }
}
