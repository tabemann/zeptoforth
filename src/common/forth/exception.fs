\ Copyright (c) 2022 Travis Bemann
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

begin-module exception

  interrupt import
  internal import
  armv6m import

  \ The exception vector indices
  3 constant hard-fault-vector
  4 constant mem-fault-vector
  5 constant bus-fault-vector
  6 constant usage-fault-vector

  \ The CFSR and HFSR registers
  $E000ED28 constant CFSR
  $E000ED2C constant HFSR

  \ Saved hooks
  variable saved-emit-hook
  variable saved-emit?-hook
  variable saved-key-hook
  variable saved-key?-hook
  variable saved-pause-hook
  
  \ Prepare a faulted state
  : prepare-faulted-state ( -- )
    emit-hook @ saved-emit-hook !
    emit?-hook @ saved-emit?-hook !
    key-hook @ saved-key-hook !
    key?-hook @ saved-key?-hook !
    pause-hook @ saved-pause-hook !
    ['] serial-emit emit-hook !
    ['] serial-emit? emit?-hook !
    ['] serial-key key-hook !
    ['] serial-key? key?-hook !
    [: ;] pause-hook !
  ;

  \ Restore hooks
  : restore-hooks ( -- )
    saved-emit-hook @ emit-hook !
    saved-emit?-hook @ emit?-hook !
    saved-key-hook @ key-hook !
    saved-key?-hook @ key?-hook !
    saved-pause-hook @ pause-hook !
  ;
  
  \ Dump registers
  : dump-registers ( many -- )
    code[
    4 dp subs_,#_
    0 dp tos str_,[_,#_]
    5 tos mrs_,_
    ]code
    cr cr ." Exception state: "
    cr ."     IPSR:           " h.8
    cr ."     XPSR:           " h.8
    cr ."     Return address: " h.8
    cr ."     LR:             " h.8
    cr ."     SP:             " h.8
    cr ."     R12:            " h.8
    cr ."     R11:            " h.8
    cr ."     R10:            " h.8
    cr ."     R9:             " h.8
    cr ."     R8:             " h.8
    cr ."     R7:             " h.8
    cr ."     R6:             " h.8
    cr ."     R5:             " h.8
    cr ."     R4:             " h.8
    cr ."     R3:             " h.8
    cr ."     R2:             " h.8
    cr ."     R1:             " h.8
    cr ."     R0:             " h.8
  ;

  \ Collect fault registers ( -- )
  : collect-registers ( -- r0 .. r12 sp lr return-addr xpsr )
    code[
    r7 r0 movs_,_
    68 dp subs_,#_
    64 dp r6 str_,[_,#_]
    32 dp r0 str_,[_,#_]
    $0 8 + r0 ldr_,[sp,#_] \ R0
    60 dp r0 str_,[_,#_]
    $4 8 + r0 ldr_,[sp,#_] \ R1
    56 dp r0 str_,[_,#_]
    $8 8 + r0 ldr_,[sp,#_] \ R2
    52 dp r0 str_,[_,#_]
    $C 8 + r0 ldr_,[sp,#_] \ R3
    48 dp r0 str_,[_,#_]
    44 dp r4 str_,[_,#_]
    40 dp r5 str_,[_,#_]
    36 dp r6 str_,[_,#_]
    r8 r0 mov4_,4_
    28 dp r0 str_,[_,#_]
    r9 r0 mov4_,4_
    24 dp r0 str_,[_,#_]
    r10 r0 mov4_,4_
    20 dp r0 str_,[_,#_]
    r11 r0 mov4_,4_
    16 dp r0 str_,[_,#_]
    $10 8 + r0 ldr_,[sp,#_] \ R12
    12 dp r0 str_,[_,#_]
    sp r0 mov4_,4_
    36 r0 adds_,#_
    $1C 8 + r1 ldr_,[sp,#_] \ XPSR
    1 r2 movs_,#_
    9 r2 r2 lsls_,_,#_
    r1 r2 tst_,_
    eq bc>
    4 r0 adds_,#_
    >mark
    8 dp r0 str_,[_,#_]
    $14 8 + r0 ldr_,[sp,#_] \ LR
    4 dp r0 str_,[_,#_]
    $18 8 + r0 ldr_,[sp,#_] \ Return address
    0 dp r0 str_,[_,#_]
    r1 r6 movs_,_
    ]code
  ;

  \ Dump the data stack
  : dump-stack ( -- )
    cr cr ." Data stack:"
    cr ."     TOS:      " dup h.8
    sp@ cell+ stack-base @ swap ?do
      cr ."     " i h.8 ." : " i @ h.8
    4 +loop
  ;
    
  \ Dump the return statck
  : dump-rstack ( -- )
    cr cr ." Return stack:"
    code[
    4 dp subs_,#_
    0 dp tos str_,[_,#_]
    sp tos mov4_,4_
    $1C 8 + r1 ldr_,[sp,#_] \ XPSR
    1 r2 movs_,#_
    9 r2 r2 lsls_,_,#_
    r1 r2 tst_,_
    eq bc>
    4 tos adds_,#_
    >mark
    ]code
    rstack-base @ swap $30 + ?do
      cr ."     " i h.8 ." : " i @ h.8
    4 +loop
  ;
    
  \ Recover from a fault
  : recover-from-fault ( -- )
    code[
    4 dp subs_,#_
    0 dp tos str_,[_,#_]
    $1C 8 + tos ldr_,[sp,#_]
    tos r0 movs_,_
    1 r1 movs_,#_
    24 r1 r1 lsls_,_,#_
    r1 r0 orrs_,_
    $1C 8 + r0 str_,[sp,#_]
    ]code
    $1FF and 0= if
      CFSR @ CFSR !
      HFSR @ HFSR !
      in-main? if
        display-red cr cr ." Returning main task to prompt" display-normal cr
        ['] abort
      else
        display-red cr cr ." Terminating task" display-normal cr
        ['] bye
      then
      code[
      $18 8 + tos str_,[sp,#_]
      tos 1 dp ldm
      ]code
      restore-hooks
    else
      display-red cr cr ." Exception in exception handler; "
      ." environment limited" display-normal cr
      abort
    then
  ;

  \ Handle a hard fault
  : handle-hard-fault ( -- )
    collect-registers prepare-faulted-state
    display-red cr ." *** HARD FAULT *** "
    dump-registers dump-stack dump-rstack display-normal
    recover-from-fault
  ;

  \ Handle a mem fault
  : handle-mem-fault ( -- )
    collect-registers prepare-faulted-state
    display-red cr ." *** MEM FAULT *** "
    dump-registers dump-stack dump-rstack display-normal
    recover-from-fault
  ;

  \ Handle a bus fault
  : handle-bus-fault ( -- )
    collect-registers prepare-faulted-state
    display-red cr ." *** BUS FAULT *** "
    dump-registers dump-stack dump-rstack display-normal
    recover-from-fault
  ;

  \ Handle a usage fault
  : handle-usage-fault ( -- )
    collect-registers prepare-faulted-state
    display-red cr ." *** USAGE FAULT *** "
    dump-registers dump-stack dump-rstack display-normal
    recover-from-fault
  ;

  \ Initialize processor exception handling
  : init-exception ( -- )
    ['] handle-hard-fault hard-fault-vector vector!
    ['] handle-mem-fault mem-fault-vector vector!
    ['] handle-bus-fault bus-fault-vector vector!
    ['] handle-usage-fault usage-fault-vector vector!
  ;
  
end-module> import

\ Initialize processor exception handling
: init ( -- )
  init
  init-exception
;

reboot
