using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;

class Program
{
    static void Main()
    {
        string plainText = new string('A', 10 * 1024 * 1024);

        var algorithms = new SymmetricAlgorithm[]
        {
            new AesCryptoServiceProvider { KeySize = 128 },
            new AesCryptoServiceProvider { KeySize = 256 },
            new AesManaged { KeySize = 128 },
            new AesManaged { KeySize = 256 },
            new RijndaelManaged { KeySize = 128 },
            new RijndaelManaged { KeySize = 256 },
            new DESCryptoServiceProvider(),
            new TripleDESCryptoServiceProvider()
        };

        Console.WriteLine("Algorithm\t\tKeySize (bits)\t\tEncrypt Time (Memory) (s)\tDecrypt Time (Memory) (s)\tEncrypt Speed (MB/s)\tDecrypt Speed (MB/s)\tEncrypt Time (Disk) (s)\tDecrypt Time (Disk) (s)\tEncrypt Speed (MB/s)\tDecrypt Speed (MB/s)");

        foreach (var algorithm in algorithms)
        {
            algorithm.GenerateKey();
            algorithm.GenerateIV();

            var encryptor = algorithm.CreateEncryptor();
            var decryptor = algorithm.CreateDecryptor();
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            byte[] cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
            stopwatch.Stop();
            double encryptTimeMemory = stopwatch.Elapsed.TotalSeconds;
            double encryptSpeedMemory = (plainBytes.Length / encryptTimeMemory) / (1024 * 1024); // MB/s

            stopwatch.Restart();
            byte[] decryptedBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
            stopwatch.Stop();
            double decryptTimeMemory = stopwatch.Elapsed.TotalSeconds;
            double decryptSpeedMemory = (plainBytes.Length / decryptTimeMemory) / (1024 * 1024); // MB/s

            string tempFilePath = Path.GetTempFileName();
            File.WriteAllText(tempFilePath, plainText);
            byte[] fileBytes = File.ReadAllBytes(tempFilePath);

            stopwatch.Restart();
            byte[] fileCipherBytes = encryptor.TransformFinalBlock(fileBytes, 0, fileBytes.Length);
            stopwatch.Stop();
            double encryptTimeDisk = stopwatch.Elapsed.TotalSeconds;
            double encryptSpeedDisk = (fileBytes.Length / encryptTimeDisk) / (1024 * 1024); // MB/s

            stopwatch.Restart();
            byte[] fileDecryptedBytes = decryptor.TransformFinalBlock(fileCipherBytes, 0, fileCipherBytes.Length);
            stopwatch.Stop();
            double decryptTimeDisk = stopwatch.Elapsed.TotalSeconds;
            double decryptSpeedDisk = (fileBytes.Length / decryptTimeDisk) / (1024 * 1024); // MB/s

            File.Delete(tempFilePath);

            Console.WriteLine($"{algorithm.GetType().Name,-30}\t{algorithm.KeySize,-15}\t{encryptTimeMemory,-30:F6}\t{decryptTimeMemory,-25:F6}\t{encryptSpeedMemory,-20:F6}\t{decryptSpeedMemory,-20:F6}\t{encryptTimeDisk,-25:F6}\t{decryptTimeDisk,-20:F6}\t{encryptSpeedDisk,-20:F6}\t{decryptSpeedDisk,-20:F6}");
        }

        Console.WriteLine("");
        Console.ReadLine();
    }
}

