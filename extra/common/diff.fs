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

begin-module diff

  oo import
  fat32 import

  begin-module diff-internal

    \ Before line count
    2 constant before-count

    \ After line count
    5 constant after-count

    \ Displayed after line count
    2 constant display-after-count

    \ Newline
    $0A constant newline

    \ Buffer size
    fat32-internal::sector-size constant buffer-size

    begin-structure file-diff-size

      <fat32-file> class-size +field diff-file
      buffer-size +field diff-buffer
      field: current-line
      field: current-offset
      field: current-buffer-fill-size
      field: before-line-count
      before-count cells +field before-lines
      before-count cells +field before-offsets
      field: after-line-count
      after-count cells +field after-lines
      after-count cells +field after-offsets

    end-structure
      
    begin-structure diff-size

      file-diff-size +field file-diff0
      file-diff-size +field file-diff1
      field: in-diff?
      
    end-structure

    \ Initialize the fields inside a file-diff structure except for the files
    : init-file-diff { file-diff -- }
      0 file-diff current-line !
      0 file-diff current-offset !
      0 file-diff current-buffer-fill-size !
      0 file-diff before-line-count !
      before-count 0 ?do 0 i cells file-diff before-lines + ! loop
      before-count 0 ?do 0 i cells file-diff before-offsets + ! loop
      0 file-diff after-line-count !
      after-count 0 ?do 0 i cells file-diff after-lines + ! loop
      after-count 0 ?do 0 i cells file-diff after-offsets + ! loop
    ;

    \ Initialize the fields inside a diff structure except for the files
    : init-diff { diff -- }
      diff file-diff0 init-file-diff
      diff file-diff1 init-file-diff
      false diff in-diff? !
    ;

    \ Mass-read a file into the buffer
    : slurp-file { file-diff -- }
      0 { len }
      begin len buffer-size < while
        file-diff diff-buffer len + buffer-size len -
        file-diff diff-file read-file ?dup if
          +to len
        else
          len file-diff current-buffer-fill-size ! exit
        then
      repeat
      len file-diff current-buffer-fill-size !
    ;

    \ Search a file for a newline and skip it, returning the offset directly
    \ after the newline
    : search-for-newline { file-diff -- offset }
      begin
        file-diff diff-buffer buffer-size file-diff diff-file read-file ?dup if
          { len }
          len 0 ?do
            file-diff diff-buffer i + c@ newline = if
              file-diff diff-file tell-file len - i + 1+ { offset }
              offset seek-set file-diff diff-file seek-file
              offset exit
            then
          loop
        else
          file-diff diff-file tell-file exit
        then
      again
    ;

    \ Add a new before line, retiring the first if there is not enough room
    : advance-before-lines { file-diff -- }
      file-diff before-line-count @ before-count = if
        file-diff before-line-count @ 1- 0 ?do
          i 1+ cells file-diff before-lines + @
          i cells file-diff before-lines + !
          i 1+ cells file-diff before-offsets + @
          i cells file-diff before-offsets + !
        loop
      then
      file-diff before-line-count @ 1+ before-count min
      file-diff before-line-count !
      file-diff current-line @
      file-diff before-line-count @ 1- cells file-diff before-lines + !
      file-diff current-offset @
      file-diff before-line-count @ 1- cells file-diff before-offsets + !
    ;

    \ Add a new after line, retiring the first if there is not enough room
    : advance-after-lines { file-diff -- }
      file-diff after-line-count @ after-count = if
        file-diff after-line-count @ 1- 0 ?do
          i 1+ cells file-diff after-lines + @
          i cells file-diff after-lines + !
          i 1+ cells file-diff after-offsets + @
          i cells file-diff after-offsets + !
        loop
      then
      file-diff after-line-count @ 1+ after-count min
      file-diff after-line-count !
      file-diff current-line @
      file-diff after-line-count @ 1- cells file-diff after-lines + !
      file-diff current-offset @
      file-diff after-line-count @ 1- cells file-diff after-offsets + !
    ;

    \ Handle a detected difference
    : handle-diff { file-diff -- }
      file-diff current-offset @ seek-set
      file-diff diff-file seek-file
      file-diff search-for-newline file-diff current-offset !
      1 file-diff current-line +!
      0 file-diff after-line-count !
    ;

    \ Advance by one line
    : advance-line { start-index index file-diff -- }
      start-index index + 1+ file-diff current-offset !
      1 file-diff current-line +!
    ;

    \ Advance to the end of a file
    : advance-end { extra file-diff -- }
      begin
        file-diff current-buffer-fill-size @ dup extra - ?do
          i file-diff diff-buffer + c@ { c }
          c newline = if
            file-diff diff-file tell-file file-diff current-buffer-fill-size @ -
            i + 1+ file-diff current-offset !
            1 file-diff current-line +!
          then
        loop
        file-diff slurp-file
        file-diff current-buffer-fill-size @ 0=
      until
      file-diff diff-file tell-file { total-len }
      file-diff current-offset @ total-len <> if
        total-len file-diff current-offset !
        1 file-diff current-line +!
      then
    ;

    \ Check a file for differences, returning whether a difference should be
    \ dumped
    : check-for-diff { diff -- dump-diff? }
      diff file-diff0 diff-file tell-file { start-index0 }
      diff file-diff1 diff-file tell-file { start-index1 }
      diff file-diff0 slurp-file
      diff file-diff1 slurp-file
      diff file-diff0 current-buffer-fill-size @
      diff file-diff1 current-buffer-fill-size @ min { len }
      len 0 ?do
        diff file-diff0 diff-buffer i + c@ { c0 }
        diff file-diff1 diff-buffer i + c@ { c1 }
        c0 c1 <> if
          diff file-diff0 handle-diff
          diff file-diff1 handle-diff
          true diff in-diff? !
          false exit
        else
          c0 newline = if
            diff in-diff? @ if
              diff file-diff0 advance-after-lines
              diff file-diff1 advance-after-lines
              start-index0 i diff file-diff0 advance-line
              start-index1 i diff file-diff1 advance-line
              diff file-diff0 after-line-count @ after-count >=
              diff file-diff1 after-line-count @ after-count >= and if
                false diff in-diff? !
                true exit
              then
            else
              diff file-diff0 advance-before-lines
              diff file-diff1 advance-before-lines
              start-index0 i diff file-diff0 advance-line
              start-index1 i diff file-diff1 advance-line
            then
          then
        then
      loop
      diff file-diff0 current-buffer-fill-size @
      diff file-diff1 current-buffer-fill-size @ <> if
        0 diff file-diff0 after-line-count !
        0 diff file-diff1 after-line-count !
        diff file-diff0 current-buffer-fill-size @
        diff file-diff1 current-buffer-fill-size @ > if
          diff file-diff0 current-buffer-fill-size @
          diff file-diff1 current-buffer-fill-size @ -
          diff file-diff0 advance-end
        else
          diff file-diff1 current-buffer-fill-size @
          diff file-diff0 current-buffer-fill-size @ -
          diff file-diff1 advance-end
        then
        true diff in-diff? !
        true
      else
        false
      then
    ;

    \ Check if we are at the end of a file
    : check-for-end { diff -- end? }
      diff file-diff0 current-offset @ diff file-diff0 file-size@ =
      diff file-diff1 current-offset @ diff file-diff1 file-size@ = or
    ;

    \ Transfer the last after lines to the before lines
    : transfer-after-to-before { file-diff -- }
      before-count file-diff after-line-count @ min
      file-diff before-line-count !
      file-diff before-line-count @ 0 ?do
        file-diff after-line-count @
        file-diff before-line-count @ - i + { after-index }
        after-index cells file-diff after-lines + @
        i cells file-diff before-lines + !
        after-index cells file-diff after-offsets + @
        i cells file-diff before-offsets + !
      loop
      0 file-diff after-line-count !
      file-diff before-line-count @ 0> if
        file-diff before-offsets file-diff before-line-count @ 1- cells + @
        seek-set file-diff diff-file seek-file
        file-diff search-for-newline file-diff current-offset !
        file-diff before-lines file-diff before-line-count @ 1- cells + @ 1+
        file-diff current-line !
      then
    ;

    \ Dump a line to the console
    : dump-line { file-diff -- }
      begin
        file-diff slurp-file
        file-diff current-buffer-fill-size @ 0 ?do
          i file-diff diff-buffer + c@ dup newline <> if
            emit
          else
            drop
            file-diff current-buffer-fill-size @ i 1+ - negate seek-cur
            file-diff diff-file seek-file
            exit
          then
        loop
        file-diff current-buffer-fill-size @ buffer-size <>
        file-diff current-buffer-fill-size @ 0= or if exit then
      again
    ;

    \ Dump multiple lines to the console, preceding them with a specified prefix
    : dump-lines { file-diff D: prefix -- }
      file-diff before-offsets @ { start-offset }
      file-diff before-lines @ { start-line }
      file-diff after-line-count @ display-after-count min { after-line-count' }
      after-line-count' 0> if
        file-diff after-offsets after-line-count' cells + @
        file-diff after-lines after-line-count' cells + @
      else
        file-diff current-offset @
        file-diff current-line @
      then
      { end-offset end-line }
      start-offset seek-set file-diff diff-file seek-file
      end-line start-line ?do prefix type file-diff dump-line cr loop
      end-offset seek-set file-diff diff-file seek-file
    ;

    \ Generate a before line
    : generate-before { file-diff -- line }
      file-diff before-line-count @ 0> if file-diff before-lines @ else 0 then
    ;

    \ Generate an after line
    : generate-after { file-diff -- line }
      file-diff after-line-count @ 0> if
        file-diff after-line-count @ display-after-count min { count }
        file-diff after-lines count 1- cells + @
        file-diff after-offsets count 1- cells + @
        file-diff diff-file file-size@ = if 1- then
      else
        file-diff current-line @
        file-diff current-offset @
        file-diff diff-file file-size@ = if 1- then
      then
    ;
    
    \ Dump a difference
    : dump-diff { diff -- }
      diff file-diff0 generate-before 1+ { before }
      diff file-diff0 generate-after 1+ { after }
      before after <> if
        before (.) ." ," after (.)
      else
        before (.)
      then
      ." c"
      diff file-diff1 generate-before 1+ { before }
      diff file-diff1 generate-after 1+ { after }
      before after <> if
        before (.) ." ," after (.)
      else
        before (.)
      then
      cr
      diff file-diff0 s" < " dump-lines
      ." ---" cr
      diff file-diff1 s" > " dump-lines
      diff file-diff0 transfer-after-to-before
      diff file-diff1 transfer-after-to-before
      false diff in-diff? !
    ;

    \ The outer loop of generating differences
    : do-diff { diff -- }
      diff init-diff
      cr
      begin
        diff check-for-diff if
          diff dump-diff false
        else
          diff check-for-end
        then
      until
      diff in-diff? @ if
        diff dump-diff
      then
    ;
    
  end-module> import

  \ Take a difference between two files which may exist on different filesystems
  : inter-fs-diff ( D: path0 fs0 D: path1 fs1 -- )
    diff-size [:
      { D: path0 fs0 D: path1 fs1 diff }
      diff file-diff0 diff-file path0 ['] clone-file
      fs0 with-open-file-at-root-path
      path1 fs1 diff [:
        { D: path1 fs1 diff }
        diff file-diff1 diff-file path1 ['] clone-file
        fs1 with-open-file-at-root-path
        diff ['] do-diff try
        diff file-diff1 diff-file close-file
        diff file-diff1 diff-file destroy
        ?raise
      ;] try
      diff file-diff0 diff-file close-file
      diff file-diff0 diff-file destroy
      ?raise
    ;] with-aligned-allot
  ;

  \ Take a difference between two files on the current filesystem
  : diff { D: path0 D: path1 -- }
    path0 fat32-tools::current-fs@ path1 fat32-tools::current-fs@ inter-fs-diff
  ;
  
end-module
