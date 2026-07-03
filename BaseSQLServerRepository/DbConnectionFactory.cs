using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace BaseSQLServerRepository;

public class DbConnectionFactory(string connectionString, ILogger<DbConnectionFactory> logger) : IDbConnectionFactory
{
    private async Task<IDbConnection> GetNewConnectionAsync()
    {
        try
        {
            DbConnection dbConnection = new SqlConnection(connectionString);
            await dbConnection.OpenAsync();
            return dbConnection;
        }
        catch (Exception e)
        {
            e.Data["BaseDao.Message-CreateDbConnection"] = "Not new SqlConnection";
            //e.Data["BaseDao.ConnectionString"] = connectionString;
            logger.LogError(e, "Exception {Message}", e.Message);
            throw;
        }
    }

    public async Task<T> WithConnection<T>(Func<IDbConnection, Task<T>> getData)
    {
        try
        {
            using var dbConnection = await GetNewConnectionAsync();
            return await getData(dbConnection);
        }
        catch (TimeoutException ex)
        {
            ex.Data["BaseDao.Message-WithConnection"] = "Execute TimeoutException";
            logger.LogError(ex, "TimeoutException {Message}", ex.Message);
            throw;
        }
        catch (SqlException ex)
        {
            ex.Data["BaseDao.Message-WithConnection"] = "Execute SqlException";
            logger.LogError(ex, "SqlException  {Type} {Message}", ex.GetType(), ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            ex.Data["BaseDao.Message-WithConnection"] = "Execute Exception";
            logger.LogError(ex, "Exception  {Type} {Message}", ex.GetType(), ex.Message);
            throw;
        }
    }

    public async Task WithConnection(Func<IDbConnection, Task> getData)
    {
        try
        {
            using var dbConnection = await GetNewConnectionAsync();
            await getData(dbConnection);
        }
        catch (TimeoutException ex)
        {
            ex.Data["BaseDao.Message-WithConnection"] = "Execute TimeoutException";
            logger.LogError(ex, "TimeoutException  {Type} {Message}", ex.GetType(), ex.Message);
            throw;
        }
        catch (SqlException ex)
        {
            ex.Data["BaseDao.Message-WithConnection"] = "Execute SqlException";
            logger.LogError(ex, "SqlException  {Type} {Message}", ex.GetType(), ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            ex.Data["BaseDao.Message-WithConnection"] = "Execute Exception";
            logger.LogError(ex, "Exception  {Type} {Message}", ex.GetType(), ex.Message);
            throw;
        }
    }

    public async Task<T> WithConnection<T>(Func<IDbConnection, IDbTransaction, Task<T>> getData)
    {
        try
        {
            using var dbConnection = await GetNewConnectionAsync();
            using IDbTransaction transaction = dbConnection.BeginTransaction();
            try
            {
                var result = await getData(dbConnection, transaction);
                transaction.Commit();
                return result;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                ex.Data["BaseDao.Message-WithConnection"] = "Execute Transaction Exception";
                logger.LogError(ex, "Exception  {Type} {Message}", ex.GetType(), ex.Message);
                throw;
            }
        }
        catch (TimeoutException ex)
        {
            ex.Data["BaseDao.Message-WithConnection"] = "Execute Transaction TimeoutException";
            logger.LogError(ex, "TimeoutException  {Type} {Message}", ex.GetType(), ex.Message);
            throw;
        }
        catch (SqlException ex)
        {
            ex.Data["BaseDao.Message-WithConnection"] = "Execute Transaction SqlException";
            logger.LogError(ex, "SqlException  {Type} {Message}", ex.GetType(), ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            ex.Data["BaseDao.Message-WithConnection"] = "Execute Transaction Exception";
            logger.LogError(ex, "Exception  {Type} {Message}", ex.GetType(), ex.Message);
            throw;
        }
    }

    public async Task WithConnection(Func<IDbConnection, IDbTransaction, Task> getData)
    {
        try
        {
            using var dbConnection = await GetNewConnectionAsync();
            using IDbTransaction transaction = dbConnection.BeginTransaction();
            try
            {
                await getData(dbConnection, transaction);
                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                ex.Data["BaseDao.Message-WithConnection"] = "Execute Transaction Exception";
                logger.LogError(ex, "Exception  {Type} {Message}", ex.GetType(), ex.Message);
                throw;
            }
        }
        catch (TimeoutException ex)
        {
            ex.Data["BaseDao.Message-WithConnection"] = "Execute Transaction TimeoutException";
            logger.LogError(ex, "TimeoutException  {Type} {Message}", ex.GetType(), ex.Message);
            throw;
        }
        catch (SqlException ex)
        {
            ex.Data["BaseDao.Message-WithConnection"] = "Execute Transaction SqlException";
            logger.LogError(ex, "SqlException  {Type} {Message}", ex.GetType(), ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            ex.Data["BaseDao.Message-WithConnection"] = "Execute Transaction Exception";
            logger.LogError(ex, "Exception  {Type} {Message}", ex.GetType(), ex.Message);
            throw;
        }
    }

    public async Task BulkCopy(DataTable table)
    {
        try
        {
            using var dbConnection = await GetNewConnectionAsync();
            SqlBulkCopy bulkCopy = new SqlBulkCopy((SqlConnection)dbConnection,
                SqlBulkCopyOptions.TableLock | SqlBulkCopyOptions.UseInternalTransaction, null)
            {
                DestinationTableName = table.TableName,
                BatchSize = 1000,
            };
            foreach (DataColumn tableColumn in table.Columns)
            {
                bulkCopy.ColumnMappings.Add(tableColumn.ColumnName, tableColumn.ColumnName);
            }

            await bulkCopy.WriteToServerAsync(table);
            table.Clear();
        }
        catch (Exception ex)
        {
            ex.Data["BaseDao.Message-BulkCopy"] = "BulkCopy Exception";
            logger.LogError(ex, "Exception  {Type} {Message}", ex.GetType(), ex.Message);
            throw;
        }
    }

    public async Task BulkCopy(DataTable table, IDbConnection connection, IDbTransaction transaction)
    {
        try
        {
            SqlBulkCopy bulkCopy = new SqlBulkCopy((SqlConnection)connection, SqlBulkCopyOptions.TableLock,
                (SqlTransaction)transaction)
            {
                DestinationTableName = table.TableName,
                BatchSize = 1000
            };
            foreach (DataColumn tableColumn in table.Columns)
            {
                bulkCopy.ColumnMappings.Add(tableColumn.ColumnName, tableColumn.ColumnName);
            }

            await bulkCopy.WriteToServerAsync(table);
            table.Clear();
        }
        catch (Exception ex)
        {
            ex.Data["BaseDao.Message-BulkCopy"] = "BulkCopy Exception";
            logger.LogError(ex, "Exception  {Type} {Message}", ex.GetType(), ex.Message);
            throw;
        }
    }
}