using CMI.DAL.Source.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
                return new List<OffenderNote>()
                {
                    new OffenderNote {Pin="1000956", Id=70829,Date=Convert.ToDateTime("2008-08-07T13:52:41.43"),Text="Minor's brother's cell phone. (Johnathan Lemus)",AuthorEmail="jorge.soriano@edcgov.us",NoteType="Phone"},
                    new OffenderNote {Pin="1003906",Id=304163,Text="Father's cellular",AuthorEmail=null,Date=Convert.ToDateTime("4/4/2011 4:24:44 PM"),NoteType="Phone"}
                };
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
