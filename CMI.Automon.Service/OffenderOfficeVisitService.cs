using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using Microsoft.Extensions.Options;
using CMI.Automon.Interface;
using CMI.Automon.Model;

namespace CMI.Automon.Service
{
    public class OffenderOfficeVisitService : IOffenderOfficeVisitService
    {
        #region Private Member Variables
        private readonly AutomonConfig automonConfig;
        #endregion

        #region Constructor
        public OffenderOfficeVisitService(
            IOptions<AutomonConfig> automonConfig
        )
        {
            this.automonConfig = automonConfig.Value;
        }
        #endregion

        public int SaveOffenderOfficeVisitDetails(string CmiDbConnString, OffenderOfficeVisit offenderOfficeVisitDetails)
        {
            if (automonConfig.IsDevMode)
            {
                string testDataJsonFileName = Path.Combine(automonConfig.TestDataJsonRepoPath, Constants.TestDataJsonFileNameAllOffenderOfficeVisitDetails);

                //check if repository parent directory exists, if not then create
                if (!Directory.Exists(automonConfig.TestDataJsonRepoPath))
                {
                    Directory.CreateDirectory(automonConfig.TestDataJsonRepoPath);
                }

                //read existing objects
                List<OffenderOfficeVisit> offenderOfficeVisitDetailsList = File.Exists(testDataJsonFileName)
                    ? JsonConvert.DeserializeObject<List<OffenderOfficeVisit>>(File.ReadAllText(testDataJsonFileName))
                    : new List<OffenderOfficeVisit>();

                //merge
                offenderOfficeVisitDetailsList.Add(offenderOfficeVisitDetails);

                //write back
                File.WriteAllText(testDataJsonFileName, JsonConvert.SerializeObject(offenderOfficeVisitDetailsList));

                return offenderOfficeVisitDetails.Id == 0 ? new Random().Next(0, 10000) : offenderOfficeVisitDetails.Id;
            }
            else
            {
                using (SqlConnection conn = new SqlConnection(CmiDbConnString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = StoredProc.SaveOffenderOfficeVisitDetails;
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.AutomonDatabaseName,
                            SqlDbType = System.Data.SqlDbType.NVarChar,
                            Value = new SqlConnectionStringBuilder(automonConfig.AutomonDbConnString).InitialCatalog
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.Pin,
                            SqlDbType = System.Data.SqlDbType.VarChar,
                            Value = offenderOfficeVisitDetails.Pin,
                            IsNullable = false
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.Id,
                            SqlDbType = System.Data.SqlDbType.Int,
                            Value = offenderOfficeVisitDetails.Id,
                            IsNullable = true
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.UpdatedBy,
                            SqlDbType = System.Data.SqlDbType.VarChar,
                            Value = offenderOfficeVisitDetails.UpdatedBy,
                            IsNullable = false
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.StartDate,
                            SqlDbType = System.Data.SqlDbType.DateTime,
                            Value = offenderOfficeVisitDetails.StartDate,
                            IsNullable = false
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.Comment,
                            SqlDbType = System.Data.SqlDbType.VarChar,
                            Value = offenderOfficeVisitDetails.Comment,
                            IsNullable = false
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.EndDate,
                            SqlDbType = System.Data.SqlDbType.DateTime,
                            Value = offenderOfficeVisitDetails.EndDate,
                            IsNullable = false
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.Status,
                            SqlDbType = System.Data.SqlDbType.Int,
                            Value = offenderOfficeVisitDetails.Status,
                            IsNullable = false
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.IsOffenderPresent,
                            SqlDbType = System.Data.SqlDbType.Bit,
                            Value = offenderOfficeVisitDetails.IsOffenderPresent,
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
