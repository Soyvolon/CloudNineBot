<!-- docs/warns.md -->
# Warning System
The warning module allows you to keep track of member infractions and get notified when they hit a certain number of warnings.

!> The following list of commands have no prefix listed. Make sure to place your servers prefix before them in order to use a command! The default prefix is `!`

# Information
This module is intended to help moderation by keeping track of which users have been warned about infractions on a server. It comes with a variety of features, including editing warnings, setting notices, and optional messaging of the user who was warned. No additional setup is required, this module works out of the box.

# Commands
## Add Mod Log Notice
**Adds a note that will sent to a moderator when a member's warnings hit a certain limit**

Requires `MANAGE_MESSAGES`

```
addmodlognotice <count> <message>
```

| Parameter | Usage |
|-----------|-------|
| count     | Number of warnings needed for this message to trigger |
| message   | Message to send to the moderator |

## Add Warning
**Adds a warning for a member or views an existing warning**

Requires `MANAGE_MESSAGES`

```
warn <member> <message>
```

| Parameter | Usage |
|-----------|-------|
| member    | Member to record a warning for. Either their mention or user ID |
| message   | Message of the warn. Use `--notify` to send this to the member who was warned |

<hr />

```
warn <warn ID>
```

| Parameter | Usage |
|-----------|-------|
| warn ID   |  ID of the warning to display |

## Clear Warn Status
**Clears any forgiven or not forgiven status on a warn**

```
clearwarnstatus <warn ID>
```

| Parameter | Usage |
|-----------|-------|
| warn ID   |  ID of the warning to clear status for |

## Delete Warning
**Deletes an existing warning**

Requires `MANAGE_GUILD`

```
deletewarn <warn ID>
```

| Parameter | Usage |
|-----------|-------|
| warn ID   |  ID of the warning to delete |

## Edit Warning
**Edits the message of a warning**

Requires `MANAGE_MESSAGES`

```
editwarn <warn ID> <message>
```

| Parameter | Usage |
|-----------|-------|
| warn ID   |  ID of the warning to edit |
| message   | The new message for the warning |

## Forgive Warning
**Forgives a warning, or, marks a warning as not forgiven**

Requires `MANAGE_MESSAGES`

```
forgivewarn <warn ID> <not Forgiven>
```

| Parameter    | Usage |
|--------------|-------|
| warn ID      |  ID of the warning to forgive |
| not Forgiven | A boolean value that defaults to `false` and determines is a warn should be marked as not Forgiven instead of Forgiven. This is optional. |

## Redo Warning Edit
**Redo an edit to a warning**

Requires `MANAGE_MESSAGES`

```
redo <warn ID>
```

| Parameter | Usage |
|-----------|-------|
| warn ID   |  ID of the warning to redo an edit for |

## Remove Mod Log Notice
**Remove an existing mod log notice**

Requires `MANAGE_MESSAGES`

```
removemodlognotice <count>
```

| Parameter | Usage |
|-----------|-------|
| count     | Number of warnings needed for the notice you want to remove to be triggered |

## Set Review Delay
**Sets a length of time before a warning is eligible for review**

Requires `MANAGE_GUILD`

```
reviewtime <days>
```

| Parameter | Usage |
|-----------|-------|
| days      | Number of days a warn must exist before it is eligible for review |

## Undo Warning Edit
**Undo an edit to a warning**

Requires `MANAGE_MESSAGES`

```
undo <warn ID>
```

| Parameter | Usage |
|-----------|-------|
| warn ID   |  ID of the warning to undo an edit for |

## View Mod Logs
**Views all warnings for a member**

Requires `MANAGE_MESSAGES`

```
modlogs <member>
```

| Parameter | Usage |
|-----------|-------|
| member    | Member to view all warnings for. Either their mention or user ID |

## View Warnings to Review
**Views all warnings eligible for forgiveness**

Requires `MANAGE_MESSAGES`

```
warnstoforgive
```