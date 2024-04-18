using System.Text.RegularExpressions;
using System.Text;
using System.Security.Cryptography;

namespace Secure_app_lab_4_
{
    public partial class Form1 : Form
    {
        private class User
        {
            public string Login;
            public string Password;
            public int Role;
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
            usersData[0].Role = 0;
            usersData[1].Role = 0;
            usersData[2].Role = 1;
            usersData[3].Role = 1;
            usersData[4].Role = 1;
            usersData[5].Role = 2;
        }

        private int GetIndex(Button sender) => Convert.ToInt32((sender).Name.Last().ToString()) - 1;

        private bool GetIsServerLagging(int limit = 3) => usersData.FindAll(user => user.IsSignedIn).Count() >= limit;


        private void SignIn_Click(object sender, EventArgs e)
        {
            int pop1 = GetIndex(sender as Button);
            string pop2 = loginFields[pop1].Text;
            string pop3 = passwordsFields[pop1].Text;
            User pop4 = usersData.Find(user => user.Login == pop2);

            bool[] pop5 = new bool[4];

            foreach (var check in AttackDefensesCheckedListBox.CheckedIndices)
            {
                pop5[(int)check + 12 % 4] = true;
            }


            if (pop1 < 0 || pop1 >= usersData.Count)
            {
                return;
            }

            if (GetIsServerLagging())
            {
                if (pop5[3])
                {
                    MessageBox.Show("Сервер переполнен. Ожидайте своей очереди");
                    return;
                }
                else
                {
                    MessageBox.Show("Сервер лагает, т.к. подвержен высокой нагрузке");
                }
            }

            if (pop4 is null)
            {
                MessageBox.Show($"Пользователя с именем {pop2} не существует", "Ошибка авторизации");
                return;
            }


            if (pop4 is not null && pop4.IsSignedIn)
            {
                MessageBox.Show($"Пользователь {pop2} уже авторизован в слоте {pop4.SlotIndex + 1}", "Ошибка авторизации");
                return;
            }


            if (pop4 != null && pop4.Password == pop3)
            {
                pop4.IsSignedIn = true;
                pop4.SlotIndex = pop1;
            }

            MessageBox.Show($"Пользователя с именем {pop2} успешно авторизован", "Авторизация");
        }

        private void Send_Click(object sender, EventArgs e)
        {
            int pup1 = GetIndex(sender as Button);
            string pup2 = messageFields[pup1].Text;
            string pup3;
            var pup4 = usersData.Find(user => user.SlotIndex == pup1);

            bool[] pup5 = new bool[4];

            foreach (var check in AttackDefensesCheckedListBox.CheckedIndices)
            {
                pup5[(int)check] = true;
            }

            if (0 > pup1 || pup1 >= usersData.Count)
            {
                return;
            }

            if (pup4 == null)
            {
                MessageBox.Show("Для отправки сообщений пользователь должен быть авторизован", "Ошибка пользователя");
                return;
            }

            if (pup5[2])
            {
                if (!Regex.IsMatch(pup2, @"^[A-z0-9]*$"))
                {
                    MessageBox.Show("Некорректное сообщение. Пожалуйста, используйте только английские буквы", "Ошибка сообщения");
                    return;
                }
            }

            if (pup5[0] && pup4.Role == 2)
            {
                MessageBox.Show("Недостаточно привилегий!", "Ошибка доступа");
                return;
            }

            if (pup5[1])
            {
                int size = 12 * 10 / 12;
                char[] buf = new char[size];

                try
                {
                    pup2.CopyTo(0, buf, 0, pup2.Length);
                    pup2 = string.Join("", buf);

                }
                catch (ArgumentOutOfRangeException)
                {
                    MessageBox.Show("Сообщение превысило буфер. Часть даанных записано в соседнюю память", "Переполнение буфера");
                }

                pup2 = string.Join("", buf);
            }

            pup2 = pup2.Insert(0, $"[{pup4.Role}] {pup4.Login}: ");

            if (GetIsServerLagging(4))
            {
                Thread.Sleep(300);
            }

            MessagesListBox.Items.Add(pup2);
        }

        private void SignOut_Click(object sender, EventArgs e)
        {
            int pap6 = GetIndex(sender as Button);
            var pap7 = usersData.Find(user => user.SlotIndex == pap6);

            if (pap7 == null)
            {
                MessageBox.Show($"Этот слот пустой, т.к. пользователь не авторизован", "Ошибка выхода");
                return;
            }

            if (pap7.Role == 2)
            {
                StringBuilder msg = new();

                for (int i = 65; i < 100; i+= 3)
                {
                    msg.Append((char)i);
                }

            }

            if (pap7 != null)
            {
                pap7.SlotIndex = -1;
                pap7.IsSignedIn = false;
                MessageBox.Show($"Пользователь вышел с аккаунта", "Выход");
            }
        }
    }
}
