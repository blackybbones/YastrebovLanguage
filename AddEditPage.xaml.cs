using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace YastrebovLanguage
{
    /// <summary>
    /// Логика взаимодействия для AddEditPage.xaml
    /// </summary>
    public partial class AddEditPage : Page
    {
        private Client currentPeople = new Client();

        public AddEditPage(Client selectedPeople)
        {
            InitializeComponent();

            if (selectedPeople != null)
            {
                currentPeople = selectedPeople;
            }
            DataContext = currentPeople;

            if (currentPeople.GenderCode == "ж")
            {
                FemaleRBtn.IsChecked = true;
            }
            else
            {
                MaleRBtn.IsChecked = true;
            }

            if (currentPeople.ID == 0)
            {
                IDTBlock.Visibility = Visibility.Hidden;
                IDTBox.Visibility = Visibility.Hidden;

            }
        }





        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();

            // Проверки ФИО
            if (string.IsNullOrWhiteSpace(currentPeople.LastName))
                errors.AppendLine("Укажите фамилию клиента");
            else if (currentPeople.LastName.Length > 50)
                errors.AppendLine("Фамилия не может превышать 50 символов");
            else if (!Regex.IsMatch(currentPeople.LastName, @"^[a-zA-Zа-яА-ЯёЁ\s-]+$"))
                errors.AppendLine("Фамилия может содержать только буквы, пробелы и дефисы");

            if (string.IsNullOrWhiteSpace(currentPeople.FirstName))
                errors.AppendLine("Укажите имя клиента");
            else if (currentPeople.FirstName.Length > 50)
                errors.AppendLine("Имя не может превышать 50 символов");
            else if (!Regex.IsMatch(currentPeople.FirstName, @"^[a-zA-Zа-яА-ЯёЁ\s-]+$"))
                errors.AppendLine("Имя может содержать только буквы, пробелы и дефисы");

            if (string.IsNullOrWhiteSpace(currentPeople.Patronymic))
                errors.AppendLine("Укажите отчество клиента");
            else if (currentPeople.Patronymic.Length > 50)
                errors.AppendLine("Отчество не может превышать 50 символов");
            else if (!Regex.IsMatch(currentPeople.Patronymic, @"^[a-zA-Zа-яА-ЯёЁ\s-]+$"))
                errors.AppendLine("Отчество может содержать только буквы, пробелы и дефисы");

            // Проверка даты рождения
            DateTime birthDate;
            if (string.IsNullOrWhiteSpace(BirthdayDPicker.Text))
            {
                errors.AppendLine("Укажите дату рождения клиента");
            }
            else if (DateTime.TryParse(BirthdayDPicker.Text, out birthDate))
            {
                if (birthDate > DateTime.Now)
                    errors.AppendLine("Дата рождения не может быть в будущем");
                else if (birthDate < DateTime.Now.AddYears(-150))
                    errors.AppendLine("Проверьте дату рождения - клиент слишком стар");
                else
                    currentPeople.Birthday = birthDate; // Устанавливаем дату только если она валидна
            }
            else
            {
                errors.AppendLine("Некорректный формат даты рождения");
            }

            // Проверка email
            if (string.IsNullOrWhiteSpace(currentPeople.Email))
                errors.AppendLine("Укажите Email!");
            else
            {
                string pattern = @"^[^@\s]+@[^@\s]+\.[a-zA-Z]{2,}$";
                if (!Regex.IsMatch(currentPeople.Email, pattern))
                    errors.AppendLine("Укажите правильный Email!");

            }

            // Проверка телефона
            if (string.IsNullOrWhiteSpace(currentPeople.Phone))
            {
                errors.AppendLine("Укажите номер клиента");
            }
            else
            {
                // Очищаем номер и сохраняем очищенную версию
                string cleanedNumber = Regex.Replace(currentPeople.Phone, @"[^\d+]", "");
                currentPeople.Phone = cleanedNumber; // Сохраняем очищенный номер

                if (!Regex.IsMatch(cleanedNumber, @"^(\+7|7|8)?[\d]{10}$"))
                {
                    errors.AppendLine("Укажите корректный телефон клиента");
                }
            }


            if (FemaleRBtn.IsChecked == true)
            {
                currentPeople.GenderCode = "ж";
            }
            else
            {
                currentPeople.GenderCode = "м";
            }

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString());
                return;
            }

            // Убедимся, что дата установлена перед сохранением
            if (currentPeople.Birthday == null && !string.IsNullOrWhiteSpace(BirthdayDPicker.Text))
            {
                if (DateTime.TryParse(BirthdayDPicker.Text, out birthDate))
                {
                    currentPeople.Birthday = birthDate;
                }
            }

            if (currentPeople.ID == 0)
            {
                YastrebovLanguageEntities.GetContext().Client.Add(currentPeople);
            }

            try
            {
                YastrebovLanguageEntities.GetContext().SaveChanges();
                MessageBox.Show("Информация сохранена");

                Manager.MainFrame.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }

        }

        private void ChangePhoto_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog myOpenFileDialog = new OpenFileDialog();

            // Получаем путь к папке "Клиенты" в папке проекта.
            string solutionPath = System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)));

            string clientsFolderPath = System.IO.Path.Combine(solutionPath, "Клиенты");

                        // Устанавливаем начальную директорию OpenFileDialog.
            myOpenFileDialog.InitialDirectory = clientsFolderPath;

            if (myOpenFileDialog.ShowDialog() == true)
            {
                string fileName = System.IO.Path.GetFileName(myOpenFileDialog.FileName);

                // Используем относительный путь относительно папки "Клиенты"
                string baseDirectory = "Клиенты";
                currentPeople.PhotoPath = System.IO.Path.Combine(baseDirectory, fileName);

                PhotoPeople.Source = new BitmapImage(new Uri(myOpenFileDialog.FileName));
            }
        }



    }
}
