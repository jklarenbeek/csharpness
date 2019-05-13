using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;

// using System.Diagnostics;
// using System.Collections;
// using System.Collections.Generic;
// using System.IO;
// using System.Net.Mail;
// using System.Security.AccessControl;
// using System.Security.Principal;
// using System.Runtime.InteropServices;
// using System.Runtime.Serialization.Formatters.Binary;
// using System.Linq;
// using System.Data;
// using System.Data.Common;
// using System.Text;
// using System.Text.RegularExpressions;
// using Microsoft.Win32.SafeHandles;
// using System.Web.UI.WebControls;
// using System.Linq.Expressions;

namespace joham.cs_futils.data
{
    public class SqlDAL
    {
        public static string IsDebugSession
        {
            get
            {
#if DEBUG
                return "Debug";
#else
                return "Release";
#endif
            }
        }


        public static void PurgeDAL()
        {
#if DEBUG
            // purge cache!
            var enumerator = HttpRuntime.Cache.GetEnumerator();
            Dictionary<string, object> cacheItems = new Dictionary<string, object>();

            while (enumerator.MoveNext())
                cacheItems.Add(enumerator.Key.ToString(), enumerator.Value);

            foreach (string key in cacheItems.Keys)
                HttpRuntime.Cache.Remove(key);
#endif

        }

        private readonly String m_ConnectionName;
        private readonly System.Data.Common.DbProviderFactory m_DbFactory;

        public SqlDAL()
        {
            m_DbFactory = DbProviderFactories.GetFactory("System.Data.SqlClient");
        }

        public DbConnection CreateConnection()
        {
            DbConnection connect = m_DbFactory.CreateConnection();
            connect.ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings[m_ConnectionName].ConnectionString;
            return connect;
        }

        private DbCommand CreateCommand(string sql, DbConnection connect)
        {
            DbCommand cmd = m_DbFactory.CreateCommand();
            cmd.Connection = connect;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = sql;
            return cmd;
        }

        private DbDataAdapter CreateDataAdapter()
        {
            return m_DbFactory.CreateDataAdapter();
        }
        private DbDataAdapter CreateSelectAdapter(DbCommand cmd)
        {
            DbDataAdapter da = CreateDataAdapter();
            da.SelectCommand = cmd;
            return da;
        }

        public static string GetSafeString(string inputSQL)
        {
            return inputSQL.Replace("'", "''");
        }

        public DbParameter NewSqlParam(string columnName, string value)
        {
            DbParameter param = m_DbFactory.CreateParameter();
            param.ParameterName = columnName;
            if (String.IsNullOrWhiteSpace(value))
                param.Value = DBNull.Value;
            else
                param.Value = value;
            return param;

            /* if (String.IsNullOrWhiteSpace(value))
                return new SqlParameter(columnName, DBNull.Value);
            else
                return new SqlParameter(columnName, value); */
        }
        public DbParameter NewSqlParam(string columnName, int? value)
        {
            DbParameter param = m_DbFactory.CreateParameter();
            param.ParameterName = columnName;
            param.DbType = DbType.Int32;
            if (value == null)
                param.Value = DBNull.Value;
            else
                param.Value = value.Value;

            return param;

            /*
            if (value == null)
                return new SqlParameter(columnName, SqlDbType.Int) { Value = DBNull.Value };
            else
                return new SqlParameter(columnName, (int)value.Value);
             */
        }
        public DbParameter NewSqlParam(string columnName, double? value)
        {
            DbParameter param = m_DbFactory.CreateParameter();
            param.ParameterName = columnName;
            param.DbType = DbType.Double;
            if (value == null)
                param.Value = DBNull.Value;
            else
                param.Value = value.Value;

            return param;
            /*
            if (value == null)
                return new SqlParameter(columnName, SqlDbType.Float) { Value = DBNull.Value };
            else
                return new SqlParameter(columnName, (double)value.Value);
             */
        }
        public DbParameter NewSqlParam(string columnName, bool? value)
        {
            DbParameter param = m_DbFactory.CreateParameter();
            param.ParameterName = columnName;
            param.DbType = DbType.Boolean;
            if (value == null)
                param.Value = DBNull.Value;
            else
                param.Value = value.Value;

            return param;
            /*
            if (value == null)
                return new SqlParameter(columnName, DBNull.Value);
            else
                return new SqlParameter(columnName, (bool)value.Value);
             */
        }
        public DbParameter NewSqlParam(string columnName, DateTime? value)
        {
            DbParameter param = m_DbFactory.CreateParameter();
            param.ParameterName = columnName;
            param.DbType = DbType.DateTime;
            if (value == null)
                param.Value = DBNull.Value;
            else
                param.Value = value.Value;

            return param;

        }

