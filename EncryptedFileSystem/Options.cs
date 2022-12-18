using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Word = Microsoft.Office.Interop.Word;


namespace EncryptedFileSystem
{
    class Options
    {
        public Algotihms alg = new Algotihms();
        public Extensions e = new Extensions();
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
        public void SignDocument(string path, string document,string username)
        {
            string privateKey = @"D:\Kriptografija\EncryptedFileSystem\EncryptedFileSystem\private\" + username + ".key";
            string signPath = @"D:\Kriptografija\EncryptedFileSystem\EncryptedFileSystem\FileSystem\" + username + @"\Signature\" + "sign" + document;
            string command = "openssl dgst -sha1 -sign " + privateKey + " -keyform PEM -out "+signPath+" " + path;
            CmdExecute(command);
        }
        public bool VerifySignature(string username,string path,string document)
        {
            string array;
            string publicKey = @"D:\Kriptografija\EncryptedFileSystem\EncryptedFileSystem\public\" +"Public"+ username + ".key";
            string signPath = @"D:\Kriptografija\EncryptedFileSystem\EncryptedFileSystem\FileSystem\" + username + @"\Signature\" + "sign" + document;
            string signaturePath = @"D:\Kriptografija\EncryptedFileSystem\EncryptedFileSystem\FileSystem\"+username+@"\Signature\"+document+"signature.txt";
            string command = "openssl dgst -sha1 -verify " + publicKey + " -signature " + signPath + " " + path+@"\" +document+ " >" + signaturePath;
            CmdExecute(command);
            Thread.Sleep(2000);
            using (StreamReader stream = new StreamReader(signaturePath))
            {
                array = stream.ReadLine();
            }
            if(array=="Verified OK")
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        public void CreateTextDocument(string path,string username)
        {
            Console.WriteLine("Unesite naziv fajla:");
            string fileName = Console.ReadLine();
            Directory.CreateDirectory(path + @"\Signature");
            if (fileName.EndsWith(".txt"))
            {
                Console.WriteLine("Unesite tekst u fajl:");
                string auxiliaryPath = path + @"\" + "helpedfile.txt";
                string absolutePath = path + @"\" + fileName;
                using (StreamWriter stream = new StreamWriter(auxiliaryPath))
                {
                    stream.WriteLine(Console.ReadLine());
                }
                string command = "openssl des3 -in " + auxiliaryPath + " -out " +absolutePath + " -k sigurnost";
                CmdExecute(command);
                SignDocument(absolutePath, fileName, username);
            }
            else if (fileName.EndsWith(".docx"))
            {
                string auxiliaryPath = path + @"\" + "helpedfile.docx";
                Console.WriteLine("Unesite tekst u fajl:");
                string text = Console.ReadLine();
                Word.Application word = new Word.Application();
                word.Visible = false;
                word.WindowState = Word.WdWindowState.wdWindowStateNormal;
                Word.Document doc = word.Documents.Add();
                Word.Paragraph paragraph;
                paragraph = doc.Paragraphs.Add();
                paragraph.Range.Text = text;
                doc.SaveAs2(auxiliaryPath);
                doc.Close();
                word.Quit();
                string absolutePath = path + @"\" + fileName;
                string command = "openssl idea -in " + auxiliaryPath + " -out " + absolutePath + " -k sigurnost";
                CmdExecute(command);
                SignDocument(absolutePath, fileName, username);
            }
            else if (fileName.EndsWith(".pdf"))
            {
                string auxiliaryPath = path + @"\" + "helpedfile.pdf";
                Console.WriteLine("Unesite tekst u fajl:");
                string text = Console.ReadLine();
                var pdfDoc = new Document(PageSize.A4);
                PdfWriter.GetInstance(pdfDoc, new FileStream(auxiliaryPath, FileMode.Create));
                pdfDoc.Open();
                pdfDoc.Add(new Paragraph(text));
                pdfDoc.Close();
                string absolutePath = path + @"\" + fileName;
                string command = "openssl rc4 -in " + auxiliaryPath + " -out " + absolutePath + " -k sigurnost";
                CmdExecute(command);
                SignDocument(absolutePath, fileName, username);
            }
            else
            {
                Console.WriteLine("Unesena ekstenzija nije podrzana ili fajl vec postoji!");
            }
            Thread.Sleep(2000);
            foreach(var files in Directory.GetFiles(path))
            {
                if(files.Contains("helpedfile"))
                {
                    File.Delete(files);
                        break;
                }
            }
        }
        public void OpenFile(string path, string username)
        {
            Console.WriteLine("Unesite naziv datoteke koju zelite otvoriti: ");
            string file = Console.ReadLine();
            string newPath = path + @"\" + file;
            string signaturePath = @"D:\Kriptografija\EncryptedFileSystem\EncryptedFileSystem\FileSystem\" + username + @"\Signature\" + "sign" + file;
            if (File.Exists(newPath) && File.Exists(signaturePath))
            {
                if (VerifySignature(username, path, file))
                {
                    if (file.EndsWith("txt"))
                    {
                        string helpedFile = path + "\\help.txt";
                        string command = "openssl des3 -d -in " + newPath + " -out " + helpedFile + " -k sigurnost";
                        CmdExecute(command);
                        Thread.Sleep(100);
                        Process process = Process.Start("notepad.exe", helpedFile);
                        process.WaitForExit();
                    }
                    else if (file.EndsWith("docx"))
                    {
                        string helpedFile = path + "\\help.docx";
                        string command = "openssl idea -d -in " + newPath + " -out " + helpedFile + " -k sigurnost";
                        CmdExecute(command);
                        Thread.Sleep(1000);
                        Process process=Process.Start("winword.exe", helpedFile);
                        process.WaitForExit();
                    }
                    else if (file.EndsWith("pdf"))
                    {
                        string helpedFile = path + "\\help.pdf";
                        string command = "openssl rc4 -d -in " + newPath + " -out " + helpedFile + " -k sigurnost";
                        CmdExecute(command);
                        Thread.Sleep(1000);
                        Process process=Process.Start("acroRd32.exe", helpedFile);
                        process.WaitForExit();
                    }
                    else if (file.EndsWith("png") || file.EndsWith("jpg"))
                    {

                        string[] array = file.Split('.');
                        string helpedFile = path + @"\help." + array[1];
                        string command = "openssl des -d -in " + newPath + " -out " + helpedFile + " -k sigurnost";
                        CmdExecute(command);
                        Thread.Sleep(1000);
                        Process process=Process.Start(helpedFile);
                    }
                }
                else
                {
                    Console.WriteLine("Neupsjesno otvaranje datoteke! Integritet narusen.");
                }
            }
            else
            {
                Console.WriteLine("Fajl ne postoji!");
            }
            Thread.Sleep(2000);
            foreach(var files in Directory.GetFiles(path))
            {
                if (files.Contains("help"))
                {
                    File.Delete(files);
                    break;
                }
            }
        }

        public void UploadFile(string path,string username)
        {
            Console.WriteLine("Unesite naziv fajla: (potrebno je unijeti apsolutnu putanju)");
            string fileNamePath = Console.ReadLine();
            string[] array = fileNamePath.Split('\\');
            string fileName = array[array.Length-1];
            string userPath = path + @"\" + fileName;
            if(File.Exists(fileNamePath))
            {
                if(fileName.EndsWith("txt"))
                {
                    string command = "openssl des3 -in " + fileNamePath + " -out " + userPath + " -k sigurnost";
                    CmdExecute(command);
                    SignDocument(userPath, fileName, username);
                }
                else if(fileName.EndsWith("docx"))
                {
                    string command = "openssl idea -in " + fileNamePath + " -out " + userPath + " -k sigurnost";
                    CmdExecute(command);
                    SignDocument(userPath, fileName, username);
                }
                else if(fileName.EndsWith("pdf"))
                {
                    string command = "openssl rc4 -in " + fileNamePath + " -out " + userPath + " -k sigurnost";
                    CmdExecute(command);
                    SignDocument(userPath, fileName, username);
                }
                else if(fileName.EndsWith("png") || fileName.EndsWith("jpg"))
                {
                    string command = "openssl des -in " + fileNamePath + " -out " + userPath + " -k sigurnost";
                    CmdExecute(command);
                    SignDocument(userPath, fileName, username);
                }

            }
            else
            {
                Console.WriteLine("Dati fajl ne postoji.");
            }
        }
        public void DownloadFile(string path,string username)
        {
            Console.WriteLine("Unesite naziv fajla koji zelite preuzeti:");
            string fileName = Console.ReadLine();
            string filePath = path + @"\" + fileName;
            string hostPath = @"C:\Users\user";
            string hostFilePath = hostPath+@"\" + fileName;
            string signPath = @"D:\Kriptografija\EncryptedFileSystem\EncryptedFileSystem\FileSystem\" + username + @"\Signature\" + "sign" + fileName;
            if(File.Exists(signPath) && File.Exists(filePath))
            {
                if(VerifySignature(username,path,fileName))
                {

                    if (fileName.EndsWith("txt"))
                    {
                        string helpedPath = path + @"\" + "helpted.txt";
                        string command = "openssl des3 -d -in " + filePath + " -out " + helpedPath + " -k sigurnost";
                        CmdExecute(command);
                        Thread.Sleep(1000);
                        File.Copy(helpedPath, hostFilePath, true);
                    }
                    else if(fileName.EndsWith("docx"))
                    {
                        string helptedPath = path + @"\" + "helpted.docx";
                        string command = "openssl idea -d -in " + filePath + " -out " + helptedPath + " -k sigurnost";
                        CmdExecute(command);
                        Thread.Sleep(1000);
                        File.Copy(helptedPath, hostFilePath, true);
                    }
                    else if(fileName.EndsWith("pdf"))
                    {
                        string helptedPath = path + @"\" + "helpted.pdf";
                        string command = "openssl rc4 -d -in " +filePath + " -out " + helptedPath + " -k sigurnost";
                        CmdExecute(command);
                        Thread.Sleep(1000);
                        File.Copy(helptedPath, hostFilePath, true);
                    }
                    else if(fileName.EndsWith("png") || fileName.EndsWith("jpg"))
                    {
                        string[] array = new string[2];
                        array = fileName.Split('.');
                        string helptedPath = path + @"\" + "helpted."+array[1];
                        string command = "openssl des -d -in " + filePath + " -out " + helptedPath + " -k sigurnost";
                        CmdExecute(command);
                        Thread.Sleep(1000);
                        File.Copy(helptedPath, hostFilePath, true);
                    }
                }
                else
                {
                    Console.WriteLine("Nije moguce preuzeti fajl. Integritet narusen!");
                }
            }
            else
            {
                Console.WriteLine("Dati fajl ne postoji.");
            }
            Thread.Sleep(100);
            foreach(var files in Directory.GetFiles(path))
            {
                if(files.Contains("helpted"))
                {
                    File.Delete(files);
                    break;
                }
            }
        }
        public void ChangeFileContent(string path, string username)
        {
            Console.WriteLine("Unesite naziv fajla koji zelite izmijeniti:");
            string fileName = Console.ReadLine();
            string filePath = path + @"\" + fileName;
            string signPath = @"D:\Kriptografija\EncryptedFileSystem\EncryptedFileSystem\FileSystem\" + username + @"\Signature\" + "sign" + fileName;
            if (File.Exists(signPath) && File.Exists(filePath))
            {
                if (VerifySignature(username, path, fileName))
                {
                    File.Delete(signPath);
                    if (fileName.EndsWith("txt"))
                    {
                        string helpedPath = path + @"\" + "helpted.txt";
                        string command = "openssl des3 -d -in " + filePath + " -out " + helpedPath + " -k sigurnost";
                        CmdExecute(command);
                        string newPath = path + @"\" + fileName;
                        Thread.Sleep(1000);
                        Process process=Process.Start("notepad.exe", helpedPath);
                        process.WaitForExit();
                        string newCommand = "openssl des3 -in " + helpedPath + " -out " + newPath + " -k sigurnost";
                        CmdExecute(newCommand);
                        Thread.Sleep(1000);
                        File.Delete(helpedPath);
                        SignDocument(filePath, fileName, username);
                    }
                    else if (fileName.EndsWith("docx"))
                    {
                        string helpedPath = path + @"\" + "helpted.docx";
                        string command = "openssl idea -d -in " + filePath + " -out " + helpedPath + " -k sigurnost";
                        CmdExecute(command);
                        string newPath = path + @"\" + fileName;
                        Thread.Sleep(1000);
                        Process process=Process.Start("winword.exe", helpedPath);
                        process.WaitForExit();
                        string newCommand = "openssl idea -in " + helpedPath + " -out " + newPath + " -k sigurnost";
                        CmdExecute(newCommand);
                        Thread.Sleep(1000);
                        File.Delete(helpedPath);
                        SignDocument(filePath, fileName, username);

                    }
                    else
                    {
                        Console.WriteLine("Nije moguce promijeniti fajl sa ekstenzijom pdf!");
                    }
                }
                else
                {
                    Console.WriteLine("Nije moguce izmijeniti fajl. Integritet narusen!");
                }
            }
            else
            {
                Console.WriteLine("Dati fajl ne postoji.");
            }
        }
        public void DeleteFile(string path,string username)
        {
            Console.WriteLine("Unesite naziv fajla koji zelite obrisati:");
            string file = Console.ReadLine();
            string filePath = path + @"\" + file;
            if(File.Exists(filePath))
            {
                File.Delete(filePath);
                Console.WriteLine("Fajl uspjesno obrisan.");
            }
            else
            {
                Console.WriteLine("Ne postoji dati fajl.");
            }
            string signPath = @"D:\Kriptografija\EncryptedFileSystem\EncryptedFileSystem\FileSystem\" + username + @"\Signature\" + "sign" + file;
            if(File.Exists(signPath))
            {
                File.Delete(signPath);
            }
        }
        public void SharedDirectoryOptions(string path,string username)
        {
            Console.WriteLine("1. Podijeli fajl");
            Console.WriteLine("2. Procitaj fajl");
            string option = Console.ReadLine();
            if(option=="1")
            {
                ShareFile(path,username);
            }
            else if(option=="2")
            {
               ReadFile(path,username);
            }
            else
            {
                Console.WriteLine("Ne postoji unesena opcija.");
            }
        }
        public void ShareFile(string path,string hostName)
        {
            Console.WriteLine("Kome zelite podijeliti fajl: ");
            string userName = Console.ReadLine();
            Console.WriteLine("Unesite naziv fajla koji zelite podijeliti: ");
            string fileName = Console.ReadLine();
            string filePath = path + @"\" + fileName;
            string algorithm;
            do
            {
                Console.WriteLine("Unesite naziv algoritma: ");
                algorithm = Console.ReadLine();
                Console.Clear();
            }
            while (!alg.algorithms.Contains(algorithm));
            Console.WriteLine("Unesite kljuc za enkripciju: ");
            string encryptKey = Console.ReadLine();
            if(CheckUserExistence(userName))
            {
                if(File.Exists(filePath))
                {
                    if (VerifySignature(hostName, path, fileName))
                    {
                        string infoPath = path + @"\"+fileName+"data.txt";
                        using (StreamWriter stream = new StreamWriter(infoPath))
                        {
                            stream.Write("algoritam: ");
                            stream.WriteLine(algorithm);
                            stream.Write("lozinka: ");
                            stream.WriteLine(encryptKey);
                        }
                        string pubKeyPath = @"D:\Kriptografija\EncryptedFileSystem\EncryptedFileSystem\public\" + "Public" + userName + ".key";
                        string sharedPath = @"D:\Kriptografija\EncryptedFileSystem\EncryptedFileSystem\FileSystem\SharedDirectory\" + fileName + "data" + userName + ".txt";
                        string command = "openssl rsautl -encrypt -in " + infoPath + " -out " + sharedPath + " -inkey " + pubKeyPath + " -pubin";
                        CmdExecute(command);
                        string decryptPath = path + @"\" + "Decrypted" + fileName;
                        if (fileName.EndsWith("txt"))
                        {
                            string decryptCommand = "openssl des3 -d -in " + filePath + " -out " + decryptPath + " -k sigurnost";
                            CmdExecute(decryptCommand);
                            Thread.Sleep(1000);
                            string sharedFile = @"D:\Kriptografija\EncryptedFileSystem\EncryptedFileSystem\FileSystem\SharedDirectory\" +fileName+ userName + ".txt";
                            if(!File.Exists(sharedFile))
                            {
                                string encrpytCommand = "openssl " + algorithm + " -in " + decryptPath + " -out " + sharedFile + " -k " + encryptKey;
                                CmdExecute(encrpytCommand);
                            }
                            else
                            {
                                Console.WriteLine("Fajl je vec podjeljen.");
                            }

                        }
                        else if (fileName.EndsWith("docx"))
                        {
                            string decryptCommand = "openssl idea -d -in " + filePath + " -out " + decryptPath + " -k sigurnost";
                            CmdExecute(decryptCommand);
                            string sharedFile = @"D:\Kriptografija\EncryptedFileSystem\EncryptedFileSystem\FileSystem\SharedDirectory\" +fileName+ userName + ".docx";
                            if (!File.Exists(sharedFile))
                            {
                                string encrpytCommand = "openssl " + algorithm + " -in " + decryptPath + " -out " + sharedFile + " -k " + encryptKey;
                                CmdExecute(encrpytCommand);
                            }
                            else
                            {
                                Console.WriteLine("Fajl je vec podjeljen.");
                            }

                        }
                        else if (fileName.EndsWith("pdf"))
                        {
                            string decryptCommand = "openssl rc4 -d -in " + filePath + " -out " + decryptPath + " -k sigurnost";
                            CmdExecute(decryptCommand);
                            string sharedFile = @"D:\Kriptografija\EncryptedFileSystem\EncryptedFileSystem\FileSystem\SharedDirectory\" + fileName+ userName + ".pdf";
                            if (!File.Exists(sharedFile))
                            {
                                string encrpytCommand = "openssl " + algorithm + " -in " + decryptPath + " -out " + sharedFile + " -k " + encryptKey;
                                CmdExecute(encrpytCommand);

                            }
                            else
                            {
                                Console.WriteLine("Fajl je vec podjeljen.");
                            }

                        }
                        else if (fileName.EndsWith("png") || fileName.EndsWith("jpg"))
                        {
                            string decryptCommand = "openssl des -d -in " + filePath + " -out " + decryptPath + " -k sigurnost";
                            CmdExecute(decryptCommand);
                            string[] array = new string[2];
                            array = fileName.Split('.');
                            string sharedFile = @"D:\Kriptografija\EncryptedFileSystem\EncryptedFileSystem\FileSystem\SharedDirectory\" + fileName+userName + "."+array[1];
                            if (!File.Exists(sharedFile))
                            {
                                string encrpytCommand = "openssl " + algorithm + " -in " + decryptPath + " -out " + sharedFile + " -k " + encryptKey;
                                CmdExecute(encrpytCommand);
                            }
                            else
                            {
                                Console.WriteLine("Fajl je vec podjeljen.");
                            }

                        }
                        Thread.Sleep(1000);
                        File.Delete(infoPath);
                        File.Delete(decryptPath);
                    }
                    else
                    {
                        Console.WriteLine("Integritet narusen!");
                    }
                }
                else
                {
                    Console.WriteLine("Ne postoji uneseni fajl.");
                }
            }
            else
            {
                Console.WriteLine("Nije moguce podijeliti fajl. Korisnik ne postoji!");
            }
        }
        public void ReadFile(string path,string userName)
        {
            foreach (var array2 in Directory.GetFiles(@"D:\Kriptografija\EncryptedFileSystem\EncryptedFileSystem\FileSystem\SharedDirectory"))
            {
                if (!array2.Contains("data"))
                {
                    string[] array = array2.Split('\\');
                    Console.WriteLine(array[array.Length-1]);
                }
            }
            Console.WriteLine("Unesite naziv fajla koji zelite procitati:");
            string algorithm;
            string key;
            string fileName = Console.ReadLine();
            string sharedPath = @"D:\Kriptografija\EncryptedFileSystem\EncryptedFileSystem\FileSystem\SharedDirectory\" + fileName+"data" + userName + ".txt";
            if (File.Exists(sharedPath))
            {
                string privateKey = @"D:\Kriptografija\EncryptedFileSystem\EncryptedFileSystem\private\" + userName + ".key";
                string infoPath = @"D:\Kriptografija\EncryptedFileSystem\EncryptedFileSystem\FileSystem\SharedDirectory\" + fileName + "PlainTextData" + userName + ".txt";
                string decrypyCommand = "openssl rsautl -decrypt -in " + sharedPath + " -out " + infoPath + " -inkey " + privateKey;
                CmdExecute(decrypyCommand);
                Thread.Sleep(2000);
                using (StreamReader stream = new StreamReader(infoPath))
                {
                    algorithm = stream.ReadLine();
                    key = stream.ReadLine();
                }
                File.Delete(infoPath);
                algorithm = algorithm.Substring(11).Trim();
                key = key.Substring(9).Trim();
                string sharedFile = @"D:\Kriptografija\EncryptedFileSystem\EncryptedFileSystem\FileSystem\SharedDirectory\" + fileName + userName + ".";
                string targetPath = null;
                foreach (var ext in e.extensions)
                {
                    string newPath = sharedFile + ext;
                    if (File.Exists(newPath))
                    {
                        targetPath = newPath;
                        break;
                    }
                }
                if (targetPath.EndsWith("txt"))
                {
                    string plainText = @"D:\Kriptografija\EncryptedFileSystem\EncryptedFileSystem\FileSystem\SharedDirectory\" + "PlainText" + userName + ".txt";
                    string command = "openssl " + algorithm + " -d -in " + targetPath + " -out " + plainText + " -k " + key;
                    CmdExecute(command);
                    Process process = Process.Start("notepad.exe", plainText);
                    process.WaitForExit();
                }
                else if (targetPath.EndsWith("docx"))
                {
                    string plainText = @"D:\Kriptografija\EncryptedFileSystem\EncryptedFileSystem\FileSystem\SharedDirectory\" + "PlainText" + userName + ".docx";
                    string command = "openssl " + algorithm + " -d -in " + targetPath + " -out " + plainText + " -k " + key;
                    CmdExecute(command);
                    Process process = Process.Start("winword.exe", plainText);
                    process.WaitForExit();
                }
                else if (targetPath.EndsWith("pdf"))
                {
                    string plainText = @"D:\Kriptografija\EncryptedFileSystem\EncryptedFileSystem\FileSystem\SharedDirectory\" + "PlainText" + userName + ".pdf";
                    string command = "openssl " + algorithm + " -d -in " + targetPath + " -out " + plainText + " -k " + key;
                    CmdExecute(command);
                    Process process = Process.Start("acroRd32.exe", plainText);
                    process.WaitForExit();
                }
                else if (targetPath.EndsWith("jpg"))
                {
                    string plainText = @"D:\Kriptografija\EncryptedFileSystem\EncryptedFileSystem\FileSystem\SharedDirectory\" + "PlainText" + userName + ".jpg";
                    string command = "openssl " + algorithm + " -d -in " + targetPath + " -out " + plainText + " -k " + key;
                    CmdExecute(command);
                    Process process = Process.Start(plainText);
                    process.WaitForExit();
                }
                else if (targetPath.EndsWith("png"))
                {
                    string plainText = @"D:\Kriptografija\EncryptedFileSystem\EncryptedFileSystem\FileSystem\SharedDirectory\" + "PlainText" + userName + ".png";
                    string command = "openssl " + algorithm + " -d -in " + targetPath + " -out " + plainText + " -k " + key;
                    CmdExecute(command);
                    Process process = Process.Start(plainText);
                    process.WaitForExit();
                }

            }
            else
            {
                Console.WriteLine("Ne postoje fajlovi koje mozete procitati.");
            }
            Thread.Sleep(1000);
            foreach(var files in Directory.GetFiles(@"D:\Kriptografija\EncryptedFileSystem\EncryptedFileSystem\FileSystem\SharedDirectory\"))
            {
                    if(files.Contains("PlainText"))
                    {
                        File.Delete(files);
                        break;
                    }
            }

        }
        public void Menu(string path,string username)
        {
            string input;
            do
            {
                Console.WriteLine("1. Kreiranje novog teksutalnog fajla");
                Console.WriteLine("2. Otvaranje fajla");
                Console.WriteLine("3. Upload fajla");
                Console.WriteLine("4. Download fajla");
                Console.WriteLine("5. Izmjena sadrzaja teksutalnog fajla");
                Console.WriteLine("6. Brisanje fajla");
                Console.WriteLine("7. Komunikacija");
                Console.WriteLine("8. Izlaz");
                input =Console.ReadLine();
                Console.Clear();
                if (input == "1")
                {
                    CreateTextDocument(path,username);
                }
                else if (input == "2")
                {
                    OpenFile(path, username);
                }
                else if (input == "3")
                {
                    UploadFile(path,username);
                }
                else if (input == "4")
                {
                    DownloadFile(path, username);
                }
                else if (input == "5")
                {
                    ChangeFileContent(path, username);
                }
                else if (input == "6")
                {
                    DeleteFile(path, username);
                }
                else if (input == "7")
                {
                    SharedDirectoryOptions(path,username);
                }
                else if (input == "8")
                {
                    break;
                }
                else
                {
                    Console.WriteLine("Neispravan unos");
                }
            }
            while (true);

        }
        public bool CheckUserExistence(string username)
        {
            string filePath = @"D:\Kriptografija\EncryptedFileSystem\EncryptedFileSystem\database.txt";
            string line;
            string[] array = new string[2];
            using (StreamReader stream = new StreamReader(filePath))
            {
                while ((line = stream.ReadLine()) != null)
                {
                    array = line.Split(' ');
                    if (array[0] == username)
                    {
                        return true;
                    }
                    else
                    {
                        continue;
                    }
                }
                return false;
            }
        }
    }
}
