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
using System.Windows.Shapes;

namespace image_editor_Herasimov_225941
{
    /// <summary>
    /// Логика взаимодействия для log_kor.xaml
    /// </summary>
    public partial class log_kor : Window
    {
        public log_kor()
        {
            InitializeComponent();
        }

        private int c;

        public int C
        {
            get => c;
            set
            {
                if (value == 0)
                    MessageBox.Show("Значение должно быть отличным от нуля!");
                else
                    c = value;
            }
        }

        private void BTNRES_Click(object sender, RoutedEventArgs e)
        {
            int bf;
            bool rez = int.TryParse(input.Text, out bf);
            if (rez)
            {
                this.C = bf;
                if (this.c != 0)
                    this.Hide();
            }

        }
    }
}
