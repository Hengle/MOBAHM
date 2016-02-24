using FireSharp;
using FireSharp.Config;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Win32.Enums;

namespace mobahm_console
{
    public class Program
    {
        private static readonly int MAX_WIDTH = 80;
        private static readonly int MAX_HEIGHT = 40;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindow(IntPtr hWnd, ShowWindowCommands nCmdShow);

        public static void Main(string[] args)
        {
            var program = new Program();
            program.Start();
            while (program.Loop()) ;

            Console.ReadKey();
        }

        private class User
        {
            public string Account { get; private set; }
            public string Name { get; }
            public int Experience { get; }
            public int Money { get; }
            public string Password { get; private set; }

            public User(string Account) : this(Account, string.Empty, -1, -1)
            {

            }
            public User(string Account, string Name, int Experience, int Money)
            {
                this.Account = Account;
                this.Name = Name;
                this.Experience = Experience;
                this.Money = Money;
                this.Password = string.Empty;
            }

            public User SetAccount(string Account)
            {
                this.Account = Account;
                return this;
            }
            public User SetPassword(string Password)
            {
                this.Password = GetPassword(this.Account, Password);
                return this;
            }

            private string GetPassword(string x, string y)
            {
                var md5 = MD5.Create();
                var bs = md5.ComputeHash(Encoding.UTF8.GetBytes(string.Format("{0}:{1}", x, y)));
                return string.Join("", bs.Select(z => string.Format("{0:x02}", z)));
            }
        }
        private enum UserState
        {
            LogOff, LogOn
        }

        //private User player = new User("test").SetPassword("test");
        //private User player = new User("r");
        private User player = new User(string.Empty);

        private void Start()
        {
            ShowWindow(Process.GetCurrentProcess().MainWindowHandle,
                ShowWindowCommands.SW_SHOWMAXIMIZED | ShowWindowCommands.SW_MAXIMIZE);

            /*
            var config = new FirebaseConfig()
            {
                BasePath = "https://brilliant-torch-522.firebaseio.com/",
                AuthSecret = "WUX7MzRBwzKD0yC8brqEuMWtjgYg3zSbewCQMhTa",
            };
            var client = new FirebaseClient(config);
            
            var user = client.Get(string.Format("users/{0}", player.Account)).ResultAs<JObject>();
            if (user == null)
            {
                Console.WriteLine("not found");
            }
            else if (user.Value<string>("password").Equals(player.Password))
            {
                player.Name = user.Value<string>("name");
                player.Experience = user.Value<int>("experience");
                player.Money = user.Value<int>("money");
            }
            else
            {
                Console.WriteLine("password invalid");
            }
            */
            if (File.Exists("credentials.json"))
            {
                var credentials = JObject.Parse(File.ReadAllText("credentials.json"));
                JToken token = null;
                Console.WriteLine(credentials.TryGetValue("account", out token));
                Console.WriteLine(token);
                Console.WriteLine(credentials.Value<string>("password"));
                Console.WriteLine(credentials.TryGetValue("password", out token));
                Console.WriteLine(token);
                Console.WriteLine(token == null);
                Console.WriteLine(credentials["password"]);
                Console.WriteLine(credentials["password"] == null);
                Console.ReadKey();
            }
            else
            {

            }
        }
        private bool Loop()
        {
            Console.WriteLine("loop");

            /*
            UI();

            var cki = Console.ReadKey(true);
            if (cki.Key == ConsoleKey.Q)
            {
                return false;
            }
            */
            if (UI() && Console.ReadKey(true).Key == ConsoleKey.Q)
            {
                return false;
            }

            return true;
        }

