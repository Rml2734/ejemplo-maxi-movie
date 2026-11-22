// Plantillas HTML de los correos de MaxiMovie.
// Está en el espacio de nombres global a propósito, para poder usarla sin agregar "using".
public static class EmailTemplates
{
    public static string Bienvenida(string nombre, string urlSitio)
    {
        var nombreSeguro = System.Net.WebUtility.HtmlEncode(nombre ?? "");
        var urlSegura = System.Net.WebUtility.HtmlEncode(urlSitio ?? "");

        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
</head>
<body style='background-color: #121212; font-family: ""Segoe UI"", Tahoma, Geneva, Verdana, sans-serif; margin: 0; padding: 30px 10px; color: #ffffff;'>
    <div style='max-width: 550px; margin: 0 auto; background-color: #1e1e2d; border-radius: 12px; overflow: hidden; box-shadow: 0 8px 24px rgba(0,0,0,0.5); border: 1px solid #2d2d3f;'>

        <!-- Encabezado con branding -->
        <div style='background-color: #0f0f17; padding: 25px; text-align: center; border-bottom: 3px solid #ffc107;'>
            <h1 style='color: #ffc107; margin: 0; font-size: 26px; letter-spacing: 2px; font-weight: 800; text-transform: uppercase;'>MAXIMOVIE 🎬</h1>
        </div>

        <!-- Contenido principal -->
        <div style='padding: 35px 30px; text-align: center;'>
            <h2 style='color: #ffffff; margin-top: 0; font-size: 22px; font-weight: 600;'>¡Bienvenido, {nombreSeguro}!</h2>
            <p style='color: #a0a0b5; font-size: 15px; line-height: 1.6; margin-bottom: 25px;'>
                Tu cuenta se creó correctamente. Ya puedes explorar el catálogo, escribir tus reseñas y descubrir resúmenes de películas generados con inteligencia artificial.
            </p>

            <!-- Botón CTA -->
            <div style='margin: 30px 0;'>
                <a href='{urlSegura}' style='background-color: #ffc107; color: #121212; padding: 14px 28px; text-decoration: none; border-radius: 6px; font-weight: bold; font-size: 15px; display: inline-block;'>
                    Explorar el catálogo
                </a>
            </div>

            <p style='color: #6c6c80; font-size: 13px; line-height: 1.5; margin-top: 30px;'>
                Si no creaste esta cuenta, puedes ignorar este mensaje de manera segura.
            </p>
        </div>

        <!-- Pie de página -->
        <div style='background-color: #0f0f17; padding: 18px; text-align: center; font-size: 12px; color: #555566; border-top: 1px solid #252535;'>
            <p style='margin: 0;'>© {System.DateTime.UtcNow.Year} MaxiMovie. Todos los derechos reservados.</p>
        </div>

    </div>
</body>
</html>";
    }
}
