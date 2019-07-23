using CMI.Automon.Model;
using CMI.Automon.Service;
using CMI.Common.Logging;
using CMI.Common.Notification;
using CMI.MessageRetriever.Model;
using CMI.Nexus.Service;
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

        protected Offender ConvertResponseToObject<T>(string clientIntegrationId, string activityIdentifier, string automonIdentifier, T activityDetails, string updatedBy)
        {
            //try to retrieve Automon identifier
            int id = 0;
            if (!string.IsNullOrEmpty(activityIdentifier) && int.TryParse(activityIdentifier.Replace(string.Format("{0}-", clientIntegrationId), string.Empty), out id))
            {
            }
            else if (!string.IsNullOrEmpty(automonIdentifier) && int.TryParse(automonIdentifier, out id))
            {
            }

            //Note
            if (typeof(T) == typeof(ClientProfileNoteActivityDetailsResponse))
            {
                ClientProfileNoteActivityDetailsResponse noteActivityDetailsResponse = (ClientProfileNoteActivityDetailsResponse)(object)activityDetails;

                return new OffenderNote()
                {
                    Pin = clientIntegrationId,
                    Id = id,
                    UpdatedBy = updatedBy,
                    Text = noteActivityDetailsResponse.NoteText,
                    AuthorEmail = noteActivityDetailsResponse.NoteAuthor,
                    Date = noteActivityDetailsResponse.CreatedDate
                };
            }
            //Office Visit
            else if (typeof(T) == typeof(ClientProfileOfficeVisitDetailsActivityResponse))
            {
                ClientProfileOfficeVisitDetailsActivityResponse details = (ClientProfileOfficeVisitDetailsActivityResponse)(object)activityDetails;

                return new OffenderOfficeVisit()
                {
                    Pin = clientIntegrationId,
                    Id = id,
                    UpdatedBy = updatedBy,
                    StartDate = details.DateTime,
                    Comment = details.Notes,
                    EndDate = details.DateTime,
                    /* Status: Pending = 0, Missed = 16, Cancelled = 10, Complete = 2 */
                    Status =
                        details.Status != null
                        ?
                            details.Status.Equals(Status.Attended, StringComparison.InvariantCultureIgnoreCase)
                            ? (int)EventStatus.Complete
                            : details.Status.Equals(Status.Missed, StringComparison.InvariantCultureIgnoreCase)
                                ? (int)EventStatus.Missed
                                : details.Status.Equals(Status.Excused, StringComparison.InvariantCultureIgnoreCase)
                                    ? (int)EventStatus.Cancelled
                                    : (int)EventStatus.Pending
                        :
                            (int)EventStatus.Pending,
                    IsOffenderPresent =
                        details.Status != null
                        ?
                            details.Status.Equals(Status.Attended, StringComparison.InvariantCultureIgnoreCase)
                        :
                            false

                };
            }
            //Drug Test Appointment
            else if (typeof(T) == typeof(ClientProfileDrugTestAppointmentDetailsActivityResponse))
            {
                ClientProfileDrugTestAppointmentDetailsActivityResponse details = (ClientProfileDrugTestAppointmentDetailsActivityResponse)(object)activityDetails;

                return new OffenderDrugTestAppointment()
                {
                    Pin = clientIntegrationId,
                    Id = id,
                    UpdatedBy = updatedBy,
                    StartDate = details.AppointmentDateTime,
                    EndDate = details.AppointmentDateTime,
                    /* Status: Pending = 0, Missed = 16, Cancelled = 10, Complete = 2 */
                    Status =
                        details.AppointmentStatus != null
                        ?
                            details.AppointmentStatus.Equals(Status.Completed, StringComparison.InvariantCultureIgnoreCase)
                            || details.AppointmentStatus.Equals(Status.Tampered, StringComparison.InvariantCultureIgnoreCase)
                            ? (int)EventStatus.Complete
                            : details.AppointmentStatus.Equals(Status.Missed, StringComparison.InvariantCultureIgnoreCase)
                                ? (int)EventStatus.Missed
                                : details.AppointmentStatus.Equals(Status.Excused, StringComparison.InvariantCultureIgnoreCase)
                                    ? (int)EventStatus.Cancelled
                                    : (int)EventStatus.Pending
                        :
                            (int)EventStatus.Pending,
                    Location = details.Location
                };
            }
            //Drug Test Result
            else if (typeof(T) == typeof(ClientProfileDrugTestResultDetailsActivityResponse))
            {
                ClientProfileDrugTestResultDetailsActivityResponse details = (ClientProfileDrugTestResultDetailsActivityResponse)(object)activityDetails;

                return new OffenderDrugTestResult()
                {
                    Pin = clientIntegrationId,
                    Id = id,
                    UpdatedBy = updatedBy,
                    StartDate = details.TestDateTime,
                    EndDate = details.TestDateTime,
                    /* Status: Pending = 0, Missed = 16, Cancelled = 10, Complete = 2 */
                    Status =
                        details.ResultStatus != null
                        ?
                            details.ResultStatus.Equals(Status.Passed, StringComparison.InvariantCultureIgnoreCase)
                            || details.ResultStatus.Equals(Status.Failed, StringComparison.InvariantCultureIgnoreCase)
                            || details.ResultStatus.Equals(Status.Tampered, StringComparison.InvariantCultureIgnoreCase)
                            ?
                                (int)EventStatus.Complete
                            :
                                (int)EventStatus.Pending
                        :
                            (int)EventStatus.Pending,
                    DeviceType = details.DrugTestType,
                    TestResult = details.ResultStatus,
                    Validities =
                        details.Dilute != null
                        ?
                            details.Dilute.Equals("Yes", StringComparison.InvariantCultureIgnoreCase)
                            ? "Diluted"
                            : string.Empty
                        :
                            string.Empty
                };
            }
            //Field Visit
            else if (typeof(T) == typeof(ClientProfileFieldVisitDetailsActivityResponse))
            {
                ClientProfileFieldVisitDetailsActivityResponse details = (ClientProfileFieldVisitDetailsActivityResponse)(object)activityDetails;

                bool isSearchConducted = false;
                string searchLocations = string.Empty;
                string searchResults = string.Empty;

                if (details.VisitedLocations != null && details.VisitedLocations.Any())
                {
                    isSearchConducted = details.VisitedLocations.Any(v => v.SearchedAreas != null && v.SearchedAreas.Any(sa => !string.IsNullOrEmpty(sa)));
                    searchLocations = isSearchConducted ? string.Join(", ", details.VisitedLocations.Select(v => string.Join(", ", v.SearchedAreas))) : string.Empty;
                }

                if (details.FoundContraband != null && details.FoundContraband.Any())
                {
                    searchResults = string.Join(", ", details.FoundContraband.Select(fc => fc.Type));
                }

                List<VisitedLocationDetails> visitedLocations = new List<VisitedLocationDetails>(details.VisitedLocations);

                return new OffenderFieldVisit()
                {
                    Pin = clientIntegrationId,
                    Id = id,
                    UpdatedBy = updatedBy,
                    StartDate = details.DateTime,
                    Comment = details.Note,
                    EndDate = details.DateTime,
                    /* Status: Pending = 0, Missed = 16, Cancelled = 10, Complete = 2 */
                    Status =
                        details.Status != null
                        ?
                            details.Status.Equals(Status.Attended, StringComparison.InvariantCultureIgnoreCase)
                            || details.Status.Equals(Status.Attempted, StringComparison.InvariantCultureIgnoreCase)
                            ?
                                (int)EventStatus.Complete
                            :
                                (int)EventStatus.Pending
                        :
                            (int)EventStatus.Pending,
                    IsOffenderPresent =
                        details.Status != null
                        ?
                            details.Status.Equals(Status.Attended, StringComparison.InvariantCultureIgnoreCase)
                        :
                            false,
                    IsSearchConducted = isSearchConducted,
                    SearchLocations = searchLocations,
                    SearchResults = searchResults
                };
            }
            //Client Profile - Personal Details
            else if (typeof(T) == typeof(ClientProfilePersonalDetailsActivityResponse))
            {
                ClientProfilePersonalDetailsActivityResponse details = (ClientProfilePersonalDetailsActivityResponse)(object)activityDetails;

                return new Offender
                {
                    Pin = string.IsNullOrEmpty(automonIdentifier) ? clientIntegrationId : automonIdentifier,
                    UpdatedBy = updatedBy,
                    FirstName = details.FirstName,
                    MiddleName = details.MiddleName,
                    LastName = details.LastName,
                    Race = details.Ethinicity,
                    DateOfBirth = details.DateOfBirth,
                    Gender = details.Gender
                };
            }
            //Client Profile - Email Details
            else if (typeof(T) == typeof(ClientProfileEmailDetailsActivityResponse))
            {
                ClientProfileEmailDetailsActivityResponse details = (ClientProfileEmailDetailsActivityResponse)(object)activityDetails;

                return new OffenderEmail
                {
                    Pin = clientIntegrationId,
                    Id = id,
                    UpdatedBy = updatedBy,
                    EmailAddress = details.Email
                };
            }
            //Client Profile - Address Details
            else if (typeof(T) == typeof(ClientProfileAddressDetailsActivityResponse))
            {
                ClientProfileAddressDetailsActivityResponse details = (ClientProfileAddressDetailsActivityResponse)(object)activityDetails;

                return new OffenderAddress
                {
                    Pin = clientIntegrationId,
                    Id = id,
                    UpdatedBy = updatedBy,
                    Line1 = details.Address,
                    AddressType =
                        details.AddressType != null
                        ?
                            details.AddressType.Equals("Home", StringComparison.InvariantCultureIgnoreCase)
                            ? "Residential"
                            : "Mailing"
                        :
                            "Unknown"
                };
            }
            //Client Profile - Contact Details
            else if (typeof(T) == typeof(ClientProfileContactDetailsActivityResponse))
            {
                ClientProfileContactDetailsActivityResponse details = (ClientProfileContactDetailsActivityResponse)(object)activityDetails;

                if (details.ContactType.Equals("E-mail", StringComparison.InvariantCultureIgnoreCase))
                {
                    return new OffenderEmail
                    {
                        Pin = clientIntegrationId,
                        Id = id,
                        UpdatedBy = updatedBy,
                        EmailAddress = details.Contact
                    };
                }
                else
                {
                    string autmonPhoneNumberType = string.Empty;

                    switch (details.ContactType)
                    {
                        case "Cell Phone":
                        case "Emergency Phone":
                            autmonPhoneNumberType = "Mobile";
                            break;
                        case "Home Phone":
                            autmonPhoneNumberType = "Residential";
                            break;
                        case "Fax":
                            autmonPhoneNumberType = "Fax";
                            break;
                        case "Business Phone":
                        case "Work Phone":
                            autmonPhoneNumberType = "Office";
                            break;
                        default:
                            autmonPhoneNumberType = "Message";
                            break;
                    }

                    return new OffenderPhone
                    {
                        Pin = clientIntegrationId,
                        Id = id,
                        UpdatedBy = updatedBy,
                        Phone =
                            details.Contact != null
                            ?
                                details.Contact.Replace(" ", string.Empty)
                            :
                                string.Empty,
                        PhoneNumberType = autmonPhoneNumberType
                    };
                }
            }
            //Client Profile - Vehicle Details
            else if (typeof(T) == typeof(ClientProfileVehicleDetailsActivityResponse))
            {
                ClientProfileVehicleDetailsActivityResponse details = (ClientProfileVehicleDetailsActivityResponse)(object)activityDetails;

                return new OffenderVehicle
                {
                    Pin = clientIntegrationId,
                    Id = id,
                    UpdatedBy = updatedBy,
                    VehicleYear = details.Year,
                    Make = details.Make,
                    BodyStyle = details.Model,
                    Color = details.Color,
                    LicensePlate = details.LicensePlate
                };
            }
            //Client Profile - Employment Details
            else if (typeof(T) == typeof(ClientProfileEmploymentDetailsActivityResponse))
            {
                ClientProfileEmploymentDetailsActivityResponse details = (ClientProfileEmploymentDetailsActivityResponse)(object)activityDetails;

                string payFrequency = string.Empty;
                if (!string.IsNullOrEmpty(details.WageUnit))
                {
                    payFrequency =
                        details.WageUnit.Equals("per Hour", StringComparison.InvariantCultureIgnoreCase)
                        ? "Hourly"
                        : details.WageUnit.Equals("per Week", StringComparison.InvariantCultureIgnoreCase)
                            ? "Weekly"
                            : details.WageUnit.Equals("per Month", StringComparison.InvariantCultureIgnoreCase)
                                ? "Monthly"
                                : "Annually";
                }
                else
                {
                    payFrequency = null;
                }

                return new OffenderEmployment
                {
                    Pin = clientIntegrationId,
                    Id = id,
                    UpdatedBy = updatedBy,
                    OrganizationName = details.Employer,
                    OrganizationAddress = details.WorkAddress,
                    OrganizationPhone = details.WorkPhone,
                    PayFrequency = payFrequency,
                    PayRate = details.Wage,
                    WorkType = details.WorkEnvironment,
                    JobTitle = details.Occupation
                };
            }
            //New Client Profile
            else if (typeof(T) == typeof(NewClientProfileActivityResponse))
            {
                NewClientProfileActivityResponse details = (NewClientProfileActivityResponse)(object)activityDetails;

                return new OffenderDetails
                {
                    Pin = null,
                    UpdatedBy = updatedBy,
                    FirstName = details.FirstName,
                    MiddleName = details.MiddleName,
                    LastName = details.LastName,
                    ClientType =
                        details.ClientType != null
                        ?
                            details.ClientType.Equals("Adult.Interstate", StringComparison.InvariantCultureIgnoreCase)
                                ?
                                    "Adult"
                                :
                                    "Juvenile"
                        :
                            "Unknown",
                    Race = details.Ethinicity,
                    DateOfBirth = details.DateOfBirth,
                    Gender = details.Gender,
                    EmailAddress = details.Email,
                    Line1 = details.Address,
                    //Line2 = details.Address,
                    AddressType =
                        details.AddressType != null
                        ?
                            details.AddressType.Equals("Home", StringComparison.InvariantCultureIgnoreCase)
                            ? "Residential"
                            : "Mailing"
                        :
                            null,
                    Phone = details.Contact != null ? details.Contact.Replace(" ", string.Empty) : null,
                    PhoneNumberType =
                        details.ContactType != null
                        ?
                            details.ContactType.Equals("HomePhone", StringComparison.InvariantCultureIgnoreCase)
                            ? "Residential"
                            :
                            (
                                details.ContactType.Equals("MobilePhone", StringComparison.InvariantCultureIgnoreCase)
                                ? "Mobile"
                                : "Message"
                            )
                        :
                            null

                };
            }
            //Treatment Appointment
            else if (typeof(T) == typeof(ClientProfileTreatmentAppointmentDetailsActivityResponse))
            {
                ClientProfileTreatmentAppointmentDetailsActivityResponse details = (ClientProfileTreatmentAppointmentDetailsActivityResponse)(object)activityDetails;

                return new OffenderTreatmentAppointment()
                {
                    Pin = clientIntegrationId,
                    Id = id,
                    UpdatedBy = updatedBy,
                    StartDate = details.AppointmentDateTime,
                    EndDate = details.AppointmentDateTime,
                    /* Status: Pending = 0, Missed = 16, Cancelled = 10, Complete = 2 */
                    Status =
                        details.Status != null
                        ?
                            details.Status.Equals(Status.Attended, StringComparison.InvariantCultureIgnoreCase)
                            ? (int)EventStatus.Complete
                            : details.Status.Equals(Status.Missed, StringComparison.InvariantCultureIgnoreCase)
                                ? (int)EventStatus.Missed
                                : details.Status.Equals(Status.Excused, StringComparison.InvariantCultureIgnoreCase)
                                    ? (int)EventStatus.Cancelled
                                    : (int)EventStatus.Pending
                        :
                            (int)EventStatus.Pending,
                    Comment = details.Notes
                };
            }

            return null;
        }
    }
}
