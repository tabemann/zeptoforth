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

begin-module monitor

  task import
  multicore import

  begin-module monitor-internal
    
    \ Our flag signaling whether this has been initialized
    false value inited?
    
    \ Our monitor task
    variable monitor-task

    \ Saved attention key handler
    variable saved-attention-hook

    \ Mailbox buffer
    cell aligned-buffer: monitor-mailbox-buf

    \ Mailbox to notify
    0 constant monitor-mailbox
    
    \ Display tasks message
    $7A5C7A5C constant display-tasks-msg
    
    \ Handle the attention key
    : do-attention ( c -- )
      dup [char] t = if
        drop false attention? !
        display-tasks-msg monitor-mailbox monitor-task @ notify-set
      else
        saved-attention-hook @ execute
      then
    ;

    \ Run the monitor
    : run-monitor ( -- )
      begin
        monitor-mailbox wait-notify display-tasks-msg = if
          cr display-red ." *** TASKS ***" display-normal
          dump-tasks
        then
        0 monitor-mailbox current-task mailbox!
      again
    ;
    
  end-module> import
  
  \ Start the monitor (if it has not already been started)
  : start-monitor ( -- )
    [:
      inited? not if true to inited? true else false then
    ;] critical-with-all-core-spinlock
    if
      0 ['] run-monitor 320 128 512 spawn monitor-task !
      monitor-mailbox-buf 1 monitor-task @ config-notify
      c" monitor" monitor-task @ task-name!
      32767 monitor-task @ task-priority!
      attention-hook @ saved-attention-hook !
      monitor-task @ run
      ['] do-attention attention-hook !
    then
  ;
  
end-module

reboot