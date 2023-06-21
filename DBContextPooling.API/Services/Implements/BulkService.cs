using DBContextPooling.API.Models;
using DBContextPooling.API.Services.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics;

namespace DBContextPooling.API.Services.Implements
{
    public class BulkService : IBulkService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<BulkService> _logger;

        public BulkService(IConfiguration configuration, ILogger<BulkService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public DataTable ConvertListToDatatable(List<Customer> list)
        {
            var item = list.FirstOrDefault();
            DataTable dt = new DataTable();
            var properties = item.GetType().GetProperties();

            foreach (var prop in properties)
            {
                dt.Columns.Add(new DataColumn(prop.Name, prop.PropertyType));
            }

            foreach (var i in list)
            {
                var row = dt.NewRow();
                foreach (var prop in properties)
                {
                    row[prop.Name] = prop.GetValue(i, null);
                }
                dt.Rows.Add(row);
            }

            return dt;
        }

        public async Task ExecuteBulkCopyAsync(DataTable dataTable, object destinationTable, CancellationToken cancellationToken = default)
        {
            try
            {
                Stopwatch stopwatch = Stopwatch.StartNew();

                using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                connection.Open();

                using var transaction = connection.BeginTransaction();
                using var bulk = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction);
                bulk.BulkCopyTimeout = 60 * 5;
                bulk.DestinationTableName = "Customers";
                bulk.BatchSize = 2000;
                bulk.NotifyAfter = 1000;

                foreach (var map in MappingColumns(destinationTable))
                {
                    bulk.ColumnMappings.Add(map);
                }

                await bulk.WriteToServerAsync(dataTable, cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                stopwatch.Stop();

                _logger.LogInformation($"Bulk copy {dataTable.Rows.Count} items sucess in {stopwatch.ElapsedMilliseconds} ms");

            }
            catch (Exception ex)
            {
                _logger.LogError($"Error {ex.Message}");
                throw ex;
            }
        }

        public async Task<int> ExecuteBulkUpdateAsync(DataTable dataTable, object destinationTable, CancellationToken cancellationToken = default)
        {
            try
            {
                Stopwatch stopwatch = Stopwatch.StartNew();

                var columns = GetListColumns(destinationTable);
                string tempTableName = "#temptable_" + Guid.NewGuid().ToString("N");

                using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                connection.Open();

                _logger.LogInformation($"Create temp table: {tempTableName}");
                var sqlCreateTempTable = $"SELECT TOP 0 {string.Join(",", columns)} INTO {tempTableName} FROM Customers";
                var command = new SqlCommand(sqlCreateTempTable, connection);
                command.CommandTimeout = 120;

                await command.ExecuteNonQueryAsync(cancellationToken);
                _logger.LogInformation(sqlCreateTempTable);

                using var transaction = connection.BeginTransaction();
                using var bulk = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction);
                bulk.BulkCopyTimeout = 60 * 5;
                bulk.DestinationTableName = tempTableName;
                bulk.BatchSize = 2000;
                bulk.NotifyAfter = 1000;

                foreach (var map in MappingColumns(destinationTable))
                {
                    bulk.ColumnMappings.Add(map);
                }

                await bulk.WriteToServerAsync(dataTable, cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                var updateQuery = BuildUpdateQuery("Customers", tempTableName, destinationTable);

                command.CommandText = updateQuery;
                _logger.LogInformation(updateQuery);

                var result = await command.ExecuteNonQueryAsync(cancellationToken);

                stopwatch.Stop();

                _logger.LogInformation($"Update {result} items sucess in {stopwatch.ElapsedMilliseconds} ms");

                return result;

            }
            catch (Exception ex)
            {
                _logger.LogError($"Error {ex.Message}");
                throw ex;
            }
        }

        private List<string> GetListColumns(object o)
        {
            var columns = new List<string>();
            var properties = o.GetType().GetProperties();

            foreach (var prop in properties)
            {
                columns.Add(prop.Name);
            }
            return columns;
        }

        private List<SqlBulkCopyColumnMapping> MappingColumns(object o)
        {
            var columnMapping = new List<SqlBulkCopyColumnMapping>();
            var columns = GetListColumns(o);
            foreach (var column in columns)
            {
                columnMapping.Add(new SqlBulkCopyColumnMapping(column, column));
            }
            return columnMapping;
        }

        private string BuildUpdateQuery(string src, string dest, object o)
        {
            var columns = GetListColumns(o);

            var updatePhase = columns.Where(c => !c.Equals("Id")).Select(x => $"{src}.{x}={dest}.{x}");

            return string.Format(@"UPDATE {0} SET {1} FROM {0} INNER JOIN {2} ON {0}.ID = {2}.ID;", src, string.Join(",", updatePhase), dest);
        }
    }
}
