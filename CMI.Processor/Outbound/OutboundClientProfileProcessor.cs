using CMI.Automon.Interface;
using Microsoft.Extensions.Configuration;
using System;

namespace CMI.Processor
{
    public class OutboundClientProfileProcessor : OutboundBaseProcessor
    {
        private readonly IOffenderService offenderService;

        public OutboundClientProfileProcessor(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            IOffenderService offenderService
        )
            : base(serviceProvider, configuration)
        {
            this.offenderService = offenderService;
        }

        public override void Execute()
        {

        }
    }
}
