using Discord.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PartyBot.Modules
{
    public class HorseModule : ModuleBase<SocketCommandContext>
    {
        public void LoadJson()
        {
            using (StreamReader reader = new StreamReader("horse.json"))
            {
                string json = reader.ReadToEnd();
                Dictionary<string, string> horseDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            }
        }
    }
}
