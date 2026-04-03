using ManejoPresupuesto.Models;

namespace ManejoPresupuesto.Interface
{
    public interface ITransaccionesRepository
    {
        Task Crear(Transaccion transaccion);
    }
}
