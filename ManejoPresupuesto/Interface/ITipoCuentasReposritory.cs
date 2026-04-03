using ManejoPresupuesto.Models;

namespace ManejoPresupuesto.Interface
{
    public interface ITipoCuentasReposritory
    {
        Task Crear(TipoCuenta tipoCuenta);
        Task<bool> Existe(string nombre, int idUsuario);
        Task<IEnumerable<TipoCuenta>> Obtener(int idUsuario);
        Task Actualizar(TipoCuenta tipoCuenta);
        Task<TipoCuenta> ObtenerPorId(int id, int idUsuario);
        Task Borrar(int id);
        Task Ordenar(IEnumerable<TipoCuenta> tipoCuentaOrdenados);
    }
}
