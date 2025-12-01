using System.Windows.Input;
using VocabularyMaster.WPF.Commands;

namespace VocabularyMaster.WPF.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private ViewModelBase _currentViewModel;
        private readonly DashboardViewModel _dashboardViewModel;
        private readonly WordListViewModel _wordListViewModel;
        private readonly TestViewModel _testViewModel;
        private readonly StatisticsViewModel _statisticsViewModel;
        private readonly FlashcardViewModel _flashcardViewModel;
        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        public ICommand NavigateToDashboardCommand { get; }
        public ICommand NavigateToWordsCommand { get; }
        public ICommand NavigateToTestCommand { get; }
        public ICommand NavigateToStatisticsCommand { get; }
        public ICommand NavigateToFlashcardCommand { get; }
        public MainViewModel(
            DashboardViewModel dashboardViewModel,
            WordListViewModel wordListViewModel,
            TestViewModel testViewModel,
            FlashcardViewModel flashcardViewModel,
            StatisticsViewModel statisticsViewModel)
        {
            _dashboardViewModel = dashboardViewModel;
            _wordListViewModel = wordListViewModel;
            _testViewModel = testViewModel;
            _statisticsViewModel = statisticsViewModel;
            _flashcardViewModel = flashcardViewModel;

            _currentViewModel = dashboardViewModel;

            NavigateToDashboardCommand = new RelayCommand(_ => NavigateToDashboard());
            NavigateToWordsCommand = new RelayCommand(_ => CurrentViewModel = _wordListViewModel);
            NavigateToTestCommand = new RelayCommand(_ => CurrentViewModel = _testViewModel);
            NavigateToStatisticsCommand = new RelayCommand(_ => NavigateToStatistics());
            NavigateToFlashcardCommand = new RelayCommand(_ => CurrentViewModel = _flashcardViewModel);
        }

        private void NavigateToDashboard()
        {
            CurrentViewModel = _dashboardViewModel;
            _dashboardViewModel.RefreshCommand.Execute(null);
        }

        private void NavigateToStatistics()
        {
            CurrentViewModel = _statisticsViewModel;
            _statisticsViewModel.RefreshCommand.Execute(null);
        }
    }
}