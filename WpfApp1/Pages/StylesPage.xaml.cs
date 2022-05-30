using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for StylesPage.xaml
    /// </summary>
    public partial class StylesPage : Page
    {
        string style;

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
            get { return (SourceCore.DB.STYLES.Count() - 1) / BlockRecordsCount + 1; }
        }

        public StylesPage()
        {
            InitializeComponent();
            DataContext = this;
            StylesDataGrid.ItemsSource = SourceCore.DB.STYLES.Where(P => P.style_id >= 0)
                .OrderBy(P => P.style)
                .Skip((BlockNum - 1) * BlockRecordsCount)
                .Take(BlockRecordsCount).ToList();
            UpdateGrid(null);
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
                Columns.Add(StylesDataGrid.Columns[0].Header.ToString());
            //}
            FilterComboBox.ItemsSource = Columns;
            FilterComboBox.SelectedIndex = 0;
            // Запрет на управление сортировкой щелчком по заголовку столбца
            foreach (DataGridColumn Column in StylesDataGrid.Columns)
            {
                Column.CanUserSort = false;
            }
        }

        private void DeleteStyleButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Удалить запись?", "Внимание!", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
            {
                try
                {
                    Data.STYLES DeletingStyle = (Data.STYLES)StylesDataGrid.SelectedItem;
                    if (StylesDataGrid.SelectedIndex < StylesDataGrid.Items.Count - 1)
                    {
                        StylesDataGrid.SelectedIndex++;
                    }
                    else
                    {
                        if (StylesDataGrid.SelectedIndex > 0)
                        {
                            StylesDataGrid.SelectedIndex--;
                        }
                    }
                    StylesDataGrid.SelectedItem = DeletingStyle;
                    SourceCore.DB.STYLES.Remove(DeletingStyle);
                    SourceCore.DB.SaveChanges();
                    UpdateGrid((Data.STYLES)StylesDataGrid.SelectedItem);
                }   
                catch
                {
                    MessageBox.Show("Невозможно удалить запись, так как она используется в других справочниках базы данных.",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.None);

                }
                StylesDataGrid.Focus();
            }
        }

        private void FilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textbox = sender as TextBox;
            switch (FilterComboBox.SelectedIndex)
            {
                case 0:
                    StylesDataGrid.ItemsSource = SourceCore.DB.STYLES.Where(filtercase => filtercase.style.ToString().Contains(textbox.Text))
                        .OrderBy(S => S.style).Skip((BlockNum - 1) * BlockRecordsCount).Take(BlockRecordsCount).ToList();
                    break;
            }

            
        }

        // Метод перехода к первому блоку данных в таблице
        private void FirstBlockButton_Click(object sender, RoutedEventArgs e)
        {
            BlockNum = 1;
        }

        // Метод перехода к предыдущему блоку данных в таблице
        private void PreviosBlockButton_Click(object sender, RoutedEventArgs e)
        {
            BlockNum--;
        }

        // Метод перехода к следующему блоку данных в таблице
        private void NextBlockButton_Click(object sender, RoutedEventArgs e)
        {
            BlockNum++;
        }

        // Метод перехода к последнему блоку данных в таблице
        private void LastBlockButton_Click(object sender, RoutedEventArgs e)
        {
            BlockNum = BlockCount;
        }


        private void ShowButtonStyles(object sender, RoutedEventArgs e)
        {
            if (ChangeColumn.Width.Value == 0)
            {
                ChangeColumn.Width = new GridLength(250);
                SplitterColumn.Width = GridLength.Auto;
                if ((sender as Button).Content.ToString() == "Добавить")
                {
                    StylesDataGrid.SelectedItem = null;
                }
                if (((sender as Button).Content.ToString() == "Копировать") && (StylesDataGrid.SelectedItem != null))
                {
                    style = StyleNameTextBox.Text;
                    StylesDataGrid.SelectedItem = null;
                    StyleNameTextBox.Text = style;
                    StylesDataGrid.IsHitTestVisible = false;
                }
            }
            else
            {
                ChangeColumn.Width = new GridLength(0);
                SplitterColumn.Width = new GridLength(0);
            }
        }

        private void DoubleClick(object sender, RoutedEventArgs e)
        {
            if (EditStyleButton.IsEnabled)
            {
                
            }
        }

        private void CommitButtonStyles(object sender, RoutedEventArgs e)
        {
            if (StyleNameTextBox.Text.Trim() == null || StyleNameTextBox.Text.Trim() == "")
            {
                MessageBox.Show("Поле название жанра не может быть пустым!",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.None);
                return;
            }
            var NewStyle = new Data.STYLES
            {
                style = StyleNameTextBox.Text
            };
            if (StylesDataGrid.SelectedItem == null)
            {
                SourceCore.DB.STYLES.Add(NewStyle);
            }
            SourceCore.DB.SaveChanges();
            UpdateGrid(NewStyle);
            CloseEdChangeClick(sender, e);
        }

        private void CloseEdChangeClick(object sender, RoutedEventArgs e)
        {
            ChangeColumn.Width = new GridLength(0);
            SplitterColumn.Width = new GridLength(0);
            StylesDataGrid.IsHitTestVisible = true;
        }

        public void UpdateGrid(Data.STYLES styles)
        {
            if ((styles == null) && (StylesDataGrid.ItemsSource != null))
            {
                styles = (Data.STYLES)StylesDataGrid.SelectedItem;
            }
            StylesDataGrid.ItemsSource = SourceCore.DB.STYLES.Where(P => P.style_id >= 0).OrderBy(P => P.style).Skip((BlockNum - 1) * BlockRecordsCount).Take(BlockRecordsCount).ToList();
            StylesDataGrid.SelectedItem = styles;
        }

    }
}
