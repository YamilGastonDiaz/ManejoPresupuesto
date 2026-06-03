namespace ManejoPresupuesto.Interface
{
    public interface IServicioEmail
    {
        Task EnviarEmailCambioPassword(string receptor, string enlace);
    }
}
