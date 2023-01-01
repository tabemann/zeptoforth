\ Copyright (c) 2022-2023 Travis Bemann
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

begin-module fs-test
  
  oo import
  file import
  ramfs import
  heap import
  
  1024 constant heap-size
  4 cells constant heap-block-size
  heap-size heap-block-size / constant heap-block-count
  heap-block-size heap-block-count heap-size buffer: my-heap
  
  256 constant my-file-0-size
  
  1024 constant my-file-1-size
  
  my-file-0-size buffer: my-file-0-buffer
  
  my-file-1-size buffer: my-file-1-buffer
  
  <ram-real-file> class-size buffer: my-file-0
  
  <ram-real-file> class-size buffer: my-file-1
  
  <ram-real-dir> class-size buffer: my-sub-dir
  
  <ram-real-dir> class-size buffer: my-root-dir
  
  128 constant my-mount-file-size

  my-mount-file-size buffer: my-mount-file-buffer
  
  <ram-real-file> class-size buffer: my-mount-file
  
  <ram-real-dir> class-size buffer: my-mount-dir
  
  <ram-fs> class-size buffer: my-mount-fs
  
  <ram-real-mount> class-size buffer: my-mount
  
  <ram-fs> class-size buffer: my-parent-fs
  
  <file-context> class-size buffer: my-context
  
  : file-size@ ( file -- ) 0 seek-end 2 pick seek-file tell-file ;
  
  defer list-dirs
  defer get-dir
  :noname ( level dir -- )
    0 begin
      2dup swap entity-name@ dup if
        cr 4 pick 0 ?do ."     " loop
        2dup type
        4 pick 4 pick 2swap [: swap 1+ swap list-dirs ;] get-dir
        1+ false
      else
        2drop 2drop drop true
      then
    until
  ; ' list-dirs defer!
  
  :noname ( level dir c-addr u xt -- )
    swap 2swap rot rot ( level xt c-addr u dir )
    [:
      dup entity-file? if
        file-size@ ." : " . ." bytes" 2drop
      else
        dup entity-dir? if
          swap execute
        else
          2drop drop
        then
      then
    ;] swap with-entity
  ; ' get-dir defer!
    
  : init-test ( -- )
    init-files
    heap-block-size heap-block-count my-heap init-heap
    my-mount-file-buffer my-mount-file-size 0 s" mount-file" <ram-real-file> my-mount-file init-object
    my-mount-file 0 s" " <ram-real-dir> my-mount-dir init-object
    my-mount-dir <ram-fs> my-mount-fs init-object
    my-file-0-buffer my-file-0-size 0 s" file-0" <ram-real-file> my-file-0 init-object
    my-file-1-buffer my-file-1-size 0 s" file-1" <ram-real-file> my-file-1 init-object
    my-mount-fs my-file-1 s" mount" <ram-real-mount> my-mount init-object
    my-mount my-file-0 s" dir" <ram-real-dir> my-sub-dir init-object
    my-sub-dir 0 s" " <ram-real-dir> my-root-dir init-object
    my-root-dir <ram-fs> my-parent-fs init-object
    s" /" my-parent-fs <file-context> my-context init-object
  ;
  
  : test-0 ( -- )
    s" /" my-context find-entity-class@ class-size my-heap allocate
    >r s" /" r@ my-context find-init-entity
    0 r@ list-dirs
    r> my-heap free
    s" /dir" my-context find-entity-class@ class-size my-heap allocate
    >r s" /dir" r@ my-context find-init-entity
    0 r@ list-dirs
    r> my-heap free
    s" /dir/mount" my-context find-entity-class@ class-size my-heap allocate
    >r s" /dir/mount" r@ my-context find-init-entity
    0 r@ list-dirs
    r> my-heap free
    s" /" [: 0 swap list-dirs ;] my-context find-with-entity
    s" /dir" [: 0 swap list-dirs ;] my-context find-with-entity
    s" /dir/mount" [: 0 swap list-dirs ;] my-context find-with-entity
    s" /foobar" [: 0 swap list-dirs ;] my-context find-with-entity
  ;
  
  : test-1 ( -- )
    s" /dir" my-context working-dir 2!
    s" mount/mount-file" [:
      128 0 ?do
        i 1 [: tuck c! 1 2 pick write-file drop ;] with-allot
      loop
    ;] my-context find-with-entity
    s" /dir/mount" my-context working-dir 2!
    s" mount-file" [:
      1 [:
        begin
          2dup 1 rot read-file 0> if
            dup c@ h.2 space false
          else
            true
          then
        until
      ;] with-allot
    ;] my-context find-with-entity
  ;

end-module
