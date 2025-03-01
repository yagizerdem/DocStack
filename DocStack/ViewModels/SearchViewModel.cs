using DocStack.Commands;
using DocStack.Utils;
using Microsoft.Extensions.DependencyInjection;
using Models.ApiResponse;
using Models.Entity;
using Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;


namespace DocStack.ViewModels
{
    public class SearchViewModel : BaseViewModel
    {
        private string _query;
        public string Query
        {
            get => _query;
            set
            {
                if(_query != value)
                {
                    _query = value;
                    OnPropertyChanged(nameof(Query));
                }
            }
        }

        private bool _isLoading;

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged(nameof(IsLoading));
                    OnPropertyChanged(nameof(IsNotLoading));
                }
            }
        }
        public bool IsNotLoading => !IsLoading;


        private bool _isListView;
        public bool IsListView
        {
            get => _isListView;
            set
            {
                if (_isListView != value)
                {
                    _isListView = value;
                    OnPropertyChanged(nameof(IsListView));
                    OnPropertyChanged(nameof(IsGridView));
                }
            }
        }
        public bool IsGridView => !IsListView;

        private bool _isSearchView;

        public bool IsSearchView
        {
            get => _isSearchView;
            set
            {
                if (_isSearchView != value)
                {
                    _isSearchView = value;
                    OnPropertyChanged(nameof(IsSearchView));
                    OnPropertyChanged(nameof(IsDetailsView));
                }
            }
        }

        public bool IsDetailsView => !IsSearchView;

        private ObservableCollection<WorkEntity> _worksList;

        public ObservableCollection<WorkEntity> WorksList
        {
            get => _worksList;
            set
            {
                _worksList = value;
                OnPropertyChanged(nameof(WorksList));
            }
        }

        private NetworkService _networkService;
        public ICommand SearchCommand { get; set; }

        public ICommand LoadMoreDocumentCommand { get; set; }

        public ICommand ClearDocumentsCommand { get; set; }

        public ICommand GoToSearchViewCommand { get; set; }

        public ICommand SaveToDatabaseCommand { get; set; }

        public ICommand StartDownloadCommand { get; set; }

        private int _pageIndex = 0;

        private int _limit = 10;

        private Dictionary<WorkEntity, Work> _keyValuePairs;

        private WorkEntity _selectedWorkEntity;

        public WorkEntity SelectedWorkEntity
        {
            get => _selectedWorkEntity;
            set {
                _selectedWorkEntity = value;
                OnPropertyChanged(nameof(SelectedWorkEntity));
                OnSelectedWorkEntityChanged(); 
            }
        }


        // contains full details
        private Work _selectedWork;

        public Work SelectedWork
        {
            get => _selectedWork;
            set
            {
                _selectedWork = value;
                OnPropertyChanged(nameof(SelectedWork));
            }
        }
        //

        private double _downloadProgress;

        public double DownloadProgress
        {
            get => _downloadProgress;
            set
            {
                if(_downloadProgress != value)
                {
                    _downloadProgress = value;
                    OnPropertyChanged(nameof(DownloadProgress));
                }
            }
        }

        public SearchViewModel()
        {
            SearchCommand = new RelayCommand<string>(async (_) => await SearchDocument());
            LoadMoreDocumentCommand = new RelayCommand<string>(async (_) => await LoadMoreDocument());
            ClearDocumentsCommand = new RelayCommand<string>((_) => ClearDocuments());
            GoToSearchViewCommand = new RelayCommand<string>((_) => GoToSearchView());
            SaveToDatabaseCommand = new RelayCommand<string>(async (_) => await SaveToDatabase());
            StartDownloadCommand = new RelayCommand<string>(async (_) => await StartDownload());

            _networkService = App.ServiceProvider.GetRequiredService<NetworkService>();

            IsLoading = false;
            WorksList = new();
            _keyValuePairs = new();
            IsListView = true;
            IsSearchView = false;

            SelectedWork = new()
            {
                title = "test titleafjlajflajflazlfjalfjaljfjalfalflajflajflalfljafljaljflj",
                downloadUrl = "http ",
                fieldOfStudy = "study field",
                fullText = String.Join("", Enumerable.Range(0, 100).ToList())
            };
        }

        private async Task SearchDocument()
        {
            ClearDocuments();
            await FetchDocuments();
        }

        private async Task LoadMoreDocument() {
            if(_pageIndex == 0)
            {
                Toaster.ShowWarning("search documents before loading more");
                return;
            }
            await FetchDocuments();
        }

        private async Task FetchDocuments()
        {
            if(string.IsNullOrWhiteSpace(Query))
            {
                Toaster.ShowWarning($"Enter keywords to search documents");
                return;
            }
            try
            {
                IsLoading = true;
                string q = String.Join('+', Query.Split(" "));
                int offset = _pageIndex * _limit;
                var response = await _networkService.GetWorks(q, offset, _limit);

                SearchWorksResponse? data = response.Data;

                if (data == null)
                {
                    Toaster.ShowWarning($"No data found for input : {Query}. Please check your input or try again.");
                    return;
                }

                data.results.ToList().ForEach(work =>
                {
                    WorkEntity workEntity = new()
                    {
                        Abstract = work.@abstract,
                        Authors = String.Join(" ", work.authors.ToList().Select(x => x.name)),
                        DOI = work.@doi,
                        Year = work.yearPublished.ToString(),
                        FullTextLink = work.downloadUrl,
                        Title = work.title,
                        Publisher = work.publisher,
                    };

                    WorksList.Add(workEntity);
                    _keyValuePairs.Add(workEntity, work);
                });


                if (response.Ok)
                {
                    Toaster.ShowSuccess(response.Message);
                    _pageIndex++;
                }
                else
                    Toaster.ShowError(response.Message);
            }

            finally
            {
                IsLoading = false;
            }
        }
    
        private void ClearDocuments()
        {
            WorksList.Clear();
            _keyValuePairs.Clear();
            _pageIndex = 0;
        }

        private void OnSelectedWorkEntityChanged()
        {
            if (SelectedWorkEntity != null)
            {
                IsSearchView = false;
                SelectedWork =  _keyValuePairs[SelectedWorkEntity];
            }
        }

        public async void HitSearchKey() => await SearchDocument();

        private void GoToSearchView() => IsSearchView = true;
    
        private async Task SaveToDatabase()
        {
            var e = this.SelectedWorkEntity;
            ;
        }

        private async Task StartDownload()
        {
            string url = SelectedWork.downloadUrl;

            // track progress of download
            var progress = new Progress<double>(percentage =>
            {
                this.DownloadProgress = percentage;
            });

            string downloadsFolder = KnownFolders.GetPath(KnownFolder.Downloads);

            var response = await _networkService.DownloadFromInternet(url, downloadsFolder, progress);

            if (!response.Ok)
                Toaster.ShowError(response.Message);    
            else
                Toaster.ShowSuccess(response.Message);
        }
    
    }


    public enum KnownFolder
    {
        Contacts,
        Downloads,
        Favorites,
        Links,
        SavedGames,
        SavedSearches
    }

    public static class KnownFolders
    {
        private static readonly Dictionary<KnownFolder, Guid> _guids = new()
        {
            [KnownFolder.Contacts] = new("56784854-C6CB-462B-8169-88E350ACB882"),
            [KnownFolder.Downloads] = new("374DE290-123F-4565-9164-39C4925E467B"),
            [KnownFolder.Favorites] = new("1777F761-68AD-4D8A-87BD-30B759FA33DD"),
            [KnownFolder.Links] = new("BFB9D5E0-C6A9-404C-B2B2-AE6DB6AF4968"),
            [KnownFolder.SavedGames] = new("4C5C32FF-BB9D-43B0-B5B4-2D72E54EAAA4"),
            [KnownFolder.SavedSearches] = new("7D1D3A04-DEBB-4115-95CF-2F29DA2920DA")
        };

        public static string GetPath(KnownFolder knownFolder)
        {
            return SHGetKnownFolderPath(_guids[knownFolder], 0);
        }

        [DllImport("shell32",
            CharSet = CharSet.Unicode, ExactSpelling = true, PreserveSig = false)]
        private static extern string SHGetKnownFolderPath(
            [MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags,
            nint hToken = 0);
    }


}
