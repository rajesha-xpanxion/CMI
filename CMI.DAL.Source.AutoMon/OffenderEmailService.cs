using CMI.DAL.Source.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
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

        public IEnumerable<OffenderEmail> GetAllOffenderEmails(string CMIDBConnString, DateTime? lastExecutionDateTime)
        {
            if (sourceConfig.IsDevMode)
            {
                //test data
                return JsonConvert.DeserializeObject<IEnumerable<OffenderEmail>>(File.ReadAllText(Path.Combine(sourceConfig.TestDataJSONRepoPath, Constants.TEST_DATA_JSON_FILE_NAME_ALL_OFFENDER_EMAIL_CONTACT_DETAILS)));
            }
            else
            {

                List<OffenderEmail> offenderEmails = new List<OffenderEmail>();

                using (SqlConnection conn = new SqlConnection(CMIDBConnString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = StoredProc.GET_ALL_OFFENDER_EMAIL_DETAILS;
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
            }
        }
    }
}
