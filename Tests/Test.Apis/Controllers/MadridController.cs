using Microsoft.AspNetCore.Mvc;

using MondoCore.Collections;
using MondoCore.Rest;
using JTran;

namespace Test.Apis.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MadridController : ControllerBase
    {
        private readonly IRestApi<Municipality> _api;
        private readonly ITransformer<List<Municipality>> _transformAll;
        private readonly ITransformer<Municipality> _transformOne;

        public MadridController(IRestApi<Municipality> api, ITransformer<List<Municipality>> transformAll, ITransformer<Municipality> transformOne)
        {
            _api          = api ?? throw new ArgumentNullException(nameof(api));
            _transformAll = transformAll ?? throw new ArgumentNullException(nameof(transformAll));
            _transformOne = transformOne ?? throw new ArgumentNullException(nameof(transformOne));
        }

        [Route("municipalities")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<string> GetMunicipalities()
        {
            var result = await _api.Get<string>("");

            return _transformAll.Transform(result);
        }

        [Route("municipality/{code}")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<string> GetMunicipality(string code)
        {
            var result = await _api.Get<string>("");

            return _transformOne.Transform(result, new TransformerContext { Arguments = (new { Code = code }).ToReadOnlyDictionary() });
        }
    }

    public class Municipality
    {
        // Don't actually need anything in here. Just used for DI
    }
}
