using System.Data.SqlClient;
using Microsoft.Extensions.Options;
using CMI.Automon.Interface;
using CMI.Automon.Model;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;

namespace CMI.Automon.Service
{
    public class OffenderEmploymentService : IOffenderEmploymentService
    {
        #region Private Member Variables
        private readonly AutomonConfig automonConfig;
        #endregion

        #region Constructor
        public OffenderEmploymentService(
            IOptions<AutomonConfig> automonConfig
        )
        {
            this.automonConfig = automonConfig.Value;
        }
        #endregion

        public int SaveOffenderEmploymentDetails(string CmiDbConnString, OffenderEmployment offenderEmploymentDetails)
        {
            if (automonConfig.IsDevMode)
            {
                string testDataJsonFileName = Path.Combine(automonConfig.TestDataJsonRepoPath, Constants.TestDataJsonFileNameAllOffenderEmploymentDetails);

                //check if repository parent directory exists, if not then create
                if (!Directory.Exists(automonConfig.TestDataJsonRepoPath))
                {
                    Directory.CreateDirectory(automonConfig.TestDataJsonRepoPath);
                }

                //read existing objects
                List<OffenderEmployment> offenderEmploymentDetailsList = File.Exists(testDataJsonFileName)
                    ? JsonConvert.DeserializeObject<List<OffenderEmployment>>(File.ReadAllText(testDataJsonFileName))
                    : new List<OffenderEmployment>();

                //merge
                offenderEmploymentDetailsList.Add(offenderEmploymentDetails);

                //write back
                File.WriteAllText(testDataJsonFileName, JsonConvert.SerializeObject(offenderEmploymentDetailsList));

                return offenderEmploymentDetails.Id == 0 ? new Random().Next(0, 10000) : offenderEmploymentDetails.Id;
            }
            else
            {
                using (SqlConnection conn = new SqlConnection(CmiDbConnString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = StoredProc.SaveOffenderEmploymentDetails;
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
                            Value = offenderEmploymentDetails.Pin,
                            IsNullable = false
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.Id,
                            SqlDbType = System.Data.SqlDbType.Int,
                            Value = offenderEmploymentDetails.Id,
                            IsNullable = true
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.UpdatedBy,
                            SqlDbType = System.Data.SqlDbType.VarChar,
                            Value = offenderEmploymentDetails.UpdatedBy,
                            IsNullable = false
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.OrganizationName,
                            SqlDbType = System.Data.SqlDbType.VarChar,
                            Value = offenderEmploymentDetails.OrganizationName,
                            IsNullable = false
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.OrganizationAddress,
                            SqlDbType = System.Data.SqlDbType.VarChar,
                            Value = offenderEmploymentDetails.OrganizationAddress,
                            IsNullable = true
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.OrganizationPhone,
                            SqlDbType = System.Data.SqlDbType.VarChar,
                            Value = offenderEmploymentDetails.OrganizationPhone,
                            IsNullable = true
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.PayFrequency,
                            SqlDbType = System.Data.SqlDbType.VarChar,
                            Value = offenderEmploymentDetails.PayFrequency,
                            IsNullable = true
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.PayRate,
                            SqlDbType = System.Data.SqlDbType.VarChar,
                            Value = offenderEmploymentDetails.PayRate,
                            IsNullable = true
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.WorkType,
                            SqlDbType = System.Data.SqlDbType.VarChar,
                            Value = offenderEmploymentDetails.WorkType,
                            IsNullable = true
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.JobTitle,
                            SqlDbType = System.Data.SqlDbType.VarChar,
                            Value = offenderEmploymentDetails.JobTitle,
                            IsNullable = true
                        });

                        cmd.Connection = conn;

                        return Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }
        }

        public void DeleteOffenderEmploymentDetails(string CmiDbConnString, OffenderEmployment offenderEmploymentDetails)
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
                        cmd.CommandText = StoredProc.DeleteOffenderEmploymentDetails;
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
                            Value = offenderEmploymentDetails.Pin,
                            IsNullable = false
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.Id,
                            SqlDbType = System.Data.SqlDbType.Int,
                            Value = offenderEmploymentDetails.Id,
                            IsNullable = false
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.UpdatedBy,
                            SqlDbType = System.Data.SqlDbType.VarChar,
                            Value = offenderEmploymentDetails.UpdatedBy,
                            IsNullable = false
                        });
                        

                        cmd.Connection = conn;

                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        public IEnumerable<OffenderEmployment> GetAllOffenderEmployments(string CmiDbConnString, DateTime? lastExecutionDateTime)
        {
            if (automonConfig.IsDevMode)
            {
                //test data
                string testDataJsonFileName = Path.Combine(automonConfig.TestDataJsonRepoPath, Constants.TestDataJsonFileNameAllOffenderEmploymentDetails);

                return File.Exists(testDataJsonFileName)
                    ? JsonConvert.DeserializeObject<IEnumerable<OffenderEmployment>>(File.ReadAllText(testDataJsonFileName))
                    : new List<OffenderEmployment>();
            }
            else
            {
                List<OffenderEmployment> offenderEmployments = new List<OffenderEmployment>();

                using (SqlConnection conn = new SqlConnection(CmiDbConnString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = StoredProc.GetAllOffenderEmploymentDetails;
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
                                offenderEmployments.Add(new OffenderEmployment()
                                {
                                    Pin = Convert.ToString(reader[DbColumnName.Pin]),
                                    Id = Convert.ToInt32(reader[DbColumnName.Id]),
                                    OrganizationName = Convert.ToString(reader[DbColumnName.Name]),
                                    OrganizationAddress = Convert.ToString(reader[DbColumnName.FullAddress]),
                                    OrganizationPhone = Convert.ToString(reader[DbColumnName.FullPhoneNumber]),
                                    PayFrequency = Convert.ToString(reader[DbColumnName.PayFrequency]),
                                    PayRate = Convert.ToString(reader[DbColumnName.PayRate]),
                                    WorkType = Convert.ToString(reader[DbColumnName.WorkType]),
                                    JobTitle = Convert.ToString(reader[DbColumnName.JobTitle]),
                                    IsActive = Convert.ToBoolean(reader[DbColumnName.IsActive])
                                });
                            }
                        }
                    }
                }

                return offenderEmployments;
            }
        }
    }
}
