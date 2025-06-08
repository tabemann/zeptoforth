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
  task import
  chan import
  i2c import
  pin import

  \ Are we emulating the keyboard using the serial port
  false value emulate-keys?
  
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
    1 constant picocalc-keys-i2c-device

    \ PicoCalc keyboard I2C SDA GPIO
    6 constant picocalc-keys-sda-pin

    \ PicoCalc keyboard I2C SCL GPIO
    7 constant picocalc-keys-scl-pin

    \ PicoCalc keyboard I2C address
    $1F constant picocalc-keys-i2c-addr

    \ PicoCalc keyboard I2C baud; note that I2C for the keyboard is slow
    10000 constant picocalc-keys-i2c-baud
    
    \ PicoCalc keyboard interval in milliseconds
    16 constant picocalc-keys-interval

    \ PicoCalc emulated keyboard interval in milliseconds
    8 constant emulate-picocalc-keys-interval

    \ Picocalc keyboard timeout in ticks
    5000 constant picocalc-keys-timeout

    \ PicoCalc keyboard channel element count
    256 constant picocalc-keys-chan-count

    \ PicoCalc reset delay in milliseconds
    100 constant picocalc-rst-delay

    \ PicoCalc keyboard reset command
    8 constant PICOCALC_RST
    
    \ PicoCalc keyboard count register
    4 constant PICOCALC_COUNT
    
    \ PicoCalc keyboard key register
    9 constant PICOCALC_KEY

    \ PicoCalc keyboard count mask
    $1F constant PICOCALC_COUNT_MASK

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
      
      \ Is control currently pressed
      cell member picocalc-keys-ctrl-held

      \ Is alt currently pressed
      cell member picocalc-keys-alt-held

      \ Has an error been displayed
      cell member picocalc-error-displayed

      \ Has a command been sent
      cell member picocalc-sent-command

      \ Are we going to get a key count
      cell member picocalc-get-key-count

      \ Are already emulated
      cell member picocalc-already-emulate
      
      \ Are we emulating
      cell member picocalc-emulate
      
      \ Are we emulating getting key count
      cell member picocalc-emulate-get-count

      \ Are we emulating getting a key
      cell member picocalc-emulate-get-key

      \ The channel of pressed keys
      2 picocalc-keys-chan-count chan-size member picocalc-keys-chan

      \ Handle an exception
      method handle-exception ( xt self -- )
      
      \ Send a command
      method send-command ( command self -- success? )

      \ Receive a reply
      method recv-reply ( self -- data success? )

      \ Handle getting a count
      method get-count ( self -- )
      
      \ Handle getting a key
      method get-key ( self -- )
      
      \ Run handling keys
      method run-keys ( self -- )

      \ Handle a key
      method handle-picocalc-key ( keycode self -- )

    end-module

    \ Initialize the PicoCalc keyboard
    method init-picocalc-keys ( core self -- )
    
    \ Are there keys to read?
    method picocalc-keys>? ( self -- read? )
    
    \ Read a key
    method picocalc-keys> ( self -- attributes key )

    \ Get whether control is currently pressed
    method picocalc-keys-ctrl? ( self -- ctrl? )

    \ Get whether alt is currently pressed
    method picocalc-keys-alt? ( self -- alt? )

    \ Inject a keycode
    method inject-picocalc-keycode ( keycode self -- )
    
  end-class

  <picocalc-keys> begin-implement

    \ Constructor
    :noname { self -- }
      self <object>->new
      false self picocalc-keys-ctrl-held !
      false self picocalc-keys-alt-held !
      false self picocalc-error-displayed !
      false self picocalc-sent-command !
      0 self picocalc-get-key-count !
      false self picocalc-already-emulate !
      false self picocalc-emulate !
      false self picocalc-emulate-get-count !
      false self picocalc-emulate-get-key !
      2 picocalc-keys-chan-count self picocalc-keys-chan init-chan
    ; define new

    \ Initialize the PicoCalc keyboard
    :noname { core self -- }
      picocalc-keys-i2c-device picocalc-keys-sda-pin i2c-pin
      picocalc-keys-i2c-device picocalc-keys-scl-pin i2c-pin
      picocalc-keys-sda-pin pull-up-pin
      picocalc-keys-scl-pin pull-up-pin
      picocalc-keys-i2c-device master-i2c
      picocalc-keys-i2c-baud picocalc-keys-i2c-device i2c-clock!
      picocalc-keys-i2c-device 7-bit-i2c-addr
      picocalc-keys-i2c-addr picocalc-keys-i2c-device i2c-target-addr!
      picocalc-keys-i2c-device enable-i2c
      PICOCALC_RST self send-command drop
      picocalc-rst-delay ms
      self 1 ['] run-keys 320 128 512 core spawn-on-core { keys-task }
      c" keys" keys-task task-name!
      keys-task run
    ; define init-picocalc-keys
        
    \ Are there keys to read?
    :noname ( self -- read? )
      picocalc-keys-chan chan-empty? not
    ; define picocalc-keys>?
    
    \ Read a key
    :noname { self -- attributes key }
      0 { W^ buf }
      begin buf 2 self picocalc-keys-chan recv-chan 2 = until
      buf h@ dup $FF and swap 8 rshift
    ; define picocalc-keys>

    \ Get whether control is currently pressed
    :noname ( self -- ctrl? )
      picocalc-keys-ctrl-held @
    ; define picocalc-keys-ctrl?

    \ Get whether alt is currently pressed
    :noname ( self -- alt? )
      picocalc-keys-alt-held @
    ; define picocalc-keys-alt?

    \ Handle an exception
    :noname { xt self -- }
      xt ['] x-i2c-target-noack <> if
        self picocalc-error-displayed @ not if
          xt [:
            display-red execute display-normal flush-console
          ;] console::with-serial-output
          true self picocalc-error-displayed !
        then
      then
    ; define handle-exception

    \ Send a command
    :noname { command self -- success? }
      command self [:
        [: { W^ buf self }
          emulate-keys? self picocalc-emulate @ or if
            buf c@ PICOCALC_RST <> self picocalc-emulate !
            buf c@ PICOCALC_COUNT = self picocalc-emulate-get-count !
            buf c@ PICOCALC_KEY = self picocalc-emulate-get-key !
            true exit
          then
          buf 1 picocalc-keys-i2c-device >i2c-stop 1 =
        ;] picocalc-keys-timeout with-timeout
      ;] try
      ?dup if self handle-exception drop false then
    ; define send-command

    \ Receive a reply
    :noname { self -- data success? }
      self [:
        [: { self }
          self picocalc-emulate @ if
            self picocalc-emulate-get-count @ if
              false self picocalc-emulate-get-count !
              [: key? ;] console::with-serial-input if
                1 true
              else
                false self picocalc-emulate !
                0 true
              then
            else
              self picocalc-emulate-get-key @ if
                false self picocalc-emulate !
                false self picocalc-emulate-get-key !
                [: key? ;] console::with-serial-input if
                  [: key ;] console::with-serial-input
                  dup $0D = if drop $0A then
                  dup $7F = if drop $08 then
                  dup $1B = if drop KEY_ESC then
                  8 lshift $01 or true
                else
                  0 true
                then
              else
                false self picocalc-emulate !
                0 true
              then
            then
            exit
          then
          0 { W^ buf }
          buf 2 picocalc-keys-i2c-device i2c-stop> 2 = if
            buf h@ true
          else
            0 false
          then
        ;] picocalc-keys-timeout with-timeout
      ;] try
      ?dup if self handle-exception drop 0 false then
    ; define recv-reply

    \ Handle getting a count
    :noname { self -- }
      self picocalc-sent-command @ not if
        PICOCALC_COUNT self send-command if
          true self picocalc-sent-command !
        then
      else
        self recv-reply if
          PICOCALC_COUNT_MASK and ?dup if self picocalc-get-key-count ! then
          false self picocalc-sent-command !
        else
          drop
        then
      then
    ; define get-count

    \ Handle getting a key
    :noname { self -- }
      self picocalc-sent-command @ not if
        PICOCALC_KEY self send-command if
          true self picocalc-sent-command !
        then
      else
        self recv-reply if
          self handle-picocalc-key
          false self picocalc-sent-command !
          -1 self picocalc-get-key-count +!
        else
          drop
        then
      then
    ; define get-key
    
    \ Run handling keys
    :noname { self -- }
      begin
        emulate-keys? { emulate }
        emulate self picocalc-already-emulate @ xor if
          false self picocalc-sent-command !
          0 self picocalc-get-key-count !
          emulate self picocalc-already-emulate !
        then
        self picocalc-get-key-count @ 0= if
          self get-count
        else
          self get-key
        then
        emulate-keys? not if
          picocalc-keys-interval ms
        else
          emulate-picocalc-keys-interval ms
        then
      again
    ; define run-keys

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
      self picocalc-keys-chan chan-full? not if
        keycode $FF and 1 = if
          keycode $FF00 and
          self picocalc-keys-ctrl-held @ ATTR_CTRL and
          self picocalc-keys-alt-held @ ATTR_ALT and or or { W^ buf }
          buf 2 self picocalc-keys-chan send-chan
        then
      then
    ; define handle-picocalc-key

    \ Inject a keycode
    :noname ( keycode self -- )
      handle-picocalc-key
    ; define inject-picocalc-keycode

  end-implement
  
end-module
