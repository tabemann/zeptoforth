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

compile-to-flash

begin-module aon-timer

  \ GPIO is not a valid GPIO for source selection exception
  : x-invalid-source-sel-gpio ( -- ) ." invalid source selection GPIO" cr ;
  
  begin-module aon-timer-internal

    \ POWMAN base address
    $40100000 constant POWMAN_BASE

    \ Select a GPIO to use as a time reference
    POWMAN_BASE $4C + constant POWMAN_EXT_TIME_REF

    \ Use the selected GPIO to drive the 32 kHz low power clock, in place of
    \ LPOSC. THhis field must only be written when POWMAN_TIMER_RUN = 0
    4 bit constant POWMAN_EXT_TIME_REF_DRIVE_LPCK

    \ GPIO time reference source selection LSB
    0 constant POWMAN_EXT_TIME_REF_SOURCE_SEL_LSB

    \ GPIO time reference source selection mask
    3 POWMAN_EXT_TIME_REF_SOURCE_SEL_LSB lshift
    constant POWMAN_EXT_TIME_REF_SOURCE_SEL_MASK

    \ GPIO 12 for time reference source selection
    0 constant POWMAN_EXT_TIME_REF_SOURCE_SEL_GPIO12
    
    \ GPIO 20 for time reference source selection
    1 constant POWMAN_EXT_TIME_REF_SOURCE_SEL_GPIO20

    \ GPIO 14 for time reference source selection
    2 constant POWMAN_EXT_TIME_REF_SOURCE_SEL_GPIO14

    \ GPIO 22 for time reference source selection
    3 constant POWMAN_EXT_TIME_REF_SOURCE_SEL_GPIO22

    \ LPOSC frequency in kHz integer component
    POWMAN_BASE $50 + constant POWMAN_LPOSC_FREQ_KHZ_INT

    \ LPOSC frequency in kHz fractional component
    POWMAN_BASE $54 + constant POWMAN_LPOSC_FREQ_KHZ_FRAC

    \ XOSC frequency in kHz integer component
    POWMAN_BASE $58 + constant POWMAN_XOSC_FREQ_KHZ_INT

    \ XOSC frequency in kHz fractional component
    POWMAN_BASE $5C + constant POWMAN_XOSC_FREQ_KHZ_FRAC
    
    \ Set time bits 63 to 48 register
    POWMAN_BASE $60 + constant POWMAN_SET_TIME_63TO48

    \ Set time bits 47 to 32 register
    POWMAN_BASE $64 + constant POWMAN_SET_TIME_47TO32

    \ Set time bits 31 to 16 register
    POWMAN_BASE $68 + constant POWMAN_SET_TIME_31TO16

    \ Set time bits 15 to 0 register
    POWMAN_BASE $6C + constant POWMAN_SET_TIME_15TO0

    \ Read time upper register (bits 63 to 32)
    POWMAN_BASE $70 + constant POWMAN_READ_TIME_UPPER

    \ Read time lower register (bits 31 to 0)
    POWMAN_BASE $74 + constant POWMAN_READ_TIME_LOWER
    
    \ Set alarm time bits 63 to 48 register
    POWMAN_BASE $78 + constant POWMAN_ALARM_TIME_63TO48

    \ Set alarm time bits 47 to 32 register
    POWMAN_BASE $7C + constant POWMAN_ALARM_TIME_47TO32

    \ Set alarm time bits 31 to 16 register
    POWMAN_BASE $80 + constant POWMAN_ALARM_TIME_31TO16

    \ Set alarm time bits 15 to 0 register
    POWMAN_BASE $41 + constant POWMAN_ALARM_TIME_15TO0

    \ Timer register
    POWMAN_BASE $88 + constant POWMAN_TIMER

    \ TImer is synchronized to a 1 Hz GPIO source
    19 bit constant POWMAN_TIMER_USING_GPIO_1HZ

    \ Timer is running from a 1 kHz GPIO source
    18 bit constant POWMAN_TIMER_USING_GPIO_1KHZ

    \ Timer is running from the LPOSC
    17 bit constant POWMAN_TIMER_USING_LPOSC

    \ Timer is running from the XOSC
    16 bit constant POWMAN_TIMER_USING_XOSC

    \ Select the GPIO source as a reference for the sec counter; the msec
    \ counter will continue to use the LPOSC or XOSC reference
    13 bit constant POWMAN_TIMER_USE_GPIO_1HZ

    \ Switch to the GPIO as the source of the 1 kHz timer tick
    10 bit constant POWMAN_TIMER_USE_GPIO_1KHZ

    \ Switch to the XOSC as the source of the 1 kHz timer tick
    9 bit constant POWMAN_TIMER_USE_XOSC

    \ Switch to the LPOSC as the source of the 1 kHz timer tick
    8 bit constant POWMAN_TIMER_USE_LPOSC

    \ Alarm has fired. Write 1 to clear the alarm
    6 bit constant POWMAN_TIMER_ALARM

    \ Alarm wakes the chip from low power mode
    5 bit constant POWMAN_TIMER_PWRUP_ON_ALARM

    \ Enables the alarm. The alarm must be disabled while writing the alarm time
    4 bit constant POWMAN_TIMER_ALARM_ENAB

    \ Clear the timer, does not disable the timer and does not affect the alarm.
    \ This control can be written at any time.
    2 bit constant POWMAN_TIMER_CLEAR

    \ Timer enable. Setting this bit causes the timer to begin counting up
    \ from its current value. Clearing this bit stops the timer from counting.
    \ POWMAN_LPOSC_FREQ and POWMAN_XOSC_FREQ must be set first.
    1 bit constant POWMAN_TIMER_RUN

    \ Control whether non-secure software can write to the timer registers.
    \ All other registers are hardwired to be inaccessible to non-secure
    0 bit constant POWMAN_TIMER_NONSEC_WRITE

    \ Initialize the AON timer
    : init-aon-timer ( -- )
      POWMAN_TIMER_NONSEC_WRITE POWMAN_TIMER bis!
      POWMAN_TIMER_RUN POWMAN_TIMER bic!
      POWMAN_TIMER_ALARM POWMAN_TIMER bis!
      POWMAN_TIMER_ALARM_ENAB POWMAN_TIMER bic!
      $20 POWMAN_LPOSC_FREQ_KHZ_INT ! \ 32
      $C49C POWMAN_LPOSC_FREQ_KHZ_FRAC ! \ 0.768
      $2EE0 POWMAN_XOSC_FREQ_KHZ_INT ! \ 12000
      $0 POWMAN_XOSC_FREQ_KHZ_FRAC ! \ 0
      POWMAN_TIMER_USE_XOSC POWMAN_TIMER bis!
      POWMAN_TIMER_RUN POWMAN_TIMER bis!
    ;

    initializer init-aon-timer
    
  end-module> import

  \ Enable the timer
  : enable-timer ( -- )
    POWMAN_TIMER_RUN POWMAN_TIMER bis!
  ;

  \ Disable the timer
  : disable-timer ( -- )
    POWMAN_TIMER_RUN POWMAN_TIMER bic!
  ;

  \ Is the timer enabled
  : timer-enabled? ( -- enabled? )
    POWMAN_TIMER_RUN POWMAN_TIMER bit@
  ;

  \ Set the time
  : time! ( D: time -- )
    timer-enabled? { enabled? }
    disable-timer
    dup 16 rshift POWMAN_SET_TIME_63TO48 !
    $FFFF POWMAN_SET_TIME_47TO32 !
    dup 16 rshift POWMAN_SET_TIME_31TO16 !
    $FFFF POWMAN_SET_TIME_15TO0 !
    enabled? if enable-timer then
  ;

  \ Get the time
  : time@ ( -- D: time )
    begin
      POWMAN_READ_TIME_UPPER @
      POWMAN_READ_TIME_LOWER @ swap
      dup POWMAN_READ_TIME_UPPER @ =
    until
  ;

  \ Clear the time
  : clear-time ( -- )
    POWMAN_TIMER_CLEAR POWMAN_TIMER bis!
  ;
  
  \ Enable the alarm
  : enable-alarm ( -- )
    POWMAN_TIMER_ALARM_ENAB POWMAN_TIMER bis!
  ;

  \ Disable the alarm
  : disable-alarm ( -- )
    POWMAN_TIMER_ALARM_ENAB POWMAN_TIMER bic!
  ;

  \ Is the alarm enabled
  : alarm-enabled? ( -- enabled? )
    POWMAN_TIMER_ALARM_ENAB POWMAN_TIMER bit@
  ;

  \ Set the alarm time
  : alarm! ( D: time -- )
    alarm-enabled? { enabled? }
    disable-alarm
    dup 16 rshift POWMAN_ALARM_TIME_63TO48 !
    $FFFF POWMAN_ALARM_TIME_47TO32 !
    dup 16 rshift POWMAN_ALARM_TIME_31TO16 !
    $FFFF POWMAN_ALARM_TIME_15TO0 !
    enabled? if enable-alarm then
  ;

  \ Get the alarm time
  : alarm@ ( -- D: alarm )
    POWMAN_ALARM_TIME_15TO0 @ $FFFF and
    POWMAN_ALARM_TIME_31TO16 @ 16 lshift or
    POWMAN_ALARM_TIME_47TO32 @ $FFFF and
    POWMAN_ALARM_TIME_63TO48 @ 16 lshift or
  ;

  \ Enable the alarm wake-up from low power mode
  : enable-alarm-wake-up ( -- )
    POWMAN_TIMER_PWRUP_ON_ALARM POWMAN_TIMER bis!
  ;

  \ Disable the alarm wake-up from low power mode
  : disable-alarm-wake-up ( -- )
    POWMAN_TIMER_PWRUP_ON_ALARM POWMAN_TIMER bic!
  ;

  \ Is the alarm wake-up from low power mode enabled enabled
  : alarm-wake-up-enabled? ( -- enabled? )
    POWMAN_TIMER_PWRUP_ON_ALARM POWMAN_TIMER bit@
  ;

  \ Switch to LPOSC as the source of the 1 kHz timer tick
  : lposc-timer ( -- )
    POWMAN_TIMER_USE_LPOSC POWMAN_TIMER bis!
  ;

  \ Switch to XOSC as the source of the 1 kHz timer tick
  : xosc-timer ( -- )
    POWMAN_TIMER_USE_XOSC POWMAN_TIMER bis!
  ;

  \ Switch to a GPIO as the source of the 1 kHz timer tick
  : gpio-1khz-timer ( -- )
    POWMAN_TIMER_USE_GPIO_1KHZ POWMAN_TIMER bis!
  ;

  \ Synchronize with a GPIO as the source of the second counter
  : sync-gpio-1hz-timer ( -- )
    POWMAN_TIMER_USE_GPIO_1HZ POWMAN_TIMER bis!
  ;

  \ Unsynchronize with a GPIO as the source of the second counter
  : unsync-gpio-1hz-timer ( -- )
    POWMAN_TIMER_USE_GPIO_1HZ POWMAN_TIMER bic!
  ;
  
  \ Get whether the timer is running from the LPOSC
  : lposc-timer? ( -- flag )
    POWMAN_TIMER_USING_LPOSC POWMAN_TIMER bit@
  ;

  \ Get whether the timer is running from the XOSC
  : xosc-timer? ( -- flag )
    POWMAN_TIMER_USING_XOSC POWMAN_TIMER bit@
  ;

  \ Get whether the timer is running from a 1 kHz GPIO source
  : gpio-1khz-timer? ( -- flag )
    POWMAN_TIMER_USING_GPIO_1KHZ POWMAN_TIMER bit@
  ;

  \ Get whether the timer is synchronized with a GPIO as the source of the
  \ second counter
  : sync-gpio-1hz-timer? ( -- flag )
    POWMAN_TIMER_USING_GPIO_1HZ POWMAN_TIMER bit@
  ;

  \ Set a GPIO as a source selection
  : source-sel! ( gpio -- )
    case
      12 of POWMAN_EXT_TIME_REF_SOURCE_SEL_GPIO12 endof
      20 of POWMAN_EXT_TIME_REF_SOURCE_SEL_GPIO20 endof
      14 of POWMAN_EXT_TIME_REF_SOURCE_SEL_GPIO14 endof
      22 of POWMAN_EXT_TIME_REF_SOURCE_SEL_GPIO22 endof
      ['] x-invalid-source-sel-gpio ?raise
    endcase
    POWMAN_EXT_TIME_REF !
  ;

  \ Set a GPIO as a source selection to drive the 32 kHz low power clock
  : source-sel-drive-lpck! ( gpio -- )
    case
      12 of POWMAN_EXT_TIME_REF_SOURCE_SEL_GPIO12 endof
      20 of POWMAN_EXT_TIME_REF_SOURCE_SEL_GPIO20 endof
      14 of POWMAN_EXT_TIME_REF_SOURCE_SEL_GPIO14 endof
      22 of POWMAN_EXT_TIME_REF_SOURCE_SEL_GPIO22 endof
      ['] x-invalid-source-sel-gpio ?raise
    endcase
    timer-enabled? { enabled? }
    disable-timer
    POWMAN_EXT_TIME_REF_DRIVE_LPCK or POWMAN_EXT_TIME_REF !
    enabled? if enable-timer then
  ;

  \ Get the source selection GPIO
  : source-sel@ ( -- gpio )
    POWMAN_EXT_TIME_REF @
    POWMAN_EXT_TIME_REF_SOURCE_SEL_MASK and
    POWMAN_EXT_TIME_REF_SOURCE_SEL_LSB rshift
    case
      POWMAN_EXT_TIME_REF_SOURCE_SEL_GPIO12 of 12 endof
      POWMAN_EXT_TIME_REF_SOURCE_SEL_GPIO20 of 20 endof
      POWMAN_EXT_TIME_REF_SOURCE_SEL_GPIO14 of 14 endof
      POWMAN_EXT_TIME_REF_SOURCE_SEL_GPIO22 of 22 endof
    endcase
  ;

  \ Get whether a GPIO is driving the low power clock
  : drive-lpck? ( -- flag )
    POWMAN_EXT_TIME_REF_DRIVE_LPCK POWMAN_EXT_TIME_REF bit@
  ;

end-module
