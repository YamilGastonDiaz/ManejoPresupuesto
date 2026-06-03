namespace ManejoPresupuesto.Models
{
    public class TransaccionesPorCuenta
    {
        public int IdUsuario { get; set; }
        public int IdCuenta { get; set; }
        public DateTime FechaInicio { get; set; }
        public  DateTime FechaFin { get; set; }
    }
}
