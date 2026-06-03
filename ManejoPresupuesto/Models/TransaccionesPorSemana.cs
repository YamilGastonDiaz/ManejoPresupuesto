namespace ManejoPresupuesto.Models
{
    public class TransaccionesPorSemana
    {
        public int Semana { get; set; }
        public decimal Monto { get; set; }
        public TipoTransaccion IdTipoTransaccion { get; set; }
        public decimal Ingresos { get; set; }
        public decimal Gastos { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
    }
}
