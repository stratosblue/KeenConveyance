#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配

using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace KeenConveyance.AspNetCore;

/// <summary>
/// KeenConveyance 的 MVC 相关选项
/// </summary>
public class KeenConveyanceMvcOptions
{
    #region Internal 属性

    internal ServiceDescriptor HttpRequestEntryKeyGeneratorServiceDescriptor { get; set; } = ServiceDescriptor.Singleton<IHttpRequestEntryKeyGenerator, QueryBaseHttpRequestEntryKeyGenerator>();

    #endregion Internal 属性

    #region Public 属性

    /// <summary>
    /// 控制器选择委托，用于选择非标准 <see cref="Controller"/> 类型作为控制器
    /// </summary>
    public Predicate<TypeInfo>? ControllerSelectPredicate { get; set; }

    /// <summary>
    /// <see cref="Endpoint"/> 选择委托，用于选择 KeenConveyance 需要支持的 <see cref="Endpoint"/>（默认将会支持所有Controller中基于接口派生的方法生成的 <see cref="Endpoint"/>）
    /// </summary>
    public Predicate<Endpoint>? EndpointSelectPredicate { get; set; }

    /// <summary>
    /// 服务入口路径， 默认为 <see cref="KeenConveyanceConstants.DefaultEntryPath"/>
    /// </summary>
    public PathString ServiceEntryPath { get; set; } = KeenConveyanceConstants.DefaultEntryPath;

    #endregion Public 属性
}
