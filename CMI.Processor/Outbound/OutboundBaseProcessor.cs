using CMI.Automon.Model;
using CMI.Common.Logging;
using CMI.Common.Notification;
using CMI.MessageRetriever.Model;
using CMI.Processor.DAL;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CMI.Processor
{
    public abstract class OutboundBaseProcessor
    {
        protected ILogger Logger { get; set; }
        protected IProcessorProvider ProcessorProvider { get; set; }
        protected ProcessorConfig ProcessorConfig { get; set; }

        public OutboundBaseProcessor(
            IServiceProvider serviceProvider,
            IConfiguration configuration
        )
        {
            Logger = (ILogger)serviceProvider.GetService(typeof(ILogger));
            ProcessorProvider = (IProcessorProvider)serviceProvider.GetService(typeof(IProcessorProvider));
            ProcessorConfig = configuration.GetSection(ConfigKeys.ProcessorConfig).Get<ProcessorConfig>();
        }

        public abstract TaskExecutionStatus Execute(IEnumerable<OutboundMessageDetails> messages, DateTime messagesReceivedOn);

        protected T RetrieveActivityDetails<T>(string activityDetails)
        {
            return JsonConvert.DeserializeObject<T>(activityDetails);
        }

        protected Offender ConvertResponseToObject<T>(String clientIntegrationId, T activityDetails, string updatedBy)
        {
            if (typeof(T) == typeof(ClientProfileNoteActivityDetailsResponse))
            {
                ClientProfileNoteActivityDetailsResponse noteActivityDetailsResponse = (ClientProfileNoteActivityDetailsResponse)(object)activityDetails;
                return new OffenderNote()
                {
                    Pin = clientIntegrationId,
                    UpdatedBy = updatedBy,
                    Text = noteActivityDetailsResponse.NoteText,
                    AuthorEmail = noteActivityDetailsResponse.NoteAuthor,
                    Date = noteActivityDetailsResponse.CreatedDate

                };
            }
            else if (typeof(T) == typeof(ClientProfileOfficeVisitDetailsActivityResponse))
            {
                ClientProfileOfficeVisitDetailsActivityResponse details = (ClientProfileOfficeVisitDetailsActivityResponse)(object)activityDetails;
                return new OffenderOfficeVisit()
                {
                    Pin = clientIntegrationId,
                    UpdatedBy = updatedBy,
                    StartDate = details.DateTime,
                    Comment = details.Notes,
                    EndDate = details.DateTime,
                    Status = details.Status.Equals("Attended", StringComparison.InvariantCultureIgnoreCase)
                        ? 2
                        : details.Status.Equals("Missed", StringComparison.InvariantCultureIgnoreCase)
                            ? 16
                            : details.Status.Equals("Excused", StringComparison.InvariantCultureIgnoreCase)
                                ? 16
                                : 0,
                    IsOffenderPresent = details.Status.Equals("Attended", StringComparison.InvariantCultureIgnoreCase)

                };
            }
            else if (typeof(T) == typeof(ClientProfileDrugTestResultDetailsActivityResponse))
            {
                ClientProfileDrugTestResultDetailsActivityResponse details = (ClientProfileDrugTestResultDetailsActivityResponse)(object)activityDetails;
                return new OffenderDrugTestResult()
                {
                    Pin = clientIntegrationId,
                    UpdatedBy = updatedBy,

                    StartDate = details.TestDateTime,
                    EndDate = details.TestDateTime,
                    Status = details.ResultStatus.Equals("Attended", StringComparison.InvariantCultureIgnoreCase)
                        ? 2
                        : details.ResultStatus.Equals("Missed", StringComparison.InvariantCultureIgnoreCase)
                            ? 16
                            : details.ResultStatus.Equals("Excused", StringComparison.InvariantCultureIgnoreCase)
                                ? 16
                                : 0,
                    DeviceType = details.DrugTestType,
                    TestResult = details.ResultStatus,
                    Validities = details.Dilute.Equals("Yes", StringComparison.InvariantCultureIgnoreCase)
                        ? "Diluted"
                        : string.Empty
                };
            }
            else if (typeof(T) == typeof(ClientProfileFieldVisitDetailsActivityResponse))
            {
                ClientProfileFieldVisitDetailsActivityResponse details = (ClientProfileFieldVisitDetailsActivityResponse)(object)activityDetails;

                bool isSearchConducted = false;
                string searchLocations = string.Empty;

                if (details.VisitedLocations != null && details.VisitedLocations.Any())
                {
                    isSearchConducted = details.VisitedLocations.Any(v => !string.IsNullOrEmpty(v.SearchedAreas));
                    searchLocations = string.Join(", ", details.VisitedLocations.Select(v => v.SearchedAreas));
                }

                List<VisitedLocationDetails> visitedLocations = new List<VisitedLocationDetails>(details.VisitedLocations);
                return new OffenderFieldVisit()
                {
                    Pin = clientIntegrationId,
                    UpdatedBy = updatedBy,
                    StartDate = details.DateTime,
                    Comment = details.Note,
                    EndDate = details.DateTime,
                    Status = details.Status.Equals("Attended", StringComparison.InvariantCultureIgnoreCase)
                        ? 2
                        : details.Status.Equals("Missed", StringComparison.InvariantCultureIgnoreCase)
                            ? 16
                            : details.Status.Equals("Excused", StringComparison.InvariantCultureIgnoreCase)
                                ? 16
                                : 0,
                    IsOffenderPresent = details.Status.Equals("Attended", StringComparison.InvariantCultureIgnoreCase),
                    IsSearchConducted = isSearchConducted,
                    SearchLocations = searchLocations,
                    SearchResults = details.FoundContraband != null
                        ? string.Join(", ", details.FoundContraband)
                        : string.Empty
                };
            }
            else if (typeof(T) == typeof(ClientProfilePersonalDetailsActivityResponse))
            {
                return new Offender
                {
                };
            }
            else if (typeof(T) == typeof(ClientProfileEmailDetailsActivityResponse))
            {
                return new OffenderEmail
                {

                };
            }
            else if (typeof(T) == typeof(ClientProfileAddressDetailsActivityResponse))
            {
                return new OffenderAddress
                {

                };
            }
            else if (typeof(T) == typeof(ClientProfileContactDetailsActivityResponse))
            {
                return new OffenderPhone
                {

                };
            }
            else if (typeof(T) == typeof(ClientProfileVehicleDetailsActivityResponse))
            {
                return new OffenderVehicle
                {

                };
            }
            else if (typeof(T) == typeof(ClientProfileEmploymentDetailsActivityResponse))
            {
                return new OffenderEmployment
                {

                };
            }

            return null;
        }
    }
}
