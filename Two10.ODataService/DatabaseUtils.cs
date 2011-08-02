using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using Simple.Data.Ado;
using Simple.Data;
using System.Text;

namespace Two10.ODataService
{
    public static class DatabaseUtils
    {
        public static SqlDataReader ExecuteReader(string query, string connectionString)
        {
            var connection = new SqlConnection(connectionString);
            var command = new SqlCommand(query, connection);
            {
                connection.Open();
                return command.ExecuteReader();

            }
        }

        public static string SelectTop(int n, string query)
        {
            return string.Format("SELECT TOP {0} * FROM ({1}) AS SubQuery", n, query);
        }

        public static string SelectWhere(string query, int top, string fields, string orderby, params Tuple<string, string, string>[] where)
        {
            StringBuilder sb = new StringBuilder();
            if (where.Length > 0)
            {
                sb.Append("\r\nWHERE\r\n");
                foreach (var item in where)
                {
                    string sqlOperator = ConvertOperator(item.Item2);
                    if (null != sqlOperator)
                    {
                        sb.Append(string.Format("[{0}] {1} '{2}'\r\n", item.Item1, sqlOperator, item.Item3));
                    }
                }
            }

            return string.Format("SELECT {0} {1} FROM ({2}) AS SubQuery {3}",
                top >= 0 ? "TOP " + top.ToString() : "",
                fields.Replace("'", string.Empty),
                query,
                sb.ToString());
        }

        private static string ConvertOperator(string value)
        {
            switch (value)
            {
                case "eq":
                    return "=";
                case "lt":
                    return "<";
                case "gt":
                    return ">";
                case "le":
                    return "<=";
                case "ge":
                    return ">=";
                case "ne":
                    return "!=";
            }
            return null;
        }

        public static Tuple<int, string> GetPK(SqlDataReader model)
        {
            for (int i = 0; i < model.FieldCount; i++)
            {
                if (model.GetName(i).EndsWith("id", StringComparison.InvariantCultureIgnoreCase))
                {
                    return new Tuple<int, string>(i, model.GetName(i));
                }
            }
            return null;
        }


    }
}