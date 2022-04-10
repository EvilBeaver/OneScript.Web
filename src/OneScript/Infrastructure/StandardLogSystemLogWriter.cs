
using Microsoft.Extensions.Logging;
using ScriptEngine;

namespace OneScript.WebHost.Infrastructure
{
    class StandardLogSystemLogWriter : ISystemLogWriter
    {
        private readonly ILogger _logger;

        public StandardLogSystemLogWriter(ILogger logger)
        {
            _logger = logger;
        }

        public void Write(string text)
        {
            _logger.LogInformation(text);
        }
    }
}