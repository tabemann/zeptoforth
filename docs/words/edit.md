# Block Editor

The block editor provides a simple interface for allowing the user to edit blocks stored in the Quad SPI flash on supported devices, currently the STM32F746 DISCOVERY board. When in use, the following keys can be used to control editing:

* Backspace: delete the character to the left of the cursor
* Delete: delete the character under the cursor
* Tab: insert one or two spaces, depending on whether the cursor is on an odd or an even column (starting from column 0)
* Up: move the cursor one line up
* Down: move the cursor one line down
* Newline: move the cursor to the start of the next line, or if the cursor was at the last line, change to the next block and move the cursor to the start of the first line
* Left / Control-B: move the cursor one character to the left
* Right / ControlF: move the cursor one character to the right
* Control-A: move the cursor to the start of the input
* Control-E: move the cursor to the end of the input
* Control-N: change to the next block
* Control-P: change to the previous block
* Control-U: insert a line above the current line
* Control-K: if the current line is empty, delete it, else delete the current line's contents
* Control-W: save the current block
* Control-X: revert the current block
* Control-V: exit the editor, saving all modified blocks

Note that tabs (which are treated as being of two spaces wide due to the limited space available in blocks) and characters of code points greater than 127 are handled correctly. Also note that only nine blocks (currently) are stored in memory, and if a modified block is removed from memory it is automatically saved.

### `forth-module`

The following words are in `forth-module`:

##### `edit`
( id -- )

Open the block editor for the given block; note that adjacent blocks are also automatically loaded into memory
