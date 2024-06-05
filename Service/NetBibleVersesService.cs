using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using DailyVerse.Domain;
using Newtonsoft.Json;
using VerseProviders;

namespace DailyVerse.Service
{
    public class NetBibleVersesService : IVerseProvider
    {
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

                    if (removeFormatting)
                    {
                        foreach (var verse in verses)
                        {
                            verse.text = verse.text = verse.text.Replace("<b>", "").Replace("</b>", "");
                        }
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

        public async Task<PassageViewModel> GetPassageAsync(string reference, bool includePassageReference = false)
        {
            var passage = new PassageViewModel();
            passage.VerseList = await GetVerseAsync(reference);

            if (passage != null && passage.VerseList.FirstOrDefault().error)
            {
                passage.Error = true;
                passage.ErrorMessage = passage.VerseList.FirstOrDefault().errorMessage;
            }

            return passage;
        }

        public bool isValidApiCode(string apiCode)
        {
            return (String.IsNullOrWhiteSpace(apiCode)) ? false : true;

            int apiDate = DateTime.Now.DayOfYear;

            if (int.TryParse(apiCode.Replace("aaca-", ""), out int apiNumericCode))
            {
                apiNumericCode = (int)Math.Round((double)(apiNumericCode - 1515) / 1155);
                if (apiDate -1 <= apiNumericCode && apiNumericCode <= apiDate + 1)
                {
                    return true;
                }
            }

            return false;
        }
    }
}