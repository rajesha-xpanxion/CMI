using System;
using System.Data.SqlClient;
using System.Data;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

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

        public IEnumerable<OutboundMessageDetails> SaveOutboundMessages(IEnumerable<OutboundMessageDetails> receivedOutboundMessages, DateTime receivedOn)
        {
            List<OutboundMessageDetails> updatedOutboundMessages = new List<OutboundMessageDetails>();

            //check if any outbound message received
            if (receivedOutboundMessages != null && receivedOutboundMessages.Any())
            {
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
                                outboundMessageDetails.IsProcessed
                            );
                        }

                        cmd.Parameters.Add(new SqlParameter
                        {
                            ParameterName = SqlParamName.OutboundMessageTbl,
                            Value = dataTable,
                            SqlDbType = SqlDbType.Structured,
                            Direction = ParameterDirection.Input
                        });
                        cmd.Parameters.Add(new SqlParameter
                        {
                            ParameterName = SqlParamName.ReceivedOn,
                            Value = receivedOn,
                            SqlDbType = SqlDbType.DateTime,
                            Direction = ParameterDirection.Input
                        });

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                updatedOutboundMessages.Add(new OutboundMessageDetails
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
                                    RawData = Convert.ToString(reader[TableColumnName.RawData])
                                });
                            }
                        }
                    }
                }
            }

            return updatedOutboundMessages;
        }

        public IEnumerable<OutboundMessageDetails> GetFailedOutboundMessages()
        {
            List<OutboundMessageDetails> failedOutboundMessages = new List<OutboundMessageDetails>();


            using (SqlConnection conn = new SqlConnection(processorConfig.CmiDbConnString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = StoredProc.GetFailedOutboundMessages;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = conn;

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            failedOutboundMessages.Add(new OutboundMessageDetails
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
                                IsProcessed = Convert.ToBoolean(reader[TableColumnName.IsProcessed])
                            });
                        }
                    }
                }
            }


            return failedOutboundMessages;
        }
        #endregion
    }
}
