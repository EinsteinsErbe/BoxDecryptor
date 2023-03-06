using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BCFileDecryptorCore;
using CryptomatorTools.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BoxDecryptor.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DecryptController : ControllerBase
    {
        //static BCFileDecryptor decryptor;
        static CryptomatorHelper cryptomatorHelper;
        public static Stopwatch sw;
        public static bool busy;
        public static bool success = false;
        public static string inputDir;

        public DecryptController() : base()
        {
            if (cryptomatorHelper == null)
            {
                string pw = Environment.GetEnvironmentVariable("PW");
                string keyFilePath = Environment.GetEnvironmentVariable("KEY_PATH");
                inputDir = Environment.GetEnvironmentVariable("INPUT_DIR");
                //decryptor = new BCFileDecryptor(keyFilePath, pw);
                cryptomatorHelper = CryptomatorHelper.Create(pw, inputDir);
            }

            if (sw == null)
            {
                sw = new Stopwatch();
            }
        }

        [HttpGet]
        [Route("")]
        [Route("info")]
        public string GetInfo()
        {
            return busy ? sw.Elapsed.TotalSeconds.ToString("N2") + "s" : (success ? "success" : "ERROR") + Environment.NewLine;
        }

        [HttpGet]
        [Route("process")]
        public string Decrypt()
        {
            if (busy)
            {
                return GetInfo();
            }
            busy = true;
            success = false;
            //Start Decryption    

            string outputDir = Environment.GetEnvironmentVariable("OUTPUT_DIR");
            Console.WriteLine("Decrypt folder: " + inputDir);
            Task.Run(() =>
            {
                try
                {
                    sw.Reset();
                    sw.Start();
                    //decryptor.currentDir = "deleting";
                    if (Directory.Exists(outputDir))
                    {
                        Console.WriteLine("Delete folder: " + outputDir);
                        Directory.Delete(outputDir, true);
                    }
                    //decryptor.DecryptDir(inputDir, outputDir);
                    Console.WriteLine("Start decryption");
                    cryptomatorHelper.DecryptFolder("", outputDir);
                    success = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Decryption ERROR: " + e.Message + Environment.NewLine + e.StackTrace);
                }
                finally
                {
                    busy = false;
                    sw.Stop();
                    Console.WriteLine("End decryption");
                }

            });

            return "Decryption started" + Environment.NewLine;
        }
    }
}
