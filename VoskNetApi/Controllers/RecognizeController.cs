using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using VoskNetApi.Models;
using VoskNetApi.Services;

namespace VoskNetApi.Controllers;

[ApiController]
[Route("[controller]")]
public class RecognizeController : Controller
{

    private readonly ITextRecognizeService _fileService;
    public RecognizeController(ITextRecognizeService fileService)
    {
        _fileService = fileService;
    }

    [HttpPost("{lang}")]
    public async Task<ActionResult<TextRecognized>> UploadFileForTheRecognizeText(IFormFile file, string lang)
    {
        var res = _fileService.Recognize(file.OpenReadStream(), file.FileName,lang);
        return Ok(res);

    }
}