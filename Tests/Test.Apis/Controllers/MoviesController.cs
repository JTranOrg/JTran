using Microsoft.AspNetCore.Mvc;

using MondoCore.Common;
using MondoCore.Rest;
using System.Reflection;
using Test.Apis.Classes;

namespace Test.Apis.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MoviesController : ControllerBase
    {
        private readonly IRestApi<Movie> _api;
        private readonly ITransformer<Movie> _transform;

        public MoviesController(IRestApi<Movie> api, ITransformer<Movie> transform)
        {
            _api       = api ?? throw new ArgumentNullException(nameof(api));
            _transform = transform ?? throw new ArgumentNullException(nameof(transform));
        }

        [Route("{year}")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<string> GetMovies(string year)
        {
            var result = await _api.Get<string>("");

            return _transform.Transform(result, new { Year = year } );
        }
    }

    public class Movie
    {
        // Don't actually need anything in here. Just used for DI
    }
}
