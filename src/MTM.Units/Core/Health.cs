using System;

namespace Core
{
    /// <summary>
    /// HP의 '작동하는 코어' 객체.
    /// 값의 변경 규칙(클램프/사망 판정)을 책임지고, "바뀌었다"는 사실만 이벤트로 통지한다.
    /// UI(HPBar)에 대한 의존성은 전혀 없다. — 관찰자(Observer) 패턴 + 의존성 역전(DIP).
    /// 유닛은 이 객체를 '상속'이 아니라 '조합(composition)'으로 보유한다.
    /// </summary>
    public sealed class Health : IHealthReadOnly, IDamageable
    {
        public int Max { get; private set; }
        public int Current { get; private set; }

        public float Normalized => Max <= 0 ? 0f : (float)Current / Max;
        public bool IsAlive => Current > 0;

        /// <summary>HP가 변할 때마다 최신 스냅샷을 통지. (구독자 = HPBar 등)</summary>
        public event Action<HealthSnapshot> Changed;

        /// <summary>HP가 0이 되어 사망한 순간 1회 통지. (Unit이 사망 처리에 사용)</summary>
        public event Action Died;

        public Health(int max)
        {
            Max = Math.Max(1, max);
            Current = Max;
        }

        public void TakeDamage(int amount)
        {
            if (amount <= 0 || !IsAlive) return;
            Apply(Current - amount);
            if (!IsAlive) Died?.Invoke();
        }

        public void Heal(int amount)
        {
            if (amount <= 0 || !IsAlive) return;
            Apply(Current + amount);
        }

        /// <summary>부활/리셋 시 최대치로 회복. (사망 상태에서도 호출 가능)</summary>
        public void ResetToFull() => Apply(Max);

        public void SetMax(int max, bool refill = false)
        {
            int newMax = Math.Max(1, max);
            bool maxChanged = newMax != Max;
            Max = newMax;
            int before = Current;
            Apply(refill ? Max : Current);
            // Max만 바뀌고 Current가 그대로면 Apply가 통지하지 않으므로, 분모(Max) 갱신을 위해 1회 강제 통지.
            if (maxChanged && Current == before)
                Changed?.Invoke(new HealthSnapshot(Current, Max));
        }

        private void Apply(int next)
        {
            int clamped = next < 0 ? 0 : (next > Max ? Max : next);
            if (clamped == Current) return;   // 값 변화가 없으면 통지하지 않는다.
            Current = clamped;
            Changed?.Invoke(new HealthSnapshot(Current, Max));
        }
    }
}
