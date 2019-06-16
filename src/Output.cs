using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CandidateWebSpy
{
    public class Output{
        public DateTime Last {get; set;}
        public List<string> Log {get; set;}      

        public static Output Load(){
            return JsonConvert.DeserializeObject<Output>(File.ReadAllText("data/output.json"));
        }

        public static void Store(Output o){
            File.WriteAllText("data/output.json", JsonConvert.SerializeObject(o));
        }
    }   
}