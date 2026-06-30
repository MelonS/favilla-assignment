using System;

namespace Core
{
    /// <summary>
    /// HP의 한 시점 스냅샷. UI 표시에 필요한 읽기 전용 값 묶음.
    /// (값 타입 — 통지 시 불변 데이터만 전달해 외부에서 코어 상태를 바꿀 수 없게 한다.)
    /// </summary>
    public readonly struct HealthSnapshot
    {
        public int Current { get; }
        public int Max { get; }

        public HealthSnapshot(int current, int max)
        {
            Current = current;
            Max = max;
        }

        /// <summary>0~1로 정규화된 HP 비율. (HP바 채움 길이 계산용)</summary>
        public float Normalized => Max <= 0 ? 0f : (float)Current / Max;

        public bool IsDepleted => Current <= 0;
    }

    /// <summary>
    /// '읽기 + 관찰'만 노출하는 HP 추상.
    /// HP를 <b>보여주는</b> 객체(HPBar 등)는 오직 이 인터페이스에만 의존한다.
    /// → 표시 객체는 Unit/Champion/Tower 같은 구체 타입을 전혀 알 필요가 없다. (의존성 역전)
    /// </summary>
    public interface IHealthReadOnly
    {
        int Current { get; }
        int Max { get; }
        float Normalized { get; }
        bool IsAlive { get; }

        /// <summary>HP가 바뀔 때마다 최신 스냅샷을 통지한다. (구독자 = HPBar 등 표시 객체)</summary>
        event Action<HealthSnapshot> Changed;
    }

    /// <summary>
    /// 데미지를 <b>가하는</b> 쪽이 의존하는 추상.
    /// 공격 코드는 대상이 챔피언인지 타워인지 몰라도 IDamageable로만 피해를 준다.
    /// </summary>
    public interface IDamageable
    {
        void TakeDamage(int amount);
    }
}
