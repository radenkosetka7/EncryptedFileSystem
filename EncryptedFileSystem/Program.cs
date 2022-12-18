using System;
namespace EncryptedFileSystem
{
    class Program
    {

        static void Main(string[] args)
        {
            bool variable;
            User user = new User();
            Options option = new Options();
            do
            {
                string input;
                Console.Clear();
                Console.WriteLine("1. Prijava");
                Console.WriteLine("2. Registracija");
                Console.WriteLine("0. Izlaz");
                input = Console.ReadLine();
                Console.Clear();
                if (input == "1")
                {
                   variable=user.Login();
                    if (variable == true)
                    {
                        option.Menu(user.GetCurrentDir(), user.GetUsername());
                    }
                    else
                        continue;
                }
                else if (input == "2")
                {
                    user.Registration();
                }
                else if(input=="0")
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
    }
}
