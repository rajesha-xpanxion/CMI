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
            if (sourceConfig.IsDevMode)
            {
                //test data
                return new List<OffenderNote>()
                {
                    new OffenderNote()
                    {
                        Pin = "5824",
                        Id = 774140,
                        Date = Convert.ToDateTime("2018-08-13 03:25:30.480"),
                        Text = "test change on 13th August 2018 03:55 PM.",
                        AuthorEmail = "rajesha@xpanxion.co.in"
                    },
                    new OffenderNote()
                    {
                        Pin = "5824",
                        Id = 774141,
                        Date = Convert.ToDateTime("2018-07-03 01:09:26.863"),
                        Text = "test change on 07/03/2018 - 13:39",
                        AuthorEmail = "rajesha@xpanxion.co.in"
                    },
                    new OffenderNote()
                    {
                        Pin = "5824",
                        Id = 774142,
                        Date = Convert.ToDateTime("2018 - 07 - 03 01:51:18.847"),
                        Text = "test change on 07 / 03 / 2018 - 14:21",
                        AuthorEmail = "rajesha@xpanxion.co.in"
                    },
                    new OffenderNote()
                    {
                        Pin = "5824",
                        Id = 774143,
                        Date = Convert.ToDateTime("2018 - 07 - 03 02:22:45.110"),
                        Text = "test change on 07 / 03 / 2018 - 14:52",
                        AuthorEmail = "rajesha@xpanxion.co.in"
                    },
                    new OffenderNote()
                    {
                        Pin = "5824",
                        Id = 774144,
                        Date = Convert.ToDateTime("2018 - 08 - 10 07:09:07.860"),
                        Text = "test change on 10th August 2018 7:38 PM.",
                        AuthorEmail = "rajesha@xpanxion.co.in"
                    },
                    new OffenderNote()
                    {
                        Pin = "5824",
                        Id = 774145,
                        Date = Convert.ToDateTime("2018 - 08 - 13 02:22:07.893"),
                        Text = "test change on 13th August 2018 02:50 PM.",
                        AuthorEmail = "rajesha@xpanxion.co.in"
                    },
                    new OffenderNote()
                    {
                        Pin = "5824",
                        Id = 774146,
                        Date = Convert.ToDateTime("2018 - 08 - 13 02:25:49.933"),
                        Text = "test change on 13th August 2018 02:55 PM.",
                        AuthorEmail = "rajesha@xpanxion.co.in"
                    }
                };
            }
            else
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
