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

begin-module picocalc-keys
  
  oo import
  picocalc-term import
  lock import
  
  \ Keycode out of range exception
  : x-key-out-of-range ( -- )
    ." key out of range" cr
  ;
  
  begin-module picocalc-keys-internal
    
    \ Key pressed attribute
    $01 constant KEY_PRESSED
    
    \ Key released attribute
    $03 constant KEY_RELEASED
    
    <object> begin-class <picocalc-keys>
      
      \ Keycode bitmap
      256 8 / member keymap
      
      \ Keys lock
      lock-size member keys-lock
      
      \ Clear the keymap
      method do-clear-keymap ( self -- )
      
      \ Update the keymap in a nonblocking fashion
      method do-update-keymap ( self -- )
      
      \ Update the keymap in a blocking fashion
      method do-wait-update-keymap ( self -- )
      
      \ Test the keymap for whether a key, by
      \ keycode, is pressed
      method do-keymap@ ( key self -- pressed )
      
      \ Update the keymap for a single key
      method update-key ( attr keycode self -- )
      
    end-class
    
    <picocalc-keys> begin-implement
    
      :noname { self -- }
        self <object>->new
        self do-clear-keymap
        self keys-lock init-lock
      ; define new
      
      :noname ( self -- )
        keymap 256 8 / $00 fill
      ; define do-clear-keymap
      
      :noname { self -- }
        raw-key>? if
          self [: { self }
            begin raw-key>? while
              raw-key> self update-key
            repeat
          ;] self keys-lock with-lock
        then
      ; define do-update-keymap
      
      :noname ( self -- )
        [: { self }
          begin
            raw-key> self update-key
          raw-key>? not until
        ;] over keys-lock with-lock
      ; define do-wait-update-keymap
      
      :noname { keycode self -- pressed }
        keycode 256 u< averts x-key-out-of-range
        keycode 7 and bit keycode 3 rshift self keymap + cbit@
      ; define do-keymap@
      
      :noname { attr keycode self -- }
        attr case
          KEY_PRESSED of
            keycode 7 and bit keycode 3 rshift self keymap + cbis!
          endof
          KEY_RELEASED of
            keycode 7 and bit keycode 3 rshift self keymap + cbic!
          endof
        endcase
      ; define update-key
      
    end-implement
    
    <picocalc-keys> class-size buffer: the-picocalc-keys
    
    \ Initialize the keymap
    : init-picocalc-keys ( -- )
      <picocalc-keys> the-picocalc-keys init-object
    ;
    
    initializer init-picocalc-keys
    
  end-module> import
  
  \ Clear the keymap
  : clear-keymap ( -- )
    the-picocalc-keys do-clear-keymap
  ;
  
  \ Update the keymap in a nonblocking fashion
  : update-keymap ( -- )
    the-picocalc-keys do-update-keymap
  ;
  
  \ Update the keymap in a blocking fashion
  : wait-update-keymap ( -- )
    the-picocalc-keys do-wait-update-keymap
  ;
  
  \ Test the keymap for whether a key, by keycode,
  \ is pressed
  : keymap@ ( key -- pressed )
    the-picocalc-keys do-keymap@
  ;
  
end-module
