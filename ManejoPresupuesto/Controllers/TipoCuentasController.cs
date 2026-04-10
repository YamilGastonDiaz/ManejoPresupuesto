using ManejoPresupuesto.Interface;
using ManejoPresupuesto.Models;
using Microsoft.AspNetCore.Mvc;

namespace ManejoPresupuesto.Controllers
{
    public class TipoCuentasController : Controller
    {
        private readonly ITipoCuentasReposritory _reposritory;
        private readonly IServicioUsuario _servicioUsuario;

        public TipoCuentasController(ITipoCuentasReposritory reposritory, IServicioUsuario servicioUsuario)
        {
            _reposritory = reposritory;
            _servicioUsuario = servicioUsuario;
        }

        public async Task<IActionResult> Index()
        {
            var idUsuario = _servicioUsuario.ObtenerUsuarioId();
            var tipoCuentas = await _reposritory.Obtener(idUsuario);

            return View(tipoCuentas);
        }

        public ActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Crear(TipoCuenta tipoCuenta)
        {
            if (!ModelState.IsValid)
            {
                return View(tipoCuenta);
            }

            tipoCuenta.IdUsuario = _servicioUsuario.ObtenerUsuarioId();

            var existeTipoCuenta = await _reposritory.Existe(tipoCuenta.NombreTipoCuenta, tipoCuenta.IdUsuario);

            if (existeTipoCuenta)
            {
                ModelState.AddModelError(nameof(tipoCuenta.NombreTipoCuenta), $"El nombre {tipoCuenta.NombreTipoCuenta} ya existe.");

                return View(tipoCuenta);
            }

            await _reposritory.Crear(tipoCuenta);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<ActionResult> Editar(int id)
        {
            var idUsuario = _servicioUsuario.ObtenerUsuarioId();
            var tipoCuenta = await _reposritory.ObtenerPorId(id, idUsuario);

            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(tipoCuenta);
        }

        [HttpPost]
        public async Task<ActionResult> Editar(TipoCuenta tipoCuenta)
        {
            var idUsuario = _servicioUsuario.ObtenerUsuarioId();
            var tipoCuentaExiste = await _reposritory.ObtenerPorId(tipoCuenta.Id, idUsuario);

            if (tipoCuentaExiste is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await _reposritory.Actualizar(tipoCuenta);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<ActionResult> Borrar(int id)
        {
            var idUsuario = _servicioUsuario.ObtenerUsuarioId();
            var tipoCuenta = await _reposritory.ObtenerPorId(id, idUsuario);

            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(tipoCuenta);
        }

        [HttpPost]
        public async Task<ActionResult> BorrarTipoCuenta(int Id)
        {
            var idUsuario = _servicioUsuario.ObtenerUsuarioId();
            var tipoCuenta = await _reposritory.ObtenerPorId(Id, idUsuario);

            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await _reposritory.Borrar(Id);

            return RedirectToAction("Index");  
        }

        [HttpGet]
        public async Task<IActionResult> VerificarexisteTipoCuenta(string nombreTipoCuenta)
        {
            var id_Usuario = _servicioUsuario.ObtenerUsuarioId();
            var existeTipoCuenta = await _reposritory.Existe(nombreTipoCuenta, id_Usuario);

            if (existeTipoCuenta)
            {
                return Json($"El nombre {nombreTipoCuenta} ya existe");
            }

            return Json(true);
        }

        [HttpPost]
        public async Task<IActionResult> Ordenar([FromBody] int[] ids)
        {
            var idUsuario = _servicioUsuario.ObtenerUsuarioId();
            var tiposCuenta = await _reposritory.Obtener(idUsuario);
            var idsTipoCuenta = tiposCuenta.Select(x => x.Id);

            var idsNoPertenecenAlUsuario = ids.Except(idsTipoCuenta).ToList();

            if (idsNoPertenecenAlUsuario.Count > 0)
            {
                return Forbid();
            }

            var tipoCuentasOrdenados = ids.Select((valor, indice) =>
                                       new TipoCuenta() { Id = valor, Orden = indice + 1 })
                                      .AsEnumerable();

            await _reposritory.Ordenar(tipoCuentasOrdenados);

            return Ok();
        }
    }
}
