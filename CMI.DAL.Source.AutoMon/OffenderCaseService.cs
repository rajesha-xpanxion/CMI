using CMI.DAL.Source.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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

        public IEnumerable<OffenderCase> GetAllOffenderCases(DateTime lastExecutionDateTime)
        {
            if (sourceConfig.IsDevMode)
            {
                //test data
                return new List<OffenderCase>
                {
                    new OffenderCase
                    {
                        Pin = "5824",
                        FirstName = "John",
                        MiddleName = "Brent",
                        LastName = "Aitkens",

                        CaseNumber = "P15CRF0407",
                        CaseStatus = "Active",
                        CaseDate = DateTime.Now,

                        OffenseLabel = "11351.5 HS F",
                        OffenseStatute = "11351.5",
                        OffenseCategory = "F",
                        IsPrimary = false
                    },
                    new OffenderCase
                    {
                        Pin = "5824",
                        FirstName = "John",
                        MiddleName = "Brent",
                        LastName = "Aitkens",

                        CaseNumber = "P15CRF0407",
                        CaseStatus = "Active",
                        CaseDate = DateTime.Now,

                        OffenseLabel = "11357(A) HS  F",
                        OffenseStatute = "11357(A)",
                        OffenseCategory = "F",
                        IsPrimary = false
                    },
                    new OffenderCase
                    {
                        Pin = "7478",
                        FirstName = "John",
                        MiddleName= "Charles",
                        LastName = "Morrissey",

                        CaseNumber = "FOR-S11CRF0110-1",
                        CaseStatus = "Closed",
                        CaseDate = new DateTime(2013, 8, 9),

                        OffenseLabel = "11366.5(A) HS  F",
                        OffenseStatute = "11366.5(A)",
                        OffenseCategory = "F",
                        IsPrimary = false
                    }
                };
            }
            else
            {
                List<OffenderCase> offenderCases = new List<OffenderCase>();

                using (SqlConnection conn = new SqlConnection(sourceConfig.AutoMonDBConnString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = SQLQuery.GET_ALL_OFFENDER_CASE_DETAILS;
                        cmd.CommandType = System.Data.CommandType.Text;
                        cmd.Parameters.Add(new SqlParameter() { ParameterName = SQLParamName.LAST_EXECUTION_DATE_TIME, SqlDbType = System.Data.SqlDbType.DateTime, Value = lastExecutionDateTime });
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
