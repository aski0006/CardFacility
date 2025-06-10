using _Core.GameManager;
using _Enums;
using _Utils;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace _Core {
    /// <summary>
    /// 全局游戏资源管理器，管理所有核心游戏资源和区域状态
    /// 遵循数据驱动设计，资源数据与业务逻辑分离
    /// </summary>
    public class ResourceManager : SingletonBaseComponent<ResourceManager> {
        // 区域资源数据线程锁
        private readonly object _regionLock = new object();

        #region 资源定义

        /// <summary>
        /// 核心资源类型
        /// </summary>
        public enum ResourceType {
            ConceptShards, // 概念碎片 (CS)
            PurifiedLogic, // 纯化逻辑 (PL)
            RealityAnchors, // 现实稳定锚 (RA)
            EntropyPollution, // 熵污染 (EP)
            CognitiveLoad // 认知负荷 (CL)
        }

        /// <summary>
        /// 区域资源数据结构
        /// </summary>
        [Serializable]
        public struct RegionResourceData {
            public float Stability; // 区域稳定性（百分比）
            public float LocalPollution; // 区域污染值
            public float PollutionDecayRate; // 污染衰减率（每秒）
        }

        #endregion

        #region 配置参数

        [Header("调试设置")]
        [SerializeField] private bool _debugMode = true;

        [Header("基础资源")]
        [SerializeField] private int _initialCS = 100;
        [SerializeField] private int _initialPL = 50;
        [SerializeField] private int _initialRA = 5;
        [SerializeField] private float _initialEP = 0f;
        [SerializeField] private float _initialCL = 0f;

        [Header("区域设置")]
        [SerializeField] private float _baseStabilityDecayRate = 0.0167f; // 每分钟-1%
        [SerializeField] private float _pollutionDecayRate = 0.05f; // 每秒衰减率

        #endregion

        #region 数据存储

        // 核心资源存储
        private Dictionary<ResourceType, float> _resources = new Dictionary<ResourceType, float>();

        // 区域资源存储
        private Dictionary<RegionType, RegionResourceData> _regionResources =
            new Dictionary<RegionType, RegionResourceData>();

        // 污染任务控制
        private bool _pollutionTaskActive = false;
        private CancellationTokenSource _pollutionTaskToken;

        #endregion

        #region 初始化

        protected override void Awake() {
            base.Awake();

            InitializeResources();
            InitializeRegionResources();
            SubscribeToEvents();

            if (_debugMode) {
                InfoBuilder.Create()
                    .AppendLine("资源管理器初始化完成")
                    .AppendLine($"概念碎片(CS): {GetResource(ResourceType.ConceptShards)}")
                    .AppendLine($"纯化逻辑(PL): {GetResource(ResourceType.PurifiedLogic)}")
                    .AppendLine($"现实稳定锚(RA): {GetResource(ResourceType.RealityAnchors)}")
                    .AppendLine($"熵污染(EP): {GetResource(ResourceType.EntropyPollution)}")
                    .AppendLine($"认知负荷(CL): {GetResource(ResourceType.CognitiveLoad)}")
                    .Log();
            }
        }

        private void InitializeResources() {
            _resources.Clear();
            _resources.Add(ResourceType.ConceptShards, _initialCS);
            _resources.Add(ResourceType.PurifiedLogic, _initialPL);
            _resources.Add(ResourceType.RealityAnchors, _initialRA);
            _resources.Add(ResourceType.EntropyPollution, _initialEP);
            _resources.Add(ResourceType.CognitiveLoad, _initialCL);
        }

        private void InitializeRegionResources() {
            // 获取网格系统中的区域定义
            var gridSystem = GridSystem.Instance;

            lock (_regionLock) {
                _regionResources.Clear();

                foreach (RegionType region in Enum.GetValues(typeof(RegionType))) {
                    if (region == RegionType.None) continue;

                    var cells = gridSystem?.GetRegionCells(region);
                    bool regionExists = cells != null && cells.Count > 0;

                    _regionResources[region] = new RegionResourceData {
                        Stability = regionExists ? 100f : 0f,
                        LocalPollution = 0f,
                        PollutionDecayRate = _pollutionDecayRate
                    };
                }
            }
        }

        private void SubscribeToEvents() {
            // 订阅游戏状态变化
            this.SubscribeEvent(GameState.GameRunning, OnGameRunning);
            this.SubscribeEvent(GameState.GamePaused, OnGamePaused);

            // 订阅区域变化事件
            this.SubscribeEvent(EventEnums.RegionUpdated, OnRegionUpdated);
        }

        #endregion

        #region 游戏状态回调

        private void OnGameRunning(object isRunning) {
            if ((bool)isRunning) {
                // 游戏进入运行状态，启动污染更新任务
                StartPollutionUpdateTask();

                if (_debugMode) {
                    InfoBuilder.Create()
                        .AppendLine("资源管理器：进入运行状态")
                        .AppendLine("启动污染更新任务")
                        .Log();
                }
            }
        }

        private void OnGamePaused(object isPaused) {
            if ((bool)isPaused) {
                // 游戏暂停，停止污染更新任务
                StopPollutionUpdateTask();

                if (_debugMode) {
                    InfoBuilder.Create()
                        .AppendLine("资源管理器：进入暂停状态")
                        .AppendLine("停止污染更新任务")
                        .Log();
                }
            }
        }

        private void OnRegionUpdated(object data) {
            // 区域更新时重新初始化区域资源
            InitializeRegionResources();

            if (_debugMode) {
                InfoBuilder.Create()
                    .AppendLine("资源管理器：区域配置已更新")
                    .AppendLine($"收容区稳定性: {GetRegionStability(RegionType.Containment):F1}%")
                    .AppendLine($"加工区稳定性: {GetRegionStability(RegionType.Processing):F1}%")
                    .AppendLine($"净化区稳定性: {GetRegionStability(RegionType.Purification):F1}%")
                    .Log();
            }
        }

        #endregion

        #region 资源管理API

        /// <summary>
        /// 获取指定资源类型的当前数量
        /// </summary>
        public float GetResource(ResourceType type) {
            return _resources.TryGetValue(type, out var value) ? value : 0f;
        }

        /// <summary>
        /// 修改资源数量
        /// </summary>
        /// <param name="type">资源类型</param>
        /// <param name="delta">变化量（可正可负）</param>
        /// <param name="source">修改来源（用于调试）</param>
        public void ModifyResource(ResourceType type, float delta, string source = "") {
            if (!_resources.ContainsKey(type)) return;

            float oldValue = _resources[type];
            float newValue = Mathf.Max(0, oldValue + delta);
            _resources[type] = newValue;

            // 发布资源变更事件
            PublishResourceChange(type, oldValue, newValue, source);

            if (_debugMode) {
                InfoBuilder.Create()
                    .AppendLine($"资源变更: {type}")
                    .AppendLine($"来源: {source}")
                    .AppendLine($"变化量: {delta}")
                    .AppendLine($"旧值: {oldValue}")
                    .AppendLine($"新值: {newValue}")
                    .Log();
            }
        }

        /// <summary>
        /// 获取指定区域的稳定性
        /// </summary>
        public float GetRegionStability(RegionType region) {
            lock (_regionLock) {
                return _regionResources.TryGetValue(region, out var data) ? data.Stability : 0f;
            }
        }

        /// <summary>
        /// 修改区域稳定性
        /// </summary>
        public void ModifyRegionStability(RegionType region, float delta) {
            lock (_regionLock) {
                if (!_regionResources.ContainsKey(region)) return;

                var data = _regionResources[region];
                float oldStability = data.Stability;
                data.Stability = Mathf.Clamp(oldStability + delta, 0f, 100f);
                _regionResources[region] = data;

                // 发布区域稳定性变化事件
                EventBus.Instance.Publish($"StabilityChanged_{region}", data.Stability);

                // 检查稳定性临界值
                CheckStabilityThreshold(region, oldStability, data.Stability);

                if (_debugMode) {
                    InfoBuilder.Create()
                        .AppendLine($"区域稳定性变更: {region}")
                        .AppendLine($"变化量: {delta}")
                        .AppendLine($"旧值: {oldStability:F1}%")
                        .AppendLine($"新值: {data.Stability:F1}%")
                        .Log();
                }
            }
        }

        /// <summary>
        /// 获取指定区域的污染值
        /// </summary>
        public float GetRegionPollution(RegionType region) {
            lock (_regionLock) {
                return _regionResources.TryGetValue(region, out var data) ? data.LocalPollution : 0f;
            }
        }

        /// <summary>
        /// 增加区域污染
        /// </summary>
        public void AddRegionPollution(RegionType region, float amount) {
            lock (_regionLock) {
                if (!_regionResources.ContainsKey(region)) return;

                var data = _regionResources[region];
                data.LocalPollution += amount;
                _regionResources[region] = data;

                // 增加全局熵污染
                ModifyResource(ResourceType.EntropyPollution, amount, $"区域污染:{region}");

                if (_debugMode) {
                    InfoBuilder.Create()
                        .AppendLine($"区域污染增加: {region}")
                        .AppendLine($"增加量: {amount}")
                        .AppendLine($"新污染值: {data.LocalPollution:F1}")
                        .Log();
                }
            }
        }

        #endregion

        #region 核心逻辑

        /// <summary>
        /// 启动污染更新任务
        /// </summary>
        private void StartPollutionUpdateTask() {
            if (_pollutionTaskActive) return;

            _pollutionTaskActive = true;
            _pollutionTaskToken = new CancellationTokenSource();

            PollutionUpdateTask(_pollutionTaskToken.Token).Forget();
        }

        /// <summary>
        /// 停止污染更新任务
        /// </summary>
        private void StopPollutionUpdateTask() {
            _pollutionTaskActive = false;
            _pollutionTaskToken?.Cancel();
            _pollutionTaskToken?.Dispose();
            _pollutionTaskToken = null;
        }

        /// <summary>
        /// 污染更新异步任务
        /// </summary>
        private async UniTaskVoid PollutionUpdateTask(CancellationToken token) {
            if (_debugMode) {
                InfoBuilder.Create()
                    .AppendLine("启动污染更新任务")
                    .AppendLine($"更新频率: 1秒")
                    .Log();
            }

            while (_pollutionTaskActive && !token.IsCancellationRequested) {
                try {
                    // 每秒更新一次污染和稳定性
                    await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: token);
                    List<RegionType> regionsToUpdate;
                    lock (_regionLock) {
                        // 复制键集合，避免在遍历过程中集合被修改
                        regionsToUpdate = new List<RegionType>(_regionResources.Keys);
                    }
                    foreach (RegionType region in regionsToUpdate) {
                        // 再次检查区域是否存在（可能在复制后被移除）
                        lock (_regionLock) {
                            if (!_regionResources.ContainsKey(region))
                                continue;
                        }
                        UpdateRegionPollution(region);
                        UpdateRegionStability(region);
                    }
                } catch (OperationCanceledException) {
                    // 任务被取消
                    break;
                } catch (Exception ex) {
                    if (_debugMode) {
                        InfoBuilder.Create(LogLevel.Error)
                            .AppendLine("污染更新任务错误:")
                            .AppendLine(ex.Message)
                            .Log();
                    }
                }
            }
        }

        /// <summary>
        /// 更新区域污染值
        /// </summary>
        private void UpdateRegionPollution(RegionType region) {
            lock (_regionLock) {
                if (!_regionResources.TryGetValue(region, out RegionResourceData data)) return;

                // 应用污染衰减
                float decay = data.LocalPollution * data.PollutionDecayRate;
                data.LocalPollution = Mathf.Max(0, data.LocalPollution - decay);
                _regionResources[region] = data;

                if (decay > 0 && _debugMode) {
                    InfoBuilder.Create()
                        .AppendLine($"区域污染衰减: {region}")
                        .AppendLine($"衰减量: {decay:F2}")
                        .AppendLine($"新污染值: {data.LocalPollution:F1}")
                        .Log();
                }
            }
        }

        /// <summary>
        /// 更新区域稳定性
        /// </summary>
        private void UpdateRegionStability(RegionType region) {
            lock (_regionLock) {
                if (!_regionResources.TryGetValue(region, out RegionResourceData data)) return;

                // 基础衰减（每分钟-1%）
                float baseDecay = _baseStabilityDecayRate;

                // 污染加速衰减（每10点污染增加0.5%衰减）
                float pollutionFactor = Mathf.Floor(data.LocalPollution / 10f) * 0.005f;

                // 总衰减
                float totalDecay = baseDecay + pollutionFactor;

                ModifyRegionStability(region, -totalDecay);
            }
        }

        /// <summary>
        /// 检查稳定性临界值
        /// </summary>
        private void CheckStabilityThreshold(RegionType region, float oldStability, float newStability) {
            // 检查是否跨过临界阈值（30%）
            if (oldStability > 30f && newStability <= 30f) {
                // 触发区域事故
                TriggerRegionAccident(region);
            }
        }

        /// <summary>
        /// 触发区域事故
        /// </summary>
        private void TriggerRegionAccident(RegionType region) {
            // 增加全局熵污染
            ModifyResource(ResourceType.EntropyPollution, 50f, $"区域事故:{region}");

            // 发布区域事故事件
            EventBus.Instance.Publish($"RegionAccident_{region}", region);

            if (_debugMode) {
                InfoBuilder.Create(LogLevel.Warning)
                    .AppendLine($"区域事故: {region}")
                    .AppendLine("稳定性低于30%，增加50点全局熵污染")
                    .Log();
            }
        }

        /// <summary>
        /// 发布资源变更事件
        /// </summary>
        private void PublishResourceChange(ResourceType type, float oldValue, float newValue, string source) {
            // 构造变更数据
            var changeData = new Dictionary<string, object> {
                { "Type", type },
                { "OldValue", oldValue },
                { "NewValue", newValue },
                { "Delta", newValue - oldValue },
                { "Source", source }
            };

            // 发布通用资源变更事件
            EventBus.Instance.Publish("ResourceChanged", changeData);

            // 发布特定资源类型变更事件
            EventBus.Instance.Publish($"ResourceChanged_{type}", changeData);
        }

        #endregion

        #region 生命周期管理

        private void OnDestroy() {
            StopPollutionUpdateTask();
        }

        #endregion
    }
}
