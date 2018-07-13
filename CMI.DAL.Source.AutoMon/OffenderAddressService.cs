using CMI.DAL.Source.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace CMI.DAL.Source.AutoMon
{
    public class OffenderAddressService : IOffenderAddressService
    {
        SourceConfig sourceConfig;

        public OffenderAddressService(Microsoft.Extensions.Options.IOptions<SourceConfig> sourceConfig)
        {
            this.sourceConfig = sourceConfig.Value;
        }

        public IEnumerable<OffenderAddress> GetAllOffenderAddresses(DateTime lastExecutionDateTime)
        {

            
            List<OffenderAddress> offenderAddresses = new List<OffenderAddress>();

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
                            offenderAddresses.Add(new OffenderAddress()
                            {
                                Pin = Convert.ToString(reader[DBColumnName.PIN]),
                                Id = Convert.ToInt32(reader[DBColumnName.ID]),
                                AddressType = Convert.ToString(reader[DBColumnName.ADDRESS_TYPE]),
                                Line1 = Convert.ToString(reader[DBColumnName.LINE1]),
                                Line2 = Convert.ToString(reader[DBColumnName.LINE2]),
                                City = Convert.ToString(reader[DBColumnName.CITY]),
                                State = Convert.ToString(reader[DBColumnName.STATE]),
                                Zip = Convert.ToString(reader[DBColumnName.ZIP]),
                                Comment = Convert.ToString(reader[DBColumnName.COMMENT])
                            });
                        }
                    }
                }
            }

            return offenderAddresses;
            

            /*
            //test data
            return new List<OffenderAddress>()
            {
                new OffenderAddress()
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

                    Id = 57284,
                    AddressType = "Mailing",
                    Line1 = "9092 Wentworth Springs Rd",
                    City = "Georgetown",
                    State = "CA",
                    Zip = "95634"
                },
                new OffenderAddress()
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

                    Id = 58639,
                    AddressType = "Residential",
                    Line1 = "1 Transient",
                    City = "Georgetown",
                    State = "CA",
                    Zip = "95636",
                    Comment = "test update revert"
                }
            };
            */
        }
    }
}
