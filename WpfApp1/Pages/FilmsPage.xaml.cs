using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    public partial class FilmsPage : Page
    {
        string film_name;
        TimeSpan film_duration;
        string film_rating;
        string film_producer;
        int film_style;
        int film_country;

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
            get { return (SourceCore.DB.FILMS.Count() - 1) / BlockRecordsCount + 1; }
        }

        public FilmsPage()
        {
            InitializeComponent();
            DataContext = this;
            FilmsDataGrid.ItemsSource = SourceCore.DB.FILMS.Where(P => P.film_id >= 0).OrderBy(P => P.film_name).Skip((BlockNum - 1) * BlockRecordsCount).Take(BlockRecordsCount).ToList();
            StyleComboBox.ItemsSource = SourceCore.DB.STYLES.ToList();
            CountryComboBox.ItemsSource = SourceCore.DB.COUNTRIES.ToList();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Первоначальная настройка фильтра данных для быстрого поиска,
            // при этом из фильтрации нужно исключить столбец "Управление"
            // Создание собствнного списка заголовков столбцов
            List<String> Columns = new List<string>();
            // Перебор и добавление в новый список только необходимых заголовков
            // Исключен столбец 4
            for (int I = 0; I < 6; I++)
            {
                Columns.Add(FilmsDataGrid.Columns[I].Header.ToString());
            }
            FilterComboBox.ItemsSource = Columns;
            FilterComboBox.SelectedIndex = 0;
            // Запрет на управление сортировкой щелчком по заголовку столбца
            foreach (DataGridColumn Column in FilmsDataGrid.Columns)
            {
                Column.CanUserSort = false;
            }
        }

        private void ShowButtonFilms(object sender, RoutedEventArgs e)
        {
            if (ChangeColumn.Width.Value == 0)
            {
                ChangeColumn.Width = new GridLength(250);
                SplitterColumn.Width = GridLength.Auto;
                if ((sender as Button).Content.ToString() == "Добавить")
                {
                    FilmsDataGrid.SelectedItem = null;
                    StyleComboBox.SelectedIndex = 0;
                    CountryComboBox.SelectedIndex = 0;
                }
                if (((sender as Button).Content.ToString() == "Копировать") && (FilmsDataGrid.SelectedItem != null))
                {
                    film_name = FilmsNameTextBox.Text;
                    film_duration = TimeSpan.Parse(DurationTextBox.Text);
                    film_rating = RatingTextBox.Text;
                    film_producer = ProducerTextBox.Text;
                    film_style = StyleComboBox.SelectedIndex;
                    film_country = CountryComboBox.SelectedIndex;
                    FilmsDataGrid.SelectedItem = null;

                    FilmsNameTextBox.Text = film_name;
                    DurationTextBox.Text = film_duration.ToString();
                    RatingTextBox.Text = film_rating;
                    ProducerTextBox.Text = film_producer;
                    StyleComboBox.SelectedIndex = film_style;
                    CountryComboBox.SelectedIndex = film_country;
                    FilmsDataGrid.IsHitTestVisible = false;
                }
            }
            else
            {
                ChangeColumn.Width = new GridLength(0);
                SplitterColumn.Width = new GridLength(0);
            }
        }

        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void FilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textbox = sender as TextBox;
            switch (FilterComboBox.SelectedIndex)
            {
                case 0:
                    FilmsDataGrid.ItemsSource = SourceCore.DB.FILMS.Where(filtercase => filtercase.film_name.ToString().Contains(textbox.Text)).ToList();
                    break;
                case 1:
                    FilmsDataGrid.ItemsSource = SourceCore.DB.FILMS.Where(filtercase => filtercase.film_duration.ToString().Contains(textbox.Text)).ToList();
                    break;
                case 2:
                    FilmsDataGrid.ItemsSource = SourceCore.DB.FILMS.Where(filtercase => filtercase.film_rating.ToString().Contains(textbox.Text)).ToList();
                    break;
                case 3:
                    FilmsDataGrid.ItemsSource = SourceCore.DB.FILMS.Where(filtercase => filtercase.film_producer.ToString().Contains(textbox.Text)).ToList();
                    break;
                case 4:
                    FilmsDataGrid.ItemsSource = SourceCore.DB.FILMS.Where(filtercase => filtercase.STYLES.style.ToString().Contains(textbox.Text)).ToList();
                    break;
                case 5:
                    FilmsDataGrid.ItemsSource = SourceCore.DB.FILMS.Where(filtercase => filtercase.COUNTRIES.country.ToString().Contains(textbox.Text)).ToList();
                    break;
            }
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

        private void DeleteFilmButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Удалить запись?", "Внимание!", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
            {
                try
                {
                    Data.FILMS DeletingFilm = (Data.FILMS)FilmsDataGrid.SelectedItem;
                    if (FilmsDataGrid.SelectedIndex < FilmsDataGrid.Items.Count - 1)
                    {
                        FilmsDataGrid.SelectedIndex++;
                    }
                    else
                    {
                        if (FilmsDataGrid.SelectedIndex > 0)
                        {
                            FilmsDataGrid.SelectedIndex--;
                        }
                    }
                    FilmsDataGrid.SelectedItem = DeletingFilm;
                    SourceCore.DB.FILMS.Remove(DeletingFilm);
                    SourceCore.DB.SaveChanges();
                    UpdateGrid((Data.FILMS)FilmsDataGrid.SelectedItem);
                }
                catch
                {
                    MessageBox.Show("Невозможно удалить запись, так как она используется в других справочниках базы данных.",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.None);

                }
                FilmsDataGrid.Focus();
            }
        }

        private void CloseEdChangeClick(object sender, RoutedEventArgs e)
        {
            ChangeColumn.Width = new GridLength(0);
            SplitterColumn.Width = new GridLength(0);
            FilmsDataGrid.IsHitTestVisible = true;
        }

        private void CommitButtonFilm(object sender, RoutedEventArgs e)
        {
            Regex regex = new Regex("^\\d{2}[-:]\\d{2}[-:]\\d{2}$");
            if (FilmsNameTextBox.Text.Trim() == null || FilmsNameTextBox.Text.Trim() == "")
            {
                MessageBox.Show("Поле название фильма не может быть пустым!",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.None);
                return;
            } else if (!regex.IsMatch(DurationTextBox.Text))
            {
                MessageBox.Show("Введено некорректное время начала сеанса!",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.None);
                return;
            } else if (RatingTextBox.Text.Trim() == null || RatingTextBox.Text.Trim() == "")
            {
                MessageBox.Show("Поле рейтинг фильма не может быть пустым!",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.None);
                return;
            } else if (ProducerTextBox.Text.Trim() == null || ProducerTextBox.Text.Trim() == "")
            {
                MessageBox.Show("Поле продюсер не может быть пустым!",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.None);
                return;
            } else if (StyleComboBox.SelectedItem == null)
            {
                MessageBox.Show("Поле жанр фильма не может быть пустым!",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.None);
                return;
            } else if (CountryComboBox.SelectedItem == null)
            {
                MessageBox.Show("Поле страна фильма не может быть пустым!",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.None);
                return;
            }
            var NewFilm = new Data.FILMS
            {
                film_name = FilmsNameTextBox.Text,
                film_duration = TimeSpan.Parse(DurationTextBox.Text),
                film_rating = RatingTextBox.Text,
                film_producer = ProducerTextBox.Text,
                STYLES = (Data.STYLES)StyleComboBox.SelectedItem,
                COUNTRIES = (Data.COUNTRIES)CountryComboBox.SelectedItem
            };
            if (FilmsDataGrid.SelectedItem == null)
            {
                SourceCore.DB.FILMS.Add(NewFilm);
            }
            SourceCore.DB.SaveChanges();
            UpdateGrid(NewFilm);
            CloseEdChangeClick(sender, e);
        }

        public void UpdateGrid(Data.FILMS films)
        {
            if ((films == null) && (FilmsDataGrid.ItemsSource != null))
            {
                films = (Data.FILMS)FilmsDataGrid.SelectedItem;
            }
            FilmsDataGrid.ItemsSource = SourceCore.DB.FILMS.Where(P => P.film_id >= 0).OrderBy(P => P.film_name).Skip((BlockNum - 1) * BlockRecordsCount).Take(BlockRecordsCount).ToList();
            FilmsDataGrid.SelectedItem = films;
        }
    }
}
