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

\ This code is based closely off of https://github.com/amosnier/sha-2

begin-module sha-256-accel-test

  sha-256-accel import

  1024 constant buf-size

  : x-invalid-hex ." invalid hex string" cr ;

  : parse-hex-string ( string-addr string-bytes buf-addr -- )
    base @ { saved-base }
    [: { string-addr string-bytes buf-addr }
      hex
      string-bytes 2 umod string-bytes + 1 rshift { buf-bytes }
      string-bytes +to string-addr
      buf-bytes 0> if
        0 buf-bytes 1- do
          string-bytes 2 min negate +to string-addr
          string-addr string-bytes 2 min parse-unsigned averts x-invalid-hex
          buf-addr i + c!
          string-bytes 2 min negate +to string-bytes
        -1 +loop
      then
    ;] try
    saved-base base !
    ?raise
  ;

  : print-hex-string { buf-addr buf-bytes }
    begin buf-bytes 0> while
      buf-addr c@ h.2 1 +to buf-addr -1 +to buf-bytes
    repeat
  ;

  : test ( data-addr data-bytes hash-addr hash-bytes -- )
    size-of-sha-256-hash [:
      size-of-sha-256-hash [:
        { data-addr data-bytes hash-addr hash-bytes hash-buf output-buf }
        hash-addr hash-bytes hash-buf parse-hex-string
        space hash-addr hash-bytes type space
        data-addr data-bytes output-buf calc-sha-256-accel
        hash-buf size-of-sha-256-hash output-buf size-of-sha-256-hash
        equal-strings? if
          ." SUCCESS"
        else
          output-buf size-of-sha-256-hash print-hex-string space
          ." FAIL"
        then
      ;] with-allot
    ;] with-allot
  ;

  : string-test { data-addr data-bytes hash-addr hash-bytes -- }
    cr data-addr data-bytes type
    data-addr data-bytes hash-addr hash-bytes test
  ;

  : binary-test
    { descr-addr descr-bytes data-addr data-bytes hash-addr hash-bytes -- }
    cr descr-addr descr-bytes type
    data-addr data-bytes hash-addr hash-bytes test
  ;
  
  : test0 { buf -- }
    s" "
    s" e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855"
    string-test
  ;

  : test1 { buf -- }
    s" abc"
    s" ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad"
    string-test
  ;

  : test2 { buf -- }
    s" 0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef"
    s" a8ae6e6ee929abea3afcfc5258c8ccd6f85273e0d4626d26c7279f3250f77c8e"
    string-test
  ;

  : test3 { buf -- }
    s" 0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcde"
    s" 057ee79ece0b9a849552ab8d3c335fe9a5f1c46ef5f1d9b190c295728628299c"
    string-test
  ;

  : test4 { buf -- }
    s" 0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef0"
    s" 2a6ad82f3620d3ebe9d678c812ae12312699d673240d5be8fac0910a70000d93"
    string-test
  ;

  : test5 { buf -- }
    s" abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq"
    s" 248d6a61d20638b8e5c026930c3e6039a33ce45964ff2167f6ecedd419db06c1"
    string-test
  ;

  : test6 { buf -- }
    s" abcdefghbcdefghicdefghijdefghijkefghijklfghijklmghijklmnhijklmnoijklmnopjklmnopqklmnopqrlmnopqrsmnopqrstnopqrstu"
    s" cf5b16a778af8380036ce59e7b0492370b249b11e8f07a51afac45037afee9d1"
    string-test
  ;

  : test7 { buf -- }
    s\" \xbd" buf swap move
    s" \xbd"
    buf 1
    s" 68325720aabd7c82f30f554b313d0570c95accbb7dc4b5aae11204c08ffe732b"
    binary-test
  ;

  : test8 { buf -- }
    s\" \xc9\x8c\x8e\x55" buf swap move
    s" \xc9\x8c\x8e\x55"
    buf 4
    s" 7abc22c0ae5af26ce93dbb94433a0e0b2e119d014f8e7f65bd56c61ccccd9504"
    binary-test
  ;

  : test9 { buf -- }
    buf 55 $00 fill
    s" 55 bytes of \x00"
    buf 55
    s" 02779466cdec163811d078815c633f21901413081449002f24aa3e80f0b88ef7"
    binary-test
  ;

  : test10 { buf -- }
    buf 56 $00 fill
    s" 56 bytes of \x00"
    buf 56
    s" d4817aa5497628e7c77e6b606107042bbba3130888c5f47a375e6179be789fbb"
    binary-test
  ;

  : test11 { buf -- }
    buf 57 $00 fill
    s" 57 bytes of \x00"
    buf 57
    s" 65a16cb7861335d5ace3c60718b5052e44660726da4cd13bb745381b235a1785"
    binary-test
  ;

  : test12 { buf -- }
    buf 64 $00 fill
    s" 64 bytes of \x00"
    buf 64
    s" f5a5fd42d16a20302798ef6ed309979b43003d2320d9f0e8ea9831a92759fb4b"
    binary-test
  ;

  : test13 { buf -- }
    buf 1000 $00 fill
    s" 1000 bytes of \x00"
    buf 1000
    s" 541b3e9daa09b20bf85fa273e5cbd3e80185aa4ec298e765db87742b70138a53"
    binary-test
  ;

  : test14 { buf -- }
    buf 1000 $41 fill
    s" 1000 bytes of \x41"
    buf 1000
    s" c2e686823489ced2017f6059b8b239318b6364f6dcd835d0a519105a1eadd6e4"
    binary-test
  ;

  : test15 { buf -- }
    buf 1005 $55 fill
    s" 1005 bytes of \x55"
    buf 1005
    s" f4d62ddec0f3dd90ea1380fa16a5ff8dc4c54b21740650f24afc4120903552b0"
    binary-test
  ;

  : run-test ( -- )
    buf-size [: { buf }
      buf test0
      buf test1
      buf test2
      buf test3
      buf test4
      buf test5
      buf test6
      buf test7
      buf test8
      buf test9
      buf test10
      buf test11
      buf test12
      buf test13
      buf test14
      buf test15
    ;] with-allot
  ;
  
end-module
