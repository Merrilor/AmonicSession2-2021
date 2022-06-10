using Microsoft.Win32;
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
using System.Windows.Shapes;

namespace Session2
{
    /// <summary>
    /// Interaction logic for ApplyScheduleChangesWindow.xaml
    /// </summary>
    public partial class ApplyScheduleChangesWindow : Window
    {

        private List<ScheduleRecord> scheduleRecords;

        public int SuccessfulChanges = 0;
        public int DuplicateRecords = 0;
        public int MissingFields = 0;


        public ApplyScheduleChangesWindow()
        {
            InitializeComponent();
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "csv files (*.csv)|*.csv";


            if(fileDialog.ShowDialog() == true)
            {
                string[] RecordStrings;

                //Check if file is available
                try
                {
                    RecordStrings = File.ReadAllLines(fileDialog.FileName);
                }
                catch
                {
                    MessageBox.Show("Ошибка доступа к файлу", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Get file name
                CsvFilePathTextBox.Text = fileDialog.FileName;

   
                //Convert csv into record list
                scheduleRecords = RecordStrings.Where(record => record.Split(',').Length == 9).Select(record => {

                    string[] data = record.Split(',');
                  
         
                    try
                    {
                        return new ScheduleRecord
                        {
                            Operation = data[0],
                            DepartureDate = DateTime.Parse(data[1]),
                            DepartureTime = TimeSpan.Parse(data[2]),
                            FlightNumber = data[3],
                            DepartureAirport = data[4],
                            ArrivalAirport = data[5],
                            AircraftCode = Int32.Parse(data[6]),
                            BasePrice = decimal.Parse(data[7].Replace('.',',')),
                            Confirmation = data[8] == "OK" ? true : false

                        };
                    }
                    catch (Exception error)
                    {

                        
                        MissingFields++;
                        return new ScheduleRecord
                        {
                            Operation = "ERROR"
                        };

                    }



                }
                ).ToList();

                // Display broken amount 
                MissingFieldsTextBlock.Text = $"[ {MissingFields} ]";

                // Find duplicates
                var entities = new Session2Entities();
                List<Schedules> Duplicates = new List<Schedules>();
                List<ScheduleRecord> RecordsToDelete = new List<ScheduleRecord>();
                foreach (var record in scheduleRecords)
                {

                    

                    try
                    {     
                        if( record.Operation == "ADD")
                        {
                            Duplicates.AddRange(entities.Schedules.Where(s => s.FlightNumber == record.FlightNumber && s.Date == record.DepartureDate));
                            if (entities.Schedules.Where(s => s.FlightNumber == record.FlightNumber && s.Date == record.DepartureDate).Count() > 0)
                            {
                                RecordsToDelete.Add(record);
                            }
                        }                        
                        
                    }
                    catch
                    {
                        MessageBox.Show("Ошибка БД", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                   
                }

                //Remove duplicate records
                foreach (var item in RecordsToDelete)
                {
                    scheduleRecords.Remove(item);
                }

                //Display duplicate amount
                DuplicateRecords = Duplicates.Count;
                DuplicateRecordsTextBlock.Text = $"[ {DuplicateRecords} ]";

                // Remove all broken records
                scheduleRecords.RemoveAll(r => r.Operation == "ERROR");


                //Add Records
                foreach (var record in scheduleRecords)
                {

                    if(record.Operation == "ADD")
                    {

                        entities.Schedules.Add(new Schedules
                        {

                            Date = record.DepartureDate,
                            Time = record.DepartureTime,
                            FlightNumber = record.FlightNumber,
                            RouteID = entities.Routes
                            .Where(r => r.Airports.IATACode == record.DepartureAirport && r.Airports1.IATACode == record.ArrivalAirport)
                            .Select(r => r.ID).First(),
                            AircraftID = record.AircraftCode,
                            EconomyPrice = record.BasePrice,
                            Confirmed = record.Confirmation



                        }) ;

                        entities.SaveChanges();
                        SuccessfulChanges++;
                    }
                    
                }


                //Update records               
                foreach (var record in scheduleRecords)
                {
                    if(record.Operation == "EDIT")
                    {

                        var ScheduleToChange = entities.Schedules.Where(s => s.FlightNumber == record.FlightNumber && s.Date == record.DepartureDate).Single();

                        if(ScheduleToChange is null)
                        {
                            MessageBox.Show("Записи для редактирования не существует, FlightNumber: " + record.FlightNumber);
                            return;
                        }


                        ScheduleToChange.Date = record.DepartureDate;
                        ScheduleToChange.Time = record.DepartureTime;
                        ScheduleToChange.RouteID = entities.Routes
                            .Where(r => r.Airports.IATACode == record.DepartureAirport && r.Airports1.IATACode == record.ArrivalAirport)
                            .Select(r => r.ID).First();
                        ScheduleToChange.EconomyPrice = record.BasePrice;
                        ScheduleToChange.AircraftID = record.AircraftCode;
                        ScheduleToChange.Confirmed = record.Confirmation;
                        entities.SaveChanges();

                        SuccessfulChanges++;
                    }
                }


                SuccessfulChangesTextBlock.Text = $"[ {SuccessfulChanges} ]";
                MessageBox.Show("Данные успешно загружены");

            }

        }
    }

    public class ScheduleRecord
    {
        public string Operation { get; set; }

        public DateTime DepartureDate { get; set; }

        public TimeSpan DepartureTime { get; set; }

        public string FlightNumber { get; set; }

        public string DepartureAirport { get; set; }

        public string ArrivalAirport { get; set; }

        public int AircraftCode { get; set; }

        public decimal BasePrice { get; set; }

        public bool Confirmation { get; set; }




    }


}
