using Dapper;
using ManejoPresupuesto.Interface;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Net.NetworkInformation;

namespace ManejoPresupuesto.Repository
{
    public class TransaccionesRepository : ITransaccionesRepository
    {
        private readonly string _connectionString;
        public TransaccionesRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(Transaccion transaccion)
        {
            using var connection = new SqlConnection(_connectionString);
            var id = await connection.QuerySingleAsync<int>(@"TransaccionesInsertar", new
            {
                transaccion.IdUsuario,
                transaccion.IdCuenta,
                transaccion.IdCategoria,
                transaccion.FechaTransaccion,
                transaccion.Monto,
                transaccion.Nota
            },
            commandType: System.Data.CommandType.StoredProcedure);

            transaccion.Id = id;
        }

        public async Task Editar(Transaccion transaccion, decimal montoAnterior, int idCuentaAnterior)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync("TransaccionesEditar", new
            {
                transaccion.Id,
                transaccion.IdCuenta,
                idCuentaAnterior,
                transaccion.IdCategoria,
                transaccion.FechaTransaccion,
                transaccion.Monto,
                montoAnterior,
                transaccion.Nota
            }, commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<Transaccion> ObtenerPorId(int id, int idUsuario)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Transaccion>(@"SELECT t.*, c.IdTipoTransaccion 
                                                                        FROM Transacciones AS t
                                                                        INNER JOIN Categorias AS c ON c.Id = t.IdCategoria
                                                                        WHERE t.Id = @Id AND t.IdUsuario = @IdUsuario",
                                                                        new {id, idUsuario});
        }

        public async Task<IEnumerable<Transaccion>> ObtenerPorCuenaId(TransaccionesPorCuenta modelo)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<Transaccion>(@"
                                    SELECT t.Id, t.Monto, t.FechaTransaccion, c.NombreCategoria as Categoria, cu.NombreCuenta as Cuenta, c.IdTipoTransaccion
                                    FROM Transacciones as t
                                    INNER JOIN Categorias as c ON c.Id = t.IdCategoria
                                    INNER JOIN Cuentas as cu ON cu.Id = t.IdCuenta
                                    WHERE t.IdCuenta = @IdCuenta AND t.IdUsuario = @IdUsuario
                                    AND FechaTransaccion BETWEEN @FechaInicio AND @FechaFin", modelo);
        }

        public async Task<IEnumerable<Transaccion>> ObtenerPorUsuarioId(TransaccionesPorUsuario modelo)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<Transaccion>(@"
                                    SELECT t.Id, t.Monto, t.FechaTransaccion, c.NombreCategoria as Categoria, cu.NombreCuenta as Cuenta, c.IdTipoTransaccion, Nota
                                    FROM Transacciones as t
                                    INNER JOIN Categorias as c ON c.Id = t.IdCategoria
                                    INNER JOIN Cuentas as cu ON cu.Id = t.IdCuenta
                                    WHERE t.IdUsuario = @IdUsuario
                                    AND FechaTransaccion BETWEEN @FechaInicio AND @FechaFin
                                    ORDER BY t.FechaTransaccion DESC", modelo);
        }

        public async Task<IEnumerable<TransaccionesPorSemana>> ObtenerPorSemana(TransaccionesPorUsuario modelo)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<TransaccionesPorSemana>(@"
                                    SELECT DATEDIFF(d, @FechaInicio, FechaTransaccion) / 7 + 1 AS Semana, SUM(Monto) AS Monto, c.IdTipoTransaccion
                                    FROM Transacciones as t
                                    INNER JOIN Categorias as c ON c.Id = t.IdCategoria
                                    WHERE t.IdUsuario = @IdUsuario AND FechaTransaccion BETWEEN @FechaInicio AND @FechaFin
                                    GROUP BY DATEDIFF(d, @FechaInicio, FechaTransaccion) / 7, c.IdTipoTransaccion", modelo);
        }

        public async Task<IEnumerable<TransaccionesPorMes>> ObtenerPorMes(int idUsuario, int anio)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<TransaccionesPorMes>(@"
                                    SELECT MONTH(FechaTransaccion) as Mes, SUM(Monto) as Monto, c.IdTipoTransaccion
                                    FROM Transacciones as t
                                    INNER JOIN Categorias as c ON c.Id = t.IdCategoria
                                    WHERE t.IdUsuario = @IdUsuario AND YEAR(FechaTransaccion) = @Anio
                                    GROUP BY MONTH(FechaTransaccion), c.IdTipoTransaccion", new {idUsuario, anio});
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync("TransaccionesBorrar", new
            {id}, commandType: System.Data.CommandType.StoredProcedure);
        }
    }
}
