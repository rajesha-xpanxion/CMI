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

        public IEnumerable<OffenderAddress> GetAllOffenderAddresses(string CMIDBConnString, DateTime? lastExecutionDateTime)
        {
            if (sourceConfig.IsDevMode)
            {
                //test data
                return new List<OffenderAddress>()
                {
                    new OffenderAddress() {Pin = "226",Id = 869,Line1 = "685 Berk Av, Apt 8, Richmond, CA, 94804",AddressType = "Shipping Address",IsPrimary = false,IsActive = true},
                    new OffenderAddress() {Pin = "401",Id = 1253,Line1 = "1 Transient, South Lake Tahoe, CA, 96150",AddressType = "Home Address",IsPrimary = false,IsActive = true},
                    new OffenderAddress() {Pin = "016",Id = 215,Line1 = "916 Mesa St, Morro Bay, CA, 93442",AddressType = "Shipping Address",IsPrimary = false,IsActive = true},
                    new OffenderAddress() {Pin = "553",Id = 1699,Line1 = "1393 Friant Ct, South Lake Tahoe, CA, 96150",AddressType = "Shipping Address",IsPrimary = false,IsActive = true},
                    new OffenderAddress() {Pin = "563",Id = 1869,Line1 = "1017 Carson Av, 3, South Lake Tahoe, CA, 96150",AddressType = "Shipping Address",IsPrimary = false,IsActive = true},
                    new OffenderAddress() {Pin = "563",Id = 1870,Line1 = "3743 Paradise Av, 1, South Lake Tahoe, CA, 96150",AddressType = "Home Address",IsPrimary = false,IsActive = true},
                    new OffenderAddress() {Pin = "719",Id = 1991,Line1 = "PO Box 5273, Stateline, NV, 89449",AddressType = "Shipping Address",IsPrimary = false,IsActive = true},
                    new OffenderAddress() {Pin = "098",Id = 682,Line1 = "PO Box 17465, South Lake Tahoe, CA, 96151",AddressType = "Shipping Address",IsPrimary = false,IsActive = true},
                    new OffenderAddress() {Pin = "167",Id = 782,Line1 = "3178 Neveda Av, South Lake Tahoe, CA, 96151",AddressType = "Shipping Address",IsPrimary = false,IsActive = true},
                    new OffenderAddress() {Pin = "178",Id = 799,Line1 = "PO Box 5228, Stateline, NV, 89449",AddressType = "Shipping Address",IsPrimary = false,IsActive = true},
                    new OffenderAddress() {Pin = "178",Id = 798,Line1 = "4055 Manzanita, Apt 22, South Lake Tahoe, CA, 96150",AddressType = "Home Address",IsPrimary = false,IsActive = true},
                    new OffenderAddress() {Pin = "058",Id = 523,Line1 = "PO Box 8588, South Lake Tahoe, CA, 96158",AddressType = "Shipping Address",IsPrimary = false,IsActive = true}
                };
            }
            else
            {
                List<OffenderAddress> offenderAddresses = new List<OffenderAddress>();

                using (SqlConnection conn = new SqlConnection(CMIDBConnString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = StoredProc.GET_ALL_OFFENDER_ADDRESS_DETAILS;
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
