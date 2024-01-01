using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DailyVerse.Domain;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Text;
using DailyVerse.Service;
using DailyVerse.Service;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DailyVerse.Controllers
{
    public class HomeController : Controller
    {
        private VersesService verseService { get; set; }
        public HomeController()
        {
            verseService = new VersesService();
        }

        [HttpGet("")]
        [HttpGet("/home")]
        [HttpGet("/home/index")]
        [HttpGet("/votd")]
        public async Task<IActionResult> votd()
        {
            ViewData["Title"] = "Verse of the Day";

            var verses = await GetVerse("votd");
            verses.LargeSize = false;

            return View("verse", verses);
        }

        [HttpGet("/random")]
        public async Task<IActionResult> Random()
        {
            ViewData["Title"] = "Random Verse";
            return View("verse", await GetVerse("random"));
        }

        [HttpGet("/votdl/{size?}")]
        [HttpGet("/l/{size?}")]
        public async Task<IActionResult> votdLarge(double size = 1)
        {
            ViewData["Title"] = "Verse of the Day";

            var verses = await GetVerse("votd");

            ViewData["Large"] = "true";
            verses.LargeSize = true;
            verses.TextSize = $"font-size: {size}em";
            verses.ReferenceTextSize = $"font-size: {size * 1.3}em;";

            return View("versel", verses);
        }

        [HttpGet("/passage")]
        public IActionResult passage()
        {
            ViewData["Title"] = "Passage";

            return View("passage", new PassageViewModel());
        }

        [HttpGet("/passage/{passage}")]
        [HttpGet("/home/passage/{passage}")]
        public async Task<IActionResult> passage(string passage, string format = "")
        {
            return View("passage", await GetPassage(passage, format));
        }

        [HttpPost("/passagepost/")]
        [HttpPost("/home/passagepost/")]
        public async Task<IActionResult> passagePost(string passage, string format = "")
        {
            return View("passage", await GetPassage(passage, format));
        }

        private async Task<PassageViewModel> GetVerse(string reference)
        {
            PassageViewModel verses = new PassageViewModel();
            verses.VerseList = await verseService.GetVerseAsync(reference);
            ViewData["Reference"] = BuildReference(verses.VerseList.FirstOrDefault());
            return verses;
        }

        private async Task<PassageViewModel> GetPassage(string passage = "", string format = "")
        {
            var verses = new PassageViewModel();

            ViewData["Title"] = "Passage";
            ViewData["Passage"] = passage;
            verses.Format = format;

            if (string.IsNullOrWhiteSpace(passage))
            {
                return verses;
            }
            else
            {
                verses.VerseList = await verseService.GetVerseAsync(passage);
                ViewData["Reference"] = BuildReference(verses);
                return verses;
            }
        }

        private static string BuildReference(PassageViewModel verses)
        {
            if (verses.Error)
            {
                return string.Empty;
            }

            StringBuilder title = new StringBuilder();
            title.Append($"{verses.BookName} {verses.Chapter}:{verses.FirstVerse}");
            if (verses.VerseList.Count > 1)
            {
                title.Append($"-{verses.LastVerse}");
            }

            return title.ToString();
        }

        private static string BuildReference(VerseViewModel verse)
        {
            return $"{verse.bookname} {verse.chapter}:{verse.verse}";
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
