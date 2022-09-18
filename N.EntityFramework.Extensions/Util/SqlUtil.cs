using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;

namespace N.EntityFramework.Extensions
{

    internal static class SqlUtil
    {
        static SqlUtil()
        {
            StringIgnoreCaseEqualityComparer = new StringIgnoreCaseEqualityComparer();
            var reservedKeywords = "ADD,EXTERNAL,PROCEDURE,ALL,FETCH,PUBLIC,ALTER,FILE,RAISERROR,AND,FILLFACTOR,READ,ANY,FOR,READTEXT,AS,FOREIGN,RECONFIGURE,ASC,FREETEXT,REFERENCES,AUTHORIZATION,FREETEXTTABLE,REPLICATION,BACKUP,FROM,RESTORE,BEGIN,FULL,RESTRICT,BETWEEN,FUNCTION,RETURN,BREAK,GOTO,REVERT,BROWSE,GRANT,REVOKE,BULK,GROUP,RIGHT,BY,HAVING,ROLLBACK,CASCADE,HOLDLOCK,ROWCOUNT,CASE,IDENTITY,ROWGUIDCOL,CHECK,IDENTITY_INSERT,RULE,CHECKPOINT,IDENTITYCOL,SAVE,CLOSE,IF,SCHEMA,CLUSTERED,IN,SECURITYAUDIT,COALESCE,INDEX,SELECT,COLLATE,INNER,SEMANTICKEYPHRASETABLE,COLUMN,INSERT,SEMANTICSIMILARITYDETAILSTABLE,COMMIT,INTERSECT,SEMANTICSIMILARITYTABLE,COMPUTE,INTO,SESSION_USER,CONSTRAINT,IS,SET,CONTAINS,JOIN,SETUSER,CONTAINSTABLE,KEY,SHUTDOWN,CONTINUE,KILL,SOME,CONVERT,LEFT,STATISTICS,CREATE,LIKE,SYSTEM_USER,CROSS,LINENO,TABLE,CURRENT,LOAD,TABLESAMPLE,CURRENT_DATE,MERGE,TEXTSIZE,CURRENT_TIME,NATIONAL,THEN,CURRENT_TIMESTAMP,NOCHECK,TO,CURRENT_USER,NONCLUSTERED,TOP,CURSOR,NOT,TRAN,DATABASE,NULL,TRANSACTION,DBCC,NULLIF,TRIGGER,DEALLOCATE,OF,TRUNCATE,DECLARE,OFF,TRY_CONVERT,DEFAULT,OFFSETS,TSEQUAL,DELETE,ON,UNION,DENY,OPEN,UNIQUE,DESC,OPENDATASOURCE,UNPIVOT,DISK,OPENQUERY,UPDATE,DISTINCT,OPENROWSET,UPDATETEXT,DISTRIBUTED,OPENXML,USE,DOUBLE,OPTION,USER,DROP,OR,VALUES,DUMP,ORDER,VARYING,ELSE,OUTER,VIEW,END,OVER,WAITFOR,ERRLVL,PERCENT,WHEN,ESCAPE,PIVOT,WHERE,EXCEPT,PLAN,WHILE,EXEC,PRECISION,WITH,EXECUTE,PRIMARY,WITHIN GROUP,EXISTS,PRINT,WRITETEXT,EXIT,PROC";
            ReservedKeywords = reservedKeywords
                .Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                .Select(item => item.Trim())
                .ToArray();
        }

        internal static string[] ReservedKeywords;
        internal static StringIgnoreCaseEqualityComparer StringIgnoreCaseEqualityComparer;

        internal static int ExecuteSql(string query, SqlConnection connection, SqlTransaction transaction, int? commandTimeout)
        {
            return SqlUtil.ExecuteSql(query, connection, transaction, null, commandTimeout);
        }

        internal static int ExecuteSql(string query, SqlConnection connection, SqlTransaction transaction, object[] parameters = null, int? commandTimeout = null)
        {
            using (var sqlCommand = new SqlCommand(query, connection, transaction))
            {
                if (commandTimeout.HasValue)
                {
                    sqlCommand.CommandTimeout = commandTimeout.Value;
                }
                else
                {

                }

                if (parameters != null)
                {
                    sqlCommand.Parameters.AddRange(parameters);
                }

                return sqlCommand.ExecuteNonQuery();
            }
        }

        internal static object ExecuteScalar(string query, SqlConnection connection, SqlTransaction transaction, object[] parameters = null, int? commandTimeout = null)
        {
            using(var sqlCommand = new SqlCommand(query, connection, transaction))
            {
                if (commandTimeout.HasValue)
                {
                    sqlCommand.CommandTimeout = commandTimeout.Value;
                }    
                if (parameters != null)
                {

                    sqlCommand.Parameters.AddRange(parameters);
                }
                return sqlCommand.ExecuteScalar();
            }
        }

        internal static int DeleteTable(string tableName, SqlConnection connection, SqlTransaction transaction, int? commandTimeout = null)
        {
            return ExecuteSql(string.Format("DROP TABLE {0}", tableName), connection, transaction, commandTimeout);
        }
        internal static int CloneTable(
            string sourceTable, 
            string destinationTable, 
            string[] columnNames, 
            SqlConnection connection, 
            SqlTransaction transaction,
            BulkOptions bulkOptions, 
            string internalIdColumnName = null)
        {

#if CHECK_PERFOMANCE
            var stopwatch = Stopwatch.StartNew();
            DbContextExtensions.LogToDebug(bulkOptions.OperationId, $"Start clone table. From '{sourceTable}' to '{destinationTable}'");
#endif

            string columns = columnNames != null && columnNames.Length > 0 ? ConvertToColumnString(columnNames) : "*";
            columns = !string.IsNullOrWhiteSpace(internalIdColumnName) ? string.Format("{0},CAST( NULL AS INT) AS {1}",columns, internalIdColumnName) : columns;

            var result = ExecuteSql(string.Format("SELECT TOP 0 {0} INTO {1} FROM {2}", columns, destinationTable, sourceTable), connection, transaction, bulkOptions.CommandTimeout);

#if CHECK_PERFOMANCE
            DbContextExtensions.LogToDebug(bulkOptions.OperationId, $"Finished clone table. From '{sourceTable}' to '{destinationTable}'", stopwatch.Elapsed);
#endif
            return result;

        }
        internal static string ConvertToColumnString(IEnumerable<string> columnNames)
        {
            var fixedColumnNames = ReplaceReservedKeywords(columnNames);
            return string.Join(",", fixedColumnNames);
        }

        internal static IEnumerable<string> ReplaceReservedKeywords(IEnumerable<string> columnNames) =>
            columnNames.Select(item => ReservedKeywords.Contains(item, StringIgnoreCaseEqualityComparer) ? $"[{item}]" : item);

        internal static int ToggleIdentityInsert(bool enable, string tableName, SqlConnection dbConnection, SqlTransaction dbTransaction)
        {
            string boolString = enable ? "ON" : "OFF";
            return ExecuteSql(string.Format("SET IDENTITY_INSERT {0} {1}", tableName, boolString), dbConnection, dbTransaction, null);
        }

        internal static bool TableExists(string tableName, SqlConnection dbConnection, SqlTransaction dbTransaction)
        {
            return Convert.ToBoolean(ExecuteScalar(string.Format("SELECT CASE WHEN OBJECT_ID(N'{0}', N'U') IS NOT NULL THEN 1 ELSE 0 END", tableName), 
                dbConnection, dbTransaction, null));
        }
    }
}

