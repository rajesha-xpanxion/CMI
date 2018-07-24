using CMI.DAL.Source.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace CMI.DAL.Source.AutoMon
{
    public class OffenderEmailService : IOffenderEmailService
    {
        SourceConfig sourceConfig;

        public OffenderEmailService(Microsoft.Extensions.Options.IOptions<SourceConfig> sourceConfig)
        {
            this.sourceConfig = sourceConfig.Value;
        }

        public IEnumerable<OffenderEmail> GetAllOffenderEmails(DateTime lastExecutionDateTime)
        {
            
            
            List<OffenderEmail> offenderEmails = new List<OffenderEmail>();

            using (SqlConnection conn = new SqlConnection(sourceConfig.AutoMonDBConnString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = SQLQuery.GET_ALL_OFFENDER_EMAIL_DETAILS;
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.Parameters.Add(new SqlParameter() { ParameterName = SQLParamName.LAST_EXECUTION_DATE_TIME, SqlDbType = System.Data.SqlDbType.DateTime, Value = lastExecutionDateTime });
                    cmd.Connection = conn;

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            offenderEmails.Add(new OffenderEmail()
                            {
                                Pin = Convert.ToString(reader[DBColumnName.PIN]),
                                EmailAddress = Convert.ToString(reader[DBColumnName.EMAIL_ADDRESS]),
                                IsPrimary = Convert.ToBoolean(reader[DBColumnName.IS_PRIMARY])
                            });
                        }
                    }
                }
            }

            return offenderEmails;
            
            
            /*
            //test data
            return new List<OffenderEmail>()
            {
                new OffenderEmail()
                {
                    Pin = "5824",
                    FirstName = "John",
                    MiddleName = "Brent",
                    LastName = "Aitkens",

                    EmailAddress = "yougotmail@yahoo.com",
                    IsPrimary = true
                },
                new OffenderEmail()
                {
                    Pin = "13475",
                    FirstName = "Destiny",
                    MiddleName = "Giana",
                    LastName = "Granger",

                    EmailAddress = "destinyg@yahoo.com",
                    IsPrimary = true
                }
            };
            */
        }
    }
}
