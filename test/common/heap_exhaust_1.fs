heap1 import

65536 constant my-base-heap-size
16 12 + constant my-block-size

: get-heap-size ( -- bytes )
  my-block-size my-base-heap-size my-block-size / heap-size
;

: memtest ( -- )
  get-heap-size [: { my-heap }
  my-block-size my-base-heap-size my-block-size / my-heap init-heap
    0 { counter }
    begin
      0 { last-data }
      1024 1 ?do
        ." cycle " counter .
        ." bytes " i .
        ." allocating "
        i my-heap allocate { data }
        last-data 0<> if
          ." freeing "
          last-data my-heap free
          ." freed "
        then
        data to last-data
      loop
    again
  ;] with-aligned-allot
;