using AutoMapper;
using ManejoPresupuesto.Interface;
using ManejoPresupuesto.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient.Diagnostics;
using System.Reflection;
using System.Transactions;

namespace ManejoPresupuesto.Controllers
{
    public class TransaccionesController : Controller
    {
        private readonly ITransaccionesRepository _transaccionesRepository;
        private readonly ICuentaRepository _cuentaRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ICategoriaRepository _categoriaRepository;
        private readonly IMapper _mapper;

        public TransaccionesController(ITransaccionesRepository transaccionesRepository, 
            ICuentaRepository cuentaRepository, IUsuarioRepository usuarioRepository,
            ICategoriaRepository categoriaRepository, IMapper mapper)
        {
            _transaccionesRepository = transaccionesRepository;
            _cuentaRepository = cuentaRepository;
            _usuarioRepository = usuarioRepository;
            _categoriaRepository = categoriaRepository;
            _mapper = mapper;
        }

        public IActionResult Index()
        {
            return View();
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

            if (modelo.IdTipoTransaccion  == TipoTransaccion.Gasto)
            {
                modelo.Monto *= -1;
            }

            await _transaccionesRepository.Crear(modelo);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
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

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Borrar(int id)
        {
            var idUsuario = _usuarioRepository.ObtenerUsuarioId();
            var transaccion = await _transaccionesRepository.ObtenerPorId(id, idUsuario);

            if (transaccion is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await _transaccionesRepository.Borrar(id);

            return RedirectToAction("Index");
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
