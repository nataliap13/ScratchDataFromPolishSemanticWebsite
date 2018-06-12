using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Test.Models;
using Test.Services;

namespace Test.Controllers
{
    public class HomeController : Controller
    {
        private Object thisLock = new Object();
        [HttpGet]
        public IActionResult Index()
        {
            var ways = new List<List<Word_n_Sim>>();
            List<double> multiply_simmilarity = new List<double>();
            ViewBag.ways = ways;
            ViewBag.multiply_simmilarity = multiply_simmilarity;
            ViewBag.time = new TimeSpan(0);
            return View();
        }

        [HttpPost]
        public IActionResult Index(InputWords words)
        {
            //10 watkow, 11s
            int threads = 16;
            var database = new WebsiteDataService();
            var ways = new List<List<Word_n_Sim>>();
            var begin = DateTime.Now;
            var time = new TimeSpan(0);
            try
            {
                int temp = 0;
                var result = database.wordnet_go(new List<Word_n_Sim>() { new Word_n_Sim("0", words.BeginWord) }, words.BeginWord, new List<List<Word_n_Sim>>() { }, ref temp, thisLock);
                result = database.wordnet_go(new List<Word_n_Sim>() { new Word_n_Sim("0", words.EndWord) }, words.EndWord, new List<List<Word_n_Sim>>() { }, ref temp, thisLock);
            }
            catch (Exception)
            {
                var endtime = DateTime.Now;
                time = endtime - begin;
                ViewBag.ways = ways;
                ViewBag.multiply_simmilarity = 0;
                ViewBag.time = time;
                return View();
            }

            var first_search = new List<Word_n_Sim>();
            first_search.Add(new Word_n_Sim("0", words.BeginWord));
            var search_lst = new List<List<Word_n_Sim>>();
            search_lst.Add(first_search);
            int length = int.MaxValue;
            var alrdy_searched = new Dictionary<string, int>();
            var tasklist = new List<Task<List<List<Word_n_Sim>>>>();
            while (true)
            {
                if (tasklist.Count == 0)
                {
                    if (search_lst.Count == 0 || (search_lst.Count > 0 && search_lst.First().Count >= length))
                    {
                        //Console.WriteLine("\nPrzerywam!");
                        break;
                    }
                }

                if (tasklist.Count < threads)
                {
                    if (search_lst.Count > 0 && search_lst.First().Count < length)
                    {
                        var way = search_lst.First();
                        search_lst.Remove(way);
                        if (way.Count() < length)
                        {
                            if (alrdy_searched.ContainsKey(way.Last().Word) == false)
                            {
                                alrdy_searched.Add(way.Last().Word, way.Count);
                                var task = Task.Run(() => database.wordnet_go(way, words.EndWord, ways, ref length, thisLock));
                                tasklist.Add(task);
                            }
                            else if (alrdy_searched[way.Last().Word] >= way.Count)
                            {
                                alrdy_searched[way.Last().Word] = way.Count;
                                var task = Task.Run(() => database.wordnet_go(way, words.EndWord, ways, ref length, thisLock));
                                tasklist.Add(task);
                            }
                        }
                    }
                }
                if (tasklist.Count >= threads || search_lst.Count == 0 || (search_lst.Count > 0 && search_lst.First().Count >= length))
                {
                    //Console.WriteLine("\nCzekam na zakonczenie " + tasklist.Count + " watkow.");
                    Task.WaitAll(tasklist.ToArray());
                    foreach (var singletask in tasklist)
                    {
                        var result = singletask.Result;
                        if (result.Count > 0)
                        {
                            search_lst = search_lst.Concat(result).ToList();
                        }
                    }
                    tasklist = new List<Task<List<List<Word_n_Sim>>>>();
                }
            }

            NumberFormatInfo provider = new NumberFormatInfo();
            provider.NumberDecimalSeparator = ".";
            List<double> multiply_simmilarity = new List<double>();
            foreach (var way in ways)
            {
                double multiply = 1;
                foreach (var step in way)
                {
                    if (step.Similarity != "0")
                    { multiply *= Convert.ToDouble(step.Similarity, provider); }
                }
                multiply_simmilarity.Add(multiply);
            }
            var end = DateTime.Now;
            time = end - begin;
            Console.WriteLine("\n" + words.BeginWord + " -> " + words.EndWord);
            Console.WriteLine("Time: " + time.TotalSeconds);
            ViewBag.ways = ways;
            ViewBag.multiply_simmilarity = multiply_simmilarity;
            ViewBag.time = time;
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
