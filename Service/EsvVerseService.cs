using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using DailyVerse.Domain;
using DailyVerse.Service;
using Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VerseProviders;

namespace DailyVerse.Service;

public class EsvVerseService : IVerseProvider
{
    private IConfiguration _config;
    private readonly string? _esvAuthToken;
    private readonly NetBibleVersesService _netBibleService;
    private readonly HttpClient _httpClient;
    private readonly ILogger<EsvVerseService> _logger;

    public EsvVerseService(IConfiguration config, ILogger<EsvVerseService> logger, NetBibleVersesService netBibleVersesService)
    {
        _config = config;
        _esvAuthToken = Environment.GetEnvironmentVariable("EsvAuthenticationToken");
        _netBibleService = netBibleVersesService;

        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Token {_esvAuthToken}");
        _logger = logger;
        _logger.LogInformation($"ESV Api: {_esvAuthToken}");
    }

    public async Task<PassageViewModel> GetPassageAsync(string reference, bool includePassageReference = true)
    {
        var parameters = new StringBuilder();
        parameters.Append($"&indent-poetry=false");
        parameters.Append($"&include-headings=false");
        parameters.Append($"&include-footnotes=false");
        parameters.Append($"&include-verse-numbers=true");
        parameters.Append($"&include-short-copyright=true");
        parameters.Append($"&include-passage-references=true");

        string url = $"https://api.esv.org/v3/passage/text/?q={Uri.EscapeDataString(reference)}{parameters.ToString()}";

        HttpResponseMessage response = await _httpClient.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            string esvPassageJson = await response.Content.ReadAsStringAsync();
            EsvPassage esvPassage = JsonConvert.DeserializeObject<EsvPassage>(esvPassageJson) ?? new EsvPassage();

            _logger.LogDebug($"Request to ESV API succeeded with status code {response.StatusCode}");

            return ConvertEsvPassageToPassageViewModel(esvPassage);
        }
        else
        {
            _logger.LogError($"Request to ESV API failed with status code {response.StatusCode}");
            throw new HttpRequestException($"Request to ESV API failed with status code {response.StatusCode}");
        }
    }

    public async Task<List<VerseViewModel>> GetVerseAsync(string reference, bool removeFormatting = true)
    {
        var passage = await GetPassageAsync(reference);
        return passage.VerseList;
    }

    public bool isValidApiCode(string apiCode)
    {
        try
        {
            // Get the date from the api code
            var dateString = apiCode.Substring(apiCode.IndexOf("-") + 1);
            var apiCodeDate = DateTime.ParseExact(dateString, "MMddyyyy", CultureInfo.InvariantCulture);
    
            if (apiCodeDate > DateTime.Now.AddDays(-2) && apiCodeDate < DateTime.Now.AddDays(2))
            {
                _logger.LogDebug("Esv Api Code validated");
                return true;
            }
        }
        catch (System.Exception)
        {
            _logger.LogDebug("Esv Api Code not valid");
            return false;
        }

        _logger.LogDebug("Esv Api Code not valid");
        return false;
    }

    /// <summary>
    /// Convert EsvPassage object to PassageViewModel object.  Parse the passage[0] text, convert to verses.
    /// </summary>
    /// <param name="esvPassage"></param>
    /// <returns></returns>
    private PassageViewModel ConvertEsvPassageToPassageViewModel(EsvPassage esvPassage)
    {
        PassageViewModel passage = new PassageViewModel
        {
            RawPassageText = esvPassage?.Passages?.FirstOrDefault()
        };

        // Split passage into verses
        var verses = passage.RawPassageText?.Replace("(ESV)", "").Split("[");
        var verseList = new List<VerseViewModel>();
        
        // Get book name from first verse
        var reference = verses[0].Replace("[", "").Replace("]", "").Trim();

        // Get the book and chapter from reference, but allow for titles such as 'John', '2 Peter', '3 John', etc.
        var bookname = reference.Split(" ")[0];
        var chapter = reference.Split(":")[0].Split(" ")[1];
        if (bookname.StartsWith("1 ") || bookname.StartsWith("2 ") || bookname.StartsWith("3 "))
        {
            bookname = $"{reference.Substring(0, 1)} {reference.Split(" ")[1]}"; 
            chapter = reference.Split(":")[0].Split(" ")[0];
        }  
        
        // For each verse, create a VerseViewModel object and add to list
        int i = 0;
        foreach (var passageText in verses.ToList())
        {
            if (!String.IsNullOrWhiteSpace(passageText?.Trim().Replace(" ", "")))
            {
                VerseViewModel verse = new VerseViewModel();
                var verseParts = passageText.Split("]");

                // For all but first row, create VerseViewModel with bookname, verse, and text
                if (i > 0)
                {
                    verseList.Add(new VerseViewModel
                    {
                        bookname = bookname,
                        chapter = chapter,
                        verse = verseParts[0].Replace("[", "").Replace("]", "").Trim(),
                        text = verseParts[1].Trim()
                    });
                }
            }
            i++;
        }

        passage.VerseList = verseList;
        passage.Reference = reference;
        passage.Translation = "ESV";
        return passage;
    }

    public async Task<PassageViewModel> GetVotdAsync(bool includePassageReference = true)
    {
        var passage = await _netBibleService.GetVotdAsync(includePassageReference);

        return await GetPassageAsync(passage.Reference, includePassageReference);
    }
}
