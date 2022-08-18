using AngleSharp.Html.Parser;
using AngleSharp.Html.Dom;

namespace FGORankGenerator.Models
{
  public static class Scraping
  {
    const double MAX_SERVANT_SCORE = 20;

    // AppMedia URL
    private const string appMediaURL = "https://appmedia.jp/fategrandorder/1351236";

    // HttpClientは使い回す必要がある
    private static readonly HttpClient _httpClient
      = new HttpClient() { Timeout = TimeSpan.FromSeconds(10) };

    /// <summary>
    /// スクレイピングした各サーヴァント評価リストを返します。
    /// </summary>
    /// <returns></returns>
    public static List<ServantModel> GetServantData()
    {
      var servantList = new List<ServantModel>();

      // AppMediaのHTML解析
      IHtmlDocument? appMediaDoc = GetParseHtml(appMediaURL).Result;

      if (appMediaDoc != null)
      {
        // 攻略ランク以外取得
        var orbitTable = appMediaDoc.QuerySelectorAll("#servant_ranking_orbit > table > tbody > tr");
        foreach (var orbitRow in orbitTable)
        {
          string? dataTag = orbitRow.GetAttribute("data-tag");
          if (dataTag == null)
          {
            var dataRank = orbitRow.GetAttribute("data-rank");

            var parser = new HtmlParser();
            IHtmlDocument rowDoc = parser.ParseDocument(orbitRow.InnerHtml);

            var aList = rowDoc.QuerySelectorAll("a");
            var imgList = rowDoc.QuerySelectorAll("img");
            int num = 0;
            foreach (var item in aList)
            {
              var url = item.GetAttribute("href");
              var rarity = item.GetAttribute("data-rarity");
              var name = imgList[num].GetAttribute("alt");
              if (name == "哪吒")
              {
                // 哪吒は文字化けするのでひらがなにする
                name = "なた";
              }
              else
              {
                // 改行は削除
                name = name.Replace("<br>", "");
              }

              if (url != null && rarity != null)
              {
                servantList.Add(new ServantModel()
                {
                  Id = int.Parse(url.Replace("https://appmedia.jp/fategrandorder/", "")),
                  Rarity = int.Parse(rarity),
                  Type = ColorToAChara(item.GetAttribute("data-type")),
                  Class = ClassToKanji(item.GetAttribute("data-class")),
                  Range = RangeToAChara(item.GetAttribute("data-range")),
                  Name = name,
                  AppMediaOrbit = RankToNum(dataRank),
                });
              }
              num++;
            }
          }
        }

        // 攻略ランク取得
        var rateTable = appMediaDoc.QuerySelectorAll("#servant_ranking_rate > table > tbody > tr");
        foreach (var rateRow in rateTable)
        {
          string? dataTag = rateRow.GetAttribute("data-tag");
          if (dataTag == null)
          {
            var dataRank = rateRow.GetAttribute("data-rank");

            var parser = new HtmlParser();
            IHtmlDocument rowDoc = parser.ParseDocument(rateRow.InnerHtml);

            var aList = rowDoc.QuerySelectorAll("a");
            foreach (var item in aList)
            {
              var url = item.GetAttribute("href");
              if (url != null)
              {
                var id = int.Parse(url.Replace("https://appmedia.jp/fategrandorder/", ""));
                var index = servantList.FindIndex(x => x.Id == id);
                servantList[index].AppMediaRate = RankToNum(dataRank);
              }
            }
          }
        }

        // 総合ランク
        foreach (var item in servantList)
        {
          item.OverallRank = (item.AppMediaRate + item.AppMediaOrbit) / MAX_SERVANT_SCORE * 10;
        }

        // 総合ランク順に整理
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
        case "SS":
          num = 10;
          break;
        case "S":
          num = 9;
          break;
        case "A+":
          num = 8;
          break;
        case "A":
          num = 7;
          break;
        case "B+":
          num = 6;
          break;
        case "B":
          num = 5;
          break;
        case "C":
          num = 4;
          break;
        case "D":
          num = 3;
          break;
        case "E":
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
