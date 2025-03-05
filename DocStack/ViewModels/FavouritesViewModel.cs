using DocStack.Commands;
using DocStack.Utils;
using Microsoft.Extensions.DependencyInjection;
using Models.Entity;
using Models.Enums;
using Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;


namespace DocStack.ViewModels
{
    public class FavouritesViewModel : BaseViewModel
    {
        private bool _isLoading;

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

        private bool _redFilter;
        public bool RedFilter
        {
            get => _redFilter;
            set
            {
                if(_redFilter != value)
                {
                    _redFilter = value;
                    OnPropertyChanged(nameof(RedFilter));
                    ApplyColorFilter();
                }
            }
        }
        private bool _blueFilter;
        public bool BlueFilter
        {
            get => _blueFilter;
            set
            {
                if (_blueFilter != value)
                {
                    _blueFilter = value;
                    OnPropertyChanged(nameof(BlueFilter));
                    ApplyColorFilter();
                }
            }
        }

        private bool _grayFilter;
        public bool GrayFilter
        {
            get => _grayFilter;
            set
            {
                if (_grayFilter != value)
                {
                    _grayFilter = value;
                    OnPropertyChanged(nameof(GrayFilter));
                    ApplyColorFilter();
                }
            }
        }

        private bool _greenFilter;
        public bool GreenFilter
        {
            get => _greenFilter;
            set
            {
                if (_greenFilter != value)
                {
                    _greenFilter = value;
                    OnPropertyChanged(nameof(GreenFilter));
                    ApplyColorFilter();
                }
            }
        }
        
        private bool _yellowFilter;
        public bool YellowFilter
        {
            get => _yellowFilter;
            set
            {
                if (_yellowFilter != value)
                {
                    _yellowFilter = value;
                    OnPropertyChanged(nameof(YellowFilter));
                    ApplyColorFilter();
                }
            }
        }

        private bool _purpleFilter;
        public bool PurpleFilter
        {
            get => _purpleFilter;
            set
            {
                if (_purpleFilter != value)
                {
                    _purpleFilter = value;
                    OnPropertyChanged(nameof(PurpleFilter));
                    ApplyColorFilter();
                }
            }
        }

        private StarredService _starredService; 
        
        public ObservableCollection<StarredEntity> StarredEntities { get; set; }

        public List<StarredEntity> StarredEntitesFromDb { get; set; }

        public RelayCommand<Guid> DeleteCommand { get; set; }

        public RelayCommand<string> SearchCommand { get; set; }

        public RelayCommand<string> ResetFilterCommand { get; set; }

        public RelayCommand<object[]> ChangeColorFilterCommand { get; set; }
        
        public RelayCommand<string> ExportFavouritesAsPdfDCommand { get; set; }
        public FavouritesViewModel()
        {
            _starredService = App.ServiceProvider.GetRequiredService<StarredService>();
            
            StarredEntities = new();
            StarredEntitesFromDb = new();

            DeleteCommand = new RelayCommand<Guid>(async (starredId) => await DeleteStarred(starredId));
            SearchCommand = new RelayCommand<string>((_) => Search());
            ResetFilterCommand = new RelayCommand<string>((_) => ResetFilters());
            ChangeColorFilterCommand = new RelayCommand<object[]>(async (args) => await ChangeColorFilter(args));
            ExportFavouritesAsPdfDCommand = new RelayCommand<string>(async (_) => await ExportFavouritesAsPdf());

            GreenFilter = true;
            GrayFilter = true;
            RedFilter = true;
            PurpleFilter = true;
            YellowFilter = true;
            BlueFilter = true;

            QuestPDF.Settings.License = LicenseType.Community;

            Task.Run(() => Fetch());
        }
        
