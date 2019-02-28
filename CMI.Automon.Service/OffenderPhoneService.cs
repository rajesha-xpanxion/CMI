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
    public class OffenderPhoneService : IOffenderPhoneService
    {
        #region Private Member Variables
        private readonly AutomonConfig automonConfig;
        #endregion

        #region Constructor
        public OffenderPhoneService(
            IOptions<AutomonConfig> automonConfig
        )
        {
            this.automonConfig = automonConfig.Value;
        }
        #endregion

        #region Public Methods
        public IEnumerable<OffenderPhone> GetAllOffenderPhones(string CmiDbConnString, DateTime? lastExecutionDateTime)
        {
            if (automonConfig.IsDevMode)
            {
                //test data
                string testDataJsonFileName = Path.Combine(automonConfig.TestDataJsonRepoPath, Constants.TestDataJsonFileNameAllOffenderPhoneContactDetails);

                return File.Exists(testDataJsonFileName)
                    ? JsonConvert.DeserializeObject<IEnumerable<OffenderPhone>>(File.ReadAllText(testDataJsonFileName))
                    : new List<OffenderPhone>();
            }
            else
            {
                List<OffenderPhone> offenderPhones = new List<OffenderPhone>();

                using (SqlConnection conn = new SqlConnection(CmiDbConnString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = StoredProc.GetAllOffenderPhoneDetails;
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
                                offenderPhones.Add(new OffenderPhone()
                                {
                                    Pin = Convert.ToString(reader[DbColumnName.Pin]),
                                    Id = Convert.ToInt32(reader[DbColumnName.Id]),
                                    PhoneNumberType = Convert.ToString(reader[DbColumnName.PhoneNumberType]),
                                    Phone = Convert.ToString(reader[DbColumnName.Phone]),
                                    IsPrimary = Convert.ToBoolean(reader[DbColumnName.IsPrimary]),
                                    Comment = Convert.ToString(reader[DbColumnName.Comment]),
                                    IsActive = Convert.ToBoolean(reader[DbColumnName.IsActive])
                                });
                            }
                        }
                    }
                }

                return offenderPhones;
            }
        }
        #endregion
    }
}
