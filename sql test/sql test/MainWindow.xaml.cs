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
using System.Security.Cryptography;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Diagnostics;
using System.IO;
using System.Windows.Threading;
using System.Windows.Shapes;
using System.Threading;

namespace sql_test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool acctexists = false;
        bool login = false;
        string globalfailstr;
        DispatcherTimer countdowntimer = new DispatcherTimer();
        DispatcherTimer gametimer = new DispatcherTimer();
        int score;
        int time;
        List<Rectangle> rectlist = new List<Rectangle>();
        Random rnd = new Random();
        int topscore = -1;
        string currentuser = null;
        public MainWindow()
        {
            InitializeComponent();
            countdowntimer.Tick += Timer_Tick;
            gametimer.Tick += Gametimer_Tick;
        }

        private void Gametimer_Tick(object sender, EventArgs e)
        {
            Rectangle rect = new Rectangle
            {
                Height = 30,
                Width = 30,
                Fill = new SolidColorBrush(Color.FromRgb(250, 0, (byte)rnd.Next(0, 50))),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Stroke = new SolidColorBrush(Color.FromRgb(0,0,0)),
                Margin = new Thickness(rnd.Next(335, 675), rnd.Next(75, 275), 0, 0),
                
            };
            rect.MouseLeftButtonDown += Rect_MouseLeftButtonDown;
            grid.Children.Add(rect);
            rectlist.Add(rect);
            if (grid.Children.OfType<Rectangle>().Count() > 4 && time < 7)
            {
                grid.Children.Remove(rectlist[rnd.Next(rectlist.Count)]);
                grid.Children.Remove(rectlist[rnd.Next(rectlist.Count)]);
                grid.Children.Remove(rectlist[rnd.Next(rectlist.Count)]);
                grid.Children.Remove(rectlist[rnd.Next(rectlist.Count)]);
            }
            else if (grid.Children.OfType<Rectangle>().Count() > 2 && time < 10)
            {
                grid.Children.Remove(rectlist[rnd.Next(rectlist.Count)]);
                grid.Children.Remove(rectlist[rnd.Next(rectlist.Count)]);
            }
            else if (grid.Children.OfType<Rectangle>().Count() > 1 && time < 15)
            {
                grid.Children.Remove(rectlist[rnd.Next(rectlist.Count)]);
            }
            
        }

        private void Rect_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var rect = sender as Rectangle;
            score += 10;
            scorelabel.Content = score + " :Score";
            grid.Children.Remove(rect);
            rectlist.Remove(rect);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (time <= 14)
            {
                countdowntimer.Interval = new TimeSpan(0, 0, 1);
            }
            timelabel.Content = "Time: " + time;
            time--;
            if (time < 0)
            {
                countdowntimer.Stop();
                gametimer.Stop();
                outp.Text = "Game over! "+"Score: " + score + "\n";
                if (true)
                {
                    MySqlDataReader reader = null;
                    DBConnection dbCon = DBConnection.Instance();
                    if (dbCon.IsConnect())
                    {
                        if(login == true && currentuser != null && score > topscore)
                        {
                            if(topscore < 0)
                            {
                                MessageBox.Show("AAA");
                                Thread.Sleep(100000000);
                            }
                            outp.Text += "New High score!\n";
                            try
                            {
                                reader = SqlExecute($"UPDATE `usertable` SET `score` = '{score}' WHERE `usertable`.`username` = '{currentuser}'", dbCon);
                            }
                            catch (Exception ex)
                            {
                                outp.Text = "Falure at score update";
                                MessageBox.Show("Failure at score update" + ex.ToString());
                            }
                        }

                        if(reader!= null)
                            reader.Close();

                        reader = SqlExecute($"SELECT * FROM `usertable` ORDER BY `score` DESC LIMIT 5", dbCon);
                        if (reader != null)
                        {
                            outp.Text += "Top Scores\n";
                            int i = 1;
                            outp.TextAlignment = TextAlignment.Left;
                            int l = 0;
                            while (reader.Read())
                            {
                                l = reader.GetString("username").Length;
                                outp.Text += i + ". " + reader.GetString("username");
                                if (l < 6) { outp.Text += "\t\t\t"; }
                                if (l >= 6 && l < 14) { outp.Text += "\t\t"; }
                                if (l >= 14) { outp.Text += "\t"; }
                                outp.Text += reader.GetString("score") + "\n";
                                if (i < 4)
                                {
                                    outp.Text += "  Says: " + reader.GetString("data") + "\n";
                                }
                                i++;
                            }
                            reader.Close();
                        }
                    }
                    
                }
                time = 15;
                for(int i = 0; i<rectlist.Count; i++)
                {
                    grid.Children.Remove(rectlist[i]);
                }
                rectlist.Clear();

                gamebutton.IsEnabled = true;
                if(login == false)
                {
                    loginbtn.IsEnabled = true;
                }
                else
                {
                    logoutbtn.IsEnabled = true;
                }
                newuserbtn.IsEnabled = true;
            }
        }

        private void gamebutton_Click(object sender, RoutedEventArgs e)
        {
            time = 15;
            score = 0;
            timelabel.Content = "Time: " + time;
            scorelabel.Content = score + " :Score";
            if(login == false)
            {
                outp.Text = "You are not logged in. Score will not be recorded";
            }
            countdowntimer.Interval = new TimeSpan(0,0,0,0,500);
            gametimer.Interval = new TimeSpan(0, 0, 0, 0, 175);
            gametimer.Start();
            countdowntimer.Start();
            gamebutton.IsEnabled = false;
            loginbtn.IsEnabled = false;
            newuserbtn.IsEnabled = false;
            logoutbtn.IsEnabled = false;
        }

        private void loginbtn_click(object sender, RoutedEventArgs e)
        {
            if (username.Text=="" || password.Text == "")
            {
                outp.Text = "Please enter Username and Password";
                goto Retry;
            }
            string hash = null;
            using (MD5 md5hash = MD5.Create())
            {
                hash = GetMd5Hash(md5hash, password.Text);
            }
            var dbCon = DBConnection.Instance();
            if (dbCon.IsConnect())
            {
                int col = 0;
                MySqlDataReader reader = null;
                reader = SqlExecute("SELECT count(*) FROM information_schema.columns WHERE table_name = 'usertable'", dbCon);
                if(reader != null && reader.Read())
                {
                    col = Int32.Parse(reader.GetString(0));
                    reader.Close();
                }

                reader = SqlExecute($"SELECT * from usertable where username='" + username.Text + "' and pwhash='" + hash + "'", dbCon);
                if (reader != null && !reader.HasRows)
                {
                    outp.Text = "Bad un or pw";
                    reader.Close();
                    goto Retry;
                }
                else if (reader != null && reader.Read())
                {
                    topscore = reader.GetInt32("score");
                    currentuser = reader.GetString("username");
                    login = true;
                    loginbtn.IsEnabled = false;
                    logoutbtn.IsEnabled = true;
                    outp.Text = "";
                    outp.Text += "Hello " + currentuser + "! Your top score: " + topscore + "\n";
                    reader.Close();
                }
                reader = SqlExecute($"SELECT * FROM `usertable` ORDER BY `score` DESC LIMIT 5", dbCon);
                if (reader != null && !reader.HasRows)
                {
                    outp.Text = "Bad un or pw";
                    reader.Close();
                }
                else if (reader != null)
                {
                    outp.Text += "Top Scores\n";
                    int i = 1;
                    outp.TextAlignment = TextAlignment.Left;
                    int l = 0;
                    while(reader.Read())
                    {
                        l = reader.GetString("username").Length;
                        outp.Text += i + ". " + reader.GetString("username");
                        if (l < 6) { outp.Text += "\t\t\t"; }
                        if (l >= 6 && l < 14) { outp.Text += "\t\t"; }
                        if (l >= 14) { outp.Text += "\t"; }
                        outp.Text += reader.GetString("score") + "\n";
                        if (i < 4)
                        {
                            outp.Text += "Says: " + reader.GetString("data") + "\n";
                        }
                        i++;
                    }
                }
            }
            else
            {
                outp.Text = dbCon.Failstr;
            }
            dbCon.Close();
            Retry:
            outp.Text += "";
        }

        MySqlDataReader SqlExecute(string query, DBConnection dbCon)
        {
            dbCon.Open();
            var cmd = new MySqlCommand(query, dbCon.Connection);
            MySqlDataReader reader = null;
            if (reader != null)
            {
                reader.Close();
            }
            try
            {
                reader = cmd.ExecuteReader();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                globalfailstr = "Failure at sqlex \n" + ex.ToString() + "\n" + ex.Message;
                outp.Text = "Failure at sqlex";
                return null;
            }
            return reader;
        }
        
        private void NewUser_Click(object sender, RoutedEventArgs e)
        {
            if (newun.Text == "" || newpw.Text == "" || newdata.Text == "")
            {
                outp.Text="All new user fields must be filled";
                goto RetryNew;
            }
            if (username.Text.Length > 16 || password.Text.Length > 16 || newdata.Text.Length > 50)
            {
                outp.Text = "Username or password cannot be longer than 16 characters. Tag must be less than 50 characters";
                goto RetryNew;
            }
            string hash = null;
            using (MD5 md5hash = MD5.Create())
            {
                hash = GetMd5Hash(md5hash, newpw.Text);
            }
            string cmd = $"INSERT INTO `usertable` (`id`, `username`, `pwhash`, `score`, `data`, `creation_date`, `datatag`) " +
                $"VALUES (NULL, '{newun.Text}', '{hash}', 0, '{newdata.Text}', NOW(), 'string')";
            var dbCon = DBConnection.Instance();
            MySqlDataReader reader = null;
            if (dbCon.IsConnect())
            {
                reader = SqlExecute(cmd, dbCon);
                if (reader != null)
                {
                    outp.Text = "Registration complete. You may now login";
                }
            }
            else
            {
                outp.Text = dbCon.Failstr;
            }
            dbCon.Close();
            RetryNew:
            outp.Text += "";

            /*
            string filepath = @"D:\Media\Images\20160731_172518.jpg";
            try
            {
                using (System.Drawing.Image i = System.Drawing.Image.FromFile(filepath, true))
                {
                    using (MemoryStream m = new MemoryStream())
                    {
                        i.Save(m, i.RawFormat);
                        byte[] b = m.ToArray();
                        string b64 = Convert.ToBase64String(b);
                        string txtpath = @"C:\Users\James\Desktop\txt.txt";
                        if (!File.Exists(txtpath))
                        {
                            File.WriteAllText(txtpath, b64);
                        }
                    }
                }
            }catch(Exception ex)
            {
                outp.Text = ex.Message;
            }
            */
        }

        static string GetMd5Hash(MD5 md5Hash, string input)
        {

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        // Verify a hash against a string.
        static bool VerifyMd5Hash(MD5 md5Hash, string input, string hash)
        {
            // Hash the input.
            string hashOfInput = GetMd5Hash(md5Hash, input);

            // Create a StringComparer an compare the hashes.
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            if (0 == comparer.Compare(hashOfInput, hash))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void logoutbtn_Click(object sender, RoutedEventArgs e)
        {
            if (login == false)
            {
                outp.Text = "You are not logged in.";
            }
            else
            {
                login = false;
                currentuser = null;
                loginbtn.IsEnabled = true;
                logoutbtn.IsEnabled = false;
                outp.Text = "Logged out";
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            outp.Text = "Connecting...";
            var dbCon = DBConnection.Instance();
            if (dbCon.IsConnect())
            {
                outp.Text = "Database connected.\nHave fun!\nRegister or login to save your score.";
            }
            else
            {
                outp.Text = "Database may be unavailible.\nLogin or register to try again.";
            }

        }
    }
}
