\ Copyright (c) 2023-2025 Travis Bemann
\ Copyright (c) 2023-2024 Ralph W. Lundvall
\ Copyright (c) 2023 Rob Probin
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

begin-module picocalc-sound

  oo import
  pwm import
  task import
  chan import
   
  \ Select a core to use for the PicoCalc tasks
  defined? select-picocalc-tasks-core [if]
    select-picocalc-tasks-core constant picocalc-tasks-core
  [else]
    1 constant picocalc-tasks-core
  [then]

 begin-module picocalc-sound-internal

    \ Tone structure
    begin-structure tone-size

      \ The end tick for the sound
      field: tone-end-tick

      \ The sound pitch
      2field: tone-pitch
      
    end-structure

    \ Beep duration in ticks
    1250 value beep-duration-ticks

    \ Beep pitch in Hz (D6# specifically)
    1245,0 2value beep-pitch
    
    \ The PWM index for sound
    5 constant sound-pwm-index
    
    \ The GPIO index for the left speaker (PWM A)
    26 constant sound-left-pin

    \ The GPIO index for the right speaker (PWM B)
    27 constant sound-right-pin

    \ The total number of tones to track
    8 constant max-tone-count
    
    \ Calculate closest dividers for a PWM frequency
    \
    \ This routine produces the three parameters for PWM. It's been tested on a 
    \ limited amount of audio frequencies and at the moment doesn't use the 
    \ fraction part at all - although it might be possible to adjust it 
    \ if the scaling part is changed from 2* to, say 1.5 or 1.2. Changes 
    \ will need to be made to make the int/frac into a S31.32 fixed point
    \ number.
    \
    \ As mentioned above this routine currently doesn't use the fractional
    \ part, just int divider 
    \ 
    : calculate-closest-dividers
      ( S31.32-frequency-in-Hz -- frac-divider int-divider top-count )
      0 -rot  \ fraction part - currently always zero (we could bind this with
              \ frac divider, and make it divide by less than 2 each time...
      1 -rot  \ scaling = int-divider
      ( 0 1 S31.32-freq )
      sysclk @ s>f 2swap f/
      begin
        \ if it's above top count, then it won't fit! (we adjust top-count by 1
        \ because fsys/((top+1)*(div_int+div_frac/16))
        2dup 65536 s>f d>
      while
        \ make it smaller, but record how much we divided it by
        2 s>f f/
        rot 2* -rot
      repeat
      f>s 1-  \ topcount-1
    ;

  end-module> import

  <object> begin-class <picocalc-sound>

    continue-module picocalc-sound-internal

      \ The pitches for each tone to generate
      max-tone-count 2 * cells member tone-pitches

      \ The end times for each pitch
      max-tone-count cells member tone-end-ticks

      \ The tone count
      cell member tone-count
      
      \ The channel of tones to generate
      tone-size 1 chan-size member tone-chan

      \ Run sound generation
      method run-sound ( self -- )

      \ Fetch a tone
      method fetch-tone ( block? self -- )

      \ Add a tone
      method add-tone ( tone-end-tick D: tone-pitch self -- )

      \ Find the next tone
      method find-next-tone ( self -- tone-end-tick D: tone-pitch found? )

      \ Retire a tone
      method retire-tone ( self -- )
      
      \ Set the pitch
      method set-pitch ( D: pitch self -- )
      
    end-module

    \ Initialize sound
    method init-sound ( self -- )

    \ Play a tone
    method do-play-tone ( tone-end-tick D: tone-pitch self -- )
    
  end-class

  <picocalc-sound> begin-implement

    \ Constructor
    :noname { self -- }
      self <object>->new
      tone-size 1 self tone-chan init-chan
      0 self tone-count !
      max-tone-count 0 ?do
        0,0 i 2 * cells self tone-pitches + 2!
        0 i cells self tone-end-ticks + !
      loop
    ; define new

    \ Initialize sound
    :noname { core self -- }
      sound-pwm-index bit disable-pwm
      sound-pwm-index free-running-pwm
      false sound-pwm-index pwm-phase-correct!
      sound-left-pin pwm-pin
      sound-right-pin pwm-pin
      1 sound-pwm-index pwm-top!
      0 sound-pwm-index pwm-counter-compare-a!
      0 sound-pwm-index pwm-counter-compare-b!
      0 1 sound-pwm-index pwm-clock-div!
      sound-pwm-index bit enable-pwm
      self 1 ['] run-sound 256 128 768 core spawn-on-core { sound-task }
      c" sound" sound-task task-name!
      sound-task run
    ; define init-sound

    \ Play a tone
    :noname ( tick D: pitch self -- )
      tone-size [: { tick D: pitch self buffer }
        tick buffer tone-end-tick !
        pitch buffer tone-pitch 2!
        buffer tone-size self tone-chan send-chan
      ;] with-aligned-allot
    ; define do-play-tone

    \ Run sound generation
    :noname { self -- }
      0,0 { D: current-pitch }
      begin
        self find-next-tone if { tick D: pitch }
          tick systick::systick-counter - 0> if
            pitch current-pitch d<> if
              pitch self set-pitch
              pitch to current-pitch
            then
          else
            self retire-tone
          then
          false self fetch-tone
        else
          2drop drop
          current-pitch d0> if
            0,0 self set-pitch
            0,0 to current-pitch
          then
          true self fetch-tone
        then
        pause
      again
    ; define run-sound

    \ Fetch a tone
    :noname ( block? self -- )
      tone-size [: { block? self buffer }
        block? { get-tone? }
        block? not if
          self tone-chan chan-empty? not to get-tone?
        then
        get-tone? if
          buffer tone-size self tone-chan recv-chan tone-size = if
            buffer tone-end-tick @ buffer tone-pitch 2@ self add-tone
          then
        then
      ;] with-aligned-allot
    ; define fetch-tone

    \ Add a tone
    :noname { tick D: pitch self -- }
      systick::systick-counter { current-tick }
      pitch d0<= if exit then
      tick current-tick - 0<= if exit then
      max-tone-count 0 ?do
        i 2 * cells self tone-pitches + { pitch-addr }
        pitch-addr 2@ { D: this-pitch }
        this-pitch d0<= if
          tick i cells self tone-end-ticks + !
          pitch pitch-addr 2!
          self tone-count @ 1+ max-tone-count min self tone-count !
          unloop exit
        then
      loop
      0 -1 { check-tick index }
      max-tone-count 0 ?do
        i cells self tone-end-ticks + @ { this-tick }
        tick 0= this-tick current-tick - check-tick current-tick - < or if
          this-tick to check-tick
          i to index
        then
      loop
      index 0>= if
        tick index cells self tone-end-ticks + !
        pitch index 2 * cells self tone-pitches + 2!
      then
    ; define add-tone
    
    \ Find the next tone
    :noname { self -- tone-end-tick D: tone-pitch found? }
      self tone-count @ 0= if 0 0,0 false exit then
      systick::systick-counter { current-tick }
      0 0,0 { tick D: pitch }
      max-tone-count 0 ?do
        i cells self tone-end-ticks + @ { this-tick }
        tick 0= this-tick current-tick - tick current-tick - < or if
          i 2 * cells self tone-pitches + 2@ { D: this-pitch }
          this-pitch d0> if
            this-tick to tick
            this-pitch to pitch
          then
        then
      loop
      pitch d0> if tick pitch true else 0 0,0 false then
    ; define find-next-tone

    \ Retire a tone
    :noname { self -- }
      systick::systick-counter { current-tick }
      0 -1 { tick index }
      max-tone-count 0 ?do
        i cells self tone-end-ticks + @ { this-tick }
        tick 0= this-tick current-tick - tick current-tick - < or if
          i 2 * cells self tone-pitches + 2@ { D: this-pitch }
          this-pitch d0> if
            tick current-tick - 0<= if
              this-tick to tick
              i to index
            then
          then
        then
      loop
      index 0>= if
        0 index cells self tone-end-ticks + !
        0,0 index 2 * cells self tone-pitches + 2!
        self tone-count @ 1- 0 max self tone-count !
      then
    ; define retire-tone
    
    \ Set the pitch for the left and right speakers
    :noname { D: pitch self -- }
      sound-pwm-index bit disable-pwm
      pitch d0> if
        pitch calculate-closest-dividers
        dup sound-pwm-index pwm-top!
        2/ dup sound-pwm-index pwm-counter-compare-a!
        sound-pwm-index pwm-counter-compare-b!
        sound-pwm-index pwm-clock-div!
      else
        1 sound-pwm-index pwm-top!
        0 sound-pwm-index pwm-counter-compare-a!
        0 sound-pwm-index pwm-counter-compare-b!
        0 1 sound-pwm-index pwm-clock-div!    
      then
      sound-pwm-index bit enable-pwm
    ; define set-pitch
    
  end-implement

  continue-module picocalc-sound-internal
    
    \ Shared sound object
    <picocalc-sound> class-size buffer: shared-sound

    \ Initialize the sound object
    : do-init-sound ( -- )
      <picocalc-sound> shared-sound init-object
      picocalc-tasks-core shared-sound init-sound
    ;
    initializer do-init-sound
    
  end-module

  \ Play a tone
  : play-tone ( end-tick D: pitch -- ) shared-sound do-play-tone ;

  \ Play a tone for a duration
  : play-tone-for-duration { duration D: pitch -- }
    duration systick::systick-counter + pitch play-tone
  ;

  \ Play a beep
  : beep ( -- ) beep-duration-ticks beep-pitch play-tone-for-duration ;
  
end-module
