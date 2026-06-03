using ManejoPresupuesto.Models;

namespace ManejoPresupuesto.Interface
{
    public interface ICuentaRepository
    {
        Task Crear(Cuenta cuenta);
        Task<IEnumerable<Cuenta>> Buscar(int idUsuario);
        Task<Cuenta> ObtenerPorId(int id, int idUsuario);
        Task Actualizar(CuentaCreacionViewModel cuenta);
        Task Borrar(int id);
    }
}
