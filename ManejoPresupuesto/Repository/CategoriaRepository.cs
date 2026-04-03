using Dapper;
using ManejoPresupuesto.Interface;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Repository
{
    public class CategoriaRepository : ICategoriaRepository
    {
        private readonly string _connectionString;
        public CategoriaRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(Categoria categoria)
        {
            using var connection = new SqlConnection(_connectionString);
            var id = await connection.QuerySingleAsync<int>(@"INSERT INTO Categorias (IdTipoTransaccion, IdUsuario, NombreCategoria)
                                                            VALUES (@IdTipoTransaccion, @IdUsuario, @NombreCategoria);
                                                            SELECT SCOPE_IDENTITY();", categoria);

            categoria.Id = id;
        }

        public async Task<IEnumerable<Categoria>> Obtener(int idUsuario)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<Categoria>(@"SELECT * FROM Categorias 
                                                        WHERE IdUsuario = @IdUsuario", new { idUsuario });
        }

        public async Task<Categoria> ObtenerPorId(int id, int idUsuario)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Categoria>(@"SELECT * FROM Categorias
                                                                      WHERE Id = @Id AND IdUsuario = @IdUsuario
                                                                      ", new {id, idUsuario});
        }

        public async Task Actualizar(Categoria categoria)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(@"UPDATE Categorias SET 
                                        IdTipoTransaccion = @IdTipoTransaccion, 
                                        NombreCategoria = @NombreCategoria
                                        WHERE Id = @Id", categoria);
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(@"DELETE Categorias WHERE Id = @Id", new { id });
        }
    }
}
