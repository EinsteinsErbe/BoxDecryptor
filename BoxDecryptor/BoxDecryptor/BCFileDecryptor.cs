using Microsoft.Extensions.FileSystemGlobbing.Internal.PathSegments;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using static System.Buffer;

namespace BCFileDecryptorCore
{
    public class BCFileDecryptor
    {
        private byte[] decryptedPrivateKey;
        public bool ready = false;
        public string currentDir;

        public BCFileDecryptor(string keyFilePath, string password)
        {
            // ============================================
            // AES decryption of private key in .bckey file
            // =============================================

            try
            {
                // collect information about the user account
                AccountData accountInfo = new AccountData();
                accountInfo.ParseBCKeyFile(keyFilePath);
                accountInfo.Password = password;

                // decrypt the private key from the .bckey file
                decryptedPrivateKey = AESHelper.DecryptDataPBKDF2(
                    accountInfo.EncryptedPrivateKey, accountInfo.Password,
                    accountInfo.PBKDF2Salt, accountInfo.PBKDF2Iterations
                );
                ready = true;
            }
            catch (Exception)
            {

            }
        }

        public void DecryptDir(string inputDir, string outputDir)
        {
            currentDir = outputDir;
            Directory.CreateDirectory(outputDir);

            foreach (string inputFile in Directory.EnumerateFiles(inputDir, "*.bc"))
            {
                string outputFile = Path.Combine(outputDir, Path.GetFileNameWithoutExtension(inputFile));
                DecryptFile(inputFile, outputFile);
            }

            foreach (string dir in Directory.EnumerateDirectories(inputDir))
            {
                string outDir = Path.Combine(outputDir, Path.GetFileName(dir));
                DecryptDir(dir, outDir);
            }
        }

        public void DecryptFile(string inputFilePath, string outputFilePath)
        {
            try
            {
                //Console.WriteLine("Decryption process started");

                // =============================================
                // RSA decryption of file information (header)
                // =============================================

                // collect information about the file to be decrypted
                FileData fileData = new FileData();

                fileData.ParseHeader(inputFilePath, outputFilePath);

                // decrypt the file key (from the header) used for decryption of file data
                byte[] decryptedFileKey = RSAHelper.DecryptData(fileData.EncryptedFileKey, decryptedPrivateKey);

                byte[] fileCryptoKey = new byte[32];
                BlockCopy(decryptedFileKey, 32, fileCryptoKey, 0, 32);
                // =============================================
                // AES decryption of encrypted file
                // =============================================

                // decrypt the file data ...
                byte[] decryptedFileBytes = AESHelper.DecryptFile(
                    fileData.EncryptedFilePath, fileCryptoKey, fileData.BaseIVec,
                    fileData.BlockSize, fileData.HeaderLen, fileData.CipherPadding);

                File.WriteAllBytes(Path.GetFullPath(fileData.OutputFilePath), decryptedFileBytes);

                //Console.WriteLine($"Successfully decrypted file '{fileData.EncryptedFilePath}', "
                //+ $"output: '{fileData.OutputFilePath}'");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                if (e.StackTrace != null)
                {
                    Console.WriteLine(e.StackTrace);
                }
            }
        }
    }
}
