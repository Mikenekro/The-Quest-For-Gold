using System.Collections;
using System.Collections.Generic;
using TeamUtility.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace schoolRPG.splash
{
    public class SplashTimer : MonoBehaviour
    {
        [SerializeField]
        private float displayTime;
        private float curTime;

        private bool run;
        private WaitForSeconds wfs;

        // Use this for initialization
        void Start()
        {
            run = true;
            wfs = new WaitForSeconds(0.1f);
            curTime = 0;

            StartCoroutine(Timer());
        }

        public IEnumerator Timer()
        {
            while(curTime < displayTime)
            {
                yield return wfs;
                curTime += 0.1f;
                
                if (InputManager.anyKeyDown)
                {
                    curTime = displayTime;
                }
            }

            curTime = 0;
            run = false;
            CreationMenu.Creating = false;
            WorldController.Paused = false;
            WorldController.InMenu = false;
            LoadPersistant.loaded = false;

            SceneManager.LoadScene(1);
            yield return null;
        }
    }
}

