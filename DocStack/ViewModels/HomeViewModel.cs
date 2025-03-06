using DocStack.Commands;
using DocStack.Utils;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Models.ApiResponse;
using Models.Entity;
using Service;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using Syncfusion.Windows.Shared;

namespace DocStack.ViewModels
{
    public class HomeViewModel : BaseViewModel
    {
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

        private readonly Random random = new Random();

        private Dictionary<string, List<string>> _categoryKeywords = new()
        {
            ["AI & ML"] = new ()
            {
                "machine learning", "artificial intelligence", "deep learning", "neural networks",
                "natural language processing", "computer vision", "reinforcement learning",
                "transfer learning", "generative ai", "federated learning", "transformer models",
                "autonomous systems", "machine learning applications", "AI ethics"
            },
            ["Data Science"] = new ()
            {
                "data science", "big data", "data mining", "data visualization",
                "statistical analysis", "predictive modeling", "data engineering",
                "time series analysis", "anomaly detection", "data preprocessing",
                "feature engineering", "dimensional reduction", "clustering algorithms"
            },
            ["Computer Science"] = new()
            {
                "algorithms", "distributed systems", "cloud computing", "cybersecurity",
                "software engineering", "parallel computing", "computer architecture",
                "microservices", "devops", "system design", "performance optimization",
                "containerization", "edge computing", "serverless architecture"
            },
            ["Physics"] = new()
            {
                "quantum computing", "quantum mechanics", "particle physics",
                "theoretical physics", "astrophysics", "condensed matter", "plasma physics",
                "quantum entanglement", "string theory", "dark matter", "cosmology",
                "quantum field theory", "quantum cryptography"
            },
            ["Mathematics"] = new()
            {
                "topology", "complex analysis", "number theory", "abstract algebra",
                "differential geometry", "mathematical optimization", "graph theory",
                "category theory", "algebraic geometry", "functional analysis",
                "discrete mathematics", "numerical methods", "chaos theory"
            },
            ["Interdisciplinary"] = new()
            {
                "bioinformatics", "computational biology", "quantum chemistry",
                "cognitive science", "robotics", "blockchain technology", "internet of things",
                "computational neuroscience", "systems biology", "synthetic biology",
                "biomedical engineering", "computational physics", "mathematical biology"
            },
            ["Electrical Engineering"] = new()
            {
                "circuit design", "power electronics", "control systems", "signal processing",
                "wireless communication", "semiconductor devices", "electromagnetic fields",
                "embedded systems", "renewable energy", "internet of things", "robotics",
                "antenna design", "optoelectronics", "nanotechnology"
            },
            ["Biomedical Sciences"] = new()
            {
                "genomics", "proteomics", "biostatistics", "biomedical imaging",
                "neurobiology", "stem cell research", "pharmacology", "medical devices",
                "epigenetics", "gene therapy", "bioinformatics", "synthetic biology",
                "cancer research", "clinical trials"
            },
            ["Environmental Science"] = new()
            {
                "climate change", "sustainable development", "renewable energy",
                "water pollution", "air quality", "conservation biology", "ecosystem modeling",
                "environmental policy", "waste management", "carbon footprint analysis",
                "biodiversity", "green chemistry", "oceanography", "geoscience"
            },
            ["Social Sciences"] = new()
            {
                "psychology", "sociology", "political science", "anthropology",
                "economics", "criminology", "cultural studies", "gender studies",
                "international relations", "media studies", "public policy",
                "social networks", "urban planning", "behavioral economics"
            }
        };

        private int _limit = 5;

        private Paper _selectedPaper;
        public Paper SelectedPaper
        {
            get => _selectedPaper;
            set
            {
                _selectedPaper = value;
                OnPropertyChanged();
            }
        }


        private readonly NetworkService _networkService;
        private readonly StarredService _starredService;
        private readonly MyDocumentsService _myDocumentsService;
        private readonly PaperService _paperService;

        public ObservableCollection<Paper> ReccomendedePapers { get; set; } = new(); 

        public RelayCommand<string> OpenInBrowserCommand { get; set; }


        public ObservableCollection<Axis> YAxis { get; set; } = new ObservableCollection<Axis>
        {
            new Axis { MinLimit = 0 }
        };

        public ObservableCollection<Axis> XAxis { get; set; } = new ObservableCollection<Axis>();

        public ObservableCollection<ISeries> BarSeries { get; set; } = new ObservableCollection<ISeries>();


        public ObservableCollection<MyDocuments> RecentAddedDocuments { get; set; } = new ObservableCollection<MyDocuments>();

        private DateTime _currentDate;

        private int _calendarRowCount;
        public int CalendarRowCount
        {
            get => _calendarRowCount;
            set
            {
                if(_calendarRowCount != value)
                {
                    _calendarRowCount = value;
                    OnPropertyChanged(nameof(CalendarRowCount));
                }
            }
        }
        private int _calendarColCount;
        public int CalendarColCount
        {
            get => _calendarColCount;
            set
            {
                if (_calendarColCount != value)
                {
                    _calendarColCount = value;
                    OnPropertyChanged(nameof(CalendarColCount));
                }
            }
        }

        private int _firstCol;
        public int FirstCol
        {
            get => _firstCol;
            set
            {
                if (FirstCol != value)
                {
                    _firstCol = value;
                    OnPropertyChanged(nameof(FirstCol));
                }
            }
        }

