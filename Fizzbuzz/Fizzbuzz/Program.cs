using System;
using System.Collections.Generic;
using System.Linq;

namespace Fizzbuzz
{
    static class Program
    {
        static void Main(string[] args)
        {
            var fizzbuzz = FizzBuzz(1,100);
            fizzbuzz.ToList().ForEach(Console.WriteLine);
        }

        public static IEnumerable<string> FizzBuzz(int rangeStart, int rangeEnd)
        {
            var range = Enumerable.Range(rangeStart, rangeEnd-rangeStart);
            return range.Select(FizzBuzzContent);
        }

        public static string FizzBuzzContent(int i)
        {
            bool fizz = i % 3 == 0;
            bool buzz = i % 5 == 0;
            if (fizz && buzz) return "FizzBuzz";
            if (fizz) return "Fizz";
            if (buzz) return "Buzz";
            return i.ToString();
        }
    }
}
