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

namespace YastrebovLanguage
{
    /// <summary>
    /// Логика взаимодействия для ClientPage.xaml
    /// </summary>
    public partial class ClientPage : Page
    {
        int CountRecord;
        int CountPage;
        int CurrentPage = 0;

        List<Client> currentPageList = new List<Client>();
        List<Client> TableList;
        public ClientPage()
        {
            InitializeComponent();
            var currentClients = YastrebovLanguageEntities.GetContext().Client.ToList();
            ClientListView.ItemsSource = currentClients;
            ComboGender.SelectedIndex = 0;
            ComboSort.SelectedIndex = 0;
            int ClientMaxRecords = 0;
            foreach (Client client in currentClients)
                ClientMaxRecords++;
            TBProductCountMaxRecords.Text = ClientMaxRecords.ToString();
            UpdateProducts();
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            var currentService = (sender as Button).DataContext as Client;

            var currentClientServices = YastrebovLanguageEntities.GetContext().ClientService.ToList();
            currentClientServices = currentClientServices.Where(p => p.ClientID == currentService.ID).ToList();

            if (currentClientServices.Count != 0)
                MessageBox.Show("Невозможно выполнить удаление, так как существует запись этого клиента!", "внимание");
            else
            {
                if (MessageBox.Show("Вы точно хотите выполнить удаление?", "Внимание!",
                   MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        YastrebovLanguageEntities.GetContext().Client.Remove(currentService);
                        YastrebovLanguageEntities.GetContext().SaveChanges();

                        ClientListView.ItemsSource = YastrebovLanguageEntities.GetContext().Service.ToList();

                        UpdateProducts();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message.ToString());
                    }
                }
            }
        }
        private void UpdateProducts()
        {
            TableList = YastrebovLanguageEntities.GetContext().Client.ToList();
            ChangePage(0, 0); // Сбрасываем на первую страницу
            if (ComboGender.SelectedIndex == 1)
                TableList = TableList.Where(p => p.GenderName == "Мужской").ToList();
            if (ComboGender.SelectedIndex == 2)
                TableList = TableList.Where(p => p.GenderName == "Женский").ToList();
            string CleanPhoneNumber(string Phone)
            {
                return Phone.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "");
            }
            TableList = TableList.Where(p => p.LastName.ToLower().Contains(TBoxSearch.Text.ToLower()) || p.FirstName.ToLower().Contains(TBoxSearch.Text.ToLower()) || p.Patronymic.ToLower().Contains(TBoxSearch.Text.ToLower()) ||
            CleanPhoneNumber(p.Phone).Contains(CleanPhoneNumber(TBoxSearch.Text)) ||
            p.Email.ToLower().Contains(TBoxSearch.Text.ToLower())).ToList();

            if (ComboSort.SelectedIndex == 1)
                TableList = TableList.OrderBy(p => p.LastName).ToList();
            if (ComboSort.SelectedIndex == 2)
                TableList = TableList.OrderByDescending(p => DateTime.TryParse(p.VisitDate, out var date) ? date : DateTime.MinValue).ToList();
            if (ComboSort.SelectedIndex == 3)
                TableList = TableList.OrderByDescending(p => p.VisitCount).ToList();
            var sum = YastrebovLanguageEntities.GetContext().Client.ToList(); TBProductCountRecords.Text = TableList.Count().ToString();
            TBProductCountMaxRecords.Text = "" + sum.Count();
            ClientListView.ItemsSource = TableList;

            ChangePage(0, 0);
        }
        private void ChangePage(int direction, int? selectedPage)
        {
            currentPageList.Clear();
            CountRecord = TableList.Count;

            // Определяем количество записей на странице
            int itemsPerPage = 10; // по умолчанию
            switch (ComboFilter.SelectedIndex)
            {
                case 0: itemsPerPage = 10; break;
                case 1: itemsPerPage = 50; break;
                case 2: itemsPerPage = 200; break;
                case 3: itemsPerPage = CountRecord; break; // "Все"
            }

            // Вычисляем количество страниц
            CountPage = itemsPerPage > 0 ? (int)Math.Ceiling((double)CountRecord / itemsPerPage) : 1;

            bool ifUpdate = true;
            int min;

            if (selectedPage.HasValue)
            {
                if (selectedPage >= 0 && selectedPage < CountPage)
                {
                    CurrentPage = (int)selectedPage;
                    min = Math.Min(CurrentPage * itemsPerPage + itemsPerPage, CountRecord);
                    for (int i = CurrentPage * itemsPerPage; i < min; i++)
                    {
                        currentPageList.Add(TableList[i]);
                    }
                }
            }
            else
            {
                switch (direction)
                {
                    case 1: // Назад
                        if (CurrentPage > 0)
                        {
                            CurrentPage--;
                            min = Math.Min(CurrentPage * itemsPerPage + itemsPerPage, CountRecord);
                            for (int i = CurrentPage * itemsPerPage; i < min; i++)
                            {
                                currentPageList.Add(TableList[i]);
                            }
                        }
                        else
                        {
                            ifUpdate = false;
                        }
                        break;
                    case 2: // Вперед
                        if (CurrentPage < CountPage - 1)
                        {
                            CurrentPage++;
                            min = Math.Min(CurrentPage * itemsPerPage + itemsPerPage, CountRecord);
                            for (int i = CurrentPage * itemsPerPage; i < min; i++)
                            {
                                currentPageList.Add(TableList[i]);
                            }
                        }
                        else
                        {
                            ifUpdate = false;
                        }
                        break;
                }
            }

            if (ifUpdate)
            {
                // Обновляем список страниц
                PageListBox.Items.Clear();
                for (int i = 1; i <= CountPage; i++)
                {
                    PageListBox.Items.Add(i);
                }
                PageListBox.SelectedIndex = CurrentPage;

              

                // Ключевое исправление - обновляем источник данных ListView
                ClientListView.ItemsSource = null; // Сначала очищаем
                ClientListView.ItemsSource = currentPageList; // Затем устанавливаем новый источник
            }
        }
        private void LeftDirButton_Click(object sender, RoutedEventArgs e)
        {
            ChangePage(1, null);
        }

        private void PageListBox_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ChangePage(0, Convert.ToInt32(PageListBox.SelectedItem.ToString()) - 1);
        }

        private void RightDirButton_Click(object sender, RoutedEventArgs e)
        {
            ChangePage(1, null);
        }

        private void ComboFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ChangePage(0, 0);
        }

        private void ComboGender_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateProducts();
        }

        private void ComboSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateProducts();
        }

        private void TBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateProducts();
        }
    }
}
