using CMI.DAL.Source.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using Microsoft.Extensions.Options;

namespace CMI.DAL.Source.AutoMon
{
    public class OffenderCaseService : IOffenderCaseService
    {
        #region Private Member Variables
        private readonly SourceConfig sourceConfig;
        #endregion

        #region Constructor
        public OffenderCaseService(
            IOptions<SourceConfig> sourceConfig
        )
        {
            this.sourceConfig = sourceConfig.Value;
        }
        #endregion

        #region Public Methods
        public IEnumerable<OffenderCase> GetAllOffenderCases(string CmiDbConnString, DateTime? lastExecutionDateTime)
        {
            if (sourceConfig.IsDevMode)
            {
                //test data
                string testDataJsonFileName = Path.Combine(sourceConfig.TestDataJsonRepoPath, Constants.TestDataJsonFileNameAllOffenderCaseDetails);

                return File.Exists(testDataJsonFileName)
                    ? JsonConvert.DeserializeObject<IEnumerable<OffenderCase>>(File.ReadAllText(testDataJsonFileName))
                    : new List<OffenderCase>();
            }
            else
            {
                List<OffenderCase> offenderCases = new List<OffenderCase>();

                using (SqlConnection conn = new SqlConnection(CmiDbConnString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = StoredProc.GetAllOffenderCaseDetails;
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
                                var offenderCase = new OffenderCase()
                                {
                                    Pin = Convert.ToString(reader[DbColumnName.Pin]),
                                    CaseNumber = Convert.ToString(reader[DbColumnName.CaseNumber]),
                                    CaseStatus = Convert.ToString(reader[DbColumnName.CaseStatus]),
                                    ClosureReason = Convert.ToString(reader[DbColumnName.ClosureReason]),

                                    OffenseLabel = Convert.ToString(reader[DbColumnName.OffenseLabel]),
                                    OffenseStatute = Convert.ToString(reader[DbColumnName.OffenseStatute]),
                                    OffenseCategory = Convert.ToString(reader[DbColumnName.OffenseCategory]),
                                    IsPrimary = Convert.ToBoolean(reader[DbColumnName.IsPrimary])
                                };

                                //case date
                                if (Convert.IsDBNull(reader[DbColumnName.CaseDate]))
                                {
                                    offenderCase.CaseDate = null;
                                }
                                else
                                {
                                    offenderCase.CaseDate = (DateTime?)reader[DbColumnName.CaseDate];
                                }

                                //supervision start date
                                if (Convert.IsDBNull(reader[DbColumnName.SupervisionStartDate]))
                                {
                                    offenderCase.SupervisionStartDate = null;
                                }
                                else
                                {
                                    offenderCase.SupervisionStartDate = (DateTime?)reader[DbColumnName.SupervisionStartDate];
                                }

                                //supervision end date
                                if (Convert.IsDBNull(reader[DbColumnName.SupervisionEndDate]))
                                {
                                    offenderCase.SupervisionEndDate = null;
                                }
                                else
                                {
                                    offenderCase.SupervisionEndDate = (DateTime?)reader[DbColumnName.SupervisionEndDate];
                                }

                                if (Convert.IsDBNull(reader[DbColumnName.OffenseDate]))
                                {
                                    offenderCase.OffenseDate = null;
                                }
                                else
                                {
                                    offenderCase.OffenseDate = (DateTime?)reader[DbColumnName.OffenseDate];
                                }

                                offenderCases.Add(offenderCase);
                            }
                        }
                    }
                }

                return offenderCases;
            }
        }
        #endregion
    }
}
