using System.Text;
using System.Text.RegularExpressions;
using DailyVerse.Domain;
using Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VerseProviders;

namespace Service;

public class EsvVerseService : IVerseProvider
{
    private IConfiguration _config;
    private readonly string? _esvAuthToken;
    private readonly HttpClient _httpClient;
    private readonly ILogger<EsvVerseService> _logger;

    public EsvVerseService(IConfiguration config, ILogger<EsvVerseService> logger)
    {
        _config = config;
        _esvAuthToken = Environment.GetEnvironmentVariable("EsvAuthenticationToken");

        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Token {_esvAuthToken}");
        _logger = logger;
        _logger.LogInformation($"ESV Api: {_esvAuthToken}");
    }

    public async Task<PassageViewModel> GetPassageAsync(string reference, bool includePassageReference = false)
    {
        var parameters = "&indent-poetry=false&include-headings=false&include-footnotes=false&include-verse-numbers=true&include-short-copyright=true&include-passage-references={includePassageReference.ToString()}";
        string url = $"https://api.esv.org/v3/passage/text/?q={Uri.EscapeDataString(reference)}{parameters}";

        HttpResponseMessage response = await _httpClient.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            string esvPassageJson = await response.Content.ReadAsStringAsync();
            EsvPassage esvPassage = JsonConvert.DeserializeObject<EsvPassage>(esvPassageJson);
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
        _logger.LogInformation("Esv Api Code validated");
        return string.IsNullOrWhiteSpace(apiCode) ? false : true;
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
        var verses = passage.RawPassageText?.Split("[");
        var verseList = new List<VerseViewModel>();
        
        // Get book name from first verse
        var reference = verses[0].Replace("[", "").Replace("]", "").Trim();
        var bookname = Regex.Replace(reference, @"(\d*:*\d+)$", "").Trim();

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
                        verse = verseParts[0].Replace("[", "").Replace("]", "").Trim(),
                        text = verseParts[1].Trim()
                    });
                }
            }
            i++;
        }

        passage.VerseList = verseList;
        passage.Reference = reference;
        return passage;
    }
}
