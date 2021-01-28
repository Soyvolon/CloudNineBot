<!-- docs/multisearch.md -->
# Fanfiction Multisearch
The Fanfiction Multisearch module is designed to search multiple fanfiction websites at the same time! 

!> The following list of commands have no prefix listed. Make sure to place your servers prefix before them in order to use a command! The default prefix is `!`

# Information
The Fanfiction Multisearch module is an easy way to search Multiple Fanfiction websites and get your results in a single place! By running a single command, Cloud Nine Bot automatically searches Fanfiction.net and Wattpad for fanfiction matching your search.

!> Archive of Our Own disallows indexing of search results, and will not be supported.

Both server settings and user settings can be customized for what information is displayed when searching with the `search config` command. 

The Multisearch feature also will send the fanfiction information for any link posted in a channel Cloud Nine Bot can see, but this can be disabled both by users and by a server.

!> Fanfiction.net uses CloudFlare, which prevents gathering information from a link. Fanfiction.net links will not be supported.

# Commands
## Cache
**Display your cache**

```
search cache
```

## Cache Server
**Display the server cache**

```
search cache guild
```

## Cache Details
**Display the details about an item in the cache**

```
search cache details <item>
```

| Parameter  | Usage |
|------------|-------|
| item       | Any positive integer that is listed next to an item in the [cache](#cache) command. Defaults to the first item |

## Cache Server Details

```
search cache guilddetails <item>
```

| Parameter  | Usage |
|------------|-------|
| item       | Any positive integer that is listed next to an item in the [cache server](#cache-server) command. Defaults to the first item |

## Cache Server Clear
**Clears a single item or the entire cache of a server**

Requires `MANAGE_MESSAGES`

```
search cache guildclear <item>
```

| Parameter  | Usage |
|------------|-------|
| item       | Any positive integer that is listed next to an item in the [cache server](#cache-server) command. Defaults clearing the entire cache. |

## Cache User Clear
**Clears a single item or the entire cache of a user**

```
search cache userclear <item>
```

| Parameter  | Usage |
|------------|-------|
| item       | Any positive integer that is listed next to an item in the [cache](#cache) command. Defaults clearing the entire cache. |

## Search
**Search for a fanfic**

```
search <parameters>
```

| Parameter  | Usage |
|------------|-------|
| parameters | Search parameters. See the full list [here](#search-parameters) |

## Search Config
**Set search options, display options, and other misc settings related the the Multisearch Module**

The following commands are used to view more information about configurations and display the current configuration for either a user or a server.

```
search config
search config user
search config guild
```

### Search Config for Users
```
search config user <option> <variable>
```

| Parameter | Usage |
|-----------|-------|
| option    | Config option to change |
| variable  | Value to change the option to |

See [Configuration Options](#configuration-options) for a list of options and variables that are valid.

### Search Config for Servers
Requires `MANAGE_MESSAGES`

```
search config guild <option> <variable>
```

| Parameter | Usage |
|-----------|-------|
| option    | Config option to change |
| variable  | Value to change the option to |

See [Configuration Options](#configuration-options) for a list of options and variables that are valid.

## Search Details
**View details about a search result**

```
search details <result>
```

| Parameter | Usage |
|-----------|-------|
| result    | Number of the result you want to see details for. The bolded number to the left in the search display is the result number |

## Search Display
**Display the results from the last search**
```
search display
```

# Search Parameters
**Parameters to use with the [search](#search) command**

Some things to note:
- Lists are command separated `,`
- Parameters with multiple words or blank spaces in them need quotes `"` around them
- Parameters can be used multiple times, but for parameters that can only set a single value the last set value is used
- Text not in a parameter is used in the basic search

| Parameter                  | Usage                                             | Examples |
|----------------------------|---------------------------------------------------|----------|
| `--help`<br />`-h`         | Displays a help message. Does not search anything ||
| `--title`<br />`-t`        | Searches for a title. | `--title "This Title"`<br />`-t "That Title"` |
| `--author`<br />`-a`       | Filters by an author. | `--author "This Author"`<br />`-a "That Author"` |
| `--character`<br />`-c`    | Filters by characters. Can be used more than once. | `--character "A"`<br />`-c "A,B,C"` |
| `--relationship`<br />`-r` | Filters by relationships. Can be used more than once. | `--relationship "A/B"`<br />`-r "A & B, B/C"` |
| `--fandom`<br />`-f`       | Filters by fandoms. Can be used more than once. | `--fandom "Pokemon"`<br />`-f "Pokemon,My Hero Academia"` |
| `--other`<br />`-o`        | Filters by other tags. Can be used more than once. | `--other "Fluff"`<br />`-o "Fluff,Hugs"` |
| `--likes`<br />`-l`        | Filters by amount of likes. Uses a [dual-integer](#dual-integers) | `--likes 1000-10000`<br />`-l 1000-0` |
| `--views`<br />`-v`        | Filters by amount of views. Uses a [dual-integer](#dual-integers) | `--views 1000-10000`<br />`-v 1000-0` |
| `--comments`<br />`-C`     | Filters by amount of comments. Uses a [dual-integer](#dual-integers) | `--comments 1000-10000`<br />`-C 1000-0` |
| `--words`<br />`-w`        | Filters by amount of words. Uses a [dual-integer](#dual-integers) | `--words 1000-10000`<br />`-w 1000-0` |
| `--updated`<br />`-u`      | Filters by update date. Uses a [dual-time](#dual-times) | `--updated >1 month`<br />`-u "20-50 days"` |
| `--published`<br />`-p`    | Filters by publish date. Uses a [dual-time](#dual-times) | `--published >1 month`<br />`-p "20-50 days"` |
| `--direction`<br />`-D`    | Changes search by direction. Must be one of: `ascending` (`1`), `descending` (`0`) | `--direction ascending`<br />`-D 0` |
| `--searchby`<br />`-s`    | Changes search by direction. Must be one of: `bestmatch` (`0`), `likes` (`1`), `views` (`2`), `updateddate` (`3`) `publisheddate` (`4`), `comments` (`5`) | `--searchby bestmatch`<br />`-s 1` |
| `--rating`<br />`-R`    | Changes search by direction. Must be one of: `any` (`0`), `general` (`1`), `teen` (`2`), `mature` (`3`), `explicit` (`4`), `notexplicit` (`5`) | `--rating any`<br />`-R 5` |
| `--status`<br />`-S`    | Changes search by direction. Must be one of: `any` (`0`), `inprogress` (`1`), `complete` (`2`) | `--status any`<br />`-S 2` |
| `--crossover`<br />`-x`    | Changes search by direction. Must be one of: `any` (`0`), `nocrossover` (`1`), `crossover` (`2`) | `--crossover any`<br />`-x 1` |

## Dual Integers
**Two numbers indicating a range of values**

This is a pair of integers. A 0 in either slot means no value, not a value of 0. It works as a `min-max` value, where a non used value has no effect.

### Examples (for word count):

| Dual Integer | Result |
|--------------|--------|
| `100-200`    | 100 to 200 words only |
| `1000-0`     | Less than or equal to 1000 words only |
| `0-1000`     | Greater than or equal to 1000 words only |

## Dual Times
**One or two values indicating a range of times**

This is a pair of times or a less then/greater than a time value. A 0 means no value, not a value of 0. It works as a `min-max` value, where a non used value has no effect.

### Examples (for publish date):

| Dual Time  | Result |
|------------|--------|
| `>5 days`  | Updated more than 5 days ago |
| `0-5 days` | Updated more than 5 days ago |
| `<5 days`  | Updated less than 5 days ago |
| `5-0 days` | Updated less than 5 days ago |
| `1-5 days` | Updated between 1 and 5 days ago |

Furthermore, you can specify the amount of weight the numbers have:

| Dual Time   | Result |
|-------------|--------|
| `>5 days`   | The value 5 is in days |
| `>5 weeks`  | The value of 5 is in days times 7 (weeks) |
| `>5 months` | The value of 5 is in days times 30 (months) | 
| `>5 years`  | The value of 5 is in days times 365 (years) |
| `>5`        | The value of 5 is in days |

# Configuration Options
**Options that are used with the [`search config`](#search-config) command.**
## Global Options
**These options are available to both servers and users**

### `overflow`
**Description:** Controls if the embed will overflow to two or more embeds if needed

**Possible Values:** `true` (`1`), `false` (`0`)

**Default Value:** `true`

### `hidesensitive`
**Description:** Controls if the embed will hide descriptions of fics with sensitive content warnings

**Possible Values:** `true` (`1`), `false` (`0`)

**Default Value:** `true`

### `taglimit`
**Description:** Sets the maximum amount of tags the embed will display. 0 will be treated as no limit.

**Possible Values:** `0`, Any positive integer

**Default Value:** `0`

### `ctaglimit`
**Description:** Same as tag limit but for character tags.

**Possible Values:** `0`, Any positive integer

**Default Value:** `0`

### `rtaglimit`
**Description:** Same as tag limit but for relationship tags.

**Possible Values:** `0`, Any positive integer

**Default Value:** `0`

### `cache`
**Description:** Controls if fic links posted will be cached. Or, for searches, if detailed fics will be cached.

**Possible Values:** `true` (`1`), `false` (`0`)

**Default Value:** `true`

### `link`
**Description:** Controls if fic links will be detailed when posted in channels

**Possible Values:** `true` (`1`), `false` (`0`)

**Default Value:** `true`

### `explicit`
**Description:** Controls if search results will include explicit content

**Possible Values:** `true` (`1`), `false` (`0`)

**Default Value:** `false`

### `warnonnowarn`
**Description:** Controls if Cloud Nine Bot will treat no warnings as sensitive content.

**Possible Values:** `true` (`1`), `false` (`0`)

**Default Value:** `false`


## User Only Options
**These options are not available to servers. They set defaults but they can be changed by using the search arguments.**

### `direction`
**Description:** Controls the default search direction.

**Possible Values:** `ascending` (`1`), `descending` (`0`)

**Default Value:** `descending`

### `searchby`
**Description:** Controls the default method to search by.

**Possible Values:** `bestmatch` (`0`), `likes` (`1`), `views` (`2`), `updateddate` (`3`) `publisheddate` (`4`), `comments` (`5`)

**Default Value:** `bestmatch`

### `rating`
**Description:** Controls the default rating to search within.

**Possible Values:** `any` (`0`), `general` (`1`), `teen` (`2`), `mature` (`3`), `explicit` (`4`), `notexplicit` (`5`)

**Default Value:** `notexplicit`

### `complete`
**Description:** Controls the default completion status to search within.

**Possible Values:** `any` (`0`), `inprogress` (`1`), `complete` (`2`)

**Default Value:** `any`

### `crossover`
**Description:** Controls the default crossover status to search within.

**Possible Values:** `any` (`0`), `nocrossover` (`1`), `crossover` (`2`)

**Default Value:** `any`