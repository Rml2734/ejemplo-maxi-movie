using maxi_movie_mvc.Data;
using maxi_movie_mvc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace maxi_movie_mvc.Controllers
{
    public class ReviewController : Controller
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly MovieDbContext _context;

        public ReviewController(UserManager<Usuario> userManager, MovieDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // GET: ReviewController
        public async Task<ActionResult> Index() // Mis Reviews
        {
            var userId = _userManager.GetUserId(User);
            var reviews = await _context.Reviews
                .Include(r => r.Pelicula)
                .Where(r => r.UsuarioId == userId)
                .ToListAsync();

            return View(reviews);
        }

        // GET: ReviewController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ReviewController/Create
        public ActionResult Create()
        {
            // Al tener la vista parcial '_CreateReview', se la especificamos aquí explícitamente
            return PartialView("_CreateReview");
        }

        // POST: ReviewController/Create
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ReviewCreateViewModel review)
        {
            try
            {
                review.UsuarioId = _userManager.GetUserId(User);

                // Validación de si ya existe una review del mismo usuario para la misma película
                var reviewExiste = _context.Reviews
                    .FirstOrDefault(r => r.PeliculaId == review.PeliculaId && r.UsuarioId == review.UsuarioId);

                if (reviewExiste != null)
                {
                    TempData["ReviewExiste"] = "Ya has realizado una reseña para esta película.";
                    return RedirectToAction("Details", "Home", new { id = review.PeliculaId });
                }

                if (ModelState.IsValid)
                {
                    var nuevaReview = new Review
                    {
                        PeliculaId = review.PeliculaId,
                        UsuarioId = review.UsuarioId,
                        Rating = review.Rating,
                        Comentario = review.Comentario,
                        FechaReview = DateTime.Now
                    };

                    _context.Reviews.Add(nuevaReview);
                    _context.SaveChanges();

                    TempData["ReviewExito"] = "¡Reseña publicada con éxito!";
                    return RedirectToAction("Details", "Home", new { id = review.PeliculaId });
                }

                // Si las validaciones del modelo fallan (ej. comentario vacío), redirige con TempData
                TempData["ReviewExiste"] = "Por favor, verifica los datos de tu reseña. El comentario o la calificación no son válidos.";
                return RedirectToAction("Details", "Home", new { id = review.PeliculaId });
            }
            catch
            {
                TempData["ReviewExiste"] = "Ocurrió un error inesperado al procesar tu reseña.";
                return RedirectToAction("Details", "Home", new { id = review.PeliculaId });
            }
        }

        // GET: ReviewController/Edit/5
        [Authorize]
        public async Task<ActionResult> Edit(int id)
        {
            var review = await _context.Reviews
                .Include(r => r.Pelicula)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (review == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            if (review.UsuarioId != user.Id && !isAdmin)
                return Forbid();

            var reviewViewModel = new ReviewCreateViewModel
            {
                Id = review.Id,
                PeliculaId = review.PeliculaId,
                UsuarioId = review.UsuarioId,
                Rating = review.Rating,
                Comentario = review.Comentario,
                PeliculaTitulo = review.Pelicula?.Titulo
            };

            return View(reviewViewModel);
        }

        // POST: ReviewController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<ActionResult> Edit(ReviewCreateViewModel review)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var reviewExistente = await _context.Reviews.FirstOrDefaultAsync(r => r.Id == review.Id);
                    if (reviewExistente == null)
                        return NotFound();

                    var user = await _userManager.GetUserAsync(User);
                    bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

                    if (review.UsuarioId != user.Id && !isAdmin)
                        return Forbid();

                    reviewExistente.Rating = review.Rating;
                    reviewExistente.Comentario = review.Comentario;

                    _context.Reviews.Update(reviewExistente);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Index", "Review");
                }

                return View(review);
            }
            catch
            {
                return View(review);
            }
        }

        // GET: ReviewController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ReviewController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}