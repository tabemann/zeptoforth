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

begin-module oo-test

  oo import
  
  <object> begin-class <my-class>
  
    cell member foo#
    cell member bar#
    
    method foo@
    method bar@
    
  end-class
  
  <my-class> begin-implement
  
    :noname dup <object>->new 0 over foo# ! 0 swap bar# ! ; define new
    :noname cr ." Destroying my-class" <object>->destroy ; define destroy
    :noname foo# @ ; define foo@
    :noname bar# @ ; define bar@
  
  end-implement
  
  <my-class> begin-class <my-subclass-0>
  
    cell member baz#
    
    method baz@
  
  end-class
  
  <my-subclass-0> begin-implement
  
    :noname dup <my-class>->new 2 over bar# ! 3 swap baz# ! ; define new
    :noname cr ." Destroying my-subclass-0" <my-class>->destroy ;
    define destroy
    :noname baz# @ ; define baz@
  
  end-implement
  
  <my-class> begin-class <my-subclass-1> end-class
  
  <my-subclass-1> begin-implement
  
    :noname cr ." Destroying my-subclass-1" <my-class>->destroy ;
    define destroy
    :noname drop 4 ; define bar@
    
  end-implement
  
  <my-class> class-size buffer: my-object-0
  <my-subclass-0> class-size buffer: my-object-1
  <my-subclass-1> class-size buffer: my-object-2
  
  compiling-to-flash? [if]
    : init ( -- )
      init
      <my-class> my-object-0 init-object
      <my-subclass-0> my-object-1 init-object
      <my-subclass-1> my-object-2 init-object
    ;
  [else]
    <my-class> my-object-0 init-object
    <my-subclass-0> my-object-1 init-object
    <my-subclass-1> my-object-2 init-object    
  [then]

end-module
