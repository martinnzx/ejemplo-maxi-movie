using maxi_movie_mvc.Data;
using maxi_movie_mvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace maxi_movie_mvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly MovieDbContext _context;

        public HomeController(ILogger<HomeController> logger, MovieDbContext context)
        {
            _logger = logger;
            _context = context;
        }
        public async Task<IActionResult> Index(int page = 1, string? search = null, int? genreId = null)
        {
            const int pageSize = 8;

            // Calcular el número de página válido
            if (page < 1) page = 1;

            // Construir la query base con Include para el género
            var query = _context.Peliculas.Include(p => p.Genero).AsQueryable();

            // Aplicar filtro de búsqueda si existe
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => p.Titulo.Contains(search));
            }

            // Aplicar filtro de género si existe
            if (genreId.HasValue && genreId > 0)
            {
                query = query.Where(p => p.GeneroId == genreId);
            }

            // Obtener el total de películas (después del filtro)
            int totalPeliculas = await query.CountAsync();

            // Calcular el total de páginas
            int totalPages = (int)Math.Ceiling(totalPeliculas / (double)pageSize);

            // Obtener las películas de la página actual
            var peliculas = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Obtener lista de géneros para el dropdown
            var generos = await _context.Generos.OrderBy(g => g.Descripcion).ToListAsync();

            // Pasar información de paginación a la vista
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = totalPages;
            ViewData["TotalMovies"] = totalPeliculas;
            ViewData["SearchTerm"] = search;
            ViewData["SelectedGenreId"] = genreId;
            ViewData["Generos"] = generos;

            return View(peliculas);
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
    }
}
