using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace CMI.Processor.DAL
{
    public class ProcessorProvider : IProcessorProvider
    {
        private ProcessorConfig processorConfig;

        public ProcessorProvider(Microsoft.Extensions.Options.IOptions<ProcessorConfig> processorConfig)
        {
            this.processorConfig = processorConfig.Value;
        }

        public DateTime GetLastExecutionDateTime()
        {
            DateTime lastExecutionDateTime = DateTime.Now.AddDays(-1);

            using (SqlConnection conn = new SqlConnection(processorConfig.CMIDBConnString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = SQLQuery.GET_LAST_EXECUTION_DATE_TIME;
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
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
                using (SqlConnection conn = new SqlConnection(processorConfig.CMIDBConnString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = SQLQuery.SAVE_EXECUTION_STATUS;
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Connection = conn;

                        cmd.Parameters.Add(new SqlParameter() { ParameterName = SQLParamName.EXECUTED_ON, Value = executionStatus.ExecutedOn, SqlDbType = System.Data.SqlDbType.DateTime, Direction = System.Data.ParameterDirection.Input });

                        cmd.Parameters.Add(new SqlParameter() { ParameterName = SQLParamName.IS_SUCCESSFUL, Value = executionStatus.IsSuccessful, SqlDbType = System.Data.SqlDbType.Bit, Direction = System.Data.ParameterDirection.Input });

                        if (!string.IsNullOrEmpty(executionStatus.ExecutionStatusMessage))
                        {
                            cmd.Parameters.Add(new SqlParameter() { ParameterName = SQLParamName.MESSAGE, Value = executionStatus.ExecutionStatusMessage, SqlDbType = System.Data.SqlDbType.NVarChar, Direction = System.Data.ParameterDirection.Input });
                        }

                        if (!string.IsNullOrEmpty(executionStatus.ErrorDetails))
                        {
                            cmd.Parameters.Add(new SqlParameter() { ParameterName = SQLParamName.ERROR_DETAILS, Value = executionStatus.ErrorDetails, SqlDbType = System.Data.SqlDbType.NVarChar, Direction = System.Data.ParameterDirection.Input });
                        }

                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}
