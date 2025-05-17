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

begin-module picocalc-keys

  oo import
  alarm import
  lock import
  sema import
  i2c import

  \ PicoCalc attributes
  0 bit constant ATTR_CTRL
  1 bit constant ATTR_ALT

  \ PicoCalc special keys
  $B1 constant KEY_ESC
  $81 constant KEY_F1
  $82 constant KEY_F2
  $83 constant KEY_F3
  $84 constant KEY_F4
  $85 constant KEY_F5
  $86 constant KEY_F6
  $87 constant KEY_F7
  $88 constant KEY_F8
  $89 constant KEY_F9
  $90 constant KEY_F10
  $B5 constant KEY_UP
  $B6 constant KEY_DOWN
  $B7 constant KEY_RIGHT
  $B4 constant KEY_LEFT
  $D0 constant KEY_BREAK
  $D1 constant KEY_INSERT
  $D2 constant KEY_HOME
  $D4 constant KEY_DEL
  $D5 constant KEY_END
  $D6 constant KEY_PUP
  $D7 constant KEY_PDOWN
  
  begin-module picocalc-keys-internal

    \ PicoCalc keyboard I2C device
    0 constant picocalc-keys-i2c-device

    \ PicoCalc keyboard I2C SDA GPIO
    6 constant picocalc-keys-sda-pin

    \ PicoCalc keyboard I2C SCL GPIO
    7 constant picocalc-keys-scl-pin

    \ PicoCalc keyboard I2C address
    $1F constant picocalc-keys-i2c-addr

    \ PicoCalc keyboard I2C baud; note that I2C for the keyboard is slow
    10000 constant picocalc-keys-i2c-baud
    
    \ PicoCalc keyboard interval in ticks
    10 constant picocalc-keys-interval

    \ PicoCalc keyboard priority
    0 constant picocalc-keys-priority

    \ Picocalc keyboard timeout in ticks
    5000 constant picocalc-keys-timeout

    \ PicoCalc keyboard buffer size
    256 constant picocalc-keys-buf-size

    \ PicoCalc keyboard register
    9 constant PICOCALC_KEY

    \ Control is now held
    $A502 constant PICOCALC_CTRL_HELD

    \ Control is now not held
    $A503 constant PICOCALC_CTRL_NOT_HELD

    \ Alt is now held
    $A102 constant PICOCALC_ALT_HELD

    \ Alt is now not held
    $A103 constant PICOCALC_ALT_NOT_HELD

  end-module> import

  \ The PicoCalc keyboard class
  <object> begin-class <picocalc-keys>

    continue-module picocalc-keys-internal

      \ The PicoCalc keyboard lock
      lock-size member picocalc-keys-lock

      \ The PicoCalc keyboard semaphore
      sema-size member picocalc-keys-sema

      \ The PicoCalc keyboard destruction semaphore
      sema-size member picocalc-keys-destroy-sema
      
      \ The PicoCalc keyboard alarm
      alarm-size member picocalc-keys-alarm

      \ Are we attempting to destroy the PicoCalc keyboard
      cell member picocalc-keys-try-destroy

      \ Are we ready to destroy the PicoCalc keyboard
      cell member picocalc-keys-ready-destroy
      
      \ Is control currently pressed
      cell member picocalc-keys-ctrl-held

      \ Is alt currently pressed
      cell member picocalc-keys-alt-held

      \ The PicoCalc keyboard key circular buffer
      picocalc-keys-buf-size member picocalc-keys-key-buf

      \ The PicoCalc keyboard key attribute circular buffer
      picocalc-keys-buf-size member picocalc-keys-attr-buf

      \ The PicoCalc keyboard circular buffer read index
      cell member picocalc-keys-read-index

      \ The PicoCalc keyboard circular buffer write index
      cell member picocalc-keys-write-index

      \ Handle an alarm
      method handle-picocalc-keys-alarm ( self -- )

      \ Handle a key
      method handle-picocalc-key ( keycode self -- )

    end-module

    \ Initialize the PicoCalc keyboard
    method init-picocalc-keys ( self -- )
    
    \ Are there keys to read?
    method picocalc-keys>? ( self -- read? )
    
    \ Read a key
    method picocalc-keys> ( self -- attributes key )

    \ Get whether control is currently pressed
    method picocalc-keys-ctrl? ( self -- ctrl? )

    \ Get whether alt is currently pressed
    method picocalc-keys-alt? ( self -- alt? )
    
  end-class

  <picocalc-keys> begin-implement

    \ Constructor
    :noname { self -- }
      self <object>->new
      self picocalc-keys-lock init-lock
      no-sema-limit 0 self picocalc-keys-sema init-sema
      no-sema-limit 1 self picocalc-keys-destroy-sema init-sema
      false self picocalc-keys-try-destroy !
      true self picocalc-keys-ready-destroy !
      false self picocalc-keys-ctrl-held !
      false self picocalc-keys-alt-held !
      self picocalc-keys-key-buf picocalc-keys-buf-size 0 fill
      self picocalc-keys-attr-buf picocalc-keys-buf-size 0 fill
      0 self picocalc-keys-read-index !
      0 self picocalc-keys-write-index !
    ; define new

    \ Destructor
    :noname { self -- }
      true self picocalc-keys-try-destroy !
      begin self picocalc-keys-ready-destroy @ until
      self picocalc-keys-sema broadcast
      self picocalc-keys-destroy-sema take
      self <object>->destroy
    ; define destroy

    \ Initialize the PicoCalc keyboard
    :noname { self -- }
      self picocalc-keys-destroy-sema ungive
      false picocalc-keys-ready-destroy !
      picocalc-keys-i2c-device picocalc-keys-sda-pin i2c-pin
      picocalc-keys-i2c-device picocalc-keys-scl-pin i2c-pin
      picocalc-keys-i2c-device master-i2c
      picocalc-keys-i2c-baud picocalc-keys-i2c-device i2c-clock!
      picocalc-keys-i2c-device 7-bit-i2c-addr
      picocalc-keys-i2c-device enable-i2c
      picocalc-keys-interval picocalc-keys-priority
      self [: drop handle-picocalc-keys-alarm ;]
      self picocalc-keys-alarm set-alarm-delay-default
    ; define init-picocalc-keys
        
    \ Are there keys to read?
    :noname ( self -- read? )
      [: { self }
        self picocalc-keys-read-index @ self picocalc-keys-write-index @ <>
      ;] over picocalc-keys-lock with-lock
    ; define picocalc-keys>?
    
    \ Read a key
    :noname { self -- attributes key }
      self picocalc-keys-destroy-sema ungive
      self picocalc-keys-sema take
      self picocalc-keys-try-destroy @ if
        self picocalc-keys-destroy-sema give false 0 exit
      then
      [: { self }
        self picocalc-keys-read-index @ { read-index }
        read-index self picocalc-keys-attr-buf + c@
        read-index self picocalc-keys-key-buf + c@
        read-index 1+ [ picocalc-keys-buf-size 1- ] literal and
        self picocalc-keys-read-index !
      ;] over picocalc-keys-lock with-lock
      self picocalc-keys-destroy-sema give
    ; define picocalc-keys>

    \ Get whether control is currently pressed
    :noname ( self -- ctrl? )
      picocalc-keys-ctrl-held @
    ; define picocalc-keys-ctrl?

    \ Get whether alt is currently pressed
    :noname ( self -- alt? )
      picocalc-keys-alt-held @
    ; define picocalc-keys-alt?

    \ Handle an alarm
    :noname { self -- }
      self picocalc-keys-try-destroy @ not if
        self [: { self }
          picocalc-keys-i2c-addr picocalc-keys-i2c-device i2c-target-addr!
          PICOCALC_KEY { W^ buf }
          buf 1 picocalc-keys-i2c-device >i2c-restart 1 = if
            0 buf !
            buf 2 picocalc-keys-i2c-device i2c-stop> 2 = if
              buf @ self handle-picocalc-key
            then
          then
        ;] picocalc-keys-timeout task::with-timeout
        picocalc-keys-interval picocalc-keys-priority
        self [: drop handle-picocalc-keys-alarm ;]
        self picocalc-keys-alarm set-alarm-delay-default        
      else
        true self picocalc-keys-ready-destroy !
        self picocalc-keys-destroy-sema give
      then
    ; define handle-picocalc-keys-alarm

    \ Handle a key
    :noname { keycode self -- }
      keycode PICOCALC_CTRL_HELD = if
        true self picocalc-keys-ctrl-held ! exit
      else
        keycode PICOCALC_CTRL_NOT_HELD = if
          false self picocalc-keys-ctrl-held ! exit
        then
      then
      keycode PICOCALC_ALT_HELD = if
        true self picocalc-keys-alt-held ! exit
      else
        keycode PICOCALC_ALT_NOT_HELD = if
          false self picocalc-keys-alt-held ! exit
        then
      then
      self picocalc-keys-write-index @ { write-index }
      write-index 1+ [ picocalc-keys-buf-size 1- ] literal and
      self picocalc-keys-read-index @ = if exit then
      keycode $FF and 1 = if
        keycode 8 rshift
        self picocalc-keys-key-buf write-index + c!
        self picocalc-keys-ctrl-held @ ATTR_CTRL and
        self picocalc-keys-alt-held @ ATTR_ALT and or
        self picocalc-keys-attr-buf write-index + c!
        write-index 1+ [ picocalc-keys-buf-size 1- ] literal and
        self picocalc-keys-write-index !
        self picocalc-keys-sema give
      then
    ; define handle-picocalc-key
    
  end-implement
  
end-module
