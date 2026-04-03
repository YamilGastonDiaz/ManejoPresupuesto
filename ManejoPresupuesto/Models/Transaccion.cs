using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuesto.Models
{
    public class Transaccion
    {
        public int Id { get; set; }
        public int IdUsuario { get; set; }
        [Range(1, maximum: int.MaxValue, ErrorMessage = "Debe seleccionar una cuenta")]
        [Display(Name = "Cuenta")]
        public int IdCuenta { get; set; }
        [Range(1, maximum: int.MaxValue, ErrorMessage = "Debe seleccionar una categoría")]
        [Display(Name = "Categoría")]
        public int IdCategoria { get; set; }
        [Display(Name = "Fecha Transacción")]
        [DataType(DataType.Date)]
        public DateTime FechaTransaccion { get; set; } = DateTime.Today;
        public decimal Monto { get; set; }
        [StringLength(maximumLength: 1000, ErrorMessage = "La nota no puede pasar de {1} caracteres")]
        public string Nota   { get; set; }
        [Display(Name = "Tipo Transacción")]
        public TipoTransaccion IdTipoTransaccion { get; set; } = TipoTransaccion.Ingreso;
    }
}
