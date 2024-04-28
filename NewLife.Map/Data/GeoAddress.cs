namespace NewLife.Data;

/// <summary>地理地址</summary>
public class GeoAddress
{
    #region 属性
    /// <summary>名称</summary>
    public String? Name { get; set; }

    /// <summary>坐标</summary>
    public GeoPoint? Location { get; set; }

    /// <summary>地址</summary>
    public String? Address { get; set; }

    /// <summary>标题。语义描述，POI详细信息，例如石下村东南282米</summary>
    public String? Title { get; set; }

    /// <summary>行政区域编码。6位数字</summary>
    public Int32 Code { get; set; }

    /// <summary>国家</summary>
    public String? Country { get; set; }

    /// <summary>省份</summary>
    public String? Province { get; set; }

    /// <summary>城市</summary>
    public String? City { get; set; }

    /// <summary>区县</summary>
    public String? District { get; set; }

    /// <summary>乡镇</summary>
    public String? Township { get; set; }

    /// <summary>乡镇编码。9位数字</summary>
    public Int32 Towncode { get; set; }

    /// <summary>街道</summary>
    public String? Street { get; set; }

    /// <summary>街道编码</summary>
    public String? StreetNumber { get; set; }

    /// <summary>级别</summary>
    public String? Level { get; set; }

    /// <summary>精确打点。位置的附加信息，是否精确查找。1为精确查找，即准确打点；0为不精确，即模糊打点。</summary>
    public Boolean Precise { get; set; }

    /// <summary>可信度。[0-100]描述打点绝对精度（即坐标点的误差范围）</summary>
    /// <remarks>
    /// confidence=100，解析误差绝对精度小于20m；
    /// confidence≥90，解析误差绝对精度小于50m；
    /// confidence≥80，解析误差绝对精度小于100m；
    /// confidence≥75，解析误差绝对精度小于200m；
    /// confidence≥70，解析误差绝对精度小于300m；
    /// confidence≥60，解析误差绝对精度小于500m；
    /// confidence≥50，解析误差绝对精度小于1000m；
    /// confidence≥40，解析误差绝对精度小于2000m；
    /// confidence≥30，解析误差绝对精度小于5000m；
    /// confidence≥25，解析误差绝对精度小于8000m；
    /// confidence≥20，解析误差绝对精度小于10000m；
    /// </remarks>
    public Int32 Confidence { get; set; }

    /// <summary>描述地址理解程度。分值范围0-100，分值越大，服务对地址理解程度越高（建议以该字段作为解析结果判断标准）</summary>
    /// <remarks>
    /// 当comprehension值为以下值时，对应的准确率如下：
    /// comprehension=100，解析误差100m内概率为91%，误差500m内概率为96%；
    /// comprehension≥90，解析误差100m内概率为89%，误差500m内概率为96%；
    /// comprehension≥80，解析误差100m内概率为88%，误差500m内概率为95%；
    /// comprehension≥70，解析误差100m内概率为84%，误差500m内概率为93%；
    /// comprehension≥60，解析误差100m内概率为81%，误差500m内概率为91%；
    /// comprehension≥50，解析误差100m内概率为79%，误差500m内概率为90%；
    /// 解析误差：地理编码服务解析地址得到的坐标位置，与地址对应的真实位置间的距离。
    /// </remarks>
    public Int32 Comprehension { get; set; }
    #endregion

    /// <summary>已重载。</summary>
    /// <returns></returns>
    public override String? ToString() => Address;
}