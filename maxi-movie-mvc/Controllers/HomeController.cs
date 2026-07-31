using maxi_movie_mvc.Data;
using maxi_movie_mvc.Models;
using maxi_movie_mvc.Service;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace maxi_movie_mvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly MovieDbContext _context;
        private const int PageSize = 8;
        private readonly LlmService _llmService;
        private readonly UserManager<Usuario> _userManager;

        public HomeController(
            ILogger<HomeController> logger,
            MovieDbContext context,
            LlmService llmService,
            UserManager<Usuario> userManager)
        {
            _logger = logger;
            _context = context;
            _llmService = llmService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int pagina = 1, string txtBusqueda = "", int generoId = 0, string vista = "grid")
        {
            if (pagina < 1) pagina = 1;

            var consulta = _context.Peliculas.AsQueryable();
            if (!string.IsNullOrEmpty(txtBusqueda))
            {
                consulta = consulta.Where(p => p.Titulo.Contains(txtBusqueda));
            }

            if (generoId > 0)
            {
                consulta = consulta.Where(p => p.GeneroId == generoId);
            }

            var totalPeliculas = await consulta.CountAsync();
            var totalPaginas = (int)Math.Ceiling(totalPeliculas / (double)PageSize);

            if (pagina > totalPaginas && totalPaginas > 0) pagina = totalPaginas;

            var peliculas = await consulta
                .Include(p => p.ListaReviews.Where(r => !r.EstaOculta))
                .OrderBy(p => p.Id)
                .Skip((pagina - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.TotalPeliculas = totalPeliculas;
            ViewBag.TxtBusqueda = txtBusqueda;

            var generos = await _context.Generos.OrderBy(g => g.Descripcion).ToListAsync();
            generos.Insert(0, new Genero { Id = 0, Descripcion = "Género" });
            ViewBag.GeneroId = new SelectList(
                generos,
                "Id",
                "Descripcion",
                generoId
            );

            ViewBag.Vista = vista;

            return View(peliculas);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            bool esAdmin = false;
            if (User?.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    esAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                }
            }

            var pelicula = await _context.Peliculas
                .Include(p => p.Genero)
                .Include(p => p.Plataforma)
                .Include(p => p.ListaReviews.Where(r => !r.EstaOculta || esAdmin))
                    .ThenInclude(r => r.Usuario)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pelicula == null)
            {
                return NotFound();
            }

            ViewBag.UserReview = false;
            if (User?.Identity?.IsAuthenticated == true)
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                ViewBag.UserReview = await _context.Reviews
                    .AnyAsync(r => r.PeliculaId == id && r.UsuarioId == userId);
            }

            return View(pelicula);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Authorize] // 👈 Exige estar autenticado para usar este endpoint
        [HttpGet]
        public async Task<IActionResult> Spoiler(string titulo)
        {
            try
            {
                var pelicula = await _context.Peliculas.FirstOrDefaultAsync(p => p.Titulo == titulo);
                if (pelicula == null)
                    return Json(new { success = false, message = "Película no encontrada." });

                // 1. Verificar si ya está almacenado en el Caché de la BD
                if (!string.IsNullOrEmpty(pelicula.SpoilerIaCache))
                {
                    return Json(new { success = true, data = pelicula.SpoilerIaCache });
                }

                // 2. Si no existe, llamar a la API de IA
                var spoilerGenerado = await _llmService.ObtenerSpoilerAsync(titulo);

                // 3. Guardar en la BD para futuras consultas
                pelicula.SpoilerIaCache = spoilerGenerado;
                await _context.SaveChangesAsync();

                return Json(new { success = true, data = spoilerGenerado });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [Authorize] // 👈 Exige estar autenticado para usar este endpoint
        [HttpGet]
        public async Task<IActionResult> Resumen(string titulo)
        {
            try
            {
                var pelicula = await _context.Peliculas.FirstOrDefaultAsync(p => p.Titulo == titulo);
                if (pelicula == null)
                    return Json(new { success = false, message = "Película no encontrada." });

                // 1. Verificar si ya está almacenado en el Caché de la BD
                if (!string.IsNullOrEmpty(pelicula.ResumenIaCache))
                {
                    return Json(new { success = true, data = pelicula.ResumenIaCache });
                }

                // 2. Si no existe, llamar a la API de IA
                var resumenGenerado = await _llmService.ObtenerResumenAsync(titulo);

                // 3. Guardar en la BD para futuras consultas
                pelicula.ResumenIaCache = resumenGenerado;
                await _context.SaveChangesAsync();

                return Json(new { success = true, data = resumenGenerado });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}