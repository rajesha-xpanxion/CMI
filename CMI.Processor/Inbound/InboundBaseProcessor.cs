using CMI.Common.Logging;
using CMI.Nexus.Interface;
using CMI.Nexus.Service;
using CMI.Processor.DAL;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace CMI.Processor
{
    public abstract class InboundBaseProcessor
    {
        private readonly IProcessorProvider processorProvider;
        private DateTime? _LastExecutionDateTime;
        private bool isLastExecutionDateTimeRetrieved { get; set; }

        protected ILogger Logger { get; set; }
        protected ILookupService LookupService { get; set; }
        protected IClientService ClientService { get; set; }
        protected DateTime? LastExecutionDateTime
        {
            get
            {
                if(isLastExecutionDateTimeRetrieved)
                {
                    return _LastExecutionDateTime;
                }
                else
                {
                    return RetrieveLastExecutionDateTime();
                }
            }
        }
        
        protected ProcessorConfig ProcessorConfig { get; set; }

        public InboundBaseProcessor(
            IServiceProvider serviceProvider,
            IConfiguration configuration
        )
        {
            Logger = (ILogger)serviceProvider.GetService(typeof(ILogger));
            processorProvider = (ProcessorProvider)serviceProvider.GetService(typeof(IProcessorProvider));
            LookupService = (LookupService)serviceProvider.GetService(typeof(ILookupService));
            ClientService = (ClientService)serviceProvider.GetService(typeof(IClientService));

            ProcessorConfig = configuration.GetSection(ConfigKeys.ProcessorConfig).Get<ProcessorConfig>();
        }

        public abstract Common.Notification.TaskExecutionStatus Execute();

        protected string FormatId(string oldId)
        {
            string newId = string.Empty;
            try
            {
                if (oldId.Length >= Nexus.Service.Constants.ExpectedMinLenghOfId)
                {
                    newId = oldId;
                }
                else
                {
                    string[] zeros = Enumerable.Repeat("0", (Nexus.Service.Constants.ExpectedMinLenghOfId - oldId.Length)).ToArray();

                    newId = string.Format("{0}{1}", string.Join("", zeros), oldId);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(new LogRequest
                {
                    OperationName = "Processor",
                    MethodName = "FormatId",
                    Message = "Error occurred while formatting Id",
                    Exception = ex
                });
            }

            return newId;
        }

        private DateTime? RetrieveLastExecutionDateTime()
        {
            try
            {
                _LastExecutionDateTime = processorProvider.GetLastExecutionDateTime();

                Logger.LogInfo(new LogRequest
                {
                    OperationName = "Processor",
                    MethodName = "RetrieveLastExecutionDateTime",
                    Message = "Successfully retrieved Last Execution Date Time",
                    CustomParams = JsonConvert.SerializeObject(LastExecutionDateTime)
                });
                isLastExecutionDateTimeRetrieved = true;
            }
            catch (Exception ex)
            {
                Logger.LogError(new LogRequest
                {
                    OperationName = "Processor",
                    MethodName = "RetrieveLastExecutionDateTime",
                    Message = "Error occurred while retriving processor last execution status.",
                    Exception = ex
                });

                isLastExecutionDateTimeRetrieved = false;
            }

            return _LastExecutionDateTime;
        }
    }
}
