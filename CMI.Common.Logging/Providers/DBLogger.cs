using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace CMI.Common.Logging
{
    public class DBLogger : ILogger
    {
        private LogConfig logConfig;

        public DBLogger(Microsoft.Extensions.Options.IOptions<LogConfig> logConfig)
        {
            this.logConfig = logConfig.Value;
        }

        public void LogDebug(LogRequest logRequest)
        {
            if(logConfig.IsEnabled && logConfig.LogLevel <= LogLevel.Debug)
            {
                Log(LogLevel.Debug, logRequest);
            }
        }

        public void LogInfo(LogRequest logRequest)
        {
            if (logConfig.IsEnabled && logConfig.LogLevel <= LogLevel.Info)
            {
                Log(LogLevel.Info, logRequest);
            }
        }

        public void LogWarning(LogRequest logRequest)
        {
            if (logConfig.LogLevel <= LogLevel.Warning)
            {
                Log(LogLevel.Warning, logRequest);
            }
        }

        public void LogError(LogRequest logRequest)
        {
            if (logConfig.IsEnabled && logConfig.LogLevel <= LogLevel.Error)
            {
                Log(LogLevel.Error, logRequest);
            }
        }

        private async void Log(LogLevel logLevel, LogRequest logRequest)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(logConfig.DBConnString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = SQLQuery.SAVE_LOG_DETAILS;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = conn;

                        cmd.Parameters.Add(new SqlParameter() { ParameterName = SQLParamName.LOG_LEVEL, Value = logLevel.ToString(), SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input });

                        if (!string.IsNullOrEmpty(logRequest.OperationName))
                        {
                            cmd.Parameters.Add(new SqlParameter() { ParameterName = SQLParamName.OPERATION_NAME, Value = logRequest.OperationName, SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input });
                        }

                        if (!string.IsNullOrEmpty(logRequest.MethodName))
                        {
                            cmd.Parameters.Add(new SqlParameter() { ParameterName = SQLParamName.METHOD_NAME, Value = logRequest.MethodName, SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input });
                        }

                        if (logRequest.ErrorType.HasValue)
                        {
                            cmd.Parameters.Add(new SqlParameter() { ParameterName = SQLParamName.ERROR_TYPE, Value = logRequest.ErrorType.Value, SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Input });
                        }

                        if (logRequest.Exception != null && !string.IsNullOrEmpty(logRequest.Exception.Message))
                        {
                            cmd.Parameters.Add(new SqlParameter() { ParameterName = SQLParamName.MESSAGE, Value = logRequest.Exception.Message, SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input });
                        }
                        else if (!string.IsNullOrEmpty(logRequest.Message))
                        {
                            cmd.Parameters.Add(new SqlParameter() { ParameterName = SQLParamName.MESSAGE, Value = logRequest.Message, SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input });
                        }

                        if (logRequest.Exception != null && !string.IsNullOrEmpty(logRequest.Exception.StackTrace))
                        {
                            cmd.Parameters.Add(new SqlParameter() { ParameterName = SQLParamName.STACK_TRACE, Value = logRequest.Exception.StackTrace, SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input });
                        }

                        if (!string.IsNullOrEmpty(logRequest.CustomParams))
                        {
                            cmd.Parameters.Add(new SqlParameter() { ParameterName = SQLParamName.CUSTOM_PARAMS, Value = logRequest.CustomParams, SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input });
                        }

                        if (!string.IsNullOrEmpty(logRequest.SourceData))
                        {
                            cmd.Parameters.Add(new SqlParameter() { ParameterName = SQLParamName.SOURCE_DATA, Value = logRequest.SourceData, SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input });
                        }

                        if (!string.IsNullOrEmpty(logRequest.DestData))
                        {
                            cmd.Parameters.Add(new SqlParameter() { ParameterName = SQLParamName.DEST_DATA, Value = logRequest.DestData, SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input });
                        }

                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch(Exception ex)
            {
                string exMsg = ex.Message;
                string exStackTrc = ex.StackTrace;
            }
        }
    }
}
