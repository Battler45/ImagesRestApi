using System;
using System.Threading.Tasks;
using ImagesRestApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ImagesRestApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ImagesController : ControllerBase
    {
        private readonly ILogger<ImagesController> _logger;
        private readonly IImagesService _service;

        public ImagesController(ILogger<ImagesController> logger, IImagesService service)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var image = await _service.GetImageAsync(id);
            if (image == null) return NotFound();
            return File(image.File, "image/jpeg");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            throw  new NotImplementedException();
        }

        [HttpPost]
        public async Task<IActionResult> Post()
        {
            throw new NotImplementedException();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}