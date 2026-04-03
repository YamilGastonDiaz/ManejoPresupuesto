using ManejoPresupuesto.Interface;
using ManejoPresupuesto.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient.Diagnostics;

namespace ManejoPresupuesto.Controllers
{
    public class TransaccionesController : Controller
    {
        private readonly ITransaccionesRepository _transaccionesRepository;
        private readonly ICuentaRepository _cuentaRepository;
        private readonly IUsuarioRepository _usuarioRepository;

        public TransaccionesController(ITransaccionesRepository transaccionesRepository, 
            ICuentaRepository cuentaRepository, IUsuarioRepository usuarioRepository)
        {
            _transaccionesRepository = transaccionesRepository;
            _cuentaRepository = cuentaRepository;
            _usuarioRepository = usuarioRepository;
        }

        public async Task<IActionResult> Crear()
        {
            var idUsuario = _usuarioRepository.ObtenerUsuarioId();
            var modelo = new TransaccionCreacionViewModel();
            modelo.Cuentas = await ObtenerCuentas(idUsuario);

            return View(modelo);
        }

        private async Task<IEnumerable<SelectListItem>> ObtenerCuentas(int idUsuario)
        {
            var cuentas = await _cuentaRepository.Buscar(idUsuario);
            return cuentas.Select(x => new SelectListItem(x.NombreCuenta, x.Id.ToString()));
        }
    }
}
