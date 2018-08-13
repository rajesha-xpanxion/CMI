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
                                Id = Convert.ToInt32(reader[DBColumnName.ID]),
                                EmailAddress = Convert.ToString(reader[DBColumnName.EMAIL_ADDRESS]),
                                IsPrimary = Convert.ToBoolean(reader[DBColumnName.IS_PRIMARY]),
                                IsActive = Convert.ToBoolean(reader[DBColumnName.IS_ACTIVE])
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
                    Id = 362,
                    EmailAddress = "yougotmail@yahoo.com",
                    IsPrimary = true,
                    IsActive = true
                },
                new OffenderEmail()
                {
                    Pin = "13475",
                    Id = 364,
                    EmailAddress = "destinyg@yahoo.com",
                    IsPrimary = true,
                    IsActive = true
                },
                new OffenderEmail()
                {
                    Pin = "1008383",
                    Id = 30,
                    EmailAddress = "christanickelllosrios@gmail.com",
                    IsPrimary = false,
                    IsActive = false
                },
                new OffenderEmail()
                {
                    Pin = "1008383",
                    Id = 31,
                    EmailAddress = "christanickell96@gmail.com",
                    IsPrimary = false,
                    IsActive = false
                },
                new OffenderEmail()
                {
                    Pin = "1008383",
                    Id = 203,
                    EmailAddress = "christanickelll96@gmail.com",
                    IsPrimary = true,
                    IsActive = true
                }
            };
            */
        }
    }
}
