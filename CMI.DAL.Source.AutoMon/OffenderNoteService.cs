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

        public IEnumerable<OffenderNote> GetAllOffenderNotes(DateTime lastExecutionDateTime)
        {

            
            List<OffenderNote> offenderNotes = new List<OffenderNote>();

            using (SqlConnection conn = new SqlConnection(sourceConfig.AutoMonDBConnString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = SQLQuery.GET_ALL_OFFENDER_NOTE_DETAILS;
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.Parameters.Add(new SqlParameter() { ParameterName = SQLParamName.LAST_EXECUTION_DATE_TIME, SqlDbType = System.Data.SqlDbType.DateTime, Value = lastExecutionDateTime });
                    cmd.Connection = conn;

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            offenderNotes.Add(new OffenderNote()
                            {
                                Pin = Convert.ToString(reader[DBColumnName.PIN]),
                                NoteId = Convert.ToInt32(reader[DBColumnName.NOTE_ID]),
                                NoteDate = Convert.ToDateTime(reader[DBColumnName.NOTE_DATE]),
                                Value = Convert.ToString(reader[DBColumnName.VALUE]),
                                Logon = Convert.ToString(reader[DBColumnName.LOGON]),
                                Email = Convert.ToString(reader[DBColumnName.EMAIL])
                            });
                        }
                    }
                }
            }

            return offenderNotes;
            

            /*
            //test data
            return new List<OffenderNote>()
            {
                new OffenderNote()
                {
                    Pin = "5824",

                    NoteId = 774140,
                    NoteDate = Convert.ToDateTime("2018-07-03 02:22:45.110"),
                    Value = "test change on 07/03/2018 - 14:52",
                    Logon = "rawate",
                    Email = "rajesha@xpanxion.co.in"
                }
            };
            */
        }
    }
}
