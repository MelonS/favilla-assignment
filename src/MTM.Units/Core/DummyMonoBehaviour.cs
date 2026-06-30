namespace Core
{
    /// <summary>
    /// Unity의 MonoBehaviour 라이프사이클을 대체하는 더미 베이스. (과제 제공 파일)
    /// 실제 엔진에서는 Awake/Start/Update/OnDestroy로 매핑된다.
    /// </summary>
    public class DummyMonoBehaviour
    {
        public virtual void DummyAwake() { }
        public virtual void DummyStart() { }
        public virtual void DummyUpdate() { }
        public virtual void DummyDestroy() { }
    }
}
