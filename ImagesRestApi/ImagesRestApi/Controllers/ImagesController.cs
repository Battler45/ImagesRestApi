using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImagesRestApi.DTO;
using ImagesRestApi.Filters;
using ImagesRestApi.Models;
using ImagesRestApi.Services.Interfaces;
using ImagesRestApi.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace ImagesRestApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ImagesController : ControllerBase
    {
        private readonly ILogger<ImagesController> _logger;
        private readonly IImagesService _service;
        private readonly LinkGenerator _linkGenerator;

        public ImagesController(ILogger<ImagesController> logger, IImagesService service, LinkGenerator linkGenerator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _linkGenerator = linkGenerator ?? throw new ArgumentNullException(nameof(linkGenerator));
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
            var deletedImagesCount = await _service.DeleteImage(id);
            return deletedImagesCount == 0 ? NotFound() : StatusCode(StatusCodes.Status204NoContent);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(List<Guid> ids)
        {
            if (ids == null) return BadRequest();
            var deletedImagesCount = await _service.DeleteImages(ids);
            return deletedImagesCount != ids.Count ? NotFound($"{deletedImagesCount} has been deleted instead {ids.Count}") : StatusCode(StatusCodes.Status204NoContent, "all images has been deleted");
        }

        #region snippet_UploadImages
        // The following upload method:
        //
        // 1. Disable the form value model binding to take control of handling 
        //    potentially large files.
        //
        // 2. Typically, anti forgery tokens are sent in request body. Since we 
        //    don't want to read the request body early, the tokens are sent via 
        //    headers. The anti forgery token filter first looks for tokens in 
        //    the request header and then falls back to reading the body.
        [HttpPost]
        [DisableFormValueModelBinding]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Post()
        {
            if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType)) return BadRequest("Content type is not multipart");
            List<ImageDTO> images;
            try
            {
                var boundary = MultipartRequestHelper.GetBoundary(MediaTypeHeaderValue.Parse(Request.ContentType), FormOptions.DefaultMultipartBoundaryLengthLimit);
                var reader = new MultipartReader(boundary, HttpContext.Request.Body);
                images = await _service.SaveImages(reader);
            }
            catch (InvalidDataException e)
            {
                return BadRequest(e.Message);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            var imagesIdsUris = images.Select(i => new
            {
                i.Id,
                uri = _linkGenerator.GetPathByAction(HttpContext, nameof(Get), values: new { i.Id })
            }).ToList();
            return images.Count == 1 ? Created(imagesIdsUris.First().uri, imagesIdsUris.First().Id) : StatusCode(StatusCodes.Status201Created, imagesIdsUris);
        }

        [HttpPut]
        [DisableFormValueModelBinding]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Put()
        {
            if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType)) return BadRequest("Content type is not multipart");
            List<ImageDTO> images;
            try
            {
                var boundary = MultipartRequestHelper.GetBoundary(MediaTypeHeaderValue.Parse(Request.ContentType), FormOptions.DefaultMultipartBoundaryLengthLimit);
                var reader = new MultipartReader(boundary, HttpContext.Request.Body);
                images = await _service.SaveImages(reader);
            }
            catch (InvalidDataException e)
            {
                return BadRequest(e.Message);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            var imagesIdsUris = images.Select(i => new
            {
                i.Id,
                uri = _linkGenerator.GetPathByAction(HttpContext, nameof(Get), values: new { i.Id })
            }).ToList();
            return images.Count == 1 ? Created(imagesIdsUris.First().uri, imagesIdsUris.First().Id) : StatusCode(StatusCodes.Status201Created, imagesIdsUris);
            throw new NotImplementedException();
        }
        #endregion
    }
}