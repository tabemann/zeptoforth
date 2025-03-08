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

begin-module inter-fs-copy

  oo import
  fat32 import

  begin-module inter-fs-copy-internal

    fat32-tools::fat32-tools-internal import
    
    read-buffer constant read-buffer
    read-buffer-size constant read-buffer-size
    fs-lock constant fs-lock
    
    \ Actually copy a file between two filesystems
    : do-copy-file-to-fs { src-file dest-file -- }
      begin
        read-buffer read-buffer-size src-file read-file dup 0> if
          read-buffer swap dest-file write-file drop false
        else
          drop true
        then
      until
      dest-file file-fs@ flush
    ;
    
  end-module> import
  
  \ Copy a file between two filesystems
  : copy-file-to-fs ( D: src-path src-fs D: dest-path dest-fs -- )
    [:
      { D: src-path src-fs D: dest-path dest-fs }
      dest-path dest-fs src-path [: { D: dest-path dest-fs src-file }
        src-file dest-path ['] do-copy-file-to-fs dest-fs
        with-create-file-at-root-path
      ;] src-fs with-open-file-at-root-path
    ;] fs-lock lock::with-lock
  ;
  
end-module
