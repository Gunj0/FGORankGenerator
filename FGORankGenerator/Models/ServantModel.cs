namespace FGORankGenerator.Models
{
  public class ServantModel
  {
    /// <summary>
    /// ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// サーヴァント名
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// レア度
    /// </summary>
    public int? Rarity { get; set; }

    /// <summary>
    /// クラス
    /// </summary>
    public string? Class { get; set; }

    /// <summary>
    /// ABCタイプ
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// 宝具範囲
    /// </summary>
    public string? Range { get; set; }

    /// <summary>
    /// AppMediaの攻略ランク
    /// </summary>
    public int AppMediaRate { get; set; }

    /// <summary>
    /// AppMediaの周回ランク
    /// </summary>
    public int AppMediaOrbit { get; set; }

    /// <summary>
    /// 総合ランク
    /// </summary>
    public double OverallRank { get; set; }
  }
}
