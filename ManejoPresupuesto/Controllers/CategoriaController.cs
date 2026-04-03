using ManejoPresupuesto.Interface;
using ManejoPresupuesto.Models;
using Microsoft.AspNetCore.Mvc;

namespace ManejoPresupuesto.Controllers
{
    public class CategoriaController : Controller
    {
        private readonly ICategoriaRepository _categoriaRepository;
        private readonly IUsuarioRepository _usuarioRepository;

        public CategoriaController(ICategoriaRepository categoriaRepository, 
            IUsuarioRepository usuarioRepository)
        {
            _categoriaRepository = categoriaRepository;
            _usuarioRepository = usuarioRepository;
        }

        public async Task<IActionResult> Index()
        {
            var idUduario = _usuarioRepository.ObtenerUsuarioId();
            var categorias = await _categoriaRepository.Obtener(idUduario);

            return View(categorias);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Crear(Categoria categoria)
        {
            if (!ModelState.IsValid)
            {
                return View(categoria);
            }

            var idUsuario = _usuarioRepository.ObtenerUsuarioId();
            categoria.IdUsuario = idUsuario;
            await _categoriaRepository.Crear(categoria);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Editar(int id)
        {
            var idUsuario = _usuarioRepository.ObtenerUsuarioId();
            var categoria = await _categoriaRepository.ObtenerPorId(id, idUsuario);

            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(categoria);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(Categoria categoriaEditar)
        {
            var idUsuario = _usuarioRepository.ObtenerUsuarioId();
            var categoria = await _categoriaRepository.ObtenerPorId(categoriaEditar.Id, idUsuario);

            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            categoriaEditar.IdUsuario = idUsuario;
            await _categoriaRepository.Actualizar(categoriaEditar);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Borrar(int id)
        {
            var idUsuario = _usuarioRepository.ObtenerUsuarioId();
            var categoria = await _categoriaRepository.ObtenerPorId(id, idUsuario);

            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(categoria);
        }

        [HttpPost]
        public async Task<IActionResult> BorrarCategoria(int id)
        {
            var idUsuario = _usuarioRepository.ObtenerUsuarioId();
            var categoria = await _categoriaRepository.ObtenerPorId(id, idUsuario);

            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await _categoriaRepository.Borrar(id);
            return RedirectToAction("Index");
        }

    }
}
