
using DailyVerse.Domain;
using DailyVerse.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DailyVerse.Controllers;

public class ApiController : Controller
{
    private readonly VersesService verseService;

    public ApiController(VersesService versesService)
    {
        verseService = versesService;
    }

    [HttpGet("api/votd/{apicode}")]
    [AllowAnonymous]
    public async Task<IActionResult> votd(string apicode)
    {
        PassageViewModel verses = new PassageViewModel();
        if (!verseService.isValidApiCode(apicode))
        {
            return Unauthorized();
        }
        
        var verseList = await verseService.GetVerseAsync("votd", true);

        return Ok(verseList.FirstOrDefault()?.FullText());
    }
}