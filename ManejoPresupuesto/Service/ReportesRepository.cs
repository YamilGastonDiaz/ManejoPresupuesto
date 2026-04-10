using ManejoPresupuesto.Interface;
using ManejoPresupuesto.Models;

namespace ManejoPresupuesto.Service
{
    public class ReportesRepository : IReportesRepository
    {
        private readonly ITransaccionesRepository _transaccionesRepository;
        private readonly HttpContext _httpContext;

        public ReportesRepository(ITransaccionesRepository transaccionesRepository,
          IHttpContextAccessor httpContextAccessor)
        {
            _transaccionesRepository = transaccionesRepository;
            _httpContext = httpContextAccessor.HttpContext;
        }

        public async Task<IEnumerable<TransaccionesPorSemana>> ObtenerReporteSemanal(int idUsuario,
           int mes, int anio, dynamic ViewBag)
        {
            (DateTime fechaInicio, DateTime fechaFin) = GenerarFecha(mes, anio);

            var transaccionPorUsuario = new TransaccionesPorUsuario()
            {
                IdUsuario = idUsuario,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            };

            AsignarValoresAlViewBag(ViewBag, fechaInicio);

            var modelo = await _transaccionesRepository.ObtenerPorSemana(transaccionPorUsuario);

            return modelo;
        }

        public async Task<ReportesTransaccionesDetallada> ObtenerTransaccionesDetalladaCuenta
            (int idUsuario, int idCuenta, int mes, int anio, dynamic ViewBag)
        {
            (DateTime fechaInicio, DateTime fechaFin) = GenerarFecha(mes, anio);

            var transaccionesPorCuenta = new TransaccionesPorCuenta
            {
                IdCuenta = idCuenta,
                IdUsuario = idUsuario,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            };

            var transacciones = await _transaccionesRepository.ObtenerPorCuenaId(transaccionesPorCuenta);

            ReportesTransaccionesDetallada modelo = GenerarReporteTransaccionesDetalladas(fechaInicio, fechaFin, transacciones);

            AsignarValoresAlViewBag(ViewBag, fechaInicio);

            return modelo;
        }

        public async Task<ReportesTransaccionesDetallada> ObtenerTransaccionesDetallada
           (int idUsuario, int mes, int anio, dynamic ViewBag)
        {
            (DateTime fechaInicio, DateTime fechaFin) = GenerarFecha(mes, anio);

            var transaccionPorUsuario = new TransaccionesPorUsuario()
            {
                IdUsuario = idUsuario,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            };

            var transacciones = await _transaccionesRepository.ObtenerPorUsuarioId(transaccionPorUsuario);

            ReportesTransaccionesDetallada modelo = GenerarReporteTransaccionesDetalladas(fechaInicio, fechaFin, transacciones);

            AsignarValoresAlViewBag(ViewBag, fechaInicio);

            return modelo;
        }

        private void AsignarValoresAlViewBag(dynamic ViewBag, DateTime fechaInicio)
        {
            ViewBag.mesAnterior = fechaInicio.AddMonths(-1).Month;
            ViewBag.anioAnterior = fechaInicio.AddMonths(-1).Year;
            ViewBag.mesPosterior = fechaInicio.AddMonths(1).Month;
            ViewBag.anioPosterior = fechaInicio.AddMonths(1).Year;
            ViewBag.urlRetorno = _httpContext.Request.Path + _httpContext.Request.QueryString;
        }

        private static ReportesTransaccionesDetallada GenerarReporteTransaccionesDetalladas(DateTime fechaInicio, DateTime fechaFin, IEnumerable<Transaccion> transacciones)
        {
            var modelo = new ReportesTransaccionesDetallada();

            var transaccionesPorFecha = transacciones.OrderByDescending(x => x.FechaTransaccion)
                .GroupBy(x => x.FechaTransaccion)
                .Select(grupo => new ReportesTransaccionesDetallada.TransaccionesPorFecha()
                {
                    FechaTransaccion = grupo.Key,
                    Transacciones = grupo.AsEnumerable()
                });

            modelo.TransaccionesAgrupadas = transaccionesPorFecha;
            modelo.FechaInicio = fechaInicio;
            modelo.FechaFin = fechaFin;
            return modelo;
        }

        private (DateTime fechaInicio, DateTime fechaFin) GenerarFecha(int mes, int anio)
        {
            DateTime fechaInicio;
            DateTime fechaFin;

            if (mes <= 0 || mes > 12 || anio <= 1900)
            {
                var hoy = DateTime.Today;
                fechaInicio = new DateTime(hoy.Year, hoy.Month, 1);

            }
            else
            {
                fechaInicio = new DateTime(anio, mes, 1);
            }

            fechaFin = fechaInicio.AddMonths(1).AddDays(-1);

            return (fechaInicio, fechaFin);
        }

    }
}
