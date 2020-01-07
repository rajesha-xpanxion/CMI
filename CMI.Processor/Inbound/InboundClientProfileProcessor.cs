using CMI.Automon.Interface;
using CMI.Automon.Model;
using CMI.Common.Logging;
using CMI.Nexus.Model;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using Microsoft.Extensions.Configuration;
using CMI.Common.Notification;
using CMI.Common.Imaging;

namespace CMI.Processor
{
    public class InboundClientProfileProcessor : InboundBaseProcessor
    {
        private readonly IOffenderService offenderService;
        private readonly IOffenderProfilePictureService offenderProfilePictureService;
        private readonly IImager imager;

        public InboundClientProfileProcessor(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            IOffenderService offenderService,
            IOffenderProfilePictureService offenderProfilePictureService,
            IImager imager
        )
            : base(serviceProvider, configuration)
        {
            this.offenderService = offenderService;
            this.offenderProfilePictureService = offenderProfilePictureService;
            this.imager = imager;
        }

        public override TaskExecutionStatus Execute(DateTime? lastExecutionDateTime, IEnumerable<string> officerLogonsToFilter)
        {
            Logger.LogInfo(new LogRequest
            {
                OperationName = this.GetType().Name,
                MethodName = "Execute",
                Message = "Client Profile processing initiated."
            });

            //load required lookup data
            LoadLookupData();

            IEnumerable<Offender> allOffenderDetails = null;
            TaskExecutionStatus taskExecutionStatus = new TaskExecutionStatus { ProcessorType = ProcessorType.Inbound, TaskName = "Process Client Profiles" };

            try
            {
                allOffenderDetails = offenderService.GetAllOffenderDetails(ProcessorConfig.CmiDbConnString, lastExecutionDateTime, GetOfficerLogonToFilterDataTable(officerLogonsToFilter));

                //log number of records received from Automon
                if (allOffenderDetails.Any())
                {
                    Logger.LogDebug(new LogRequest
                    {
                        OperationName = this.GetType().Name,
                        MethodName = "Execute",
                        Message = "Offender records received from Automon.",
                        CustomParams = allOffenderDetails.Count().ToString()
                    });
                }

                foreach (var offenderDetails in allOffenderDetails)
                {
                    taskExecutionStatus.AutomonReceivedRecordCount++;

                    Client client = null;
                    try
                    {
                        client = new Client()
                        {
                            IntegrationId = FormatId(offenderDetails.Pin),
                            FirstName = offenderDetails.FirstName,
                            MiddleName = string.IsNullOrEmpty(offenderDetails.MiddleName) ? null : offenderDetails.MiddleName,
                            LastName = offenderDetails.LastName,
                            ClientType = offenderDetails.ClientType,
                            TimeZone = offenderDetails.TimeZone,
                            Gender = offenderDetails.Gender,
                            Ethnicity = MapEthnicity(offenderDetails.RaceDescription, offenderDetails.RacePermDesc),
                            DateOfBirth = offenderDetails.DateOfBirth.ToShortDateString(),

                            CaseloadId = MapCaseload(offenderDetails.CaseloadName),
                            SupervisingOfficerEmailId = MapSupervisingOfficer(offenderDetails.OfficerFirstName, offenderDetails.OfficerLastName, offenderDetails.OfficerEmail),
                            StaticRiskRating = MapStaticRiskRating(offenderDetails.DeptSupLevel),
                            CmsStatus = MapCmsStatus(offenderDetails.SupervisionStatus, offenderDetails.BodyStatus)
                        };

                        //check if client already exists and if yes then retrieve it
                        Client existingClientDetails = ClientService.GetClientDetails(client.IntegrationId);
                        if (existingClientDetails == null)
                        {
                            //add new client profile details to Nexus
                            if (ClientService.AddNewClientDetails(client))
                            {
                                //increase add record count
                                taskExecutionStatus.NexusAddRecordCount++;

                                Logger.LogDebug(new LogRequest
                                {
                                    OperationName = this.GetType().Name,
                                    MethodName = "Execute",
                                    Message = "New Client Profile added successfully.",
                                    AutomonData = JsonConvert.SerializeObject(offenderDetails),
                                    NexusData = JsonConvert.SerializeObject(client)
                                });

                                //get offender mugshot photo details
                                OffenderMugshot offenderMugshot = offenderProfilePictureService.GetOffenderMugshotPhoto(ProcessorConfig.CmiDbConnString, offenderDetails.Pin);

                                if (offenderMugshot != null)
                                {
                                    Logger.LogDebug(new LogRequest
                                    {
                                        OperationName = this.GetType().Name,
                                        MethodName = "Execute",
                                        Message = "Offender MugShot-Photo found in Automon.",
                                        AutomonData = JsonConvert.SerializeObject(offenderMugshot)
                                    });
                                }

                                //check if there is any mugshot photo set for given offender
                                if (offenderMugshot != null && offenderMugshot.DocumentData!= null)
                                {
                                    //check if magshot size is exceeding threshold limit, YES = Resize it to small size
                                    double offenderMugshotPhotoSizeInMegaBytes = imager.ConvertBytesToMegaBytes(offenderMugshot.DocumentData.LongLength);
                                    if (offenderMugshotPhotoSizeInMegaBytes > ProcessorConfig.InboundProcessorConfig.InputImageSizeThresholdInMegaBytes)
                                    {
                                        Logger.LogDebug(new LogRequest
                                        {
                                            OperationName = this.GetType().Name,
                                            MethodName = "Execute",
                                            Message = string.Format(
                                                "Offender MugShot-Photo having size of {0} MB is exceeding threshold size of {1} MB. Attempting to resize it.",
                                                Math.Round(offenderMugshotPhotoSizeInMegaBytes, 2),
                                                ProcessorConfig.InboundProcessorConfig.InputImageSizeThresholdInMegaBytes
                                            )
                                        });

                                        //try to resize image
                                        offenderMugshot.DocumentData = imager.ResizeImage(offenderMugshot.DocumentData, ProcessorConfig.InboundProcessorConfig.OutputImageMaxSize);

                                        Logger.LogDebug(new LogRequest
                                        {
                                            OperationName = this.GetType().Name,
                                            MethodName = "Execute",
                                            Message = string.Format(
                                                "Offender MugShot-Photo resized to size of {0} MB.",
                                                Math.Round(imager.ConvertBytesToMegaBytes(offenderMugshot.DocumentData.LongLength), 2)
                                            )
                                        });
                                    }

                                    //transform offender mugshot photo into Nexus compliant model
                                    ClientProfilePicture clientProfilePicture = new ClientProfilePicture
                                    {
                                        IntegrationId = FormatId(offenderMugshot.Pin),
                                        ImageBase64String = Convert.ToBase64String(offenderMugshot.DocumentData)
                                    };

                                    if (clientProfilePicture != null)
                                    {
                                        Logger.LogDebug(new LogRequest
                                        {
                                            OperationName = this.GetType().Name,
                                            MethodName = "Execute",
                                            Message = "Offender MugShot-Photo object transformed successfully.",
                                            AutomonData = JsonConvert.SerializeObject(offenderMugshot),
                                            NexusData = JsonConvert.SerializeObject(clientProfilePicture)
                                        });
                                    }

                                    if (ClientService.AddNewClientProfilePicture(clientProfilePicture))
                                    {
                                        Logger.LogDebug(new LogRequest
                                        {
                                            OperationName = this.GetType().Name,
                                            MethodName = "Execute",
                                            Message = "New Client Profile Picture added successfully.",
                                            AutomonData = JsonConvert.SerializeObject(offenderMugshot),
                                            NexusData = JsonConvert.SerializeObject(clientProfilePicture)
                                        });
                                    }
                                }

                                //update CMS Status
                                if(!string.IsNullOrEmpty(client.CmsStatus))
                                {
                                    if(ClientService.UpdateClientCmsStatus(client.IntegrationId, client.CmsStatus))
                                    {
                                        Logger.LogDebug(new LogRequest
                                        {
                                            OperationName = this.GetType().Name,
                                            MethodName = "Execute",
                                            Message = "Client Cms Status updated successfully.",
                                            AutomonData = JsonConvert.SerializeObject(new { offenderDetails.SupervisionStatus, offenderDetails.BodyStatus }),
                                            NexusData = JsonConvert.SerializeObject(client.CmsStatus)
                                        });
                                    }
                                }
                            }
                        }
                        else
                        {
                            //log details of existing client profile
                            Logger.LogDebug(new LogRequest
                            {
                                OperationName = this.GetType().Name,
                                MethodName = "Execute",
                                Message = "Client Profile already exists.",
                                AutomonData = JsonConvert.SerializeObject(offenderDetails),
                                NexusData = JsonConvert.SerializeObject(existingClientDetails)
                            });

                            //check if any value already exists for static risk rating, yes = replace it in passing model so that existing value will not be replaced/changed
                            if (!string.IsNullOrEmpty(existingClientDetails.StaticRiskRating) && !existingClientDetails.StaticRiskRating.Equals(Nexus.Service.StaticRiskRating.Unspecified))
                            {
                                client.StaticRiskRating = existingClientDetails.StaticRiskRating;
                            }

                            //set original value of NeedsClassification retrieved from Nexus to field in passing model so that it will not get overridden
                            client.NeedsClassification = existingClientDetails.NeedsClassification;

                            if (ClientService.UpdateClientDetails(client))
                            {
                                taskExecutionStatus.NexusUpdateRecordCount++;

                                Logger.LogDebug(new LogRequest
                                {
                                    OperationName = this.GetType().Name,
                                    MethodName = "Execute",
                                    Message = "Existing Client Profile updated successfully.",
                                    AutomonData = JsonConvert.SerializeObject(offenderDetails),
                                    NexusData = JsonConvert.SerializeObject(client)
                                });

                                //check if any profile picture exists in Nexus
                                if(ClientService.GetClientProfilePicture(client.IntegrationId) == null)
                                {
                                    //profile picture does not exist in Nexus. Try to get it from Automon and set in Nexus.

                                    //get offender mugshot photo details
                                    OffenderMugshot offenderMugshot = offenderProfilePictureService.GetOffenderMugshotPhoto(ProcessorConfig.CmiDbConnString, offenderDetails.Pin);

                                    if (offenderMugshot != null)
                                    {
                                        Logger.LogDebug(new LogRequest
                                        {
                                            OperationName = this.GetType().Name,
                                            MethodName = "Execute",
                                            Message = "Offender MugShot-Photo found in Automon.",
                                            AutomonData = JsonConvert.SerializeObject(offenderMugshot)
                                        });
                                    }

                                    //check if there is any mugshot photo set for given offender
                                    if (offenderMugshot != null && offenderMugshot.DocumentData != null)
                                    {
                                        //check if magshot size is exceeding threshold limit, YES = Resize it to small size
                                        double offenderMugshotPhotoSizeInMegaBytes = imager.ConvertBytesToMegaBytes(offenderMugshot.DocumentData.LongLength);
                                        if (offenderMugshotPhotoSizeInMegaBytes > ProcessorConfig.InboundProcessorConfig.InputImageSizeThresholdInMegaBytes)
                                        {
                                            Logger.LogDebug(new LogRequest
                                            {
                                                OperationName = this.GetType().Name,
                                                MethodName = "Execute",
                                                Message = string.Format(
                                                    "Offender MugShot-Photo having size of {0} MB is exceeding threshold size of {1} MB. Attempting to resize it.",
                                                    Math.Round(offenderMugshotPhotoSizeInMegaBytes, 2),
                                                    ProcessorConfig.InboundProcessorConfig.InputImageSizeThresholdInMegaBytes
                                                )
                                            });

                                            //try to resize image
                                            offenderMugshot.DocumentData = imager.ResizeImage(offenderMugshot.DocumentData, ProcessorConfig.InboundProcessorConfig.OutputImageMaxSize);

                                            Logger.LogDebug(new LogRequest
                                            {
                                                OperationName = this.GetType().Name,
                                                MethodName = "Execute",
                                                Message = string.Format(
                                                    "Offender MugShot-Photo resized to size of {0} MB.",
                                                    Math.Round(imager.ConvertBytesToMegaBytes(offenderMugshot.DocumentData.LongLength), 2)
                                                )
                                            });
                                        }

                                        //transform offender mugshot photo into Nexus compliant model
                                        ClientProfilePicture clientProfilePicture = new ClientProfilePicture
                                        {
                                            IntegrationId = FormatId(offenderMugshot.Pin),
                                            ImageBase64String = Convert.ToBase64String(offenderMugshot.DocumentData)
                                        };

                                        if (clientProfilePicture != null)
                                        {
                                            Logger.LogDebug(new LogRequest
                                            {
                                                OperationName = this.GetType().Name,
                                                MethodName = "Execute",
                                                Message = "Offender MugShot-Photo object transformed successfully.",
                                                AutomonData = JsonConvert.SerializeObject(offenderMugshot),
                                                NexusData = JsonConvert.SerializeObject(clientProfilePicture)
                                            });
                                        }

                                        if (ClientService.AddNewClientProfilePicture(clientProfilePicture))
                                        {
                                            Logger.LogDebug(new LogRequest
                                            {
                                                OperationName = this.GetType().Name,
                                                MethodName = "Execute",
                                                Message = "New Client Profile Picture added successfully.",
                                                AutomonData = JsonConvert.SerializeObject(offenderMugshot),
                                                NexusData = JsonConvert.SerializeObject(clientProfilePicture)
                                            });
                                        }
                                    }
                                }

                                //update CMS Status
                                if (!string.IsNullOrEmpty(client.CmsStatus))
                                {
                                    if (ClientService.UpdateClientCmsStatus(client.IntegrationId, client.CmsStatus))
                                    {
                                        Logger.LogDebug(new LogRequest
                                        {
                                            OperationName = this.GetType().Name,
                                            MethodName = "Execute",
                                            Message = "Client Cms Status updated successfully.",
                                            AutomonData = JsonConvert.SerializeObject(new { offenderDetails.SupervisionStatus, offenderDetails.BodyStatus }),
                                            NexusData = JsonConvert.SerializeObject(client.CmsStatus)
                                        });
                                    }
                                }
                            }
                        }
                    }
                    catch (CmiException ce)
                    {
                        taskExecutionStatus.NexusFailureRecordCount++;

                        Logger.LogWarning(new LogRequest
                        {
                            OperationName = this.GetType().Name,
                            MethodName = "Execute",
                            Message = "Error occurred in API while processing a Client Profile.",
                            Exception = ce,
                            AutomonData = JsonConvert.SerializeObject(offenderDetails),
                            NexusData = JsonConvert.SerializeObject(client)
                        });
                    }
                    catch (Exception ex)
                    {
                        taskExecutionStatus.NexusFailureRecordCount++;

                        Logger.LogError(new LogRequest
                        {
                            OperationName = this.GetType().Name,
                            MethodName = "Execute",
                            Message = "Error occurred while processing a Client Profile.",
                            Exception = ex,
                            AutomonData = JsonConvert.SerializeObject(offenderDetails),
                            NexusData = JsonConvert.SerializeObject(client)
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
                    Message = "Error occurred while processing Client Profiles.",
                    Exception = ex,
                    AutomonData = JsonConvert.SerializeObject(allOffenderDetails)
                });
            }

            Logger.LogInfo(new LogRequest
            {
                OperationName = this.GetType().Name,
                MethodName = "Execute",
                Message = "Client Profile processing completed.",
                CustomParams = JsonConvert.SerializeObject(taskExecutionStatus)
            });

            return taskExecutionStatus;
        }

        protected override void LoadLookupData()
        {
            //load Ethnicities lookup data
            try
            {
                if (LookupService.Ethnicities != null)
                {
                    Logger.LogDebug(new LogRequest
                    {
                        OperationName = this.GetType().Name,
                        MethodName = "LoadLookupData",
                        Message = "Successfully retrieved Ethnicities from lookup",
                        CustomParams = JsonConvert.SerializeObject(LookupService.Ethnicities)
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(new LogRequest
                {
                    OperationName = this.GetType().Name,
                    MethodName = "LoadLookupData",
                    Message = "Error occurred while loading Ethnicities lookup data",
                    Exception = ex
                });
            }

            //load CaseLoads lookup data
            try
            {
                if (LookupService.CaseLoads != null)
                {
                    Logger.LogDebug(new LogRequest
                    {
                        OperationName = this.GetType().Name,
                        MethodName = "LoadLookupData",
                        Message = "Successfully retrieved Caseloads from lookup",
                        CustomParams = JsonConvert.SerializeObject(LookupService.CaseLoads)
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(new LogRequest
                {
                    OperationName = this.GetType().Name,
                    MethodName = "LoadLookupData",
                    Message = "Error occurred while loading CaseLoads lookup data",
                    Exception = ex
                });
            }

            //load SupervisingOfficers lookup data
            try
            {
                if (LookupService.SupervisingOfficers != null)
                {
                    Logger.LogDebug(new LogRequest
                    {
                        OperationName = this.GetType().Name,
                        MethodName = "LoadLookupData",
                        Message = "Successfully retrieved SupervisingOfficers from lookup",
                        CustomParams = JsonConvert.SerializeObject(LookupService.SupervisingOfficers)
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(new LogRequest
                {
                    OperationName = this.GetType().Name,
                    MethodName = "LoadLookupData",
                    Message = "Error occurred while loading SupervisingOfficers lookup data",
                    Exception = ex
                });
            }

            //load Genders lookup data
            try
            {
                if (LookupService.Genders != null)
                {
                    Logger.LogDebug(new LogRequest
                    {
                        OperationName = this.GetType().Name,
                        MethodName = "LoadLookupData",
                        Message = "Successfully retrieved Genders from lookup",
                        CustomParams = JsonConvert.SerializeObject(LookupService.Genders)
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(new LogRequest
                {
                    OperationName = this.GetType().Name,
                    MethodName = "LoadLookupData",
                    Message = "Error occurred while loading Genders lookup data",
                    Exception = ex
                });
            }

            //load TimeZones lookup data
            try
            {
                if (LookupService.TimeZones != null)
                {
                    Logger.LogDebug(new LogRequest
                    {
                        OperationName = this.GetType().Name,
                        MethodName = "LoadLookupData",
                        Message = "Successfully retrieved TimeZones from lookup",
                        CustomParams = JsonConvert.SerializeObject(LookupService.TimeZones)
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(new LogRequest
                {
                    OperationName = this.GetType().Name,
                    MethodName = "LoadLookupData",
                    Message = "Error occurred while loading TimeZones lookup data",
                    Exception = ex
                });
            }

            //load ClientTypes lookup data
            try
            {
                if (LookupService.ClientTypes != null)
                {
                    Logger.LogDebug(new LogRequest
                    {
                        OperationName = this.GetType().Name,
                        MethodName = "LoadLookupData",
                        Message = "Successfully retrieved ClientTypes from lookup",
                        CustomParams = JsonConvert.SerializeObject(LookupService.ClientTypes)
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(new LogRequest
                {
                    OperationName = this.GetType().Name,
                    MethodName = "LoadLookupData",
                    Message = "Error occurred while loading ClientTypes lookup data",
                    Exception = ex
                });
            }

            //load StaticRiskRatings lookup data
            try
            {
                if (LookupService.StaticRiskRatings != null)
                {
                    Logger.LogDebug(new LogRequest
                    {
                        OperationName = this.GetType().Name,
                        MethodName = "LoadLookupData",
                        Message = "Successfully retrieved StaticRiskRatings from lookup",
                        CustomParams = JsonConvert.SerializeObject(LookupService.StaticRiskRatings)
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(new LogRequest
                {
                    OperationName = this.GetType().Name,
                    MethodName = "LoadLookupData",
                    Message = "Error occurred while loading StaticRiskRatings lookup data",
                    Exception = ex
                });
            }
        }

        private string MapEthnicity(string automonRaceDescription, string automonRacePermDesc)
        {
            string mappedEthnicity = DAL.Constants.EthnicityUnknown;

            if(LookupService.Ethnicities != null && LookupService.Ethnicities.Any())
            {
                if(LookupService.Ethnicities.Any(e => e.Equals(automonRaceDescription, StringComparison.InvariantCultureIgnoreCase)))
                {
                    mappedEthnicity = automonRaceDescription;
                }
                else if (LookupService.Ethnicities.Any(e => e.Equals(automonRacePermDesc, StringComparison.InvariantCultureIgnoreCase)))
                {
                    mappedEthnicity = automonRacePermDesc;
                }
            }

            return mappedEthnicity;
        }

        private string MapCaseload(string automonCaseloadName)
        {
            if (LookupService.CaseLoads != null && LookupService.CaseLoads.Any(c => c.Name.Equals(automonCaseloadName, StringComparison.InvariantCultureIgnoreCase)))
            {
                return LookupService.CaseLoads.FirstOrDefault(c => c.Name.Equals(automonCaseloadName, StringComparison.InvariantCultureIgnoreCase)).Id.ToString();
            }

            return null;
        }

        private string MapSupervisingOfficer(string automonFirstName, string automonLastName, string automonEmailAddress)
        {
            if (
                LookupService.SupervisingOfficers != null 
                && LookupService.SupervisingOfficers.Any(s => 
                    s.FirstName.Equals(automonFirstName, StringComparison.InvariantCultureIgnoreCase) 
                    && s.LastName.Equals(automonLastName, StringComparison.InvariantCultureIgnoreCase) 
                    && s.Email.Equals(automonEmailAddress, StringComparison.InvariantCultureIgnoreCase)
                )
            )
            {
                return LookupService.SupervisingOfficers.FirstOrDefault(s => 
                    s.FirstName.Equals(automonFirstName, StringComparison.InvariantCultureIgnoreCase) 
                    && s.LastName.Equals(automonLastName, StringComparison.InvariantCultureIgnoreCase) 
                    && s.Email.Equals(automonEmailAddress, StringComparison.InvariantCultureIgnoreCase)
                ).Email;
            }

            return null;
        }

        private string MapStaticRiskRating(string automonDeptSupLevel)
        {
            //check if passed value is null or empty string, yes = return null
            if(string.IsNullOrEmpty(automonDeptSupLevel))
            {
                return null;
            }

            string nexusStaticRiskRating = Nexus.Service.StaticRiskRating.Unspecified;

            //try to convert Dept Sup Level from Automon into suitable Static Risk Rating of Nexus
            if(automonDeptSupLevel.Equals(Automon.Service.DeptSupLevel.Low))
            {
                nexusStaticRiskRating = Nexus.Service.StaticRiskRating.Low;
            }
            else if (automonDeptSupLevel.Equals(Automon.Service.DeptSupLevel.Medium))
            {
                nexusStaticRiskRating = Nexus.Service.StaticRiskRating.Medium;
            }
            else if(automonDeptSupLevel.Equals(Automon.Service.DeptSupLevel.HighD))
            {
                nexusStaticRiskRating = Nexus.Service.StaticRiskRating.HighDrug;
            }
            else if (automonDeptSupLevel.Equals(Automon.Service.DeptSupLevel.HighP))
            {
                nexusStaticRiskRating = Nexus.Service.StaticRiskRating.HighProperty;
            }
            else if (automonDeptSupLevel.Equals(Automon.Service.DeptSupLevel.HighV))
            {
                nexusStaticRiskRating = Nexus.Service.StaticRiskRating.HighViolence;
            }
            else if (automonDeptSupLevel.Equals(Automon.Service.DeptSupLevel.CMLow))
            {
                nexusStaticRiskRating = Nexus.Service.StaticRiskRating.CMLow;
            }
            else if (automonDeptSupLevel.Equals(Automon.Service.DeptSupLevel.CMMedium))
            {
                nexusStaticRiskRating = Nexus.Service.StaticRiskRating.CMMedium;
            }
            else if (automonDeptSupLevel.Equals(Automon.Service.DeptSupLevel.CMHigh))
            {
                nexusStaticRiskRating = Nexus.Service.StaticRiskRating.CMHigh;
            }
            else
            {
                nexusStaticRiskRating = Nexus.Service.StaticRiskRating.Unspecified;
            }

            // check if match can be found in lookup values
            if (LookupService.StaticRiskRatings != null && LookupService.StaticRiskRatings.Any(c => c.Equals(nexusStaticRiskRating, StringComparison.InvariantCultureIgnoreCase)))
            {
                return nexusStaticRiskRating;
            }

            //given value does not exists in lookup, set it as null.
            return null;
        }

        private string MapCmsStatus(string automonSupervisionStatus, string automonBodyStatus)
        {
            string nexusCmsStatus = string.Empty;

            //check if both values available, YES = concat them
            if(!string.IsNullOrEmpty(automonSupervisionStatus) && !string.IsNullOrEmpty(automonBodyStatus))
            {
                nexusCmsStatus = string.Format("{0} - {1}", automonSupervisionStatus, automonBodyStatus);
            }
            //check if only supervision status available, YES = set it as nexus cms status
            else if(!string.IsNullOrEmpty(automonSupervisionStatus))
            {
                nexusCmsStatus = automonSupervisionStatus;
            }
            //check if only body status available, YES = set it as nexus cms status
            else if (!string.IsNullOrEmpty(automonBodyStatus))
            {
                nexusCmsStatus = automonBodyStatus;
            }
            //no value is available, set nexus cms status as null
            else
            {
                nexusCmsStatus = null;
            }

            return nexusCmsStatus;
        }
    }
}
