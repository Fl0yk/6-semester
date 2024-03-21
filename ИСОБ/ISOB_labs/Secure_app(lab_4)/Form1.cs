using System.Text.RegularExpressions;
using System.Text;
using System.Security.Cryptography;

namespace Secure_app_lab_4_
{
    public partial class Form1 : Form
    {
        private enum Roles
        {
            Admin,
            Vip,
            Peasant
        }

        private class DefenseSystemSwitcher
        {
            public bool PrivilegeMinimization;
            public bool Buffer;
            public bool Xss;
            public bool Dos;
        }

        private class User
        {
            public string Login;
            public string Password;
            public Roles Role;
            public bool IsSignedIn;
            public int SlotIndex;

            public User(string login, string password)
            {
                Login = login;
                Password = password;
                IsSignedIn = false;
                SlotIndex = -1;
            }

            public override bool Equals(object? obj)
            {
                return obj is User user &&
                       Login == user.Login;
            }
        }

        private List<User> usersData = new();
        private readonly SHA256 sha256 = SHA256.Create();

        public Form1()
        {
            InitializeComponent();

            for (int i = 1; i < 7; i++)
            {
                string user = $"user{i}";
                string password = $"{i}{i + 1}{i + 2}{i + 3}";
                byte[] encryptedUser = ASCIIEncoding.ASCII.GetBytes(user);
                byte[] encryptedPassword = ASCIIEncoding.ASCII.GetBytes(password);

                usersData.Add(new User(user, password));

                this.Controls[$"Login{i}"].Text = user;
                this.Controls[$"Password{i}"].Text = password;

                loginFields.Add(this.Controls[$"Login{i}"] as TextBox);
                passwordsFields.Add(this.Controls[$"Password{i}"] as TextBox);
                messageFields.Add(this.Controls[$"Message{i}"] as TextBox);
                signInButtons.Add(this.Controls[$"SignIn{i}"] as Button);
                sendButtons.Add(this.Controls[$"Send{i}"] as Button);
            }
            usersData[0].Role = Roles.Admin;
            usersData[1].Role = Roles.Admin;
            usersData[2].Role = Roles.Vip;
            usersData[3].Role = Roles.Vip;
            usersData[4].Role = Roles.Vip;
            usersData[5].Role = Roles.Peasant;
        }

        private int GetIndex(Button sender) => Convert.ToInt32((sender).Name.Last().ToString()) - 1;

        private bool GetIsServerLagging(int limit = 3) => usersData.FindAll(user => user.IsSignedIn).Count() >= limit;

        private DefenseSystemSwitcher GetDefenseSystemSwitcher()
        {
            DefenseSystemSwitcher defenseCheks = new();
            bool[] bools = new bool[4];

            foreach (var check in AttackDefensesCheckedListBox.CheckedIndices)
            {
                bools[(int)check] = true;
            }

            defenseCheks.PrivilegeMinimization = bools[0];
            defenseCheks.Buffer = bools[1];
            defenseCheks.Dos = bools[2];
            defenseCheks.Xss = bools[3];

            return defenseCheks;
        }

        private void SignIn_Click(object sender, EventArgs e)
        {
            int index = GetIndex(sender as Button);
            string login = loginFields[index].Text;
            string password = passwordsFields[index].Text;
            User userByName = usersData.Find(user => user.Login == login);
            DefenseSystemSwitcher defense = GetDefenseSystemSwitcher();


            if (index < 0 || index >= usersData.Count)
            {
                return;
            }

            if (GetIsServerLagging())
            {
                if (defense.Dos)
                {
                    MessageBox.Show("Сервер переполнен. Ожидайте своей очереди");
                    return;
                }
                else
                {
                    MessageBox.Show("Сервер лагает, т.к. подвержен высокой нагрузке");
                }
            }

            if (userByName is null)
            {
                MessageBox.Show($"Пользователя с именем {login} не существует", "Ошибка авторизации");
                return;
            }


            if (userByName is not null && userByName.IsSignedIn)
            {
                MessageBox.Show($"Пользователь {login} уже авторизован в слоте {userByName.SlotIndex + 1}", "Ошибка авторизации");
                return;
            }


            if (userByName != null && userByName.Password == password)
            {
                userByName.IsSignedIn = true;
                userByName.SlotIndex = index;
            }

            MessageBox.Show($"Пользователя с именем {login} успешно авторизован", "Авторизация");
        }

        private void Send_Click(object sender, EventArgs e)
        {
            int index = GetIndex(sender as Button);
            string messageText = messageFields[index].Text;
            string answer;
            var userByIndex = usersData.Find(user => user.SlotIndex == index);
            DefenseSystemSwitcher defense = GetDefenseSystemSwitcher();

            if (0 > index || index >= usersData.Count)
            {
                return;
            }

            if (userByIndex == null)
            {
                MessageBox.Show("Для отправки сообщений пользователь должен быть авторизован", "Ошибка пользователя");
                return;
            }

            if (defense.Xss)
            {
                if (!Regex.IsMatch(messageText, @"^[A-z0-9]*$"))
                {
                    MessageBox.Show("Некорректное сообщение. Пожалуйста, используйте только английские буквы", "Ошибка сообщения");
                    return;
                }
            }

            if (defense.PrivilegeMinimization && userByIndex.Role == Roles.Peasant)
            {
                MessageBox.Show("Недостаточно привилегий!", "Ошибка доступа");
                return;
            }

            if (defense.Buffer)
            {
                int size = 10;
                char[] buf = new char[size];

                try
                {
                    messageText.CopyTo(0, buf, 0, messageText.Length);
                    messageText = string.Join("", buf);

                }
                catch (ArgumentOutOfRangeException)
                {
                    MessageBox.Show("Сообщение превысило буфер. Часть даанных записано в соседнюю память", "Переполнение буфера");
                }

                messageText = string.Join("", buf);
            }

            messageText = messageText.Insert(0, $"[{userByIndex.Role}] {userByIndex.Login}: ");

            if (GetIsServerLagging(4))
            {
                Thread.Sleep(300);
            }

            MessagesListBox.Items.Add(messageText);
        }

        private void SignOut_Click(object sender, EventArgs e)
        {
            int index = GetIndex(sender as Button);
            var userByIndex = usersData.Find(user => user.SlotIndex == index);


            if (userByIndex == null)
            {
                MessageBox.Show($"Этот слот пустой, т.к. пользователь не авторизован", "Ошибка выхода");
                return;
            }

            if (userByIndex != null)
            {
                userByIndex.SlotIndex = -1;
                userByIndex.IsSignedIn = false;
                MessageBox.Show($"Пользователь вышел с аккаунта", "Выход");
            }
        }
    }
}
