using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PartyBot.Modules
{
    public class TestModule : ModuleBase<SocketCommandContext>
    {
        [Command("test")]
        public async Task test()
        {
            await ReplyAsync("motherfucker");
        }
    }
}
