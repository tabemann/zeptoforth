# Internal Words

##### `advance-here`
( a -- )

Fill memory with zeros up until a given address

##### `word-flags`
( word -- h-addr )

Get the address of the flags for a word

##### `wordlist-id`
( word -- h-addr )

Get the address of the wordlist ID for a word

##### `prev-word`
( word -- h-addr )

Get the address of the previous word for a word

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

##### `flash-align,`
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

##### `ram-bvariable`
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

##### `set-current-flash-wordlist`
( wid -- )

Specify current flash wordlist

##### `get-current-flash-wordlist`
( -- wid )

Look up the current flash wordlist

##### `decode-mov16`
( h-addr -- h )

Decode the immediate field from a MOVW or MOVT instruction

##### `decode-literal`
( h-addr -- x )

Decode the immediate field from a pair of a MOVW instruction followed by a MOVT instruction

##### `defer-ram@`
( xt-deferred -- xt )

Get the referred xt from a deferred word in RAM

##### `defer-flash@`
( xt-deferred -- xt )

Get the referred xt from a deferred word in flash

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

Find a word in a specific dictionary for a specific wordlist
( addr bytes mask dict wid -- addr|0 )

##### `find-in-wordlist`

Find a word in a specific wordlist
( addr bytes mask wid -- addr|0 )

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
