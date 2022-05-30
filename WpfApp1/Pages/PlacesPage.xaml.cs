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
    /// Interaction logic for PlacesPage.xaml
    /// </summary>
    public partial class PlacesPage : Page
    {
        int hall_id;
        int row;
        int place;

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
            get { return (SourceCore.DB.PLACES.Count() - 1) / BlockRecordsCount + 1; }
        }

        public PlacesPage()
        {
            InitializeComponent();
            DataContext = this;
            PlacesDataGrid.ItemsSource = SourceCore.DB.PLACES.Where(P => P.place_id >= 0).OrderBy(P => P.hall_id).Skip((BlockNum - 1) * BlockRecordsCount).Take(BlockRecordsCount).ToList();
            HallComboBox.ItemsSource = SourceCore.DB.HALLS.ToList();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Первоначальная настройка фильтра данных для быстрого поиска,
            // при этом из фильтрации нужно исключить столбец "Управление"
            // Создание собствнного списка заголовков столбцов
            List<String> Columns = new List<string>();
            // Перебор и добавление в новый список только необходимых заголовков
            // Исключен столбец 4
            for (int I = 0; I < 3; I++)
            {
                Columns.Add(PlacesDataGrid.Columns[I].Header.ToString());
            }
            FilterComboBox.ItemsSource = Columns;
            FilterComboBox.SelectedIndex = 0;
            // Запрет на управление сортировкой щелчком по заголовку столбца
            foreach (DataGridColumn Column in PlacesDataGrid.Columns)
            {
                Column.CanUserSort = false;
            }
        }

        private void ShowButtonPlaces(object sender, RoutedEventArgs e)
        {
            if (ChangeColumn.Width.Value == 0)
            {
                ChangeColumn.Width = new GridLength(250);
                SplitterColumn.Width = GridLength.Auto;
                if ((sender as Button).Content.ToString() == "Добавить")
                {
                    PlacesDataGrid.SelectedItem = null;
                }
                if (((sender as Button).Content.ToString() == "Копировать") && (PlacesDataGrid.SelectedItem != null))
                {
                    hall_id = HallComboBox.SelectedIndex;
                    row = int.Parse(RowTextBox.Text);
                    place = int.Parse(PlaceTextBox.Text);
                    PlacesDataGrid.SelectedItem = null;

                    HallComboBox.SelectedIndex = hall_id;
                    RowTextBox.Text = row.ToString();
                    PlaceTextBox.Text = place.ToString();
                    PlacesDataGrid.IsHitTestVisible = false;
                }
                //SourceCore.DB.SaveChanges();
            }
            else
            {
                ChangeColumn.Width = new GridLength(0);
                SplitterColumn.Width = new GridLength(0);
            }
        }

        private void DeletePlaceButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Удалить запись?", "Внимание!", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
            {
                try
                {
                    Data.PLACES DeletingPlace = (Data.PLACES)PlacesDataGrid.SelectedItem;
                    if (PlacesDataGrid.SelectedIndex < PlacesDataGrid.Items.Count - 1)
                    {
                        PlacesDataGrid.SelectedIndex++;
                    }
                    else
                    {
                        if (PlacesDataGrid.SelectedIndex > 0)
                        {
                            PlacesDataGrid.SelectedIndex--;
                        }
                    }
                    PlacesDataGrid.SelectedItem = DeletingPlace;
                    SourceCore.DB.PLACES.Remove(DeletingPlace);
                    SourceCore.DB.SaveChanges();
                    UpdateGrid((Data.PLACES)PlacesDataGrid.SelectedItem);
                }
                catch
                {
                    MessageBox.Show("Невозможно удалить запись, так как она используется в других справочниках базы данных.",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.None);

                }
                PlacesDataGrid.Focus();
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
                    PlacesDataGrid.ItemsSource = SourceCore.DB.PLACES.Where(filtercase => filtercase.HALLS.hall_id.ToString().Contains(textbox.Text)).ToList();
                    break;
                case 1:
                    PlacesDataGrid.ItemsSource = SourceCore.DB.PLACES.Where(filtercase => filtercase.row.ToString().Contains(textbox.Text)).ToList();
                    break;
                case 2:
                    PlacesDataGrid.ItemsSource = SourceCore.DB.PLACES.Where(filtercase => filtercase.place.ToString().Contains(textbox.Text)).ToList();
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

        private void CloseEdChangeClick(object sender, RoutedEventArgs e)
        {
            ChangeColumn.Width = new GridLength(0);
            SplitterColumn.Width = new GridLength(0);
            PlacesDataGrid.IsHitTestVisible = true;
        }

        private void CommitButtonPlace(object sender, RoutedEventArgs e)
        {
            if (RowTextBox.Text.Trim() == null || RowTextBox.Text.Trim() == "")
            {
                MessageBox.Show("Поле ряд не может быть пустым!",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.None);
                return;
            }
            else if (PlaceTextBox.Text.Trim() == null || PlaceTextBox.Text.Trim() == "")
            {
                MessageBox.Show("Поле место не может быть пустым!",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.None);
                return;
            }
            else if (HallComboBox.SelectedItem == null)
            {
                MessageBox.Show("Поле зал не может быть пустым!",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.None);
                return;
            }
            var NewPlace = new Data.PLACES
            {
                HALLS = (Data.HALLS)HallComboBox.SelectedItem,
                row = int.Parse(RowTextBox.Text),
                place = int.Parse(PlaceTextBox.Text)
            };
            if (PlacesDataGrid.SelectedItem == null)
            {
                SourceCore.DB.PLACES.Add(NewPlace);
            }
            SourceCore.DB.SaveChanges();
            UpdateGrid(NewPlace);
            CloseEdChangeClick(sender, e);
        }

        public void UpdateGrid(Data.PLACES places)
        {
            if ((places == null) && (PlacesDataGrid.ItemsSource != null))
            {
                places = (Data.PLACES)PlacesDataGrid.SelectedItem;
            }
            PlacesDataGrid.ItemsSource = SourceCore.DB.PLACES.ToList();
            PlacesDataGrid.SelectedItem = places;
        }
    }
}
