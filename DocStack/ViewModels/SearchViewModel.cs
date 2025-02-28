using DocStack.Commands;
using DocStack.Utils;
using Microsoft.Extensions.DependencyInjection;
using Models.ApiResponse;
using Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
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

        private ObservableCollection<Work> _worksList;

        public ObservableCollection<Work> WorksList
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

        public SearchViewModel()
        {
            SearchCommand = new RelayCommand<string>((_)=>SearchDocument());

            _networkService = App.ServiceProvider.GetRequiredService<NetworkService>();

            IsLoading = false;
            WorksList = new();

            for (int i = 0; i <10; i++)
            {
                WorksList.Add(new Work()
                {
                    @abstract = "good paper"
                });
            }

        }

        private async Task SearchDocument()
        {
            if (String.IsNullOrEmpty(Query)) return;

            try
            {
                IsLoading = true;
                string q = String.Join('+', Query.Split(" "));
                var response = await _networkService.GetWorks(q);

                SearchWorksResponse? data = response.Data;

                if (data == null)
                {
                    Toaster.ShowWarning("No data found. Please check your input or try again.");
                    return;
                }

                data.results.ToList().ForEach(work => { 
                    WorksList.Add(work);
                });
                

                if (response.Ok) 
                    Toaster.ShowSuccess(response.Message);
                else
                    Toaster.ShowError(response.Message);
            }

            finally
            {
                IsLoading = false;
            }     
        }

    }
}
