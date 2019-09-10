using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using Microsoft.Extensions.Options;
using CMI.Automon.Interface;
using CMI.Automon.Model;
using System.Data;

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
        public IEnumerable<Offender> GetAllOffenderDetails(string CmiDbConnString, DateTime? lastExecutionDateTime, DataTable officerLogonsToFilterTbl)
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
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.AutomonDatabaseName,
                            SqlDbType = SqlDbType.NVarChar,
                            Value = new SqlConnectionStringBuilder(automonConfig.AutomonDbConnString).InitialCatalog
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.LastExecutionDateTime,
                            SqlDbType = SqlDbType.DateTime,
                            Value = lastExecutionDateTime.HasValue ? lastExecutionDateTime.Value : (object)DBNull.Value,
                            IsNullable = true
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.OfficerLogonsToFilterTbl,
                            SqlDbType = SqlDbType.Structured,
                            Value = officerLogonsToFilterTbl,
                            Direction = ParameterDirection.Input
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
                                    RaceDescription = Convert.ToString(reader[DbColumnName.RaceDescription]),
                                    RacePermDesc = Convert.ToString(reader[DbColumnName.RacePermDesc]),
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

        public string SaveOffenderDetails(string CmiDbConnString, OffenderDetails offenderDetails)
        {
            string pin = string.Empty;

            if (automonConfig.IsDevMode)
            {
                string testDataJsonFileName = Path.Combine(automonConfig.TestDataJsonRepoPath, Constants.TestDataJsonFileNameAllOffenderDetails);

                //check if repository parent directory exists, if not then create
                if (!Directory.Exists(automonConfig.TestDataJsonRepoPath))
                {
                    Directory.CreateDirectory(automonConfig.TestDataJsonRepoPath);
                }

                //read existing objects
                List<OffenderDetails> offenderDetailsList = File.Exists(testDataJsonFileName)
                    ? JsonConvert.DeserializeObject<List<OffenderDetails>>(File.ReadAllText(testDataJsonFileName))
                    : new List<OffenderDetails>();

                //merge
                offenderDetailsList.Add(offenderDetails);

                //write back
                File.WriteAllText(testDataJsonFileName, JsonConvert.SerializeObject(offenderDetailsList));

                pin = string.IsNullOrEmpty(offenderDetails.Pin) ? new Random().Next(0, 10000).ToString() : offenderDetails.Pin;
            }
            else
            {
                using (SqlConnection conn = new SqlConnection(CmiDbConnString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = StoredProc.SaveNewOffender;
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
                            IsNullable = true
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.FirstName,
                            SqlDbType = System.Data.SqlDbType.VarChar,
                            Value = offenderDetails.FirstName,
                            IsNullable = true
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.MiddleName,
                            SqlDbType = System.Data.SqlDbType.VarChar,
                            Value = offenderDetails.MiddleName,
                            IsNullable = true
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.LastName,
                            SqlDbType = System.Data.SqlDbType.VarChar,
                            Value = offenderDetails.LastName,
                            IsNullable = false
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.OffenderType,
                            SqlDbType = System.Data.SqlDbType.VarChar,
                            Value = offenderDetails.ClientType,
                            IsNullable = false
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.UpdatedBy,
                            SqlDbType = System.Data.SqlDbType.VarChar,
                            Value = offenderDetails.UpdatedBy,
                            IsNullable = false
                        });

                        cmd.Connection = conn;

                        pin = Convert.ToString(cmd.ExecuteScalar());
                    }
                }
            }

            return pin;
        }
        
        public string GetTimeZone()
        {
            if (automonConfig.IsDevMode)
            {
                //test data
                return TimeZoneInfo.Local.Id;
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
