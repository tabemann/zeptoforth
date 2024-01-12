# zeptoed

zeptoed is an optional text editor that may be loaded into RAM or compiled to flash for the purpose of editing files in FAT32 filesystems. It supports editing in multiple buffers simultaneously, copy/cut-and-paste operations, undo operations, LF/CRLF conversion, Unicode (UTF-8), and indentation and unindentation operations. It requires a fully functional ANSI terminal, and hence cannot be used under zeptocom.js.

Each buffer takes up the middle portion of the screen, i.e. excluding the top and bottom lines of the screen. The top line is taken up by the path of the file corresponding to the current buffer and, if the current buffer is dirty (i.e. a modification has been made to it since has time it has been written out or reverted), an asterisk indicating the fact. The bottom line is taken up by the "minibuffer", which displays messages and takes prompted user input.

Note that zeptoed requires a FAT32 filesystem to be registered via `fat32-tools::current-fs!` or `fat32-tools::init-simple-fat32`. In most cases the latter will be the easiest approach (e.g. one is using SDHC/SDXC cards containing only one partition and one is using only one FAT32 filesystem).

zeptoed lives in `extra/common/zeptoed.fs` and is dependent upon `extra/common/dyn_buffer.fs`. Both can be loaded at once by loading `extra/common/zeptoed_all.fs` with `utils/codeload3.sh`, zeptocom.js, or e4thcom.

## Words

##### `zeptoed`
( path-addr path-bytes -- )

Open the file at the path specified by *path-addr* *path-bytes* in the current FAT32 filesystem into a new buffer in zeptoed.

##### `zed`
( path-addr path-bytes -- )

The same as `zeptoed` in case you feel like `zeptoed` is too long to enter.

##### `zeptoed-heap-size`
( -- bytes )

This a `value` containing the base heap size used by zeptoed while it is running. It defaults to 65536 bytes. Note that zeptoed actually uses more space for data than this, because this does not include the heap bitmap or arena-allocated structures used by zeptoed.

##### `zeptoed-indent-size`
( -- spaces )

This is a `value` containing the number of spaces taken up by a single indentation. It defaults to 2 spaces.

##### `zeptoed-tab-size`
( -- spaces )

This is a `value` containing the number of spaces that a tab character is aligned to. It defaults to 8 spaces.

##### `zeptoed-save-crlf-enabled`
( -- enabled? )

This is a `value` containing whether files saved by zeptoed will use CRLF endlines (rather than the default of LF endlines). It defaults to `false`.

## Key mappings

zeptoed has the following key mappings; all other non-control characters will be inserted at the cursor:

* Enter: Insert a newline with indentation to match the indentation of the preceding line.
* Tab: Indent the current line or the current selection by one indentation incremnt.
* Shift-Tab: Unindent the current lien or the current selection by one indentation increment. Note that tabs may be automatically converted to spaces in the process.
* Control-Meta-Tab: Insert a single tab character (rather than indent).
* Backspace: If there is no selection, delete backward (left) one character; if there is a selection, delete the current selection.
* Delete: If there is no selection, delete forward (right) one character; if there is a selection, delete the current selection.
* Left, Control-B: Move the cursor backward (left) one character.
* Right, Control-F: Move the cursor forward (right) one character.
* Up: Move the cursor up by one row.
* Down: Move the cursor down by one row.
* Page Up: Move the cursor up by roughly one screen.
* Page Down: Move the cursor down by roughly one screen.
* Control-Space: Toggle selection; if there previously was no selection, the selection point is set to the current position of the cursor.
* Control-A: Move the cursor to the start of the line; note that under many terminal programs (GNU Screen, picocom) Control-A is captured, and generating a Control-A requires more keys to be entered (e.g. entering Control-A again under picocom).
* Control-E: Move the cursor to the end of the line.
* Control-N: Go to the next buffer.
* Control-P: Go to the previous buffer.
* Control-O: Open a buffer with the specified file path; if a file with that path exists it is loaded, otherwise it is created.
* Control-V: Exit; note that if dirty buffers exist, the user will be prompted whether they wish to exit.
* Control-W: Write the current buffer to its file and mark it clean.
* Control-Meta-W: Select another file and write the current buffer into it, creating it if it did not exist and overwriting its contents if it did, and set the current buffer to point to it; the current buffer will be marked as clean.
* Control-X: Reload the current buffer from its file and mark it clean.
* Control-K: Cut the current selection and transfer it to the clipboard.
* Control-Meta-K: Copy the current selection and transfer it to the clipboard.
* Control-Y: Paste the contents of the clipboard at the cursor.
* Control-Z: Carry out one undo; note that some operations have their undos combined, while others may be split into multiple undos (i.e. are non-atomic).
