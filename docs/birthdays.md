<!-- docs/birthdays.md -->
# Birthday Module
The birthday module allows you to set a birthday channel and automatically wish people a happy birthday (and remind everyone else about the birthday too).

!> The following list of commands have no prefix listed. Make sure to place your servers prefix before them in order to use a command! The default prefix is `!`

# Information
The intent behind this module is to setup and auto locking channel that removes and adds people based on when their birthday is. Setting a birthday channel will allow members to take use of the `register` command, setting their birthdays and participating in the channel.

Once members have registered, the channel will lock them out if their birthday is on one of the next two dates with birthdays. Take the following for an example:

**Birthdays Registered**

| User | Birthday |
|------|----------|
| Alex | 04/24    |
| Jess | 04/27    |
| Sam  | 04/27    |
| Meg  | 05/02    |

On 04/23, three people will be locked out of the channel: Alex, Jess, and Sam. When the 24th comes around, Alex is let back in, but Meg is locked out as well as Jess and Sam.

Using this functionality you now have a channel that hides itself from members before their birthday where you can then plan gifts and other sunrises!

# Commands
## Admin Register
**Register another member with a birthday**

Requires `MANAGE_GUILD`

```
aregister <member> <birthday>
```

| Parameter | Usage |
|-----------|-------|
| member    | A member of the server you are on |
| birthday  | A date in the format of MM/DD, Month Day, or Day Month |

## Admin Remove
**Deregister another member with a birthday**

Requires `MANAGE_GUILD`

```
aremove <member>
```

| Parameter | Usage |
|-----------|-------|
| member    | A member of the server you are on that has a registered birthday |

## Force Update
**Forces the birthday channel description to update**

Requires `MANAGE_GUILD`

```
forceupdate
```

## Get Birthday List
**Gets the list of all birthdays in the server**

```
bdaylist <args>
```

| Parameter | Usage |
|-----------|-------|
| args      | Optional. Use `s` to sort results by relevance. |

## Register Birthday
**Register your birthday with the server**

```
register <birthday>
```

| Parameter | Usage |
|-----------|-------|
| birthday  | A date in the format of MM/DD, Month Day, or Day Month |

## Remove Birthday
**Remove your birthday from the server**

```
remove
```

## Set Birthday Channel
**Sets a channel to be used as the birthday channel**

Requires `MANAGE_GUILD`

```
bdaychannel <channel>
```

| Parameter | Usage |
|-----------|-------|
| channel   | Sets a channel to be used as the Birthday channel |
