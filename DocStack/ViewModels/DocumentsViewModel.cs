using DocStack.Commands;
using DocStack.Utils;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.DependencyInjection;
using Models;
using Models.Entity;
using Service;
using System;
using System.Collections.Generic;
using System.Linq;
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

        private int _offset = 0;

        private int _limit = 2;

        private bool _allFetched = false;

        public RelayCommand<string> SearchCommand { get; set; }

        public RelayCommand<string> LoadMoreCommand { get; set; }

        public RelayCommand<string> ClearCommand { get; set; }

        private PaperService _paperService;
        public DocumentsViewModel()
        {
            SearchCommand = new RelayCommand<string>(async (_) => await Search(), (_) => CanSearch());
            LoadMoreCommand = new RelayCommand<string>(async (_) => await LoadMore(), (_) => CanLoadMore());
            ClearCommand = new RelayCommand<string>((_) => Clear());
            _paperService = App.ServiceProvider.GetRequiredService<PaperService>();
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

            LoadMoreCommand.RaiseCanExecuteChanged();
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
                    false);
            }
            else
            {
                response = await _paperService.GetPapersAsync(Query,
                    _offset,
                    _limit,
                    _titleFilter,
                    _authroFilter,
                    _publisherFilter,
                    _yearFilter);
            }

            if (!response.Ok)
                Toaster.ShowWarning(response.Message);
            else
                Toaster.ShowSuccess(response.Message);

            _offset += _limit;

            var data = response.Data;

            if(data.Count < _limit)
            {
                _allFetched = true;
            }

            LoadMoreCommand.RaiseCanExecuteChanged();
        }
        public bool CanSearch()
        {
            return !String.IsNullOrWhiteSpace(Query);
        }

        public bool CanLoadMore()
        {
            return !String.IsNullOrWhiteSpace(Query) && _offset > 0 && !_allFetched;
        }
    }
}
