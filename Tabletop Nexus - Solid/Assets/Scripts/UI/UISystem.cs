using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TabletopNexus.UI
{
    public class UISystem : MonoBehaviour
    {

        #region Variables
        [Header("Main Properties")]
        public UIScreen m_StartScreen;

        [Header("System Events")]
        public UnityEvent onSwitchedScreen = new UnityEvent();

        [Header("Fader Properties")]
        public Image m_Fader;
        public float m_FadeInDuration = 1f;
        public float m_FadeOutDuration = 1f;

        [SerializeField]
        UIScreen characterEditScreen;
        [SerializeField]
        GameObject characterEditCamera;
        [SerializeField]
        UIScreen dungeonEditScreen;
        [SerializeField]
        UIScreen dungeonGenScreen;
        [SerializeField]
        GameObject dungeonEditCamera;
        [SerializeField]
        UIScreen placeDungeonScreen;
        [SerializeField]
        GameObject DMPanel;
        [SerializeField]
        GameObject dungeonButton;
        [SerializeField]
        GameObject characterButton;

        private Component[] screens = new Component[0];

        private UIScreen previousScreen;
        public UIScreen PreviousScreen { get { return previousScreen; } }

        private UIScreen currentScreen;
        public UIScreen CurrentScreen { get { return currentScreen; } }
        #endregion

        #region Main Methods
        // Use this for initialization
        void Start()
        {
            screens = GetComponentsInChildren<UIScreen>(true);

            InitializeScreens();

            if (m_StartScreen)
            {
                SwitchScreens(m_StartScreen);
            }

            if (m_Fader)
            {
                m_Fader.gameObject.SetActive(true);
            }

            FadeIn();
        }
        #endregion

        #region Helper Methods
        public void SwitchScreens(UIScreen aScreen)
        {
            if (aScreen)
            {
                if (currentScreen)
                {
                    currentScreen.CloseScreen();
                    previousScreen = currentScreen;
                }

                currentScreen = aScreen;
                currentScreen.gameObject.SetActive(true);
                currentScreen.StartScreen();

                if(onSwitchedScreen != null)
                {
                    onSwitchedScreen.Invoke();
                }
            }

            if(aScreen == characterEditScreen)
            {
                characterEditCamera.SetActive(true);
            }
            else
            {
                characterEditCamera.SetActive(false);
            }

            if(aScreen == dungeonEditScreen || aScreen == dungeonGenScreen)
            {
                dungeonEditCamera.SetActive(true);
            }
            else
            {
                dungeonEditCamera.SetActive(false);
            }
        }

        public void ActiveStateChange(GameObject go)
        {
            if (go.activeSelf)
            {
                go.SetActive(false);
            }else
            {
                go.SetActive(true);
            }
        }

        public void FadeIn()
        {
            if (m_Fader)
            {
                m_Fader.CrossFadeAlpha(0f, m_FadeInDuration, false);
            }
        }

        public void FadeOut()
        {
            if (m_Fader)
            {
                m_Fader.CrossFadeAlpha(1f, m_FadeOutDuration, false);
            }
        }

        public void GoToPreviousScreen()
        {
            if (previousScreen)
            {
                SwitchScreens(previousScreen);
            }
        }

        public void LoadScene(int sceneIndex)
        {
            StartCoroutine(WaitToLoadScene(sceneIndex));
        }

        IEnumerator WaitToLoadScene(int sceneIndex)
        {
            yield return null;
        }

        void InitializeScreens()
        {
            foreach(var screen in screens)
            {
                screen.gameObject.SetActive(true);
            }
        }

        public void DMPanelSwitch()
        {
                if (DMPanel.activeSelf)
                {
                    DMPanel.SetActive(false);
                }
                else if (!DMPanel.activeSelf)
                {
                    DMPanel.SetActive(true);
                }
        }
        #endregion

        public void OpenCreditLink(int num)
        {
            switch (num)
            {
                case 1:
                    Application.OpenURL("");
                    break;
                case 2:
                    Application.OpenURL("http://terrac00p.com/");
                    break;
                case 3:
                    Application.OpenURL("chrisknightartist.myportfolio.com");
                    break;
                case 4:
                    Application.OpenURL("");
                    break;
                case 5:
                    Application.OpenURL("");
                    break;
                case 6:
                    Application.OpenURL("https://www.patreon.com/secretanorak");
                    break;
                default:
                    break;
            }
        }

        public void CharacterButton(bool b)
        {
            if (b)
            {
                characterButton.SetActive(true);
                Debug.Log("Character Button Should be Active.");
            }
            else if(!b)
            {
                characterButton.SetActive(false);
                Debug.Log("Character NOT");
            }
        }

        public void DungeonButton(bool b)
        {
            if (b)
            {
                dungeonButton.SetActive(true);
                Debug.Log("Dungeon Button Should be Active.");
            }
            else if(!b)
            {
                dungeonButton.SetActive(false);
                Debug.Log("Dungeon Not");
            }
        }
    }
}