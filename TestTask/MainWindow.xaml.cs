using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Data;
using Microsoft.Win32;
using System.Diagnostics;

namespace TestTask
{
    public struct Counter
    {
        public int rkk;
        public int appeals;
    }
    public partial class MainWindow : Window
    {
        private Dictionary<string, Counter> respPerformers = new();
        private string[] rkk;
        private string[] appeals;
       
        private static void FillTable(Dictionary<string, Counter> respPerformers, string[] file, string count)
        {
            string responsiblePerformer;
            Counter counter = new();
            foreach (string item in file)
            {
                string[] leadersPerformers = item.Split('\t'); // руководители | исполнители
                if (leadersPerformers[0] == "Климов Сергей Александрович")
                {
                    //Отв. исполнителем является первый исполнитель
                    responsiblePerformer = leadersPerformers[1].Split(';')[0].Replace(" (Отв.)", ""); 
                }
                else
                {
                    //Отв. исполнителем является руководитель
                    string[] str = leadersPerformers[0].Split(' ');
                    responsiblePerformer = str[0] + " " + str[1].Substring(0, 1) + "." + str[2].Substring(0, 1) + ".";
                }


                if (respPerformers.ContainsKey(responsiblePerformer))
                {
                    Counter value = respPerformers[responsiblePerformer];
                    if (count == "rkk")
                        value.rkk++;
                    else if(count == "appeals")
                        value.appeals++;
                    respPerformers[responsiblePerformer] = value;
                }
                else
                {
                    if (count == "rkk")
                        counter.rkk = 1;
                    else if (count == "appeals")
                        counter.appeals = 1;
                    respPerformers.Add(responsiblePerformer, counter);
                }
            }
        }
        public MainWindow()
        {
            InitializeComponent();
        }
        private void Button_OpenRKK(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                rkk = File.ReadAllLines(openFileDialog.FileName);
                loadRKK.Text = $"Загружен файл РКК: {openFileDialog.FileName} ";
            }
        }
        private void Button_OpenAppeals(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                appeals = File.ReadAllLines(openFileDialog.FileName);
                loadAppeals.Text = $"Загружен файл обращений: {openFileDialog.FileName} ";
            }
        }
        private void SelectDG()
        {
            DG.ItemsSource = respPerformers.Select(p => new
            {
                Name = p.Key,
                CountRKK = p.Value.rkk,
                CountAppeals = p.Value.appeals,
                CountRKKandAppleals = p.Value.rkk + p.Value.appeals
            });
            DG.Columns[0].Header = "Ответственный \nисполнитель";
            DG.Columns[1].Header = "Количество \nнеисполненных \nвходящих документов";
            DG.Columns[2].Header = "Количество \nнеисполненных \nписьменных \nобращений граждан";
            DG.Columns[3].Header = "Общее количество \nдокументов и \nобращений";
        }
        private void Button_Load(object sender, RoutedEventArgs e)
        {
            if (rkk != null && appeals != null)
            {
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                FillTable(respPerformers, rkk, "rkk");
                FillTable(respPerformers, appeals, "appeals");
                stopWatch.Stop();
                time.Text = $"Время выполнения алгоритма: {stopWatch.ElapsedMilliseconds} мс";
                total.Text = $"Не исполнено в срок {respPerformers.Sum(p => p.Value.rkk + p.Value.appeals)} документов, из них:";
                totalrkk.Text = $"- количество неисполненных входящих документов: {respPerformers.Sum(p => p.Value.rkk)};";
                totalappeals.Text = $"- количество неисполненных письменных обращений граждан: {respPerformers.Sum(p => p.Value.appeals)}.";
                date.Text = $"Дата составления справки: {DateTime.Now.ToShortDateString()}";

                SelectDG();
            }
            else MessageBox.Show("Необходимо загрузить файлы!");
        }
        private void Button_SortedFIO(object sender, RoutedEventArgs e)
        {
            sort.Text = "Сортировка: по фамилии ответственного исполнителя";
            respPerformers = respPerformers.OrderBy(p => p.Key).ToDictionary(p => p.Key, p => p.Value);
            SelectDG();
        }
        private void Button_SortedRKK(object sender, RoutedEventArgs e)
        {
            sort.Text = "Сортировка: по количеству РКК";
            respPerformers = respPerformers.OrderByDescending(p => p.Value.rkk).ThenByDescending(p => p.Value.appeals).ToDictionary(p => p.Key, p => p.Value);
            SelectDG();
        }
        private void Button_SortedAppeals(object sender, RoutedEventArgs e)
        {
            sort.Text = "Сортировка: по количеству обращений";
            respPerformers = respPerformers.OrderByDescending(p => p.Value.appeals).ThenByDescending(p => p.Value.rkk).ToDictionary(p => p.Key, p => p.Value);
            SelectDG();
        }
        private void Button_SortedRKKandAppeals(object sender, RoutedEventArgs e)
        {
            sort.Text = "Сортировка: по общему количеству документов";
            respPerformers = respPerformers.OrderByDescending(p => p.Value.rkk + p.Value.appeals).ThenByDescending(p => p.Value.rkk).ToDictionary(p => p.Key, p => p.Value);
            SelectDG();
        }
        private void Button_SaveFile(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text files (*.txt)|*.txt";
            if (saveFileDialog.ShowDialog() == true)
            {
                using (StreamWriter writer = new(saveFileDialog.FileName, false))
                {
                    writer.WriteLine("{0,4} |{1,20} |{2,11} |{3,16}|{4,13} ",
                        "№", "Исполнитель", "Кол-во ркк", "Кол-во обращений", "Общее кол-во");
                    int i = 1;
                    foreach (KeyValuePair<string, Counter> item in respPerformers)
                    {
                        writer.WriteLine("------------------------------------------------------------------------");
                        writer.WriteLine("{0,4} |{1,20} |{2,11} |{3,15} |{4,13} ",
                            i++, item.Key, item.Value.rkk, item.Value.appeals, item.Value.rkk + item.Value.appeals);
                    }
                }
            }
        }
    }
}
