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

begin-module picocalc-bios

  oo import
  task import
  chan import
  rchan import
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
  
  begin-module picocalc-bios-internal

    \ PicoCalc BIOS I2C device
    1 constant picocalc-bios-i2c-device

    \ PicoCalc BIOS I2C SDA GPIO
    6 constant picocalc-bios-sda-pin

    \ PicoCalc BIOS I2C SCL GPIO
    7 constant picocalc-bios-scl-pin

    \ PicoCalc BIOS I2C address
    $1F constant picocalc-bios-i2c-addr

    \ PicoCalc BIOS I2C baud; note that I2C for the BIOS is slow
    10000 constant picocalc-bios-i2c-baud
    
    \ PicoCalc BIOS interval in milliseconds
    24 constant picocalc-bios-interval

    \ PicoCalc emulated BIOS interval in milliseconds
    8 constant emulate-picocalc-bios-interval

    \ Picocalc BIOS timeout in ticks
    5000 constant picocalc-bios-timeout

    \ PicoCalc keyboard channel element count
    256 constant picocalc-keys-chan-count

    \ PicoCalc reset delay in milliseconds
    100 constant picocalc-rst-delay

    \ PicoCalc keyboard count register
    4 constant PICOCALC_COUNT

    \ PicoCalc backlight register
    5 constant PICOCALC_BACKLIGHT
    
    \ PicoCalc BIOS reset command
    8 constant PICOCALC_RST
    
    \ PicoCalc keyboard key register
    9 constant PICOCALC_KEY

    \ PicoCalc keyboard backlight register
    10 constant PICOCALC_KBD_BACKLIGHT

    \ PicoCalc battery register
    11 constant PICOCALC_BATTERY

    \ Write bit
    $80 constant WRITE_BIT
    
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
  <object> begin-class <picocalc-bios>

    continue-module picocalc-bios-internal
      
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

      \ Are we emulating reading the battery
      cell member picocalc-emulate-read-battery

      \ Are we emulating a write
      cell member picocalc-emulate-write
      
      \ Are we emulating the backlight
      cell member picocalc-emulate-backlight
      
      \ Are we emulating the keyboard backlight
      cell member picocalc-emulate-kbd-backlight

      \ The channel of pressed keys
      2 picocalc-keys-chan-count chan-size member picocalc-keys-chan

      \ The reply channel for controlling the BIOS
      rchan-size member picocalc-bios-rchan

      \ Handle an exception
      method handle-exception ( xt self -- )

      \ Handle a request
      method handle-request ( self -- )
      
      \ Send an 8-bit command
      method send-command8 ( command self -- success? )

      \ Send a 16-bit command
      method send-command16 ( command self -- success? )

      \ Receive a 16-bit reply
      method recv-reply16 ( self -- data success? )

      \ Handle getting a count
      method get-count ( self -- )
      
      \ Handle getting a key
      method get-key ( self -- )

      \ Implement a delay
      method do-delay ( self -- )
      
      \ Run handling keys
      method run-keys ( self -- )

      \ Handle a key
      method handle-picocalc-key ( keycode self -- )

      \ Issue a request
      method request ( request self -- reply )
      
    end-module

    \ Initialize the PicoCalc BIOS support
    method init-picocalc-bios ( core self -- )
    
    \ Are there keys to read?
    method picocalc-keys>? ( self -- read? )
    
    \ Read a key
    method picocalc-keys> ( self -- attributes key )

    \ Get whether control is currently pressed
    method picocalc-keys-ctrl? ( self -- ctrl? )

    \ Get whether alt is currently pressed
    method picocalc-keys-alt? ( self -- alt? )

    \ Inject a keycode
    method inject-keycode ( keycode self -- )

    \ Read the battery
    method read-battery ( self -- val )

    \ Set the backlight
    method set-backlight ( val self -- val' )

    \ Read the backlight
    method read-backlight ( self -- val )
    
    \ Set the keyboard backlight
    method set-kbd-backlight ( val self -- val' )

    \ Read the keyboard backlight
    method read-kbd-backlight ( self -- val )
    
  end-class

  <picocalc-bios> begin-implement

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
      false self picocalc-emulate-read-battery !
      false self picocalc-emulate-write !
      false self picocalc-emulate-backlight !
      false self picocalc-emulate-kbd-backlight !
      2 picocalc-keys-chan-count self picocalc-keys-chan init-chan
      self picocalc-bios-rchan init-rchan
    ; define new

    \ Initialize the PicoCalc BIOS support
    :noname { core self -- }
      picocalc-bios-i2c-device picocalc-bios-sda-pin i2c-pin
      picocalc-bios-i2c-device picocalc-bios-scl-pin i2c-pin
      picocalc-bios-sda-pin pull-up-pin
      picocalc-bios-scl-pin pull-up-pin
      picocalc-bios-i2c-device master-i2c
      picocalc-bios-i2c-baud picocalc-bios-i2c-device i2c-clock!
      picocalc-bios-i2c-device 7-bit-i2c-addr
      picocalc-bios-i2c-addr picocalc-bios-i2c-device i2c-target-addr!
      picocalc-bios-i2c-device enable-i2c
      PICOCALC_RST self send-command8 drop
      picocalc-rst-delay ms
      self 1 ['] run-keys 512 128 768 core spawn-on-core { keys-task }
      c" keys" keys-task task-name!
      keys-task run
    ; define init-picocalc-bios
    
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

    \ Send an 8-bit command
    :noname { command self -- success? }
      command self [:
        [: { W^ buf self }
          emulate-keys? self picocalc-emulate @ or if
            self picocalc-emulate-write @ if
              false self picocalc-emulate-write ! true exit
            then
            buf c@ WRITE_BIT and if true self picocalc-emulate-write ! then
            WRITE_BIT buf cbic!
            buf c@ PICOCALC_RST <> self picocalc-emulate !
            buf c@ PICOCALC_COUNT = self picocalc-emulate-get-count !
            buf c@ PICOCALC_KEY = self picocalc-emulate-get-key !
            buf c@ PICOCALC_BATTERY = self picocalc-emulate-read-battery !
            buf c@ PICOCALC_BACKLIGHT = self picocalc-emulate-backlight !
            buf c@ PICOCALC_KBD_BACKLIGHT =
            self picocalc-emulate-kbd-backlight !
            true exit
          then
          buf 1 picocalc-bios-i2c-device >i2c-stop 1 =
        ;] picocalc-bios-timeout with-timeout
      ;] try
      ?dup if self handle-exception drop false then
    ; define send-command8

    \ Send a 16-bit command
    :noname { command self -- success? }
      command self [:
        [: { W^ buf self }
          emulate-keys? self picocalc-emulate @ or if
            self picocalc-emulate-write @ if
              false self picocalc-emulate-write ! true exit
            then
            buf c@ WRITE_BIT and if true self picocalc-emulate-write ! then
            WRITE_BIT buf cbic!
            buf c@ PICOCALC_RST <> self picocalc-emulate !
            buf c@ PICOCALC_COUNT = self picocalc-emulate-get-count !
            buf c@ PICOCALC_KEY = self picocalc-emulate-get-key !
            buf c@ PICOCALC_BATTERY = self picocalc-emulate-read-battery !
            buf c@ PICOCALC_BACKLIGHT = self picocalc-emulate-backlight !
            buf c@ PICOCALC_KBD_BACKLIGHT =
            self picocalc-emulate-kbd-backlight !
            true exit
          then
          buf 2 picocalc-bios-i2c-device >i2c-stop 2 =
        ;] picocalc-bios-timeout with-timeout
      ;] try
      ?dup if self handle-exception drop false then
    ; define send-command16

    \ Receive a 16-bit reply
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
              false self picocalc-emulate !
              self picocalc-emulate-get-key @ if
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
                self picocalc-emulate-read-battery @ if
                  false self picocalc-emulate-read-battery !
                  [ PICOCALC_BATTERY 100 8 lshift or ] literal
                else
                  self picocalc-emulate-backlight @ if
                    false self picocalc-emulate-backlight !
                    [ PICOCALC_BACKLIGHT 255 8 lshift or ] literal
                  else
                    self picocalc-emulate-kbd-backlight @ if
                      false self picocalc-emulate-kbd-backlight !
                      [ PICOCALC_KBD_BACKLIGHT 255 8 lshift or ] literal
                    else
                      0
                    then
                  then
                then
                true
              then
            then
            exit
          then
          0 { W^ buf }
          buf 2 picocalc-bios-i2c-device i2c-stop> 2 = if
            buf h@ true
          else
            0 false
          then
        ;] picocalc-bios-timeout with-timeout
      ;] try
      ?dup if self handle-exception drop 0 false then
    ; define recv-reply16

    \ Handle a request
    :noname { self -- }
      0 0 { W^ send-buffer W^ recv-buffer }
      send-buffer cell self picocalc-bios-rchan recv-rchan-no-block 0> if
        send-buffer c@ WRITE_BIT and if
          begin send-buffer h@ self send-command16 until
        else
          begin send-buffer c@ self send-command8 until
        then
        self do-delay
        begin
          self recv-reply16 dup if
            swap 8 rshift recv-buffer !
          else
            nip
          then
        until
        recv-buffer cell self picocalc-bios-rchan reply-rchan
        self do-delay
      then
    ; define handle-request
    
    \ Handle getting a count
    :noname { self -- }
      self picocalc-sent-command @ not if
        PICOCALC_COUNT self send-command8 if
          true self picocalc-sent-command !
        then
      else
        self recv-reply16 if
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
        PICOCALC_KEY self send-command8 if
          true self picocalc-sent-command !
        then
      else
        self recv-reply16 if
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
        self picocalc-sent-command @ not if
          self ['] handle-request try dup ['] x-would-block = if
            2drop 0
          then
          ?raise
        then
        self picocalc-get-key-count @ 0= if
          self get-count
        else
          self get-key
        then
        self do-delay
      again
    ; define run-keys

    \ Implement a delay
    :noname { self -- }
      emulate-keys? not if
        picocalc-bios-interval ms
      else
        emulate-picocalc-bios-interval ms
      then
    ; define do-delay
    
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
    ; define inject-keycode

    \ Issue a request
    :noname { W^ send-buffer self -- reply }
      0 { W^ recv-buffer }
      send-buffer cell recv-buffer cell self picocalc-bios-rchan
      send-rchan 0> if recv-buffer @ else 0 then
    ; define request

    \ Read the battery
    :noname { self -- val }
      PICOCALC_BATTERY self request
    ; define read-battery

    \ Set the backlight
    :noname { val self -- val' }
      PICOCALC_BACKLIGHT WRITE_BIT or val $FF and 8 lshift or self request
    ; define set-backlight

    \ Read the backlight
    :noname { self -- val }
      PICOCALC_BACKLIGHT self request
    ; define read-backlight
    
    \ Set the keyboard backlight
    :noname { val self -- val' }
      PICOCALC_KBD_BACKLIGHT WRITE_BIT or val $FF and 8 lshift or self request
    ; define set-kbd-backlight

    \ Read the keyboard backlight
    :noname { self -- val }
      PICOCALC_KBD_BACKLIGHT self request
    ; define read-kbd-backlight

  end-implement
  
end-module
