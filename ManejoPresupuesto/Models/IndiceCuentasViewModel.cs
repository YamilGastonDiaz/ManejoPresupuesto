namespace ManejoPresupuesto.Models
{
    public class IndiceCuentasViewModel
    {
        public string NombreTipoCuenta { get; set; }
        public IEnumerable<Cuenta> Cuenta { get; set; }
        public decimal Balance => Cuenta.Sum(x => x.Balance);
    }
}
