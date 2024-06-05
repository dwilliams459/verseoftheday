
using DailyVerse.Domain;
using DailyVerse.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service;
using VerseProviders;

namespace DailyVerse.Controllers;

public class ApiController : Controller
{
    private readonly NetBibleVersesService verseService;
    private readonly EsvVerseService _esvVerseService;
    private readonly IVerseProvider _verseProvider;

    public ApiController(NetBibleVersesService versesService, EsvVerseService esvVerseService)
    {
        verseService = versesService;
        _esvVerseService = esvVerseService;
        _verseProvider = esvVerseService;
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

    /// <summary>
    /// Retrieves the passage of a Bible verse based on the provided API code and reference.
    /// </summary>
    /// <param name="apiCode">The API code used for authorization.</param>
    /// <param name="reference">The reference of the Bible verse.</param>
    /// <returns>The passage of the Bible verse as a string.</returns>
    [HttpGet("api/passage/{apiCode}/{reference}")]
    [AllowAnonymous]
    public async Task<IActionResult> Passage(string apiCode, string reference)
    {
        if (!_esvVerseService.isValidApiCode(apiCode))
        {
            return Unauthorized();
        }

        var passage = await _verseProvider.GetPassageAsync(reference.Replace('+', ' '), true);

        return Ok(passage.JoinedVerses()); 
    }
}