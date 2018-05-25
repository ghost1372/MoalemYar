﻿using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MoalemYar.UserControls
{
    /// <summary>
    /// Interaction logic for Achievement.xaml
    /// </summary>
    public partial class Achievement : UserControl
    {
        private List<DataClass.Tables.Score> _initialCollection;

        public Achievement()
        {
            InitializeComponent();

            getSchool();
        }

        #region Async Query

        public async static Task<List<DataClass.Tables.School>> GetAllSchoolsAsync()
        {
            using (var db = new DataClass.myDbContext())
            {
                var query = db.Schools.Select(x => x);
                return await query.ToListAsync();
            }
        }

        public async static Task<List<DataClass.DataTransferObjects.StudentsDto>> GetAllStudentsAsync(long BaseId)
        {
            using (var db = new DataClass.myDbContext())
            {
                var query = db.Students.OrderBy(x => x.LName).Where(x => x.BaseId == BaseId).Select(x => new DataClass.DataTransferObjects.StudentsDto { Name = x.Name, LName = x.LName, FName = x.FName, BaseId = x.BaseId, Id = x.Id });
                return await query.ToListAsync();
            }
        }

        public async static Task<List<DataClass.Tables.Score>> GetAllStudentsScoreAsync(long StudentId)
        {
            using (var db = new DataClass.myDbContext())
            {
                var query = db.Scores.Where(x => x.StudentId == StudentId).Select(x => x);
                return await query.ToListAsync();
            }
        }

        private void getSchool()
        {
            try
            {
                var query = GetAllSchoolsAsync();
                query.Wait();
                List<DataClass.Tables.School> data = query.Result;
                if (data.Any())
                {
                    cmbEditBase.ItemsSource = data;
                }
            }
            catch (Exception)
            {
            }
        }

        private void getStudent(long BaseId)
        {
            try
            {
                var query = GetAllStudentsAsync(BaseId);
                query.Wait();
                List<DataClass.DataTransferObjects.StudentsDto> data = query.Result;
                if (data.Any())
                {
                    dataGrid.ItemsSource = data;
                }
                else
                {
                    MainWindow.main.ShowNoDataNotification(null);
                }
            }
            catch (Exception)
            {
            }
        }

        private void getStudentScore(long StudentId)
        {
            try
            {
                var query = GetAllStudentsScoreAsync(StudentId);
                query.Wait();
                List<DataClass.Tables.Score> data = query.Result;
                if (data.Any())
                    _initialCollection = data;
                else
                    _initialCollection = null;
            }
            catch (NullReferenceException)
            {
            }
        }

        #endregion Async Query

        private void cmbEditBase_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            getStudent(Convert.ToInt64(cmbEditBase.SelectedValue));
        }

        private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                dynamic selectedItem = dataGrid.SelectedItems[0];

                waterfallFlow.Children.Clear();
                Series series = new ColumnSeries();

                switch (Convert.ToInt32(FindElement.Settings[AppVariable.ChartType] ?? 0))
                {
                    case 0:
                        series = new ColumnSeries { };
                        break;

                    case 1:
                        series = new StackedColumnSeries { };
                        break;

                    case 2:
                        series = new LineSeries { };
                        break;

                    case 3:
                        series = new StepLineSeries { };
                        break;

                    case 4:
                        series = new StackedAreaSeries { };
                        break;
                }

                getStudentScore(selectedItem.Id); // get Student scores

                //get scores merge duplicates and replace string to int
                var score = _initialCollection.GroupBy(x => new { x.Book, x.Date, x.StudentId })
                            .Select(x => new
                            {
                                x.Key.StudentId,
                                x.Key.Book,
                                x.Key.Date,
                                Sum = x.Sum(y => AppVariable.EnumToNumber(y.Scores))
                            }).ToArray();

                //get Book Count for generate chart
                var bookCount = score.GroupBy(x => new { x.Book })
                     .Select(g => new
                     {
                         g.Key.Book
                     }).ToList();

                MaterialChart _addUser;
                Control _currentUser;

                //generate chart based on count of books
                foreach (var item in bookCount)
                {
                    _addUser = new MaterialChart(item.Book, selectedItem.Name + " " + selectedItem.LName, getDateArray(item.Book), getScoreArray(item.Book), getAverage(item.Book), getAverageStatus(item.Book), series, AppVariable.GetBrush(Convert.ToString(FindElement.Settings[AppVariable.ChartColor] ?? AppVariable.CHART_GREEN)));
                    _currentUser = _addUser;
                    waterfallFlow.Children.Add(_currentUser);
                }

                waterfallFlow.Refresh();
            }
            catch (NullReferenceException)
            {
            }
        }

        //get Score Average to string
        private string getAverage(string Book)
        {
            var score = _initialCollection.GroupBy(x => new { x.Book, x.Date, x.StudentId })
                           .Select(x => new
                           {
                               x.Key.StudentId,
                               x.Key.Book,
                               x.Key.Date,
                               Sum = x.Sum(y => AppVariable.EnumToNumber(y.Scores))
                           }).Where(x => x.Book == Book).ToArray();
            var dCount = score.Select(x => x.Date).Count();
            var sum = score.Sum(x => x.Sum);

            var one = decimal.Divide(sum, 1);
            var sec = decimal.Divide(sum, 2);
            var thi = decimal.Divide(sum, 3);
            var forth = decimal.Divide(sum, 4);

            return decimal.Divide(sum, dCount).ToString("0.00");
        }

        private string getAverageStatus(string Book)
        {
            var score = _initialCollection.GroupBy(x => new { x.Book, x.Date, x.StudentId })
                           .Select(x => new
                           {
                               x.Key.StudentId,
                               x.Key.Book,
                               x.Key.Date,
                               Sum = x.Sum(y => AppVariable.EnumToNumber(y.Scores))
                           }).Where(x => x.Book == Book).ToArray();

            var sum = score.Sum(x => x.Sum);

            var dCount = score.Select(x => x.Date).Count();

            var Avg = decimal.Divide(sum, dCount).ToString("0.00");

            var one = decimal.Divide(dCount * 4, 1).ToString("0.00");
            var sec = decimal.Divide(dCount * 4, 2).ToString("0.00");
            var thi = decimal.Divide(dCount * 4, 3).ToString("0.00");
            var forth = decimal.Divide(dCount * 4, 4).ToString("0.00");

            string status = string.Empty;

            if (Convert.ToDecimal(Avg) >= Convert.ToDecimal(sec))
                status = "خیلی خوب";
            else if (Convert.ToDecimal(Avg) < Convert.ToDecimal(one) && Convert.ToDecimal(Avg) >= Convert.ToDecimal(thi))
                status = "خوب";
            else if (Convert.ToDecimal(Avg) < Convert.ToDecimal(sec) && Convert.ToDecimal(Avg) >= Convert.ToDecimal(forth))
                status = "قابل قبول";
            else if (Convert.ToDecimal(Avg) < Convert.ToDecimal(forth))
                status = "نیاز به تلاش بیشتر";

            return status;
        }

        //get Dates to string[]
        private string[] getDateArray(string Book)
        {
            var score = _initialCollection.GroupBy(x => new { x.Book, x.Date, x.StudentId })
                           .Select(x => new
                           {
                               x.Key.StudentId,
                               x.Key.Book,
                               x.Key.Date,
                               Sum = x.Sum(y => AppVariable.EnumToNumber(y.Scores))
                           }).Where(x => x.Book == Book).ToArray();
            return score.Select(x => x.Date).ToArray();
        }

        //get Scores to double[]
        private double[] getScoreArray(string Book)
        {
            var score = _initialCollection.GroupBy(x => new { x.Book, x.Date, x.StudentId })
                           .Select(x => new
                           {
                               x.Key.StudentId,
                               x.Key.Book,
                               x.Key.Date,
                               Sum = x.Sum(y => AppVariable.EnumToNumber(y.Scores))
                           }).Where(x => x.Book == Book).ToArray();
            return score.Select(x => Convert.ToDouble(x.Sum)).ToArray();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            cmbEditBase.SelectedIndex = Convert.ToInt32(FindElement.Settings[AppVariable.DefaultSchool] ?? -1);
        }
    }
}