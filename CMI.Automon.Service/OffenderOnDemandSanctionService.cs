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
    public class OffenderOnDemandSanctionService : IOffenderOnDemandSanctionService
    {
        #region Private Member Variables
        private readonly AutomonConfig automonConfig;
        #endregion

        #region Constructor
        public OffenderOnDemandSanctionService(
            IOptions<AutomonConfig> automonConfig
        )
        {
            this.automonConfig = automonConfig.Value;
        }
        #endregion

        public void SaveOffenderOnDemandSanctionDetails(string CmiDbConnString, OffenderOnDemandSanction offenderOnDemandSanctionDetails)
        {
            if (automonConfig.IsDevMode)
            {
                string testDataJsonFileName = Path.Combine(automonConfig.TestDataJsonRepoPath, Constants.TestDataJsonFileNameAllOffenderOnDemandSanctionDetails);

                //check if repository parent directory exists, if not then create
                if (!Directory.Exists(automonConfig.TestDataJsonRepoPath))
                {
                    Directory.CreateDirectory(automonConfig.TestDataJsonRepoPath);
                }

                //read existing objects
                List<OffenderOnDemandSanction> offenderOnDemandSanctionDetailsList = File.Exists(testDataJsonFileName)
                    ? JsonConvert.DeserializeObject<List<OffenderOnDemandSanction>>(File.ReadAllText(testDataJsonFileName))
                    : new List<OffenderOnDemandSanction>();

                //merge
                offenderOnDemandSanctionDetailsList.Add(offenderOnDemandSanctionDetails);

                //write back
                File.WriteAllText(testDataJsonFileName, JsonConvert.SerializeObject(offenderOnDemandSanctionDetailsList));
            }
            else
            {
                using (SqlConnection conn = new SqlConnection(CmiDbConnString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = StoredProc.SaveOffenderOnDemandSanctionDetails;
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
                            Value = offenderOnDemandSanctionDetails.Pin,
                            IsNullable = false
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.UpdatedBy,
                            SqlDbType = SqlDbType.VarChar,
                            Value = offenderOnDemandSanctionDetails.UpdatedBy,
                            IsNullable = false
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.Magnitude,
                            SqlDbType = SqlDbType.VarChar,
                            Value = offenderOnDemandSanctionDetails.Magnitude,
                            IsNullable = true
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.Response,
                            SqlDbType = SqlDbType.VarChar,
                            Value = offenderOnDemandSanctionDetails.Response,
                            IsNullable = true
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.IsSkipped,
                            SqlDbType = SqlDbType.Bit,
                            Value = offenderOnDemandSanctionDetails.IsSkipped,
                            IsNullable = false
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.Comment,
                            SqlDbType = SqlDbType.VarChar,
                            Value = offenderOnDemandSanctionDetails.Notes,
                            IsNullable = true
                        });

                        var dataTable = new DataTable(UserDefinedTableType.OnDemandSanctionedActivityDetailsTbl)
                        {
                            Locale = CultureInfo.InvariantCulture
                        };
                        dataTable.Columns.Add(TableColumnName.TermOfSupervision, typeof(string));
                        dataTable.Columns.Add(TableColumnName.Description, typeof(string));
                        dataTable.Columns.Add(TableColumnName.EventDateTime, typeof(DateTime));
                        //check for null & check if any record to process
                        if (offenderOnDemandSanctionDetails.OnDemandSanctionedActivities != null && offenderOnDemandSanctionDetails.OnDemandSanctionedActivities.Any())
                        {
                            foreach (var onDemandsanctionedActivityDetails in offenderOnDemandSanctionDetails.OnDemandSanctionedActivities)
                            {
                                dataTable.Rows.Add(
                                    onDemandsanctionedActivityDetails.TermOfSupervision,
                                    onDemandsanctionedActivityDetails.Description,
                                    onDemandsanctionedActivityDetails.EventDateTime
                                );
                            }
                        }
                        cmd.Parameters.Add(new SqlParameter
                        {
                            ParameterName = SqlParamName.OnDemandSanctionedActivityDetailsTbl,
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
