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

        private struct User
        {
            public string Account;
            public string Name;
            public int Experience;
            public int Money;
            public string Password;

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
        private User player = new User("r");

        private void Start()
        {
            ShowWindow(Process.GetCurrentProcess().MainWindowHandle,
                ShowWindowCommands.SW_SHOWMAXIMIZED | ShowWindowCommands.SW_MAXIMIZE);

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
        }
        private bool Loop()
        {
            Console.WriteLine("loop");

            UI();

            var cki = Console.ReadKey(true);
            if (cki.Key == ConsoleKey.Q)
            {
                return false;
            }

            return true;
        }

        private void UI()
        {
            Console.Clear();

            var state = player.Name.Equals(string.Empty) ? UserState.LogOff : UserState.LogOn;

            UITop(state);
            UICenter(state);
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
                user = string.Format(" [ {0}({1}) 접속 ]", player.Name, player.Account);
            }
            var now = DateTime.Now;
            string latest = string.Format("최근 갱신 시간 : {0:00}:{1:00}:{2:00} ", now.Hour, now.Minute, now.Second);
            Console.Write(user);
            int padding = Encoding.Default.GetByteCount(user) + (Encoding.Default.GetByteCount(latest) - latest.Length);
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(string.Format(string.Format("{{0,{0}}}", MAX_WIDTH - padding), latest));

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(new string('=', MAX_WIDTH));
        }
        private void UICenter(UserState state)
        {
            Console.SetCursorPosition(0, 6);

            switch(state)
            {
                case UserState.LogOff: UILogIn(); break;
                case UserState.LogOn: break;
            }

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
        private void UILogIn()
        {
            Console.WriteLine();

            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(" [ 로그인 ]");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("   계정 이름 : ");
            Console.ForegroundColor = ConsoleColor.White;
            string account = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("   비밀 번호 : ");
            Console.ForegroundColor = ConsoleColor.White;
            string pasword = Console.ReadLine();
        }
    }
}