        private bool UI()
        {
            Console.Clear();

            var state = player.Name.Equals(string.Empty) ? UserState.LogOff : UserState.LogOn;

            UITop(state);
            UICenter(state);

            return true;
        }
        private enum Alignment
        {
            Left, Right
        }
        private string FullLine(object o)
        {
            string s = Convert.ToString(o);
            int padding = Encoding.Default.GetByteCount(s) - s.Length;
            return string.Format(string.Format("{{0,{0}}}", MAX_WIDTH - padding), s);
        }
        private string FillLine(object o)
        {
            string s = Convert.ToString(o);
            return string.Format("{0}{1}", s, new string(' ', MAX_WIDTH - Encoding.Default.GetByteCount(s)));
        }
        private void UITop(UserState state)
        {
            Console.SetCursorPosition(0, 0);

            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(new string('=', MAX_WIDTH));

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(FullLine(string.Format("v{0} ", Assembly.GetExecutingAssembly().GetName().Version)));

            string name = "Multiplayer Online Battle Arena Heroes Manager";
            Console.Write(new string(' ', (MAX_WIDTH - name.Length) / 2 - 1));

            Console.BackgroundColor = ConsoleColor.Cyan;
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.Write(string.Format(" {0} ", name));

            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine(new string(' ', (MAX_WIDTH - name.Length) / 2 - 1));

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(FullLine("made by R "));

            string user = string.Empty;
            if (state == UserState.LogOn)
            {
                //user = string.Format(" [ {0}({1}) 접속 ]", player.Name, player.Account);
                user = $" [ {player.Name}({player.Account}) 접속 ]";
            }
            var now = DateTime.Now;
            string latest = string.Format("최근 갱신 시간 : {0:00}:{1:00}:{2:00} ", now.Hour, now.Minute, now.Second);
            Console.Write(user);
            int padding = Encoding.Default.GetByteCount(user) + (Encoding.Default.GetByteCount(latest) - latest.Length);
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.ForegroundColor = ConsoleColor.White;
            //Console.WriteLine(string.Format(string.Format("{{0,{0}}}", MAX_WIDTH - padding), latest));
            Console.WriteLine(FullLine(latest));

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(new string('=', MAX_WIDTH));
        }
        private void UICenter(UserState state)
        {
            switch(state)
            {
                case UserState.LogOff: UILogOff(); break;
                case UserState.LogOn: UILogOn(); break;
            }
        }
        private void UILogOff()
        {
            Console.SetCursorPosition(0, 7);

            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("1. 접속하기");
            Console.WriteLine("2. 가입하기");
            Console.WriteLine("Q. 종료하기");

            switch (Console.ReadKey(true).Key)
            {
                case ConsoleKey.D1: UILogIn(); break;
                case ConsoleKey.D2: UIRegister(); break;
                case ConsoleKey.Q: break;
                default: UILogOff(); break;
            }
        }
        private void UIRegister()
        {

        }
        private void UILogIn()
        {
            Console.SetCursorPosition(0, 7);

            Console.WriteLine(FillLine(""));
            Console.WriteLine(FillLine(""));
            Console.WriteLine(FillLine(""));

            Console.SetCursorPosition(0, 7);

            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("   [ 로그인 ]");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("    계정 이름 : ");
            Console.ForegroundColor = ConsoleColor.White;
            string account = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("    비밀 번호 : ");
            Console.ForegroundColor = ConsoleColor.White;
            //string password = Console.ReadLine();
            string password = string.Empty;
            var sb = new StringBuilder();
            //char c = '\0';
            var cki = new ConsoleKeyInfo();
            //while ((c = Console.ReadKey(true).KeyChar) != '\r')
            while ((cki = Console.ReadKey(true)).Key != ConsoleKey.Enter)
            {
                //sb.Append(c);
                sb.Append(cki.KeyChar);
                Console.Write('*');
            }
            password = Convert.ToString(sb);
            Console.WriteLine();

            player.SetAccount(account).SetPassword(password);

            var config = new FirebaseConfig()
            {
                BasePath = "https://brilliant-torch-522.firebaseio.com/",
                AuthSecret = "WUX7MzRBwzKD0yC8brqEuMWtjgYg3zSbewCQMhTa",
            };
            var client = new FirebaseClient(config);

            var user = client.Get(string.Format("users/{0}", player.Account)).ResultAs<JObject>();
            if (user == null)
            {
                Console.WriteLine("not found");
            }
            else if (user.Value<string>("password").Equals(player.Password))
            {
                player = new User(player.Account,
                    user.Value<string>("name"),
                    user.Value<int>("experience"),
                    user.Value<int>("money"));
            }
            else
            {
                Console.WriteLine("password invalid");
            }
        }
        private void UILogOn()
        {
            Console.SetCursorPosition(0, 7);

            Console.BackgroundColor = ConsoleColor.DarkGray;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(FillLine(" [공지] 공지 테스트"));

            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();
            Console.WriteLine("  1. test1");
            Console.WriteLine("  2. test2");
            Console.WriteLine("  3. test3");
            Console.WriteLine("  4. test4");
        }
    }
}
