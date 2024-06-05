
using DailyVerse.Domain;

namespace VerseProviders;

public interface IVerseProvider
{
    Task<List<VerseViewModel>> GetVerseAsync(string reference, bool removeFormatting = true);
    Task<PassageViewModel> GetPassageAsync(string reference, bool includePassageReference = false);
    bool isValidApiCode(string apiCode);
}