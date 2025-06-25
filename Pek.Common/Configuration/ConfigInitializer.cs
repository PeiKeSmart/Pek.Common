namespace Pek.Configuration
{
    /// <summary>
    /// 配置系统初始化器
    /// 用于在需要时显式初始化特定配置类
    /// </summary>
    public static class ConfigInitializer
    {
        /// <summary>
        /// 初始化指定的配置类
        /// </summary>
        /// <typeparam name="TConfig">配置类型</typeparam>
        /// <remarks>
        /// 此方法通过访问Config&lt;TConfig&gt;.Current属性来触发配置类的初始化。
        /// 在大多数情况下，您不需要显式调用此方法，因为配置类会在首次访问时自动初始化。
        /// 此方法主要用于特殊场景，如需要在应用启动时预加载特定配置。
        /// </remarks>
        public static void InitializeConfig<TConfig>() where TConfig : Config<TConfig>, new()
        {
            // 直接访问Current属性触发初始化
            _ = Config<TConfig>.Current;
        }
    }
}