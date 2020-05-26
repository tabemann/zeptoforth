# Development Tools Guide

There are a number of useful words provided with zeptoforthas development tools

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

This word dumps memory as hexdump as bytes, along with displaying each byte, if a valid ASCII character, starting from *start-addr* and ending at, non-inclusive, *end-addr*.

## Listing Words

##### `words`
( -- )

This word simply lists every user-viewable word in RAM or in flash, organized as four columns.

##### `lookup`
( "name" -- )

This word lists each word which has a prefix corresponding to the maximal prefix or the specified token which any word in RAM or in flash matches, organized as four columns.