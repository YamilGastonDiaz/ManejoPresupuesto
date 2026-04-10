using Dapper;
using ManejoPresupuesto.Interface;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Service
{
    public class TipoCuentaRepository : ITipoCuentasReposritory
    {
        private readonly string _connectionString;

        public TipoCuentaRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        
        public async Task Crear(TipoCuenta tipoCuenta)
        {
            using var connection = new SqlConnection(_connectionString);
            var id = await connection.QuerySingleAsync<int>
                                        (@"TiposCuentaInsertar", 
                                        new {idUsuario = tipoCuenta.IdUsuario, 
                                            nombreTipoCuenta = tipoCuenta.NombreTipoCuenta},
                                            commandType: System.Data.CommandType.StoredProcedure);

            tipoCuenta.Id = id;
        }

        public async Task<bool> Existe(string nombreTipoCuenta, int idUsuario)
        {
            using var connection = new SqlConnection(_connectionString);
            var existe = await connection.QueryFirstOrDefaultAsync<int>
                                            (@"SELECT 1 FROM TipoCuentas
                                            WHERE NombreTipoCuenta = @nombreTipoCuenta AND IdUsuario = @idUsuario;",
                                            new {nombreTipoCuenta, idUsuario});

            return existe == 1;
        }

        public async Task<IEnumerable<TipoCuenta>> Obtener(int idUsuario)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<TipoCuenta>
                                        (@"SELECT Id, IdUsuario, NombreTipoCuenta, Orden
                                        FROM TipoCuentas 
                                        WHERE IdUsuario = @idUsuario
                                        ORDER BY Orden", new {idUsuario});
        }

        public async Task Actualizar(TipoCuenta tipoCuenta)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(@"UPDATE TipoCuentas
                                        SET NombreTipoCuenta = @NombreTipoCuenta
                                        WHERE Id = @Id", tipoCuenta);
        }

        public async Task<TipoCuenta> ObtenerPorId(int Id, int idUsuario)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<TipoCuenta>
                                        (@"SELECT Id, NombreTipoCuenta, Orden
                                        FROM TipoCuentas
                                        WHERE Id = @Id AND IdUsuario = @idUsuario",
                                        new {Id, idUsuario});
        }

        public async Task Borrar(int Id)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(@"DELETE TipoCuentas 
                                            WHERE Id = @Id", new { Id });
        }

        public async Task Ordenar(IEnumerable<TipoCuenta>tipoCuentaOrdenados)
        {
            var query = "UPDATE TipoCuentas SET Orden = @Orden WHERE Id = @Id;";
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(query, tipoCuentaOrdenados);
        }
    }
}
