using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BCFileDecryptorCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BoxDecryptor.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DecryptController : ControllerBase
    {
        static BCFileDecryptor decryptor;
        public static Stopwatch sw;
        public static bool busy;

        public DecryptController() : base()
        {
            if (decryptor == null)
            {
                string pw = Environment.GetEnvironmentVariable("PW");
                string keyFilePath = Environment.GetEnvironmentVariable("KEY_PATH");
                decryptor = new BCFileDecryptor(keyFilePath, pw);
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
            return busy ? sw.Elapsed.TotalSeconds.ToString("N2") + "s [" + decryptor.currentDir + "]" : "idle" + Environment.NewLine;
        }

        [HttpGet]
        [Route("process")]
        public string Decrypt()
        {
            if (!decryptor.ready)
            {
                return "Decryptor not ready" + Environment.NewLine;
            }
            if (busy)
            {
                return GetInfo();
            }
            busy = true;

            //Start Decryption    
            string inputDir = Environment.GetEnvironmentVariable("INPUT_DIR");
            string outputDir = Environment.GetEnvironmentVariable("OUTPUT_DIR");

            Task.Run(() =>
            {
                sw.Reset();
                sw.Start();
                decryptor.currentDir = "deleting";
                Directory.Delete(outputDir, true);
                decryptor.DecryptDir(inputDir, outputDir);
                busy = false;
                sw.Stop();
            });

            return "Decryption started" + Environment.NewLine;
        }
    }
}
