\ Copyright (c) 2025 Travis Bemann
\ 
\ Permission is hereby granted, free of charge, to any person obtaining a copy
\ of this software and associated documentation files (the "Software"), to deal
\ in the Software without restriction, including without limitation the rights
\ to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
\ copies of the Software, and to permit persons to whom the Software is
\ furnished to do so, subject to the following conditions:
\ 
\ The above copyright notice and this permission notice shall be included in
\ all copies or substantial portions of the Software.
\ 
\ THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
\ IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
\ FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
\ AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
\ LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
\ OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
\ SOFTWARE.

\ This code is based closely off of https://github.com/amonsier/sha-2 which in
\ turn is closely based off https://en.wikipedia.org/wiki/SHA-2

begin-module sha-256

  \ Size of an SHA-256 hash in bytes
  32 constant size-of-sha-256-hash

  begin-module sha-256-internal

    \ Size of an SHA-256 chunk in bytes
    64 constant size-of-sha-256-chunk

    \ Size of the total length of a SHA-256 data in bytes
    8 constant total-len-len

    \ SHA-256 structure
    begin-structure sha-256-size
      field: sha-256-hash
      size-of-sha-256-chunk +field sha-256-chunk
      field: sha-256-chunk-pos
      2field: sha-256-space-left
      2field: sha-256-total-len
      8 cells +field sha-256-h
    end-structure

    \ Array of round constants
    \ (first 32 bits of the fractional parts of the cube roots of the first 64
    \ primes 2..311)
    create k
    $428a2f98 , $71374491 , $b5c0fbcf , $e9b5dba5 ,
    $3956c25b , $59f111f1 , $923f82a4 , $ab1c5ed5 ,
    $d807aa98 , $12835b01 , $243185be , $550c7dc3 ,
    $72be5d74 , $80deb1fe , $9bdc06a7 , $c19bf174 ,
    $e49b69c1 , $efbe4786 , $0fc19dc6 , $240ca1cc ,
    $2de92c6f , $4a7484aa , $5cb0a9dc , $76f988da ,
    $983e5152 , $a831c66d , $b00327c8 , $bf597fc7 ,
    $c6e00bf3 , $d5a79147 , $06ca6351 , $14292967 ,
    $27b70a85 , $2e1b2138 , $4d2c6dfc , $53380d13 ,
    $650a7354 , $766a0abb , $81c2c92e , $92722c85 ,
    $a2bfe8a1 , $a81a664b , $c24b8b70 , $c76c51a3 ,
    $d192e819 , $d6990624 , $f40e3585 , $106aa070 ,
    $19a4c116 , $1e376c08 , $2748774c , $34b0bcb5 ,
    $391c0cb3 , $4ed8aa4a , $5b9cca4f , $682e6ff3 ,
    $748f82ee , $78a5636f , $84c87814 , $8cc70208 ,
    $90befffa , $a4506ceb , $bef9a3f7 , $c67178f2 ,

    \ Rotate a 32-bit value by a number of bits to the right
    \
    \ val: the value to be rotated
    \ count: the number of bits to rotate by
    \
    \ Returns the rotated value
    : right-rot { val count -- } val count rshift val 32 count - lshift or ;

    \ First part of consuming a chunk
    : consume-chunk-1 { h p w j i -- p' }
      j 0= if
        p c@ 24 lshift
        p 1+ c@ 16 lshift or
        p 2 + c@ 8 lshift or
        p 3 + c@ or
        w i cells + !
        4 +to p
      else
        \ Extend the first 16 cells into the remaining 48 cells
        \ w[16.63] of the message schedule array
        i 1+ $F and cells w + @ { w0 }
        w0 7 right-rot
        w0 18 right-rot xor
        w0 3 rshift xor { s0 }
        i 14 + $F and cells w + @ { w1 }
        w1 17 right-rot
        w1 19 right-rot xor
        w1 10 rshift xor { s1 }
        i cells w + @ s0 + i 9 + $F and cells w + @ + s1 +
        i cells w + !
      then
      p
    ;
            
    \ Update a hash value under calculation with a new chunk of data
    \
    \ h: address of the first hash item, of a total of eight
    \ p: address of the chunk data, which has a standard length
    \
    \ This is the SHA-256 work horse.
    : consume-chunk ( h p -- )
      8 cells [:
        
        \ The w-array is really 64 cells, but since we only need 16 of them
        \ at a time, we save space by calculating 16 at a time.
        \
        \ This optimization was not there initially and the rest of the
        \ comments about w[64] are kept in their initial state

        \ Create a 64-entry message schedule array w[0..63] of 32-bit cells
        \ (The initial values in w[0..63] don't matter, so many implementions
        \ zero them here) copy chunk into the first 16 cells w[0..15] of the
        \ message schedule array.
        
        16 cells [: { h p ah w }
          
          \ Initialize working variables to current hash value
          h ah 8 cells move
          
          \ Compression function main loop
          4 0 do
            16 0 do
              h p w j i consume-chunk-1 to p
              
              4 cells ah + @ { ah4 }
              ah4 6 right-rot ah4 11 right-rot xor ah4 25 right-rot xor { s1 }

              ah4 5 cells ah + @ and ah4 not 6 cells ah + @ and xor { ch }
              
              7 cells ah + @ s1 + ch + j 4 lshift i or cells k + @ +
              i cells w + @ + { temp1 }
              ah @ 2 right-rot ah @ 13 right-rot xor ah @ 22 right-rot xor
              { s0 }
              cell ah + @ { ah1 }
              2 cells ah + @ { ah2 }
              ah @ ah1 and ah @ ah2 and xor ah1 ah2 and xor { maj }
              s0 maj + { temp2 }

              6 cells ah + @ 7 cells ah + !
              5 cells ah + @ 6 cells ah + !
              4 cells ah + @ 5 cells ah + !
              3 cells ah + @ temp1 + 4 cells ah + !
              2 cells ah + @ 3 cells ah + !
              1 cells ah + @ 2 cells ah + !
              ah @ 1 cells ah + !
              temp1 temp2 + ah !

            loop
          loop

          \ Add the compressed chunk to the current hash value
          8 0 do i cells ah + @ i cells h + +! loop
          
        ;] with-aligned-allot
      ;] with-aligned-allot
    ;

  end-module> import

  \ Size of SHA-256 structure in bytes
  sha-256-size constant sha-256-size

  \ Initialize an SHA-256 streaming calculation.
  \
  \ hash: address of hash array, where the result is delivered
  \ sha-256: address of an SHA-256 structure
  \
  \ If all the data you are calculating the hash value on is not available in a
  \ contiguous buffer in memory, this is where you should start. Instantiate a
  \ SHA-256 structure, and invoke this word. Once a SHA-256 hash has been
  \ calculated the SHA-256 structure can be initialized again for the next
  \ calculation.
  : init-sha-256 { hash sha-256 -- }
    hash sha-256 sha-256-hash !
    sha-256 sha-256-chunk sha-256 sha-256-chunk-pos !
    size-of-sha-256-chunk sha-256 sha-256-space-left !
    0. sha-256 sha-256-total-len 2!

    \ Initialize hash values (first 32 bits of the fractional parts of the
    \ share roots of the first 8 primes 2..19)
    $6a09e667 sha-256 sha-256-h !
    $bb67ae85 sha-256 sha-256-h 1 cells + !
    $3c6ef372 sha-256 sha-256-h 2 cells + !
    $a54ff53a sha-256 sha-256-h 3 cells + !
    $510e527f sha-256 sha-256-h 4 cells + !
    $9b05688c sha-256 sha-256-h 5 cells + !
    $1f83d9ab sha-256 sha-256-h 6 cells + !
    $5be0cd19 sha-256 sha-256-h 7 cells + !
  ;

  \ Stream more input for an on-going SHA-256 calculation.
  \
  \ data: address of data to be added to the calculation
  \ len: length of the data to add, in bytes
  \ sha-256: address of a previously initialized SHA-256 structure
  \
  \ This word may be invoked an arbitrary number of times between initialization
  \ and closing, but the data length is limited by the SHA-256 algorithm: the
  \ total number of bits (i.e. the total number of bytes times eight) must be
  \ representable by a 64-bit unsigned integer. While that is not a practical
  \ limitation, the result is unpredictable if that limit is exceeded.
  \
  \ This word may be invoked on empty data (zero length), although that
  \ obviously will not add any data.
  : write-sha-256 { data len sha-256 -- }
    len 0 sha-256 sha-256-total-len 2+!

    begin len 0> while
      \ If the input chunks have sizes that are multiples of the calculation
      \ chunk size, no copies are necessary. We operate directly on the input
      \ data instead.
      sha-256 sha-256-space-left @ size-of-sha-256-chunk =
      len size-of-sha-256-chunk >= and if
        sha-256 sha-256-h data consume-chunk
        size-of-sha-256-chunk negate +to len
        size-of-sha-256-chunk +to data
      else
        \ General case, no particular optimization
        len sha-256 sha-256-space-left @ < if
          len
        else
          sha-256 sha-256-space-left @
        then { consumed-len }
        data sha-256 sha-256-chunk-pos @ consumed-len move
        consumed-len negate sha-256 sha-256-space-left +!
        consumed-len negate +to len
        consumed-len +to data
        sha-256 sha-256-space-left @ 0= if
          sha-256 sha-256-h sha-256 sha-256-chunk consume-chunk
          sha-256 sha-256-chunk sha-256 sha-256-chunk-pos !
          size-of-sha-256-chunk sha-256 sha-256-space-left !
        else
          consumed-len sha-256 sha-256-chunk-pos +!
        then
      then
    repeat
  ;

  \ Conclude an SHA-256 streaming calculation, making the hash value available.
  \
  \ sha-256: address of a previously initialized SHA-256 structure
  \
  \ After this word has been invoked, the result is available in the hash buffer
  \ that initially was provided. The address of the hash value is returned for
  \ convenience, but you should feel free to drop it; it is simply the address
  \ of the first byte of your initially provided hash array.
  \
  \ Invoking this word for a calculation with no data (the writing function has
  \ never been invoked, or it only has been invoked with empty data) is legal.
  \ It will calculate the SHA-256 value of the empty string.
  : close-sha-256 { sha-256 -- }
    sha-256 sha-256-chunk-pos @ { pos }
    sha-256 sha-256-space-left @ { space-left }
    sha-256 sha-256-h { h }

    \ The current chunk cannot be full. Otherwise, it would already have been
    \ consumed. I.e. there is space left for at least one byte. The next step
    \ in the calculation is to add a single one-bit to the data.
    $80 pos c!
    1 +to pos
    -1 +to space-left

    \ Now, the last step is to add the total data length at the end of the last
    \ chunk, and zero padding before that. But we do not necessary have enough
    \ space left. If not, we pad the current chunk with zeroes, and add an
    \ extra chunk at the end.
    space-left total-len-len < if
      pos space-left 0 fill
      h sha-256 sha-256-chunk consume-chunk
      sha-256 sha-256-chunk to pos
      size-of-sha-256-chunk to space-left
    then
    space-left total-len-len - { left }
    pos left 0 fill
    left +to pos
    sha-256 sha-256-total-len 2@ { D: len }
    len drop 3 lshift $FF and pos 7 + c!
    len 5 2rshift to len
    0 6 do
      len drop $FF and pos i + c!
      len 8 2rshift to len
    -1 +loop
    h sha-256 sha-256-chunk consume-chunk

    \ Produce the final hash value (big-endian)
    0 { x }
    sha-256 sha-256-hash @ { hash }
    8 0 do
      i cells h + @ { hh }
      hh 24 rshift $FF and x hash + c! 1 +to x
      hh 16 rshift $FF and x hash + c! 1 +to x
      hh 8 rshift $FF and x hash + c! 1 +to x
      hh $FF and x hash + c! 1 +to x
    loop

    hash
  ;
  
  \ The simple SHA-256 calculation word
  \ 
  \ input: address of the data the hash will be calculated on
  \ len: length of the input data, in bytes
  \ hash: address of hash array, where the result is delivered
  \ 
  \ If all the data you are calculating the hash value on is available in a
  \ contiguous buffer in memory, this is the word you should use.
  \
  \ The length must be correspond to an unsigned 64-bit number of bits (i.e.
  \ the length times eight must be less than 2^64).
  : calc-sha-256 ( input len hash -- )
    sha-256-size [: { input len hash sha-256 }
      hash sha-256 init-sha-256
      input len sha-256 write-sha-256
      sha-256 close-sha-256 drop
    ;] with-aligned-allot
  ;

end-module
