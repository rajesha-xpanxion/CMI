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
    public class OffenderService : IOffenderService
    {
        #region Private Member Variables
        private readonly AutomonConfig automonConfig;
        #endregion

        #region Constructor
        public OffenderService(
            IOptions<AutomonConfig> automonConfig
        )
        {
            this.automonConfig = automonConfig.Value;
        }
        #endregion

        #region Public Methods
        public IEnumerable<Offender> GetAllOffenderDetails(string CmiDbConnString, DateTime? lastExecutionDateTime)
        {
            string timeZone = GetTimeZone();

            if (automonConfig.IsDevMode)
            {
                //test data
                string testDataJsonFileName = Path.Combine(automonConfig.TestDataJsonRepoPath, Constants.TestDataJsonFileNameAllOffenderDetails);

                return File.Exists(testDataJsonFileName)
                    ? JsonConvert.DeserializeObject<IEnumerable<Offender>>(File.ReadAllText(testDataJsonFileName))
                    : new List<Offender>();
            }
            else
            {
                List<Offender> offenders = new List<Offender>();

                using (SqlConnection conn = new SqlConnection(CmiDbConnString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = StoredProc.GetAllOffenderDetails;
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.AutomonDatabaseName,
                            SqlDbType = System.Data.SqlDbType.NVarChar,
                            Value = new SqlConnectionStringBuilder(automonConfig.AutomonDbConnString).InitialCatalog
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.LastExecutionDateTime,
                            SqlDbType = System.Data.SqlDbType.DateTime,
                            Value = lastExecutionDateTime.HasValue ? lastExecutionDateTime.Value : (object)DBNull.Value,
                            IsNullable = true
                        });
                        cmd.Connection = conn;

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                offenders.Add(new Offender()
                                {
                                    Pin = Convert.ToString(reader[DbColumnName.Pin]),
                                    FirstName = Convert.ToString(reader[DbColumnName.FirstName]),
                                    MiddleName = Convert.ToString(reader[DbColumnName.MiddleName]),
                                    LastName = Convert.ToString(reader[DbColumnName.LastName]),
                                    DateOfBirth = Convert.ToDateTime(reader[DbColumnName.DateOfBirth]),
                                    ClientType = Convert.ToString(reader[DbColumnName.ClientType]),
                                    TimeZone = timeZone,
                                    Gender = Convert.ToString(reader[DbColumnName.Gender]),
                                    Race = Convert.ToString(reader[DbColumnName.Race]),
                                    CaseloadName = Convert.ToString(reader[DbColumnName.CaseloadName]),
                                    OfficerLogon = Convert.ToString(reader[DbColumnName.OfficerLogon]),
                                    OfficerEmail = Convert.ToString(reader[DbColumnName.OfficerEmail]),
                                    OfficerFirstName = Convert.ToString(reader[DbColumnName.OfficerFirstName]),
                                    OfficerLastName = Convert.ToString(reader[DbColumnName.OfficerLastName])
                                });
                            }
                        }
                    }
                }

                return offenders;
            }
        }

        public void SaveOffenderPersonalDetails(string CmiDbConnString, Offender offenderDetails)
        {
            if (automonConfig.IsDevMode)
            {
                //test data
                //string testDataJsonFileName = Path.Combine(automonConfig.TestDataJsonRepoPath, Constants.TestDataJsonFileNameAllOffenderNoteDetails);

                //return File.Exists(testDataJsonFileName)
                //    ? JsonConvert.DeserializeObject<IEnumerable<OffenderNote>>(File.ReadAllText(testDataJsonFileName))
                //    : new List<OffenderNote>();
            }
            else
            {
                using (SqlConnection conn = new SqlConnection(CmiDbConnString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = StoredProc.SaveOffenderPersonalDetails;
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
                            Value = offenderDetails.Pin,
                            IsNullable = false
                        });
                        
                        

                        cmd.Connection = conn;

                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
        #endregion

        #region Private Helper Methods
        private string GetTimeZone()
        {
            if (automonConfig.IsDevMode)
            {
                //test data
                return "Pacific Standard Time";
            }
            else
            {
                string timeZone = string.Empty;

                using (SqlConnection conn = new SqlConnection(automonConfig.AutomonDbConnString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = SqlQuery.GetTimeZone;
                        cmd.CommandType = System.Data.CommandType.Text;
                        cmd.Connection = conn;

                        timeZone = Convert.ToString(cmd.ExecuteScalar());
                    }
                }

                return timeZone;
            }
        }
        #endregion
    }
}
