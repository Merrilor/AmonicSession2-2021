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

namespace Session2
{
    /// <summary>
    /// Interaction logic for EditFlightWindow.xaml
    /// </summary>
    public partial class EditFlightWindow : Window
    {

        FlightSchedule SelectedSchedule { get; set; }

        public EditFlightWindow(FlightSchedule selectedSchedule)
        {
            InitializeComponent();
            SelectedSchedule = selectedSchedule;
            DataContext = SelectedSchedule;
        }





        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void DigitTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            char character = (char)KeyInterop.VirtualKeyFromKey(e.Key);

            if (!char.IsDigit(character) && e.Key != Key.Back)
                e.Handled = true;

        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {

            Session2Entities entities = new Session2Entities();
            Schedules ScheduleToChange;

            try
            {
               ScheduleToChange = entities.Schedules.Where(s => s.ID == SelectedSchedule.Id).Single();
            }
            catch
            {
                MessageBox.Show("Ошибка");
                return;
            }

            ScheduleToChange.Date = (DateTime)ScheduleDatePicker.SelectedDate;

            try
            {
                ScheduleToChange.Time =TimeSpan.Parse(TimeTextBox.Text);
            }
            catch 
            {

                MessageBox.Show("Время введено в неправильном формате","Ошибка",MessageBoxButton.OK,MessageBoxImage.Error);
                return;
            }

            if(PriceTextBox.Text == string.Empty)
            {
                MessageBox.Show("Необходимо ввести цену", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }


            ScheduleToChange.EconomyPrice = decimal.Parse(PriceTextBox.Text.Replace('.',','));

            entities.SaveChanges();
            MessageBox.Show("Данные успешно изменены");

            this.Close();

            
        }
    }
}
