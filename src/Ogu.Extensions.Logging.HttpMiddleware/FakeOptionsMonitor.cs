using Microsoft.Extensions.Options;
using System;

namespace Ogu.Extensions.Logging.HttpMiddleware
{
    internal class FakeOptionsMonitor<TOptions> : IOptionsMonitor<TOptions>
    {
        private FakeOptionsMonitor(TOptions options)
        {
            CurrentValue = options;
        }

        public TOptions Get(string name)
        {
            return CurrentValue;
        }

        public IDisposable OnChange(Action<TOptions, string> listener)
        {
            return null;
        }

        public TOptions CurrentValue { get; }

        public static FakeOptionsMonitor<TOptions> Create(TOptions options) => new FakeOptionsMonitor<TOptions>(options);
    }
}