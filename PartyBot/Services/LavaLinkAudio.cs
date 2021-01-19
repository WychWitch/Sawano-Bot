﻿using Discord;
using Discord.WebSocket;
using PartyBot.Handlers;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Victoria;
using Victoria.EventArgs;
using Victoria.Enums;
using Victoria.Responses.Rest;
using System.Diagnostics;
using System.Collections.Generic;

namespace PartyBot.Services
{
    public sealed class LavaLinkAudio
    {
        LavaTrack track = new LavaTrack();
        private readonly LavaNode _lavaNode;

        public LavaLinkAudio(LavaNode lavaNode)
            => _lavaNode = lavaNode;

        public string ProgressBar()
        {
            double progress = Math.Round((track.Position / track.Duration), 1);
            string progressBar;

            switch (progress)
            {
                case .10:
                    progressBar = "▬▬⭕▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬";
                    break;
                case .20:
                    progressBar = "▬▬▬▬⭕▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬";
                    break;
                case .30:
                    progressBar = "▬▬▬▬▬▬⭕▬▬▬▬▬▬▬▬▬▬▬▬▬";
                    break;
                case .40:
                    progressBar = "▬▬▬▬▬▬▬▬⭕▬▬▬▬▬▬▬▬▬▬▬";
                    break;
                case .50:
                    progressBar = "▬▬▬▬▬▬▬▬▬▬⭕▬▬▬▬▬▬▬▬▬";
                    break;
                case .60:
                    progressBar = "▬▬▬▬▬▬▬▬▬▬▬▬⭕▬▬▬▬▬▬▬";
                    break;
                case .70:
                    progressBar = "▬▬▬▬▬▬▬▬▬▬▬▬▬▬⭕▬▬▬▬▬";
                    break;
                case .80:
                    progressBar = "▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬⭕▬▬▬";
                    break;
                case .90:
                    progressBar = "▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬⭕▬";
                    break;
                default:
                    progressBar = "⭕▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬";
                    break;
            }
            return progressBar;
        }

        public string DurationFormatter()
        {
            string duration;

            if (track.Duration.TotalMinutes >= 60)
            {
                duration = $"{track.Position.Hours:d2}:{track.Position.Minutes:d2}:{ track.Position.Seconds:d2}/{track.Duration.Hours:d2}:{track.Duration.Minutes:d2}:{ track.Duration.Seconds:d2}";
            }
            else
            {
                duration = $"{track.Position.Minutes:d2}:{ track.Position.Seconds:d2}/{ track.Duration.Minutes:d2}:{ track.Duration.Seconds:d2}";
            }

            return duration;
        }

        public async Task<Embed> JoinAsync(IGuild guild, IVoiceState voiceState, ITextChannel textChannel)
        {
            if (_lavaNode.HasPlayer(guild))
            {
                return await EmbedHandler.CreateErrorEmbed("Music, Join", "I'm already connected to a voice channel!");
            }

            if (voiceState.VoiceChannel is null)
            {
                return await EmbedHandler.CreateErrorEmbed("Music, Join", "You must be connected to a voice channel!");
            }

            try
            {
                await _lavaNode.JoinAsync(voiceState.VoiceChannel, textChannel);
                return await EmbedHandler.CreateBasicEmbed("Music, Join", $"Joined {voiceState.VoiceChannel.Name}.", Color.Green);
            }
            catch (Exception ex)
            {
                return await EmbedHandler.CreateErrorEmbed("Music, Join", ex.Message);
            }
        }

        /*This is ran when a user uses either the command Join or Play
            I decided to put these two commands as one, will probably change it in future. 
            Task Returns an Embed which is used in the command call.. */

