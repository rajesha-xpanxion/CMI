using System;
using System.Data.SqlClient;
using System.Data;
using Microsoft.Extensions.Options;

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
        public DateTime? GetLastExecutionDateTime()
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
        #endregion
    }
}