        public List<string> SelectSQLStringList(string sql, string columnName, string columnValue)
        {
            return SelectSQLStringList(CreateConnection(), sql, columnName, columnValue);
        }
        public List<string> SelectSQLStringList(DbConnection conn, string sql, string columnName, string columnValue)
        {
            List<string> list = new List<string>();
            using (conn)
            {
                conn.Open();
                using (var cmd = CreateCommand(sql, conn))
                {
                    cmd.Parameters.Add(NewSqlParam(columnName, columnValue));
                    DbDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                    while (dr.Read())
                    {
                        list.Add(dr[columnName] as string);
                    }
                    return list;
                }
            }
        }

        public DataTable SelectSQLDataTable(DbConnection conn, string tableName, string sql, DbParameter[] sqlParams)
        {
            using (conn)
            {
                using (var cmd = CreateCommand(sql, conn))
                {
                    if (sqlParams != null)
                        cmd.Parameters.AddRange(sqlParams);

                    using (var da = CreateSelectAdapter(cmd))
                    {
                        DataTable data = new DataTable(tableName);
                        da.Fill(data);
                        return data;
                    }
                }
            }
        }
        public DataTable SelectSQLDataTable(string tableName, string sql, DbParameter[] sqlParams)
        {
            return SelectSQLDataTable(CreateConnection(), tableName, sql, sqlParams);
        }
        
        public object InsertSQLDataRecord(DbConnection conn, string sql, DbParameter[] sqlParams)
        {
            using (conn)
            {
                conn.Open();
                using (var cmd = CreateCommand(sql, conn))
                {
                    if (sqlParams != null)
                        cmd.Parameters.AddRange(sqlParams);

                    return cmd.ExecuteScalar();
                }
            }
        }
        public object InsertSQLDataRecord(string sql, DbParameter[] sqlParams)
        {
            return InsertSQLDataRecord(CreateConnection(), sql, sqlParams);
        }

        #region CONVERT HELPER FUNCTIONS
        #endregion

        #region CACHE HELPER FUNCTIONS
        protected T CacheGet<T>(object lockKey, string cacheKey, TimeSpan expires, Func<T> refresh)
            where T : class
        {
            T data = System.Web.HttpRuntime.Cache[cacheKey] as T;
            if (data == null)
            {
                lock (lockKey)
                {
                    data = System.Web.HttpRuntime.Cache[cacheKey] as T;
                    if (data != null)
                        return data;

                    data = refresh();
                    if (data != default(T))
                    {
                        System.Web.HttpRuntime.Cache.Insert(
                            cacheKey,
                            data,
                            null,
                            DateTime.Now.Add(expires),
                            System.Web.Caching.Cache.NoSlidingExpiration);
                    }
                }
            }
            return data;
        }
        protected void CacheRemove(object lockKey, string cacheKey)
        {
            lock (lockKey)
            {
                System.Web.HttpRuntime.Cache.Remove(cacheKey);
            }
        }

        public static DateTime CacheGetUtcExpiryDateTime(string cacheKey)
        {
            object cacheEntry = System.Web.HttpRuntime.Cache.GetType()
                .GetMethod("Get", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .Invoke(System.Web.HttpRuntime.Cache, new object[] { cacheKey, 1 });
            System.Reflection.PropertyInfo utcExpiresProperty = cacheEntry.GetType()
                .GetProperty("UtcExpires", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            DateTime utcExpiresValue = (DateTime)utcExpiresProperty.GetValue(cacheEntry, null);

            return utcExpiresValue;
        }

        #endregion


    }

}
