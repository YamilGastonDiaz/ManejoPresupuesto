using Dapper;
using ManejoPresupuesto.Interface;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

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

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync("TransaccionesBorrar", new
            {id}, commandType: System.Data.CommandType.StoredProcedure);
        }
    }
}
