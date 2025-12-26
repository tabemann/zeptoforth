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

\ This code is based closely off of https://github.com/amosnier/sha-2 which in
\ turn is closely based off https://en.wikipedia.org/wiki/SHA-2

begin-module sha-256-accel

  lock import
  
  \ Size of an SHA-256 hash in bytes
  32 constant size-of-sha-256-hash

  begin-module sha-256-accel-internal

    \ SHA-256 lock
    lock-size buffer: sha-256-accel-lock

    \ Initialize the SHA-256 accelerator lock
    : init-sha-256-accel-lock ( -- )
      sha-256-accel-lock init-lock
    ;
    initializer init-sha-256-accel-lock
    
    \ Size of an SHA-256 chunk in bytes
    64 constant size-of-sha-256-chunk

    \ Size of the total length of a SHA-256 data in bytes
    8 constant total-len-len

    \ SHA-256 accelerator peripheral base address
    $400F8000 constant SHA256_BASE

    \ SHA-256 accelerator control/status register address
    SHA256_BASE $00 + constant SHA256_CSR

    \ Sum for SHA-256 accelerator is valid
    2 bit constant SHA256_CSR_SUM_VLD
    
    \ SHA-256 accelerator is ready for a word of input
    1 bit constant SHA256_CSR_WDATA_RDY

    \ Set to initialize the SHA-256 accelerator
    0 bit constant SHA256_CSR_START

    \ SHA-256 accelerator data write register address
    SHA256_BASE $04 + constant SHA256_WDATA

    \ SHA-256 accelerator first output cell register address
    SHA256_BASE $08 + constant SHA256_SUM0
    
    \ SHA-256 accelerator structure
    begin-structure sha-256-accel-size
      field: sha-256-hash
      size-of-sha-256-chunk +field sha-256-chunk
      field: sha-256-chunk-pos
      2field: sha-256-space-left
      2field: sha-256-total-len
    end-structure

    \ Update a hash value under calculation with a new chunk of data
    \
    \ p: address of the chunk data, which has a standard length
    \
    \ This is the SHA-256 work horse.
    : consume-chunk ( p -- )
      size-of-sha-256-chunk 0 do
        begin SHA256_CSR_WDATA_RDY SHA256_CSR bit@ until
        dup i + @ SHA256_WDATA !
      cell +loop
      drop
    ;

    \ Inner workings of initializing SHA-256 accelerator; this does not lock
    \ the SHA-256 accelerator peripheral
    : actually-init-sha-256-accel { hash sha-256 -- }
      hash sha-256 sha-256-hash !
      sha-256 sha-256-chunk sha-256 sha-256-chunk-pos !
      size-of-sha-256-chunk sha-256 sha-256-space-left !
      0. sha-256 sha-256-total-len 2!
      SHA256_CSR_START SHA256_CSR bis!
    ;

    \ Inner workings of closing the SHA-256 accelerator; this does not lock
    \ the SHA-256 accelerator peripheral
    : actually-close-sha-256-accel { sha-256 -- hash }
      sha-256 sha-256-chunk-pos @ { pos }
      sha-256 sha-256-space-left @ { space-left }

      \ The current chunk cannot be full. Otherwise, it would already have been
      \ consumed. I.e. there is space left for at least one byte. The next step
      \ in the calculation is to add a single one-bit to the data.
      $80 pos c!
      1 +to pos
      -1 +to space-left

      \ Now, the last step is to add the total data length at the end of the
      \ last chunk, and zero padding before that. But we do not necessary have
      \ enough space left. If not, we pad the current chunk with zeroes, and
      \ add an extra chunk at the end.
      space-left total-len-len < if
        pos space-left 0 fill
        sha-256 sha-256-chunk consume-chunk
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
      sha-256 sha-256-chunk consume-chunk

      \ Produce the final hash value
      begin SHA256_CSR_SUM_VLD SHA256_CSR bit@ until
      0 { x }
      sha-256 sha-256-hash @ { hash }
      8 0 do
        i cells SHA256_SUM0 + @ { hh }
        hh 24 rshift $FF and x hash + c! 1 +to x
        hh 16 rshift $FF and x hash + c! 1 +to x
        hh 8 rshift $FF and x hash + c! 1 +to x
        hh $FF and x hash + c! 1 +to x
      loop
    
      hash
    ;
    
  end-module> import

  \ Size of SHA-256 accelerator structure in bytes
  sha-256-accel-size constant sha-256-accel-size

  \ Initialize an SHA-256 accelerator streaming calculation.
  \
  \ hash: address of hash array, where the result is delivered
  \ sha-256: address of an SHA-256 accelerator structure
  \
  \ If all the data you are calculating the hash value on is not available in a
  \ contiguous buffer in memory, this is where you should start. Instantiate a
  \ SHA-256 structure, and invoke this word. Once a SHA-256 hash has been
  \ calculated the SHA-256 accelerator structure can be initialized again for
  \ the next calculation.
  : init-sha-256-accel ( hash sha-256 -- )
    sha-256-accel-lock claim-lock
    actually-init-sha-256-accel
  ;

  \ Stream more input for an on-going SHA-256 accelerator calculation.
  \
  \ data: address of data to be added to the calculation
  \ len: length of the data to add, in bytes
  \ sha-256: address of a previously initialized SHA-256 accelerator structure
  \
  \ This word may be invoked an arbitrary number of times between initialization
  \ and closing, but the data length is limited by the SHA-256 algorithm: the
  \ total number of bits (i.e. the total number of bytes times eight) must be
  \ representable by a 64-bit unsigned integer. While that is not a practical
  \ limitation, the result is unpredictable if that limit is exceeded.
  \
  \ This word may be invoked on empty data (zero length), although that
  \ obviously will not add any data.
  : write-sha-256-accel { data len sha-256 -- }
    len 0 sha-256 sha-256-total-len 2+!

    begin len 0> while
      \ If the input chunks have sizes that are multiples of the calculation
      \ chunk size, no copies are necessary. We operate directly on the input
      \ data instead.
      sha-256 sha-256-space-left @ size-of-sha-256-chunk =
      len size-of-sha-256-chunk >= and if
        data consume-chunk
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
          sha-256 sha-256-chunk consume-chunk
          sha-256 sha-256-chunk sha-256 sha-256-chunk-pos !
          size-of-sha-256-chunk sha-256 sha-256-space-left !
        else
          consumed-len sha-256 sha-256-chunk-pos +!
        then
      then
    repeat
  ;

  \ Conclude an SHA-256 accelerator streaming calculation, making the hash
  \ value available.
  \
  \ sha-256: address of a previously initialized SHA-256 accelerator structure
  \
  \ After this word has been invoked, the result is available in the hash buffer
  \ that initially was provided. The address of the hash value is returned for
  \ convenience, but you should feel free to drop it; it is simply the address
  \ of the first byte of your initially provided hash array.
  \
  \ Invoking this word for a calculation with no data (the writing function has
  \ never been invoked, or it only has been invoked with empty data) is legal.
  \ It will calculate the SHA-256 value of the empty string.
  : close-sha-256-accel { sha-256 -- hash }
    sha-256 actually-close-sha-256-accel
    sha-256-accel-lock release-lock
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
  : calc-sha-256-accel ( input len hash -- )
    sha-256-accel-size [:
      [: { input len hash sha-256 }
        hash sha-256 actually-init-sha-256-accel
        input len sha-256 write-sha-256-accel
        sha-256 actually-close-sha-256-accel drop
      ;] sha-256-accel-lock with-lock
    ;] with-aligned-allot
  ;

end-module
