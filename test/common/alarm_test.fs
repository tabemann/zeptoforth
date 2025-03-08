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

begin-module alarm-test

  alarm import
  
  alarm-task-size buffer: my-alarm-task
  alarm-size buffer: my-alarm0
  alarm-size buffer: my-alarm1
  alarm-size buffer: my-alarm2
  
  320 128 512 0 my-alarm-task init-alarm-task
  
  defer do-alarm0
  :noname
    2drop ." A" 5000 0 0 ['] do-alarm0 my-alarm0 my-alarm-task set-alarm-delay
  ; is do-alarm0

  defer do-alarm1
  :noname
    2drop ." B" 5000 1 0 ['] do-alarm1 my-alarm1 my-alarm-task set-alarm-delay
  ; is do-alarm1

  defer do-alarm2
  :noname
    2drop ." C" 5000 2 0 ['] do-alarm2 my-alarm2 my-alarm-task set-alarm-delay
  ; is do-alarm2
  
  5000 0 0 ' do-alarm0 my-alarm0 my-alarm-task set-alarm-delay
  5000 1 0 ' do-alarm1 my-alarm1 my-alarm-task set-alarm-delay
  5000 2 0 ' do-alarm2 my-alarm2 my-alarm-task set-alarm-delay
  
end-module