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
                    new OffenderAddress {Pin="12860",Id=54966,Comment="",Line1="C/O General Delivery, Carmichael, CA, 95608",AddressType="Mailing",IsPrimary=false,IsActive=true},
                    new OffenderAddress {Pin="11784",Id=58078,Comment="",Line1="7321 La Crascenta Dr, Cameron Park, CA, 95682",AddressType="Mailing",IsPrimary=false,IsActive=false},
                    new OffenderAddress {Pin="10652",Id=59304,Comment="",Line1="Progress House/T House, Placerville, CA, 95667",AddressType="Residential",IsPrimary=false,IsActive=true},
                    new OffenderAddress {Pin="5193",Id=33455,Comment="Received via USPS 10/11/11.  Sent from Defendant VIA USPS on 09/02/11.",Line1="Calle 3 poniente #1304 Bario Santiago, Puebla, Puebla, 72980",AddressType="Mailing",IsPrimary=false,IsActive=true},
                    new OffenderAddress {Pin="7948",Id=40162,Comment="Address changed / FTB letter returned",Line1="6213 Laurine Way, Sacramento, CA, 95824-3812",AddressType="Mailing",IsPrimary=false,IsActive=true},
                    new OffenderAddress {Pin="8563",Id=40174,Comment="Address changed / FTB letter returned - remailed",Line1="6815 Hibiscus Ct, Sparks, NV, 89436-9074",AddressType="Mailing",IsPrimary=false,IsActive=true},
                    new OffenderAddress {Pin="3195",Id=40186,Comment="Address changed / FTB letter returned - remailed",Line1="10445 White Oak Ave, Granada Hills, CA, 91344-5927",AddressType="Mailing",IsPrimary=false,IsActive=true},
                    new OffenderAddress {Pin="3195",Id=41500,Comment="Address changed / FTB letter returned - remailed",Line1="10445 White Oak Ave, Granada Hills, CA, 91344-5927",AddressType="Residential",IsPrimary=false,IsActive=true},
                    new OffenderAddress {Pin="5070",Id=41680,Comment="Peugh is in violation of ISC, not at residence.  Gave him until 2/12/13 to get residence or transfer could be canceled.",Line1="transient, southern, OR",AddressType="Residential",IsPrimary=false,IsActive=true},
                    new OffenderAddress {Pin="9412",Id=48219,Comment="D states there are other houses/buildings on property.  He has a private entrance into his residence.  Property is called Lacy's Arabian Ranch.",Line1="5120 Marshall Road, Garden Valley, CA, 95633",AddressType="Residential",IsPrimary=false,IsActive=true},
                    new OffenderAddress {Pin="6768",Id=50089,Comment="8/14/14: This is def's mother's address. Def. stays with mother when Aunt's boyfriend is in town. ",Line1="3266 Cimmarron Rd, Apt 23, Cameron Park, CA, 95682",AddressType="Mailing",IsPrimary=false,IsActive=true},
                    new OffenderAddress {Pin="9885",Id=41585,Comment="verified as of 10/25/14",Line1="2334 Tahoe Vista, 1, South Lake Tahoe, CA, 96150",AddressType="Residential",IsPrimary=false,IsActive=true},
                    new OffenderAddress {Pin="9582",Id=40470,Comment="verified as of 1/21/15",Line1="1 Transient, South Lake Tahoe, CA, 96150",AddressType="Residential",IsPrimary=false,IsActive=true},
                    new OffenderAddress {Pin="12175",Id=51429,Comment="verified 4/20/15",Line1="4127 Pine Bv, 168, South Lake Tahoe, CA, 96150",AddressType="Residential",IsPrimary=false,IsActive=true},
                    new OffenderAddress {Pin="4373",Id=11428,Comment="verified as of 05/08/15",Line1="3426 Hobart Rd, South Lake Tahoe, CA, 96150",AddressType="Residential",IsPrimary=false,IsActive=true},
                    new OffenderAddress {Pin="12060",Id=50973,Comment="verified as of 10/29/14",Line1="2270 Utah Av, South Lake Tahoe, CA, 96150",AddressType="Residential",IsPrimary=false,IsActive=true},
                    new OffenderAddress {Pin="10503",Id=44187,Comment="verified as of 05/08/15",Line1="1849 Normuk St, South Lake Tahoe, CA, 96150",AddressType="Mailing",IsPrimary=false,IsActive=true},
                    new OffenderAddress {Pin="12498",Id=53284,Comment="verified as of 6/10/15",Line1="1027 Glen Rd, South Lake Tahoe, CA, 96150",AddressType="Residential",IsPrimary=false,IsActive=true},
                    new OffenderAddress {Pin="8096",Id=34462,Comment="Verified as of 9/10/15",Line1="804 Capistrano Av, South Lake Tahoe, CA, 96150",AddressType="Residential",IsPrimary=false,IsActive=true},
                    new OffenderAddress {Pin="11880",Id=52093,Comment="Updated the address to the proper format for \"transient\" in order to clear the SRF error reported on 11/12/2015.",Line1="Transient, El Dorado  , CA, 95623",AddressType="Residential",IsPrimary=false,IsActive=true},
                    new OffenderAddress {Pin="9246",Id=56541,Comment="Admission Date 4/22/2015 - CDCR# AW4966",Line1="PO Box 8500, Coalinga, CA, 93210",AddressType="Mailing",IsPrimary=false,IsActive=true},
                    new OffenderAddress {Pin="12244",Id=51757,Comment="J. Wyatt added Shingle Springs/CA/El Dorado to ensure the SRF is complete. ",Line1="Transient, Shingle Springs, CA",AddressType="Mailing",IsPrimary=false,IsActive=true},
                    new OffenderAddress {Pin="12771",Id=54689,Comment="as of 3/3",Line1="1108 3rd St. St, Apt 1, South Lake Tahoe, CA, 96150",AddressType="Residential",IsPrimary=false,IsActive=true},
                    new OffenderAddress {Pin="13682",Id=57634,Comment="3/11/2016 - notified DOJ of their county listing has Grizzly Flats represented as Grizzly Flat.  DOJ will update and we will change back to Grizzly FLATS once DOJ has been updated (j.wyatt)",Line1="4963 Deer Track Ct, Grizzly Flats, CA, 95636",AddressType="Residential",IsPrimary=false,IsActive=true},
                    new OffenderAddress {Pin="5115",Id=51361,Comment="As of 5/12/16",Line1="3668 Spruce, 10, South Lake Tahoe, CA, 96150",AddressType="Residential",IsPrimary=false,IsActive=true},
                    new OffenderAddress {Pin="10392",Id=58788,Comment="Removed the ' from the address, as it was creating an SRF error.  (j.wyatt 5/23/2016)",Line1="1087 O Malley Dr, A, South Lake Tahoe, CA, 96150",AddressType="Residential",IsPrimary=false,IsActive=true},
                    new OffenderAddress {Pin="10392",Id=58789,Comment="Removed the ' from the address, as it was creating an SRF error.  (j.wyatt 5/23/2016)",Line1="1087 O Malley Dr, A, South Lake Tahoe, CA, 96150",AddressType="Mailing",IsPrimary=false,IsActive=true}
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
