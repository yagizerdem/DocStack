using DocStack.ViewModels;
using Syncfusion.Windows.PdfViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DocStack.Views
{
    /// <summary>
    /// Interaction logic for MyPapersView.xaml
    /// </summary>
    public partial class MyPapersView : UserControl
    {

        public PdfViewerControl pdfViewer;
        public MyPapersView()
        {
            InitializeComponent();
            pdfViewer = this.pdfViewerControl;
            this.Loaded += MyPapersView_Loaded; // Subscribe to the event
   
        }
        private void MyPapersView_Loaded(object sender, RoutedEventArgs e)
        {
            ((MyPapersViewModel)this.DataContext).MyPapersViewInstance = this;
        }
    }
}
