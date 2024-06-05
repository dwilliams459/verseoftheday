using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DailyVerse.Domain
{
    public class PassageViewModel
    {
        public PassageViewModel(string format = "Default")
        {
            Format = format;
        }

        public List<VerseViewModel> VerseList { get; set; }
        public string BookName => VerseList?.FirstOrDefault()?.bookname;
        public string Chapter => VerseList?.FirstOrDefault()?.chapter;
        public string FirstVerse => VerseList?.FirstOrDefault()?.verse;
        public string LastVerse => (VerseList.Count() > 0) ? VerseList?.LastOrDefault()?.verse : string.Empty;
        public string Reference { get; set; }

        public bool Error { get; set; }
        public string ErrorMessage { get; set; }

        public string JoinedVerses(bool indludeReference = false) 
        {
            var joinedVerses = $"{string.Join(" ", VerseList.Select(v => v.text.Trim()))}";
            return (indludeReference && !string.IsNullOrWhiteSpace(Reference)) ? $"{joinedVerses}  -{Reference}" : joinedVerses;
        }  

        public string? RawPassageText { get; set; }
        
        private string _format;
        public string Format
        {
            get 
            { 
                _format = (string.IsNullOrWhiteSpace(_format) ? "Default" : _format);
                return _format;
            }
            set { _format = (string.IsNullOrWhiteSpace(value) ? "Default" : value); }
        }

        public List<SelectListItem> DisplayStyles
        {
            get
            {
                return new List<SelectListItem>() {
                    new SelectListItem("Default", "Default", ("Default" == Format)),
                    new SelectListItem("Book", "Book", ("Book" == Format)),
                    new SelectListItem("Manuscript", "Manuscript", ("Manuscript" == Format))
                };
            }
        }

        public bool LargeSize { get; set; }
        public string TextSize { get; set; }
        public string ReferenceTextSize { get; set; }
    }

    public class VerseViewModel
    {
        public string bookname { get; set; }
        public string chapter { get; set; }
        public string verse { get; set; }
        public string text { get; set; }
        public string title { get; set; }
        public bool error { get; set; }
        public string errorMessage { get; set; }

        public string Reference() => $"{bookname} {chapter}:{verse}";
        public string FullText() => $"{text.Trim()}  -{bookname} {chapter}:{verse}".Replace("   ", "");

    }
}