﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Web.Http;
using System.Web.Mvc;
using Newtonsoft.Json;
using WinningBot.Models;
using WinningBot.Strategies;

namespace WinningBot.Controllers
{
    public class HomeController : Controller
    {
        [System.Web.Mvc.HttpGet]
        public JsonResult Index()
        {
            return Json("hello", JsonRequestBehavior.AllowGet);
        }

        [System.Web.Mvc.HttpPost]
        public JsonResult Index([FromBody] JsonData gameText)
        {
            try
            {
                IStrategy strat = new NoOverlap();
                Game game = JsonConvert.DeserializeObject<Game>(gameText.Data);
                List<Move> moves = strat.getMoves(game);
                Debug.WriteLine(JsonConvert.SerializeObject(moves));
                return Json(moves);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return Json(new List<Move>());
            }
        }
    }
}