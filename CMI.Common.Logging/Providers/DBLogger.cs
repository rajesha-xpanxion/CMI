using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace CMI.Common.Logging
{
    public class DbLogger : ILogger
    {
        #region Private Member Variables
        private readonly LogConfig logConfig;
        #endregion

        #region Constructor
        public DbLogger(
            IOptions<LogConfig> logConfig
        )
        {
            this.logConfig = logConfig.Value;
        }
        #endregion

        #region Public Methods
        public async void LogDebug(LogRequest logRequest)
        {
            if(logConfig.IsEnabled && logConfig.LogLevel <= LogLevel.Debug)
            {
                await Log(LogLevel.Debug, logRequest);
            }
        }

        public async void LogInfo(LogRequest logRequest)
        {
            if (logConfig.IsEnabled && logConfig.LogLevel <= LogLevel.Info)
            {
                await Log(LogLevel.Info, logRequest);
            }
        }

        public async void LogWarning(LogRequest logRequest)
        {
            if (logConfig.IsEnabled && logConfig.LogLevel <= LogLevel.Warning)
            {
                await Log(LogLevel.Warning, logRequest);
            }
        }

        public async void LogError(LogRequest logRequest)
        {
            if (logConfig.IsEnabled && logConfig.LogLevel <= LogLevel.Error)
            {
                await Log(LogLevel.Error, logRequest);
            }
        }
        #endregion

        #region Private Helper Methods
        private async Task Log(LogLevel logLevel, LogRequest logRequest)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(logConfig.DatabaseConnectionString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = SqlQuery.SaveLogDetails;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = conn;

                        cmd.Parameters.Add(new SqlParameter { ParameterName = SqlParamName.LogLevel, Value = logLevel.ToString(), SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input });

                        if (!string.IsNullOrEmpty(logRequest.OperationName))
                        {
                            cmd.Parameters.Add(new SqlParameter { ParameterName = SqlParamName.OperationName, Value = logRequest.OperationName, SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input });
                        }

                        if (!string.IsNullOrEmpty(logRequest.MethodName))
                        {
                            cmd.Parameters.Add(new SqlParameter { ParameterName = SqlParamName.MethodName, Value = logRequest.MethodName, SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input });
                        }

                        if (logRequest.ErrorType.HasValue)
                        {
                            cmd.Parameters.Add(new SqlParameter { ParameterName = SqlParamName.ErrorType, Value = logRequest.ErrorType.Value, SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Input });
                        }

                        if (logRequest.Exception != null && !string.IsNullOrEmpty(logRequest.Exception.Message))
                        {
                            cmd.Parameters.Add(new SqlParameter { ParameterName = SqlParamName.Message, Value = logRequest.Exception.Message, SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input });
                        }
                        else if (!string.IsNullOrEmpty(logRequest.Message))
                        {
                            cmd.Parameters.Add(new SqlParameter { ParameterName = SqlParamName.Message, Value = logRequest.Message, SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input });
                        }

                        if (logRequest.Exception != null && !string.IsNullOrEmpty(logRequest.Exception.StackTrace))
                        {
                            cmd.Parameters.Add(new SqlParameter { ParameterName = SqlParamName.StackTrace, Value = logRequest.Exception.StackTrace, SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input });
                        }

                        if (!string.IsNullOrEmpty(logRequest.CustomParams))
                        {
                            cmd.Parameters.Add(new SqlParameter { ParameterName = SqlParamName.CustomParams, Value = logRequest.CustomParams, SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input });
                        }

                        if (!string.IsNullOrEmpty(logRequest.AutomonData))
                        {
                            cmd.Parameters.Add(new SqlParameter { ParameterName = SqlParamName.AutomonData, Value = logRequest.AutomonData, SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input });
                        }

                        if (!string.IsNullOrEmpty(logRequest.NexusData))
                        {
                            cmd.Parameters.Add(new SqlParameter { ParameterName = SqlParamName.NexusData, Value = logRequest.NexusData, SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input });
                        }

                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("{0}Error occurred in database logging:{0}{1}{0}", Environment.NewLine, ex.ToString());
            }
        }
        #endregion
    }
}
