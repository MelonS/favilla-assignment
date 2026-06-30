using System;
using Core;

namespace UI
{
    /// <summary>
    /// 임의 유닛의 HP를 <b>보여주기만</b> 하는 UI 오브젝트. (과제 제공 파일을 확장)
    ///
    /// 핵심: 유닛/챔피언/타워 같은 <b>구체 타입을 전혀 모른 채</b>,
    /// 관찰 가능한 HP 추상(IHealthReadOnly)에만 의존한다.
    ///   - 표시(HPBar)  →  추상(IHealthReadOnly)  ←  코어(Health)  ←  Unit
    ///   - 의존성은 모두 '추상'을 향하고, 유닛은 자신을 그리는 HPBar의 존재를 모른다.
    /// </summary>
    public class HPBar : DummyMonoBehaviour
    {
        private int _currentHP;
        private int _maxHP = 1;
        private IHealthReadOnly _source;

        /// <summary>렌더러가 읽는 현재 표시 상태.</summary>
        public int CurrentHP => _currentHP;
        public int MaxHP => _maxHP;
        public float Fill => _maxHP <= 0 ? 0f : (float)_currentHP / _maxHP;
        public bool IsBound => _source != null;

        /// <summary>
        /// 임의의 대상(IHealthReadOnly)에 부착한다.
        /// 즉시 현재값을 반영하고, 이후 HP 변화를 자동으로 추종한다.
        /// 대상은 자신을 표시하는 HPBar의 존재를 알 필요가 없다.
        /// </summary>
        public void Bind(IHealthReadOnly source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));   // 검증 먼저: 실패해도 기존 바인딩 보존
            Unbind();   // 재바인딩 안전: 이전 구독을 먼저 해제
            _source = source;
            _source.Changed += OnHealthChanged;
            _maxHP = source.Max;
            RefreshHP(source.Current);   // 초기 동기화
        }

        /// <summary>대상에서 분리한다. 구독 해제로 메모리 누수/유령 갱신을 방지.</summary>
        public void Unbind()
        {
            if (_source == null) return;
            _source.Changed -= OnHealthChanged;
            _source = null;
        }

        private void OnHealthChanged(HealthSnapshot snapshot)
        {
            _maxHP = snapshot.Max;
            RefreshHP(snapshot.Current);
        }

        /// <summary>(과제 제공 API) 최신 HP를 화면에 반영. 실제로는 게이지 길이/색을 갱신.</summary>
        public void RefreshHP(int newHP)
        {
            _currentHP = newHP;
        }

        /// <summary>오브젝트 파괴 시 자동으로 구독 해제.</summary>
        public override void DummyDestroy() => Unbind();
    }
}
