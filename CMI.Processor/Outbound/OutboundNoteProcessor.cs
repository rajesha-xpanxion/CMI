using CMI.Nexus.Interface;
using Microsoft.Extensions.Configuration;
using System;

namespace CMI.Processor
{
    public class OutboundNoteProcessor: OutboundBaseProcessor
    {
        private readonly INoteService noteService;

        public OutboundNoteProcessor(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            INoteService noteService
        )
            : base(serviceProvider, configuration)
        {
            this.noteService = noteService;
        }

        public override void Execute()
        {

        }
    }
}
