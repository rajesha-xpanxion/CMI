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

        public void SaveOutboundMessages(IEnumerable<OutboundMessageDetails> outboundMessages)
        {
            if (outboundMessages != null && outboundMessages.Any())
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

                        dataTable.Columns.Add(UserDefinedTableTypeColumn.Id, typeof(int));
                        dataTable.Columns.Add(UserDefinedTableTypeColumn.ActivityTypeName, typeof(string));
                        dataTable.Columns.Add(UserDefinedTableTypeColumn.ActionReasonName, typeof(string));
                        dataTable.Columns.Add(UserDefinedTableTypeColumn.ClientIntegrationId, typeof(string));
                        dataTable.Columns.Add(UserDefinedTableTypeColumn.ActivityIdentifier, typeof(string));
                        dataTable.Columns.Add(UserDefinedTableTypeColumn.ActionOccurredOn, typeof(DateTime));
                        dataTable.Columns.Add(UserDefinedTableTypeColumn.ActionUpdatedBy, typeof(string));
                        dataTable.Columns.Add(UserDefinedTableTypeColumn.Details, typeof(string));

                        foreach(var outboundMessageDetails in outboundMessages)
                        {
                            dataTable.Rows.Add(
                                outboundMessageDetails.Id,
                                outboundMessageDetails.ActivityTypeName,
                                outboundMessageDetails.ActionReasonName,
                                outboundMessageDetails.ClientIntegrationId,
                                outboundMessageDetails.ActivityIdentifier,
                                outboundMessageDetails.ActionOccurredOn,
                                outboundMessageDetails.ActionUpdatedBy,
                                outboundMessageDetails.Details
                            );
                        }

                        cmd.Parameters.Add(new SqlParameter
                        {
                            ParameterName = SqlParamName.OutboundMessageTbl,
                            Value = dataTable,
                            SqlDbType = SqlDbType.Structured,
                            Direction = ParameterDirection.Input
                        });

                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
        #endregion
    }
}
