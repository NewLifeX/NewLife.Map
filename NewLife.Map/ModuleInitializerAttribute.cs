#if !NET5_0_OR_GREATER
using System;

namespace System.Runtime.CompilerServices;

/// <summary>模块初始化标记。net45/netstandard 等旧目标框架没有该特性，补齐供多语言源生成器注册语言表</summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
internal sealed class ModuleInitializerAttribute : Attribute { }
#endif
