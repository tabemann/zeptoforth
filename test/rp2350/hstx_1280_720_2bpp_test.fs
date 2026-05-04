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

\ Adapted from:
\ https://github.com/WolfWings/rp2350-cpuless-dvi/blob/main/dma_720p/dma_720p.c

begin-module hstx-demo

  pin import
  hstx import
  dma import
  dma-pool import
  busctrl import
  
  %1101010100 constant TMDS_CTRL_00
  %0010101011 constant TMDS_CTRL_01
  %0101010100 constant TMDS_CTRL_10
  %1010101011 constant TMDS_CTRL_11
  
  TMDS_CTRL_00 TMDS_CTRL_00 10 lshift or TMDS_CTRL_00 20 lshift or
  constant SYNC_V0_H0
  TMDS_CTRL_01 TMDS_CTRL_00 10 lshift or TMDS_CTRL_00 20 lshift or
  constant SYNC_V0_H1
  TMDS_CTRL_10 TMDS_CTRL_00 10 lshift or TMDS_CTRL_00 20 lshift or
  constant SYNC_V1_H0
  TMDS_CTRL_11 TMDS_CTRL_00 10 lshift or TMDS_CTRL_00 20 lshift or
  constant SYNC_V1_H1
  
  24 constant MODE_H_FRONT_PORCH
  32 constant MODE_H_SYNC_WIDTH
  24 constant MODE_H_BACK_PORCH
  1280 constant MODE_H_ACTIVE_PIXELS
  
  5 constant MODE_V_FRONT_PORCH
  6 constant MODE_V_SYNC_WIDTH
  5 constant MODE_V_BACK_PORCH
  720 constant MODE_V_ACTIVE_LINES

  \ In this part we 'bake in' the hsync band commands directly into the
  \ framebuffer. We end up with an 'oddball' stride but that's already true at
  \ all 16:9 resolutions so it's no net loss.
  \
  \ Note the 7 instead of 6 because we need to bake in the 'active pixels'
  \ command!
  \
  \ At 1280 2bpp this ends up 320 bytes for the stride + 28 bytes commands.
  \
  \ This is a 8.75% increase in framebuffer size in this case per scanline.
  MODE_H_ACTIVE_PIXELS 4 / cell / 7 + constant FRAMEBUF_STRIDE_DWORDS
  FRAMEBUF_STRIDE_DWORDS cells constant FRAMEBUF_STRIDE

  \ Theory here is we 'unroll' the vsync band fully. This also allows us to
  \ merge the front and back porch parts in each section, so instead of...
  \
  \ ...
  \ FRONT_PORCH
  \ SYNC_WIDTH
  \ BACK_PORCH + ACTIVE_PIXELS
  \ FRONT_PORCH
  \ SYNC_WIDTH
  \ BACK_PORCH + ACTIVE_PIXELS
  \ ...
  \
  \ ...we can merge the adjacent items and streamline it to...
  \
  \ ...
  \ FRONT_PORCH
  \ SYNC_WIDTH
  \ BACK_PORCH + ACTIVE_PIXELS + FRONT_PORCH
  \ SYNC_WIDTH
  \ BACK_PORCH + ACTIVE_PIXELS
  \ ...
  \
  \ ...so we need 4 per region plus 2 additional ones for transition, per line.

  MODE_V_FRONT_PORCH 4 * 2 + cells constant FRAMEBUF_VBLANK_FRONT
  MODE_V_SYNC_WIDTH 4 * 2 + cells constant FRAMEBUF_VBLANK_SYNC
  MODE_V_BACK_PORCH 4 * 2 + cells constant FRAMEBUF_VBLANK_BACK
  FRAMEBUF_VBLANK_FRONT FRAMEBUF_VBLANK_SYNC + FRAMEBUF_VBLANK_BACK +
  constant FRAMEBUF_PREQUEL
  
  FRAMEBUF_PREQUEL + FRAMEBUF_STRIDE MODE_V_ACTIVE_LINES * +
  buffer: framebuf-raw
  
  framebuf-raw FRAMEBUF_PREQUEL + 7 cells + constant framebuf

  0 value dmach-screen
  0 value dmach-looper
  
  : init-dma-channels ( -- )
    allocate-dma to dmach-screen
    allocate-dma to dmach-looper
  ;
  initializer init-dma-channels

  \ Fill the framebuffer with a RAW_REPEAT command
  : raw-repeat!+ ( addr count -- addr' )
    swap { addr }
    addr hstx-cmd-raw-repeat!
    addr cell+
  ;

  \ Fill the framebuffer with a TMDS command
  : tmds!+ ( addr count -- addr' )
    swap { addr }
    addr hstx-cmd-tmds!
    addr cell+
  ;

  \ Fill the framebuffer with data
  : data!+ ( addr x -- addr' )
    swap { addr }
    addr !
    addr cell+
  ;
  
  \ Run demo
  : run-demo ( -- )
    23 out-pin
    on 23 pin!
    
    framebuf-raw
    MODE_H_FRONT_PORCH raw-repeat!+
    SYNC_V1_H1 data!+
    MODE_V_FRONT_PORCH 0 ?do
      MODE_H_SYNC_WIDTH raw-repeat!+
      SYNC_V1_H0 data!+
      MODE_H_BACK_PORCH MODE_H_ACTIVE_PIXELS + MODE_H_FRONT_PORCH + raw-repeat!+
      SYNC_V1_H1 data!+
    loop
    MODE_H_BACK_PORCH MODE_H_ACTIVE_PIXELS + over 2 cells - hstx-cmd-raw-repeat!
    MODE_H_FRONT_PORCH raw-repeat!+
    SYNC_V0_H1 data!+
    MODE_V_SYNC_WIDTH 0 ?do
      MODE_H_SYNC_WIDTH raw-repeat!+
      SYNC_V0_H0 data!+
      MODE_H_BACK_PORCH MODE_H_ACTIVE_PIXELS + MODE_H_FRONT_PORCH + raw-repeat!+
      SYNC_V0_H1 data!+
    loop
    MODE_H_BACK_PORCH MODE_H_ACTIVE_PIXELS + over 2 cells - hstx-cmd-raw-repeat!
    MODE_H_FRONT_PORCH raw-repeat!+
    SYNC_V1_H1 data!+
    MODE_V_BACK_PORCH 0 ?do
      MODE_H_SYNC_WIDTH raw-repeat!+
      SYNC_V1_H0 data!+
      MODE_H_BACK_PORCH MODE_H_ACTIVE_PIXELS + MODE_H_FRONT_PORCH + raw-repeat!+
      SYNC_V1_H1 data!+
    loop
    MODE_H_BACK_PORCH MODE_H_ACTIVE_PIXELS + over 2 cells - hstx-cmd-raw-repeat!
    MODE_V_ACTIVE_LINES 0 ?do
      MODE_H_FRONT_PORCH raw-repeat!+
      SYNC_V1_H1 data!+
      MODE_H_SYNC_WIDTH raw-repeat!+
      SYNC_V1_H0 data!+
      MODE_H_BACK_PORCH raw-repeat!+
      SYNC_V1_H1 data!+
      MODE_H_ACTIVE_PIXELS tmds!+
      [ MODE_H_ACTIVE_PIXELS 4 / ] literal +
      i [ MODE_H_ACTIVE_PIXELS 4 / ] literal * wallpaper +
      i FRAMEBUF_STRIDE * framebuf +
      [ MODE_H_ACTIVE_PIXELS 4 / ] literal move
    loop
    drop

    \ Serial output config: clock period of 5 cycles, pop from command
    \ expander every 5 cycles, shift the output shiftreg by 2 every cycle.
    false hstx-en!
    5 hstx-clockdiv!
    5 hstx-n-shifts!
    2 hstx-shift!
    true hstx-expand-en!
    
    \ Note we are leaving the HSTX clock at the SDK default of 150 MHz; since
    \ we shift out two bits per HSTX clock cycle this gives us 300 Mbps. I've
    \ manually tuned the 1280x720p30 video timing to align as closely to this
    \ as possible, ending up at 30000000/(1360*736)=29.97122 refresh rate.
    
    \ HSTX outputs 0 through 7 appear on GPIO 12 through 19.
    \ Pinout on Pico DVI sock:
    \
    \   GP12 D0+  GP13 D0-
    \   GP14 CK+  GP15 CK-
    \   GP16 D2+  GP17 D2-
    \   GP18 D1+  GP19 D1-
    
    \ Assign clock pair to two neighbouring pins:
    true 2 hstx-bit-clk!
    false 2 hstx-bit-inv!
    true 3 hstx-bit-clk!
    true 3 hstx-bit-inv!
    3 0 ?do
      i case 0 of 0 endof 1 of 6 endof 2 of 4 endof endcase { bit }
      lane 10 * bit hstx-bit-sel-p!
      lane 10 * 1+ bit hstx-bit-sel-n!
      false bit hstx-bit-inv!
      lane 10 * bit 1+ hstx-bit-sel-p!
      lane 10 * 1+ bit 1+ hstx-bit-sel-n!
      true bit 1+ hstx-bit-inv!
    loop

    \ Palette: #000000 #804000 #008080 #80c080
    1 2 hstx-l-nbits!
    26 2 hstx-l-rot!
    0 1 hstx-l-nbits!
    26 1 hstx-l-rot!
    0 0 hstx-l-nbits!
    25 0 hstx-l-rot!

    16 hstx-enc-n-shifts!
    2 hstx-enc-shift!
    1 hstx-raw-n-shifts!
    0 hstx-raw-shift!

    \ Configure the functions of the HSTX pins
    20 12 ?do i hstx-pin loop

    \ Enable HSTX
    true hstx-en!

    \ Set the DMA write and read bus priorities to high
    high dma-w-bus-priority!
    high dma-r-bus-priority!    
    
    \ _SCREEN channel does 99% of the work and handles the entire screen
    \ autonomously with the commands pre-baked into the memory chain.
    \
    \ _LOOPER exists only to restart _SCREEN without relying on any IRQ
    \ to trigger in a timely fashion, and in fact we use zero IRQs now!

    dmach-screen framebuf-raw dmach-screen dma-src-addr@ 1 cell
    HSTX_DREQ dmach-looper prepare-register>register-dma-with-chain

    dmach-looper framebuf-raw hstx-internal::HSTX_FIFO_FIFO
    FRAMEBUF_PREQUEL + FRAMEBUF_STRIDE MODE_V_ACTIVE_LINES * + cell / cell
    HSTX_DREQ dmach-screen start-buffer>register-dma-with-chain
    
    off 23 pin!
  ;
  
end-module
