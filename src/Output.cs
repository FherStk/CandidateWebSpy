using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CandidateWebSpy
{
    public class Output{
        public OutputDates Dates {get; set;}
        public List<string> Log {get; set;}      

        public static Output Load(){
            return JsonConvert.DeserializeObject<Output>(File.ReadAllText("data/output.json"));
        }

        public static void Store(Output o){
            File.WriteAllText("data/output.json", JsonConvert.SerializeObject(o));
        }
    }   

    public class OutputDates{
        public DateTime Ratings {get; set;}
        public DateTime Advertisements {get; set;}
        public DateTime Lists {get; set;}
        public DateTime Announcements {get; set;}
    }
}