using DocStack.Commands;
using DocStack.Utils;
using iTextSharp.text.pdf;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using Models.Entity;
using Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocStack.ViewModels
{
    public class MyPapersViewModel : BaseViewModel
    {
        private readonly MyDocumentsService _myDocumentsService;
        public RelayCommand<string> ImportDocumentsCommand { get; set; }

        public RelayCommand<Guid> DeleteDocumentCommand { get; set; }

        private bool _isLoading = false;
        
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if(_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged(nameof(IsLoading));
                    OnPropertyChanged(nameof(IsNotLoading));
                }
            }
        }

        public bool IsNotLoading => !IsLoading;
        public ObservableCollection<MyDocuments> MyDocuments { get;set; }
        public MyPapersViewModel()
        {
            ImportDocumentsCommand = new RelayCommand<string>(async (_) => await ImportDocuments());
            DeleteDocumentCommand = new RelayCommand<Guid>(async (documentId) => await DeleteDocument(documentId));

            _myDocumentsService = App.ServiceProvider.GetRequiredService<MyDocumentsService>();
            MyDocuments = new();

            Task.Run(() => FetchAllDocuments());
        }
        private async Task ImportDocuments()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Select a File",
                Filter = "PDF Files (*.pdf)|*.pdf",
                Multiselect = false
            };
            bool? result = openFileDialog.ShowDialog();
            if (result == true)
            {
                string fullPath = openFileDialog.FileName;


                PdfReader reader = new PdfReader(fullPath);

                var metadata = reader.Info;

                string title = metadata.ContainsKey("Title") ? metadata["Title"] : "Unknown";
                string author = metadata.ContainsKey("Author") ? metadata["Author"] : "Unknown";
                string publisher = metadata.ContainsKey("Producer") ? metadata["Producer"] : "Unknown"; 
                string creationDate = metadata.ContainsKey("CreationDate") ? metadata["CreationDate"] : "Unknown";

                string year = "Unknown";
                if (creationDate != "Unknown")
                {
                    if (creationDate.StartsWith("D:") && creationDate.Length >= 6)
                    {
                        year = creationDate.Substring(2, 4);
                    }
                }

                reader.Close();

                byte[] fileData = await File.ReadAllBytesAsync(fullPath);

                MyDocuments myDocument = new();
                myDocument.Year = year;
                myDocument.Publisher = publisher;
                myDocument.Authors = author;
                myDocument.Title = title;
                myDocument.FileData = fileData;

                // save to database
                var response = await _myDocumentsService.AddAsync(myDocument);
                
                if(response.Ok) Toaster.ShowSuccess(response.Message);
                else Toaster.ShowError(response.Message);
            }

            // refresh ui 
            Task.Run(() => FetchAllDocuments());
        }
        
        private async Task  FetchAllDocuments()
        {
            try
            {
                IsLoading = false;
                var response = await _myDocumentsService.FetchAllAsync();

                if (!response.Ok) throw response.Exception;

                List<MyDocuments> myDocumentsFromDb = response.Data;
                
                App.Current.Dispatcher.Invoke(() =>
                {
                    MyDocuments.Clear();
                    foreach (var myDocument in myDocumentsFromDb)
                    {
                        MyDocuments.Add(myDocument);
                    }
                    Toaster.ShowSuccess("Documents Fetched Successfully");
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                App.Current.Dispatcher.Invoke(() =>
                {
                    Toaster.ShowError(ex.Message ?? "Error Occured");
                });
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        private async Task DeleteDocument(Guid documentId)
        {

            try
            {
                IsLoading = false;
                var response = await _myDocumentsService.DeleteByIdAsync(documentId);

                if (!response.Ok) throw response.Exception;

                MyDocuments deletedDocument = response.Data;

                App.Current.Dispatcher.Invoke(() =>
                {
                    List<MyDocuments> shallowCopy = MyDocuments.ToList().Where(x => 
                    x.Id != deletedDocument.Id).ToList();

                    MyDocuments.Clear();
                    foreach (var myDocument in shallowCopy)
                    {
                        MyDocuments.Add(myDocument);
                    }
                    Toaster.ShowSuccess("Document Deleted");
                });


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);  
                App.Current.Dispatcher.Invoke(() =>
                {
                    Toaster.ShowError(ex.Message ?? "Error Occured");
                });
            }
            finally
            {
                IsLoading = false;
            }
        }

    }
}
