\ This code is derived from puff.h and puff.c, with the following license block
\ taken from puff.h
\
\ The Forth conversion of this code is:
\ Copyright (c) 2026 Travis Bemann

\ puff.h
\ Copyright (C) 2002-2013 Mark Adler, all rights reserved
\ version 2.3, 21 Jan 2013
\ 
\ This software is provided 'as-is', without any express or implied
\ warranty.  In no event will the author be held liable for any damages
\ arising from the use of this software.
\ 
\ Permission is granted to anyone to use this software for any purpose,
\ including commercial applications, and to alter it and redistribute it
\ freely, subject to the following restrictions:
\ 
\ 1. The origin of this software must not be misrepresented; you must not
\    claim that you wrote the original software. If you use this software
\    in a product, an acknowledgment in the product documentation would be
\    appreciated but is not required.
\ 2. Altered source versions must be plainly marked as such, and must not be
\    misrepresented as being the original software.
\ 3. This notice may not be removed or altered from any source distribution.
\
\ Mark Adler    madler@alumni.caltech.edu

\ puff.c
\ Copyright (C) 2002-2013 Mark Adler
\ For conditions of distribution and use, see copyright notice in puff.h
\ version 2.3, 21 Jan 2013
\ 
\ puff.c is a simple inflate written to be an unambiguous way to specify the
\ deflate format.  It is not written for speed but rather simplicity.  As a
\ side benefit, this code might actually be useful when small code is more
\ important than speed, such as bootstrap applications.  For typical deflate
\ data, zlib's inflate() is about four times as fast as puff().  zlib's
\ inflate compiles to around 20K on my machine, whereas puff.c compiles to
\ around 4K on my machine (a PowerPC using GNU cc).  If the faster decode()
\ function here is used, then puff() is only twice as slow as zlib's
\ inflate().
\ 
\ All dynamically allocated memory comes from the stack.  The stack required
\ is less than 2K bytes.  This code is compatible with 16-bit int's and
\ assumes that long's are at least 32 bits.  puff.c uses the short data type,
\ assumed to be 16 bits, for arrays in order to conserve memory.  The code
\ works whether integers are stored big endian or little endian.
\ 
\ In the comments below are "Format notes" that describe the inflate process
\ and document some of the less obvious aspects of the format.  This source
\ code is meant to supplement RFC 1951, which formally describes the deflate
\ format:
\ 
\    http://www.zlib.org/rfc-deflate.html
\
\ Change history:
\ 
\ 1.0  10 Feb 2002     - First version
\ 1.1  17 Feb 2002     - Clarifications of some comments and notes
\                      - Update puff() dest and source pointers on negative
\                        errors to facilitate debugging deflators
\                      - Remove longest from struct huffman -- not needed
\                      - Simplify offs[] index in construct()
\                      - Add input size and checking, using longjmp() to
\                        maintain easy readability
\                      - Use short data type for large arrays
\                      - Use pointers instead of long to specify source and
\                        destination sizes to avoid arbitrary 4 GB limits
\ 1.2  17 Mar 2002     - Add faster version of decode(), doubles speed (!),
\                        but leave simple version for readability
\                      - Make sure invalid distances detected if pointers
\                        are 16 bits
\                      - Fix fixed codes table error
\                      - Provide a scanning mode for determining size of
\                        uncompressed data
\ 1.3  20 Mar 2002     - Go back to lengths for puff() parameters [Gailly]
\                      - Add a puff.h file for the interface
\                      - Add braces in puff() for else do [Gailly]
\                      - Use indexes instead of pointers for readability
\ 1.4  31 Mar 2002     - Simplify construct() code set check
\                      - Fix some comments
\                      - Add FIXLCODES #define
\ 1.5   6 Apr 2002     - Minor comment fixes
\ 1.6   7 Aug 2002     - Minor format changes
\ 1.7   3 Mar 2003     - Added test code for distribution
\                      - Added zlib-like license
\ 1.8   9 Jan 2004     - Added some comments on no distance codes case
\ 1.9  21 Feb 2008     - Fix bug on 16-bit integer architectures [Pohland]
\                      - Catch missing end-of-block symbol error
\ 2.0  25 Jul 2008     - Add #define to permit distance too far back
\                      - Add option in TEST code for puff to write the data
\                      - Add option in TEST code to skip input bytes
\                      - Allow TEST code to read from piped stdin
\ 2.1   4 Apr 2010     - Avoid variable initialization for happier compilers
\                      - Avoid unsigned comparisons for even happier compilers
\ 2.2  25 Apr 2010     - Fix bug in variable initializations [Oberhumer]
\                      - Add const where appropriate [Oberhumer]
\                      - Split if's and ?'s for coverage testing
\                      - Break out test code to separate file
\                      - Move NIL to puff.h
\                      - Allow incomplete code only if single code length is 1
\                      - Add full code coverage test to Makefile
\ 2.3  21 Jan 2013     - Check for invalid code length codes in dynamic blocks

