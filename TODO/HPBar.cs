using Core;

namespace UI
{
    public class HPBar : DummyMonoBehaviour
    {
        private int _currentHP;

        public void RefreshHP(int newHP)
        {
            _currentHP = newHP;
        }
    }
}
