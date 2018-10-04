using CMI.DAL.Source.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace CMI.DAL.Source.AutoMon
{
    public class OffenderService : IOffenderService
    {
        SourceConfig sourceConfig;

        public OffenderService(Microsoft.Extensions.Options.IOptions<SourceConfig> sourceConfig)
        {
            this.sourceConfig = sourceConfig.Value;
        }

        public IEnumerable<Offender> GetAllOffenderDetails(string CMIDBConnString, DateTime? lastExecutionDateTime)
        {
            string timeZone = GetTimeZone();

            if (sourceConfig.IsDevMode)
            {
                //test data
                return new List<Offender>()
                {
                    new Offender {Pin="14105",FirstName="William",MiddleName="Jean",LastName="Witter",ClientType="Formal",TimeZone="Pacific Standard Time",Gender="Male",Race="Unknown",DateOfBirth=Convert.ToDateTime("7/27/1972")},
                    new Offender {Pin="8877",FirstName="Marshall",MiddleName="Jesse",LastName="Baldwin",ClientType="MCS",TimeZone="Pacific Standard Time",Gender="Male",Race="Unknown",DateOfBirth=Convert.ToDateTime("12/18/1989")}
                };
            }
            else
            {
                List<Offender> offenders = new List<Offender>();

                using (SqlConnection conn = new SqlConnection(CMIDBConnString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = StoredProc.GET_ALL_OFFENDER_DETAILS;
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SQLParamName.SOURCE_DATABASE_NAME,
                            SqlDbType = System.Data.SqlDbType.NVarChar,
                            Value = new SqlConnectionStringBuilder(sourceConfig.AutoMonDBConnString).InitialCatalog
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SQLParamName.LAST_EXECUTION_DATE_TIME,
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
                                    Pin = Convert.ToString(reader[DBColumnName.PIN]),
                                    FirstName = Convert.ToString(reader[DBColumnName.FIRST_NAME]),
                                    MiddleName = Convert.ToString(reader[DBColumnName.MIDDLE_NAME]),
                                    LastName = Convert.ToString(reader[DBColumnName.LAST_NAME]),
                                    DateOfBirth = Convert.ToDateTime(reader[DBColumnName.DATE_OF_BIRTH]),
                                    ClientType = Convert.ToString(reader[DBColumnName.CLIENT_TYPE]),
                                    TimeZone = timeZone,
                                    Gender = Convert.ToString(reader[DBColumnName.GENDER]),
                                    Race = Convert.ToString(reader[DBColumnName.RACE]),
                                    CaseloadName = Convert.ToString(reader[DBColumnName.CASELOAD_NAME]),
                                    OfficerLogon = Convert.ToString(reader[DBColumnName.OFFICER_LOGON]),
                                    OfficerEmail = Convert.ToString(reader[DBColumnName.OFFICER_EMAIL]),
                                    OfficerFirstName = Convert.ToString(reader[DBColumnName.OFFICER_FIRST_NAME]),
                                    OfficerLastName = Convert.ToString(reader[DBColumnName.OFFICER_LAST_NAME])
                                });
                            }
                        }
                    }
                }

                return offenders;
            }
        }

        private string GetTimeZone()
        {
            if (sourceConfig.IsDevMode)
            {
                //test data
                return "Pacific Standard Time";
            }
            else
            {
                string timeZone = string.Empty;

                using (SqlConnection conn = new SqlConnection(sourceConfig.AutoMonDBConnString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = SQLQuery.GET_TIME_ZONE;
                        cmd.CommandType = System.Data.CommandType.Text;
                        cmd.Connection = conn;

                        timeZone = Convert.ToString(cmd.ExecuteScalar());
                    }
                }

                return timeZone;
            }
        }
    }
}
