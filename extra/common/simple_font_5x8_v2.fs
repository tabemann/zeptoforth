\ Copyright (c) 2022-2025 Travis Bemann
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

begin-module simple-font-5x8-v2

  oo import
  font import

  begin-module simple-font-5x8-v2-internal

    \ Our character columns
    5 constant char-cols
    
    \ Our character rows
    8 constant char-rows
    
    \ Our character count
    128 constant char-count
  
    \ Our font buffer
    char-cols char-rows $20 $7E font-buf-size 4 align buffer: a-font-buf

    \ Parse a row the font
    : %% ( -- )
      0 { data }
      source >parse @ 5 + min + source drop >parse @ + ?do
        data 1 lshift to data
        i c@ [char] # = if data 1 or to data then
      loop
      source >parse @ 5 + min >parse ! drop
      data c,
      ."  Data: " data h.2
    ;

    \ Our font data
    create char-data

    char ! c,
    %% ..#..
    %% ..#..
    %% ..#..
    %% ..#..
    %% ..#..
    %% .....
    %% ..#..
    %% .....

    char " c,
    %% .#.#.
    %% .#.#.
    %% .....
    %% .....
    %% .....
    %% .....
    %% .....
    %% .....
    
    char # c,
    %% .....
    %% .##..
    %% ####.
    %% .##..
    %% ####.
    %% .##..
    %% .....
    %% .....

    char $ c,
    %% .##..
    %% #.##.
    %% #.#..
    %% .##..
    %% .#.#.
    %% ##.#.
    %% .##..
    %% .....

    char % c,
    %% .#..#
    %% #.##.
    %% .#.#.
    %% ..#..
    %% .#.#.
    %% .##.#
    %% #..#.
    %% .....

    char & c,
    %% ..#..
    %% .#.#.
    %% #..##
    %% .##..
    %% #....
    %% #...#
    %% .###.
    %% .....

    char ' c,
    %% ..#..
    %% ..#..
    %% .....
    %% .....
    %% .....
    %% .....
    %% .....
    %% .....

    char ( c,
    %% ...#.
    %% ..#..
    %% .#...
    %% .#...
    %% .#...
    %% ..#..
    %% ...#.
    %% .....

    char ) c,
    %% .#...
    %% ..#..
    %% ...#.
    %% ...#.
    %% ...#.
    %% ..#..
    %% .#...
    %% .....

    char * c,
    %% ..#..
    %% ..#..
    %% #####
    %% .#.#.
    %% #...#
    %% .....
    %% .....
    %% .....

    char + c,
    %% .....
    %% ..#..
    %% ..#..
    %% #####
    %% ..#..
    %% ..#..
    %% .....
    %% .....

    char , c,
    %% .....
    %% .....
    %% .....
    %% .....
    %% .....
    %% .....
    %% ...#.
    %% ..#..

    char - c,
    %% .....
    %% .....
    %% .....
    %% .###.
    %% .....
    %% .....
    %% .....
    %% .....

    char . c,
    %% .....
    %% .....
    %% .....
    %% .....
    %% .....
    %% ..#..
    %% .....
    %% .....    

    char / c,
    %% ...#.
    %% ..#..
    %% ..#..
    %% ..#..
    %% .#...
    %% .#...
    %% #....
    %% .....    
    
    char 0 c,
    %% .##..
    %% #..#.
    %% #.##.
    %% ##.#.
    %% ##.#.
    %% #..#.
    %% .##..
    %% .....    

    char 1 c,
    %% ..#..
    %% .##..
    %% #.#..
    %% ..#..
    %% ..#..
    %% ..#..
    %% #####
    %% .....    
    
    char 2 c,
    %% .##..
    %% #..#.
    %% ...#.
    %% .##..
    %% #....
    %% #....
    %% ####.
    %% .....

    char 3 c,
    %% .##..
    %% #..#.
    %% ...#.
    %% ..#..
    %% ...#.
    %% #..#.
    %% .##..
    %% .....

    char 4 c,
    %% ...#.
    %% ..##.
    %% .#.#.
    %% #####
    %% ...#.
    %% ...#.
    %% ...#.
    %% .....
    
    char 5 c,
    %% ####.
    %% #....
    %% #....
    %% ###..
    %% ...#.
    %% ...#.
    %% ###..
    %% .....    
    
    char 6 c,
    %% .##..
    %% #..#.
    %% #....
    %% ###..
    %% #..#.
    %% #..#.
    %% .##..
    %% .....

    char 7 c,
    %% ####.
    %% ...#.
    %% ...#.
    %% ..#..
    %% ..#..
    %% .#...
    %% .#...
    %% .....

    char 8 c,
    %% .##..
    %% #..#.
    %% #..#.
    %% .##..
    %% #..#.
    %% #..#.
    %% .##..
    %% .....    

    char 9 c,
    %% .##..
    %% #..#.
    %% #..#.
    %% .###.
    %% ...#.
    %% ...#.
    %% ###..
    %% .....    

    char : c,
    %% .....
    %% .....
    %% ..#..
    %% .....
    %% .....
    %% .....
    %% ..#..
    %% .....    

    char ; c,
    %% .....
    %% .....
    %% ..#..
    %% .....
    %% .....
    %% .....
    %% ..#..
    %% .#...

    char < c,
    %% ...#.
    %% ..#..
    %% .#...
    %% #....
    %% .#...
    %% ..#..
    %% ...#.
    %% .....    

    char = c,
    %% .....
    %% .....
    %% .###.
    %% .....
    %% .###.
    %% .....
    %% .....    
    %% .....

    char > c,
    %% #....
    %% .#...
    %% ..#..
    %% ...#.
    %% ..#..
    %% .#...
    %% #....
    %% .....    

    char ? c,
    %% .##..
    %% #..#.
    %% ...#.
    %% ..#..
    %% ..#..
    %% .....
    %% ..#..
    %% .....    

    char @ c,
    %% .##..
    %% #..#.
    %% #.##.
    %% #.##.
    %% #.##.
    %% #....
    %% .##..
    %% .....    

    char A c,
    %% .##..
    %% #. #.
    %% #..#.
    %% ####.
    %% #..#.
    %% #..#.
    %% #..#.
    %% .....    

    char B c,
    %% ###..
    %% #..#.
    %% #..#.
    %% ###..
    %% #..#.
    %% #..#.
    %% ###..
    %% .....    

    char C c,
    %% .##..
    %% #..#.
    %% #....
    %% #....
    %% #....
    %% #..#.
    %% .##..
    %% .....

    char D c,
    %% ##...
    %% #.#..
    %% #..#.
    %% #..#.
    %% #..#.
    %% #.#..
    %% ##...
    %% .....

    char E c,
    %% ####.
    %% #....
    %% #....
    %% ####.
    %% #....
    %% #....
    %% ####.
    %% .....

    char F c,
    %% ####.
    %% #....
    %% #....
    %% ####.
    %% #....
    %% #....
    %% #....
    %% .....

    char G c,
    %% .##..
    %% #..#.
    %% #....
    %% #.##.
    %% #..#.
    %% #..#.
    %% .##..
    %% .....

    char H c,
    %% #..#.
    %% #..#.
    %% #..#.
    %% ####.
    %% #..#.
    %% #..#.
    %% #..#.
    %% .....

    char I c,
    %% .###.
    %% ..#..
    %% ..#..
    %% ..#..
    %% ..#..
    %% ..#..
    %% .###.
    %% .....

    char J c,
    %% ...#.
    %% ...#.
    %% ...#.
    %% ...#.
    %% #..#.
    %% #..#.
    %% .##..
    %% .....

    char K c,
    %% #..#.
    %% #.#..
    %% ##...
    %% ##...
    %% #.#..
    %% #..#.
    %% #..#.
    %% .....

    char L c,
    %% #....
    %% #....
    %% #....
    %% #....
    %% #....
    %% #....
    %% ####.
    %% .....

    char M c,
    %% #..#.
    %% ####.
    %% ####.
    %% #..#.
    %% #..#.
    %% #..#.
    %% #..#.
    %% .....
    
    char N c,
    %% #..#.
    %% #..#.
    %% ##.#.
    %% ####.
    %% #.##.
    %% #..#.
    %% #..#.
    %% .....

    char O c,
    %% .##..
    %% #..#.
    %% #..#.
    %% #..#.
    %% #..#.
    %% #..#.
    %% .##..
    %% .....

    char P c,
    %% ###..
    %% #..#.
    %% #..#.
    %% ###..
    %% #....
    %% #....
    %% #....
    %% .....

    char Q c,
    %% .##..
    %% #..#.
    %% #..#.
    %% #..#.
    %% #..#.
    %% #..#.
    %% .##..
    %% ...#.

    char R c,
    %% ###..
    %% #..#.
    %% #..#.
    %% ###..
    %% ##...
    %% #.#..
    %% #..#.
    %% .....

    char S c,
    %% .##..
    %% #..#.
    %% #....
    %% .##..
    %% ...#.
    %% #..#.
    %% .##..
    %% .....
    
    char T c,
    %% #####
    %% ..#..
    %% ..#..
    %% ..#..
    %% ..#..
    %% ..#..
    %% ..#..
    %% .....

    char U c,
    %% #..#.
    %% #..#.
    %% #..#.
    %% #..#.
    %% #..#.
    %% #..#.
    %% .##..
    %% .....

    char V c,
    %% #..#.
    %% #..#.
    %% #..#.
    %% #..#.
    %% .##..
    %% .##..
    %% ..#..
    %% .....

    char W c,
    %% #...#
    %% #...#
    %% #...#
    %% #.#.#
    %% #####
    %% .#.#.
    %% .#.#.
    %% .....    

    char X c,
    %% #..#.
    %% #.,#.
    %% .##..
    %% .##..
    %% .##..
    %% #..#.
    %% #..#.
    %% .....

    char Y c,
    %% #..#.
    %% #..#.
    %% #..#.
    %% .##..
    %% ..#..
    %% ..#..
    %% ..#..
    %% .....

    char Z c,
    %% ####.
    %% ...#.
    %% ..#..
    %% .##..
    %% .#...
    %% #....
    %% ####.
    %% .....

    char [ c,
    %% .###.
    %% .#...
    %% .#...
    %% .#...
    %% .#...
    %% .#...
    %% .###.
    %% .....

    $5C c, \ backslash, in hex here to avoid comment stripping
    %% #....
    %% .#...
    %% .#...
    %% ..#..
    %% ..#..
    %% ..#..
    %% ...#.
    %% .....

    char ] c,
    %% .###.
    %% ...#.
    %% ...#.
    %% ...#.
    %% ...#.
    %% ...#.
    %% .###.
    %% .....
    
    char ^ c,
    %% ..#..
    %% .##..
    %% #..#.
    %% .....
    %% .....
    %% .....
    %% .....
    %% .....    

    char _ c,
    %% .....
    %% .....
    %% .....
    %% .....
    %% .....
    %% .....
    %% ####.
    %% .....

    char ` c,
    %% .#...
    %% ..#..
    %% .....
    %% .....
    %% .....
    %% .....
    %% .....
    %% .....

    char a c,
    %% .....
    %% .....
    %% ###..
    %% ...#.
    %% .###.
    %% #.##.
    %% .#.#.
    %% .....

    char b c,
    %% #....
    %% #....
    %% ###..
    %% #..#.
    %% #..#.
    %% #..#.
    %% ###..
    %% .....

    char c c,
    %% .....
    %% .....
    %% .##..
    %% #..#.
    %% #....
    %% #..#.
    %% .##..
    %% .....

    char d c,
    %% ...#.
    %% ...#.
    %% .###.
    %% #..#.
    %% #..#.
    %% #..#.
    %% .###.
    %% .....

    char e c,
    %% .....
    %% .....
    %% .##..
    %% #..#.
    %% ####.
    %% #....
    %% .###.
    %% .....

    char f c,
    %% ..##.
    %% .#...
    %% .#...
    %% ####.
    %% .#...
    %% .#...
    %% .#...
    %% .....

    char g c,
    %% .....
    %% .....
    %% .##..
    %% #..#.
    %% #..#.
    %% .###.
    %% ...#.
    %% .##..

    char h c,
    %% #....
    %% #....
    %% ###..
    %% #..#.
    %% #..#.
    %% #..#.
    %% #..#.
    %% .....

    char i c,
    %% ..#..
    %% .....
    %% .##..
    %% ..#..
    %% ..#..
    %% ..#..
    %% ####.
    %% .....

    char j c,
    %% ..#..
    %% .....
    %% .##..
    %% ..#..
    %% ..#..
    %% ..#..
    %% ..#..
    %% ##...

    char k c,
    %% #....
    %% #....
    %% #..#.
    %% #.#..
    %% ##...
    %% #.#..
    %% #..#.
    %% .....    

    char l c,
    %% ##...
    %% .#...
    %% .#...
    %% .#...
    %% .#...
    %% .#...
    %% ..##.
    %% .....

    char m c,
    %% .....
    %% .....
    %% ##.#.
    %% #.#.#
    %% #.#.#
    %% #.#.#
    %% #.#.#    
    %% .....    

    char n c,
    %% .....
    %% .....
    %% #.#..
    %% ##.#.
    %% #..#.
    %% #..#.
    %% #..#.
    %% .....

    char o c,
    %% .....
    %% .....
    %% .##..
    %% #..#.
    %% #..#.
    %% #..#.
    %% .##..    
    %% .....    

    char p c,
    %% .....
    %% .....
    %% ###..
    %% #..#.
    %% #..#.
    %% ###..
    %% #....
    %% #....
    
    char q c,
    %% .....
    %% .....
    %% .###.
    %% #..#.
    %% #..#.
    %% .###.
    %% ...#.
    %% ...#.

    char r c,
    %% .....
    %% .....
    %% #.#..
    %% ##.#.
    %% #....
    %% #....
    %% #....
    %% .....
    
    char s c,
    %% .....
    %% .....
    %% .###.
    %% #....
    %% .##..
    %% ...#.
    %% ###..
    %% .....

    char t c,
    %% .....
    %% .#...
    %% ###,.
    %% .#...
    %% .#...
    %% .#...
    %% ..#..
    %% .....

    char u c,
    %% .....
    %% .....
    %% #..#.
    %% #..#.
    %% #..#.
    %% #.##.
    %% .#.#.    
    %% .....    

    char v c,
    %% .....
    %% .....
    %% #...#
    %% .#.#.
    %% .#.#.
    %% .#.#.
    %% ..#..
    %% .....

    char w c,
    %% .....
    %% .....
    %% #...#
    %% #.#.#
    %% #####
    %% .#.#.
    %% .#.#.
    %% .....

    char x c,
    %% .....
    %% .....
    %% #...#
    %% .#.#.
    %% ..#..
    %% .#.#.
    %% #..,#
    %% .....

    char y c,
    %% .....
    %% .....
    %% #..#.
    %% #..#.
    %% .#.#.
    %% ..#..
    %% .#...
    %% #....

    char z c,
    %% .....
    %% .....
    %% ####.
    %% ..##.
    %% ,##..
    %% ##,,.
    %% ####.
    %% .....

    char { c,
    %% ..##.
    %% .#...
    %% .#...
    %% ##...
    %% .#...
    %% .#...
    %% ..##.
    %% .....

    char | c,
    %% ..#..
    %% ..#..
    %% ..#..
    %% ..#..
    %% ..#..
    %% ..#..
    %% ..#..
    %% .....

    char } c,
    %% ##...
    %% ..#..
    %% ..#..
    %% ..##.
    %% ..#..
    %% ..#..
    %% ##...
    %% .....

    char ~ c,
    %% .....
    %% .....
    %% .##..
    %% #.#.#
    %% ..##.
    %% .....
    %% .....    
    %% .....

    0 c,

    variable align-this
    
  end-module> import

  \ Our font
  <font> class-size buffer: a-simple-font-5x8-v2

  continue-module simple-font-5x8-v2-internal
    
    \ Load our simple font
    : load-simple-font-5x8-v2 { char-data -- }
      begin char-data c@ 0<> while
        char-data c@ { c }
        1 +to char-data
        char-rows 0 ?do
          char-data c@ i c a-simple-font-5x8-v2 char-row!
          1 +to char-data
        loop
      repeat
    ;

  end-module
  
  \ Initialize our font
  : init-simple-font-5x8-v2 ( -- )
    a-font-buf $20 char-cols char-rows $20 $7E
    <font> a-simple-font-5x8-v2 init-object
    char-data load-simple-font-5x8-v2
  ;

end-module
