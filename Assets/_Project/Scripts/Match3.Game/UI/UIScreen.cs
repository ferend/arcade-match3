using UnityEngine;
using UnityEngine.Serialization;

namespace _Project.Scripts.Match3.Game.UI
{
    public interface  IUIScreen
    {
        public virtual void OpenScreen()
        {
            PlayOpenAnim();
        }

        public virtual void CloseScreen()
        {
            PlayCloseAnim();
        }

        public void PlayOpenAnim();
        public void PlayCloseAnim();

    }
}
