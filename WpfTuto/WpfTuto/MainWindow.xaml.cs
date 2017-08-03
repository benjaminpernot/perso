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

namespace WpfTuto
{
    public class MyImage
    {
        private ImageSource _image;
        private string _name;

        public MyImage(ImageSource image, string name)
        {
            _image = image;
            _name = name;
        }

        public override string ToString()
        {
            return _name;
        }

        public ImageSource Image
        {
            get { return _image; }
        }

        public string Name
        {
            get { return _name; }
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            FolderSelector.Text = Environment.GetFolderPath(
                   Environment.SpecialFolder.MyPictures);
            DataContext = this;
        }
        public List<MyImage> AllImages
        {
            get
            {
                List<MyImage> result = new List<MyImage>();
                foreach (string filename in
                   System.IO.Directory.GetFiles(
                   FolderSelector.Text))
                {
                    try
                    {
                        result.Add(
                         new MyImage(
                         new BitmapImage(
                         new Uri(filename)),
                         System.IO.Path.GetFileNameWithoutExtension(filename)));
                    }
                    catch { }
                }
                return result;
            }
        }
    }
}
