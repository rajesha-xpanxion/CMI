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
                    new OffenderPhone()
                    {
                        Pin = "5824",
                        Id = 75182,
                        PhoneNumberType = "Mobile",
                        Phone = "(530)957-1521",
                        Comment = null,
                        IsPrimary = false,
                        IsActive = false
                    },
                    new OffenderPhone()
                    {
                        Pin = "5824",
                        Id = 84580,
                        PhoneNumberType = "Mobile",
                        Phone = "(530)308-3607",
                        Comment = null,
                        IsPrimary = false,
                        IsActive = false
                    },
                    new OffenderPhone()
                    {
                        Pin = "5824",
                        Id = 92112,
                        PhoneNumberType = "Mobile",
                        Phone = "(530)334-0991",
                        Comment = null,
                        IsPrimary = false,
                        IsActive = false
                    },
                    new OffenderPhone()
                    {
                        Pin = "5824",
                        Id = 98937,
                        PhoneNumberType = "Message",
                        Phone = "(997)032-7365",
                        Comment = "test phone number",
                        IsPrimary = false,
                        IsActive = false
                    },
                    new OffenderPhone()
                    {
                        Pin = "5824",
                        Id = 97560,
                        PhoneNumberType = "Mobile",
                        Phone = "(530)320-8045",
                        Comment = "test update revert",
                        IsPrimary = false,
                        IsActive = true
                    }
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
