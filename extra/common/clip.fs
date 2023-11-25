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

begin-module clip
  
  \ Clip a destination-only rectangle
  : clip-dst-only
    { dst-col col-count dst-row row-count dst-cols dst-rows --
    new-dst-col new-col-count new-dst-row new-row-count }
    dst-col 0 < if
      dst-col col-count + 0>= if
        dst-col +to col-count
      else
        0 to col-count
      then
      0 to dst-col
    then
    dst-col dst-cols < if
      dst-col col-count + dst-cols > if
        dst-cols dst-col col-count + - +to col-count
      then
    else
      0 to col-count
      dst-cols to dst-col
    then
    dst-row 0 < if
      dst-row row-count + 0>= if
        dst-row +to row-count
      else
        0 to row-count
      then
      0 to dst-row
    then
    dst-row dst-rows < if
      dst-row row-count + dst-rows > if
        dst-rows dst-row row-count + - +to row-count
      then
    else
      0 to row-count
      dst-rows to dst-row
    then
    dst-col col-count dst-row row-count
  ;

  \ Clip a rectangle to the source dimensions
  : clip-src
    { src-col dst-col col-count src-row dst-row row-count src-cols src-rows --
    new-src-col new-dst-col new-col-count new-src-row new-dst-row
    new-row-count }
    src-col 0 < if
      src-col negate +to dst-col
      src-col col-count + 0>= if
        src-col +to col-count
      else
        0 to col-count
      then
      0 to src-col
    then
    src-col src-cols < if
      src-col col-count + src-cols > if
        src-cols src-col col-count + - +to col-count
      then
    else
      0 to row-count
      src-cols to dst-col
    then
    src-row 0 < if
      src-row negate +to dst-row
      src-row row-count + 0>= if
        src-row +to row-count
      else
        0 to row-count
      then
      0 to src-row
    then
    src-row src-rows < if
      src-row row-count + src-rows > if
        src-rows src-row row-count + - +to row-count
      then
    else
      0 to row-count
      src-rows to dst-row
    then
    src-col dst-col col-count src-row dst-row row-count
  ;

  \ Clip a rectangle to the destination dimensions
  : clip-dst
    { src-col dst-col col-count src-row dst-row row-count dst-cols dst-rows --
    new-src-col new-dst-col new-col-count new-src-row new-dst-row
    new-row-count }
    dst-col 0 < if
      dst-col negate +to src-col
      dst-col col-count + 0>= if
        dst-col +to col-count
      else
        0 to col-count
      then
      0 to dst-col
    then
    dst-col dst-cols < if
      dst-col col-count + dst-cols > if
        dst-cols dst-col col-count + - +to col-count
      then
    else
      0 to row-count
      dst-cols to src-col
    then
    dst-row 0 < if
      dst-row negate +to src-row
      dst-row row-count + 0>= if
        dst-row +to row-count
      else
        0 to row-count
      then
      0 to dst-row
    then
    dst-row dst-rows < if
      dst-row row-count + dst-rows > if
        dst-rows dst-row row-count + - +to row-count
      then
    else
      0 to row-count
      dst-rows to src-row
    then
    src-col dst-col col-count src-row dst-row row-count
  ;

  \ Clip a rectangle to the mask dimensions
  : clip-mask
    { mask-col src-col dst-col col-count
    mask-row src-row dst-row row-count mask-cols mask-rows --
    new-mask-col new-src-col new-dst-col new-col-count
    new-mask-row new-src-row new-dst-row
    new-row-count }
    mask-col 0 < if
      mask-col negate +to src-col
      mask-col negate +to dst-col
      mask-col col-count + 0>= if
        mask-col +to col-count
      else
        0 to col-count
      then
      0 to mask-col
    then
    mask-col mask-cols < if
      mask-col col-count + mask-cols > if
        mask-cols mask-col col-count + - +to col-count
      then
    else
      0 to row-count
      mask-cols to src-col
      mask-cols to dst-col
    then
    mask-row 0 < if
      mask-row negate +to src-row
      mask-row negate +to dst-row
      mask-row row-count + 0>= if
        mask-row +to row-count
      else
        0 to row-count
      then
      0 to mask-row
    then
    mask-row mask-rows < if
      mask-row row-count + mask-rows > if
        mask-rows mask-row row-count + - +to row-count
      then
    else
      0 to row-count
      mask-rows to src-row
      mask-rows to dst-row
    then
    mask-col src-col dst-col col-count mask-row src-row dst-row row-count
  ;

  \ Clip a rectangle to the source dimensions with a mask
  : clip-src-w/-mask
    { mask-col src-col dst-col col-count
    mask-row src-row dst-row row-count src-cols src-rows --
    new-mask-col new-src-col new-dst-col new-col-count
    new-mask-row new-src-row new-dst-row
    new-row-count }
    src-col 0 < if
      src-col negate +to mask-col
      src-col negate +to dst-col
      src-col col-count + 0>= if
        src-col +to col-count
      else
        0 to col-count
      then
      0 to src-col
    then
    src-col src-cols < if
      src-col col-count + src-cols > if
        src-cols src-col col-count + - +to col-count
      then
    else
      0 to row-count
      src-cols to mask-col
      src-cols to dst-col
    then
    src-row 0 < if
      src-row negate +to mask-row
      src-row negate +to dst-row
      src-row row-count + 0>= if
        src-row +to row-count
      else
        0 to row-count
      then
      0 to src-row
    then
    src-row src-rows < if
      src-row row-count + src-rows > if
        src-rows src-row row-count + - +to row-count
      then
    else
      0 to row-count
      src-rows to mask-row
      src-rows to dst-row
    then
    mask-col src-col dst-col col-count mask-row src-row dst-row row-count
  ;

  \ Clip a rectangle to the destination dimensions with a mask
  : clip-dst-w/-mask
    { mask-col src-col dst-col col-count
    mask-row src-row dst-row row-count dst-cols dst-rows --
    new-mask-col new-src-col new-dst-col new-col-count
    new-mask-row new-src-row new-dst-row
    new-row-count }
    dst-col 0 < if
      dst-col negate +to mask-col
      dst-col negate +to src-col
      dst-col col-count + 0>= if
        dst-col +to col-count
      else
        0 to col-count
      then
      0 to dst-col
    then
    dst-col dst-cols < if
      dst-col col-count + dst-cols > if
        dst-cols dst-col col-count + - +to col-count
      then
    else
      0 to row-count
      dst-cols to mask-col
      dst-cols to src-col
    then
    dst-row 0 < if
      dst-row negate +to mask-row
      dst-row negate +to src-row
      dst-row row-count + 0>= if
        dst-row +to row-count
      else
        0 to row-count
      then
      0 to dst-row
    then
    dst-row dst-rows < if
      dst-row row-count + dst-rows > if
        dst-rows dst-row row-count + - +to row-count
      then
    else
      0 to row-count
      dst-rows to mask-row
      dst-rows to src-row
    then
    mask-col src-col dst-col col-count mask-row src-row dst-row row-count
  ;

  \ Clip a rectangle
  : clip-src-dst
    ( start-src-col start-dst-col col-count start-src-row start-dst-row )
    ( row-count src-cols src-rows dst-cols dst-rows -- )
    { src-cols src-rows dst-cols dst-rows }
    src-cols src-rows clip-src
    dst-cols dst-rows clip-dst
  ;

  \ Clip a masked rectable
  : clip-mask-src-dst
    ( mask-col dst-col src-col cols mask-row src-row dst-row rows )
    ( mask-cols mask-rows src-cols src-rows dst-cols dst-rows -- )
    { mask-cols mask-rows src-cols src-rows dst-cols dst-rows }
    mask-cols mask-rows clip-mask
    src-cols src-rows clip-src-w/-mask
    dst-cols dst-rows clip-dst-w/-mask
  ;

end-module
