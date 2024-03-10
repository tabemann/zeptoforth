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

begin-module profile

  \ Profile map size must be a power of two
  : x-profile-map-size-not-power-of-two ( -- )
    ." profile map size not a power of two" cr
  ;

  \ Profile map is already initialized
  : x-profile-map-already-inited ( -- )
    ." profile map already initialized" cr
  ;

  begin-module profile-internal

    \ Profile map size
    0 value profile-map-size

    \ The profile map address
    0 value profile-map-addr

    \ The profile map end address
    0 value profile-map-end-addr

    \ Profile map mask
    0 value profile-map-mask

    \ Validate profile map size
    : validate-profile-map-size { size -- }
      1 begin ?dup while
        dup size = if drop exit then
        1 lshift
      repeat
      ['] x-profile-map-size-not-power-of-two ?raise
    ;
    
    \ Record entering a word
    : enter-word { xt -- }
      profile-map-addr 0= if exit then
      xt 3 lshift profile-map-mask and profile-map-addr +
      begin
        dup @ dup 0<> swap xt <> and
      while
        [ 2 cells ] literal +
        dup profile-map-end-addr = if drop profile-map-addr then
      repeat
      xt over !
      cell+ 1 over +!
      dup @ 0<> if drop else -1 swap ! then
    ;
  
  end-module> import

  \ Initialize the profiler
  : init-profile { size -- }
    size validate-profile-map-size
    profile-map-addr 0= averts x-profile-map-already-inited
    size to profile-map-size
    size 2 cells * 1- to profile-map-mask
    here to profile-map-addr size 2 cells * allot
    profile-map-addr profile-map-size 2 cells * + to profile-map-end-addr
    profile-map-addr profile-map-size 2 cells * 0 fill
  ;

  \ Dump the profiled data
  : dump-profile ( -- )
    profile-map-addr begin dup profile-map-end-addr < while
      dup @ { xt }
      cell+ dup @ { times }
      cell+
      times 0<> if
        cr xt h.8 space
        xt flash-latest internal::find-dict-by-xt ?dup if
          internal::word-name count ?dup if type else drop ." <no name>" then
        else
          xt ram-latest internal::find-dict-by-xt ?dup if
            internal::word-name count ?dup if type else drop ." <no name>" then
          else
            ." <no name>"
          then
        then
        space times u.
      then
    repeat
    drop
  ;

end-module

\ Profile an anonymous word
: :noname
  :noname
  \ We find the xt by subtracting 2 for a PUSH {LR} instruction from HERE
  here 2 - lit,
  postpone profile::profile-internal::enter-word
;

\ Profile a normal word
: :
  :
  \ We find the xt by subtracting 2 for a PUSH {LR} instruction from HERE
  here 2 - lit,
  postpone profile::profile-internal::enter-word
;
