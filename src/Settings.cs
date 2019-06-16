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
        public SettingsCredentials Credentials {get; set;}
        public SettingsPolling Polling {get; set;}
        public SettingsLog Log {get; set;}
        public SettingsMailing Mailing {get; set;}

        public static Settings Load(){
            return JsonConvert.DeserializeObject<Settings>(File.ReadAllText("settings.json"));
        }
    }

    public class SettingsCredentials{
        public SettingsID ID {get; set;}
        public string User {get; set;}
        public string Pass {get; set;}  
    }

    public class SettingsPolling{
        public short Interval {get; set;}
    }

    public class SettingsLog{
        public short Level;
        public short Entries;
    }

     public class SettingsMailing{
        public string SmtpServer;
        public string User;
        public string Pass;
        public string From;
        public string To;
    }
}