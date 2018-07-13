using CMI.DAL.Source.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace CMI.DAL.Source.AutoMon
{
    public class OffenderPhoneService : IOffenderPhoneService
    {
        SourceConfig sourceConfig;

        public OffenderPhoneService(Microsoft.Extensions.Options.IOptions<SourceConfig> sourceConfig)
        {
            this.sourceConfig = sourceConfig.Value;
        }

        public IEnumerable<OffenderPhone> GetAllOffenderPhones(DateTime lastExecutionDateTime)
        {
            
            List<OffenderPhone> offenderPhones = new List<OffenderPhone>();

            using (SqlConnection conn = new SqlConnection(sourceConfig.AutoMonDBConnString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = SQLQuery.GET_ALL_OFFENDER_ADDRESS_DETAILS;
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.Parameters.Add(new SqlParameter() { ParameterName = SQLParamName.LAST_EXECUTION_DATE_TIME, SqlDbType = System.Data.SqlDbType.DateTime, Value = lastExecutionDateTime });
                    cmd.Connection = conn;

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            offenderPhones.Add(new OffenderPhone()
                            {
                                Pin = Convert.ToString(reader[DBColumnName.PIN]),
                                Id = Convert.ToInt32(reader[DBColumnName.ID]),
                                PhoneNumberType = Convert.ToString(reader[DBColumnName.PHONE_NUMBER_TYPE]),
                                Phone = Convert.ToString(reader[DBColumnName.PHONE]),
                                IsPrimary = Convert.ToBoolean(reader[DBColumnName.IS_PRIMARY]),
                                Comment = Convert.ToString(reader[DBColumnName.COMMENT])
                            });
                        }
                    }
                }
            }

            return offenderPhones;
            
            
            /*
            //test data
            return new List<OffenderPhone>()
            {
                new OffenderPhone()
                {
                    Pin = "5824",
                    FirstName = "John",
                    MiddleName = "Brent",
                    LastName = "Aitkens",
                    DateOfBirth = new DateTime(1961, 6, 12),
                    ClientType = "Private",//"PRCS",
                    TimeZone = "Pacific Standard Time",
                    Gender = "Male",
                    Race = "Caucasian",
                    CaseloadName = "A-wYaTt TeStInG FuN",
                    CaseloadType = "Adult Formal Supervision - PRCS",
                    OfficerLogon = "jwyatt",
                    OfficerEmail = string.Empty,//"julie.wyatt@edcgov.us",
                    OfficerFirstName = "Julie",
                    OfficerLastName = "Wyatt",

                    Id = 97560,
                    PhoneNumberType = "Mobile",
                    Phone = "(530)320-8045",
                    IsPrimary = false,
                    Comment = "test update revert"
                }
            };
            */
        }
    }
}
