using CMI.Automon.Interface;
using Microsoft.Extensions.Configuration;
using System;

namespace CMI.Processor
{
    public class OutboundDrugTestProcessor : OutboundBaseProcessor
    {
        private readonly IOffenderDrugTestService offenderDrugTestService;

        public OutboundDrugTestProcessor(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            IOffenderDrugTestService offenderDrugTestService
        )
            : base(serviceProvider, configuration)
        {
            this.offenderDrugTestService = offenderDrugTestService;
        }

        public override void Execute()
        {

        }
    }
}
