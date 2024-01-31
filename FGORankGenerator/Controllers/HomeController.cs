using System;
using System.Collections.Generic;
using System.Text;
using FGORankGenerator.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

namespace FGORankGenerator.Controllers
{
  public class HomeController : Controller
  {
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
      _logger = logger;
    }

    [HttpGet]
    public IActionResult Index()
    {
      var servantList = Scraping.GetServantData();

      return View("Index", servantList);
    }

    [HttpGet]
    public IActionResult Contact()
    {
      return View();
    }

    [HttpGet]
    public IActionResult CsvDownload(string download)
    {
      if (download == "download")
      {
        var servantList = Scraping.GetServantData();

        // csv生成
        var csvString = CsvWriter.CreateCsv(servantList);
        var fileName = DateTime.Now.ToString("yyMMdd") + "FGOrank.csv";

        // byteデータに変換
        var csvData = Encoding.GetEncoding("Shift_JIS").GetBytes(csvString);

        return File(csvData, "text/csv", fileName);
      }
      return View();
    }

    [HttpGet]
    public IActionResult GetLatestData(string getData)
    {
      if (getData == "getData")
      {
        var servantList = Scraping.GetServantData();

        return View("Index", servantList);
      }
      return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
      return View(new ErrorViewModel { 
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
      });
    }
  }
}
