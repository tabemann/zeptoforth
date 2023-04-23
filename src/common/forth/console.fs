\ Copyright (c) 2023 Travis Bemann
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

begin-module console

  task import
  stream import
  closure import
  slock import
  alarm import

  \ End of input exception
  : x-end-of-input ( -- ) ." end of input" cr ;

  begin-module console-internal

    \ Console alarm delay in ticks
    500 constant console-alarm-delay

    \ Console buffer size
    128 constant console-buffer-size

    \ Data associated with stream input and output
    begin-structure console-stream-data-size
      
      \ A closure associated with KEY or EMIT
      closure-size +field console-io

      \ A closure associated with KEY? or EMIT?
      closure-size +field console-io?
      
      \ The stream associated with the input or output
      field: console-stream

      \ Buffer start index
      field: console-start

      \ Buffer end index
      field: console-end
      
      \ Console lock
      slock-size +field console-slock

      \ Console alarm
      alarm-size +field console-alarm

      \ Console alarm set
      field: console-alarm-set?

      \ A buffer associated with the input or output
      console-buffer-size +field console-buffer

    end-structure

    \ Initialize console stream data for input
    : init-console-stream-input { stream data -- }
      data console-slock init-slock
      0 data console-start !
      0 data console-end !
      false data console-alarm-set? !
      stream data console-stream !
      data data console-io [:
        [: { data }
          data console-start @ data console-end @ = if
            data console-buffer console-buffer-size
            data console-stream @ recv-stream
            dup 0> averts x-end-of-input
            data console-end !
            0 data console-start !
          then
          data console-buffer data console-start @ + c@
          1 data console-start +!
        ;] over console-slock with-slock
      ;] bind
      data data console-io? [: { data }
        data console-stream @ stream-empty? not
        data console-start @ data console-end @ <> or
      ;] bind
    ;

    \ Initialize console stream data for output
    : init-console-stream-output { stream data -- }
      data console-slock init-slock
      0 data console-start !
      0 data console-end !
      false data console-alarm-set? !
      stream data console-stream !
      data data console-io [:
        [: { byte data }
          byte data console-buffer data console-end @ + c!
          1 data console-end +!
          data console-end @ console-buffer-size < if
            data console-alarm-set? @ not if
              true data console-alarm-set? !
              console-alarm-delay
              current-task task-priority@
              data
              [: drop
                [: { data }
                  data console-buffer data console-end @
                  data console-stream @ send-stream-parts
                  0 data console-end !
                  false data console-alarm-set? !
                ;] over console-slock with-slock
              ;]
              data console-alarm
              set-alarm-delay-default
            then
          else
            data console-alarm-set? @ if data console-alarm unset-alarm then
            data console-buffer console-buffer-size
            data console-stream @ send-stream-parts
            0 data console-end !
            false data console-alarm-set? !
          then
        ;] over console-slock with-slock
      ;] bind
      data data console-io? [: { data }
        data console-stream @ stream-full? not
        data console-end @ console-buffer-size <> or
      ;] bind
    ;

  end-module> import
  
  \ Set the current input within an xt
  : with-input ( input-hook input?-hook xt -- )
    key-hook @ key?-hook @ { saved-input-hook saved-input?-hook }
    [:
      swap key?-hook ! swap key-hook ! execute
    ;] try saved-input?-hook key?-hook ! saved-input-hook key-hook ! ?raise
  ;

  \ Set the current output within an xt
  : with-output ( output-hook output?-hook xt -- )
    emit-hook @ emit?-hook @ { saved-output-hook saved-output?-hook }
    [:
      swap emit?-hook ! swap emit-hook ! execute
    ;] try saved-output?-hook emit?-hook ! saved-output-hook emit-hook ! ?raise
  ;

  \ Set the current input to null within an xt
  : with-null-input ( xt -- )
    key-hook @ key?-hook @ { saved-input-hook saved-input?-hook }
    [:
      ['] false key?-hook ! ['] false key-hook ! execute
    ;] try saved-input?-hook key?-hook ! saved-input-hook key-hook ! ?raise
  ;

  \ Set the current output to null within an xt
  : with-null-output ( xt -- )
    emit-hook @ emit?-hook @ { saved-output-hook saved-output?-hook }
    [:
      ['] true emit?-hook ! ['] drop emit-hook ! execute
    ;] try saved-output?-hook emit?-hook ! saved-output-hook emit-hook ! ?raise
  ;

  \ Set the curent input to serial within an xt
  : with-serial-input ( xt -- )
    ['] int-io::int-io-internal::do-key
    ['] int-io::int-io-internal::do-key?
    rot with-input
  ;

  \ Set the current output to serial within an xt
  : with-serial-output ( xt -- )
    ['] int-io::int-io-internal::do-emit
    ['] int-io::int-io-internal::do-emit?
    rot with-output
  ;

  \ Set the current input to a stream within an xt
  : with-stream-input ( stream xt -- )
    console-stream-data-size [: { data }
      swap data init-console-stream-input
      data console-io data console-io? rot with-input
    ;] with-aligned-allot
  ;

  \ Set the current output to a stream within an xt
  : with-stream-output ( stream xt -- )
    console-stream-data-size [: { data }
      swap data init-console-stream-output
      data console-io data console-io? rot with-output
    ;] with-aligned-allot
  ;

end-module

compile-to-ram
