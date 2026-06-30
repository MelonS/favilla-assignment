using System;
using System.Collections.Generic;
using System.Reflection;
using Battle;
using UI;
using Xunit;

namespace MTM.Units.Tests
{
    /// <summary>
    /// 구조적 디커플링을 '코드 형상'으로 검증한다.
    ///  - HPBar(표시)의 표면(필드/프로퍼티/메서드/생성자/상속) 어디에도 Battle 유닛 타입이 없어야 한다.
    ///  - 각 유닛(코어)의 표면 어디에도 UI 타입이 없어야 한다.
    /// → 두 계층은 오직 Core 추상을 통해서만 연결된다.
    /// </summary>
    public class ArchitectureTests
    {
        [Fact]
        public void HPBar_surface_has_no_reference_to_Battle_units()
        {
            var roots = SurfaceTopNamespaces(typeof(HPBar));
            Assert.DoesNotContain("Battle", roots);
        }

        [Theory]
        [InlineData(typeof(Unit))]
        [InlineData(typeof(Champion))]
        [InlineData(typeof(Minion))]
        [InlineData(typeof(Tower))]
        public void Units_surface_has_no_reference_to_UI(Type unitType)
        {
            var roots = SurfaceTopNamespaces(unitType);
            Assert.DoesNotContain("UI", roots);
        }

        /// <summary>해당 타입의 '공개/내부 표면'이 참조하는 최상위 네임스페이스 집합을 수집.</summary>
        private static HashSet<string> SurfaceTopNamespaces(Type t)
        {
            var result = new HashSet<string>();

            void Add(Type x)
            {
                if (x == null) return;
                if (x.Namespace != null) result.Add(x.Namespace.Split('.')[0]);
                if (x.IsGenericType)
                    foreach (var g in x.GetGenericArguments())
                        Add(g);
            }

            const BindingFlags F = BindingFlags.Public | BindingFlags.NonPublic
                                 | BindingFlags.Instance | BindingFlags.Static
                                 | BindingFlags.DeclaredOnly;

            Add(t.BaseType);
            foreach (var f in t.GetFields(F)) Add(f.FieldType);
            foreach (var p in t.GetProperties(F)) Add(p.PropertyType);
            foreach (var m in t.GetMethods(F))
            {
                Add(m.ReturnType);
                foreach (var par in m.GetParameters()) Add(par.ParameterType);
            }
            foreach (var c in t.GetConstructors(F))
                foreach (var par in c.GetParameters()) Add(par.ParameterType);

            return result;
        }
    }
}