        public async Task Fetch()
        {
            try
            {
                StarredEntitesFromDb.Clear();

                IsLoading = true;
                
                var response = await _starredService.GetAll();
                
                if(response.Exception != null) throw response.Exception;
               
                Application.Current.Dispatcher.Invoke(() =>
                {
                    response.Data.ForEach(e => StarredEntitesFromDb.Add(e));
                    ApplyFilters();
                    Toaster.ShowSuccess("fetched favourites from database");
                });

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Toaster.ShowError("Error Occured");
                });

            }
            finally
            {
                IsLoading = false;
            }

        }
    

        private void ApplyFilters()
        {
            this.StarredEntities.Clear();
            StarredEntitesFromDb.ForEach(x => StarredEntities.Add(x));
        }

        private async Task DeleteStarred(Guid starredId)
        {
            try
            {
                IsLoading = true;
                var respnse = await _starredService.RemoveStarred(starredId);
                if (!respnse.Ok) throw respnse.Exception;
                Toaster.ShowSuccess(respnse.Message);

                StarredEntitesFromDb.Remove(StarredEntitesFromDb.FirstOrDefault(x => x.Id == starredId));

                // refresh ui
                StarredEntities.Remove(StarredEntities.FirstOrDefault(x => x.Id == starredId));

            }catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                Toaster.ShowError(ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void Search()
        {
            var filteredList = StarredEntities.Where(x =>
                ContainsIgnoreCase(x.PaperEntity.Title ?? "", Query) ||
                ContainsIgnoreCase(x.PaperEntity.Authors ?? "", Query) ||
                ContainsIgnoreCase(x.PaperEntity.Publisher ?? "", Query) ||
                ContainsIgnoreCase(x.PaperEntity.Year ?? "", Query))
                .ToList();

            StarredEntities.Clear();
            foreach (var item in filteredList)
            {
                StarredEntities.Add(item);
            }

        }

        public void ResetFilters()
        {
            Query = String.Empty;
         
            this.StarredEntities.Clear() ;
            foreach (StarredEntity e in StarredEntitesFromDb)
            {
                StarredEntities.Add(e);
            }
            GreenFilter = true;
            GrayFilter = true;
            RedFilter = true;
            PurpleFilter = true;
            YellowFilter = true;
            BlueFilter = true;
        }

        private bool ContainsIgnoreCase(string target, string value) => target.ToLower().Contains(value?.ToLower() ?? "");

        public void HitSearchKey() => Search();
        
        private async Task ChangeColorFilter(object[] args)
        {
            if(args.Length != 2)
            {
                Toaster.ShowError("invalid arguments");
                return;
            }
            string selectedColor = (string)args[1];
            ColorFilters colorFilter = ColorFilters.Gray;
            switch (selectedColor)
            {
                case "Gray":
                    colorFilter = ColorFilters.Gray;
                    break;
                case "Red":
                    colorFilter = ColorFilters.Red;
                    break;
                case "Purple":
                    colorFilter = ColorFilters.Purple;
                    break;
                case "Blue":
                    colorFilter = ColorFilters.Blue;
                    break;
                case "Yellow":
                    colorFilter = ColorFilters.Yellow;
                    break;
                case "Green":
                    colorFilter = ColorFilters.Green;
                    break;
                default:
                    colorFilter = ColorFilters.Gray;
                    break;
            }

            Guid starredId = (Guid)args[0];
            StarredEntity? starred = StarredEntitesFromDb.FirstOrDefault(x => x.Id == starredId);

            if(starred == null)
            {
                Toaster.ShowError($"Entity with id {starredId} not exist");
                return;
            }
            starred.ColorFilter = colorFilter;
            var response = await _starredService.Update(starred);

            if (!response.Ok) Toaster.ShowError(response.Message);
            else {
                Toaster.ShowSuccess(response.Message);
                // refresh ui
                StarredEntities.Clear();
                foreach (StarredEntity starredEntity in StarredEntitesFromDb)
                {
                    StarredEntities.Add(starredEntity);
                }
            }
        }
        
        private void ApplyColorFilter()
        {
            var query = StarredEntitesFromDb.AsQueryable();

            if (!GrayFilter) query = query.Where(x => x.ColorFilter != ColorFilters.Gray);
            if(!RedFilter) query = query.Where(x => x.ColorFilter != ColorFilters.Red);
            if (!BlueFilter) query = query.Where(x => x.ColorFilter != ColorFilters.Blue);
            if (!YellowFilter) query = query.Where(x => x.ColorFilter != ColorFilters.Yellow);
            if (!GreenFilter) query = query.Where(x => x.ColorFilter != ColorFilters.Green);
            if (!PurpleFilter) query = query.Where(x => x.ColorFilter != ColorFilters.Purple);

            var filteredList = query.ToList();

            StarredEntities.Clear();
            // refresh ui 
            foreach (StarredEntity e in filteredList)
            {
                StarredEntities.Add(e);
            }
        }
        
        private async Task ExportFavouritesAsPdf()
        {
            try
            {
                IsLoading = true;

                string DocumentsPath = KnownFolders.GetPath(KnownFolder.Downloads) ??
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string fileName = $"{Guid.CreateVersion7()}.pdf";
                string fullFilePath = Path.Join(DocumentsPath, fileName);

                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(12).FontFamily(Fonts.Calibri));

                        // Header
                        page.Header()
                            .AlignCenter()
                            .Text("Research Papers Summary")
                            .SemiBold().FontSize(24).FontColor(Colors.Blue.Medium);

                        page.Content()
                            .PaddingVertical(1, Unit.Centimetre)
                            .Column(column =>
                            {
                                column.Spacing(15);

                                foreach (var paper in this.StarredEntitesFromDb)
                                {
                                    column.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(innerColumn =>
                                    {
                                        innerColumn.Spacing(5);

                                        innerColumn.Item().Text($"📄 **Title:** {paper.PaperEntity.Title ?? "-- No Title Provided --"}")
                                            .Bold().FontSize(14).FontColor(Colors.Black);

                                        innerColumn.Item().Text($"✍ **Authors:** {paper.PaperEntity.Authors ?? "Unknown"}")
                                            .Italic().FontSize(12).FontColor(Colors.Grey.Darken3);

                                        innerColumn.Item().Text($"🏛 **Publisher:** {paper.PaperEntity.Publisher ?? "Not Provided"}");

                                        innerColumn.Item().Text($"📅 **Year:** {paper.PaperEntity.Year ?? "N/A"}")
                                            .FontColor(Colors.Blue.Darken1);

                                        if (!string.IsNullOrEmpty(paper.PaperEntity.Abstract))
                                        {
                                            innerColumn.Item().Text($"📝 **Abstract:** {paper.PaperEntity.Abstract}")
                                                .FontSize(11).FontColor(Colors.Grey.Darken2);
                                        }

                                        if (!string.IsNullOrEmpty(paper.PaperEntity.DOI))
                                        {
                                            innerColumn.Item().Text($"🔗 **DOI:** {paper.PaperEntity.DOI}")
                                                .Underline().FontColor(Colors.Blue.Medium);
                                        }

                                        if (!string.IsNullOrEmpty(paper.PaperEntity.FullTextLink))
                                        {
                                            innerColumn.Item().Text($"🔗 **Full Text:** {paper.PaperEntity.FullTextLink}")
                                                .Underline().FontColor(Colors.Green.Darken2);
                                        }
                                    });
                                }
                            });

                        // Footer with page numbers
                        page.Footer()
                            .AlignCenter()
                            .Text(x =>
                            {
                                x.Span("Page ");
                                x.CurrentPageNumber();
                            });
                    });
                })
                .GeneratePdf(fullFilePath);


                Toaster.ShowSuccess($"pdf generated successfully \n {fullFilePath}");

                SetTimeOut(() =>
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                    {
                        FileName = fullFilePath,
                        UseShellExecute = true,
                        Verb = "open"
                    });
                }, 2000);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Toaster.ShowError("Error Occured");
            }finally
            {
                IsLoading = false;
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
    
        public void SetTimeOut(Action fun, double delay)
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                Thread.Sleep((int)delay); 
                fun?.Invoke();
            });
        }
    }
    public static class ObservableCollectionExtensions
    {
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> source)
        {
            return new ObservableCollection<T>(source);
        }
    }

}
