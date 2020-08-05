# NetworkBot

Private discord bot that listens to DHCP requests (port 67) and alerts about new connections in a discord channel.

## Usage

Needs a file called config.json that provides a valid discord bot token and the channel id the bot should use. The file should look like this

```
{
  "token": "yourToken",
  "channelId": "yourId"
}
```

You can also provide a file called ignore.json, which is just a string array used to ingore specific names from dhcp requests

```
["Name1", "Name2"]
```
