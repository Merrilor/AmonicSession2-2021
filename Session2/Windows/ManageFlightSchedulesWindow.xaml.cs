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

namespace Session2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ManageFlightSchedulesWindow : Window
    {

        List<FlightSchedule> ScheduleList { get; set; }

        List<FlightSchedule> FilteredScheduleList { get; set; }

        public string CurrentDateSearch = "";

        public string CurrentFlightNumberSearch = "";


        public ManageFlightSchedulesWindow()
        {
            InitializeComponent();


            SelectSchedule();
            LoadItems();
            ApplyFilter();
        }


        private void SelectSchedule()
        {
            Session2Entities entities = new Session2Entities();

            var test = entities.Schedules.Select(s => new FlightSchedule
            {
                Id = s.ID,
                Date = s.Date,
                Time = s.Time,
                From = s.Routes.Airports.IATACode,
                To = s.Routes.Airports1.IATACode,
                FlightNumber = s.FlightNumber,
                Aircraft = s.Aircrafts.MakeModel,
                EconomyPrice = s.EconomyPrice,
                BusinessPrice = s.EconomyPrice + (s.EconomyPrice /100 ) * 35,
                FirstClassPrice = (s.EconomyPrice + (s.EconomyPrice / 100) * 35) + ((s.EconomyPrice + (s.EconomyPrice / 100) * 35) / 100) * 30,
                Confirmed = s.Confirmed


            });

            ScheduleList = test.ToList();

            FilteredScheduleList = ScheduleList;

            FlightScheduleDatagrid.ItemsSource = FilteredScheduleList;


        }


        private void LoadItems()
        {
            Session2Entities entities = new Session2Entities();

            List<string> AirportList = new List<string>();
            AirportList.Add("Any");
            AirportList.AddRange( entities.Airports.Select(a => a.IATACode));
            FromAirportComboBox.ItemsSource = AirportList;
            ToAirportComboBox.ItemsSource = AirportList;

            

        }

        private void ApplyFilter()
        {
            FilteredScheduleList = ScheduleList;
            //TODO on any combo box/ text box change
            if(FromAirportComboBox.Text != "Any")
            {
                FilteredScheduleList = FilteredScheduleList.Where(s => s.From == FromAirportComboBox.Text).ToList();
            }

            if(ToAirportComboBox.Text != "Any")
            {
                FilteredScheduleList = FilteredScheduleList.Where(s => s.To == ToAirportComboBox.Text).ToList();
            }

            if(OutboundTextBox.Text != string.Empty)
            {
                FilteredScheduleList = FilteredScheduleList.Where(s => s.Date.Date == DateTime.Parse(OutboundTextBox.Text)).ToList();
            }

            if(FlightNumberTextBox.Text != string.Empty)
            {
                FilteredScheduleList = FilteredScheduleList.Where(s => s.FlightNumber == FlightNumberTextBox.Text).ToList();
            }


          
            switch (SortComboBox.Text)
            {
                case "Date-time":
                    FilteredScheduleList = FilteredScheduleList.OrderByDescending(s => s.Date).ThenByDescending(s => s.Time).ToList();
                    break;
                case "Economy Price":
                    FilteredScheduleList = FilteredScheduleList.OrderByDescending(s=>s.EconomyPrice).ToList();
                    break;
                case "Confirmed":
                    FilteredScheduleList = FilteredScheduleList.OrderByDescending(s => s.Confirmed).ToList();
                    break;
                   
            }

            FlightScheduleDatagrid.ItemsSource = FilteredScheduleList;


        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilter();
        }

        private void FlightScheduleDatagrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FlightScheduleDatagrid.SelectedItem is null)
                return;

            UpdateConfirmCancelButton();

           
        }

        private void UpdateConfirmCancelButton()
        {
            FlightSchedule SelectedSchedule = (FlightSchedule)FlightScheduleDatagrid.SelectedItem;

            ConfirmCancelButton.Background = SelectedSchedule.Confirmed == true ? Brushes.Red : Brushes.ForestGreen;
            ConfirmCancelButton.Content = SelectedSchedule.Confirmed == true ? "Cancel Flight" : "Confirm Flight";
        }

        private void ConfirmCancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (FlightScheduleDatagrid.SelectedItem is null)
                return;

            FlightSchedule SelectedSchedule = (FlightSchedule)FlightScheduleDatagrid.SelectedItem;

            // Update schedule list
            foreach (var item in FilteredScheduleList)
            {
                if (item.Id == SelectedSchedule.Id)
                    item.Confirmed = item.Confirmed == true ? false : true;
            }

            //Update DB
            Session2Entities entities = new Session2Entities();
            entities.Schedules
                .Where(s => SelectedSchedule.Id == s.ID)
                .Single()
                .Confirmed = !entities.Schedules.Where(s => SelectedSchedule.Id == s.ID).Single().Confirmed;
            entities.SaveChanges();

            FlightScheduleDatagrid.Items.Refresh();
            UpdateConfirmCancelButton();


        }

        private void EditFlightButton_Click(object sender, RoutedEventArgs e)
        {
            if (FlightScheduleDatagrid.SelectedItem is null)
                return;

            EditFlightWindow window = new EditFlightWindow((FlightSchedule)FlightScheduleDatagrid.SelectedItem);
            window.ShowDialog();
            SelectSchedule();

        }

        private void ImportChangesButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new ApplyScheduleChangesWindow();
            window.ShowDialog();


        }
    }

    public class FlightSchedule
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public TimeSpan Time { get; set; }

        public string From { get; set; }


        public string To { get; set; }

        public string FlightNumber { get; set; }

        public string Aircraft { get; set; }

        public decimal EconomyPrice { get; set; }

        public decimal BusinessPrice { get; set; }

        public decimal FirstClassPrice { get; set; }

        public bool Confirmed { get; set; }


    }
  

}