            /*s*/
        public async Task<Embed> PlayAsync(SocketGuildUser user, IGuild guild, IVoiceState voiceState, ITextChannel textChannel, string query)
        {
            //Check If User Is Connected To Voice Cahnnel.
            if (user.VoiceChannel == null)
            {
                return await EmbedHandler.CreateErrorEmbed("Music, Join/Play", "You Must First Join a Voice Channel.");
            }



            //Check the guild has a player available.
            if (!_lavaNode.HasPlayer(guild))
            {
                if (_lavaNode.HasPlayer(guild))
                {
                    return await EmbedHandler.CreateErrorEmbed("Music, Join", "I'm already connected to a voice channel!");
                }

                if (voiceState.VoiceChannel is null)
                {
                    return await EmbedHandler.CreateErrorEmbed("Music, Join", "You must be connected to a voice channel!");
                }

                try
                {
                    await _lavaNode.JoinAsync(voiceState.VoiceChannel, textChannel);
                }
                catch (Exception ex)
                {
                    return await EmbedHandler.CreateErrorEmbed("Music, Join", ex.Message);
                }
            }

            try
            {
                //Get the player for that guild.
                var player = _lavaNode.GetPlayer(guild);

                ////Find The Youtube Track the User requested.
                //LavaTrack track;

                var search = Uri.IsWellFormedUriString(query, UriKind.Absolute) ?
                    await _lavaNode.SearchAsync(query)
                    : await _lavaNode.SearchYouTubeAsync(query);

                //If we couldn't find anything, tell the user.
                if (search.LoadStatus == LoadStatus.NoMatches)
                {
                    return await EmbedHandler.CreateErrorEmbed("Music", $"I wasn't able to find anything for {query}.");
                }

                //Get the first track from the search results.
                //TODO: Add a 1-5 list for the user to pick from. (Like Fredboat)
                track = search.Tracks.FirstOrDefault();

                //If the Bot is already playing music, or if it is paused but still has music in the playlist, Add the requested track to the queue.
                if (player.Track != null && player.PlayerState is PlayerState.Playing || player.PlayerState is PlayerState.Paused)
                {
                    player.Queue.Enqueue(track);
                    await LoggingService.LogInformationAsync("Music", $"{track.Title} has been added to the music queue.");
                    return await EmbedHandler.CreateBasicEmbed("Music", $"{track.Title} has been added to queue.", Color.Blue);
                }

                //Player was not playing anything, so lets play the requested track.
                await player.PlayAsync(track);
                await LoggingService.LogInformationAsync("Music", $"Bot Now Playing: {track.Title}\nUrl: {track.Url}");
                string progressBar = ProgressBar();
                string duration = DurationFormatter();
                return await EmbedHandler.CreateBasicEmbed("Music", $"Now Playing: [{track.Title}]({track.Url})\n{progressBar} {duration}", Color.Blue);
            }

            //If after all the checks we did, something still goes wrong. Tell the user about it so they can report it back to us.
            catch (Exception ex)
            {
                return await EmbedHandler.CreateErrorEmbed("Music, Play", ex.Message);
            }

        }

        public async Task<Embed> JumpAsync(SocketGuildUser user, IGuild guild, IVoiceState voiceState, ITextChannel textChannel, string number)
        {
            var player = _lavaNode.GetPlayer(guild);
            int num;
            if (int.TryParse(number, out num))
            {
                if (num > player.Queue.Count())
                {
                    return await EmbedHandler.CreateBasicEmbed("Error", "Number was too big!", Color.Blue);
                }
                else
                {
                    List<Victoria.Interfaces.IQueueable> oldQueue = player.Queue.ToList();
                    player.Queue.Clear();
                    await player.PlayAsync(oldQueue.ElementAt(num - 1) as LavaTrack);

                    for (int i = num; i < oldQueue.Count(); i++)
                    {
                        player.Queue.Enqueue(oldQueue.ElementAt(i));
                    }


                    await LoggingService.LogInformationAsync("Music", $"Jumped to {track.Title}!");
                    return await EmbedHandler.CreateBasicEmbed("Music", $"Jumped to {track.Title}!", Color.Red);
                }
                
            }
            else
            {
                return await EmbedHandler.CreateBasicEmbed("Error", "You provided an invalid string. Try a real number", Color.Blue);
            }
            

        }


        public async Task<Embed> MoveTrackAsync(SocketGuildUser user, IGuild guild, IVoiceState voiceState, ITextChannel textChannel, string tracks)
        {
            var player = _lavaNode.GetPlayer(guild);

            string[] trackArr = tracks.Split(" ");

            int fTrackInt;
            int sTrackInt;

            if (int.TryParse(trackArr[0], out fTrackInt) && int.TryParse(trackArr[1], out sTrackInt))
            {
                if (fTrackInt > 0 && fTrackInt <= player.Queue.Count && sTrackInt > 0 && sTrackInt <= player.Queue.Count)
                {
                    List<Victoria.Interfaces.IQueueable> oldQueue = player.Queue.ToList();
                    player.Queue.Clear();
                    Victoria.Interfaces.IQueueable tempTrack = oldQueue[fTrackInt - 1];

                    oldQueue.RemoveAt(fTrackInt - 1);

                    oldQueue.Insert(sTrackInt - 1, tempTrack);


                    for (int i = 0; i < oldQueue.Count(); i++)
                    {
                        player.Queue.Enqueue(oldQueue.ElementAt(i));
                    }

                    return await EmbedHandler.CreateBasicEmbed("Move track", "Moved track!", Color.Blue);
                }
                else
                {
                    return await EmbedHandler.CreateBasicEmbed("Error", "One of your numbers were out of range", Color.Red);
                }
                
            }
            else
            {
                return await EmbedHandler.CreateBasicEmbed("Error", "Please insert a real number", Color.Red);
            }
            
        }

