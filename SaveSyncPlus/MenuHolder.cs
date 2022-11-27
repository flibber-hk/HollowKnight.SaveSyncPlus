using MenuChanger;
using MenuChanger.Extensions;
using MenuChanger.MenuElements;
using MenuChanger.MenuPanels;
using RandomizerMod.Menu;
using System;
using static RandomizerMod.Localization;

namespace SaveSyncPlus
{
    internal class MenuHolder
    {
        internal MenuPage SspMenuPage;
        internal MultiGridItemPanel SspGIP;
        internal SmallButton JumpToSspButton;

        internal static MenuHolder Instance { get; private set; }

        public static void OnExitMenu() => Instance = null;

        public static void Hook()
        {
            RandomizerMenuAPI.AddMenuPage(ConstructMenu, HandleButton);
            MenuChangerMod.OnExitMainMenu += OnExitMenu;
        }

        private static bool HandleButton(MenuPage landingPage, out SmallButton button)
        {
            button = Instance.JumpToSspButton;
            return true;
        }

        private static void ConstructMenu(MenuPage landingPage) => Instance = new(landingPage);

        private void SetTopLevelButtonColor()
        {
            if (JumpToSspButton != null)
            {
                JumpToSspButton.Text.color = SaveSyncPlus.GS.IsEnabled() ? Colors.TRUE_COLOR : Colors.DEFAULT_COLOR;
            }
        }

        private MenuHolder(MenuPage landingPage)
        {
            SspMenuPage = new(Localize("SaveSyncPlus"), landingPage);

            SspGIP = new(SspMenuPage, 5, 3, 60f, 650f, new(0, 300), Array.Empty<IMenuElement>());
            ResetPanel();

            JumpToSspButton = new(landingPage, Localize("SaveSyncPlus"));
            JumpToSspButton.AddHideAndShowEvent(landingPage, SspMenuPage);
            SetTopLevelButtonColor();
        }

        private void ResetPanel()
        {
            SspGIP.Clear();

            foreach (string packName in SaveSyncPlus.GetPackNames())
            {
                ToggleButton button = new(SspMenuPage, packName);
                button.SetValue(SaveSyncPlus.GS.EnabledPackNames.Contains(packName));
                button.ValueChanged += v =>
                {
                    if (v) SaveSyncPlus.GS.EnabledPackNames.Add(packName);
                    else SaveSyncPlus.GS.EnabledPackNames.Remove(packName);
                    SetTopLevelButtonColor();
                };

                SspGIP.Add(button);
            }
        }

        internal void ResetMenu()
        {
            ResetPanel();
            SetTopLevelButtonColor();
        }
    }
}
