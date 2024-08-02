# Development Tools Words

There are a number of useful words provided with zeptoforth's development tools.

These words are in `forth`.

## Pager

##### `more`
( ? xt -- ? )

A general-purpose pager provided by `full`, `full_swdcom`, and `full_usb` builds. It redirects output from a given xt such that it is displayed one screenful at a time, and the user can after each screenful enter `q` or `Q` to exit out of the pager early; entering any other page continues output. When exiting out of the pager early the data stack is cleaned up afterwards. Otherwise, the stack state before the pager is entered and the stack state left over after xt exits is left unperturbed.

Note that this words cannot be used with zeptocom.js or e4thcom as it assumes full ANSI terminal support, and xterm.js, used by zeptocom.js, and e4thcom do not provide this. This results in waiting forever for a response from the terminal when attempting to look up the size of the terminal or the current cursor coordinates.

## Disassembler

A disassembler that covers (almost all of) the instructions utilized by zeptoforth is included with zeptoforth. It has two different orthogonal modes of operation; one is whether it disassembles instructions in a specified range of addresses or it disassembles just a selected word, the other is whether it disassembles user-friendly assembly including instruction addresses, instructions as hex, addresses to go along with labels, and in one set of cases computes absolute addresses from PC-relative instructions, or whether it disassembles assembler-friendly assembly without such niceities. In both modes it

As a result there are four different words for invoking the disassembler:

##### `disassemble`
( start-addr end-addr -- )

This word disassembles instructions, for human consumption, starting from *start-addr* and ending at, non-inclusive, *end-addr*.

##### `disassemble-for-gas`
( start-addr end-addr -- )

This word disassembles instructions, for feeding into an assembler, starting from *start-addr* and ending at, non-inclusive, *end-addr*.

##### `see`
( "name" -- )

This word disassembles instructions, for human consumption, comprising the whole of the word whose name is specified afterwards.

##### `see-for-gas`
( "name" -- )

This word disassembles instructions, for feeding into an assembler, comprising the whole of the word whose name is specified afterwards.

## Viewing Memory

Memory can be viewed with the following word:

##### `dump`
( start-addr end-addr -- )

This word dumps memory as sequences of 16 bytes in hexadecimal, along with displaying each byte, if a valid ASCII character, starting from *start-addr* and ending at, non-inclusive, *end-addr*.

##### `dump-halfs`
( start-addr end-addr -- )

This word dumps memory as sequences of 8 16-bit halfwords in hexadecimal, along with displaying each byte, if a valid ASCII character, starting from *start-addr* and ending at, non-inclusive, *end-addr*.

##### `dump-cells`
( start-addr end-addr -- )

This word dumps memory as sequences of 4 32-bit cells in hexadecimal, along with displaying each byte, if a valid ASCII character, starting from *start-addr* and ending at, non-inclusive, *end-addr*.

##### `dump-ascii`
( start-addr end-addr -- )

This word dumps memory as sequences of 64 ASCII characters, starting from *start-addr* and ending at, non-inclusing *end-addr*.

## Listing Words

##### `words`
( -- )

This word simply lists every user-viewable word in the current namespace in RAM or in flash, organized as four columns.

##### `words-in`
( module -- )

This word list every user-viewable word in a selected module in RAM or in flash, organized as four columns.

##### `lookup`
( "name" -- )

This word lists each word which has a prefix corresponding to the maximal prefix or the specified token which any word in the current namespace in RAM or in flash matches, organized as four columns.

##### `lookup-in`
( module "name" -- )

This word lists each word which has a prefix corresponding to the maximal prefix or the specified token which any word in a selected module in RAM or in flash matches, organized as four columns.

##### `word-info`
( "name" -- )

Dump all the words that go by a certain name.

## Listing Words with a Pager

In full builds of zeptoforth "more" versions of `words`, `words-in`, `lookup`, and `lookup-in` are provided. These are named `more-words`, `more-words-in`, `more-lookup`, and `more-lookup-in` respectively. Unlike their non-"more" counterparts these integrate a pager, where one screenful of words is printed to the console at a time, and then afterwards the user is prompted to either enter `q` to exit the "more" word or to enter any other key to continue

##### `more-words`
( -- )

This word simply lists with a pager every user-viewable word in the current namespace in RAM or in flash, organized as four columns.

##### `more-words-in`
( module -- )

This word list with a pager every user-viewable word in a selected module in RAM or in flash, organized as four columns.

##### `more-lookup`
( "name" -- )

This word lists with a pager each word which has a prefix corresponding to the maximal prefix or the specified token which any word in the current namespace in RAM or in flash matches, organized as four columns.

##### `more-lookup-in`
( module "name" -- )

This word lists with a pager each word which has a prefix corresponding to the maximal prefix or the specified token which any word in a selected module in RAM or in flash matches, organized as four columns.
