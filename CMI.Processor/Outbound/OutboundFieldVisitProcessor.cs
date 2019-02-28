using CMI.Automon.Interface;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace CMI.Processor
{
    public class OutboundFieldVisitProcessor: OutboundBaseProcessor
    {
        private readonly IOffenderFieldVisitService offenderFieldVisitService;

        public OutboundFieldVisitProcessor(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            IOffenderFieldVisitService offenderFieldVisitService
        )
            : base(serviceProvider, configuration)
        {
            this.offenderFieldVisitService = offenderFieldVisitService;
        }

        public override void Execute()
        {

        }
    }
}
