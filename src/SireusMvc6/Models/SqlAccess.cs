using Microsoft.AspNet.Http;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;

namespace SireusMvc6.Models
{
    public class SqlAccess
    {
        public static string GetUserId()
        {
            //if (HttpContext.Current.Request.UserHostAddress == null) return null;
            //var ipSource = HttpContext.Current.Request.UserHostAddress;
            var ipSource = "1.0.0.127";
            var functionReturnValue = IP_Exists(ipSource) ? "-1" : GetUserID_Increment().ToString();
            return functionReturnValue;
        }

        Action<long> example1 =
        (long id) =>
        {
            using (
    var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["Counters"].ConnectionString))
            {
                using (var command = new SqlCommand("UpdateUserIDs", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ID", id));
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
        };

        public static void UpdateUserIDs(long id)
        {
            using (
                var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["Counters"].ConnectionString))
            {
                using (var command = new SqlCommand("UpdateUserIDs", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ID", id));
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
        }


        public static void IncreaseVacationGadget()
        {
            using (
                var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["Counters"].ConnectionString))
            {
                using (var command = new SqlCommand("IncreaseVacationGadget", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
        }

        public static int GetIntFromSql(int i)
        {
            using (
                var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["Counters"].ConnectionString))
            {
                using (var command = new SqlCommand("ObscureProcedure", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    var I = new SqlParameter("@i", i) {Direction = ParameterDirection.Input};
                    command.Parameters.Add(I);
                    connection.Open();
                    var result = (int) command.ExecuteScalar();
                    try
                    {
                        return result;
                    }
                    catch (ArgumentNullException e)
                    {
                        var list = new List<VacationGadget>();
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var temp = new VacationGadget(Convert.ToInt32(reader["ObscureCount"]));
                                list.Add(temp);
                            }
                            connection.Close();
                        }
                        try
                        {
                            return list[0].ObscureCount;
                        }
                        catch (ArgumentOutOfRangeException ee)
                        {
                            return -1;
                        }
                    }
                }
            }
        }

        public static long GetUserID_Increment()
        {
            const long rc = 0;
            using (
                var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["Counters"].ConnectionString))
            {
                using (var command = new SqlCommand("GetUserID_Increment", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    var rowCnt = new SqlParameter("Nbr", rc) {Direction = ParameterDirection.ReturnValue};
                    command.Parameters.Add(rowCnt);
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                    return Convert.ToInt64(rowCnt.Value);
                }
            }
        }

        public static bool IP_UI_Exists(string ipUi)
        {
            const long rc = 0;
            using (
                var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["Counters"].ConnectionString))
            {
                using (var command = new SqlCommand("IP_UI_Exists", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@IP_UI", ipUi));
                    var rowCnt = new SqlParameter("Nbr", rc) {Direction = ParameterDirection.ReturnValue};
                    command.Parameters.Add(rowCnt);
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                    return Convert.ToInt64(rowCnt.Value) > 0;
                }
            }
        }

        public static bool IP_Exists(string IP)
        {
            const long rc = 0;
            using (
                var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["Counters"].ConnectionString))
            {
                using (var command = new SqlCommand("IP_Exists", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@IP", IP));
                    var rowCnt = new SqlParameter("Nbr", rc) {Direction = ParameterDirection.ReturnValue};
                    command.Parameters.Add(rowCnt);
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                    return Convert.ToInt64(rowCnt.Value) > 0;
                }
            }
        }

        public static long AddTotal(string ipUi)
        {
            const long rc = 0;
            using (
                var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["Counters"].ConnectionString))
            {
                using (var command = new SqlCommand("AddTotal", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@IP_UI", ipUi));
                    var rowCnt = new SqlParameter("Nbr", rc) {Direction = ParameterDirection.ReturnValue};
                    command.Parameters.Add(rowCnt);
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                    return Convert.ToInt64(rowCnt.Value);
                }
            }
        }

        public static long IncrementTotal(string p, string ipUi)
        {
            const long rc = 0;
            using (
                var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["Counters"].ConnectionString))
            {
                using (var command = new SqlCommand("Increment" + p, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@IP_UI", ipUi));
                    var rowCnt = new SqlParameter("Nbr", rc) {Direction = ParameterDirection.ReturnValue};
                    command.Parameters.Add(rowCnt);
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                    return Convert.ToInt64(rowCnt.Value);
                }
            }
        }

        public static long DecrementTotal(string p, string ipUi)
        {
            const long rc = 0;
            using (
                var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["Counters"].ConnectionString))
            {
                using (var command = new SqlCommand("Decrement" + p, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@IP_UI", ipUi));
                    var rowCnt = new SqlParameter("Nbr", rc) {Direction = ParameterDirection.ReturnValue};
                    command.Parameters.Add(rowCnt);
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                    return Convert.ToInt64(rowCnt.Value);
                }
            }
        }

        public static long CountTotal(string P)
        {
            const long rc = 0;
            using (
                var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["Counters"].ConnectionString))
            {
                using (var command = new SqlCommand(P + "Count", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    var rowCnt = new SqlParameter("Nbr", rc) {Direction = ParameterDirection.ReturnValue};
                    command.Parameters.Add(rowCnt);
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                    return Convert.ToInt64(rowCnt.Value);
                }
            }
        }

        public static long CountVisitor(string p)
        {
            const long rc = 0;
            using (
                var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["Counters"].ConnectionString))
            {
                using (var command = new SqlCommand("CountVisitor", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@Page", p));
                    var rowCnt = new SqlParameter("Nbr", rc) {Direction = ParameterDirection.ReturnValue};
                    command.Parameters.Add(rowCnt);
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                    return Convert.ToInt64(rowCnt.Value);
                }
            }
        }

        public static long CheckVisitorTimeOut(string IP_UI, DateTime N)
        {
            const long rc = 0;
            using (
                var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["Counters"].ConnectionString))
            {
                using (var command = new SqlCommand("CheckVisitorTimeOut", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@IP_UI", IP_UI));
                    command.Parameters.Add(new SqlParameter("@Now", N));
                    var rowCnt = new SqlParameter("RowCnt", rc) {Direction = ParameterDirection.ReturnValue};
                    command.Parameters.Add(rowCnt);
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                    return Convert.ToInt64(rowCnt.Value);
                }
            }
        }

        public static long UpdateVisitor(string ipUi, string p, DateTime n, string L)
        {
            const long rc = 0;
            using (
                var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["Counters"].ConnectionString))
            {
                using (var command = new SqlCommand("UpdateVisitor", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@IP_UI", ipUi));
                    command.Parameters.Add(new SqlParameter("@Page", p));
                    command.Parameters.Add(new SqlParameter("@Time", n));
                    command.Parameters.Add(new SqlParameter("@Language", L));
                    var rowCnt = new SqlParameter("RowCnt", rc) {Direction = ParameterDirection.ReturnValue};
                    command.Parameters.Add(rowCnt);
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                    return Convert.ToInt64(rowCnt.Value);
                }
            }
        }

        public static long AddVisitor(string ipUi)
        {
            const long rc = 0;
            using (
                var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["Counters"].ConnectionString))
            {
                using (var command = new SqlCommand("AddVisitor", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@IP_UI", ipUi));
                    var rowCnt = new SqlParameter("RowCnt", rc) {Direction = ParameterDirection.ReturnValue};
                    command.Parameters.Add(rowCnt);
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                    return Convert.ToInt64(rowCnt.Value);
                }
            }
        }

        public static long PopulateVisitorTable(string ipUi, string p, DateTime dt, string l)
        {
            const long rc = 0;
            using (
                var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["Counters"].ConnectionString))
            {
                using (var command = new SqlCommand("AddVisitor", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@IP_UI", ipUi));
                    command.Parameters.Add(new SqlParameter("@Page", p));
                    command.Parameters.Add(new SqlParameter("@Time", dt));
                    command.Parameters.Add(new SqlParameter("@Language", l));
                    var rowCnt = new SqlParameter("RowCnt", rc) {Direction = ParameterDirection.ReturnValue};
                    command.Parameters.Add(rowCnt);
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                    return Convert.ToInt64(rowCnt.Value);
                }
            }
        }

        public static long PopulateTotalTable(string ipUi, string hc, string rc, string liC, string ltc)
        {
            const long r = 0;

            using (
                var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["Counters"].ConnectionString))
            {
                using (var command = new SqlCommand("AddTotal", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@IP_UI", ipUi));
                    command.Parameters.Add(new SqlParameter("@HomeCnt", hc));
                    command.Parameters.Add(new SqlParameter("@ResumeCnt", rc));
                    command.Parameters.Add(new SqlParameter("@LinksCnt", liC));
                    command.Parameters.Add(new SqlParameter("@LTBCnt", ltc));
                    var rowCnt = new SqlParameter("RowCnt", r) {Direction = ParameterDirection.ReturnValue};
                    command.Parameters.Add(rowCnt);
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                    return Convert.ToInt64(rowCnt.Value);
                }
            }
        }
    }
}