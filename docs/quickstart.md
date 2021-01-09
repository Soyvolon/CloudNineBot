<!-- docs/quickstart.md -->
# Invite Cloud Nine Bot
Step one, and arguably the most important step of all, invite the bot to your server!

You can find the invite link [here](https://discord.com/api/oauth2/authorize?client_id=750486299789754389&permissions=388176&redirect_uri=https%3A%2F%2Fandrewbounds.com%2Flogin&scope=bot%20applications.commands), and make sure to give Cloud Nine Bot all the permissions it asks for otherwise some of the features will not work!

Once you get past the captcha, you will be asked to select which server you want to invite the bot to. Make sure you have `MANAGE_GUILD` permissions on the server then select it from the list!

Once Cloud Nine Bot is added, its time to get your server setup to use all the cool features!

# Server Setup
> Everything in this section is optional, but enrich your experience! Pick and chose how you want to use Cloud Nine Bot!

!> Commands listed here have no prefix in front of them. In order to run a command, make sure to place a prefix before the command name! This is true everywhere on the docs.  The default prefix is `!`

## Setting a custom prefix
> Change how you interact with Cloud Nine Bot!

Want a different prefix? No problem!
```
prefix <new prefix>
```
Thats how you set a new prefix. Easy, right? Just replace `<new prefix>` with your custom prefix and **boom** a brand new prefix for your server.

## Quotes
> Saving the funny and insane things that happen on your server!

Arguably the most used feature, the quotes function allows anyone on your server with the `MANAGE_MESSAGES` permission or higher to save any text as a quote:
```
addquote @Author This is a quote!
```
This saves a new quote, by the mentioned author (or use their ID, up to you), with the content: `This is a quote!`. Not very interesting at this point, **but**, you can now use a new command!
```
quote
```
Yup, thats it. This command gets a *random* quote from your server and displays it to the user. Granted, if you only have a single quote, thats all it will show, but your quote library will grow with time!

Speaking of growth, there is a whole bunch of other features with the quotes that you can check out [here](/quotes)

## Birthday Channel
> Creating the prefect place to plan for a persons birthday (without them seeing it, of course) and then letting them in on their special day!

First, you need to set a channel as the birthday channel! Only members with the `MANAGE_GUILD` permission can do this, so have an admin set it up:
```
bdaychannel #channel-name
```
And there you go, a birthday channel just for your server. Now that you have a channel, lets explain how this works:
1. Members register their birthdays with `register MM/DD`
2. The bot scans the list of birthdays once a day, and locks out the users who have birthdays on the next two closest days.
    - To explain on this a bit, if the day is April 10th, and there is one user who has a birthday on April 11th, and two with birthdays on April 14th, all three will be locked out, but anyone with a birthday later than April 14th wont be locked out until on or after April 11th.
3. The bot updates the channel description which the mentions of those who are locked out, so the other members know who's birthday is next.
4. On a users birthday, the bot pings then with a Happy Birthday message, which also lets all the users know who has a birthday on that day!
5. Repeat 2-5.

*If a member no longer wants their birthday registered, they can use `remove` to get rid of it.*

More details on the Birthday Module can be found [here](/birthdays)

## Fanfiction Multisearch
> Searching for fanfiction across multiple websites, all at once!

Looking to find more fanfics to read? Look no further than the Fanfiction Multisearch:
```
search Thing you want to search for
```
And there you go! A little wait later and you have fresh results from multiple fanfiction websites! Navigate your results with the left and right emojis, or stop navigating all together with the stop emoji. Want to see more information about the Fic you just searched? Thats a command too:
```
search details <fic number>
```
Each result has a number at the start of it. Place that number in instead of `<fic number>` and get the full details of a search item! Next up, finding the fanfic of your dreams!

There is a lot of server and personal customization along with a **ton** of search options. Check them all out [here](/multisearch)

## Warn System
> Keeping track of your members infractions since you added Cloud Nine Bot!

Got a user giving you trouble? Warned them about their actions by sending them a message but have no way to keep track if you warned them before, or if another mod did as well? Now you can!
```
warn <user ID> Warn message
```
Replace `<user ID>` with the ID of the member you are warning (see [this page](https://support.discord.com/hc/en-us/articles/206346498-Where-can-I-find-my-User-Server-Message-ID-) for info on how to get user IDs) and set your warn message! By default, this wont send a message to the user, but it *does* store it for the other moderators.

You must have the `MANAGE_MESSAGES` permission to add a warn and view them. Speaking of viewing them, there is *also* a command for that (not too useful without it methinks)!
```
modlogs <user ID>
```
Again, replace the `<user ID>` with the ID of the member who you want to see warns for, and Cloud Nine Bot will show you all the warnings you have logged for them (along with a handy dandy total count)!

There are a few more cool features to improve how you use the warnings system, so check them out [here](/warns)