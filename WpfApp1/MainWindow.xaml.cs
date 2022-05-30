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

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int CurrentPageIndex;

        // Список активных основных страниц (не диалогов!), открытых в окне приложения
        private List<Page> ActivePages;

        public MainWindow()
        {
            InitializeComponent();
            ActivePages = new List<Page>();
            CurrentPageIndex = -1;
        }

        // Метод поиска активной страницы в окне приложения по заданному типу страницы
        private int GetActivePageIndexByType(Type PageType) {
            int Index = ActivePages.Count - 1;
            while ((Index >= 0) && (ActivePages[Index].GetType() != PageType))
            {
                Index--;
            }
            return Index;
        }

        private void ShowPage(Type PageType)
        {
            Page Page;
            if (PageType != null)
            {
                // Поиск страницы заданного типа среди активных страниц приложения
                int Index = GetActivePageIndexByType(PageType);
                if (Index < 0)
                {
                    // Если страница не найдена среди активных, создание страницы 
                    // заданного типа при помощи метода класса Activator. Последние два 
                    // параметра передаются на вход конструктора страницы
                    Page = (Page)Activator.CreateInstance(PageType);
                    // Добавление созданной страницы в список активных
                    ActivePages.Add(Page);
                    CurrentPageIndex = ActivePages.Count - 1;
                }
                else
                {
                    // Переход к существующей активной странице
                    Page = ActivePages[Index];
                    CurrentPageIndex = Index;
                }
            }
            else
            {
                // Подготовка параметра для метода Navigate компонента RootFrame,
                // чтобы он "закрыл" текущую активную страницу
                Page = null;
            }
            // Отображение в фрейме текущей выбранной страницы
            RootFrame.Navigate(Page);
        }

        private void CountriesButton_Click(object sender, RoutedEventArgs e)
        {
            ShowPage(typeof(Pages.CountriesPage));
        }

        private void StylesButton_Click(object sender, RoutedEventArgs e)
        {
            ShowPage(typeof(Pages.StylesPage));
        }

        private void FilmsButton_Click(object sender, RoutedEventArgs e)
        {
            ShowPage(typeof(Pages.FilmsPage));
        }

        private void HallsButton_Click(object sender, RoutedEventArgs e)
        {
            ShowPage(typeof(Pages.HallsPage));
        }

        private void PlacesButton_Click(object sender, RoutedEventArgs e)
        {
            ShowPage(typeof(Pages.PlacesPage));
        }

        private void SessionsButton_Click(object sender, RoutedEventArgs e)
        {
            ShowPage(typeof(Pages.SessionsPage));
        }

        private void PreviosPageButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentPageIndex--;
            RootFrame.Navigate(ActivePages[CurrentPageIndex]);
        }

        private void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentPageIndex++;
            RootFrame.Navigate(ActivePages[CurrentPageIndex]);
        }

        private void ClosePageButton_Click(object sender, RoutedEventArgs e)
        {
            Page Page;
            // Непосредственно удаление текущей страницы из списка активных
            // страниц окна приложения
            ActivePages.RemoveAt(CurrentPageIndex);
            // Переход к соседней активной странице, если она существует
            if (CurrentPageIndex > 0)
            {
                CurrentPageIndex--;
                Page = ActivePages[CurrentPageIndex];
            }
            else
            {
                if (CurrentPageIndex < ActivePages.Count)
                {
                    Page = ActivePages[CurrentPageIndex];
                }
                else
                {
                    Page = null;
                }
            }
            // Отображение в фрейме новой выбранной страницы
            RootFrame.Navigate(Page);
        }

        private void RootFrame_LoadCompleted(object sender, NavigationEventArgs e)
        {
            while (RootFrame.CanGoBack)
            {
                RootFrame.RemoveBackEntry();
            }
            SetControlsEnabled();
        }

        // Метод установки доступности элементов управления интерфейса окна приложения
        private void SetControlsEnabled()
        {
            PreviosPageButton.IsEnabled = (CurrentPageIndex > 0);
            NextPageButton.IsEnabled = (CurrentPageIndex < ActivePages.Count - 1);
            ClosePageButton.IsEnabled = (ActivePages.Count > 0);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetControlsEnabled();
        }
    }
}
