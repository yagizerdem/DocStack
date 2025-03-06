using DocStack.Commands;
using DocStack.Utils;
using DocStack.Views;
using iTextSharp.text.pdf;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using Models.Entity;
using Service;
using Syncfusion.Windows.PdfViewer;
using Syncfusion.Windows.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static System.Net.Mime.MediaTypeNames;

namespace DocStack.ViewModels
{
    public class MyPapersViewModel : BaseViewModel
    {
        private readonly MyDocumentsService _myDocumentsService;
        public RelayCommand<string> ImportDocumentsCommand { get; set; }

        public RelayCommand<Guid> DeleteDocumentCommand { get; set; }

        public RelayCommand<string> ExportDocumentsAsPdfCommand { get; set; }

        public RelayCommand<Guid> OpenInPdfEditorCommand { get; set; }

        public RelayCommand<string> GoBackCommand { get; set; }

        public RelayCommand<string> SaveEditedFileCommand { get; set; }

        public RelayCommand<string> OverwriteEditedFileCommand { get; set; }

        private MyDocuments _selectedDocument;
        
        public MyDocuments SelectedDocument
        {
            get => _selectedDocument;
            set
            {
                _selectedDocument = value;
                OnPropertyChanged(nameof(SelectedDocument));
            }
        }

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

        private int _limit = 1;

        private bool _editorMode;

        public bool EditorMode
        {
            get => _editorMode;
            set
            {
                if(_editorMode != value)
                {
                    _editorMode = value;
                    OnPropertyChanged(nameof(EditorMode));
                    OnPropertyChanged(nameof(NotEditorMode));
                }
            }
        }

        public bool NotEditorMode => !EditorMode;
        public ObservableCollection<MyDocuments> MyDocuments { get;set; }

        public MyPapersView MyPapersViewInstance { get; set; }
        public MyPapersViewModel()
        {
            ImportDocumentsCommand = new RelayCommand<string>(async (_) => await ImportDocuments());
            DeleteDocumentCommand = new RelayCommand<Guid>(async (documentId) => await DeleteDocument(documentId));
            ExportDocumentsAsPdfCommand = new RelayCommand<string>(async (_) => await ExportDocumentsAsPdf());
            OpenInPdfEditorCommand = new RelayCommand<Guid>(async (documentId) => await OpenInPdfEditor(documentId));
            GoBackCommand = new RelayCommand<string>((_) => GoBack());
            SaveEditedFileCommand = new RelayCommand<string>((_) => Task.Run(()=> SaveEditedFile()));
            OverwriteEditedFileCommand = new RelayCommand<string>((_) => Task.Run(() => OverwriteEditedFile()));

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

        private async Task ExportDocumentsAsPdf()
        {
            try
            {
                IsLoading = true;
                int page = 0;
                string zipFilePath = Path.Combine(Path.GetTempPath(), $"ExportedDocuments_{DateTime.Now:yyyyMMddHHmmss}.zip");

                using (FileStream zipToCreate = new FileStream(zipFilePath, FileMode.Create))
                using (ZipArchive archive = new ZipArchive(zipToCreate, ZipArchiveMode.Create, true))
                {
                    while (true)
                    {
                        int offset = page * _limit;
                        var response = await _myDocumentsService.GetWithPaginationAsync(offset, _limit);

                        if (!response.Ok) throw response.Exception;

                        if (response.Data.Count == 0) break; 

                        foreach (MyDocuments doc in response.Data)
                        {
                            if (doc.FileData != null)
                            {
                                string sanitizedFileName = $"{doc.Title}.pdf".Replace("/", "_").Replace("\\", "_"); 
                                ZipArchiveEntry entry = archive.CreateEntry(sanitizedFileName, CompressionLevel.Optimal);

                                using (var entryStream = entry.Open())
                                {
                                    await entryStream.WriteAsync(doc.FileData, 0, doc.FileData.Length);
                                }
                            }
                        }

                        if (response.Data.Count < _limit)
                            break; 

                        page++; 
                    }
                }

                Toaster.ShowSuccess("Documents exported successfully!");

                SetTimeOut(() =>
                {
                    Process.Start("explorer.exe", $"/select,\"{zipFilePath}\"");
                },2000);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Toaster.ShowError(ex.Message);
            }
            finally
            {
                IsLoading = false;
            }

        }

        public void SetTimeOut(Action fun, double delay)
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                Thread.Sleep((int)delay);
                fun?.Invoke();
            });
        }
        
        public async Task OpenInPdfEditor(Guid documentId)
        {
            try
            {

                SelectedDocument = MyDocuments.FirstOrDefault(x => x.Id == documentId);

                EditorMode = true;

                var response = await _myDocumentsService.GetFileAsync(documentId);
                if (!response.Ok) throw response.Exception;
                var doc = response.Data;
                if (doc == null) throw new Exception($"document with Id {documentId} not exist");
                if (doc.FileData == null || doc.FileData.Length == 0) return;

                var editor = MyPapersViewInstance.pdfViewerControl;

                var stream = new MemoryStream(doc.FileData);

                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    MyPapersViewInstance.pdfViewer.Loaded += (sender, e) =>
                    {
                        // Dispose the stream after the PDF is loaded
                        stream.Dispose();
                    };
                    MyPapersViewInstance.pdfViewer.Load(stream);
                }));

            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Toaster.ShowError(ex.Message);
            }
            finally
            {

            }

        }

        public void GoBack() => EditorMode = false;

        private async Task SaveEditedFile()
        {
            try
            {
                IsLoading = true;
                string filePath = KnownFolders.GetPath(KnownFolder.Downloads) ?? Environment.GetFolderPath(
                                    Environment.SpecialFolder.ApplicationData);

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss"); // Format: 20240306_153045
                string fullPath = Path.Join(filePath, $"PDF_{timestamp}.pdf");

                MyPapersViewInstance.pdfViewer.Save(fullPath);

                App.Current.Dispatcher.Invoke(() =>
                {
                    Toaster.ShowSuccess("Document Saved");
                });

                SetTimeOut(() =>
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                    {
                        FileName = fullPath,
                        UseShellExecute = true,
                        Verb = "open"
                    });
                },2000);
                
            }
            finally
            {
                IsLoading = false;  
            }
        }

        private async Task OverwriteEditedFile()
        {

            try
            {
                IsLoading = true;
                string filePath = Path.Combine(Path.GetTempPath(), "MyAppTemp");
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss"); // Format: 20240306_153045
                string fullPath = Path.Join(filePath, $"PDF_{timestamp}.pdf");
                
                MyPapersViewInstance.pdfViewer.Save(fullPath);

                byte[] blob = File.ReadAllBytes(fullPath);
                Guid documentId = SelectedDocument.Id;
                await _myDocumentsService.ChangeFileAsync(documentId, blob);

                App.Current.Dispatcher.Invoke(() =>
                {
                    Toaster.ShowSuccess("Document Overwrite ...");
                });
            }
            finally
            {
                IsLoading = false;
            }


        }

    }

}


