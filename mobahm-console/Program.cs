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
    /// <summary>
    ///                 Main()
    ///                   l
    ///                   v
    ///                Program()
    ///                   l
    ///                   v
    ///            Program.Start()
    ///                   l
    ///                   v
    ///           CheckCredentials()
    ///                   l
    ///      l------------l----------l--------------------l
    ///      l                       l                    l
    ///      l (FileNotFound)        l (NoPassword)       | (AllCredentials: Auto Log In)
    ///      l                       l                    l
    ///      `---------   -----------/                    l
    ///                ` /                                l
    ///                 l                                 l
    ///                 v                                 l
    ///             UILogOff()                            l
    ///                 l                                 l
    ///      l-----------------------|                    l
    ///      l                       l                    l
    ///      l                       l--------------------/
    ///      l 2. UIRegister()       l 1. UILogIn()
    ///      l                       l
    ///      v                       v
    ///  Register()                LogIn()
    ///      l                       l
    ///      `---------   -----------/
    ///                ` /
    ///                 l
    ///                 v
    ///           Program.Loop()
    ///                 l
    ///                 v
    ///              UILogOn()
    /// </summary>
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

            //Console.ReadKey();
        }

        private class User
        {
            public UserState State { get; set; }
            public string Account { get; private set; }
            public string Name { get; }
            public int Experience { get; }
            public int Money { get; }
            public string Password { get; private set; }

            public User() : this(string.Empty) { }
            public User(string Account) : this(Account, string.Empty, -1, -1) { }
            public User(string Account, string Name, int Experience, int Money)
            {
                this.State = UserState.LogOff;
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
                using (var sha256 = SHA256.Create())
                {
                    return "".Join(sha256.ComputeHash(Encoding.UTF8.GetBytes($"{x}:{y}")).Select(z => $"{z:x02}"));
                }
            }
        }
        private enum UserState
        {
            LogOff, LogOn
        }
        private User player = new User(string.Empty);

        private void Start()
        {
            /// Make Console UI to Maximize
            ShowWindow(Process.GetCurrentProcess().MainWindowHandle,
                ShowWindowCommands.SW_SHOWMAXIMIZED | ShowWindowCommands.SW_MAXIMIZE);

            switch (CheckCredentials())
            {
                case CredentialsState.FileNotFound:
                case CredentialsState.NoPassword:
                    UILogOff();
                    break;
                case CredentialsState.AllCredentials:
                    break;
            }
        }
        private bool Loop()
        {
            if (Console.ReadKey(true).Key == ConsoleKey.Q)
            {
                return false;
            }
            return true;
        }

        private enum CredentialsState
        {
            FileNotFound, NoPassword, AllCredentials
        }
        private CredentialsState CheckCredentials()
        {
            if (File.Exists("credentials.json"))
            {
                var credentials = JObject.Parse(File.ReadAllText("credentials.json"));
                /*
                JToken token = null;
                if (credentials.TryGetValue("account", out token))
                {
                    string account = token.Value<string>();
                    if (credentials.TryGetValue("hash", out token))
                    {
                        string hash = token.Value<string>();
                        return CredentialsState.AllCredentials;
                    }
                    return CredentialsState.NoPassword;
                }
                /// error: account not found
                return CredentialsState.FileNotFound;
                */
                string account = credentials.Value<string>("account");
                if (account == null)
                {
                    /// error: account not found
                    return CredentialsState.FileNotFound;
                }
                string hash = credentials.Value<string>("hash");
                if (hash == null)
                {
                    return CredentialsState.NoPassword;
                }
                return CredentialsState.AllCredentials;
            }
            return CredentialsState.FileNotFound;
        }

        private void UI()
        {
            switch (player.State)
            {
                case UserState.LogOff:
                    UILogOff();
                    break;
                case UserState.LogOn:
                    UILogOn();
                    break;
            }
        }
        private void UILogOff()
        {
            UITop();
        }
        private void UILogOn()
        {

        }
        private void UITop()
        {
            Console.SetCursorPosition(0, 0);

            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(new string('=', MAX_WIDTH));

            Console.ForegroundColor = ConsoleColor.Yellow;
            string version = $"v{Assembly.GetExecutingAssembly().GetName().Version} ";
            Console.Write(new string(' ', MAX_WIDTH - version.Length()));
            Console.WriteLine(version);

            string name = " Multiplayer Online Battle Arena Heroes Manager ";
            Console.Write(new string(' ', (MAX_WIDTH - name.Length()) / 2));
            Console.BackgroundColor = ConsoleColor.DarkGreen;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(name);
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine(new string(' ', (MAX_WIDTH - name.Length()) / 2));

            Console.ForegroundColor = ConsoleColor.White;
            string author = "made by R ";
            Console.Write(new string(' ', MAX_WIDTH - author.Length()));
            Console.WriteLine(author);

            string user = " [ 로그인이 필요합니다 ]";
            if (player.State == UserState.LogOn)
            {
                user = $" [ {player.Name}({player.Account}) 접속 ]";
            }
            var now = DateTime.Now;
            string latest = $"최근 갱신 시간 : {now.Hour:00}:{now.Minute:00}:{now.Second:00} ";
            Console.Write(user);
            Console.Write(new string(' ', MAX_WIDTH - user.Length() - latest.Length()));
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(latest);

            Console.WriteLine(new string('=', MAX_WIDTH));
        }

        /*
        //private User player = new User("test").SetPassword("test");
        //private User player = new User("r");
        private User player = new User(string.Empty);

        private void Start()
        {
            /// Make Console UI to Maximize
            ShowWindow(Process.GetCurrentProcess().MainWindowHandle,
                ShowWindowCommands.SW_SHOWMAXIMIZED | ShowWindowCommands.SW_MAXIMIZE);

            //var config = new FirebaseConfig()
            //{
            //    BasePath = "https://brilliant-torch-522.firebaseio.com/",
            //    AuthSecret = "WUX7MzRBwzKD0yC8brqEuMWtjgYg3zSbewCQMhTa",
            //};
            //var client = new FirebaseClient(config);
            
            //var user = client.Get(string.Format("users/{0}", player.Account)).ResultAs<JObject>();
            //if (user == null)
            //{
            //    Console.WriteLine("not found");
            //}
            //else if (user.Value<string>("password").Equals(player.Password))
            //{
            //    player.Name = user.Value<string>("name");
            //    player.Experience = user.Value<int>("experience");
            //    player.Money = user.Value<int>("money");
            //}
            //else
            //{
            //    Console.WriteLine("password invalid");
            //}

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
        */
    }

    public static class Extensions
    {
        public static string Join(this string separator, IEnumerable<string> values)
        {
            return string.Join(separator, values);
        }
        public static string Join(this string separator, params object[] values)
        {
            return string.Join(separator, values);
        }

        public static int Length(this string s, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.Default;
            return encoding.GetByteCount(s);
        }
    }
}
