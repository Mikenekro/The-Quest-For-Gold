using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace schoolRPG
{
    public enum Quality
    {
        FASTEST, FAST, SIMPLE, GOOD, GREAT, AMAZING, CUSTOM
    }
    public enum AntiAliasing
    {
        NONE, X2, X4, X8, X16
    }
    public enum PixelLightCount
    {
        ZERO, ONE, TWO, FOUR, EIGHT
    }
    public enum TextureQuality
    {
        FULL, HALF, QUARTER, EIGHTH
    }
    public enum VSync
    {
        DONTSYNC, EVERYVBLANK, EVERYSECVBLANK
    }
    public enum RaycastBudget
    {
        FOUR, SIXTEEN, SIXTYFOUR, TWOFIFTYSIX, TENTWENTYFOUR, FOURTYNINETYSIX
    }
    public enum Framerate
    {
        DEFAULT, THIRTY, SIXTY, NINETY, UNLIMITED
    }

    [System.Serializable]
    public class GraphicData
    {
        public int resX;
        public int resY;
        public Quality curQuality;
        public PixelLightCount pixel;
        public TextureQuality texture;
        public AnisotropicFiltering ansi;
        public AntiAliasing antiA;
        public VSync vSync;
        public RaycastBudget rayBudget;
        public Framerate fps;
    }

    public class GraphicsMenu : MonoBehaviour
    {
        public static GraphicsMenu menu;
        private static GraphicData gd;
        private static bool fromNext = false;

        private Resolution curRes;
        private Resolution[] resolutions;
        private bool first = true;
        private List<int> addRes;
        private int resPos;

        [SerializeField]
        private TextMeshProUGUI qLevel;
        [SerializeField]
        private TextMeshProUGUI qRes;
        [SerializeField]
        private TextMeshProUGUI qPixel;
        [SerializeField]
        private TextMeshProUGUI qTexture;
        [SerializeField]
        private TextMeshProUGUI qAnsi;
        [SerializeField]
        private TextMeshProUGUI qAntiA;
        [SerializeField]
        private TextMeshProUGUI qVSync;
        [SerializeField]
        private TextMeshProUGUI qRayBudget;
        [SerializeField]
        private TextMeshProUGUI qFps;
        [SerializeField]
        private Button firstClick;

        public void Awake()
        {
            menu = this;
        }

        // Use this for initialization
        public void OnEnable()
        {
            bool good = true;
            menu = this;
            gd = new GraphicData();
            gd.curQuality = Quality.AMAZING;
            curRes = Screen.currentResolution;
            gd.resX = curRes.width;
            gd.resY = curRes.height;
            

            // Get a list of each Resolution the current monitor supports excluding multiples of the same resolution
            if (first)
            {
                first = false;
                addRes = new List<int>();
                resolutions = Screen.resolutions;

                // Loop through all resolutions supported
                for (int i = 0; i < resolutions.Length; ++i)
                {
                    good = true;
                    // Always add the first resolution
                    if (addRes.Count == 0)
                    {
                        addRes.Add(i);
                        continue;
                    }
                    // Loop through all of the resolution positions we have added
                    for (int j = 0; j < addRes.Count; ++j)
                    {
                        // If we find multiples of the same height 
                        if (resolutions[i].height == resolutions[addRes[j]].height)
                        {
                            // Don't add this resolution
                            good = false;
                            break;
                        }
                    }

                    // If we are good, add the resolution
                    if (good)
                        addRes.Add(i);
                }
                
                // Create a new array of the number of resolutions we can add
                resolutions = new Resolution[addRes.Count];

                // Loop through all of the positions we have added
                for (int i = 0; i < addRes.Count; ++i)
                {
                    // Set this resolution from the list of positions
                    resolutions[i] = Screen.resolutions[addRes[i]];

                    // If this is the same resolution that we are currently using
                    if (Screen.resolutions[addRes[i]].height == gd.resY && Screen.resolutions[addRes[i]].width == gd.resX)
                    {
                        // Store the resolution position
                        resPos = i;
                    }
                }
            }

            curRes = Screen.currentResolution;
            qRes.text = curRes.ToString();

            LoadSettings();
            //Apply(true, true);
            //SetQuality(gd.curQuality);

            StartCoroutine(HoverSelect.SelectFirstButton(firstClick));
        }
        
        /// <summary>
        /// Saves the Graphic Settings
        /// </summary>
        public void Save()
        {
            string _saveFullPath = "";
            string savePath = "";

            // Set each value for Saving and Loading games
            if (Application.isEditor)
                _saveFullPath = Application.dataPath;
            else
                _saveFullPath = Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "/QuestForGold";

            _saveFullPath = _saveFullPath.Replace("/Assets", "");
            savePath = _saveFullPath + "/GameData/GraphicData.sav";

            try
            {
                // Make sure the Directory exists
                if (!Directory.Exists(_saveFullPath + "/GameData/"))
                    Directory.CreateDirectory(_saveFullPath + "/GameData/");


                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Create(savePath);
                bf.Serialize(file, gd);
                file.Close();
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }
        /// <summary>
        /// Loads the last used Settings
        /// </summary>
        public static bool LoadSettings()
        {
            bool good = true;
            string _saveFullPath = "";
            string savePath = "";

            // Set each value for Saving and Loading games
            if (Application.isEditor)
                _saveFullPath = Application.dataPath;
            else
                _saveFullPath = Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "/QuestForGold";
            _saveFullPath = _saveFullPath.Replace("/Assets", "");
            savePath = _saveFullPath + "/GameData/GraphicData.sav";

            try
            {
                if (File.Exists(savePath))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    FileStream file = File.Open(savePath, FileMode.Open);
                    gd = (GraphicData)bf.Deserialize(file);
                    file.Close();

                    // Apply the settings we found
                    if (menu != null)
                    {
                        if (gd.curQuality == Quality.CUSTOM)
                            fromNext = true;
                        menu.SetQuality(gd.curQuality);
                    }
                    else
                        throw new System.Exception("Error: Graphics Options cannot be loaded! Menu not found...");
                    
                }
                else
                {
                    Debug.Log("Load Settings: Cannot Load Settings. No Graphic Settings have been saved or has been deleted!");
                }
                return good;
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex.Message);
                good = false;
                return good;    
            }
        }

        /// <summary>
        /// Apply the settings to the Game
        /// </summary>
        /// <param name="expensive"></param>
        public void Apply(bool expensive, bool fromList)
        {
            fromNext = false;

            // Set the Resolution of the screen 
            if (Screen.currentResolution.height != gd.resY || Screen.currentResolution.width != gd.resX)
            {
                Screen.SetResolution(gd.resX, gd.resY, true);
            }

            // Set Framerate
            if (gd.fps == Framerate.DEFAULT)
                Application.targetFrameRate = -1;
            else if (gd.fps == Framerate.THIRTY)
                Application.targetFrameRate = 30;
            else if (gd.fps == Framerate.SIXTY)
                Application.targetFrameRate = 60;
            else if (gd.fps == Framerate.NINETY)
                Application.targetFrameRate = 90;
            else
            {
                Application.targetFrameRate = -1;
                gd.vSync = VSync.DONTSYNC;
                qVSync.text = "Dont SYNC (Unlimited)";
            }

            PlayerPrefs.SetInt("FrameRate", Application.targetFrameRate);
            PlayerPrefs.SetInt("FrameRateEnum", (int)gd.fps);
            PlayerPrefs.Save();

            if (fromList)
            {
                QualitySettings.SetQualityLevel((int)gd.curQuality, expensive);
                //if (gd.curQuality != Quality.CUSTOM || !PlayerPrefs.HasKey("FrameRate"))
                //{
                //    Application.targetFrameRate = -1;
                //    gd.fps = Framerate.DEFAULT;
                //}
                //else if (PlayerPrefs.HasKey("FrameRate"))
                //{
                //    Application.targetFrameRate = PlayerPrefs.GetInt("FrameRate");
                //    gd.fps = (Framerate)PlayerPrefs.GetInt("FrameRateEnum");
                //}

                QualitySettings.vSyncCount = (int)gd.vSync;
                gd.texture = (TextureQuality)QualitySettings.masterTextureLimit;
                gd.ansi = QualitySettings.anisotropicFiltering;

                if (QualitySettings.pixelLightCount == 0)
                    gd.pixel = PixelLightCount.ZERO;
                else if (QualitySettings.pixelLightCount == 1)
                    gd.pixel = PixelLightCount.ONE;
                else if (QualitySettings.pixelLightCount == 2)
                    gd.pixel = PixelLightCount.TWO;
                else if (QualitySettings.pixelLightCount == 4)
                    gd.pixel = PixelLightCount.FOUR;
                else if (QualitySettings.pixelLightCount == 8)
                    gd.pixel = PixelLightCount.EIGHT;

                if (QualitySettings.particleRaycastBudget == 4)
                    gd.rayBudget = RaycastBudget.FOUR;
                else if (QualitySettings.particleRaycastBudget == 16)
                    gd.rayBudget = RaycastBudget.SIXTEEN;
                else if (QualitySettings.particleRaycastBudget == 64)
                    gd.rayBudget = RaycastBudget.SIXTYFOUR;
                else if (QualitySettings.particleRaycastBudget == 246)
                    gd.rayBudget = RaycastBudget.TWOFIFTYSIX;
                else if (QualitySettings.particleRaycastBudget == 1024)
                    gd.rayBudget = RaycastBudget.TENTWENTYFOUR;
                else if (QualitySettings.particleRaycastBudget == 4096)
                    gd.rayBudget = RaycastBudget.FOURTYNINETYSIX;

                if (QualitySettings.antiAliasing == 0)
                    gd.antiA = AntiAliasing.NONE;
                else if (QualitySettings.antiAliasing == 2)
                    gd.antiA = AntiAliasing.X2;
                else if (QualitySettings.antiAliasing == 4)
                    gd.antiA = AntiAliasing.X4;
                else if (QualitySettings.antiAliasing == 8)
                    gd.antiA = AntiAliasing.X8;
            }
            else
            {
                QualitySettings.SetQualityLevel((int)gd.curQuality, expensive);

                //if (gd.fps == Framerate.DEFAULT)
                //    Application.targetFrameRate = -1;
                //else if (gd.fps == Framerate.THIRTY)
                //    Application.targetFrameRate = 30;
                //else if (gd.fps == Framerate.SIXTY)
                //    Application.targetFrameRate = 60;
                //else if (gd.fps == Framerate.NINETY)
                //    Application.targetFrameRate = 90;
                //else
                //{
                //    Application.targetFrameRate = -1;
                //    gd.vSync = VSync.DONTSYNC;
                //    qVSync.text = "Dont SYNC (Unlimited)";
                //}

                //PlayerPrefs.SetInt("FrameRate", Application.targetFrameRate);
                //PlayerPrefs.SetInt("FrameRateEnum", (int)gd.fps);
                //PlayerPrefs.Save();

                if (gd.rayBudget == RaycastBudget.FOUR)
                    QualitySettings.particleRaycastBudget = 4;
                else if (gd.rayBudget == RaycastBudget.SIXTEEN)
                    QualitySettings.particleRaycastBudget = 16;
                else if (gd.rayBudget == RaycastBudget.SIXTYFOUR)
                    QualitySettings.particleRaycastBudget = 64;
                else if (gd.rayBudget == RaycastBudget.TWOFIFTYSIX)
                    QualitySettings.particleRaycastBudget = 246;
                else if (gd.rayBudget == RaycastBudget.TENTWENTYFOUR)
                    QualitySettings.particleRaycastBudget = 1024;
                else if (gd.rayBudget == RaycastBudget.FOURTYNINETYSIX)
                    QualitySettings.particleRaycastBudget = 4096;

                if (gd.antiA == AntiAliasing.NONE)
                    QualitySettings.antiAliasing = 0;
                else if (gd.antiA == AntiAliasing.X2)
                    QualitySettings.antiAliasing = 2;
                else if (gd.antiA == AntiAliasing.X4)
                    QualitySettings.antiAliasing = 4;
                else if (gd.antiA == AntiAliasing.X8)
                    QualitySettings.antiAliasing = 8;
                else if (gd.antiA == AntiAliasing.X16)
                    QualitySettings.antiAliasing = 16;

                if (gd.pixel == PixelLightCount.ZERO)
                    QualitySettings.pixelLightCount = 0;
                else if (gd.pixel == PixelLightCount.ONE)
                    QualitySettings.pixelLightCount = 1;
                else if (gd.pixel == PixelLightCount.TWO)
                    QualitySettings.pixelLightCount = 2;
                else if (gd.pixel == PixelLightCount.FOUR)
                    QualitySettings.pixelLightCount = 4;
                else if (gd.pixel == PixelLightCount.EIGHT)
                    QualitySettings.pixelLightCount = 8;

                QualitySettings.masterTextureLimit = (int)gd.texture;
                QualitySettings.anisotropicFiltering = gd.ansi;
                QualitySettings.vSyncCount = (int)gd.vSync;
            }
        }

        public void SetQuality(bool next)
        {
            bool good = true;

            if (next)
            {
                if (gd.curQuality != Quality.CUSTOM)
                    gd.curQuality++;
                else
                    good = false;
            }
            else
            {
                if (gd.curQuality != Quality.FASTEST)
                    gd.curQuality--;
                else
                    good = false;
            }

            

            if (good)
            {
                fromNext = true;
                SetQuality(gd.curQuality);
            }
        }

        public void SetQuality(Quality q)
        {
            int frame = 0;
            gd.curQuality = q;
            qLevel.text = gd.curQuality.ToString().Substring(0, 1).ToUpper() + gd.curQuality.ToString().Substring(1).ToLower();
            
            if (gd.curQuality != Quality.CUSTOM || fromNext)
                Apply(true, true);

            qRes.text = curRes.ToString();

            if (gd.ansi == AnisotropicFiltering.Disable)
                qAnsi.text = "Disabled";
            else if (gd.ansi == AnisotropicFiltering.Enable)
                qAnsi.text = "Enabled";
            else
                qAnsi.text = "Force Enabled";

            qAntiA.text = gd.antiA.ToString().ToLower() + " AA";

            if (gd.pixel == PixelLightCount.ZERO)
                qPixel.text = "None";
            else if (gd.pixel == PixelLightCount.ONE)
                qPixel.text = "1 Pixel Light";
            else if (gd.pixel == PixelLightCount.TWO)
                qPixel.text = "2 Pixel Lights";
            else if (gd.pixel == PixelLightCount.FOUR)
                qPixel.text = "4 Pixel Lights";
            else if (gd.pixel == PixelLightCount.EIGHT)
                qPixel.text = "8 Pixel Lights";

            if (gd.rayBudget == RaycastBudget.FOUR)
                qRayBudget.text = "4 Collision Rays";
            else if (gd.rayBudget == RaycastBudget.SIXTEEN)
                qRayBudget.text = "16 Collision Rays";
            else if (gd.rayBudget == RaycastBudget.SIXTYFOUR)
                qRayBudget.text = "64 Collision Rays";
            else if (gd.rayBudget == RaycastBudget.TWOFIFTYSIX)
                qRayBudget.text = "256 Collision Rays";
            else if (gd.rayBudget == RaycastBudget.TENTWENTYFOUR)
                qRayBudget.text = "1024 Collision Rays";
            else if (gd.rayBudget == RaycastBudget.FOURTYNINETYSIX)
                qRayBudget.text = "4096 Collision Rays";

            qTexture.text = gd.texture.ToString().Substring(0, 1).ToUpper() + gd.texture.ToString().Substring(1).ToLower() + " Res";

            if (gd.fps == Framerate.DEFAULT)
                qFps.text = "Default";
            else if (gd.fps == Framerate.THIRTY)
                qFps.text = "30 FPS";
            else if (gd.fps == Framerate.SIXTY)
                qFps.text = "60 FPS";
            else if (gd.fps == Framerate.NINETY)
                qFps.text = "90 FPS";
            else if (gd.fps == Framerate.UNLIMITED)
                qFps.text = "Unlimited";
            

            if (gd.vSync == VSync.DONTSYNC)
                qVSync.text = "Dont SYNC (Unlimited)";
            else if (gd.vSync == VSync.EVERYVBLANK && gd.fps != Framerate.UNLIMITED)
                qVSync.text = "Every V Blank";
            else
                qVSync.text = "Every Second V Blank";

            if (int.TryParse(qFps.text.Substring(0, 2), out frame) && gd.vSync != VSync.DONTSYNC)
            {
                if (gd.vSync == VSync.EVERYVBLANK)
                    qVSync.text += " (" + frame.ToString() + " at " + qFps.text + ")";
                else if (gd.vSync == VSync.EVERYSECVBLANK)
                    qVSync.text += " (" + (frame / 2).ToString() + " at " + qFps.text + ")";
            }

            if (gd.curQuality == Quality.CUSTOM)
                Apply(true, false);

            qRes.text = curRes.ToString();
        }

        public void SetResolution(bool next)
        {
            bool good = true;

            if (next)
            {
                if (resPos < resolutions.Length - 1)
                    resPos++;
                else
                    good = false;
            }
            else
            {
                if (resPos > 0)
                    resPos--;
                else
                    good = false;
            }

            curRes = resolutions[resPos];
            gd.resX = curRes.width;
            gd.resY = curRes.height;
            Screen.SetResolution(gd.resX, gd.resY, true);

            Debug.Log("Set Resolution: " + Screen.currentResolution);

            // Resolution does not change the Quality
            if (good)
                SetQuality(gd.curQuality);
        }

        public void SetPixelLight(bool next)
        {
            bool good = true;

            if (next)
            {
                if (gd.pixel != PixelLightCount.EIGHT)
                    gd.pixel++;
                else
                    good = false;
            }
            else
            {
                if (gd.pixel != PixelLightCount.ZERO)
                    gd.pixel--;
                else
                    good = false;
            }

            if (good)
                SetQuality(Quality.CUSTOM);
        }

        public void SetTexture(bool next)
        {
            bool good = true;

            if (next)
            {
                if (gd.texture != TextureQuality.FULL)
                    gd.texture--;
                else
                    good = false;
            }
            else
            {
                if (gd.texture != TextureQuality.EIGHTH)
                    gd.texture++;
                else
                    good = false;
            }

            if (good)
                SetQuality(Quality.CUSTOM);
        }

        public void SetAnsi(bool next)
        {
            bool good = true;

            if (next)
            {
                if (gd.ansi != AnisotropicFiltering.ForceEnable)
                    gd.ansi++;
                else
                    good = false;
            }
            else
            {
                if (gd.ansi != AnisotropicFiltering.Disable)
                    gd.ansi--;
                else
                    good = false;
            }

            if (good)
                SetQuality(Quality.CUSTOM);
        }

        public void SetAntiAliasing(bool next)
        {
            bool good = true;

            if (next)
            {
                if (gd.antiA != AntiAliasing.X8)
                    gd.antiA++;
                else
                    good = false;
            }
            else
            {
                if (gd.antiA != AntiAliasing.NONE)
                    gd.antiA--;
                else
                    good = false;
            }

            if (good)
                SetQuality(Quality.CUSTOM);
        }

        public void SetVSync(bool next)
        {
            bool good = true;

            if (next)
            {
                if (gd.vSync != VSync.EVERYSECVBLANK)
                    gd.vSync++;
                else
                    good = false;
            }
            else
            {
                if (gd.vSync != VSync.DONTSYNC)
                    gd.vSync--;
                else
                    good = false;
            }

            // VSync does not change quality
            if (good)
                SetQuality(gd.curQuality);
        }

        public void SetRayBudget(bool next)
        {
            bool good = true;

            if (next)
            {
                if (gd.rayBudget != RaycastBudget.FOURTYNINETYSIX)
                    gd.rayBudget++;
                else
                    good = false;
            }
            else
            {
                if (gd.rayBudget != RaycastBudget.FOUR)
                    gd.rayBudget--;
                else
                    good = false;
            }

            if (good)
                SetQuality(Quality.CUSTOM);
        }

        public void SetFramerate(bool next)
        {
            bool good = true;

            if (next)
            {
                if (gd.fps != Framerate.UNLIMITED)
                    gd.fps++;
                else
                    good = false;
            }
            else
            {
                if (gd.fps != Framerate.DEFAULT)
                    gd.fps--;
                else
                    good = false;
                
            }

            // Framerate does not change Quality
            if (good)
                SetQuality(gd.curQuality);
        }
    }
    
}

