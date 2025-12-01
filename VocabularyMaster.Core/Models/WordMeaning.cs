namespace VocabularyMaster.Core.Models
{
    public class WordMeaning
    {
        public int Id { get; set; }
        public int WordId { get; set; }
        public string Turkish { get; set; } = string.Empty;
        public string? ExampleSentence { get; set; }
        public int DisplayOrder { get; set; }

        // Navigation property
        public Word Word { get; set; } = null!;
    }
}