using DocStack.Commands;
using DocStack.Utils;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.DependencyInjection;
using Models;
using Models.ApiResponse;
using Models.Entity;
using Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DocStack.ViewModels
{
    public class DocumentsViewModel : BaseViewModel
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
                    SearchCommand.RaiseCanExecuteChanged();
                    LoadMoreCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private bool _titleFilter;
        public bool TitleFilter
        {
            get => _titleFilter;
            set
            {
                if (_titleFilter != value)
                {
                    _titleFilter = value;
                    OnPropertyChanged(nameof(TitleFilter));
                }
            }
        }

        private bool _authroFilter;
        public bool AuthorFilter
        {
            get => _authroFilter;
            set
            {
                if (_authroFilter != value)
                {
                    _authroFilter = value;
                    OnPropertyChanged(nameof(AuthorFilter));
                }
            }
        }

        private bool _publisherFilter;
        public bool PublisherFilter
        {
            get => _publisherFilter;
            set
            {
                if (_publisherFilter != value)
                {
                    _publisherFilter = value;
                    OnPropertyChanged(nameof(PublisherFilter));
                }
            }
        }

        private bool _yearFilter;
        public bool YearFilter
        {
            get => _yearFilter;
            set
            {
                if (_yearFilter != value)
                {
                    _yearFilter = value;
                    OnPropertyChanged(nameof(YearFilter));
                }
            }
        }

        private bool _sortAscending;
        public bool SortAscending
        {
            get => _sortAscending;
            set
            {
                if(_sortAscending != value)
                {
                    _sortAscending = value;
                    OnPropertyChanged(nameof(SortAscending));
                }
            }
        }
        
        private bool _isListView;

        public bool IsListView
        {
            get => _isListView;
            set
            {
                if(_isListView != value)
                {
                    _isListView = value;
                    OnPropertyChanged(nameof(IsListView));
                    OnPropertyChanged(nameof(IsGridView));
                }
            }
        }

        public bool IsGridView => !IsListView;
        
        private int _offset = 0;

        private int _limit = 10;

        private bool _allFetched = false;

        public RelayCommand<string> SearchCommand { get; set; }

        public RelayCommand<string> LoadMoreCommand { get; set; }

        public RelayCommand<string> ClearCommand { get; set; }

        public RelayCommand<Guid> AddToStarredCommand { get; set; }

        public RelayCommand<Guid> RemoveStarredCommand { get; set; }

        public RelayCommand<Guid> OpenInBrowserCommand { get; set; }

        private PaperService _paperService;
        private StarredService _starredServcie;
        private NetworkService _networkService;
        public ObservableCollection<PaperEntity> PaperEntities { get; set; }
        public DocumentsViewModel()
        {
            SearchCommand = new RelayCommand<string>(async (_) => await Search(), (_) => CanSearch());
            LoadMoreCommand = new RelayCommand<string>(async (_) => await LoadMore(), (_) => CanLoadMore());
            ClearCommand = new RelayCommand<string>((_) => Clear());
            AddToStarredCommand = new RelayCommand<Guid>(async (paperId) => await AddToStarred(paperId));
            RemoveStarredCommand  = new RelayCommand<Guid>(async (paperId) => await RemoveStarred(paperId));
            OpenInBrowserCommand = new RelayCommand<Guid>(async (paperId) => await OpenInBrowser(paperId));

            _paperService = App.ServiceProvider.GetRequiredService<PaperService>();
            _starredServcie = App.ServiceProvider.GetRequiredService<StarredService>();
            _networkService = App.ServiceProvider.GetRequiredService<NetworkService>();

            PaperEntities = new();

            SortAscending = true;
            IsListView = true;
        }

        public async Task Search()
        {
            Clear();
            await Fetch();
        }
        public async Task LoadMore()
        {
            await Fetch();
        }

        public void Clear()
        {
            _offset = 0;
            _allFetched = false;
            PaperEntities.Clear();

            LoadMoreCommand.RaiseCanExecuteChanged();
        }

        public async Task AddToStarred(Guid paperId)
        {
            try
            {
                var response = await _paperService.GetById(paperId);

                if (!response.Ok) throw response.Exception;

                PaperEntity PaperFromDb = response.Data;

                if (PaperFromDb == null) throw new Exception($"paper not found in database");

                var response_ = await _starredServcie.AddPaperToStarred(PaperFromDb);

                if (!response_.Ok) throw response.Exception;

                PaperFromDb.StarredEntity = response_.Data;

                // refresh ui 
                var index = PaperEntities.IndexOf(PaperFromDb);
                if (index != -1)
                {
                    PaperEntities.RemoveAt(index); // Remove the item
                    PaperEntities.Insert(index, PaperFromDb); // Insert it back at the same position
                }


                Toaster.ShowSuccess("starred");
            }
            catch(Exception ex)
            {
                Console.Write(ex.Message);
                Toaster.ShowError(ex.Message);
            }
        }

        public async Task RemoveStarred(Guid paperId)
        {
            try
            {
                var response = await _paperService.GetById(paperId);

                if (!response.Ok) throw response.Exception;

                PaperEntity PaperFromDb = response.Data;

                if (PaperFromDb == null) throw new Exception($"paper not found in database");
                if(PaperFromDb.StarredEntity == null) throw new Exception($"paper is not starred");

                var response_ = await _starredServcie.RemoveStarred(PaperFromDb.StarredEntityId!.Value);

                if (!response_.Ok) throw response.Exception;

                PaperFromDb.StarredEntity = null;

                // refresh ui 
                var index = PaperEntities.IndexOf(PaperFromDb);
                if (index != -1)
                {
                    PaperEntities.RemoveAt(index); // Remove the item
                    PaperEntities.Insert(index, PaperFromDb); // Insert it back at the same position
                }

                Toaster.ShowSuccess("removed from starred");
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                Toaster.ShowError(ex.Message);
            }
        }
        public async Task Fetch()
        {
            bool isAllFalse = !(_authroFilter || _publisherFilter || _yearFilter || _titleFilter);

            ServiceResponse<List<PaperEntity>> response = null;
            if (isAllFalse)
            {
                response = await _paperService.GetPapersAsync(Query,
                    _offset,
                    _limit,
                    true,
                    false,
                    false,
                    false,
                    SortAscending);
            }
            else
            {
                response = await _paperService.GetPapersAsync(Query,
                    _offset,
                    _limit,
                    _titleFilter,
                    _authroFilter,
                    _publisherFilter,
                    _yearFilter,
                    SortAscending);
            }

            if (response.Ok)
            {
                if (response.Data.Count > 0)
                    Toaster.ShowSuccess(response.Message);
                else
                    Toaster.ShowWarning("No Data fetched");
            }
            else
                Toaster.ShowError(response.Message);

            _offset += _limit;

            var data = response.Data;

            if(data.Count < _limit)
            {
                _allFetched = true;
            }

            LoadMoreCommand.RaiseCanExecuteChanged();

            foreach (PaperEntity pe in data)
            {
                PaperEntities.Add(pe);
            }
        }
      
        public async Task OpenInBrowser(Guid paperId)
        {
            PaperEntity? paper = PaperEntities.ToList().FirstOrDefault(x => x.Id == paperId);
            if (paper != null && paper.DOI != null)
            {
                bool flag = await _networkService.CheckDOIExists(paper.DOI);
                if (flag)
                {
                    System.Diagnostics.Process.Start(new ProcessStartInfo
                    {
                        FileName = paper.FullTextLink,
                        UseShellExecute = true
                    });
                }
                else
                {
                    Toaster.ShowError($"document object identifier is invalid");
                }
            }
            else
            {
                Toaster.ShowError($"document object identifier is invalid");
            }
        }

        public bool CanSearch()
        {
            return !String.IsNullOrWhiteSpace(Query);
        }

        public bool CanLoadMore()
        {
            return !String.IsNullOrWhiteSpace(Query) && _offset > 0 && !_allFetched;
        }

        public async Task HitSearchKey() => await Search();
    }
}
