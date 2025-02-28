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

        public ICommand NavigateHomePanel { get; set; }
        public ICommand NavigateSearchPanel { get; set; }
        public ICommand NavigateFavouritesPanel { get; set; }
        public ICommand NavigateMyPapersPanel { get; set; }
        public ICommand NavigateSettingsPanel { get; set; }
        public ICommand NavigateDocumentsPanel { get; set; }


        private BaseViewModel _currentViewModel;

        public BaseViewModel CurrentViewModel { 
            get => _currentViewModel; 
            set 
            {
                _currentViewModel = value;
                OnPropertyChanged(nameof(CurrentViewModel));
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
            NavigateFavouritesPanel = new RelayCommand<string>((_) => CurrentViewModel = _favouritesViewModel,
                (_) => CurrentViewModel != _favouritesViewModel);
            NavigateMyPapersPanel = new RelayCommand<string>((_) => CurrentViewModel = _myPapersViewModel,
                (_) => CurrentViewModel != _myPapersViewModel);
            NavigateSettingsPanel = new RelayCommand<string>((_) => CurrentViewModel = _settingsViewModel,
                (_) => CurrentViewModel != _settingsViewModel);
            NavigateDocumentsPanel = new RelayCommand<string>((_) => CurrentViewModel = _documentsViewModel,
                (_) => CurrentViewModel != _documentsViewModel);


            NavigateSearchPanel.Execute(null);
        }
        

    }
}
