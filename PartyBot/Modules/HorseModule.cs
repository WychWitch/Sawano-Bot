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
using System.Linq;

namespace PartyBot.Modules
{
    public class HorseModule : ModuleBase<SocketCommandContext>
    {
        Dictionary<string, string> horseDictionary = new Dictionary<string, string>();

        [Command("Horse")]
        public async Task Horse(string substring = "horse")
        {
            LoadJson();
            string lowerString = substring.ToLower();
            
            if (horseDictionary.ContainsKey(lowerString))
            {
                await ReplyAsync(horseDictionary[lowerString]);
            }
            else if (lowerString == "random")
            {
                List<string> listURLs = horseDictionary.Values.ToList();
                Random rng = new Random();
                string randURL = listURLs[rng.Next(listURLs.Count)];
                await ReplyAsync(randURL);
            }
            else
            {
                await ReplyAsync("neigh");
            }    
        }
        public void LoadJson()
        {
            using (StreamReader reader = new StreamReader("horse.json"))
            {
                string json = reader.ReadToEnd();
                horseDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            }
        }
    }
}
