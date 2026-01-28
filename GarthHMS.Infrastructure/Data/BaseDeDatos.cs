using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using Npgsql;
using Microsoft.Extensions.Configuration;

namespace GarthHMS.Infrastructure.Data
{
    /// <summary>
    /// Clase base para manejo de conexiones a PostgreSQL
    /// Adaptada desde SQL Server a PostgreSQL con Npgsql
    /// </summary>
    public class BaseDeDatos
    {
        private readonly string _connectionString;

        public BaseDeDatos(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        /// <summary>
        /// Crea y abre una nueva conexión a PostgreSQL
        /// </summary>
        public async Task<NpgsqlConnection> GetConnectionAsync()
        {
            var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            return connection;
        }

        /// <summary>
        /// Crea una conexión sincrónica (para casos legacy)
        /// </summary>
        public NpgsqlConnection GetConnection()
        {
            var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            return connection;
        }

        /// <summary>
        /// Ejecuta un stored procedure que devuelve un DataTable
        /// </summary>
        public async Task<DataTable> ExecuteStoredProcedureAsync(string procedureName, params NpgsqlParameter[] parameters)
        {
            await using var connection = await GetConnectionAsync();
            await using var command = new NpgsqlCommand(procedureName, connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            if (parameters != null && parameters.Length > 0)
            {
                command.Parameters.AddRange(parameters);
            }

            var dataTable = new DataTable();
            await using var reader = await command.ExecuteReaderAsync();
            dataTable.Load(reader);

            return dataTable;
        }

        /// <summary>
        /// Ejecuta un stored procedure que devuelve un DataTable (versión sincrónica)
        /// </summary>
        public DataTable ExecuteStoredProcedure(string procedureName, params NpgsqlParameter[] parameters)
        {
            using var connection = GetConnection();
            using var command = new NpgsqlCommand(procedureName, connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            if (parameters != null && parameters.Length > 0)
            {
                command.Parameters.AddRange(parameters);
            }

            var dataTable = new DataTable();
            using var reader = command.ExecuteReader();
            dataTable.Load(reader);

            return dataTable;
        }

        /// <summary>
        /// Ejecuta un stored procedure que no devuelve resultados (INSERT, UPDATE, DELETE)
        /// </summary>
        public async Task<int> ExecuteNonQueryAsync(string procedureName, params NpgsqlParameter[] parameters)
        {
            await using var connection = await GetConnectionAsync();
            await using var command = new NpgsqlCommand(procedureName, connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            if (parameters != null && parameters.Length > 0)
            {
                command.Parameters.AddRange(parameters);
            }

            return await command.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Ejecuta un stored procedure que devuelve un valor escalar (COUNT, SUM, etc)
        /// </summary>
        public async Task<object?> ExecuteScalarAsync(string procedureName, params NpgsqlParameter[] parameters)
        {
            await using var connection = await GetConnectionAsync();
            await using var command = new NpgsqlCommand(procedureName, connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            if (parameters != null && parameters.Length > 0)
            {
                command.Parameters.AddRange(parameters);
            }

            return await command.ExecuteScalarAsync();
        }

        /// <summary>
        /// Ejecuta un stored procedure que devuelve un valor escalar de tipo T
        /// </summary>
        public async Task<T?> ExecuteScalarAsync<T>(string procedureName, params NpgsqlParameter[] parameters)
        {
            var result = await ExecuteScalarAsync(procedureName, parameters);

            if (result == null || result == DBNull.Value)
                return default;

            return (T)Convert.ChangeType(result, typeof(T));
        }

        /// <summary>
        /// Ejecuta un stored procedure con transacción
        /// </summary>
        public async Task<bool> ExecuteTransactionAsync(Func<NpgsqlConnection, NpgsqlTransaction, Task<bool>> operation)
        {
            await using var connection = await GetConnectionAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                var result = await operation(connection, transaction);

                if (result)
                {
                    await transaction.CommitAsync();
                    return true;
                }
                else
                {
                    await transaction.RollbackAsync();
                    return false;
                }
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Ejecuta múltiples stored procedures en una transacción
        /// </summary>
        public async Task<bool> ExecuteMultipleStoredProceduresAsync(
            params (string ProcedureName, NpgsqlParameter[] Parameters)[] procedures)
        {
            await using var connection = await GetConnectionAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                foreach (var (procedureName, parameters) in procedures)
                {
                    await using var command = new NpgsqlCommand(procedureName, connection, transaction)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    if (parameters != null && parameters.Length > 0)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    await command.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Crea un parámetro para PostgreSQL
        /// </summary>
        public static NpgsqlParameter CreateParameter(string name, object? value, NpgsqlTypes.NpgsqlDbType? dbType = null)
        {
            var parameter = new NpgsqlParameter(name, value ?? DBNull.Value);

            if (dbType.HasValue)
            {
                parameter.NpgsqlDbType = dbType.Value;
            }

            return parameter;
        }

        /// <summary>
        /// Crea un parámetro de salida
        /// </summary>
        public static NpgsqlParameter CreateOutputParameter(string name, NpgsqlTypes.NpgsqlDbType dbType)
        {
            return new NpgsqlParameter(name, dbType)
            {
                Direction = ParameterDirection.Output
            };
        }
    }
}