begin-module puff

  \ Configuration defines
  false constant SLOW
  true constant INFLATE_ALLOW_INVALID_DISTANCE_TOOFAR_ARRR
  
  \ Maximums for allocations and loops.  It is not useful to change these --
  \ they are fixed by the deflate format.
  15 constant MAXBITS           \ maximum bits in a code
  286 constant MAXLCODES        \ maximum number of literal/length codes
  30 constant MAXDCODES         \ maximum number of distance codes
  MAXLCODES MAXDCODES + constant MAXCODES \ maximum codes lengths to read
  288 constant FIXLCODES        \ number of fixed literal/length codes

  \ input and output state
  begin-structure puff-state-size
    \ output state
    field: puff-out         \ address of output buffer
    field: puff-outlen      \ available space at puff-out
    field: puff-outcnt      \ bytes written to puff-out so far

    \ input state
    field: puff-in          \ address of input buffer
    field: puff-inlen       \ available space at puff-in
    field: puff-incnt       \ bytes read so far
    field: puff-bitbuf      \ bit buffer
    field: puff-bitcnt      \ number of bits in bit buffer
  end-structure

  \ Out of bits exception
  : x-out-of-bits ." out of bits!" cr ;
  
  \ Return need bits from the input stream.  This always leaves less than
  \ eight bits in the buffer.  bits() works properly for need == 0.
  \ 
  \ Format notes:
  \ 
  \ - Bits are stored in bytes from the least significant bit to the most
  \   significant bit.  Therefore bits are dropped from the bottom of the bit
  \   buffer, using shift right, and new bytes are appended to the top of the
  \   bit buffer, using shift left.
  : bits { need s -- data }
    s puff-bitbuf @ { val } \ bit accumulator (can use up to 20 bits

    begin s puff-bitcnt @ need < while
      s puff-incnt @ s puff-inlen @ <> averts x-out-of-bits
      s puff-in @ s puff-incnt @ + c@ s puff-bitcnt @ lshift val or to val
      1 s puff-incnt +!
      8 s puff-bitcnt +!
    repeat

    \ drop need bits and update buffer, always zero to seven bits left
    val need rshift s puff-bitbuf !
    need negate s puff-bitcnt +!

    \ return need bits, zeroing the bits above that
    val 1 need lshift 1- and
  ;

  \ Process a stored block.
  \ 
  \ Format notes:
  \ 
  \ - After the two-bit stored block type (00), the stored block length and
  \   stored bytes are byte-aligned for fast copying.  Therefore any leftover
  \   bits in the byte that has the last bit of the type, as many as seven, are
  \   discarded.  The value of the discarded bits are not defined and should
  \   not be checked against any expectation.
  \ 
  \ - The second inverted copy of the stored block length does not have to be
  \   checked, but it's probably a good idea to do so anyway.
  \ 
  \ - A stored block can have zero length.  This is sometimes used to
  \   byte-align subsets of the compressed data for random access or partial
  \   recovery.
  : stored { s -- check }
    \ discard leftover bits from current byte (assumes puff-bitcnt < 8)
    0 s puff-bitbuf !
    0 s puff-bitcnt !

    \ get length and check against its one's complement
    s puff-incnt @ 4 + s puff-inlen @ > if
      2 exit \ not enough input
    then
    s puff-incnt @ s puff-in @ + c@ { len }
    1 s puff-incnt +!
    s puff-incnt @ s puff-in @ + c@ 8 lshift len or to len
    1 s puff-incnt +!
    s puff-incnt @ s puff-in @ + c@ len not $FF and <>
    1 s puff-incnt +!
    s puff-incnt @ s puff-in @ + c@ len not 8 rshift $FF and <>
    1 s puff-incnt +!
    or if
      -2 exit \ didn't match complement
    then
    \ copy len bytes from in to out
    s puff-incnt @ len + s puff-inlen @ > if
      2 exit \ not enough input
    then
    s puff-out @ if
      s puff-outcnt @ len + s puff-outlen @ > if
        1 exit \ not enough output space
      then
      begin len while
        -1 +to len
        s puff-incnt @ s puff-in @ + c@
        s puff-outcnt @ s puff-out @ + c!
        1 s puff-outcnt +!
        1 s puff-incnt +!
      repeat
    else \ just scanning
      len s puff-outcnt +!
      len s puff-incnt +!
    then
    
    \ done with a valid stored block
    0
  ;

  \ Huffman code decoding tables.  count[1..MAXBITS] is the number of symbols
  \ of each length, which for a canonical code are stepped through in order.
  \ puff-symbol are the symbol values in canonical order, where the number of
  \ entries is the sum of the counts in count[].  The decoding process can be
  \ seen in the function decode() below.
  begin-structure puff-huffman-size
    field: puff-count \ address of array of number of symbols of each length,
                      \ each 16-bit in size
    field: puff-symbol \ address of array of canonically ordered symbols, each
                       \ 16-bit in size
  end-structure

  \ Decode a code from the stream s using huffman table h.  Return the symbol
  \ or a negative value if there is an error.  If all of the lengths are zero,
  \ i.e. an empty code, or if the code is incomplete and an invalid code is
  \ received, then -10 is returned after reading MAXBITS bits.
  \ 
  \ Format notes:
  \ 
  \ - The codes as stored in the compressed data are bit-reversed relative to
  \   a simple integer ordering of codes of the same lengths.  Hence below the
  \   bits are pulled from the compressed data one at a time and used to
  \   build the code value reversed from what is in the stream in order to
  \   permit simple integer comparisons for decoding.  A table-based decoding
  \   scheme (as used in zlib) does not need to do this reversal.
  \ 
  \ - The first code for the shortest length is all zeros.  Subsequent codes of
  \   the same length are simply integer increments of the previous code.  When
  \   moving up a length, a zero bit is appended to the code.  For a complete
  \   code, the last code of the longest length will be all ones.
  \ 
  \ - Incomplete codes are handled by this decoder, since they are permitted
  \   in the deflate format.  See the format notes for fixed() and dynamic().
  SLOW [if]
    : decode { h s -- symbol }
      1 { len } \ current number of bits in code
      0 { bcode } \ len bits being decoded
      0 { first } \ first code of length len
      0 { index } \ index of first code of length len in symbol table

      begin len MAXBITS <= while
        1 s bits bcode or to bcode \ get next bit
        h puff-count @ len 2* + h@ { count } \ number of codes of length len
        bcode count - first < if \ if length len return symbol
          index bcode first - + 2* h puff-symbol @ + h@ exit
        then
        count +to index \ else update for next length
        count +to first
        first 1 lshift to first
        bcode 1 lshift to bcode
        1 +to len
      repeat

      -10 \ ran out of codes
    ;
  [else] \ SLOW not

    \ A faster version of decode() for real applications of this code.   It's
    \ not as readable, but it makes puff() twice as fast.  And it only makes
    \ the code a few percent larger.
    : decode { h s -- symbol }
      1 { len } \ current number of bits in code
      0 { bcode } \ len bits being decoded
      0 { first } \ first code of length len
      0 { index } \ index of first code of length len in symbol table
      s puff-bitbuf @ { bitbuf } \ bits from stream
      s puff-bitcnt @ { left } \ bits left in next or left to process
      h puff-count @ 2 + { cnext } \ next number of codes

      begin
        begin
          left
          -1 +to left
        while
          bitbuf 1 and bcode or to bcode
          bitbuf 1 rshift to bitbuf
          cnext h@ { count } \ number of codes of length len
          2 +to cnext
          bcode count - first < if \ if length len, return symbol
            bitbuf s puff-bitbuf !
            s puff-bitcnt @ len - 7 and s puff-bitcnt !
            index bcode first - + 2* h puff-symbol @ + h@ exit
          then
          count +to index \ else update for next length
          count +to first
          first 1 lshift to first
          bcode 1 lshift to bcode
          1 +to len
        repeat
        [ MAXBITS 1+ ] literal len - to left
        left 0= if
          -10 exit \ ran out of codes
        then
        s puff-incnt @ s puff-inlen @ <> averts x-out-of-bits
        s puff-incnt @ s puff-in @ + c@ to bitbuf
        1 s puff-incnt +!
        left 8 > if
          8 to left
        then
      again
    ;
  [then] \ SLOW

  \ Given the list of code lengths length[0..n-1] representing a canonical
  \ Huffman code for n symbols, construct the tables required to decode those
  \ codes.  Those tables are the number of codes of each length, and the
  \ symbols sorted by length, retaining their original order within each
  \ length.  The return value is zero for a complete code set, negative for an
  \ over-subscribed code set, and positive for an incomplete code set.  The
  \ tables can be used if the return value is zero or positive, but they
  \ cannot be used if the return value is negative.  If the return value is
  \ zero, it is not possible for decode() using that table to return an
  \ error--any stream of enough bits will resolve to a symbol.  If the return
  \ value is positive, then it is possible for decode() using that table to
  \ return an error for received codes past the end of the incomplete lengths.
  \ 
  \ Not used by decode(), but used for error checking, h->count[0] is the
  \ number of the n symbols not in the code.  So n - h->count[0] is the number
  \ of codes.  This is useful for checking for incomplete codes that have more
  \ than one symbol, which is an error in a dynamic block.
  \ 
  \ Assumption: for all i in 0..n-1, 0 <= length[i] <= MAXBITS
  \ This is assured by the construction of the length arrays in dynamic() and
  \ fixed() and is not verified by construct().
  \ 
  \ Format notes:
  \ 
  \ - Permitted and expected examples of incomplete codes are one of the fixed
  \   codes and any code with a single symbol which in deflate is coded as one
  \   bit instead of zero bits.  See the format notes for fixed() and
  \   dynamic().
  \ 
  \ - Within a given code length, the symbols are kept in ascending order for
  \   the code bits definition.
  : construct ( n length h -- x )
    MAXBITS 1+ 2* [:
      { n length h offs } \ offsets in symbol table for each length

      \ count number of codes of each length
      0 { len } \ current length when stepping through h->count[]
      begin len MAXBITS <= while
        0 len 2* h puff-count @ + h!
        1 +to len
      repeat
      0 { symbol } \ current symbol when stepping through length[]
      begin symbol n < while
        \ assumes lengths are within bounds
        1 symbol 2* length + h@ 2* h puff-count @ + h+!
        1 +to symbol
      repeat
      0 h puff-count @ + h@ n = if \ no codes!
        0 exit \ complete, but decode() will fail
      then
      
      \ check for an over-subscribed or incomplete set of lengths
      1 { left } \ number of possible codes left of current length
      \ one possible code of zero length
      1 to len
      begin len MAXBITS <= while
        left 1 lshift to left \ one more bit, double codes left
        \ deduct count from possible codes
        left len 2* h puff-count @ + h@ - to left
        left 0< if
          left exit \ over-subscribed--return negative
        then
        1 +to len
      repeat
      \ left > 0 means incomplete

      \ generate offsets into symbol table for each length for sorting
      0 1 2* offs + h!
      1 to len
      begin len MAXBITS < while
        len 2* offs + h@ len 2* h puff-count @ + h@ + len 1+ 2* offs + h!
        1 +to len
      repeat

      \ put symbols in table sorted by length, by symbol order within each
      \ length
      0 to symbol
      begin symbol n < while
        symbol 2* length + h@ if
          symbol 2* length + h@ 2* offs +
          dup h@ dup { off } 1+ swap h!
          symbol off 2* h puff-symbol @ + h!
        then
        1 +to symbol
      repeat

      \ return zero for complete set, positive for incomplete set
      left
    ;] with-aligned-allot
  ;

  \ Decode literal/length and distance codes until an end-of-block code.
  \ 
  \ Format notes:
  \ 
  \ - Compressed data that is after the block type if fixed or after the code
  \   description if dynamic is a combination of literals and length/distance
  \   pairs terminated by and end-of-block code.  Literals are simply Huffman
  \   coded bytes.  A length/distance pair is a coded length followed by a
  \   coded distance to represent a string that occurs earlier in the
  \   uncompressed data that occurs again at the current location.
  \ 
  \ - Literals, lengths, and the end-of-block code are combined into a single
  \   code of up to 286 symbols.  They are 256 literals (0..255), 29 length
  \   symbols (257..285), and the end-of-block symbol (256).
  \ 
  \ - There are 256 possible lengths (3..258), and so 29 symbols are not enough
  \   to represent all of those.  Lengths 3..10 and 258 are in fact represented
  \   by just a length symbol.  Lengths 11..257 are represented as a symbol and
  \   some number of extra bits that are added as an integer to the base length
  \   of the length symbol.  The number of extra bits is determined by the base
  \   length symbol.  These are in the static arrays below, lens[] for the base
  \   lengths and lext[] for the corresponding number of extra bits.
  \ 
  \ - The reason that 258 gets its own symbol is that the longest length is used
  \   often in highly redundant files.  Note that 258 can also be coded as the
  \   base value 227 plus the maximum extra value of 31.  While a good deflate
  \   should never do this, it is not an error, and should be decoded properly.
  \ 
  \ - If a length is decoded, including its extra bits if any, then it is
  \   followed a distance code.  There are up to 30 distance symbols.  Again
  \   there are many more possible distances (1..32768), so extra bits are added
  \   to a base value represented by the symbol.  The distances 1..4 get their
  \   own symbol, but the rest require extra bits.  The base distances and
  \   corresponding number of extra bits are below in the static arrays dist[]
  \   and dext[].
  \ 
  \ - Literal bytes are simply written to the output.  A length/distance pair is
  \   an instruction to copy previously uncompressed bytes to the output.  The
  \   copy is from distance bytes back in the output stream, copying for length
  \   bytes.
  \ 
  \ - Distances pointing before the beginning of the output data are not
  \   permitted.
  \ 
  \ - Overlapped copies, where the length is greater than the distance, are
  \   allowed and common.  For example, a distance of one and a length of 258
  \   simply copies the last byte 258 times.  A distance of four and a length of
  \   twelve copies the last four bytes three times.  A simple forward copy
  \   ignoring whether the length is greater than the distance or not implements
  \   this correctly.  You should not use memcpy() since its behavior is not
  \   defined for overlapped arrays.  You should not use memmove() or bcopy()
  \   since though their behavior -is- defined for overlapping arrays, it is
  \   defined to do the wrong thing in this case.

  \ Size base for length codes 257..285
  create puff-lens
  3 h, 4 h, 5 h, 6 h, 7 h, 8 h, 9 h, 10 h, 11 h, 13 h, 15 h, 17 h, 19 h, 23 h,
  27 h, 31 h, 35 h, 43 h, 51 h, 59 h, 67 h, 83 h, 99 h, 115 h, 131 h, 163 h,
  195 h, 227 h, 258 h,
  29 constant puff-lens-count

  \ Extra bits for length codes 257..285
  create puff-lext
  0 h, 0 h, 0 h, 0 h, 0 h, 0 h, 0 h, 0 h, 1 h, 1 h, 1 h, 1 h, 2 h, 2 h, 2 h,
  2 h, 3 h, 3 h, 3 h, 3 h, 4 h, 4 h, 4 h, 4 h, 5 h, 5 h, 5 h, 5 h, 0 h,
  29 constant puff-lext-count

  \ Offset base for distance codes 0..29
  create puff-dists
  1 h, 2 h, 3 h, 4 h, 5 h, 7 h, 9 h, 13 h, 17 h, 25 h, 33 h, 49 h, 65 h, 97 h,
  129 h, 193 h, 257 h, 385 h, 513 h, 769 h, 1025 h, 1537 h, 2049 h, 3073 h,
  4097 h, 6145 h, 8193 h, 12289 h, 16385 h, 24577 h,
  30 constant puff-dists-count

  \ Extra bits for distance codes 0..29
  create puff-dext
  0 h, 0 h, 0 h, 0 h, 1 h, 1 h, 2 h, 2 h, 3 h, 3 h, 4 h, 4 h, 5 h, 5 h, 6 h,
  6 h, 7 h, 7 h, 8 h, 8 h, 9 h, 9 h, 10 h, 10 h, 11 h, 11 h,
  12 h, 12 h, 13 h, 13 h,
  30 constant puff-dext-count

  : codes { distcode lencode s -- x }

    \ decode literals and length/distance pairs
    begin
      lencode s decode { symbol } \ decoded symbol
      symbol 0< if
        symbol exit \ invalid symbol
      then
      symbol 256 < if \ literal: symbol is the byte
        \ write out the literal
        s puff-out @ if
          s puff-outcnt @ s puff-outlen @ = if
            1 exit
          then
          symbol s puff-outcnt @ s puff-out @ + c!
        then
        1 s puff-outcnt +!
      else
        symbol 256 > if \ length
          \ get and compute length
          -257 +to symbol
          symbol 29 >= if
            -10 exit \ invalid fixed code
          then
          \ length for copy
          symbol 2* puff-lens + h@ symbol 2* puff-lext + h@ s bits + { len }

          \ get and check distance
          distcode s decode to symbol
          symbol 0< if
            symbol exit \ invalid symbol
          then
          \ distance for copy
          symbol 2* puff-dists + h@ symbol 2* puff-dext + h@ s bits + { dist }
          [ INFLATE_ALLOW_INVALID_DISTANCE_TOOFAR_ARRR not ] [if]
            dist s puff-outcnt @ > if
              -11 exit \ distance too far back
            then
          [then]

          \ copy length bytes from distance bytes back
          s puff-out @ if
            s puff-outcnt @ len + s puff-outlen @ > if
              1 exit
            then
            begin len while
              -1 +to len
              [ INFLATE_ALLOW_INVALID_DISTANCE_TOOFAR_ARRR ] [if]
                dist s puff-outcnt @ > if
                  0
                else
                  s puff-outcnt @ dist - s puff-out @ + c@
                then
              [else]
                s puff-outcnt @ dist - s puff-out @ + c@
              [then]
              s puff-outcnt @ s puff-out @ + c!
              1 s puff-outcnt +!
            repeat
          else
            len s puff-outcnt +!
          then
        then
      then

      symbol 256 = \ end of block symbol
    until
    
    \ done with a valid fixed or dynamic block
    0
  ;

  \ A structure containing the code tables
  begin-structure puff-codes-size
    MAXBITS 1+ 2* cell align +field puff-lencnt
    FIXLCODES 2* cell align +field puff-lensym
    MAXBITS 1+ 2* cell align +field puff-distcnt
    MAXDCODES 2* cell align +field puff-distsym
    puff-huffman-size +field puff-lencode
    puff-huffman-size +field puff-distcode
  end-structure

  \ Actually allot space for the fixed code tables
  puff-codes-size buffer: puff-fixed

  \ Initialize the fixed code tables
  : init-puff-fixed ( -- )
    FIXLCODES 2* [: { lengths }
      \ construct lencode and distcode
      puff-fixed puff-lencnt puff-fixed puff-lencode puff-count !
      puff-fixed puff-lensym puff-fixed puff-lencode puff-symbol !
      puff-fixed puff-distcnt puff-fixed puff-distcode puff-count !
      puff-fixed puff-distsym puff-fixed puff-distcode puff-symbol !

      \ literal/length table
      0 { symbol }
      begin symbol 144 < while
        8 symbol 2* lengths + h!
        1 +to symbol
      repeat
      begin symbol 256 < while
        9 symbol 2* lengths + h!
        1 +to symbol
      repeat
      begin symbol 280 < while
        7 symbol 2* lengths + h!
        1 +to symbol
      repeat
      begin symbol FIXLCODES < while
        8 symbol 2* lengths + h!
        1 +to symbol
      repeat
      FIXLCODES lengths puff-fixed puff-lencode construct drop
      
      \ distance table
      0 to symbol
      begin symbol MAXDCODES < while
        5 symbol 2* lengths + h!
        1 +to symbol
      repeat
      MAXDCODES lengths puff-fixed puff-distcode construct drop
    ;] with-aligned-allot
  ;
  initializer init-puff-fixed

  \ Process a fixed codes block.
  \
  \ Format notes:
  \
  \ - This block type can be useful for compressing small amounts of data for
  \   which the size of the code descriptions in a dynamic block exceeds the
  \   benefit of custom codes for that block.  For fixed codes, no bits are
  \   spent on code descriptions.  Instead the code lengths for literal/length
  \   codes and distance codes are fixed.  The specific lengths for each symbol
  \   can be seen in the "for" loops below.
  \
  \ - The literal/length code is complete, but has two symbols that are invalid
  \   and should result in an error if received.  This cannot be implemented
  \   simply as an incomplete code since those two symbols are in the "middle"
  \   of the code.  They are eight bits long and the longest literal/length\
  \   code is nine bits.  Therefore the code must be constructed with those
  \   symbols, and the invalid symbols must be detected after decoding.
  \
  \ - The fixed distance codes also have two invalid symbols that should result
  \   in an error if received.  Since all of the distance codes are the same
  \   length, this can be implemented as an incomplete code.  Then the invalid
  \   codes are detected while decoding.
  : fixed { s -- x }
    \ decode data until end-of-block code
    puff-fixed puff-distcode puff-fixed puff-lencode s codes
  ;

  \ Process a dynamic codes block.
  \
  \ Format notes:
  \
  \ - A dynamic block starts with a description of the literal/length and
  \   distance codes for that block.  New dynamic blocks allow the compressor to
  \   rapidly adapt to changing data with new codes optimized for that data.
  \
  \ - The codes used by the deflate format are "canonical", which means that
  \   the actual bits of the codes are generated in an unambiguous way simply
  \   from the number of bits in each code.  Therefore the code descriptions
  \   are simply a list of code lengths for each symbol.
  \
  \ - The code lengths are stored in order for the symbols, so lengths are
  \   provided for each of the literal/length symbols, and for each of the
  \   distance symbols.
  \
  \ - If a symbol is not used in the block, this is represented by a zero as the
  \   code length.  This does not mean a zero-length code, but rather that no
  \   code should be created for this symbol.  There is no way in the deflate
  \   format to represent a zero-length code.
  \
  \ - The maximum number of bits in a code is 15, so the possible lengths for
  \   any code are 1..15.
  \
  \ - The fact that a length of zero is not permitted for a code has an
  \   interesting consequence.  Normally if only one symbol is used for a given
  \   code, then in fact that code could be represented with zero bits.  However
  \   in deflate, that code has to be at least one bit.  So for example, if
  \   only a single distance base symbol appears in a block, then it will be
  \   represented by a single code of length one, in particular one 0 bit.  This
  \   is an incomplete code, since if a 1 bit is received, it has no meaning,
  \   and should result in an error.  So incomplete distance codes of one symbol
  \   should be permitted, and the receipt of invalid codes should be handled.
  \
  \ - It is also possible to have a single literal/length code, but that code
  \   must be the end-of-block code, since every dynamic block has one.  This
  \   is not the most efficient way to create an empty block (an empty fixed
  \   block is fewer bits), but it is allowed by the format.  So incomplete
  \   literal/length codes of one symbol should also be permitted.
  \
  \ - If there are only literal codes and no lengths, then there are no distance
  \   codes.  This is represented by one distance code with zero bits.
  \
  \ - The list of up to 286 length/literal lengths and up to 30 distance lengths
  \   are themselves compressed using Huffman codes and run-length encoding.  In
  \   the list of code lengths, a 0 symbol means no code, a 1..15 symbol means
  \   that length, and the symbols 16, 17, and 18 are run-length instructions.
  \   Each of 16, 17, and 18 are followed by extra bits to define the length of
  \   the run.  16 copies the last length 3 to 6 times.  17 represents 3 to 10
  \   zero lengths, and 18 represents 11 to 138 zero lengths.  Unused symbols
  \   are common, hence the special coding for zero lengths.
  \
  \ - The symbols for 0..18 are Huffman coded, and so that code must be
  \   described first.  This is simply a sequence of up to 19 three-bit values
  \   representing no code (0) or the code length for that symbol (1..7).
  \
  \ - A dynamic block starts with three fixed-size counts from which is computed
  \   the number of literal/length code lengths, the number of distance code
  \   lengths, and the number of code length code lengths (ok, you come up with
  \   a better name!) in the code descriptions.  For the literal/length and
  \   distance codes, lengths after those provided are considered zero, i.e. no
  \   code.  The code length code lengths are received in a permuted order (see
  \   the order[] array below) to make a short code length code length list more
  \   likely.  As it turns out, very short and very long codes are less likely
  \   to be seen in a dynamic code description, hence what may appear initially
  \   to be a peculiar ordering.
  \
  \ - Given the number of literal/length code lengths (nlen) and distance code
  \   lengths (ndist), then they are treated as one long list of nlen + ndist
  \   code lengths.  Therefore run-length coding can and often does cross the
  \   boundary between the two sets of lengths.
  \
  \ - So to summarize, the code description at the start of a dynamic block is
  \   three counts for the number of code lengths for the literal/length codes,
  \   the distance codes, and the code length codes.  This is followed by the
  \   code length code lengths, three bits each.  This is used to construct the
  \   code length code which is used to read the remainder of the lengths.  Then
  \   the literal/length code lengths and distance lengths are read as a single
  \   set of lengths using the code length codes.  Codes are constructed from
  \   the resulting two sets of lengths, and then finally you can start
  \   decoding actual compressed data in the block.
  \
  \ - For reference, a "typical" size for the code description in a dynamic
  \   block is around 80 bytes.

  \ Permutation of code length codes
  create puff-order
  16 h, 17 h, 18 h, 0 h, 8 h, 7 h, 9 h, 6 h, 10 h, 5 h, 11 h, 4 h, 12 h, 3 h,
  13 h, 2 h, 14 h, 1 h, 15 h,
  19 constant puff-order-count

  : dynamic ( s -- x )
    MAXCODES 2* [:
      puff-codes-size [: { s lengths codesb }
        \ construct lencode and distcode
        codesb puff-lencnt codesb puff-lencode puff-count !
        codesb puff-lensym codesb puff-lencode puff-symbol !
        codesb puff-distcnt codesb puff-distcode puff-count !
        codesb puff-distsym codesb puff-distcode puff-symbol !

        \ get number of lengths in each table, check lengths
        5 s bits 257 + { nlen }
        5 s bits 1 + { ndist }
        4 s bits 4 + { ncode }
        nlen MAXLCODES > ndist MAXDCODES > or if
          -3 exit \ bad counts
        then

        \ read code length code lengths (really), missing lengths are zero
        0 { index }
        begin index ncode < while
          3 s bits index 2* puff-order + h@ 2* lengths + h!
          1 +to index
        repeat
        begin index 19 < while
          0 index 2* puff-order + h@ 2* lengths + h!
          1 +to index
        repeat

        \ build huffman table for code lengths codes (use lencode temporarily)
        19 lengths codesb puff-lencode construct if
          -1 exit \ require complete code set here
        then
        
        \ read length/literal and distance code length tables
        0 to index
        begin index nlen ndist + < while
          codesb puff-lencode s decode { symbol } \ decoded value

          symbol 0< if
            symbol exit \ invalid symbol
          then
          symbol 16 < if \ length in 0..15
            symbol index 2* lengths + h!
            1 +to index
          else \ repeat instruction
            0 { len } \ last length to repeat -- assume repeating zeroes
            symbol 16 = if \ repeat last length 3..6 times
              index 0= if
                -5 exit \ no last length!
              then
              index 1- 2* lengths + h@ to len \ last length
              2 s bits 3 + to symbol
            else
              symbol 17 = if \ repeat zero 3..10 times
                3 s bits 3 + to symbol
              else \ = 18, repeat zero 11.138 times
                7 s bits 11 + to symbol
              then
            then
            index symbol + nlen ndist + > if
              -1 exit \ too many lengths!
            then
            begin symbol while
              len index 2* lengths + h!
              -1 +to symbol
              1 +to index
            repeat
          then
        repeat

        \ check for end-of-block code -- there better be one!
        256 2* lengths + h@ 0= if
          -9 exit
        then

        \ build huffman table for literal/length codes
        nlen lengths codesb puff-lencode construct { err }
        err 0<>
        err 0<
        0 2* codesb puff-lencode puff-count @ + h@
        1 2* codesb puff-lencode puff-count @ + h@ + nlen <> or and if
          -7 exit \ incomplete code ok only for single length 1 code
        then

        \ build huffman table for distance codes
        ndist lengths nlen 2* + codesb puff-distcode construct to err
        err 0<>
        err 0<
        0 2* codesb puff-distcode puff-count @ + h@
        1 2* codesb puff-distcode puff-count @ + h@ + ndist <> or and if
          -8 exit \ incomplete code ok only for single length 1 code
        then

        \ decode data until end-of-block code
        codesb puff-distcode codesb puff-lencode s codes
      ;] with-aligned-allot
    ;] with-aligned-allot
  ;

  \ Inflate source to dest.  On return, destlen and sourcelen are updated to the
  \ size of the uncompressed data and the size of the deflate data respectively.
  \ On success, the return value of puff() is zero.  If there is an error in the
  \ source data, i.e. it is not in the deflate format, then a negative value is
  \ returned.  If there is not enough input available or there is not enough
  \ output space, then a positive error is returned.  In that case, destlen and
  \ sourcelen are not updated to facilitate retrying from the beginning with the
  \ provision of more input data or more output space.  In the case of invalid
  \ inflate data (a negative error), the dest and source pointers are updated to
  \ facilitate the debugging of deflators.
  \
  \ puff() also has a mode to determine the size of the uncompressed output with
  \ no output written.  For this dest must be (unsigned char *)0.  In this case,
  \ the input value of *destlen is ignored, and on return *destlen is set to the
  \ size of the uncompressed output.
  \
  \ The return codes are:
  \
  \   2:  available inflate data did not terminate
  \   1:  output space exhausted before completing inflate
  \   0:  successful inflate
  \  -1:  invalid block type (type == 3)
  \  -2:  stored block length did not match one's complement
  \  -3:  dynamic block code description: too many length or distance codes
  \  -4:  dynamic block code description: code lengths codes incomplete
  \  -5:  dynamic block code description: repeat lengths with no first length
  \  -6:  dynamic block code description: repeat more than specified lengths
  \  -7:  dynamic block code description: invalid literal/length code lengths
  \  -8:  dynamic block code description: invalid distance code lengths
  \  -9:  dynamic block code description: missing end-of-block code
  \ -10:  invalid literal/length or distance code in fixed or dynamic block
  \ -11:  distance is too far back in fixed or dynamic block
  \
  \ Format notes:
  \
  \ - Three bits are read for each block to determine the kind of block and
  \   whether or not it is the last block.  Then the block is decoded and the
  \   process repeated if it was not the last block.
  \
  \ - The leftover bits in the last byte of the deflate data after the last
  \   block (if it was a fixed or dynamic block) are undefined and have no
  \   expected values to check.
  : puff ( source sourcelen dest destlen -- sourcelen' destlen' err )
    puff-state-size [: { source sourcelen dest destlen s }
      \ initialize output state
      dest s puff-out !
      destlen s puff-outlen !
      0 s puff-outcnt !

      \ initialize input state
      source s puff-in !
      sourcelen s puff-inlen !
      0 s puff-incnt !
      0 s puff-bitbuf !
      0 s puff-bitcnt !

      s [: { s }
        0 { err }
        \ process blocks until last block or error
        begin
          1 s bits { last } \ one if last block
          2 s bits { type } \ block type 0..3
          type 0= if
            s stored
          else
            type 1 = if
              s fixed
            else
              type 2 = if
                s dynamic
              else
                -1 \ type = 3, invalid
              then
            then
          then to err
          err 0<> last 1 = or
        until

        \ update the lengths and return
        err 0<= if
          s puff-incnt @ s puff-outcnt @
        else
          s puff-inlen @ s puff-outlen @
        then
        err
      ;] try dup ['] x-out-of-bits = if
        \ bits or decode tried to read past available input
        2drop s puff-inlen @ s puff-outlen @ 2
      else
        ?raise
      then
    ;] with-aligned-allot
  ;

end-module
