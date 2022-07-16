using System.Text;

namespace FGORankGenerator.Models
{
  /// <summary>
  /// リストからCSVを生成
  /// </summary>
  public static class CsvWriter
  {
    public static string CreateCsv(List<ServantModel> servantList)
    {
      var sb = new StringBuilder();

      // ヘッダ作成
      sb.AppendLine(CreateCsvHeader(headerArray));

      // ボディ作成
      servantList.ForEach(item => sb.AppendLine(CreateCsvBody(item)));

      return sb.ToString();
    }

    /// <summary>
    /// ヘッダリスト
    /// </summary>
    private static string[] headerArray
      = {
      "No.",
      "鯖名",
      "星",
      "Class",
      "色",
      "範囲",
      "総",
      "攻",
      "周"
    };

    /// <summary>
    /// csvのヘッダを生成
    /// </summary>
    /// <param name="headerArray"></param>
    /// <returns></returns>
    private static string CreateCsvHeader(string[] headerArray)
    {
      var sb = new StringBuilder();

      foreach(var header in headerArray)
      {
        sb.Append($@"""{header}"",");
      }

      return sb.Remove(sb.Length - 1, 1).ToString();
    }

    /// <summary>
    /// csvのbodyを生成
    /// </summary>
    /// <param name="servant"></param>
    /// <returns></returns>
    private static string CreateCsvBody(ServantModel servant)
    {
      var sb = new StringBuilder();

      sb.Append(string.Format($@"""{servant.Id}"","));
      sb.Append(string.Format($@"""{servant.Name}"","));
      sb.Append(string.Format($@"""{servant.Rarity}"","));
      sb.Append(string.Format($@"""{servant.Class}"","));
      sb.Append(string.Format($@"""{servant.Type}"","));
      sb.Append(string.Format($@"""{servant.Range}"","));
      sb.Append(string.Format($@"""{servant.OverallRank}"","));
      sb.Append(string.Format($@"""{servant.AppMediaRate}"","));
      sb.Append(string.Format($@"""{servant.AppMediaOrbit}"","));

      return sb.ToString();
    }
  }
}
