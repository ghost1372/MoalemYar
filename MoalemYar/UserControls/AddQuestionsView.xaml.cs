﻿/****************************** ghost1372.github.io ******************************\
*	Module Name:	AddQuestions.xaml.cs
*	Project:		MoalemYar
*	Copyright (C) 2017 Mahdi Hosseini, All rights reserved.
*	This software may be modified and distributed under the terms of the MIT license.  See LICENSE file for details.
*
*	Written by Mahdi Hosseini <Mahdidvb72@gmail.com>,  2018, 6, 1, 06:55 ب.ظ
*
***********************************************************************************/

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MoalemYar.UserControls
{
    /// <summary>
    /// Interaction logic for AddQuestions.xaml
    /// </summary>
    public partial class AddQuestionsView : UserControl
    {
        internal static AddQuestionsView main;
        private List<DataClass.Tables.AQuestion> _initialCollection;
        private PersianCalendar pc = new PersianCalendar();
        private string strDate;

        public AddQuestionsView()
        {
            InitializeComponent();
            this.DataContext = this;
            main = this;
            strDate = pc.GetYear(DateTime.Now).ToString("0000") + "/" + pc.GetMonth(DateTime.Now).ToString("00") + "/" + pc.GetDayOfMonth(DateTime.Now).ToString("00");
            getGroup();
        }

        #region "Async Query"

        public async static Task<List<DataClass.Tables.AQuestion>> GetAllAQuestionsAsync(long GroupId)
        {
            using (var db = new DataClass.myDbContext())
            {
                var query = db.AQuestions.Where(x => x.GroupId == GroupId).ToListAsync();
                return await query;
            }
        }

        public static async Task<string> DeleteAQuestionsAsync(long id)
        {
            using (var db = new DataClass.myDbContext())
            {
                var DeleteAQuestions = await db.AQuestions.FindAsync(id);
                db.AQuestions.Remove(DeleteAQuestions);
                await db.SaveChangesAsync();
                return "AQuestions Deleted Successfully";
            }
        }

        #endregion "Async Query"

        #region Func get Query Wait"

        private void getAQuestions(long GroupId)
        {
            try
            {
                var query = GetAllAQuestionsAsync(GroupId);
                query.Wait();

                List<DataClass.Tables.AQuestion> data = query.Result;
                _initialCollection = query.Result;
                if (data.Any())
                {
                    dataGrid.ItemsSource = data;
                }
                else
                {
                    dataGrid.ItemsSource = null;
                    MainWindow.main.showGrowlNotification(NotificationKEY: AppVariable.No_Data_KEY, param: "AQuestions");
                }
            }
            catch (Exception)
            {
            }
        }

        private void getGroup()
        {
            try
            {
                using (var db = new DataClass.myDbContext())
                {
                    var query = db.Groups.ToList();
                    if (query.Any())
                    {
                        cmbGroup.ItemsSource = query;
                        cmbBaseEdit.ItemsSource = query;
                        cmbGroupEdit.ItemsSource = query;
                    }
                    else
                    {
                        dataGrid.ItemsSource = null;
                        MainWindow.main.showGrowlNotification(NotificationKEY: AppVariable.No_Data_KEY, param: "Group");
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private void deleteAQuestions(long id)
        {
            var query = DeleteAQuestionsAsync(id);
            query.Wait();
        }

        private void updateAQuestions(long id, long GroupId, string Class, string QuestionText, string Case1, string Case2, string Case3, string Case4, int Answer, string Date)
        {
            using (var db = new DataClass.myDbContext())
            {
                var EditAQuestions = db.AQuestions.Find(id);
                EditAQuestions.GroupId = GroupId;
                EditAQuestions.Class = Class;
                EditAQuestions.QuestionText = QuestionText;
                EditAQuestions.Case1 = Case1;
                EditAQuestions.Case2 = Case2;
                EditAQuestions.Case3 = Case3;
                EditAQuestions.Case4 = Case4;
                EditAQuestions.Answer = Answer;
                EditAQuestions.Date = Date;
                db.SaveChanges();
            }
        }

        private void addAQuestions(long GroupId, string Class, string QuestionText, string Case1, string Case2, string Case3, string Case4, int Answer, string Date)
        {
            using (var db = new DataClass.myDbContext())
            {
                var aQuestion = new DataClass.Tables.AQuestion();
                aQuestion.GroupId = GroupId;
                aQuestion.Class = Class;
                aQuestion.QuestionText = QuestionText;
                aQuestion.Case1 = Case1;
                aQuestion.Case2 = Case2;
                aQuestion.Case3 = Case3;
                aQuestion.Case4 = Case4;
                aQuestion.Answer = Answer;
                aQuestion.Date = Date;
                db.AQuestions.Add(aQuestion);

                db.SaveChanges();
            }
        }

        #endregion Func get Query Wait"

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.main.ClearScreen();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            int answ = 0;
            if (txtQuestionText.Text == string.Empty || txtCase1.Text == string.Empty || txtCase2.Text == string.Empty || txtCase3.Text == string.Empty || txtCase4.Text == string.Empty || cmbBase.SelectedIndex == -1 || cmbGroup.SelectedIndex == -1 || cmbEditBase.SelectedIndex == -1)
            {
                MainWindow.main.showGrowlNotification(NotificationKEY: AppVariable.Fill_All_Data_KEY);
            }
            else
            {
                try
                {
                    switch (cmbEditBase.SelectedIndex)
                    {
                        case 0:
                            answ = 1;
                            break;

                        case 1:
                            answ = 2;
                            break;

                        case 2:
                            answ = 3;
                            break;

                        case 3:
                            answ = 4;
                            break;
                    }
                    addAQuestions(Convert.ToInt64(cmbGroup.SelectedValue), cmbBase.Text, txtQuestionText.Text, txtCase1.Text, txtCase2.Text, txtCase3.Text, txtCase4.Text, answ, strDate);
                    MainWindow.main.showGrowlNotification(AppVariable.Add_Data_KEY, true, "", "سوال");
                    txtCase1.Text = string.Empty;
                    txtCase2.Text = string.Empty;
                    txtCase3.Text = string.Empty;
                    txtCase4.Text = string.Empty;
                    txtQuestionText.Text = string.Empty;
                    txtQuestionText.Focus();
                    if (cmbBaseEdit.SelectedIndex > -1)
                        cmbBaseEdit_SelectionChanged(null, null);
                }
                catch (Exception)
                {
                    MainWindow.main.showGrowlNotification(AppVariable.Add_Data_KEY, false, "", "سوال");
                }
            }
        }

        private void txtEditSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (dataGrid.ItemsSource != null)
            {
                if (txtEditSearch.Text != string.Empty)
                    dataGrid.ItemsSource = _initialCollection.Where(x => x.QuestionText.Contains(txtEditSearch.Text) || x.Date.Contains(txtEditSearch.Text));
                else
                    dataGrid.ItemsSource = _initialCollection.Select(x => x);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.main.showGrowlNotification(NotificationKEY: AppVariable.Delete_Confirm_KEY, param: new[] { string.Empty, "سوال" });
        }

        public void deleteGroup()
        {
            try
            {
                dynamic selectedItem = dataGrid.SelectedItems[0];
                long id = selectedItem.Id;
                deleteAQuestions(id);
                MainWindow.main.showGrowlNotification(AppVariable.Deleted_KEY, true, "", "سوال");
                getAQuestions(Convert.ToInt64(cmbBaseEdit.SelectedValue));
            }
            catch (Exception)
            {
                MainWindow.main.showGrowlNotification(AppVariable.Deleted_KEY, false, "", "سوال");
            }
        }

        private void setComboValue(string index)
        {
            switch (index)
            {
                case "اول":
                    cmbBaseEditData.SelectedIndex = 0;
                    break;

                case "دوم":
                    cmbBaseEditData.SelectedIndex = 1;
                    break;

                case "سوم":
                    cmbBaseEditData.SelectedIndex = 2;
                    break;

                case "چهارم":
                    cmbBaseEditData.SelectedIndex = 3;
                    break;

                case "پنجم":
                    cmbBaseEditData.SelectedIndex = 4;
                    break;

                case "ششم":
                    cmbBaseEditData.SelectedIndex = 5;
                    break;

                case null:
                    cmbBaseEditData.SelectedIndex = -1;
                    break;
            }
        }

        private void setComboValue2(string index)
        {
            switch (index)
            {
                case "1":
                    cmbEditAnswersData.SelectedIndex = 0;
                    break;

                case "2":
                    cmbEditAnswersData.SelectedIndex = 1;
                    break;

                case "3":
                    cmbEditAnswersData.SelectedIndex = 2;
                    break;

                case "4":
                    cmbEditAnswersData.SelectedIndex = 3;
                    break;

                case null:
                    cmbEditAnswersData.SelectedIndex = -1;
                    break;
            }
        }

        private void cmbBaseEdit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            getAQuestions(Convert.ToInt64(cmbBaseEdit.SelectedValue));
        }

        private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                dynamic selectedItem = dataGrid.SelectedItems[0];
                txtEditCase1.Text = selectedItem.Case1;
                txtEditCase2.Text = selectedItem.Case2;
                txtEditCase3.Text = selectedItem.Case3;
                txtEditCase4.Text = selectedItem.Case4;
                txtEditQuestionText.Text = selectedItem.QuestionText;
                setComboValue2(Convert.ToString(selectedItem.Answer));
                cmbGroupEdit.SelectedValue = selectedItem.GroupId;
                setComboValue(selectedItem.Class);
                txtDateEdit.SelectedDate = new PersianCalendarWPF.PersianDate(Convert.ToInt32(selectedItem.Date.Substring(0, 4)), Convert.ToInt32(selectedItem.Date.Substring(5, 2)), Convert.ToInt32(selectedItem.Date.Substring(8, 2)));
            }
            catch (Exception)
            {
            }
        }

        private void btnEditSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                dynamic selectedItem = dataGrid.SelectedItems[0];
                long id = selectedItem.Id;
                updateAQuestions(id, Convert.ToInt64(cmbGroupEdit.SelectedValue), cmbBaseEditData.Text, txtEditQuestionText.Text, txtEditCase1.Text, txtEditCase2.Text, txtEditCase3.Text, txtEditCase4.Text, Convert.ToInt32(cmbEditAnswersData.Text), txtDateEdit.SelectedDate.ToString());
                MainWindow.main.showGrowlNotification(AppVariable.Update_Data_KEY, true, string.Empty, "سوال");
                getAQuestions(Convert.ToInt64(cmbBaseEdit.SelectedValue));
            }
            catch (Exception)
            {
                MainWindow.main.showGrowlNotification(AppVariable.Update_Data_KEY, false, string.Empty, "سوال");
            }
        }

        private void btnEditCancel_Click(object sender, RoutedEventArgs e)
        {
            txtEditCase1.Text = string.Empty;
            txtEditCase2.Text = string.Empty;
            txtEditCase3.Text = string.Empty;
            txtEditCase4.Text = string.Empty;
            txtEditQuestionText.Text = string.Empty;
            setComboValue(null);
            setComboValue2(null);
            cmbGroupEdit.SelectedIndex = -1;
            dataGrid.UnselectAll();
        }
    }
}