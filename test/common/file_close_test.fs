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

begin-module file-close-test

  fat32-tools import

  : do-create-file ( -- )
    cr ." Creating FRED.TXT"
    s\" cr .\" This is FRED.TXT\"" s" FRED.TXT" create-file
  ;

  : do-remove-file ( -- )
    cr ." Removing FRED.TXT"
    s" FRED.TXT" remove-file
  ;
  
  : run-test ( -- )
    cr ." Checking for existence"
    s" FRED.TXT" exists? if
      do-remove-file
    then
    
    do-create-file
    cr ." Running INCLUDED"
    s" FRED.TXT" included
    do-remove-file
    
    do-create-file
    cr ." Running WRITE-FILE"
    s\" cr .\" This is FRED.TXT\"" s" FRED.TXT" write-file
    do-remove-file
    
    do-create-file
    cr ." Running COPY-FILE"
    s" FRED.TXT" s" FRED1.TXT" copy-file
    do-remove-file
    cr ." Removing FRED1.TXT"
    s" FRED1.TXT" remove-file
    
    do-create-file
    cr ." Running APPEND-FILE"
    s"  cr" s" FRED.TXT" append-file
    do-remove-file
    
    do-create-file
    cr ." Running WRITE-FILE-WINDOW"
    s" cr" 0 s" FRED.TXT" write-file-window
    do-remove-file
    
    do-create-file
    cr ." Running LIST-FILE"
    cr s" FRED.TXT" list-file
    do-remove-file
    
    do-create-file
    cr ." Running LIST-FILE-WINDOW"
    cr 0 1024 s" FRED.TXT" list-file-window
    do-remove-file
    
    do-create-file
    cr ." Running DUMP-FILE-RAW"
    cr s" FRED.TXT" dump-file-raw
    do-remove-file
    
    do-create-file
    cr ." Running DUMP-FILE-RAW-WINDOW"
    cr 0 1024 s" FRED.TXT" dump-file-raw-window
    do-remove-file
    
    do-create-file
    cr ." Running DUMP-FILE"
    cr s" FRED.TXT" dump-file
    do-remove-file
    
    do-create-file
    cr ." Running DUMP-FILE-WINDOW"
    cr 0 1024 s" FRED.TXT" dump-file-window
    do-remove-file

    do-create-file
    cr ." Running DUMP-FILE-ASCII"
    cr s" FRED.TXT" dump-file-ascii
    do-remove-file
    
    do-create-file
    cr ." Running DUMP-FILE-ASCII-WINDOW"
    cr 0 1024 s" FRED.TXT" dump-file-ascii-window
    do-remove-file

    do-create-file
    cr ." Running DUMP-FILE-HALFS"
    cr s" FRED.TXT" dump-file-halfs
    do-remove-file
    
    do-create-file
    cr ." Running DUMP-FILE-HALFS-WINDOW"
    cr 0 1024 s" FRED.TXT" dump-file-halfs-window
    do-remove-file

    do-create-file
    cr ." Running DUMP-FILE-CELLS"
    cr s" FRED.TXT" dump-file-cells
    do-remove-file
    
    do-create-file
    cr ." Running DUMP-FILE-CELLS-WINDOW"
    cr 0 1024 s" FRED.TXT" dump-file-cells-window
    do-remove-file

    1024 [: { buffer }
      do-create-file
      cr ." Running READ-FILE"
      cr buffer 1024 0 s" FRED.TXT" read-file buffer swap type
      do-remove-file
    ;] with-allot

    do-create-file
    cr ." Running FILE-SIZE@"
    cr s" FRED.TXT" file-size@ .
    do-remove-file

    do-create-file
    cr ." Running WITH-FILE-INPUT"
    cr s" FRED.TXT" [:
      begin key? while key emit repeat
    ;] with-file-input
    do-remove-file

    do-create-file
    cr ." Running WITH-FILE-OUTPUT"
    s" FRED.TXT" [: ." This is only a test, repeat, this is only a test!" ;]
    with-file-output

    do-create-file
    cr ." Running WITH-FILE-ERROR-OUTPUT"
    s" FRED.TXT" [: ." This is only a test, repeat, this is only a test!" ;]
    with-file-error-output

  ;
  
end-module
