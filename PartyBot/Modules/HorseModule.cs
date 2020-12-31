using Discord.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using PartyBot.Services;

namespace PartyBot.Modules
{
    public class HorseModule : ModuleBase<SocketCommandContext>
    {
        [Command("Horse")]

       /* public async Task Horse()
        {
            return await 
        } */
        public void LoadJson()
        {
            using (StreamReader reader = new StreamReader(".../horse.json"))
            {
                string json = reader.ReadToEnd();
                Dictionary<string, string> horseDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            }
        }
    }
}
