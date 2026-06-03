using ManejoPresupuesto.Interface;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace ManejoPresupuesto.Service
{
    public class ServicioEmail : IServicioEmail
    {
        private readonly IConfiguration _configuration;

        public ServicioEmail(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task EnviarEmailCambioPassword(string receptor, string enlace)
        {
            var email = _configuration.GetValue<string>("CONFIGURACIONES_EMAIL:EMAIL");
            var password = _configuration.GetValue<string>("CONFIGURACIONES_EMAIL:PASSWORD");
            var host = _configuration.GetValue<string>("CONFIGURACIONES_EMAIL:HOST");
            var puerto = _configuration.GetValue<int>("CONFIGURACIONES_EMAIL:PUERTO");

            var cliente = new SmtpClient(host, puerto);
            cliente.EnableSsl = true;
            cliente.UseDefaultCredentials = false;

            cliente.Credentials = new NetworkCredential(email, password);
            var emisor = email;
            var tema = "¿Ha olvidado su contraseña?";

            var contenidoHTML = $@"Saludos,

                Este mensaje le llega porque usted ha solicitado un cambio de contraseña. 
                Si esta solicitud no fue hecha por usted, puede ignorar este mensaje.
                Para cambiar su contraseña, haga click en el siguiente enlace:

                {enlace}

                Atentamente,
                Equipo Manejo Presupuesto";

            var mensajeEmail = new MailMessage(emisor, receptor, tema, contenidoHTML);
            await cliente.SendMailAsync(mensajeEmail);
        }
    }
}
