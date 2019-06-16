using System.IO;
using Newtonsoft.Json;

namespace CandidateWebSpy
{

    public enum SettingsID{
        NONE = 0,
        DNI = 1,
        NIE = 2,
        PASSAPORT = 3
        }
        
    public class Settings{
        SettingsCredentials Credentials {get; set;}
        SettingsPolling Polling {get; set;}
        SettingsLog Log {get; set;}

        public static Settings Load(){
            return JsonConvert.DeserializeObject<Settings>(File.ReadAllText("settings.json"));
        }
    }

    public class SettingsCredentials{
        SettingsID ID {get; set;}
        string User {get; set;}
        string Pass {get; set;}  
    }

    public class SettingsPolling{
        short Interval {get; set;}
    }

    public class SettingsLog{
        short Level;
        short Entries;
    }
}