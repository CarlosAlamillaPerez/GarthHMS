using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Npgsql;

namespace GarthHMS.Infrastructure.Data
{
    /// <summary>
    /// Clase para ejecutar stored procedures de PostgreSQL usando Dapper
    /// </summary>
    public class Procedimientos
    {
        private readonly BaseDeDatos _baseDeDatos;

        public Procedimientos(BaseDeDatos baseDeDatos)
        {
            _baseDeDatos = baseDeDatos;
        }

        /// <summary>
        /// Ejecuta un SP y retorna una lista de objetos
        /// </summary>
        public async Task<List<T>> EjecutarListaAsync<T>(string procedureName, object parameters = null)
        {
            try
            {
                using var connection = await _baseDeDatos.GetConnectionAsync();

                // PostgreSQL: SELECT * FROM function_name(param1, param2, ...)
                var sql = BuildPostgreSqlCall(procedureName, parameters);

                var result = await connection.QueryAsync<T>(sql, parameters);
                return result?.ToList() ?? new List<T>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en EjecutarListaAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Ejecuta un SP y retorna un único objeto
        /// </summary>
        public async Task<T> EjecutarUnicoAsync<T>(string procedureName, object parameters = null)
        {
            try
            {
                using var connection = await _baseDeDatos.GetConnectionAsync();

                // PostgreSQL: SELECT * FROM function_name(param1, param2, ...)
                var sql = BuildPostgreSqlCall(procedureName, parameters);

                var result = await connection.QueryFirstOrDefaultAsync<T>(sql, parameters);
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en EjecutarUnicoAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Ejecuta un SP sin retornar datos (INSERT/UPDATE/DELETE)
        /// </summary>
        public async Task<int> EjecutarAsync(string procedureName, object parameters = null)
        {
            try
            {
                using var connection = await _baseDeDatos.GetConnectionAsync();

                // PostgreSQL: SELECT function_name(param1, param2, ...)
                var sql = BuildPostgreSqlCall(procedureName, parameters, returnTable: false);

                var result = await connection.ExecuteAsync(sql, parameters);
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en EjecutarAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Ejecuta un SP y retorna un valor escalar
        /// </summary>
        public async Task<T> EjecutarEscalarAsync<T>(string procedureName, object parameters = null)
        {
            try
            {
                using var connection = await _baseDeDatos.GetConnectionAsync();

                // PostgreSQL: SELECT function_name(param1, param2, ...)
                var sql = BuildPostgreSqlCall(procedureName, parameters, returnTable: false);

                var result = await connection.ExecuteScalarAsync<T>(sql, parameters);
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en EjecutarEscalarAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Construye la llamada SQL correcta para PostgreSQL
        /// </summary>
        private string BuildPostgreSqlCall(string functionName, object parameters, bool returnTable = true)
        {
            if (parameters == null)
            {
                return returnTable
                    ? $"SELECT * FROM {functionName}()"
                    : $"SELECT {functionName}()";
            }

            // Obtener nombres de parámetros
            var props = parameters.GetType().GetProperties();
            var paramNames = string.Join(", ", props.Select(p => $"@{p.Name}"));

            return returnTable
                ? $"SELECT * FROM {functionName}({paramNames})"
                : $"SELECT {functionName}({paramNames})";
        }

        /// <summary>
        /// Ejecuta un SP y retorna DataTable (legacy support)
        /// </summary>
        public async Task<DataTable> EjecutarDataTableAsync(string procedureName, object parameters = null)
        {
            try
            {
                using var connection = await _baseDeDatos.GetConnectionAsync();

                var sql = BuildPostgreSqlCall(procedureName, parameters);

                var reader = await connection.ExecuteReaderAsync(sql, parameters);
                var dataTable = new DataTable();
                dataTable.Load(reader);

                return dataTable;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en EjecutarDataTableAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Ejecuta múltiples SPs en una transacción
        /// </summary>
        public async Task<bool> EjecutarTransaccionAsync(List<(string procedureName, object parameters)> commands)
        {
            using var connection = await _baseDeDatos.GetConnectionAsync();
            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                foreach (var (procedureName, parameters) in commands)
                {
                    var sql = BuildPostgreSqlCall(procedureName, parameters, returnTable: false);
                    await connection.ExecuteAsync(sql, parameters, transaction);
                }

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Error en EjecutarTransaccionAsync: {ex.Message}");
                throw;
            }
        }
    }
}