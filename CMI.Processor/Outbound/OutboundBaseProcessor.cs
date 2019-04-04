using CMI.Automon.Model;
using CMI.Common.Logging;
using CMI.Common.Notification;
using CMI.MessageRetriever.Model;
using CMI.Processor.DAL;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CMI.Processor
{
    public abstract class OutboundBaseProcessor
    {
        protected ILogger Logger { get; set; }
        protected ProcessorConfig ProcessorConfig { get; set; }

        public OutboundBaseProcessor(
            IServiceProvider serviceProvider,
            IConfiguration configuration
        )
        {
            Logger = (ILogger)serviceProvider.GetService(typeof(ILogger));
            ProcessorConfig = configuration.GetSection(ConfigKeys.ProcessorConfig).Get<ProcessorConfig>();
        }

        public abstract TaskExecutionStatus Execute(IEnumerable<MessageBodyResponse> messages);

        protected T RetrieveActivityDetails<T>(dynamic activityDetails)
        {
            string activityDetailsString = JsonConvert.SerializeObject(activityDetails);

            return JsonConvert.DeserializeObject<T>(activityDetailsString);
        }

        protected Offender ConvertResponseToObject<T>(ClientResponse clientDetails, T activityDetails)
        {
            if (typeof(T) == typeof(NoteActivityDetailsResponse))
            {
                NoteActivityDetailsResponse noteActivityDetailsResponse = (NoteActivityDetailsResponse)(object)activityDetails;
                return new OffenderNote()
                {
                    Pin = clientDetails.IntegrationId,
                    Text = noteActivityDetailsResponse.NoteText,
                    AuthorEmail = noteActivityDetailsResponse.NoteAuthor,
                    Date = noteActivityDetailsResponse.CreatedDate
                };
            }
            //else if (typeof(T) == typeof(OfficeVisitActivityResponse))
            //{
            //    OfficeVisitActivityResponse officeVisitActivityResponse = (OfficeVisitActivityResponse)(object)contentDetails;
            //    return new OfficeVisit()
            //    {
            //        Pin = clientDetails.IntegrationId,
            //        Status = officeVisitActivityResponse.Status,
            //        Notes = officeVisitActivityResponse.Notes,
            //        DateTime = officeVisitActivityResponse.DateTime,
            //        Location = officeVisitActivityResponse.Summary.Location,
            //        OnTime = officeVisitActivityResponse.Summary.OnTime
            //        //other members
            //    };
            //}
            //else if (typeof(T) == typeof(DrugTestAppointmentActivityResponse))
            //{
            //    DrugTestAppointmentActivityResponse drugTestAppointmentActivityResponse = (DrugTestAppointmentActivityResponse)(object)contentDetails;
            //    return new DrugTestAppointment()
            //    {
            //        Pin = clientDetails.IntegrationId,
            //        Identifier = drugTestAppointmentActivityResponse.Identifier,
            //        AppointmentDateTime = drugTestAppointmentActivityResponse.AppointmentDateTime,
            //        AppointmentStatus = drugTestAppointmentActivityResponse.AppointmentStatus,
            //        Location = drugTestAppointmentActivityResponse.Location
            //    };
            //}
            //else if (typeof(T) == typeof(DrugTestResultActivityResponse))
            //{
            //    DrugTestResultActivityResponse drugTestResultActivityResponse = (DrugTestResultActivityResponse)(object)contentDetails;
            //    return new DrugTestResult()
            //    {
            //        Pin = clientDetails.IntegrationId,
            //        Identifier = drugTestResultActivityResponse.Identifier,
            //        TestDateTime = drugTestResultActivityResponse.TestDateTime,
            //        ResultStatus = drugTestResultActivityResponse.ResultStatus,
            //        Location = drugTestResultActivityResponse.Location,
            //        DrugTestType = drugTestResultActivityResponse.Details.DrugTestType,
            //        DeliberateTamper = drugTestResultActivityResponse.Details.DeliberateTamper,
            //        Screened = drugTestResultActivityResponse.Details.Screened,
            //        SentToLab = drugTestResultActivityResponse.Details.SentToLab,
            //        DrugTestId = drugTestResultActivityResponse.Details.DrugTestId,
            //        Dilute = drugTestResultActivityResponse.Details.Dilute,
            //        CreatinineLevel = drugTestResultActivityResponse.Details.CreatinineLevel
            //    };
            //}

            return null;
        }
    }
}
