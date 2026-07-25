using maxi_movie_mvc.Models;
using maxi_movie_mvc.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace maxi_movie_mvc.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly SignInManager<Usuario> _signInManager;
        private readonly ImagenStorage _imagenStorage;
        private readonly IEmailService _emailService;

        public UsuarioController(UserManager<Usuario> userManager, SignInManager<Usuario> signInManager, ImagenStorage imagenStorage, IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _imagenStorage = imagenStorage;
            _emailService = emailService;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel usuario)
        {
            if (ModelState.IsValid)
            {
                var resultado = await _signInManager.PasswordSignInAsync(usuario.Email, usuario.Clave, usuario.Recordarme, lockoutOnFailure: false);
                if (resultado.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Inicio de sesión inválido.");
                }
            }
            return View(usuario);
        }

        public IActionResult Registro()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registro(RegistroViewModel usuario)
        {
            if (ModelState.IsValid)
            {
                var nuevoUsuario = new Usuario
                {
                    UserName = usuario.Email,
                    Email = usuario.Email,
                    Nombre = usuario.Nombre,
                    Apellido = usuario.Apellido,
                    ImagenUrlPerfil = "/images/default-avatar.png"
                };
                var resultado = await _userManager.CreateAsync(nuevoUsuario, usuario.Clave);
                if (resultado.Succeeded)
                {
                    await _signInManager.SignInAsync(nuevoUsuario, isPersistent: false);
                    await _emailService.SendAsync(nuevoUsuario.Email, "Bienvenido a Maxi Movie", "<h1>Gracias por registrarte en Maxi Movie!</h1><p>Esperamos que disfrutes de nuestra plataforma.</p>");
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    foreach (var error in resultado.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            return View(usuario);
        }

        public IActionResult Logout()
        {
            _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> MiPerfil()
        {
            var usuarioActual = await _userManager.GetUserAsync(User);

            var usuarioVM = new MiPerfilViewModel
            {
                Nombre = usuarioActual.Nombre,
                Apellido = usuarioActual.Apellido,
                Email = usuarioActual.Email,
                ImagenUrlPerfil = usuarioActual.ImagenUrlPerfil
            };

            return View(usuarioVM);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MiPerfil(MiPerfilViewModel usuarioVM)
        {
            if (ModelState.IsValid)
            {
                var usuarioActual = await _userManager.GetUserAsync(User);

                try
                {
                    if (usuarioVM.ImagenPerfil is not null && usuarioVM.ImagenPerfil.Length > 0)
                    {
                        if (!string.IsNullOrWhiteSpace(usuarioActual.ImagenUrlPerfil) && !usuarioActual.ImagenUrlPerfil.Contains("default-avatar.png"))
                        {
                            await _imagenStorage.DeleteAsync(usuarioActual.ImagenUrlPerfil);
                        }

                        var nuevaRuta = await _imagenStorage.SaveAsync(usuarioActual.Id, usuarioVM.ImagenPerfil);
                        usuarioActual.ImagenUrlPerfil = nuevaRuta;
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                    return View(usuarioVM);
                }

                usuarioActual.Nombre = usuarioVM.Nombre;
                usuarioActual.Apellido = usuarioVM.Apellido;

                var resultado = await _userManager.UpdateAsync(usuarioActual);

                if (resultado.Succeeded)
                {
                    TempData["Mensaje"] = "Perfil actualizado con éxito.";
                    return RedirectToAction(nameof(MiPerfil));
                }
                else
                {
                    foreach (var error in resultado.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            var usuarioReintento = await _userManager.GetUserAsync(User);
            usuarioVM.ImagenUrlPerfil = usuarioReintento.ImagenUrlPerfil;

            return View(usuarioVM);
        }

        // ==========================================
        // 🔑 RECUPERACIÓN Y RESTABLECIMIENTO DE CONTRASEÑA
        // ==========================================

        // GET: /Usuario/RecuperarPassword
        [HttpGet]
        public IActionResult RecuperarPassword()
        {
            return View();
        }

        // POST: /Usuario/RecuperarPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecuperarPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                TempData["Mensaje"] = "Por favor ingresa un correo electrónico válido.";
                return View();
            }

            var usuario = await _userManager.FindByEmailAsync(email);

            if (usuario != null)
            {
                // 1. Generar el token de reseteo
                var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);

                // 2. Crear el enlace seguro
                var callbackUrl = Url.Action("ResetPassword", "Usuario",
                    new { token = token, email = usuario.Email }, protocol: Request.Scheme);

                // 3. Plantilla HTML "PRO" Estilo Dark Cinema
                string cuerpoEmail = $@"
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
                    <h2 style='color: #ffffff; margin-top: 0; font-size: 22px; font-weight: 600;'>¿Olvidaste tu contraseña?</h2>
                    <p style='color: #a0a0b5; font-size: 15px; line-height: 1.6; margin-bottom: 25px;'>
                        Hola <strong style='color: #ffffff;'>{usuario.Nombre}</strong>. Recibimos una solicitud para restablecer la contraseña de tu cuenta. Haz clic en el siguiente botón para crear una nueva:
                    </p>

                    <!-- Botón CTA -->
                    <div style='margin: 30px 0;'>
                        <a href='{callbackUrl}' style='background-color: #ffc107; color: #121212; padding: 14px 28px; text-decoration: none; border-radius: 6px; font-weight: bold; font-size: 15px; display: inline-block; transition: all 0.3s ease;'>
                            Restablecer Contraseña
                        </a>
                    </div>

                    <p style='color: #6c6c80; font-size: 13px; line-height: 1.5; margin-top: 30px;'>
                        Si no realizaste esta solicitud, puedes ignorar este mensaje de manera segura; tu contraseña seguirá siendo la misma.
                    </p>
                </div>

                <!-- Pie de página -->
                <div style='background-color: #0f0f17; padding: 18px; text-align: center; font-size: 12px; color: #555566; border-top: 1px solid #252535;'>
                    <p style='margin: 0;'>© {DateTime.Now.Year} MaxiMovie. Todos los derechos reservados.</p>
                </div>

            </div>
        </body>
        </html>";

                await _emailService.SendAsync(usuario.Email, "Restablecer Contraseña - MaxiMovie", cuerpoEmail);
            }

            TempData["Mensaje"] = "Si el correo ingresado coincide con una cuenta registrada, te hemos enviado las instrucciones a tu bandeja de entrada.";
            return View();
        }

        // GET: /Usuario/ResetPassword
        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            if (token == null || email == null)
            {
                return RedirectToAction("Login");
            }

            var modelo = new ResetPasswordViewModel { Token = token, Email = email };
            return View(modelo);
        }

        // POST: /Usuario/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            var usuario = await _userManager.FindByEmailAsync(modelo.Email);
            if (usuario == null)
            {
                // Por seguridad no revelamos si existe o no
                return RedirectToAction("Login");
            }

            var resultado = await _userManager.ResetPasswordAsync(usuario, modelo.Token, modelo.Password);

            if (resultado.Succeeded)
            {
                TempData["Mensaje"] = "Tu contraseña ha sido restablecida con éxito. Ya puedes iniciar sesión.";
                return RedirectToAction("Login");
            }

            foreach (var error in resultado.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(modelo);
        }
    }
}
