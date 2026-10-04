namespace System.Runtime.CompilerServices;

/// <summary>
/// net472 的 BCL 没有 CallerArgumentExpressionAttribute，而 Roslyn 只按**名字**识别它——
/// 自带一个 internal 版本即可让设置页 helper 在调用点捕获字段名（键派生纪律的引擎，
/// 见 MwahSettingsUI 顶注）。编译器不认识时它退化为一个普通的可选字符串参数，无副作用。
/// </summary>
[System.AttributeUsage(System.AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
internal sealed class CallerArgumentExpressionAttribute : System.Attribute
{
    public CallerArgumentExpressionAttribute(string parameterName) => ParameterName = parameterName;

    public string ParameterName { get; }
}