        public async Task<Embed> ShuffleAsync(SocketGuildUser user, IGuild guild, IVoiceState voiceState, ITextChannel textChannel)
        {
            //Check If User Is Connected To Voice Cahnnel.
            if (user.VoiceChannel == null)
            {
                return await EmbedHandler.CreateErrorEmbed("Music, Join/Play", "You Must First Join a Voice Channel.");
            }



            //Check the guild has a player available.
            if (!_lavaNode.HasPlayer(guild))
            {
                if (_lavaNode.HasPlayer(guild))
                {
                    return await EmbedHandler.CreateErrorEmbed("Music, Join", "I'm already connected to a voice channel!");
                }

                if (voiceState.VoiceChannel is null)
                {
                    return await EmbedHandler.CreateErrorEmbed("Music, Join", "You must be connected to a voice channel!");
                }

                try
                {
                    await _lavaNode.JoinAsync(voiceState.VoiceChannel, textChannel);
                }
                catch (Exception ex)
                {
                    return await EmbedHandler.CreateErrorEmbed("Music, Join", ex.Message);
                }
            }

            try
            {
                //Get the player for that guild.
                var player = _lavaNode.GetPlayer(guild);

                if (player.Queue.Count < 2)
                {
                    return await EmbedHandler.CreateBasicEmbed("Music", $"Not enough songs to shuffle!", Color.Blue);
                }
                else
                {
                    player.Queue.Shuffle();
                    return await EmbedHandler.CreateBasicEmbed("Music", $"Shuffled!!", Color.Blue);
                }
            }

            //If after all the checks we did, something still goes wrong. Tell the user about it so they can report it back to us.
            catch (Exception ex)
            {
                return await EmbedHandler.CreateErrorEmbed("Music, Play", ex.Message);
            }

        }

        public async Task<Embed> PlayNextAsync(SocketGuildUser user, IGuild guild, IVoiceState voiceState, ITextChannel textChannel, string query)
        {
            //Check If User Is Connected To Voice Cahnnel.
            if (user.VoiceChannel == null)
            {
                return await EmbedHandler.CreateErrorEmbed("Music, Join/Play", "You Must First Join a Voice Channel.");
            }



            //Check the guild has a player available.
            if (!_lavaNode.HasPlayer(guild))
            {
                if (_lavaNode.HasPlayer(guild))
                {
                    return await EmbedHandler.CreateErrorEmbed("Music, Join", "I'm already connected to a voice channel!");
                }

                if (voiceState.VoiceChannel is null)
                {
                    return await EmbedHandler.CreateErrorEmbed("Music, Join", "You must be connected to a voice channel!");
                }

                try
                {
                    await _lavaNode.JoinAsync(voiceState.VoiceChannel, textChannel);
                }
                catch (Exception ex)
                {
                    return await EmbedHandler.CreateErrorEmbed("Music, Join", ex.Message);
                }
            }

            try
            {
                //Get the player for that guild.
                var player = _lavaNode.GetPlayer(guild);

                ////Find The Youtube Track the User requested.
                //LavaTrack track;

                var search = Uri.IsWellFormedUriString(query, UriKind.Absolute) ?
                    await _lavaNode.SearchAsync(query)
                    : await _lavaNode.SearchYouTubeAsync(query);

                //If we couldn't find anything, tell the user.
                if (search.LoadStatus == LoadStatus.NoMatches)
                {
                    return await EmbedHandler.CreateErrorEmbed("Music", $"I wasn't able to find anything for {query}.");
                }

                //Get the first track from the search results.
                //TODO: Add a 1-5 list for the user to pick from. (Like Fredboat)
                track = search.Tracks.FirstOrDefault();

                //If the Bot is already playing music, or if it is paused but still has music in the playlist, Add the requested track to the queue.
                if (player.Track != null && player.PlayerState is PlayerState.Playing || player.PlayerState is PlayerState.Paused)
                {
                    List<Victoria.Interfaces.IQueueable> oldQueue = player.Queue.ToList();
                    player.Queue.Clear();
                    player.Queue.Enqueue(track);
                    foreach (var oldTrack in oldQueue)
                    {
                        player.Queue.Enqueue(oldTrack);
                    }
                    await LoggingService.LogInformationAsync("Music", $"{track.Title} has been added to the top of the queue!");
                    return await EmbedHandler.CreateBasicEmbed("Music", $"{track.Title} has been added to the top of the queue!", Color.Red);
                }

                //Player was not playing anything, so lets play the requested track.
                await player.PlayAsync(track);
                await LoggingService.LogInformationAsync("Music", $"Bot Now Playing: {track.Title}\nUrl: {track.Url}");
                return await EmbedHandler.CreateBasicEmbed("Music", $"Now Playing: [{track.Title}]({track.Url})\n{track.Position.Minutes:d2}:{track.Position.Seconds:d2}/{track.Duration.Minutes:d2}:{track.Duration.Seconds:d2}", Color.Blue);
            }

            //If after all the checks we did, something still goes wrong. Tell the user about it so they can report it back to us.
            catch (Exception ex)
            {
                return await EmbedHandler.CreateErrorEmbed("Music, Play", ex.Message);
            }

        }


