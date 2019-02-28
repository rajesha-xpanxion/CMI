using CMI.Automon.Interface;
using Microsoft.Extensions.Configuration;
using System;

namespace CMI.Processor
{
    public class OutboundOfficeVisitProcessor : OutboundBaseProcessor
    {
        private readonly IOffenderOfficeVisitService offenderOfficeVisitService;

        public OutboundOfficeVisitProcessor(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            IOffenderOfficeVisitService offenderOfficeVisitService
        )
            : base(serviceProvider, configuration)
        {
            this.offenderOfficeVisitService = offenderOfficeVisitService;
        }

        public override void Execute()
        {

        }
    }
}
