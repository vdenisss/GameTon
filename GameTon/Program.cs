using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using GameTon;
using Newtonsoft.Json;
using static System.Net.WebRequestMethods;

class Program
{
    static void Main(string[] args)
    {
        Words.GetWords().Wait();
    }
}