        public async Task<Embed> NpAsync(SocketGuildUser user, IGuild guild, IVoiceState voiceState, ITextChannel textChannel)
        {
            string progressBar = ProgressBar();
            string duration = DurationFormatter();
            return await EmbedHandler.CreateBasicEmbed("Music", $"Now Playing: [{track.Title}]({track.Url})\n {progressBar} {duration}", Color.Blue);
        }

        /*This is ran when a user uses the command Leave.
            Task Returns an Embed which is used in the command call. */
        public async Task<Embed> LeaveAsync(IGuild guild)
        {
            try
            {
                //Get The Player Via GuildID.
                var player = _lavaNode.GetPlayer(guild);

                //if The Player is playing, Stop it.
                if (player.PlayerState is PlayerState.Playing)
                {
                    await player.StopAsync();
                }

                //Leave the voice channel.
                await _lavaNode.LeaveAsync(player.VoiceChannel);

                await LoggingService.LogInformationAsync("Music", $"Bot has left.");
                return await EmbedHandler.CreateBasicEmbed("Music", $"See ya!", Color.Blue);
            }
            //Tell the user about the error so they can report it back to us.
            catch (InvalidOperationException ex)
            {
                return await EmbedHandler.CreateErrorEmbed("Music, Leave", ex.Message);
            }
        }

        /*This is ran when a user uses the command List 
            Task Returns an Embed which is used in the command call. */
        public async Task<Embed> ListAsync(IGuild guild)
        {
            try
            {
                /* Create a string builder we can use to format how we want our list to be displayed. */
                var descriptionBuilder = new StringBuilder();

                /* Get The Player and make sure it isn't null. */
                var player = _lavaNode.GetPlayer(guild);
                if (player == null)
                    return await EmbedHandler.CreateErrorEmbed("Music, List", $"Could not aquire player.\nAre you using the bot right now? check{GlobalData.Config.DefaultPrefix}Help for info on how to use the bot.");

                if (player.PlayerState is PlayerState.Playing)
                {
                    /*If the queue count is less than 1 and the current track IS NOT null then we wont have a list to reply with.
                        In this situation we simply return an embed that displays the current track instead. */
                    if (player.Queue.Count < 1 && player.Track != null)
                    {
                        return await EmbedHandler.CreateBasicEmbed($"Now Playing: {player.Track.Title}", "Nothing else is queued.", Color.Blue);
                    }
                    else
                    {
                        /* Now we know if we have something in the queue worth replying with, so we itterate through all the Tracks in the queue.
                         *  Next Add the Track title and the url however make use of Discords Markdown feature to display everything neatly.
                            This trackNum variable is used to display the number in which the song is in place. (Start at 2 because we're including the current song.*/
                        var trackNum = 1;
                        foreach (LavaTrack track in player.Queue)
                        {
                            descriptionBuilder.Append($"{trackNum}) [{track.Title}]({track.Url})\n");
                            trackNum++;
                        }
                        return await EmbedHandler.CreateBasicEmbed("Music Playlist", $"Now Playing: [{player.Track.Title}]({player.Track.Url}) \n \n{descriptionBuilder}", Color.Blue);
                    }
                }
                else
                {
                    return await EmbedHandler.CreateErrorEmbed("Music, List", "Player doesn't seem to be playing anything right now.");
                }
            }
            catch (Exception ex)
            {
                return await EmbedHandler.CreateErrorEmbed("Music, List", ex.Message);
            }

        }

