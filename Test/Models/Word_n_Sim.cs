using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Test.Models
{
    public class Word_n_Sim
    {
        private string _word;
        private string _similarity;

        public string Word { get => _word; set => _word = value; }
        public string Similarity { get => _similarity; set => _similarity = value; }

        public Word_n_Sim(string similarity, string word)
        {
            _word = word;
            _similarity = similarity;
        }
    }
}
