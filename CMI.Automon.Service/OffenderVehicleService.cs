using System.Data.SqlClient;
using Microsoft.Extensions.Options;
using CMI.Automon.Interface;
using CMI.Automon.Model;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System;
using System.Data;

namespace CMI.Automon.Service
{
    public class OffenderVehicleService : IOffenderVehicleService
    {
        #region Private Member Variables
        private readonly AutomonConfig automonConfig;
        #endregion

        #region Constructor
        public OffenderVehicleService(
            IOptions<AutomonConfig> automonConfig
        )
        {
            this.automonConfig = automonConfig.Value;
        }
        #endregion

        public int SaveOffenderVehicleDetails(string CmiDbConnString, OffenderVehicle offenderVehicleDetails)
        {
            if (automonConfig.IsDevMode)
            {
                string testDataJsonFileName = Path.Combine(automonConfig.TestDataJsonRepoPath, Constants.TestDataJsonFileNameAllOffenderVehicleDetails);

                //check if repository parent directory exists, if not then create
                if (!Directory.Exists(automonConfig.TestDataJsonRepoPath))
                {
                    Directory.CreateDirectory(automonConfig.TestDataJsonRepoPath);
                }

                //read existing objects
                List<OffenderVehicle> offenderVehicleDetailsList = File.Exists(testDataJsonFileName)
                    ? JsonConvert.DeserializeObject<List<OffenderVehicle>>(File.ReadAllText(testDataJsonFileName))
                    : new List<OffenderVehicle>();

                //merge
                offenderVehicleDetailsList.Add(offenderVehicleDetails);

                //write back
                File.WriteAllText(testDataJsonFileName, JsonConvert.SerializeObject(offenderVehicleDetailsList));

                return offenderVehicleDetails.Id == 0 ? new Random().Next(0, 10000) : offenderVehicleDetails.Id;
            }
            else
            {
                using (SqlConnection conn = new SqlConnection(CmiDbConnString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = StoredProc.SaveOffenderVehicleDetails;
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
                            Value = offenderVehicleDetails.Pin,
                            IsNullable = false
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.Id,
                            SqlDbType = System.Data.SqlDbType.Int,
                            Value = offenderVehicleDetails.Id,
                            IsNullable = true
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.UpdatedBy,
                            SqlDbType = System.Data.SqlDbType.VarChar,
                            Value = offenderVehicleDetails.UpdatedBy,
                            IsNullable = false
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.VehicleYear,
                            SqlDbType = System.Data.SqlDbType.Int,
                            Value = offenderVehicleDetails.VehicleYear,
                            IsNullable = true
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.Make,
                            SqlDbType = System.Data.SqlDbType.VarChar,
                            Value = offenderVehicleDetails.Make,
                            IsNullable = false
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.Color,
                            SqlDbType = System.Data.SqlDbType.VarChar,
                            Value = offenderVehicleDetails.Color,
                            IsNullable = false
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.LicensePlate,
                            SqlDbType = System.Data.SqlDbType.VarChar,
                            Value = offenderVehicleDetails.LicensePlate,
                            IsNullable = false
                        });


                        cmd.Connection = conn;

                        return Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }
        }

        public void DeleteOffenderVehicleDetails(string CmiDbConnString, OffenderVehicle offenderVehicleDetails)
        {
            if (automonConfig.IsDevMode)
            {
                //test data
                //string testDataJsonFileName = Path.Combine(automonConfig.TestDataJsonRepoPath, Constants.TestDataJsonFileNameAllOffenderNoteDetails);

                //return File.Exists(testDataJsonFileName)
                //    ? JsonConvert.DeserializeObject<IEnumerable<OffenderNote>>(File.ReadAllText(testDataJsonFileName))
                //    : new List<OffenderNote>();
            }
            else
            {
                using (SqlConnection conn = new SqlConnection(CmiDbConnString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = StoredProc.DeleteOffenderVehicleDetails;
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
                            Value = offenderVehicleDetails.Pin,
                            IsNullable = false
                        });
                        cmd.Parameters.Add(new SqlParameter
                        {
                            ParameterName = SqlParamName.Id,
                            SqlDbType = System.Data.SqlDbType.Int,
                            Value = offenderVehicleDetails.Id,
                            IsNullable = false
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.UpdatedBy,
                            SqlDbType = System.Data.SqlDbType.VarChar,
                            Value = offenderVehicleDetails.UpdatedBy,
                            IsNullable = false
                        });

                        cmd.Connection = conn;

                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        public IEnumerable<OffenderVehicle> GetAllOffenderVehicles(string CmiDbConnString, DateTime? lastExecutionDateTime, DataTable officerLogonsToFilterTbl)
        {
            if (automonConfig.IsDevMode)
            {
                //test data
                string testDataJsonFileName = Path.Combine(automonConfig.TestDataJsonRepoPath, Constants.TestDataJsonFileNameAllOffenderVehicleDetails);

                return File.Exists(testDataJsonFileName)
                    ? JsonConvert.DeserializeObject<IEnumerable<OffenderVehicle>>(File.ReadAllText(testDataJsonFileName))
                    : new List<OffenderVehicle>();
            }
            else
            {
                List<OffenderVehicle> offenderVehicles = new List<OffenderVehicle>();

                using (SqlConnection conn = new SqlConnection(CmiDbConnString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = StoredProc.GetAllOffenderVehicleDetails;
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
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.OfficerLogonsToFilterTbl,
                            SqlDbType = SqlDbType.Structured,
                            Value = officerLogonsToFilterTbl,
                            Direction = ParameterDirection.Input
                        });
                        cmd.Connection = conn;

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                offenderVehicles.Add(new OffenderVehicle()
                                {
                                    Pin = Convert.ToString(reader[DbColumnName.Pin]),
                                    Id = Convert.ToInt32(reader[DbColumnName.Id]),
                                    Make = Convert.ToString(reader[DbColumnName.Make]),
                                    VehicleYear = Convert.ToInt32(reader[DbColumnName.Vyear]),
                                    LicensePlate = Convert.ToString(reader[DbColumnName.LicensePlate]),
                                    Color = Convert.ToString(reader[DbColumnName.Color]),
                                    IsActive = Convert.ToBoolean(reader[DbColumnName.IsActive])
                                });
                            }
                        }
                    }
                }

                return offenderVehicles;
            }
        }
    }
}
