# Line Editor

The line editor provides a simple interface for allowing the user to edit input without the limitations in the normal `refill` mechanism. When enabled, the following keys can be used to control editing:

* Backspace: delete the character to the left of the cursor
* Delete: delete the character under the cursor
* Left / Control-B: move the cursor one character to the left
* Right / Control-F: move the cursor one character to the right
* Up: switch to the previous line in the history
* Down: switch to the next line in the history
* Control-A: move the cursor to the start of the input
* Control-E: move the cursor to the end of the input
* F1: enter upload mode, where line editing features are turned off
* F2: leave upload mode and re-enable line editing features; this must be entered as the first key entered on a line

Note that tabs and characters of code points greater than 127 are handled correctly. Also note that it is not compatible with non-terminal operation except when in upload mode, i.e. with loading code automatically via serial or swdcom; before this is done one must be in upload mode or it must be disabled.

After a reboot, it is initialized for the main task only, and if, when enabled, `refill` is used for a different task, it needs to have already been initialized for that task.

### `line-module`

The following words are in `line-module`:

##### `init-line`
( index-ptr count-ptr buffer-ptr buffer-size -- )

Enable the line editor for the current task, with a pointer to the index variable, a poiner to the count variable, a pointer to the input buffer, and a size of the input buffer provided. Note that it allots memory in the current task's dictionary.

### `forth-module`

The following words are in `forth-module`:

##### `enable-line`
( -- )

Enable the line editor.

##### `disable-line`
( -- )

Disable the line editor. This must be done before loading code automatically.
