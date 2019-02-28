using CMI.Common.Logging;
using Microsoft.Extensions.Configuration;
using System;

namespace CMI.Processor
{
    public abstract class OutboundBaseProcessor
    {
        protected ILogger Logger { get; set; }

        public OutboundBaseProcessor(
            IServiceProvider serviceProvider,
            IConfiguration configuration
        )
        {
            Logger = (ILogger)serviceProvider.GetService(typeof(ILogger));
        }

        public abstract void Execute();
    }
}
