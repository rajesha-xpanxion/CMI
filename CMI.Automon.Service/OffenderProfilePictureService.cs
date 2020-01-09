using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using Microsoft.Extensions.Options;
using CMI.Automon.Interface;
using CMI.Automon.Model;

namespace CMI.Automon.Service
{
    public class OffenderProfilePictureService : IOffenderProfilePictureService
    {
        #region Private Member Variables
        private readonly AutomonConfig automonConfig;
        #endregion

        #region Constructor
        public OffenderProfilePictureService(
            IOptions<AutomonConfig> automonConfig
        )
        {
            this.automonConfig = automonConfig.Value;
        }
        #endregion

        #region Public Methods
        public OffenderMugshot GetOffenderMugshotPhoto(string CmiDbConnString, string pin)
        {
            OffenderMugshot offenderMugshot = null;

            if (automonConfig.IsDevMode)
            {
                //test data

                /*
                //test file
                using (FileStream fs = new FileStream(@"D:\Projects\AMS\POC\MyImageResizer1\MyImageResizer1\SampleInputs\car wall paper.jpg", FileMode.Open, FileAccess.Read))
                {
                    using (BinaryReader br = new BinaryReader(fs))
                    {
                        byte[] imageBytes = br.ReadBytes((int)fs.Length);
                        br.Close();
                        fs.Close();

                        return new OffenderMugshot { Pin = pin, DocumentId = new Random().Next(0, 10000), DocumentData = imageBytes };
                    }
                }
                */

                offenderMugshot = new OffenderMugshot { Pin = pin, DocumentId = new Random().Next(0, 10000) };
            }
            else
            {
                using (SqlConnection conn = new SqlConnection(CmiDbConnString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = StoredProc.GetOffenderMugshotPhoto;
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.AutomonDatabaseName,
                            SqlDbType = System.Data.SqlDbType.NVarChar,
                            Value = new SqlConnectionStringBuilder(automonConfig.AutomonDbConnString).InitialCatalog
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.Pin,
                            SqlDbType = System.Data.SqlDbType.VarChar,
                            Value = pin
                        });

                        cmd.Connection = conn;

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                offenderMugshot = new OffenderMugshot
                                {
                                    Pin = Convert.ToString(reader[DbColumnName.Pin]),
                                    DocumentId = Convert.ToInt32(reader[DbColumnName.DocumentId])
                                };

                                //document date
                                if (Convert.IsDBNull(reader[DbColumnName.DocumentDate]))
                                {
                                    offenderMugshot.DocumentDate = null;
                                }
                                else
                                {
                                    offenderMugshot.DocumentDate = (DateTime?)reader[DbColumnName.DocumentDate];
                                }
                            }
                        }
                    }
                }
            }

            return offenderMugshot;
        }

        public int SaveOffenderMugshotPhoto(string CmiDbConnString, OffenderMugshot offenderMugshotDetails)
        {
            if (automonConfig.IsDevMode)
            {
                string testDataJsonFileName = Path.Combine(automonConfig.TestDataJsonRepoPath, Constants.TestDataJsonFileNameAllOffenderMugshotDetails);

                //check if repository parent directory exists, if not then create
                if (!Directory.Exists(automonConfig.TestDataJsonRepoPath))
                {
                    Directory.CreateDirectory(automonConfig.TestDataJsonRepoPath);
                }

                //read existing objects
                List<OffenderMugshot> offenderMugshotDetailsList = File.Exists(testDataJsonFileName)
                    ? JsonConvert.DeserializeObject<List<OffenderMugshot>>(File.ReadAllText(testDataJsonFileName))
                    : new List<OffenderMugshot>();

                //merge
                offenderMugshotDetailsList.Add(offenderMugshotDetails);

                //write back
                File.WriteAllText(testDataJsonFileName, JsonConvert.SerializeObject(offenderMugshotDetailsList));

                return offenderMugshotDetails.Id == 0 ? new Random().Next(0, 10000) : offenderMugshotDetails.Id;
            }
            else
            {
                using (SqlConnection conn = new SqlConnection(CmiDbConnString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = StoredProc.SaveOffenderMugshotDetails;
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.AutomonDatabaseName,
                            SqlDbType = System.Data.SqlDbType.NVarChar,
                            Value = new SqlConnectionStringBuilder(automonConfig.AutomonDbConnString).InitialCatalog
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.Pin,
                            SqlDbType = System.Data.SqlDbType.VarChar,
                            Value = offenderMugshotDetails.Pin,
                            IsNullable = false
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.DocumentData,
                            SqlDbType = System.Data.SqlDbType.Image,
                            Value = offenderMugshotDetails.DocumentData,
                            IsNullable = true
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.DocumentId,
                            SqlDbType = System.Data.SqlDbType.Int,
                            Value = offenderMugshotDetails.DocumentId,
                            IsNullable = true
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.DocumentDate,
                            SqlDbType = System.Data.SqlDbType.DateTime,
                            Value = offenderMugshotDetails.DocumentDate,
                            IsNullable = true
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.UpdatedBy,
                            SqlDbType = System.Data.SqlDbType.VarChar,
                            Value = offenderMugshotDetails.UpdatedBy,
                            IsNullable = false
                        });

                        cmd.Connection = conn;

                        return Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }
        }

        public void DeleteOffenderMugshotPhoto(string CmiDbConnString, OffenderMugshot offenderMugshot)
        {
            if (automonConfig.IsDevMode)
            {
                //test data
                //string testDataJsonFileName = Path.Combine(automonConfig.TestDataJsonRepoPath, Constants.TestDataJsonFileNameAllOffenderNoteDetails);

                //return File.Exists(testDataJsonFileName)
                //    ? JsonConvert.DeserializeObject<IEnumerable<OffenderNote>>(File.ReadAllText(testDataJsonFileName))
                //    : new List<OffenderNote>();
            }
            else
            {
                using (SqlConnection conn = new SqlConnection(CmiDbConnString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = StoredProc.DeleteOffenderMushotPhoto;
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.AutomonDatabaseName,
                            SqlDbType = System.Data.SqlDbType.NVarChar,
                            Value = new SqlConnectionStringBuilder(automonConfig.AutomonDbConnString).InitialCatalog
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.Id,
                            SqlDbType = System.Data.SqlDbType.Int,
                            Value = offenderMugshot.Id,
                            IsNullable = false
                        });

                        cmd.Connection = conn;

                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
        #endregion
    }
}
