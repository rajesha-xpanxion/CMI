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
    public class OffenderSanctionService : IOffenderSanctionService
    {
        #region Private Member Variables
        private readonly AutomonConfig automonConfig;
        #endregion

        #region Constructor
        public OffenderSanctionService(
            IOptions<AutomonConfig> automonConfig
        )
        {
            this.automonConfig = automonConfig.Value;
        }
        #endregion

        public void SaveOffenderSanctionDetails(string CmiDbConnString, OffenderSanction offenderSanctionDetails)
        {
            if (automonConfig.IsDevMode)
            {
                string testDataJsonFileName = Path.Combine(automonConfig.TestDataJsonRepoPath, Constants.TestDataJsonFileNameAllOffenderSanctionDetails);

                //check if repository parent directory exists, if not then create
                if (!Directory.Exists(automonConfig.TestDataJsonRepoPath))
                {
                    Directory.CreateDirectory(automonConfig.TestDataJsonRepoPath);
                }

                //read existing objects
                List<OffenderSanction> offenderSanctionDetailsList = File.Exists(testDataJsonFileName)
                    ? JsonConvert.DeserializeObject<List<OffenderSanction>>(File.ReadAllText(testDataJsonFileName))
                    : new List<OffenderSanction>();

                //merge
                offenderSanctionDetailsList.Add(offenderSanctionDetails);

                //write back
                File.WriteAllText(testDataJsonFileName, JsonConvert.SerializeObject(offenderSanctionDetailsList));

            }
            else
            {
                using (SqlConnection conn = new SqlConnection(CmiDbConnString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = StoredProc.SaveOffenderSanctionDetails;
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
                            Value = offenderSanctionDetails.Pin,
                            IsNullable = false
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.UpdatedBy,
                            SqlDbType = SqlDbType.VarChar,
                            Value = offenderSanctionDetails.UpdatedBy,
                            IsNullable = false
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.Magnitude,
                            SqlDbType = SqlDbType.VarChar,
                            Value = offenderSanctionDetails.Magnitude,
                            IsNullable = true
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.Response,
                            SqlDbType = SqlDbType.VarChar,
                            Value = offenderSanctionDetails.Response,
                            IsNullable = true
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.DateIssued,
                            SqlDbType = SqlDbType.VarChar,
                            Value = offenderSanctionDetails.DateIssued,
                            IsNullable = true
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.IsBundled,
                            SqlDbType = SqlDbType.Bit,
                            Value = offenderSanctionDetails.IsBundled,
                            IsNullable = false
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.IsSkipped,
                            SqlDbType = SqlDbType.Bit,
                            Value = offenderSanctionDetails.IsSkipped,
                            IsNullable = false
                        });

                        var dataTable = new DataTable(UserDefinedTableType.SanctionedActivityDetailsTbl)
                        {
                            Locale = CultureInfo.InvariantCulture
                        };
                        dataTable.Columns.Add(TableColumnName.ActivityTypeName, typeof(string));
                        dataTable.Columns.Add(TableColumnName.ActivityIdentifier, typeof(string));
                        //check for null & check if any record to process
                        if (offenderSanctionDetails.SanctionedActivities != null && offenderSanctionDetails.SanctionedActivities.Any())
                        {
                            foreach (var sanctionedActivityDetails in offenderSanctionDetails.SanctionedActivities)
                            {
                                dataTable.Rows.Add(
                                    sanctionedActivityDetails.ActivityTypeName,
                                    sanctionedActivityDetails.ActivityIdentifier
                                );
                            }
                        }
                        cmd.Parameters.Add(new SqlParameter
                        {
                            ParameterName = SqlParamName.SanctionedActivityDetailsTbl,
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
