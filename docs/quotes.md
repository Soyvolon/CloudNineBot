<!-- docs/quotes.md -->
# Quote Module
The quote module is a fun tool to save and display iconic moments from your server!

!> The following list of commands have no prefix listed. Make sure to place your servers prefix before them in order to use a command! The default prefix is `!`

# Information

The intent behind this module is to save and store moments for your server that you can later look back to! No special setup is required, this module works right out of the box.

# Commands
## Add Quote
**Adds a new quote.**

Requires `MANAGE_MESSAGES`

```
addquote <author> <content>
```

| Parameter | Usage |
|-----------|-------|
| author    | The author of the quote. If left out a quote will still be created. |
| content   | The content of a quote. Required. |

<hr />

```
addquote <relay parameters>
```

| Parameter | Usage |
|-----------|-------|
| relay parameters | Parameters from the [quote relay](/quoterelay#parameters). Ignores the `--channel` parameter. |

## Delete Quote
**Deletes a quote.**

Requires `MANAGE_MESSAGES`

```
deletequote <quote ID>
```

| Parameter | Usage |
|-----------|-------|
| quote ID | The ID of the quote to delete |

## Edit Quote
**Edits an existing quote**

Requires `MANAGE_MESSAGES`

```
editquote <quote ID> <relay parameters>
```

| Parameter | Usage |
|-----------|-------|
| quote ID | The ID of the quote to edit |
| relay parameters | Parameters from the [quote relay](/quoterelay#parameters). Ignores the `--channel` parameter. |

## Favorite Quote
**Adds a quote to your personal favorites**
```
favoritequote <quote ID>
```

| Parameter | Usage |
|-----------|-------|
| quote ID | The ID of the quote to edit |

## Hide Quote
**Creates a new hidden quote**

Requires `MANAGE_MESSAGES`

```
hidequote
```
*This command is interactive and has no additional parameters*

## List Favorite Quotes
**Lists a members favorites quotes**

```
favoritequotes <member>
```

| Parameter | Usage |
|-----------|-------|
| member    | A member of the current server to list the favorites of. If left blank, lists the favorites of the member who ran the command |

## List Hidden Quotes
**Lists the hidden quotes for the server**

Requires `MANAGE_MESSAGES`

```
listhiddenquotes
```

## Quote
**Displays a quote.**
```
quote
```

*Displays a random quote from the server.*
<hr />

```
quote <quote ID>
```

| Parameter | Usage |
|-----------|-------|
| quote ID  | The ID of the quote to display |

## Quotes List
**Displays a list of all the quotes on the server**
```
listquotes
```

## Remove Favorite Quote
**Removes a quote from your favorites list**
```
unfavoritequote <quote ID>
```

| Parameter | Usage |
|-----------|-------|
| quote ID  | The ID of the quote to remove from your favorites |

## Search Quotes
**Search for quotes matching a query**
```
serachquotes <query>
```

| Parameter | Usage |
|-----------|-------|
| query     | Search Query. See the following section for more details. |

### Search Query
> All search string use the MSDN Like operator for comparisons. Read more about it [here](https://docs.microsoft.com/en-us/office/vba/language/reference/user-interface-help/like-operator#remarks)

| Characters in *pattern* |	Matches in *string* |
|-------------------------|---------------------|
| ? 	                  | Any single character. |
| * 	                  | Zero or more characters. |
| # 	                  | Any single digit (0-9). |
| [charlist]              | Any single character in charlist. |
| [!charlist]             | Any single character not in charlist. |

*Table From [MSDN](https://docs.microsoft.com/en-us/office/vba/language/reference/user-interface-help/like-operator#remarks)*

The following rules are used when searing strings. Searching is case sensitive.

!> Any search pattern that is multiple words **must** be surrounded by quotes `"` or else you will get unexpected results.

The following parameters are used to search three different parts of a quote:

**Search by quote author:**

`-a`, `--author`
```
-a <pattern>
--author <pattern>
```

**Search by who Saved the quote:**

`-s`, `--saved`
```
-s <pattern>
--saved <pattern>
```

**Search in quote content:**

`-c`, `--content`
```
-c <pattern>
--content <pattern>
```

| Parameter | Usage |
|-----------|-------|
| pattern   | The search pattern to use. Surround multi word patterns in quotes `"` |