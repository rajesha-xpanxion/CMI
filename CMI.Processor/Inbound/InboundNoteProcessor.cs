using CMI.Automon.Interface;
using CMI.Automon.Model;
using CMI.Common.Logging;
using CMI.Nexus.Interface;
using CMI.Nexus.Model;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

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

        public override Common.Notification.TaskExecutionStatus Execute(DateTime? lastExecutionDateTime)
        {
            Logger.LogInfo(new LogRequest
            {
                OperationName = this.GetType().Name,
                MethodName = "Execute",
                Message = "Note processing intiated."
            });

            IEnumerable<OffenderNote> allOffenderNoteDetails = null;
            Common.Notification.TaskExecutionStatus taskExecutionStatus = new Common.Notification.TaskExecutionStatus { TaskName = "Process Notes" };

            try
            {
                allOffenderNoteDetails = offenderNoteService.GetAllOffenderNotes(ProcessorConfig.CmiDbConnString, lastExecutionDateTime);

                foreach (var offenderNoteDetails in allOffenderNoteDetails)
                {
                    taskExecutionStatus.AutomonReceivedRecordCount++;

                    Note note = null;
                    try
                    {
                        note = new Note()
                        {
                            ClientId = FormatId(offenderNoteDetails.Pin),
                            NoteId = FormatId(Convert.ToString(offenderNoteDetails.Id)),
                            NoteText = offenderNoteDetails.Text,
                            NoteDatetime = offenderNoteDetails.Date.ToString(),
                            NoteType = offenderNoteDetails.NoteType
                        };

                        if (ClientService.GetClientDetails(note.ClientId) != null)
                        {
                            if (noteService.GetNoteDetails(note.ClientId, note.NoteId) == null)
                            {
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
                            }
                            else
                            {
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
                            }
                        }
                        else
                        {
                            taskExecutionStatus.AutomonReceivedRecordCount--;
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
    }
}
