using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PackageManagerServer.Models.Entities;
using PackageManagerServer.Services;
using PM.Models.Manifests;

namespace PackageManagerServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PackageManagerController : ControllerBase
    {
       
        private readonly ILogger<PackageManagerController> _logger;

        public PackageManagerController(ILogger<PackageManagerController> logger)
        {
            _logger = logger;
        }

        [HttpGet()]
        [Route("packages")]
        public ActionResult<IEnumerable<PackageManifest>> GetAll()
        {
            try
            {
                return Ok(ContextService.Connection.Select<PackageEntity>()
                    .Where(x => !string.IsNullOrWhiteSpace(x?.Data))
                    .Select(x => JsonConvert.DeserializeObject<PackageManifest>(x.Data))
                    .Where(x => x is not null));
            }
            catch (Exception ex) 
            {
                return UnprocessableEntity(ex);
            }
        }

        [HttpGet()]
        [Route("packages/{name}/tag/{tag}")]
        public ActionResult<IEnumerable<PackageManifest>> GetAll([FromRoute] string name, [FromRoute] string tag)
        {
            try
            {
                return Ok(ContextService.Connection.Select<PackageEntity>()
                    .Where(x => !string.IsNullOrWhiteSpace(x?.Data) && 
                                x?.Name == name &&
                                x?.Tag == tag)
                    .Select(x => JsonConvert.DeserializeObject<PackageManifest>(x.Data))
                    .Where(x => x is not null).FirstOrDefault());
            }
            catch (Exception ex)
            {
                return UnprocessableEntity(ex);
            }
        }

        [HttpGet()]
        [Route("tags/{name}")]
        public ActionResult<IEnumerable<string>> GetTags([FromRoute] string name)
        {
            try
            {
                return Ok(ContextService.Connection.Select<PackageEntity>()
                    .Where(x => !string.IsNullOrWhiteSpace(x?.Data) &&
                                x?.Name == name)
                    .Select(x => x.Tag)
                    .Where(x => x is not null));
            }
            catch (Exception ex)
            {
                return UnprocessableEntity(ex);
            }
        }

        [HttpPost]
        [Route("packages")]
        public ActionResult PostPackage([FromBody] PackageManifest manifest)
        {
            try
            {
                ContextService.Connection.Insert(new PackageEntity
                {
                    Guid = $"{manifest.Name}:{manifest.Tag}",
                    Name = manifest.Name,
                    Tag = manifest.Tag,
                    Data = JsonConvert.SerializeObject(manifest)
                });
                return Ok();
            }
            catch (Exception ex)
            {
                return Conflict(ex.Message);
            }
        }
    }
}
