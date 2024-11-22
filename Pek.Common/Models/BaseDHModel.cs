using System.Xml.Serialization;

namespace Pek;

/// <summary>
/// 代表基本的模型
/// </summary>
public partial record BaseDHModel
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public BaseDHModel()
    {
        CustomProperties = [];
        PostInitialize();
    }

    /// <summary>
    /// 执行模型初始化的附加操作
    /// </summary>
    /// <remarks>开发人员可以在自定义的派生类中重写此方法，以便为构造函数添加一些自定义初始化代码</remarks>
    protected virtual void PostInitialize()
    {
    }

    /// <summary>
    /// 获取或设置属性以存储模型的自定义值
    /// </summary>
    [XmlIgnore]
    public Dictionary<String, String> CustomProperties { get; set; }
}
