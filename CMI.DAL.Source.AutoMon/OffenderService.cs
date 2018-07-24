﻿using CMI.DAL.Source.Models;
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

        public IEnumerable<Offender> GetAllOffenderDetails(DateTime lastExecutionDateTime)
        {

            string timeZone = GetTimeZone();


            
            List<Offender> offenders = new List<Offender>();

            using (SqlConnection conn = new SqlConnection(sourceConfig.AutoMonDBConnString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = SQLQuery.GET_ALL_OFFENDER_DETAILS;
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.Parameters.Add(new SqlParameter() { ParameterName = SQLParamName.LAST_EXECUTION_DATE_TIME, SqlDbType = System.Data.SqlDbType.DateTime, Value = lastExecutionDateTime });
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
                                CaseloadType = Convert.ToString(reader[DBColumnName.CASELOAD_TYPE]),
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
            

            /*
            //test data
            return new List<Offender>()
            {
                new Offender()
                {
                    Pin = "5824",
                    FirstName = "John",
                    MiddleName = "Brent",
                    LastName = "Aitkens",
                    DateOfBirth = new DateTime(1961, 6, 12),
                    ClientType = "PRCS",
                    TimeZone = "Pacific Standard Time",
                    Gender = "Male",
                    Race = "Caucasian"
                },
                new Offender()
                {
                    Pin = "7478",
                    FirstName = "John",
                    MiddleName = "Charles",
                    LastName = "Morrissey",
                    DateOfBirth = new DateTime(1981,6,25),
                    ClientType = "PRCS",
                    TimeZone = "Pacific Standard Time",
                    Gender = "Male",
                    Race = "Caucasian"
                },
                new Offender()
                {
                    Pin = "13475",
                    FirstName = "Destiny",
                    MiddleName = "Giana",
                    LastName = "Granger",
                    DateOfBirth = new DateTime(1980,11,03),
                    ClientType = "PRCS",
                    TimeZone = "Pacific Standard Time",
                    Gender ="Female",
                    Race = "Caucasian"
                }
            };
            */
        }

        private string GetTimeZone()
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
            
            
            /*
            //test data
            return "Pacific Standard Time";
            */
        }
    }
}
