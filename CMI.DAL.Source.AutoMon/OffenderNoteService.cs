using CMI.DAL.Source.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Text;

namespace CMI.DAL.Source.AutoMon
{
    public class OffenderNoteService : IOffenderNoteService
    {
        SourceConfig sourceConfig;

        public OffenderNoteService(Microsoft.Extensions.Options.IOptions<SourceConfig> sourceConfig)
        {
            this.sourceConfig = sourceConfig.Value;
        }

        public IEnumerable<OffenderNote> GetAllOffenderNotes(string CMIDBConnString, DateTime? lastExecutionDateTime)
        {
            if (sourceConfig.IsDevMode)
            {
                //test data
                return JsonConvert.DeserializeObject<IEnumerable<OffenderNote>>(File.ReadAllText(Path.Combine(sourceConfig.TestDataJSONRepoPath, Constants.TEST_DATA_JSON_FILE_NAME_ALL_OFFENDER_NOTE_DETAILS)));
            }
            else
            {

                List<OffenderNote> offenderNotes = new List<OffenderNote>();

                using (SqlConnection conn = new SqlConnection(CMIDBConnString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = StoredProc.GET_ALL_OFFENDER_NOTE_DETAILS;
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
                                offenderNotes.Add(new OffenderNote()
                                {
                                    Pin = Convert.ToString(reader[DBColumnName.PIN]),
                                    Id = Convert.ToInt32(reader[DBColumnName.ID]),
                                    Date = Convert.ToDateTime(reader[DBColumnName.DATE]),
                                    Text = Convert.ToString(reader[DBColumnName.TEXT]),
                                    AuthorEmail = Convert.ToString(reader[DBColumnName.AUTHOR_EMAIL]),
                                    NoteType = Convert.ToString(reader[DBColumnName.NOTE_TYPE])
                                });
                            }
                        }
                    }
                }

                return offenderNotes;
            }
        }
    }
}
