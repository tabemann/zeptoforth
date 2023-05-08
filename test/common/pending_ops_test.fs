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

begin-module pending-ops-test

  task import
  
  pending-op-size buffer: my-pending-op-0
  pending-op-size buffer: my-pending-op-1
  pending-op-size buffer: my-pending-op-2
  
  cell buffer: my-mailboxes-0
  cell buffer: my-mailboxes-1
  cell buffer: my-mailboxes-2
  
  variable my-task-0
  variable my-task-1
  variable my-task-2
  
  0 :noname 0 wait-notify drop cr ." FOO" ; 320 128 512 spawn my-task-0 !
  0 :noname 0 wait-notify drop cr ." BAR" ; 320 128 512 spawn my-task-1 !
  0 :noname 0 wait-notify drop cr ." BAZ" ; 320 128 512 spawn my-task-2 !
  
  my-mailboxes-0 1 my-task-0 @ config-notify
  my-mailboxes-1 1 my-task-1 @ config-notify
  my-mailboxes-2 1 my-task-2 @ config-notify
  
  my-task-0 @ run
  my-task-1 @ run
  my-task-2 @ run
  
  : add-pending-ops ( -- )
    [: 0 my-task-0 @ notify ;] my-pending-op-0 add-pending-op
    [: 0 my-task-1 @ notify ;] my-pending-op-1 add-pending-op
    [: 0 my-task-2 @ notify ;] my-pending-op-2 add-pending-op
    force-pending-ops
  ;
  
  add-pending-ops
  
end-module
