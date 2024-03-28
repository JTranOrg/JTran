using Microsoft.AspNetCore.Mvc;

using MondoCore.Rest;
using Test.Apis.Classes;

namespace Test.Apis.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MadridController : ControllerBase
    {
        private readonly IRestApi<Municipality> _api;
        private readonly ITransformer<Municipality> _transform;

        public MadridController(IRestApi<Municipality> api, ITransformer<Municipality> transform)
        {
            _api       = api ?? throw new ArgumentNullException(nameof(api));
            _transform = transform ?? throw new ArgumentNullException(nameof(transform));
        }

        [HttpGet(Name = "GetMunicipalities")]
        public async Task<string> Get()
        {
            var result = await _api.Get<string>("");

            return _transform.Transform(result);
        }
    }

    public class Municipality
    {
        public string? Code             { get; set; }
        public string? Name             { get; set; }
        public string? Nuts4Code        { get; set; }
        public string? Nuts4Name        { get; set; }
        public string? INECode          { get; set; }
        public double? Density          { get; set; }
        public double? Area             { get; set; }
    }
}
