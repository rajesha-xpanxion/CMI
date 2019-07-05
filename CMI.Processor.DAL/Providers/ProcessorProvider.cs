using System;
using System.Data.SqlClient;
using System.Data;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using Newtonsoft.Json;
using System.IO;

namespace CMI.Processor.DAL
{
    public class ProcessorProvider : IProcessorProvider
    {
        #region Private Member Variables
        private readonly ProcessorConfig processorConfig;
        #endregion

        #region Constructor
        public ProcessorProvider(
            IOptions<ProcessorConfig> processorConfig
        )
        {
            this.processorConfig = processorConfig.Value;
        }
        #endregion

        #region Public Methods
        public DateTime? GetLastExecutionDateTime(ProcessorType processorType = ProcessorType.Inbound)
        {
            DateTime? lastExecutionDateTime = null;

            using (SqlConnection conn = new SqlConnection(processorConfig.CmiDbConnString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = StoredProc.GetLastExecutionDateTime;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = conn;

                    cmd.Parameters.Add(new SqlParameter
                    {
                        ParameterName = SqlParamName.ProcessorTypeId,
                        Value = processorType,
                        SqlDbType = SqlDbType.Int,
                        Direction = ParameterDirection.Input
                    });

                    object objLastExecutionDateTime = cmd.ExecuteScalar();

                    if(!Convert.IsDBNull(objLastExecutionDateTime))
                    {
                        lastExecutionDateTime = (DateTime)objLastExecutionDateTime;
                    }
                }
            }

            return lastExecutionDateTime;
        }

