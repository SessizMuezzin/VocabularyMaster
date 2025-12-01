using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using VocabularyMaster.Core.Models;
using VocabularyMaster.Core.Enums;

namespace VocabularyMaster.WPF.Views
{
    public partial class AddEditWordDialog : Window
    {
        public Word? WordData { get; private set; }
        private ObservableCollection<WordMeaning> Meanings { get; set; }

        public AddEditWordDialog(Word? existingWord = null)
        {
            InitializeComponent();

            Meanings = new ObservableCollection<WordMeaning>();
            MeaningsItemsControl.ItemsSource = Meanings;

            if (existingWord != null)
            {
                Title = "Kelime Düzenle";
                WordData = existingWord;
                EnglishTextBox.Text = existingWord.English;
                CategoryTextBox.Text = existingWord.Category ?? string.Empty;
                DifficultyComboBox.SelectedIndex = (int)existingWord.DifficultyLevel - 1;

                // Mevcut anlamları yükle
                if (existingWord.Meanings != null && existingWord.Meanings.Any())
                {
                    foreach (var meaning in existingWord.Meanings.OrderBy(m => m.DisplayOrder))
                    {
                        Meanings.Add(new WordMeaning
                        {
                            Id = meaning.Id,
                            Turkish = meaning.Turkish,
                            ExampleSentence = meaning.ExampleSentence,
                            DisplayOrder = meaning.DisplayOrder
                        });
                    }
                }
                else if (!string.IsNullOrEmpty(existingWord.Turkish))
                {
                    // Eski format - Turkish ve ExampleSentence varsa
                    Meanings.Add(new WordMeaning
                    {
                        Turkish = existingWord.Turkish,
                        ExampleSentence = existingWord.ExampleSentence,
                        DisplayOrder = 1
                    });
                }
            }
            else
            {
                Title = "Yeni Kelime Ekle";
                DifficultyComboBox.SelectedIndex = 0;
                
                // Başlangıçta bir boş anlam ekle
                Meanings.Add(new WordMeaning { DisplayOrder = 1 });
            }
        }

        private void AddMeaning_Click(object sender, RoutedEventArgs e)
        {
            Meanings.Add(new WordMeaning 
            { 
                DisplayOrder = Meanings.Count + 1 
            });
        }

        private void RemoveMeaning_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button && button.Tag is WordMeaning meaning)
            {
                if (Meanings.Count <= 1)
                {
                    MessageBox.Show("En az bir anlam olmalıdır!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                Meanings.Remove(meaning);
                
                // DisplayOrder'ları yeniden düzenle
                int order = 1;
                foreach (var m in Meanings)
                {
                    m.DisplayOrder = order++;
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Validasyon
            if (string.IsNullOrWhiteSpace(EnglishTextBox.Text))
            {
                MessageBox.Show("İngilizce kelime alanı zorunludur!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!Meanings.Any() || Meanings.All(m => string.IsNullOrWhiteSpace(m.Turkish)))
            {
                MessageBox.Show("En az bir Türkçe anlam girmelisiniz!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Boş anlamları temizle
            var validMeanings = Meanings.Where(m => !string.IsNullOrWhiteSpace(m.Turkish)).ToList();

            if (WordData == null)
            {
                WordData = new Word
                {
                    DateAdded = DateTime.Now
                };
            }

            WordData.English = EnglishTextBox.Text.Trim();
            WordData.Category = string.IsNullOrWhiteSpace(CategoryTextBox.Text) ? null : CategoryTextBox.Text.Trim();
            WordData.DifficultyLevel = (DifficultyLevel)(DifficultyComboBox.SelectedIndex + 1);

            // Anlamları kaydet
            WordData.Meanings.Clear();
            int displayOrder = 1;
            foreach (var meaning in validMeanings)
            {
                WordData.Meanings.Add(new WordMeaning
                {
                    Turkish = meaning.Turkish.Trim(),
                    ExampleSentence = string.IsNullOrWhiteSpace(meaning.ExampleSentence) 
                        ? null 
                        : meaning.ExampleSentence.Trim(),
                    DisplayOrder = displayOrder++
                });
            }

            // Geriye dönük uyumluluk için ilk anlamı Turkish alanına da koy
            if (WordData.Meanings.Any())
            {
                var firstMeaning = WordData.Meanings.First();
                WordData.Turkish = firstMeaning.Turkish;
                WordData.ExampleSentence = firstMeaning.ExampleSentence;
            }

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}