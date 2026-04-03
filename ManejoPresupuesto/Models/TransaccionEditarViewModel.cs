namespace ManejoPresupuesto.Models
{
    public class TransaccionEditarViewModel : TransaccionCreacionViewModel
    {
        public int IdCuentaAnterior { get; set; }
        public decimal MontoAnterior { get; set; }
    }
}
