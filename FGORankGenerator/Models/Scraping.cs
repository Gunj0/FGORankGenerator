using AngleSharp.Html.Parser;
using AngleSharp.Html.Dom;
using AngleSharp.Text;

namespace FGORankGenerator.Models
{
  public static class Scraping
  {
    // 周回ランクと攻略ランクの合計最高ポイント
    private const double MAX_SERVANT_SCORE = 19;

    // AppMedia URL
    private const string appMediaURL = "https://appmedia.jp/fategrandorder/1351236";

    // HttpClientは使い回す必要がある
    private static readonly HttpClient _httpClient
      = new HttpClient() { Timeout = TimeSpan.FromSeconds(10) };

    /// <summary>
    /// スクレイピングした各サーヴァント評価リストを返します。
    /// </summary>
    public static List<ServantModel> GetServantData()
    {
      var servantList = new List<ServantModel>();

      // AppMediaのHTML解析
      IHtmlDocument? appMediaDoc = GetParseHtml(appMediaURL).Result;

      if (appMediaDoc != null)
      {
        // 周回ランクテーブル取得
        var orbitTable = appMediaDoc.QuerySelectorAll("#servant_ranking_orbit > table > tbody > tr");
        foreach (var orbitRow in orbitTable)
        {
          // data-tagに値が入っていたらラベルなのでスキップ
          string? dataTag = orbitRow.GetAttribute("data-tag");
          if (dataTag == null)
          {
            // 周回ランクを保持
            var dataRank = orbitRow.GetAttribute("data-rank");

            // 中身を再解析
            var parser = new HtmlParser();
            IHtmlDocument rowDoc = parser.ParseDocument(orbitRow.InnerHtml);

            // aタグから鯖URL・レア度・タイプ・クラス・範囲を取得
            var aList = rowDoc.QuerySelectorAll("a");
            // imgタグから鯖名を取得
            var imgList = rowDoc.QuerySelectorAll("img");
            int num = 0;

            foreach (var item in aList)
            {
              var url = item.GetAttribute("href");
              if (url != null && url.Contains("/fategrandorder/"))
              {
                url = url.Replace("/fategrandorder/", "");
              }
              else
              {
                url = url.Replace("fategrandorder/", "");
              }
              var rarity = item.GetAttribute("data-rarity");
              var name = imgList[num].GetAttribute("alt");
              num++;

              // メカエリIIはIとURLが同じでIDがかぶるのでスキップ
              if (name == "メカエリチャンⅡ号機") continue;
              // 哪吒は文字化けするのでひらがなにする
              if (name == "哪吒") name = "なた";

              // リストに追加
              if (url != null && rarity != null && name != null)
              {
                servantList.Add(new ServantModel()
                {
                  Id = int.Parse(url),
                  Rarity = int.Parse(rarity),
                  Type = ColorToAChara(item.GetAttribute("data-type")),
                  Class = ClassToKanji(item.GetAttribute("data-class")),
                  Range = RangeToAChara(item.GetAttribute("data-range")),
                  Name = name.Replace("<br>", ""),
                  AppMediaOrbit = RankToInt(dataRank),
                });
              }
            }
          }
        }

        // 攻略ランクテーブル取得
        var rateTable = appMediaDoc.QuerySelectorAll("#servant_ranking_rate > table > tbody > tr");
        foreach (var rateRow in rateTable)
        {
          // data-tagに値が入っていたらラベルなのでスキップ
          string? dataTag = rateRow.GetAttribute("data-tag");
          if (dataTag == null)
          {
            // 攻略ランクを保持
            var dataRank = rateRow.GetAttribute("data-rank");

            // 中身を再解析
            var parser = new HtmlParser();
            IHtmlDocument rowDoc = parser.ParseDocument(rateRow.InnerHtml);

            // aタグから鯖URLを取得
            var aList = rowDoc.QuerySelectorAll("a");

            foreach (var item in aList)
            {
              var url = item.GetAttribute("href");
              if (url != null && url.Contains("/fategrandorder/"))
              {
                url = url.Replace("/fategrandorder/", "");
              }
              else
              {
                url = url.Replace("fategrandorder/", "");
              }
              if (url != null)
              {
                var id = int.Parse(url);
                // URL IDからリストのインデックス検索
                var index = servantList.FindIndex(x => x.Id == id);

                // 攻略ランクを挿入
                if (index != -1)  // 攻略ランクにだけ存在すると、index==-1になる
                {
                  servantList[index].AppMediaRate = RankToInt(dataRank);
                }
              }
            }
          }
        }

        // 総合ランクを挿入
        foreach (var item in servantList)
        {
          item.OverallRank = (item.AppMediaRate + item.AppMediaOrbit) / MAX_SERVANT_SCORE * 10;
        }

        // 総合ランクで降順ソート
        servantList = servantList.OrderByDescending(item => item.OverallRank).ToList();
      }
      return servantList;
    }

    /// <summary>
    /// URLを渡すと解析済みHTMLドキュメントを返します。
    /// </summary>
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
    private static int RankToInt(string? rankStr)
    {
      int rankInt;
      switch (rankStr)
      {
        case "EX":
          rankInt = 10;
          break;
        case "SSS":
          rankInt = 10;
          break;
        case "SS":
          rankInt = 9;
          break;
        case "S":
          rankInt = 8;
          break;
        case "A+":
          rankInt = 7;
          break;
        case "A":
          rankInt = 6;
          break;
        case "B+":
          rankInt = 6;
          break;
        case "B":
          rankInt = 5;
          break;
        case "C+":
          rankInt = 4;
          break;
        case "C":
          rankInt = 3;
          break;
        case "D":
          rankInt = 2;
          break;
        default:
          rankInt = 1;
          break;
      }

      return rankInt;
    }

    /// <summary>
    /// クラスに応じた漢字一文字を返します。
    /// </summary>
    private static string? ClassToKanji(string? className)
    {
      string? kanji;
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
        case "ルーラー":
          kanji = "裁";
          break;
        case "アヴェンジャー":
          kanji = "讐";
          break;
        case "アルターエゴ":
          kanji = "分";
          break;
        case "ムーンキャンサー":
          kanji = "月";
          break;
        case "フォーリナー":
          kanji = "降";
          break;
        case "プリテンダー":
          kanji = "詐";
          break;
        case "ビースト":
          kanji = "獣";
          break;
        case "シールダー":
          kanji = "盾";
          break;
        default:
          kanji = "謎";
          break;
      }

      return kanji;
    }

    /// <summary>
    /// 色を一文字にして返します。
    /// </summary>
    private static string? ColorToAChara(string? color)
    {
      string? chr;
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
          chr = "複";
          break;
      }

      return chr;
    }

    /// <summary>
    /// 範囲を一文字にして返します。
    /// </summary>
    private static string? RangeToAChara(string? range)
    {
      string? chr;
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
          chr = "複";
          break;
      }

      return chr;
    }
  }
}
