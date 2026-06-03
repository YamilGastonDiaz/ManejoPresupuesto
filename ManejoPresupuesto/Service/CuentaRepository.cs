using Dapper;
using ManejoPresupuesto.Interface;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Service
{
    public class CuentaRepository : ICuentaRepository
    {
        private readonly string _connectionString;

        public CuentaRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(Cuenta cuenta)
        {
            using var connection = new SqlConnection(_connectionString);
            var id = await connection.QuerySingleAsync<int>(@"INSERT INTO Cuentas (IdTipoCuenta, NombreCuenta, Balance, Descripcion)
                                                        VALUES (@IdTipoCuenta, @NombreCuenta, @Balance, @Descripcion);
                                                        SELECT SCOPE_IDENTITY();", cuenta);
            cuenta.Id = id;
        }

        public async Task<IEnumerable<Cuenta>> Buscar(int idUsuario)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<Cuenta>(@"SELECT c.Id, c.NombreCuenta, Balance, tc.NombreTipoCuenta
                                                        FROM Cuentas AS c
                                                        INNER JOIN TipoCuentas AS tc
                                                        ON tc.Id = c.IdTipoCuenta
                                                        WHERE tc.IdUsuario = @IdUsuario
                                                        ORDER BY tc.Orden", new { idUsuario });
        }

        public async Task<Cuenta> ObtenerPorId(int id, int idUsuario)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Cuenta>(@"SELECT c.Id, c.NombreCuenta, Balance, Descripcion, IdTipoCuenta
                                                        FROM Cuentas AS c
                                                        INNER JOIN TipoCuentas AS tc
                                                        ON tc.Id = c.IdTipoCuenta
                                                        WHERE tc.IdUsuario = @IdUsuario AND c.Id = @Id", new {id, idUsuario});
        }

        public async Task Actualizar(CuentaCreacionViewModel cuenta)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(@"UPDATE Cuentas
                                        SET NombreCuenta = @NombreCuenta, Balance = @Balance, Descripcion = @Descripcion
                                        WHERE Id = @Id;", cuenta);
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(@"DELETE Cuentas WHERE Id = @Id", new {id});
        }
    }
}
