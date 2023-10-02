using UnityEngine;
using UnityEngine.Serialization;

namespace _Project.Scripts.Match3.Game.UI
{
    public class UIManager : Manager
    {
        private ScreenType _currentPanel = ScreenType.Start;

        [Header("UI Screens")]
        [SerializeField] internal HUDScreen hudScreen;

        public void SetHUD()
        {
            SwitchPanel(ScreenType.HUDScreen);
        }

        private void SwitchPanel(ScreenType type)
        {
            // if(_currentPanel != ScreenType.Unset)
            //     FetchPanel(_currentPanel).CloseScreen();

            FetchPanel(type).OpenScreen();

            _currentPanel = type;
        }
        
        private IUIScreen FetchPanel(ScreenType panelType)
        {
            switch (panelType)
            {
                case ScreenType.HUDScreen:
                    return hudScreen;
                default:
                    Debug.LogWarning("ERROR: Could not fetch panel for some reason. Please check your code logic.");
                    return null;
            }
        }
        
    }
}