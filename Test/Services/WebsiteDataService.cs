using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Test.Models;
using HtmlAgilityPack;

namespace Test.Services
{
    public class WebsiteDataService
    {
        public List<Word_n_Sim> wordnet_go(List<Word_n_Sim> way, string end_word, List<List<Word_n_Sim>> search_lst, Object thisLock)
        {
        //    lock (thisLock)
        //    {
        //        Console.WriteLine("");
        //        foreach (var step in way)
        //        {
        //            Console.Write(" -> " + step.Word);
        //        }
        //    }

            var html = @"http://nlp.pwr.wroc.pl/wordnet/msr/" + way.Last().Word;
            HtmlWeb web = new HtmlWeb();
            var doc = web.Load(html);

            var nodes_similarities = doc.DocumentNode.SelectNodes("/html/body/table/tr/td[1]");//podobienstwo
            //var task_similarities = Task.Run(() => Get_list_of_strings_from_HtmlNodeCollection(nodes_similarities));
            var similarities = Get_list_of_strings_from_HtmlNodeCollection(nodes_similarities);

            var nodes_words = doc.DocumentNode.SelectNodes("//*[@class=\"l\"]");//slowo
            //var task_words = Task.Run(() => Get_list_of_strings_from_HtmlNodeCollection(nodes_words));
            var words = Get_list_of_strings_from_HtmlNodeCollection(nodes_words);

            //var task_array = new Task<List<string>>[2]//[2]
            //{
            //        task_similarities,
            //        task_words
            //};
            //Task.WaitAll(task_array);

            //var similarities = task_similarities.Result;
            //var words = task_words.Result;

            //Console.WriteLine("Znaleziono nastepujace slowa");
            //foreach (var word in words)
            //{ Console.Write(word + " "); }
            //Console.WriteLine("");

            //laczymy slowo i podobienstwo w pare
            List<Word_n_Sim> list_of_words_n_sims = new List<Word_n_Sim>();
            for (int i = 0; i < words.Count; i++)
            { list_of_words_n_sims.Add(new Word_n_Sim(similarities[i], words[i])); }

            foreach (var word_n_sim in list_of_words_n_sims)
            {
                if (word_n_sim.Word == end_word)//jest tylko jedno slowo na liscie slow odczytanych, ktore bedzie szukanym
                {
                    way.Add(word_n_sim);
                    return way;
                }
            }

            //jesli zadne ze znalezionych slow nie jest slowem szukanym
            foreach (var word_n_sim in list_of_words_n_sims)
            {
                if (way.Exists(x => x.Word == word_n_sim.Word))//pomin ten wyraz
                { }
                else
                {
                    List<Word_n_Sim> way_to_be_searched = new List<Word_n_Sim>();
                    foreach (var word in way)//kopiowanie sciezki
                    {
                        way_to_be_searched.Add(word);
                    }

                    way_to_be_searched.Add(word_n_sim);
                    lock (thisLock)
                    { search_lst.Add(way_to_be_searched); }
                }
            }
            return new List<Word_n_Sim>();
        }
        private List<string> Get_list_of_strings_from_HtmlNodeCollection(HtmlNodeCollection nodes)
        {
            List<string> list_of_strings = new List<string>();
            foreach (var x in nodes)
            { list_of_strings.Add(x.InnerText); }
            return list_of_strings;
        }
    }
}
