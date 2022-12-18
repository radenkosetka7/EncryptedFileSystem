using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace EncryptedFileSystem
{
    class User
    {
        private string username;
        private string password;
        static string path = @"D:\Kriptografija\EncryptedFileSystem\EncryptedFileSystem\FileSystem";
        private DirectoryInfo rootdirectory;
        private string currentDir;
        static string re = @"^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*#?&])[A-Za-z\d@$!%*#?&]{8,}$";
        Regex regex = new Regex(re);
        public User()
        {
            this.CreateRootDirectory();
        }
        public void Registration()
        {
            Console.WriteLine("Kreiranje novog naloga");
            Console.Write("Korisnicko ime: ");
            this.SetUsername(Console.ReadLine().Trim());
            Console.Write("Lozinka: ");
            SecureString passwd = new SecureString();
            while (true)
            {
                ConsoleKeyInfo key;
                do
                {
                    key = Console.ReadKey(true);
                    if (!char.IsControl(key.KeyChar))
                    {
                        passwd.AppendChar(key.KeyChar);
                        Console.Write("*");
                    }
                    else if (key.Key == ConsoleKey.Backspace && passwd.Length > 0)
                    {
                        passwd.RemoveAt(passwd.Length - 1);
                        Console.Write("\b \b");
                    }
                    else if (key.Key == ConsoleKey.Enter)
                    {
                        break;
                    }
                }
                while (true);
                string pass = new System.Net.NetworkCredential(string.Empty, passwd).Password;
                Console.Clear();
                if (regex.IsMatch(pass))
                {
                    this.SetPassword(pass);
                    Console.Clear();
                    break;
                }
                else
                {
                    Console.WriteLine("Neispravan unos. Lozinka se mora sastojati od minimalno 8 karaktera," +
                        "jednog velikog slova, jednog broja, i jednog specijalno karaktera najmanje");
                    continue;
                }
            }
            Directory.SetCurrentDirectory(path);
            try
            {
                Directory.CreateDirectory(username);
            }
            catch(Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
            Console.WriteLine("Nalog uspjesno kreiran!");
            string hashedPassword = GenerateHash(this.password);
            this.CreateDatabase(this.username, hashedPassword);
            try
            {
                var countDirs = rootdirectory.GetDirectories().Count();
                if (countDirs > 1)
                {
                    Directory.SetCurrentDirectory(path);
                    Directory.CreateDirectory("SharedDirectory");

                }

            }
            catch (DirectoryNotFoundException exception)
            {
                Console.WriteLine(exception.Message);
            }
        }
        public bool Login()
        {
            Console.Write("Korisnicko ime: ");
            this.SetUsername(Console.ReadLine().Trim());
            Console.Write("Lozinka: ");
            SecureString passwd = new SecureString();
            ConsoleKeyInfo key;
            do
            {
                    key = Console.ReadKey(true);
                    if (!char.IsControl(key.KeyChar))
                    {
                        passwd.AppendChar(key.KeyChar);
                        Console.Write("*");
                    }
                    else if (key.Key == ConsoleKey.Backspace && passwd.Length > 0)
                    {
                        passwd.RemoveAt(passwd.Length - 1);
                        Console.Write("\b \b");
                    }
                    else if (key.Key == ConsoleKey.Enter)
                    {
                        break;
                    }
            }
            while (true);
            string pass = new System.Net.NetworkCredential(string.Empty, passwd).Password;
            Console.Clear();
            this.SetPassword(pass);
            string hashPassword = GenerateHash(this.GetPassword());
            string line;
            string filePath = @"D:\Kriptografija\EncryptedFileSystem\EncryptedFileSystem\database.txt";
            string[] array = new string[2];
            using (StreamReader stream = new StreamReader(filePath))
            {
                while ((line = stream.ReadLine()) != null)
                {
                    array = line.Split(' ');
                    if (array[0] == this.GetUsername() && array[1] == hashPassword && CheckCertificate() && VerifyCertificate(this.GetUsername()))
                    {
                        string currentDir = path + @"\"+this.username;
                        Directory.SetCurrentDirectory(currentDir);
                        this.Listing(currentDir);
                        this.SetCurrentDir(Directory.GetCurrentDirectory());
                        return true;
                    }
                }
                Console.WriteLine();
                Console.WriteLine("Neupjesna prijava!");
                return false;
            }

        }
        public void CreateRootDirectory()
        {
            try
            {
                rootdirectory = Directory.CreateDirectory(path);
            }
            catch (DirectoryNotFoundException exception)
            {
                Console.WriteLine(exception.Message);
            }
        }
        public void CreateDatabase(string username, string password)
        {
            string filePath = @"D:\Kriptografija\EncryptedFileSystem\EncryptedFileSystem\database.txt";
            using (StreamWriter sw = File.AppendText(filePath))
            {
                sw.Write(username);
                sw.Write(" ");
                sw.WriteLine(password);
            }
        }
        public string GenerateHash(string password)
        {
            string saltedPassword=password;
            using (SHA512 hash = SHA512.Create())
            {
                byte[] sourceBytes = Encoding.UTF8.GetBytes(saltedPassword);
                byte[] hashBytes = hash.ComputeHash(sourceBytes);
                return BitConverter.ToString(hashBytes).Replace("-",String.Empty);
            }
        }
        public bool CheckCertificate()
        {
            string path = @"D:\Kriptografija\EncryptedFileSystem\EncryptedFileSystem\index.txt";
            string line;
            using (StreamReader stream = new StreamReader(path))
            {
                while ((line = stream.ReadLine()) != null)
                {
                    if (line.Contains(GetUsername()))
                    {
                        break;
                    }
                }
            }
            string[] array = line.Split('\t');

            if (array[0] == "V")
            {
                string dateFormat = @"yyMMddHHmmss";
                DateTime certificateDate;
                DateTime today = DateTime.Today;
                string date = array[1].Substring(0, array[1].Length - 1);
                DateTime.TryParseExact(date, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out certificateDate);
                int result = DateTime.Compare(certificateDate, today);
                if ((DateTime.Compare(certificateDate, today)) > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        public bool VerifyCertificate(string username)
        {
            string rootCAPath = @"D:\Kriptografija\EncryptedFileSystem\EncryptedFileSystem\rootCA.crt";
            string userPath = @"D:\Kriptografija\EncryptedFileSystem\EncryptedFileSystem\certs\" + username + ".crt";
            string fileName = username + "Verification";
            string path = @"D:\Kriptografija\EncryptedFileSystem\EncryptedFileSystem\bin\Debug\" + fileName;
            string[] array = new string[2];
            string command = "openssl verify -CAfile " + rootCAPath + " -verbose " + userPath + " >" + path;
            CmdExecute(command);
            using (StreamReader stream = new StreamReader(path))
            {
                array = stream.ReadLine().Split(' ');
            }
            if (array[0].Contains(username) && array[1] == "OK")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public void Listing(string path)
        {
            try
            {
                foreach (var array2 in Directory.GetFiles(path))
                {
                    Console.WriteLine(array2.Remove(0, path.Length + 1));
                }
            }
            catch (DirectoryNotFoundException exception)
            {
                Console.WriteLine(exception.Message);
            }
        }
        public void SetUsername(string username)
        {
            this.username = username;
        }
        public void SetPassword(string password)
        {
            this.password = password;
        }
        public string GetUsername()
        {
            return this.username;
        }
        public string GetCurrentDir()
        {
            return this.currentDir;
        }
        public string SetCurrentDir(string current)
        {
            return this.currentDir = current;
        }
        public string GetPassword()
        {
            return this.password;
        }
        public void CmdExecute(string command)
        {
            System.Diagnostics.ProcessStartInfo procStartInfo = new System.Diagnostics.ProcessStartInfo("cmd", "/c " + command);
            procStartInfo.RedirectStandardOutput = true;
            procStartInfo.UseShellExecute = false;
            procStartInfo.CreateNoWindow = true;
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo = procStartInfo;
            proc.Start();
        }
    }
}


