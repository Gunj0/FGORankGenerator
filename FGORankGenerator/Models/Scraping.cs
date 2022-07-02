using AngleSharp.Html.Parser;
using AngleSharp.Html.Dom;

namespace FGORankGenerator.Models
{
  public static class Scraping
  {
    // AppMedia URL
    private const string appMediaURL = "https://appmedia.jp/fategrandorder/1351236";

    // GameWith URL
    private const string gameWithURL = "https://gamewith.jp/fgo/article/show/62409";

    /// <summary>
    /// スクレイピングで得た各サーヴァント評価リストを返します。
    /// </summary>
    /// <returns></returns>
    public static List<ServantModel> GetServantData()
    {
      var servantList = new List<ServantModel>();

      // AppMediaのHTML解析
      IHtmlDocument appMediaDoc = GetParseHtml(appMediaURL);

      // 鯖ID取得
      var elements = appMediaDoc.QuerySelectorAll(".servant_results_tr > td");
      int order = 1;
      int i = 0;
      foreach (var element in elements)
      {
        if (order % 3 == 1)
        {
          servantList.Add(new ServantModel()
          {
            Id = int.Parse(element.GetAttribute("data-for_sort")),
          });
        }
        order++;
        i++;
      };

      // 鯖名取得
      elements = appMediaDoc.QuerySelectorAll(".servant_results_tr > td > a > div");
      i = 0;
      foreach (var element in elements)
      {
         // 哪吒は文字化けするのでひらがなにする
        if(element.InnerHtml == "哪吒")
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
        servantList[i].Rarity = int.Parse(element.GetAttribute("data-rarity"));  // レア度
        servantList[i].AppMediaRate = element.GetAttribute("data-rate");    // 攻略ランク
        servantList[i].AppMediaOrbit = element.GetAttribute("data-orbit");   // 周回ランク
        servantList[i].Class = element.GetAttribute("data-class");   // クラス
        servantList[i].Type = element.GetAttribute("data-type");    // タイプ
        servantList[i].Range = element.GetAttribute("data-range");   // 範囲
        i++;
      };

      // GamewithのHTML解析
      IHtmlDocument gameWithDoc = GetParseHtml(gameWithURL);

      // 評価取得
      i = 1;
      elements = gameWithDoc.QuerySelectorAll(".fgo-ranking-table");
      foreach (var element in elements)
      {
        string rank = "";
        switch (i)
        {
          case 1:
            rank = "S+";
            break;
          case 2:
            rank = "S";
            break;
          case 3:
            rank = "A+";
            break;
          case 4:
            rank = "A";
            break;
          case 5:
            rank = "B+";
            break;
          case 6:
            rank = "B";
            break;
          case 7:
            rank = "C";
            break;
          case 8:
            rank = "D";
            break;
          default:
            rank = "EX";
            break;
        }

        var parser = new HtmlParser();
        var img = parser.ParseDocument(element.InnerHtml).
          QuerySelectorAll("table > tbody > tr > td > div > a > div > div > img");
        foreach (var item in img)
        {
          // GameWith上のIDを取得
          var id = int.Parse(item.GetAttribute("data-original").ToString().Substring(53, 3));
          // リストからID一致検索
          var index = servantList.FindIndex(x => x.Id == id);
          // GameWithランクを格納
          servantList[index].GameWithRank = rank;
        };
        i++;
      };

      // メカエリIIの評価はメカエリと同じ
      var mechaIndex = servantList.FindIndex(x => x.Id == 190);
      var mechaIIIndex = servantList.FindIndex(x => x.Id == 191);
      servantList[mechaIIIndex].GameWithRank = servantList[mechaIndex].GameWithRank;

      // ID順に整理
      servantList = servantList.OrderBy(item => item.Id).ToList();

      return servantList;
    }

    /// <summary>
    /// URLを渡すと解析済みHTMLドキュメントを返します。
    /// </summary>
    /// <param name="url">解析したいURL</param>
    /// <returns>解析済みドキュメント</returns>
    private static IHtmlDocument GetParseHtml(string url)
    {
      // HTML取得
      using var client = new HttpClient();
      string contents = client.GetStringAsync(url).Result;

      // HTMLパース
      var parser = new HtmlParser();
      IHtmlDocument doc = parser.ParseDocument(contents);

      return doc;
    }
  }
}
