using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
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
using File = ImagesRestApi.Models.File;

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

        #region  Get
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var image = await _service.GetImageAsync(id);
            if (image == null) return NotFound();
            return File(image.Content, image.ContentType);
        }
        #endregion

        #region  Delete

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



        #endregion

        #region Post
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
        public async Task<IActionResult> Post()
        {
            List<ImageDTO> imagesDto;
            try
            {
                if (MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
                {
                    var boundary = MultipartRequestHelper.GetBoundary(MediaTypeHeaderValue.Parse(Request.ContentType),
                        FormOptions.DefaultMultipartBoundaryLengthLimit);
                    var reader = new MultipartReader(boundary, Request.Body);
                    imagesDto = await _service.SaveImages(reader);
                }
                else if (Request.ContentType.Contains("image/"))
                {
                    var image = await _service.SaveImage(Request.BodyReader, Request.ContentType);
                    imagesDto = new List<ImageDTO>() { image };
                }
                else return BadRequest("Expected a multipart/ or image/ request");
            }
            catch (InvalidDataException e)
            {
                return BadRequest(e.Message);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            var images = ToImages(imagesDto);
            return images.Count == 1 ? Created(images.First().Uri, images.First().Id) : StatusCode(StatusCodes.Status201Created, images);
        }


        [HttpPost("url")]
        public async Task<IActionResult> Post([FromBody]string url, [FromServices] IHttpClientFactory httpClientFactory)
        {
            var file = new File();
            var httpClient = httpClientFactory.CreateClient();
            using var result = await httpClient.GetAsync(url);
            if (result.IsSuccessStatusCode)
                file.Content = await result.Content.ReadAsByteArrayAsync();
            else
                return BadRequest("");

            var dto = await _service.SaveImage(file);
            return Created(GenerateUri(dto), dto.Id);
            //return File(file, "image/jpg");
        }
        [HttpPost("urls")]
        public async Task<IActionResult> Post([FromBody] List<string> urls, [FromServices] IHttpClientFactory httpClientFactory)
        {
            var files = new List<File>();
            var httpClient = httpClientFactory.CreateClient();
            var requests = urls.Select(url => httpClient.GetAsync(url)).ToArray();
            Task.WaitAll(requests);
            if (requests.Any(r => !r.Result.IsSuccessStatusCode)) return BadRequest("");
            var contents = requests.Select(request => request.Result.Content.ReadAsByteArrayAsync()).ToArray();
            Task.WaitAll(contents);
            files = contents.Select(c => new File() {Content = c.Result}).ToList();
            /*
            foreach (var url in urls)
            {
                using var result = await httpClient.GetAsync(url);
                if (result.IsSuccessStatusCode)
                    files.Add(new File() { Content = await result.Content.ReadAsByteArrayAsync() });
                else
                    return BadRequest("");
            }
            */
            var dto = await _service.SaveImages(files);
            var images = ToImages(dto);
            return StatusCode(StatusCodes.Status201Created, images);
        }
        #endregion

        #region Put
        [HttpPut("{id}")]
        [DisableFormValueModelBinding]
        public async Task<IActionResult> Put(Guid id)
        {
            ImageDTO imageDto;
            try
            {
                if (MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
                {
                    var boundary = MultipartRequestHelper.GetBoundary(MediaTypeHeaderValue.Parse(Request.ContentType),
                        FormOptions.DefaultMultipartBoundaryLengthLimit);
                    var reader = new MultipartReader(boundary, Request.Body);
                    imageDto = await _service.UpdateImage(reader, id);
                }
                else if (Request.ContentType.Contains("image/"))
                    imageDto = await _service.UpdateImage(Request.BodyReader, id, Request.ContentType);
                else return BadRequest("Expected a multipart/ or image/ request");
            }
            catch (InvalidDataException e)
            {
                return BadRequest(e.Message);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            return Created(GenerateUri(imageDto), imageDto.Id);
        }

        #endregion

        #region Base64
        /// <summary>
        /// I don't want to optimize this two methods(Base64) using streams because base64 is not optimal format to send files
        /// </summary>
        /*
        [HttpPost("Base64")]
        public async Task<IActionResult> PostBase64([FromBody] Base64File model)
        {
            var file = new File(Convert.FromBase64String(model.Base64));
            var image = await _service.SaveImage(file);
            var uri = GenerateUri(image);
            return Created(uri, new Image()
            {
                Id = image.Id,
                Uri = uri
            });
        }
        */
        [HttpPost("Base64")]
        public async Task<IActionResult> PostBase64([FromBody] List<Base64File> models)
        {
            var files = new List<File>();
            models.ForEach(m => files.Add(new File()
            {
                Content = Convert.FromBase64String(m.Base64)
            }));
            var imagesDto = await _service.SaveImages(files);
            var images = ToImages(imagesDto);
            return StatusCode(StatusCodes.Status201Created, images);
        }
        [HttpPut("Base64")]
        public async Task<IActionResult> PutBase64([FromBody] List<Base64Image> models)
        {
            var images = new List<Image>();
            models.ForEach(m => images.Add(new Image()
            {
                Id = m.Id,
                Content = Convert.FromBase64String(m.Base64)
            }));
            await _service.UpdateImages(images);
            return StatusCode(StatusCodes.Status204NoContent);
        }
        //UpdateImages
        #endregion

        private string GenerateUri(ImageDTO image) =>
            _linkGenerator.GetPathByAction(HttpContext, nameof(Get), values: new { image.Id });
        private List<Image> ToImages(List<ImageDTO> images) => images.Select(i => new Image
        {
            Id = i.Id,
            Uri = GenerateUri(i)
        }).ToList();
    }
}