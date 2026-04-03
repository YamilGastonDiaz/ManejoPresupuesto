using ManejoPresupuesto.Models;

namespace ManejoPresupuesto.Interface
{
    public interface ITransaccionesRepository
    {
        Task<Transaccion> ObtenerPorId(int id, int idUsuario);
        Task Crear(Transaccion transaccion);
        Task Editar(Transaccion transaccion, decimal montoAnterior, int cuentaAnterior);
        Task Borrar(int id);
    }
}