        private string _dateLabel;

        public string DateLabel
        {
            get => _dateLabel;
            set
            {
                if (_dateLabel != value)
                {
                    _dateLabel = value;
                    OnPropertyChanged(nameof(DateLabel));
                }
            }
        }

        public ObservableCollection<DayModel> Day
        {
            get;
            set;
        } = new();

        public ObservableCollection<RowDefinition> CalendarRows { get; set; } = new ObservableCollection<RowDefinition>();
        public ObservableCollection<ColumnDefinition> CalendarColumns { get; set; } = new ObservableCollection<ColumnDefinition>();

        public HomeViewModel()
        {
            _networkService = App.ServiceProvider.GetRequiredService<NetworkService>();
            _starredService = App.ServiceProvider.GetRequiredService<StarredService>();
            _myDocumentsService = App.ServiceProvider.GetRequiredService<MyDocumentsService>();
            _paperService = App.ServiceProvider.GetRequiredService<PaperService>();

            OpenInBrowserCommand = new RelayCommand<string>((_) => OpenInBrowser());

            Task.Run(() => FetchDocuments());

            Task.Run(() => TaskInitilizeMainContentAsync());
        }

        private string GetRandomCategory => _categoryKeywords.Values.OrderBy(x => random.Next(999)).First().OrderBy(x => random.Next(999)).First();
        private async Task FetchDocuments()
        {
            // fetch 30 documents
            try
            {
                IsLoading = true;
                int offset = 0;
                for (int i = 0; i < 5; i++)
                {
                    try
                    {
                        var response = await  _networkService.GetWorks(GetRandomCategory, offset, _limit);
                        if (!response.Ok) throw response.Exception;
                        if (response.Data == null) throw new Exception("no papers returned");
                        
                        List<Paper> data = response.Data.results;

                        App.Current.Dispatcher.Invoke(() =>
                        {
                            foreach (Paper p in data)
                            {
                                ReccomendedePapers.Add(p);
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

                    offset += _limit;
                }
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        public async Task OpenInBrowser()
        {
            try
            {
                IsLoading = true;
                Paper paper = SelectedPaper;
                bool flag = await _networkService.CheckDOIExists(paper.doi);

                if (!flag) throw new Exception("documetn object identifier is invalid");

                Process myProcess = new Process();
                myProcess.StartInfo.UseShellExecute = true;
                myProcess.StartInfo.FileName = paper.downloadUrl;
                myProcess.Start();

            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                Toaster.ShowError(ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        private async Task TaskInitilizeMainContentAsync()
        {
            try
            {
                IsLoading = true;

                var createChart = CreateChartAsync();
                var createRecentAddedDocumentsList = CreateRecentAddedList();
                
                InitilizeCalendar();

                await Task.WhenAll(createChart, createRecentAddedDocumentsList);

            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Toaster.ShowError(ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }
        private async Task CreateChartAsync()
        {
            try
            {
                var response = await this._starredService.GetAll();
                if (!response.Ok) throw response.Exception;

                List<StarredEntity> starredEntities = response.Data;

                List<string> labels = new();
                List<int> citiationCounts = new();
                int counter = 0;

                foreach(StarredEntity entity in starredEntities)
                {
                    if (counter >= 5) break;

                    if(entity?.PaperEntity?.DOI != null)
                    {
                        int citiationCount = await _networkService.GetCitationCountAsync(entity.PaperEntity.DOI);

                        labels.Add(entity?.PaperEntity?.Title != null ?
                            entity.PaperEntity.
                            Title!.
                            Trim()
                            .Substring(0, 10)
                            .ToString() :  "undefined");
                        
                        citiationCounts.Add(citiationCount);

                        counter++;
                    }
                
                }

                XAxis.Add(new Axis()
                {
                    Labels = labels
                });

                BarSeries.Add(new ColumnSeries<int>
                {
                    Values = citiationCounts,
                    Stroke = new SolidColorPaint(SKColors.Black) { StrokeThickness = 2 },
                    Fill = new SolidColorPaint(SKColors.CornflowerBlue)
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    
        private async Task CreateRecentAddedList()
        {
            try
            {
                IsLoading = true;
                var response = await _myDocumentsService.FetchAllAsync();

                if (!response.Ok) throw response.Exception;

                foreach (MyDocuments doc in response.Data)
                {
                    RecentAddedDocuments.Add(doc);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void InitilizeCalendar()
        {
            _currentDate = DateTime.Now;

            int daysInCurrentMont = DateTime.DaysInMonth(_currentDate.Year, _currentDate.Month);

            CalendarRowCount = daysInCurrentMont / 7;
            CalendarColCount = 7;


            DateTime firstDayOfMonth = new DateTime(_currentDate.Year, _currentDate.Month, 1);
            FirstCol = (int)firstDayOfMonth.DayOfWeek - 1;

            for (int i = 1; i <= daysInCurrentMont; i++)
            {
                Day.Add(new DayModel
                {
                    Year = _currentDate.Year,
                    Month = _currentDate.Month,
                    Day = i
                });
            }
        }



    }
    public class DayModel
    {
        public int Year { get; set; }

        public int Month { get; set; }
        public int Day { get; set; }

        public bool IsCurrentDate =>
          Year == DateTime.Today.Year &&
          Month == DateTime.Today.Month &&
          Day == DateTime.Today.Day;
    }
}
