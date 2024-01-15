using AngleSharp.Html.Parser;
using AngleSharp.Html.Dom;
using AngleSharp.Dom;

namespace FGORankGenerator.Models
{
  public static class Scraping
  {
    // 周回ランクと攻略ランクの合計最高ポイント
    private const double MAX_SERVANT_SCORE = 21.0; // SSS + SS

    // HttpClientは1つを使い回す必要がある
    private static readonly HttpClient _httpClient
      = new HttpClient() { Timeout = TimeSpan.FromSeconds(10) };

    // AppMedia URL
    private const string _appMediaURL = "https://appmedia.jp/fategrandorder/1351236";

    /// <summary>
    /// スクレイピングしたサーヴァント評価リストを返します。
    /// </summary>
    public static List<ServantModel> GetServantData()
    {
      var servantList = new List<ServantModel>();

      // AppMediaのHTML解析
      IHtmlDocument? appMediaDoc = GetParseHtml(_appMediaURL).Result;

      if (appMediaDoc != null)
      {
        // 周回全ランクテーブル取得
        InsertRank(
          servantList,
          appMediaDoc.QuerySelectorAll("#servant_ranking_orbit_all > table > tbody > tr"),
          "全"
        );

        // 周回単ランクテーブル取得
        InsertRank(
          servantList,
          appMediaDoc.QuerySelectorAll("#servant_ranking_orbit_single > table > tbody > tr"),
          "単"
        );

        // 周回サポランクテーブル取得
        InsertRank(
          servantList,
          appMediaDoc.QuerySelectorAll("#servant_ranking_orbit_supporter > table > tbody > tr"),
          "援"
        );

        // 高難易度サポランクテーブル取得
        InsertRank(
          servantList,
          appMediaDoc.QuerySelectorAll("#servant_ranking_difficulty_supporter > table > tbody > tr"),
          "難援"
        );

        // 高難易度サポテーブル取得
        var rateTable = appMediaDoc.QuerySelectorAll("#servant_ranking_difficulty_supporter > table > tbody > tr");
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
              if (url != null && url.Contains("fategrandorder/"))
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
          item.OverallRank = (item.AppMediaRate + item.AppMediaOrbit) / MAX_SERVANT_SCORE * 10.0;
        }

        // 総合ランクで降順ソート
        servantList = servantList.OrderByDescending(item => item.OverallRank).ToList();
      }

      return servantList;
    }
    

