using ManejoPresupuesto.Models;

namespace ManejoPresupuesto.Interface
{
    public interface IUsuarioRepository
    {
        Task<int> CrearUsuario(Usuario usuario);
        Task<Usuario> BuscarUsuarioPorEmail(string emailNormalizado);
    }
}
