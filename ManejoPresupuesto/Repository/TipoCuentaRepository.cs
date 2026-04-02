using Dapper;
using ManejoPresupuesto.Interface;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Repository
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
                                        new {id_Usuario = tipoCuenta.Id_Usuario, 
                                            nombreTipoCuenta = tipoCuenta.NombreTipoCuenta},
                                            commandType: System.Data.CommandType.StoredProcedure);

            tipoCuenta.TipoCuenta_Id = id;
        }

        public async Task<bool> Existe(string nombreTipoCuenta, int id_Usuario)
        {
            using var connection = new SqlConnection(_connectionString);
            var existe = await connection.QueryFirstOrDefaultAsync<int>
                                            (@"SELECT 1 FROM TipoCuentas
                                            WHERE NombreTipoCuenta = @nombreTipoCuenta AND Id_Usuario = @id_Usuario;",
                                            new {nombreTipoCuenta, id_Usuario});

            return existe == 1;
        }

        public async Task<IEnumerable<TipoCuenta>> Obtener(int id_Usuario)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<TipoCuenta>
                                        (@"SELECT TipoCuenta_Id, Id_Usuario, NombreTipoCuenta, Orden
                                        FROM TipoCuentas 
                                        WHERE Id_Usuario = @id_Usuario
                                        ORDER BY Orden", new {id_Usuario});
        }

        public async Task Actualizar(TipoCuenta tipoCuenta)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(@"UPDATE TipoCuentas
                                        SET NombreTipoCuenta = @NombreTipoCuenta
                                        WHERE TipoCuenta_Id = @TipoCuenta_Id", tipoCuenta);
        }

        public async Task<TipoCuenta> ObtenerPorId(int tipoCuenta_Id, int id_Usuario)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<TipoCuenta>
                                        (@"SELECT TipoCuenta_Id, NombreTipoCuenta, Orden
                                        FROM TipoCuentas
                                        WHERE TipoCuenta_Id = @tipoCuenta_Id AND Id_Usuario = @id_Usuario",
                                        new {tipoCuenta_Id, id_Usuario});
        }

        public async Task Borrar(int tipoCuenta_Id)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(@"DELETE TipoCuentas 
                                            WHERE TipoCuenta_Id = @tipoCuenta_Id", new { tipoCuenta_Id });
        }

        public async Task Ordenar(IEnumerable<TipoCuenta>tipoCuentaOrdenados)
        {
            var query = "UPDATE TipoCuentas SET Orden = @Orden WHERE TipoCuenta_Id = @tipoCuenta_Id;";
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(query, tipoCuentaOrdenados);
        }
    }
}
