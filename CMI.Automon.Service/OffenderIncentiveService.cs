using System.Data.SqlClient;
using Microsoft.Extensions.Options;
using CMI.Automon.Interface;
using CMI.Automon.Model;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Globalization;
using System.Linq;

namespace CMI.Automon.Service
{
    public class OffenderIncentiveService : IOffenderIncentiveService
    {
        #region Private Member Variables
        private readonly AutomonConfig automonConfig;
        #endregion

        #region Constructor
        public OffenderIncentiveService(
            IOptions<AutomonConfig> automonConfig
        )
        {
            this.automonConfig = automonConfig.Value;
        }
        #endregion

        public void SaveOffenderIncentiveDetails(string CmiDbConnString, OffenderIncentive offenderIncentiveDetails)
        {
            if (automonConfig.IsDevMode)
            {
                string testDataJsonFileName = Path.Combine(automonConfig.TestDataJsonRepoPath, Constants.TestDataJsonFileNameAllOffenderIncentiveDetails);

                //check if repository parent directory exists, if not then create
                if (!Directory.Exists(automonConfig.TestDataJsonRepoPath))
                {
                    Directory.CreateDirectory(automonConfig.TestDataJsonRepoPath);
                }

                //read existing objects
                List<OffenderIncentive> offenderIncentiveDetailsList = File.Exists(testDataJsonFileName)
                    ? JsonConvert.DeserializeObject<List<OffenderIncentive>>(File.ReadAllText(testDataJsonFileName))
                    : new List<OffenderIncentive>();

                //merge
                offenderIncentiveDetailsList.Add(offenderIncentiveDetails);

                //write back
                File.WriteAllText(testDataJsonFileName, JsonConvert.SerializeObject(offenderIncentiveDetailsList));
            }
            else
            {
                using (SqlConnection conn = new SqlConnection(CmiDbConnString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = StoredProc.SaveOffenderIncentiveDetails;
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.AutomonDatabaseName,
                            SqlDbType = SqlDbType.NVarChar,
                            Value = new SqlConnectionStringBuilder(automonConfig.AutomonDbConnString).InitialCatalog
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.Pin,
                            SqlDbType = SqlDbType.VarChar,
                            Value = offenderIncentiveDetails.Pin,
                            IsNullable = false
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.UpdatedBy,
                            SqlDbType = SqlDbType.VarChar,
                            Value = offenderIncentiveDetails.UpdatedBy,
                            IsNullable = false
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.Magnitude,
                            SqlDbType = SqlDbType.VarChar,
                            Value = offenderIncentiveDetails.Magnitude,
                            IsNullable = true
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.Response,
                            SqlDbType = SqlDbType.VarChar,
                            Value = offenderIncentiveDetails.Response,
                            IsNullable = true
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.DateIssued,
                            SqlDbType = SqlDbType.VarChar,
                            Value = offenderIncentiveDetails.DateIssued,
                            IsNullable = true
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.IsBundled,
                            SqlDbType = SqlDbType.Bit,
                            Value = offenderIncentiveDetails.IsBundled,
                            IsNullable = false
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.IsSkipped,
                            SqlDbType = SqlDbType.Bit,
                            Value = offenderIncentiveDetails.IsSkipped,
                            IsNullable = false
                        });

                        var dataTable = new DataTable(UserDefinedTableType.IncentedActivityDetailsTbl)
                        {
                            Locale = CultureInfo.InvariantCulture
                        };
                        dataTable.Columns.Add(TableColumnName.ActivityTypeName, typeof(string));
                        dataTable.Columns.Add(TableColumnName.ActivityIdentifier, typeof(string));
                        //check for null & check if any record to process
                        if (offenderIncentiveDetails.IncentedActivities != null && offenderIncentiveDetails.IncentedActivities.Any())
                        {
                            foreach (var incentedActivityDetails in offenderIncentiveDetails.IncentedActivities)
                            {
                                dataTable.Rows.Add(
                                    incentedActivityDetails.ActivityTypeName,
                                    incentedActivityDetails.ActivityIdentifier
                                );
                            }
                        }
                        cmd.Parameters.Add(new SqlParameter
                        {
                            ParameterName = SqlParamName.IncentedActivityDetailsTbl,
                            Value = dataTable,
                            SqlDbType = SqlDbType.Structured,
                            Direction = ParameterDirection.Input
                        });

                        cmd.Connection = conn;

                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}
