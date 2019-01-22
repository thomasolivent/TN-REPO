using UnityEngine;
using Vatio.UI;

namespace Vatio.Examples
{
    /// <summary>
    /// This script controls the submenus hierarchy.
    /// It's a good example on how to make a view hierarchy for the menu.
    /// Note the "SideMenuStateChanged(SideMenu.MenuState state)" method - it's called from the SideMenu script on it's state change.
    /// It enables us to show the main menu each time the menu is shown regardless of the state it was in when it was hidden.
    /// </summary>
    public class Submenu : MonoBehaviour
    {
        public GameObject[] menus;
        public GameObject topMenu;

        public void SideMenuStateChanged(SideMenu.MenuState state)
        {
            if (state == SideMenu.MenuState.Out)
                GoToMenu(topMenu);
        }

        public void HideAllMenus()
        {
            if (menus != null)
            {
                foreach (GameObject menu in menus)
                {
                    menu.SetActive(false);
                }
            }
        }

        public void GoToMenu(GameObject menu)
        {
            HideAllMenus();
            menu.SetActive(true);
        }

    }
}