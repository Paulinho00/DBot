# DBot - discord bot with basic audio functionality
Simple discord bot created with Discord.Net, Lavalink, Victoria with simple commands and audio functionality.
## Table of contents
* [Introduction](#Introduction)
* [Technologies](#Technologies)
* [Features](#features)
* [Launch](#Launch)
## Introduction
Discord bot with functionality to play tracks from youtube (with direct links or search queries) or from local files (works like binds) with help of Lavalink server. Bot sends request with string query or direct link to Lavalink server and receives ready track to be played. It has also implemented commands in form of simple messages with prefix. I created it because I need my own audio bot in my server and I wanted to train working with documentation of unknown library/framework  and consolidate skills for example in dependecy injection.
## Technologies
Project uses:
* .NET 6.0
* Discord.Net 3.7.2
* Victoria 5.2.8
* Java 17
* Lavalink 

## Features
* Basic message commands like echo, userinfo or pingpong (pings 10 times) and extendable service for more commands
* Audio player for tracks found in internet
* Searching for tracks on yt using string query
* Playing audio from local files located in Resources folder

TODO:
* help command to display all commands
* show all possible playable local files
* command for delayed start of playing tracks


## Launch
First of all, you need to add bot to your discord server.\
Then start your Lavalink server.
Go to Lavalink folder in console and run server by typing following command:
```
java -jar .\475506-Lavalink.jar
```
Then you have to create your own `config.txt` file with your bot's token. It can be generated at Discord.com -> Developers Portal -> Applications(choose your bot) -> Bot.
Finally you run DBot, with Visual Studio or running it from command line using `dotnet run`.\
BEWARE! To run project from Command line you HAVE TO change path to config.txt in  `MainAsync` method in `Startup.cs` in calling `LoginAsync` method.

If you want to play your local files, you need to put `.mp3` file in `Resources` folder.

