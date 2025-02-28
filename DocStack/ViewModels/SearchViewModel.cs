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

        public SearchViewModel()
        {
            SearchCommand = new RelayCommand<string>((_)=>SearchDocument());
            LoadMoreDocumentCommand = new RelayCommand<string>((_) => LoadMoreDocument());
            ClearDocumentsCommand = new RelayCommand<string>((_) => ClearDocuments());

            _networkService = App.ServiceProvider.GetRequiredService<NetworkService>();

            IsLoading = false;
            WorksList = new();
            _keyValuePairs = new();
            IsListView = true;
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
                Work workModel =  _keyValuePairs[SelectedWorkEntity];
                ;
            }
        }

        public async void HitSearchKey() => await SearchDocument();
        
    }
}
