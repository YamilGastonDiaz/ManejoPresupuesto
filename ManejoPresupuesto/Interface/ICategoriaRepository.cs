using ManejoPresupuesto.Models;

namespace ManejoPresupuesto.Interface
{
    public interface ICategoriaRepository
    {
        Task Crear(Categoria categoria);
        Task<IEnumerable<Categoria>> Obtener(int idUsuario);
        Task<Categoria> ObtenerPorId(int id, int idUsuario);
        Task Actualizar(Categoria categoria);
        Task Borrar(int id);
    }
}
