using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Project.Scripts.Match3.Game.UI
{
    public class UIManager : Manager
    {
        private ScreenType _currentPanel = ScreenType.Start;

        [Header("UI Screens")]
        [SerializeField] internal HUDScreen hudScreen;
        [SerializeField] internal WinScreen winScreen;

        public override void Setup()
        {
            _currentPanel = ScreenType.Unset;
        }

        public void SetHUD()
        {
            SwitchPanel(ScreenType.HUDScreen);
        }

        public void SetWinScreen()
        {
            SwitchPanel(ScreenType.WinScreen);
        }

        private void SwitchPanel(ScreenType type)
        {
             if(_currentPanel != ScreenType.Unset)
                 FetchPanel(_currentPanel).CloseScreen();

             FetchPanel(type).OpenScreen();

             _currentPanel = type;
        }
        
        private IUIScreen FetchPanel(ScreenType panelType)
        {
            switch (panelType)
            {
                case ScreenType.HUDScreen:
                    return hudScreen;
                case ScreenType.WinScreen:
                    return winScreen;
                default:
                    Debug.LogWarning("ERROR: Could not fetch panel for some reason. Please check your code logic.");
                    return null;
            }
        }
        
    }
}