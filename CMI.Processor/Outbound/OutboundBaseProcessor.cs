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
                ClientProfilePersonalDetailsActivityResponse details = (ClientProfilePersonalDetailsActivityResponse)(object)activityDetails;

                return new Offender
                {
                    Pin = clientIntegrationId,
                    UpdatedBy = updatedBy,
                    FirstName = details.FirstName,
                    MiddleName = details.MiddleName,
                    LastName = details.LastName,
                    Race = details.Ethinicity,
                    DateOfBirth = details.DateOfBirth,
                    Gender = details.Gender
                };
            }
            else if (typeof(T) == typeof(ClientProfileEmailDetailsActivityResponse))
            {
                ClientProfileEmailDetailsActivityResponse details = (ClientProfileEmailDetailsActivityResponse)(object)activityDetails;

                return new OffenderEmail
                {
                    Pin = clientIntegrationId,
                    UpdatedBy = updatedBy,
                    EmailAddress = details.Email
                };
            }
            else if (typeof(T) == typeof(ClientProfileAddressDetailsActivityResponse))
            {
                ClientProfileAddressDetailsActivityResponse details = (ClientProfileAddressDetailsActivityResponse)(object)activityDetails;

                return new OffenderAddress
                {
                    Pin = clientIntegrationId,
                    UpdatedBy = updatedBy,
                    Line1 = details.Address,
                    //Line2 = details.Address,
                    AddressType =
                        details.AddressType.Equals("Home", StringComparison.InvariantCultureIgnoreCase)
                        ? "Residential"
                        : "Unknown"
                };
            }
            else if (typeof(T) == typeof(ClientProfileContactDetailsActivityResponse))
            {
                ClientProfileContactDetailsActivityResponse details = (ClientProfileContactDetailsActivityResponse)(object)activityDetails;

                return new OffenderPhone
                {
                    Pin = clientIntegrationId,
                    UpdatedBy = updatedBy,
                    Phone = details.Contact.Replace(" ", string.Empty),
                    PhoneNumberType = 
                        details.ContactType.Equals("HomePhone", StringComparison.InvariantCultureIgnoreCase)
                        ? "Residential"
                        : 
                        (
                            details.ContactType.Equals("MobilePhone", StringComparison.InvariantCultureIgnoreCase)
                            ? "Mobile"
                            : "Message"
                        )
                };
            }
            else if (typeof(T) == typeof(ClientProfileVehicleDetailsActivityResponse))
            {
                ClientProfileVehicleDetailsActivityResponse details = (ClientProfileVehicleDetailsActivityResponse)(object)activityDetails;

                return new OffenderVehicle
                {
                    Pin = clientIntegrationId,
                    UpdatedBy = updatedBy,
                    VehicleYear = details.Year,
                    Make = details.Make,
                    BodyStyle = details.Model,
                    Color = details.Color,
                    LicensePlate = details.LicensePlate
                };
            }
            else if (typeof(T) == typeof(ClientProfileEmploymentDetailsActivityResponse))
            {
                ClientProfileEmploymentDetailsActivityResponse details = (ClientProfileEmploymentDetailsActivityResponse)(object)activityDetails;

                return new OffenderEmployment
                {
                    Pin = clientIntegrationId,
                    UpdatedBy = updatedBy,
                    OrganizationName = details.Employer
                };
            }
            else if (typeof(T) == typeof(ClientProfileDetailsActivityResponse))
            {
                ClientProfileDetailsActivityResponse details = (ClientProfileDetailsActivityResponse)(object)activityDetails;

                return new OffenderDetails
                {
                    Pin = clientIntegrationId,
                    UpdatedBy = updatedBy,
                    FirstName = details.FirstName,
                    MiddleName = details.MiddleName,
                    LastName = details.LastName,
                    Race = details.Ethinicity,
                    ClientType = details.ClientType,
                    EmailAddress = details.Email,
                    Line1 = details.Address,
                    //Line2 = details.Address,
                    AddressType = details.AddressType,
                    Phone = details.Contact,
                    PhoneNumberType = details.ContactType
                };
            }

            return null;
        }
    }
}
