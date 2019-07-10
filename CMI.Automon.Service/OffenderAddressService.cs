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
    public class OffenderAddressService : IOffenderAddressService
    {
        #region Private Member Variables
        private readonly AutomonConfig automonConfig;
        #endregion

        #region Constructor
        public OffenderAddressService(
            IOptions<AutomonConfig> automonConfig
        )
        {
            this.automonConfig = automonConfig.Value;
        }
        #endregion

        #region Public Methods
        public IEnumerable<OffenderAddress> GetAllOffenderAddresses(string CmiDbConnString, DateTime? lastExecutionDateTime)
        {
            if (automonConfig.IsDevMode)
            {
                //test data
                string testDataJsonFileName = Path.Combine(automonConfig.TestDataJsonRepoPath, Constants.TestDataJsonFileNameAllOffenderAddressDetails);

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

        public int SaveOffenderAddressDetails(string CmiDbConnString, OffenderAddress offenderAddressDetails)
        {
            if (automonConfig.IsDevMode)
            {
                string testDataJsonFileName = Path.Combine(automonConfig.TestDataJsonRepoPath, Constants.TestDataJsonFileNameAllOffenderAddressDetails);

                //check if repository parent directory exists, if not then create
                if (!Directory.Exists(automonConfig.TestDataJsonRepoPath))
                {
                    Directory.CreateDirectory(automonConfig.TestDataJsonRepoPath);
                }

                //read existing objects
                List<OffenderAddress> offenderAddressDetailsList = File.Exists(testDataJsonFileName)
                    ? JsonConvert.DeserializeObject<List<OffenderAddress>>(File.ReadAllText(testDataJsonFileName))
                    : new List<OffenderAddress>();

                //merge
                offenderAddressDetailsList.Add(offenderAddressDetails);

                //write back
                File.WriteAllText(testDataJsonFileName, JsonConvert.SerializeObject(offenderAddressDetailsList));

                return offenderAddressDetails.Id == 0 ? new Random().Next(0, 10000) : offenderAddressDetails.Id;
            }
            else
            {
                using (SqlConnection conn = new SqlConnection(CmiDbConnString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = StoredProc.SaveOffenderAddressDetails;
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.AutomonDatabaseName,
                            SqlDbType = System.Data.SqlDbType.NVarChar,
                            Value = new SqlConnectionStringBuilder(automonConfig.AutomonDbConnString).InitialCatalog
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.Pin,
                            SqlDbType = System.Data.SqlDbType.VarChar,
                            Value = offenderAddressDetails.Pin,
                            IsNullable = false
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.Id,
                            SqlDbType = System.Data.SqlDbType.Int,
                            Value = offenderAddressDetails.Id,
                            IsNullable = true
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.UpdatedBy,
                            SqlDbType = System.Data.SqlDbType.VarChar,
                            Value = offenderAddressDetails.UpdatedBy,
                            IsNullable = false
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.Line1,
                            SqlDbType = System.Data.SqlDbType.VarChar,
                            Value = offenderAddressDetails.Line1,
                            IsNullable = false
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.Line2,
                            SqlDbType = System.Data.SqlDbType.VarChar,
                            Value = offenderAddressDetails.Line2,
                            IsNullable = false
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.AddressType,
                            SqlDbType = System.Data.SqlDbType.VarChar,
                            Value = offenderAddressDetails.AddressType,
                            IsNullable = false
                        });


                        cmd.Connection = conn;

                        return Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }
        }
        #endregion
    }
}
