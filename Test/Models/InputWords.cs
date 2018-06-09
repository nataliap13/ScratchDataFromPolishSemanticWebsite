using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
namespace Test.Models
{
    public class InputWords
    {
        private string _beginword;
        private string _endnword;
        [Display(Name = "Słowo początkowe")]
        public string BeginWord
        {
            get => _beginword;
            set
            { _beginword = Validate(value); }
        }

        [Display(Name = "Słowo końcowe")]
        public string EndWord
        {
            get => _endnword;
            set
            { _endnword = Validate(value); }
        }

        public InputWords()
        { }

        private string Validate(string OriginWord)
        {
            string PolishDictionary = "ąćęłńóśżź";
            var word = OriginWord.ToLower();
            string result = string.Empty;
            foreach (var letter in word)
            {
                
                if (('a' <= letter && letter <= 'z') || PolishDictionary.Contains(letter))
                {
                    result += letter;
                }
                else
                if (letter == ' ' && result.Count() > 0 && ('a' <= result.Last() && result.Last() <= 'z') || PolishDictionary.Contains(result.Last()))
                {
                    result += letter;
                }
            }
            //Console.WriteLine("result " + result);
            return result;
        }
        /*
        public InputWords(string BeginWord, string EndWord)
        {
            Console.WriteLine("KONSTRUKTOR");
            this.BeginWord = string.Empty;
            this.EndWord = string.Empty;
            string PolishDictionary = "ąćęłńóśżź";
            var beginword = BeginWord.ToLower();
            var endword = EndWord.ToLower();
            Console.WriteLine(beginword);
            Console.WriteLine(endword);
            foreach (var letter in beginword)
            {
                if (('a' <= letter && letter <= 'z') || PolishDictionary.Contains(letter))
                {
                    this.BeginWord += letter;
                }
            }

            foreach (var letter in endword)
            {
                if (('a' <= letter && letter <= 'z') || PolishDictionary.Contains(letter))
                {
                    this.EndWord += letter;
                }
            }
        }
        */
    }
}
