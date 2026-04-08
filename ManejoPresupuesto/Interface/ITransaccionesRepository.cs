using ManejoPresupuesto.Models;

namespace ManejoPresupuesto.Interface
{
    public interface ITransaccionesRepository
    {
        Task<Transaccion> ObtenerPorId(int id, int idUsuario);
        Task<IEnumerable<Transaccion>> ObtenerPorCuenaId(TransaccionesPorCuenta modelo);
        Task<IEnumerable<Transaccion>> ObtenerPorUsuarioId(TransaccionesPorUsuario modelo);
        Task<IEnumerable<TransaccionesPorSemana>> ObtenerPorSemana(TransaccionesPorUsuario modelo);
        Task<IEnumerable<TransaccionesPorMes>> ObtenerPorMes(int idUsuario, int anio);
        Task Crear(Transaccion transaccion);
        Task Editar(Transaccion transaccion, decimal montoAnterior, int cuentaAnterior);
        Task Borrar(int id);
    }
}
