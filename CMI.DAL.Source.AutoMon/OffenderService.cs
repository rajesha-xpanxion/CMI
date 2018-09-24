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

        public IEnumerable<Offender> GetAllOffenderDetails(DateTime? lastExecutionDateTime)
        {
            string timeZone = GetTimeZone();

            if (sourceConfig.IsDevMode)
            {
                //test data
                return new List<Offender>()
                {
                    new Offender {Pin="13",FirstName="Thomas",MiddleName="Matthew",LastName="Adams",DateOfBirth=Convert.ToDateTime("1970-03-12T00:00:00"),ClientType="MCS",TimeZone="Pacific Standard Time",Gender="Male",Race="White",CaseloadName="SLT MCS Sup 1",OfficerLogon="alandry",OfficerEmail="anne.landry@edcgov.us",OfficerFirstName="Anne",OfficerLastName="Landry"}//,
                    //new Offender {Pin="16",FirstName="Derek",MiddleName="Ernest",LastName="Bogard",DateOfBirth=Convert.ToDateTime("1979-05-20T00:00:00"),ClientType="Formal",TimeZone="Pacific Standard Time",Gender="Male",Race="White",CaseloadName="SLT Adult Sup 3",OfficerLogon="ldaley",OfficerEmail="leianna.daley@edcgov.us",OfficerFirstName="Leianna",OfficerLastName="Daley"},
                    //new Offender {Pin="34",FirstName="Timothy",MiddleName="Michael",LastName="Angel",DateOfBirth=Convert.ToDateTime("1970-05-09T00:00:00"),ClientType="Formal",TimeZone="Pacific Standard Time",Gender="Male",Race="White",CaseloadName="SLT PRCS Sup 1",OfficerLogon="alandry",OfficerEmail="anne.landry@edcgov.us",OfficerFirstName="Anne",OfficerLastName="Landry"},
                    //new Offender {Pin="34",FirstName="Timothy",MiddleName="Michael",LastName="Angel",DateOfBirth=Convert.ToDateTime("1970-05-09T00:00:00"),ClientType="PRCS",TimeZone="Pacific Standard Time",Gender="Male",Race="White",CaseloadName="SLT PRCS Sup 1",OfficerLogon="alandry",OfficerEmail="anne.landry@edcgov.us",OfficerFirstName="Anne",OfficerLastName="Landry"},
                    //new Offender {Pin="58",FirstName="Eric",MiddleName="James",LastName="Brown",DateOfBirth=Convert.ToDateTime("1965-08-04T00:00:00"),ClientType="MCS",TimeZone="Pacific Standard Time",Gender="Male",Race="White",CaseloadName="SLT MCS Sup 1",OfficerLogon="alandry",OfficerEmail="anne.landry@edcgov.us",OfficerFirstName="Anne",OfficerLastName="Landry"},
                    //new Offender {Pin="98",FirstName="Jorge",MiddleName="Lemus",LastName="Campos",DateOfBirth=Convert.ToDateTime("1958-11-30T00:00:00"),ClientType="Formal",TimeZone="Pacific Standard Time",Gender="Male",Race="Hispanic",CaseloadName="SLT Adult Sup 3",OfficerLogon="ldaley",OfficerEmail="leianna.daley@edcgov.us",OfficerFirstName="Leianna",OfficerLastName="Daley"}
                };
            }
            else
            {
                List<Offender> offenders = new List<Offender>();

                using (SqlConnection conn = new SqlConnection(sourceConfig.AutoMonDBConnString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = SQLQuery.GET_ALL_OFFENDER_DETAILS;
                        cmd.CommandType = System.Data.CommandType.Text;
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
