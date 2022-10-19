# Internal Words

### `internal`

These words are in `internal`.

##### `advance-here`
( a -- )

Fill memory with zeros up until a given address

##### `word-flags`
( word -- h-addr )

Get the address of the flags for a word

##### `wordlist-id`
( word -- h-addr )

Get the address of the wordlist ID for a word

##### `next-word`
( word -- addr-addr )

Get the address of the address of the next word in the word specified;

##### `word-name`
( word -- b-addr )

Get the name of a word (a counted word)

##### `common-prefix`
( b-addr1 bytes1 b-addr2 bytes2 -- bytes3 )

Get the length of a common prefix to two strings

##### `prefix?`
( b-addr1 bytes1 b-addr2 bytes2 -- flag )

Get whether a string has a prefix (the prefix is the first string)

##### `hidden?`
( word -- f )

Get whether a word is hidden (note that this means whether a word is displayed by WORDS, not whether it will be found by `find` set to find visible words)

##### `words-column`
( b-addr bytes column1 -- column2 )

Actually print a string in one out of four columns, taking up more than one
column if necessary

##### `words-column-wrap`
( b-addr bytes column1 -- column2 )

Print a string in one out of four columns, taking up more than one column
if necessary

##### `words-dict`
( dict wid column1 -- column2 )

Display all the words in a dictionary starting at a given column, and
returning the next column

##### `lookup-dict`
( b-addr bytes dict wid column1 -- column2 )

Display all the words in a dictionary starting at a given column and returning
the next column

##### `find-prefix-len`
( b-addr bytes dict -- prefix-bytes )

Find the common prefix length to a word

##### `search-word-info`
( b-addr bytes dict -- )

Search for all the words that go by a certain name in a given dictionary

##### `search-by-xt`
( dict xt -- name|0 flag )

Search for a word by its xt

##### `<builds-with-name`
( addr bytes -- )

Create a word that executes code specified by DOES>

##### `block-align,`
Align to flash block if compiling to flash

##### `next-user-space`
( -- addr )

Look up next available user space

##### `set-next-user-space`
( addr -- )

Specify next available user space

##### `next-ram-space`
( -- addr )

Look up next available RAM space

##### `set-next-ram-space`
( addr -- )

Specify next available RAM space

##### `user>`
( -- )

Complete a USER variable word

##### `ram-cvariable`
( "name" -- )

Allocate a byte variable in RAM

##### `ram-hvariable`
( "name" -- )

Allocate a halfword variable in RAM

##### `ram-variable`
( "name" -- )

Allocate a variable in RAM

##### `ram-2variable`
( "name" -- )

Allocate a doubleword variable in RAM

##### `ram-buffer:`
( bytes "name" -- )

Allocate a buffer in RAM

##### `ram-aligned-buffer:`
( bytes "name" -- )

Allocate a cell-aligned buffer in RAM

##### `cpus-deferred-context-switch`

Is there a deferred context switch for a given CPU.

##### `cpus-in-critical`

Critical section state for a given CPU.

##### `in-critical`
( -- addr )

Address of critical section state for the current CPU.

##### `set-current-flash-wordlist`
( wid -- )

Specify current flash wordlist

##### `get-current-flash-wordlist`
( -- wid )

Look up the current flash wordlist

##### `defer-xt@`
( deferred-xt -- addr )

Get the address at which the xt for a deferred xt is stored.

##### `fraction-size`
( -- u )

Get current fraction size

##### `format-fraction`
( u b-addr bytes -- b-addr bytes )

Format digits to the right of the decimal point

##### `add-decimal`
( b-addr bytes -- b-addr bytes )

Add a decimal point

##### `do-nothing`

Do nothing

##### `init-flash-dict`

Initialize the flash dictionary
	
##### `init-dict`

Initiatlize the dictionary

##### `find-last-visible-word`

Find the last visible word
	
##### `do-init`

 Run the initialization routine, if there is one
	
##### `init-variables`

Initialize the variables

##### `find-dict`
( addr bytes dict wid -- word|0 )

Find a word in a specific dictionary for a specific wordlist or return zero for no word found

##### `find-in-wordlist`
( addr bytes wid -- word|0 )

Find a word in a specific wordlist or return zero for no word found

##### `find-dict-by-xt`
( xt dict -- word|0 )

Find a word in a dictionary by execution token or return zero for no word found; only words with headers will be found

##### `min-ram-wordlist`
( -- wid )

The minimum RAM wordlist value.

##### `current-ram-wordlist`
( -- addr )

Address of the current RAM wordlist value.

##### `main`

The main functionality, within the main exception handler
	
##### `validate`

Validate the current state

##### `do-prompt`

Display a prompt

##### `do-refill`

Implement the refill hook
	
##### `do-failed-parse`

Implement the failed parse hook
	
##### `do-handle-number`

