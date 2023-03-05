\ Copyright (c) 2022-2023 Travis Bemann
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

begin-module core-lock

  task import
  multicore import

  begin-module core-lock-internal
  
    begin-structure core-lock-size
      
      \ The claiming core plus one
      field: core-lock-index

      \ The claiming core count
      field: core-lock-count

    end-structure
    
  end-module> import

  ' core-lock-size export core-lock-size

  \ Initialize a core lock
  : init-core-lock ( addr -- )
    0 over core-lock-index !
    0 swap core-lock-count !
  ;
  
  \ Claim a core lock
  : claim-core-lock ( core-lock -- )
    begin
      disable-int
      cpu-index 1+ over core-lock-index @ <> if
        cpu-index 1+ over core-lock-index test-set-raw dup not if
          enable-int
          pause
        then
      else
        1 over core-lock-count +!
        enable-int
        true
      then
    until
    drop
  ;

  \ Claim a core lock while spinning
  : claim-core-lock-spin ( core-lock -- )
    begin
      disable-int
      cpu-index 1+ over core-lock-index @ <> if
        cpu-index 1+ over core-lock-index test-set-raw
        enable-int
      else
        1 over core-lock-count +!
        enable-int
        true
      then
    until
    drop
  ;
  
  \ Claim a core lock with a timeout
  : claim-core-lock-timeout ( core-lock -- )
    begin
      current-task compare-timeout if
        ['] x-timed-out ?raise
      else
        disable-int
        cpu-index 1+ over core-lock-index @ <> if
          cpu-index 1+ over core-lock-index test-set-raw dup not if
            enable-int
            pause
          then
        else
          1 over core-lock-count +!
          enable-int
          true
        then
      then
    until
    drop
  ;

  \ Release a core lock
  : release-core-lock ( core-lock -- )
    disable-int
    -1 over core-lock-count +!
    dup core-lock-count @ 0< if
      0 over core-lock-count !
      0 over core-lock-index !
    then
    drop
    enable-int
  ;

  \ Claim and release a core lock while properly handling exceptions
  : with-core-lock ( xt core-lock -- )
    >r r@ claim-core-lock try r> release-core-lock ?raise
  ;

  \ Claim and release a core lock with spinning while properly handling
  \ exceptions
  : with-core-lock-spin ( xt core-lock -- )
    >r r@ claim-core-lock-spin try r> release-core-lock ?raise
  ;

end-module

reboot