\ Copyright (c) 2026 Travis Bemann
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

begin-module pio-pool

  pio import
  pio-internal import

  \ Invalid number of PIO blocks to allocate
  : x-invalid-sm-count ." invalid PIO state machine count" cr ;

  \ Unable to allocate PIO/state machine
  : x-unable-allocate-pio/sm ." unable to allocate PIO/state machines" cr ;
  
  begin-module pio-pool-internal

    lock import

    \ The PIO lock
    lock-size buffer: pio-lock

    \ Initialize the PIO lock hook
    : init-pio-lock ( -- )
      pio-lock init-lock
      [: pio-lock with-lock ;] pio-lock-hook !
    ;
    initializer init-pio-lock

    \ The PIO/state machine bitmap
    0 value pio/sm-bitmap

    \ Convert a PIO block to a PIO index
    rp2350? [if]
      : pio>index ( pio -- index )
        dup validate-pio
        case
          PIO0 of 0 endof
          PIO1 of 1 endof
          PIO2 of 2 endof
        endcase
      ;
    [else]
      : pio>index ( pio -- index )
        dup validate-pio
        case
          PIO0 of 0 endof
          PIO1 of 1 endof
        endcase
      ;
    [then]
    
    \ Check whether a given number of state machines are available for a PIO
    : pio/sm-available? { sm-count pio -- available? }
      pio pio>index 4 * dup 4 + swap do
        i bit pio/sm-bitmap and 0= if -1 +to sm-count then
      loop
      sm-count 0<=
    ;

    \ Allocate state machines for a PIO block
    : set-pio-sms { sm-count pio -- smn ... sm0 }
      pio pio>index 4 * dup 3 + do
        sm-count 0> if
          i bit pio/sm-bitmap and 0= if
            pio/sm-bitmap i bit or to pio/sm-bitmap
            i pio pio>index 4 * -
            -1 +to sm-count
          then
        then
      -1 +loop
    ;
    
    \ Search for a PIO block to use
    : search-pio { sm-count -- pio }
      sm-count PIO0 pio/sm-available? if PIO0 exit then
      sm-count PIO1 pio/sm-available? if PIO1 exit then
      [ rp2350? ] [if] sm-count PIO2 pio/sm-available? if PIO2 exit then [then]
      true triggers x-unable-allocate-pio/sm
    ;

    \ Search for a PIO block to use, and if found reserve space for a program
    : search-pio-alloc-piomem { size sm-count -- base pio }
      sm-count PIO0 pio/sm-available? if
        size [: PIO0 swap alloc-piomem ;] try ?dup if
          dup ['] x-pio-no-room = if 2drop 0 then ?raise
        else
          PIO0 exit
        then
      then
      sm-count PIO1 pio/sm-available? if
        size [: PIO1 swap alloc-piomem ;] try ?dup if
          dup ['] x-pio-no-room = if 2drop 0 then ?raise
        else
          PIO1 exit
        then
      then
      [ rp2350? ] [if]
        sm-count PIO2 pio/sm-available? if
          size [: PIO2 swap alloc-piomem ;] try ?dup if
            dup ['] x-pio-no-room = if 2drop 0 then ?raise
          else
            PIO2 exit
          then
        then
      [then]
      true triggers x-unable-allocate-pio/sm
    ;

    \ Validate PIO state machine count
    : validate-sm-count ( sm-count -- )
      dup 1 >= averts x-invalid-sm-count
      4 <= averts x-invalid-sm-count
    ;
    
  end-module> import

  \ Allocate state machines for some PIO block
  : allocate-pio-sms ( sm-count -- smn ... sm0 pio )
    dup validate-sm-count
    [: { sm-count }
      sm-count search-pio { pio }
      sm-count pio set-pio-sms
      pio
    ;] with-pio-lock
  ;

  \ Allocate state machines for some PIO block and allocate PIO memory
  : allocate-pio-sms-w-piomem ( size sm-count -- smn ... sm0 base pio )
    dup validate-sm-count
    [: { size sm-count }
      size sm-count search-pio-alloc-piomem { base pio }
      sm-count pio set-pio-sms
      base pio
    ;] with-pio-lock
  ;

  \ Allocate state machines for some PIO block and set up a program
  : allocate-pio-sms-w-prog ( program sm-count -- smn ... sm0 base pio )
    dup validate-sm-count
    [: { program sm-count }
      program p-size sm-count search-pio-alloc-piomem { base pio }
      sm-count pio set-pio-sms
      program p-prog base pio pio-instr-relocate-mem!
      sm-count 0 ?do
        i pick { sm }
        program p-transfer sm pio sm-addr!
        program p-wrap sm pio sm-wrap!
      loop
      base pio
    ;] with-pio-lock
  ;

  \ Free a PIO state machine
  : free-pio-sm ( sm pio -- )
    [:
      2dup validate-sm-pio
      pio>index 4 * + bit pio/sm-bitmap swap bic to pio/sm-bitmap
    ;] with-pio-lock
  ;
  
end-module

reboot