    /// <summary>
    /// URLからHTML解析ドキュメントを返します。
    /// </summary>
    /// <param name="url"></param>
    /// <returns>HTMl解析ドキュメント</returns>
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
    /// ランクテーブルを追加して返します。
    /// </summary>
    /// <param name="servantList"></param>
    /// <param name="table"></param>
    /// <param name="rankTyoe"></param>
    /// <returns></returns>
    private static List<ServantModel> InsertRank(
      List<ServantModel> servantList,
      IHtmlCollection<IElement> table,
      string rankType
    )
    {
      foreach (var row in table)
      {
        // data-tagに値が入っていたらラベルなのでスキップ
        string? dataTag = row.GetAttribute("data-tag");
        if (dataTag == null)
        {
          // ランクを保持
          var dataRank = row.GetAttribute("data-rank");

          // 中身を再解析
          var parser = new HtmlParser();
          IHtmlDocument rowDoc = parser.ParseDocument(row.InnerHtml);

          // aタグ（鯖URL, レア度, タイプ, クラス）を取得
          var aList = rowDoc.QuerySelectorAll("a");

          // imgタグ（鯖名）を取得
          var imgList = rowDoc.QuerySelectorAll("img");

          int num = 0;
          foreach (var item in aList)
          {
            // URLからIDを取得
            var url = item.GetAttribute("href");
            if (url != null && url.Contains("fategrandorder/"))
            {
              url = url.Replace("/fategrandorder/", "");
            }
            else
            {
              url = url.Replace("fategrandorder/", "");
            }
            // レア度取得
            var rarity = item.GetAttribute("data-rarity");
            // 鯖名を取得。imgタグはaタグと一対一前提なので、NEWタグだったらスキップ
            var name = imgList[num].GetAttribute("alt");
            if (name == "NEW")
            {
              num++;
              name = imgList[num].GetAttribute("alt");
            }
            num++;

            // メカエリIIはIとURLが同じでIDがかぶるのでスキップ
            if (name == "メカエリチャンⅡ号機") continue;
            // 哪吒は文字化けするのでひらがなにする
            if (name == "哪吒") name = "なた";

            // 周回ランクの場合
            if (rankType != "難援")
            {
              // リストに追加
              if (url != null && rarity != null && name != null)
              {
                servantList.Add(new ServantModel()
                {
                  Id = int.Parse(url),
                  Name = name.Replace("<br>", ""),
                  Rarity = int.Parse(rarity),
                  Class = ClassToKanji(item.GetAttribute("data-class")),
                  Type = ColorToAChara(item.GetAttribute("data-type")),
                  Range = rankType,
                  AppMediaOrbit = RankToInt(dataRank),
                });
              }
            }
            // 高難易度ランクの場合
            else
            {
              var id = int.Parse(url);

              // URL IDからリストのインデックス検索
              var index = servantList.FindIndex(x => x.Id == id);

              // ランクを挿入
              if (index != -1)  // 高難易度ランクにだけ存在すると、index==-1になる
              {
                servantList[index].AppMediaRate = RankToInt(dataRank);
              }
              else
              {
                // 高難易度ランクのみの場合、リストに追加
                if (url != null && rarity != null && name != null)
                {
                  servantList.Add(new ServantModel()
                  {
                    Id = int.Parse(url),
                    Name = name.Replace("<br>", ""),
                    Rarity = int.Parse(rarity),
                    Class = ClassToKanji(item.GetAttribute("data-class")),
                    Type = ColorToAChara(item.GetAttribute("data-type")),
                    Range = "援",
                    AppMediaRate = RankToInt(dataRank),
                  });
                }
              }
            }
          }
        }
      }
      return servantList;
    }

    /// <summary>
    /// ランクに応じた数値を返します。
    /// </summary>
    private static int RankToInt(string? rankStr)
    {
      return rankStr switch
      {
        "SSS" => 11,
        "SS" => 10,
        "EX" => 10,
        "S+" => 9,
        "S" => 8,
        "A+" => 7,
        "A" => 6,
        "B+" => 5,
        "B" => 4,
        "C+" => 3,
        "C" => 2,
        "D" => 1,
        _ => 0,
      };
    }

    /// <summary>
    /// クラスに応じた漢字一文字を返します。
    /// </summary>
    private static string? ClassToKanji(string? className)
    {
      return className switch
      {
        "セイバー" => "剣",
        "アーチャー" => "弓",
        "ランサー" => "槍",
        "ライダー" => "騎",
        "キャスター" => "術",
        "アサシン" => "殺",
        "バーサーカー" => "狂",
        "ルーラー" => "裁",
        "アヴェンジャー" => "讐",
        "アルターエゴ" => "分",
        "ムーンキャンサー" => "月",
        "フォーリナー" => "降",
        "プリテンダー" => "詐",
        "ビースト" => "獣",
        "シールダー" => "盾",
        _ => "謎",
      };
    }

    /// <summary>
    /// 色を一文字にして返します。
    /// </summary>
    private static string? ColorToAChara(string? color)
    {
      return color switch
      {
        "Arts" => "A",
        "Buster" => "B",
        "Quick" => "Q",
        _ => "複",
      };
    }

    /// <summary>
    /// 範囲を一文字にして返します。
    /// </summary>
    private static string? RangeToAChara(string? range)
    {
      return range switch
      {
        "全体" => "全",
        "単体" => "単",
        "補助" => "補",
        _ => "複",
      };
    }
  }
}
