\ Copyright (c) 2023 Travis Bemann
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

compile-to-flash

begin-module compat

  begin-module compat-internal

    \ Temporary buffer for WORD
    256 constant word-buffer-size
    word-buffer-size buffer: word-buffer    
    
  end-module> import

  \ Parse a token delimited by a given character; note that this is not
  \ reentrant because the returned counted string is stored in a single global
  \ buffer; for new code TOKEN / PARSE-NAME is recommended when possible. Also,
  \ this word does not properly handle all sorts of whitespace, such as tabs
  \ and values less than $20.
  : word ( delim "<delims>word<delim>" -- c-addr ) { delim }
    source { c-addr u }
    begin
      >in @ u < if
        c-addr >in @ + c@ delim = if
          1 >in +! false
        else
          true
        then
      else
        true
      then
    until
    delim internal::parse-to-char { c-addr' u' }
    u' word-buffer c!
    c-addr' word-buffer 1+ u' move
    word-buffer
  ;

  \ Parse text up to a given character; the the returned string is in the
  \ input buffer and thus avoids the reentrancy problems of WORD.
  : parse ( delim "text<delim>" -- c-addr u )
    internal::parse-to-char
  ;

  \ Find a word's xt using a counted string for its name and whether it is
  \ immediate (signaled by 1) or non-immediate (signaled by -1); return the name
  \ as a counted string and 0 if it is not found.
  : find ( c-addr -- c-addr 0 | xt 1 | xt -1 )
    dup count find { counted word }
    word if
      word >xt word internal::word-flags h@ immediate-flag and if 1 else -1 then
    else
      counted 0
    then
  ;

  \ Implement the traditional Forth string copying word CMOVE - for new code
  \ using MOVE is recommended.
  : cmove ( c-addr1 c-addr2 u -- ) internal::move> ;

  \ Implement the traditional Forth string copying word CMOVE> - for new code
  \ using MOVE is recommended.
  : cmove> ( c-add1 c-addr2 u -- ) internal::<move ;

  \ Determine whether a value is between 'low', inclusive, and 'high',
  \ exclusive.
  : within ( test low high -- flag ) over - >r - r> u< ;

  \ Parse a number in a string 'c-addr u' with an accumulator initialized as a
  \ double-cell value 'acc' using the base stored in BASE
  : >number { D: acc c-addr u -- acc' c-addr' u' }
    begin u while
      c-addr c@ to-upper-char { c }
      c [char] 0 >= c [char] 9 <= and if
        c [char] 0 - base @ < if
          acc base @ s>d d* c [char] 0 - s>d d+ to acc
          1 +to c-addr
          -1 +to u
        else
          acc c-addr u exit
        then
      else
        c [char] A >= c [char] Z <= and if
          c [char] A - base @ 10 - < if
            acc base @ s>d d* c [ char A 10 - ] literal - s>d d+ to acc
            1 +to c-addr
            -1 +to u
          else
            acc c-addr u exit
          then
        else
          acc c-addr u exit
        then
      then
    repeat
    acc c-addr u
  ;

  \ Compare two strings for both content and length using the numeric values of
  \ bytes compared within and shorter common length.
  : compare { c-addr1 u1 c-addr2 u2 -- n }
    begin u1 0<> u2 0<> and while
      c-addr1 c@ c-addr2 c@ { c1 c2 }
      c1 c2 < if
        -1 exit
      else
        c1 c2 > if
          1 exit
        then
      then
      1 +to c-addr1
      -1 +to u1
      1 +to c-addr2
      -1 +to u2
    repeat
    u1 u2 u< if
      -1
    else
      u1 u2 u> if
        1
      else
        0
      then
    then
  ;

  \ Fill a buffer with zero bytes.
  : erase ( c-addr u -- ) 0 fill ;

  \ Fill a buffer with spaces.
  : blank ( c-addr u -- ) bl fill ;

  \ Truncate the first n bytes of a string.
  : /string ( c-addr u n -- c-addr' u' ) tuck - -rot + swap ;

  \ Remove spaces at the end of a string.
  : -trailing { c-addr u -- c-addr' u' }
    begin u 0> while
      c-addr u 1- + c@ bl <> if
        c-addr u exit
      else
        -1 +to u
      then
    repeat
    c-addr 0
  ;

  \ Search a string from its start for a second string; if it is found, return
  \ the remainder of the first string starting from where the second string was
  \ found along with true; else return the whole first string and false.
  : search { c-addr1 u1 c-addr2 u2 -- c-addr3 u3 flag }
    c-addr1 u1 { current-addr current-len }
    begin current-len u2 >= while
      current-addr u2 c-addr2 u2 equal-strings? if
        current-addr current-len true exit
      else
        1 +to current-addr
        -1 +to current-len
      then
    repeat
    c-addr1 u1 false
  ;

  \ Compile a string literal.
  : sliteral ( compilation: c-addr1 u -- ) ( runtime: -- c-addr2 u )
    { c-addr1 u }
    [immediate]
    [compile-only]
    undefer-lit
    internal::reserve-branch { the-branch }
    here { start }
    c-addr1 u + c-addr1 ?do i c@ c, loop
    2 align,
    here the-branch internal::branch-back!
    start lit,
    u lit,
  ;

  \ Align the current HERE pointer to the next closest cell.
  : align ( -- ) cell align, ;

  \ Align an address to the next closest cell
  : aligned ( a-addr -- a-addr' ) cell align ;

  \ Increment an address by the size of one character, i.e. one byte.
  : char+ ( c-addr -- c-addr' ) 1+ ;

  \ Get the size of n characters in bytes; this is a no-nop.
  : chars ( n -- n' ) ;

  \ Parse a single token from the input.
  : parse-name ( "token" -- c-addr u ) token ;

  \ Output a right-justified signed value in a specified field width; note that
  \ if the value is wider than the specified field width the whole value will
  \ be output but no padding spaces will be added.
  : .r { n width -- }
    n 0< if n negate 1 else n 0 then { current bytes }
    begin
      1 +to bytes
      current base @ u/ to current
      current 0=
    until
    width bytes u> if width bytes - spaces then
    n (.)
  ;

  \ Output a right-justified unsigned value in a specified field width; note
  \ that if the value is wider than the specified field width the whole value
  \ will be output but no padding spaces will be added.
  : u.r { u width -- }
    u 0 { current bytes }
    begin
      1 +to bytes
      current base @ u/ to current
      current 0=
    until
    width bytes u> if width bytes - spaces then
    u (u.)
  ;

  \ Add multiple characters to <# # #> numeric formatting.
  : holds ( c-addr u -- )
    begin dup while 1- 2dup + c@ hold repeat 2drop
  ;

  \ Transfer N items and count to the return stack.
  : n>r ( xn .. x1 N -- ; R: -- x1 .. xn n )
    dup \ xn .. x1 N N -- 
    begin
      dup
    while
      rot r> swap >r >r \ xn .. N N -- ; R: .. x1 --
      1- \ xn .. N 'N -- ; R: .. x1 --
    repeat
    drop \ N -- ; R. x1 .. xn --
    r> swap >r >r
  ;

  \ Pull N items and count off the return stack.
  : nr> ( -- xn .. x1 N ; R: x1 .. xn N -- )
    r> r> swap >r dup
    begin
      dup
    while
      r> r> swap >r -rot
      1-
    repeat
    drop
  ;

  \ Invalid input specification exception.
  : x-invalid-input-spec ( -- ) cr ." invalid input specification" cr ;

  \ Save input specification.
  : save-input ( -- xn ... x1 n )
    internal::eval-data @
    internal::eval-refill @
    internal::eval-eof @
    internal::prompt-disabled @
    internal::eval-index-ptr @
    internal::eval-count-ptr @
    internal::eval-ptr @
    7
  ;

  \ Restore input specification.
  : restore-input ( xn ... x1 n -- )
    7 = averts x-invalid-input-spec
    internal::eval-ptr !
    internal::eval-count-ptr !
    internal::eval-index-ptr !
    internal::prompt-disabled !
    internal::eval-eof !
    internal::eval-refill !
    internal::eval-data !
  ;

  \ Refill the input buffer (and return whether EOF has not been reached)
  : refill ( -- flag )
    internal::eval-eof @ ?dup if execute if false exit then then
    display-prompt refill true
  ;

  \ Symmetric division (just a renaming of M/MOD)
  : sm/rem ( d n -- rem quot ) m/mod ;

  \ Floored division (I don't know why anyone would want this but supposedly
  \ having this is standard)
  : fm/mod ( d n -- rem quot )
    dup >r
    m/mod
    ( if the remainder is not zero and has a different sign than the divisor )
    over dup 0<> swap 0< r@ 0< xor and if
    1- swap r> + swap
    else
      rdrop
    then
  ;

  \ Get the amount of remaining dictionary space in the current task's RAM
  \ dictionary
  : unused ( -- u )
    task::current-task task::task-internal::task-dict-end ram-here -
  ;

  \ An exception which displays a message `aborted`.
  : x-abort ( -- ) ." aborted" cr ;
  
  \ An unknown exception, corresponding to exception numbers < 0 which do not
  \ have standard meanings.
  : x-unknown ( -- ) ." unknown exception" cr ;
  
  continue-module compat-internal

    \ Standard exception -7
    : x-std-7 ." do-loops nested too deeply during execution" cr ;
    
    \ Standard exception -8
    : x-std-8 ." dictionary overflow" cr ;
    
    \ Standard exception -9
    : x-std-9 ." invalid memory address" cr ;
    
    \ Standard exception -10
    : x-std-10 ." division by zero" cr ;
    
    \ Standard exception -11
    : x-std-11 ." result out of range" cr ;
    
    \ Standard exception -12
    : x-std-12 ." argument type mismatch" cr ;
    
    \ Standard exception -13
    : x-std-13 ." undefined word" cr ;
    
    \ Standard exception -14
    : x-std-14 ." interpreting a compile-only word" cr ;
    
    \ Standard exception -15
    : x-std-15 ." invalid FORGET" cr ;
    
    \ Standard exception -16
    : x-std-16 ." attempt to use zero-length string as a name" cr ;
    
    \ Standard exception -17
    : x-std-17 ." pictured numeric output string overflow" cr ;
    
    \ Standard exception -18
    : x-std-18 ." parsed string overflow" cr ;
    
    \ Standard exception -19
    : x-std-19 ." definition name too long" cr ;
    
    \ Standard exception -20
    : x-std-20 ." write to a read-only location" cr ;
    
    \ Standard exception -21
    : x-std-21 ." unsupported operation" cr ;
    
    \ Standard exception -22
    : x-std-22 ." control structure mismatch" cr ;
    
    \ Standard exception -23
    : x-std-23 ." address alignment exception" cr ;
    
    \ Standard exception -24
    : x-std-24 ." invalid numeric argument" cr ;
    
    \ Standard exception -25
    : x-std-25 ." return stack imbalance" cr ;
    
    \ Standard exception -26
    : x-std-26 ." loop parameters unavailable" cr ;
    
    \ Standard exception -27
    : x-std-27 ." invalid recursion" cr ;
    
    \ Standard exception -28
    : x-std-28 ." user interrupt" cr ;
    
    \ Standard exception -29
    : x-std-29 ." compiler nesting" cr ;
    
    \ Standard exception -30
    : x-std-30 ." obsolescent feature" cr ;
    
    \ Standard exception -31
    : x-std-31 ." >BODY used on non-CREATEd definition" cr ;
    
    \ Standard exception -32
    : x-std-32 ." invalid name argument (e.g., TO name)" cr ;
    
    \ Standard exception -33
    : x-std-33 ." block read exception" cr ;
    
    \ Standard exception -34
    : x-std-34 ." block write exception" cr ;
    
    \ Standard exception -35
    : x-std-35 ." invalid block number" cr ;
    
    \ Standard exception -36
    : x-std-36 ." invalid file position" cr ;
    
    \ Standard exception -37
    : x-std-37 ." file I/O exception" cr ;
    
    \ Standard exception -38
    : x-std-38 ." non-existent file" cr ;
    
    \ Standard exception -39
    : x-std-39 ." unexpected end of file" cr ;
    
    \ Standard exception -40
    : x-std-40 ." invalid BASE for floating point conversion" cr ;
    
    \ Standard exception -41
    : x-std-41 ." loss of precision" cr ;
    
    \ Standard exception -42
    : x-std-42 ." floating-point divide by zero" cr ;
    
    \ Standard exception -43
    : x-std-43 ." floating-point result out of range" cr ;
    
    \ Standard exception -44
    : x-std-44 ." floating-point stack overflow" cr ;
    
    \ Standard exception -45
    : x-std-45 ." floating-point stack underflow" cr ;
    
    \ Standard exception -46
    : x-std-46 ." floating-point invalid argument" cr ;
    
    \ Standard exception -47
    : x-std-47 ." compilation word list deleted" cr ;
    
    \ Standard exception -48
    : x-std-48 ." invalid POSTPONE" cr ;
    
    \ Standard exception -49
    : x-std-49 ." search-order overflow" cr ;
    
    \ Standard exception -50
    : x-std-50 ." search-order underflow" cr ;
    
    \ Standard exception -51
    : x-std-51 ." compilation word list changed" cr ;
    
    \ Standard exception -52
    : x-std-52 ." control-flow stack overflow" cr ;
    
    \ Standard exception -53
    : x-std-53 ." exception stack overflow" cr ;
    
    \ Standard exception -54
    : x-std-54 ." floating-point underflow" cr ;
    
    \ Standard exception -55
    : x-std-55 ." floating-point unidentified fault" cr ;
    
    \ Standard exception -56
    : x-std-56 ." QUIT" cr ;
    
    \ Standard exception -57
    : x-std-57 ." exception in sending or receiving a character" cr ;
    
    \ Standard exception -58
    : x-std-58 ." [IF], [ELSE], or [THEN] exception" cr ;
    
    \ Standard exception -59
    : x-std-59 ." ALLOCATE" cr ;
    
    \ Standard exception -60
    : x-std-60 ." FREE" cr ;
    
    \ Standard exception -61
    : x-std-61 ." RESIZE" cr ;
    
    \ Standard exception -62
    : x-std-62 ." CLOSE-FILE" cr ;
    
    \ Standard exception -63
    : x-std-63 ." CREATE-FILE" cr ;
    
    \ Standard exception -64
    : x-std-64 ." DELETE-FILE" cr ;
    
    \ Standard exception -65
    : x-std-65 ." FILE-POSITION" cr ;
    
    \ Standard exception -66
    : x-std-66 ." FILE-SIZE" cr ;
    
    \ Standard exception -67
    : x-std-67 ." FILE-STATUS" cr ;
    
    \ Standard exception -68
    : x-std-68 ." FLUSH-FILE" cr ;
    
    \ Standard exception -69
    : x-std-69 ." OPEN-FILE" cr ;
    
    \ Standard exception -70
    : x-std-70 ." READ-FILE" cr ;
    
    \ Standard exception -71
    : x-std-71 ." READ-LINE" cr ;
    
    \ Standard exception -72
    : x-std-72 ." RENAME-FILE" cr ;
    
    \ Standard exception -73
    : x-std-73 ." REPOSITION-FILE" cr ;
    
    \ Standard exception -74
    : x-std-74 ." RESIZE-FILE" cr ;
    
    \ Standard exception -75
    : x-std-75 ." WRITE-FILE" cr ;
    
    \ Standard exception -76
    : x-std-76 ." WRITE-LINE" cr ;
    
    \ Standard exception -77
    : x-std-77 ." Malformed xchar" cr ;
    
    \ Standard exception -78
    : x-std-78 ." SUBSTITUTE" cr ;
    
    \ Standard exception -79
    : x-std-79 ." REPLACES" cr ;

    \ Standard exception count
    80 constant std-except-count
    
    create std-excepts
    0 ,
    ' x-abort ,
    ' x-abort ,
    ' stack-overflow ,
    ' stack-underflow ,
    ' rstack-overflow ,
    ' rstack-underflow ,
    ' x-std-7 ,
    ' x-std-8 ,
    ' x-std-9 ,
    ' x-std-10 ,
    ' x-std-11 ,
    ' x-std-12 ,
    ' x-std-13 ,
    ' x-std-14 ,
    ' x-std-15 ,
    ' x-std-16 ,
    ' x-std-17 ,
    ' x-std-18 ,
    ' x-std-19 ,
    ' x-std-20 ,
    ' x-std-21 ,
    ' x-std-22 ,
    ' x-std-23 ,
    ' x-std-24 ,
    ' x-std-25 ,
    ' x-std-26 ,
    ' x-std-27 ,
    ' x-std-28 ,
    ' x-std-29 ,
    ' x-std-30 ,
    ' x-std-31 ,
    ' x-std-32 ,
    ' x-std-33 ,
    ' x-std-34 ,
    ' x-std-35 ,
    ' x-std-36 ,
    ' x-std-37 ,
    ' x-std-38 ,
    ' x-std-39 ,
    ' x-std-40 ,
    ' x-std-41 ,
    ' x-std-42 ,
    ' x-std-43 ,
    ' x-std-44 ,
    ' x-std-45 ,
    ' x-std-46 ,
    ' x-std-47 ,
    ' x-std-48 ,
    ' x-std-49 ,
    ' x-std-50 ,
    ' x-std-51 ,
    ' x-std-52 ,
    ' x-std-53 ,
    ' x-std-54 ,
    ' x-std-55 ,
    ' x-std-56 ,
    ' x-std-57 ,
    ' x-std-58 ,
    ' x-std-59 ,
    ' x-std-60 ,
    ' x-std-61 ,
    ' x-std-62 ,
    ' x-std-63 ,
    ' x-std-64 ,
    ' x-std-65 ,
    ' x-std-66 ,
    ' x-std-67 ,
    ' x-std-68 ,
    ' x-std-69 ,
    ' x-std-70 ,
    ' x-std-71 ,
    ' x-std-72 ,
    ' x-std-73 ,
    ' x-std-74 ,
    ' x-std-75 ,
    ' x-std-76 ,
    ' x-std-77 ,
    ' x-std-78 ,
    ' x-std-79 ,

    \ Convert an exception
    : convert-except ( std-except -- except )
      dup negate dup 0>= swap std-except-count < and if
        negate cells std-excepts + @
      else
        dup 0< if
          drop ['] x-unknown
        then
      then
    ;
    
  end-module
  
  \ Raise an exception X-ABORT.
  : abort ( -- ) ['] x-abort ?raise ;

  \ Raise an exception that displays a message and a following newline if the
  \ value on the stack at runtime is non-zero.
  : abort" ( "message" -- ) ( Runtime: flag -- )
    [immediate]
    postpone if
    postpone [:
    postpone ."
    postpone cr
    postpone ;]
    postpone ?raise
    postpone then
  ;

  \ Catch an exception; a synonym for TRY.
  : catch ( xt -- except|0 ) try ;

  \ Throw an exception, converting standard exceptions to zeptoforth exceptions.
  \ Note that -2 is not handled in a standard way because there is no fixed
  \ message buffer for ABORT" .
  : throw ( except -- ) convert-except ?raise ;
  
  \ Fetch a value from an address and print it as an integer.
  : ? ( addr -- ) @ . ;

  \ Duplicate the first entry on the search order.
  : also ( -- )
    get-order over swap 1+ set-order
  ; 

  \ Make the compilation wordlist the same as the first entry on the search
  \ order.
  : definitions ( -- )
    get-order swap set-current 0 ?do drop loop
  ;

  \ A synonym for zeptoforth FORTH.
  forth constant forth-wordlist

  continue-module compat-internal

    \ Create a word which takes the topmost wordlist and replaces it with a
    \ given wordlist.
    : (wordlist) ( wid "<name>" -- ; )
      <builds ,
      does>
      @ >r
      get-order nip
      r> swap set-order
    ;

  end-module

  \ Set the topmost wordlist with the FORTH wordlist.
  forth-wordlist (wordlist) forth

  \ Set the wordlist order to the minimum default wordlist order.
  : only ( -- ) -1 set-order ;

  \ Display the searchlist order.
  : order ( -- )
    cr ." current compilation wordlist: " get-current .
    cr ." current wordlist order:"
    get-order 0 ?do cr i (.) ." : " . loop
  ;

  \ Remove the topmost entry of the wordlist order.
  : previous ( -- ) get-order ?dup if nip 1- set-order then ;

  \ Find a word in a wordlist's xt using a string for its name and whether it is
  \ immediate (signaled by 1) or non-immediate (signaled by -1); return 0 if it
  \ is not found. Unlike ANS FIND it does not used a counted string and does
  \ not return the string being searched for if no word is found.
  : search-wordlist ( c-addr u wid -- 0 | xt 1 | xt -1 )
    internal::find-in-wordlist { word }
    word if
      word >xt word internal::word-flags h@ immediate-flag and if 1 else -1 then
    else
      0
    then
  ;
  
end-module

compile-to-ram
