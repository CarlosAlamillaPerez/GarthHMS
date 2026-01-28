using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using Npgsql;
using Dapper;

namespace GarthHMS.Infrastructure.Data
{
    /// <summary>
    /// Clase helper para ejecutar stored procedures con mapeo automático
    /// Combina ADO.NET con Dapper para mayor flexibilidad
    /// </summary>
    public class Procedimientos
    {
        private readonly BaseDeDatos _baseDeDatos;

        public Procedimientos(BaseDeDatos baseDeDatos)
        {
            _baseDeDatos = baseDeDatos;
        }

        /// <summary>
        /// Ejecuta un SP y devuelve una lista de objetos del tipo T
        /// Usa Dapper para mapeo automático
        /// </summary>
        public async Task<List<T>> EjecutarListaAsync<T>(string procedureName, object? parameters = null)
        {
            await using var connection = await _baseDeDatos.GetConnectionAsync();

            var result = await connection.QueryAsync<T>(
                procedureName,
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result.ToList();
        }

        /// <summary>
        /// Ejecuta un SP y devuelve un único objeto del tipo T
        /// </summary>
        public async Task<T?> EjecutarUnicoAsync<T>(string procedureName, object? parameters = null)
        {
            await using var connection = await _baseDeDatos.GetConnectionAsync();

            var result = await connection.QueryFirstOrDefaultAsync<T>(
                procedureName,
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result;
        }

        /// <summary>
        /// Ejecuta un SP que no devuelve resultados (INSERT, UPDATE, DELETE)
        /// Devuelve el número de filas afectadas
        /// </summary>
        public async Task<int> EjecutarAsync(string procedureName, object? parameters = null)
        {
            await using var connection = await _baseDeDatos.GetConnectionAsync();

            var result = await connection.ExecuteAsync(
                procedureName,
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result;
        }

        /// <summary>
        /// Ejecuta un SP que devuelve un valor escalar
        /// </summary>
        public async Task<T?> EjecutarEscalarAsync<T>(string procedureName, object? parameters = null)
        {
            await using var connection = await _baseDeDatos.GetConnectionAsync();

            var result = await connection.ExecuteScalarAsync<T>(
                procedureName,
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result;
        }

        /// <summary>
        /// Ejecuta un SP que devuelve múltiples result sets
        /// </summary>
        public async Task<(List<T1>, List<T2>)> EjecutarMultipleAsync<T1, T2>(
            string procedureName,
            object? parameters = null)
        {
            await using var connection = await _baseDeDatos.GetConnectionAsync();

            using var multi = await connection.QueryMultipleAsync(
                procedureName,
                parameters,
                commandType: CommandType.StoredProcedure
            );

            var result1 = (await multi.ReadAsync<T1>()).ToList();
            var result2 = (await multi.ReadAsync<T2>()).ToList();

            return (result1, result2);
        }

        /// <summary>
        /// Ejecuta un SP con parámetros de salida
        /// </summary>
        public async Task<(int RowsAffected, DynamicParameters OutputParams)> EjecutarConSalidaAsync(
            string procedureName,
            DynamicParameters parameters)
        {
            await using var connection = await _baseDeDatos.GetConnectionAsync();

            var rowsAffected = await connection.ExecuteAsync(
                procedureName,
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return (rowsAffected, parameters);
        }

        /// <summary>
        /// Ejecuta múltiples SPs en una transacción
        /// </summary>
        public async Task<bool> EjecutarTransaccionAsync(
            params (string ProcedureName, object? Parameters)[] procedures)
        {
            await using var connection = await _baseDeDatos.GetConnectionAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                foreach (var (procedureName, parameters) in procedures)
                {
                    await connection.ExecuteAsync(
                        procedureName,
                        parameters,
                        transaction: transaction,
                        commandType: CommandType.StoredProcedure
                    );
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
        /// Ejecuta un SP y devuelve un DataTable (para casos legacy)
        /// </summary>
        public async Task<DataTable> EjecutarDataTableAsync(string procedureName, params NpgsqlParameter[] parameters)
        {
            return await _baseDeDatos.ExecuteStoredProcedureAsync(procedureName, parameters);
        }

        /// <summary>
        /// Helper: Crea parámetros de Dapper dinámicos
        /// </summary>
        public static DynamicParameters CrearParametros(object? parameters = null)
        {
            var dynamicParams = new DynamicParameters();

            if (parameters != null)
            {
                dynamicParams.AddDynamicParams(parameters);
            }

            return dynamicParams;
        }

        /// <summary>
        /// Helper: Agrega un parámetro de salida
        /// </summary>
        public static void AgregarParametroSalida(DynamicParameters parameters, string name, DbType dbType)
        {
            parameters.Add(name, dbType: dbType, direction: ParameterDirection.Output);
        }
    }
}
