﻿using System;
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
        // some globals
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

                string loginCommand = $"LOGIN {username} {password}\r\n";
                byte[] commandBytes = Encoding.ASCII.GetBytes(loginCommand);
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



    }

}