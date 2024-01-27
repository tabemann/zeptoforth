\ Copyright (c) 2023 Travis Bemann
\ Copyright (c) 2023 Ralph W. Lundvall
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

begin-module music

  oo import
  pin import
  pwm import
  timer import
  
  begin-module music-internal

    \ RP2040 system frequency
    125000000 constant FSYS

    \ calculate closest dividers for a PWM frequency
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
      FSYS s>f 2swap f/
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

  \ Lyric "pitch"
  65535 constant lyric-pitch

  begin-structure note-size

    \ Note length in ms
    hfield: note-length

    \ Note pitch in Hz
    hfield: note-pitch
    
  end-structure

  \ Write a lyric to a part
  : lyric, { c-addr u -- }
    u h, lyric-pitch h, c-addr u + c-addr ?do i c@ c, loop cell align,
  ;
  
  \ Integrate a lyric string
  : lyric" ( "string" -- )
    [immediate]
    state @ if
      [char] " compile-cstring postpone count postpone lyric,
    else
      [char] " internal::parse-to-char lyric,
    then
  ;
  
  \ Integrate a single lyric newline
  : lyric-cr ( -- ) s\" \r\n" lyric, ;

  begin-structure part-size

    \ The start of a part
    field: part-start

    \ The end of a part
    field: part-end

  end-structure
  
  <object> begin-class <voice>

    continue-module music-internal
      
      \ The pin for the voice
      cell member pin-index
      
      \ The PWM slice for the voice
      cell member pwm-index

      \ The start of the voice
      cell member voice-start

      \ The end of the voice
      cell member voice-end

      \ The current notea
      cell member current-note
      
      \ The current note's start
      cell member note-start

      \ Are we playing
      cell member playing-voice

      \ Start a tone
      method tone-on ( S31.S32-frequency-in-Hz voice -- )
      
      \ End a tone
      method tone-off ( voice -- )

      \ Play a note at a given time in micrseconds
      method play-note ( us voice -- )
    
    end-module

    \ Play a voice
    method play-voice ( us voice -- )
    
    \ Is a voice playing
    method voice-playing? ( voice -- playing? )
    
  end-class

  <voice> begin-implement

    \ Initialize a voice
    :noname
      { part new-pwm-index new-pin-index voice -- }
      voice <object>->new
      new-pin-index voice pin-index !
      new-pwm-index voice pwm-index !
      part @ voice voice-end !
      part cell+ voice voice-start !
      voice voice-start @ voice current-note !
      us-counter-lsb voice note-start !
      false voice playing-voice !
    ; define new
    
    \ Start a tone
    :noname { D: S31.32-frequency-in-Hz voice -- }
      voice pwm-index @ { my-pwm-index }
      my-pwm-index bit disable-pwm
      0 my-pwm-index pwm-counter!

      \ Freq-PWM = Fsys / period
      \ period = (TOP+1) * (CSR_PH_CORRECT+1) + (DIV_INT + DIV_FRAC/16)
      \ e.g. 125 MHz / 16.0 = 7.8125 MHz rate base rate
      \ divider is 8 bit integer part, 4 bit fractional part
      \ Since phase correct is false/0, we only need to worry about TOP and
      \ Divider
      S31.32-frequency-in-Hz calculate-closest-dividers
      dup ( pwm-wrap-count ) my-pwm-index pwm-top!
      2/ ( 50% of pwm-wrap-count ) my-pwm-index pwm-counter-compare-a!
      ( frac-divider int-divider ) my-pwm-index pwm-clock-div!

      my-pwm-index free-running-pwm
      false my-pwm-index pwm-phase-correct!
      my-pwm-index bit enable-pwm
      voice pin-index @ pwm-pin
    ; define tone-on

    \ End a tone
    :noname { voice -- }
      voice pin-index @ input-pin
      voice pwm-index @ bit disable-pwm
    ; define tone-off
    
    \ Play a note at a given time in micrseconds
    :noname { us voice -- }
      begin
        voice current-note @ voice voice-end @ < if
          us voice note-start !
          voice current-note @ note-pitch h@ ?dup if
            dup lyric-pitch = if
              drop
              voice current-note @ note-length h@ { chars }
              cell voice current-note +!
              voice current-note @ chars type
              voice current-note @ chars + cell align voice current-note !
              false
            else
              s>f voice tone-on true
            then
          else
            voice tone-off true
          then
        else
          voice tone-off true
        then
      until
    ; define play-note

    \ Play a voice
    :noname { us voice -- }
      voice playing-voice @ if
        voice current-note @ voice voice-end @ < if
          us voice note-start @ -
          voice current-note @ note-length h@ 1000 * > if
            note-size voice current-note +!
            us voice play-note
          then
        else
          false voice playing-voice !
          voice tone-off
        then
      else
        voice voice-start @ voice voice-end @ <> if
          true voice playing-voice !
          voice voice-start @ voice current-note !
          us voice play-note
        else
          false voice playing-voice !
          voice tone-off
        then
      then
    ; define play-voice

    \ Is a voice playing?
    :noname ( voice -- playing? ) playing-voice @ ; define voice-playing?
    
  end-implement

  \ Play a set of voices
  : play-voices ( voicen ... voice0 count -- )
    dup cells [: { count array }
      count 0 ?do array i cells + ! loop
      us-counter-lsb { us }
      count 0 ?do us array i cells + @ play-voice loop
      begin
        false { playing? }
        us-counter-lsb to us
        count 0 ?do
          array i cells + @ { voice }
          voice voice-playing? if
            true to playing?
            us voice play-voice
          then
        loop
        playing? not
      until
    ;] with-aligned-allot
  ;

  \ Start a part
  : begin-part ( "name" -- addr ) create reserve ;

  \ End a part
  : end-part ( addr -- ) here swap current! ;

  \ The note pitches
  19 constant D0# 21 constant E0 22 constant F0
  23 constant F0# 24 constant G0 26 constant G0#
  28 constant A0 29 constant A0# 31 constant B0
  33 constant C1 35 constant C1# 37 constant D1
  39 constant D1# 41 constant E1 44 constant F1
  46 constant F1# 49 constant G1 52 constant G1#
  55 constant A1 58 constant A1# 62 constant B1
  65 constant C2 69 constant C2# 73 constant D2
  78 constant D2# 82 constant E2 87 constant F2
  92 constant F2# 98 constant G2 104 constant G2#
  110 constant A2 117 constant A2# 123 constant B2
  131 constant C3 139 constant C3# 147 constant D3
  156 constant D3# 165 constant E3 175 constant F3
  185 constant F3# 196 constant G3 208 constant G3#
  220 constant A3 233 constant A3# 247 constant B3
  262 constant C4 277 constant C4# 294 constant D4 
  311 constant D4# 330 constant E4 349 constant F4 
  370 constant F4# 392 constant G4 415 constant G4# 
  440 constant A4 466 constant A4# 494 constant B4 
  523 constant C5 554 constant C5# 587 constant D5 
  622 constant D5# 659 constant E5 698 constant F5 
  740 constant F5# 784 constant G5 831 constant G5# 
  880 constant A5 932 constant A5# 988 constant B5 
  1047 constant C6 1109 constant C6# 1175 constant D6 
  1245 constant D6# 1319 constant E6 1397 constant F6 
  1480 constant F6# 1568 constant G6 1661 constant G6# 
  1760 constant A6 1865 constant A6# 1976 constant B6 
  2093 constant C7 2217 constant C7# 2349 constant D7 
  2489 constant D7# 2637 constant E7 2794 constant F7 
  2960 constant F7# 3136 constant G7 3322 constant G7# 
  3520 constant A7 3729 constant A7# 3951 constant B7 
  4186 constant C8 4435 constant C8# 
  
  \ 5 octaves of the 5 black keys ( flats )
  \ D, E, G, A, B
  : D2B C2# ; : E2B D2# ; : G2B F2# ; : A2B G2# ; : B2B A2# ;
  : D3B C3# ; : E3B D3# ; : G3B F3# ; : A3B G3# ; : B3B A3# ;
  : D4B C4# ; : E4B D4# ; : G4B F4# ; : A4B G4# ; : B4B A4# ;
  : D5B C5# ; : E5B D5# ; : G5B F5# ; : A5B G5# ; : B5B A5# ;
  : D6B C6# ; : E6B D6# ; : G6B F6# ; : A6B G6# ; : B6B A6# ;

  0 constant rst

  2000000 value ~mpm 

  240000000 value ~mscale  

  0 value ~nbump

  : BPM   ~mscale SWAP / to ~mpm ;
  : BPM?   ~mscale ~mpm / . ;
  : bump?   ~nbump ." Transpose set to " . ;
  : bump    to ~nbump ;

  : nbump { half-steps start-hz -- hz }
    2,0 half-steps s>f 12,0 f/ f** start-hz s>f f* f>s
  ;

  : note ( hz ms -- ) h, h, ;
  : rest ( ms --) h, 0 h, ;

  : ntr  ( hz ms -- new hz ms ) swap ~nbump swap nbump swap ; 

  \ Note periods; place these after the note pitches
  : .1  ( n -- ) ~mpm 1000 / ntr note ;
  : .2   ~mpm 2000 / ntr note ;
  : .4   ~mpm 4000 / ntr note ;
  : .8   ~mpm 8000 / ntr note ;
  : .12   ~mpm 12000 / ntr NOTE ;
  : .16   ~mpm 16000 / ntr note ;
  : .32   ~mpm 32000 / ntr note ;
  : .64   ~mpm 64000 / ntr note ;
  : .4.   ~mpm 2667 / ntr note ;
  : .2.   ~mpm 1333 / ntr note ;
  : .8.   ~mpm 5333 / ntr note ; 
  : .28   ~mpm 1600 / ntr note ; 
  : .3   ~mpm 3000 / ntr note ;
  : .6   ~mpm 6000 / ntr note ;
  : .9   ~mpm 9000 / ntr note ;

  \ Define rests  e.g.  rst ,4 -- a quarter note rest
  : ,1  ( n -- ) ~mpm 1000 / rest drop ;
  : ,2   ~mpm 2000 / rest drop ;
  : ,4   ~mpm 4000 / rest drop ;
  : ,8   ~mpm 8000 / rest drop ;
  : ,12   ~mpm 12000 / rest drop ;
  : ,16   ~mpm 16000 / rest drop ;
  : ,32   ~mpm 32000 / rest drop ;
  : ,64   ~mpm 64000 / rest drop ;
  : ,4.   ~mpm 2667 / rest drop ;
  : ,2.   ~mpm 1333 / rest drop ;
  : ,8.   ~mpm 5333 / rest drop ; 
  : ,28   ~mpm 1600 / rest drop ; 
  : ,3   ~mpm 3000 / rest drop ;
  : ,6   ~mpm 6000 / rest drop ; 


  : |   ( space for double notes ) ~mpm 128000 / rest ;

  : |0  ( no ops for visual input help ) ;
  : |1  ;
  : |2  ;
  : |3  ;
  : |4  ;
  : |5  ;
  : |6  ;
  : |7  ;
  : |8  ;
  : |9  ;
  : |A  ;
  : |B  ;
  : |C  ;
  : |D  ;
  : |E  ;
  : |F  ; 

end-module