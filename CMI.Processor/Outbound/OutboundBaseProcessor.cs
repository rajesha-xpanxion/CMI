using CMI.Automon.Interface;
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
        protected IOffenderService OffenderService { get; set; }
        protected string AutomonTimeZone { get; set; }

        public OutboundBaseProcessor(
            IServiceProvider serviceProvider,
            IConfiguration configuration
        )
        {
            Logger = (ILogger)serviceProvider.GetService(typeof(ILogger));
            ProcessorProvider = (IProcessorProvider)serviceProvider.GetService(typeof(IProcessorProvider));
            ProcessorConfig = configuration.GetSection(ConfigKeys.ProcessorConfig).Get<ProcessorConfig>();
            OffenderService = (IOffenderService)serviceProvider.GetService(typeof(IOffenderService));

            AutomonTimeZone = OffenderService.GetTimeZone();
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

                //check if timezone information provided in given datetime, NO = specify it
                if (noteActivityDetailsResponse.CreatedDate.Kind != DateTimeKind.Utc)
                {
                    noteActivityDetailsResponse.CreatedDate = DateTime.SpecifyKind(noteActivityDetailsResponse.CreatedDate, DateTimeKind.Utc);
                }

                return new OffenderNote()
                {
                    Pin = clientIntegrationId,
                    Id = id,
                    UpdatedBy = updatedBy,
                    Text = noteActivityDetailsResponse.NoteText,
                    AuthorEmail = noteActivityDetailsResponse.NoteAuthor,
                    Date = 
                        !string.IsNullOrEmpty(AutomonTimeZone) 
                            ? TimeZoneInfo.ConvertTimeBySystemTimeZoneId(noteActivityDetailsResponse.CreatedDate, AutomonTimeZone)
                            : noteActivityDetailsResponse.CreatedDate.ToLocalTime()
                };
            }
            //Office Visit
            else if (typeof(T) == typeof(ClientProfileOfficeVisitDetailsActivityResponse))
            {
                ClientProfileOfficeVisitDetailsActivityResponse details = (ClientProfileOfficeVisitDetailsActivityResponse)(object)activityDetails;

                //check if timezone information provided in given datetime, NO = specify it
                if(details.DateTime.Kind != DateTimeKind.Utc)
                {
                    details.DateTime = DateTime.SpecifyKind(details.DateTime, DateTimeKind.Utc);
                }

                DateTime convertedDateTime =
                    !string.IsNullOrEmpty(AutomonTimeZone)
                            ? TimeZoneInfo.ConvertTimeBySystemTimeZoneId(details.DateTime, AutomonTimeZone)
                            : details.DateTime.ToLocalTime();

                return new OffenderOfficeVisit()
                {
                    Pin = clientIntegrationId,
                    Id = id,
                    UpdatedBy = updatedBy,
                    StartDate = convertedDateTime,
                    Comment = details.Notes,
                    EndDate = convertedDateTime,
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

                //check if timezone information provided in given datetime, NO = specify it
                if (details.AppointmentDateTime.Kind != DateTimeKind.Utc)
                {
                    details.AppointmentDateTime = DateTime.SpecifyKind(details.AppointmentDateTime, DateTimeKind.Utc);
                }

                DateTime convertedDateTime =
                    !string.IsNullOrEmpty(AutomonTimeZone)
                            ? TimeZoneInfo.ConvertTimeBySystemTimeZoneId(details.AppointmentDateTime, AutomonTimeZone)
                            : details.AppointmentDateTime.ToLocalTime();

                return new OffenderDrugTestAppointment()
                {
                    Pin = clientIntegrationId,
                    Id = id,
                    UpdatedBy = updatedBy,
                    StartDate = convertedDateTime,
                    EndDate = convertedDateTime,
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
                    Location = details.Location,
                    IsOffenderPresent =
                        details.AppointmentStatus != null
                        ?
                            details.AppointmentStatus.Equals(Status.Completed, StringComparison.InvariantCultureIgnoreCase)
                            || details.AppointmentStatus.Equals(Status.Tampered, StringComparison.InvariantCultureIgnoreCase)
                        :
                            false
                };
            }
            //Drug Test Result
            else if (typeof(T) == typeof(ClientProfileDrugTestResultDetailsActivityResponse))
            {
                ClientProfileDrugTestResultDetailsActivityResponse details = (ClientProfileDrugTestResultDetailsActivityResponse)(object)activityDetails;

                //check if timezone information provided in given datetime, NO = specify it
                if (details.TestDateTime.Kind != DateTimeKind.Utc)
                {
                    details.TestDateTime = DateTime.SpecifyKind(details.TestDateTime, DateTimeKind.Utc);
                }

                DateTime convertedDateTime =
                    !string.IsNullOrEmpty(AutomonTimeZone)
                            ? TimeZoneInfo.ConvertTimeBySystemTimeZoneId(details.TestDateTime, AutomonTimeZone)
                            : details.TestDateTime.ToLocalTime();

                //derive automon test result value
                string automonTestResult = string.Empty;
                if(details.ResultStatus != null)
                {
                    if(details.ResultStatus.Equals(Status.Passed, StringComparison.InvariantCultureIgnoreCase))
                    {
                        automonTestResult = TestResult.Negative;
                    }
                    else if(details.ResultStatus.Equals(Status.Failed, StringComparison.InvariantCultureIgnoreCase))
                    {
                        automonTestResult = TestResult.Positive;
                    }
                    else
                    {
                        automonTestResult = details.ResultStatus;
                    }
                }

                return new OffenderDrugTestResult()
                {
                    Pin = clientIntegrationId,
                    Id = id,
                    UpdatedBy = updatedBy,
                    StartDate = convertedDateTime,
                    EndDate = convertedDateTime,
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
                    TestResult = automonTestResult,
                    Validities =
                        details.Dilute != null
                        ?
                            details.Dilute.Equals("Yes", StringComparison.InvariantCultureIgnoreCase)
                            ? "Diluted"
                            : string.Empty
                        :
                            string.Empty,
                    IsSaveFinalTestResult =
                        !string.IsNullOrEmpty(details.SentToLab)
                        ?
                            details.SentToLab.Equals("Yes", StringComparison.InvariantCultureIgnoreCase)
                        :
                            false
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

                //check if timezone information provided in given datetime, NO = specify it
                if (details.DateTime.Kind != DateTimeKind.Utc)
                {
                    details.DateTime = DateTime.SpecifyKind(details.DateTime, DateTimeKind.Utc);
                }

                DateTime convertedDateTime =
                    !string.IsNullOrEmpty(AutomonTimeZone)
                            ? TimeZoneInfo.ConvertTimeBySystemTimeZoneId(details.DateTime, AutomonTimeZone)
                            : details.DateTime.ToLocalTime();

                return new OffenderFieldVisit()
                {
                    Pin = clientIntegrationId,
                    Id = id,
                    UpdatedBy = updatedBy,
                    StartDate = convertedDateTime,
                    Comment = details.Note,
                    EndDate = convertedDateTime,
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
                    Pin = string.IsNullOrEmpty(automonIdentifier) ? null : automonIdentifier,
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

                //check if timezone information provided in given datetime, NO = specify it
                if (details.AppointmentDateTime.Kind != DateTimeKind.Utc)
                {
                    details.AppointmentDateTime = DateTime.SpecifyKind(details.AppointmentDateTime, DateTimeKind.Utc);
                }

                DateTime convertedDateTime =
                    !string.IsNullOrEmpty(AutomonTimeZone)
                            ? TimeZoneInfo.ConvertTimeBySystemTimeZoneId(details.AppointmentDateTime, AutomonTimeZone)
                            : details.AppointmentDateTime.ToLocalTime();

                return new OffenderTreatmentAppointment()
                {
                    Pin = clientIntegrationId,
                    Id = id,
                    UpdatedBy = updatedBy,
                    StartDate = convertedDateTime,
                    EndDate = convertedDateTime,
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
            //Client Profile - Profile Picture
            else if (typeof(T) == typeof(ClientProfilePictureDetailsActivityResponse))
            {
                ClientProfilePictureDetailsActivityResponse details = (ClientProfilePictureDetailsActivityResponse)(object)activityDetails;

                return new OffenderMugshot
                {
                    Pin = clientIntegrationId,
                    Id = id,
                    UpdatedBy = updatedBy,
                    DocumentData = Convert.FromBase64String(details.Image)
                };
            }
            //CAM Alert
            else if (typeof(T) == typeof(ClientProfileCAMAlertDetailsActivityResponse))
            {
                ClientProfileCAMAlertDetailsActivityResponse details = (ClientProfileCAMAlertDetailsActivityResponse)(object)activityDetails;

                //check if timezone information provided in given datetime, NO = specify it
                if (details.AlertDateTime.Kind != DateTimeKind.Utc)
                {
                    details.AlertDateTime = DateTime.SpecifyKind(details.AlertDateTime, DateTimeKind.Utc);
                }

                DateTime convertedDateTime =
                    !string.IsNullOrEmpty(AutomonTimeZone)
                            ? TimeZoneInfo.ConvertTimeBySystemTimeZoneId(details.AlertDateTime, AutomonTimeZone)
                            : details.AlertDateTime.ToLocalTime();

                return new OffenderCAMViolation()
                {
                    Pin = clientIntegrationId,
                    Id = id,
                    UpdatedBy = updatedBy,
                    ViolationDateTime = convertedDateTime,
                    ViolationStatus = details.Status
                };
            }
            //CAM Supervision
            else if (typeof(T) == typeof(ClientProfileCAMSupervisionDetailsActivityResponse))
            {
                ClientProfileCAMSupervisionDetailsActivityResponse details = (ClientProfileCAMSupervisionDetailsActivityResponse)(object)activityDetails;

                //check if timezone information provided in given datetime, NO = specify it
                if (details.MonitorDate.Kind != DateTimeKind.Utc)
                {
                    details.MonitorDate = DateTime.SpecifyKind(details.MonitorDate, DateTimeKind.Utc);
                }

                DateTime convertedDateTime =
                    !string.IsNullOrEmpty(AutomonTimeZone)
                            ? TimeZoneInfo.ConvertTimeBySystemTimeZoneId(details.MonitorDate, AutomonTimeZone)
                            : details.MonitorDate.ToLocalTime();

                return new OffenderCAMViolation()
                {
                    Pin = clientIntegrationId,
                    Id = id,
                    UpdatedBy = updatedBy,
                    ViolationDateTime = convertedDateTime,
                    ViolationStatus = details.Status
                };
            }
            //GPS Alert
            else if (typeof(T) == typeof(ClientProfileGPSAlertDetailsActivityResponse))
            {
                ClientProfileGPSAlertDetailsActivityResponse details = (ClientProfileGPSAlertDetailsActivityResponse)(object)activityDetails;

                //check if timezone information provided in given datetime, NO = specify it
                if (details.AlertDateTime.Kind != DateTimeKind.Utc)
                {
                    details.AlertDateTime = DateTime.SpecifyKind(details.AlertDateTime, DateTimeKind.Utc);
                }

                DateTime convertedDateTime =
                    !string.IsNullOrEmpty(AutomonTimeZone)
                            ? TimeZoneInfo.ConvertTimeBySystemTimeZoneId(details.AlertDateTime, AutomonTimeZone)
                            : details.AlertDateTime.ToLocalTime();

                return new OffenderGPSViolation()
                {
                    Pin = clientIntegrationId,
                    Id = id,
                    UpdatedBy = updatedBy,
                    ViolationDateTime = convertedDateTime,
                    ViolationStatus = details.Status
                };
            }
            //GPS Supervision
            else if (typeof(T) == typeof(ClientProfileGPSSupervisionDetailsActivityResponse))
            {
                ClientProfileGPSSupervisionDetailsActivityResponse details = (ClientProfileGPSSupervisionDetailsActivityResponse)(object)activityDetails;

                //check if timezone information provided in given datetime, NO = specify it
                if (details.MonitorDate.Kind != DateTimeKind.Utc)
                {
                    details.MonitorDate = DateTime.SpecifyKind(details.MonitorDate, DateTimeKind.Utc);
                }

                DateTime convertedDateTime =
                    !string.IsNullOrEmpty(AutomonTimeZone)
                            ? TimeZoneInfo.ConvertTimeBySystemTimeZoneId(details.MonitorDate, AutomonTimeZone)
                            : details.MonitorDate.ToLocalTime();

                return new OffenderGPSViolation()
                {
                    Pin = clientIntegrationId,
                    Id = id,
                    UpdatedBy = updatedBy,
                    ViolationDateTime = convertedDateTime,
                    ViolationStatus = details.Status
                };
            }
            //Incentive
            else if (typeof(T) == typeof(ClientProfileIncentiveDetailsActivityResponse))
            {
                ClientProfileIncentiveDetailsActivityResponse details = (ClientProfileIncentiveDetailsActivityResponse)(object)activityDetails;

                DateTime convertedDateTime = DateTime.Now;

                DateTime dateAssigned = DateTime.UtcNow;
                if (details.AssignedIncentive != null && DateTime.TryParse(details.AssignedIncentive.DateAssigned, out dateAssigned))
                {
                    //check if timezone information provided in given datetime, NO = specify it
                    if (dateAssigned.Kind != DateTimeKind.Utc)
                    {
                        dateAssigned = DateTime.SpecifyKind(dateAssigned, DateTimeKind.Utc);
                    }

                    //convert in required timezone
                    convertedDateTime =
                        !string.IsNullOrEmpty(AutomonTimeZone)
                                ? TimeZoneInfo.ConvertTimeBySystemTimeZoneId(dateAssigned, AutomonTimeZone)
                                : dateAssigned.ToLocalTime();
                }

                return new OffenderIncentive()
                {
                    Pin = clientIntegrationId,
                    Id = id,
                    UpdatedBy = updatedBy,
                    EventDateTime = convertedDateTime,
                    Magnitude = details.AssignedIncentive != null ? details.AssignedIncentive.Magnitude : null,
                    Response = details.AssignedIncentive != null ? details.AssignedIncentive.Description : null,
                    DateIssued = details.AssignedIncentive != null ? details.AssignedIncentive.DateAssigned : null,
                    IsBundled = details.IncentedActivities != null && details.IncentedActivities.Count() > 1,
                    IsSkipped = details.Status.Equals(Status.Skipped, StringComparison.InvariantCultureIgnoreCase),
                    IncentedActivities =
                        details.IncentedActivities != null && details.IncentedActivities.Any()
                        ? details.IncentedActivities.Select(x => new Automon.Model.IncentedActivityDetails { ActivityTypeName = x.Activity, ActivityIdentifier = x.ActivityIdentifier })
                        : null
                };
            }
            //Sanction
            else if (typeof(T) == typeof(ClientProfileSanctionDetailsActivityResponse))
            {
                ClientProfileSanctionDetailsActivityResponse details = (ClientProfileSanctionDetailsActivityResponse)(object)activityDetails;

                DateTime convertedDateTime = DateTime.Now;

                DateTime dateAssigned = DateTime.UtcNow;
                if (details.AssignedSanction != null && DateTime.TryParse(details.AssignedSanction.DateAssigned, out dateAssigned))
                {
                    //check if timezone information provided in given datetime, NO = specify it
                    if (dateAssigned.Kind != DateTimeKind.Utc)
                    {
                        dateAssigned = DateTime.SpecifyKind(dateAssigned, DateTimeKind.Utc);
                    }

                    //convert in required timezone
                    convertedDateTime =
                        !string.IsNullOrEmpty(AutomonTimeZone)
                                ? TimeZoneInfo.ConvertTimeBySystemTimeZoneId(dateAssigned, AutomonTimeZone)
                                : dateAssigned.ToLocalTime();
                }

                return new OffenderSanction()
                {
                    Pin = clientIntegrationId,
                    Id = id,
                    UpdatedBy = updatedBy,
                    EventDateTime = convertedDateTime,
                    Magnitude = details.AssignedSanction != null ? details.AssignedSanction.Magnitude : null,
                    Response = details.AssignedSanction != null ? details.AssignedSanction.Description : null,
                    DateIssued = details.AssignedSanction != null ? details.AssignedSanction.DateAssigned : null,
                    IsBundled = details.SanctionedActivities != null && details.SanctionedActivities.Count() > 1,
                    IsSkipped = details.Status.Equals(Status.Skipped, StringComparison.InvariantCultureIgnoreCase),
                    SanctionedActivities =
                        details.SanctionedActivities != null && details.SanctionedActivities.Any()
                        ? details.SanctionedActivities.Select(x => new Automon.Model.SanctionedActivityDetails { ActivityTypeName = x.Activity, ActivityIdentifier = x.ActivityIdentifier })
                        : null
                };
            }
            //OnDemand Sanction
            else if (typeof(T) == typeof(ClientProfileOnDemandSanctionDetailsActivityResponse))
            {
                ClientProfileOnDemandSanctionDetailsActivityResponse details = (ClientProfileOnDemandSanctionDetailsActivityResponse)(object)activityDetails;

                return new OffenderOnDemandSanction()
                {
                    Pin = clientIntegrationId,
                    Id = id,
                    UpdatedBy = updatedBy
                };
            }

            return null;
        }
    }
}
