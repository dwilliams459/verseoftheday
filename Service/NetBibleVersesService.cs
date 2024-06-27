using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using DailyVerse.Domain;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VerseProviders;

namespace DailyVerse.Service
{
    public class NetBibleVersesService : IVerseProvider
    {
        private ILogger<NetBibleVersesService> _logger;

        public NetBibleVersesService(ILogger<NetBibleVersesService> logger)
        {
            _logger = logger;
        }

        public async Task<List<VerseViewModel>> GetVerseAsync(string reference, bool removeFormatting = true)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://labs.bible.org");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/x-javascript"));

            List<VerseViewModel> verses = new List<VerseViewModel>();

            try
            {
                reference = reference.Trim().Replace(" ", "+");
                HttpResponseMessage response = await client.GetAsync($"https://labs.bible.org/api/?passage={reference}&type=json");
                if (response.IsSuccessStatusCode)
                {
                    var v = await response.Content.ReadAsStringAsync();
                    verses = JsonConvert.DeserializeObject<List<VerseViewModel>>(v);

                    foreach (var verse in verses)
                    {
                        verse.bookname = verse.bookname;
                        verse.chapter = verse.chapter;
                        verse.text = (removeFormatting) ? verse.text.Replace("<b>", "").Replace("</b>", "") : verse.text;
                    }
                }
            }
            catch (Exception ex)
            {
                verses.Add(new VerseViewModel { error = true, errorMessage = ex.Message });
                return verses;
            }
            return verses ?? new List<VerseViewModel>();
        }

        public async Task<PassageViewModel> GetPassageAsync(string reference, bool includePassageReference = true)
        {
            var passage = new PassageViewModel();
            passage.VerseList = await GetVerseAsync(reference);

            if (passage != null && passage.VerseList.FirstOrDefault().error)
            {
                passage.Error = true;
                passage.ErrorMessage = passage.VerseList.FirstOrDefault().errorMessage;
            }

            var firstVerse = passage.VerseList.FirstOrDefault();

            // Set reference
            passage.Reference = $"{firstVerse?.bookname} {firstVerse?.chapter}";
            passage.Reference = (string.IsNullOrWhiteSpace(firstVerse?.verse)) ? passage.Reference : $"{passage.Reference}:{firstVerse?.verse}";

            if (passage.VerseList.Count() > 1)
            {
                var lastVerse = passage.VerseList.LastOrDefault();
                passage.Reference = $"{passage.Reference}-{lastVerse?.verse}";
            }

            passage.Translation = "Net Bible";
            return passage;
        }

        public async Task<PassageViewModel> GetVotdAsync(bool includePassageReference = true)
        {
            return await GetPassageAsync("votd", includePassageReference);
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
                    _logger.LogDebug("Net Bible Api Code validated");
                    return true;
                }
            }
            catch (System.Exception)
            {
                _logger.LogDebug("Net Bible Api Code not valid");
                return false;
            }

            _logger.LogDebug("Net Bible Api Code not valid");
            return false;
        }
    }
}