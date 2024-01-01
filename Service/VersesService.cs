using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using DailyVerse.Domain;
using Newtonsoft.Json;

namespace DailyVerse.Service
{
    public class VersesService
    {
        public async Task<List<VerseViewModel>> GetVerseAsync(string reference)
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
                        verse.text = verse.text.Replace("<b>", "").Replace("</b>", "");
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

        public async Task<PassageViewModel> GetPassageAsync(string reference)
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
    }
}