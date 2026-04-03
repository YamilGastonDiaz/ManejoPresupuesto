using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuesto.Models
{
    public class Categoria
    {
        public int Id { get; set; }
        [Display(Name = "Tipo Operación")]
        public TipoTransaccion IdTipoTransaccion { get; set; }
        public int IdUsuario { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(maximumLength: 50, ErrorMessage = "No puede ser mayor {1} caracteres")]
        [Display(Name = "Nombre Categoria")]
        public string NombreCategoria { get; set; }
    }
}
