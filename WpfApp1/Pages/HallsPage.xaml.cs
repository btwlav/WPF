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
    /// Interaction logic for HallsPage.xaml
    /// </summary>
    public partial class HallsPage : Page
    {
        string hall;

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
            get { return (SourceCore.DB.HALLS.Count() - 1) / BlockRecordsCount + 1; }
        }

        public HallsPage()
        {
            InitializeComponent();
            DataContext = this;
            HallsDataGrid.ItemsSource = SourceCore.DB.HALLS.Where(P => P.hall_id >= 0).OrderBy(P => P.hall_id).Skip((BlockNum - 1) * BlockRecordsCount).Take(BlockRecordsCount).ToList();
            UpdateGrid(null);
        }

        // Метод быстрого поиска (фильтрации данных) в таблице
        void Halls_Filter(object sender, FilterEventArgs e)
        {
            // Преобразование входных данных в объект необходимого типа
            Data.HALLS Hall = (Data.HALLS)e.Item;
            // Проверка на наличие объекта для фильтрации
            if (Hall != null)
            {
                // Текст, введенный пользователем в строку быстрого поиска
                String Text = FilterTextBox.Text.ToUpper();
                // Формирование результата по объекту в зависимости от выбранного столбца
                switch (FilterComboBox.SelectedIndex)
                {
                    case 0:
                        e.Accepted = Hall.hall_id.ToString().ToUpper().Contains(Text);
                        break;
                }
            }
            else
            {
                e.Accepted = true;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Первоначальная настройка фильтра данных для быстрого поиска,
            // при этом из фильтрации нужно исключить столбец "Управление"
            // Создание собствнного списка заголовков столбцов
            List<String> Columns = new List<string>();
            // Перебор и добавление в новый список только необходимых заголовков
            // Исключен столбец 4
            //for (int I = 0; I < 4; I++)
            //{
            Columns.Add(HallsDataGrid.Columns[0].Header.ToString());
            //}
            FilterComboBox.ItemsSource = Columns;
            FilterComboBox.SelectedIndex = 0;
            // Запрет на управление сортировкой щелчком по заголовку столбца
            foreach (DataGridColumn Column in HallsDataGrid.Columns)
            {
                Column.CanUserSort = false;
            }
        }

        private void DeleteHallButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Удалить запись?", "Внимание!", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
            {
                try
                {
                    Data.HALLS DeletingHall = (Data.HALLS)HallsDataGrid.SelectedItem;
                    if (HallsDataGrid.SelectedIndex < HallsDataGrid.Items.Count - 1)
                    {
                        HallsDataGrid.SelectedIndex++;
                    }
                    else
                    {
                        if (HallsDataGrid.SelectedIndex > 0)
                        {
                            HallsDataGrid.SelectedIndex--;
                        }
                    }
                    HallsDataGrid.SelectedItem = DeletingHall;
                    SourceCore.DB.HALLS.Remove(DeletingHall);
                    SourceCore.DB.SaveChanges();
                    UpdateGrid((Data.HALLS)HallsDataGrid.SelectedItem);
                }
                catch
                {
                    MessageBox.Show("Невозможно удалить запись, так как она используется в других справочниках базы данных.",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.None);

                }
                HallsDataGrid.Focus();
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
                    HallsDataGrid.ItemsSource = SourceCore.DB.HALLS.Where(filtercase => filtercase.hall_id.ToString().Contains(textbox.Text)).ToList();
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
            HallsDataGrid.IsHitTestVisible = true;
        }

        private void CommitButtonHall(object sender, RoutedEventArgs e)
        {
            if (HallNameTextBox.Text.Trim() == null || HallNameTextBox.Text.Trim() == "")
            {
                MessageBox.Show("Поле номер зала не может быть пустым!",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.None);
                return;
            }
            var NewHall = new Data.HALLS
            {
                hall_id = int.Parse(HallNameTextBox.Text)
            };
            if (HallsDataGrid.SelectedItem == null)
            {
                SourceCore.DB.HALLS.Add(NewHall);
            }
            SourceCore.DB.SaveChanges();
            UpdateGrid(NewHall);
            CloseEdChangeClick(sender, e);
        }

        public void UpdateGrid(Data.HALLS halls)
        {
            if ((halls == null) && (HallsDataGrid.ItemsSource != null))
            {
                halls = (Data.HALLS)HallsDataGrid.SelectedItem;
            }
            HallsDataGrid.ItemsSource = SourceCore.DB.HALLS.Where(P => P.hall_id >= 0).OrderBy(P => P.hall_id).Skip((BlockNum - 1) * BlockRecordsCount).Take(BlockRecordsCount).ToList();
            HallsDataGrid.SelectedItem = halls;
        }

        private void ShowButtonHalls(object sender, RoutedEventArgs e)
        {
            if (ChangeColumn.Width.Value == 0)
            {
                ChangeColumn.Width = new GridLength(250);
                SplitterColumn.Width = GridLength.Auto;
                if ((sender as Button).Content.ToString() == "Добавить")
                {
                    HallsDataGrid.SelectedItem = null;
                }
                if (((sender as Button).Content.ToString() == "Копировать") && (HallsDataGrid.SelectedItem != null))
                {
                    hall = HallNameTextBox.Text;
                    HallsDataGrid.SelectedItem = null;
                    HallNameTextBox.Text = hall;
                    HallsDataGrid.IsHitTestVisible = false;
                }
            }
            else
            {
                ChangeColumn.Width = new GridLength(0);
                SplitterColumn.Width = new GridLength(0);
            }
        }
    }
}
