using System;
using System.Collections.Generic;
using System.IO;
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
using Microsoft.Win32;
using System.Drawing;
using Color = System.Drawing.Color;
using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Defaults;
using System.Windows.Interop;

namespace image_editor_Herasimov_225941
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public SeriesCollection SeriesCollection { get; set; }

        public Uri globalURL;
        public string filename;
        public byte[] All = new byte[256];
        public byte[] R = new byte[256];
        public byte[] G = new byte[256];
        public byte[] B = new byte[256];
        public struct clr
        {
            public byte red, green, blue;
        };
        public clr[,] britness;

        public MainWindow()
        {
            InitializeComponent();

            SeriesCollection = new SeriesCollection
            {
               new ColumnSeries
                {
               Fill=System.Windows.Media.Brushes.Gray,
               Values =new ChartValues<int> {}
        }
            };
            if (globalURL == null)
                edit.Visibility = Visibility.Collapsed;
        }

        private void btnUpl_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                gistAxis_1.Visibility = Visibility.Hidden;
                labelgistUp.Visibility = Visibility.Visible;
                path.Text = "";

                imagebox.Source = null;
                OpenFileDialog saveFileDialog = new OpenFileDialog();
                saveFileDialog.InitialDirectory = Directory.GetCurrentDirectory();
                saveFileDialog.Filter = "Photo(*.jpg)|*.jpg|All files(*.*)|*.*";

                if (saveFileDialog.ShowDialog() == false)
                {
                    MessageBox.Show("Изображение не загружено!");
                    return;
                }
                filename = saveFileDialog.FileName;
                FileInfo f = new FileInfo(filename);

                Uri u = new Uri(filename, UriKind.RelativeOrAbsolute);
                globalURL = u;
                imagebox.Source = new BitmapImage(u);
                calchist(filename);
                //MessageBox.Show("Фото обновлено");
                if (globalURL == null)
                    edit.Visibility = Visibility.Collapsed;
                else edit.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        void calchist(string source)
        {

            Bitmap obj = new Bitmap(source);
            labelgistUp.Visibility = Visibility.Hidden;

            int i, j;
            Color color;
            britness = new clr[obj.Width, obj.Height];
            // собираем статистику для изображения
            for (i = 0; i < obj.Height; ++i)
                for (j = 0; j < obj.Width; ++j)
                {
                    color = obj.GetPixel(j, i);

                    britness[j, i].red = color.R;
                    britness[j, i].green = color.G;
                    britness[j, i].blue = color.B;

                    byte Y = Convert.ToByte(0.3 * color.R + 0.59 * color.G + 0.11 * color.B);
                    //byte Y = Convert.ToByte(0.212 * color.R + 0.715 * color.G + 0.072 * color.B);
                    if (Y > 255)
                        Y = 255;
                    //использование хэш-таблиц
                    ++All[Y];
                    ++R[color.R];
                    ++G[color.G];
                    ++B[color.B];

                }

            if (SeriesCollection.Count != 0)
                SeriesCollection[0].Values.Clear();
            gistAxis_1.Visibility = Visibility.Visible;

            for (int k = 0; k < 255; k++)
                SeriesCollection[0].Values.Add(Convert.ToInt32(All[k]));
            DataContext = this;

        }



        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (globalURL == null)
                    edit.Visibility = Visibility.Collapsed;
                log_kor obj = new log_kor();
                obj.ShowDialog();
                int c = obj.C;
                byte[] RGB = new byte[3];
                Color color, new_color;
                Bitmap new_image = new Bitmap(filename);
                int i, j;
                for (i = 0; i < new_image.Height; ++i)
                    for (j = 0; j < new_image.Width; ++j)
                    {
                        color = new_image.GetPixel(j, i);
                        RGB[0] = (byte)(c * Math.Log(1.0 + color.R));
                        RGB[1] = (byte)(c * Math.Log(1.0 + color.G));
                        RGB[2] = (byte)(c * Math.Log(1.0 + color.B));
                        new_color = Color.FromArgb(0, RGB[0], RGB[1], RGB[2]);
                        new_image.SetPixel(j, i, new_color);
                    }
                string name = Directory.GetCurrentDirectory() + "\\Log_" + DateTime.Now.ToFileTimeUtc() + "Constant=" + c.ToString() + ".png";
                new_image.Save(name, System.Drawing.Imaging.ImageFormat.Png);
                Uri u = new Uri(name, UriKind.RelativeOrAbsolute);
                globalURL = u;
                imagebox.Source = new BitmapImage(u);
                calchist(name);
                path.Text = "Изображение сохранилось по адресу\n" + name;
                // MessageBox.Show("Фото обновлено");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {

            try
            {
                int[,] h_1 = { { 0, 1 }, { -1, 0 } };//ядра свертки
                int[,] h_2 = { { 1, 0 }, { 0, -1 } };

                if (globalURL == null)
                    edit.Visibility = Visibility.Collapsed;
                byte z1_r, z1_g, z1_b, z2_r, z2_g, z2_b, z3_r, z3_g, z3_b, z4_r, z4_g, z4_b, SxyR, SxyG, SxyB;
                byte[] RGB = new byte[3];
                Color new_color;
                Bitmap new_image = new Bitmap(filename);
                int i, j;
                for (i = 0; i < new_image.Width - 2; ++i)
                    for (j = 0; j < new_image.Height - 2; ++j)
                    {
                        z1_r = (byte)(britness[i, j].red * 1 + britness[i + 1, j].red * 0 + britness[i, j + 1].red * 0 + britness[i + 1, j + 1].red * -1);//x,y
                        z1_g = (byte)(britness[i, j].green * 1 + britness[i + 1, j].green * 0 + britness[i, j + 1].green * 0 + britness[i + 1, j + 1].green * -1);//x,y
                        z1_b = (byte)(britness[i, j].blue * 1 + britness[i + 1, j].blue * 0 + britness[i, j + 1].blue * 0 + britness[i + 1, j + 1].blue * -1);//x,y

                        z2_r = (byte)(britness[i + 1, j].red * 0 + britness[i + 2, j].red * 1 + britness[i + 1, j + 1].red * -1 + britness[i + 2, j + 1].red * 0);//x+1,y
                        z2_g = (byte)(britness[i + 1, j].green * 0 + britness[i + 2, j].green * 1 + britness[i + 1, j + 1].green * -1 + britness[i + 2, j + 1].green * 0);//x+1,y
                        z2_b = (byte)(britness[i + 1, j].blue * 0 + britness[i + 2, j].blue * 1 + britness[i + 1, j + 1].blue * -1 + britness[i + 2, j + 1].blue * 0);//x+1,y

                        z3_r = (byte)(britness[i, j + 1].red * 0 + britness[i + 1, j + 1].red * 1 + britness[i, j + 2].red * -1 + britness[i + 1, j + 2].red * 0);//x,y+1
                        z3_g = (byte)(britness[i, j + 1].green * 0 + britness[i + 1, j + 1].green * 1 + britness[i, j + 2].green * -1 + britness[i + 1, j + 2].green * 0);//x,y+1
                        z3_b = (byte)(britness[i, j + 1].blue * 0 + britness[i + 1, j + 1].blue * 1 + britness[i, j + 2].blue * -1 + britness[i + 1, j + 2].blue * 0);//x,y+1

                        z4_r = (byte)(britness[i + 1, j + 1].red * 1 + britness[i + 2, j + 1].red * 0 + britness[i + 1, j + 2].red * 0 + britness[i + 2, j + 2].red * -1);//x+1,y+1
                        z4_g = (byte)(britness[i + 1, j + 1].green * 1 + britness[i + 2, j + 1].green * 0 + britness[i + 1, j + 2].green * 0 + britness[i + 2, j + 2].green * -1);//x+1,y+1
                        z4_b = (byte)(britness[i + 1, j + 1].blue * 1 + britness[i + 2, j + 1].blue * 0 + britness[i + 1, j + 2].blue * 0 + britness[i + 2, j + 2].blue * -1);//x+1,y+1

                        SxyR = (byte)(Math.Sqrt(Math.Pow(z2_r - z3_r, 2) + Math.Pow(z1_r - z4_r, 2)));
                        SxyG = (byte)(Math.Sqrt(Math.Pow(z2_g - z3_g, 2) + Math.Pow(z1_g - z4_g, 2)));
                        SxyB = (byte)(Math.Sqrt(Math.Pow(z2_b - z3_b, 2) + Math.Pow(z1_b - z4_b, 2)));

                        new_color = Color.FromArgb(0, SxyR, SxyG, SxyB);
                        new_image.SetPixel(i, j, new_color);
                    }
                string name = Directory.GetCurrentDirectory() + "\\Roberst_" + DateTime.Now.ToFileTimeUtc() + ".png";
                path.Text = "Изображение сохранилось по адресу\n" + name;
                new_image.Save(name, System.Drawing.Imaging.ImageFormat.Png);
                Uri u = new Uri(name, UriKind.RelativeOrAbsolute);
                globalURL = u;
                imagebox.Source = new BitmapImage(u);
                calchist(name);
                // MessageBox.Show("Фото обновлено");

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("магистрант группы 225941, Герасимов В.А.");
        }

        private void gist_Click(object sender, RoutedEventArgs e)
        {
            string gist_type = (string)((MenuItem)e.OriginalSource).Name;
            reinitgist(gist_type);
        }
        private void reinitgist(string color)
        {
            if (SeriesCollection.Count != 0)
                SeriesCollection.Clear();
            gistAxis_1.Visibility = Visibility.Visible;

            Byte[] br = new Byte[256];
            switch (color)
            {
                case "redgist":
                    {
                        br = R;
                        SeriesCollection.Add(new ColumnSeries
                        {
                            Fill = System.Windows.Media.Brushes.Red,
                            Values = new ChartValues<int> { }
                        });

                        break;
                    }
                case "bluegist":
                    {

                        SeriesCollection.Add(new ColumnSeries
                        {
                            Fill = System.Windows.Media.Brushes.Blue,
                            Values = new ChartValues<int> { }
                        });

                        br = B;
                        break;
                    }
                case "greengist":
                    {

                        SeriesCollection.Add(new ColumnSeries
                        {
                            Fill = System.Windows.Media.Brushes.Green,
                            Values = new ChartValues<int> { }
                        });
                        br = G;
                        break;
                    }
                case "gist":
                    {

                        SeriesCollection.Add(new ColumnSeries
                        {
                            Fill = System.Windows.Media.Brushes.Gray,
                            Values = new ChartValues<int> { }
                        });
                        br = All;
                        break;
                    }
            }

            for (int k = 0; k < 255; k++)
                SeriesCollection[0].Values.Add(Convert.ToInt32(br[k]));
            DataContext = this;
        }


    }
}

