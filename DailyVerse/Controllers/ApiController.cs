
using DailyVerse.Domain;
using DailyVerse.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VerseProviders;

namespace DailyVerse.Controllers;

public class ApiController : Controller
{
    private readonly NetBibleVersesService verseService;
    private readonly EsvVerseService _esvVerseService;
    private readonly IVerseProvider _verseProvider;
    private readonly ILogger<ApiController> _logger;

    public ApiController(NetBibleVersesService versesService, EsvVerseService esvVerseService, ILogger<ApiController> logger)
    {
        verseService = versesService;
        _esvVerseService = esvVerseService;
        _verseProvider = esvVerseService;
        _logger = logger;
    }

    [HttpGet("api/votd/{apicode}")]
    [AllowAnonymous]
    public async Task<IActionResult> votd(string apicode, [FromQuery(Name = "v")] string version = "esv")
    {
        PassageViewModel verses = new PassageViewModel();
        if (!verseService.isValidApiCode(apicode))
        {
            return Unauthorized();
        }

        var passage = new PassageViewModel();
        if (version != "esv")
        {
            if (!verseService.isValidApiCode(apicode)) { return Unauthorized(); }
            passage = await verseService.GetVotdAsync(true);
        }
        else
        {
            if (!_esvVerseService.isValidApiCode(apicode)) { return Unauthorized(); }
            passage = await _esvVerseService.GetVotdAsync(true);
        }

        return Ok(passage.JoinedVerses(true));
    }

    /// <summary>
    /// Retrieves the passage of a Bible verse based on the provided API code and reference.
    /// </summary>
    /// <param name="apiCode">The API code used for authorization.</param>
    /// <param name="reference">The reference of the Bible verse.</param>
    /// <returns>The passage of the Bible verse as a string.</returns>
    [HttpGet("api/passage/")]
    [AllowAnonymous]
    public async Task<IActionResult> Passage([FromQuery(Name = "a")] string? apiCode, [FromQuery(Name = "r")] string reference, 
        [FromQuery(Name = "ir")] bool includeReference = true,  [FromQuery(Name = "v")] string version = "esv")
    {
        try
        {
        var passage = new PassageViewModel();
        if (version != "esv")
        {
            if (!verseService.isValidApiCode(apiCode)) { return Unauthorized(); }
            passage = await verseService.GetPassageAsync(reference, includeReference);
        }
        else
        {
            if (!_esvVerseService.isValidApiCode(apiCode)) { return Unauthorized(); }
            passage = await _esvVerseService.GetPassageAsync(reference, includeReference);
        }


            return Ok(passage.JoinedVerses(includeReference));
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error Getting API passage");
            throw;
        }
    }
}