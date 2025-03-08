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

begin-module ansi-effect-test

  ansi-term import

  : run-test ( -- )
    cr normal color-effect!
    ." This is normal"
    cr bold color-effect!
    ." This is bold"
    cr normal color-effect!
    ." This is normal"
    cr dim color-effect!
    ." This is dim"
    cr normal color-effect!
    ." This is normal"
    cr bold color-effect! dim color-effect!
    ." This is bold and dim"
    cr normal color-effect!
    ." This is normal"
    cr underline color-effect! bold color-effect!
    ." This is bold and underlined"
    cr underline-off color-effect!
    ." This is bold"
    cr normal color-effect!
    ." This is normal"
    cr underline color-effect! dim color-effect!
    ." This is dim and underlined"
    cr underline-off color-effect!
    ." This is dim"
    cr normal color-effect!
    ." This is normal"
    cr underline color-effect! bold color-effect! dim color-effect!
    ." This is bold and dim and underlined"
    cr underline-off color-effect!
    ." This is bold and dim"
    cr normal color-effect!
    ." This is normal"
    cr black color-effect!
    ." This is black"
    cr red color-effect!
    ." This is red"
    cr green color-effect!
    ." This is green"
    cr yellow color-effect!
    ." This is yellow"
    cr blue color-effect!
    ." This is blue"
    cr magenta color-effect!
    ." This is magenta"
    cr cyan color-effect!
    ." This is cyan"
    cr white color-effect!
    ." This is white"
    cr b-black color-effect!
    ." This is bright black"
    cr b-red color-effect!
    ." This is bright red"
    cr b-green color-effect!
    ." This is bright green"
    cr b-yellow color-effect!
    ." This is bright yellow"
    cr b-blue color-effect!
    ." This is bright blue"
    cr b-magenta color-effect!
    ." This is bright magenta"
    cr b-cyan color-effect!
    ." This is bright cyan"
    cr b-white color-effect!
    ." This is bright white"
    cr none color-effect!
    ." This is reset"
    cr black background color-effect!
    ." This has a black background"
    cr red background color-effect!
    ." This has a red background"
    cr green background color-effect!
    ." This has a green background"
    cr yellow background color-effect!
    ." This has a yellow background"
    cr blue background color-effect!
    ." This has a blue background"
    cr magenta background color-effect!
    ." This has a magenta background"
    cr cyan background color-effect!
    ." This has a cyan background"
    cr white background color-effect!
    ." This has a white background"
    cr b-black background color-effect!
    ." This has a bright black background"
    cr b-red background color-effect!
    ." This has a bright red background"
    cr b-green background color-effect!
    ." This has a bright green background"
    cr b-yellow background color-effect!
    ." This has a bright yellow background"
    cr b-blue background color-effect!
    ." This has a bright blue background"
    cr b-magenta background color-effect!
    ." This has a bright magenta background"
    cr b-cyan background color-effect!
    ." This has a bright cyan background"
    cr b-white background color-effect!
    ." This has a bright white background"
    cr none color-effect!
    ." This is reset"
    [: bold background drop ;] try if
      cr ." Caught bad color successfully"
    else
      cr ." Did not catch bad color"
    then
    [: white background background drop ;] try if
      cr ." Did not correctly handle a background color"
    else
      cr ." Correctly handled a background color"
    then
  ;

end-module

