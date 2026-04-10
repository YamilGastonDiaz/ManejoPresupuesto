using Dapper;
using ManejoPresupuesto.Interface;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Service
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly string _connectionString;

        public UsuarioRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<int> CrearUsuario(Usuario usuario)
        {
            using var connection = new SqlConnection(_connectionString);
            var IdUsuario = await connection.QuerySingleAsync<int>(@"
                                INSERT INTO Usuarios(Email, EmailNormalizado, PasswordHash)
                                VALUES (@Email, @EmailNormalizado, @PasswordHash)
                                SELECT SCOPE_IDENTITY();", usuario);

            await connection.ExecuteAsync("CrearDatosUsuarioNuevo", new { IdUsuario },
                commandType: System.Data.CommandType.StoredProcedure);

            return IdUsuario;
        }

        public async Task<Usuario> BuscarUsuarioPorEmail(string emailNormalizado)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleOrDefaultAsync<Usuario>(@"SELECT * FROM Usuarios 
                                WHERE EmailNormalizado = @EmailNormalizado", new {emailNormalizado});
        }
    }
}
