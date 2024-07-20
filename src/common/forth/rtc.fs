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

\ This is a stub RTC that only reports the date/time as Thu, 1 Jan 1970 00:00:00

compile-to-flash

begin-module rtc
  
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

  \ Check whether two date-times are equal (ignoring the day of the week)
  : date-time-equal? { date-time0 date-time1 -- equal? }
    date-time0 date-time-year @ date-time1 date-time-year @ =
    date-time0 date-time-month c@ date-time1 date-time-month c@ = and
    date-time0 date-time-day c@ date-time1 date-time-day c@ = and
    date-time0 date-time-hour c@ date-time1 date-time-hour c@ = and
    date-time0 date-time-minute c@ date-time1 date-time-minute c@ = and
    date-time0 date-time-second c@ date-time1 date-time-second c@ = and
  ;
  
  \ Get the current date and time
  : date-time@ { date-time -- }
    0 date-time date-time-second c!
    0 date-time date-time-minute c!
    0 date-time date-time-hour c!
    4 date-time date-time-dotw c!
    1 date-time date-time-day c!
    1 date-time date-time-month c!
    1970 date-time date-time-year !
  ;

  begin-module rtc-internal
  
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

    \ Number of days in a mont
    create days-in-month 0 c, 31 c, 28 c, 31 c, 30 c, 31 c, 30 c, 31 c,
    31 c, 30 c, 31 c, 30 c, 31 c,

    \ Get the number of seconds in a year
    : year-secs ( year -- secs )
      [ 365 24 * 60 * 60 * ] literal swap leap-year? if
        [ 24 60 * 60 * ] literal +
      then
    ;

    \ Get the number of seconds in a month
    : month-secs ( year month -- secs )
      dup days-in-month + c@ -rot
      2 = swap leap-year? and if 1+ then [ 24 60 * 60 * ] literal *
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

  end-module> import

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

reboot
