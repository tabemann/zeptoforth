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

float32 import

: do-parse-float32-test { D: string }
  cr string type ." : " string parse-float32 . v.
;

: parse-float32-test
  s" 0.0625" do-parse-float32-test
  s" 0.125" do-parse-float32-test
  s" 0.25" do-parse-float32-test
  s" 0.5" do-parse-float32-test
  s" 0.3" do-parse-float32-test
  s" 3e-1" do-parse-float32-test
  s" 0" do-parse-float32-test
  s" 1" do-parse-float32-test
  s" 2" do-parse-float32-test
  s" 3" do-parse-float32-test
  s" 4" do-parse-float32-test
  s" 5" do-parse-float32-test
  s" 6" do-parse-float32-test
  s" 7" do-parse-float32-test
  s" 8" do-parse-float32-test
  s" 9" do-parse-float32-test
  s" 10" do-parse-float32-test
  s" 11" do-parse-float32-test
  s" 12" do-parse-float32-test
  s" 13" do-parse-float32-test
  s" 14" do-parse-float32-test
  s" 15" do-parse-float32-test
  s" 16" do-parse-float32-test
  s" 17" do-parse-float32-test
  s" 18" do-parse-float32-test
  s" 19" do-parse-float32-test
  s" 20" do-parse-float32-test
  s" 1234567" do-parse-float32-test
  s" 12345678" do-parse-float32-test
  s" 2e1" do-parse-float32-test
  s" 1.1e0" do-parse-float32-test
  s" 1.1e1" do-parse-float32-test
  s" 1e-1" do-parse-float32-test
  s" 2e-1" do-parse-float32-test
  s" 5e-1" do-parse-float32-test
  s" +infinity" do-parse-float32-test
  s" -infinity" do-parse-float32-test
  s" nan" do-parse-float32-test
  s" 1e-37" do-parse-float32-test
  s" 1e-38" do-parse-float32-test
  s" 1e-39" do-parse-float32-test
;

float32 unimport