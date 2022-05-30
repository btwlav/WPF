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
    /// <summary>
    /// Interaction logic for SessionsPage.xaml
    /// </summary>
    public partial class SessionsPage : Page
    {
//abc
        int film;
        int hall_id;
        DateTime? date;
        TimeSpan time;
        decimal ticket_price;

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
            get { return (SourceCore.DB.SESSIONS.Count() - 1) / BlockRecordsCount + 1; }
        }

        public SessionsPage()
        {
            InitializeComponent();
            DataContext = this;
            SessionsDataGrid.ItemsSource = SourceCore.DB.SESSIONS.Where(P => P.session_id >= 0).OrderBy(P => P.FILMS.film_name).Skip((BlockNum - 1) * BlockRecordsCount).Take(BlockRecordsCount).ToList();
            FilmComboBox.ItemsSource = SourceCore.DB.FILMS.ToList();
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
            for (int I = 0; I < 5; I++)
            {
                Columns.Add(SessionsDataGrid.Columns[I].Header.ToString());
            }
            FilterComboBox.ItemsSource = Columns;
            FilterComboBox.SelectedIndex = 0;
            // Запрет на управление сортировкой щелчком по заголовку столбца
            foreach (DataGridColumn Column in SessionsDataGrid.Columns)
            {
                Column.CanUserSort = false;
            }
        }

        private void DeleteSessionButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Удалить запись?", "Внимание!", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
            {
                try
                {
                    Data.SESSIONS DeletingSession = (Data.SESSIONS)SessionsDataGrid.SelectedItem;
                    if (SessionsDataGrid.SelectedIndex < SessionsDataGrid.Items.Count - 1)
                    {
                        SessionsDataGrid.SelectedIndex++;
                    }
                    else
                    {
                        if (SessionsDataGrid.SelectedIndex > 0)
                        {
                            SessionsDataGrid.SelectedIndex--;
                        }
                    }
                    SessionsDataGrid.SelectedItem = DeletingSession;
                    SourceCore.DB.SESSIONS.Remove(DeletingSession);
                    SourceCore.DB.TICKETS.RemoveRange(SourceCore.DB.TICKETS.Where(T => T.session_id == DeletingSession.session_id));
                    SourceCore.DB.SaveChanges();
                    UpdateGrid((Data.SESSIONS)SessionsDataGrid.SelectedItem);
                }
                catch
                {
                    MessageBox.Show("Невозможно удалить запись, так как она используется в других справочниках базы данных.",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.None);

                }
                SessionsDataGrid.Focus();
            }
        }

        private void FilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textbox = sender as TextBox;
            switch (FilterComboBox.SelectedIndex)
            {
                case 0:
                    SessionsDataGrid.ItemsSource = SourceCore.DB.SESSIONS.Where(filtercase => filtercase.FILMS.film_name.ToString().Contains(textbox.Text)).ToList();
                    break;
                case 1:
                    SessionsDataGrid.ItemsSource = SourceCore.DB.SESSIONS.Where(filtercase => filtercase.HALLS.hall_id.ToString().Contains(textbox.Text)).ToList();
                    break;
                case 2:
                    SessionsDataGrid.ItemsSource = SourceCore.DB.SESSIONS.Where(filtercase => filtercase.session_date.ToString().Contains(textbox.Text)).ToList();
                    break;
                case 3:
                    SessionsDataGrid.ItemsSource = SourceCore.DB.SESSIONS.Where(filtercase => filtercase.session_time.ToString().Contains(textbox.Text)).ToList();
                    break;
                case 4:
                    SessionsDataGrid.ItemsSource = SourceCore.DB.SESSIONS.Where(filtercase => filtercase.ticket_price.ToString().Contains(textbox.Text)).ToList();
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
            SessionsDataGrid.IsHitTestVisible = true;
        }

        private void CommitButtonSession(object sender, RoutedEventArgs e)
        {
            Regex regex = new Regex("^\\d{2}[-:]\\d{2}[-:]\\d{2}$");
            if (HallComboBox.SelectedItem == null)
            {
                MessageBox.Show("Поле зал не может быть пустым!",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.None);
                return;
            } else if (FilmComboBox.SelectedItem == null)
            {
                MessageBox.Show("Поле фильм не может быть пустым!",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.None);
                return;
            } else if (!regex.IsMatch(TimeTextBox.Text))
            {
                MessageBox.Show("Введено некорректное время начала сеанса!",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.None);
                return;
            }
            else if (SessionsDataGrid.SelectedItem == null && SourceCore.DB.SESSIONS.AsEnumerable().Any(S => HallComboBox.SelectedIndex + 1 == S.hall_id && S.session_date == DateTextBox.SelectedDate && TimeSpan.Parse(TimeTextBox.Text) < (S.session_time + S.FILMS.film_duration + TimeSpan.Parse("00:10:00"))))
            {
                MessageBox.Show("На данную дату и время уже существует сеанс!",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.None);
                return;
            } else if (PriceTextBox.Text.Trim() == null || PriceTextBox.Text.Trim() == "")
            {
                MessageBox.Show("Поле цена билета не может быть пустым!",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.None);
                return;
            } else
            {
                decimal price_boof;
                try
                {
                    price_boof = decimal.Parse(PriceTextBox.Text);
                }
                catch (Exception)
                {
                    string z = PriceTextBox.Text.Replace('.', ',');
                    price_boof = decimal.Parse(z);
                }
                var NewSession = new Data.SESSIONS
                {
                    FILMS = (Data.FILMS)FilmComboBox.SelectedItem,
                    HALLS = (Data.HALLS)HallComboBox.SelectedItem,
                    session_date = DateTextBox.SelectedDate,
                    session_time = TimeSpan.Parse(TimeTextBox.Text),
                    ticket_price = price_boof
                };
                if (SessionsDataGrid.SelectedItem == null)
                {
                    SourceCore.DB.SESSIONS.Add(NewSession);
                    for (int i = 1; i < 4; i++)
                    {
                        for (int j = 1; j <= 10; j++)
                        {
                            SourceCore.DB.TICKETS.Add(new Data.TICKETS
                            {
                                release_date = NewSession.session_date,
                                SESSIONS = NewSession,
                                PLACES = SourceCore.DB.PLACES.Find(SourceCore.DB.PLACES.First(P => P.hall_id == NewSession.hall_id && P.row == i && P.place == j).place_id),
                                sold_or_not = "Не продан"
                            });
                        }
                    }
                }
                SourceCore.DB.SaveChanges();
                UpdateGrid(NewSession);
                UpdateGridTickets(null);
                CloseEdChangeClick(sender, e);
            }
        }

        public void UpdateGrid(Data.SESSIONS sessions)
        {
            if ((sessions == null) && (SessionsDataGrid.ItemsSource != null))
            {
                sessions = (Data.SESSIONS)SessionsDataGrid.SelectedItem;
            }
            SessionsDataGrid.ItemsSource = SourceCore.DB.SESSIONS.Where(S => S.session_id >= 0).OrderBy(S => S.session_date).OrderBy(S => S.session_time).Skip((BlockNum - 1) * BlockRecordsCount).Take(BlockRecordsCount).ToList();
            SessionsDataGrid.SelectedItem = sessions;
        }

        public void UpdateGridTickets(Data.TICKETS tickets)
        {
            if ((tickets == null) && (TicketsDataGrid.ItemsSource != null))
            {
                tickets = (Data.TICKETS)TicketsDataGrid.SelectedItem;
            }
            TicketsDataGrid.ItemsSource = SourceCore.DB.TICKETS.ToList();
            TicketsDataGrid.SelectedItem = tickets;
        }

        private void ShowButtonSessions(object sender, RoutedEventArgs e)
        {
            if (ChangeColumn.Width.Value == 0)
            {
                ChangeColumn.Width = new GridLength(250);
                SplitterColumn1.Width = GridLength.Auto;
                if ((sender as Button).Content.ToString() == "Добавить")
                {
                    SessionsDataGrid.SelectedItem = null;
                    FilmComboBox.SelectedIndex = 0;
                    HallComboBox.SelectedIndex = 0;
                }
                if (((sender as Button).Content.ToString() == "Копировать") && (SessionsDataGrid.SelectedItem != null))
                {
                    decimal price_boof;
                    try
                    {
                        price_boof = decimal.Parse(PriceTextBox.Text);
                    }
                    catch (Exception)
                    {
                        string z = PriceTextBox.Text.Replace('.', ',');
                        price_boof = decimal.Parse(z);
                    }
                    film = FilmComboBox.SelectedIndex;
                    hall_id = HallComboBox.SelectedIndex;
                    date = DateTextBox.SelectedDate;
                    time = TimeSpan.Parse(TimeTextBox.Text);
                    ticket_price = price_boof;
                    SessionsDataGrid.SelectedItem = null;

                    FilmComboBox.SelectedIndex = film;
                    HallComboBox.SelectedIndex = hall_id;
                    DateTextBox.SelectedDate = date;
                    TimeTextBox.Text = time.ToString();
                    PriceTextBox.Text = ticket_price.ToString();
                    SessionsDataGrid.IsHitTestVisible = false;
                }
            }
            else
            {
                ChangeColumn.Width = new GridLength(0);
                SplitterColumn1.Width = new GridLength(0);
            }
        }

        private void PriceTextBox_KeyPress(object sender, KeyEventArgs e)
        {
            if (e.Key >= Key.D0 && e.Key <= Key.Z)
            {
                if (e.Key != Key.Back)
                {
                    e.Handled = true;
                }
            }
        }

        private void Grid_BegginingEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            e.Cancel = true;
        }

        private void BuyTicket(object sender, RoutedEventArgs e)
        {
            Data.TICKETS boughtTicket = (Data.TICKETS)TicketsDataGrid.SelectedItem;
            if (boughtTicket != null && boughtTicket.sold_or_not == "Не продан")
            {
                boughtTicket.sold_or_not = "Продан";
                SourceCore.DB.SaveChanges();
                UpdateGrid(null);
                UpdateGridTickets(boughtTicket);
            }
        }

        private void DeleteTicket(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Delete) && TicketsDataGrid.SelectedItem != null)
            {
                if (MessageBox.Show("Удалить запись?", "Внимание!", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
                {
                    try
                    {
                        Data.TICKETS DeletingTicket = (Data.TICKETS)TicketsDataGrid.SelectedItem;
                        if (TicketsDataGrid.SelectedIndex < TicketsDataGrid.Items.Count - 1)
                        {
                            TicketsDataGrid.SelectedIndex++;
                        }
                        else
                        {
                            if (TicketsDataGrid.SelectedIndex > 0)
                            {
                                TicketsDataGrid.SelectedIndex--;
                            }
                        }
                        TicketsDataGrid.SelectedItem = DeletingTicket;
                        SourceCore.DB.TICKETS.Remove(DeletingTicket);
                        SourceCore.DB.SaveChanges();
                        UpdateGrid((Data.SESSIONS)TicketsDataGrid.SelectedItem);
                    }
                    catch
                    {
                        MessageBox.Show("Невозможно удалить запись, так как она используется в других справочниках базы данных.",
                        "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.None);

                    }
                    TicketsDataGrid.Focus();
                }
            }
        }
    }
}
