using CMI.Automon.Interface;
using CMI.Automon.Model;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;

namespace CMI.Automon.Service
{
    public class OffenderDrugTestResultService : IOffenderDrugTestResultService
    {
        #region Private Member Variables
        private readonly AutomonConfig automonConfig;
        #endregion

        #region Constructor
        public OffenderDrugTestResultService(
            IOptions<AutomonConfig> automonConfig
        )
        {
            this.automonConfig = automonConfig.Value;
        }

        public int SaveOffenderDrugTestResultDetails(string CmiDbConnString, OffenderDrugTestResult offenderDrugTestResultDetails)
        {
            if (automonConfig.IsDevMode)
            {
                string testDataJsonFileName = Path.Combine(automonConfig.TestDataJsonRepoPath, Constants.TestDataJsonFileNameAllOffenderDrugTestResultDetails);

                //check if repository parent directory exists, if not then create
                if (!Directory.Exists(automonConfig.TestDataJsonRepoPath))
                {
                    Directory.CreateDirectory(automonConfig.TestDataJsonRepoPath);
                }

                //read existing objects
                List<OffenderDrugTestResult> offenderDrugTestResultDetailsList = File.Exists(testDataJsonFileName)
                    ? JsonConvert.DeserializeObject<List<OffenderDrugTestResult>>(File.ReadAllText(testDataJsonFileName))
                    : new List<OffenderDrugTestResult>();

                //merge
                offenderDrugTestResultDetailsList.Add(offenderDrugTestResultDetails);

                //write back
                File.WriteAllText(testDataJsonFileName, JsonConvert.SerializeObject(offenderDrugTestResultDetailsList));

                return offenderDrugTestResultDetails.Id == 0 ? new Random().Next(0, 10000) : offenderDrugTestResultDetails.Id;
            }
            else
            {
                using (SqlConnection conn = new SqlConnection(CmiDbConnString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = StoredProc.SaveOffenderDrugTestResultDetails;
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
                            Value = offenderDrugTestResultDetails.Pin,
                            IsNullable = false
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.Id,
                            SqlDbType = System.Data.SqlDbType.Int,
                            Value = offenderDrugTestResultDetails.Id,
                            IsNullable = true
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.UpdatedBy,
                            SqlDbType = System.Data.SqlDbType.VarChar,
                            Value = offenderDrugTestResultDetails.UpdatedBy,
                            IsNullable = false
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.StartDate,
                            SqlDbType = System.Data.SqlDbType.DateTime,
                            Value = offenderDrugTestResultDetails.StartDate,
                            IsNullable = false
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.Comment,
                            SqlDbType = System.Data.SqlDbType.VarChar,
                            Value = offenderDrugTestResultDetails.Comment,
                            IsNullable = false
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.EndDate,
                            SqlDbType = System.Data.SqlDbType.DateTime,
                            Value = offenderDrugTestResultDetails.EndDate,
                            IsNullable = false
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.Status,
                            SqlDbType = System.Data.SqlDbType.Int,
                            Value = offenderDrugTestResultDetails.Status,
                            IsNullable = false
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.DeviceType,
                            SqlDbType = System.Data.SqlDbType.VarChar,
                            Value = offenderDrugTestResultDetails.DeviceType,
                            IsNullable = false
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.TestResult,
                            SqlDbType = System.Data.SqlDbType.VarChar,
                            Value = offenderDrugTestResultDetails.TestResult,
                            IsNullable = false
                        });
                        if (!string.IsNullOrEmpty(offenderDrugTestResultDetails.Validities))
                        {
                            cmd.Parameters.Add(new SqlParameter()
                            {
                                ParameterName = SqlParamName.Validities,
                                SqlDbType = System.Data.SqlDbType.VarChar,
                                Value = offenderDrugTestResultDetails.Validities,
                                IsNullable = true
                            });
                        }
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.IsSaveFinalTestResult,
                            SqlDbType = System.Data.SqlDbType.Bit,
                            Value = offenderDrugTestResultDetails.IsSaveFinalTestResult,
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
