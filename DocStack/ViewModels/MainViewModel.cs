using DocStack.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DocStack.ViewModels
{
    public class MainViewModel: BaseViewModel
    {

        private HomeViewModel   _homeViewModel;
        private SearchViewModel _searchViewModel;
        private FavouritesViewModel _favouritesViewModel;
        private MyPapersViewModel _myPapersViewModel;
        private SettingsViewModel _settingsViewModel;
        private DocumentsViewModel _documentsViewModel;

        public string test { get; set; }

        public RelayCommand<string> NavigateHomePanel { get; set; }
        public RelayCommand<string> NavigateSearchPanel { get; set; }
        public RelayCommand<string> NavigateFavouritesPanel { get; set; }
        public RelayCommand<string> NavigateMyPapersPanel { get; set; }
        public RelayCommand<string> NavigateSettingsPanel { get; set; }
        public RelayCommand<string> NavigateDocumentsPanel { get; set; }


        private BaseViewModel _currentViewModel;

        public BaseViewModel CurrentViewModel { 
            get => _currentViewModel; 
            set 
            {
                _currentViewModel = value;
                OnPropertyChanged(nameof(CurrentViewModel));
                NavigateHomePanel.RaiseCanExecuteChanged();
                NavigateSearchPanel.RaiseCanExecuteChanged();
                NavigateMyPapersPanel.RaiseCanExecuteChanged();
                NavigateSettingsPanel.RaiseCanExecuteChanged();
                NavigateDocumentsPanel.RaiseCanExecuteChanged();
                NavigateFavouritesPanel.RaiseCanExecuteChanged();
            }
        }



        public MainViewModel()
        {
            _homeViewModel = new();
            _searchViewModel = new();
            _myPapersViewModel = new();
            _settingsViewModel = new();
            _favouritesViewModel = new();
            _documentsViewModel = new();

            NavigateHomePanel = new RelayCommand<string>((_) =>CurrentViewModel = _homeViewModel,
                (_) => CurrentViewModel != _homeViewModel);
            NavigateSearchPanel = new RelayCommand<string>((_) => CurrentViewModel = _searchViewModel, 
                (_) => CurrentViewModel != _searchViewModel);
            NavigateFavouritesPanel = new RelayCommand<string>(async (_) =>
            {
                CurrentViewModel = _favouritesViewModel;
                await _favouritesViewModel.Fetch();
                _favouritesViewModel.ResetFilters();
            },
                (_) => CurrentViewModel != _favouritesViewModel);
            NavigateMyPapersPanel = new RelayCommand<string>((_) => CurrentViewModel = _myPapersViewModel,
                (_) => CurrentViewModel != _myPapersViewModel);
            NavigateSettingsPanel = new RelayCommand<string>((_) => CurrentViewModel = _settingsViewModel,
                (_) => CurrentViewModel != _settingsViewModel);
            NavigateDocumentsPanel = new RelayCommand<string>((_) => CurrentViewModel = _documentsViewModel,
                (_) => CurrentViewModel != _documentsViewModel);


            _currentViewModel = _homeViewModel;
        }
        

    }
}
