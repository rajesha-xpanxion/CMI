using CMI.Common.Logging;
using CMI.Nexus.Interface;
using CMI.Nexus.Service;
using CMI.Processor.DAL;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;

namespace CMI.Processor
{
    public abstract class InboundBaseProcessor
    {
        private readonly IProcessorProvider processorProvider;

        protected ILogger Logger { get; set; }
        protected ILookupService LookupService { get; set; }
        protected IClientService ClientService { get; set; }
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

        public abstract Common.Notification.TaskExecutionStatus Execute(DateTime? lastExecutionDateTime, IEnumerable<string> officerLogonsToFilter);

        protected abstract void LoadLookupData();

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
                    OperationName = this.GetType().Name,
                    MethodName = "FormatId",
                    Message = "Error occurred while formatting Id",
                    Exception = ex
                });
            }

            return newId;
        }

        protected DataTable GetOfficerLogonToFilterDataTable(IEnumerable<string> officerLogonsToFilter)
        {
            var dataTable = new DataTable(UserDefinedTableType.Varchar50Tbl)
            {
                Locale = CultureInfo.InvariantCulture
            };

            dataTable.Columns.Add(TableColumnName.Item, typeof(string));

            //check for null & check if any record to process
            if (officerLogonsToFilter != null && officerLogonsToFilter.Any())
            {
                foreach (var officerLogon in officerLogonsToFilter)
                {
                    dataTable.Rows.Add(officerLogon);
                }
            }

            return dataTable;
        }
    }
}
