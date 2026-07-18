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

begin-module star
  
  lindenmayer import
  
  45 turn t+
  -45 turn t-
  step F
  step X
  step Y
  0,1 16,0 f/ 4 :rcolor-forward-step F F ;step
  :step X X t+ Y F t+ t+ Y F t- F X t- t- F X F X t- Y F t+ X ;step
  :step Y t- F X t+ Y F Y F t+ t+ Y F t+ F X t- t- F X t- Y F ;step
  
  2 0,0 255 255 255 80,0 80,0 0 :axiom draw-star
    X t+ X t+ X t+ X t+ X t+ X t+ X t+ X
  ;axiom
  
end-module