Implement the handle number hook

##### `parse-integer-core`
( b-addr u base -- n success )

Actually parse an integer 
	
##### `parse-unsigned-core`
( b-addr u base -- u success )

Actually parse an unsigned integer

##### `constant-with-name`

Create a constant with a specified name as a string

##### `2constant-with-name`

Create a 2-word constant with a name specified as a string

##### `skip-to-token`

Skip to the start of a token

##### `format-integer-inner`

The inner portion of formatting an integer as a string

##### `eval-index-ptr`
( -- addr-addr )

Get the address of the address of the evaluation buffer index value.

##### `eval-count-ptr`
( -- addr-addr )

Get the address of the address of the evaluation buffer byte count value.

##### `eval-ptr`
( -- addr )

Get the address of the evaluation buffer.

##### `eval-data`
( -- addr )

Get address of a general-purpose data value to be used by the evaluation refill and EOF check routines.

##### `eval-refill`
( -- xt-addr ) ( xt: -- )

Get address of the evaluation refill routine. This routine is called when `outer` has completed interpreting the entire contents of the evaluation buffer, and should update the evaluation buffer pointer, the evaluation index, and the evaluation byte count accordingly.

##### `eval-eof`
( -- xt-addr ) ( xt: -- eof? )

Get address of the evaluation EOF check routine. `outer` exits if this routine returns non-zero when called.

##### `prompt-disabled`
( -- addr )

Get the address of the prompt-disabled counter (positive values correspond to prompt display being disabled).

##### `dump-ascii-16`
( start-addr -- )

Dump 16 bytets of ASCII.

##### `picture-size`
( -- bytes )

The maximum pictures numeric output size.

##### `picture-offset`
( -- addr )

The start of pictured numeric output user variable.

##### `fraction-size-table`
( -- addr )

The fraction size lookup table.

##### `flush-console-hook`
( -- addr )

The address of the flush console hook variable.

##### `flash-dict-warned`
( -- addr )

The address of the user warned about flash dictionary space flag.

##### `do-flash-validate-dict`
( -- )

Warn the user if flash space is running low.

##### `flash-mini-dict`
( -- addr )

The base address of the flash mini-dictionary. (RP2040 only.)

##### `flash-mini-dict-free`
( -- addr )

The address of the flash mini-dictionary entries left count. (RP2040 only.)

##### `hash-string`
( b-addr bytes -- hash )

Hash a string as a 32-bit value. (RP2040 only.)

##### `hash-string-and-wid`
( b-addr bytes wid -- hash )

Hash a string and a wordlist ID as a 32-bit value; note that this 32-bit value is guaranteed to not be zero. (RP2040 only.)

##### `hash-word`
( word -- hash )

Hash the name and the wordlist ID of a word as a 32-bit value; note that this 32-bit value is guaranteed to not be zero. (RP2040 only.)

##### `clear-flash-mini-dict`
( -- )

Clear the flash mini-dictionary. (RP2040 only.)

##### `x-flash-mini-dict-out-of-space`
( -- )

Flash mini-dictionary is out of space exception. (RP2040 only.)

##### `register-flash-mini-dict-space`
( -- )

Register a new entry is being added to the flash mini-dictionary (as opposed to replacing an existing entry). (RP2040 only.)

##### `equal-words?`
( word0 word1 -- equal? )

Get whether two words have equal names and wordlist ID's. (RP2040 only.)

##### `add-flash-mini-dict-end`
( word -- )

Add an entry to the flash mini-dictionary when filling from start to end. (RP2040 only.)

##### `add-flash-mini-dict-start`
( word -- )

Add an entry to the flash mini-dictionary when filling from end to start. (RP2040 only.)

##### `init-flash-mini-dict`
( -- )

Initialize the flash mini-dictionary, populating it with entries corresponding to the entire flash dictionary. (RP2040 only.)

##### `find-flash-mini-dict`
( b-addr bytes wid -- addr|0 )

Find a word by name in the flash mini-dictionary for a given wordlist ID, returning the word's address or, if not found, 0. (RP2040 only.)

##### `add-flash-mini-dict`
( -- )

Add the word compiled most recently to flash to the flash mini-dictionary, if currently compiling to flash. (RP2040 only.)

##### `find-optimized-wid`
( b-addr bytes wid -- addr|0 )

Find a word by name with the specified wordlist ID making use of the flash mini-dictionary, using the RAM dictionary first if compiling to RAM, and if not found in the RAM dictionary or if currently compiling to flash then trying to find it in the flash mini-dictionary, returning the word's address or, if not found, 0. (RP2040 only.)

##### `find-optimized`
( b-addr bytes -- addr|0 )

Find a word by name in the current wordlist order, making use of the RAM dictionary if compiling to RAM, and making use of the flash mini-dictionary, returning the word's address or, if not found, 0. (RP2040 only.)

