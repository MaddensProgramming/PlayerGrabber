using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PlayerGrabber
{



    class Program
    {

        private static string[] periods = new string[] { "01", "04", "07", "10" };
        private static string[] periodsEarly = new string[] { "01", "07", };

        private static List<Player> Players { get; set; }


        static void Main(string[] args)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            Players = new List<Player>();

            var results = GetGames(6653);
            var test = results;
            // test.ForEach(line => Console.WriteLine(line));
            stopwatch.Stop();

            Console.WriteLine($"THIS TOOK {stopwatch.ElapsedMilliseconds} ms");


            results = results.Where(game => game.Opponent.Name != "?").ToList() ;
            var games = results.OrderBy(result => result.Opponent.Name).ToList();

            // games.ForEach(game => Console.WriteLine($"{game.Opponent.Name} {game.Result}"));

            var grouped = games.GroupBy(game => game.Opponent);
            var tester = grouped.OrderByDescending(group => group.Sum(game => game.Gains)).ToList();


            tester.ForEach(group =>
            {
                Console.WriteLine($"{group.Key.Name} {group.Sum(game => game.Gains)}");
                group.ToList().ForEach(game => Console.WriteLine($"   {game.Result} {game.Gains} {game.Date.ToShortDateString()}"));

            });


            Console.ReadLine();
        }







        public static List<Game> GetGames(int id)
        {
            var results = new List<Game>();

            for (int year = 2006; year < 2023; year++)
            {
                var usedperiod = year < 2013 ? periodsEarly : periods;
                foreach (var period in usedperiod)
                {

                    var result = GetDoc(id, year, period);
                    result.ForEach(game => results.Add(ProcessGame(game, year, period)));

                }
            }
            Console.WriteLine(results.Count);

            return results.Where(game => game.Opponent != null).ToList();

        }

        private static List<string> ProcessDoc(HtmlNode body)
        {

            if (body == null)
                return new List<string>();
            string text = Regex.Replace(body.InnerText, @"&#x([0-9]*C*);", " ");
            var lines = text.Split('\n');
            return lines.Where(line => line.Length > 4 && char.IsDigit(line[4]) && !line.Contains("Gemiddelde") && !line.Contains("Moyenne")).ToList();
        }

        private static List<string> GetDoc(int id, int year, string month)
        {
            string url = $"https://www.frbe-kbsb.be/sites/manager/GestionFICHES/FRBE_Indiv.php?mat={id}&per={year.ToString()}{month}";
            HtmlWeb web = new HtmlWeb();


            bool loaded = false;
            List<string> result = new List<string>();

            while (!loaded)
            {
                var doc = web.LoadFromWebAsync(url, Encoding.ASCII);

                var callback = doc.Result;
                var title = callback.DocumentNode.SelectSingleNode("//title");
                if (title.InnerText != "Service unavailable!")
                {
                    loaded = true;
                    result = ProcessDoc(callback.DocumentNode.SelectSingleNode("//div"));
                }
            }
            return result;
        }


        private static Game ProcessGame(string game, int year, string period)
        {
            if (!game.Contains('+') && !game.Contains("-0."))
                return new Game();

            if (year > 2016 || (year == 2016 && period == "10"))
            {
                return ProcessGameAfter2017(game);
            };

            return ProcessGameBefore2017(game);
        }

        private static Game ProcessGameAfter2017(string game)
        {
            var idOpponent = game.Substring(6, 5);
            var name = game.Substring(12, 27);
            var player = TryAddPlayer(idOpponent, name);
            var ratingOpponent = game.Substring(39, 4);
            var color = game.Substring(44, 1);
            var result = game.Substring(47, 3);
            var gains = game.Substring(58, 5);
            var date = game.Substring(67, 8);
            return new Game()
            {
                Opponent = player,
                Color = GetColor(color),
                Date = GetDate(date),
                RatingOpponent = int.Parse(ratingOpponent),
                Result = GetResult(result),
                Gains = decimal.Parse(gains)
            };
        }

        private static Game ProcessGameBefore2017(string game)
        {
            var idOpponent = game.Substring(6, 5);
            var name = game.Substring(12, 26);
            var player = TryAddPlayer(idOpponent, name);
            var ratingOpponent = game.Substring(38, 4);
            var color = game.Substring(43, 1);
            var result = game.Substring(46, 3);
            var gains = game.Substring(57, 5);
            var date = game.Substring(66, 8);
            return new Game()
            {
                Opponent = player,
                Color = GetColor(color),
                Date = GetDate(date),
                RatingOpponent = int.Parse(ratingOpponent),
                Result = GetResult(result),
                Gains = decimal.Parse(gains)
            };
        }

        private static ResultEnum GetResult(string result)
        {
            if (result == "1.0")
                return ResultEnum.Win;
            if (result == "0.5")
                return ResultEnum.Draw;
            if (result == "0.0")
                return ResultEnum.Loss;
            return ResultEnum.Error;
        }

        private static DateTime GetDate(string date)
        {
            return DateTime.ParseExact(date, "yyyyMMdd", CultureInfo.InvariantCulture);
        }

        private static ColorEnum GetColor(string color)
        {
            if (color == "W" || color == "B")
                return ColorEnum.White;
            if (color == "Z" || color == "N")
                return ColorEnum.Black;
            else return ColorEnum.Error;
        }

        private static Player TryAddPlayer(string id, string name)
        {
            name = name.Replace(",", "");
            name = name.ToUpper();
            name = name.Trim();
            var result = Players.FirstOrDefault(player => player.Name == name);
            //add id check
            if (result != null)
                return result;

            result = new Player() { Id = id, Name = name };
            Players.Add(result);
            return result;
        }

    }
}
