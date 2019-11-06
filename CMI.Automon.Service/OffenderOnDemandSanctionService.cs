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

        public int SaveOffenderOnDemandSanctionDetails(string CmiDbConnString, OffenderOnDemandSanction offenderOnDemandSanctionDetails)
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

                return offenderOnDemandSanctionDetails.Id == 0 ? new Random().Next(0, 10000) : offenderOnDemandSanctionDetails.Id;
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
                            ParameterName = SqlParamName.Id,
                            SqlDbType = SqlDbType.Int,
                            Value = offenderOnDemandSanctionDetails.Id,
                            IsNullable = true
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
                            ParameterName = SqlParamName.EventDateTime,
                            SqlDbType = SqlDbType.DateTime,
                            Value = offenderOnDemandSanctionDetails.EventDateTime,
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
                        /*
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.DateIssued,
                            SqlDbType = SqlDbType.VarChar,
                            Value = offenderOnDemandSanctionDetails.DateIssued,
                            IsNullable = true
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.IsBundled,
                            SqlDbType = SqlDbType.Bit,
                            Value = offenderOnDemandSanctionDetails.IsBundled,
                            IsNullable = false
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.IsSkipped,
                            SqlDbType = SqlDbType.Bit,
                            Value = offenderOnDemandSanctionDetails.IsSkipped,
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
                        */

                        cmd.Connection = conn;

                        return Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }
        }
    }
}
