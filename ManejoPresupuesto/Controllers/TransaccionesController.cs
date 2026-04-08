using AutoMapper;
using ClosedXML.Excel;
using ManejoPresupuesto.Interface;
using ManejoPresupuesto.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient.Diagnostics;
using System.Data;
using System.IO.Pipes;
using System.Reflection;
using System.Threading.Tasks;
using System.Transactions;

namespace ManejoPresupuesto.Controllers
{
    public class TransaccionesController : Controller
    {
        private readonly ITransaccionesRepository _transaccionesRepository;
        private readonly ICuentaRepository _cuentaRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ICategoriaRepository _categoriaRepository;
        private readonly IReportesRepository _reportesRepository;
        private readonly IMapper _mapper;

        public TransaccionesController(ITransaccionesRepository transaccionesRepository,
            ICuentaRepository cuentaRepository, IUsuarioRepository usuarioRepository,
            ICategoriaRepository categoriaRepository, IReportesRepository reportesRepository, IMapper mapper)
        {
            _transaccionesRepository = transaccionesRepository;
            _cuentaRepository = cuentaRepository;
            _usuarioRepository = usuarioRepository;
            _categoriaRepository = categoriaRepository;
            _reportesRepository = reportesRepository;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index(int mes, int anio)
        {
            var idUsuario = _usuarioRepository.ObtenerUsuarioId();

            var modelo = await _reportesRepository.ObtenerTransaccionesDetallada(idUsuario, mes, anio, ViewBag);

            return View(modelo);
        }

        public async Task<IActionResult> Semanal(int mes, int anio)
        {
            var idUsuario = _usuarioRepository.ObtenerUsuarioId();
            IEnumerable<TransaccionesPorSemana> transaccionesPorSemana = await _reportesRepository.ObtenerReporteSemanal(idUsuario, mes, anio, ViewBag);

            var agrupado = transaccionesPorSemana.GroupBy(x => x.Semana).Select(x => new TransaccionesPorSemana()
            {
                Semana = x.Key,
                Ingresos = x.Where(x => x.IdTipoTransaccion == TipoTransaccion.Ingreso)
                        .Select(x => x.Monto).FirstOrDefault(),
                Gastos = x.Where(x => x.IdTipoTransaccion == TipoTransaccion.Gasto)
                        .Select(x => x.Monto).FirstOrDefault()
            }).ToList();

            if (anio == 0 || mes == 0)
            {
                var hoy = DateTime.Today;
                anio = hoy.Year;
                mes = hoy.Month;
            }

            var fechaReferencia = new DateTime(anio, mes, 1);
            var diasDelMes = Enumerable.Range(1, fechaReferencia.AddMonths(1).AddDays(-1).Day);

            var diasSegmentados = diasDelMes.Chunk(7).ToList();

            for (int i = 0; i < diasSegmentados.Count(); i++)
            {
                var semana = i + 1;
                var fechaInicio = new DateTime(anio, mes, diasSegmentados[i].First());
                var fechaFin = new DateTime(anio, mes, diasSegmentados[i].Last());
                var grupoSemana = agrupado.FirstOrDefault(x => x.Semana == semana);

                if (grupoSemana is null)
                {
                    agrupado.Add(new TransaccionesPorSemana()
                    {
                        Semana = semana,
                        FechaInicio = fechaInicio,
                        FechaFin = fechaFin
                    });
                }
                else
                {
                    grupoSemana.FechaInicio = fechaInicio;
                    grupoSemana.FechaFin = fechaFin;
                }
            }

            agrupado = agrupado.OrderByDescending(x => x.Semana).ToList();

            var modelo = new ReporteSemanalViewModel();
            modelo.TransaccionesPorSemana = agrupado;
            modelo.FechaReferencia = fechaReferencia;

            return View(modelo);
        }

        public async Task<IActionResult> Mensual(int anio)
        {
            var idUsuario = _usuarioRepository.ObtenerUsuarioId();

            if (anio == 0)
            {
                anio = DateTime.Today.Year;
            }

            var transaccionesPorMes = await _transaccionesRepository.ObtenerPorMes(idUsuario, anio);

            var agrupado = transaccionesPorMes.GroupBy(x => x.Mes).Select(x => new TransaccionesPorMes()
            {
                Mes = x.Key,
                Ingreso = x.Where(x => x.IdTipoTransaccion == TipoTransaccion.Ingreso)
                       .Select(x => x.Monto).FirstOrDefault(),
                Gasto = x.Where(x => x.IdTipoTransaccion == TipoTransaccion.Gasto)
                       .Select(x => x.Monto).FirstOrDefault()
            }).ToList();

            for (int mes = 1; mes <= 12; mes++)
            {
                var transaccion = agrupado.FirstOrDefault(x => x.Mes == mes);
                var fechaReferencia = new DateTime(anio, mes, 1);

                if (transaccion is null)
                {
                    agrupado.Add(new TransaccionesPorMes()
                    {
                        Mes = mes,
                        FechaReferencia = fechaReferencia,
                    });
                }
                else
                {
                    transaccion.FechaReferencia = fechaReferencia;
                }
            }

            agrupado = agrupado.OrderByDescending(x => x.Mes).ToList();

            var modelo = new ReporteMensualViewModel();
            modelo.Anio = anio;
            modelo.TransaccionesPorMes = agrupado;

            return View(modelo);
        }

        public IActionResult ExcelReporte()
        {
            return View();
        }

        [HttpGet]
        public async Task<FileResult> ExportarExcelPorMes(int mes, int anio)
        {
            var fechaInicio = new DateTime(anio, mes, 1);
            var fechaFin = fechaInicio.AddMonths(1).AddDays(-1);
            var idUsuario = _usuarioRepository.ObtenerUsuarioId();

            var transacciones = await _transaccionesRepository.ObtenerPorUsuarioId(
                new TransaccionesPorUsuario
                {
                    IdUsuario = idUsuario,
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin
                });

            var nombreArchivo = $"Manejo Presupuesto - {fechaInicio.ToString("MMM yyyy")}.xlsx";
            return GenerarExcel(nombreArchivo, transacciones);
        }

        [HttpGet]
        public async Task<FileResult> ExportarExcelPorAnio(int anio)
        {
            var fechaInicio = new DateTime(anio, 1, 1);
            var fechaFin = fechaInicio.AddYears(1).AddDays(-1);
            var idUsuario = _usuarioRepository.ObtenerUsuarioId();

            var transacciones = await _transaccionesRepository.ObtenerPorUsuarioId(
                new TransaccionesPorUsuario
                {
                    IdUsuario = idUsuario,
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin
                });

            var nombreArchivo = $"Manejo Presupuesto - {fechaInicio.ToString("yyyy")}.xlsx";
            return GenerarExcel(nombreArchivo, transacciones);
        }

        [HttpGet]
        public async Task<FileResult> ExportarExcelTodo()
        {
            var fechaInicio = DateTime.Today.AddYears(-100);
            var fechaFin = DateTime.Today.AddYears(1000);
            var idUsuario = _usuarioRepository.ObtenerUsuarioId();

            var transacciones = await _transaccionesRepository.ObtenerPorUsuarioId(
               new TransaccionesPorUsuario
               {
                   IdUsuario = idUsuario,
                   FechaInicio = fechaInicio,
                   FechaFin = fechaFin
               });

            var nombreArchivo = $"Manejo Presupuestos - {DateTime.Today.ToString("dd-MM-yyyy")}.xlsx";
            return GenerarExcel(nombreArchivo, transacciones);
        }

        private FileResult GenerarExcel(string nombreArchivo,
            IEnumerable<Transaccion> transacciones)
        {
            DataTable dataTable = new DataTable("Transacciones");
            dataTable.Columns.AddRange(new DataColumn[] {
                new DataColumn("Fecha"),
                new DataColumn("Cuenta"),
                new DataColumn("Categoria"),
                new DataColumn("Nota"),
                new DataColumn("Monto"),
                new DataColumn("Ingreso/Gasto"),
            });

            foreach (var transaccion in transacciones)
            {
                dataTable.Rows.Add(transaccion.FechaTransaccion,
                    transaccion.Cuenta,
                    transaccion.Categoria,
                    transaccion.Nota,
                    transaccion.Monto,
                    transaccion.IdTipoTransaccion);
            }

            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dataTable);

                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        nombreArchivo);
                }
            }
        }

        public IActionResult Calendario()
        {
            return View();
        }

        public async Task<JsonResult> ObtenerTransaccionesCalendario(DateTime start,
           DateTime end)
        {
            var idUsuario = _usuarioRepository.ObtenerUsuarioId();

            var transacciones = await _transaccionesRepository.ObtenerPorUsuarioId(
               new TransaccionesPorUsuario
               {
                   IdUsuario = idUsuario,
                   FechaInicio = start,
                   FechaFin = end
               });

            var eventosCalendario = transacciones.Select(transaccion => new EventoCalendario()
            {
                Title = transaccion.Monto.ToString("N"),
                Start = transaccion.FechaTransaccion.ToString("yyyy-MM-dd"),
                End = transaccion.FechaTransaccion.ToString("yyyy-MM-dd"),
                Color = (transaccion.IdTipoTransaccion == TipoTransaccion.Gasto) ? "Red" : null
            });

            return Json(eventosCalendario);
        }

        public async Task<JsonResult> ObtenerTransaccionesPorFecha(DateTime fecha)
        {
            var idUsuario = _usuarioRepository.ObtenerUsuarioId();

            var transacciones = await _transaccionesRepository.ObtenerPorUsuarioId(
               new TransaccionesPorUsuario
               {
                   IdUsuario = idUsuario,
                   FechaInicio = fecha,
                   FechaFin = fecha
               });

            return Json(transacciones);
        }


        public async Task<IActionResult> Crear()
        {
            var idUsuario = _usuarioRepository.ObtenerUsuarioId();
            var modelo = new TransaccionCreacionViewModel();
            modelo.Cuentas = await ObtenerCuentas(idUsuario);
            modelo.Categorias = await ObtenerCategorias(idUsuario, modelo.IdTipoTransaccion);

            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(TransaccionCreacionViewModel modelo)
        {
            var idUsuario = _usuarioRepository.ObtenerUsuarioId();

            if (!ModelState.IsValid)
            {
                modelo.Cuentas = await ObtenerCuentas(idUsuario);
                modelo.Categorias = await ObtenerCategorias(idUsuario, modelo.IdTipoTransaccion);

                return View(modelo);
            }

            var cuenta = await _cuentaRepository.ObtenerPorId(modelo.IdCuenta, idUsuario);

            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var categoria = await _categoriaRepository.ObtenerPorId(modelo.IdCategoria, idUsuario);

            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            modelo.IdUsuario = idUsuario;

            if (modelo.IdTipoTransaccion == TipoTransaccion.Gasto)
            {
                modelo.Monto *= -1;
            }

            await _transaccionesRepository.Crear(modelo);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id, string urlRetorno = null)
        {
            var idUsuario = _usuarioRepository.ObtenerUsuarioId();
            var transaccion = await _transaccionesRepository.ObtenerPorId(id, idUsuario);

            if (transaccion is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var modelo = _mapper.Map<TransaccionEditarViewModel>(transaccion);

            modelo.MontoAnterior = modelo.Monto;

            if (modelo.IdTipoTransaccion == TipoTransaccion.Gasto)
            {
                modelo.MontoAnterior = modelo.Monto * -1;
            }

            modelo.IdCuentaAnterior = transaccion.IdCuenta;
            modelo.Categorias = await ObtenerCategorias(idUsuario, transaccion.IdTipoTransaccion);
            modelo.Cuentas = await ObtenerCuentas(idUsuario);
            modelo.UrlRetorno = urlRetorno;

            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(TransaccionEditarViewModel modelo)
        {
            var idUsuario = _usuarioRepository.ObtenerUsuarioId();

            if (!ModelState.IsValid)
            {
                modelo.Categorias = await ObtenerCategorias(idUsuario, modelo.IdTipoTransaccion);
                modelo.Cuentas = await ObtenerCuentas(idUsuario);

                return View(modelo);
            }

            var cuentas = await _cuentaRepository.ObtenerPorId(modelo.IdCuenta, idUsuario);

            if (cuentas is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var categorias = await _categoriaRepository.ObtenerPorId(modelo.IdCategoria, idUsuario);

            if (categorias is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var transaccion = _mapper.Map<Transaccion>(modelo);

            if (modelo.IdTipoTransaccion == TipoTransaccion.Gasto)
            {
                transaccion.Monto *= -1;
            }

            await _transaccionesRepository.Editar(transaccion, modelo.MontoAnterior, modelo.IdCuentaAnterior);

            if (string.IsNullOrEmpty(modelo.UrlRetorno))
            {
                return RedirectToAction("Index");
            }
            else
            {
                return LocalRedirect(modelo.UrlRetorno);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Borrar(int id, string urlRetorno = null)
        {
            var idUsuario = _usuarioRepository.ObtenerUsuarioId();
            var transaccion = await _transaccionesRepository.ObtenerPorId(id, idUsuario);

            if (transaccion is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await _transaccionesRepository.Borrar(id);

            if (string.IsNullOrEmpty(urlRetorno))
            {
                return RedirectToAction("Index");
            }
            else
            {
                return LocalRedirect(urlRetorno);
            }
        }

        private async Task<IEnumerable<SelectListItem>> ObtenerCuentas(int idUsuario)
        {
            var cuentas = await _cuentaRepository.Buscar(idUsuario);
            return cuentas.Select(x => new SelectListItem(x.NombreCuenta, x.Id.ToString()));
        }

        private async Task<IEnumerable<SelectListItem>> ObtenerCategorias(int idUsuario, TipoTransaccion IdTipoTransaccion)
        {
            var categorias = await _categoriaRepository.Obtener(idUsuario, IdTipoTransaccion);
            return categorias.Select(x => new SelectListItem(x.NombreCategoria, x.Id.ToString()));
        }

        [HttpPost]
        public async Task<IActionResult> ObtenerCategorias([FromBody] TipoTransaccion IdTipoTransaccion)
        {
            var idUsuario = _usuarioRepository.ObtenerUsuarioId();
            var categorias = await ObtenerCategorias(idUsuario, IdTipoTransaccion);
            return Ok(categorias);
        }
    }
}
