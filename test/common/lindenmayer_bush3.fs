\ Copyright (c) 2026 Travis Bemann
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

begin-module bush3
  
  lindenmayer import
  
  20 turn t+
  -20 turn t-
  step F
  step V
  step W
  step X
  step Y
  step Z
  0,1 16,0 f/ 5 :rcolor-forward-step F F ;step
  :step V [[ t+ t+ t+ W ]] [[ t- t- t- W ]] Y V ;step
  :step W t+ X [[ t- W ]] Z ;step
  :step X t- W [[ t+ X ]] Z ;step
  :step Y Y Z ;step
  :step Z [[ t- F F F ]] [[ t+ F F F ]] F ;step
  
  10 0,0 255 255 255 0,0 -130,0 0 :axiom draw-bush V Z F F F ;axiom
  
end-module
