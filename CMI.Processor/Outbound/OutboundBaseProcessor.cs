﻿using CMI.Automon.Model;
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
            //Note
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
            //Office Visit
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
                    /* Status: Pending = 0, Missed = 16, Cancelled = 10, Complete = 2 */
                    Status =
                        details.Status != null
                        ?
                            details.Status.Equals("Attended", StringComparison.InvariantCultureIgnoreCase)
                            ? 2
                            : details.Status.Equals("Missed", StringComparison.InvariantCultureIgnoreCase)
                                ? 16
                                : details.Status.Equals("Excused", StringComparison.InvariantCultureIgnoreCase)
                                    ? 10
                                    : 0
                        :
                            0,
                    IsOffenderPresent =
                        details.Status != null
                        ?
                            details.Status.Equals("Attended", StringComparison.InvariantCultureIgnoreCase)
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
                    UpdatedBy = updatedBy,
                    StartDate = details.AppointmentDateTime,
                    EndDate = details.AppointmentDateTime,
                    /* Status: Pending = 0, Missed = 16, Cancelled = 10, Complete = 2 */
                    Status =
                        details.AppointmentStatus != null
                        ?
                            details.AppointmentStatus.Equals("Completed", StringComparison.InvariantCultureIgnoreCase)
                            || details.AppointmentStatus.Equals("Tampered", StringComparison.InvariantCultureIgnoreCase)
                            ? 2
                            : details.AppointmentStatus.Equals("Missed", StringComparison.InvariantCultureIgnoreCase)
                                ? 16
                                : details.AppointmentStatus.Equals("Excused", StringComparison.InvariantCultureIgnoreCase)
                                    ? 10
                                    : 0
                        :
                            0,
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
                    UpdatedBy = updatedBy,
                    StartDate = details.TestDateTime,
                    EndDate = details.TestDateTime,
                    /* Status: Pending = 0, Missed = 16, Cancelled = 10, Complete = 2 */
                    Status =
                        details.ResultStatus != null
                        ?
                            details.ResultStatus.Equals("Passed", StringComparison.InvariantCultureIgnoreCase)
                            || details.ResultStatus.Equals("Failed", StringComparison.InvariantCultureIgnoreCase)
                            || details.ResultStatus.Equals("Tampered", StringComparison.InvariantCultureIgnoreCase)
                            ?
                                2
                            :
                                0
                        :
                            0,
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
                    UpdatedBy = updatedBy,
                    StartDate = details.DateTime,
                    Comment = details.Note,
                    EndDate = details.DateTime,
                    /* Status: Pending = 0, Missed = 16, Cancelled = 10, Complete = 2 */
                    Status =
                        details.Status != null
                        ?
                            details.Status.Equals("Attended", StringComparison.InvariantCultureIgnoreCase)
                            || details.Status.Equals("Attempted", StringComparison.InvariantCultureIgnoreCase)
                            ?
                                2
                            :
                                0
                        :
                            0,
                    IsOffenderPresent =
                        details.Status != null
                        ?
                            details.Status.Equals("Attended", StringComparison.InvariantCultureIgnoreCase)
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
            //Client Profile - Email Details
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
            //Client Profile - Address Details
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
                    UpdatedBy = updatedBy,

                    StartDate = details.AppointmentDateTime,
                    EndDate = details.AppointmentDateTime,
                    /* Status: Pending = 0, Missed = 16, Cancelled = 10, Complete = 2 */
                    Status =
                        details.Status != null
                        ?
                            details.Status.Equals("Attended", StringComparison.InvariantCultureIgnoreCase)
                            ? 2
                            : details.Status.Equals("Missed", StringComparison.InvariantCultureIgnoreCase)
                                ? 16
                                : details.Status.Equals("Excused", StringComparison.InvariantCultureIgnoreCase)
                                    ? 10
                                    : 0
                        :
                            0,
                    Comment = details.Notes
                };
            }

            return null;
        }
    }
}
