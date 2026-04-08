using AutoMapper;
using ManejoPresupuesto.Interface;
using ManejoPresupuesto.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Reflection;

namespace ManejoPresupuesto.Controllers
{
    public class CuentasController : Controller
    {
        private readonly ITipoCuentasReposritory _tipoCuentasReposritory;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ICuentaRepository _cuentaRepository;
        private readonly ITransaccionesRepository _transaccionesRepository;
        private readonly IReportesRepository _reportesRepository;
        private readonly IMapper _mapper;

        public CuentasController(ITipoCuentasReposritory tipoCuentasReposritory, 
            IUsuarioRepository usuarioRepository, ICuentaRepository cuentaRepository,
            ITransaccionesRepository transaccionesRepository, IReportesRepository reportesRepository, IMapper mapper)
        {
            _tipoCuentasReposritory = tipoCuentasReposritory;
            _usuarioRepository = usuarioRepository;
            _cuentaRepository = cuentaRepository;
            _transaccionesRepository = transaccionesRepository;
            _reportesRepository = reportesRepository;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var idUsuario = _usuarioRepository.ObtenerUsuarioId();
            var cuentasConTipoCuentas = await _cuentaRepository.Buscar(idUsuario);

            var modelo = cuentasConTipoCuentas
                            .GroupBy(x => x.NombreTipoCuenta)
                            .Select(grupo => new IndiceCuentasViewModel
                            {
                                NombreTipoCuenta = grupo.Key,
                                Cuenta = grupo.AsEnumerable()
                            }).ToList();

            return View(modelo);
        }

        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            var idUsuario = _usuarioRepository.ObtenerUsuarioId();
            var modelo = new CuentaCreacionViewModel();
            modelo.TipsoCuentas = await ObtenerTipoCuentas(idUsuario);
            return View(modelo);
        }

        public async Task<IActionResult> Detalle(int id, int mes, int anio)
        {
            var idUsuario = _usuarioRepository.ObtenerUsuarioId();
            var cuenta = await _cuentaRepository.ObtenerPorId(id, idUsuario);

            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            ViewBag.Cuenta = cuenta.NombreCuenta;

            var modelo = await _reportesRepository.ObtenerTransaccionesDetalladaCuenta(idUsuario, id, mes, anio, ViewBag);

            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(CuentaCreacionViewModel cuenta)
        {
            var idUsuario = _usuarioRepository.ObtenerUsuarioId();
            var tipoCuentas = await _tipoCuentasReposritory.ObtenerPorId(cuenta.IdTipoCuenta, idUsuario);

            if (tipoCuentas is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            if (!ModelState.IsValid)
            {
                cuenta.TipsoCuentas = await ObtenerTipoCuentas(idUsuario);
                return View(cuenta);
            }

            await _cuentaRepository.Crear(cuenta);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Editar(int id)
        {
            var idUsuario = _usuarioRepository.ObtenerUsuarioId();
            var cuenta = await _cuentaRepository.ObtenerPorId(id, idUsuario);

            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var modelo = _mapper.Map<CuentaCreacionViewModel>(cuenta);

            modelo.TipsoCuentas = await ObtenerTipoCuentas(idUsuario);
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(CuentaCreacionViewModel cuentaEditar)
        {
            var idUsuario = _usuarioRepository.ObtenerUsuarioId();
            var cuenta = await _cuentaRepository.ObtenerPorId(cuentaEditar.Id, idUsuario);

            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var tipoCuenta = await _tipoCuentasReposritory.ObtenerPorId(cuentaEditar.IdTipoCuenta, idUsuario);

            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await _cuentaRepository.Actualizar(cuentaEditar);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Borrar(int id)
        {
            var idUsuario = _usuarioRepository.ObtenerUsuarioId();
            var cuenta = await _cuentaRepository.ObtenerPorId(id, idUsuario);

            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(cuenta);
        }

        [HttpPost]
        public async Task<IActionResult> BorrarCuenta(int id)
        {
            var idUsuario = _usuarioRepository.ObtenerUsuarioId();
            var cuenta = await _cuentaRepository.ObtenerPorId(id, idUsuario);

            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await _cuentaRepository.Borrar(id);
            return RedirectToAction("Index");
        }

        private async Task<IEnumerable<SelectListItem>> ObtenerTipoCuentas(int idUsuario)
        {
            var tipoCuentas = await _tipoCuentasReposritory.Obtener(idUsuario);
            return tipoCuentas.Select(x => new SelectListItem(x.NombreTipoCuenta, x.Id.ToString()));
        } 
    }
}
