using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using Microsoft.Extensions.Options;
using CMI.Automon.Interface;
using CMI.Automon.Model;

namespace CMI.Automon.Service
{
    public class OffenderEmailService : IOffenderEmailService
    {
        #region Private Member Variables
        private readonly AutomonConfig automonConfig;
        #endregion

        #region Constructor
        public OffenderEmailService(
            IOptions<AutomonConfig> automonConfig
        )
        {
            this.automonConfig = automonConfig.Value;
        }
        #endregion

        #region Public Methods
        public IEnumerable<OffenderEmail> GetAllOffenderEmails(string CmiDbConnString, DateTime? lastExecutionDateTime)
        {
            if (automonConfig.IsDevMode)
            {
                //test data
                string testDataJsonFileName = Path.Combine(automonConfig.TestDataJsonRepoPath, Constants.TestDataJsonFileNameAllOffenderEmailContactDetails);

                return File.Exists(testDataJsonFileName)
                    ? JsonConvert.DeserializeObject<IEnumerable<OffenderEmail>>(File.ReadAllText(testDataJsonFileName))
                    : new List<OffenderEmail>();
            }
            else
            {
                List<OffenderEmail> offenderEmails = new List<OffenderEmail>();

                using (SqlConnection conn = new SqlConnection(CmiDbConnString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = StoredProc.GetAllOffenderEmailDetails;
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.AutomonDatabaseName,
                            SqlDbType = System.Data.SqlDbType.NVarChar,
                            Value = new SqlConnectionStringBuilder(automonConfig.AutomonDbConnString).InitialCatalog
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.LastExecutionDateTime,
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
                                    Pin = Convert.ToString(reader[DbColumnName.Pin]),
                                    Id = Convert.ToInt32(reader[DbColumnName.Id]),
                                    EmailAddress = Convert.ToString(reader[DbColumnName.EmailAddress]),
                                    IsPrimary = Convert.ToBoolean(reader[DbColumnName.IsPrimary]),
                                    IsActive = Convert.ToBoolean(reader[DbColumnName.IsActive])
                                });
                            }
                        }
                    }
                }

                return offenderEmails;
            }
        }
        #endregion
    }
}
