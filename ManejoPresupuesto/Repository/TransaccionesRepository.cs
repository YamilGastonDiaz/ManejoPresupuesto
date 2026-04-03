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
    }
}
