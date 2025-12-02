using System;
using System.Collections.Generic;
using VocabularyMaster.Core.Enums;

namespace VocabularyMaster.Core.Models
{
    public class Word
    {
        public int Id { get; set; }
        public string English { get; set; } = string.Empty;
        public string? Category { get; set; }
        public DifficultyLevel DifficultyLevel { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime? LastReviewed { get; set; }
        public int ReviewCount { get; set; }
        public int CorrectCount { get; set; }
        public int WrongCount { get; set; }
        public bool IsFavorite { get; set; }

        // Anlamlar koleksiyonu
        public List<WordMeaning> Meanings { get; set; } = new List<WordMeaning>();

        // Eski alanlar - geriye dönük uyumluluk için
        public string Turkish { get; set; } = string.Empty;
        public string? ExampleSentence { get; set; }

        public double SuccessRate => ReviewCount > 0
            ? (double)CorrectCount / ReviewCount * 100
            : 0;

        public bool IsNew => ReviewCount == 0 || (DateTime.Now - DateAdded).TotalDays <= 7;


    }
}