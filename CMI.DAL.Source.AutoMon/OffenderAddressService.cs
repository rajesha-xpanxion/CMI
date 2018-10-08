using CMI.DAL.Source.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
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
                return JsonConvert.DeserializeObject<IEnumerable<OffenderAddress>>(File.ReadAllText(Path.Combine(sourceConfig.TestDataJSONRepoPath, Constants.TEST_DATA_JSON_FILE_NAME_ALL_OFFENDER_ADDRESS_DETAILS)));
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
