using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace CMI.Processor
{
    public static class Helper
    {
        public static DateTime GetLastExecutionDateTime(string dbConnString)
        {
            using (SqlConnection conn = new SqlConnection(dbConnString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "";
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.Connection = conn;

                    object result = cmd.ExecuteScalar();

                    return result is DBNull ? DateTime.Now.AddDays(-1) : Convert.ToDateTime(result);
                }
            }
        }
    }
}
