using System.Data.SqlClient;
using Microsoft.Extensions.Options;
using CMI.Automon.Interface;
using CMI.Automon.Model;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;
using System.Data;

namespace CMI.Automon.Service
{
    public class OffenderTouchPointCheckInService : IOffenderTouchPointCheckInService
    {
        #region Private Member Variables
        private readonly AutomonConfig automonConfig;
        #endregion

        #region Constructor
        public OffenderTouchPointCheckInService(
            IOptions<AutomonConfig> automonConfig
        )
        {
            this.automonConfig = automonConfig.Value;
        }
        #endregion

        public int SaveOffenderTouchPointCheckInDetails(string CmiDbConnString, OffenderTouchPointCheckIn offenderTouchPointCheckInDetails)
        {
            if (automonConfig.IsDevMode)
            {
                string testDataJsonFileName = Path.Combine(automonConfig.TestDataJsonRepoPath, Constants.TestDataJsonFileNameAllOffenderTouchPointCheckInDetails);

                //check if repository parent directory exists, if not then create
                if (!Directory.Exists(automonConfig.TestDataJsonRepoPath))
                {
                    Directory.CreateDirectory(automonConfig.TestDataJsonRepoPath);
                }

                //read existing objects
                List<OffenderTouchPointCheckIn> offenderTouchPointCheckInDetailsList = File.Exists(testDataJsonFileName)
                    ? JsonConvert.DeserializeObject<List<OffenderTouchPointCheckIn>>(File.ReadAllText(testDataJsonFileName))
                    : new List<OffenderTouchPointCheckIn>();

                //merge
                offenderTouchPointCheckInDetailsList.Add(offenderTouchPointCheckInDetails);

                //write back
                File.WriteAllText(testDataJsonFileName, JsonConvert.SerializeObject(offenderTouchPointCheckInDetailsList));

                return offenderTouchPointCheckInDetails.Id == 0 ? new Random().Next(0, 10000) : offenderTouchPointCheckInDetails.Id;
            }
            else
            {
                using (SqlConnection conn = new SqlConnection(CmiDbConnString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = StoredProc.SaveOffenderTouchPointCheckInDetails;
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.AutomonDatabaseName,
                            SqlDbType = SqlDbType.NVarChar,
                            Value = new SqlConnectionStringBuilder(automonConfig.AutomonDbConnString).InitialCatalog
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.Pin,
                            SqlDbType = SqlDbType.VarChar,
                            Value = offenderTouchPointCheckInDetails.Pin,
                            IsNullable = false
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.Id,
                            SqlDbType = SqlDbType.Int,
                            Value = offenderTouchPointCheckInDetails.Id,
                            IsNullable = true
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.UpdatedBy,
                            SqlDbType = SqlDbType.VarChar,
                            Value = offenderTouchPointCheckInDetails.UpdatedBy,
                            IsNullable = false
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.StartDate,
                            SqlDbType = SqlDbType.DateTime,
                            Value = offenderTouchPointCheckInDetails.StartDate,
                            IsNullable = false
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.Comment,
                            SqlDbType = SqlDbType.VarChar,
                            Value = offenderTouchPointCheckInDetails.Comment,
                            IsNullable = false
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.EndDate,
                            SqlDbType = SqlDbType.DateTime,
                            Value = offenderTouchPointCheckInDetails.EndDate,
                            IsNullable = false
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.Status,
                            SqlDbType = SqlDbType.Int,
                            Value = offenderTouchPointCheckInDetails.Status,
                            IsNullable = false
                        });

                        cmd.Connection = conn;

                        return Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }
        }
    }
}
