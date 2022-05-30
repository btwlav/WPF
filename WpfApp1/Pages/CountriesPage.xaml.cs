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

namespace WpfApp1.Pages
{
    /// <summary>
    /// Interaction logic for CountriesPage.xaml
    /// </summary>
    public partial class CountriesPage : Page
    {
        string country;

        // Компонент для организации работы с данными в таблице DataGrid
        private CollectionViewSource StylesViewModel { get; set; }

        // Текущий номер блока информации в таблице
        private int _BlockNum = 1;
        public int BlockNum
        {
            get
            {
                return _BlockNum;
            }
            set
            {
                if (value <= 0)
                {
                    value = 1;
                }
                else
                {
                    if (value > BlockCount)
                    {
                        value = BlockCount;
                    }
                }
                if (_BlockNum != value)
                {
                    _BlockNum = value;
                    BlockNumLabel.GetBindingExpression(Label.ContentProperty).UpdateTarget();
                }
                UpdateGrid(null);
            }
        }

        // Количество записей в блоке информации в таблице
        private int _BlockRecordsCount = 5;
        public int BlockRecordsCount
        {
            get
            {
                return _BlockRecordsCount;
            }
            set
            {
                if (value <= 0)
                {
                    value = 1;
                }
                if (_BlockRecordsCount != value)
                {
                    _BlockRecordsCount = value;
                    BlockCountLabel.GetBindingExpression(Label.ContentProperty).UpdateTarget();
                    BlockNum = _BlockNum;
                    UpdateGrid(null);
                }
            }
        }

        // Количество блоков информации в таблице всего
        public int BlockCount
        {
            get { return (SourceCore.DB.COUNTRIES.Count() - 1) / BlockRecordsCount + 1; }
        }

        public CountriesPage()
        {
            InitializeComponent();
            DataContext = this;
            CountriesDataGrid.ItemsSource = SourceCore.DB.COUNTRIES.Where(P => P.country_id >= 0).OrderBy(P => P.country).Skip((BlockNum - 1) * BlockRecordsCount).Take(BlockRecordsCount).ToList();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Первоначальная настройка фильтра данных для быстрого поиска,
            // при этом из фильтрации нужно исключить столбец "Управление"
            // Создание собствнного списка заголовков столбцов
            List<String> Columns = new List<string>
            {
                CountriesDataGrid.Columns[0].Header.ToString()
            };
            FilterComboBox.ItemsSource = Columns;
            FilterComboBox.SelectedIndex = 0;
            // Запрет на управление сортировкой щелчком по заголовку столбца
            foreach (DataGridColumn Column in CountriesDataGrid.Columns)
            {
                Column.CanUserSort = false;
            }
        }

        private void ShowButtonCountries(object sender, RoutedEventArgs e)
        {
            if (ChangeColumn.Width.Value == 0)
            {
                ChangeColumn.Width = new GridLength(250);
                SplitterColumn.Width = GridLength.Auto;
                if ((sender as Button).Content.ToString() == "Добавить")
                {
                    CountriesDataGrid.SelectedItem = null;
                }
                if (((sender as Button).Content.ToString() == "Копировать") && (CountriesDataGrid.SelectedItem != null))
                {
                    country = CountryNameTextBox.Text;
                    CountriesDataGrid.SelectedItem = null;
                    CountryNameTextBox.Text = country;
                    CountriesDataGrid.IsHitTestVisible = false;
                }
            }
            else
            {
                ChangeColumn.Width = new GridLength(0);
                SplitterColumn.Width = new GridLength(0);
            }
        }

        private void DeleteCountryButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Удалить запись?", "Внимание!", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
            {
                try
                {
                    Data.COUNTRIES DeletingStyle = (Data.COUNTRIES)CountriesDataGrid.SelectedItem;
                    if (CountriesDataGrid.SelectedIndex < CountriesDataGrid.Items.Count - 1)
                    {
                        CountriesDataGrid.SelectedIndex++;
                    }
                    else
                    {
                        if (CountriesDataGrid.SelectedIndex > 0)
                        {
                            CountriesDataGrid.SelectedIndex--;
                        }
                    }
                    CountriesDataGrid.SelectedItem = DeletingStyle;
                    SourceCore.DB.COUNTRIES.Remove(DeletingStyle);
                    SourceCore.DB.SaveChanges();
                    UpdateGrid((Data.COUNTRIES)CountriesDataGrid.SelectedItem);
                }
                catch
                {
                    MessageBox.Show("Невозможно удалить запись, так как она используется в других справочниках базы данных.",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.None);

                }
                CountriesDataGrid.Focus();
            }
        }

        private void FilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textbox = sender as TextBox;
            switch (FilterComboBox.SelectedIndex)
            {
                case 0:
                    CountriesDataGrid.ItemsSource = SourceCore.DB.COUNTRIES.Where(filtercase => filtercase.country.ToString().Contains(textbox.Text)).ToList();
                    break;
            }
        }

        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void FirstBlockButton_Click(object sender, RoutedEventArgs e)
        {
            BlockNum = 1;
        }

        private void PreviosBlockButton_Click(object sender, RoutedEventArgs e)
        {
            BlockNum--;
        }

        private void NextBlockButton_Click(object sender, RoutedEventArgs e)
        {
            BlockNum++;
        }

        private void LastBlockButton_Click(object sender, RoutedEventArgs e)
        {
            BlockNum = BlockCount;
        }

        private void CloseEdChangeClick(object sender, RoutedEventArgs e)
        {
            ChangeColumn.Width = new GridLength(0);
            SplitterColumn.Width = new GridLength(0);
            CountriesDataGrid.IsHitTestVisible = true;
        }

        //Метод принятия изменений
        private void CommitButtonCountry(object sender, RoutedEventArgs e)
        {
            if (CountryNameTextBox.Text.Trim() == null || CountryNameTextBox.Text.Trim() == "")
            {
                MessageBox.Show("Поле название страны не может быть пустым!",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.None);
                return;
            }
            var NewCountry = new Data.COUNTRIES
            {
                country = CountryNameTextBox.Text
            };
            if (CountriesDataGrid.SelectedItem == null)
            {
                SourceCore.DB.COUNTRIES.Add(NewCountry);
            }
            SourceCore.DB.SaveChanges();
            UpdateGrid(NewCountry);

            CloseEdChangeClick(sender, e);
        }

        //Метод обновления грида
        public void UpdateGrid(Data.COUNTRIES Countries)
        {
            if ((Countries == null) && (CountriesDataGrid.ItemsSource != null))
            {
                Countries = (Data.COUNTRIES)CountriesDataGrid.SelectedItem;
            }
            CountriesDataGrid.ItemsSource = SourceCore.DB.COUNTRIES.Where(P => P.country_id >= 0).OrderBy(P => P.country).Skip((BlockNum - 1) * BlockRecordsCount).Take(BlockRecordsCount).ToList();
            CountriesDataGrid.SelectedItem = Countries;
        }
    }
}
