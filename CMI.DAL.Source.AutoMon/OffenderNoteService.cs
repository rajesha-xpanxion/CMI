﻿using CMI.DAL.Source.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using Microsoft.Extensions.Options;

namespace CMI.DAL.Source.AutoMon
{
    public class OffenderNoteService : IOffenderNoteService
    {
        #region Private Member Variables
        private readonly SourceConfig sourceConfig;
        #endregion

        #region Constructor
        public OffenderNoteService(
            IOptions<SourceConfig> sourceConfig
        )
        {
            this.sourceConfig = sourceConfig.Value;
        }
        #endregion

        #region Public Methods
        public IEnumerable<OffenderNote> GetAllOffenderNotes(string CmiDbConnString, DateTime? lastExecutionDateTime)
        {
            if (sourceConfig.IsDevMode)
            {
                //test data
                string testDataJsonFileName = Path.Combine(sourceConfig.TestDataJsonRepoPath, Constants.TestDataJsonFileNameAllOffenderNoteDetails);

                return File.Exists(testDataJsonFileName)
                    ? JsonConvert.DeserializeObject<IEnumerable<OffenderNote>>(File.ReadAllText(testDataJsonFileName))
                    : new List<OffenderNote>();
            }
            else
            {
                List<OffenderNote> offenderNotes = new List<OffenderNote>();

                using (SqlConnection conn = new SqlConnection(CmiDbConnString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = StoredProc.GetAllOffenderNoteDetails;
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
                                offenderNotes.Add(new OffenderNote()
                                {
                                    Pin = Convert.ToString(reader[DbColumnName.Pin]),
                                    Id = Convert.ToInt32(reader[DbColumnName.Id]),
                                    Date = Convert.ToDateTime(reader[DbColumnName.Date]),
                                    Text = Convert.ToString(reader[DbColumnName.Text]),
                                    AuthorEmail = Convert.ToString(reader[DbColumnName.AuthorEmail]),
                                    NoteType = Convert.ToString(reader[DbColumnName.NoteType])
                                });
                            }
                        }
                    }
                }

                return offenderNotes;
            }
        }
        #endregion
    }
}
