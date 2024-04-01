using Microsoft.AspNetCore.Mvc;

using MondoCore.Common;
using MondoCore.Rest;
using System.Reflection;
using Test.Apis.Classes;

using Newtonsoft.Json;
using System.Xml;

namespace Test.Apis.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HistoryController : ControllerBase
    {
        private readonly IRestApi<History> _api;
        private readonly ITransformer<History> _transform;

        public HistoryController(IRestApi<History> api, ITransformer<History> transform)
        {
            _api       = api ?? throw new ArgumentNullException(nameof(api));
            _transform = transform ?? throw new ArgumentNullException(nameof(transform));
        }

        [Route("{beginYear}/{endYear}")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<string> GetMovies(string beginYear, string endYear)
        {
            // This api doesn't return valid json so we need to load as xml and convert ourselves
            var result = await _api.Get<string>($"?format=xml&begin_date={beginYear}0101&end_date={endYear}0101&lang=en");

            if(result == "")
                return "[]";

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(result);

            var json = JsonConvert.SerializeXmlNode(doc);

            return _transform.Transform(json);
        }
    }

    public class History
    {
        // Don't actually need anything in here. Just used for DI
    }
}