        public void SaveExecutionStatus(ExecutionStatus executionStatus)
        {
            if (executionStatus != null)
            {
                using (SqlConnection conn = new SqlConnection(processorConfig.CmiDbConnString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = StoredProc.SaveExecutionStatus;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = conn;

                        cmd.Parameters.Add(new SqlParameter
                        {
                            ParameterName = SqlParamName.ProcessorTypeId,
                            Value = executionStatus.ProcessorType,
                            SqlDbType = SqlDbType.Int,
                            Direction = ParameterDirection.Input
                        });
                        cmd.Parameters.Add(new SqlParameter
                        {
                            ParameterName = SqlParamName.ExecutedOn,
                            Value = executionStatus.ExecutedOn,
                            SqlDbType = SqlDbType.DateTime,
                            Direction = ParameterDirection.Input
                        });
                        cmd.Parameters.Add(new SqlParameter
                        {
                            ParameterName = SqlParamName.IsSuccessful,
                            Value = executionStatus.IsSuccessful,
                            SqlDbType = SqlDbType.Bit,
                            Direction = ParameterDirection.Input
                        });
                        cmd.Parameters.Add(new SqlParameter
                        {
                            ParameterName = SqlParamName.NumTaskProcessed,
                            Value = executionStatus.NumTaskProcessed,
                            SqlDbType = SqlDbType.Int,
                            Direction = ParameterDirection.Input
                        });
                        cmd.Parameters.Add(new SqlParameter
                        {
                            ParameterName = SqlParamName.NumTaskSucceeded,
                            Value = executionStatus.NumTaskSucceeded,
                            SqlDbType = SqlDbType.Int,
                            Direction = ParameterDirection.Input
                        });
                        cmd.Parameters.Add(new SqlParameter
                        {
                            ParameterName = SqlParamName.NumTaskFailed,
                            Value = executionStatus.NumTaskFailed,
                            SqlDbType = SqlDbType.Int,
                            Direction = ParameterDirection.Input
                        });
                        if (!string.IsNullOrEmpty(executionStatus.ExecutionStatusMessage))
                        {
                            cmd.Parameters.Add(new SqlParameter
                            {
                                ParameterName = SqlParamName.Message,
                                Value = executionStatus.ExecutionStatusMessage,
                                SqlDbType = SqlDbType.NVarChar,
                                Direction = ParameterDirection.Input
                            });
                        }
                        if (!string.IsNullOrEmpty(executionStatus.ErrorDetails))
                        {
                            cmd.Parameters.Add(new SqlParameter
                            {
                                ParameterName = SqlParamName.ErrorDetails,
                                Value = executionStatus.ErrorDetails,
                                SqlDbType = SqlDbType.NVarChar,
                                Direction = ParameterDirection.Input
                            });
                        }

                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        public IEnumerable<OutboundMessageDetails> SaveOutboundMessagesToDatabase(IEnumerable<OutboundMessageDetails> receivedOutboundMessages)
        {
            List<OutboundMessageDetails> outboundMessages = new List<OutboundMessageDetails>();

            using (SqlConnection conn = new SqlConnection(processorConfig.CmiDbConnString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = StoredProc.SaveOutboundMessages;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = conn;

                    var dataTable = new DataTable(UserDefinedTableType.OutboundMessageTbl)
                    {
                        Locale = CultureInfo.InvariantCulture
                    };

                    dataTable.Columns.Add(TableColumnName.Id, typeof(int));
                    dataTable.Columns.Add(TableColumnName.ActivityTypeName, typeof(string));
                    dataTable.Columns.Add(TableColumnName.ActivitySubTypeName, typeof(string));
                    dataTable.Columns.Add(TableColumnName.ActionReasonName, typeof(string));
                    dataTable.Columns.Add(TableColumnName.ClientIntegrationId, typeof(string));
                    dataTable.Columns.Add(TableColumnName.ActivityIdentifier, typeof(string));
                    dataTable.Columns.Add(TableColumnName.ActionOccurredOn, typeof(DateTime));
                    dataTable.Columns.Add(TableColumnName.ActionUpdatedBy, typeof(string));
                    dataTable.Columns.Add(TableColumnName.Details, typeof(string));
                    dataTable.Columns.Add(TableColumnName.IsSuccessful, typeof(bool));
                    dataTable.Columns.Add(TableColumnName.ErrorDetails, typeof(string));
                    dataTable.Columns.Add(TableColumnName.RawData, typeof(string));
                    dataTable.Columns.Add(TableColumnName.IsProcessed, typeof(bool));
                    dataTable.Columns.Add(TableColumnName.ReceivedOn, typeof(DateTime));
                    dataTable.Columns.Add(TableColumnName.AutomonIdentifier, typeof(string));

                    //check for null & check if any record to process
                    if (receivedOutboundMessages != null && receivedOutboundMessages.Any())
                    {
                        foreach (var outboundMessageDetails in receivedOutboundMessages)
                        {
                            dataTable.Rows.Add(
                                outboundMessageDetails.Id,
                                outboundMessageDetails.ActivityTypeName,
                                outboundMessageDetails.ActivitySubTypeName,
                                outboundMessageDetails.ActionReasonName,
                                outboundMessageDetails.ClientIntegrationId,
                                outboundMessageDetails.ActivityIdentifier,
                                outboundMessageDetails.ActionOccurredOn,
                                outboundMessageDetails.ActionUpdatedBy,
                                outboundMessageDetails.Details,
                                outboundMessageDetails.IsSuccessful,
                                outboundMessageDetails.ErrorDetails,
                                outboundMessageDetails.RawData,
                                outboundMessageDetails.IsProcessed,
                                outboundMessageDetails.ReceivedOn,
                                string.IsNullOrEmpty(outboundMessageDetails.AutomonIdentifier) ? null : outboundMessageDetails.AutomonIdentifier
                            );
                        }
                    }
                    cmd.Parameters.Add(new SqlParameter
                    {
                        ParameterName = SqlParamName.OutboundMessageTbl,
                        Value = dataTable,
                        SqlDbType = SqlDbType.Structured,
                        Direction = ParameterDirection.Input
                    });

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            outboundMessages.Add(new OutboundMessageDetails
                            {
                                Id = Convert.ToInt32(reader[TableColumnName.Id]),
                                ActivityTypeName = Convert.ToString(reader[TableColumnName.ActivityTypeName]),
                                ActivitySubTypeName = Convert.ToString(reader[TableColumnName.ActivitySubTypeName]),
                                ActionReasonName = Convert.ToString(reader[TableColumnName.ActionReasonName]),
                                ClientIntegrationId = Convert.ToString(reader[TableColumnName.ClientIntegrationId]),
                                ActivityIdentifier = Convert.ToString(reader[TableColumnName.ActivityIdentifier]),
                                ActionOccurredOn = Convert.ToDateTime(reader[TableColumnName.ActionOccurredOn]),
                                ActionUpdatedBy = Convert.ToString(reader[TableColumnName.ActionUpdatedBy]),
                                Details = Convert.ToString(reader[TableColumnName.Details]),
                                IsSuccessful = Convert.ToBoolean(reader[TableColumnName.IsSuccessful]),
                                ErrorDetails = Convert.ToString(reader[TableColumnName.ErrorDetails]),
                                RawData = Convert.ToString(reader[TableColumnName.RawData]),
                                ReceivedOn = Convert.ToDateTime(reader[TableColumnName.ReceivedOn]),
                                AutomonIdentifier = Convert.ToString(reader[TableColumnName.AutomonIdentifier])
                            });
                        }
                    }
                }
            }

            return outboundMessages;
        }

        public IEnumerable<OutboundMessageDetails> GetOutboundMessagesFromDisk()
        {
            //check if file exist for failed outbound messages
            if(File.Exists(processorConfig.OutboundProcessorConfig.SecondaryStorageRepositoryFileFullPath))
            {
                //read messages from file
                IEnumerable<OutboundMessageDetails> outboundMessages = JsonConvert.DeserializeObject<IEnumerable<OutboundMessageDetails>>(
                    File.ReadAllText(processorConfig.OutboundProcessorConfig.SecondaryStorageRepositoryFileFullPath)
                );

                //delete file as it will no longer be required
                File.Delete(processorConfig.OutboundProcessorConfig.SecondaryStorageRepositoryFileFullPath);

                return outboundMessages;
            }

            return new List<OutboundMessageDetails>();
        }

        public void SaveOutboundMessagesToDisk(IEnumerable<OutboundMessageDetails> outboundMessages)
        {
            FileInfo fileInfo = new FileInfo(processorConfig.OutboundProcessorConfig.SecondaryStorageRepositoryFileFullPath);

            //check if repository parent directory exists, if not then create
            if(!Directory.Exists(fileInfo.DirectoryName))
            {
                Directory.CreateDirectory(fileInfo.DirectoryName);
            }

            //write message by serializing into file
            File.WriteAllText(processorConfig.OutboundProcessorConfig.SecondaryStorageRepositoryFileFullPath, JsonConvert.SerializeObject(outboundMessages));
        }
        #endregion
    }
}
