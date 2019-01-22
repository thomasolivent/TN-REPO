using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Vatio.Examples
{
    /// <summary>
    /// This script loads the appropriate scene when called from a menu button.
    /// </summary>
    public class MenuButtons : MonoBehaviour
    {
        private enum ExampleScene
        {
            MainMenu,
            SingleMenu,
            MultipleMenus,
            Submenu
        }

        private static readonly Dictionary<ExampleScene, string> sceneNameMapping = new Dictionary<ExampleScene, string>()
        {
            { ExampleScene.MainMenu, "MainMenu" },
            { ExampleScene.SingleMenu, "SingleMenu" },
            { ExampleScene.MultipleMenus, "MultipleMenus" },
            { ExampleScene.Submenu, "Submenu" }
        };

        private static void LoadScene(ExampleScene scene)
        {
            SceneManager.LoadScene(sceneNameMapping[scene]);
        }

        public void LoadMainMenu()
        {
            LoadScene(ExampleScene.MainMenu);
        }

        public void LoadSingleMenu()
        {
            LoadScene(ExampleScene.SingleMenu);
        }

        public void LoadMultipleMenus()
        {
            LoadScene(ExampleScene.MultipleMenus);
        }

        public void LoadSubmenu()
        {
            LoadScene(ExampleScene.Submenu);
        }
    }
}
