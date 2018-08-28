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
            if (sourceConfig.IsDevMode)
            {
                //test data
                return new List<OffenderAddress>()
                {
                    new OffenderAddress()
                    {
                        Pin = "5824",
                        Id = 20653,
                        AddressType = "Mailing",
                        Line1 = "7434 Wentworth Springs Rd",
                        Line2 = "Apt 2",
                        City = "Georgetown",
                        State = "CA",
                        Zip = "95634",
                        IsPrimary = false,
                        IsActive = false
                    },
                    new OffenderAddress()
                    {
                        Pin = "5824",
                        Id = 20603,
                        AddressType = "Mailing",
                        Line1 = "5050 Hope Mountain Rd",
                        City = "Georgetown",
                        State = "CA",
                        Zip = "95633",
                        IsPrimary = false,
                        IsActive = false
                    },
                    new OffenderAddress()
                    {
                        Pin = "5824",
                        Id = 20602,
                        AddressType = "Residential",
                        Line1 = "5050 Hope Mountain Rd",
                        City = "Georgetown",
                        State = "CA",
                        Zip = "95633",
                        IsPrimary = false,
                        IsActive =false
                    },
                    new OffenderAddress()
                    {
                        Pin = "5824",
                        Id = 24855,
                        AddressType = "Mailing",
                        Line1 = "4211 Shoemaker Rd",
                        City = "Georgetown",
                        State = "CA",
                        Zip = "95634",
                        IsPrimary = true,
                        IsActive =false
                    },
                    new OffenderAddress()
                    {
                        Pin = "5824",
                        Id = 24856,
                        AddressType = "Residential",
                        Line1 = "4211 Shoemaker Rd",
                        City = "Georgetown",
                        State = "CA",
                        Zip = "95634",
                        IsPrimary = false,
                        IsActive =false
                    },
                    new OffenderAddress()
                    {
                        Pin = "5824",
                        Id = 55364,
                        AddressType = "Residential",
                        Line1 = "3821 Quest  Ct",
                        Line2 = "Spc 5",
                        City = "Cameron Park",
                        State = "CA",
                        Zip = "95682",
                        IsPrimary = false,
                        IsActive =false
                    },
                    new OffenderAddress()
                    {
                        Pin = "5824",
                        Id = 44974,
                        AddressType = "Residential",
                        Line1 = "transient",
                        City = "Georgetown",
                        State = "CA",
                        Zip = "95634",
                        IsPrimary = false,
                        IsActive =false
                    },
                    new OffenderAddress()
                    {
                        Pin = "5824",
                        Id = 56024,
                        AddressType = "Residential",
                        Line1 = "9097 Wentworth Springs Rd",
                        Line2 = "Spc 8",
                        City = "Georgetown",
                        State = "CA",
                        Zip = "95634",
                        IsPrimary = false,
                        IsActive =false
                    },
                    new OffenderAddress()
                    {
                        Pin = "5824",
                        Id = 52350,
                        AddressType = "Mailing",
                        Line1 = "9400 Wentworth Springs Rd",
                        Line2 = "Spc 8",
                        City = "Georgetown",
                        State = "CA",
                        Zip = "95634",
                        IsPrimary = false,
                        IsActive =false
                    },
                    new OffenderAddress()
                    {
                        Pin = "5824",
                        Id = 56738,
                        AddressType = "Residential",
                        Line1 = "Transient",
                        City = "Georgetown",
                        State = "CA",
                        Zip = "95634",
                        IsPrimary = false,
                        IsActive =false
                    },
                    new OffenderAddress()
                    {
                        Pin = "5824",
                        Id = 57285,
                        AddressType = "Residential",
                        Line1 = "9092 Wentworth springs Rd",
                        City = "Georgetown",
                        State = "CA",
                        Zip = "95634",
                        IsPrimary = false,
                        IsActive =false
                    },
                    new OffenderAddress()
                    {
                        Pin = "5824",
                        Id = 57284,
                        AddressType = "Mailing",
                        Line1 = "9092 Wentworth Springs Rd",
                        City = "Georgetown",
                        State = "CA",
                        Zip = "95634",
                        IsPrimary = false,
                        IsActive = true
                    },
                    new OffenderAddress()
                    {
                        Pin = "5824",
                        Id = 59763,
                        AddressType = "School",
                        Line1 = "test line 1",
                        City = "Anaheim",
                        State = "CA",
                        Zip = "11007",
                        IsPrimary = false,
                        IsActive =false
                    }
    ,
                    new OffenderAddress()
                    {
                        Pin = "5824",
                        Id = 58639,
                        AddressType = "Residential",
                        Line1 = "1 Transient",
                        City = "Georgetown",
                        State = "CA",
                        Zip = "95636",
                        Comment = "test update revert",
                        IsPrimary = false,
                        IsActive = true
                    }
                };
            }
            else
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
                                    Comment = Convert.ToString(reader[DBColumnName.COMMENT]),
                                    IsActive = Convert.ToBoolean(reader[DBColumnName.IS_ACTIVE])
                                });
                            }
                        }
                    }
                }

                return offenderAddresses;
            }
        }
    }
}
