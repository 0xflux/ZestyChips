using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ZestyChips
{
    internal class Program
    {
        // some globals
        private static TcpClient client = new TcpClient();

        static void Main(string[] args)
        {
            Helpers.PrintInfo("starting ZestyChips...");

            string name = AppDomain.CurrentDomain.BaseDirectory + AppDomain.CurrentDomain.FriendlyName;

            // cheat the hardcoded IP
            string c2_ip = "127.0.0.1";
            int c2_port = 143;
            string username = "username";
            string password = "super_secure_!pw123$#@";

            // establish connection to c2
            if (Connect(c2_ip, c2_port))
            {
                Helpers.PrintSuccess("connected to c2 server.");

                if (Login(username, password))
                {
                    Helpers.PrintSuccess("login to IMAP successful.");
                    // todo from here..

                }
            }

            Helpers.PrintInfo("ZestyChips finshed. Press any key to continue...");

            // stop from exiting for debug
            Console.ReadKey();
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
                // IMAP login requires a tag before the command - usually alphanumeric
                string loginCommand = $"a1 LOGIN {username} {password}\r\n";
                byte[] commandBytes = Encoding.ASCII.GetBytes(loginCommand);
                stream.Write(commandBytes, 0, commandBytes.Length);

                // read server response
                byte[] response = new byte[256];
                int bytes = stream.Read(response, 0, response.Length);
                string responseString = Encoding.ASCII.GetString(response, 0, bytes);

                // if login is successful
                if (responseString.Contains("a1 OK"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                Helpers.PrintFail("failed to login to IMAP server.");
                return false;
            }
        }

    }

}