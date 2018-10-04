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

        public IEnumerable<OffenderPhone> GetAllOffenderPhones(string CMIDBConnString, DateTime? lastExecutionDateTime)
        {
            if (sourceConfig.IsDevMode)
            {
                //test data
                return new List<OffenderPhone>()
                {
                    new OffenderPhone {Pin="1006332", Id=81406,PhoneNumberType="Residential",Phone="(530)541-0502",IsPrimary=true,Comment="",IsActive=true},
                    new OffenderPhone {Pin="12646",Id=97604,Comment="defendant's cell phone as of 02/18/16",Phone="(530)204-0218",PhoneNumberType="Cell Phone",IsPrimary=false,IsActive=true},
                    new OffenderPhone {Pin="5318",Id=94272,Comment="Defendant's cell phone as of 01/30/15",Phone="(775)870-8707",PhoneNumberType="Cell Phone",IsPrimary=true,IsActive=true},
                    new OffenderPhone {Pin="436",Id=97213,Comment="New cell phone number as of 12/23/15",Phone="(530)314-1625",PhoneNumberType="Cell Phone",IsPrimary=true,IsActive=true},
                    new OffenderPhone {Pin="6875",Id=98160,Comment="Offender's cell as of 4/27/16",Phone="(530)308-3844",PhoneNumberType="Cell Phone",IsPrimary=true,IsActive=true},
                    new OffenderPhone {Pin="12241",Id=98166,Comment="new cell phone as of 04/28/16",Phone="(530)314-9913",PhoneNumberType="Cell Phone",IsPrimary=true,IsActive=true},
                    new OffenderPhone {Pin="1030",Id=98534,Comment="Rick Harvey, Owner of Restoration House/ Message #",Phone="(805)710-3032",PhoneNumberType="Cell Phone",IsPrimary=false,IsActive=true},
                    new OffenderPhone {Pin="10102",Id=98560,Comment="Defendant's cell phone as of 06/10/16.",Phone="(530)721-5065",PhoneNumberType="Cell Phone",IsPrimary=true,IsActive=true},
                    new OffenderPhone {Pin="12350",Id=98684,Comment="roommate cell\r\n",Phone="(209)872-8444",PhoneNumberType="Cell Phone",IsPrimary=false,IsActive=true},
                    new OffenderPhone {Pin="1429",Id=98878,Comment="Debbie (fiancée)",Phone="(603)493-1168",PhoneNumberType="Cell Phone",IsPrimary=true,IsActive=true},
                    new OffenderPhone {Pin="12243",Id=97683,Comment="Defendant's new cell phone as of 03/02/16",Phone="(530)721-7940",PhoneNumberType="Cell Phone",IsPrimary=true,IsActive=true}
                };
            }
            else
            {

                List<OffenderPhone> offenderPhones = new List<OffenderPhone>();

                using (SqlConnection conn = new SqlConnection(CMIDBConnString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = StoredProc.GET_ALL_OFFENDER_PHONE_DETAILS;
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
                                offenderPhones.Add(new OffenderPhone()
                                {
                                    Pin = Convert.ToString(reader[DBColumnName.PIN]),
                                    Id = Convert.ToInt32(reader[DBColumnName.ID]),
                                    PhoneNumberType = Convert.ToString(reader[DBColumnName.PHONE_NUMBER_TYPE]),
                                    Phone = Convert.ToString(reader[DBColumnName.PHONE]),
                                    IsPrimary = Convert.ToBoolean(reader[DBColumnName.IS_PRIMARY]),
                                    Comment = Convert.ToString(reader[DBColumnName.COMMENT]),
                                    IsActive = Convert.ToBoolean(reader[DBColumnName.IS_ACTIVE])
                                });
                            }
                        }
                    }
                }

                return offenderPhones;
            }
        }
    }
}
