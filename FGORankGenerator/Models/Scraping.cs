using AngleSharp.Html.Parser;
using AngleSharp.Html.Dom;

namespace FGORankGenerator.Models
{
  public static class Scraping
  {
    const double MAX_SERVANT_SCORE = 18;

    // AppMedia URL
    private const string appMediaURL = "https://appmedia.jp/fategrandorder/1351236";

    // HttpClientは使い回す必要がある
    private static readonly HttpClient _httpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(10) };

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
        int order = 1; // 3つおきに取得する
        foreach (var element in elements)
        {
          if (order % 3 == 1)
          {
            string? servantId = element.GetAttribute("data-for_sort");
            if (!string.IsNullOrEmpty(servantId))
            {
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
            // 改行は削除
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
          if (!string.IsNullOrEmpty(rarity))
          {
            servantList[i].Rarity = int.Parse(rarity);                // 星
          }

          servantList[i].Class = ClassToKanji(element.GetAttribute("data-class"));           // 種
          servantList[i].Type = ColorToAChara(element.GetAttribute("data-type"));    // 色
          servantList[i].Range = RangeToAChara(element.GetAttribute("data-range"));  // 範

          int rate = RankToNum(element.GetAttribute("data-rate"));
          int orbit = RankToNum(element.GetAttribute("data-orbit"));
          servantList[i].AppMediaRate = rate;                         // 攻
          servantList[i].AppMediaOrbit = orbit;                       // 周
          servantList[i].OverallRank = (rate + orbit) / MAX_SERVANT_SCORE * 10; // 総
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
      catch
      {
      }

      return null;
    }

    /// <summary>
    /// ランクに応じた数値を返します。
    /// </summary>
    /// <param name="rank"></param>
    /// <returns></returns>
    private static int RankToNum(string? rank)
    {
      int num;
      switch (rank)
      {
        case "SSS":
          num = 10;
          break;
        case "SS":
          num = 9;
          break;
        case "S+":
          num = 8;
          break;
        case "S":
          num = 7;
          break;
        case "A+":
          num = 6;
          break;
        case "A":
          num = 5;
          break;
        case "B":
          num = 4;
          break;
        case "C":
          num = 3;
          break;
        case "D":
          num = 2;
          break;
        default:
          num = 1;
          break;
      }

      return num;
    }

    /// <summary>
    /// クラスに応じた漢字一文字を返します。
    /// </summary>
    /// <param name="className"></param>
    /// <returns></returns>
    private static string ClassToKanji(string? className)
    {
      string kanji;
      switch (className)
      {
        case "セイバー":
          kanji = "剣";
          break;
        case "アーチャー":
          kanji = "弓";
          break;
        case "ランサー":
          kanji = "槍";
          break;
        case "ライダー":
          kanji = "騎";
          break;
        case "キャスター":
          kanji = "術";
          break;
        case "アサシン":
          kanji = "殺";
          break;
        case "バーサーカー":
          kanji = "狂";
          break;
        case "シールダー":
          kanji = "盾";
          break;
        case "ルーラー":
          kanji = "裁";
          break;
        case "アヴェンジャー":
          kanji = "讐";
          break;
        case "ムーンキャンサー":
          kanji = "月";
          break;
        case "アルターエゴ":
          kanji = "分";
          break;
        case "フォーリナー":
          kanji = "降";
          break;
        case "プリテンダー":
          kanji = "詐";
          break;
        default:
          kanji = className;
          break;
      }

      return kanji;
    }

    /// <summary>
    /// 色を一文字にします。
    /// </summary>
    /// <param name="color"></param>
    /// <returns></returns>
    private static string ColorToAChara(string? color)
    {
      string chr;
      switch (color)
      {
        case "Arts":
          chr = "A";
          break;
        case "Buster":
          chr = "B";
          break;
        case "Quick":
          chr = "Q";
          break;
        default:
          chr = color;
          break;
      }

      return chr;
    }

    /// <summary>
    /// 範囲を一文字にします。
    /// </summary>
    /// <param name="color"></param>
    /// <returns></returns>
    private static string RangeToAChara(string? range)
    {
      string chr;
      switch (range)
      {
        case "全体":
          chr = "全";
          break;
        case "単体":
          chr = "単";
          break;
        case "補助":
          chr = "補";
          break;
        default:
          chr = range;
          break;
      }

      return chr;
    }
  }
}
