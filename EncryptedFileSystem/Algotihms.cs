using System.Collections.Generic;


namespace EncryptedFileSystem
{
    class Algotihms
    {
        public List<string> algorithms = new List<string>();
        public Algotihms()
        {
            algorithms.Add("rc4");
            algorithms.Add("des");
            algorithms.Add("idea");
            algorithms.Add("des3");
            algorithms.Add("aes-256-cbc");
            algorithms.Add("camellia-256-cbc");
        }
    }
}
