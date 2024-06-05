namespace Domain;

using Newtonsoft.Json;

public class EsvPassage
{
    [JsonProperty("query")]
    public string? Query { get; set; }

    [JsonProperty("canonical")]
    public string? Canonical { get; set; }

    [JsonProperty("parsed")]
    public List<List<int>>? Parsed { get; set; }

    [JsonProperty("passage_meta")]
    public List<PassageMeta>? PassageMeta { get; set; }

    [JsonProperty("passages")]
    public List<string> Passages { get; set; }
}

public class PassageMeta
{
    [JsonProperty("canonical")]
    public string Canonical { get; set; }

    [JsonProperty("chapter_start")]
    public List<int>? ChapterStart { get; set; }

    [JsonProperty("chapter_end")]
    public List<int>? ChapterEnd { get; set; }

    [JsonProperty("prev_verse")]
    public int? PreviousVerse { get; set; }

    [JsonProperty("next_verse")]
    public int? NextVerse { get; set; }

    [JsonProperty("prev_chapter")]
    public List<int>? PreviousChapter { get; set; }

    [JsonProperty("next_chapter")]
    public List<int>? NextChapter { get; set; }
}
