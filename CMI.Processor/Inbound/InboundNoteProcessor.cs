using CMI.Automon.Interface;
using CMI.Automon.Model;
using CMI.Common.Logging;
using CMI.Common.Notification;
using CMI.Nexus.Interface;
using CMI.Nexus.Model;
using CMI.Processor.DAL;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CMI.Processor
{
    public class InboundNoteProcessor : InboundBaseProcessor
    {
        private readonly IOffenderNoteService offenderNoteService;
        private readonly INoteService noteService;

        public InboundNoteProcessor(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            IOffenderNoteService offenderNoteService,
            INoteService noteService
        )
            : base(serviceProvider, configuration)
        {
            this.offenderNoteService = offenderNoteService;
            this.noteService = noteService;
        }

        public override TaskExecutionStatus Execute(DateTime? lastExecutionDateTime, IEnumerable<string> officerLogonsToFilter)
        {
            Logger.LogInfo(new LogRequest
            {
                OperationName = this.GetType().Name,
                MethodName = "Execute",
                Message = "Note processing intiated."
            });

            IEnumerable<OffenderNote> allOffenderNoteDetails = null;
            TaskExecutionStatus taskExecutionStatus = new TaskExecutionStatus { ProcessorType = Common.Notification.ProcessorType.Inbound, TaskName = "Process Notes" };

            try
            {
                //retrieve data from Automon for processing
                allOffenderNoteDetails = offenderNoteService.GetAllOffenderNotes(ProcessorConfig.CmiDbConnString, lastExecutionDateTime, GetOfficerLogonToFilterDataTable(officerLogonsToFilter));

                //check if there are any records to process
                if (allOffenderNoteDetails.Any())
                {
                    //log number of records received from Automon
                    Logger.LogDebug(new LogRequest
                    {
                        OperationName = this.GetType().Name,
                        MethodName = "Execute",
                        Message = "Offender Note records received from Automon.",
                        CustomParams = allOffenderNoteDetails.Count().ToString()
                    });

                    //retrieve distinct list of offender pin
                    List<string> distinctOffenderPin = allOffenderNoteDetails.Select(p => p.Pin).Distinct().ToList();

                    //iterate through each of offender pin
                    foreach (string currentOffenderPin in distinctOffenderPin)
                    {
                        //check if client exist on Nexus side for current offender pin. This is to avoid validation errors from Nexus API in further calls.
                        if (ClientService.GetClientDetails(currentOffenderPin) != null)
                        {
                            //get all notes for given offender pin
                            var allExistingNoteDetails = noteService.GetAllNoteDetails(currentOffenderPin);

                            if (allExistingNoteDetails != null && allExistingNoteDetails.Any())
                            {
                                //set ClientId value
                                allExistingNoteDetails.ForEach(ea => ea.ClientId = currentOffenderPin);
                            }

                            //iterate through each of offender note details for current offender pin
                            foreach (var offenderNoteDetails in allOffenderNoteDetails.Where(a => a.Pin.Equals(currentOffenderPin, StringComparison.InvariantCultureIgnoreCase)))
                            {
                                taskExecutionStatus.AutomonReceivedRecordCount++;

                                Note note = null;
                                try
                                {
                                    //transform offender note details in Nexus compliant model
                                    note = new Note()
                                    {
                                        ClientId = FormatId(offenderNoteDetails.Pin),
                                        NoteId = FormatId(Convert.ToString(offenderNoteDetails.Id)),
                                        NoteText = offenderNoteDetails.Text,
                                        NoteDatetime = offenderNoteDetails.Date.ToString(),
                                        NoteType = offenderNoteDetails.NoteType,
                                        NoteAuthor = offenderNoteDetails.AuthorEmail
                                    };

                                    //get crud action type based on comparison
                                    switch (GetCrudActionType(note, allExistingNoteDetails))
                                    {
                                        case CrudActionType.Add:
                                            if (noteService.AddNewNoteDetails(note))
                                            {
                                                taskExecutionStatus.NexusAddRecordCount++;

                                                Logger.LogDebug(new LogRequest
                                                {
                                                    OperationName = this.GetType().Name,
                                                    MethodName = "Execute",
                                                    Message = "New Client Note details added successfully.",
                                                    AutomonData = JsonConvert.SerializeObject(offenderNoteDetails),
                                                    NexusData = JsonConvert.SerializeObject(note)
                                                });
                                            }
                                            break;
                                        case CrudActionType.Update:
                                            if (noteService.UpdateNoteDetails(note))
                                            {
                                                taskExecutionStatus.NexusUpdateRecordCount++;

                                                Logger.LogDebug(new LogRequest
                                                {
                                                    OperationName = this.GetType().Name,
                                                    MethodName = "Execute",
                                                    Message = "Existing Client Note details updated successfully.",
                                                    AutomonData = JsonConvert.SerializeObject(offenderNoteDetails),
                                                    NexusData = JsonConvert.SerializeObject(note)
                                                });
                                            }
                                            break;
                                        default:
                                            taskExecutionStatus.AutomonReceivedRecordCount--;
                                            break;
                                    }
                                }
                                catch (CmiException ce)
                                {
                                    taskExecutionStatus.NexusFailureRecordCount++;

                                    Logger.LogWarning(new LogRequest
                                    {
                                        OperationName = this.GetType().Name,
                                        MethodName = "Execute",
                                        Message = "Error occurred in API while processing a Client Note.",
                                        Exception = ce,
                                        AutomonData = JsonConvert.SerializeObject(offenderNoteDetails),
                                        NexusData = JsonConvert.SerializeObject(note)
                                    });
                                }
                                catch (Exception ex)
                                {
                                    taskExecutionStatus.NexusFailureRecordCount++;

                                    Logger.LogError(new LogRequest
                                    {
                                        OperationName = this.GetType().Name,
                                        MethodName = "Execute",
                                        Message = "Error occurred while processing a Client Note.",
                                        Exception = ex,
                                        AutomonData = JsonConvert.SerializeObject(offenderNoteDetails),
                                        NexusData = JsonConvert.SerializeObject(note)
                                    });
                                }
                            }
                        }
                    }
                }

                taskExecutionStatus.IsSuccessful = taskExecutionStatus.NexusFailureRecordCount == 0;
            }
            catch (Exception ex)
            {
                taskExecutionStatus.IsSuccessful = false;

                Logger.LogError(new LogRequest
                {
                    OperationName = this.GetType().Name,
                    MethodName = "Execute",
                    Message = "Error occurred while processing Notes.",
                    Exception = ex,
                    AutomonData = JsonConvert.SerializeObject(allOffenderNoteDetails)
                });
            }

            Logger.LogInfo(new LogRequest
            {
                OperationName = this.GetType().Name,
                MethodName = "Execute",
                Message = "Notes processing completed.",
                CustomParams = JsonConvert.SerializeObject(taskExecutionStatus)
            });

            return taskExecutionStatus;
        }

        protected override void LoadLookupData()
        {
            throw new NotImplementedException();
        }

        private CrudActionType GetCrudActionType(Note note, IEnumerable<Note> notes)
        {
            //check if list is null, YES = return Add Action type
            if (notes == null)
            {
                return CrudActionType.Add;
            }

            //try to get existing record using ClientId & NoteId
            Note existingNote = notes.Where(a
                =>
                    a.ClientId.Equals(note.ClientId, StringComparison.InvariantCultureIgnoreCase)
                    && a.NoteId.Equals(note.NoteId, StringComparison.InvariantCultureIgnoreCase)
            )
            .FirstOrDefault();

            //check if record already exists
            if (existingNote == null)
            {
                //record does not exist
                return CrudActionType.Add;
            }
            else
            {
                //record already exist.
                //compare with existing record. Equal = return None action type, Not Equal = return Update action type
                if (note.Equals(existingNote))
                {
                    return CrudActionType.None;
                }
                else
                {
                    return CrudActionType.Update;
                }
            }
        }
    }
}
