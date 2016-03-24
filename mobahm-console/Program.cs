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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
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
            public string Hash { get; private set; }

            public User() : this(string.Empty) { }
            public User(string Account)
            {
                this.State = UserState.LogOff;
                this.Account = Account;
                this.Name = string.Empty;
                this.Experience = -1;
                this.Money = -1;
                this.Hash = string.Empty;
            }
            public User(string Account, string Name, int Experience, int Money)
            {
                this.State = UserState.LogOn;
                this.Account = Account;
                this.Name = Name;
                this.Experience = Experience;
                this.Money = Money;
                this.Hash = string.Empty;
            }

            public User SetAccount(string Account)
            {
                this.Account = Account;
                return this;
            }
            public User SetPassword(string Password)
            {
                this.Hash = GetPassword(this.Account, Password);
                return this;
            }
            public User SetHash(string Hash)
            {
                this.Hash = Hash;
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
            LogOff, LogOn,
            LogIn, Register,
            Exit,
        }
        private enum LogInState : int
        {
            InputAccount, InputPassword,
            AllCredentials,
            WrongAccount, WrongHash,
            InvalidAccount, InvalidHash,
        }
        private User player = new User(string.Empty);

        private void Start()
        {
            /// Make Console UI to Maximize
            ShowWindow(Process.GetCurrentProcess().MainWindowHandle,
                ShowWindowCommands.SW_SHOWMAXIMIZED | ShowWindowCommands.SW_MAXIMIZE);

            /// Check Credentials File
            switch (CheckCredentials())
            {
                case CredentialsState.FileNotFound:
                case CredentialsState.NoPassword:
                    //UILogOff();
                    break;
                case CredentialsState.AllCredentials:
                    break;
            }
        }
        private bool Loop()
        {
            if (UI() == false)
            {
                return false;
            }
            Thread.Sleep(100);

            //if (Console.ReadKey(true).Key == ConsoleKey.Q) return false;
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
                player.SetAccount(account);

                string hash = credentials.Value<string>("hash");
                if (hash == null)
                {
                    return CredentialsState.NoPassword;
                }
                player.SetHash(hash);

                return CredentialsState.AllCredentials;
            }
            return CredentialsState.FileNotFound;
        }

        private bool UI()
        {
            switch (player.State)
            {
                case UserState.LogOff: UILogOff(); break;
                case UserState.LogIn: UILogIn(); break;
                case UserState.Register: UIRegister(); break;
                case UserState.LogOn: UILogOn(); break;
                case UserState.Exit: return false;
            }
            if (player.State == UserState.Exit)
            {
                Console.WriteLine(" MOBAHM 프로그램을 종료합니다.");
                Thread.Sleep(1000);
            }
            return true;
        }
        private void UILogOff()
        {
            UITop();
            UICenter();
        }
        private void UILogIn()
        {
            UITop();
            UICenter();
        }
        private void UIRegister()
        {
            UITop();
            UICenter();
        }
        private void UILogOn()
        {
            UITop();
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
        private void UICenter()
        {
            Console.SetCursorPosition(0, 6);

            if (player.State == UserState.LogOff)
            {
                var items = new Dictionary<ConsoleKey, string>();
                items.Add(ConsoleKey.D1, " 1. 유저 로그인 ");
                items.Add(ConsoleKey.D2, " 2. 유저 등록 ");
                items.Add(ConsoleKey.Q, " Q. 프로그램 종료 ");
                int padding = (MAX_WIDTH - items.Select(x => x.Value.Length).Max()) / 2 - 4;

                int index = -1;
                var key = ConsoleKey.Escape;
                while (items.ContainsKey(key) == false)
                {
                    Console.SetCursorPosition(0, 8);

                    foreach (var x in items)
                    {
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write(new string(' ', padding));

                        if (index >= 0 && x.Key == items.Keys.ToArray()[index % items.Count])
                        {
                            Console.BackgroundColor = ConsoleColor.Gray;
                            Console.ForegroundColor = ConsoleColor.Black;
                        }
                        Console.Write(x.Value);

                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine(new string(' ', MAX_WIDTH - (padding + x.Value.Length)));
                        Console.WriteLine();
                    }

                    key = Console.ReadKey(true).Key;

                    switch (key)
                    {
                        case ConsoleKey.UpArrow:
                            if (index < 0) index = items.Count;
                            index = (index - 1 + items.Count) % items.Count;
                            break;
                        case ConsoleKey.DownArrow:
                            index = (index + 1) % items.Count;
                            break;
                        case ConsoleKey.Enter:
                            if (index >= 0)
                            {
                                key = items.Keys.ToArray()[index % items.Count];
                            }
                            break;
                    }
                    if (items.ContainsKey(key))
                    {
                        switch (key)
                        {
                            case ConsoleKey.D1: player.State = UserState.LogIn; break;
                            case ConsoleKey.D2: player.State = UserState.Register; break;
                            case ConsoleKey.Q: player.State = UserState.Exit; break;
                        }

                        Console.SetCursorPosition(0, 8);
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.White;
                        foreach (var x in Enumerable.Range(0, items.Count * 2))
                        {
                            Console.WriteLine(new string(' ', MAX_WIDTH));
                        }

                        break;
                    }
                }
            }
            else if (player.State == UserState.LogIn)
            {
                Console.SetCursorPosition(0, 8);

                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.White;

                var step = LogInState.InputAccount;

                string account = player.Account;
                string password = string.Empty;

                while (true)
                {
                    Console.SetCursorPosition(0, 8);
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.WriteLine(new string(' ', MAX_WIDTH));

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.SetCursorPosition(10, 8);
                    string accountPrefix = "아 이 디 : ";
                    Console.Write(accountPrefix);

                    if (step != LogInState.InputAccount)
                    {
                        if (account.Length > 0)
                        {
                            Console.BackgroundColor = ConsoleColor.DarkBlue;
                        }
                        else
                        {
                            Console.BackgroundColor = ConsoleColor.DarkRed;
                        }
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }
                    Console.Write(account);
                    Console.Write(new string(' ', MAX_WIDTH - (10 + accountPrefix.Length() + account.Length() + 20)));

                    Console.SetCursorPosition(0, 9);
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.WriteLine(new string(' ', MAX_WIDTH));

                    Console.SetCursorPosition(10, 9);
                    Console.ForegroundColor = ConsoleColor.White;
                    string passwordPrefix = "비밀번호 : ";
                    Console.Write(passwordPrefix);

                    if (step != LogInState.InputPassword)
                    {
                        if (password.Length > 0)
                        {
                            Console.BackgroundColor = ConsoleColor.DarkBlue;
                        }
                        else
                        {
                            Console.BackgroundColor = ConsoleColor.DarkRed;
                        }
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }
                    Console.Write(new string('*', password.Length));
                    Console.Write(new string(' ', MAX_WIDTH - (10 + passwordPrefix.Length() + password.Length() + 20)));

                    //Console.SetCursorPosition(0, 13);
                    //Console.BackgroundColor = ConsoleColor.Black;
                    //Console.ForegroundColor = ConsoleColor.White;
                    //Console.WriteLine(" <Ctrl + C> 종료");

                    //MessageBox.Show(step.ToString());
                    //int left = Console.CursorLeft;
                    //int top = Console.CursorTop;
                    //Console.SetCursorPosition(0, 20);
                    //Console.WriteLine(step);
                    //Console.SetCursorPosition(left, top);
                    if (step == LogInState.InputAccount)
                    {
                    }
                    else if (step == LogInState.InputPassword)
                    {
                    }
                    else if (step == LogInState.AllCredentials)
                    {
                        bool error = false;
                        var code = step;
                        string message = string.Empty;

                        if (account.Length <= 0)
                        {
                            step = LogInState.InputAccount;
                            error = true;
                            message = "아이디를 입력해주세요.";
                        }
                        else if (password.Length <= 0)
                        {
                            step = LogInState.InputPassword;
                            error = true;
                            message = "비밀번호를 입력해주세요.";
                        }

                        JObject user = null;
                        if (error == false)
                        {
                            player.SetAccount(account).SetPassword(password);
                            Console.SetCursorPosition(10 + passwordPrefix.Length() + password.Length(), 9);
                            password = string.Empty;
                            var login = LogIn();
                            error = login.Value<bool>("error");
                            if (error)
                            {
                                message = login.Value<string>("errorMessage");
                                code = (LogInState)login.Value<int>("errorCode");
                            }
                            else
                            {
                                user = login.Value<JObject>("user");
                            }
                        }

                        if (error)
                        {
                            Console.SetCursorPosition(0, 11);
                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine(new string(' ', MAX_WIDTH));

                            Console.SetCursorPosition(10, 11);
                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine(message);
                            
                            switch (code)
                            {
                                case LogInState.WrongAccount: step = LogInState.InputAccount; break;
                                case LogInState.WrongHash: step = LogInState.InputPassword; break;
                            }
                            continue;
                        }
                        else
                        {
                            string name = user.Value<string>("name");
                            int money = user.Value<int>("money");
                            int experience = user.Value<int>("experience");
                            player = new User(account, name, money, experience);
                            break;
                        }
                        /*
                        if (account.Length <= 0)
                        {
                            step = LogInState.InputAccount;

                            Console.SetCursorPosition(0, 11);
                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine(new string(' ', MAX_WIDTH));

                            Console.SetCursorPosition(10, 11);
                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine("아이디를 입력해주세요.");

                            continue;
                        }
                        if (password.Length <= 0)
                        {
                            step = LogInState.InputPassword;

                            Console.SetCursorPosition(0, 11);
                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine(new string(' ', MAX_WIDTH));

                            Console.SetCursorPosition(10, 11);
                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine("비밀번호를 입력해주세요.");

                            continue;
                        }

                        player.SetAccount(account).SetPassword(password);
                        password = string.Empty;
                        var fc = new FirebaseClient(new FirebaseConfig() { BasePath = "https://mobahm.firebaseio.com/" });
                        var fr = fc.Get($"users/{account}");
                        var user = fr.ResultAs<JObject>();

                        if (user == null)
                        {
                            step = LogInState.InputAccount;

                            Console.SetCursorPosition(0, 11);
                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine(new string(' ', MAX_WIDTH));

                            Console.SetCursorPosition(10, 11);
                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine("존재하지 않는 아이디입니다.");

                            continue;
                        }
                        else if (user.Value<string>("hash").Equals(player.Hash))
                        {
                            string name = user.Value<string>("name");
                            int money = user.Value<int>("money");
                            int experience = user.Value<int>("experience");
                            player = new User(account, name, money, experience);

                            Console.SetCursorPosition(0, 8);
                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.ForegroundColor = ConsoleColor.White;
                            foreach (var x in Enumerable.Range(0, 5))
                            {
                                Console.WriteLine(new string(' ', MAX_WIDTH));
                            }

                            break;
                        }
                        else
                        {
                            step = LogInState.InputPassword;

                            Console.SetCursorPosition(0, 11);
                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine(new string(' ', MAX_WIDTH));

                            Console.SetCursorPosition(10, 11);
                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine("비밀번호가 일치하지 않습니다.");

                            continue;
                        }
                        */
                    }

                    switch (step)
                    {
                        case LogInState.InputAccount:
                            Console.SetCursorPosition(10 + accountPrefix.Length() + account.Length(), 8);
                            break;
                        case LogInState.InputPassword:
                            Console.SetCursorPosition(10 + passwordPrefix.Length() + password.Length(), 9);
                            break;

                    }

                    var cki = Console.ReadKey(true);
                    switch (cki.Key)
                    {
                        case ConsoleKey.Backspace:
                            if (step == LogInState.InputAccount)
                            {
                                if (account.Length > 0)
                                {
                                    account = account.Substring(0, account.Length - 1);
                                }
                            }
                            else if (step == LogInState.InputPassword)
                            {
                                if (password.Length > 0)
                                {
                                    password = password.Substring(0, password.Length - 1);
                                }
                            }
                            break;
                        case ConsoleKey.Tab:
                            if (step == LogInState.InputAccount)
                            {
                                step = LogInState.InputPassword;
                            }
                            else if (step == LogInState.InputPassword)
                            {
                                step = LogInState.InputAccount;
                            }
                            break;
                        case ConsoleKey.Enter:
                            if (step == LogInState.InputAccount)
                            {
                                step = LogInState.InputPassword;
                            }
                            else if (step == LogInState.InputPassword)
                            {
                                step = LogInState.AllCredentials;
                            }
                            else
                            {
                                step = LogInState.AllCredentials;
                            }
                            break;
                        default:
                            if (step == LogInState.InputAccount)
                            {
                                if (Enumerable.Range('0', 10).Contains(cki.KeyChar) ||
                                    Enumerable.Range('a', 26).Contains(cki.KeyChar) ||
                                    Enumerable.Range('A', 26).Contains(cki.KeyChar) ||
                                    new char[] { '-', '_', '.' }.Contains(cki.KeyChar))
                                {
                                    account = $"{account}{cki.KeyChar}";
                                }
                            }
                            else if (step == LogInState.InputPassword)
                            {
                                if (Enumerable.Range(0x20, 0x5F).Contains(cki.KeyChar))
                                {
                                    password = $"{password}{cki.KeyChar}";
                                }
                            }
                            break;
                    }
                }
            }
            else if (player.State == UserState.LogOn)
            {

            }
        }

        private JObject LogIn()
        {
            var result = new JObject();
            using (var fc = new FirebaseClient(new FirebaseConfig() { BasePath = "https://mobahm.firebaseio.com/" }))
            {
                var fr = fc.Get($"users/{player.Account}");
                var user = fr.ResultAs<JObject>();
                if (user == null)
                {
                    result.Add("error", true);
                    result.Add("errorCode", Convert.ToInt32(LogInState.WrongAccount));
                    result.Add("errorMessage", "존재하지 않는 아이디입니다.");
                }
                else if (user.Value<string>("hash").Equals(player.Hash))
                {
                    result.Add("error", false);
                    /*
                    string name = user.Value<string>("name");
                    int money = user.Value<int>("money");
                    int experience = user.Value<int>("experience");
                    player = new User(player.Account, name, money, experience);
                    */
                    result.Add("user", user);
                }
                else
                {
                    result.Add("error", true);
                    result.Add("errorCode", Convert.ToInt32(LogInState.WrongHash));
                    result.Add("errorMessage", "비밀번호가 일치하지 않습니다.");
                }
            }
            return result;
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