        /*This is ran when a user uses the command Skip 
            Task Returns an Embed which is used in the command call. */
        public async Task<Embed> SkipTrackAsync(IGuild guild)
        {
            try
            {
                var player = _lavaNode.GetPlayer(guild);
                /* Check if the player exists */
                if (player == null)
                    return await EmbedHandler.CreateErrorEmbed("Music, List", $"Could not aquire player.\nAre you using the bot right now? check{GlobalData.Config.DefaultPrefix}Help for info on how to use the bot.");
                /* Check The queue, if it is less than one (meaning we only have the current song available to skip) it wont allow the user to skip.
                     User is expected to use the Stop command if they're only wanting to skip the current song. */
                if (player.Queue.Count < 1)
                {
                    return await EmbedHandler.CreateErrorEmbed("Music, SkipTrack", $"Unable To skip a track as there is only one or no songs currently playing." +
                        $"\n\nDid you mean {GlobalData.Config.DefaultPrefix}Stop?");
                }
                else
                {
                    try
                    {
                        /* Save the current song for use after we skip it. */
                        var currentTrack = player.Track;
                        /* Skip the current song. */
                        await player.SkipAsync();
                        await LoggingService.LogInformationAsync("Music", $"Bot skipped: {currentTrack.Title}");
                        return await EmbedHandler.CreateBasicEmbed("Music Skip", $"I have successfully skiped {currentTrack.Title}", Color.Blue);
                    }
                    catch (Exception ex)
                    {
                        return await EmbedHandler.CreateErrorEmbed("Music, Skip", ex.Message);
                    }

                }
            }
            catch (Exception ex)
            {
                return await EmbedHandler.CreateErrorEmbed("Music, Skip", ex.Message);
            }
        }



        /*This is ran when a user uses the command Stop 
            Task Returns an Embed which is used in the command call. */
        public async Task<Embed> StopAsync(IGuild guild)
        {
            try
            {
                var player = _lavaNode.GetPlayer(guild);

                if (player == null)
                    return await EmbedHandler.CreateErrorEmbed("Music, List", $"Could not aquire player.\nAre you using the bot right now? check{GlobalData.Config.DefaultPrefix}Help for info on how to use the bot.");

                /* Check if the player exists, if it does, check if it is playing.
                     If it is playing, we can stop.*/
                if (player.PlayerState is PlayerState.Playing)
                {
                    await player.StopAsync();
                }

                await LoggingService.LogInformationAsync("Music", $"Bot has stopped playback.");
                return await EmbedHandler.CreateBasicEmbed("Music Stop", "I have stopped playback, and the playlist has been cleared.", Color.Blue);
            }
            catch (Exception ex)
            {
                return await EmbedHandler.CreateErrorEmbed("Music, Stop", ex.Message);
            }
        }

        /*This is ran when a user uses the command Volume 
            Task Returns a String which is used in the command call. */
        public async Task<string> SetVolumeAsync(IGuild guild, int volume)
        {
            if (volume > 150 || volume <= 0)
            {
                return $"Volume must be between 1 and 150.";
            }
            try
            {
                var player = _lavaNode.GetPlayer(guild);
                await player.UpdateVolumeAsync((ushort)volume);
                await LoggingService.LogInformationAsync("Music", $"Bot Volume set to: {volume}");
                return $"Volume has been set to {volume}.";
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message;
            }
        }

        public async Task<string> PauseAsync(IGuild guild)
        {
            try
            {
                var player = _lavaNode.GetPlayer(guild);
                if (!(player.PlayerState is PlayerState.Playing))
                {
                    await player.PauseAsync();
                    return $"There is nothing to pause.";
                }

                await player.PauseAsync();
                return $"**Paused:** {player.Track.Title}";
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message;
            }
        }

        public async Task<string> ResumeAsync(IGuild guild)
        {
            try
            {
                var player = _lavaNode.GetPlayer(guild);

                if (player.PlayerState is PlayerState.Paused)
                { 
                    await player.ResumeAsync(); 
                }

                return $"**Resumed:** {player.Track.Title}";
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message;
            }
        }

        public async Task TrackEnded(TrackEndedEventArgs args)
        {
            if (!args.Reason.ShouldPlayNext())
            {
                return;
            }

            if (!args.Player.Queue.TryDequeue(out var queueable))
            {
                //await args.Player.TextChannel.SendMessageAsync("Playback Finished.");
                return;
            }

            if (!(queueable is LavaTrack track))
            {
                await args.Player.TextChannel.SendMessageAsync("Next item in queue is not a track.");
                return;
            }

            await args.Player.PlayAsync(track);
            await args.Player.TextChannel.SendMessageAsync(
                embed: await EmbedHandler.CreateBasicEmbed("Now Playing", $"[{track.Title}]({track.Url})", Color.Blue));
        }
    }
}
