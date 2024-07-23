# DBot - discord bot with basic audio functionality
Simple discord bot created with DisCatSharp, Lavalink, with simple commands and audio functionality.
## Table of contents
* [Introduction](#Introduction)
* [Technologies](#Technologies)
* [Features](#Features)
* [Launch](#Launch)
## Introduction
Discord bot with functionality to play tracks from youtube (with direct links or search queries) or from local files (works like binds) with help of Lavalink server. Bot sends request with string query or direct link to Lavalink server and receives ready track to be played. It has also implemented commands in form of simple messages with prefix. I created it because I need my own audio bot in my server and I wanted to train working with documentation of unknown library/framework  and consolidate skills for example in dependecy injection.
## Technologies
Project uses:
* .NET 8.0
* DisCatSharp 10.6.4
* Lavalink 4.0.6

## Features
* Basic message commands like pingpong (pings 10 times) and extendable service for more commands
* Audio player for tracks found in internet
* Searching for tracks on yt using string query
* Playing audio from local files located in Resources folder
* Displays all available local sounds grouped into categories

TODO:
* Add support for other sources of sounds like Spotify, Soundcloud etc.
* Rewrite Command modules to use Slash Commands
* Use AI for new features
* basic audio commands in the form of embed and buttons


## Launch
First, you need to create account for your bot. Example tutorial is [here](https://docs.discord.red/en/stable/bot_application_guide.html). Save the token as it will be useful in next steps.
Then download a recent release of DBot and run the Lavalink client.
Go to Lavalink folder in console and run server by typing following command:
```
java -jar .\Lavalink.jar
```
The application.yml contains a full configuration. Changing this configuration is not recommended.

Then you have to create your own `config.txt` file with your bot's token. Use token generated during the creation of bot account or generate it again. It can be generated at Discord.com -> Developers Portal -> Applications(choose your bot) -> Bot.
You have to place `config.txt` file in the same directory as your bot's `.exe` file.
Finally you can run DBot.

**IMPORTANT!** If you want to play your local files, you need to put `.mp3` file in `Resources` folder. You have to move `Resources` folder to destination of your  `.exe` file. 

 If you want to have more clear view of your all sounds, you can put your sounds in folders inside `Resources` folder, so to be displayed in seperate fields in embed message when typing `sounds` command.

