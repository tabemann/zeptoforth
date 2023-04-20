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

begin-module console

  stream import
  closure import

  \ End of input exception
  : x-end-of-input ( -- ) ." end of input" cr ;

  begin-module console-internal

    \ Data associated with stream input and output
    begin-structure console-stream-data-size
      
      \ A closure associated with KEY or EMIT
      closure-size +field console-io

      \ A closure associated with KEY? or EMIT?
      closure-size +field console-io?
      
      \ The stream associated with the input or output
      field: console-stream

      \ A buffer associated with the input or output
      field: console-buffer

    end-structure

    \ Initialize console stream data for input
    : init-console-stream-input { stream data -- }
      data data console-stream !
      data data console-io [: { data }
        data console-buffer 1 data console-stream @ recv-stream
        0> averts x-end-of-input
        data console-buffer c@
      ;] bind
      data data console-io? [: { data }
        data console-stream @ stream-empty? not
      ;] bind
    ;

    \ Initialize console stream data for output
    : init-console-stream-output { stream data -- }
      data data console-stream !
      data data console-io [: { byte data }
        byte data console-buffer c!
        data console-buffer 1 data console-stream @ send-stream
      ;] bind
      data data console-io? [: { data }
        data console-stream @ stream-full? not
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

  \ Set the current input to a stream within an xt
  : with-stream-input ( stream xt -- )
    console-stream-data-size [: { data }
      swap data init-console-stream-input
      data console-stream-io data console-stream-io? rot with-input
    ;] with-aligned-allot
  ;

  \ Set the current output to a stream within an xt
  : with-stream-output ( stream xt -- )
    console-stream-data-size [: { data }
      swap data init-console-stream-output
      data console-stream-io data console-stream-io? rot with-output
    ;] with-aligned-allot
  ;

end-module