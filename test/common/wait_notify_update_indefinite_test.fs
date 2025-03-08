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

begin-module wait-notify-update-indefinite-test

  task import
  alarm import
  
  variable my-mailbox
  variable my-task
  alarm-size buffer: my-alarm0
  alarm-size buffer: my-alarm1
  5000 constant alarm0-delay
  12500 constant alarm1-delay

  defer do-alarm0
  :noname
    2drop 0 bit ['] or 0 my-task @ notify-update
    alarm0-delay 0 0 ['] do-alarm0 my-alarm0 set-alarm-delay-default
  ; is do-alarm0
  
  defer do-alarm1
  :noname
    2drop 1 bit ['] or 0 my-task @ notify-update
    alarm1-delay 1 0 ['] do-alarm1 my-alarm1 set-alarm-delay-default
  ; is do-alarm1

  : handle-alarms ( -- )
    begin
      0 ['] and 0 wait-notify-update
      dup 0 bit and if cr ." 0 fired" then
      1 bit and if cr ." 1 fired" then
    again
  ;
  
  : run-test ( -- )
    0 ['] handle-alarms 320 128 512 spawn my-task !
    my-mailbox 1 my-task @ config-notify
    my-task @ run
    alarm0-delay 0 0 ['] do-alarm0 my-alarm0 set-alarm-delay-default
    alarm1-delay 1 0 ['] do-alarm1 my-alarm1 set-alarm-delay-default
  ;
  
end-module
