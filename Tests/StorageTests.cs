﻿
namespace ME.ECS.Tests {

    using System.Linq;

    public class StorageTest {
        
        private class TestState : State {}
        private struct TestComponent : IStructComponent {}

        private class TestSystem : ISystem, IAdvanceTick {

            public World world { get; set; }

            private Filter filter;
            
            public void OnConstruct() {
                
                this.filter = Filter.Create("Test").WithStructComponent<TestComponent>().Push();
                
            }

            public void OnDeconstruct() {
                
            }

            public void AdvanceTick(in float deltaTime) {
                
                var entity = this.world.AddEntity();
                entity.SetData(new TestComponent());
                
                this.filter.ApplyAllRequests();
                foreach (var ent in this.filter) {
                
                    var ent2 = this.world.AddEntity();
                    UnityEngine.Debug.Log(ent);
                    ent2.SetData(new TestComponent());
                    UnityEngine.Debug.Log(ent2);
                    
                    ent.Destroy();
                    
                    var ent3 = this.world.AddEntity();
                    ent3.SetData(new TestComponent());
                    UnityEngine.Debug.Log(ent3);
                
                }
                this.filter.ApplyAllRequests();
                foreach (var ent in this.filter) {
                    
                    UnityEngine.Debug.LogWarning(ent);
                    
                }

            }

        }

        private class TestStatesHistoryModule : ME.ECS.StatesHistory.StatesHistoryModule<TestState> {

        }

        private class TestNetworkModule : ME.ECS.Network.NetworkModule<TestState> {

            protected override ME.ECS.Network.NetworkType GetNetworkType() {
                return ME.ECS.Network.NetworkType.RunLocal | ME.ECS.Network.NetworkType.SendToNet;
            }


        }
        
        [NUnit.Framework.TestAttribute]
        public void AddRemoveWithFilter() {

            World world = null;
            WorldUtilities.CreateWorld<TestState>(ref world, 0.033f);
            {
                world.AddModule<TestStatesHistoryModule>();
                world.AddModule<TestNetworkModule>();
                world.SetState<TestState>(WorldUtilities.CreateState<TestState>());
                {
                    WorldUtilities.InitComponentTypeId<TestComponent>(false);
                    ComponentsInitializerWorld.Setup((e) => {
                
                        e.ValidateData<TestComponent>();
                
                    });
                }
                {
                    
                    var group = new SystemGroup(world, "TestGroup");
                    group.AddSystem(new TestSystem());

                }
            }
            world.SaveResetState<TestState>();
            
            world.SetFromToTicks(0, 1);
            world.Update(1f);

        }

        [NUnit.Framework.TestAttribute]
        public void AddRemove() {
            
            var st = new Storage();
            st.Initialize(100);

            var entity = st.Alloc();
            NUnit.Framework.Assert.AreEqual(entity.id, 0);
            NUnit.Framework.Assert.AreEqual(entity.version, 1);

            NUnit.Framework.Assert.AreEqual(st.AliveCount, 1);
            NUnit.Framework.Assert.AreEqual(st.DeadCount, 0);

            NUnit.Framework.Assert.IsTrue(st.IsAlive(entity.id, entity.version));

            st.Dealloc(entity);
            st.ApplyDead();

            NUnit.Framework.Assert.IsTrue(st.IsAlive(entity.id, entity.version) == false);

            NUnit.Framework.Assert.AreEqual(st.AliveCount, 0);
            NUnit.Framework.Assert.AreEqual(st.DeadCount, 1);

            {
                var entity2 = st.Alloc();
                NUnit.Framework.Assert.AreEqual(entity2.id, 0);
                NUnit.Framework.Assert.AreEqual(entity2.version, 2);

                NUnit.Framework.Assert.AreEqual(st.AliveCount, 1);
                NUnit.Framework.Assert.AreEqual(st.DeadCount, 0);
            }

        }

        [NUnit.Framework.TestAttribute]
        public void AddRemoveMulti() {

            var st = new Storage();
            st.Initialize(20);

            var list = new System.Collections.Generic.List<Entity>();
            var v = 1;
            for (int j = 0; j < 10; ++j) {

                list.Clear();
                for (int i = 0; i < 10000; ++i) {

                    var entity = st.Alloc();
                    list.Add(entity);
                    //NUnit.Framework.Assert.AreEqual(entity.id, i);
                    NUnit.Framework.Assert.AreEqual(entity.version, v);

                    //NUnit.Framework.Assert.AreEqual(st.AliveCount, i + 1);

                    NUnit.Framework.Assert.IsTrue(st.IsAlive(entity.id, entity.version));

                }

                for (int i = 0; i < list.Count; ++i) {

                    st.Dealloc(list[i]);

                }
                
                for (int i = 0; i < 10000; ++i) {

                    var entity = st.Alloc();
                    list.Add(entity);
                    //NUnit.Framework.Assert.AreEqual(entity.id, i);
                    NUnit.Framework.Assert.AreEqual(entity.version, v);

                    //NUnit.Framework.Assert.AreEqual(st.AliveCount, i + 1);

                    NUnit.Framework.Assert.IsTrue(st.IsAlive(entity.id, entity.version));

                }

                for (int i = 0; i < list.Count; ++i) {

                    st.Dealloc(list[i]);

                }

                st.ApplyDead();
                ++v;

            }
            
            UnityEngine.Debug.Log("Stats: " + st.AliveCount + " :: " + st.DeadCount);
            
        }

    }

}