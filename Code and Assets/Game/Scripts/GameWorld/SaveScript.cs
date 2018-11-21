using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace schoolRPG.SaveLoad
{

    /// <summary>
    /// Contains all the functions to Save and Load a game
    /// </summary>
    public class SaveScript
    {
        private static int _totalSaves;
        private static bool _loadVals;
        private static string _saveFullPath;
        private static string _savePath;
        private static string _realPath;
        private static string _autoPrefix;
        private static string _saveExt;


        public static bool Loading { get; set; }
        /// <summary>
        /// Total times every game has been saved
        /// </summary>
        public static int TotalSaves { get { return _totalSaves; } }
        /// <summary>
        /// The full Save Path to the game folder
        /// </summary>
        public static string SaveFullPath { get { return _saveFullPath; } }
        /// <summary>
        /// The Path where we will find the saved games at
        /// </summary>
        public string SavePath { get { return _savePath; } }
        /// <summary>
        /// The Prefix for AutoSaved Games
        /// </summary>
        public string AutoSavePrefix { get { return _autoPrefix; } }
        /// <summary>
        /// The Extension for the SavedGame Files (Ex. .sav)
        /// </summary>
        public static string SaveExtension { get { return _saveExt; } }

        public SaveScript()
        {
            LoadVals();
        }

        /// <summary>
        /// Loads any values that the Save Script needs to function correctly
        /// </summary>
        private static void LoadVals()
        {
            // Set each value for Saving and Loading games
            if (Application.isEditor)
                _saveFullPath = Application.dataPath;
            else
               _saveFullPath = Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "/QuestForGold";
            
            _savePath = "/SavedGames/";
            _saveExt = ".sav";
            _autoPrefix = "AutoSave_";
            _saveFullPath = _saveFullPath.Replace("/Assets", "");
            _realPath = _saveFullPath + _savePath;

            // Make sure the Directory exists
            if (!Directory.Exists(_saveFullPath + _savePath))
                Directory.CreateDirectory(_saveFullPath + _savePath);

            _loadVals = true;
        }

        /// <summary>
        /// Gets the latest AutoSave from the SavedGames directory
        /// </summary>
        /// <returns>The name of the latest AutoSave (Without .sav)</returns>
        public static string GetAutoSaveName()
        {
            string sName = "";
            string[] files;
            string file;
            DateTime time;
            DateTime lastTime = new DateTime(1, 1, 1);

            try
            {
                files = Directory.GetFiles(_realPath, _autoPrefix + "*");

                for (int i = 0; i < files.Length; ++i)
                {
                    file = files[i].Replace(_realPath, "").Replace(_saveExt, "");
                    //time = File.GetLastAccessTime(_realPath + files[i]);
                    time = DateTime.ParseExact(file.Replace(_autoPrefix, ""), "MM-dd-yyyy", null);

                    // Get the latest autosave
                    if (time > lastTime)
                    {
                        lastTime = time;
                        sName = file;
                    }
                }

                return sName;
            }
            catch
            {
                Debug.Log("Could not find an Auto Saved Game");
                return null;
            }
        }

        /// <summary>
        /// Call this function when we want to autosave the game
        /// </summary>
        /// <returns>If saving was successful</returns>
        public bool AutoSave(PlayerData data)
        {
            DateTime current;
            string date = "";
            bool good = false;

            current = DateTime.Now;

            date = current.Month.ToString("00") + "-" + current.Day.ToString("00") + "-" + current.Year;

            good = SaveData(_autoPrefix + date + _saveExt, data);

            return good;
        }
        

        /// <summary>
        /// Saves a New Game
        /// </summary>
        /// <returns>If saving was successful</returns>
        public bool SaveGame(PlayerData data)
        {
            // Call the SaveGame that overrides old saves since it will not override if the name does not exist
            bool good = SaveGame("FakeName", data);

            return good;
        }

        /// <summary>
        /// Overrides the oldName with the newName and saves the current data
        /// </summary>
        /// <param name="oldName">The Saved Game that we want to replace</param>
        /// <returns>If saving was successful</returns>
        public bool SaveGame(string oldName, PlayerData data)
        {
            bool good = false;
            string pName = data.Player.Name;
            string newName, newPath;
            string savePath = _saveFullPath + _savePath;
            // Set the old path for the saved game
            oldName = savePath + oldName;

            // Load the current count for total saved games
            LoadTotals();
            // Increment that count by 1
            _totalSaves += 1;
            // Save the current count for total saved games
            SaveTotals();

            // Set the new name of the Saved Game
            pName = pName.Replace(" ", "-");
            newName = "Save" + _totalSaves + "_Level" + data.Player.Level + "_" + pName + _saveExt;

            // Set the new path for the saved game
            newPath = savePath + newName;
            try
            {
                // Rename the file if it exists
                if (File.Exists(oldName))
                    File.Move(oldName, newPath);

                // Save the new file
                good = SaveData(newName, data);
            }
            catch
            {
                good = false;
                return good;
            }

            return good;
        }

        /// <summary>
        /// Saves the current PlayerData. Returns True if save was sucessfull
        /// </summary>
        /// <returns></returns>
        private bool SaveData(string saveName, PlayerData data)
        {
            bool good = false;
            string savePath = _saveFullPath + _savePath;

            savePath = savePath + saveName;
            // Don't identify as loaded before we save
            data.SetLoaded(false);

            Debug.Log("Seconds Left In Day: " + data.SecondsLeftInDay + ", out of: " + data.SecondsInDay);

            try
            {
                // Serialize and Save the current Game
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Create(savePath);
                bf.Serialize(file, data);
                file.Close();

                good = true;
            }
            catch
            {
                good = false;
                return good;
            }

            return good;
        }

        /// <summary>
        /// Loads the specified PlayerData from the Saved Games folder. Returns True if load was sucessfull
        /// </summary>
        /// <param name="loadName"></param>
        /// <returns></returns>
        public PlayerData LoadData(string loadName)
        {
            string savePath = _saveFullPath + _savePath + loadName + _saveExt;
            PlayerData data = new PlayerData();
            
            try
            {
                if (File.Exists(savePath))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    FileStream file = File.Open(savePath, FileMode.Open);
                    data = (PlayerData)bf.Deserialize(file);
                    file.Close();

                    Loading = true;
                    // Set the data as being loaded
                    data.SetLoaded(true);
                    return data;
                }
            }
            catch
            {
            }

            return data;
        }

        /// <summary>
        /// Saves the Total times we have saved a game
        /// </summary>
        private void SaveTotals()
        {
            string savePath = _saveFullPath + "/GameData/Totals" + _saveExt;

            try
            {
                if (!Directory.Exists(_saveFullPath + "/GameData/"))
                    Directory.CreateDirectory(_saveFullPath + "/GameData/");


                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Create(savePath);
                bf.Serialize(file, _totalSaves);
                file.Close();

                Debug.Log("Total Saved Games: " + _totalSaves);
            }
            catch
            {
            }
        }
        /// <summary>
        /// Loads the Total times we have saved a game
        /// </summary>
        private void LoadTotals()
        {
            string savePath = _saveFullPath + "/GameData/Totals" + _saveExt;

            try
            {
                if (File.Exists(savePath))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    FileStream file = File.Open(savePath, FileMode.Open);
                    _totalSaves = (int)bf.Deserialize(file);
                    file.Close();
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Returns an array with each saved game in the SavedGames directory and sorts 
        /// each saved game by the last time we accessed it
        /// </summary>
        /// <returns>Array of each Saved Game</returns>
        public static string[] AllSaves()
        {
            int i;
            int k;
            string allSav;
            string temp;
            string[] saves;
            DateTime current;
            DateTime next;

            // Make sure we have the correct values loaded before searching for Saved Games
            if (!_loadVals)
                LoadVals();

            // Set the search pattern and find the file names
            allSav = "*" + _saveExt;
            saves = Directory.GetFiles((_saveFullPath + _savePath), allSav);

            // Sort each saved game by the last game we accessed
            // This way, the user sees the last character they used
            for (i = 0; i < saves.Length - 1; ++i)
            {
                for (k = 0; k < saves.Length - 1; ++k)
                {
                    // Get the write times for each save
                    current = File.GetLastWriteTime(saves[k]);
                    next = File.GetLastWriteTime(saves[k + 1]);
                    // Switch the positions if the next access time is after the current access time
                    if ((next.Date > current.Date) || (next.Date == current.Date && next.TimeOfDay > current.TimeOfDay))
                    {
                        temp = saves[k];
                        saves[k] = saves[k + 1];
                        saves[k + 1] = temp;
                    }
                }
            }

            return saves;
        }


        /// <summary>
        /// Deletes the specified file 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public bool DeleteFile(string file)
        {
            bool good = false;

            if (File.Exists(file))
            {
                File.Delete(file);
                good = true;
            }
            else
            {
                good = false;
            }

            Refresh();
            return good;
        }

        private static void Refresh()
        {
            #if UNITY_EDITOR
                UnityEditor.AssetDatabase.Refresh();
            #endif
        }

    }
}

