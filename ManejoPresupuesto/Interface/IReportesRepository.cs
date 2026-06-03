using ManejoPresupuesto.Models;

namespace ManejoPresupuesto.Interface
{
    public interface IReportesRepository
    {
        Task<ReportesTransaccionesDetallada> ObtenerTransaccionesDetalladaCuenta
            (int idUsuario, int idCuenta, int mes, int anio, dynamic ViewBag);

        Task<ReportesTransaccionesDetallada> ObtenerTransaccionesDetallada
           (int idUsuario, int mes, int anio, dynamic ViewBag);

        Task<IEnumerable<TransaccionesPorSemana>> ObtenerReporteSemanal(int usuarioId,
           int mes, int anio, dynamic ViewBag);
    }
}
