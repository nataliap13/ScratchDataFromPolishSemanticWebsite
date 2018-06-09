using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Test.Models;
using Test.Services;

namespace Test.Controllers
{
    public class WWWController : Controller
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
            int threads = 8;
            var database = new WebsiteDataService();
            var ways = new List<List<Word_n_Sim>>();
            var begin = DateTime.Now;
            var time = new TimeSpan(0);
            try
            {
                var result = database.wordnet_go(new List<Word_n_Sim>() { new Word_n_Sim("0", words.BeginWord) }, words.BeginWord, new List<List<Word_n_Sim>>() { }, thisLock);
                result = database.wordnet_go(new List<Word_n_Sim>() { new Word_n_Sim("0", words.EndWord) }, words.EndWord, new List<List<Word_n_Sim>>() { }, thisLock);
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
            var already_searched = new List<string>();
            var tasklist = new List<Task<List<Word_n_Sim>>>();
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
                        lock (thisLock)
                        {
                            search_lst.Remove(way);
                        }
                        if (way.Count() < length && (already_searched.Contains(way.Last().Word)) == false)
                        {
                            already_searched.Add(way.Last().Word);
                            var task = Task.Run(() => database.wordnet_go(way, words.EndWord, search_lst, thisLock));
                            tasklist.Add(task);
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
                            ways.Add(result);
                            length = result.Count;
                        }
                    }
                    tasklist = new List<Task<List<Word_n_Sim>>>();
                }
            }
            var end = DateTime.Now;
            time = end - begin;
            Console.WriteLine("\n" + words.BeginWord + " -> " + words.EndWord);
            Console.WriteLine("Time: " + time.TotalSeconds);

            NumberFormatInfo provider = new NumberFormatInfo();
            provider.NumberDecimalSeparator = ".";
            //provider.NumberGroupSizes = new int[] { 3 };
            List<double> multiply_simmilarity = new List<double>();
            foreach(var way in ways)
            {
                double multiply = 1;
                foreach(var step in way)
                {
                    if(step.Similarity != "0")
                    { multiply *= Convert.ToDouble(step.Similarity, provider); }
                }
                Console.WriteLine(multiply);
                multiply_simmilarity.Add(multiply);
            }
            ViewBag.ways = ways;
            ViewBag.multiply_simmilarity = multiply_simmilarity;
            ViewBag.time = time;
            return View();
        }

    }
}