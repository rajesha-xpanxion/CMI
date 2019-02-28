using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace CMI.Processor
{
    public class InboundProcessor
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IConfiguration configuration;

        public InboundProcessor(
            IServiceProvider serviceProvider,
            IConfiguration configuration
        )
        {
            this.serviceProvider = serviceProvider;
            this.configuration = configuration;
        }

        public void Execute()
        {
            ((InboundClientProfileProcessor)serviceProvider.GetService(typeof(InboundClientProfileProcessor))).Execute();
        }
    }
}
