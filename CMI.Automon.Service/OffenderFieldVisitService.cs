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
    public class OffenderFieldVisitService : IOffenderFieldVisitService
    {
        #region Private Member Variables
        private readonly AutomonConfig automonConfig;
        #endregion

        #region Constructor
        public OffenderFieldVisitService(
            IOptions<AutomonConfig> automonConfig
        )
        {
            this.automonConfig = automonConfig.Value;
        }
        #endregion

        public int SaveOffenderFieldVisitDetails(string CmiDbConnString, OffenderFieldVisit offenderFieldVisitDetails)
        {
            if (automonConfig.IsDevMode)
            {
                string testDataJsonFileName = Path.Combine(automonConfig.TestDataJsonRepoPath, Constants.TestDataJsonFileNameAllOffenderFieldVisitDetails);

                //check if repository parent directory exists, if not then create
                if (!Directory.Exists(automonConfig.TestDataJsonRepoPath))
                {
                    Directory.CreateDirectory(automonConfig.TestDataJsonRepoPath);
                }

                //read existing objects
                List<OffenderFieldVisit> offenderFieldVisitDetailsList = File.Exists(testDataJsonFileName)
                    ? JsonConvert.DeserializeObject<List<OffenderFieldVisit>>(File.ReadAllText(testDataJsonFileName))
                    : new List<OffenderFieldVisit>();

                //merge
                offenderFieldVisitDetailsList.Add(offenderFieldVisitDetails);

                //write back
                File.WriteAllText(testDataJsonFileName, JsonConvert.SerializeObject(offenderFieldVisitDetailsList));

                return offenderFieldVisitDetails.Id == 0 ? new Random().Next(0, 10000) : offenderFieldVisitDetails.Id;
            }
            else
            {
                using (SqlConnection conn = new SqlConnection(CmiDbConnString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = StoredProc.SaveOffenderFieldVisitDetails;
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
                            Value = offenderFieldVisitDetails.Pin,
                            IsNullable = false
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.Id,
                            SqlDbType = System.Data.SqlDbType.Int,
                            Value = offenderFieldVisitDetails.Id,
                            IsNullable = true
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.UpdatedBy,
                            SqlDbType = System.Data.SqlDbType.VarChar,
                            Value = offenderFieldVisitDetails.UpdatedBy,
                            IsNullable = false
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.StartDate,
                            SqlDbType = System.Data.SqlDbType.DateTime,
                            Value = offenderFieldVisitDetails.StartDate,
                            IsNullable = false
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.Comment,
                            SqlDbType = System.Data.SqlDbType.VarChar,
                            Value = offenderFieldVisitDetails.Comment,
                            IsNullable = false
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.EndDate,
                            SqlDbType = System.Data.SqlDbType.DateTime,
                            Value = offenderFieldVisitDetails.EndDate,
                            IsNullable = false
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.Status,
                            SqlDbType = System.Data.SqlDbType.Int,
                            Value = offenderFieldVisitDetails.Status,
                            IsNullable = false
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.IsOffenderPresent,
                            SqlDbType = System.Data.SqlDbType.Bit,
                            Value = offenderFieldVisitDetails.IsOffenderPresent,
                            IsNullable = false
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.IsSearchConducted,
                            SqlDbType = System.Data.SqlDbType.Bit,
                            Value = offenderFieldVisitDetails.IsSearchConducted,
                            IsNullable = false
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.SearchLocations,
                            SqlDbType = System.Data.SqlDbType.VarChar,
                            Value = offenderFieldVisitDetails.SearchLocations,
                            IsNullable = false
                        });
                        if (!string.IsNullOrEmpty(offenderFieldVisitDetails.SearchResults))
                        {
                            cmd.Parameters.Add(new SqlParameter()
                            {
                                ParameterName = SqlParamName.SearchResults,
                                SqlDbType = System.Data.SqlDbType.VarChar,
                                Value = offenderFieldVisitDetails.SearchResults,
                                IsNullable = true
                            });
                        }

                        cmd.Connection = conn;

                        return Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }
        }
    }
}
