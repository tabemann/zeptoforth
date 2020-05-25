\ Copyright (c) 2020 Travis Bemann
\
\ This program is free software: you can redistribute it and/or modify
\ it under the terms of the GNU General Public License as published by
\ the Free Software Foundation, either version 3 of the License, or
\ (at your option) any later version.
\
\ This program is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.
\
\ You should have received a copy of the GNU General Public License
\ along with this program.  If not, see <http://www.gnu.org/licenses/>.

\ Capture test match
0 constant capture-match

\ Capture test skip until
1 constant capture-skip

\ Capture test ignore
2 constant capture-ignore

\ Capture test record
begin-structure capture-record
  \ Capture hook address
  field: capture-hook
  
  \ Capture string address
  field: capture-addr

  \ Capture string length
  hfield: capture-len

  \ Capture string offset
  hfield: capture-off

  \ Capture test type
  hfield: capture-type

  \ Capture string skip limit
  hfield: capture-limit
end-structure

\ Variable for emit capture test record buffer read-index
hvariable capture-read-index

\ Variable for emit capture test record buffer write-index
hvariable capture-write-index

\ Constant for the size of the emit capture test record buffer
32 constant capture-buffer-size

\ Emit capture test record buffer
capture-buffer-size capture-record *
aligned-buffer: capture-buffer

\ The saved EMIT hook
variable saved-emit-hook

\ The failed test hook
variable capture-fail-hook

\ Is emit capture nabled
variable capture-enabled

\ Enable emit capture
: enable-capture ( -- ) 1 capture-enabled +! ;

\ Disable emit capture
: disable-capture ( -- ) -1 capture-enabled +! ;

\ Do not capture for this block
: no-capture ( xt -- )
  capture-enabled @ >r 0 capture-enabled ! execute r> capture-enabled !
;

\ Get whether the emit capture test record buffer is full
: capture-full? ( -- f )
  capture-write-index h@ capture-read-index h@
  capture-buffer-size 1- + capture-buffer-size umod =
;

\ Get whether the emit capture test record buffer is empty
: capture-empty? ( -- f )
  capture-read-index h@ capture-write-index h@ =
;

\ Emit capture test record buffer is full
: x-capture-full space ." emit capture test buffer is full" cr ;

\ Allot a record in the emit capture test record buffer
: allot-capture ( -- addr )
  capture-full? triggers x-capture-full
  capture-write-index h@ capture-record * capture-buffer +
  capture-write-index h@ 1+ capture-buffer-size mod
  capture-write-index h!
;

\ Read a record from the emit capture test record buffer without disposing it
: read-capture ( -- addr|0 )
  capture-empty? not if
    capture-read-index h@ capture-record * capture-buffer +
  else
    0
  then
;

\ Dispose of a record from the emit capture test record buffer
: dispose-capture ( -- )
  capture-empty? not if
    capture-read-index h@ 1+ capture-buffer-size mod
    capture-read-index h!
  then
;

\ Clear the emit capture test record buffer
: clear-capture ( -- )
  0 capture-read-index ! 0 capture-write-index !
;

\ Pass an emit capture test
: pass-capture ( capture -- )
  capture-hook @ true swap ?execute dispose-capture
;

\ Fail an emit capture test
: fail-capture ( capture -- )
  capture-hook @ false swap ?execute dispose-capture
  capture-fail-hook @ ?execute
;

\ Get the current character from an emit capture test
: capture-current ( capture -- b )
  dup capture-off h@ swap capture-addr @ + b@
;

\ Advance an emit capture test
: advance-capture ( capture -- )
  dup capture-off h@ 1+ swap capture-off h!
;

\ Advance an email capture test and decrement the limit
: advance-limited-capture ( capture -- )
  dup capture-limit h@ 1- dup 0>= if
    over capture-limit h! advance-capture
  else
    fail-capture
  then
;

\ Skip a byte when capturing
: skip-capture ( capture -- )
  0 over capture-off h!
  dup capture-limit h@ 1- dup 0>= if
    swap capture-limit h!
  else
    drop fail-capture
  then
;

\ Is an emit capture test complete
: capture-complete? ( capture -- f )
  dup capture-off h@ swap capture-len h@ =
;

\ Handle a restarting character for an ignoring skipping emit capture test
: handle-capture-ignore-last ( b capture -- complete )
  dup capture-type h@ capture-ignore = if
    dup capture-off h@ dup 0= 2 pick capture-len h@ rot = or if
      dup capture-addr @ b@ rot = if
	1 over capture-off h!
	dup capture-limit h@ 1- dup 0>= if
	  over capture-limit h! false
	else
	  fail-capture false
	then
      else
	pass-capture true
      then
    else
      tuck capture-current = if
	advance-limited-capture false
      else
	fail-capture true
      then
    then
  else
    2drop false
  then
;

\ Handle a non-skipping emit capture test
: handle-capture-match ( b capture -- )
  tuck capture-current = if
    dup advance-capture dup capture-complete? if pass-capture else drop then
  else
    fail-capture
  then
;

\ Handle a skipping emit capture test
: handle-capture-skip ( b capture -- )
  tuck capture-current = if
    dup advance-capture dup capture-complete? if pass-capture else drop then
  else
    skip-capture
  then
;

\ Capture emit
: capture-emit ( b -- )
  dup saved-emit-hook @ ?dup if execute else drop then
  capture-enabled @ 0> if
    begin
      read-capture ?dup if
	2dup handle-capture-ignore-last not if
	  dup capture-type h@ case
	    capture-match of handle-capture-match endof
	    capture-skip of handle-capture-skip endof
	    capture-ignore of 2drop endof
	  endcase
	  true
	else
	  2drop false
	then
      else
	drop true
      then
    until
  else
    drop
  then
;

\ Register a non-skipping emit capture test
: add-match-capture ( b-addr bytes xt -- )
  allot-capture
  tuck capture-hook !
  tuck capture-len h!
  tuck capture-addr !
  capture-match over capture-type h!
  0 over capture-limit h!
  0 swap capture-off h!
;

\ Register a skipping emit capture test
: add-skip-capture ( b-addr bytes limit xt -- )
  allot-capture
  tuck capture-hook !
  tuck capture-limit h!
  tuck capture-len h!
  tuck capture-addr !
  capture-skip over capture-type h!
  0 swap capture-off h!
;

\ Register an ignoring emit capture test
: add-ignore-capture ( b-addr bytes limit xt -- )
  allot-capture
  tuck capture-hook !
  tuck capture-limit h!
  tuck capture-len h!
  tuck capture-addr !
  capture-ignore over capture-type h!
  0 swap capture-off h!
;

compiling-to-flash? not [if]

  \ Initialize the emit capture buffer read index
  0 capture-read-index h!
  
  \ Initialize the emit capture buffer write index
  0 capture-write-index h!
  
  \ Save the emit hook
  emit-hook @ saved-emit-hook !
  
  \ Set the fail hook to its default (null)
  0 capture-fail-hook !
  
  \ Initialize emit capture enabled
  0 capture-enabled !

  \ Set the emit hook
  ' capture-emit emit-hook !

[else]

  \ Initialize
  : init ( -- )
    init
    0 capture-read-index h!
    0 capture-write-index h!
    emit-hook @ saved-emit-hook !
    0 capture-fail-hook !
    0 capture-enabled !
    ' capture-emit emit-hook !
  ;

[then]
