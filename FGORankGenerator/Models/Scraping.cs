using AngleSharp.Html.Parser;
using AngleSharp.Html.Dom;

namespace FGORankGenerator.Models
{
  public static class Scraping
  {
    // AppMedia URL
    private const string appMediaURL = "https://appmedia.jp/fategrandorder/1351236";

    // HttpClientは使い回す必要がある
    private static readonly HttpClient _httpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(10)};

    /// <summary>
    /// スクレイピングで得た各サーヴァント評価リストを返します。
    /// </summary>
    /// <returns></returns>
    public static List<ServantModel> GetServantData()
    {
      var servantList = new List<ServantModel>();

      // AppMediaのHTML解析
      IHtmlDocument? appMediaDoc = GetParseHtml(appMediaURL).Result;

      if (appMediaDoc != null)
      {
        // 鯖ID取得
        var elements = appMediaDoc.QuerySelectorAll(".servant_results_tr > td");
        int order = 1;
        foreach (var element in elements)
        {
          if (order % 3 == 1)
          {
            string? servantId = element.GetAttribute("data-for_sort");
            if (!string.IsNullOrEmpty(servantId)){
              servantList.Add(new ServantModel()
              {
                Id = int.Parse(servantId),
            });
            }
          }
          order++;
        };

        // 鯖名取得
        elements = appMediaDoc.QuerySelectorAll(".servant_results_tr > td > a > div");
        int i = 0;
        foreach (var element in elements)
        {
          // 哪吒は文字化けするのでひらがなにする
          if (element.InnerHtml == "哪吒")
          {
            servantList[i].Name = "なた";
          }
          else
          {
            servantList[i].Name = element.InnerHtml.Replace("<br>", "");
          }
          i++;
        };

        // その他
        elements = appMediaDoc.GetElementsByClassName("servant_results_tr");
        i = 0;
        foreach (var element in elements)
        {
          string? rarity = element.GetAttribute("data-rarity");
          if (!string.IsNullOrEmpty(rarity)){
            servantList[i].Rarity = int.Parse(rarity);                // 星
          }
          servantList[i].Class = element.GetAttribute("data-class");  // クラス
          servantList[i].Type = element.GetAttribute("data-type");    // タイプ
          servantList[i].Range = element.GetAttribute("data-range");  // 範囲

          int rate = RankToNum(element.GetAttribute("data-rate"));
          int orbit =　RankToNum(element.GetAttribute("data-orbit"));
          servantList[i].OverallRank = rate + orbit;                  // 総合ポイント
          servantList[i].AppMediaRate = rate;                         // 攻略ポイント
          servantList[i].AppMediaOrbit = orbit;                       // 周回ポイント
          i++;
        };

        // ID順に整理
        servantList = servantList.OrderByDescending(item => item.OverallRank).ToList();
      }
      return servantList;
    }

    /// <summary>
    /// URLを渡すと解析済みHTMLドキュメントを返します。
    /// </summary>
    /// <param name="url">解析したいURL</param>
    /// <returns>解析済みドキュメント</returns>
    private static async Task<IHtmlDocument?> GetParseHtml(string url)
    {
      try
      {
        HttpResponseMessage response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        string contents = await response.Content.ReadAsStringAsync();

        // HTMLパース
        var parser = new HtmlParser();
        return parser.ParseDocument(contents);
      }
      catch (HttpRequestException e)
      {
      }

      return null;
    }

    /// <summary>
    /// ランクに応じた数値を返します。
    /// </summary>
    /// <param name="rank"></param>
    /// <returns></returns>
    private static int RankToNum (string? rank)
    {
      int num = 0;
      switch (rank)
      {
        case "SSS":
          num = 9;
          break;
        case "SS":
          num = 8;
          break;
        case "S+":
          num = 7;
          break;
        case "S":
          num = 6;
          break;
        case "A+":
          num = 5;
          break;
        case "A":
          num = 4;
          break;
        case "B":
          num = 3;
          break;
        case "C":
          num = 2;
          break;
        case "D":
          num = 1;
          break;
        default:
          num = 0;
          break;
      }

      return num;
    }
  }
}
