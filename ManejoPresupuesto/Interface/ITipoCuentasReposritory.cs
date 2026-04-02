using ManejoPresupuesto.Models;

namespace ManejoPresupuesto.Interface
{
    public interface ITipoCuentasReposritory
    {
        Task Crear(TipoCuenta tipoCuenta);
        Task<bool> Existe(string nombre, int idUser);
        Task<IEnumerable<TipoCuenta>> Obtener(int idUser);
        Task Actualizar(TipoCuenta tipoCuenta);
        Task<TipoCuenta> ObtenerPorId(int tipoCuenta_Id, int id_Usuario);
        Task Borrar(int tipoCuenta_Id);
        Task Ordenar(IEnumerable<TipoCuenta> tipoCuentaOrdenados);
    }
}
