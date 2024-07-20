\ Copyright (c) 2023-2024 Travis Bemann
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

begin-module rtc

  multicore import
  interrupt import
  
  \ Invalid date/time exception
  : x-invalid-date-time ( -- ) ." invalid date/time" cr ;

  \ Our date-time structure; this is meant for user consumption
  \
  \ Each field, when set to $FFFFFFFF or $FF, means to ignore the field when
  \ setting the date and time
  begin-structure date-time-size
    
    \ The year; valid values are from 0 to 4095
    field: date-time-year

    \ The month; valid values are from 1 to 12
    cfield: date-time-month

    \ The day; valid values are from 1 to [28, 29, 30, 31] depending on the
    \ month
    cfield: date-time-day

    \ The day of the week; valid values are from 0 to 6, 0 is Sunday
    cfield: date-time-dotw

    \ The hour; valid values are 0 to 23
    cfield: date-time-hour

    \ The minute; valid values are from 0 to 59
    cfield: date-time-minute

    \ The second; valid values are from 0 to 59
    cfield: date-time-second

    \ Align the size of the date/time
    cell align
    
  end-structure

  begin-module rtc-internal

    \ RTC base
    $4005C000 constant RTC_Base

    \ RTC clock divider, minus 1
    RTC_Base $00 + constant RTC_CLKDIV_M1

    \ RTC setup register 0
    RTC_Base $04 + constant RTC_SETUP_0

    \ RTC setup register 1
    RTC_Base $08 + constant RTC_SETUP_1

    \ RTC control and status register
    RTC_Base $0C + constant RTC_CTRL

    \ RTC interrupt setup register 0
    RTC_Base $10 + constant RTC_IRQ_SETUP_0

    \ RTC interrupt setup register 1
    RTC_Base $14 + constant RTC_IRQ_SETUP_1

    \ RTC register 1
    RTC_Base $18 + constant RTC_RTC_1

    \ RTC register 0 - read this before RTC_RTC_1!
    RTC_Base $1C + constant RTC_RTC_0

    \ RTC raw interrupts register
    RTC_Base $20 + constant RTC_INTR

    \ RTC interrupt enable register
    RTC_Base $24 + constant RTC_INTE

    \ RTC interrupt force register
    RTC_Base $28 + constant RTC_INTF

    \ RTC interrupt status after masking and forcing register
    RTC_Base $2C + constant RTC_INTS

    \ RTC IRQ
    25 constant rtc-irq

    \ RTC exception vector
    rtc-irq 16 + constant rtc-vector

    \ Set the RTC year
    : RTC_SETUP_0_YEAR! { year -- }
      RTC_SETUP_0 @ $FFF000 bic year 12 lshift or RTC_SETUP_0 !
    ;

    \ Set the RTC month
    : RTC_SETUP_0_MONTH! { month -- }
      RTC_SETUP_0 @ $F00 bic month 8 lshift or RTC_SETUP_0 !
    ;

    \ Set the RTC day
    : RTC_SETUP_0_DAY! { day -- }
      RTC_SETUP_0 @ $1F bic day or RTC_SETUP_0 !
    ;

    \ Set the RTC day of the week
    : RTC_SETUP_1_DOTW! { dotw -- }
      RTC_SETUP_1 @ $7000000 bic dotw 24 lshift or RTC_SETUP_1 !
    ;

    \ Set the RTC hour
    : RTC_SETUP_1_HOUR! { hour -- }
      RTC_SETUP_1 @ $1F0000 bic hour 16 lshift or RTC_SETUP_1 !
    ;

    \ Set the RTC minute
    : RTC_SETUP_1_MIN! { minute -- }
      RTC_SETUP_1 @ $3F00 bic minute 8 lshift or RTC_SETUP_1 !
    ;

    \ Set the RTC second
    : RTC_SETUP_1_SEC! { second -- }
      RTC_SETUP_1 @ $3F bic second or RTC_SETUP_1 !
    ;

    \ Set forcing the leap year off
    : RTC_CTRL_FORCE_NOTLEAPYEAR! { not-leap-year -- }
      8 bit RTC_CTRL not-leap-year if bis! else bic! then
    ;

    \ Load the RTC date/time
    : RTC_CTRL_LOAD! ( -- )
      4 bit RTC_CTRL bis!
    ;

    \ Get whether the RTC is active
    : RTC_CTRL_RTC_ACTIVE@ ( -- active )
      1 bit RTC_CTRL bit@
    ;

    \ Set RTC enabled
    : RTC_CTRL_RTC_ENABLE! { enable -- }
      0 bit RTC_CTRL enable if bis! else bic! then
    ;
    
    \ Get whether RTC matching is active
    : RTC_IRQ_SETUP_0_MATCH_ACTIVE@ ( -- active )
      29 bit RTC_IRQ_SETUP_0 bit@
    ;

    \ Set RTC matching enabled
    : RTC_IRQ_SETUP_0_MATCH_ENA! { enable -- }
      28 bit RTC_IRQ_SETUP_0 enable if bis! else bic! then
    ;

    \ Set year matching enabled
    : RTC_IRQ_SETUP_0_YEAR_ENA! { enable -- }
      26 bit RTC_IRQ_SETUP_0 enable if bis! else bic! then
    ;

    \ Set month matching enabled
    : RTC_IRQ_SETUP_0_MONTH_ENA! { enable -- }
      25 bit RTC_IRQ_SETUP_0 enable if bis! else bic! then
    ;

    \ Set day matching enabled
    : RTC_IRQ_SETUP_0_DAY_ENA! { enable -- }
      24 bit RTC_IRQ_SETUP_0 enable if bis! else bic! then
    ;

    \ Set the matched year
    : RTC_IRQ_SETUP_0_YEAR! { year -- }
      RTC_IRQ_SETUP_0 @ $FFF000 bic year 12 lshift or RTC_IRQ_SETUP_0 !
    ;

    \ Set the matched month
    : RTC_IRQ_SETUP_0_MONTH! { month -- }
      RTC_IRQ_SETUP_0 @ $F00 bic month 8 lshift or RTC_IRQ_SETUP_0 !
    ;

    \ Set the matched day
    : RTC_IRQ_SETUP_0_DAY! { day -- }
      RTC_IRQ_SETUP_0 @ $1F bic day or RTC_IRQ_SETUP_0 !
    ;

    \ Set day of the week matching enabled
    : RTC_IRQ_SETUP_1_DOTW_ENA! { enable -- }
      31 bit RTC_IRQ_SETUP_1 enable if bis! else bic! then
    ;

    \ Set hour matching enabled
    : RTC_IRQ_SETUP_1_HOUR_ENA! { enable -- }
      30 bit RTC_IRQ_SETUP_1 enable if bis! else bic! then
    ;

    \ Set minute matching enabled
    : RTC_IRQ_SETUP_1_MIN_ENA! { enable -- }
      29 bit RTC_IRQ_SETUP_1 enable if bis! else bic! then
    ;

    \ Set second matching enabled
    : RTC_IRQ_SETUP_1_SEC_ENA! { enable -- }
      28 bit RTC_IRQ_SETUP_1 enable if bis! else bic! then
    ;
    
    \ Set the matched day of the week
    : RTC_IRQ_SETUP_1_DOTW! { dotw -- }
      RTC_IRQ_SETUP_1 @ $7000000 bic dotw 24 lshift or RTC_IRQ_SETUP_1 !
    ;

    \ Set the matched hour
    : RTC_IRQ_SETUP_1_HOUR! { hour -- }
      RTC_IRQ_SETUP_1 @ $1F0000 bic hour 16 lshift or RTC_IRQ_SETUP_1 !
    ;

    \ Set the matched minute
    : RTC_IRQ_SETUP_1_MIN! { minute -- }
      RTC_IRQ_SETUP_1 @ $3F00 bic minute 8 lshift or RTC_IRQ_SETUP_1 !
    ;

    \ Set the matched second
    : RTC_IRQ_SETUP_1_SEC! { second -- }
      RTC_IRQ_SETUP_1 @ $3F bic second or RTC_IRQ_SETUP_1 !
    ;
    
    \ Get the current year
    : RTC_RTC_1_YEAR@ ( -- year )
      RTC_RTC_1 @ $FFF000 and 12 rshift
    ;

    \ Get the current month
    : RTC_RTC_1_MONTH@ ( -- month )
      RTC_RTC_1 @ $F00 and 8 rshift
    ;

    \ Get the current day
    : RTC_RTC_1_DAY@ ( -- day )
      RTC_RTC_1 @ $1F and
    ;

    \ Get the current day of the week
    : RTC_RTC_0_DOTW@ ( -- dotw )
      RTC_RTC_0 @ $7000000 and 24 rshift
    ;

    \ Get the current hour
    : RTC_RTC_0_HOUR@ ( -- hour )
      RTC_RTC_0 @ $1F0000 and 16 rshift
    ;

    \ Get the current minute
    : RTC_RTC_0_MIN@ ( -- minute )
      RTC_RTC_0 @ $3F00 and 8 rshift
    ;

    \ Get the current second
    : RTC_RTC_0_SEC@ ( -- second )
      RTC_RTC_0 @ $3F and
    ;

  end-module> import
  
  \ Get the current date and time
  : date-time@ ( date-time -- )
    [: { date-time }
      RTC_RTC_0_SEC@ date-time date-time-second c!
      RTC_RTC_0_MIN@ date-time date-time-minute c!
      RTC_RTC_0_HOUR@ date-time date-time-hour c!
      RTC_RTC_0_DOTW@ date-time date-time-dotw c!
      RTC_RTC_1_DAY@ date-time date-time-day c!
      RTC_RTC_1_MONTH@ date-time date-time-month c!
      RTC_RTC_1_YEAR@ date-time date-time-year !
    ;] rtc-spinlock critical-with-spinlock
  ;

  continue-module rtc-internal
  
    \ Test for a value in a range
    : test-range { test-value min-value max-value -- }
      test-value min-value >= averts x-invalid-date-time
      test-value max-value <= averts x-invalid-date-time
    ;

    \ Get whether a year is a leap year
    : leap-year? { year -- }
      year 100 umod 0= if
        year 400 umod 0=
      else
        year 4 umod 0=
      then
    ;

    \ Number of days in a month
    create days-in-month 0 c, 31 c, 28 c, 31 c, 30 c, 31 c, 30 c, 31 c,
    31 c, 30 c, 31 c, 30 c, 31 c,

    \ Get the number of seconds in a year
    : year-secs ( year -- )
      [ 365 24 * 60 * 60 * ] literal swap leap-year? if
        [ 24 60 * 60 * ] literal +
      then
    ;
    
    \ Get the number of seconds in a month
    : month-secs ( year month -- secs )
      dup days-in-month + c@ -rot
      2 = swap leap-year? and if 1+ then [ 24 60 * 60 * ] literal *
    ;
    
    \ Validate a date/time
    : validate-date-time ( date-time -- )
      date-time-size [: { new-date-time old-date-time }
        old-date-time date-time@
        new-date-time date-time-year @ -1 <> if
          new-date-time date-time-year @ 0 4095 test-range
          new-date-time date-time-year @
        else
          old-date-time date-time-year @
        then { test-year }
        new-date-time date-time-month c@ $FF <> if
          new-date-time date-time-month c@ 1 31 test-range
          new-date-time date-time-month c@
        else
          old-date-time date-time-month c@
        then { test-month }
        new-date-time date-time-day c@ $FF <> if
          test-month 2 <> if
            new-date-time date-time-day c@ 1 days-in-month test-month + c@
            test-range
          else
            new-date-time date-time-day c@ 1
            test-year leap-year? if 29 else 28 then test-range
          then
        then
        new-date-time date-time-dotw c@ $FF <> if
          new-date-time date-time-dotw c@ 0 6 test-range
        then
        new-date-time date-time-hour c@ $FF <> if
          new-date-time date-time-hour c@ 0 23 test-range
        then
        new-date-time date-time-minute c@ $FF <> if
          new-date-time date-time-minute c@ 0 59 test-range
        then
        new-date-time date-time-second c@ $FF <> if
          new-date-time date-time-second c@ 0 59 test-range
        then
      ;] with-aligned-allot
    ;

    \ Validate a date/time match
    : validate-date-time-not-current { date-time -- }
      date-time date-time-year @ -1 <> if
        date-time date-time-year @ 0 4095 test-range
      then
      date-time date-time-month c@ $FF <> if
        date-time date-time-month c@ 1 31 test-range
      then
      date-time date-time-day c@ $FF <> if
        date-time date-time-month c@ $FF <> if
          date-time date-time-month c@ 2 <> if
            date-time date-time-day c@ 1
            days-in-month date-time date-time-month c@ + c@
            test-range
          else
            date-time date-time-day c@ 1
            date-time date-time-year @ -1 <> if
              date-time date-time-year @ leap-year? if 29 else 28 then
            else
              29
            then
            test-range
          then
        else
          date-time date-time-day c@ 1 31 test-range
        then
      then
      date-time date-time-dotw c@ $FF <> if
        date-time date-time-dotw c@ 0 6 test-range
      then
      date-time date-time-hour c@ $FF <> if
        date-time date-time-hour c@ 0 23 test-range
      then
      date-time date-time-minute c@ $FF <> if
        date-time date-time-minute c@ 0 59 test-range
      then
      date-time date-time-second c@ $FF <> if
        date-time date-time-second c@ 0 59 test-range
      then
    ;

    \ Raw set the RTC
    : raw-date-time! { date-time -- }
      date-time date-time-year @ -1 <> if
        date-time date-time-year @ RTC_SETUP_0_YEAR!
      then
      date-time date-time-month c@ $FF <> if
        date-time date-time-month c@ RTC_SETUP_0_MONTH!
      then
      date-time date-time-day c@ $FF <> if
        date-time date-time-day c@ RTC_SETUP_0_DAY!
      then
      date-time date-time-dotw c@ $FF <> if
        date-time date-time-dotw c@ RTC_SETUP_1_DOTW!
      then
      date-time date-time-hour c@ $FF <> if
        date-time date-time-hour c@ RTC_SETUP_1_HOUR!
      then
      date-time date-time-minute c@ $FF <> if
        date-time date-time-minute c@ RTC_SETUP_1_MIN!
      then
      date-time date-time-second c@ $FF <> if
        date-time date-time-second c@ RTC_SETUP_1_SEC!
      then
      RTC_CTRL_LOAD!
    ;

    \ Raw set the RTC matching
    : raw-date-time-match! { date-time -- }
      date-time date-time-year @ -1 <> if
        date-time date-time-year @ RTC_IRQ_SETUP_0_YEAR!
        true RTC_IRQ_SETUP_0_YEAR_ENA!
      else
        false RTC_IRQ_SETUP_0_YEAR_ENA!
      then
      date-time date-time-month c@ $FF <> if
        date-time date-time-month c@ RTC_IRQ_SETUP_0_MONTH!
        true RTC_IRQ_SETUP_0_MONTH_ENA!
      else
        false RTC_IRQ_SETUP_0_MONTH_ENA!
      then
      date-time date-time-day c@ $FF <> if
        date-time date-time-day c@ RTC_IRQ_SETUP_0_DAY!
        true RTC_IRQ_SETUP_0_DAY_ENA!
      else
        false RTC_IRQ_SETUP_0_DAY_ENA!
      then
      date-time date-time-dotw c@ $FF <> if
        date-time date-time-dotw c@ RTC_IRQ_SETUP_1_DOTW!
        true RTC_IRQ_SETUP_1_DOTW_ENA!
      else
        false RTC_IRQ_SETUP_1_DOTW_ENA!
      then
      date-time date-time-hour c@ $FF <> if
        date-time date-time-hour c@ RTC_IRQ_SETUP_1_HOUR!
        true RTC_IRQ_SETUP_1_HOUR_ENA!
      else
        false RTC_IRQ_SETUP_1_HOUR_ENA!
      then
      date-time date-time-minute c@ $FF <> if
        date-time date-time-minute c@ RTC_IRQ_SETUP_1_MIN!
        true RTC_IRQ_SETUP_1_MIN_ENA!
      else
        false RTC_IRQ_SETUP_1_MIN_ENA!
      then
      date-time date-time-second c@ $FF <> if
        date-time date-time-second c@ RTC_IRQ_SETUP_1_SEC!
        true RTC_IRQ_SETUP_1_SEC_ENA!
      else
        false RTC_IRQ_SETUP_1_SEC_ENA!
      then
    ;

  end-module

  \ Disable the RTC
  : disable-rtc ( -- )
    false RTC_CTRL_RTC_ENABLE!
    begin RTC_CTRL_RTC_ACTIVE@ not until
  ;

  \ Enable the RTC
  : enable-rtc ( -- )
    true RTC_CTRL_RTC_ENABLE!
    begin RTC_CTRL_RTC_ACTIVE@ until
  ;
  
  continue-module rtc-internal

    \ Initialize the RTC
    : init-rtc ( -- )
      disable-rtc
      46874 RTC_CLKDIV_M1 !
      date-time-size [: { date-time }
        1970 date-time date-time-year !
        1 date-time date-time-month c!
        1 date-time date-time-day c!
        4 date-time date-time-dotw c! \ Thursday
        0 date-time date-time-hour c!
        0 date-time date-time-minute c!
        0 date-time date-time-second c!
        date-time raw-date-time!
      ;] with-aligned-allot
      enable-rtc
    ;

  end-module

  \ Check whether two date-times are equal (ignoring the day of the week)
  : date-time-equal? { date-time0 date-time1 -- equal? }
    date-time0 date-time-year @ date-time1 date-time-year @ =
    date-time0 date-time-month c@ date-time1 date-time-month c@ = and
    date-time0 date-time-day c@ date-time1 date-time-day c@ = and
    date-time0 date-time-hour c@ date-time1 date-time-hour c@ = and
    date-time0 date-time-minute c@ date-time1 date-time-minute c@ = and
    date-time0 date-time-second c@ date-time1 date-time-second c@ = and
  ;
  
  \ Set the date/time; with this -1 year and $FF other value settings do not
  \ change the date/time fields in question
  : date-time! { date-time -- }
    date-time validate-date-time
    disable-rtc
    date-time raw-date-time!
    enable-rtc
  ;

  \ Set the RTC alarm; with this -1 year and $FF other value settings are
  \ ignored for the set alarm (i.e. recurrent alarms can be set)
  : set-rtc-alarm { date-time xt -- }
    date-time validate-date-time-not-current
    rtc-irq NVIC_ICER_CLRENA!
    0 bit RTC_INTE bic!
    false RTC_IRQ_SETUP_0_MATCH_ENA!
    begin RTC_IRQ_SETUP_0_MATCH_ACTIVE@ not until
    date-time raw-date-time-match!
    xt rtc-vector vector!
    true RTC_IRQ_SETUP_0_MATCH_ENA!
    begin RTC_IRQ_SETUP_0_MATCH_ACTIVE@ until
    0 bit RTC_INTE bis!
    rtc-irq NVIC_ISER_SETENA!
  ;

  \ Clear the RTC alarm
  : clear-rtc-alarm ( -- )
    rtc-irq NVIC_ICER_CLRENA!
    0 bit RTC_INTE bic!
    false RTC_IRQ_SETUP_0_MATCH_ENA!
    begin RTC_IRQ_SETUP_0_MATCH_ACTIVE@ not until
    rtc-irq NVIC_ISER_SETENA!
  ;

  \ Get the name of a day of the week
  : dotw-name ( dotw -- c-addr u )
    case
      0 of s" Sun" endof
      1 of s" Mon" endof
      2 of s" Tue" endof
      3 of s" Wed" endof
      4 of s" Thu" endof
      5 of s" Fri" endof
      6 of s" Sat" endof
      s" ???" rot
    endcase
  ;

  \ Get the name of a month
  : month-name ( month -- c-addr u )
    case
      1 of s" Jan" endof
      2 of s" Feb" endof
      3 of s" Mar" endof
      4 of s" Apr" endof
      5 of s" May" endof
      6 of s" Jun" endof
      7 of s" Jul" endof
      8 of s" Aug" endof
      9 of s" Sep" endof
      10 of s" Oct" endof
      11 of s" Nov" endof
      12 of s" Dec" endof
      s" ???" rot
    endcase
  ;

  \ Maximum date/time format size
  25 constant max-date-time-format-size
  
  \ Format a date/time
  : format-date-time { addr date-time -- addr bytes }
    date-time validate-date-time-not-current
    addr { cur-addr }
    date-time date-time-dotw c@ $FF <> if
      date-time date-time-dotw c@ dotw-name cur-addr swap move
      s" , " cur-addr 3 + swap move
      5 +to cur-addr
    then
    date-time date-time-day c@ s>d <# # # #> cur-addr swap move
    $20 cur-addr 2 + c!
    3 +to cur-addr
    date-time date-time-month c@ month-name cur-addr swap move
    $20 cur-addr 3 + c!
    4 +to cur-addr
    date-time date-time-year @ s>d <# # # # # #> cur-addr swap move
    $20 cur-addr 4 + c!
    5 +to cur-addr
    date-time date-time-hour c@ s>d <# # # #> cur-addr swap move
    [char] : cur-addr 2 + c!
    3 +to cur-addr
    date-time date-time-minute c@ s>d <# # # #> cur-addr swap move
    2 +to cur-addr
    date-time date-time-second c@ $FF <> if
      [char] : cur-addr c!
      1 +to cur-addr
      date-time date-time-second c@ s>d <# # # #> cur-addr swap move
      2 +to cur-addr
    then
    addr cur-addr over -
  ;

  \ Print the date and time
  : date-time. ( date-time -- )
    max-date-time-format-size [: swap format-date-time type ;] with-allot
  ;

  continue-module rtc-internal

    \ A set of constants needed for the day of the week calculation
    create dotw-t 0 c, 3 c, 2 c, 5 c, 0 c, 3 c, 5 c, 1 c, 4 c, 6 c, 2 c, 4 c,

  end-module

  \ Calculate the day of the week for a date/time
  : get-dotw { date-time -- dotw }
    date-time validate-date-time-not-current
    date-time date-time-year @ { year }
    date-time date-time-month c@ { month }
    year -1 <> month $FF <> and date-time date-time-day c@ $FF <> and if
      month 3 < if -1 +to year then
      year year 4 / +
      year 100 / -
      year 400 / +
      dotw-t month 1- + c@ +
      date-time date-time-day c@ +
      7 umod
    else
      $FF
    then
  ;

  \ Update the day of the week for a date/time
  : update-dotw { date-time -- }
    $FF date-time date-time-dotw c!
    date-time validate-date-time-not-current
    date-time get-dotw date-time date-time-dotw c!
  ;
    
  \ Convenience word for setting the date/time from the stack rather than a
  \ date-time structure; with this -1 fields (and for fields other than the
  \ year, $FF) do not change the date/time fields in question. Also note that
  \ the day of the week is automatically calculated.
  : simple-date-time! ( year month day hour minute second -- )
    date-time-size [: { year month day hour minute second date-time }
      year -1 <> if
        year 0 4095 test-range
      then
      month -1 <> month $FF <> and if
        month 1 12 test-range
      else
        $FF to month
      then
      day -1 <> day $FF <> and if
        day 1 31 test-range
      else
        $FF to day
      then
      hour -1 <> hour $FF <> and if
        hour 0 23 test-range
      else
        $FF to hour
      then
      minute -1 <> minute $FF <> and if
        minute 0 59 test-range
      else
        $FF to minute
      then
      second -1 <> second $FF <> and if
        second 0 59 test-range
      else
        $FF to second
      then
      year date-time date-time-year !
      month date-time date-time-month c!
      day date-time date-time-day c!
      $FF date-time date-time-dotw c!
      hour date-time date-time-hour c!
      minute date-time date-time-minute c!
      second date-time date-time-second c!
      date-time update-dotw
      date-time date-time!
    ;] with-aligned-allot
  ;

  \ Set a day-time from seconds since 1970-01-01 00:00:00 UTC
  : convert-secs-since-1970 { second date-time -- }
    1970 { year }
    begin second year year-secs u>= while
      year year-secs negate +to second
      1 +to year
    repeat
    1 { month }
    begin second year month month-secs u>= while
      year month month-secs negate +to second
      1 +to month
    repeat
    second [ 24 60 * 60 * ] literal u/mod 1+ { day } to second
    second 3600 u/mod { hour } to second
    second 60 u/mod { minute } to second
    year date-time date-time-year !
    month date-time date-time-month c!
    day date-time date-time-day c!
    hour date-time date-time-hour c!
    minute date-time date-time-minute c!
    second date-time date-time-second c!
    date-time update-dotw
  ;
  
end-module

\ Initialize
: init ( -- ) init rtc::rtc-internal::init-rtc ;

reboot
