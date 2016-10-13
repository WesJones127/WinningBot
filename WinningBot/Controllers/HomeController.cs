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
            IStrategy strat = new NoOverlap();
            Game game = JsonConvert.DeserializeObject<Game>(gameText.Data);
            Util.ParseGrid(ref game);

            List<Move> moves = new List<Move>();

            try
            {
                Debug.WriteLine("MOVE: " + (game.state.turnsElapsed + 1) + " ----- PLAYER: " + game.player);
                if(game.gridData.playerCoords.Count >= 1)
               moves = strat.getMoves(game);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
            }

            foreach (Move move in moves)
            {
                Util.Log(game.player, move.Print(game.state.cols));
            }
            JsonResult result = Json(moves);
            return result;
        }
    }
}
