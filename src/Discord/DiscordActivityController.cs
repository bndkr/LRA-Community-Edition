using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

//TODO: Finish a better Game SDK implementation  

namespace linerider
{
  class DiscordActivityController
  {
    public Discord.Discord discord;

    public void initDiscord()
    {
      discord = new Discord.Discord(506953593945980933, (UInt64)Discord.CreateFlags.NoRequireDiscord); //Create discord for game sdk activity
    }

  }
}
