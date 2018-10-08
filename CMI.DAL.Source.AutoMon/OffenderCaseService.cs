using CMI.DAL.Source.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Text;
//using System.Linq;

namespace CMI.DAL.Source.AutoMon
{
    public class OffenderCaseService : IOffenderCaseService
    {
        SourceConfig sourceConfig;

        public OffenderCaseService(Microsoft.Extensions.Options.IOptions<SourceConfig> sourceConfig)
        {
            this.sourceConfig = sourceConfig.Value;
        }

        public IEnumerable<OffenderCase> GetAllOffenderCases(string CMIDBConnString, DateTime? lastExecutionDateTime)
        {
            if (sourceConfig.IsDevMode)
            {
                //test data
                return JsonConvert.DeserializeObject<IEnumerable<OffenderCase>>(File.ReadAllText(Path.Combine(sourceConfig.TestDataJSONRepoPath, Constants.TEST_DATA_JSON_FILE_NAME_ALL_OFFENDER_CASE_DETAILS)));
            }
            else
            {
                List<OffenderCase> offenderCases = new List<OffenderCase>();

                using (SqlConnection conn = new SqlConnection(CMIDBConnString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = StoredProc.GET_ALL_OFFENDER_CASE_DETAILS;
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SQLParamName.SOURCE_DATABASE_NAME,
                            SqlDbType = System.Data.SqlDbType.NVarChar,
                            Value = new SqlConnectionStringBuilder(sourceConfig.AutoMonDBConnString).InitialCatalog
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SQLParamName.LAST_EXECUTION_DATE_TIME,
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
                                    Pin = Convert.ToString(reader[DBColumnName.PIN]),
                                    CaseNumber = Convert.ToString(reader[DBColumnName.CASE_NUMBER]),
                                    CaseStatus = Convert.ToString(reader[DBColumnName.CASE_STATUS]),
                                    ClosureReason = Convert.ToString(reader[DBColumnName.CLOSURE_REASON]),

                                    OffenseLabel = Convert.ToString(reader[DBColumnName.OFFENSE_LABEL]),
                                    OffenseStatute = Convert.ToString(reader[DBColumnName.OFFENSE_STATUTE]),
                                    OffenseCategory = Convert.ToString(reader[DBColumnName.OFFENSE_CATEGORY]),
                                    IsPrimary = Convert.ToBoolean(reader[DBColumnName.IS_PRIMARY])
                                };

                                //case date
                                if (Convert.IsDBNull(reader[DBColumnName.CASE_DATE]))
                                {
                                    offenderCase.CaseDate = null;
                                }
                                else
                                {
                                    offenderCase.CaseDate = (DateTime?)reader[DBColumnName.CASE_DATE];
                                }

                                //supervision start date
                                if (Convert.IsDBNull(reader[DBColumnName.SUPERVISION_START_DATE]))
                                {
                                    offenderCase.SupervisionStartDate = null;
                                }
                                else
                                {
                                    offenderCase.SupervisionStartDate = (DateTime?)reader[DBColumnName.SUPERVISION_START_DATE];
                                }

                                //supervision end date
                                if (Convert.IsDBNull(reader[DBColumnName.SUPERVISION_END_DATE]))
                                {
                                    offenderCase.SupervisionEndDate = null;
                                }
                                else
                                {
                                    offenderCase.SupervisionEndDate = (DateTime?)reader[DBColumnName.SUPERVISION_END_DATE];
                                }

                                if (Convert.IsDBNull(reader[DBColumnName.OFFENSE_DATE]))
                                {
                                    offenderCase.OffenseDate = null;
                                }
                                else
                                {
                                    offenderCase.OffenseDate = (DateTime?)reader[DBColumnName.OFFENSE_DATE];
                                }

                                offenderCases.Add(offenderCase);
                            }
                        }
                    }
                }

                return offenderCases;
            }
        }
    }
}
