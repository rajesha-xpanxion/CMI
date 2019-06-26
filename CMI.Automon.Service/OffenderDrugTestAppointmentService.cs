﻿using CMI.Automon.Interface;
using CMI.Automon.Model;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;

namespace CMI.Automon.Service
{
    public class OffenderDrugTestAppointmentService : IOffenderDrugTestAppointmentService
    {
        #region Private Member Variables
        private readonly AutomonConfig automonConfig;
        #endregion

        #region Constructor
        public OffenderDrugTestAppointmentService(
            IOptions<AutomonConfig> automonConfig
        )
        {
            this.automonConfig = automonConfig.Value;
        }
        #endregion

        #region Public Methods
        public void SaveOffenderDrugTestAppointmentDetails(string CmiDbConnString, OffenderDrugTestAppointment offenderDrugTestAppointmentDetails)
        {
            if (automonConfig.IsDevMode)
            {
                string testDataJsonFileName = Path.Combine(automonConfig.TestDataJsonRepoPath, Constants.TestDataJsonFileNameAllOffenderDrugTestAppointmentDetails);

                //check if repository parent directory exists, if not then create
                if (!Directory.Exists(automonConfig.TestDataJsonRepoPath))
                {
                    Directory.CreateDirectory(automonConfig.TestDataJsonRepoPath);
                }

                //read existing objects
                List<OffenderDrugTestAppointment> offenderDrugTestAppointmentDetailsList = File.Exists(testDataJsonFileName)
                    ? JsonConvert.DeserializeObject<List<OffenderDrugTestAppointment>>(File.ReadAllText(testDataJsonFileName))
                    : new List<OffenderDrugTestAppointment>();

                //merge
                offenderDrugTestAppointmentDetailsList.Add(offenderDrugTestAppointmentDetails);

                //write back
                File.WriteAllText(testDataJsonFileName, JsonConvert.SerializeObject(offenderDrugTestAppointmentDetailsList));
            }
            else
            {
                using (SqlConnection conn = new SqlConnection(CmiDbConnString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = StoredProc.SaveOffenderDrugTestAppointmentDetails;
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
                            Value = offenderDrugTestAppointmentDetails.Pin,
                            IsNullable = false
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.UpdatedBy,
                            SqlDbType = System.Data.SqlDbType.VarChar,
                            Value = offenderDrugTestAppointmentDetails.UpdatedBy,
                            IsNullable = false
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.StartDate,
                            SqlDbType = System.Data.SqlDbType.DateTime,
                            Value = offenderDrugTestAppointmentDetails.StartDate,
                            IsNullable = false
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.EndDate,
                            SqlDbType = System.Data.SqlDbType.DateTime,
                            Value = offenderDrugTestAppointmentDetails.EndDate,
                            IsNullable = false
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = SqlParamName.Status,
                            SqlDbType = System.Data.SqlDbType.Int,
                            Value = offenderDrugTestAppointmentDetails.Status,
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