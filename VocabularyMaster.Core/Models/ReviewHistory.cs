using System;
using VocabularyMaster.Core.Enums;

namespace VocabularyMaster.Core.Models
{
    public class ReviewHistory
    {
        public int Id { get; set; }
        public int WordId { get; set; }
        public DateTime ReviewDate { get; set; }
        public bool IsCorrect { get; set; }
        public TestMode TestMode { get; set; }
    }
}