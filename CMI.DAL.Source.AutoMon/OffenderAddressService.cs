using CMI.DAL.Source.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using Microsoft.Extensions.Options;

namespace CMI.DAL.Source.AutoMon
{
    public class OffenderAddressService : IOffenderAddressService
    {
        #region Private Member Variables
        private readonly SourceConfig sourceConfig;
        #endregion

        #region Constructor
        public OffenderAddressService(
            IOptions<SourceConfig> sourceConfig
        )
        {
            this.sourceConfig = sourceConfig.Value;
        }
        #endregion

        #region Public Methods
        public IEnumerable<OffenderAddress> GetAllOffenderAddresses(string CmiDbConnString, DateTime? lastExecutionDateTime)
        {
            if (sourceConfig.IsDevMode)
            {
                //test data
                string testDataJsonFileName = Path.Combine(sourceConfig.TestDataJsonRepoPath, Constants.TestDataJsonFileNameAllOffenderAddressDetails);

                return File.Exists(testDataJsonFileName)
                    ? JsonConvert.DeserializeObject<IEnumerable<OffenderAddress>>(File.ReadAllText(testDataJsonFileName))
                    : new List<OffenderAddress>();
            }
            else
            {
                List<OffenderAddress> offenderAddresses = new List<OffenderAddress>();

                using (SqlConnection conn = new SqlConnection(CmiDbConnString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = StoredProc.GetAllOffenderAddressDetails;
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.SourceDatabaseName,
                            SqlDbType = System.Data.SqlDbType.NVarChar,
                            Value = new SqlConnectionStringBuilder(sourceConfig.AutoMonDbConnString).InitialCatalog
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
                                offenderAddresses.Add(new OffenderAddress()
                                {
                                    Pin = Convert.ToString(reader[DbColumnName.Pin]),
                                    Id = Convert.ToInt32(reader[DbColumnName.Id]),
                                    AddressType = Convert.ToString(reader[DbColumnName.AddressType]),
                                    Line1 = Convert.ToString(reader[DbColumnName.Line1]),
                                    Line2 = Convert.ToString(reader[DbColumnName.Line2]),
                                    City = Convert.ToString(reader[DbColumnName.City]),
                                    State = Convert.ToString(reader[DbColumnName.State]),
                                    Zip = Convert.ToString(reader[DbColumnName.Zip]),
                                    Comment = Convert.ToString(reader[DbColumnName.Comment]),
                                    IsActive = Convert.ToBoolean(reader[DbColumnName.IsActive])
                                });
                            }
                        }
                    }
                }

                return offenderAddresses;
            }
        }
        #endregion
    }
}
