\ Copyright (c) 2024 Travis Bemann
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

begin-module ssd1306-terminal

  ssd1306-print import

  \ Our saved hooks
  0 value saved-emit?-hook
  0 value saved-emit-hook
  0 value saved-error-emit?-hook
  0 value saved-error-emit-hook

  \ Our saved color display
  true value saved-color-enabled

  \ Emit a character on the saved console
  : saved-emit ( c -- ) saved-emit-hook execute ;

  \ Always block for emit
  : handle-emit? ( -- emit? ) true ;

  \ Emit a character on both the SSD1306 and on the saved console
  : handle-emit ( c -- ) dup emit-ssd1306 saved-emit ;

  \ Start the terminal on the SSD1306
  : start-terminal ( -- )
    emit?-hook @ to saved-emit?-hook
    emit-hook @ to saved-emit-hook
    error-emit?-hook @ to saved-error-emit?-hook
    error-emit?-hook @ to saved-error-emit-hook
    color-enabled @ to saved-color-enabled
    ['] handle-emit? emit?-hook !
    ['] handle-emit emit-hook !
    ['] handle-emit? error-emit?-hook !
    ['] handle-emit error-emit-hook !
    false color-enabled !
  ;

  \ Exit the terminal on the SSD1306
  : exit-terminal ( -- )
    saved-emit?-hook emit?-hook !
    saved-emit-hook emit-hook !
    saved-error-emit?-hook error-emit?-hook !
    saved-error-emit-hook error-emit-hook !
    saved-color-enabled color-enabled !
  ;
    
end-module
