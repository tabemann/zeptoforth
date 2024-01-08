\ Copyright (c) 2023-2024 Travis Bemann
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

#include extra/common/dyn_buffer.fs

oo import
heap import
dyn-buffer import
 
\ My base heap size
65536 constant my-base-heap-size

\ My block size
default-segment-size dyn-buffer-internal::segment-header-size + constant my-block-size

\ My block count
my-base-heap-size my-block-size / constant my-block-count

\ My real heap size
my-block-size my-block-count heap-size constant my-heap-size

\ My heap
my-heap-size cell align aligned-buffer: my-heap

\ Initialize the heap
my-block-size my-block-count my-heap init-heap

\ My dynamic buffer
<dyn-buffer> class-size cell align aligned-buffer: my-dyn-buffer
my-heap <dyn-buffer> my-dyn-buffer init-object

\ My main cursor
<cursor> class-size cell align aligned-buffer: my-cursor
my-dyn-buffer <cursor> my-cursor init-object

\ My second cursor
<cursor> class-size cell align aligned-buffer: my-cursor-1
my-dyn-buffer <cursor> my-cursor-1 init-object

\ Print the dynamic buffer
: dyn. ( -- )
  default-segment-size [:
    my-dyn-buffer <cursor> [: { data cursor }
      cr
      0 { offset }
      begin offset my-dyn-buffer dyn-buffer-len@ < while
        default-segment-size my-dyn-buffer dyn-buffer-len@ offset - min { bytes }
        data bytes cursor read-data to bytes
        bytes 0= if exit then
        bytes 0 ?do
          i offset + my-cursor offset@ = if ." |" then
          i offset + my-cursor-1 offset@ = if ." #" then
          data i + c@ emit
        loop
        bytes +to offset
      repeat
    ;] with-object
  ;] with-aligned-allot
;

s" foobar" my-cursor insert-data
s" foobar" my-cursor insert-data
s" foobar" my-cursor insert-data
s" quux" my-cursor insert-data
: test 100 0 ?do s" F00BAR " my-cursor insert-data loop ;
test
dyn.
100 my-cursor go-to-offset
dyn.
50 my-cursor-1 go-to-offset
dyn.
90 my-cursor delete-data
dyn.
90 my-cursor delete-data
dyn.
s" Fred Foobar! " my-cursor insert-data
dyn.
: delete-all my-dyn-buffer dyn-buffer-len@ my-cursor go-to-offset begin my-dyn-buffer dyn-buffer-len@ while 1 my-cursor delete-data repeat ;
delete-all
dyn.

s" foobarXfoobarXfoobar" my-cursor insert-data
10 my-cursor-1 go-to-offset
: match-X ( c -- flag ) [char] X = ;
' match-X my-cursor find-prev
dyn.
-1 my-cursor adjust-offset
' match-X my-cursor find-prev
dyn.
-1 my-cursor adjust-offset
' match-X my-cursor find-prev
dyn.
s" ***" my-cursor-1 insert-data
dyn.
' match-X my-cursor find-next
dyn.
1 my-cursor adjust-offset
' match-X my-cursor find-next
dyn.
1 my-cursor adjust-offset
' match-X my-cursor find-next
dyn.

