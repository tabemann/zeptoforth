\ Copyright (c) 2020 Terry Porter <terry@tjporter.com.au>
\ Copyright (c) 2020 Travis Bemann
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

compile-to-flash

forth-module 1 set-order
forth-module set-current
defined? memmap-module not [if]
wordlist constant memmap-module
[then]
forth-module memmap-module 2 set-order
memmap-module set-current

execute-defined? use-DAC defined? DAC_CR_EN1 not and [if]
\ DAC_CR (read-write) Reset:0x00000000
: DAC_CR_EN1 ( -- x addr ) 0 bit DAC_CR ; \ DAC_CR_EN1, DAC channel1 enable
: DAC_CR_TEN1 ( -- x addr ) 2 bit DAC_CR ; \ DAC_CR_TEN1, DAC channel1 trigger  enable
: DAC_CR_TSEL1 ( %bbb -- x addr ) 3 lshift DAC_CR ; \ DAC_CR_TSEL1, DAC channel1 trigger  selection
: DAC_CR_WAVE1 ( %bb -- x addr ) 6 lshift DAC_CR ; \ DAC_CR_WAVE1, DAC channel1 noise/triangle wave  generation enable
: DAC_CR_MAMP1 ( %bbbb -- x addr ) 8 lshift DAC_CR ; \ DAC_CR_MAMP1, DAC channel1 mask/amplitude  selector
: DAC_CR_DMAEN1 ( -- x addr ) 12 bit DAC_CR ; \ DAC_CR_DMAEN1, DAC channel1 DMA enable
: DAC_CR_DMAUDRIE1 ( -- x addr ) 13 bit DAC_CR ; \ DAC_CR_DMAUDRIE1, DAC channel1 DMA Underrun Interrupt  enable
: DAC_CR_CEN1 ( -- x addr ) 14 bit DAC_CR ; \ DAC_CR_CEN1, DAC Channel 1 calibration  enable
: DAC_CR_EN2 ( -- x addr ) 16 bit DAC_CR ; \ DAC_CR_EN2, DAC channel2 enable
: DAC_CR_TEN2 ( -- x addr ) 18 bit DAC_CR ; \ DAC_CR_TEN2, DAC channel2 trigger  enable
: DAC_CR_TSEL2 ( %bbb -- x addr ) 19 lshift DAC_CR ; \ DAC_CR_TSEL2, DAC channel2 trigger  selection
: DAC_CR_WAVE2 ( %bb -- x addr ) 22 lshift DAC_CR ; \ DAC_CR_WAVE2, DAC channel2 noise/triangle wave  generation enable
: DAC_CR_MAMP2 ( %bbbb -- x addr ) 24 lshift DAC_CR ; \ DAC_CR_MAMP2, DAC channel2 mask/amplitude  selector
: DAC_CR_DMAEN2 ( -- x addr ) 28 bit DAC_CR ; \ DAC_CR_DMAEN2, DAC channel2 DMA enable
: DAC_CR_DMAUDRIE2 ( -- x addr ) 29 bit DAC_CR ; \ DAC_CR_DMAUDRIE2, DAC channel2 DMA underrun interrupt  enable
: DAC_CR_CEN2 ( -- x addr ) 30 bit DAC_CR ; \ DAC_CR_CEN2, DAC Channel 2 calibration  enable
[then]

execute-defined? use-DAC defined? DAC_SWTRIGR_SWTRIG1 not and [if]
\ DAC_SWTRIGR (write-only) Reset:0x00000000
: DAC_SWTRIGR_SWTRIG1 ( -- x addr ) 0 bit DAC_SWTRIGR ; \ DAC_SWTRIGR_SWTRIG1, DAC channel1 software  trigger
: DAC_SWTRIGR_SWTRIG2 ( -- x addr ) 1 bit DAC_SWTRIGR ; \ DAC_SWTRIGR_SWTRIG2, DAC channel2 software  trigger
[then]

defined? use-DAC defined? DAC_DHR12R1_DACC1DHR not and [if]
\ DAC_DHR12R1 (read-write) Reset:0x00000000
: DAC_DHR12R1_DACC1DHR ( %bbbbbbbbbbb -- x addr ) DAC_DHR12R1 ; \ DAC_DHR12R1_DACC1DHR, DAC channel1 12-bit right-aligned  data
[then]

execute-defined? use-DAC defined? DAC_DHR12L1_DACC1DHR not and [if]
\ DAC_DHR12L1 (read-write) Reset:0x00000000
: DAC_DHR12L1_DACC1DHR ( %bbbbbbbbbbb -- x addr ) 4 lshift DAC_DHR12L1 ; \ DAC_DHR12L1_DACC1DHR, DAC channel1 12-bit left-aligned  data
[then]

defined? use-DAC defined? DAC_DHR8R1_DACC1DHR not and [if]
\ DAC_DHR8R1 (read-write) Reset:0x00000000
: DAC_DHR8R1_DACC1DHR ( %bbbbbbbb -- x addr ) DAC_DHR8R1 ; \ DAC_DHR8R1_DACC1DHR, DAC channel1 8-bit right-aligned  data
[then]

execute-defined? use-DAC defined? DAC_DHR12R2_DACC2DHR not and [if]
\ DAC_DHR12R2 (read-write) Reset:0x00000000
: DAC_DHR12R2_DACC2DHR ( %bbbbbbbbbbb -- x addr ) DAC_DHR12R2 ; \ DAC_DHR12R2_DACC2DHR, DAC channel2 12-bit right-aligned  data
[then]

defined? use-DAC defined? DAC_DHR12L2_DACC2DHR not and [if]
\ DAC_DHR12L2 (read-write) Reset:0x00000000
: DAC_DHR12L2_DACC2DHR ( %bbbbbbbbbbb -- x addr ) 4 lshift DAC_DHR12L2 ; \ DAC_DHR12L2_DACC2DHR, DAC channel2 12-bit left-aligned  data
[then]

execute-defined? use-DAC defined? DAC_DHR8R2_DACC2DHR not and [if]
\ DAC_DHR8R2 (read-write) Reset:0x00000000
: DAC_DHR8R2_DACC2DHR ( %bbbbbbbb -- x addr ) DAC_DHR8R2 ; \ DAC_DHR8R2_DACC2DHR, DAC channel2 8-bit right-aligned  data
[then]

defined? use-DAC defined? DAC_DHR12RD_DACC1DHR not and [if]
\ DAC_DHR12RD (read-write) Reset:0x00000000
: DAC_DHR12RD_DACC1DHR ( %bbbbbbbbbbb -- x addr ) DAC_DHR12RD ; \ DAC_DHR12RD_DACC1DHR, DAC channel1 12-bit right-aligned  data
: DAC_DHR12RD_DACC2DHR ( %bbbbbbbbbbb -- x addr ) 16 lshift DAC_DHR12RD ; \ DAC_DHR12RD_DACC2DHR, DAC channel2 12-bit right-aligned  data
[then]

execute-defined? use-DAC defined? DAC_DHR12LD_DACC1DHR not and [if]
\ DAC_DHR12LD (read-write) Reset:0x00000000
: DAC_DHR12LD_DACC1DHR ( %bbbbbbbbbbb -- x addr ) 4 lshift DAC_DHR12LD ; \ DAC_DHR12LD_DACC1DHR, DAC channel1 12-bit left-aligned  data
: DAC_DHR12LD_DACC2DHR ( %bbbbbbbbbbb -- x addr ) 20 lshift DAC_DHR12LD ; \ DAC_DHR12LD_DACC2DHR, DAC channel2 12-bit left-aligned  data
[then]

defined? use-DAC defined? DAC_DHR8RD_DACC1DHR not and [if]
\ DAC_DHR8RD (read-write) Reset:0x00000000
: DAC_DHR8RD_DACC1DHR ( %bbbbbbbb -- x addr ) DAC_DHR8RD ; \ DAC_DHR8RD_DACC1DHR, DAC channel1 8-bit right-aligned  data
: DAC_DHR8RD_DACC2DHR ( %bbbbbbbb -- x addr ) 8 lshift DAC_DHR8RD ; \ DAC_DHR8RD_DACC2DHR, DAC channel2 8-bit right-aligned  data
[then]

execute-defined? use-DAC defined? DAC_DOR1_DACC1DOR? not and [if]
\ DAC_DOR1 (read-only) Reset:0x00000000
: DAC_DOR1_DACC1DOR? ( --  x ) DAC_DOR1 @ ; \ DAC_DOR1_DACC1DOR, DAC channel1 data output
[then]

defined? use-DAC defined? DAC_DOR2_DACC2DOR? not and [if]
\ DAC_DOR2 (read-only) Reset:0x00000000
: DAC_DOR2_DACC2DOR? ( --  x ) DAC_DOR2 @ ; \ DAC_DOR2_DACC2DOR, DAC channel2 data output
[then]

execute-defined? use-DAC defined? DAC_SR_DMAUDR1? not and [if]
\ DAC_SR (multiple-access)  Reset:0x00000000
: DAC_SR_DMAUDR1? ( -- 1|0 ) 13 bit DAC_SR bit@ ; \ DAC_SR_DMAUDR1, DAC channel1 DMA underrun  flag
: DAC_SR_CAL_FLAG1? ( -- 1|0 ) 14 bit DAC_SR bit@ ; \ DAC_SR_CAL_FLAG1, DAC Channel 1 calibration offset  status
: DAC_SR_BWST1? ( -- 1|0 ) 15 bit DAC_SR bit@ ; \ DAC_SR_BWST1, DAC Channel 1 busy writing sample time  flag
: DAC_SR_DMAUDR2? ( -- 1|0 ) 29 bit DAC_SR bit@ ; \ DAC_SR_DMAUDR2, DAC channel2 DMA underrun  flag
: DAC_SR_CAL_FLAG2? ( -- 1|0 ) 30 bit DAC_SR bit@ ; \ DAC_SR_CAL_FLAG2, DAC Channel 2 calibration offset  status
: DAC_SR_BWST2? ( -- 1|0 ) 31 bit DAC_SR bit@ ; \ DAC_SR_BWST2, DAC Channel 2 busy writing sample time  flag
[then]

defined? use-DAC defined? DAC_CCR_OTRIM1 not and [if]
\ DAC_CCR (read-write) Reset:0x00000000
: DAC_CCR_OTRIM1 ( %bbbbb -- x addr ) DAC_CCR ; \ DAC_CCR_OTRIM1, DAC Channel 1 offset trimming  value
: DAC_CCR_OTRIM2 ( %bbbbb -- x addr ) 16 lshift DAC_CCR ; \ DAC_CCR_OTRIM2, DAC Channel 2 offset trimming  value
[then]

execute-defined? use-DAC defined? DAC_MCR_MODE1 not and [if]
\ DAC_MCR (read-write) Reset:0x00000000
: DAC_MCR_MODE1 ( %bbb -- x addr ) DAC_MCR ; \ DAC_MCR_MODE1, DAC Channel 1 mode
: DAC_MCR_MODE2 ( %bbb -- x addr ) 16 lshift DAC_MCR ; \ DAC_MCR_MODE2, DAC Channel 2 mode
[then]

defined? use-DAC defined? DAC_SHSR1_TSAMPLE1 not and [if]
\ DAC_SHSR1 (read-write) Reset:0x00000000
: DAC_SHSR1_TSAMPLE1 ( %bbbbbbbbbb -- x addr ) DAC_SHSR1 ; \ DAC_SHSR1_TSAMPLE1, DAC Channel 1 sample Time
[then]

execute-defined? use-DAC defined? DAC_SHSR2_TSAMPLE2 not and [if]
\ DAC_SHSR2 (read-write) Reset:0x00000000
: DAC_SHSR2_TSAMPLE2 ( %bbbbbbbbbb -- x addr ) DAC_SHSR2 ; \ DAC_SHSR2_TSAMPLE2, DAC Channel 2 sample Time
[then]

defined? use-DAC defined? DAC_SHHR_THOLD1 not and [if]
\ DAC_SHHR (read-write) Reset:0x00010001
: DAC_SHHR_THOLD1 ( %bbbbbbbbbb -- x addr ) DAC_SHHR ; \ DAC_SHHR_THOLD1, DAC Channel 1 hold Time
: DAC_SHHR_THOLD2 ( %bbbbbbbbbb -- x addr ) 16 lshift DAC_SHHR ; \ DAC_SHHR_THOLD2, DAC Channel 2 hold time
[then]

execute-defined? use-DAC defined? DAC_SHRR_TREFRESH1 not and [if]
\ DAC_SHRR (read-write) Reset:0x00000001
: DAC_SHRR_TREFRESH1 ( %bbbbbbbb -- x addr ) DAC_SHRR ; \ DAC_SHRR_TREFRESH1, DAC Channel 1 refresh Time
: DAC_SHRR_TREFRESH2 ( %bbbbbbbb -- x addr ) 16 lshift DAC_SHRR ; \ DAC_SHRR_TREFRESH2, DAC Channel 2 refresh Time
[then]

defined? use-DMA1 defined? DMA1_ISR_TEIF7? not and [if]
\ DMA1_ISR (read-only) Reset:0x00000000
: DMA1_ISR_TEIF7? ( --  1|0 ) 27 bit DMA1_ISR bit@ ; \ DMA1_ISR_TEIF7, Channel x transfer error flag x = 1  ..7
: DMA1_ISR_HTIF7? ( --  1|0 ) 26 bit DMA1_ISR bit@ ; \ DMA1_ISR_HTIF7, Channel x half transfer flag x = 1  ..7
: DMA1_ISR_TCIF7? ( --  1|0 ) 25 bit DMA1_ISR bit@ ; \ DMA1_ISR_TCIF7, Channel x transfer complete flag x = 1  ..7
: DMA1_ISR_GIF7? ( --  1|0 ) 24 bit DMA1_ISR bit@ ; \ DMA1_ISR_GIF7, Channel x global interrupt flag x = 1  ..7
: DMA1_ISR_TEIF6? ( --  1|0 ) 23 bit DMA1_ISR bit@ ; \ DMA1_ISR_TEIF6, Channel x transfer error flag x = 1  ..7
: DMA1_ISR_HTIF6? ( --  1|0 ) 22 bit DMA1_ISR bit@ ; \ DMA1_ISR_HTIF6, Channel x half transfer flag x = 1  ..7
: DMA1_ISR_TCIF6? ( --  1|0 ) 21 bit DMA1_ISR bit@ ; \ DMA1_ISR_TCIF6, Channel x transfer complete flag x = 1  ..7
: DMA1_ISR_GIF6? ( --  1|0 ) 20 bit DMA1_ISR bit@ ; \ DMA1_ISR_GIF6, Channel x global interrupt flag x = 1  ..7
: DMA1_ISR_TEIF5? ( --  1|0 ) 19 bit DMA1_ISR bit@ ; \ DMA1_ISR_TEIF5, Channel x transfer error flag x = 1  ..7
: DMA1_ISR_HTIF5? ( --  1|0 ) 18 bit DMA1_ISR bit@ ; \ DMA1_ISR_HTIF5, Channel x half transfer flag x = 1  ..7
: DMA1_ISR_TCIF5? ( --  1|0 ) 17 bit DMA1_ISR bit@ ; \ DMA1_ISR_TCIF5, Channel x transfer complete flag x = 1  ..7
: DMA1_ISR_GIF5? ( --  1|0 ) 16 bit DMA1_ISR bit@ ; \ DMA1_ISR_GIF5, Channel x global interrupt flag x = 1  ..7
: DMA1_ISR_TEIF4? ( --  1|0 ) 15 bit DMA1_ISR bit@ ; \ DMA1_ISR_TEIF4, Channel x transfer error flag x = 1  ..7
: DMA1_ISR_HTIF4? ( --  1|0 ) 14 bit DMA1_ISR bit@ ; \ DMA1_ISR_HTIF4, Channel x half transfer flag x = 1  ..7
: DMA1_ISR_TCIF4? ( --  1|0 ) 13 bit DMA1_ISR bit@ ; \ DMA1_ISR_TCIF4, Channel x transfer complete flag x = 1  ..7
: DMA1_ISR_GIF4? ( --  1|0 ) 12 bit DMA1_ISR bit@ ; \ DMA1_ISR_GIF4, Channel x global interrupt flag x = 1  ..7
: DMA1_ISR_TEIF3? ( --  1|0 ) 11 bit DMA1_ISR bit@ ; \ DMA1_ISR_TEIF3, Channel x transfer error flag x = 1  ..7
: DMA1_ISR_HTIF3? ( --  1|0 ) 10 bit DMA1_ISR bit@ ; \ DMA1_ISR_HTIF3, Channel x half transfer flag x = 1  ..7
: DMA1_ISR_TCIF3? ( --  1|0 ) 9 bit DMA1_ISR bit@ ; \ DMA1_ISR_TCIF3, Channel x transfer complete flag x = 1  ..7
: DMA1_ISR_GIF3? ( --  1|0 ) 8 bit DMA1_ISR bit@ ; \ DMA1_ISR_GIF3, Channel x global interrupt flag x = 1  ..7
: DMA1_ISR_TEIF2? ( --  1|0 ) 7 bit DMA1_ISR bit@ ; \ DMA1_ISR_TEIF2, Channel x transfer error flag x = 1  ..7
: DMA1_ISR_HTIF2? ( --  1|0 ) 6 bit DMA1_ISR bit@ ; \ DMA1_ISR_HTIF2, Channel x half transfer flag x = 1  ..7
: DMA1_ISR_TCIF2? ( --  1|0 ) 5 bit DMA1_ISR bit@ ; \ DMA1_ISR_TCIF2, Channel x transfer complete flag x = 1  ..7
: DMA1_ISR_GIF2? ( --  1|0 ) 4 bit DMA1_ISR bit@ ; \ DMA1_ISR_GIF2, Channel x global interrupt flag x = 1  ..7
: DMA1_ISR_TEIF1? ( --  1|0 ) 3 bit DMA1_ISR bit@ ; \ DMA1_ISR_TEIF1, Channel x transfer error flag x = 1  ..7
: DMA1_ISR_HTIF1? ( --  1|0 ) 2 bit DMA1_ISR bit@ ; \ DMA1_ISR_HTIF1, Channel x half transfer flag x = 1  ..7
: DMA1_ISR_TCIF1? ( --  1|0 ) 1 bit DMA1_ISR bit@ ; \ DMA1_ISR_TCIF1, Channel x transfer complete flag x = 1  ..7
: DMA1_ISR_GIF1? ( --  1|0 ) 0 bit DMA1_ISR bit@ ; \ DMA1_ISR_GIF1, Channel x global interrupt flag x = 1  ..7
[then]

execute-defined? use-DMA1 defined? DMA1_IFCR_CTEIF7 not and [if]
\ DMA1_IFCR (write-only) Reset:0x00000000
: DMA1_IFCR_CTEIF7 ( -- x addr ) 27 bit DMA1_IFCR ; \ DMA1_IFCR_CTEIF7, Channel x transfer error clear x = 1  ..7
: DMA1_IFCR_CHTIF7 ( -- x addr ) 26 bit DMA1_IFCR ; \ DMA1_IFCR_CHTIF7, Channel x half transfer clear x = 1  ..7
: DMA1_IFCR_CTCIF7 ( -- x addr ) 25 bit DMA1_IFCR ; \ DMA1_IFCR_CTCIF7, Channel x transfer complete clear x = 1  ..7
: DMA1_IFCR_CGIF7 ( -- x addr ) 24 bit DMA1_IFCR ; \ DMA1_IFCR_CGIF7, Channel x global interrupt clear x = 1  ..7
: DMA1_IFCR_CTEIF6 ( -- x addr ) 23 bit DMA1_IFCR ; \ DMA1_IFCR_CTEIF6, Channel x transfer error clear x = 1  ..7
: DMA1_IFCR_CHTIF6 ( -- x addr ) 22 bit DMA1_IFCR ; \ DMA1_IFCR_CHTIF6, Channel x half transfer clear x = 1  ..7
: DMA1_IFCR_CTCIF6 ( -- x addr ) 21 bit DMA1_IFCR ; \ DMA1_IFCR_CTCIF6, Channel x transfer complete clear x = 1  ..7
: DMA1_IFCR_CGIF6 ( -- x addr ) 20 bit DMA1_IFCR ; \ DMA1_IFCR_CGIF6, Channel x global interrupt clear x = 1  ..7
: DMA1_IFCR_CTEIF5 ( -- x addr ) 19 bit DMA1_IFCR ; \ DMA1_IFCR_CTEIF5, Channel x transfer error clear x = 1  ..7
: DMA1_IFCR_CHTIF5 ( -- x addr ) 18 bit DMA1_IFCR ; \ DMA1_IFCR_CHTIF5, Channel x half transfer clear x = 1  ..7
: DMA1_IFCR_CTCIF5 ( -- x addr ) 17 bit DMA1_IFCR ; \ DMA1_IFCR_CTCIF5, Channel x transfer complete clear x = 1  ..7
: DMA1_IFCR_CGIF5 ( -- x addr ) 16 bit DMA1_IFCR ; \ DMA1_IFCR_CGIF5, Channel x global interrupt clear x = 1  ..7
: DMA1_IFCR_CTEIF4 ( -- x addr ) 15 bit DMA1_IFCR ; \ DMA1_IFCR_CTEIF4, Channel x transfer error clear x = 1  ..7
: DMA1_IFCR_CHTIF4 ( -- x addr ) 14 bit DMA1_IFCR ; \ DMA1_IFCR_CHTIF4, Channel x half transfer clear x = 1  ..7
: DMA1_IFCR_CTCIF4 ( -- x addr ) 13 bit DMA1_IFCR ; \ DMA1_IFCR_CTCIF4, Channel x transfer complete clear x = 1  ..7
: DMA1_IFCR_CGIF4 ( -- x addr ) 12 bit DMA1_IFCR ; \ DMA1_IFCR_CGIF4, Channel x global interrupt clear x = 1  ..7
: DMA1_IFCR_CTEIF3 ( -- x addr ) 11 bit DMA1_IFCR ; \ DMA1_IFCR_CTEIF3, Channel x transfer error clear x = 1  ..7
: DMA1_IFCR_CHTIF3 ( -- x addr ) 10 bit DMA1_IFCR ; \ DMA1_IFCR_CHTIF3, Channel x half transfer clear x = 1  ..7
: DMA1_IFCR_CTCIF3 ( -- x addr ) 9 bit DMA1_IFCR ; \ DMA1_IFCR_CTCIF3, Channel x transfer complete clear x = 1  ..7
: DMA1_IFCR_CGIF3 ( -- x addr ) 8 bit DMA1_IFCR ; \ DMA1_IFCR_CGIF3, Channel x global interrupt clear x = 1  ..7
: DMA1_IFCR_CTEIF2 ( -- x addr ) 7 bit DMA1_IFCR ; \ DMA1_IFCR_CTEIF2, Channel x transfer error clear x = 1  ..7
: DMA1_IFCR_CHTIF2 ( -- x addr ) 6 bit DMA1_IFCR ; \ DMA1_IFCR_CHTIF2, Channel x half transfer clear x = 1  ..7
: DMA1_IFCR_CTCIF2 ( -- x addr ) 5 bit DMA1_IFCR ; \ DMA1_IFCR_CTCIF2, Channel x transfer complete clear x = 1  ..7
: DMA1_IFCR_CGIF2 ( -- x addr ) 4 bit DMA1_IFCR ; \ DMA1_IFCR_CGIF2, Channel x global interrupt clear x = 1  ..7
: DMA1_IFCR_CTEIF1 ( -- x addr ) 3 bit DMA1_IFCR ; \ DMA1_IFCR_CTEIF1, Channel x transfer error clear x = 1  ..7
: DMA1_IFCR_CHTIF1 ( -- x addr ) 2 bit DMA1_IFCR ; \ DMA1_IFCR_CHTIF1, Channel x half transfer clear x = 1  ..7
: DMA1_IFCR_CTCIF1 ( -- x addr ) 1 bit DMA1_IFCR ; \ DMA1_IFCR_CTCIF1, Channel x transfer complete clear x = 1  ..7
: DMA1_IFCR_CGIF1 ( -- x addr ) 0 bit DMA1_IFCR ; \ DMA1_IFCR_CGIF1, Channel x global interrupt clear x = 1  ..7
[then]

defined? use-DMA1 defined? DMA1_CCR1_MEM2MEM not and [if]
\ DMA1_CCR1 (read-write) Reset:0x00000000
: DMA1_CCR1_MEM2MEM ( -- x addr ) 14 bit DMA1_CCR1 ; \ DMA1_CCR1_MEM2MEM, Memory to memory mode
: DMA1_CCR1_PL ( %bb -- x addr ) 12 lshift DMA1_CCR1 ; \ DMA1_CCR1_PL, Channel priority level
: DMA1_CCR1_MSIZE ( %bb -- x addr ) 10 lshift DMA1_CCR1 ; \ DMA1_CCR1_MSIZE, Memory size
: DMA1_CCR1_PSIZE ( %bb -- x addr ) 8 lshift DMA1_CCR1 ; \ DMA1_CCR1_PSIZE, Peripheral size
: DMA1_CCR1_MINC ( -- x addr ) 7 bit DMA1_CCR1 ; \ DMA1_CCR1_MINC, Memory increment mode
: DMA1_CCR1_PINC ( -- x addr ) 6 bit DMA1_CCR1 ; \ DMA1_CCR1_PINC, Peripheral increment mode
: DMA1_CCR1_CIRC ( -- x addr ) 5 bit DMA1_CCR1 ; \ DMA1_CCR1_CIRC, Circular mode
: DMA1_CCR1_DIR ( -- x addr ) 4 bit DMA1_CCR1 ; \ DMA1_CCR1_DIR, Data transfer direction
: DMA1_CCR1_TEIE ( -- x addr ) 3 bit DMA1_CCR1 ; \ DMA1_CCR1_TEIE, Transfer error interrupt  enable
: DMA1_CCR1_HTIE ( -- x addr ) 2 bit DMA1_CCR1 ; \ DMA1_CCR1_HTIE, Half transfer interrupt  enable
: DMA1_CCR1_TCIE ( -- x addr ) 1 bit DMA1_CCR1 ; \ DMA1_CCR1_TCIE, Transfer complete interrupt  enable
: DMA1_CCR1_EN ( -- x addr ) 0 bit DMA1_CCR1 ; \ DMA1_CCR1_EN, Channel enable
[then]

execute-defined? use-DMA1 defined? DMA1_CNDTR1_NDT not and [if]
\ DMA1_CNDTR1 (read-write) Reset:0x00000000
: DMA1_CNDTR1_NDT ( %bbbbbbbbbbbbbbbb -- x addr ) DMA1_CNDTR1 ; \ DMA1_CNDTR1_NDT, Number of data to transfer
[then]

defined? use-DMA1 defined? DMA1_CPAR1_PA not and [if]
\ DMA1_CPAR1 (read-write) Reset:0x00000000
: DMA1_CPAR1_PA ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) DMA1_CPAR1 ; \ DMA1_CPAR1_PA, Peripheral address
[then]

execute-defined? use-DMA1 defined? DMA1_CMAR1_MA not and [if]
\ DMA1_CMAR1 (read-write) Reset:0x00000000
: DMA1_CMAR1_MA ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) DMA1_CMAR1 ; \ DMA1_CMAR1_MA, Memory address
[then]

defined? use-DMA1 defined? DMA1_CCR2_MEM2MEM not and [if]
\ DMA1_CCR2 (read-write) Reset:0x00000000
: DMA1_CCR2_MEM2MEM ( -- x addr ) 14 bit DMA1_CCR2 ; \ DMA1_CCR2_MEM2MEM, Memory to memory mode
: DMA1_CCR2_PL ( %bb -- x addr ) 12 lshift DMA1_CCR2 ; \ DMA1_CCR2_PL, Channel priority level
: DMA1_CCR2_MSIZE ( %bb -- x addr ) 10 lshift DMA1_CCR2 ; \ DMA1_CCR2_MSIZE, Memory size
: DMA1_CCR2_PSIZE ( %bb -- x addr ) 8 lshift DMA1_CCR2 ; \ DMA1_CCR2_PSIZE, Peripheral size
: DMA1_CCR2_MINC ( -- x addr ) 7 bit DMA1_CCR2 ; \ DMA1_CCR2_MINC, Memory increment mode
: DMA1_CCR2_PINC ( -- x addr ) 6 bit DMA1_CCR2 ; \ DMA1_CCR2_PINC, Peripheral increment mode
: DMA1_CCR2_CIRC ( -- x addr ) 5 bit DMA1_CCR2 ; \ DMA1_CCR2_CIRC, Circular mode
: DMA1_CCR2_DIR ( -- x addr ) 4 bit DMA1_CCR2 ; \ DMA1_CCR2_DIR, Data transfer direction
: DMA1_CCR2_TEIE ( -- x addr ) 3 bit DMA1_CCR2 ; \ DMA1_CCR2_TEIE, Transfer error interrupt  enable
: DMA1_CCR2_HTIE ( -- x addr ) 2 bit DMA1_CCR2 ; \ DMA1_CCR2_HTIE, Half transfer interrupt  enable
: DMA1_CCR2_TCIE ( -- x addr ) 1 bit DMA1_CCR2 ; \ DMA1_CCR2_TCIE, Transfer complete interrupt  enable
: DMA1_CCR2_EN ( -- x addr ) 0 bit DMA1_CCR2 ; \ DMA1_CCR2_EN, Channel enable
[then]

execute-defined? use-DMA1 defined? DMA1_CNDTR2_NDT not and [if]
\ DMA1_CNDTR2 (read-write) Reset:0x00000000
: DMA1_CNDTR2_NDT ( %bbbbbbbbbbbbbbbb -- x addr ) DMA1_CNDTR2 ; \ DMA1_CNDTR2_NDT, Number of data to transfer
[then]

defined? use-DMA1 defined? DMA1_CPAR2_PA not and [if]
\ DMA1_CPAR2 (read-write) Reset:0x00000000
: DMA1_CPAR2_PA ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) DMA1_CPAR2 ; \ DMA1_CPAR2_PA, Peripheral address
[then]

execute-defined? use-DMA1 defined? DMA1_CMAR2_MA not and [if]
\ DMA1_CMAR2 (read-write) Reset:0x00000000
: DMA1_CMAR2_MA ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) DMA1_CMAR2 ; \ DMA1_CMAR2_MA, Memory address
[then]

defined? use-DMA1 defined? DMA1_CCR3_MEM2MEM not and [if]
\ DMA1_CCR3 (read-write) Reset:0x00000000
: DMA1_CCR3_MEM2MEM ( -- x addr ) 14 bit DMA1_CCR3 ; \ DMA1_CCR3_MEM2MEM, Memory to memory mode
: DMA1_CCR3_PL ( %bb -- x addr ) 12 lshift DMA1_CCR3 ; \ DMA1_CCR3_PL, Channel priority level
: DMA1_CCR3_MSIZE ( %bb -- x addr ) 10 lshift DMA1_CCR3 ; \ DMA1_CCR3_MSIZE, Memory size
: DMA1_CCR3_PSIZE ( %bb -- x addr ) 8 lshift DMA1_CCR3 ; \ DMA1_CCR3_PSIZE, Peripheral size
: DMA1_CCR3_MINC ( -- x addr ) 7 bit DMA1_CCR3 ; \ DMA1_CCR3_MINC, Memory increment mode
: DMA1_CCR3_PINC ( -- x addr ) 6 bit DMA1_CCR3 ; \ DMA1_CCR3_PINC, Peripheral increment mode
: DMA1_CCR3_CIRC ( -- x addr ) 5 bit DMA1_CCR3 ; \ DMA1_CCR3_CIRC, Circular mode
: DMA1_CCR3_DIR ( -- x addr ) 4 bit DMA1_CCR3 ; \ DMA1_CCR3_DIR, Data transfer direction
: DMA1_CCR3_TEIE ( -- x addr ) 3 bit DMA1_CCR3 ; \ DMA1_CCR3_TEIE, Transfer error interrupt  enable
: DMA1_CCR3_HTIE ( -- x addr ) 2 bit DMA1_CCR3 ; \ DMA1_CCR3_HTIE, Half transfer interrupt  enable
: DMA1_CCR3_TCIE ( -- x addr ) 1 bit DMA1_CCR3 ; \ DMA1_CCR3_TCIE, Transfer complete interrupt  enable
: DMA1_CCR3_EN ( -- x addr ) 0 bit DMA1_CCR3 ; \ DMA1_CCR3_EN, Channel enable
[then]

execute-defined? use-DMA1 defined? DMA1_CNDTR3_NDT not and [if]
\ DMA1_CNDTR3 (read-write) Reset:0x00000000
: DMA1_CNDTR3_NDT ( %bbbbbbbbbbbbbbbb -- x addr ) DMA1_CNDTR3 ; \ DMA1_CNDTR3_NDT, Number of data to transfer
[then]

defined? use-DMA1 defined? DMA1_CPAR3_PA not and [if]
\ DMA1_CPAR3 (read-write) Reset:0x00000000
: DMA1_CPAR3_PA ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) DMA1_CPAR3 ; \ DMA1_CPAR3_PA, Peripheral address
[then]

execute-defined? use-DMA1 defined? DMA1_CMAR3_MA not and [if]
\ DMA1_CMAR3 (read-write) Reset:0x00000000
: DMA1_CMAR3_MA ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) DMA1_CMAR3 ; \ DMA1_CMAR3_MA, Memory address
[then]

defined? use-DMA1 defined? DMA1_CCR4_MEM2MEM not and [if]
\ DMA1_CCR4 (read-write) Reset:0x00000000
: DMA1_CCR4_MEM2MEM ( -- x addr ) 14 bit DMA1_CCR4 ; \ DMA1_CCR4_MEM2MEM, Memory to memory mode
: DMA1_CCR4_PL ( %bb -- x addr ) 12 lshift DMA1_CCR4 ; \ DMA1_CCR4_PL, Channel priority level
: DMA1_CCR4_MSIZE ( %bb -- x addr ) 10 lshift DMA1_CCR4 ; \ DMA1_CCR4_MSIZE, Memory size
: DMA1_CCR4_PSIZE ( %bb -- x addr ) 8 lshift DMA1_CCR4 ; \ DMA1_CCR4_PSIZE, Peripheral size
: DMA1_CCR4_MINC ( -- x addr ) 7 bit DMA1_CCR4 ; \ DMA1_CCR4_MINC, Memory increment mode
: DMA1_CCR4_PINC ( -- x addr ) 6 bit DMA1_CCR4 ; \ DMA1_CCR4_PINC, Peripheral increment mode
: DMA1_CCR4_CIRC ( -- x addr ) 5 bit DMA1_CCR4 ; \ DMA1_CCR4_CIRC, Circular mode
: DMA1_CCR4_DIR ( -- x addr ) 4 bit DMA1_CCR4 ; \ DMA1_CCR4_DIR, Data transfer direction
: DMA1_CCR4_TEIE ( -- x addr ) 3 bit DMA1_CCR4 ; \ DMA1_CCR4_TEIE, Transfer error interrupt  enable
: DMA1_CCR4_HTIE ( -- x addr ) 2 bit DMA1_CCR4 ; \ DMA1_CCR4_HTIE, Half transfer interrupt  enable
: DMA1_CCR4_TCIE ( -- x addr ) 1 bit DMA1_CCR4 ; \ DMA1_CCR4_TCIE, Transfer complete interrupt  enable
: DMA1_CCR4_EN ( -- x addr ) 0 bit DMA1_CCR4 ; \ DMA1_CCR4_EN, Channel enable
[then]

execute-defined? use-DMA1 defined? DMA1_CNDTR4_NDT not and [if]
\ DMA1_CNDTR4 (read-write) Reset:0x00000000
: DMA1_CNDTR4_NDT ( %bbbbbbbbbbbbbbbb -- x addr ) DMA1_CNDTR4 ; \ DMA1_CNDTR4_NDT, Number of data to transfer
[then]

defined? use-DMA1 defined? DMA1_CPAR4_PA not and [if]
\ DMA1_CPAR4 (read-write) Reset:0x00000000
: DMA1_CPAR4_PA ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) DMA1_CPAR4 ; \ DMA1_CPAR4_PA, Peripheral address
[then]

execute-defined? use-DMA1 defined? DMA1_CMAR4_MA not and [if]
\ DMA1_CMAR4 (read-write) Reset:0x00000000
: DMA1_CMAR4_MA ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) DMA1_CMAR4 ; \ DMA1_CMAR4_MA, Memory address
[then]

defined? use-DMA1 defined? DMA1_CCR5_MEM2MEM not and [if]
\ DMA1_CCR5 (read-write) Reset:0x00000000
: DMA1_CCR5_MEM2MEM ( -- x addr ) 14 bit DMA1_CCR5 ; \ DMA1_CCR5_MEM2MEM, Memory to memory mode
: DMA1_CCR5_PL ( %bb -- x addr ) 12 lshift DMA1_CCR5 ; \ DMA1_CCR5_PL, Channel priority level
: DMA1_CCR5_MSIZE ( %bb -- x addr ) 10 lshift DMA1_CCR5 ; \ DMA1_CCR5_MSIZE, Memory size
: DMA1_CCR5_PSIZE ( %bb -- x addr ) 8 lshift DMA1_CCR5 ; \ DMA1_CCR5_PSIZE, Peripheral size
: DMA1_CCR5_MINC ( -- x addr ) 7 bit DMA1_CCR5 ; \ DMA1_CCR5_MINC, Memory increment mode
: DMA1_CCR5_PINC ( -- x addr ) 6 bit DMA1_CCR5 ; \ DMA1_CCR5_PINC, Peripheral increment mode
: DMA1_CCR5_CIRC ( -- x addr ) 5 bit DMA1_CCR5 ; \ DMA1_CCR5_CIRC, Circular mode
: DMA1_CCR5_DIR ( -- x addr ) 4 bit DMA1_CCR5 ; \ DMA1_CCR5_DIR, Data transfer direction
: DMA1_CCR5_TEIE ( -- x addr ) 3 bit DMA1_CCR5 ; \ DMA1_CCR5_TEIE, Transfer error interrupt  enable
: DMA1_CCR5_HTIE ( -- x addr ) 2 bit DMA1_CCR5 ; \ DMA1_CCR5_HTIE, Half transfer interrupt  enable
: DMA1_CCR5_TCIE ( -- x addr ) 1 bit DMA1_CCR5 ; \ DMA1_CCR5_TCIE, Transfer complete interrupt  enable
: DMA1_CCR5_EN ( -- x addr ) 0 bit DMA1_CCR5 ; \ DMA1_CCR5_EN, Channel enable
[then]

execute-defined? use-DMA1 defined? DMA1_CNDTR5_NDT not and [if]
\ DMA1_CNDTR5 (read-write) Reset:0x00000000
: DMA1_CNDTR5_NDT ( %bbbbbbbbbbbbbbbb -- x addr ) DMA1_CNDTR5 ; \ DMA1_CNDTR5_NDT, Number of data to transfer
[then]

defined? use-DMA1 defined? DMA1_CPAR5_PA not and [if]
\ DMA1_CPAR5 (read-write) Reset:0x00000000
: DMA1_CPAR5_PA ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) DMA1_CPAR5 ; \ DMA1_CPAR5_PA, Peripheral address
[then]

execute-defined? use-DMA1 defined? DMA1_CMAR5_MA not and [if]
\ DMA1_CMAR5 (read-write) Reset:0x00000000
: DMA1_CMAR5_MA ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) DMA1_CMAR5 ; \ DMA1_CMAR5_MA, Memory address
[then]

defined? use-DMA1 defined? DMA1_CCR6_MEM2MEM not and [if]
\ DMA1_CCR6 (read-write) Reset:0x00000000
: DMA1_CCR6_MEM2MEM ( -- x addr ) 14 bit DMA1_CCR6 ; \ DMA1_CCR6_MEM2MEM, Memory to memory mode
: DMA1_CCR6_PL ( %bb -- x addr ) 12 lshift DMA1_CCR6 ; \ DMA1_CCR6_PL, Channel priority level
: DMA1_CCR6_MSIZE ( %bb -- x addr ) 10 lshift DMA1_CCR6 ; \ DMA1_CCR6_MSIZE, Memory size
: DMA1_CCR6_PSIZE ( %bb -- x addr ) 8 lshift DMA1_CCR6 ; \ DMA1_CCR6_PSIZE, Peripheral size
: DMA1_CCR6_MINC ( -- x addr ) 7 bit DMA1_CCR6 ; \ DMA1_CCR6_MINC, Memory increment mode
: DMA1_CCR6_PINC ( -- x addr ) 6 bit DMA1_CCR6 ; \ DMA1_CCR6_PINC, Peripheral increment mode
: DMA1_CCR6_CIRC ( -- x addr ) 5 bit DMA1_CCR6 ; \ DMA1_CCR6_CIRC, Circular mode
: DMA1_CCR6_DIR ( -- x addr ) 4 bit DMA1_CCR6 ; \ DMA1_CCR6_DIR, Data transfer direction
: DMA1_CCR6_TEIE ( -- x addr ) 3 bit DMA1_CCR6 ; \ DMA1_CCR6_TEIE, Transfer error interrupt  enable
: DMA1_CCR6_HTIE ( -- x addr ) 2 bit DMA1_CCR6 ; \ DMA1_CCR6_HTIE, Half transfer interrupt  enable
: DMA1_CCR6_TCIE ( -- x addr ) 1 bit DMA1_CCR6 ; \ DMA1_CCR6_TCIE, Transfer complete interrupt  enable
: DMA1_CCR6_EN ( -- x addr ) 0 bit DMA1_CCR6 ; \ DMA1_CCR6_EN, Channel enable
[then]

execute-defined? use-DMA1 defined? DMA1_CNDTR6_NDT not and [if]
\ DMA1_CNDTR6 (read-write) Reset:0x00000000
: DMA1_CNDTR6_NDT ( %bbbbbbbbbbbbbbbb -- x addr ) DMA1_CNDTR6 ; \ DMA1_CNDTR6_NDT, Number of data to transfer
[then]

defined? use-DMA1 defined? DMA1_CPAR6_PA not and [if]
\ DMA1_CPAR6 (read-write) Reset:0x00000000
: DMA1_CPAR6_PA ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) DMA1_CPAR6 ; \ DMA1_CPAR6_PA, Peripheral address
[then]

execute-defined? use-DMA1 defined? DMA1_CMAR6_MA not and [if]
\ DMA1_CMAR6 (read-write) Reset:0x00000000
: DMA1_CMAR6_MA ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) DMA1_CMAR6 ; \ DMA1_CMAR6_MA, Memory address
[then]

defined? use-DMA1 defined? DMA1_CCR7_MEM2MEM not and [if]
\ DMA1_CCR7 (read-write) Reset:0x00000000
: DMA1_CCR7_MEM2MEM ( -- x addr ) 14 bit DMA1_CCR7 ; \ DMA1_CCR7_MEM2MEM, Memory to memory mode
: DMA1_CCR7_PL ( %bb -- x addr ) 12 lshift DMA1_CCR7 ; \ DMA1_CCR7_PL, Channel priority level
: DMA1_CCR7_MSIZE ( %bb -- x addr ) 10 lshift DMA1_CCR7 ; \ DMA1_CCR7_MSIZE, Memory size
: DMA1_CCR7_PSIZE ( %bb -- x addr ) 8 lshift DMA1_CCR7 ; \ DMA1_CCR7_PSIZE, Peripheral size
: DMA1_CCR7_MINC ( -- x addr ) 7 bit DMA1_CCR7 ; \ DMA1_CCR7_MINC, Memory increment mode
: DMA1_CCR7_PINC ( -- x addr ) 6 bit DMA1_CCR7 ; \ DMA1_CCR7_PINC, Peripheral increment mode
: DMA1_CCR7_CIRC ( -- x addr ) 5 bit DMA1_CCR7 ; \ DMA1_CCR7_CIRC, Circular mode
: DMA1_CCR7_DIR ( -- x addr ) 4 bit DMA1_CCR7 ; \ DMA1_CCR7_DIR, Data transfer direction
: DMA1_CCR7_TEIE ( -- x addr ) 3 bit DMA1_CCR7 ; \ DMA1_CCR7_TEIE, Transfer error interrupt  enable
: DMA1_CCR7_HTIE ( -- x addr ) 2 bit DMA1_CCR7 ; \ DMA1_CCR7_HTIE, Half transfer interrupt  enable
: DMA1_CCR7_TCIE ( -- x addr ) 1 bit DMA1_CCR7 ; \ DMA1_CCR7_TCIE, Transfer complete interrupt  enable
: DMA1_CCR7_EN ( -- x addr ) 0 bit DMA1_CCR7 ; \ DMA1_CCR7_EN, Channel enable
[then]

execute-defined? use-DMA1 defined? DMA1_CNDTR7_NDT not and [if]
\ DMA1_CNDTR7 (read-write) Reset:0x00000000
: DMA1_CNDTR7_NDT ( %bbbbbbbbbbbbbbbb -- x addr ) DMA1_CNDTR7 ; \ DMA1_CNDTR7_NDT, Number of data to transfer
[then]

defined? use-DMA1 defined? DMA1_CPAR7_PA not and [if]
\ DMA1_CPAR7 (read-write) Reset:0x00000000
: DMA1_CPAR7_PA ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) DMA1_CPAR7 ; \ DMA1_CPAR7_PA, Peripheral address
[then]

execute-defined? use-DMA1 defined? DMA1_CMAR7_MA not and [if]
\ DMA1_CMAR7 (read-write) Reset:0x00000000
: DMA1_CMAR7_MA ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) DMA1_CMAR7 ; \ DMA1_CMAR7_MA, Memory address
[then]

defined? use-DMA1 defined? DMA1_CSELR_C7S not and [if]
\ DMA1_CSELR (read-write) Reset:0x00000000
: DMA1_CSELR_C7S ( %bbbb -- x addr ) 24 lshift DMA1_CSELR ; \ DMA1_CSELR_C7S, DMA channel 7 selection
: DMA1_CSELR_C6S ( %bbbb -- x addr ) 20 lshift DMA1_CSELR ; \ DMA1_CSELR_C6S, DMA channel 6 selection
: DMA1_CSELR_C5S ( %bbbb -- x addr ) 16 lshift DMA1_CSELR ; \ DMA1_CSELR_C5S, DMA channel 5 selection
: DMA1_CSELR_C4S ( %bbbb -- x addr ) 12 lshift DMA1_CSELR ; \ DMA1_CSELR_C4S, DMA channel 4 selection
: DMA1_CSELR_C3S ( %bbbb -- x addr ) 8 lshift DMA1_CSELR ; \ DMA1_CSELR_C3S, DMA channel 3 selection
: DMA1_CSELR_C2S ( %bbbb -- x addr ) 4 lshift DMA1_CSELR ; \ DMA1_CSELR_C2S, DMA channel 2 selection
: DMA1_CSELR_C1S ( %bbbb -- x addr ) DMA1_CSELR ; \ DMA1_CSELR_C1S, DMA channel 1 selection
[then]

execute-defined? use-DMA2 defined? DMA2_ISR_TEIF7? not and [if]
\ DMA2_ISR (read-only) Reset:0x00000000
: DMA2_ISR_TEIF7? ( --  1|0 ) 27 bit DMA2_ISR bit@ ; \ DMA2_ISR_TEIF7, Channel x transfer error flag x = 1  ..7
: DMA2_ISR_HTIF7? ( --  1|0 ) 26 bit DMA2_ISR bit@ ; \ DMA2_ISR_HTIF7, Channel x half transfer flag x = 1  ..7
: DMA2_ISR_TCIF7? ( --  1|0 ) 25 bit DMA2_ISR bit@ ; \ DMA2_ISR_TCIF7, Channel x transfer complete flag x = 1  ..7
: DMA2_ISR_GIF7? ( --  1|0 ) 24 bit DMA2_ISR bit@ ; \ DMA2_ISR_GIF7, Channel x global interrupt flag x = 1  ..7
: DMA2_ISR_TEIF6? ( --  1|0 ) 23 bit DMA2_ISR bit@ ; \ DMA2_ISR_TEIF6, Channel x transfer error flag x = 1  ..7
: DMA2_ISR_HTIF6? ( --  1|0 ) 22 bit DMA2_ISR bit@ ; \ DMA2_ISR_HTIF6, Channel x half transfer flag x = 1  ..7
: DMA2_ISR_TCIF6? ( --  1|0 ) 21 bit DMA2_ISR bit@ ; \ DMA2_ISR_TCIF6, Channel x transfer complete flag x = 1  ..7
: DMA2_ISR_GIF6? ( --  1|0 ) 20 bit DMA2_ISR bit@ ; \ DMA2_ISR_GIF6, Channel x global interrupt flag x = 1  ..7
: DMA2_ISR_TEIF5? ( --  1|0 ) 19 bit DMA2_ISR bit@ ; \ DMA2_ISR_TEIF5, Channel x transfer error flag x = 1  ..7
: DMA2_ISR_HTIF5? ( --  1|0 ) 18 bit DMA2_ISR bit@ ; \ DMA2_ISR_HTIF5, Channel x half transfer flag x = 1  ..7
: DMA2_ISR_TCIF5? ( --  1|0 ) 17 bit DMA2_ISR bit@ ; \ DMA2_ISR_TCIF5, Channel x transfer complete flag x = 1  ..7
: DMA2_ISR_GIF5? ( --  1|0 ) 16 bit DMA2_ISR bit@ ; \ DMA2_ISR_GIF5, Channel x global interrupt flag x = 1  ..7
: DMA2_ISR_TEIF4? ( --  1|0 ) 15 bit DMA2_ISR bit@ ; \ DMA2_ISR_TEIF4, Channel x transfer error flag x = 1  ..7
: DMA2_ISR_HTIF4? ( --  1|0 ) 14 bit DMA2_ISR bit@ ; \ DMA2_ISR_HTIF4, Channel x half transfer flag x = 1  ..7
: DMA2_ISR_TCIF4? ( --  1|0 ) 13 bit DMA2_ISR bit@ ; \ DMA2_ISR_TCIF4, Channel x transfer complete flag x = 1  ..7
: DMA2_ISR_GIF4? ( --  1|0 ) 12 bit DMA2_ISR bit@ ; \ DMA2_ISR_GIF4, Channel x global interrupt flag x = 1  ..7
: DMA2_ISR_TEIF3? ( --  1|0 ) 11 bit DMA2_ISR bit@ ; \ DMA2_ISR_TEIF3, Channel x transfer error flag x = 1  ..7
: DMA2_ISR_HTIF3? ( --  1|0 ) 10 bit DMA2_ISR bit@ ; \ DMA2_ISR_HTIF3, Channel x half transfer flag x = 1  ..7
: DMA2_ISR_TCIF3? ( --  1|0 ) 9 bit DMA2_ISR bit@ ; \ DMA2_ISR_TCIF3, Channel x transfer complete flag x = 1  ..7
: DMA2_ISR_GIF3? ( --  1|0 ) 8 bit DMA2_ISR bit@ ; \ DMA2_ISR_GIF3, Channel x global interrupt flag x = 1  ..7
: DMA2_ISR_TEIF2? ( --  1|0 ) 7 bit DMA2_ISR bit@ ; \ DMA2_ISR_TEIF2, Channel x transfer error flag x = 1  ..7
: DMA2_ISR_HTIF2? ( --  1|0 ) 6 bit DMA2_ISR bit@ ; \ DMA2_ISR_HTIF2, Channel x half transfer flag x = 1  ..7
: DMA2_ISR_TCIF2? ( --  1|0 ) 5 bit DMA2_ISR bit@ ; \ DMA2_ISR_TCIF2, Channel x transfer complete flag x = 1  ..7
: DMA2_ISR_GIF2? ( --  1|0 ) 4 bit DMA2_ISR bit@ ; \ DMA2_ISR_GIF2, Channel x global interrupt flag x = 1  ..7
: DMA2_ISR_TEIF1? ( --  1|0 ) 3 bit DMA2_ISR bit@ ; \ DMA2_ISR_TEIF1, Channel x transfer error flag x = 1  ..7
: DMA2_ISR_HTIF1? ( --  1|0 ) 2 bit DMA2_ISR bit@ ; \ DMA2_ISR_HTIF1, Channel x half transfer flag x = 1  ..7
: DMA2_ISR_TCIF1? ( --  1|0 ) 1 bit DMA2_ISR bit@ ; \ DMA2_ISR_TCIF1, Channel x transfer complete flag x = 1  ..7
: DMA2_ISR_GIF1? ( --  1|0 ) 0 bit DMA2_ISR bit@ ; \ DMA2_ISR_GIF1, Channel x global interrupt flag x = 1  ..7
[then]

defined? use-DMA2 defined? DMA2_IFCR_CTEIF7 not and [if]
\ DMA2_IFCR (write-only) Reset:0x00000000
: DMA2_IFCR_CTEIF7 ( -- x addr ) 27 bit DMA2_IFCR ; \ DMA2_IFCR_CTEIF7, Channel x transfer error clear x = 1  ..7
: DMA2_IFCR_CHTIF7 ( -- x addr ) 26 bit DMA2_IFCR ; \ DMA2_IFCR_CHTIF7, Channel x half transfer clear x = 1  ..7
: DMA2_IFCR_CTCIF7 ( -- x addr ) 25 bit DMA2_IFCR ; \ DMA2_IFCR_CTCIF7, Channel x transfer complete clear x = 1  ..7
: DMA2_IFCR_CGIF7 ( -- x addr ) 24 bit DMA2_IFCR ; \ DMA2_IFCR_CGIF7, Channel x global interrupt clear x = 1  ..7
: DMA2_IFCR_CTEIF6 ( -- x addr ) 23 bit DMA2_IFCR ; \ DMA2_IFCR_CTEIF6, Channel x transfer error clear x = 1  ..7
: DMA2_IFCR_CHTIF6 ( -- x addr ) 22 bit DMA2_IFCR ; \ DMA2_IFCR_CHTIF6, Channel x half transfer clear x = 1  ..7
: DMA2_IFCR_CTCIF6 ( -- x addr ) 21 bit DMA2_IFCR ; \ DMA2_IFCR_CTCIF6, Channel x transfer complete clear x = 1  ..7
: DMA2_IFCR_CGIF6 ( -- x addr ) 20 bit DMA2_IFCR ; \ DMA2_IFCR_CGIF6, Channel x global interrupt clear x = 1  ..7
: DMA2_IFCR_CTEIF5 ( -- x addr ) 19 bit DMA2_IFCR ; \ DMA2_IFCR_CTEIF5, Channel x transfer error clear x = 1  ..7
: DMA2_IFCR_CHTIF5 ( -- x addr ) 18 bit DMA2_IFCR ; \ DMA2_IFCR_CHTIF5, Channel x half transfer clear x = 1  ..7
: DMA2_IFCR_CTCIF5 ( -- x addr ) 17 bit DMA2_IFCR ; \ DMA2_IFCR_CTCIF5, Channel x transfer complete clear x = 1  ..7
: DMA2_IFCR_CGIF5 ( -- x addr ) 16 bit DMA2_IFCR ; \ DMA2_IFCR_CGIF5, Channel x global interrupt clear x = 1  ..7
: DMA2_IFCR_CTEIF4 ( -- x addr ) 15 bit DMA2_IFCR ; \ DMA2_IFCR_CTEIF4, Channel x transfer error clear x = 1  ..7
: DMA2_IFCR_CHTIF4 ( -- x addr ) 14 bit DMA2_IFCR ; \ DMA2_IFCR_CHTIF4, Channel x half transfer clear x = 1  ..7
: DMA2_IFCR_CTCIF4 ( -- x addr ) 13 bit DMA2_IFCR ; \ DMA2_IFCR_CTCIF4, Channel x transfer complete clear x = 1  ..7
: DMA2_IFCR_CGIF4 ( -- x addr ) 12 bit DMA2_IFCR ; \ DMA2_IFCR_CGIF4, Channel x global interrupt clear x = 1  ..7
: DMA2_IFCR_CTEIF3 ( -- x addr ) 11 bit DMA2_IFCR ; \ DMA2_IFCR_CTEIF3, Channel x transfer error clear x = 1  ..7
: DMA2_IFCR_CHTIF3 ( -- x addr ) 10 bit DMA2_IFCR ; \ DMA2_IFCR_CHTIF3, Channel x half transfer clear x = 1  ..7
: DMA2_IFCR_CTCIF3 ( -- x addr ) 9 bit DMA2_IFCR ; \ DMA2_IFCR_CTCIF3, Channel x transfer complete clear x = 1  ..7
: DMA2_IFCR_CGIF3 ( -- x addr ) 8 bit DMA2_IFCR ; \ DMA2_IFCR_CGIF3, Channel x global interrupt clear x = 1  ..7
: DMA2_IFCR_CTEIF2 ( -- x addr ) 7 bit DMA2_IFCR ; \ DMA2_IFCR_CTEIF2, Channel x transfer error clear x = 1  ..7
: DMA2_IFCR_CHTIF2 ( -- x addr ) 6 bit DMA2_IFCR ; \ DMA2_IFCR_CHTIF2, Channel x half transfer clear x = 1  ..7
: DMA2_IFCR_CTCIF2 ( -- x addr ) 5 bit DMA2_IFCR ; \ DMA2_IFCR_CTCIF2, Channel x transfer complete clear x = 1  ..7
: DMA2_IFCR_CGIF2 ( -- x addr ) 4 bit DMA2_IFCR ; \ DMA2_IFCR_CGIF2, Channel x global interrupt clear x = 1  ..7
: DMA2_IFCR_CTEIF1 ( -- x addr ) 3 bit DMA2_IFCR ; \ DMA2_IFCR_CTEIF1, Channel x transfer error clear x = 1  ..7
: DMA2_IFCR_CHTIF1 ( -- x addr ) 2 bit DMA2_IFCR ; \ DMA2_IFCR_CHTIF1, Channel x half transfer clear x = 1  ..7
: DMA2_IFCR_CTCIF1 ( -- x addr ) 1 bit DMA2_IFCR ; \ DMA2_IFCR_CTCIF1, Channel x transfer complete clear x = 1  ..7
: DMA2_IFCR_CGIF1 ( -- x addr ) 0 bit DMA2_IFCR ; \ DMA2_IFCR_CGIF1, Channel x global interrupt clear x = 1  ..7
[then]

execute-defined? use-DMA2 defined? DMA2_CCR1_MEM2MEM not and [if]
\ DMA2_CCR1 (read-write) Reset:0x00000000
: DMA2_CCR1_MEM2MEM ( -- x addr ) 14 bit DMA2_CCR1 ; \ DMA2_CCR1_MEM2MEM, Memory to memory mode
: DMA2_CCR1_PL ( %bb -- x addr ) 12 lshift DMA2_CCR1 ; \ DMA2_CCR1_PL, Channel priority level
: DMA2_CCR1_MSIZE ( %bb -- x addr ) 10 lshift DMA2_CCR1 ; \ DMA2_CCR1_MSIZE, Memory size
: DMA2_CCR1_PSIZE ( %bb -- x addr ) 8 lshift DMA2_CCR1 ; \ DMA2_CCR1_PSIZE, Peripheral size
: DMA2_CCR1_MINC ( -- x addr ) 7 bit DMA2_CCR1 ; \ DMA2_CCR1_MINC, Memory increment mode
: DMA2_CCR1_PINC ( -- x addr ) 6 bit DMA2_CCR1 ; \ DMA2_CCR1_PINC, Peripheral increment mode
: DMA2_CCR1_CIRC ( -- x addr ) 5 bit DMA2_CCR1 ; \ DMA2_CCR1_CIRC, Circular mode
: DMA2_CCR1_DIR ( -- x addr ) 4 bit DMA2_CCR1 ; \ DMA2_CCR1_DIR, Data transfer direction
: DMA2_CCR1_TEIE ( -- x addr ) 3 bit DMA2_CCR1 ; \ DMA2_CCR1_TEIE, Transfer error interrupt  enable
: DMA2_CCR1_HTIE ( -- x addr ) 2 bit DMA2_CCR1 ; \ DMA2_CCR1_HTIE, Half transfer interrupt  enable
: DMA2_CCR1_TCIE ( -- x addr ) 1 bit DMA2_CCR1 ; \ DMA2_CCR1_TCIE, Transfer complete interrupt  enable
: DMA2_CCR1_EN ( -- x addr ) 0 bit DMA2_CCR1 ; \ DMA2_CCR1_EN, Channel enable
[then]

defined? use-DMA2 defined? DMA2_CNDTR1_NDT not and [if]
\ DMA2_CNDTR1 (read-write) Reset:0x00000000
: DMA2_CNDTR1_NDT ( %bbbbbbbbbbbbbbbb -- x addr ) DMA2_CNDTR1 ; \ DMA2_CNDTR1_NDT, Number of data to transfer
[then]

execute-defined? use-DMA2 defined? DMA2_CPAR1_PA not and [if]
\ DMA2_CPAR1 (read-write) Reset:0x00000000
: DMA2_CPAR1_PA ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) DMA2_CPAR1 ; \ DMA2_CPAR1_PA, Peripheral address
[then]

defined? use-DMA2 defined? DMA2_CMAR1_MA not and [if]
\ DMA2_CMAR1 (read-write) Reset:0x00000000
: DMA2_CMAR1_MA ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) DMA2_CMAR1 ; \ DMA2_CMAR1_MA, Memory address
[then]

execute-defined? use-DMA2 defined? DMA2_CCR2_MEM2MEM not and [if]
\ DMA2_CCR2 (read-write) Reset:0x00000000
: DMA2_CCR2_MEM2MEM ( -- x addr ) 14 bit DMA2_CCR2 ; \ DMA2_CCR2_MEM2MEM, Memory to memory mode
: DMA2_CCR2_PL ( %bb -- x addr ) 12 lshift DMA2_CCR2 ; \ DMA2_CCR2_PL, Channel priority level
: DMA2_CCR2_MSIZE ( %bb -- x addr ) 10 lshift DMA2_CCR2 ; \ DMA2_CCR2_MSIZE, Memory size
: DMA2_CCR2_PSIZE ( %bb -- x addr ) 8 lshift DMA2_CCR2 ; \ DMA2_CCR2_PSIZE, Peripheral size
: DMA2_CCR2_MINC ( -- x addr ) 7 bit DMA2_CCR2 ; \ DMA2_CCR2_MINC, Memory increment mode
: DMA2_CCR2_PINC ( -- x addr ) 6 bit DMA2_CCR2 ; \ DMA2_CCR2_PINC, Peripheral increment mode
: DMA2_CCR2_CIRC ( -- x addr ) 5 bit DMA2_CCR2 ; \ DMA2_CCR2_CIRC, Circular mode
: DMA2_CCR2_DIR ( -- x addr ) 4 bit DMA2_CCR2 ; \ DMA2_CCR2_DIR, Data transfer direction
: DMA2_CCR2_TEIE ( -- x addr ) 3 bit DMA2_CCR2 ; \ DMA2_CCR2_TEIE, Transfer error interrupt  enable
: DMA2_CCR2_HTIE ( -- x addr ) 2 bit DMA2_CCR2 ; \ DMA2_CCR2_HTIE, Half transfer interrupt  enable
: DMA2_CCR2_TCIE ( -- x addr ) 1 bit DMA2_CCR2 ; \ DMA2_CCR2_TCIE, Transfer complete interrupt  enable
: DMA2_CCR2_EN ( -- x addr ) 0 bit DMA2_CCR2 ; \ DMA2_CCR2_EN, Channel enable
[then]

defined? use-DMA2 defined? DMA2_CNDTR2_NDT not and [if]
\ DMA2_CNDTR2 (read-write) Reset:0x00000000
: DMA2_CNDTR2_NDT ( %bbbbbbbbbbbbbbbb -- x addr ) DMA2_CNDTR2 ; \ DMA2_CNDTR2_NDT, Number of data to transfer
[then]

execute-defined? use-DMA2 defined? DMA2_CPAR2_PA not and [if]
\ DMA2_CPAR2 (read-write) Reset:0x00000000
: DMA2_CPAR2_PA ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) DMA2_CPAR2 ; \ DMA2_CPAR2_PA, Peripheral address
[then]

defined? use-DMA2 defined? DMA2_CMAR2_MA not and [if]
\ DMA2_CMAR2 (read-write) Reset:0x00000000
: DMA2_CMAR2_MA ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) DMA2_CMAR2 ; \ DMA2_CMAR2_MA, Memory address
[then]

execute-defined? use-DMA2 defined? DMA2_CCR3_MEM2MEM not and [if]
\ DMA2_CCR3 (read-write) Reset:0x00000000
: DMA2_CCR3_MEM2MEM ( -- x addr ) 14 bit DMA2_CCR3 ; \ DMA2_CCR3_MEM2MEM, Memory to memory mode
: DMA2_CCR3_PL ( %bb -- x addr ) 12 lshift DMA2_CCR3 ; \ DMA2_CCR3_PL, Channel priority level
: DMA2_CCR3_MSIZE ( %bb -- x addr ) 10 lshift DMA2_CCR3 ; \ DMA2_CCR3_MSIZE, Memory size
: DMA2_CCR3_PSIZE ( %bb -- x addr ) 8 lshift DMA2_CCR3 ; \ DMA2_CCR3_PSIZE, Peripheral size
: DMA2_CCR3_MINC ( -- x addr ) 7 bit DMA2_CCR3 ; \ DMA2_CCR3_MINC, Memory increment mode
: DMA2_CCR3_PINC ( -- x addr ) 6 bit DMA2_CCR3 ; \ DMA2_CCR3_PINC, Peripheral increment mode
: DMA2_CCR3_CIRC ( -- x addr ) 5 bit DMA2_CCR3 ; \ DMA2_CCR3_CIRC, Circular mode
: DMA2_CCR3_DIR ( -- x addr ) 4 bit DMA2_CCR3 ; \ DMA2_CCR3_DIR, Data transfer direction
: DMA2_CCR3_TEIE ( -- x addr ) 3 bit DMA2_CCR3 ; \ DMA2_CCR3_TEIE, Transfer error interrupt  enable
: DMA2_CCR3_HTIE ( -- x addr ) 2 bit DMA2_CCR3 ; \ DMA2_CCR3_HTIE, Half transfer interrupt  enable
: DMA2_CCR3_TCIE ( -- x addr ) 1 bit DMA2_CCR3 ; \ DMA2_CCR3_TCIE, Transfer complete interrupt  enable
: DMA2_CCR3_EN ( -- x addr ) 0 bit DMA2_CCR3 ; \ DMA2_CCR3_EN, Channel enable
[then]

defined? use-DMA2 defined? DMA2_CNDTR3_NDT not and [if]
\ DMA2_CNDTR3 (read-write) Reset:0x00000000
: DMA2_CNDTR3_NDT ( %bbbbbbbbbbbbbbbb -- x addr ) DMA2_CNDTR3 ; \ DMA2_CNDTR3_NDT, Number of data to transfer
[then]

execute-defined? use-DMA2 defined? DMA2_CPAR3_PA not and [if]
\ DMA2_CPAR3 (read-write) Reset:0x00000000
: DMA2_CPAR3_PA ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) DMA2_CPAR3 ; \ DMA2_CPAR3_PA, Peripheral address
[then]

defined? use-DMA2 defined? DMA2_CMAR3_MA not and [if]
\ DMA2_CMAR3 (read-write) Reset:0x00000000
: DMA2_CMAR3_MA ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) DMA2_CMAR3 ; \ DMA2_CMAR3_MA, Memory address
[then]

execute-defined? use-DMA2 defined? DMA2_CCR4_MEM2MEM not and [if]
\ DMA2_CCR4 (read-write) Reset:0x00000000
: DMA2_CCR4_MEM2MEM ( -- x addr ) 14 bit DMA2_CCR4 ; \ DMA2_CCR4_MEM2MEM, Memory to memory mode
: DMA2_CCR4_PL ( %bb -- x addr ) 12 lshift DMA2_CCR4 ; \ DMA2_CCR4_PL, Channel priority level
: DMA2_CCR4_MSIZE ( %bb -- x addr ) 10 lshift DMA2_CCR4 ; \ DMA2_CCR4_MSIZE, Memory size
: DMA2_CCR4_PSIZE ( %bb -- x addr ) 8 lshift DMA2_CCR4 ; \ DMA2_CCR4_PSIZE, Peripheral size
: DMA2_CCR4_MINC ( -- x addr ) 7 bit DMA2_CCR4 ; \ DMA2_CCR4_MINC, Memory increment mode
: DMA2_CCR4_PINC ( -- x addr ) 6 bit DMA2_CCR4 ; \ DMA2_CCR4_PINC, Peripheral increment mode
: DMA2_CCR4_CIRC ( -- x addr ) 5 bit DMA2_CCR4 ; \ DMA2_CCR4_CIRC, Circular mode
: DMA2_CCR4_DIR ( -- x addr ) 4 bit DMA2_CCR4 ; \ DMA2_CCR4_DIR, Data transfer direction
: DMA2_CCR4_TEIE ( -- x addr ) 3 bit DMA2_CCR4 ; \ DMA2_CCR4_TEIE, Transfer error interrupt  enable
: DMA2_CCR4_HTIE ( -- x addr ) 2 bit DMA2_CCR4 ; \ DMA2_CCR4_HTIE, Half transfer interrupt  enable
: DMA2_CCR4_TCIE ( -- x addr ) 1 bit DMA2_CCR4 ; \ DMA2_CCR4_TCIE, Transfer complete interrupt  enable
: DMA2_CCR4_EN ( -- x addr ) 0 bit DMA2_CCR4 ; \ DMA2_CCR4_EN, Channel enable
[then]

defined? use-DMA2 defined? DMA2_CNDTR4_NDT not and [if]
\ DMA2_CNDTR4 (read-write) Reset:0x00000000
: DMA2_CNDTR4_NDT ( %bbbbbbbbbbbbbbbb -- x addr ) DMA2_CNDTR4 ; \ DMA2_CNDTR4_NDT, Number of data to transfer
[then]

execute-defined? use-DMA2 defined? DMA2_CPAR4_PA not and [if]
\ DMA2_CPAR4 (read-write) Reset:0x00000000
: DMA2_CPAR4_PA ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) DMA2_CPAR4 ; \ DMA2_CPAR4_PA, Peripheral address
[then]

defined? use-DMA2 defined? DMA2_CMAR4_MA not and [if]
\ DMA2_CMAR4 (read-write) Reset:0x00000000
: DMA2_CMAR4_MA ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) DMA2_CMAR4 ; \ DMA2_CMAR4_MA, Memory address
[then]

execute-defined? use-DMA2 defined? DMA2_CCR5_MEM2MEM not and [if]
\ DMA2_CCR5 (read-write) Reset:0x00000000
: DMA2_CCR5_MEM2MEM ( -- x addr ) 14 bit DMA2_CCR5 ; \ DMA2_CCR5_MEM2MEM, Memory to memory mode
: DMA2_CCR5_PL ( %bb -- x addr ) 12 lshift DMA2_CCR5 ; \ DMA2_CCR5_PL, Channel priority level
: DMA2_CCR5_MSIZE ( %bb -- x addr ) 10 lshift DMA2_CCR5 ; \ DMA2_CCR5_MSIZE, Memory size
: DMA2_CCR5_PSIZE ( %bb -- x addr ) 8 lshift DMA2_CCR5 ; \ DMA2_CCR5_PSIZE, Peripheral size
: DMA2_CCR5_MINC ( -- x addr ) 7 bit DMA2_CCR5 ; \ DMA2_CCR5_MINC, Memory increment mode
: DMA2_CCR5_PINC ( -- x addr ) 6 bit DMA2_CCR5 ; \ DMA2_CCR5_PINC, Peripheral increment mode
: DMA2_CCR5_CIRC ( -- x addr ) 5 bit DMA2_CCR5 ; \ DMA2_CCR5_CIRC, Circular mode
: DMA2_CCR5_DIR ( -- x addr ) 4 bit DMA2_CCR5 ; \ DMA2_CCR5_DIR, Data transfer direction
: DMA2_CCR5_TEIE ( -- x addr ) 3 bit DMA2_CCR5 ; \ DMA2_CCR5_TEIE, Transfer error interrupt  enable
: DMA2_CCR5_HTIE ( -- x addr ) 2 bit DMA2_CCR5 ; \ DMA2_CCR5_HTIE, Half transfer interrupt  enable
: DMA2_CCR5_TCIE ( -- x addr ) 1 bit DMA2_CCR5 ; \ DMA2_CCR5_TCIE, Transfer complete interrupt  enable
: DMA2_CCR5_EN ( -- x addr ) 0 bit DMA2_CCR5 ; \ DMA2_CCR5_EN, Channel enable
[then]

defined? use-DMA2 defined? DMA2_CNDTR5_NDT not and [if]
\ DMA2_CNDTR5 (read-write) Reset:0x00000000
: DMA2_CNDTR5_NDT ( %bbbbbbbbbbbbbbbb -- x addr ) DMA2_CNDTR5 ; \ DMA2_CNDTR5_NDT, Number of data to transfer
[then]

execute-defined? use-DMA2 defined? DMA2_CPAR5_PA not and [if]
\ DMA2_CPAR5 (read-write) Reset:0x00000000
: DMA2_CPAR5_PA ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) DMA2_CPAR5 ; \ DMA2_CPAR5_PA, Peripheral address
[then]

defined? use-DMA2 defined? DMA2_CMAR5_MA not and [if]
\ DMA2_CMAR5 (read-write) Reset:0x00000000
: DMA2_CMAR5_MA ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) DMA2_CMAR5 ; \ DMA2_CMAR5_MA, Memory address
[then]

execute-defined? use-DMA2 defined? DMA2_CCR6_MEM2MEM not and [if]
\ DMA2_CCR6 (read-write) Reset:0x00000000
: DMA2_CCR6_MEM2MEM ( -- x addr ) 14 bit DMA2_CCR6 ; \ DMA2_CCR6_MEM2MEM, Memory to memory mode
: DMA2_CCR6_PL ( %bb -- x addr ) 12 lshift DMA2_CCR6 ; \ DMA2_CCR6_PL, Channel priority level
: DMA2_CCR6_MSIZE ( %bb -- x addr ) 10 lshift DMA2_CCR6 ; \ DMA2_CCR6_MSIZE, Memory size
: DMA2_CCR6_PSIZE ( %bb -- x addr ) 8 lshift DMA2_CCR6 ; \ DMA2_CCR6_PSIZE, Peripheral size
: DMA2_CCR6_MINC ( -- x addr ) 7 bit DMA2_CCR6 ; \ DMA2_CCR6_MINC, Memory increment mode
: DMA2_CCR6_PINC ( -- x addr ) 6 bit DMA2_CCR6 ; \ DMA2_CCR6_PINC, Peripheral increment mode
: DMA2_CCR6_CIRC ( -- x addr ) 5 bit DMA2_CCR6 ; \ DMA2_CCR6_CIRC, Circular mode
: DMA2_CCR6_DIR ( -- x addr ) 4 bit DMA2_CCR6 ; \ DMA2_CCR6_DIR, Data transfer direction
: DMA2_CCR6_TEIE ( -- x addr ) 3 bit DMA2_CCR6 ; \ DMA2_CCR6_TEIE, Transfer error interrupt  enable
: DMA2_CCR6_HTIE ( -- x addr ) 2 bit DMA2_CCR6 ; \ DMA2_CCR6_HTIE, Half transfer interrupt  enable
: DMA2_CCR6_TCIE ( -- x addr ) 1 bit DMA2_CCR6 ; \ DMA2_CCR6_TCIE, Transfer complete interrupt  enable
: DMA2_CCR6_EN ( -- x addr ) 0 bit DMA2_CCR6 ; \ DMA2_CCR6_EN, Channel enable
[then]

defined? use-DMA2 defined? DMA2_CNDTR6_NDT not and [if]
\ DMA2_CNDTR6 (read-write) Reset:0x00000000
: DMA2_CNDTR6_NDT ( %bbbbbbbbbbbbbbbb -- x addr ) DMA2_CNDTR6 ; \ DMA2_CNDTR6_NDT, Number of data to transfer
[then]

execute-defined? use-DMA2 defined? DMA2_CPAR6_PA not and [if]
\ DMA2_CPAR6 (read-write) Reset:0x00000000
: DMA2_CPAR6_PA ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) DMA2_CPAR6 ; \ DMA2_CPAR6_PA, Peripheral address
[then]

defined? use-DMA2 defined? DMA2_CMAR6_MA not and [if]
\ DMA2_CMAR6 (read-write) Reset:0x00000000
: DMA2_CMAR6_MA ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) DMA2_CMAR6 ; \ DMA2_CMAR6_MA, Memory address
[then]

execute-defined? use-DMA2 defined? DMA2_CCR7_MEM2MEM not and [if]
\ DMA2_CCR7 (read-write) Reset:0x00000000
: DMA2_CCR7_MEM2MEM ( -- x addr ) 14 bit DMA2_CCR7 ; \ DMA2_CCR7_MEM2MEM, Memory to memory mode
: DMA2_CCR7_PL ( %bb -- x addr ) 12 lshift DMA2_CCR7 ; \ DMA2_CCR7_PL, Channel priority level
: DMA2_CCR7_MSIZE ( %bb -- x addr ) 10 lshift DMA2_CCR7 ; \ DMA2_CCR7_MSIZE, Memory size
: DMA2_CCR7_PSIZE ( %bb -- x addr ) 8 lshift DMA2_CCR7 ; \ DMA2_CCR7_PSIZE, Peripheral size
: DMA2_CCR7_MINC ( -- x addr ) 7 bit DMA2_CCR7 ; \ DMA2_CCR7_MINC, Memory increment mode
: DMA2_CCR7_PINC ( -- x addr ) 6 bit DMA2_CCR7 ; \ DMA2_CCR7_PINC, Peripheral increment mode
: DMA2_CCR7_CIRC ( -- x addr ) 5 bit DMA2_CCR7 ; \ DMA2_CCR7_CIRC, Circular mode
: DMA2_CCR7_DIR ( -- x addr ) 4 bit DMA2_CCR7 ; \ DMA2_CCR7_DIR, Data transfer direction
: DMA2_CCR7_TEIE ( -- x addr ) 3 bit DMA2_CCR7 ; \ DMA2_CCR7_TEIE, Transfer error interrupt  enable
: DMA2_CCR7_HTIE ( -- x addr ) 2 bit DMA2_CCR7 ; \ DMA2_CCR7_HTIE, Half transfer interrupt  enable
: DMA2_CCR7_TCIE ( -- x addr ) 1 bit DMA2_CCR7 ; \ DMA2_CCR7_TCIE, Transfer complete interrupt  enable
: DMA2_CCR7_EN ( -- x addr ) 0 bit DMA2_CCR7 ; \ DMA2_CCR7_EN, Channel enable
[then]

defined? use-DMA2 defined? DMA2_CNDTR7_NDT not and [if]
\ DMA2_CNDTR7 (read-write) Reset:0x00000000
: DMA2_CNDTR7_NDT ( %bbbbbbbbbbbbbbbb -- x addr ) DMA2_CNDTR7 ; \ DMA2_CNDTR7_NDT, Number of data to transfer
[then]

execute-defined? use-DMA2 defined? DMA2_CPAR7_PA not and [if]
\ DMA2_CPAR7 (read-write) Reset:0x00000000
: DMA2_CPAR7_PA ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) DMA2_CPAR7 ; \ DMA2_CPAR7_PA, Peripheral address
[then]

defined? use-DMA2 defined? DMA2_CMAR7_MA not and [if]
\ DMA2_CMAR7 (read-write) Reset:0x00000000
: DMA2_CMAR7_MA ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) DMA2_CMAR7 ; \ DMA2_CMAR7_MA, Memory address
[then]

execute-defined? use-DMA2 defined? DMA2_CSELR_C7S not and [if]
\ DMA2_CSELR (read-write) Reset:0x00000000
: DMA2_CSELR_C7S ( %bbbb -- x addr ) 24 lshift DMA2_CSELR ; \ DMA2_CSELR_C7S, DMA channel 7 selection
: DMA2_CSELR_C6S ( %bbbb -- x addr ) 20 lshift DMA2_CSELR ; \ DMA2_CSELR_C6S, DMA channel 6 selection
: DMA2_CSELR_C5S ( %bbbb -- x addr ) 16 lshift DMA2_CSELR ; \ DMA2_CSELR_C5S, DMA channel 5 selection
: DMA2_CSELR_C4S ( %bbbb -- x addr ) 12 lshift DMA2_CSELR ; \ DMA2_CSELR_C4S, DMA channel 4 selection
: DMA2_CSELR_C3S ( %bbbb -- x addr ) 8 lshift DMA2_CSELR ; \ DMA2_CSELR_C3S, DMA channel 3 selection
: DMA2_CSELR_C2S ( %bbbb -- x addr ) 4 lshift DMA2_CSELR ; \ DMA2_CSELR_C2S, DMA channel 2 selection
: DMA2_CSELR_C1S ( %bbbb -- x addr ) DMA2_CSELR ; \ DMA2_CSELR_C1S, DMA channel 1 selection
[then]

defined? use-CRC defined? CRC_DR_DR not and [if]
\ CRC_DR (read-write) Reset:0xFFFFFFFF
: CRC_DR_DR ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) CRC_DR ; \ CRC_DR_DR, Data register bits
[then]

execute-defined? use-CRC defined? CRC_IDR_IDR not and [if]
\ CRC_IDR (read-write) Reset:0x00000000
: CRC_IDR_IDR ( %bbbbbbbb -- x addr ) CRC_IDR ; \ CRC_IDR_IDR, General-purpose 8-bit data register  bits
[then]

defined? use-CRC defined? CRC_CR_REV_OUT not and [if]
\ CRC_CR (multiple-access)  Reset:0x00000000
: CRC_CR_REV_OUT ( -- x addr ) 7 bit CRC_CR ; \ CRC_CR_REV_OUT, Reverse output data
: CRC_CR_REV_IN ( %bb -- x addr ) 5 lshift CRC_CR ; \ CRC_CR_REV_IN, Reverse input data
: CRC_CR_POLYSIZE ( %bb -- x addr ) 3 lshift CRC_CR ; \ CRC_CR_POLYSIZE, Polynomial size
: CRC_CR_RESET ( -- x addr ) 0 bit CRC_CR ; \ CRC_CR_RESET, RESET bit
[then]

execute-defined? use-CRC defined? CRC_INIT_CRC_INIT not and [if]
\ CRC_INIT (read-write) Reset:0xFFFFFFFF
: CRC_INIT_CRC_INIT ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) CRC_INIT ; \ CRC_INIT_CRC_INIT, Programmable initial CRC  value
[then]

defined? use-CRC defined? CRC_POL_Polynomialcoefficients not and [if]
\ CRC_POL (read-write) Reset:0x04C11DB7
: CRC_POL_Polynomialcoefficients ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) CRC_POL ; \ CRC_POL_Polynomialcoefficients, Programmable polynomial
[then]

execute-defined? use-IWDG defined? IWDG_KR_KEY not and [if]
\ IWDG_KR (write-only) Reset:0x00000000
: IWDG_KR_KEY ( %bbbbbbbbbbbbbbbb -- x addr ) IWDG_KR ; \ IWDG_KR_KEY, Key value write only, read  $0000
[then]

defined? use-IWDG defined? IWDG_PR_PR not and [if]
\ IWDG_PR (read-write) Reset:0x00000000
: IWDG_PR_PR ( %bbb -- x addr ) IWDG_PR ; \ IWDG_PR_PR, Prescaler divider
[then]

execute-defined? use-IWDG defined? IWDG_RLR_RL not and [if]
\ IWDG_RLR (read-write) Reset:0x00000FFF
: IWDG_RLR_RL ( %bbbbbbbbbbb -- x addr ) IWDG_RLR ; \ IWDG_RLR_RL, Watchdog counter reload  value
[then]

defined? use-IWDG defined? IWDG_SR_WVU? not and [if]
\ IWDG_SR (read-only) Reset:0x00000000
: IWDG_SR_WVU? ( --  1|0 ) 2 bit IWDG_SR bit@ ; \ IWDG_SR_WVU, Watchdog counter window value  update
: IWDG_SR_RVU? ( --  1|0 ) 1 bit IWDG_SR bit@ ; \ IWDG_SR_RVU, Watchdog counter reload value  update
: IWDG_SR_PVU? ( --  1|0 ) 0 bit IWDG_SR bit@ ; \ IWDG_SR_PVU, Watchdog prescaler value  update
[then]

execute-defined? use-IWDG defined? IWDG_WINR_WIN not and [if]
\ IWDG_WINR (read-write) Reset:0x00000FFF
: IWDG_WINR_WIN ( %bbbbbbbbbbb -- x addr ) IWDG_WINR ; \ IWDG_WINR_WIN, Watchdog counter window  value
[then]

defined? use-WWDG defined? WWDG_CR_WDGA not and [if]
\ WWDG_CR (read-write) Reset:0x0000007F
: WWDG_CR_WDGA ( -- x addr ) 7 bit WWDG_CR ; \ WWDG_CR_WDGA, Activation bit
: WWDG_CR_T ( %bbbbbbb -- x addr ) WWDG_CR ; \ WWDG_CR_T, 7-bit counter MSB to LSB
[then]

execute-defined? use-WWDG defined? WWDG_CFR_EWI not and [if]
\ WWDG_CFR (read-write) Reset:0x0000007F
: WWDG_CFR_EWI ( -- x addr ) 9 bit WWDG_CFR ; \ WWDG_CFR_EWI, Early wakeup interrupt
: WWDG_CFR_WDGTB ( %bb -- x addr ) 7 lshift WWDG_CFR ; \ WWDG_CFR_WDGTB, Timer base
: WWDG_CFR_W ( %bbbbbbb -- x addr ) WWDG_CFR ; \ WWDG_CFR_W, 7-bit window value
[then]

defined? use-WWDG defined? WWDG_SR_EWIF not and [if]
\ WWDG_SR (read-write) Reset:0x00000000
: WWDG_SR_EWIF ( -- x addr ) 0 bit WWDG_SR ; \ WWDG_SR_EWIF, Early wakeup interrupt  flag
[then]

execute-defined? use-I2C1 defined? I2C1_CR1_PE not and [if]
\ I2C1_CR1 (read-write) Reset:0x00000000
: I2C1_CR1_PE ( -- x addr ) 0 bit I2C1_CR1 ; \ I2C1_CR1_PE, Peripheral enable
: I2C1_CR1_TXIE ( -- x addr ) 1 bit I2C1_CR1 ; \ I2C1_CR1_TXIE, TX Interrupt enable
: I2C1_CR1_RXIE ( -- x addr ) 2 bit I2C1_CR1 ; \ I2C1_CR1_RXIE, RX Interrupt enable
: I2C1_CR1_ADDRIE ( -- x addr ) 3 bit I2C1_CR1 ; \ I2C1_CR1_ADDRIE, Address match interrupt enable slave  only
: I2C1_CR1_NACKIE ( -- x addr ) 4 bit I2C1_CR1 ; \ I2C1_CR1_NACKIE, Not acknowledge received interrupt  enable
: I2C1_CR1_STOPIE ( -- x addr ) 5 bit I2C1_CR1 ; \ I2C1_CR1_STOPIE, STOP detection Interrupt  enable
: I2C1_CR1_TCIE ( -- x addr ) 6 bit I2C1_CR1 ; \ I2C1_CR1_TCIE, Transfer Complete interrupt  enable
: I2C1_CR1_ERRIE ( -- x addr ) 7 bit I2C1_CR1 ; \ I2C1_CR1_ERRIE, Error interrupts enable
: I2C1_CR1_DNF ( %bbbb -- x addr ) 8 lshift I2C1_CR1 ; \ I2C1_CR1_DNF, Digital noise filter
: I2C1_CR1_ANFOFF ( -- x addr ) 12 bit I2C1_CR1 ; \ I2C1_CR1_ANFOFF, Analog noise filter OFF
: I2C1_CR1_TXDMAEN ( -- x addr ) 14 bit I2C1_CR1 ; \ I2C1_CR1_TXDMAEN, DMA transmission requests  enable
: I2C1_CR1_RXDMAEN ( -- x addr ) 15 bit I2C1_CR1 ; \ I2C1_CR1_RXDMAEN, DMA reception requests  enable
: I2C1_CR1_SBC ( -- x addr ) 16 bit I2C1_CR1 ; \ I2C1_CR1_SBC, Slave byte control
: I2C1_CR1_NOSTRETCH ( -- x addr ) 17 bit I2C1_CR1 ; \ I2C1_CR1_NOSTRETCH, Clock stretching disable
: I2C1_CR1_WUPEN ( -- x addr ) 18 bit I2C1_CR1 ; \ I2C1_CR1_WUPEN, Wakeup from STOP enable
: I2C1_CR1_GCEN ( -- x addr ) 19 bit I2C1_CR1 ; \ I2C1_CR1_GCEN, General call enable
: I2C1_CR1_SMBHEN ( -- x addr ) 20 bit I2C1_CR1 ; \ I2C1_CR1_SMBHEN, SMBus Host address enable
: I2C1_CR1_SMBDEN ( -- x addr ) 21 bit I2C1_CR1 ; \ I2C1_CR1_SMBDEN, SMBus Device Default address  enable
: I2C1_CR1_ALERTEN ( -- x addr ) 22 bit I2C1_CR1 ; \ I2C1_CR1_ALERTEN, SMBUS alert enable
: I2C1_CR1_PECEN ( -- x addr ) 23 bit I2C1_CR1 ; \ I2C1_CR1_PECEN, PEC enable
[then]

defined? use-I2C1 defined? I2C1_CR2_PECBYTE not and [if]
\ I2C1_CR2 (read-write) Reset:0x00000000
: I2C1_CR2_PECBYTE ( -- x addr ) 26 bit I2C1_CR2 ; \ I2C1_CR2_PECBYTE, Packet error checking byte
: I2C1_CR2_AUTOEND ( -- x addr ) 25 bit I2C1_CR2 ; \ I2C1_CR2_AUTOEND, Automatic end mode master  mode
: I2C1_CR2_RELOAD ( -- x addr ) 24 bit I2C1_CR2 ; \ I2C1_CR2_RELOAD, NBYTES reload mode
: I2C1_CR2_NBYTES ( %bbbbbbbb -- x addr ) 16 lshift I2C1_CR2 ; \ I2C1_CR2_NBYTES, Number of bytes
: I2C1_CR2_NACK ( -- x addr ) 15 bit I2C1_CR2 ; \ I2C1_CR2_NACK, NACK generation slave  mode
: I2C1_CR2_STOP ( -- x addr ) 14 bit I2C1_CR2 ; \ I2C1_CR2_STOP, Stop generation master  mode
: I2C1_CR2_START ( -- x addr ) 13 bit I2C1_CR2 ; \ I2C1_CR2_START, Start generation
: I2C1_CR2_HEAD10R ( -- x addr ) 12 bit I2C1_CR2 ; \ I2C1_CR2_HEAD10R, 10-bit address header only read  direction master receiver mode
: I2C1_CR2_ADD10 ( -- x addr ) 11 bit I2C1_CR2 ; \ I2C1_CR2_ADD10, 10-bit addressing mode master  mode
: I2C1_CR2_RD_WRN ( -- x addr ) 10 bit I2C1_CR2 ; \ I2C1_CR2_RD_WRN, Transfer direction master  mode
: I2C1_CR2_SADD ( %bbbbbbbbbb -- x addr ) I2C1_CR2 ; \ I2C1_CR2_SADD, Slave address bit master  mode
[then]

execute-defined? use-I2C1 defined? I2C1_OAR1_OA1 not and [if]
\ I2C1_OAR1 (read-write) Reset:0x00000000
: I2C1_OAR1_OA1 ( %bbbbbbbbbb -- x addr ) I2C1_OAR1 ; \ I2C1_OAR1_OA1, Interface address
: I2C1_OAR1_OA1MODE ( -- x addr ) 10 bit I2C1_OAR1 ; \ I2C1_OAR1_OA1MODE, Own Address 1 10-bit mode
: I2C1_OAR1_OA1EN ( -- x addr ) 15 bit I2C1_OAR1 ; \ I2C1_OAR1_OA1EN, Own Address 1 enable
[then]

defined? use-I2C1 defined? I2C1_OAR2_OA2 not and [if]
\ I2C1_OAR2 (read-write) Reset:0x00000000
: I2C1_OAR2_OA2 ( %bbbbbbb -- x addr ) 1 lshift I2C1_OAR2 ; \ I2C1_OAR2_OA2, Interface address
: I2C1_OAR2_OA2MSK ( %bbb -- x addr ) 8 lshift I2C1_OAR2 ; \ I2C1_OAR2_OA2MSK, Own Address 2 masks
: I2C1_OAR2_OA2EN ( -- x addr ) 15 bit I2C1_OAR2 ; \ I2C1_OAR2_OA2EN, Own Address 2 enable
[then]

execute-defined? use-I2C1 defined? I2C1_TIMINGR_SCLL not and [if]
\ I2C1_TIMINGR (read-write) Reset:0x00000000
: I2C1_TIMINGR_SCLL ( %bbbbbbbb -- x addr ) I2C1_TIMINGR ; \ I2C1_TIMINGR_SCLL, SCL low period master  mode
: I2C1_TIMINGR_SCLH ( %bbbbbbbb -- x addr ) 8 lshift I2C1_TIMINGR ; \ I2C1_TIMINGR_SCLH, SCL high period master  mode
: I2C1_TIMINGR_SDADEL ( %bbbb -- x addr ) 16 lshift I2C1_TIMINGR ; \ I2C1_TIMINGR_SDADEL, Data hold time
: I2C1_TIMINGR_SCLDEL ( %bbbb -- x addr ) 20 lshift I2C1_TIMINGR ; \ I2C1_TIMINGR_SCLDEL, Data setup time
: I2C1_TIMINGR_PRESC ( %bbbb -- x addr ) 28 lshift I2C1_TIMINGR ; \ I2C1_TIMINGR_PRESC, Timing prescaler
[then]

defined? use-I2C1 defined? I2C1_TIMEOUTR_TIMEOUTA not and [if]
\ I2C1_TIMEOUTR (read-write) Reset:0x00000000
: I2C1_TIMEOUTR_TIMEOUTA ( %bbbbbbbbbbb -- x addr ) I2C1_TIMEOUTR ; \ I2C1_TIMEOUTR_TIMEOUTA, Bus timeout A
: I2C1_TIMEOUTR_TIDLE ( -- x addr ) 12 bit I2C1_TIMEOUTR ; \ I2C1_TIMEOUTR_TIDLE, Idle clock timeout  detection
: I2C1_TIMEOUTR_TIMOUTEN ( -- x addr ) 15 bit I2C1_TIMEOUTR ; \ I2C1_TIMEOUTR_TIMOUTEN, Clock timeout enable
: I2C1_TIMEOUTR_TIMEOUTB ( %bbbbbbbbbbb -- x addr ) 16 lshift I2C1_TIMEOUTR ; \ I2C1_TIMEOUTR_TIMEOUTB, Bus timeout B
: I2C1_TIMEOUTR_TEXTEN ( -- x addr ) 31 bit I2C1_TIMEOUTR ; \ I2C1_TIMEOUTR_TEXTEN, Extended clock timeout  enable
[then]

execute-defined? use-I2C1 defined? I2C1_ISR_ADDCODE not and [if]
\ I2C1_ISR (multiple-access)  Reset:0x00000001
: I2C1_ISR_ADDCODE ( %bbbbbbb -- x addr ) 17 lshift I2C1_ISR ; \ I2C1_ISR_ADDCODE, Address match code Slave  mode
: I2C1_ISR_DIR ( -- x addr ) 16 bit I2C1_ISR ; \ I2C1_ISR_DIR, Transfer direction Slave  mode
: I2C1_ISR_BUSY ( -- x addr ) 15 bit I2C1_ISR ; \ I2C1_ISR_BUSY, Bus busy
: I2C1_ISR_ALERT ( -- x addr ) 13 bit I2C1_ISR ; \ I2C1_ISR_ALERT, SMBus alert
: I2C1_ISR_TIMEOUT? ( -- 1|0 ) 12 bit I2C1_ISR bit@ ; \ I2C1_ISR_TIMEOUT, Timeout or t_low detection  flag
: I2C1_ISR_PECERR ( -- x addr ) 11 bit I2C1_ISR ; \ I2C1_ISR_PECERR, PEC Error in reception
: I2C1_ISR_OVR ( -- x addr ) 10 bit I2C1_ISR ; \ I2C1_ISR_OVR, Overrun/Underrun slave  mode
: I2C1_ISR_ARLO ( -- x addr ) 9 bit I2C1_ISR ; \ I2C1_ISR_ARLO, Arbitration lost
: I2C1_ISR_BERR ( -- x addr ) 8 bit I2C1_ISR ; \ I2C1_ISR_BERR, Bus error
: I2C1_ISR_TCR ( -- x addr ) 7 bit I2C1_ISR ; \ I2C1_ISR_TCR, Transfer Complete Reload
: I2C1_ISR_TC ( -- x addr ) 6 bit I2C1_ISR ; \ I2C1_ISR_TC, Transfer Complete master  mode
: I2C1_ISR_STOPF? ( -- 1|0 ) 5 bit I2C1_ISR bit@ ; \ I2C1_ISR_STOPF, Stop detection flag
: I2C1_ISR_NACKF? ( -- 1|0 ) 4 bit I2C1_ISR bit@ ; \ I2C1_ISR_NACKF, Not acknowledge received  flag
: I2C1_ISR_ADDR ( -- x addr ) 3 bit I2C1_ISR ; \ I2C1_ISR_ADDR, Address matched slave  mode
: I2C1_ISR_RXNE ( -- x addr ) 2 bit I2C1_ISR ; \ I2C1_ISR_RXNE, Receive data register not empty  receivers
: I2C1_ISR_TXIS? ( -- 1|0 ) 1 bit I2C1_ISR bit@ ; \ I2C1_ISR_TXIS, Transmit interrupt status  transmitters
: I2C1_ISR_TXE ( -- x addr ) 0 bit I2C1_ISR ; \ I2C1_ISR_TXE, Transmit data register empty  transmitters
[then]

defined? use-I2C1 defined? I2C1_ICR_ALERTCF not and [if]
\ I2C1_ICR (write-only) Reset:0x00000000
: I2C1_ICR_ALERTCF ( -- x addr ) 13 bit I2C1_ICR ; \ I2C1_ICR_ALERTCF, Alert flag clear
: I2C1_ICR_TIMOUTCF ( -- x addr ) 12 bit I2C1_ICR ; \ I2C1_ICR_TIMOUTCF, Timeout detection flag  clear
: I2C1_ICR_PECCF ( -- x addr ) 11 bit I2C1_ICR ; \ I2C1_ICR_PECCF, PEC Error flag clear
: I2C1_ICR_OVRCF ( -- x addr ) 10 bit I2C1_ICR ; \ I2C1_ICR_OVRCF, Overrun/Underrun flag  clear
: I2C1_ICR_ARLOCF ( -- x addr ) 9 bit I2C1_ICR ; \ I2C1_ICR_ARLOCF, Arbitration lost flag  clear
: I2C1_ICR_BERRCF ( -- x addr ) 8 bit I2C1_ICR ; \ I2C1_ICR_BERRCF, Bus error flag clear
: I2C1_ICR_STOPCF ( -- x addr ) 5 bit I2C1_ICR ; \ I2C1_ICR_STOPCF, Stop detection flag clear
: I2C1_ICR_NACKCF ( -- x addr ) 4 bit I2C1_ICR ; \ I2C1_ICR_NACKCF, Not Acknowledge flag clear
: I2C1_ICR_ADDRCF ( -- x addr ) 3 bit I2C1_ICR ; \ I2C1_ICR_ADDRCF, Address Matched flag clear
[then]

execute-defined? use-I2C1 defined? I2C1_PECR_PEC? not and [if]
\ I2C1_PECR (read-only) Reset:0x00000000
: I2C1_PECR_PEC? ( --  x ) I2C1_PECR @ ; \ I2C1_PECR_PEC, Packet error checking  register
[then]

defined? use-I2C1 defined? I2C1_RXDR_RXDATA? not and [if]
\ I2C1_RXDR (read-only) Reset:0x00000000
: I2C1_RXDR_RXDATA? ( --  x ) I2C1_RXDR @ ; \ I2C1_RXDR_RXDATA, 8-bit receive data
[then]

execute-defined? use-I2C1 defined? I2C1_TXDR_TXDATA not and [if]
\ I2C1_TXDR (read-write) Reset:0x00000000
: I2C1_TXDR_TXDATA ( %bbbbbbbb -- x addr ) I2C1_TXDR ; \ I2C1_TXDR_TXDATA, 8-bit transmit data
[then]

defined? use-I2C2 defined? I2C2_CR1_PE not and [if]
\ I2C2_CR1 (read-write) Reset:0x00000000
: I2C2_CR1_PE ( -- x addr ) 0 bit I2C2_CR1 ; \ I2C2_CR1_PE, Peripheral enable
: I2C2_CR1_TXIE ( -- x addr ) 1 bit I2C2_CR1 ; \ I2C2_CR1_TXIE, TX Interrupt enable
: I2C2_CR1_RXIE ( -- x addr ) 2 bit I2C2_CR1 ; \ I2C2_CR1_RXIE, RX Interrupt enable
: I2C2_CR1_ADDRIE ( -- x addr ) 3 bit I2C2_CR1 ; \ I2C2_CR1_ADDRIE, Address match interrupt enable slave  only
: I2C2_CR1_NACKIE ( -- x addr ) 4 bit I2C2_CR1 ; \ I2C2_CR1_NACKIE, Not acknowledge received interrupt  enable
: I2C2_CR1_STOPIE ( -- x addr ) 5 bit I2C2_CR1 ; \ I2C2_CR1_STOPIE, STOP detection Interrupt  enable
: I2C2_CR1_TCIE ( -- x addr ) 6 bit I2C2_CR1 ; \ I2C2_CR1_TCIE, Transfer Complete interrupt  enable
: I2C2_CR1_ERRIE ( -- x addr ) 7 bit I2C2_CR1 ; \ I2C2_CR1_ERRIE, Error interrupts enable
: I2C2_CR1_DNF ( %bbbb -- x addr ) 8 lshift I2C2_CR1 ; \ I2C2_CR1_DNF, Digital noise filter
: I2C2_CR1_ANFOFF ( -- x addr ) 12 bit I2C2_CR1 ; \ I2C2_CR1_ANFOFF, Analog noise filter OFF
: I2C2_CR1_TXDMAEN ( -- x addr ) 14 bit I2C2_CR1 ; \ I2C2_CR1_TXDMAEN, DMA transmission requests  enable
: I2C2_CR1_RXDMAEN ( -- x addr ) 15 bit I2C2_CR1 ; \ I2C2_CR1_RXDMAEN, DMA reception requests  enable
: I2C2_CR1_SBC ( -- x addr ) 16 bit I2C2_CR1 ; \ I2C2_CR1_SBC, Slave byte control
: I2C2_CR1_NOSTRETCH ( -- x addr ) 17 bit I2C2_CR1 ; \ I2C2_CR1_NOSTRETCH, Clock stretching disable
: I2C2_CR1_WUPEN ( -- x addr ) 18 bit I2C2_CR1 ; \ I2C2_CR1_WUPEN, Wakeup from STOP enable
: I2C2_CR1_GCEN ( -- x addr ) 19 bit I2C2_CR1 ; \ I2C2_CR1_GCEN, General call enable
: I2C2_CR1_SMBHEN ( -- x addr ) 20 bit I2C2_CR1 ; \ I2C2_CR1_SMBHEN, SMBus Host address enable
: I2C2_CR1_SMBDEN ( -- x addr ) 21 bit I2C2_CR1 ; \ I2C2_CR1_SMBDEN, SMBus Device Default address  enable
: I2C2_CR1_ALERTEN ( -- x addr ) 22 bit I2C2_CR1 ; \ I2C2_CR1_ALERTEN, SMBUS alert enable
: I2C2_CR1_PECEN ( -- x addr ) 23 bit I2C2_CR1 ; \ I2C2_CR1_PECEN, PEC enable
[then]

execute-defined? use-I2C2 defined? I2C2_CR2_PECBYTE not and [if]
\ I2C2_CR2 (read-write) Reset:0x00000000
: I2C2_CR2_PECBYTE ( -- x addr ) 26 bit I2C2_CR2 ; \ I2C2_CR2_PECBYTE, Packet error checking byte
: I2C2_CR2_AUTOEND ( -- x addr ) 25 bit I2C2_CR2 ; \ I2C2_CR2_AUTOEND, Automatic end mode master  mode
: I2C2_CR2_RELOAD ( -- x addr ) 24 bit I2C2_CR2 ; \ I2C2_CR2_RELOAD, NBYTES reload mode
: I2C2_CR2_NBYTES ( %bbbbbbbb -- x addr ) 16 lshift I2C2_CR2 ; \ I2C2_CR2_NBYTES, Number of bytes
: I2C2_CR2_NACK ( -- x addr ) 15 bit I2C2_CR2 ; \ I2C2_CR2_NACK, NACK generation slave  mode
: I2C2_CR2_STOP ( -- x addr ) 14 bit I2C2_CR2 ; \ I2C2_CR2_STOP, Stop generation master  mode
: I2C2_CR2_START ( -- x addr ) 13 bit I2C2_CR2 ; \ I2C2_CR2_START, Start generation
: I2C2_CR2_HEAD10R ( -- x addr ) 12 bit I2C2_CR2 ; \ I2C2_CR2_HEAD10R, 10-bit address header only read  direction master receiver mode
: I2C2_CR2_ADD10 ( -- x addr ) 11 bit I2C2_CR2 ; \ I2C2_CR2_ADD10, 10-bit addressing mode master  mode
: I2C2_CR2_RD_WRN ( -- x addr ) 10 bit I2C2_CR2 ; \ I2C2_CR2_RD_WRN, Transfer direction master  mode
: I2C2_CR2_SADD ( %bbbbbbbbbb -- x addr ) I2C2_CR2 ; \ I2C2_CR2_SADD, Slave address bit master  mode
[then]

defined? use-I2C2 defined? I2C2_OAR1_OA1 not and [if]
\ I2C2_OAR1 (read-write) Reset:0x00000000
: I2C2_OAR1_OA1 ( %bbbbbbbbbb -- x addr ) I2C2_OAR1 ; \ I2C2_OAR1_OA1, Interface address
: I2C2_OAR1_OA1MODE ( -- x addr ) 10 bit I2C2_OAR1 ; \ I2C2_OAR1_OA1MODE, Own Address 1 10-bit mode
: I2C2_OAR1_OA1EN ( -- x addr ) 15 bit I2C2_OAR1 ; \ I2C2_OAR1_OA1EN, Own Address 1 enable
[then]

execute-defined? use-I2C2 defined? I2C2_OAR2_OA2 not and [if]
\ I2C2_OAR2 (read-write) Reset:0x00000000
: I2C2_OAR2_OA2 ( %bbbbbbb -- x addr ) 1 lshift I2C2_OAR2 ; \ I2C2_OAR2_OA2, Interface address
: I2C2_OAR2_OA2MSK ( %bbb -- x addr ) 8 lshift I2C2_OAR2 ; \ I2C2_OAR2_OA2MSK, Own Address 2 masks
: I2C2_OAR2_OA2EN ( -- x addr ) 15 bit I2C2_OAR2 ; \ I2C2_OAR2_OA2EN, Own Address 2 enable
[then]

defined? use-I2C2 defined? I2C2_TIMINGR_SCLL not and [if]
\ I2C2_TIMINGR (read-write) Reset:0x00000000
: I2C2_TIMINGR_SCLL ( %bbbbbbbb -- x addr ) I2C2_TIMINGR ; \ I2C2_TIMINGR_SCLL, SCL low period master  mode
: I2C2_TIMINGR_SCLH ( %bbbbbbbb -- x addr ) 8 lshift I2C2_TIMINGR ; \ I2C2_TIMINGR_SCLH, SCL high period master  mode
: I2C2_TIMINGR_SDADEL ( %bbbb -- x addr ) 16 lshift I2C2_TIMINGR ; \ I2C2_TIMINGR_SDADEL, Data hold time
: I2C2_TIMINGR_SCLDEL ( %bbbb -- x addr ) 20 lshift I2C2_TIMINGR ; \ I2C2_TIMINGR_SCLDEL, Data setup time
: I2C2_TIMINGR_PRESC ( %bbbb -- x addr ) 28 lshift I2C2_TIMINGR ; \ I2C2_TIMINGR_PRESC, Timing prescaler
[then]

execute-defined? use-I2C2 defined? I2C2_TIMEOUTR_TIMEOUTA not and [if]
\ I2C2_TIMEOUTR (read-write) Reset:0x00000000
: I2C2_TIMEOUTR_TIMEOUTA ( %bbbbbbbbbbb -- x addr ) I2C2_TIMEOUTR ; \ I2C2_TIMEOUTR_TIMEOUTA, Bus timeout A
: I2C2_TIMEOUTR_TIDLE ( -- x addr ) 12 bit I2C2_TIMEOUTR ; \ I2C2_TIMEOUTR_TIDLE, Idle clock timeout  detection
: I2C2_TIMEOUTR_TIMOUTEN ( -- x addr ) 15 bit I2C2_TIMEOUTR ; \ I2C2_TIMEOUTR_TIMOUTEN, Clock timeout enable
: I2C2_TIMEOUTR_TIMEOUTB ( %bbbbbbbbbbb -- x addr ) 16 lshift I2C2_TIMEOUTR ; \ I2C2_TIMEOUTR_TIMEOUTB, Bus timeout B
: I2C2_TIMEOUTR_TEXTEN ( -- x addr ) 31 bit I2C2_TIMEOUTR ; \ I2C2_TIMEOUTR_TEXTEN, Extended clock timeout  enable
[then]

defined? use-I2C2 defined? I2C2_ISR_ADDCODE not and [if]
\ I2C2_ISR (multiple-access)  Reset:0x00000001
: I2C2_ISR_ADDCODE ( %bbbbbbb -- x addr ) 17 lshift I2C2_ISR ; \ I2C2_ISR_ADDCODE, Address match code Slave  mode
: I2C2_ISR_DIR ( -- x addr ) 16 bit I2C2_ISR ; \ I2C2_ISR_DIR, Transfer direction Slave  mode
: I2C2_ISR_BUSY ( -- x addr ) 15 bit I2C2_ISR ; \ I2C2_ISR_BUSY, Bus busy
: I2C2_ISR_ALERT ( -- x addr ) 13 bit I2C2_ISR ; \ I2C2_ISR_ALERT, SMBus alert
: I2C2_ISR_TIMEOUT? ( -- 1|0 ) 12 bit I2C2_ISR bit@ ; \ I2C2_ISR_TIMEOUT, Timeout or t_low detection  flag
: I2C2_ISR_PECERR ( -- x addr ) 11 bit I2C2_ISR ; \ I2C2_ISR_PECERR, PEC Error in reception
: I2C2_ISR_OVR ( -- x addr ) 10 bit I2C2_ISR ; \ I2C2_ISR_OVR, Overrun/Underrun slave  mode
: I2C2_ISR_ARLO ( -- x addr ) 9 bit I2C2_ISR ; \ I2C2_ISR_ARLO, Arbitration lost
: I2C2_ISR_BERR ( -- x addr ) 8 bit I2C2_ISR ; \ I2C2_ISR_BERR, Bus error
: I2C2_ISR_TCR ( -- x addr ) 7 bit I2C2_ISR ; \ I2C2_ISR_TCR, Transfer Complete Reload
: I2C2_ISR_TC ( -- x addr ) 6 bit I2C2_ISR ; \ I2C2_ISR_TC, Transfer Complete master  mode
: I2C2_ISR_STOPF? ( -- 1|0 ) 5 bit I2C2_ISR bit@ ; \ I2C2_ISR_STOPF, Stop detection flag
: I2C2_ISR_NACKF? ( -- 1|0 ) 4 bit I2C2_ISR bit@ ; \ I2C2_ISR_NACKF, Not acknowledge received  flag
: I2C2_ISR_ADDR ( -- x addr ) 3 bit I2C2_ISR ; \ I2C2_ISR_ADDR, Address matched slave  mode
: I2C2_ISR_RXNE ( -- x addr ) 2 bit I2C2_ISR ; \ I2C2_ISR_RXNE, Receive data register not empty  receivers
: I2C2_ISR_TXIS? ( -- 1|0 ) 1 bit I2C2_ISR bit@ ; \ I2C2_ISR_TXIS, Transmit interrupt status  transmitters
: I2C2_ISR_TXE ( -- x addr ) 0 bit I2C2_ISR ; \ I2C2_ISR_TXE, Transmit data register empty  transmitters
[then]

execute-defined? use-I2C2 defined? I2C2_ICR_ALERTCF not and [if]
\ I2C2_ICR (write-only) Reset:0x00000000
: I2C2_ICR_ALERTCF ( -- x addr ) 13 bit I2C2_ICR ; \ I2C2_ICR_ALERTCF, Alert flag clear
: I2C2_ICR_TIMOUTCF ( -- x addr ) 12 bit I2C2_ICR ; \ I2C2_ICR_TIMOUTCF, Timeout detection flag  clear
: I2C2_ICR_PECCF ( -- x addr ) 11 bit I2C2_ICR ; \ I2C2_ICR_PECCF, PEC Error flag clear
: I2C2_ICR_OVRCF ( -- x addr ) 10 bit I2C2_ICR ; \ I2C2_ICR_OVRCF, Overrun/Underrun flag  clear
: I2C2_ICR_ARLOCF ( -- x addr ) 9 bit I2C2_ICR ; \ I2C2_ICR_ARLOCF, Arbitration lost flag  clear
: I2C2_ICR_BERRCF ( -- x addr ) 8 bit I2C2_ICR ; \ I2C2_ICR_BERRCF, Bus error flag clear
: I2C2_ICR_STOPCF ( -- x addr ) 5 bit I2C2_ICR ; \ I2C2_ICR_STOPCF, Stop detection flag clear
: I2C2_ICR_NACKCF ( -- x addr ) 4 bit I2C2_ICR ; \ I2C2_ICR_NACKCF, Not Acknowledge flag clear
: I2C2_ICR_ADDRCF ( -- x addr ) 3 bit I2C2_ICR ; \ I2C2_ICR_ADDRCF, Address Matched flag clear
[then]

defined? use-I2C2 defined? I2C2_PECR_PEC? not and [if]
\ I2C2_PECR (read-only) Reset:0x00000000
: I2C2_PECR_PEC? ( --  x ) I2C2_PECR @ ; \ I2C2_PECR_PEC, Packet error checking  register
[then]

execute-defined? use-I2C2 defined? I2C2_RXDR_RXDATA? not and [if]
\ I2C2_RXDR (read-only) Reset:0x00000000
: I2C2_RXDR_RXDATA? ( --  x ) I2C2_RXDR @ ; \ I2C2_RXDR_RXDATA, 8-bit receive data
[then]

defined? use-I2C2 defined? I2C2_TXDR_TXDATA not and [if]
\ I2C2_TXDR (read-write) Reset:0x00000000
: I2C2_TXDR_TXDATA ( %bbbbbbbb -- x addr ) I2C2_TXDR ; \ I2C2_TXDR_TXDATA, 8-bit transmit data
[then]

execute-defined? use-I2C3 defined? I2C3_CR1_PE not and [if]
\ I2C3_CR1 (read-write) Reset:0x00000000
: I2C3_CR1_PE ( -- x addr ) 0 bit I2C3_CR1 ; \ I2C3_CR1_PE, Peripheral enable
: I2C3_CR1_TXIE ( -- x addr ) 1 bit I2C3_CR1 ; \ I2C3_CR1_TXIE, TX Interrupt enable
: I2C3_CR1_RXIE ( -- x addr ) 2 bit I2C3_CR1 ; \ I2C3_CR1_RXIE, RX Interrupt enable
: I2C3_CR1_ADDRIE ( -- x addr ) 3 bit I2C3_CR1 ; \ I2C3_CR1_ADDRIE, Address match interrupt enable slave  only
: I2C3_CR1_NACKIE ( -- x addr ) 4 bit I2C3_CR1 ; \ I2C3_CR1_NACKIE, Not acknowledge received interrupt  enable
: I2C3_CR1_STOPIE ( -- x addr ) 5 bit I2C3_CR1 ; \ I2C3_CR1_STOPIE, STOP detection Interrupt  enable
: I2C3_CR1_TCIE ( -- x addr ) 6 bit I2C3_CR1 ; \ I2C3_CR1_TCIE, Transfer Complete interrupt  enable
: I2C3_CR1_ERRIE ( -- x addr ) 7 bit I2C3_CR1 ; \ I2C3_CR1_ERRIE, Error interrupts enable
: I2C3_CR1_DNF ( %bbbb -- x addr ) 8 lshift I2C3_CR1 ; \ I2C3_CR1_DNF, Digital noise filter
: I2C3_CR1_ANFOFF ( -- x addr ) 12 bit I2C3_CR1 ; \ I2C3_CR1_ANFOFF, Analog noise filter OFF
: I2C3_CR1_TXDMAEN ( -- x addr ) 14 bit I2C3_CR1 ; \ I2C3_CR1_TXDMAEN, DMA transmission requests  enable
: I2C3_CR1_RXDMAEN ( -- x addr ) 15 bit I2C3_CR1 ; \ I2C3_CR1_RXDMAEN, DMA reception requests  enable
: I2C3_CR1_SBC ( -- x addr ) 16 bit I2C3_CR1 ; \ I2C3_CR1_SBC, Slave byte control
: I2C3_CR1_NOSTRETCH ( -- x addr ) 17 bit I2C3_CR1 ; \ I2C3_CR1_NOSTRETCH, Clock stretching disable
: I2C3_CR1_WUPEN ( -- x addr ) 18 bit I2C3_CR1 ; \ I2C3_CR1_WUPEN, Wakeup from STOP enable
: I2C3_CR1_GCEN ( -- x addr ) 19 bit I2C3_CR1 ; \ I2C3_CR1_GCEN, General call enable
: I2C3_CR1_SMBHEN ( -- x addr ) 20 bit I2C3_CR1 ; \ I2C3_CR1_SMBHEN, SMBus Host address enable
: I2C3_CR1_SMBDEN ( -- x addr ) 21 bit I2C3_CR1 ; \ I2C3_CR1_SMBDEN, SMBus Device Default address  enable
: I2C3_CR1_ALERTEN ( -- x addr ) 22 bit I2C3_CR1 ; \ I2C3_CR1_ALERTEN, SMBUS alert enable
: I2C3_CR1_PECEN ( -- x addr ) 23 bit I2C3_CR1 ; \ I2C3_CR1_PECEN, PEC enable
[then]

defined? use-I2C3 defined? I2C3_CR2_PECBYTE not and [if]
\ I2C3_CR2 (read-write) Reset:0x00000000
: I2C3_CR2_PECBYTE ( -- x addr ) 26 bit I2C3_CR2 ; \ I2C3_CR2_PECBYTE, Packet error checking byte
: I2C3_CR2_AUTOEND ( -- x addr ) 25 bit I2C3_CR2 ; \ I2C3_CR2_AUTOEND, Automatic end mode master  mode
: I2C3_CR2_RELOAD ( -- x addr ) 24 bit I2C3_CR2 ; \ I2C3_CR2_RELOAD, NBYTES reload mode
: I2C3_CR2_NBYTES ( %bbbbbbbb -- x addr ) 16 lshift I2C3_CR2 ; \ I2C3_CR2_NBYTES, Number of bytes
: I2C3_CR2_NACK ( -- x addr ) 15 bit I2C3_CR2 ; \ I2C3_CR2_NACK, NACK generation slave  mode
: I2C3_CR2_STOP ( -- x addr ) 14 bit I2C3_CR2 ; \ I2C3_CR2_STOP, Stop generation master  mode
: I2C3_CR2_START ( -- x addr ) 13 bit I2C3_CR2 ; \ I2C3_CR2_START, Start generation
: I2C3_CR2_HEAD10R ( -- x addr ) 12 bit I2C3_CR2 ; \ I2C3_CR2_HEAD10R, 10-bit address header only read  direction master receiver mode
: I2C3_CR2_ADD10 ( -- x addr ) 11 bit I2C3_CR2 ; \ I2C3_CR2_ADD10, 10-bit addressing mode master  mode
: I2C3_CR2_RD_WRN ( -- x addr ) 10 bit I2C3_CR2 ; \ I2C3_CR2_RD_WRN, Transfer direction master  mode
: I2C3_CR2_SADD ( %bbbbbbbbbb -- x addr ) I2C3_CR2 ; \ I2C3_CR2_SADD, Slave address bit master  mode
[then]

execute-defined? use-I2C3 defined? I2C3_OAR1_OA1 not and [if]
\ I2C3_OAR1 (read-write) Reset:0x00000000
: I2C3_OAR1_OA1 ( %bbbbbbbbbb -- x addr ) I2C3_OAR1 ; \ I2C3_OAR1_OA1, Interface address
: I2C3_OAR1_OA1MODE ( -- x addr ) 10 bit I2C3_OAR1 ; \ I2C3_OAR1_OA1MODE, Own Address 1 10-bit mode
: I2C3_OAR1_OA1EN ( -- x addr ) 15 bit I2C3_OAR1 ; \ I2C3_OAR1_OA1EN, Own Address 1 enable
[then]

defined? use-I2C3 defined? I2C3_OAR2_OA2 not and [if]
\ I2C3_OAR2 (read-write) Reset:0x00000000
: I2C3_OAR2_OA2 ( %bbbbbbb -- x addr ) 1 lshift I2C3_OAR2 ; \ I2C3_OAR2_OA2, Interface address
: I2C3_OAR2_OA2MSK ( %bbb -- x addr ) 8 lshift I2C3_OAR2 ; \ I2C3_OAR2_OA2MSK, Own Address 2 masks
: I2C3_OAR2_OA2EN ( -- x addr ) 15 bit I2C3_OAR2 ; \ I2C3_OAR2_OA2EN, Own Address 2 enable
[then]

execute-defined? use-I2C3 defined? I2C3_TIMINGR_SCLL not and [if]
\ I2C3_TIMINGR (read-write) Reset:0x00000000
: I2C3_TIMINGR_SCLL ( %bbbbbbbb -- x addr ) I2C3_TIMINGR ; \ I2C3_TIMINGR_SCLL, SCL low period master  mode
: I2C3_TIMINGR_SCLH ( %bbbbbbbb -- x addr ) 8 lshift I2C3_TIMINGR ; \ I2C3_TIMINGR_SCLH, SCL high period master  mode
: I2C3_TIMINGR_SDADEL ( %bbbb -- x addr ) 16 lshift I2C3_TIMINGR ; \ I2C3_TIMINGR_SDADEL, Data hold time
: I2C3_TIMINGR_SCLDEL ( %bbbb -- x addr ) 20 lshift I2C3_TIMINGR ; \ I2C3_TIMINGR_SCLDEL, Data setup time
: I2C3_TIMINGR_PRESC ( %bbbb -- x addr ) 28 lshift I2C3_TIMINGR ; \ I2C3_TIMINGR_PRESC, Timing prescaler
[then]

defined? use-I2C3 defined? I2C3_TIMEOUTR_TIMEOUTA not and [if]
\ I2C3_TIMEOUTR (read-write) Reset:0x00000000
: I2C3_TIMEOUTR_TIMEOUTA ( %bbbbbbbbbbb -- x addr ) I2C3_TIMEOUTR ; \ I2C3_TIMEOUTR_TIMEOUTA, Bus timeout A
: I2C3_TIMEOUTR_TIDLE ( -- x addr ) 12 bit I2C3_TIMEOUTR ; \ I2C3_TIMEOUTR_TIDLE, Idle clock timeout  detection
: I2C3_TIMEOUTR_TIMOUTEN ( -- x addr ) 15 bit I2C3_TIMEOUTR ; \ I2C3_TIMEOUTR_TIMOUTEN, Clock timeout enable
: I2C3_TIMEOUTR_TIMEOUTB ( %bbbbbbbbbbb -- x addr ) 16 lshift I2C3_TIMEOUTR ; \ I2C3_TIMEOUTR_TIMEOUTB, Bus timeout B
: I2C3_TIMEOUTR_TEXTEN ( -- x addr ) 31 bit I2C3_TIMEOUTR ; \ I2C3_TIMEOUTR_TEXTEN, Extended clock timeout  enable
[then]

execute-defined? use-I2C3 defined? I2C3_ISR_ADDCODE not and [if]
\ I2C3_ISR (multiple-access)  Reset:0x00000001
: I2C3_ISR_ADDCODE ( %bbbbbbb -- x addr ) 17 lshift I2C3_ISR ; \ I2C3_ISR_ADDCODE, Address match code Slave  mode
: I2C3_ISR_DIR ( -- x addr ) 16 bit I2C3_ISR ; \ I2C3_ISR_DIR, Transfer direction Slave  mode
: I2C3_ISR_BUSY ( -- x addr ) 15 bit I2C3_ISR ; \ I2C3_ISR_BUSY, Bus busy
: I2C3_ISR_ALERT ( -- x addr ) 13 bit I2C3_ISR ; \ I2C3_ISR_ALERT, SMBus alert
: I2C3_ISR_TIMEOUT? ( -- 1|0 ) 12 bit I2C3_ISR bit@ ; \ I2C3_ISR_TIMEOUT, Timeout or t_low detection  flag
: I2C3_ISR_PECERR ( -- x addr ) 11 bit I2C3_ISR ; \ I2C3_ISR_PECERR, PEC Error in reception
: I2C3_ISR_OVR ( -- x addr ) 10 bit I2C3_ISR ; \ I2C3_ISR_OVR, Overrun/Underrun slave  mode
: I2C3_ISR_ARLO ( -- x addr ) 9 bit I2C3_ISR ; \ I2C3_ISR_ARLO, Arbitration lost
: I2C3_ISR_BERR ( -- x addr ) 8 bit I2C3_ISR ; \ I2C3_ISR_BERR, Bus error
: I2C3_ISR_TCR ( -- x addr ) 7 bit I2C3_ISR ; \ I2C3_ISR_TCR, Transfer Complete Reload
: I2C3_ISR_TC ( -- x addr ) 6 bit I2C3_ISR ; \ I2C3_ISR_TC, Transfer Complete master  mode
: I2C3_ISR_STOPF? ( -- 1|0 ) 5 bit I2C3_ISR bit@ ; \ I2C3_ISR_STOPF, Stop detection flag
: I2C3_ISR_NACKF? ( -- 1|0 ) 4 bit I2C3_ISR bit@ ; \ I2C3_ISR_NACKF, Not acknowledge received  flag
: I2C3_ISR_ADDR ( -- x addr ) 3 bit I2C3_ISR ; \ I2C3_ISR_ADDR, Address matched slave  mode
: I2C3_ISR_RXNE ( -- x addr ) 2 bit I2C3_ISR ; \ I2C3_ISR_RXNE, Receive data register not empty  receivers
: I2C3_ISR_TXIS? ( -- 1|0 ) 1 bit I2C3_ISR bit@ ; \ I2C3_ISR_TXIS, Transmit interrupt status  transmitters
: I2C3_ISR_TXE ( -- x addr ) 0 bit I2C3_ISR ; \ I2C3_ISR_TXE, Transmit data register empty  transmitters
[then]

defined? use-I2C3 defined? I2C3_ICR_ALERTCF not and [if]
\ I2C3_ICR (write-only) Reset:0x00000000
: I2C3_ICR_ALERTCF ( -- x addr ) 13 bit I2C3_ICR ; \ I2C3_ICR_ALERTCF, Alert flag clear
: I2C3_ICR_TIMOUTCF ( -- x addr ) 12 bit I2C3_ICR ; \ I2C3_ICR_TIMOUTCF, Timeout detection flag  clear
: I2C3_ICR_PECCF ( -- x addr ) 11 bit I2C3_ICR ; \ I2C3_ICR_PECCF, PEC Error flag clear
: I2C3_ICR_OVRCF ( -- x addr ) 10 bit I2C3_ICR ; \ I2C3_ICR_OVRCF, Overrun/Underrun flag  clear
: I2C3_ICR_ARLOCF ( -- x addr ) 9 bit I2C3_ICR ; \ I2C3_ICR_ARLOCF, Arbitration lost flag  clear
: I2C3_ICR_BERRCF ( -- x addr ) 8 bit I2C3_ICR ; \ I2C3_ICR_BERRCF, Bus error flag clear
: I2C3_ICR_STOPCF ( -- x addr ) 5 bit I2C3_ICR ; \ I2C3_ICR_STOPCF, Stop detection flag clear
: I2C3_ICR_NACKCF ( -- x addr ) 4 bit I2C3_ICR ; \ I2C3_ICR_NACKCF, Not Acknowledge flag clear
: I2C3_ICR_ADDRCF ( -- x addr ) 3 bit I2C3_ICR ; \ I2C3_ICR_ADDRCF, Address Matched flag clear
[then]

execute-defined? use-I2C3 defined? I2C3_PECR_PEC? not and [if]
\ I2C3_PECR (read-only) Reset:0x00000000
: I2C3_PECR_PEC? ( --  x ) I2C3_PECR @ ; \ I2C3_PECR_PEC, Packet error checking  register
[then]

defined? use-I2C3 defined? I2C3_RXDR_RXDATA? not and [if]
\ I2C3_RXDR (read-only) Reset:0x00000000
: I2C3_RXDR_RXDATA? ( --  x ) I2C3_RXDR @ ; \ I2C3_RXDR_RXDATA, 8-bit receive data
[then]

execute-defined? use-I2C3 defined? I2C3_TXDR_TXDATA not and [if]
\ I2C3_TXDR (read-write) Reset:0x00000000
: I2C3_TXDR_TXDATA ( %bbbbbbbb -- x addr ) I2C3_TXDR ; \ I2C3_TXDR_TXDATA, 8-bit transmit data
[then]

defined? use-RCC defined? RCC_CR_PLLSAI2RDY? not and [if]
\ RCC_CR (multiple-access)  Reset:0x00000063
: RCC_CR_PLLSAI2RDY? ( -- 1|0 ) 29 bit RCC_CR bit@ ; \ RCC_CR_PLLSAI2RDY, SAI2 PLL clock ready flag
: RCC_CR_PLLSAI2ON ( -- x addr ) 28 bit RCC_CR ; \ RCC_CR_PLLSAI2ON, SAI2 PLL enable
: RCC_CR_PLLSAI1RDY? ( -- 1|0 ) 27 bit RCC_CR bit@ ; \ RCC_CR_PLLSAI1RDY, SAI1 PLL clock ready flag
: RCC_CR_PLLSAI1ON ( -- x addr ) 26 bit RCC_CR ; \ RCC_CR_PLLSAI1ON, SAI1 PLL enable
: RCC_CR_PLLRDY? ( -- 1|0 ) 25 bit RCC_CR bit@ ; \ RCC_CR_PLLRDY, Main PLL clock ready flag
: RCC_CR_PLLON ( -- x addr ) 24 bit RCC_CR ; \ RCC_CR_PLLON, Main PLL enable
: RCC_CR_CSSON ( -- x addr ) 19 bit RCC_CR ; \ RCC_CR_CSSON, Clock security system  enable
: RCC_CR_HSEBYP ( -- x addr ) 18 bit RCC_CR ; \ RCC_CR_HSEBYP, HSE crystal oscillator  bypass
: RCC_CR_HSERDY? ( -- 1|0 ) 17 bit RCC_CR bit@ ; \ RCC_CR_HSERDY, HSE clock ready flag
: RCC_CR_HSEON ( -- x addr ) 16 bit RCC_CR ; \ RCC_CR_HSEON, HSE clock enable
: RCC_CR_HSIASFS ( -- x addr ) 11 bit RCC_CR ; \ RCC_CR_HSIASFS, HSI automatic start from  Stop
: RCC_CR_HSIRDY? ( -- 1|0 ) 10 bit RCC_CR bit@ ; \ RCC_CR_HSIRDY, HSI clock ready flag
: RCC_CR_HSIKERON ( -- x addr ) 9 bit RCC_CR ; \ RCC_CR_HSIKERON, HSI always enable for peripheral  kernels
: RCC_CR_HSION ( -- x addr ) 8 bit RCC_CR ; \ RCC_CR_HSION, HSI clock enable
: RCC_CR_MSIRANGE ( %bbbb -- x addr ) 4 lshift RCC_CR ; \ RCC_CR_MSIRANGE, MSI clock ranges
: RCC_CR_MSIRGSEL ( -- x addr ) 3 bit RCC_CR ; \ RCC_CR_MSIRGSEL, MSI clock range selection
: RCC_CR_MSIPLLEN ( -- x addr ) 2 bit RCC_CR ; \ RCC_CR_MSIPLLEN, MSI clock PLL enable
: RCC_CR_MSIRDY? ( -- 1|0 ) 1 bit RCC_CR bit@ ; \ RCC_CR_MSIRDY, MSI clock ready flag
: RCC_CR_MSION ( -- x addr ) 0 bit RCC_CR ; \ RCC_CR_MSION, MSI clock enable
[then]

execute-defined? use-RCC defined? RCC_ICSCR_HSITRIM not and [if]
\ RCC_ICSCR (multiple-access)  Reset:0x10000000
: RCC_ICSCR_HSITRIM ( %bbbbb -- x addr ) 24 lshift RCC_ICSCR ; \ RCC_ICSCR_HSITRIM, HSI clock trimming
: RCC_ICSCR_HSICAL ( %bbbbbbbb -- x addr ) 16 lshift RCC_ICSCR ; \ RCC_ICSCR_HSICAL, HSI clock calibration
: RCC_ICSCR_MSITRIM ( %bbbbbbbb -- x addr ) 8 lshift RCC_ICSCR ; \ RCC_ICSCR_MSITRIM, MSI clock trimming
: RCC_ICSCR_MSICAL ( %bbbbbbbb -- x addr ) RCC_ICSCR ; \ RCC_ICSCR_MSICAL, MSI clock calibration
[then]

defined? use-RCC defined? RCC_CFGR_MCOPRE not and [if]
\ RCC_CFGR (multiple-access)  Reset:0x00000000
: RCC_CFGR_MCOPRE ( %bbb -- x addr ) 28 lshift RCC_CFGR ; \ RCC_CFGR_MCOPRE, Microcontroller clock output  prescaler
: RCC_CFGR_MCOSEL ( %bbb -- x addr ) 24 lshift RCC_CFGR ; \ RCC_CFGR_MCOSEL, Microcontroller clock  output
: RCC_CFGR_STOPWUCK ( -- x addr ) 15 bit RCC_CFGR ; \ RCC_CFGR_STOPWUCK, Wakeup from Stop and CSS backup clock  selection
: RCC_CFGR_PPRE2 ( %bbb -- x addr ) 11 lshift RCC_CFGR ; \ RCC_CFGR_PPRE2, APB high-speed prescaler  APB2
: RCC_CFGR_PPRE1 ( %bbb -- x addr ) 8 lshift RCC_CFGR ; \ RCC_CFGR_PPRE1, PB low-speed prescaler  APB1
: RCC_CFGR_HPRE ( %bbbb -- x addr ) 4 lshift RCC_CFGR ; \ RCC_CFGR_HPRE, AHB prescaler
: RCC_CFGR_SWS? ( %bb -- 1|0 ) 2 lshift RCC_CFGR bit@ ; \ RCC_CFGR_SWS, System clock switch status
: RCC_CFGR_SW ( %bb -- x addr ) RCC_CFGR ; \ RCC_CFGR_SW, System clock switch
[then]

execute-defined? use-RCC defined? RCC_PLLCFGR_PLLR not and [if]
\ RCC_PLLCFGR (read-write) Reset:0x00001000
: RCC_PLLCFGR_PLLR ( %bb -- x addr ) 25 lshift RCC_PLLCFGR ; \ RCC_PLLCFGR_PLLR, Main PLL division factor for PLLCLK  system clock
: RCC_PLLCFGR_PLLREN ( -- x addr ) 24 bit RCC_PLLCFGR ; \ RCC_PLLCFGR_PLLREN, Main PLL PLLCLK output  enable
: RCC_PLLCFGR_PLLQ ( %bb -- x addr ) 21 lshift RCC_PLLCFGR ; \ RCC_PLLCFGR_PLLQ, Main PLL division factor for  PLLUSB1CLK48 MHz clock
: RCC_PLLCFGR_PLLQEN ( -- x addr ) 20 bit RCC_PLLCFGR ; \ RCC_PLLCFGR_PLLQEN, Main PLL PLLUSB1CLK output  enable
: RCC_PLLCFGR_PLLP ( -- x addr ) 17 bit RCC_PLLCFGR ; \ RCC_PLLCFGR_PLLP, Main PLL division factor for PLLSAI3CLK  SAI1 and SAI2 clock
: RCC_PLLCFGR_PLLPEN ( -- x addr ) 16 bit RCC_PLLCFGR ; \ RCC_PLLCFGR_PLLPEN, Main PLL PLLSAI3CLK output  enable
: RCC_PLLCFGR_PLLN ( %bbbbbbb -- x addr ) 8 lshift RCC_PLLCFGR ; \ RCC_PLLCFGR_PLLN, Main PLL multiplication factor for  VCO
: RCC_PLLCFGR_PLLM ( %bbb -- x addr ) 4 lshift RCC_PLLCFGR ; \ RCC_PLLCFGR_PLLM, Division factor for the main PLL and  audio PLL PLLSAI1 and PLLSAI2 input  clock
: RCC_PLLCFGR_PLLSRC ( %bb -- x addr ) RCC_PLLCFGR ; \ RCC_PLLCFGR_PLLSRC, Main PLL, PLLSAI1 and PLLSAI2 entry  clock source
[then]

defined? use-RCC defined? RCC_PLLSAI1CFGR_PLLSAI1R not and [if]
\ RCC_PLLSAI1CFGR (read-write) Reset:0x00001000
: RCC_PLLSAI1CFGR_PLLSAI1R ( %bb -- x addr ) 25 lshift RCC_PLLSAI1CFGR ; \ RCC_PLLSAI1CFGR_PLLSAI1R, PLLSAI1 division factor for PLLADC1CLK  ADC clock
: RCC_PLLSAI1CFGR_PLLSAI1REN ( -- x addr ) 24 bit RCC_PLLSAI1CFGR ; \ RCC_PLLSAI1CFGR_PLLSAI1REN, PLLSAI1 PLLADC1CLK output  enable
: RCC_PLLSAI1CFGR_PLLSAI1Q ( %bb -- x addr ) 21 lshift RCC_PLLSAI1CFGR ; \ RCC_PLLSAI1CFGR_PLLSAI1Q, SAI1PLL division factor for PLLUSB2CLK  48 MHz clock
: RCC_PLLSAI1CFGR_PLLSAI1QEN ( -- x addr ) 20 bit RCC_PLLSAI1CFGR ; \ RCC_PLLSAI1CFGR_PLLSAI1QEN, SAI1PLL PLLUSB2CLK output  enable
: RCC_PLLSAI1CFGR_PLLSAI1P ( -- x addr ) 17 bit RCC_PLLSAI1CFGR ; \ RCC_PLLSAI1CFGR_PLLSAI1P, SAI1PLL division factor for PLLSAI1CLK  SAI1 or SAI2 clock
: RCC_PLLSAI1CFGR_PLLSAI1PEN ( -- x addr ) 16 bit RCC_PLLSAI1CFGR ; \ RCC_PLLSAI1CFGR_PLLSAI1PEN, SAI1PLL PLLSAI1CLK output  enable
: RCC_PLLSAI1CFGR_PLLSAI1N ( %bbbbbbb -- x addr ) 8 lshift RCC_PLLSAI1CFGR ; \ RCC_PLLSAI1CFGR_PLLSAI1N, SAI1PLL multiplication factor for  VCO
[then]

execute-defined? use-RCC defined? RCC_PLLSAI2CFGR_PLLSAI2R not and [if]
\ RCC_PLLSAI2CFGR (read-write) Reset:0x00001000
: RCC_PLLSAI2CFGR_PLLSAI2R ( %bb -- x addr ) 25 lshift RCC_PLLSAI2CFGR ; \ RCC_PLLSAI2CFGR_PLLSAI2R, PLLSAI2 division factor for PLLADC2CLK  ADC clock
: RCC_PLLSAI2CFGR_PLLSAI2REN ( -- x addr ) 24 bit RCC_PLLSAI2CFGR ; \ RCC_PLLSAI2CFGR_PLLSAI2REN, PLLSAI2 PLLADC2CLK output  enable
: RCC_PLLSAI2CFGR_PLLSAI2P ( -- x addr ) 17 bit RCC_PLLSAI2CFGR ; \ RCC_PLLSAI2CFGR_PLLSAI2P, SAI1PLL division factor for PLLSAI2CLK  SAI1 or SAI2 clock
: RCC_PLLSAI2CFGR_PLLSAI2PEN ( -- x addr ) 16 bit RCC_PLLSAI2CFGR ; \ RCC_PLLSAI2CFGR_PLLSAI2PEN, SAI2PLL PLLSAI2CLK output  enable
: RCC_PLLSAI2CFGR_PLLSAI2N ( %bbbbbbb -- x addr ) 8 lshift RCC_PLLSAI2CFGR ; \ RCC_PLLSAI2CFGR_PLLSAI2N, SAI2PLL multiplication factor for  VCO
[then]

defined? use-RCC defined? RCC_CIER_LSECSSIE not and [if]
\ RCC_CIER (read-write) Reset:0x00000000
: RCC_CIER_LSECSSIE ( -- x addr ) 9 bit RCC_CIER ; \ RCC_CIER_LSECSSIE, LSE clock security system interrupt  enable
: RCC_CIER_PLLSAI2RDYIE ( -- x addr ) 7 bit RCC_CIER ; \ RCC_CIER_PLLSAI2RDYIE, PLLSAI2 ready interrupt  enable
: RCC_CIER_PLLSAI1RDYIE ( -- x addr ) 6 bit RCC_CIER ; \ RCC_CIER_PLLSAI1RDYIE, PLLSAI1 ready interrupt  enable
: RCC_CIER_PLLRDYIE ( -- x addr ) 5 bit RCC_CIER ; \ RCC_CIER_PLLRDYIE, PLL ready interrupt enable
: RCC_CIER_HSERDYIE ( -- x addr ) 4 bit RCC_CIER ; \ RCC_CIER_HSERDYIE, HSE ready interrupt enable
: RCC_CIER_HSIRDYIE ( -- x addr ) 3 bit RCC_CIER ; \ RCC_CIER_HSIRDYIE, HSI ready interrupt enable
: RCC_CIER_MSIRDYIE ( -- x addr ) 2 bit RCC_CIER ; \ RCC_CIER_MSIRDYIE, MSI ready interrupt enable
: RCC_CIER_LSERDYIE ( -- x addr ) 1 bit RCC_CIER ; \ RCC_CIER_LSERDYIE, LSE ready interrupt enable
: RCC_CIER_LSIRDYIE ( -- x addr ) 0 bit RCC_CIER ; \ RCC_CIER_LSIRDYIE, LSI ready interrupt enable
[then]

execute-defined? use-RCC defined? RCC_CIFR_LSECSSF? not and [if]
\ RCC_CIFR (read-only) Reset:0x00000000
: RCC_CIFR_LSECSSF? ( --  1|0 ) 9 bit RCC_CIFR bit@ ; \ RCC_CIFR_LSECSSF, LSE Clock security system interrupt  flag
: RCC_CIFR_CSSF? ( --  1|0 ) 8 bit RCC_CIFR bit@ ; \ RCC_CIFR_CSSF, Clock security system interrupt  flag
: RCC_CIFR_PLLSAI2RDYF? ( --  1|0 ) 7 bit RCC_CIFR bit@ ; \ RCC_CIFR_PLLSAI2RDYF, PLLSAI2 ready interrupt  flag
: RCC_CIFR_PLLSAI1RDYF? ( --  1|0 ) 6 bit RCC_CIFR bit@ ; \ RCC_CIFR_PLLSAI1RDYF, PLLSAI1 ready interrupt  flag
: RCC_CIFR_PLLRDYF? ( --  1|0 ) 5 bit RCC_CIFR bit@ ; \ RCC_CIFR_PLLRDYF, PLL ready interrupt flag
: RCC_CIFR_HSERDYF? ( --  1|0 ) 4 bit RCC_CIFR bit@ ; \ RCC_CIFR_HSERDYF, HSE ready interrupt flag
: RCC_CIFR_HSIRDYF? ( --  1|0 ) 3 bit RCC_CIFR bit@ ; \ RCC_CIFR_HSIRDYF, HSI ready interrupt flag
: RCC_CIFR_MSIRDYF? ( --  1|0 ) 2 bit RCC_CIFR bit@ ; \ RCC_CIFR_MSIRDYF, MSI ready interrupt flag
: RCC_CIFR_LSERDYF? ( --  1|0 ) 1 bit RCC_CIFR bit@ ; \ RCC_CIFR_LSERDYF, LSE ready interrupt flag
: RCC_CIFR_LSIRDYF? ( --  1|0 ) 0 bit RCC_CIFR bit@ ; \ RCC_CIFR_LSIRDYF, LSI ready interrupt flag
[then]

defined? use-RCC defined? RCC_CICR_LSECSSC not and [if]
\ RCC_CICR (write-only) Reset:0x00000000
: RCC_CICR_LSECSSC ( -- x addr ) 9 bit RCC_CICR ; \ RCC_CICR_LSECSSC, LSE Clock security system interrupt  clear
: RCC_CICR_CSSC ( -- x addr ) 8 bit RCC_CICR ; \ RCC_CICR_CSSC, Clock security system interrupt  clear
: RCC_CICR_PLLSAI2RDYC ( -- x addr ) 7 bit RCC_CICR ; \ RCC_CICR_PLLSAI2RDYC, PLLSAI2 ready interrupt  clear
: RCC_CICR_PLLSAI1RDYC ( -- x addr ) 6 bit RCC_CICR ; \ RCC_CICR_PLLSAI1RDYC, PLLSAI1 ready interrupt  clear
: RCC_CICR_PLLRDYC ( -- x addr ) 5 bit RCC_CICR ; \ RCC_CICR_PLLRDYC, PLL ready interrupt clear
: RCC_CICR_HSERDYC ( -- x addr ) 4 bit RCC_CICR ; \ RCC_CICR_HSERDYC, HSE ready interrupt clear
: RCC_CICR_HSIRDYC ( -- x addr ) 3 bit RCC_CICR ; \ RCC_CICR_HSIRDYC, HSI ready interrupt clear
: RCC_CICR_MSIRDYC ( -- x addr ) 2 bit RCC_CICR ; \ RCC_CICR_MSIRDYC, MSI ready interrupt clear
: RCC_CICR_LSERDYC ( -- x addr ) 1 bit RCC_CICR ; \ RCC_CICR_LSERDYC, LSE ready interrupt clear
: RCC_CICR_LSIRDYC ( -- x addr ) 0 bit RCC_CICR ; \ RCC_CICR_LSIRDYC, LSI ready interrupt clear
[then]

execute-defined? use-RCC defined? RCC_AHB1RSTR_TSCRST not and [if]
\ RCC_AHB1RSTR (read-write) Reset:0x00000000
: RCC_AHB1RSTR_TSCRST ( -- x addr ) 16 bit RCC_AHB1RSTR ; \ RCC_AHB1RSTR_TSCRST, Touch Sensing Controller  reset
: RCC_AHB1RSTR_CRCRST ( -- x addr ) 11 bit RCC_AHB1RSTR ; \ RCC_AHB1RSTR_CRCRST, Reserved
: RCC_AHB1RSTR_FLASHRST ( -- x addr ) 8 bit RCC_AHB1RSTR ; \ RCC_AHB1RSTR_FLASHRST, Flash memory interface  reset
: RCC_AHB1RSTR_DMA2RST ( -- x addr ) 1 bit RCC_AHB1RSTR ; \ RCC_AHB1RSTR_DMA2RST, DMA2 reset
: RCC_AHB1RSTR_DMA1RST ( -- x addr ) 0 bit RCC_AHB1RSTR ; \ RCC_AHB1RSTR_DMA1RST, DMA1 reset
[then]

defined? use-RCC defined? RCC_AHB2RSTR_RNGRST not and [if]
\ RCC_AHB2RSTR (read-write) Reset:0x00000000
: RCC_AHB2RSTR_RNGRST ( -- x addr ) 18 bit RCC_AHB2RSTR ; \ RCC_AHB2RSTR_RNGRST, Random number generator  reset
: RCC_AHB2RSTR_AESRST ( -- x addr ) 16 bit RCC_AHB2RSTR ; \ RCC_AHB2RSTR_AESRST, AES hardware accelerator  reset
: RCC_AHB2RSTR_ADCRST ( -- x addr ) 13 bit RCC_AHB2RSTR ; \ RCC_AHB2RSTR_ADCRST, ADC reset
: RCC_AHB2RSTR_OTGFSRST ( -- x addr ) 12 bit RCC_AHB2RSTR ; \ RCC_AHB2RSTR_OTGFSRST, USB OTG FS reset
: RCC_AHB2RSTR_GPIOHRST ( -- x addr ) 7 bit RCC_AHB2RSTR ; \ RCC_AHB2RSTR_GPIOHRST, IO port H reset
: RCC_AHB2RSTR_GPIOGRST ( -- x addr ) 6 bit RCC_AHB2RSTR ; \ RCC_AHB2RSTR_GPIOGRST, IO port G reset
: RCC_AHB2RSTR_GPIOFRST ( -- x addr ) 5 bit RCC_AHB2RSTR ; \ RCC_AHB2RSTR_GPIOFRST, IO port F reset
: RCC_AHB2RSTR_GPIOERST ( -- x addr ) 4 bit RCC_AHB2RSTR ; \ RCC_AHB2RSTR_GPIOERST, IO port E reset
: RCC_AHB2RSTR_GPIODRST ( -- x addr ) 3 bit RCC_AHB2RSTR ; \ RCC_AHB2RSTR_GPIODRST, IO port D reset
: RCC_AHB2RSTR_GPIOCRST ( -- x addr ) 2 bit RCC_AHB2RSTR ; \ RCC_AHB2RSTR_GPIOCRST, IO port C reset
: RCC_AHB2RSTR_GPIOBRST ( -- x addr ) 1 bit RCC_AHB2RSTR ; \ RCC_AHB2RSTR_GPIOBRST, IO port B reset
: RCC_AHB2RSTR_GPIOARST ( -- x addr ) 0 bit RCC_AHB2RSTR ; \ RCC_AHB2RSTR_GPIOARST, IO port A reset
[then]

execute-defined? use-RCC defined? RCC_AHB3RSTR_QSPIRST not and [if]
\ RCC_AHB3RSTR (read-write) Reset:0x00000000
: RCC_AHB3RSTR_QSPIRST ( -- x addr ) 8 bit RCC_AHB3RSTR ; \ RCC_AHB3RSTR_QSPIRST, Quad SPI memory interface  reset
: RCC_AHB3RSTR_FMCRST ( -- x addr ) 0 bit RCC_AHB3RSTR ; \ RCC_AHB3RSTR_FMCRST, Flexible memory controller  reset
[then]

defined? use-RCC defined? RCC_APB1RSTR1_LPTIM1RST not and [if]
\ RCC_APB1RSTR1 (read-write) Reset:0x00000000
: RCC_APB1RSTR1_LPTIM1RST ( -- x addr ) 31 bit RCC_APB1RSTR1 ; \ RCC_APB1RSTR1_LPTIM1RST, Low Power Timer 1 reset
: RCC_APB1RSTR1_OPAMPRST ( -- x addr ) 30 bit RCC_APB1RSTR1 ; \ RCC_APB1RSTR1_OPAMPRST, OPAMP interface reset
: RCC_APB1RSTR1_DAC1RST ( -- x addr ) 29 bit RCC_APB1RSTR1 ; \ RCC_APB1RSTR1_DAC1RST, DAC1 interface reset
: RCC_APB1RSTR1_PWRRST ( -- x addr ) 28 bit RCC_APB1RSTR1 ; \ RCC_APB1RSTR1_PWRRST, Power interface reset
: RCC_APB1RSTR1_CAN1RST ( -- x addr ) 25 bit RCC_APB1RSTR1 ; \ RCC_APB1RSTR1_CAN1RST, CAN1 reset
: RCC_APB1RSTR1_I2C3RST ( -- x addr ) 23 bit RCC_APB1RSTR1 ; \ RCC_APB1RSTR1_I2C3RST, I2C3 reset
: RCC_APB1RSTR1_I2C2RST ( -- x addr ) 22 bit RCC_APB1RSTR1 ; \ RCC_APB1RSTR1_I2C2RST, I2C2 reset
: RCC_APB1RSTR1_I2C1RST ( -- x addr ) 21 bit RCC_APB1RSTR1 ; \ RCC_APB1RSTR1_I2C1RST, I2C1 reset
: RCC_APB1RSTR1_UART5RST ( -- x addr ) 20 bit RCC_APB1RSTR1 ; \ RCC_APB1RSTR1_UART5RST, UART5 reset
: RCC_APB1RSTR1_UART4RST ( -- x addr ) 19 bit RCC_APB1RSTR1 ; \ RCC_APB1RSTR1_UART4RST, UART4 reset
: RCC_APB1RSTR1_USART3RST ( -- x addr ) 18 bit RCC_APB1RSTR1 ; \ RCC_APB1RSTR1_USART3RST, USART3 reset
: RCC_APB1RSTR1_USART2RST ( -- x addr ) 17 bit RCC_APB1RSTR1 ; \ RCC_APB1RSTR1_USART2RST, USART2 reset
: RCC_APB1RSTR1_SPI3RST ( -- x addr ) 15 bit RCC_APB1RSTR1 ; \ RCC_APB1RSTR1_SPI3RST, SPI3 reset
: RCC_APB1RSTR1_SPI2RST ( -- x addr ) 14 bit RCC_APB1RSTR1 ; \ RCC_APB1RSTR1_SPI2RST, SPI2 reset
: RCC_APB1RSTR1_LCDRST ( -- x addr ) 9 bit RCC_APB1RSTR1 ; \ RCC_APB1RSTR1_LCDRST, LCD interface reset
: RCC_APB1RSTR1_TIM7RST ( -- x addr ) 5 bit RCC_APB1RSTR1 ; \ RCC_APB1RSTR1_TIM7RST, TIM7 timer reset
: RCC_APB1RSTR1_TIM6RST ( -- x addr ) 4 bit RCC_APB1RSTR1 ; \ RCC_APB1RSTR1_TIM6RST, TIM6 timer reset
: RCC_APB1RSTR1_TIM5RST ( -- x addr ) 3 bit RCC_APB1RSTR1 ; \ RCC_APB1RSTR1_TIM5RST, TIM5 timer reset
: RCC_APB1RSTR1_TIM4RST ( -- x addr ) 2 bit RCC_APB1RSTR1 ; \ RCC_APB1RSTR1_TIM4RST, TIM3 timer reset
: RCC_APB1RSTR1_TIM3RST ( -- x addr ) 1 bit RCC_APB1RSTR1 ; \ RCC_APB1RSTR1_TIM3RST, TIM3 timer reset
: RCC_APB1RSTR1_TIM2RST ( -- x addr ) 0 bit RCC_APB1RSTR1 ; \ RCC_APB1RSTR1_TIM2RST, TIM2 timer reset
[then]

execute-defined? use-RCC defined? RCC_APB1RSTR2_LPTIM2RST not and [if]
\ RCC_APB1RSTR2 (read-write) Reset:0x00000000
: RCC_APB1RSTR2_LPTIM2RST ( -- x addr ) 5 bit RCC_APB1RSTR2 ; \ RCC_APB1RSTR2_LPTIM2RST, Low-power timer 2 reset
: RCC_APB1RSTR2_SWPMI1RST ( -- x addr ) 2 bit RCC_APB1RSTR2 ; \ RCC_APB1RSTR2_SWPMI1RST, Single wire protocol reset
: RCC_APB1RSTR2_LPUART1RST ( -- x addr ) 0 bit RCC_APB1RSTR2 ; \ RCC_APB1RSTR2_LPUART1RST, Low-power UART 1 reset
[then]

defined? use-RCC defined? RCC_APB2RSTR_DFSDMRST not and [if]
\ RCC_APB2RSTR (read-write) Reset:0x00000000
: RCC_APB2RSTR_DFSDMRST ( -- x addr ) 24 bit RCC_APB2RSTR ; \ RCC_APB2RSTR_DFSDMRST, Digital filters for sigma-delata  modulators DFSDM reset
: RCC_APB2RSTR_SAI2RST ( -- x addr ) 22 bit RCC_APB2RSTR ; \ RCC_APB2RSTR_SAI2RST, Serial audio interface 2 SAI2  reset
: RCC_APB2RSTR_SAI1RST ( -- x addr ) 21 bit RCC_APB2RSTR ; \ RCC_APB2RSTR_SAI1RST, Serial audio interface 1 SAI1  reset
: RCC_APB2RSTR_TIM17RST ( -- x addr ) 18 bit RCC_APB2RSTR ; \ RCC_APB2RSTR_TIM17RST, TIM17 timer reset
: RCC_APB2RSTR_TIM16RST ( -- x addr ) 17 bit RCC_APB2RSTR ; \ RCC_APB2RSTR_TIM16RST, TIM16 timer reset
: RCC_APB2RSTR_TIM15RST ( -- x addr ) 16 bit RCC_APB2RSTR ; \ RCC_APB2RSTR_TIM15RST, TIM15 timer reset
: RCC_APB2RSTR_USART1RST ( -- x addr ) 14 bit RCC_APB2RSTR ; \ RCC_APB2RSTR_USART1RST, USART1 reset
: RCC_APB2RSTR_TIM8RST ( -- x addr ) 13 bit RCC_APB2RSTR ; \ RCC_APB2RSTR_TIM8RST, TIM8 timer reset
: RCC_APB2RSTR_SPI1RST ( -- x addr ) 12 bit RCC_APB2RSTR ; \ RCC_APB2RSTR_SPI1RST, SPI1 reset
: RCC_APB2RSTR_TIM1RST ( -- x addr ) 11 bit RCC_APB2RSTR ; \ RCC_APB2RSTR_TIM1RST, TIM1 timer reset
: RCC_APB2RSTR_SDMMCRST ( -- x addr ) 10 bit RCC_APB2RSTR ; \ RCC_APB2RSTR_SDMMCRST, SDMMC reset
: RCC_APB2RSTR_SYSCFGRST ( -- x addr ) 0 bit RCC_APB2RSTR ; \ RCC_APB2RSTR_SYSCFGRST, System configuration SYSCFG  reset
[then]

execute-defined? use-RCC defined? RCC_AHB1ENR_TSCEN not and [if]
\ RCC_AHB1ENR (read-write) Reset:0x00000100
: RCC_AHB1ENR_TSCEN ( -- x addr ) 16 bit RCC_AHB1ENR ; \ RCC_AHB1ENR_TSCEN, Touch Sensing Controller clock  enable
: RCC_AHB1ENR_CRCEN ( -- x addr ) 11 bit RCC_AHB1ENR ; \ RCC_AHB1ENR_CRCEN, Reserved
: RCC_AHB1ENR_FLASHEN ( -- x addr ) 8 bit RCC_AHB1ENR ; \ RCC_AHB1ENR_FLASHEN, Flash memory interface clock  enable
: RCC_AHB1ENR_DMA2EN ( -- x addr ) 1 bit RCC_AHB1ENR ; \ RCC_AHB1ENR_DMA2EN, DMA2 clock enable
: RCC_AHB1ENR_DMA1EN ( -- x addr ) 0 bit RCC_AHB1ENR ; \ RCC_AHB1ENR_DMA1EN, DMA1 clock enable
[then]

defined? use-RCC defined? RCC_AHB2ENR_RNGEN not and [if]
\ RCC_AHB2ENR (read-write) Reset:0x00000000
: RCC_AHB2ENR_RNGEN ( -- x addr ) 18 bit RCC_AHB2ENR ; \ RCC_AHB2ENR_RNGEN, Random Number Generator clock  enable
: RCC_AHB2ENR_AESEN ( -- x addr ) 16 bit RCC_AHB2ENR ; \ RCC_AHB2ENR_AESEN, AES accelerator clock  enable
: RCC_AHB2ENR_ADCEN ( -- x addr ) 13 bit RCC_AHB2ENR ; \ RCC_AHB2ENR_ADCEN, ADC clock enable
: RCC_AHB2ENR_OTGFSEN ( -- x addr ) 12 bit RCC_AHB2ENR ; \ RCC_AHB2ENR_OTGFSEN, OTG full speed clock  enable
: RCC_AHB2ENR_GPIOHEN ( -- x addr ) 7 bit RCC_AHB2ENR ; \ RCC_AHB2ENR_GPIOHEN, IO port H clock enable
: RCC_AHB2ENR_GPIOGEN ( -- x addr ) 6 bit RCC_AHB2ENR ; \ RCC_AHB2ENR_GPIOGEN, IO port G clock enable
: RCC_AHB2ENR_GPIOFEN ( -- x addr ) 5 bit RCC_AHB2ENR ; \ RCC_AHB2ENR_GPIOFEN, IO port F clock enable
: RCC_AHB2ENR_GPIOEEN ( -- x addr ) 4 bit RCC_AHB2ENR ; \ RCC_AHB2ENR_GPIOEEN, IO port E clock enable
: RCC_AHB2ENR_GPIODEN ( -- x addr ) 3 bit RCC_AHB2ENR ; \ RCC_AHB2ENR_GPIODEN, IO port D clock enable
: RCC_AHB2ENR_GPIOCEN ( -- x addr ) 2 bit RCC_AHB2ENR ; \ RCC_AHB2ENR_GPIOCEN, IO port C clock enable
: RCC_AHB2ENR_GPIOBEN ( -- x addr ) 1 bit RCC_AHB2ENR ; \ RCC_AHB2ENR_GPIOBEN, IO port B clock enable
: RCC_AHB2ENR_GPIOAEN ( -- x addr ) 0 bit RCC_AHB2ENR ; \ RCC_AHB2ENR_GPIOAEN, IO port A clock enable
[then]

execute-defined? use-RCC defined? RCC_AHB3ENR_QSPIEN not and [if]
\ RCC_AHB3ENR (read-write) Reset:0x00000000
: RCC_AHB3ENR_QSPIEN ( -- x addr ) 8 bit RCC_AHB3ENR ; \ RCC_AHB3ENR_QSPIEN, QSPIEN
: RCC_AHB3ENR_FMCEN ( -- x addr ) 0 bit RCC_AHB3ENR ; \ RCC_AHB3ENR_FMCEN, Flexible memory controller clock  enable
[then]

defined? use-RCC defined? RCC_APB1ENR1_LPTIM1EN not and [if]
\ RCC_APB1ENR1 (read-write) Reset:0x00000000
: RCC_APB1ENR1_LPTIM1EN ( -- x addr ) 31 bit RCC_APB1ENR1 ; \ RCC_APB1ENR1_LPTIM1EN, Low power timer 1 clock  enable
: RCC_APB1ENR1_OPAMPEN ( -- x addr ) 30 bit RCC_APB1ENR1 ; \ RCC_APB1ENR1_OPAMPEN, OPAMP interface clock  enable
: RCC_APB1ENR1_DAC1EN ( -- x addr ) 29 bit RCC_APB1ENR1 ; \ RCC_APB1ENR1_DAC1EN, DAC1 interface clock  enable
: RCC_APB1ENR1_PWREN ( -- x addr ) 28 bit RCC_APB1ENR1 ; \ RCC_APB1ENR1_PWREN, Power interface clock  enable
: RCC_APB1ENR1_CAN1EN ( -- x addr ) 25 bit RCC_APB1ENR1 ; \ RCC_APB1ENR1_CAN1EN, CAN1 clock enable
: RCC_APB1ENR1_I2C3EN ( -- x addr ) 23 bit RCC_APB1ENR1 ; \ RCC_APB1ENR1_I2C3EN, I2C3 clock enable
: RCC_APB1ENR1_I2C2EN ( -- x addr ) 22 bit RCC_APB1ENR1 ; \ RCC_APB1ENR1_I2C2EN, I2C2 clock enable
: RCC_APB1ENR1_I2C1EN ( -- x addr ) 21 bit RCC_APB1ENR1 ; \ RCC_APB1ENR1_I2C1EN, I2C1 clock enable
: RCC_APB1ENR1_UART5EN ( -- x addr ) 20 bit RCC_APB1ENR1 ; \ RCC_APB1ENR1_UART5EN, UART5 clock enable
: RCC_APB1ENR1_UART4EN ( -- x addr ) 19 bit RCC_APB1ENR1 ; \ RCC_APB1ENR1_UART4EN, UART4 clock enable
: RCC_APB1ENR1_USART3EN ( -- x addr ) 18 bit RCC_APB1ENR1 ; \ RCC_APB1ENR1_USART3EN, USART3 clock enable
: RCC_APB1ENR1_USART2EN ( -- x addr ) 17 bit RCC_APB1ENR1 ; \ RCC_APB1ENR1_USART2EN, USART2 clock enable
: RCC_APB1ENR1_SP3EN ( -- x addr ) 15 bit RCC_APB1ENR1 ; \ RCC_APB1ENR1_SP3EN, SPI3 clock enable
: RCC_APB1ENR1_SPI2EN ( -- x addr ) 14 bit RCC_APB1ENR1 ; \ RCC_APB1ENR1_SPI2EN, SPI2 clock enable
: RCC_APB1ENR1_WWDGEN ( -- x addr ) 11 bit RCC_APB1ENR1 ; \ RCC_APB1ENR1_WWDGEN, Window watchdog clock  enable
: RCC_APB1ENR1_LCDEN ( -- x addr ) 9 bit RCC_APB1ENR1 ; \ RCC_APB1ENR1_LCDEN, LCD clock enable
: RCC_APB1ENR1_TIM7EN ( -- x addr ) 5 bit RCC_APB1ENR1 ; \ RCC_APB1ENR1_TIM7EN, TIM7 timer clock enable
: RCC_APB1ENR1_TIM6EN ( -- x addr ) 4 bit RCC_APB1ENR1 ; \ RCC_APB1ENR1_TIM6EN, TIM6 timer clock enable
: RCC_APB1ENR1_TIM5EN ( -- x addr ) 3 bit RCC_APB1ENR1 ; \ RCC_APB1ENR1_TIM5EN, Reserved
: RCC_APB1ENR1_TIM4EN ( -- x addr ) 2 bit RCC_APB1ENR1 ; \ RCC_APB1ENR1_TIM4EN, TIM4 timer clock enable
: RCC_APB1ENR1_TIM3EN ( -- x addr ) 1 bit RCC_APB1ENR1 ; \ RCC_APB1ENR1_TIM3EN, TIM3 timer clock enable
: RCC_APB1ENR1_TIM2EN ( -- x addr ) 0 bit RCC_APB1ENR1 ; \ RCC_APB1ENR1_TIM2EN, TIM2 timer clock enable
[then]

execute-defined? use-RCC defined? RCC_APB1ENR2_LPTIM2EN not and [if]
\ RCC_APB1ENR2 (read-write) Reset:0x00000000
: RCC_APB1ENR2_LPTIM2EN ( -- x addr ) 5 bit RCC_APB1ENR2 ; \ RCC_APB1ENR2_LPTIM2EN, LPTIM2EN
: RCC_APB1ENR2_SWPMI1EN ( -- x addr ) 2 bit RCC_APB1ENR2 ; \ RCC_APB1ENR2_SWPMI1EN, Single wire protocol clock  enable
: RCC_APB1ENR2_LPUART1EN ( -- x addr ) 0 bit RCC_APB1ENR2 ; \ RCC_APB1ENR2_LPUART1EN, Low power UART 1 clock  enable
[then]

defined? use-RCC defined? RCC_APB2ENR_DFSDMEN not and [if]
\ RCC_APB2ENR (read-write) Reset:0x00000000
: RCC_APB2ENR_DFSDMEN ( -- x addr ) 24 bit RCC_APB2ENR ; \ RCC_APB2ENR_DFSDMEN, DFSDM timer clock enable
: RCC_APB2ENR_SAI2EN ( -- x addr ) 22 bit RCC_APB2ENR ; \ RCC_APB2ENR_SAI2EN, SAI2 clock enable
: RCC_APB2ENR_SAI1EN ( -- x addr ) 21 bit RCC_APB2ENR ; \ RCC_APB2ENR_SAI1EN, SAI1 clock enable
: RCC_APB2ENR_TIM17EN ( -- x addr ) 18 bit RCC_APB2ENR ; \ RCC_APB2ENR_TIM17EN, TIM17 timer clock enable
: RCC_APB2ENR_TIM16EN ( -- x addr ) 17 bit RCC_APB2ENR ; \ RCC_APB2ENR_TIM16EN, TIM16 timer clock enable
: RCC_APB2ENR_TIM15EN ( -- x addr ) 16 bit RCC_APB2ENR ; \ RCC_APB2ENR_TIM15EN, TIM15 timer clock enable
: RCC_APB2ENR_USART1EN ( -- x addr ) 14 bit RCC_APB2ENR ; \ RCC_APB2ENR_USART1EN, USART1clock enable
: RCC_APB2ENR_TIM8EN ( -- x addr ) 13 bit RCC_APB2ENR ; \ RCC_APB2ENR_TIM8EN, TIM8 timer clock enable
: RCC_APB2ENR_SPI1EN ( -- x addr ) 12 bit RCC_APB2ENR ; \ RCC_APB2ENR_SPI1EN, SPI1 clock enable
: RCC_APB2ENR_TIM1EN ( -- x addr ) 11 bit RCC_APB2ENR ; \ RCC_APB2ENR_TIM1EN, TIM1 timer clock enable
: RCC_APB2ENR_SDMMCEN ( -- x addr ) 10 bit RCC_APB2ENR ; \ RCC_APB2ENR_SDMMCEN, SDMMC clock enable
: RCC_APB2ENR_FIREWALLEN ( -- x addr ) 7 bit RCC_APB2ENR ; \ RCC_APB2ENR_FIREWALLEN, Firewall clock enable
: RCC_APB2ENR_SYSCFGEN ( -- x addr ) 0 bit RCC_APB2ENR ; \ RCC_APB2ENR_SYSCFGEN, SYSCFG clock enable
[then]

execute-defined? use-RCC defined? RCC_AHB1SMENR_TSCSMEN not and [if]
\ RCC_AHB1SMENR (read-write) Reset:0x00011303
: RCC_AHB1SMENR_TSCSMEN ( -- x addr ) 16 bit RCC_AHB1SMENR ; \ RCC_AHB1SMENR_TSCSMEN, Touch Sensing Controller clocks enable  during Sleep and Stop modes
: RCC_AHB1SMENR_CRCSMEN ( -- x addr ) 11 bit RCC_AHB1SMENR ; \ RCC_AHB1SMENR_CRCSMEN, CRCSMEN
: RCC_AHB1SMENR_SRAM1SMEN ( -- x addr ) 9 bit RCC_AHB1SMENR ; \ RCC_AHB1SMENR_SRAM1SMEN, SRAM1 interface clocks enable during  Sleep and Stop modes
: RCC_AHB1SMENR_FLASHSMEN ( -- x addr ) 8 bit RCC_AHB1SMENR ; \ RCC_AHB1SMENR_FLASHSMEN, Flash memory interface clocks enable  during Sleep and Stop modes
: RCC_AHB1SMENR_DMA2SMEN ( -- x addr ) 1 bit RCC_AHB1SMENR ; \ RCC_AHB1SMENR_DMA2SMEN, DMA2 clocks enable during Sleep and Stop  modes
: RCC_AHB1SMENR_DMA1SMEN ( -- x addr ) 0 bit RCC_AHB1SMENR ; \ RCC_AHB1SMENR_DMA1SMEN, DMA1 clocks enable during Sleep and Stop  modes
[then]

defined? use-RCC defined? RCC_AHB2SMENR_RNGSMEN not and [if]
\ RCC_AHB2SMENR (read-write) Reset:0x000532FF
: RCC_AHB2SMENR_RNGSMEN ( -- x addr ) 18 bit RCC_AHB2SMENR ; \ RCC_AHB2SMENR_RNGSMEN, Random Number Generator clocks enable  during Sleep and Stop modes
: RCC_AHB2SMENR_AESSMEN ( -- x addr ) 16 bit RCC_AHB2SMENR ; \ RCC_AHB2SMENR_AESSMEN, AES accelerator clocks enable during  Sleep and Stop modes
: RCC_AHB2SMENR_ADCFSSMEN ( -- x addr ) 13 bit RCC_AHB2SMENR ; \ RCC_AHB2SMENR_ADCFSSMEN, ADC clocks enable during Sleep and Stop  modes
: RCC_AHB2SMENR_OTGFSSMEN ( -- x addr ) 12 bit RCC_AHB2SMENR ; \ RCC_AHB2SMENR_OTGFSSMEN, OTG full speed clocks enable during  Sleep and Stop modes
: RCC_AHB2SMENR_SRAM2SMEN ( -- x addr ) 9 bit RCC_AHB2SMENR ; \ RCC_AHB2SMENR_SRAM2SMEN, SRAM2 interface clocks enable during  Sleep and Stop modes
: RCC_AHB2SMENR_GPIOHSMEN ( -- x addr ) 7 bit RCC_AHB2SMENR ; \ RCC_AHB2SMENR_GPIOHSMEN, IO port H clocks enable during Sleep and  Stop modes
: RCC_AHB2SMENR_GPIOGSMEN ( -- x addr ) 6 bit RCC_AHB2SMENR ; \ RCC_AHB2SMENR_GPIOGSMEN, IO port G clocks enable during Sleep and  Stop modes
: RCC_AHB2SMENR_GPIOFSMEN ( -- x addr ) 5 bit RCC_AHB2SMENR ; \ RCC_AHB2SMENR_GPIOFSMEN, IO port F clocks enable during Sleep and  Stop modes
: RCC_AHB2SMENR_GPIOESMEN ( -- x addr ) 4 bit RCC_AHB2SMENR ; \ RCC_AHB2SMENR_GPIOESMEN, IO port E clocks enable during Sleep and  Stop modes
: RCC_AHB2SMENR_GPIODSMEN ( -- x addr ) 3 bit RCC_AHB2SMENR ; \ RCC_AHB2SMENR_GPIODSMEN, IO port D clocks enable during Sleep and  Stop modes
: RCC_AHB2SMENR_GPIOCSMEN ( -- x addr ) 2 bit RCC_AHB2SMENR ; \ RCC_AHB2SMENR_GPIOCSMEN, IO port C clocks enable during Sleep and  Stop modes
: RCC_AHB2SMENR_GPIOBSMEN ( -- x addr ) 1 bit RCC_AHB2SMENR ; \ RCC_AHB2SMENR_GPIOBSMEN, IO port B clocks enable during Sleep and  Stop modes
: RCC_AHB2SMENR_GPIOASMEN ( -- x addr ) 0 bit RCC_AHB2SMENR ; \ RCC_AHB2SMENR_GPIOASMEN, IO port A clocks enable during Sleep and  Stop modes
[then]

execute-defined? use-RCC defined? RCC_AHB3SMENR_QSPISMEN not and [if]
\ RCC_AHB3SMENR (read-write) Reset:0x000000101
: RCC_AHB3SMENR_QSPISMEN ( -- x addr ) 8 bit RCC_AHB3SMENR ; \ RCC_AHB3SMENR_QSPISMEN, QSPISMEN
: RCC_AHB3SMENR_FMCSMEN ( -- x addr ) 0 bit RCC_AHB3SMENR ; \ RCC_AHB3SMENR_FMCSMEN, Flexible memory controller clocks enable  during Sleep and Stop modes
[then]

defined? use-RCC defined? RCC_APB1SMENR1_LPTIM1SMEN not and [if]
\ RCC_APB1SMENR1 (read-write) Reset:0xF2FECA3F
: RCC_APB1SMENR1_LPTIM1SMEN ( -- x addr ) 31 bit RCC_APB1SMENR1 ; \ RCC_APB1SMENR1_LPTIM1SMEN, Low power timer 1 clocks enable during  Sleep and Stop modes
: RCC_APB1SMENR1_OPAMPSMEN ( -- x addr ) 30 bit RCC_APB1SMENR1 ; \ RCC_APB1SMENR1_OPAMPSMEN, OPAMP interface clocks enable during  Sleep and Stop modes
: RCC_APB1SMENR1_DAC1SMEN ( -- x addr ) 29 bit RCC_APB1SMENR1 ; \ RCC_APB1SMENR1_DAC1SMEN, DAC1 interface clocks enable during  Sleep and Stop modes
: RCC_APB1SMENR1_PWRSMEN ( -- x addr ) 28 bit RCC_APB1SMENR1 ; \ RCC_APB1SMENR1_PWRSMEN, Power interface clocks enable during  Sleep and Stop modes
: RCC_APB1SMENR1_CAN1SMEN ( -- x addr ) 25 bit RCC_APB1SMENR1 ; \ RCC_APB1SMENR1_CAN1SMEN, CAN1 clocks enable during Sleep and Stop  modes
: RCC_APB1SMENR1_I2C3SMEN ( -- x addr ) 23 bit RCC_APB1SMENR1 ; \ RCC_APB1SMENR1_I2C3SMEN, I2C3 clocks enable during Sleep and Stop  modes
: RCC_APB1SMENR1_I2C2SMEN ( -- x addr ) 22 bit RCC_APB1SMENR1 ; \ RCC_APB1SMENR1_I2C2SMEN, I2C2 clocks enable during Sleep and Stop  modes
: RCC_APB1SMENR1_I2C1SMEN ( -- x addr ) 21 bit RCC_APB1SMENR1 ; \ RCC_APB1SMENR1_I2C1SMEN, I2C1 clocks enable during Sleep and Stop  modes
: RCC_APB1SMENR1_UART5SMEN ( -- x addr ) 20 bit RCC_APB1SMENR1 ; \ RCC_APB1SMENR1_UART5SMEN, UART5 clocks enable during Sleep and  Stop modes
: RCC_APB1SMENR1_UART4SMEN ( -- x addr ) 19 bit RCC_APB1SMENR1 ; \ RCC_APB1SMENR1_UART4SMEN, UART4 clocks enable during Sleep and  Stop modes
: RCC_APB1SMENR1_USART3SMEN ( -- x addr ) 18 bit RCC_APB1SMENR1 ; \ RCC_APB1SMENR1_USART3SMEN, USART3 clocks enable during Sleep and  Stop modes
: RCC_APB1SMENR1_USART2SMEN ( -- x addr ) 17 bit RCC_APB1SMENR1 ; \ RCC_APB1SMENR1_USART2SMEN, USART2 clocks enable during Sleep and  Stop modes
: RCC_APB1SMENR1_SP3SMEN ( -- x addr ) 15 bit RCC_APB1SMENR1 ; \ RCC_APB1SMENR1_SP3SMEN, SPI3 clocks enable during Sleep and Stop  modes
: RCC_APB1SMENR1_SPI2SMEN ( -- x addr ) 14 bit RCC_APB1SMENR1 ; \ RCC_APB1SMENR1_SPI2SMEN, SPI2 clocks enable during Sleep and Stop  modes
: RCC_APB1SMENR1_WWDGSMEN ( -- x addr ) 11 bit RCC_APB1SMENR1 ; \ RCC_APB1SMENR1_WWDGSMEN, Window watchdog clocks enable during  Sleep and Stop modes
: RCC_APB1SMENR1_LCDSMEN ( -- x addr ) 9 bit RCC_APB1SMENR1 ; \ RCC_APB1SMENR1_LCDSMEN, LCD clocks enable during Sleep and Stop  modes
: RCC_APB1SMENR1_TIM7SMEN ( -- x addr ) 5 bit RCC_APB1SMENR1 ; \ RCC_APB1SMENR1_TIM7SMEN, TIM7 timer clocks enable during Sleep  and Stop modes
: RCC_APB1SMENR1_TIM6SMEN ( -- x addr ) 4 bit RCC_APB1SMENR1 ; \ RCC_APB1SMENR1_TIM6SMEN, TIM6 timer clocks enable during Sleep  and Stop modes
: RCC_APB1SMENR1_TIM5SMEN ( -- x addr ) 3 bit RCC_APB1SMENR1 ; \ RCC_APB1SMENR1_TIM5SMEN, Reserved
: RCC_APB1SMENR1_TIM4SMEN ( -- x addr ) 2 bit RCC_APB1SMENR1 ; \ RCC_APB1SMENR1_TIM4SMEN, TIM4 timer clocks enable during Sleep  and Stop modes
: RCC_APB1SMENR1_TIM3SMEN ( -- x addr ) 1 bit RCC_APB1SMENR1 ; \ RCC_APB1SMENR1_TIM3SMEN, TIM3 timer clocks enable during Sleep  and Stop modes
: RCC_APB1SMENR1_TIM2SMEN ( -- x addr ) 0 bit RCC_APB1SMENR1 ; \ RCC_APB1SMENR1_TIM2SMEN, TIM2 timer clocks enable during Sleep  and Stop modes
[then]

execute-defined? use-RCC defined? RCC_APB1SMENR2_LPTIM2SMEN not and [if]
\ RCC_APB1SMENR2 (read-write) Reset:0x000000025
: RCC_APB1SMENR2_LPTIM2SMEN ( -- x addr ) 5 bit RCC_APB1SMENR2 ; \ RCC_APB1SMENR2_LPTIM2SMEN, LPTIM2SMEN
: RCC_APB1SMENR2_SWPMI1SMEN ( -- x addr ) 2 bit RCC_APB1SMENR2 ; \ RCC_APB1SMENR2_SWPMI1SMEN, Single wire protocol clocks enable  during Sleep and Stop modes
: RCC_APB1SMENR2_LPUART1SMEN ( -- x addr ) 0 bit RCC_APB1SMENR2 ; \ RCC_APB1SMENR2_LPUART1SMEN, Low power UART 1 clocks enable during  Sleep and Stop modes
[then]

defined? use-RCC defined? RCC_APB2SMENR_DFSDMSMEN not and [if]
\ RCC_APB2SMENR (read-write) Reset:0x01677C01
: RCC_APB2SMENR_DFSDMSMEN ( -- x addr ) 24 bit RCC_APB2SMENR ; \ RCC_APB2SMENR_DFSDMSMEN, DFSDM timer clocks enable during Sleep  and Stop modes
: RCC_APB2SMENR_SAI2SMEN ( -- x addr ) 22 bit RCC_APB2SMENR ; \ RCC_APB2SMENR_SAI2SMEN, SAI2 clocks enable during Sleep and Stop  modes
: RCC_APB2SMENR_SAI1SMEN ( -- x addr ) 21 bit RCC_APB2SMENR ; \ RCC_APB2SMENR_SAI1SMEN, SAI1 clocks enable during Sleep and Stop  modes
: RCC_APB2SMENR_TIM17SMEN ( -- x addr ) 18 bit RCC_APB2SMENR ; \ RCC_APB2SMENR_TIM17SMEN, TIM17 timer clocks enable during Sleep  and Stop modes
: RCC_APB2SMENR_TIM16SMEN ( -- x addr ) 17 bit RCC_APB2SMENR ; \ RCC_APB2SMENR_TIM16SMEN, TIM16 timer clocks enable during Sleep  and Stop modes
: RCC_APB2SMENR_TIM15SMEN ( -- x addr ) 16 bit RCC_APB2SMENR ; \ RCC_APB2SMENR_TIM15SMEN, TIM15 timer clocks enable during Sleep  and Stop modes
: RCC_APB2SMENR_USART1SMEN ( -- x addr ) 14 bit RCC_APB2SMENR ; \ RCC_APB2SMENR_USART1SMEN, USART1clocks enable during Sleep and  Stop modes
: RCC_APB2SMENR_TIM8SMEN ( -- x addr ) 13 bit RCC_APB2SMENR ; \ RCC_APB2SMENR_TIM8SMEN, TIM8 timer clocks enable during Sleep  and Stop modes
: RCC_APB2SMENR_SPI1SMEN ( -- x addr ) 12 bit RCC_APB2SMENR ; \ RCC_APB2SMENR_SPI1SMEN, SPI1 clocks enable during Sleep and Stop  modes
: RCC_APB2SMENR_TIM1SMEN ( -- x addr ) 11 bit RCC_APB2SMENR ; \ RCC_APB2SMENR_TIM1SMEN, TIM1 timer clocks enable during Sleep  and Stop modes
: RCC_APB2SMENR_SDMMCSMEN ( -- x addr ) 10 bit RCC_APB2SMENR ; \ RCC_APB2SMENR_SDMMCSMEN, SDMMC clocks enable during Sleep and  Stop modes
: RCC_APB2SMENR_SYSCFGSMEN ( -- x addr ) 0 bit RCC_APB2SMENR ; \ RCC_APB2SMENR_SYSCFGSMEN, SYSCFG clocks enable during Sleep and  Stop modes
[then]

execute-defined? use-RCC defined? RCC_CCIPR_DFSDMSEL not and [if]
\ RCC_CCIPR (read-write) Reset:0x00000000
: RCC_CCIPR_DFSDMSEL ( -- x addr ) 31 bit RCC_CCIPR ; \ RCC_CCIPR_DFSDMSEL, DFSDM clock source  selection
: RCC_CCIPR_SWPMI1SEL ( -- x addr ) 30 bit RCC_CCIPR ; \ RCC_CCIPR_SWPMI1SEL, SWPMI1 clock source  selection
: RCC_CCIPR_ADCSEL ( %bb -- x addr ) 28 lshift RCC_CCIPR ; \ RCC_CCIPR_ADCSEL, ADCs clock source  selection
: RCC_CCIPR_CLK48SEL ( %bb -- x addr ) 26 lshift RCC_CCIPR ; \ RCC_CCIPR_CLK48SEL, 48 MHz clock source  selection
: RCC_CCIPR_SAI2SEL ( %bb -- x addr ) 24 lshift RCC_CCIPR ; \ RCC_CCIPR_SAI2SEL, SAI2 clock source  selection
: RCC_CCIPR_SAI1SEL ( %bb -- x addr ) 22 lshift RCC_CCIPR ; \ RCC_CCIPR_SAI1SEL, SAI1 clock source  selection
: RCC_CCIPR_LPTIM2SEL ( %bb -- x addr ) 20 lshift RCC_CCIPR ; \ RCC_CCIPR_LPTIM2SEL, Low power timer 2 clock source  selection
: RCC_CCIPR_LPTIM1SEL ( %bb -- x addr ) 18 lshift RCC_CCIPR ; \ RCC_CCIPR_LPTIM1SEL, Low power timer 1 clock source  selection
: RCC_CCIPR_I2C3SEL ( %bb -- x addr ) 16 lshift RCC_CCIPR ; \ RCC_CCIPR_I2C3SEL, I2C3 clock source  selection
: RCC_CCIPR_I2C2SEL ( %bb -- x addr ) 14 lshift RCC_CCIPR ; \ RCC_CCIPR_I2C2SEL, I2C2 clock source  selection
: RCC_CCIPR_I2C1SEL ( %bb -- x addr ) 12 lshift RCC_CCIPR ; \ RCC_CCIPR_I2C1SEL, I2C1 clock source  selection
: RCC_CCIPR_LPUART1SEL ( %bb -- x addr ) 10 lshift RCC_CCIPR ; \ RCC_CCIPR_LPUART1SEL, LPUART1 clock source  selection
: RCC_CCIPR_UART5SEL ( %bb -- x addr ) 8 lshift RCC_CCIPR ; \ RCC_CCIPR_UART5SEL, UART5 clock source  selection
: RCC_CCIPR_UART4SEL ( %bb -- x addr ) 6 lshift RCC_CCIPR ; \ RCC_CCIPR_UART4SEL, UART4 clock source  selection
: RCC_CCIPR_USART3SEL ( %bb -- x addr ) 4 lshift RCC_CCIPR ; \ RCC_CCIPR_USART3SEL, USART3 clock source  selection
: RCC_CCIPR_USART2SEL ( %bb -- x addr ) 2 lshift RCC_CCIPR ; \ RCC_CCIPR_USART2SEL, USART2 clock source  selection
: RCC_CCIPR_USART1SEL ( %bb -- x addr ) RCC_CCIPR ; \ RCC_CCIPR_USART1SEL, USART1 clock source  selection
[then]

defined? use-RCC defined? RCC_BDCR_LSCOSEL not and [if]
\ RCC_BDCR (multiple-access)  Reset:0x00000000
: RCC_BDCR_LSCOSEL ( -- x addr ) 25 bit RCC_BDCR ; \ RCC_BDCR_LSCOSEL, Low speed clock output  selection
: RCC_BDCR_LSCOEN ( -- x addr ) 24 bit RCC_BDCR ; \ RCC_BDCR_LSCOEN, Low speed clock output  enable
: RCC_BDCR_BDRST ( -- x addr ) 16 bit RCC_BDCR ; \ RCC_BDCR_BDRST, Backup domain software  reset
: RCC_BDCR_RTCEN ( -- x addr ) 15 bit RCC_BDCR ; \ RCC_BDCR_RTCEN, RTC clock enable
: RCC_BDCR_RTCSEL ( %bb -- x addr ) 8 lshift RCC_BDCR ; \ RCC_BDCR_RTCSEL, RTC clock source selection
: RCC_BDCR_LSECSSD ( -- x addr ) 6 bit RCC_BDCR ; \ RCC_BDCR_LSECSSD, LSECSSD
: RCC_BDCR_LSECSSON ( -- x addr ) 5 bit RCC_BDCR ; \ RCC_BDCR_LSECSSON, LSECSSON
: RCC_BDCR_LSEDRV ( %bb -- x addr ) 3 lshift RCC_BDCR ; \ RCC_BDCR_LSEDRV, SE oscillator drive  capability
: RCC_BDCR_LSEBYP ( -- x addr ) 2 bit RCC_BDCR ; \ RCC_BDCR_LSEBYP, LSE oscillator bypass
: RCC_BDCR_LSERDY ( -- x addr ) 1 bit RCC_BDCR ; \ RCC_BDCR_LSERDY, LSE oscillator ready
: RCC_BDCR_LSEON ( -- x addr ) 0 bit RCC_BDCR ; \ RCC_BDCR_LSEON, LSE oscillator enable
[then]

execute-defined? use-RCC defined? RCC_CSR_LPWRSTF? not and [if]
\ RCC_CSR (multiple-access)  Reset:0x0C000600
: RCC_CSR_LPWRSTF? ( -- 1|0 ) 31 bit RCC_CSR bit@ ; \ RCC_CSR_LPWRSTF, Low-power reset flag
: RCC_CSR_WWDGRSTF? ( -- 1|0 ) 30 bit RCC_CSR bit@ ; \ RCC_CSR_WWDGRSTF, Window watchdog reset flag
: RCC_CSR_IWDGRSTF? ( -- 1|0 ) 29 bit RCC_CSR bit@ ; \ RCC_CSR_IWDGRSTF, Independent window watchdog reset  flag
: RCC_CSR_SFTRSTF? ( -- 1|0 ) 28 bit RCC_CSR bit@ ; \ RCC_CSR_SFTRSTF, Software reset flag
: RCC_CSR_BORRSTF? ( -- 1|0 ) 27 bit RCC_CSR bit@ ; \ RCC_CSR_BORRSTF, BOR flag
: RCC_CSR_PINRSTF? ( -- 1|0 ) 26 bit RCC_CSR bit@ ; \ RCC_CSR_PINRSTF, Pin reset flag
: RCC_CSR_OBLRSTF? ( -- 1|0 ) 25 bit RCC_CSR bit@ ; \ RCC_CSR_OBLRSTF, Option byte loader reset  flag
: RCC_CSR_FIREWALLRSTF? ( -- 1|0 ) 24 bit RCC_CSR bit@ ; \ RCC_CSR_FIREWALLRSTF, Firewall reset flag
: RCC_CSR_RMVF? ( -- 1|0 ) 23 bit RCC_CSR bit@ ; \ RCC_CSR_RMVF, Remove reset flag
: RCC_CSR_MSISRANGE ( %bbbb -- x addr ) 8 lshift RCC_CSR ; \ RCC_CSR_MSISRANGE, SI range after Standby  mode
: RCC_CSR_LSIRDY ( -- x addr ) 1 bit RCC_CSR ; \ RCC_CSR_LSIRDY, LSI oscillator ready
: RCC_CSR_LSION ( -- x addr ) 0 bit RCC_CSR ; \ RCC_CSR_LSION, LSI oscillator enable
[then]

defined? use-PWR defined? PWR_CR1_LPR not and [if]
\ PWR_CR1 (read-write) Reset:0x00000200
: PWR_CR1_LPR ( -- x addr ) 14 bit PWR_CR1 ; \ PWR_CR1_LPR, Low-power run
: PWR_CR1_VOS ( %bb -- x addr ) 9 lshift PWR_CR1 ; \ PWR_CR1_VOS, Voltage scaling range  selection
: PWR_CR1_DBP ( -- x addr ) 8 bit PWR_CR1 ; \ PWR_CR1_DBP, Disable backup domain write  protection
: PWR_CR1_LPMS ( %bbb -- x addr ) PWR_CR1 ; \ PWR_CR1_LPMS, Low-power mode selection
[then]

execute-defined? use-PWR defined? PWR_CR2_USV not and [if]
\ PWR_CR2 (read-write) Reset:0x00000000
: PWR_CR2_USV ( -- x addr ) 10 bit PWR_CR2 ; \ PWR_CR2_USV, VDDUSB USB supply valid
: PWR_CR2_IOSV ( -- x addr ) 9 bit PWR_CR2 ; \ PWR_CR2_IOSV, VDDIO2 Independent I/Os supply  valid
: PWR_CR2_PVME4 ( -- x addr ) 7 bit PWR_CR2 ; \ PWR_CR2_PVME4, Peripheral voltage monitoring 4 enable:  VDDA vs. 2.2V
: PWR_CR2_PVME3 ( -- x addr ) 6 bit PWR_CR2 ; \ PWR_CR2_PVME3, Peripheral voltage monitoring 3 enable:  VDDA vs. 1.62V
: PWR_CR2_PVME2 ( -- x addr ) 5 bit PWR_CR2 ; \ PWR_CR2_PVME2, Peripheral voltage monitoring 2 enable:  VDDIO2 vs. 0.9V
: PWR_CR2_PVME1 ( -- x addr ) 4 bit PWR_CR2 ; \ PWR_CR2_PVME1, Peripheral voltage monitoring 1 enable:  VDDUSB vs. 1.2V
: PWR_CR2_PLS ( %bbb -- x addr ) 1 lshift PWR_CR2 ; \ PWR_CR2_PLS, Power voltage detector level  selection
: PWR_CR2_PVDE ( -- x addr ) 0 bit PWR_CR2 ; \ PWR_CR2_PVDE, Power voltage detector  enable
[then]

defined? use-PWR defined? PWR_CR3_EWF not and [if]
\ PWR_CR3 (read-write) Reset:0X00008000
: PWR_CR3_EWF ( -- x addr ) 15 bit PWR_CR3 ; \ PWR_CR3_EWF, Enable internal wakeup  line
: PWR_CR3_APC ( -- x addr ) 10 bit PWR_CR3 ; \ PWR_CR3_APC, Apply pull-up and pull-down  configuration
: PWR_CR3_RRS ( -- x addr ) 8 bit PWR_CR3 ; \ PWR_CR3_RRS, SRAM2 retention in Standby  mode
: PWR_CR3_EWUP5 ( -- x addr ) 4 bit PWR_CR3 ; \ PWR_CR3_EWUP5, Enable Wakeup pin WKUP5
: PWR_CR3_EWUP4 ( -- x addr ) 3 bit PWR_CR3 ; \ PWR_CR3_EWUP4, Enable Wakeup pin WKUP4
: PWR_CR3_EWUP3 ( -- x addr ) 2 bit PWR_CR3 ; \ PWR_CR3_EWUP3, Enable Wakeup pin WKUP3
: PWR_CR3_EWUP2 ( -- x addr ) 1 bit PWR_CR3 ; \ PWR_CR3_EWUP2, Enable Wakeup pin WKUP2
: PWR_CR3_EWUP1 ( -- x addr ) 0 bit PWR_CR3 ; \ PWR_CR3_EWUP1, Enable Wakeup pin WKUP1
[then]

execute-defined? use-PWR defined? PWR_CR4_VBRS not and [if]
\ PWR_CR4 (read-write) Reset:0x00000000
: PWR_CR4_VBRS ( -- x addr ) 9 bit PWR_CR4 ; \ PWR_CR4_VBRS, VBAT battery charging resistor  selection
: PWR_CR4_VBE ( -- x addr ) 8 bit PWR_CR4 ; \ PWR_CR4_VBE, VBAT battery charging  enable
: PWR_CR4_WP5 ( -- x addr ) 4 bit PWR_CR4 ; \ PWR_CR4_WP5, Wakeup pin WKUP5 polarity
: PWR_CR4_WP4 ( -- x addr ) 3 bit PWR_CR4 ; \ PWR_CR4_WP4, Wakeup pin WKUP4 polarity
: PWR_CR4_WP3 ( -- x addr ) 2 bit PWR_CR4 ; \ PWR_CR4_WP3, Wakeup pin WKUP3 polarity
: PWR_CR4_WP2 ( -- x addr ) 1 bit PWR_CR4 ; \ PWR_CR4_WP2, Wakeup pin WKUP2 polarity
: PWR_CR4_WP1 ( -- x addr ) 0 bit PWR_CR4 ; \ PWR_CR4_WP1, Wakeup pin WKUP1 polarity
[then]

defined? use-PWR defined? PWR_SR1_WUFI? not and [if]
\ PWR_SR1 (read-only) Reset:0x00000000
: PWR_SR1_WUFI? ( --  1|0 ) 15 bit PWR_SR1 bit@ ; \ PWR_SR1_WUFI, Wakeup flag internal
: PWR_SR1_CSBF? ( --  1|0 ) 8 bit PWR_SR1 bit@ ; \ PWR_SR1_CSBF, Standby flag
: PWR_SR1_CWUF5? ( --  1|0 ) 4 bit PWR_SR1 bit@ ; \ PWR_SR1_CWUF5, Wakeup flag 5
: PWR_SR1_CWUF4? ( --  1|0 ) 3 bit PWR_SR1 bit@ ; \ PWR_SR1_CWUF4, Wakeup flag 4
: PWR_SR1_CWUF3? ( --  1|0 ) 2 bit PWR_SR1 bit@ ; \ PWR_SR1_CWUF3, Wakeup flag 3
: PWR_SR1_CWUF2? ( --  1|0 ) 1 bit PWR_SR1 bit@ ; \ PWR_SR1_CWUF2, Wakeup flag 2
: PWR_SR1_CWUF1? ( --  1|0 ) 0 bit PWR_SR1 bit@ ; \ PWR_SR1_CWUF1, Wakeup flag 1
[then]

execute-defined? use-PWR defined? PWR_SR2_PVMO4? not and [if]
\ PWR_SR2 (read-only) Reset:0x00000000
: PWR_SR2_PVMO4? ( --  1|0 ) 15 bit PWR_SR2 bit@ ; \ PWR_SR2_PVMO4, Peripheral voltage monitoring output:  VDDA vs. 2.2 V
: PWR_SR2_PVMO3? ( --  1|0 ) 14 bit PWR_SR2 bit@ ; \ PWR_SR2_PVMO3, Peripheral voltage monitoring output:  VDDA vs. 1.62 V
: PWR_SR2_PVMO2? ( --  1|0 ) 13 bit PWR_SR2 bit@ ; \ PWR_SR2_PVMO2, Peripheral voltage monitoring output:  VDDIO2 vs. 0.9 V
: PWR_SR2_PVMO1? ( --  1|0 ) 12 bit PWR_SR2 bit@ ; \ PWR_SR2_PVMO1, Peripheral voltage monitoring output:  VDDUSB vs. 1.2 V
: PWR_SR2_PVDO? ( --  1|0 ) 11 bit PWR_SR2 bit@ ; \ PWR_SR2_PVDO, Power voltage detector  output
: PWR_SR2_VOSF? ( --  1|0 ) 10 bit PWR_SR2 bit@ ; \ PWR_SR2_VOSF, Voltage scaling flag
: PWR_SR2_REGLPF? ( --  1|0 ) 9 bit PWR_SR2 bit@ ; \ PWR_SR2_REGLPF, Low-power regulator flag
: PWR_SR2_REGLPS? ( --  1|0 ) 8 bit PWR_SR2 bit@ ; \ PWR_SR2_REGLPS, Low-power regulator  started
[then]

defined? use-PWR defined? PWR_SCR_SBF not and [if]
\ PWR_SCR (write-only) Reset:0x00000000
: PWR_SCR_SBF ( -- x addr ) 8 bit PWR_SCR ; \ PWR_SCR_SBF, Clear standby flag
: PWR_SCR_WUF5 ( -- x addr ) 4 bit PWR_SCR ; \ PWR_SCR_WUF5, Clear wakeup flag 5
: PWR_SCR_WUF4 ( -- x addr ) 3 bit PWR_SCR ; \ PWR_SCR_WUF4, Clear wakeup flag 4
: PWR_SCR_WUF3 ( -- x addr ) 2 bit PWR_SCR ; \ PWR_SCR_WUF3, Clear wakeup flag 3
: PWR_SCR_WUF2 ( -- x addr ) 1 bit PWR_SCR ; \ PWR_SCR_WUF2, Clear wakeup flag 2
: PWR_SCR_WUF1 ( -- x addr ) 0 bit PWR_SCR ; \ PWR_SCR_WUF1, Clear wakeup flag 1
[then]

execute-defined? use-PWR defined? PWR_PUCRA_PU15 not and [if]
\ PWR_PUCRA (read-write) Reset:0x00000000
: PWR_PUCRA_PU15 ( -- x addr ) 15 bit PWR_PUCRA ; \ PWR_PUCRA_PU15, Port A pull-up bit y  y=0..15
: PWR_PUCRA_PU14 ( -- x addr ) 14 bit PWR_PUCRA ; \ PWR_PUCRA_PU14, Port A pull-up bit y  y=0..15
: PWR_PUCRA_PU13 ( -- x addr ) 13 bit PWR_PUCRA ; \ PWR_PUCRA_PU13, Port A pull-up bit y  y=0..15
: PWR_PUCRA_PU12 ( -- x addr ) 12 bit PWR_PUCRA ; \ PWR_PUCRA_PU12, Port A pull-up bit y  y=0..15
: PWR_PUCRA_PU11 ( -- x addr ) 11 bit PWR_PUCRA ; \ PWR_PUCRA_PU11, Port A pull-up bit y  y=0..15
: PWR_PUCRA_PU10 ( -- x addr ) 10 bit PWR_PUCRA ; \ PWR_PUCRA_PU10, Port A pull-up bit y  y=0..15
: PWR_PUCRA_PU9 ( -- x addr ) 9 bit PWR_PUCRA ; \ PWR_PUCRA_PU9, Port A pull-up bit y  y=0..15
: PWR_PUCRA_PU8 ( -- x addr ) 8 bit PWR_PUCRA ; \ PWR_PUCRA_PU8, Port A pull-up bit y  y=0..15
: PWR_PUCRA_PU7 ( -- x addr ) 7 bit PWR_PUCRA ; \ PWR_PUCRA_PU7, Port A pull-up bit y  y=0..15
: PWR_PUCRA_PU6 ( -- x addr ) 6 bit PWR_PUCRA ; \ PWR_PUCRA_PU6, Port A pull-up bit y  y=0..15
: PWR_PUCRA_PU5 ( -- x addr ) 5 bit PWR_PUCRA ; \ PWR_PUCRA_PU5, Port A pull-up bit y  y=0..15
: PWR_PUCRA_PU4 ( -- x addr ) 4 bit PWR_PUCRA ; \ PWR_PUCRA_PU4, Port A pull-up bit y  y=0..15
: PWR_PUCRA_PU3 ( -- x addr ) 3 bit PWR_PUCRA ; \ PWR_PUCRA_PU3, Port A pull-up bit y  y=0..15
: PWR_PUCRA_PU2 ( -- x addr ) 2 bit PWR_PUCRA ; \ PWR_PUCRA_PU2, Port A pull-up bit y  y=0..15
: PWR_PUCRA_PU1 ( -- x addr ) 1 bit PWR_PUCRA ; \ PWR_PUCRA_PU1, Port A pull-up bit y  y=0..15
: PWR_PUCRA_PU0 ( -- x addr ) 0 bit PWR_PUCRA ; \ PWR_PUCRA_PU0, Port A pull-up bit y  y=0..15
[then]

defined? use-PWR defined? PWR_PDCRA_PD15 not and [if]
\ PWR_PDCRA (read-write) Reset:0x00000000
: PWR_PDCRA_PD15 ( -- x addr ) 15 bit PWR_PDCRA ; \ PWR_PDCRA_PD15, Port A pull-down bit y  y=0..15
: PWR_PDCRA_PD14 ( -- x addr ) 14 bit PWR_PDCRA ; \ PWR_PDCRA_PD14, Port A pull-down bit y  y=0..15
: PWR_PDCRA_PD13 ( -- x addr ) 13 bit PWR_PDCRA ; \ PWR_PDCRA_PD13, Port A pull-down bit y  y=0..15
: PWR_PDCRA_PD12 ( -- x addr ) 12 bit PWR_PDCRA ; \ PWR_PDCRA_PD12, Port A pull-down bit y  y=0..15
: PWR_PDCRA_PD11 ( -- x addr ) 11 bit PWR_PDCRA ; \ PWR_PDCRA_PD11, Port A pull-down bit y  y=0..15
: PWR_PDCRA_PD10 ( -- x addr ) 10 bit PWR_PDCRA ; \ PWR_PDCRA_PD10, Port A pull-down bit y  y=0..15
: PWR_PDCRA_PD9 ( -- x addr ) 9 bit PWR_PDCRA ; \ PWR_PDCRA_PD9, Port A pull-down bit y  y=0..15
: PWR_PDCRA_PD8 ( -- x addr ) 8 bit PWR_PDCRA ; \ PWR_PDCRA_PD8, Port A pull-down bit y  y=0..15
: PWR_PDCRA_PD7 ( -- x addr ) 7 bit PWR_PDCRA ; \ PWR_PDCRA_PD7, Port A pull-down bit y  y=0..15
: PWR_PDCRA_PD6 ( -- x addr ) 6 bit PWR_PDCRA ; \ PWR_PDCRA_PD6, Port A pull-down bit y  y=0..15
: PWR_PDCRA_PD5 ( -- x addr ) 5 bit PWR_PDCRA ; \ PWR_PDCRA_PD5, Port A pull-down bit y  y=0..15
: PWR_PDCRA_PD4 ( -- x addr ) 4 bit PWR_PDCRA ; \ PWR_PDCRA_PD4, Port A pull-down bit y  y=0..15
: PWR_PDCRA_PD3 ( -- x addr ) 3 bit PWR_PDCRA ; \ PWR_PDCRA_PD3, Port A pull-down bit y  y=0..15
: PWR_PDCRA_PD2 ( -- x addr ) 2 bit PWR_PDCRA ; \ PWR_PDCRA_PD2, Port A pull-down bit y  y=0..15
: PWR_PDCRA_PD1 ( -- x addr ) 1 bit PWR_PDCRA ; \ PWR_PDCRA_PD1, Port A pull-down bit y  y=0..15
: PWR_PDCRA_PD0 ( -- x addr ) 0 bit PWR_PDCRA ; \ PWR_PDCRA_PD0, Port A pull-down bit y  y=0..15
[then]

execute-defined? use-PWR defined? PWR_PUCRB_PU15 not and [if]
\ PWR_PUCRB (read-write) Reset:0x00000000
: PWR_PUCRB_PU15 ( -- x addr ) 15 bit PWR_PUCRB ; \ PWR_PUCRB_PU15, Port B pull-up bit y  y=0..15
: PWR_PUCRB_PU14 ( -- x addr ) 14 bit PWR_PUCRB ; \ PWR_PUCRB_PU14, Port B pull-up bit y  y=0..15
: PWR_PUCRB_PU13 ( -- x addr ) 13 bit PWR_PUCRB ; \ PWR_PUCRB_PU13, Port B pull-up bit y  y=0..15
: PWR_PUCRB_PU12 ( -- x addr ) 12 bit PWR_PUCRB ; \ PWR_PUCRB_PU12, Port B pull-up bit y  y=0..15
: PWR_PUCRB_PU11 ( -- x addr ) 11 bit PWR_PUCRB ; \ PWR_PUCRB_PU11, Port B pull-up bit y  y=0..15
: PWR_PUCRB_PU10 ( -- x addr ) 10 bit PWR_PUCRB ; \ PWR_PUCRB_PU10, Port B pull-up bit y  y=0..15
: PWR_PUCRB_PU9 ( -- x addr ) 9 bit PWR_PUCRB ; \ PWR_PUCRB_PU9, Port B pull-up bit y  y=0..15
: PWR_PUCRB_PU8 ( -- x addr ) 8 bit PWR_PUCRB ; \ PWR_PUCRB_PU8, Port B pull-up bit y  y=0..15
: PWR_PUCRB_PU7 ( -- x addr ) 7 bit PWR_PUCRB ; \ PWR_PUCRB_PU7, Port B pull-up bit y  y=0..15
: PWR_PUCRB_PU6 ( -- x addr ) 6 bit PWR_PUCRB ; \ PWR_PUCRB_PU6, Port B pull-up bit y  y=0..15
: PWR_PUCRB_PU5 ( -- x addr ) 5 bit PWR_PUCRB ; \ PWR_PUCRB_PU5, Port B pull-up bit y  y=0..15
: PWR_PUCRB_PU4 ( -- x addr ) 4 bit PWR_PUCRB ; \ PWR_PUCRB_PU4, Port B pull-up bit y  y=0..15
: PWR_PUCRB_PU3 ( -- x addr ) 3 bit PWR_PUCRB ; \ PWR_PUCRB_PU3, Port B pull-up bit y  y=0..15
: PWR_PUCRB_PU2 ( -- x addr ) 2 bit PWR_PUCRB ; \ PWR_PUCRB_PU2, Port B pull-up bit y  y=0..15
: PWR_PUCRB_PU1 ( -- x addr ) 1 bit PWR_PUCRB ; \ PWR_PUCRB_PU1, Port B pull-up bit y  y=0..15
: PWR_PUCRB_PU0 ( -- x addr ) 0 bit PWR_PUCRB ; \ PWR_PUCRB_PU0, Port B pull-up bit y  y=0..15
[then]

defined? use-PWR defined? PWR_PDCRB_PD15 not and [if]
\ PWR_PDCRB (read-write) Reset:0x00000000
: PWR_PDCRB_PD15 ( -- x addr ) 15 bit PWR_PDCRB ; \ PWR_PDCRB_PD15, Port B pull-down bit y  y=0..15
: PWR_PDCRB_PD14 ( -- x addr ) 14 bit PWR_PDCRB ; \ PWR_PDCRB_PD14, Port B pull-down bit y  y=0..15
: PWR_PDCRB_PD13 ( -- x addr ) 13 bit PWR_PDCRB ; \ PWR_PDCRB_PD13, Port B pull-down bit y  y=0..15
: PWR_PDCRB_PD12 ( -- x addr ) 12 bit PWR_PDCRB ; \ PWR_PDCRB_PD12, Port B pull-down bit y  y=0..15
: PWR_PDCRB_PD11 ( -- x addr ) 11 bit PWR_PDCRB ; \ PWR_PDCRB_PD11, Port B pull-down bit y  y=0..15
: PWR_PDCRB_PD10 ( -- x addr ) 10 bit PWR_PDCRB ; \ PWR_PDCRB_PD10, Port B pull-down bit y  y=0..15
: PWR_PDCRB_PD9 ( -- x addr ) 9 bit PWR_PDCRB ; \ PWR_PDCRB_PD9, Port B pull-down bit y  y=0..15
: PWR_PDCRB_PD8 ( -- x addr ) 8 bit PWR_PDCRB ; \ PWR_PDCRB_PD8, Port B pull-down bit y  y=0..15
: PWR_PDCRB_PD7 ( -- x addr ) 7 bit PWR_PDCRB ; \ PWR_PDCRB_PD7, Port B pull-down bit y  y=0..15
: PWR_PDCRB_PD6 ( -- x addr ) 6 bit PWR_PDCRB ; \ PWR_PDCRB_PD6, Port B pull-down bit y  y=0..15
: PWR_PDCRB_PD5 ( -- x addr ) 5 bit PWR_PDCRB ; \ PWR_PDCRB_PD5, Port B pull-down bit y  y=0..15
: PWR_PDCRB_PD4 ( -- x addr ) 4 bit PWR_PDCRB ; \ PWR_PDCRB_PD4, Port B pull-down bit y  y=0..15
: PWR_PDCRB_PD3 ( -- x addr ) 3 bit PWR_PDCRB ; \ PWR_PDCRB_PD3, Port B pull-down bit y  y=0..15
: PWR_PDCRB_PD2 ( -- x addr ) 2 bit PWR_PDCRB ; \ PWR_PDCRB_PD2, Port B pull-down bit y  y=0..15
: PWR_PDCRB_PD1 ( -- x addr ) 1 bit PWR_PDCRB ; \ PWR_PDCRB_PD1, Port B pull-down bit y  y=0..15
: PWR_PDCRB_PD0 ( -- x addr ) 0 bit PWR_PDCRB ; \ PWR_PDCRB_PD0, Port B pull-down bit y  y=0..15
[then]

execute-defined? use-PWR defined? PWR_PUCRC_PU15 not and [if]
\ PWR_PUCRC (read-write) Reset:0x00000000
: PWR_PUCRC_PU15 ( -- x addr ) 15 bit PWR_PUCRC ; \ PWR_PUCRC_PU15, Port C pull-up bit y  y=0..15
: PWR_PUCRC_PU14 ( -- x addr ) 14 bit PWR_PUCRC ; \ PWR_PUCRC_PU14, Port C pull-up bit y  y=0..15
: PWR_PUCRC_PU13 ( -- x addr ) 13 bit PWR_PUCRC ; \ PWR_PUCRC_PU13, Port C pull-up bit y  y=0..15
: PWR_PUCRC_PU12 ( -- x addr ) 12 bit PWR_PUCRC ; \ PWR_PUCRC_PU12, Port C pull-up bit y  y=0..15
: PWR_PUCRC_PU11 ( -- x addr ) 11 bit PWR_PUCRC ; \ PWR_PUCRC_PU11, Port C pull-up bit y  y=0..15
: PWR_PUCRC_PU10 ( -- x addr ) 10 bit PWR_PUCRC ; \ PWR_PUCRC_PU10, Port C pull-up bit y  y=0..15
: PWR_PUCRC_PU9 ( -- x addr ) 9 bit PWR_PUCRC ; \ PWR_PUCRC_PU9, Port C pull-up bit y  y=0..15
: PWR_PUCRC_PU8 ( -- x addr ) 8 bit PWR_PUCRC ; \ PWR_PUCRC_PU8, Port C pull-up bit y  y=0..15
: PWR_PUCRC_PU7 ( -- x addr ) 7 bit PWR_PUCRC ; \ PWR_PUCRC_PU7, Port C pull-up bit y  y=0..15
: PWR_PUCRC_PU6 ( -- x addr ) 6 bit PWR_PUCRC ; \ PWR_PUCRC_PU6, Port C pull-up bit y  y=0..15
: PWR_PUCRC_PU5 ( -- x addr ) 5 bit PWR_PUCRC ; \ PWR_PUCRC_PU5, Port C pull-up bit y  y=0..15
: PWR_PUCRC_PU4 ( -- x addr ) 4 bit PWR_PUCRC ; \ PWR_PUCRC_PU4, Port C pull-up bit y  y=0..15
: PWR_PUCRC_PU3 ( -- x addr ) 3 bit PWR_PUCRC ; \ PWR_PUCRC_PU3, Port C pull-up bit y  y=0..15
: PWR_PUCRC_PU2 ( -- x addr ) 2 bit PWR_PUCRC ; \ PWR_PUCRC_PU2, Port C pull-up bit y  y=0..15
: PWR_PUCRC_PU1 ( -- x addr ) 1 bit PWR_PUCRC ; \ PWR_PUCRC_PU1, Port C pull-up bit y  y=0..15
: PWR_PUCRC_PU0 ( -- x addr ) 0 bit PWR_PUCRC ; \ PWR_PUCRC_PU0, Port C pull-up bit y  y=0..15
[then]

defined? use-PWR defined? PWR_PDCRC_PD15 not and [if]
\ PWR_PDCRC (read-write) Reset:0x00000000
: PWR_PDCRC_PD15 ( -- x addr ) 15 bit PWR_PDCRC ; \ PWR_PDCRC_PD15, Port C pull-down bit y  y=0..15
: PWR_PDCRC_PD14 ( -- x addr ) 14 bit PWR_PDCRC ; \ PWR_PDCRC_PD14, Port C pull-down bit y  y=0..15
: PWR_PDCRC_PD13 ( -- x addr ) 13 bit PWR_PDCRC ; \ PWR_PDCRC_PD13, Port C pull-down bit y  y=0..15
: PWR_PDCRC_PD12 ( -- x addr ) 12 bit PWR_PDCRC ; \ PWR_PDCRC_PD12, Port C pull-down bit y  y=0..15
: PWR_PDCRC_PD11 ( -- x addr ) 11 bit PWR_PDCRC ; \ PWR_PDCRC_PD11, Port C pull-down bit y  y=0..15
: PWR_PDCRC_PD10 ( -- x addr ) 10 bit PWR_PDCRC ; \ PWR_PDCRC_PD10, Port C pull-down bit y  y=0..15
: PWR_PDCRC_PD9 ( -- x addr ) 9 bit PWR_PDCRC ; \ PWR_PDCRC_PD9, Port C pull-down bit y  y=0..15
: PWR_PDCRC_PD8 ( -- x addr ) 8 bit PWR_PDCRC ; \ PWR_PDCRC_PD8, Port C pull-down bit y  y=0..15
: PWR_PDCRC_PD7 ( -- x addr ) 7 bit PWR_PDCRC ; \ PWR_PDCRC_PD7, Port C pull-down bit y  y=0..15
: PWR_PDCRC_PD6 ( -- x addr ) 6 bit PWR_PDCRC ; \ PWR_PDCRC_PD6, Port C pull-down bit y  y=0..15
: PWR_PDCRC_PD5 ( -- x addr ) 5 bit PWR_PDCRC ; \ PWR_PDCRC_PD5, Port C pull-down bit y  y=0..15
: PWR_PDCRC_PD4 ( -- x addr ) 4 bit PWR_PDCRC ; \ PWR_PDCRC_PD4, Port C pull-down bit y  y=0..15
: PWR_PDCRC_PD3 ( -- x addr ) 3 bit PWR_PDCRC ; \ PWR_PDCRC_PD3, Port C pull-down bit y  y=0..15
: PWR_PDCRC_PD2 ( -- x addr ) 2 bit PWR_PDCRC ; \ PWR_PDCRC_PD2, Port C pull-down bit y  y=0..15
: PWR_PDCRC_PD1 ( -- x addr ) 1 bit PWR_PDCRC ; \ PWR_PDCRC_PD1, Port C pull-down bit y  y=0..15
: PWR_PDCRC_PD0 ( -- x addr ) 0 bit PWR_PDCRC ; \ PWR_PDCRC_PD0, Port C pull-down bit y  y=0..15
[then]

execute-defined? use-PWR defined? PWR_PUCRD_PU15 not and [if]
\ PWR_PUCRD (read-write) Reset:0x00000000
: PWR_PUCRD_PU15 ( -- x addr ) 15 bit PWR_PUCRD ; \ PWR_PUCRD_PU15, Port D pull-up bit y  y=0..15
: PWR_PUCRD_PU14 ( -- x addr ) 14 bit PWR_PUCRD ; \ PWR_PUCRD_PU14, Port D pull-up bit y  y=0..15
: PWR_PUCRD_PU13 ( -- x addr ) 13 bit PWR_PUCRD ; \ PWR_PUCRD_PU13, Port D pull-up bit y  y=0..15
: PWR_PUCRD_PU12 ( -- x addr ) 12 bit PWR_PUCRD ; \ PWR_PUCRD_PU12, Port D pull-up bit y  y=0..15
: PWR_PUCRD_PU11 ( -- x addr ) 11 bit PWR_PUCRD ; \ PWR_PUCRD_PU11, Port D pull-up bit y  y=0..15
: PWR_PUCRD_PU10 ( -- x addr ) 10 bit PWR_PUCRD ; \ PWR_PUCRD_PU10, Port D pull-up bit y  y=0..15
: PWR_PUCRD_PU9 ( -- x addr ) 9 bit PWR_PUCRD ; \ PWR_PUCRD_PU9, Port D pull-up bit y  y=0..15
: PWR_PUCRD_PU8 ( -- x addr ) 8 bit PWR_PUCRD ; \ PWR_PUCRD_PU8, Port D pull-up bit y  y=0..15
: PWR_PUCRD_PU7 ( -- x addr ) 7 bit PWR_PUCRD ; \ PWR_PUCRD_PU7, Port D pull-up bit y  y=0..15
: PWR_PUCRD_PU6 ( -- x addr ) 6 bit PWR_PUCRD ; \ PWR_PUCRD_PU6, Port D pull-up bit y  y=0..15
: PWR_PUCRD_PU5 ( -- x addr ) 5 bit PWR_PUCRD ; \ PWR_PUCRD_PU5, Port D pull-up bit y  y=0..15
: PWR_PUCRD_PU4 ( -- x addr ) 4 bit PWR_PUCRD ; \ PWR_PUCRD_PU4, Port D pull-up bit y  y=0..15
: PWR_PUCRD_PU3 ( -- x addr ) 3 bit PWR_PUCRD ; \ PWR_PUCRD_PU3, Port D pull-up bit y  y=0..15
: PWR_PUCRD_PU2 ( -- x addr ) 2 bit PWR_PUCRD ; \ PWR_PUCRD_PU2, Port D pull-up bit y  y=0..15
: PWR_PUCRD_PU1 ( -- x addr ) 1 bit PWR_PUCRD ; \ PWR_PUCRD_PU1, Port D pull-up bit y  y=0..15
: PWR_PUCRD_PU0 ( -- x addr ) 0 bit PWR_PUCRD ; \ PWR_PUCRD_PU0, Port D pull-up bit y  y=0..15
[then]

defined? use-PWR defined? PWR_PDCRD_PD15 not and [if]
\ PWR_PDCRD (read-write) Reset:0x00000000
: PWR_PDCRD_PD15 ( -- x addr ) 15 bit PWR_PDCRD ; \ PWR_PDCRD_PD15, Port D pull-down bit y  y=0..15
: PWR_PDCRD_PD14 ( -- x addr ) 14 bit PWR_PDCRD ; \ PWR_PDCRD_PD14, Port D pull-down bit y  y=0..15
: PWR_PDCRD_PD13 ( -- x addr ) 13 bit PWR_PDCRD ; \ PWR_PDCRD_PD13, Port D pull-down bit y  y=0..15
: PWR_PDCRD_PD12 ( -- x addr ) 12 bit PWR_PDCRD ; \ PWR_PDCRD_PD12, Port D pull-down bit y  y=0..15
: PWR_PDCRD_PD11 ( -- x addr ) 11 bit PWR_PDCRD ; \ PWR_PDCRD_PD11, Port D pull-down bit y  y=0..15
: PWR_PDCRD_PD10 ( -- x addr ) 10 bit PWR_PDCRD ; \ PWR_PDCRD_PD10, Port D pull-down bit y  y=0..15
: PWR_PDCRD_PD9 ( -- x addr ) 9 bit PWR_PDCRD ; \ PWR_PDCRD_PD9, Port D pull-down bit y  y=0..15
: PWR_PDCRD_PD8 ( -- x addr ) 8 bit PWR_PDCRD ; \ PWR_PDCRD_PD8, Port D pull-down bit y  y=0..15
: PWR_PDCRD_PD7 ( -- x addr ) 7 bit PWR_PDCRD ; \ PWR_PDCRD_PD7, Port D pull-down bit y  y=0..15
: PWR_PDCRD_PD6 ( -- x addr ) 6 bit PWR_PDCRD ; \ PWR_PDCRD_PD6, Port D pull-down bit y  y=0..15
: PWR_PDCRD_PD5 ( -- x addr ) 5 bit PWR_PDCRD ; \ PWR_PDCRD_PD5, Port D pull-down bit y  y=0..15
: PWR_PDCRD_PD4 ( -- x addr ) 4 bit PWR_PDCRD ; \ PWR_PDCRD_PD4, Port D pull-down bit y  y=0..15
: PWR_PDCRD_PD3 ( -- x addr ) 3 bit PWR_PDCRD ; \ PWR_PDCRD_PD3, Port D pull-down bit y  y=0..15
: PWR_PDCRD_PD2 ( -- x addr ) 2 bit PWR_PDCRD ; \ PWR_PDCRD_PD2, Port D pull-down bit y  y=0..15
: PWR_PDCRD_PD1 ( -- x addr ) 1 bit PWR_PDCRD ; \ PWR_PDCRD_PD1, Port D pull-down bit y  y=0..15
: PWR_PDCRD_PD0 ( -- x addr ) 0 bit PWR_PDCRD ; \ PWR_PDCRD_PD0, Port D pull-down bit y  y=0..15
[then]

execute-defined? use-PWR defined? PWR_PUCRE_PU15 not and [if]
\ PWR_PUCRE (read-write) Reset:0x00000000
: PWR_PUCRE_PU15 ( -- x addr ) 15 bit PWR_PUCRE ; \ PWR_PUCRE_PU15, Port E pull-up bit y  y=0..15
: PWR_PUCRE_PU14 ( -- x addr ) 14 bit PWR_PUCRE ; \ PWR_PUCRE_PU14, Port E pull-up bit y  y=0..15
: PWR_PUCRE_PU13 ( -- x addr ) 13 bit PWR_PUCRE ; \ PWR_PUCRE_PU13, Port E pull-up bit y  y=0..15
: PWR_PUCRE_PU12 ( -- x addr ) 12 bit PWR_PUCRE ; \ PWR_PUCRE_PU12, Port E pull-up bit y  y=0..15
: PWR_PUCRE_PU11 ( -- x addr ) 11 bit PWR_PUCRE ; \ PWR_PUCRE_PU11, Port E pull-up bit y  y=0..15
: PWR_PUCRE_PU10 ( -- x addr ) 10 bit PWR_PUCRE ; \ PWR_PUCRE_PU10, Port E pull-up bit y  y=0..15
: PWR_PUCRE_PU9 ( -- x addr ) 9 bit PWR_PUCRE ; \ PWR_PUCRE_PU9, Port E pull-up bit y  y=0..15
: PWR_PUCRE_PU8 ( -- x addr ) 8 bit PWR_PUCRE ; \ PWR_PUCRE_PU8, Port E pull-up bit y  y=0..15
: PWR_PUCRE_PU7 ( -- x addr ) 7 bit PWR_PUCRE ; \ PWR_PUCRE_PU7, Port E pull-up bit y  y=0..15
: PWR_PUCRE_PU6 ( -- x addr ) 6 bit PWR_PUCRE ; \ PWR_PUCRE_PU6, Port E pull-up bit y  y=0..15
: PWR_PUCRE_PU5 ( -- x addr ) 5 bit PWR_PUCRE ; \ PWR_PUCRE_PU5, Port E pull-up bit y  y=0..15
: PWR_PUCRE_PU4 ( -- x addr ) 4 bit PWR_PUCRE ; \ PWR_PUCRE_PU4, Port E pull-up bit y  y=0..15
: PWR_PUCRE_PU3 ( -- x addr ) 3 bit PWR_PUCRE ; \ PWR_PUCRE_PU3, Port E pull-up bit y  y=0..15
: PWR_PUCRE_PU2 ( -- x addr ) 2 bit PWR_PUCRE ; \ PWR_PUCRE_PU2, Port E pull-up bit y  y=0..15
: PWR_PUCRE_PU1 ( -- x addr ) 1 bit PWR_PUCRE ; \ PWR_PUCRE_PU1, Port E pull-up bit y  y=0..15
: PWR_PUCRE_PU0 ( -- x addr ) 0 bit PWR_PUCRE ; \ PWR_PUCRE_PU0, Port E pull-up bit y  y=0..15
[then]

defined? use-PWR defined? PWR_PDCRE_PD15 not and [if]
\ PWR_PDCRE (read-write) Reset:0x00000000
: PWR_PDCRE_PD15 ( -- x addr ) 15 bit PWR_PDCRE ; \ PWR_PDCRE_PD15, Port E pull-down bit y  y=0..15
: PWR_PDCRE_PD14 ( -- x addr ) 14 bit PWR_PDCRE ; \ PWR_PDCRE_PD14, Port E pull-down bit y  y=0..15
: PWR_PDCRE_PD13 ( -- x addr ) 13 bit PWR_PDCRE ; \ PWR_PDCRE_PD13, Port E pull-down bit y  y=0..15
: PWR_PDCRE_PD12 ( -- x addr ) 12 bit PWR_PDCRE ; \ PWR_PDCRE_PD12, Port E pull-down bit y  y=0..15
: PWR_PDCRE_PD11 ( -- x addr ) 11 bit PWR_PDCRE ; \ PWR_PDCRE_PD11, Port E pull-down bit y  y=0..15
: PWR_PDCRE_PD10 ( -- x addr ) 10 bit PWR_PDCRE ; \ PWR_PDCRE_PD10, Port E pull-down bit y  y=0..15
: PWR_PDCRE_PD9 ( -- x addr ) 9 bit PWR_PDCRE ; \ PWR_PDCRE_PD9, Port E pull-down bit y  y=0..15
: PWR_PDCRE_PD8 ( -- x addr ) 8 bit PWR_PDCRE ; \ PWR_PDCRE_PD8, Port E pull-down bit y  y=0..15
: PWR_PDCRE_PD7 ( -- x addr ) 7 bit PWR_PDCRE ; \ PWR_PDCRE_PD7, Port E pull-down bit y  y=0..15
: PWR_PDCRE_PD6 ( -- x addr ) 6 bit PWR_PDCRE ; \ PWR_PDCRE_PD6, Port E pull-down bit y  y=0..15
: PWR_PDCRE_PD5 ( -- x addr ) 5 bit PWR_PDCRE ; \ PWR_PDCRE_PD5, Port E pull-down bit y  y=0..15
: PWR_PDCRE_PD4 ( -- x addr ) 4 bit PWR_PDCRE ; \ PWR_PDCRE_PD4, Port E pull-down bit y  y=0..15
: PWR_PDCRE_PD3 ( -- x addr ) 3 bit PWR_PDCRE ; \ PWR_PDCRE_PD3, Port E pull-down bit y  y=0..15
: PWR_PDCRE_PD2 ( -- x addr ) 2 bit PWR_PDCRE ; \ PWR_PDCRE_PD2, Port E pull-down bit y  y=0..15
: PWR_PDCRE_PD1 ( -- x addr ) 1 bit PWR_PDCRE ; \ PWR_PDCRE_PD1, Port E pull-down bit y  y=0..15
: PWR_PDCRE_PD0 ( -- x addr ) 0 bit PWR_PDCRE ; \ PWR_PDCRE_PD0, Port E pull-down bit y  y=0..15
[then]

execute-defined? use-PWR defined? PWR_PUCRF_PU15 not and [if]
\ PWR_PUCRF (read-write) Reset:0x00000000
: PWR_PUCRF_PU15 ( -- x addr ) 15 bit PWR_PUCRF ; \ PWR_PUCRF_PU15, Port F pull-up bit y  y=0..15
: PWR_PUCRF_PU14 ( -- x addr ) 14 bit PWR_PUCRF ; \ PWR_PUCRF_PU14, Port F pull-up bit y  y=0..15
: PWR_PUCRF_PU13 ( -- x addr ) 13 bit PWR_PUCRF ; \ PWR_PUCRF_PU13, Port F pull-up bit y  y=0..15
: PWR_PUCRF_PU12 ( -- x addr ) 12 bit PWR_PUCRF ; \ PWR_PUCRF_PU12, Port F pull-up bit y  y=0..15
: PWR_PUCRF_PU11 ( -- x addr ) 11 bit PWR_PUCRF ; \ PWR_PUCRF_PU11, Port F pull-up bit y  y=0..15
: PWR_PUCRF_PU10 ( -- x addr ) 10 bit PWR_PUCRF ; \ PWR_PUCRF_PU10, Port F pull-up bit y  y=0..15
: PWR_PUCRF_PU9 ( -- x addr ) 9 bit PWR_PUCRF ; \ PWR_PUCRF_PU9, Port F pull-up bit y  y=0..15
: PWR_PUCRF_PU8 ( -- x addr ) 8 bit PWR_PUCRF ; \ PWR_PUCRF_PU8, Port F pull-up bit y  y=0..15
: PWR_PUCRF_PU7 ( -- x addr ) 7 bit PWR_PUCRF ; \ PWR_PUCRF_PU7, Port F pull-up bit y  y=0..15
: PWR_PUCRF_PU6 ( -- x addr ) 6 bit PWR_PUCRF ; \ PWR_PUCRF_PU6, Port F pull-up bit y  y=0..15
: PWR_PUCRF_PU5 ( -- x addr ) 5 bit PWR_PUCRF ; \ PWR_PUCRF_PU5, Port F pull-up bit y  y=0..15
: PWR_PUCRF_PU4 ( -- x addr ) 4 bit PWR_PUCRF ; \ PWR_PUCRF_PU4, Port F pull-up bit y  y=0..15
: PWR_PUCRF_PU3 ( -- x addr ) 3 bit PWR_PUCRF ; \ PWR_PUCRF_PU3, Port F pull-up bit y  y=0..15
: PWR_PUCRF_PU2 ( -- x addr ) 2 bit PWR_PUCRF ; \ PWR_PUCRF_PU2, Port F pull-up bit y  y=0..15
: PWR_PUCRF_PU1 ( -- x addr ) 1 bit PWR_PUCRF ; \ PWR_PUCRF_PU1, Port F pull-up bit y  y=0..15
: PWR_PUCRF_PU0 ( -- x addr ) 0 bit PWR_PUCRF ; \ PWR_PUCRF_PU0, Port F pull-up bit y  y=0..15
[then]

defined? use-PWR defined? PWR_PDCRF_PD15 not and [if]
\ PWR_PDCRF (read-write) Reset:0x00000000
: PWR_PDCRF_PD15 ( -- x addr ) 15 bit PWR_PDCRF ; \ PWR_PDCRF_PD15, Port F pull-down bit y  y=0..15
: PWR_PDCRF_PD14 ( -- x addr ) 14 bit PWR_PDCRF ; \ PWR_PDCRF_PD14, Port F pull-down bit y  y=0..15
: PWR_PDCRF_PD13 ( -- x addr ) 13 bit PWR_PDCRF ; \ PWR_PDCRF_PD13, Port F pull-down bit y  y=0..15
: PWR_PDCRF_PD12 ( -- x addr ) 12 bit PWR_PDCRF ; \ PWR_PDCRF_PD12, Port F pull-down bit y  y=0..15
: PWR_PDCRF_PD11 ( -- x addr ) 11 bit PWR_PDCRF ; \ PWR_PDCRF_PD11, Port F pull-down bit y  y=0..15
: PWR_PDCRF_PD10 ( -- x addr ) 10 bit PWR_PDCRF ; \ PWR_PDCRF_PD10, Port F pull-down bit y  y=0..15
: PWR_PDCRF_PD9 ( -- x addr ) 9 bit PWR_PDCRF ; \ PWR_PDCRF_PD9, Port F pull-down bit y  y=0..15
: PWR_PDCRF_PD8 ( -- x addr ) 8 bit PWR_PDCRF ; \ PWR_PDCRF_PD8, Port F pull-down bit y  y=0..15
: PWR_PDCRF_PD7 ( -- x addr ) 7 bit PWR_PDCRF ; \ PWR_PDCRF_PD7, Port F pull-down bit y  y=0..15
: PWR_PDCRF_PD6 ( -- x addr ) 6 bit PWR_PDCRF ; \ PWR_PDCRF_PD6, Port F pull-down bit y  y=0..15
: PWR_PDCRF_PD5 ( -- x addr ) 5 bit PWR_PDCRF ; \ PWR_PDCRF_PD5, Port F pull-down bit y  y=0..15
: PWR_PDCRF_PD4 ( -- x addr ) 4 bit PWR_PDCRF ; \ PWR_PDCRF_PD4, Port F pull-down bit y  y=0..15
: PWR_PDCRF_PD3 ( -- x addr ) 3 bit PWR_PDCRF ; \ PWR_PDCRF_PD3, Port F pull-down bit y  y=0..15
: PWR_PDCRF_PD2 ( -- x addr ) 2 bit PWR_PDCRF ; \ PWR_PDCRF_PD2, Port F pull-down bit y  y=0..15
: PWR_PDCRF_PD1 ( -- x addr ) 1 bit PWR_PDCRF ; \ PWR_PDCRF_PD1, Port F pull-down bit y  y=0..15
: PWR_PDCRF_PD0 ( -- x addr ) 0 bit PWR_PDCRF ; \ PWR_PDCRF_PD0, Port F pull-down bit y  y=0..15
[then]

execute-defined? use-PWR defined? PWR_PUCRG_PU15 not and [if]
\ PWR_PUCRG (read-write) Reset:0x00000000
: PWR_PUCRG_PU15 ( -- x addr ) 15 bit PWR_PUCRG ; \ PWR_PUCRG_PU15, Port G pull-up bit y  y=0..15
: PWR_PUCRG_PU14 ( -- x addr ) 14 bit PWR_PUCRG ; \ PWR_PUCRG_PU14, Port G pull-up bit y  y=0..15
: PWR_PUCRG_PU13 ( -- x addr ) 13 bit PWR_PUCRG ; \ PWR_PUCRG_PU13, Port G pull-up bit y  y=0..15
: PWR_PUCRG_PU12 ( -- x addr ) 12 bit PWR_PUCRG ; \ PWR_PUCRG_PU12, Port G pull-up bit y  y=0..15
: PWR_PUCRG_PU11 ( -- x addr ) 11 bit PWR_PUCRG ; \ PWR_PUCRG_PU11, Port G pull-up bit y  y=0..15
: PWR_PUCRG_PU10 ( -- x addr ) 10 bit PWR_PUCRG ; \ PWR_PUCRG_PU10, Port G pull-up bit y  y=0..15
: PWR_PUCRG_PU9 ( -- x addr ) 9 bit PWR_PUCRG ; \ PWR_PUCRG_PU9, Port G pull-up bit y  y=0..15
: PWR_PUCRG_PU8 ( -- x addr ) 8 bit PWR_PUCRG ; \ PWR_PUCRG_PU8, Port G pull-up bit y  y=0..15
: PWR_PUCRG_PU7 ( -- x addr ) 7 bit PWR_PUCRG ; \ PWR_PUCRG_PU7, Port G pull-up bit y  y=0..15
: PWR_PUCRG_PU6 ( -- x addr ) 6 bit PWR_PUCRG ; \ PWR_PUCRG_PU6, Port G pull-up bit y  y=0..15
: PWR_PUCRG_PU5 ( -- x addr ) 5 bit PWR_PUCRG ; \ PWR_PUCRG_PU5, Port G pull-up bit y  y=0..15
: PWR_PUCRG_PU4 ( -- x addr ) 4 bit PWR_PUCRG ; \ PWR_PUCRG_PU4, Port G pull-up bit y  y=0..15
: PWR_PUCRG_PU3 ( -- x addr ) 3 bit PWR_PUCRG ; \ PWR_PUCRG_PU3, Port G pull-up bit y  y=0..15
: PWR_PUCRG_PU2 ( -- x addr ) 2 bit PWR_PUCRG ; \ PWR_PUCRG_PU2, Port G pull-up bit y  y=0..15
: PWR_PUCRG_PU1 ( -- x addr ) 1 bit PWR_PUCRG ; \ PWR_PUCRG_PU1, Port G pull-up bit y  y=0..15
: PWR_PUCRG_PU0 ( -- x addr ) 0 bit PWR_PUCRG ; \ PWR_PUCRG_PU0, Port G pull-up bit y  y=0..15
[then]

defined? use-PWR defined? PWR_PDCRG_PD15 not and [if]
\ PWR_PDCRG (read-write) Reset:0x00000000
: PWR_PDCRG_PD15 ( -- x addr ) 15 bit PWR_PDCRG ; \ PWR_PDCRG_PD15, Port G pull-down bit y  y=0..15
: PWR_PDCRG_PD14 ( -- x addr ) 14 bit PWR_PDCRG ; \ PWR_PDCRG_PD14, Port G pull-down bit y  y=0..15
: PWR_PDCRG_PD13 ( -- x addr ) 13 bit PWR_PDCRG ; \ PWR_PDCRG_PD13, Port G pull-down bit y  y=0..15
: PWR_PDCRG_PD12 ( -- x addr ) 12 bit PWR_PDCRG ; \ PWR_PDCRG_PD12, Port G pull-down bit y  y=0..15
: PWR_PDCRG_PD11 ( -- x addr ) 11 bit PWR_PDCRG ; \ PWR_PDCRG_PD11, Port G pull-down bit y  y=0..15
: PWR_PDCRG_PD10 ( -- x addr ) 10 bit PWR_PDCRG ; \ PWR_PDCRG_PD10, Port G pull-down bit y  y=0..15
: PWR_PDCRG_PD9 ( -- x addr ) 9 bit PWR_PDCRG ; \ PWR_PDCRG_PD9, Port G pull-down bit y  y=0..15
: PWR_PDCRG_PD8 ( -- x addr ) 8 bit PWR_PDCRG ; \ PWR_PDCRG_PD8, Port G pull-down bit y  y=0..15
: PWR_PDCRG_PD7 ( -- x addr ) 7 bit PWR_PDCRG ; \ PWR_PDCRG_PD7, Port G pull-down bit y  y=0..15
: PWR_PDCRG_PD6 ( -- x addr ) 6 bit PWR_PDCRG ; \ PWR_PDCRG_PD6, Port G pull-down bit y  y=0..15
: PWR_PDCRG_PD5 ( -- x addr ) 5 bit PWR_PDCRG ; \ PWR_PDCRG_PD5, Port G pull-down bit y  y=0..15
: PWR_PDCRG_PD4 ( -- x addr ) 4 bit PWR_PDCRG ; \ PWR_PDCRG_PD4, Port G pull-down bit y  y=0..15
: PWR_PDCRG_PD3 ( -- x addr ) 3 bit PWR_PDCRG ; \ PWR_PDCRG_PD3, Port G pull-down bit y  y=0..15
: PWR_PDCRG_PD2 ( -- x addr ) 2 bit PWR_PDCRG ; \ PWR_PDCRG_PD2, Port G pull-down bit y  y=0..15
: PWR_PDCRG_PD1 ( -- x addr ) 1 bit PWR_PDCRG ; \ PWR_PDCRG_PD1, Port G pull-down bit y  y=0..15
: PWR_PDCRG_PD0 ( -- x addr ) 0 bit PWR_PDCRG ; \ PWR_PDCRG_PD0, Port G pull-down bit y  y=0..15
[then]

execute-defined? use-PWR defined? PWR_PUCRH_PU1 not and [if]
\ PWR_PUCRH (read-write) Reset:0x00000000
: PWR_PUCRH_PU1 ( -- x addr ) 1 bit PWR_PUCRH ; \ PWR_PUCRH_PU1, Port H pull-up bit y  y=0..1
: PWR_PUCRH_PU0 ( -- x addr ) 0 bit PWR_PUCRH ; \ PWR_PUCRH_PU0, Port H pull-up bit y  y=0..1
[then]

defined? use-PWR defined? PWR_PDCRH_PD1 not and [if]
\ PWR_PDCRH (read-write) Reset:0x00000000
: PWR_PDCRH_PD1 ( -- x addr ) 1 bit PWR_PDCRH ; \ PWR_PDCRH_PD1, Port H pull-down bit y  y=0..1
: PWR_PDCRH_PD0 ( -- x addr ) 0 bit PWR_PDCRH ; \ PWR_PDCRH_PD0, Port H pull-down bit y  y=0..1
[then]

execute-defined? use-SYSCFG defined? SYSCFG_MEMRMP_FB_MODE not and [if]
\ SYSCFG_MEMRMP (read-write) Reset:0x00000000
: SYSCFG_MEMRMP_FB_MODE ( -- x addr ) 8 bit SYSCFG_MEMRMP ; \ SYSCFG_MEMRMP_FB_MODE, Flash Bank mode selection
: SYSCFG_MEMRMP_QFS ( -- x addr ) 3 bit SYSCFG_MEMRMP ; \ SYSCFG_MEMRMP_QFS, QUADSPI memory mapping  swap
: SYSCFG_MEMRMP_MEM_MODE ( %bbb -- x addr ) SYSCFG_MEMRMP ; \ SYSCFG_MEMRMP_MEM_MODE, Memory mapping selection
[then]

defined? use-SYSCFG defined? SYSCFG_CFGR1_FPU_IE not and [if]
\ SYSCFG_CFGR1 (read-write) Reset:0x7C000001
: SYSCFG_CFGR1_FPU_IE ( %bbbbbb -- x addr ) 26 lshift SYSCFG_CFGR1 ; \ SYSCFG_CFGR1_FPU_IE, Floating Point Unit interrupts enable  bits
: SYSCFG_CFGR1_I2C3_FMP ( -- x addr ) 22 bit SYSCFG_CFGR1 ; \ SYSCFG_CFGR1_I2C3_FMP, I2C3 Fast-mode Plus driving capability  activation
: SYSCFG_CFGR1_I2C2_FMP ( -- x addr ) 21 bit SYSCFG_CFGR1 ; \ SYSCFG_CFGR1_I2C2_FMP, I2C2 Fast-mode Plus driving capability  activation
: SYSCFG_CFGR1_I2C1_FMP ( -- x addr ) 20 bit SYSCFG_CFGR1 ; \ SYSCFG_CFGR1_I2C1_FMP, I2C1 Fast-mode Plus driving capability  activation
: SYSCFG_CFGR1_I2C_PB9_FMP ( -- x addr ) 19 bit SYSCFG_CFGR1 ; \ SYSCFG_CFGR1_I2C_PB9_FMP, Fast-mode Plus Fm+ driving capability  activation on PB9
: SYSCFG_CFGR1_I2C_PB8_FMP ( -- x addr ) 18 bit SYSCFG_CFGR1 ; \ SYSCFG_CFGR1_I2C_PB8_FMP, Fast-mode Plus Fm+ driving capability  activation on PB8
: SYSCFG_CFGR1_I2C_PB7_FMP ( -- x addr ) 17 bit SYSCFG_CFGR1 ; \ SYSCFG_CFGR1_I2C_PB7_FMP, Fast-mode Plus Fm+ driving capability  activation on PB7
: SYSCFG_CFGR1_I2C_PB6_FMP ( -- x addr ) 16 bit SYSCFG_CFGR1 ; \ SYSCFG_CFGR1_I2C_PB6_FMP, Fast-mode Plus Fm+ driving capability  activation on PB6
: SYSCFG_CFGR1_BOOSTEN ( -- x addr ) 8 bit SYSCFG_CFGR1 ; \ SYSCFG_CFGR1_BOOSTEN, I/O analog switch voltage booster  enable
: SYSCFG_CFGR1_FWDIS ( -- x addr ) 0 bit SYSCFG_CFGR1 ; \ SYSCFG_CFGR1_FWDIS, Firewall disable
[then]

execute-defined? use-SYSCFG defined? SYSCFG_EXTICR1_EXTI3 not and [if]
\ SYSCFG_EXTICR1 (read-write) Reset:0x00000000
: SYSCFG_EXTICR1_EXTI3 ( %bbb -- x addr ) 12 lshift SYSCFG_EXTICR1 ; \ SYSCFG_EXTICR1_EXTI3, EXTI 3 configuration bits
: SYSCFG_EXTICR1_EXTI2 ( %bbb -- x addr ) 8 lshift SYSCFG_EXTICR1 ; \ SYSCFG_EXTICR1_EXTI2, EXTI 2 configuration bits
: SYSCFG_EXTICR1_EXTI1 ( %bbb -- x addr ) 4 lshift SYSCFG_EXTICR1 ; \ SYSCFG_EXTICR1_EXTI1, EXTI 1 configuration bits
: SYSCFG_EXTICR1_EXTI0 ( %bbb -- x addr ) SYSCFG_EXTICR1 ; \ SYSCFG_EXTICR1_EXTI0, EXTI 0 configuration bits
[then]

defined? use-SYSCFG defined? SYSCFG_EXTICR2_EXTI7 not and [if]
\ SYSCFG_EXTICR2 (read-write) Reset:0x00000000
: SYSCFG_EXTICR2_EXTI7 ( %bbb -- x addr ) 12 lshift SYSCFG_EXTICR2 ; \ SYSCFG_EXTICR2_EXTI7, EXTI 7 configuration bits
: SYSCFG_EXTICR2_EXTI6 ( %bbb -- x addr ) 8 lshift SYSCFG_EXTICR2 ; \ SYSCFG_EXTICR2_EXTI6, EXTI 6 configuration bits
: SYSCFG_EXTICR2_EXTI5 ( %bbb -- x addr ) 4 lshift SYSCFG_EXTICR2 ; \ SYSCFG_EXTICR2_EXTI5, EXTI 5 configuration bits
: SYSCFG_EXTICR2_EXTI4 ( %bbb -- x addr ) SYSCFG_EXTICR2 ; \ SYSCFG_EXTICR2_EXTI4, EXTI 4 configuration bits
[then]

execute-defined? use-SYSCFG defined? SYSCFG_EXTICR3_EXTI11 not and [if]
\ SYSCFG_EXTICR3 (read-write) Reset:0x00000000
: SYSCFG_EXTICR3_EXTI11 ( %bbb -- x addr ) 12 lshift SYSCFG_EXTICR3 ; \ SYSCFG_EXTICR3_EXTI11, EXTI 11 configuration bits
: SYSCFG_EXTICR3_EXTI10 ( %bbb -- x addr ) 8 lshift SYSCFG_EXTICR3 ; \ SYSCFG_EXTICR3_EXTI10, EXTI 10 configuration bits
: SYSCFG_EXTICR3_EXTI9 ( %bbb -- x addr ) 4 lshift SYSCFG_EXTICR3 ; \ SYSCFG_EXTICR3_EXTI9, EXTI 9 configuration bits
: SYSCFG_EXTICR3_EXTI8 ( %bbb -- x addr ) SYSCFG_EXTICR3 ; \ SYSCFG_EXTICR3_EXTI8, EXTI 8 configuration bits
[then]

defined? use-SYSCFG defined? SYSCFG_EXTICR4_EXTI15 not and [if]
\ SYSCFG_EXTICR4 (read-write) Reset:0x00000000
: SYSCFG_EXTICR4_EXTI15 ( %bbb -- x addr ) 12 lshift SYSCFG_EXTICR4 ; \ SYSCFG_EXTICR4_EXTI15, EXTI15 configuration bits
: SYSCFG_EXTICR4_EXTI14 ( %bbb -- x addr ) 8 lshift SYSCFG_EXTICR4 ; \ SYSCFG_EXTICR4_EXTI14, EXTI14 configuration bits
: SYSCFG_EXTICR4_EXTI13 ( %bbb -- x addr ) 4 lshift SYSCFG_EXTICR4 ; \ SYSCFG_EXTICR4_EXTI13, EXTI13 configuration bits
: SYSCFG_EXTICR4_EXTI12 ( %bbb -- x addr ) SYSCFG_EXTICR4 ; \ SYSCFG_EXTICR4_EXTI12, EXTI12 configuration bits
[then]

execute-defined? use-SYSCFG defined? SYSCFG_SCSR_SRAM2BSY not and [if]
\ SYSCFG_SCSR (multiple-access)  Reset:0x00000000
: SYSCFG_SCSR_SRAM2BSY ( -- x addr ) 1 bit SYSCFG_SCSR ; \ SYSCFG_SCSR_SRAM2BSY, SRAM2 busy by erase  operation
: SYSCFG_SCSR_SRAM2ER ( -- x addr ) 0 bit SYSCFG_SCSR ; \ SYSCFG_SCSR_SRAM2ER, SRAM2 Erase
[then]

defined? use-SYSCFG defined? SYSCFG_CFGR2_SPF? not and [if]
\ SYSCFG_CFGR2 (multiple-access)  Reset:0x00000000
: SYSCFG_CFGR2_SPF? ( -- 1|0 ) 8 bit SYSCFG_CFGR2 bit@ ; \ SYSCFG_CFGR2_SPF, SRAM2 parity error flag
: SYSCFG_CFGR2_ECCL ( -- x addr ) 3 bit SYSCFG_CFGR2 ; \ SYSCFG_CFGR2_ECCL, ECC Lock
: SYSCFG_CFGR2_PVDL ( -- x addr ) 2 bit SYSCFG_CFGR2 ; \ SYSCFG_CFGR2_PVDL, PVD lock enable bit
: SYSCFG_CFGR2_SPL ( -- x addr ) 1 bit SYSCFG_CFGR2 ; \ SYSCFG_CFGR2_SPL, SRAM2 parity lock bit
: SYSCFG_CFGR2_CLL ( -- x addr ) 0 bit SYSCFG_CFGR2 ; \ SYSCFG_CFGR2_CLL,   Cortex-M4  LOCKUP Hardfault output enable bit
[then]

execute-defined? use-SYSCFG defined? SYSCFG_SWPR_P31WP not and [if]
\ SYSCFG_SWPR (write-only) Reset:0x00000000
: SYSCFG_SWPR_P31WP ( -- x addr ) 31 bit SYSCFG_SWPR ; \ SYSCFG_SWPR_P31WP, SRAM2 page 31 write  protection
: SYSCFG_SWPR_P30WP ( -- x addr ) 30 bit SYSCFG_SWPR ; \ SYSCFG_SWPR_P30WP, P30WP
: SYSCFG_SWPR_P29WP ( -- x addr ) 29 bit SYSCFG_SWPR ; \ SYSCFG_SWPR_P29WP, P29WP
: SYSCFG_SWPR_P28WP ( -- x addr ) 28 bit SYSCFG_SWPR ; \ SYSCFG_SWPR_P28WP, P28WP
: SYSCFG_SWPR_P27WP ( -- x addr ) 27 bit SYSCFG_SWPR ; \ SYSCFG_SWPR_P27WP, P27WP
: SYSCFG_SWPR_P26WP ( -- x addr ) 26 bit SYSCFG_SWPR ; \ SYSCFG_SWPR_P26WP, P26WP
: SYSCFG_SWPR_P25WP ( -- x addr ) 25 bit SYSCFG_SWPR ; \ SYSCFG_SWPR_P25WP, P25WP
: SYSCFG_SWPR_P24WP ( -- x addr ) 24 bit SYSCFG_SWPR ; \ SYSCFG_SWPR_P24WP, P24WP
: SYSCFG_SWPR_P23WP ( -- x addr ) 23 bit SYSCFG_SWPR ; \ SYSCFG_SWPR_P23WP, P23WP
: SYSCFG_SWPR_P22WP ( -- x addr ) 22 bit SYSCFG_SWPR ; \ SYSCFG_SWPR_P22WP, P22WP
: SYSCFG_SWPR_P21WP ( -- x addr ) 21 bit SYSCFG_SWPR ; \ SYSCFG_SWPR_P21WP, P21WP
: SYSCFG_SWPR_P20WP ( -- x addr ) 20 bit SYSCFG_SWPR ; \ SYSCFG_SWPR_P20WP, P20WP
: SYSCFG_SWPR_P19WP ( -- x addr ) 19 bit SYSCFG_SWPR ; \ SYSCFG_SWPR_P19WP, P19WP
: SYSCFG_SWPR_P18WP ( -- x addr ) 18 bit SYSCFG_SWPR ; \ SYSCFG_SWPR_P18WP, P18WP
: SYSCFG_SWPR_P17WP ( -- x addr ) 17 bit SYSCFG_SWPR ; \ SYSCFG_SWPR_P17WP, P17WP
: SYSCFG_SWPR_P16WP ( -- x addr ) 16 bit SYSCFG_SWPR ; \ SYSCFG_SWPR_P16WP, P16WP
: SYSCFG_SWPR_P15WP ( -- x addr ) 15 bit SYSCFG_SWPR ; \ SYSCFG_SWPR_P15WP, P15WP
: SYSCFG_SWPR_P14WP ( -- x addr ) 14 bit SYSCFG_SWPR ; \ SYSCFG_SWPR_P14WP, P14WP
: SYSCFG_SWPR_P13WP ( -- x addr ) 13 bit SYSCFG_SWPR ; \ SYSCFG_SWPR_P13WP, P13WP
: SYSCFG_SWPR_P12WP ( -- x addr ) 12 bit SYSCFG_SWPR ; \ SYSCFG_SWPR_P12WP, P12WP
: SYSCFG_SWPR_P11WP ( -- x addr ) 11 bit SYSCFG_SWPR ; \ SYSCFG_SWPR_P11WP, P11WP
: SYSCFG_SWPR_P10WP ( -- x addr ) 10 bit SYSCFG_SWPR ; \ SYSCFG_SWPR_P10WP, P10WP
: SYSCFG_SWPR_P9WP ( -- x addr ) 9 bit SYSCFG_SWPR ; \ SYSCFG_SWPR_P9WP, P9WP
: SYSCFG_SWPR_P8WP ( -- x addr ) 8 bit SYSCFG_SWPR ; \ SYSCFG_SWPR_P8WP, P8WP
: SYSCFG_SWPR_P7WP ( -- x addr ) 7 bit SYSCFG_SWPR ; \ SYSCFG_SWPR_P7WP, P7WP
: SYSCFG_SWPR_P6WP ( -- x addr ) 6 bit SYSCFG_SWPR ; \ SYSCFG_SWPR_P6WP, P6WP
: SYSCFG_SWPR_P5WP ( -- x addr ) 5 bit SYSCFG_SWPR ; \ SYSCFG_SWPR_P5WP, P5WP
: SYSCFG_SWPR_P4WP ( -- x addr ) 4 bit SYSCFG_SWPR ; \ SYSCFG_SWPR_P4WP, P4WP
: SYSCFG_SWPR_P3WP ( -- x addr ) 3 bit SYSCFG_SWPR ; \ SYSCFG_SWPR_P3WP, P3WP
: SYSCFG_SWPR_P2WP ( -- x addr ) 2 bit SYSCFG_SWPR ; \ SYSCFG_SWPR_P2WP, P2WP
: SYSCFG_SWPR_P1WP ( -- x addr ) 1 bit SYSCFG_SWPR ; \ SYSCFG_SWPR_P1WP, P1WP
: SYSCFG_SWPR_P0WP ( -- x addr ) 0 bit SYSCFG_SWPR ; \ SYSCFG_SWPR_P0WP, P0WP
[then]

defined? use-SYSCFG defined? SYSCFG_SKR_KEY not and [if]
\ SYSCFG_SKR (write-only) Reset:0x00000000
: SYSCFG_SKR_KEY ( %bbbbbbbb -- x addr ) SYSCFG_SKR ; \ SYSCFG_SKR_KEY, SRAM2 write protection key for software  erase
[then]

execute-defined? use-RNG defined? RNG_CR_IE not and [if]
\ RNG_CR (read-write) Reset:0x00000000
: RNG_CR_IE ( -- x addr ) 3 bit RNG_CR ; \ RNG_CR_IE, Interrupt enable
: RNG_CR_RNGEN ( -- x addr ) 2 bit RNG_CR ; \ RNG_CR_RNGEN, Random number generator  enable
[then]

defined? use-RNG defined? RNG_SR_SEIS? not and [if]
\ RNG_SR (multiple-access)  Reset:0x00000000
: RNG_SR_SEIS? ( -- 1|0 ) 6 bit RNG_SR bit@ ; \ RNG_SR_SEIS, Seed error interrupt  status
: RNG_SR_CEIS? ( -- 1|0 ) 5 bit RNG_SR bit@ ; \ RNG_SR_CEIS, Clock error interrupt  status
: RNG_SR_SECS? ( -- 1|0 ) 2 bit RNG_SR bit@ ; \ RNG_SR_SECS, Seed error current status
: RNG_SR_CECS? ( -- 1|0 ) 1 bit RNG_SR bit@ ; \ RNG_SR_CECS, Clock error current status
: RNG_SR_DRDY ( -- x addr ) 0 bit RNG_SR ; \ RNG_SR_DRDY, Data ready
[then]

execute-defined? use-RNG defined? RNG_DR_RNDATA? not and [if]
\ RNG_DR (read-only) Reset:0x00000000
: RNG_DR_RNDATA? ( --  x ) RNG_DR @ ; \ RNG_DR_RNDATA, Random data
[then]

defined? use-ADC1 defined? ADC1_ISR_JQOVF not and [if]
\ ADC1_ISR (read-write) Reset:0x00000000
: ADC1_ISR_JQOVF ( -- x addr ) 10 bit ADC1_ISR ; \ ADC1_ISR_JQOVF, JQOVF
: ADC1_ISR_AWD3 ( -- x addr ) 9 bit ADC1_ISR ; \ ADC1_ISR_AWD3, AWD3
: ADC1_ISR_AWD2 ( -- x addr ) 8 bit ADC1_ISR ; \ ADC1_ISR_AWD2, AWD2
: ADC1_ISR_AWD1 ( -- x addr ) 7 bit ADC1_ISR ; \ ADC1_ISR_AWD1, AWD1
: ADC1_ISR_JEOS ( -- x addr ) 6 bit ADC1_ISR ; \ ADC1_ISR_JEOS, JEOS
: ADC1_ISR_JEOC ( -- x addr ) 5 bit ADC1_ISR ; \ ADC1_ISR_JEOC, JEOC
: ADC1_ISR_OVR ( -- x addr ) 4 bit ADC1_ISR ; \ ADC1_ISR_OVR, OVR
: ADC1_ISR_EOS ( -- x addr ) 3 bit ADC1_ISR ; \ ADC1_ISR_EOS, EOS
: ADC1_ISR_EOC ( -- x addr ) 2 bit ADC1_ISR ; \ ADC1_ISR_EOC, EOC
: ADC1_ISR_EOSMP ( -- x addr ) 1 bit ADC1_ISR ; \ ADC1_ISR_EOSMP, EOSMP
: ADC1_ISR_ADRDY ( -- x addr ) 0 bit ADC1_ISR ; \ ADC1_ISR_ADRDY, ADRDY
[then]

execute-defined? use-ADC1 defined? ADC1_IER_JQOVFIE not and [if]
\ ADC1_IER (read-write) Reset:0x00000000
: ADC1_IER_JQOVFIE ( -- x addr ) 10 bit ADC1_IER ; \ ADC1_IER_JQOVFIE, JQOVFIE
: ADC1_IER_AWD3IE ( -- x addr ) 9 bit ADC1_IER ; \ ADC1_IER_AWD3IE, AWD3IE
: ADC1_IER_AWD2IE ( -- x addr ) 8 bit ADC1_IER ; \ ADC1_IER_AWD2IE, AWD2IE
: ADC1_IER_AWD1IE ( -- x addr ) 7 bit ADC1_IER ; \ ADC1_IER_AWD1IE, AWD1IE
: ADC1_IER_JEOSIE ( -- x addr ) 6 bit ADC1_IER ; \ ADC1_IER_JEOSIE, JEOSIE
: ADC1_IER_JEOCIE ( -- x addr ) 5 bit ADC1_IER ; \ ADC1_IER_JEOCIE, JEOCIE
: ADC1_IER_OVRIE ( -- x addr ) 4 bit ADC1_IER ; \ ADC1_IER_OVRIE, OVRIE
: ADC1_IER_EOSIE ( -- x addr ) 3 bit ADC1_IER ; \ ADC1_IER_EOSIE, EOSIE
: ADC1_IER_EOCIE ( -- x addr ) 2 bit ADC1_IER ; \ ADC1_IER_EOCIE, EOCIE
: ADC1_IER_EOSMPIE ( -- x addr ) 1 bit ADC1_IER ; \ ADC1_IER_EOSMPIE, EOSMPIE
: ADC1_IER_ADRDYIE ( -- x addr ) 0 bit ADC1_IER ; \ ADC1_IER_ADRDYIE, ADRDYIE
[then]

defined? use-ADC1 defined? ADC1_CR_ADCAL not and [if]
\ ADC1_CR (read-write) Reset:0x00000000
: ADC1_CR_ADCAL ( -- x addr ) 31 bit ADC1_CR ; \ ADC1_CR_ADCAL, ADCAL
: ADC1_CR_ADCALDIF ( -- x addr ) 30 bit ADC1_CR ; \ ADC1_CR_ADCALDIF, ADCALDIF
: ADC1_CR_DEEPPWD ( -- x addr ) 29 bit ADC1_CR ; \ ADC1_CR_DEEPPWD, DEEPPWD
: ADC1_CR_ADVREGEN ( -- x addr ) 28 bit ADC1_CR ; \ ADC1_CR_ADVREGEN, ADVREGEN
: ADC1_CR_JADSTP ( -- x addr ) 5 bit ADC1_CR ; \ ADC1_CR_JADSTP, JADSTP
: ADC1_CR_ADSTP ( -- x addr ) 4 bit ADC1_CR ; \ ADC1_CR_ADSTP, ADSTP
: ADC1_CR_JADSTART ( -- x addr ) 3 bit ADC1_CR ; \ ADC1_CR_JADSTART, JADSTART
: ADC1_CR_ADSTART ( -- x addr ) 2 bit ADC1_CR ; \ ADC1_CR_ADSTART, ADSTART
: ADC1_CR_ADDIS ( -- x addr ) 1 bit ADC1_CR ; \ ADC1_CR_ADDIS, ADDIS
: ADC1_CR_ADEN ( -- x addr ) 0 bit ADC1_CR ; \ ADC1_CR_ADEN, ADEN
[then]

execute-defined? use-ADC1 defined? ADC1_CFGR_AWDCH1CH not and [if]
\ ADC1_CFGR (read-write) Reset:0x00000000
: ADC1_CFGR_AWDCH1CH ( %bbbbb -- x addr ) 26 lshift ADC1_CFGR ; \ ADC1_CFGR_AWDCH1CH, AWDCH1CH
: ADC1_CFGR_JAUTO ( -- x addr ) 25 bit ADC1_CFGR ; \ ADC1_CFGR_JAUTO, JAUTO
: ADC1_CFGR_JAWD1EN ( -- x addr ) 24 bit ADC1_CFGR ; \ ADC1_CFGR_JAWD1EN, JAWD1EN
: ADC1_CFGR_AWD1EN ( -- x addr ) 23 bit ADC1_CFGR ; \ ADC1_CFGR_AWD1EN, AWD1EN
: ADC1_CFGR_AWD1SGL ( -- x addr ) 22 bit ADC1_CFGR ; \ ADC1_CFGR_AWD1SGL, AWD1SGL
: ADC1_CFGR_JQM ( -- x addr ) 21 bit ADC1_CFGR ; \ ADC1_CFGR_JQM, JQM
: ADC1_CFGR_JDISCEN ( -- x addr ) 20 bit ADC1_CFGR ; \ ADC1_CFGR_JDISCEN, JDISCEN
: ADC1_CFGR_DISCNUM ( %bbb -- x addr ) 17 lshift ADC1_CFGR ; \ ADC1_CFGR_DISCNUM, DISCNUM
: ADC1_CFGR_DISCEN ( -- x addr ) 16 bit ADC1_CFGR ; \ ADC1_CFGR_DISCEN, DISCEN
: ADC1_CFGR_AUTOFF ( -- x addr ) 15 bit ADC1_CFGR ; \ ADC1_CFGR_AUTOFF, AUTOFF
: ADC1_CFGR_AUTDLY ( -- x addr ) 14 bit ADC1_CFGR ; \ ADC1_CFGR_AUTDLY, AUTDLY
: ADC1_CFGR_CONT ( -- x addr ) 13 bit ADC1_CFGR ; \ ADC1_CFGR_CONT, CONT
: ADC1_CFGR_OVRMOD ( -- x addr ) 12 bit ADC1_CFGR ; \ ADC1_CFGR_OVRMOD, OVRMOD
: ADC1_CFGR_EXTEN ( %bb -- x addr ) 10 lshift ADC1_CFGR ; \ ADC1_CFGR_EXTEN, EXTEN
: ADC1_CFGR_EXTSEL ( %bbbb -- x addr ) 6 lshift ADC1_CFGR ; \ ADC1_CFGR_EXTSEL, EXTSEL
: ADC1_CFGR_ALIGN ( -- x addr ) 5 bit ADC1_CFGR ; \ ADC1_CFGR_ALIGN, ALIGN
: ADC1_CFGR_RES ( %bb -- x addr ) 3 lshift ADC1_CFGR ; \ ADC1_CFGR_RES, RES
: ADC1_CFGR_DMACFG ( -- x addr ) 1 bit ADC1_CFGR ; \ ADC1_CFGR_DMACFG, DMACFG
: ADC1_CFGR_DMAEN ( -- x addr ) 0 bit ADC1_CFGR ; \ ADC1_CFGR_DMAEN, DMAEN
[then]

defined? use-ADC1 defined? ADC1_CFGR2_ROVSM not and [if]
\ ADC1_CFGR2 (read-write) Reset:0x00000000
: ADC1_CFGR2_ROVSM ( -- x addr ) 10 bit ADC1_CFGR2 ; \ ADC1_CFGR2_ROVSM, EXTEN
: ADC1_CFGR2_TOVS ( -- x addr ) 9 bit ADC1_CFGR2 ; \ ADC1_CFGR2_TOVS, EXTSEL
: ADC1_CFGR2_OVSS ( %bbbb -- x addr ) 5 lshift ADC1_CFGR2 ; \ ADC1_CFGR2_OVSS, ALIGN
: ADC1_CFGR2_OVSR ( %bbb -- x addr ) 2 lshift ADC1_CFGR2 ; \ ADC1_CFGR2_OVSR, RES
: ADC1_CFGR2_JOVSE ( -- x addr ) 1 bit ADC1_CFGR2 ; \ ADC1_CFGR2_JOVSE, DMACFG
: ADC1_CFGR2_ROVSE ( -- x addr ) 0 bit ADC1_CFGR2 ; \ ADC1_CFGR2_ROVSE, DMAEN
[then]

execute-defined? use-ADC1 defined? ADC1_SMPR1_SMP9 not and [if]
\ ADC1_SMPR1 (read-write) Reset:0x00000000
: ADC1_SMPR1_SMP9 ( %bbb -- x addr ) 27 lshift ADC1_SMPR1 ; \ ADC1_SMPR1_SMP9, SMP9
: ADC1_SMPR1_SMP8 ( %bbb -- x addr ) 24 lshift ADC1_SMPR1 ; \ ADC1_SMPR1_SMP8, SMP8
: ADC1_SMPR1_SMP7 ( %bbb -- x addr ) 21 lshift ADC1_SMPR1 ; \ ADC1_SMPR1_SMP7, SMP7
: ADC1_SMPR1_SMP6 ( %bbb -- x addr ) 18 lshift ADC1_SMPR1 ; \ ADC1_SMPR1_SMP6, SMP6
: ADC1_SMPR1_SMP5 ( %bbb -- x addr ) 15 lshift ADC1_SMPR1 ; \ ADC1_SMPR1_SMP5, SMP5
: ADC1_SMPR1_SMP4 ( %bbb -- x addr ) 12 lshift ADC1_SMPR1 ; \ ADC1_SMPR1_SMP4, SMP4
: ADC1_SMPR1_SMP3 ( %bbb -- x addr ) 9 lshift ADC1_SMPR1 ; \ ADC1_SMPR1_SMP3, SMP3
: ADC1_SMPR1_SMP2 ( %bbb -- x addr ) 6 lshift ADC1_SMPR1 ; \ ADC1_SMPR1_SMP2, SMP2
: ADC1_SMPR1_SMP1 ( %bbb -- x addr ) 3 lshift ADC1_SMPR1 ; \ ADC1_SMPR1_SMP1, SMP1
[then]

defined? use-ADC1 defined? ADC1_SMPR2_SMP18 not and [if]
\ ADC1_SMPR2 (read-write) Reset:0x00000000
: ADC1_SMPR2_SMP18 ( %bbb -- x addr ) 24 lshift ADC1_SMPR2 ; \ ADC1_SMPR2_SMP18, SMP18
: ADC1_SMPR2_SMP17 ( %bbb -- x addr ) 21 lshift ADC1_SMPR2 ; \ ADC1_SMPR2_SMP17, SMP17
: ADC1_SMPR2_SMP16 ( %bbb -- x addr ) 18 lshift ADC1_SMPR2 ; \ ADC1_SMPR2_SMP16, SMP16
: ADC1_SMPR2_SMP15 ( %bbb -- x addr ) 15 lshift ADC1_SMPR2 ; \ ADC1_SMPR2_SMP15, SMP15
: ADC1_SMPR2_SMP14 ( %bbb -- x addr ) 12 lshift ADC1_SMPR2 ; \ ADC1_SMPR2_SMP14, SMP14
: ADC1_SMPR2_SMP13 ( %bbb -- x addr ) 9 lshift ADC1_SMPR2 ; \ ADC1_SMPR2_SMP13, SMP13
: ADC1_SMPR2_SMP12 ( %bbb -- x addr ) 6 lshift ADC1_SMPR2 ; \ ADC1_SMPR2_SMP12, SMP12
: ADC1_SMPR2_SMP11 ( %bbb -- x addr ) 3 lshift ADC1_SMPR2 ; \ ADC1_SMPR2_SMP11, SMP11
: ADC1_SMPR2_SMP10 ( %bbb -- x addr ) ADC1_SMPR2 ; \ ADC1_SMPR2_SMP10, SMP10
[then]

execute-defined? use-ADC1 defined? ADC1_TR1_HT1 not and [if]
\ ADC1_TR1 (read-write) Reset:0x0FFF0000
: ADC1_TR1_HT1 ( %bbbbbbbbbbb -- x addr ) 16 lshift ADC1_TR1 ; \ ADC1_TR1_HT1, HT1
: ADC1_TR1_LT1 ( %bbbbbbbbbbb -- x addr ) ADC1_TR1 ; \ ADC1_TR1_LT1, LT1
[then]

defined? use-ADC1 defined? ADC1_TR2_HT2 not and [if]
\ ADC1_TR2 (read-write) Reset:0x0FFF0000
: ADC1_TR2_HT2 ( %bbbbbbbb -- x addr ) 16 lshift ADC1_TR2 ; \ ADC1_TR2_HT2, HT2
: ADC1_TR2_LT2 ( %bbbbbbbb -- x addr ) ADC1_TR2 ; \ ADC1_TR2_LT2, LT2
[then]

execute-defined? use-ADC1 defined? ADC1_TR3_HT3 not and [if]
\ ADC1_TR3 (read-write) Reset:0x0FFF0000
: ADC1_TR3_HT3 ( %bbbbbbbb -- x addr ) 16 lshift ADC1_TR3 ; \ ADC1_TR3_HT3, HT3
: ADC1_TR3_LT3 ( %bbbbbbbb -- x addr ) ADC1_TR3 ; \ ADC1_TR3_LT3, LT3
[then]

defined? use-ADC1 defined? ADC1_SQR1_SQ4 not and [if]
\ ADC1_SQR1 (read-write) Reset:0x00000000
: ADC1_SQR1_SQ4 ( %bbbbb -- x addr ) 24 lshift ADC1_SQR1 ; \ ADC1_SQR1_SQ4, SQ4
: ADC1_SQR1_SQ3 ( %bbbbb -- x addr ) 18 lshift ADC1_SQR1 ; \ ADC1_SQR1_SQ3, SQ3
: ADC1_SQR1_SQ2 ( %bbbbb -- x addr ) 12 lshift ADC1_SQR1 ; \ ADC1_SQR1_SQ2, SQ2
: ADC1_SQR1_SQ1 ( %bbbbb -- x addr ) 6 lshift ADC1_SQR1 ; \ ADC1_SQR1_SQ1, SQ1
: ADC1_SQR1_L3 ( %bbbb -- x addr ) ADC1_SQR1 ; \ ADC1_SQR1_L3, L3
[then]

execute-defined? use-ADC1 defined? ADC1_SQR2_SQ9 not and [if]
\ ADC1_SQR2 (read-write) Reset:0x00000000
: ADC1_SQR2_SQ9 ( %bbbbb -- x addr ) 24 lshift ADC1_SQR2 ; \ ADC1_SQR2_SQ9, SQ9
: ADC1_SQR2_SQ8 ( %bbbbb -- x addr ) 18 lshift ADC1_SQR2 ; \ ADC1_SQR2_SQ8, SQ8
: ADC1_SQR2_SQ7 ( %bbbbb -- x addr ) 12 lshift ADC1_SQR2 ; \ ADC1_SQR2_SQ7, SQ7
: ADC1_SQR2_SQ6 ( %bbbbb -- x addr ) 6 lshift ADC1_SQR2 ; \ ADC1_SQR2_SQ6, SQ6
: ADC1_SQR2_SQ5 ( %bbbbb -- x addr ) ADC1_SQR2 ; \ ADC1_SQR2_SQ5, SQ5
[then]

defined? use-ADC1 defined? ADC1_SQR3_SQ14 not and [if]
\ ADC1_SQR3 (read-write) Reset:0x00000000
: ADC1_SQR3_SQ14 ( %bbbbb -- x addr ) 24 lshift ADC1_SQR3 ; \ ADC1_SQR3_SQ14, SQ14
: ADC1_SQR3_SQ13 ( %bbbbb -- x addr ) 18 lshift ADC1_SQR3 ; \ ADC1_SQR3_SQ13, SQ13
: ADC1_SQR3_SQ12 ( %bbbbb -- x addr ) 12 lshift ADC1_SQR3 ; \ ADC1_SQR3_SQ12, SQ12
: ADC1_SQR3_SQ11 ( %bbbbb -- x addr ) 6 lshift ADC1_SQR3 ; \ ADC1_SQR3_SQ11, SQ11
: ADC1_SQR3_SQ10 ( %bbbbb -- x addr ) ADC1_SQR3 ; \ ADC1_SQR3_SQ10, SQ10
[then]

execute-defined? use-ADC1 defined? ADC1_SQR4_SQ16 not and [if]
\ ADC1_SQR4 (read-write) Reset:0x00000000
: ADC1_SQR4_SQ16 ( %bbbbb -- x addr ) 6 lshift ADC1_SQR4 ; \ ADC1_SQR4_SQ16, SQ16
: ADC1_SQR4_SQ15 ( %bbbbb -- x addr ) ADC1_SQR4 ; \ ADC1_SQR4_SQ15, SQ15
[then]

defined? use-ADC1 defined? ADC1_DR_regularDATA? not and [if]
\ ADC1_DR (read-only) Reset:0x00000000
: ADC1_DR_regularDATA? ( --  x ) ADC1_DR @ ; \ ADC1_DR_regularDATA, regularDATA
[then]

execute-defined? use-ADC1 defined? ADC1_JSQR_JSQ4 not and [if]
\ ADC1_JSQR (read-write) Reset:0x00000000
: ADC1_JSQR_JSQ4 ( %bbbbb -- x addr ) 26 lshift ADC1_JSQR ; \ ADC1_JSQR_JSQ4, JSQ4
: ADC1_JSQR_JSQ3 ( %bbbbb -- x addr ) 20 lshift ADC1_JSQR ; \ ADC1_JSQR_JSQ3, JSQ3
: ADC1_JSQR_JSQ2 ( %bbbbb -- x addr ) 14 lshift ADC1_JSQR ; \ ADC1_JSQR_JSQ2, JSQ2
: ADC1_JSQR_JSQ1 ( %bbbbb -- x addr ) 8 lshift ADC1_JSQR ; \ ADC1_JSQR_JSQ1, JSQ1
: ADC1_JSQR_JEXTEN ( %bb -- x addr ) 6 lshift ADC1_JSQR ; \ ADC1_JSQR_JEXTEN, JEXTEN
: ADC1_JSQR_JEXTSEL ( %bbbb -- x addr ) 2 lshift ADC1_JSQR ; \ ADC1_JSQR_JEXTSEL, JEXTSEL
: ADC1_JSQR_JL ( %bb -- x addr ) ADC1_JSQR ; \ ADC1_JSQR_JL, JL
[then]

defined? use-ADC1 defined? ADC1_OFR1_OFFSET1_EN not and [if]
\ ADC1_OFR1 (read-write) Reset:0x00000000
: ADC1_OFR1_OFFSET1_EN ( -- x addr ) 31 bit ADC1_OFR1 ; \ ADC1_OFR1_OFFSET1_EN, OFFSET1_EN
: ADC1_OFR1_OFFSET1_CH ( %bbbbb -- x addr ) 26 lshift ADC1_OFR1 ; \ ADC1_OFR1_OFFSET1_CH, OFFSET1_CH
: ADC1_OFR1_OFFSET1 ( %bbbbbbbbbbb -- x addr ) ADC1_OFR1 ; \ ADC1_OFR1_OFFSET1, OFFSET1
[then]

execute-defined? use-ADC1 defined? ADC1_OFR2_OFFSET2_EN not and [if]
\ ADC1_OFR2 (read-write) Reset:0x00000000
: ADC1_OFR2_OFFSET2_EN ( -- x addr ) 31 bit ADC1_OFR2 ; \ ADC1_OFR2_OFFSET2_EN, OFFSET2_EN
: ADC1_OFR2_OFFSET2_CH ( %bbbbb -- x addr ) 26 lshift ADC1_OFR2 ; \ ADC1_OFR2_OFFSET2_CH, OFFSET2_CH
: ADC1_OFR2_OFFSET2 ( %bbbbbbbbbbb -- x addr ) ADC1_OFR2 ; \ ADC1_OFR2_OFFSET2, OFFSET2
[then]

defined? use-ADC1 defined? ADC1_OFR3_OFFSET3_EN not and [if]
\ ADC1_OFR3 (read-write) Reset:0x00000000
: ADC1_OFR3_OFFSET3_EN ( -- x addr ) 31 bit ADC1_OFR3 ; \ ADC1_OFR3_OFFSET3_EN, OFFSET3_EN
: ADC1_OFR3_OFFSET3_CH ( %bbbbb -- x addr ) 26 lshift ADC1_OFR3 ; \ ADC1_OFR3_OFFSET3_CH, OFFSET3_CH
: ADC1_OFR3_OFFSET3 ( %bbbbbbbbbbb -- x addr ) ADC1_OFR3 ; \ ADC1_OFR3_OFFSET3, OFFSET3
[then]

execute-defined? use-ADC1 defined? ADC1_OFR4_OFFSET4_EN not and [if]
\ ADC1_OFR4 (read-write) Reset:0x00000000
: ADC1_OFR4_OFFSET4_EN ( -- x addr ) 31 bit ADC1_OFR4 ; \ ADC1_OFR4_OFFSET4_EN, OFFSET4_EN
: ADC1_OFR4_OFFSET4_CH ( %bbbbb -- x addr ) 26 lshift ADC1_OFR4 ; \ ADC1_OFR4_OFFSET4_CH, OFFSET4_CH
: ADC1_OFR4_OFFSET4 ( %bbbbbbbbbbb -- x addr ) ADC1_OFR4 ; \ ADC1_OFR4_OFFSET4, OFFSET4
[then]

defined? use-ADC1 defined? ADC1_JDR1_JDATA1? not and [if]
\ ADC1_JDR1 (read-only) Reset:0x00000000
: ADC1_JDR1_JDATA1? ( --  x ) ADC1_JDR1 @ ; \ ADC1_JDR1_JDATA1, JDATA1
[then]

execute-defined? use-ADC1 defined? ADC1_JDR2_JDATA2? not and [if]
\ ADC1_JDR2 (read-only) Reset:0x00000000
: ADC1_JDR2_JDATA2? ( --  x ) ADC1_JDR2 @ ; \ ADC1_JDR2_JDATA2, JDATA2
[then]

defined? use-ADC1 defined? ADC1_JDR3_JDATA3? not and [if]
\ ADC1_JDR3 (read-only) Reset:0x00000000
: ADC1_JDR3_JDATA3? ( --  x ) ADC1_JDR3 @ ; \ ADC1_JDR3_JDATA3, JDATA3
[then]

execute-defined? use-ADC1 defined? ADC1_JDR4_JDATA4? not and [if]
\ ADC1_JDR4 (read-only) Reset:0x00000000
: ADC1_JDR4_JDATA4? ( --  x ) ADC1_JDR4 @ ; \ ADC1_JDR4_JDATA4, JDATA4
[then]

defined? use-ADC1 defined? ADC1_AWD2CR_AWD2CH not and [if]
\ ADC1_AWD2CR (read-write) Reset:0x00000000
: ADC1_AWD2CR_AWD2CH x addr ) 1 lshift ADC1_AWD2CR ; \ ADC1_AWD2CR_AWD2CH, AWD2CH
[then]

execute-defined? use-ADC1 defined? ADC1_AWD3CR_AWD3CH not and [if]
\ ADC1_AWD3CR (read-write) Reset:0x00000000
: ADC1_AWD3CR_AWD3CH x addr ) 1 lshift ADC1_AWD3CR ; \ ADC1_AWD3CR_AWD3CH, AWD3CH
[then]

defined? use-ADC1 defined? ADC1_DIFSEL_DIFSEL_1_15 not and [if]
\ ADC1_DIFSEL (multiple-access)  Reset:0x00000000
: ADC1_DIFSEL_DIFSEL_1_15 ( %bbbbbbbbbbbbbbb -- x addr ) 1 lshift ADC1_DIFSEL ; \ ADC1_DIFSEL_DIFSEL_1_15, Differential mode for channels 15 to  1
: ADC1_DIFSEL_DIFSEL_16_18 ( %bbb -- x addr ) 16 lshift ADC1_DIFSEL ; \ ADC1_DIFSEL_DIFSEL_16_18, Differential mode for channels 18 to  16
[then]

execute-defined? use-ADC1 defined? ADC1_CALFACT_CALFACT_D not and [if]
\ ADC1_CALFACT (read-write) Reset:0x00000000
: ADC1_CALFACT_CALFACT_D ( %bbbbbbb -- x addr ) 16 lshift ADC1_CALFACT ; \ ADC1_CALFACT_CALFACT_D, CALFACT_D
: ADC1_CALFACT_CALFACT_S ( %bbbbbbb -- x addr ) ADC1_CALFACT ; \ ADC1_CALFACT_CALFACT_S, CALFACT_S
[then]

defined? use-ADC2 defined? ADC2_ISR_JQOVF not and [if]
\ ADC2_ISR (read-write) Reset:0x00000000
: ADC2_ISR_JQOVF ( -- x addr ) 10 bit ADC2_ISR ; \ ADC2_ISR_JQOVF, JQOVF
: ADC2_ISR_AWD3 ( -- x addr ) 9 bit ADC2_ISR ; \ ADC2_ISR_AWD3, AWD3
: ADC2_ISR_AWD2 ( -- x addr ) 8 bit ADC2_ISR ; \ ADC2_ISR_AWD2, AWD2
: ADC2_ISR_AWD1 ( -- x addr ) 7 bit ADC2_ISR ; \ ADC2_ISR_AWD1, AWD1
: ADC2_ISR_JEOS ( -- x addr ) 6 bit ADC2_ISR ; \ ADC2_ISR_JEOS, JEOS
: ADC2_ISR_JEOC ( -- x addr ) 5 bit ADC2_ISR ; \ ADC2_ISR_JEOC, JEOC
: ADC2_ISR_OVR ( -- x addr ) 4 bit ADC2_ISR ; \ ADC2_ISR_OVR, OVR
: ADC2_ISR_EOS ( -- x addr ) 3 bit ADC2_ISR ; \ ADC2_ISR_EOS, EOS
: ADC2_ISR_EOC ( -- x addr ) 2 bit ADC2_ISR ; \ ADC2_ISR_EOC, EOC
: ADC2_ISR_EOSMP ( -- x addr ) 1 bit ADC2_ISR ; \ ADC2_ISR_EOSMP, EOSMP
: ADC2_ISR_ADRDY ( -- x addr ) 0 bit ADC2_ISR ; \ ADC2_ISR_ADRDY, ADRDY
[then]

execute-defined? use-ADC2 defined? ADC2_IER_JQOVFIE not and [if]
\ ADC2_IER (read-write) Reset:0x00000000
: ADC2_IER_JQOVFIE ( -- x addr ) 10 bit ADC2_IER ; \ ADC2_IER_JQOVFIE, JQOVFIE
: ADC2_IER_AWD3IE ( -- x addr ) 9 bit ADC2_IER ; \ ADC2_IER_AWD3IE, AWD3IE
: ADC2_IER_AWD2IE ( -- x addr ) 8 bit ADC2_IER ; \ ADC2_IER_AWD2IE, AWD2IE
: ADC2_IER_AWD1IE ( -- x addr ) 7 bit ADC2_IER ; \ ADC2_IER_AWD1IE, AWD1IE
: ADC2_IER_JEOSIE ( -- x addr ) 6 bit ADC2_IER ; \ ADC2_IER_JEOSIE, JEOSIE
: ADC2_IER_JEOCIE ( -- x addr ) 5 bit ADC2_IER ; \ ADC2_IER_JEOCIE, JEOCIE
: ADC2_IER_OVRIE ( -- x addr ) 4 bit ADC2_IER ; \ ADC2_IER_OVRIE, OVRIE
: ADC2_IER_EOSIE ( -- x addr ) 3 bit ADC2_IER ; \ ADC2_IER_EOSIE, EOSIE
: ADC2_IER_EOCIE ( -- x addr ) 2 bit ADC2_IER ; \ ADC2_IER_EOCIE, EOCIE
: ADC2_IER_EOSMPIE ( -- x addr ) 1 bit ADC2_IER ; \ ADC2_IER_EOSMPIE, EOSMPIE
: ADC2_IER_ADRDYIE ( -- x addr ) 0 bit ADC2_IER ; \ ADC2_IER_ADRDYIE, ADRDYIE
[then]

defined? use-ADC2 defined? ADC2_CR_ADCAL not and [if]
\ ADC2_CR (read-write) Reset:0x00000000
: ADC2_CR_ADCAL ( -- x addr ) 31 bit ADC2_CR ; \ ADC2_CR_ADCAL, ADCAL
: ADC2_CR_ADCALDIF ( -- x addr ) 30 bit ADC2_CR ; \ ADC2_CR_ADCALDIF, ADCALDIF
: ADC2_CR_DEEPPWD ( -- x addr ) 29 bit ADC2_CR ; \ ADC2_CR_DEEPPWD, DEEPPWD
: ADC2_CR_ADVREGEN ( -- x addr ) 28 bit ADC2_CR ; \ ADC2_CR_ADVREGEN, ADVREGEN
: ADC2_CR_JADSTP ( -- x addr ) 5 bit ADC2_CR ; \ ADC2_CR_JADSTP, JADSTP
: ADC2_CR_ADSTP ( -- x addr ) 4 bit ADC2_CR ; \ ADC2_CR_ADSTP, ADSTP
: ADC2_CR_JADSTART ( -- x addr ) 3 bit ADC2_CR ; \ ADC2_CR_JADSTART, JADSTART
: ADC2_CR_ADSTART ( -- x addr ) 2 bit ADC2_CR ; \ ADC2_CR_ADSTART, ADSTART
: ADC2_CR_ADDIS ( -- x addr ) 1 bit ADC2_CR ; \ ADC2_CR_ADDIS, ADDIS
: ADC2_CR_ADEN ( -- x addr ) 0 bit ADC2_CR ; \ ADC2_CR_ADEN, ADEN
[then]

execute-defined? use-ADC2 defined? ADC2_CFGR_AWDCH1CH not and [if]
\ ADC2_CFGR (read-write) Reset:0x00000000
: ADC2_CFGR_AWDCH1CH ( %bbbbb -- x addr ) 26 lshift ADC2_CFGR ; \ ADC2_CFGR_AWDCH1CH, AWDCH1CH
: ADC2_CFGR_JAUTO ( -- x addr ) 25 bit ADC2_CFGR ; \ ADC2_CFGR_JAUTO, JAUTO
: ADC2_CFGR_JAWD1EN ( -- x addr ) 24 bit ADC2_CFGR ; \ ADC2_CFGR_JAWD1EN, JAWD1EN
: ADC2_CFGR_AWD1EN ( -- x addr ) 23 bit ADC2_CFGR ; \ ADC2_CFGR_AWD1EN, AWD1EN
: ADC2_CFGR_AWD1SGL ( -- x addr ) 22 bit ADC2_CFGR ; \ ADC2_CFGR_AWD1SGL, AWD1SGL
: ADC2_CFGR_JQM ( -- x addr ) 21 bit ADC2_CFGR ; \ ADC2_CFGR_JQM, JQM
: ADC2_CFGR_JDISCEN ( -- x addr ) 20 bit ADC2_CFGR ; \ ADC2_CFGR_JDISCEN, JDISCEN
: ADC2_CFGR_DISCNUM ( %bbb -- x addr ) 17 lshift ADC2_CFGR ; \ ADC2_CFGR_DISCNUM, DISCNUM
: ADC2_CFGR_DISCEN ( -- x addr ) 16 bit ADC2_CFGR ; \ ADC2_CFGR_DISCEN, DISCEN
: ADC2_CFGR_AUTOFF ( -- x addr ) 15 bit ADC2_CFGR ; \ ADC2_CFGR_AUTOFF, AUTOFF
: ADC2_CFGR_AUTDLY ( -- x addr ) 14 bit ADC2_CFGR ; \ ADC2_CFGR_AUTDLY, AUTDLY
: ADC2_CFGR_CONT ( -- x addr ) 13 bit ADC2_CFGR ; \ ADC2_CFGR_CONT, CONT
: ADC2_CFGR_OVRMOD ( -- x addr ) 12 bit ADC2_CFGR ; \ ADC2_CFGR_OVRMOD, OVRMOD
: ADC2_CFGR_EXTEN ( %bb -- x addr ) 10 lshift ADC2_CFGR ; \ ADC2_CFGR_EXTEN, EXTEN
: ADC2_CFGR_EXTSEL ( %bbbb -- x addr ) 6 lshift ADC2_CFGR ; \ ADC2_CFGR_EXTSEL, EXTSEL
: ADC2_CFGR_ALIGN ( -- x addr ) 5 bit ADC2_CFGR ; \ ADC2_CFGR_ALIGN, ALIGN
: ADC2_CFGR_RES ( %bb -- x addr ) 3 lshift ADC2_CFGR ; \ ADC2_CFGR_RES, RES
: ADC2_CFGR_DMACFG ( -- x addr ) 1 bit ADC2_CFGR ; \ ADC2_CFGR_DMACFG, DMACFG
: ADC2_CFGR_DMAEN ( -- x addr ) 0 bit ADC2_CFGR ; \ ADC2_CFGR_DMAEN, DMAEN
[then]

defined? use-ADC2 defined? ADC2_CFGR2_ROVSM not and [if]
\ ADC2_CFGR2 (read-write) Reset:0x00000000
: ADC2_CFGR2_ROVSM ( -- x addr ) 10 bit ADC2_CFGR2 ; \ ADC2_CFGR2_ROVSM, EXTEN
: ADC2_CFGR2_TOVS ( -- x addr ) 9 bit ADC2_CFGR2 ; \ ADC2_CFGR2_TOVS, EXTSEL
: ADC2_CFGR2_OVSS ( %bbbb -- x addr ) 5 lshift ADC2_CFGR2 ; \ ADC2_CFGR2_OVSS, ALIGN
: ADC2_CFGR2_OVSR ( %bbb -- x addr ) 2 lshift ADC2_CFGR2 ; \ ADC2_CFGR2_OVSR, RES
: ADC2_CFGR2_JOVSE ( -- x addr ) 1 bit ADC2_CFGR2 ; \ ADC2_CFGR2_JOVSE, DMACFG
: ADC2_CFGR2_ROVSE ( -- x addr ) 0 bit ADC2_CFGR2 ; \ ADC2_CFGR2_ROVSE, DMAEN
[then]

execute-defined? use-ADC2 defined? ADC2_SMPR1_SMP9 not and [if]
\ ADC2_SMPR1 (read-write) Reset:0x00000000
: ADC2_SMPR1_SMP9 ( %bbb -- x addr ) 27 lshift ADC2_SMPR1 ; \ ADC2_SMPR1_SMP9, SMP9
: ADC2_SMPR1_SMP8 ( %bbb -- x addr ) 24 lshift ADC2_SMPR1 ; \ ADC2_SMPR1_SMP8, SMP8
: ADC2_SMPR1_SMP7 ( %bbb -- x addr ) 21 lshift ADC2_SMPR1 ; \ ADC2_SMPR1_SMP7, SMP7
: ADC2_SMPR1_SMP6 ( %bbb -- x addr ) 18 lshift ADC2_SMPR1 ; \ ADC2_SMPR1_SMP6, SMP6
: ADC2_SMPR1_SMP5 ( %bbb -- x addr ) 15 lshift ADC2_SMPR1 ; \ ADC2_SMPR1_SMP5, SMP5
: ADC2_SMPR1_SMP4 ( %bbb -- x addr ) 12 lshift ADC2_SMPR1 ; \ ADC2_SMPR1_SMP4, SMP4
: ADC2_SMPR1_SMP3 ( %bbb -- x addr ) 9 lshift ADC2_SMPR1 ; \ ADC2_SMPR1_SMP3, SMP3
: ADC2_SMPR1_SMP2 ( %bbb -- x addr ) 6 lshift ADC2_SMPR1 ; \ ADC2_SMPR1_SMP2, SMP2
: ADC2_SMPR1_SMP1 ( %bbb -- x addr ) 3 lshift ADC2_SMPR1 ; \ ADC2_SMPR1_SMP1, SMP1
[then]

defined? use-ADC2 defined? ADC2_SMPR2_SMP18 not and [if]
\ ADC2_SMPR2 (read-write) Reset:0x00000000
: ADC2_SMPR2_SMP18 ( %bbb -- x addr ) 24 lshift ADC2_SMPR2 ; \ ADC2_SMPR2_SMP18, SMP18
: ADC2_SMPR2_SMP17 ( %bbb -- x addr ) 21 lshift ADC2_SMPR2 ; \ ADC2_SMPR2_SMP17, SMP17
: ADC2_SMPR2_SMP16 ( %bbb -- x addr ) 18 lshift ADC2_SMPR2 ; \ ADC2_SMPR2_SMP16, SMP16
: ADC2_SMPR2_SMP15 ( %bbb -- x addr ) 15 lshift ADC2_SMPR2 ; \ ADC2_SMPR2_SMP15, SMP15
: ADC2_SMPR2_SMP14 ( %bbb -- x addr ) 12 lshift ADC2_SMPR2 ; \ ADC2_SMPR2_SMP14, SMP14
: ADC2_SMPR2_SMP13 ( %bbb -- x addr ) 9 lshift ADC2_SMPR2 ; \ ADC2_SMPR2_SMP13, SMP13
: ADC2_SMPR2_SMP12 ( %bbb -- x addr ) 6 lshift ADC2_SMPR2 ; \ ADC2_SMPR2_SMP12, SMP12
: ADC2_SMPR2_SMP11 ( %bbb -- x addr ) 3 lshift ADC2_SMPR2 ; \ ADC2_SMPR2_SMP11, SMP11
: ADC2_SMPR2_SMP10 ( %bbb -- x addr ) ADC2_SMPR2 ; \ ADC2_SMPR2_SMP10, SMP10
[then]

execute-defined? use-ADC2 defined? ADC2_TR1_HT1 not and [if]
\ ADC2_TR1 (read-write) Reset:0x0FFF0000
: ADC2_TR1_HT1 ( %bbbbbbbbbbb -- x addr ) 16 lshift ADC2_TR1 ; \ ADC2_TR1_HT1, HT1
: ADC2_TR1_LT1 ( %bbbbbbbbbbb -- x addr ) ADC2_TR1 ; \ ADC2_TR1_LT1, LT1
[then]

defined? use-ADC2 defined? ADC2_TR2_HT2 not and [if]
\ ADC2_TR2 (read-write) Reset:0x0FFF0000
: ADC2_TR2_HT2 ( %bbbbbbbb -- x addr ) 16 lshift ADC2_TR2 ; \ ADC2_TR2_HT2, HT2
: ADC2_TR2_LT2 ( %bbbbbbbb -- x addr ) ADC2_TR2 ; \ ADC2_TR2_LT2, LT2
[then]

execute-defined? use-ADC2 defined? ADC2_TR3_HT3 not and [if]
\ ADC2_TR3 (read-write) Reset:0x0FFF0000
: ADC2_TR3_HT3 ( %bbbbbbbb -- x addr ) 16 lshift ADC2_TR3 ; \ ADC2_TR3_HT3, HT3
: ADC2_TR3_LT3 ( %bbbbbbbb -- x addr ) ADC2_TR3 ; \ ADC2_TR3_LT3, LT3
[then]

defined? use-ADC2 defined? ADC2_SQR1_SQ4 not and [if]
\ ADC2_SQR1 (read-write) Reset:0x00000000
: ADC2_SQR1_SQ4 ( %bbbbb -- x addr ) 24 lshift ADC2_SQR1 ; \ ADC2_SQR1_SQ4, SQ4
: ADC2_SQR1_SQ3 ( %bbbbb -- x addr ) 18 lshift ADC2_SQR1 ; \ ADC2_SQR1_SQ3, SQ3
: ADC2_SQR1_SQ2 ( %bbbbb -- x addr ) 12 lshift ADC2_SQR1 ; \ ADC2_SQR1_SQ2, SQ2
: ADC2_SQR1_SQ1 ( %bbbbb -- x addr ) 6 lshift ADC2_SQR1 ; \ ADC2_SQR1_SQ1, SQ1
: ADC2_SQR1_L3 ( %bbbb -- x addr ) ADC2_SQR1 ; \ ADC2_SQR1_L3, L3
[then]

execute-defined? use-ADC2 defined? ADC2_SQR2_SQ9 not and [if]
\ ADC2_SQR2 (read-write) Reset:0x00000000
: ADC2_SQR2_SQ9 ( %bbbbb -- x addr ) 24 lshift ADC2_SQR2 ; \ ADC2_SQR2_SQ9, SQ9
: ADC2_SQR2_SQ8 ( %bbbbb -- x addr ) 18 lshift ADC2_SQR2 ; \ ADC2_SQR2_SQ8, SQ8
: ADC2_SQR2_SQ7 ( %bbbbb -- x addr ) 12 lshift ADC2_SQR2 ; \ ADC2_SQR2_SQ7, SQ7
: ADC2_SQR2_SQ6 ( %bbbbb -- x addr ) 6 lshift ADC2_SQR2 ; \ ADC2_SQR2_SQ6, SQ6
: ADC2_SQR2_SQ5 ( %bbbbb -- x addr ) ADC2_SQR2 ; \ ADC2_SQR2_SQ5, SQ5
[then]

defined? use-ADC2 defined? ADC2_SQR3_SQ14 not and [if]
\ ADC2_SQR3 (read-write) Reset:0x00000000
: ADC2_SQR3_SQ14 ( %bbbbb -- x addr ) 24 lshift ADC2_SQR3 ; \ ADC2_SQR3_SQ14, SQ14
: ADC2_SQR3_SQ13 ( %bbbbb -- x addr ) 18 lshift ADC2_SQR3 ; \ ADC2_SQR3_SQ13, SQ13
: ADC2_SQR3_SQ12 ( %bbbbb -- x addr ) 12 lshift ADC2_SQR3 ; \ ADC2_SQR3_SQ12, SQ12
: ADC2_SQR3_SQ11 ( %bbbbb -- x addr ) 6 lshift ADC2_SQR3 ; \ ADC2_SQR3_SQ11, SQ11
: ADC2_SQR3_SQ10 ( %bbbbb -- x addr ) ADC2_SQR3 ; \ ADC2_SQR3_SQ10, SQ10
[then]

execute-defined? use-ADC2 defined? ADC2_SQR4_SQ16 not and [if]
\ ADC2_SQR4 (read-write) Reset:0x00000000
: ADC2_SQR4_SQ16 ( %bbbbb -- x addr ) 6 lshift ADC2_SQR4 ; \ ADC2_SQR4_SQ16, SQ16
: ADC2_SQR4_SQ15 ( %bbbbb -- x addr ) ADC2_SQR4 ; \ ADC2_SQR4_SQ15, SQ15
[then]

defined? use-ADC2 defined? ADC2_DR_regularDATA? not and [if]
\ ADC2_DR (read-only) Reset:0x00000000
: ADC2_DR_regularDATA? ( --  x ) ADC2_DR @ ; \ ADC2_DR_regularDATA, regularDATA
[then]

execute-defined? use-ADC2 defined? ADC2_JSQR_JSQ4 not and [if]
\ ADC2_JSQR (read-write) Reset:0x00000000
: ADC2_JSQR_JSQ4 ( %bbbbb -- x addr ) 26 lshift ADC2_JSQR ; \ ADC2_JSQR_JSQ4, JSQ4
: ADC2_JSQR_JSQ3 ( %bbbbb -- x addr ) 20 lshift ADC2_JSQR ; \ ADC2_JSQR_JSQ3, JSQ3
: ADC2_JSQR_JSQ2 ( %bbbbb -- x addr ) 14 lshift ADC2_JSQR ; \ ADC2_JSQR_JSQ2, JSQ2
: ADC2_JSQR_JSQ1 ( %bbbbb -- x addr ) 8 lshift ADC2_JSQR ; \ ADC2_JSQR_JSQ1, JSQ1
: ADC2_JSQR_JEXTEN ( %bb -- x addr ) 6 lshift ADC2_JSQR ; \ ADC2_JSQR_JEXTEN, JEXTEN
: ADC2_JSQR_JEXTSEL ( %bbbb -- x addr ) 2 lshift ADC2_JSQR ; \ ADC2_JSQR_JEXTSEL, JEXTSEL
: ADC2_JSQR_JL ( %bb -- x addr ) ADC2_JSQR ; \ ADC2_JSQR_JL, JL
[then]

defined? use-ADC2 defined? ADC2_OFR1_OFFSET1_EN not and [if]
\ ADC2_OFR1 (read-write) Reset:0x00000000
: ADC2_OFR1_OFFSET1_EN ( -- x addr ) 31 bit ADC2_OFR1 ; \ ADC2_OFR1_OFFSET1_EN, OFFSET1_EN
: ADC2_OFR1_OFFSET1_CH ( %bbbbb -- x addr ) 26 lshift ADC2_OFR1 ; \ ADC2_OFR1_OFFSET1_CH, OFFSET1_CH
: ADC2_OFR1_OFFSET1 ( %bbbbbbbbbbb -- x addr ) ADC2_OFR1 ; \ ADC2_OFR1_OFFSET1, OFFSET1
[then]

execute-defined? use-ADC2 defined? ADC2_OFR2_OFFSET2_EN not and [if]
\ ADC2_OFR2 (read-write) Reset:0x00000000
: ADC2_OFR2_OFFSET2_EN ( -- x addr ) 31 bit ADC2_OFR2 ; \ ADC2_OFR2_OFFSET2_EN, OFFSET2_EN
: ADC2_OFR2_OFFSET2_CH ( %bbbbb -- x addr ) 26 lshift ADC2_OFR2 ; \ ADC2_OFR2_OFFSET2_CH, OFFSET2_CH
: ADC2_OFR2_OFFSET2 ( %bbbbbbbbbbb -- x addr ) ADC2_OFR2 ; \ ADC2_OFR2_OFFSET2, OFFSET2
[then]

defined? use-ADC2 defined? ADC2_OFR3_OFFSET3_EN not and [if]
\ ADC2_OFR3 (read-write) Reset:0x00000000
: ADC2_OFR3_OFFSET3_EN ( -- x addr ) 31 bit ADC2_OFR3 ; \ ADC2_OFR3_OFFSET3_EN, OFFSET3_EN
: ADC2_OFR3_OFFSET3_CH ( %bbbbb -- x addr ) 26 lshift ADC2_OFR3 ; \ ADC2_OFR3_OFFSET3_CH, OFFSET3_CH
: ADC2_OFR3_OFFSET3 ( %bbbbbbbbbbb -- x addr ) ADC2_OFR3 ; \ ADC2_OFR3_OFFSET3, OFFSET3
[then]

execute-defined? use-ADC2 defined? ADC2_OFR4_OFFSET4_EN not and [if]
\ ADC2_OFR4 (read-write) Reset:0x00000000
: ADC2_OFR4_OFFSET4_EN ( -- x addr ) 31 bit ADC2_OFR4 ; \ ADC2_OFR4_OFFSET4_EN, OFFSET4_EN
: ADC2_OFR4_OFFSET4_CH ( %bbbbb -- x addr ) 26 lshift ADC2_OFR4 ; \ ADC2_OFR4_OFFSET4_CH, OFFSET4_CH
: ADC2_OFR4_OFFSET4 ( %bbbbbbbbbbb -- x addr ) ADC2_OFR4 ; \ ADC2_OFR4_OFFSET4, OFFSET4
[then]

defined? use-ADC2 defined? ADC2_JDR1_JDATA1? not and [if]
\ ADC2_JDR1 (read-only) Reset:0x00000000
: ADC2_JDR1_JDATA1? ( --  x ) ADC2_JDR1 @ ; \ ADC2_JDR1_JDATA1, JDATA1
[then]

execute-defined? use-ADC2 defined? ADC2_JDR2_JDATA2? not and [if]
\ ADC2_JDR2 (read-only) Reset:0x00000000
: ADC2_JDR2_JDATA2? ( --  x ) ADC2_JDR2 @ ; \ ADC2_JDR2_JDATA2, JDATA2
[then]

defined? use-ADC2 defined? ADC2_JDR3_JDATA3? not and [if]
\ ADC2_JDR3 (read-only) Reset:0x00000000
: ADC2_JDR3_JDATA3? ( --  x ) ADC2_JDR3 @ ; \ ADC2_JDR3_JDATA3, JDATA3
[then]

execute-defined? use-ADC2 defined? ADC2_JDR4_JDATA4? not and [if]
\ ADC2_JDR4 (read-only) Reset:0x00000000
: ADC2_JDR4_JDATA4? ( --  x ) ADC2_JDR4 @ ; \ ADC2_JDR4_JDATA4, JDATA4
[then]

defined? use-ADC2 defined? ADC2_AWD2CR_AWD2CH not and [if]
\ ADC2_AWD2CR (read-write) Reset:0x00000000
: ADC2_AWD2CR_AWD2CH x addr ) 1 lshift ADC2_AWD2CR ; \ ADC2_AWD2CR_AWD2CH, AWD2CH
[then]

execute-defined? use-ADC2 defined? ADC2_AWD3CR_AWD3CH not and [if]
\ ADC2_AWD3CR (read-write) Reset:0x00000000
: ADC2_AWD3CR_AWD3CH x addr ) 1 lshift ADC2_AWD3CR ; \ ADC2_AWD3CR_AWD3CH, AWD3CH
[then]

defined? use-ADC2 defined? ADC2_DIFSEL_DIFSEL_1_15 not and [if]
\ ADC2_DIFSEL (multiple-access)  Reset:0x00000000
: ADC2_DIFSEL_DIFSEL_1_15 ( %bbbbbbbbbbbbbbb -- x addr ) 1 lshift ADC2_DIFSEL ; \ ADC2_DIFSEL_DIFSEL_1_15, Differential mode for channels 15 to  1
: ADC2_DIFSEL_DIFSEL_16_18 ( %bbb -- x addr ) 16 lshift ADC2_DIFSEL ; \ ADC2_DIFSEL_DIFSEL_16_18, Differential mode for channels 18 to  16
[then]

execute-defined? use-ADC2 defined? ADC2_CALFACT_CALFACT_D not and [if]
\ ADC2_CALFACT (read-write) Reset:0x00000000
: ADC2_CALFACT_CALFACT_D ( %bbbbbbb -- x addr ) 16 lshift ADC2_CALFACT ; \ ADC2_CALFACT_CALFACT_D, CALFACT_D
: ADC2_CALFACT_CALFACT_S ( %bbbbbbb -- x addr ) ADC2_CALFACT ; \ ADC2_CALFACT_CALFACT_S, CALFACT_S
[then]

defined? use-ADC3 defined? ADC3_ISR_JQOVF not and [if]
\ ADC3_ISR (read-write) Reset:0x00000000
: ADC3_ISR_JQOVF ( -- x addr ) 10 bit ADC3_ISR ; \ ADC3_ISR_JQOVF, JQOVF
: ADC3_ISR_AWD3 ( -- x addr ) 9 bit ADC3_ISR ; \ ADC3_ISR_AWD3, AWD3
: ADC3_ISR_AWD2 ( -- x addr ) 8 bit ADC3_ISR ; \ ADC3_ISR_AWD2, AWD2
: ADC3_ISR_AWD1 ( -- x addr ) 7 bit ADC3_ISR ; \ ADC3_ISR_AWD1, AWD1
: ADC3_ISR_JEOS ( -- x addr ) 6 bit ADC3_ISR ; \ ADC3_ISR_JEOS, JEOS
: ADC3_ISR_JEOC ( -- x addr ) 5 bit ADC3_ISR ; \ ADC3_ISR_JEOC, JEOC
: ADC3_ISR_OVR ( -- x addr ) 4 bit ADC3_ISR ; \ ADC3_ISR_OVR, OVR
: ADC3_ISR_EOS ( -- x addr ) 3 bit ADC3_ISR ; \ ADC3_ISR_EOS, EOS
: ADC3_ISR_EOC ( -- x addr ) 2 bit ADC3_ISR ; \ ADC3_ISR_EOC, EOC
: ADC3_ISR_EOSMP ( -- x addr ) 1 bit ADC3_ISR ; \ ADC3_ISR_EOSMP, EOSMP
: ADC3_ISR_ADRDY ( -- x addr ) 0 bit ADC3_ISR ; \ ADC3_ISR_ADRDY, ADRDY
[then]

execute-defined? use-ADC3 defined? ADC3_IER_JQOVFIE not and [if]
\ ADC3_IER (read-write) Reset:0x00000000
: ADC3_IER_JQOVFIE ( -- x addr ) 10 bit ADC3_IER ; \ ADC3_IER_JQOVFIE, JQOVFIE
: ADC3_IER_AWD3IE ( -- x addr ) 9 bit ADC3_IER ; \ ADC3_IER_AWD3IE, AWD3IE
: ADC3_IER_AWD2IE ( -- x addr ) 8 bit ADC3_IER ; \ ADC3_IER_AWD2IE, AWD2IE
: ADC3_IER_AWD1IE ( -- x addr ) 7 bit ADC3_IER ; \ ADC3_IER_AWD1IE, AWD1IE
: ADC3_IER_JEOSIE ( -- x addr ) 6 bit ADC3_IER ; \ ADC3_IER_JEOSIE, JEOSIE
: ADC3_IER_JEOCIE ( -- x addr ) 5 bit ADC3_IER ; \ ADC3_IER_JEOCIE, JEOCIE
: ADC3_IER_OVRIE ( -- x addr ) 4 bit ADC3_IER ; \ ADC3_IER_OVRIE, OVRIE
: ADC3_IER_EOSIE ( -- x addr ) 3 bit ADC3_IER ; \ ADC3_IER_EOSIE, EOSIE
: ADC3_IER_EOCIE ( -- x addr ) 2 bit ADC3_IER ; \ ADC3_IER_EOCIE, EOCIE
: ADC3_IER_EOSMPIE ( -- x addr ) 1 bit ADC3_IER ; \ ADC3_IER_EOSMPIE, EOSMPIE
: ADC3_IER_ADRDYIE ( -- x addr ) 0 bit ADC3_IER ; \ ADC3_IER_ADRDYIE, ADRDYIE
[then]

defined? use-ADC3 defined? ADC3_CR_ADCAL not and [if]
\ ADC3_CR (read-write) Reset:0x00000000
: ADC3_CR_ADCAL ( -- x addr ) 31 bit ADC3_CR ; \ ADC3_CR_ADCAL, ADCAL
: ADC3_CR_ADCALDIF ( -- x addr ) 30 bit ADC3_CR ; \ ADC3_CR_ADCALDIF, ADCALDIF
: ADC3_CR_DEEPPWD ( -- x addr ) 29 bit ADC3_CR ; \ ADC3_CR_DEEPPWD, DEEPPWD
: ADC3_CR_ADVREGEN ( -- x addr ) 28 bit ADC3_CR ; \ ADC3_CR_ADVREGEN, ADVREGEN
: ADC3_CR_JADSTP ( -- x addr ) 5 bit ADC3_CR ; \ ADC3_CR_JADSTP, JADSTP
: ADC3_CR_ADSTP ( -- x addr ) 4 bit ADC3_CR ; \ ADC3_CR_ADSTP, ADSTP
: ADC3_CR_JADSTART ( -- x addr ) 3 bit ADC3_CR ; \ ADC3_CR_JADSTART, JADSTART
: ADC3_CR_ADSTART ( -- x addr ) 2 bit ADC3_CR ; \ ADC3_CR_ADSTART, ADSTART
: ADC3_CR_ADDIS ( -- x addr ) 1 bit ADC3_CR ; \ ADC3_CR_ADDIS, ADDIS
: ADC3_CR_ADEN ( -- x addr ) 0 bit ADC3_CR ; \ ADC3_CR_ADEN, ADEN
[then]

execute-defined? use-ADC3 defined? ADC3_CFGR_AWDCH1CH not and [if]
\ ADC3_CFGR (read-write) Reset:0x00000000
: ADC3_CFGR_AWDCH1CH ( %bbbbb -- x addr ) 26 lshift ADC3_CFGR ; \ ADC3_CFGR_AWDCH1CH, AWDCH1CH
: ADC3_CFGR_JAUTO ( -- x addr ) 25 bit ADC3_CFGR ; \ ADC3_CFGR_JAUTO, JAUTO
: ADC3_CFGR_JAWD1EN ( -- x addr ) 24 bit ADC3_CFGR ; \ ADC3_CFGR_JAWD1EN, JAWD1EN
: ADC3_CFGR_AWD1EN ( -- x addr ) 23 bit ADC3_CFGR ; \ ADC3_CFGR_AWD1EN, AWD1EN
: ADC3_CFGR_AWD1SGL ( -- x addr ) 22 bit ADC3_CFGR ; \ ADC3_CFGR_AWD1SGL, AWD1SGL
: ADC3_CFGR_JQM ( -- x addr ) 21 bit ADC3_CFGR ; \ ADC3_CFGR_JQM, JQM
: ADC3_CFGR_JDISCEN ( -- x addr ) 20 bit ADC3_CFGR ; \ ADC3_CFGR_JDISCEN, JDISCEN
: ADC3_CFGR_DISCNUM ( %bbb -- x addr ) 17 lshift ADC3_CFGR ; \ ADC3_CFGR_DISCNUM, DISCNUM
: ADC3_CFGR_DISCEN ( -- x addr ) 16 bit ADC3_CFGR ; \ ADC3_CFGR_DISCEN, DISCEN
: ADC3_CFGR_AUTOFF ( -- x addr ) 15 bit ADC3_CFGR ; \ ADC3_CFGR_AUTOFF, AUTOFF
: ADC3_CFGR_AUTDLY ( -- x addr ) 14 bit ADC3_CFGR ; \ ADC3_CFGR_AUTDLY, AUTDLY
: ADC3_CFGR_CONT ( -- x addr ) 13 bit ADC3_CFGR ; \ ADC3_CFGR_CONT, CONT
: ADC3_CFGR_OVRMOD ( -- x addr ) 12 bit ADC3_CFGR ; \ ADC3_CFGR_OVRMOD, OVRMOD
: ADC3_CFGR_EXTEN ( %bb -- x addr ) 10 lshift ADC3_CFGR ; \ ADC3_CFGR_EXTEN, EXTEN
: ADC3_CFGR_EXTSEL ( %bbbb -- x addr ) 6 lshift ADC3_CFGR ; \ ADC3_CFGR_EXTSEL, EXTSEL
: ADC3_CFGR_ALIGN ( -- x addr ) 5 bit ADC3_CFGR ; \ ADC3_CFGR_ALIGN, ALIGN
: ADC3_CFGR_RES ( %bb -- x addr ) 3 lshift ADC3_CFGR ; \ ADC3_CFGR_RES, RES
: ADC3_CFGR_DMACFG ( -- x addr ) 1 bit ADC3_CFGR ; \ ADC3_CFGR_DMACFG, DMACFG
: ADC3_CFGR_DMAEN ( -- x addr ) 0 bit ADC3_CFGR ; \ ADC3_CFGR_DMAEN, DMAEN
[then]

defined? use-ADC3 defined? ADC3_CFGR2_ROVSM not and [if]
\ ADC3_CFGR2 (read-write) Reset:0x00000000
: ADC3_CFGR2_ROVSM ( -- x addr ) 10 bit ADC3_CFGR2 ; \ ADC3_CFGR2_ROVSM, EXTEN
: ADC3_CFGR2_TOVS ( -- x addr ) 9 bit ADC3_CFGR2 ; \ ADC3_CFGR2_TOVS, EXTSEL
: ADC3_CFGR2_OVSS ( %bbbb -- x addr ) 5 lshift ADC3_CFGR2 ; \ ADC3_CFGR2_OVSS, ALIGN
: ADC3_CFGR2_OVSR ( %bbb -- x addr ) 2 lshift ADC3_CFGR2 ; \ ADC3_CFGR2_OVSR, RES
: ADC3_CFGR2_JOVSE ( -- x addr ) 1 bit ADC3_CFGR2 ; \ ADC3_CFGR2_JOVSE, DMACFG
: ADC3_CFGR2_ROVSE ( -- x addr ) 0 bit ADC3_CFGR2 ; \ ADC3_CFGR2_ROVSE, DMAEN
[then]

execute-defined? use-ADC3 defined? ADC3_SMPR1_SMP9 not and [if]
\ ADC3_SMPR1 (read-write) Reset:0x00000000
: ADC3_SMPR1_SMP9 ( %bbb -- x addr ) 27 lshift ADC3_SMPR1 ; \ ADC3_SMPR1_SMP9, SMP9
: ADC3_SMPR1_SMP8 ( %bbb -- x addr ) 24 lshift ADC3_SMPR1 ; \ ADC3_SMPR1_SMP8, SMP8
: ADC3_SMPR1_SMP7 ( %bbb -- x addr ) 21 lshift ADC3_SMPR1 ; \ ADC3_SMPR1_SMP7, SMP7
: ADC3_SMPR1_SMP6 ( %bbb -- x addr ) 18 lshift ADC3_SMPR1 ; \ ADC3_SMPR1_SMP6, SMP6
: ADC3_SMPR1_SMP5 ( %bbb -- x addr ) 15 lshift ADC3_SMPR1 ; \ ADC3_SMPR1_SMP5, SMP5
: ADC3_SMPR1_SMP4 ( %bbb -- x addr ) 12 lshift ADC3_SMPR1 ; \ ADC3_SMPR1_SMP4, SMP4
: ADC3_SMPR1_SMP3 ( %bbb -- x addr ) 9 lshift ADC3_SMPR1 ; \ ADC3_SMPR1_SMP3, SMP3
: ADC3_SMPR1_SMP2 ( %bbb -- x addr ) 6 lshift ADC3_SMPR1 ; \ ADC3_SMPR1_SMP2, SMP2
: ADC3_SMPR1_SMP1 ( %bbb -- x addr ) 3 lshift ADC3_SMPR1 ; \ ADC3_SMPR1_SMP1, SMP1
[then]

defined? use-ADC3 defined? ADC3_SMPR2_SMP18 not and [if]
\ ADC3_SMPR2 (read-write) Reset:0x00000000
: ADC3_SMPR2_SMP18 ( %bbb -- x addr ) 24 lshift ADC3_SMPR2 ; \ ADC3_SMPR2_SMP18, SMP18
: ADC3_SMPR2_SMP17 ( %bbb -- x addr ) 21 lshift ADC3_SMPR2 ; \ ADC3_SMPR2_SMP17, SMP17
: ADC3_SMPR2_SMP16 ( %bbb -- x addr ) 18 lshift ADC3_SMPR2 ; \ ADC3_SMPR2_SMP16, SMP16
: ADC3_SMPR2_SMP15 ( %bbb -- x addr ) 15 lshift ADC3_SMPR2 ; \ ADC3_SMPR2_SMP15, SMP15
: ADC3_SMPR2_SMP14 ( %bbb -- x addr ) 12 lshift ADC3_SMPR2 ; \ ADC3_SMPR2_SMP14, SMP14
: ADC3_SMPR2_SMP13 ( %bbb -- x addr ) 9 lshift ADC3_SMPR2 ; \ ADC3_SMPR2_SMP13, SMP13
: ADC3_SMPR2_SMP12 ( %bbb -- x addr ) 6 lshift ADC3_SMPR2 ; \ ADC3_SMPR2_SMP12, SMP12
: ADC3_SMPR2_SMP11 ( %bbb -- x addr ) 3 lshift ADC3_SMPR2 ; \ ADC3_SMPR2_SMP11, SMP11
: ADC3_SMPR2_SMP10 ( %bbb -- x addr ) ADC3_SMPR2 ; \ ADC3_SMPR2_SMP10, SMP10
[then]

execute-defined? use-ADC3 defined? ADC3_TR1_HT1 not and [if]
\ ADC3_TR1 (read-write) Reset:0x0FFF0000
: ADC3_TR1_HT1 ( %bbbbbbbbbbb -- x addr ) 16 lshift ADC3_TR1 ; \ ADC3_TR1_HT1, HT1
: ADC3_TR1_LT1 ( %bbbbbbbbbbb -- x addr ) ADC3_TR1 ; \ ADC3_TR1_LT1, LT1
[then]

defined? use-ADC3 defined? ADC3_TR2_HT2 not and [if]
\ ADC3_TR2 (read-write) Reset:0x0FFF0000
: ADC3_TR2_HT2 ( %bbbbbbbb -- x addr ) 16 lshift ADC3_TR2 ; \ ADC3_TR2_HT2, HT2
: ADC3_TR2_LT2 ( %bbbbbbbb -- x addr ) ADC3_TR2 ; \ ADC3_TR2_LT2, LT2
[then]

execute-defined? use-ADC3 defined? ADC3_TR3_HT3 not and [if]
\ ADC3_TR3 (read-write) Reset:0x0FFF0000
: ADC3_TR3_HT3 ( %bbbbbbbb -- x addr ) 16 lshift ADC3_TR3 ; \ ADC3_TR3_HT3, HT3
: ADC3_TR3_LT3 ( %bbbbbbbb -- x addr ) ADC3_TR3 ; \ ADC3_TR3_LT3, LT3
[then]

defined? use-ADC3 defined? ADC3_SQR1_SQ4 not and [if]
\ ADC3_SQR1 (read-write) Reset:0x00000000
: ADC3_SQR1_SQ4 ( %bbbbb -- x addr ) 24 lshift ADC3_SQR1 ; \ ADC3_SQR1_SQ4, SQ4
: ADC3_SQR1_SQ3 ( %bbbbb -- x addr ) 18 lshift ADC3_SQR1 ; \ ADC3_SQR1_SQ3, SQ3
: ADC3_SQR1_SQ2 ( %bbbbb -- x addr ) 12 lshift ADC3_SQR1 ; \ ADC3_SQR1_SQ2, SQ2
: ADC3_SQR1_SQ1 ( %bbbbb -- x addr ) 6 lshift ADC3_SQR1 ; \ ADC3_SQR1_SQ1, SQ1
: ADC3_SQR1_L3 ( %bbbb -- x addr ) ADC3_SQR1 ; \ ADC3_SQR1_L3, L3
[then]

execute-defined? use-ADC3 defined? ADC3_SQR2_SQ9 not and [if]
\ ADC3_SQR2 (read-write) Reset:0x00000000
: ADC3_SQR2_SQ9 ( %bbbbb -- x addr ) 24 lshift ADC3_SQR2 ; \ ADC3_SQR2_SQ9, SQ9
: ADC3_SQR2_SQ8 ( %bbbbb -- x addr ) 18 lshift ADC3_SQR2 ; \ ADC3_SQR2_SQ8, SQ8
: ADC3_SQR2_SQ7 ( %bbbbb -- x addr ) 12 lshift ADC3_SQR2 ; \ ADC3_SQR2_SQ7, SQ7
: ADC3_SQR2_SQ6 ( %bbbbb -- x addr ) 6 lshift ADC3_SQR2 ; \ ADC3_SQR2_SQ6, SQ6
: ADC3_SQR2_SQ5 ( %bbbbb -- x addr ) ADC3_SQR2 ; \ ADC3_SQR2_SQ5, SQ5
[then]

defined? use-ADC3 defined? ADC3_SQR3_SQ14 not and [if]
\ ADC3_SQR3 (read-write) Reset:0x00000000
: ADC3_SQR3_SQ14 ( %bbbbb -- x addr ) 24 lshift ADC3_SQR3 ; \ ADC3_SQR3_SQ14, SQ14
: ADC3_SQR3_SQ13 ( %bbbbb -- x addr ) 18 lshift ADC3_SQR3 ; \ ADC3_SQR3_SQ13, SQ13
: ADC3_SQR3_SQ12 ( %bbbbb -- x addr ) 12 lshift ADC3_SQR3 ; \ ADC3_SQR3_SQ12, SQ12
: ADC3_SQR3_SQ11 ( %bbbbb -- x addr ) 6 lshift ADC3_SQR3 ; \ ADC3_SQR3_SQ11, SQ11
: ADC3_SQR3_SQ10 ( %bbbbb -- x addr ) ADC3_SQR3 ; \ ADC3_SQR3_SQ10, SQ10
[then]

execute-defined? use-ADC3 defined? ADC3_SQR4_SQ16 not and [if]
\ ADC3_SQR4 (read-write) Reset:0x00000000
: ADC3_SQR4_SQ16 ( %bbbbb -- x addr ) 6 lshift ADC3_SQR4 ; \ ADC3_SQR4_SQ16, SQ16
: ADC3_SQR4_SQ15 ( %bbbbb -- x addr ) ADC3_SQR4 ; \ ADC3_SQR4_SQ15, SQ15
[then]

defined? use-ADC3 defined? ADC3_DR_regularDATA? not and [if]
\ ADC3_DR (read-only) Reset:0x00000000
: ADC3_DR_regularDATA? ( --  x ) ADC3_DR @ ; \ ADC3_DR_regularDATA, regularDATA
[then]

execute-defined? use-ADC3 defined? ADC3_JSQR_JSQ4 not and [if]
\ ADC3_JSQR (read-write) Reset:0x00000000
: ADC3_JSQR_JSQ4 ( %bbbbb -- x addr ) 26 lshift ADC3_JSQR ; \ ADC3_JSQR_JSQ4, JSQ4
: ADC3_JSQR_JSQ3 ( %bbbbb -- x addr ) 20 lshift ADC3_JSQR ; \ ADC3_JSQR_JSQ3, JSQ3
: ADC3_JSQR_JSQ2 ( %bbbbb -- x addr ) 14 lshift ADC3_JSQR ; \ ADC3_JSQR_JSQ2, JSQ2
: ADC3_JSQR_JSQ1 ( %bbbbb -- x addr ) 8 lshift ADC3_JSQR ; \ ADC3_JSQR_JSQ1, JSQ1
: ADC3_JSQR_JEXTEN ( %bb -- x addr ) 6 lshift ADC3_JSQR ; \ ADC3_JSQR_JEXTEN, JEXTEN
: ADC3_JSQR_JEXTSEL ( %bbbb -- x addr ) 2 lshift ADC3_JSQR ; \ ADC3_JSQR_JEXTSEL, JEXTSEL
: ADC3_JSQR_JL ( %bb -- x addr ) ADC3_JSQR ; \ ADC3_JSQR_JL, JL
[then]

defined? use-ADC3 defined? ADC3_OFR1_OFFSET1_EN not and [if]
\ ADC3_OFR1 (read-write) Reset:0x00000000
: ADC3_OFR1_OFFSET1_EN ( -- x addr ) 31 bit ADC3_OFR1 ; \ ADC3_OFR1_OFFSET1_EN, OFFSET1_EN
: ADC3_OFR1_OFFSET1_CH ( %bbbbb -- x addr ) 26 lshift ADC3_OFR1 ; \ ADC3_OFR1_OFFSET1_CH, OFFSET1_CH
: ADC3_OFR1_OFFSET1 ( %bbbbbbbbbbb -- x addr ) ADC3_OFR1 ; \ ADC3_OFR1_OFFSET1, OFFSET1
[then]

execute-defined? use-ADC3 defined? ADC3_OFR2_OFFSET2_EN not and [if]
\ ADC3_OFR2 (read-write) Reset:0x00000000
: ADC3_OFR2_OFFSET2_EN ( -- x addr ) 31 bit ADC3_OFR2 ; \ ADC3_OFR2_OFFSET2_EN, OFFSET2_EN
: ADC3_OFR2_OFFSET2_CH ( %bbbbb -- x addr ) 26 lshift ADC3_OFR2 ; \ ADC3_OFR2_OFFSET2_CH, OFFSET2_CH
: ADC3_OFR2_OFFSET2 ( %bbbbbbbbbbb -- x addr ) ADC3_OFR2 ; \ ADC3_OFR2_OFFSET2, OFFSET2
[then]

defined? use-ADC3 defined? ADC3_OFR3_OFFSET3_EN not and [if]
\ ADC3_OFR3 (read-write) Reset:0x00000000
: ADC3_OFR3_OFFSET3_EN ( -- x addr ) 31 bit ADC3_OFR3 ; \ ADC3_OFR3_OFFSET3_EN, OFFSET3_EN
: ADC3_OFR3_OFFSET3_CH ( %bbbbb -- x addr ) 26 lshift ADC3_OFR3 ; \ ADC3_OFR3_OFFSET3_CH, OFFSET3_CH
: ADC3_OFR3_OFFSET3 ( %bbbbbbbbbbb -- x addr ) ADC3_OFR3 ; \ ADC3_OFR3_OFFSET3, OFFSET3
[then]

execute-defined? use-ADC3 defined? ADC3_OFR4_OFFSET4_EN not and [if]
\ ADC3_OFR4 (read-write) Reset:0x00000000
: ADC3_OFR4_OFFSET4_EN ( -- x addr ) 31 bit ADC3_OFR4 ; \ ADC3_OFR4_OFFSET4_EN, OFFSET4_EN
: ADC3_OFR4_OFFSET4_CH ( %bbbbb -- x addr ) 26 lshift ADC3_OFR4 ; \ ADC3_OFR4_OFFSET4_CH, OFFSET4_CH
: ADC3_OFR4_OFFSET4 ( %bbbbbbbbbbb -- x addr ) ADC3_OFR4 ; \ ADC3_OFR4_OFFSET4, OFFSET4
[then]

defined? use-ADC3 defined? ADC3_JDR1_JDATA1? not and [if]
\ ADC3_JDR1 (read-only) Reset:0x00000000
: ADC3_JDR1_JDATA1? ( --  x ) ADC3_JDR1 @ ; \ ADC3_JDR1_JDATA1, JDATA1
[then]

execute-defined? use-ADC3 defined? ADC3_JDR2_JDATA2? not and [if]
\ ADC3_JDR2 (read-only) Reset:0x00000000
: ADC3_JDR2_JDATA2? ( --  x ) ADC3_JDR2 @ ; \ ADC3_JDR2_JDATA2, JDATA2
[then]

defined? use-ADC3 defined? ADC3_JDR3_JDATA3? not and [if]
\ ADC3_JDR3 (read-only) Reset:0x00000000
: ADC3_JDR3_JDATA3? ( --  x ) ADC3_JDR3 @ ; \ ADC3_JDR3_JDATA3, JDATA3
[then]

execute-defined? use-ADC3 defined? ADC3_JDR4_JDATA4? not and [if]
\ ADC3_JDR4 (read-only) Reset:0x00000000
: ADC3_JDR4_JDATA4? ( --  x ) ADC3_JDR4 @ ; \ ADC3_JDR4_JDATA4, JDATA4
[then]

defined? use-ADC3 defined? ADC3_AWD2CR_AWD2CH not and [if]
\ ADC3_AWD2CR (read-write) Reset:0x00000000
: ADC3_AWD2CR_AWD2CH x addr ) 1 lshift ADC3_AWD2CR ; \ ADC3_AWD2CR_AWD2CH, AWD2CH
[then]

execute-defined? use-ADC3 defined? ADC3_AWD3CR_AWD3CH not and [if]
\ ADC3_AWD3CR (read-write) Reset:0x00000000
: ADC3_AWD3CR_AWD3CH x addr ) 1 lshift ADC3_AWD3CR ; \ ADC3_AWD3CR_AWD3CH, AWD3CH
[then]

defined? use-ADC3 defined? ADC3_DIFSEL_DIFSEL_1_15 not and [if]
\ ADC3_DIFSEL (multiple-access)  Reset:0x00000000
: ADC3_DIFSEL_DIFSEL_1_15 ( %bbbbbbbbbbbbbbb -- x addr ) 1 lshift ADC3_DIFSEL ; \ ADC3_DIFSEL_DIFSEL_1_15, Differential mode for channels 15 to  1
: ADC3_DIFSEL_DIFSEL_16_18 ( %bbb -- x addr ) 16 lshift ADC3_DIFSEL ; \ ADC3_DIFSEL_DIFSEL_16_18, Differential mode for channels 18 to  16
[then]

execute-defined? use-ADC3 defined? ADC3_CALFACT_CALFACT_D not and [if]
\ ADC3_CALFACT (read-write) Reset:0x00000000
: ADC3_CALFACT_CALFACT_D ( %bbbbbbb -- x addr ) 16 lshift ADC3_CALFACT ; \ ADC3_CALFACT_CALFACT_D, CALFACT_D
: ADC3_CALFACT_CALFACT_S ( %bbbbbbb -- x addr ) ADC3_CALFACT ; \ ADC3_CALFACT_CALFACT_S, CALFACT_S
[then]

defined? use-GPIOA defined? GPIOA_MODER_MODER15 not and [if]
\ GPIOA_MODER (read-write) Reset:0xA8000000
: GPIOA_MODER_MODER15 ( %bb -- x addr ) 30 lshift GPIOA_MODER ; \ GPIOA_MODER_MODER15, Port x configuration bits y =  0..15
: GPIOA_MODER_MODER14 ( %bb -- x addr ) 28 lshift GPIOA_MODER ; \ GPIOA_MODER_MODER14, Port x configuration bits y =  0..15
: GPIOA_MODER_MODER13 ( %bb -- x addr ) 26 lshift GPIOA_MODER ; \ GPIOA_MODER_MODER13, Port x configuration bits y =  0..15
: GPIOA_MODER_MODER12 ( %bb -- x addr ) 24 lshift GPIOA_MODER ; \ GPIOA_MODER_MODER12, Port x configuration bits y =  0..15
: GPIOA_MODER_MODER11 ( %bb -- x addr ) 22 lshift GPIOA_MODER ; \ GPIOA_MODER_MODER11, Port x configuration bits y =  0..15
: GPIOA_MODER_MODER10 ( %bb -- x addr ) 20 lshift GPIOA_MODER ; \ GPIOA_MODER_MODER10, Port x configuration bits y =  0..15
: GPIOA_MODER_MODER9 ( %bb -- x addr ) 18 lshift GPIOA_MODER ; \ GPIOA_MODER_MODER9, Port x configuration bits y =  0..15
: GPIOA_MODER_MODER8 ( %bb -- x addr ) 16 lshift GPIOA_MODER ; \ GPIOA_MODER_MODER8, Port x configuration bits y =  0..15
: GPIOA_MODER_MODER7 ( %bb -- x addr ) 14 lshift GPIOA_MODER ; \ GPIOA_MODER_MODER7, Port x configuration bits y =  0..15
: GPIOA_MODER_MODER6 ( %bb -- x addr ) 12 lshift GPIOA_MODER ; \ GPIOA_MODER_MODER6, Port x configuration bits y =  0..15
: GPIOA_MODER_MODER5 ( %bb -- x addr ) 10 lshift GPIOA_MODER ; \ GPIOA_MODER_MODER5, Port x configuration bits y =  0..15
: GPIOA_MODER_MODER4 ( %bb -- x addr ) 8 lshift GPIOA_MODER ; \ GPIOA_MODER_MODER4, Port x configuration bits y =  0..15
: GPIOA_MODER_MODER3 ( %bb -- x addr ) 6 lshift GPIOA_MODER ; \ GPIOA_MODER_MODER3, Port x configuration bits y =  0..15
: GPIOA_MODER_MODER2 ( %bb -- x addr ) 4 lshift GPIOA_MODER ; \ GPIOA_MODER_MODER2, Port x configuration bits y =  0..15
: GPIOA_MODER_MODER1 ( %bb -- x addr ) 2 lshift GPIOA_MODER ; \ GPIOA_MODER_MODER1, Port x configuration bits y =  0..15
: GPIOA_MODER_MODER0 ( %bb -- x addr ) GPIOA_MODER ; \ GPIOA_MODER_MODER0, Port x configuration bits y =  0..15
[then]

execute-defined? use-GPIOA defined? GPIOA_OTYPER_OT15 not and [if]
\ GPIOA_OTYPER (read-write) Reset:0x00000000
: GPIOA_OTYPER_OT15 ( -- x addr ) 15 bit GPIOA_OTYPER ; \ GPIOA_OTYPER_OT15, Port x configuration bits y =  0..15
: GPIOA_OTYPER_OT14 ( -- x addr ) 14 bit GPIOA_OTYPER ; \ GPIOA_OTYPER_OT14, Port x configuration bits y =  0..15
: GPIOA_OTYPER_OT13 ( -- x addr ) 13 bit GPIOA_OTYPER ; \ GPIOA_OTYPER_OT13, Port x configuration bits y =  0..15
: GPIOA_OTYPER_OT12 ( -- x addr ) 12 bit GPIOA_OTYPER ; \ GPIOA_OTYPER_OT12, Port x configuration bits y =  0..15
: GPIOA_OTYPER_OT11 ( -- x addr ) 11 bit GPIOA_OTYPER ; \ GPIOA_OTYPER_OT11, Port x configuration bits y =  0..15
: GPIOA_OTYPER_OT10 ( -- x addr ) 10 bit GPIOA_OTYPER ; \ GPIOA_OTYPER_OT10, Port x configuration bits y =  0..15
: GPIOA_OTYPER_OT9 ( -- x addr ) 9 bit GPIOA_OTYPER ; \ GPIOA_OTYPER_OT9, Port x configuration bits y =  0..15
: GPIOA_OTYPER_OT8 ( -- x addr ) 8 bit GPIOA_OTYPER ; \ GPIOA_OTYPER_OT8, Port x configuration bits y =  0..15
: GPIOA_OTYPER_OT7 ( -- x addr ) 7 bit GPIOA_OTYPER ; \ GPIOA_OTYPER_OT7, Port x configuration bits y =  0..15
: GPIOA_OTYPER_OT6 ( -- x addr ) 6 bit GPIOA_OTYPER ; \ GPIOA_OTYPER_OT6, Port x configuration bits y =  0..15
: GPIOA_OTYPER_OT5 ( -- x addr ) 5 bit GPIOA_OTYPER ; \ GPIOA_OTYPER_OT5, Port x configuration bits y =  0..15
: GPIOA_OTYPER_OT4 ( -- x addr ) 4 bit GPIOA_OTYPER ; \ GPIOA_OTYPER_OT4, Port x configuration bits y =  0..15
: GPIOA_OTYPER_OT3 ( -- x addr ) 3 bit GPIOA_OTYPER ; \ GPIOA_OTYPER_OT3, Port x configuration bits y =  0..15
: GPIOA_OTYPER_OT2 ( -- x addr ) 2 bit GPIOA_OTYPER ; \ GPIOA_OTYPER_OT2, Port x configuration bits y =  0..15
: GPIOA_OTYPER_OT1 ( -- x addr ) 1 bit GPIOA_OTYPER ; \ GPIOA_OTYPER_OT1, Port x configuration bits y =  0..15
: GPIOA_OTYPER_OT0 ( -- x addr ) 0 bit GPIOA_OTYPER ; \ GPIOA_OTYPER_OT0, Port x configuration bits y =  0..15
[then]

defined? use-GPIOA defined? GPIOA_OSPEEDR_OSPEEDR15 not and [if]
\ GPIOA_OSPEEDR (read-write) Reset:0x00000000
: GPIOA_OSPEEDR_OSPEEDR15 ( %bb -- x addr ) 30 lshift GPIOA_OSPEEDR ; \ GPIOA_OSPEEDR_OSPEEDR15, Port x configuration bits y =  0..15
: GPIOA_OSPEEDR_OSPEEDR14 ( %bb -- x addr ) 28 lshift GPIOA_OSPEEDR ; \ GPIOA_OSPEEDR_OSPEEDR14, Port x configuration bits y =  0..15
: GPIOA_OSPEEDR_OSPEEDR13 ( %bb -- x addr ) 26 lshift GPIOA_OSPEEDR ; \ GPIOA_OSPEEDR_OSPEEDR13, Port x configuration bits y =  0..15
: GPIOA_OSPEEDR_OSPEEDR12 ( %bb -- x addr ) 24 lshift GPIOA_OSPEEDR ; \ GPIOA_OSPEEDR_OSPEEDR12, Port x configuration bits y =  0..15
: GPIOA_OSPEEDR_OSPEEDR11 ( %bb -- x addr ) 22 lshift GPIOA_OSPEEDR ; \ GPIOA_OSPEEDR_OSPEEDR11, Port x configuration bits y =  0..15
: GPIOA_OSPEEDR_OSPEEDR10 ( %bb -- x addr ) 20 lshift GPIOA_OSPEEDR ; \ GPIOA_OSPEEDR_OSPEEDR10, Port x configuration bits y =  0..15
: GPIOA_OSPEEDR_OSPEEDR9 ( %bb -- x addr ) 18 lshift GPIOA_OSPEEDR ; \ GPIOA_OSPEEDR_OSPEEDR9, Port x configuration bits y =  0..15
: GPIOA_OSPEEDR_OSPEEDR8 ( %bb -- x addr ) 16 lshift GPIOA_OSPEEDR ; \ GPIOA_OSPEEDR_OSPEEDR8, Port x configuration bits y =  0..15
: GPIOA_OSPEEDR_OSPEEDR7 ( %bb -- x addr ) 14 lshift GPIOA_OSPEEDR ; \ GPIOA_OSPEEDR_OSPEEDR7, Port x configuration bits y =  0..15
: GPIOA_OSPEEDR_OSPEEDR6 ( %bb -- x addr ) 12 lshift GPIOA_OSPEEDR ; \ GPIOA_OSPEEDR_OSPEEDR6, Port x configuration bits y =  0..15
: GPIOA_OSPEEDR_OSPEEDR5 ( %bb -- x addr ) 10 lshift GPIOA_OSPEEDR ; \ GPIOA_OSPEEDR_OSPEEDR5, Port x configuration bits y =  0..15
: GPIOA_OSPEEDR_OSPEEDR4 ( %bb -- x addr ) 8 lshift GPIOA_OSPEEDR ; \ GPIOA_OSPEEDR_OSPEEDR4, Port x configuration bits y =  0..15
: GPIOA_OSPEEDR_OSPEEDR3 ( %bb -- x addr ) 6 lshift GPIOA_OSPEEDR ; \ GPIOA_OSPEEDR_OSPEEDR3, Port x configuration bits y =  0..15
: GPIOA_OSPEEDR_OSPEEDR2 ( %bb -- x addr ) 4 lshift GPIOA_OSPEEDR ; \ GPIOA_OSPEEDR_OSPEEDR2, Port x configuration bits y =  0..15
: GPIOA_OSPEEDR_OSPEEDR1 ( %bb -- x addr ) 2 lshift GPIOA_OSPEEDR ; \ GPIOA_OSPEEDR_OSPEEDR1, Port x configuration bits y =  0..15
: GPIOA_OSPEEDR_OSPEEDR0 ( %bb -- x addr ) GPIOA_OSPEEDR ; \ GPIOA_OSPEEDR_OSPEEDR0, Port x configuration bits y =  0..15
[then]

execute-defined? use-GPIOA defined? GPIOA_PUPDR_PUPDR15 not and [if]
\ GPIOA_PUPDR (read-write) Reset:0x64000000
: GPIOA_PUPDR_PUPDR15 ( %bb -- x addr ) 30 lshift GPIOA_PUPDR ; \ GPIOA_PUPDR_PUPDR15, Port x configuration bits y =  0..15
: GPIOA_PUPDR_PUPDR14 ( %bb -- x addr ) 28 lshift GPIOA_PUPDR ; \ GPIOA_PUPDR_PUPDR14, Port x configuration bits y =  0..15
: GPIOA_PUPDR_PUPDR13 ( %bb -- x addr ) 26 lshift GPIOA_PUPDR ; \ GPIOA_PUPDR_PUPDR13, Port x configuration bits y =  0..15
: GPIOA_PUPDR_PUPDR12 ( %bb -- x addr ) 24 lshift GPIOA_PUPDR ; \ GPIOA_PUPDR_PUPDR12, Port x configuration bits y =  0..15
: GPIOA_PUPDR_PUPDR11 ( %bb -- x addr ) 22 lshift GPIOA_PUPDR ; \ GPIOA_PUPDR_PUPDR11, Port x configuration bits y =  0..15
: GPIOA_PUPDR_PUPDR10 ( %bb -- x addr ) 20 lshift GPIOA_PUPDR ; \ GPIOA_PUPDR_PUPDR10, Port x configuration bits y =  0..15
: GPIOA_PUPDR_PUPDR9 ( %bb -- x addr ) 18 lshift GPIOA_PUPDR ; \ GPIOA_PUPDR_PUPDR9, Port x configuration bits y =  0..15
: GPIOA_PUPDR_PUPDR8 ( %bb -- x addr ) 16 lshift GPIOA_PUPDR ; \ GPIOA_PUPDR_PUPDR8, Port x configuration bits y =  0..15
: GPIOA_PUPDR_PUPDR7 ( %bb -- x addr ) 14 lshift GPIOA_PUPDR ; \ GPIOA_PUPDR_PUPDR7, Port x configuration bits y =  0..15
: GPIOA_PUPDR_PUPDR6 ( %bb -- x addr ) 12 lshift GPIOA_PUPDR ; \ GPIOA_PUPDR_PUPDR6, Port x configuration bits y =  0..15
: GPIOA_PUPDR_PUPDR5 ( %bb -- x addr ) 10 lshift GPIOA_PUPDR ; \ GPIOA_PUPDR_PUPDR5, Port x configuration bits y =  0..15
: GPIOA_PUPDR_PUPDR4 ( %bb -- x addr ) 8 lshift GPIOA_PUPDR ; \ GPIOA_PUPDR_PUPDR4, Port x configuration bits y =  0..15
: GPIOA_PUPDR_PUPDR3 ( %bb -- x addr ) 6 lshift GPIOA_PUPDR ; \ GPIOA_PUPDR_PUPDR3, Port x configuration bits y =  0..15
: GPIOA_PUPDR_PUPDR2 ( %bb -- x addr ) 4 lshift GPIOA_PUPDR ; \ GPIOA_PUPDR_PUPDR2, Port x configuration bits y =  0..15
: GPIOA_PUPDR_PUPDR1 ( %bb -- x addr ) 2 lshift GPIOA_PUPDR ; \ GPIOA_PUPDR_PUPDR1, Port x configuration bits y =  0..15
: GPIOA_PUPDR_PUPDR0 ( %bb -- x addr ) GPIOA_PUPDR ; \ GPIOA_PUPDR_PUPDR0, Port x configuration bits y =  0..15
[then]

defined? use-GPIOA defined? GPIOA_IDR_IDR15? not and [if]
\ GPIOA_IDR (read-only) Reset:0x00000000
: GPIOA_IDR_IDR15? ( --  1|0 ) 15 bit GPIOA_IDR bit@ ; \ GPIOA_IDR_IDR15, Port input data y =  0..15
: GPIOA_IDR_IDR14? ( --  1|0 ) 14 bit GPIOA_IDR bit@ ; \ GPIOA_IDR_IDR14, Port input data y =  0..15
: GPIOA_IDR_IDR13? ( --  1|0 ) 13 bit GPIOA_IDR bit@ ; \ GPIOA_IDR_IDR13, Port input data y =  0..15
: GPIOA_IDR_IDR12? ( --  1|0 ) 12 bit GPIOA_IDR bit@ ; \ GPIOA_IDR_IDR12, Port input data y =  0..15
: GPIOA_IDR_IDR11? ( --  1|0 ) 11 bit GPIOA_IDR bit@ ; \ GPIOA_IDR_IDR11, Port input data y =  0..15
: GPIOA_IDR_IDR10? ( --  1|0 ) 10 bit GPIOA_IDR bit@ ; \ GPIOA_IDR_IDR10, Port input data y =  0..15
: GPIOA_IDR_IDR9? ( --  1|0 ) 9 bit GPIOA_IDR bit@ ; \ GPIOA_IDR_IDR9, Port input data y =  0..15
: GPIOA_IDR_IDR8? ( --  1|0 ) 8 bit GPIOA_IDR bit@ ; \ GPIOA_IDR_IDR8, Port input data y =  0..15
: GPIOA_IDR_IDR7? ( --  1|0 ) 7 bit GPIOA_IDR bit@ ; \ GPIOA_IDR_IDR7, Port input data y =  0..15
: GPIOA_IDR_IDR6? ( --  1|0 ) 6 bit GPIOA_IDR bit@ ; \ GPIOA_IDR_IDR6, Port input data y =  0..15
: GPIOA_IDR_IDR5? ( --  1|0 ) 5 bit GPIOA_IDR bit@ ; \ GPIOA_IDR_IDR5, Port input data y =  0..15
: GPIOA_IDR_IDR4? ( --  1|0 ) 4 bit GPIOA_IDR bit@ ; \ GPIOA_IDR_IDR4, Port input data y =  0..15
: GPIOA_IDR_IDR3? ( --  1|0 ) 3 bit GPIOA_IDR bit@ ; \ GPIOA_IDR_IDR3, Port input data y =  0..15
: GPIOA_IDR_IDR2? ( --  1|0 ) 2 bit GPIOA_IDR bit@ ; \ GPIOA_IDR_IDR2, Port input data y =  0..15
: GPIOA_IDR_IDR1? ( --  1|0 ) 1 bit GPIOA_IDR bit@ ; \ GPIOA_IDR_IDR1, Port input data y =  0..15
: GPIOA_IDR_IDR0? ( --  1|0 ) 0 bit GPIOA_IDR bit@ ; \ GPIOA_IDR_IDR0, Port input data y =  0..15
[then]

execute-defined? use-GPIOA defined? GPIOA_ODR_ODR15 not and [if]
\ GPIOA_ODR (read-write) Reset:0x00000000
: GPIOA_ODR_ODR15 ( -- x addr ) 15 bit GPIOA_ODR ; \ GPIOA_ODR_ODR15, Port output data y =  0..15
: GPIOA_ODR_ODR14 ( -- x addr ) 14 bit GPIOA_ODR ; \ GPIOA_ODR_ODR14, Port output data y =  0..15
: GPIOA_ODR_ODR13 ( -- x addr ) 13 bit GPIOA_ODR ; \ GPIOA_ODR_ODR13, Port output data y =  0..15
: GPIOA_ODR_ODR12 ( -- x addr ) 12 bit GPIOA_ODR ; \ GPIOA_ODR_ODR12, Port output data y =  0..15
: GPIOA_ODR_ODR11 ( -- x addr ) 11 bit GPIOA_ODR ; \ GPIOA_ODR_ODR11, Port output data y =  0..15
: GPIOA_ODR_ODR10 ( -- x addr ) 10 bit GPIOA_ODR ; \ GPIOA_ODR_ODR10, Port output data y =  0..15
: GPIOA_ODR_ODR9 ( -- x addr ) 9 bit GPIOA_ODR ; \ GPIOA_ODR_ODR9, Port output data y =  0..15
: GPIOA_ODR_ODR8 ( -- x addr ) 8 bit GPIOA_ODR ; \ GPIOA_ODR_ODR8, Port output data y =  0..15
: GPIOA_ODR_ODR7 ( -- x addr ) 7 bit GPIOA_ODR ; \ GPIOA_ODR_ODR7, Port output data y =  0..15
: GPIOA_ODR_ODR6 ( -- x addr ) 6 bit GPIOA_ODR ; \ GPIOA_ODR_ODR6, Port output data y =  0..15
: GPIOA_ODR_ODR5 ( -- x addr ) 5 bit GPIOA_ODR ; \ GPIOA_ODR_ODR5, Port output data y =  0..15
: GPIOA_ODR_ODR4 ( -- x addr ) 4 bit GPIOA_ODR ; \ GPIOA_ODR_ODR4, Port output data y =  0..15
: GPIOA_ODR_ODR3 ( -- x addr ) 3 bit GPIOA_ODR ; \ GPIOA_ODR_ODR3, Port output data y =  0..15
: GPIOA_ODR_ODR2 ( -- x addr ) 2 bit GPIOA_ODR ; \ GPIOA_ODR_ODR2, Port output data y =  0..15
: GPIOA_ODR_ODR1 ( -- x addr ) 1 bit GPIOA_ODR ; \ GPIOA_ODR_ODR1, Port output data y =  0..15
: GPIOA_ODR_ODR0 ( -- x addr ) 0 bit GPIOA_ODR ; \ GPIOA_ODR_ODR0, Port output data y =  0..15
[then]

defined? use-GPIOA defined? GPIOA_BSRR_BR15 not and [if]
\ GPIOA_BSRR (write-only) Reset:0x00000000
: GPIOA_BSRR_BR15 ( -- ) 31 bit GPIOA_BSRR ! ; \ GPIOA_BSRR_BR15, Port x reset bit y y =  0..15
: GPIOA_BSRR_BR14 ( -- ) 30 bit GPIOA_BSRR ! ; \ GPIOA_BSRR_BR14, Port x reset bit y y =  0..15
: GPIOA_BSRR_BR13 ( -- ) 29 bit GPIOA_BSRR ! ; \ GPIOA_BSRR_BR13, Port x reset bit y y =  0..15
: GPIOA_BSRR_BR12 ( -- ) 28 bit GPIOA_BSRR ! ; \ GPIOA_BSRR_BR12, Port x reset bit y y =  0..15
: GPIOA_BSRR_BR11 ( -- ) 27 bit GPIOA_BSRR ! ; \ GPIOA_BSRR_BR11, Port x reset bit y y =  0..15
: GPIOA_BSRR_BR10 ( -- ) 26 bit GPIOA_BSRR ! ; \ GPIOA_BSRR_BR10, Port x reset bit y y =  0..15
: GPIOA_BSRR_BR9 ( -- ) 25 bit GPIOA_BSRR ! ; \ GPIOA_BSRR_BR9, Port x reset bit y y =  0..15
: GPIOA_BSRR_BR8 ( -- ) 24 bit GPIOA_BSRR ! ; \ GPIOA_BSRR_BR8, Port x reset bit y y =  0..15
: GPIOA_BSRR_BR7 ( -- ) 23 bit GPIOA_BSRR ! ; \ GPIOA_BSRR_BR7, Port x reset bit y y =  0..15
: GPIOA_BSRR_BR6 ( -- ) 22 bit GPIOA_BSRR ! ; \ GPIOA_BSRR_BR6, Port x reset bit y y =  0..15
: GPIOA_BSRR_BR5 ( -- ) 21 bit GPIOA_BSRR ! ; \ GPIOA_BSRR_BR5, Port x reset bit y y =  0..15
: GPIOA_BSRR_BR4 ( -- ) 20 bit GPIOA_BSRR ! ; \ GPIOA_BSRR_BR4, Port x reset bit y y =  0..15
: GPIOA_BSRR_BR3 ( -- ) 19 bit GPIOA_BSRR ! ; \ GPIOA_BSRR_BR3, Port x reset bit y y =  0..15
: GPIOA_BSRR_BR2 ( -- ) 18 bit GPIOA_BSRR ! ; \ GPIOA_BSRR_BR2, Port x reset bit y y =  0..15
: GPIOA_BSRR_BR1 ( -- ) 17 bit GPIOA_BSRR ! ; \ GPIOA_BSRR_BR1, Port x reset bit y y =  0..15
: GPIOA_BSRR_BR0 ( -- ) 16 bit GPIOA_BSRR ! ; \ GPIOA_BSRR_BR0, Port x set bit y y=  0..15
: GPIOA_BSRR_BS15 ( -- ) 15 bit GPIOA_BSRR ! ; \ GPIOA_BSRR_BS15, Port x set bit y y=  0..15
: GPIOA_BSRR_BS14 ( -- ) 14 bit GPIOA_BSRR ! ; \ GPIOA_BSRR_BS14, Port x set bit y y=  0..15
: GPIOA_BSRR_BS13 ( -- ) 13 bit GPIOA_BSRR ! ; \ GPIOA_BSRR_BS13, Port x set bit y y=  0..15
: GPIOA_BSRR_BS12 ( -- ) 12 bit GPIOA_BSRR ! ; \ GPIOA_BSRR_BS12, Port x set bit y y=  0..15
: GPIOA_BSRR_BS11 ( -- ) 11 bit GPIOA_BSRR ! ; \ GPIOA_BSRR_BS11, Port x set bit y y=  0..15
: GPIOA_BSRR_BS10 ( -- ) 10 bit GPIOA_BSRR ! ; \ GPIOA_BSRR_BS10, Port x set bit y y=  0..15
: GPIOA_BSRR_BS9 ( -- ) 9 bit GPIOA_BSRR ! ; \ GPIOA_BSRR_BS9, Port x set bit y y=  0..15
: GPIOA_BSRR_BS8 ( -- ) 8 bit GPIOA_BSRR ! ; \ GPIOA_BSRR_BS8, Port x set bit y y=  0..15
: GPIOA_BSRR_BS7 ( -- ) 7 bit GPIOA_BSRR ! ; \ GPIOA_BSRR_BS7, Port x set bit y y=  0..15
: GPIOA_BSRR_BS6 ( -- ) 6 bit GPIOA_BSRR ! ; \ GPIOA_BSRR_BS6, Port x set bit y y=  0..15
: GPIOA_BSRR_BS5 ( -- ) 5 bit GPIOA_BSRR ! ; \ GPIOA_BSRR_BS5, Port x set bit y y=  0..15
: GPIOA_BSRR_BS4 ( -- ) 4 bit GPIOA_BSRR ! ; \ GPIOA_BSRR_BS4, Port x set bit y y=  0..15
: GPIOA_BSRR_BS3 ( -- ) 3 bit GPIOA_BSRR ! ; \ GPIOA_BSRR_BS3, Port x set bit y y=  0..15
: GPIOA_BSRR_BS2 ( -- ) 2 bit GPIOA_BSRR ! ; \ GPIOA_BSRR_BS2, Port x set bit y y=  0..15
: GPIOA_BSRR_BS1 ( -- ) 1 bit GPIOA_BSRR ! ; \ GPIOA_BSRR_BS1, Port x set bit y y=  0..15
: GPIOA_BSRR_BS0 ( -- ) 0 bit GPIOA_BSRR ! ; \ GPIOA_BSRR_BS0, Port x set bit y y=  0..15
[then]

execute-defined? use-GPIOA defined? GPIOA_LCKR_LCKK not and [if]
\ GPIOA_LCKR (read-write) Reset:0x00000000
: GPIOA_LCKR_LCKK ( -- x addr ) 16 bit GPIOA_LCKR ; \ GPIOA_LCKR_LCKK, Port x lock bit y y=  0..15
: GPIOA_LCKR_LCK15 ( -- x addr ) 15 bit GPIOA_LCKR ; \ GPIOA_LCKR_LCK15, Port x lock bit y y=  0..15
: GPIOA_LCKR_LCK14 ( -- x addr ) 14 bit GPIOA_LCKR ; \ GPIOA_LCKR_LCK14, Port x lock bit y y=  0..15
: GPIOA_LCKR_LCK13 ( -- x addr ) 13 bit GPIOA_LCKR ; \ GPIOA_LCKR_LCK13, Port x lock bit y y=  0..15
: GPIOA_LCKR_LCK12 ( -- x addr ) 12 bit GPIOA_LCKR ; \ GPIOA_LCKR_LCK12, Port x lock bit y y=  0..15
: GPIOA_LCKR_LCK11 ( -- x addr ) 11 bit GPIOA_LCKR ; \ GPIOA_LCKR_LCK11, Port x lock bit y y=  0..15
: GPIOA_LCKR_LCK10 ( -- x addr ) 10 bit GPIOA_LCKR ; \ GPIOA_LCKR_LCK10, Port x lock bit y y=  0..15
: GPIOA_LCKR_LCK9 ( -- x addr ) 9 bit GPIOA_LCKR ; \ GPIOA_LCKR_LCK9, Port x lock bit y y=  0..15
: GPIOA_LCKR_LCK8 ( -- x addr ) 8 bit GPIOA_LCKR ; \ GPIOA_LCKR_LCK8, Port x lock bit y y=  0..15
: GPIOA_LCKR_LCK7 ( -- x addr ) 7 bit GPIOA_LCKR ; \ GPIOA_LCKR_LCK7, Port x lock bit y y=  0..15
: GPIOA_LCKR_LCK6 ( -- x addr ) 6 bit GPIOA_LCKR ; \ GPIOA_LCKR_LCK6, Port x lock bit y y=  0..15
: GPIOA_LCKR_LCK5 ( -- x addr ) 5 bit GPIOA_LCKR ; \ GPIOA_LCKR_LCK5, Port x lock bit y y=  0..15
: GPIOA_LCKR_LCK4 ( -- x addr ) 4 bit GPIOA_LCKR ; \ GPIOA_LCKR_LCK4, Port x lock bit y y=  0..15
: GPIOA_LCKR_LCK3 ( -- x addr ) 3 bit GPIOA_LCKR ; \ GPIOA_LCKR_LCK3, Port x lock bit y y=  0..15
: GPIOA_LCKR_LCK2 ( -- x addr ) 2 bit GPIOA_LCKR ; \ GPIOA_LCKR_LCK2, Port x lock bit y y=  0..15
: GPIOA_LCKR_LCK1 ( -- x addr ) 1 bit GPIOA_LCKR ; \ GPIOA_LCKR_LCK1, Port x lock bit y y=  0..15
: GPIOA_LCKR_LCK0 ( -- x addr ) 0 bit GPIOA_LCKR ; \ GPIOA_LCKR_LCK0, Port x lock bit y y=  0..15
[then]

defined? use-GPIOA defined? GPIOA_AFRL_AFRL7 not and [if]
\ GPIOA_AFRL (read-write) Reset:0x00000000
: GPIOA_AFRL_AFRL7 ( %bbbb -- x addr ) 28 lshift GPIOA_AFRL ; \ GPIOA_AFRL_AFRL7, Alternate function selection for port x  bit y y = 0..7
: GPIOA_AFRL_AFRL6 ( %bbbb -- x addr ) 24 lshift GPIOA_AFRL ; \ GPIOA_AFRL_AFRL6, Alternate function selection for port x  bit y y = 0..7
: GPIOA_AFRL_AFRL5 ( %bbbb -- x addr ) 20 lshift GPIOA_AFRL ; \ GPIOA_AFRL_AFRL5, Alternate function selection for port x  bit y y = 0..7
: GPIOA_AFRL_AFRL4 ( %bbbb -- x addr ) 16 lshift GPIOA_AFRL ; \ GPIOA_AFRL_AFRL4, Alternate function selection for port x  bit y y = 0..7
: GPIOA_AFRL_AFRL3 ( %bbbb -- x addr ) 12 lshift GPIOA_AFRL ; \ GPIOA_AFRL_AFRL3, Alternate function selection for port x  bit y y = 0..7
: GPIOA_AFRL_AFRL2 ( %bbbb -- x addr ) 8 lshift GPIOA_AFRL ; \ GPIOA_AFRL_AFRL2, Alternate function selection for port x  bit y y = 0..7
: GPIOA_AFRL_AFRL1 ( %bbbb -- x addr ) 4 lshift GPIOA_AFRL ; \ GPIOA_AFRL_AFRL1, Alternate function selection for port x  bit y y = 0..7
: GPIOA_AFRL_AFRL0 ( %bbbb -- x addr ) GPIOA_AFRL ; \ GPIOA_AFRL_AFRL0, Alternate function selection for port x  bit y y = 0..7
[then]

execute-defined? use-GPIOA defined? GPIOA_AFRH_AFRH15 not and [if]
\ GPIOA_AFRH (read-write) Reset:0x00000000
: GPIOA_AFRH_AFRH15 ( %bbbb -- x addr ) 28 lshift GPIOA_AFRH ; \ GPIOA_AFRH_AFRH15, Alternate function selection for port x  bit y y = 8..15
: GPIOA_AFRH_AFRH14 ( %bbbb -- x addr ) 24 lshift GPIOA_AFRH ; \ GPIOA_AFRH_AFRH14, Alternate function selection for port x  bit y y = 8..15
: GPIOA_AFRH_AFRH13 ( %bbbb -- x addr ) 20 lshift GPIOA_AFRH ; \ GPIOA_AFRH_AFRH13, Alternate function selection for port x  bit y y = 8..15
: GPIOA_AFRH_AFRH12 ( %bbbb -- x addr ) 16 lshift GPIOA_AFRH ; \ GPIOA_AFRH_AFRH12, Alternate function selection for port x  bit y y = 8..15
: GPIOA_AFRH_AFRH11 ( %bbbb -- x addr ) 12 lshift GPIOA_AFRH ; \ GPIOA_AFRH_AFRH11, Alternate function selection for port x  bit y y = 8..15
: GPIOA_AFRH_AFRH10 ( %bbbb -- x addr ) 8 lshift GPIOA_AFRH ; \ GPIOA_AFRH_AFRH10, Alternate function selection for port x  bit y y = 8..15
: GPIOA_AFRH_AFRH9 ( %bbbb -- x addr ) 4 lshift GPIOA_AFRH ; \ GPIOA_AFRH_AFRH9, Alternate function selection for port x  bit y y = 8..15
: GPIOA_AFRH_AFRH8 ( %bbbb -- x addr ) GPIOA_AFRH ; \ GPIOA_AFRH_AFRH8, Alternate function selection for port x  bit y y = 8..15
[then]

defined? use-GPIOB defined? GPIOB_MODER_MODER15 not and [if]
\ GPIOB_MODER (read-write) Reset:0x00000280
: GPIOB_MODER_MODER15 ( %bb -- x addr ) 30 lshift GPIOB_MODER ; \ GPIOB_MODER_MODER15, Port x configuration bits y =  0..15
: GPIOB_MODER_MODER14 ( %bb -- x addr ) 28 lshift GPIOB_MODER ; \ GPIOB_MODER_MODER14, Port x configuration bits y =  0..15
: GPIOB_MODER_MODER13 ( %bb -- x addr ) 26 lshift GPIOB_MODER ; \ GPIOB_MODER_MODER13, Port x configuration bits y =  0..15
: GPIOB_MODER_MODER12 ( %bb -- x addr ) 24 lshift GPIOB_MODER ; \ GPIOB_MODER_MODER12, Port x configuration bits y =  0..15
: GPIOB_MODER_MODER11 ( %bb -- x addr ) 22 lshift GPIOB_MODER ; \ GPIOB_MODER_MODER11, Port x configuration bits y =  0..15
: GPIOB_MODER_MODER10 ( %bb -- x addr ) 20 lshift GPIOB_MODER ; \ GPIOB_MODER_MODER10, Port x configuration bits y =  0..15
: GPIOB_MODER_MODER9 ( %bb -- x addr ) 18 lshift GPIOB_MODER ; \ GPIOB_MODER_MODER9, Port x configuration bits y =  0..15
: GPIOB_MODER_MODER8 ( %bb -- x addr ) 16 lshift GPIOB_MODER ; \ GPIOB_MODER_MODER8, Port x configuration bits y =  0..15
: GPIOB_MODER_MODER7 ( %bb -- x addr ) 14 lshift GPIOB_MODER ; \ GPIOB_MODER_MODER7, Port x configuration bits y =  0..15
: GPIOB_MODER_MODER6 ( %bb -- x addr ) 12 lshift GPIOB_MODER ; \ GPIOB_MODER_MODER6, Port x configuration bits y =  0..15
: GPIOB_MODER_MODER5 ( %bb -- x addr ) 10 lshift GPIOB_MODER ; \ GPIOB_MODER_MODER5, Port x configuration bits y =  0..15
: GPIOB_MODER_MODER4 ( %bb -- x addr ) 8 lshift GPIOB_MODER ; \ GPIOB_MODER_MODER4, Port x configuration bits y =  0..15
: GPIOB_MODER_MODER3 ( %bb -- x addr ) 6 lshift GPIOB_MODER ; \ GPIOB_MODER_MODER3, Port x configuration bits y =  0..15
: GPIOB_MODER_MODER2 ( %bb -- x addr ) 4 lshift GPIOB_MODER ; \ GPIOB_MODER_MODER2, Port x configuration bits y =  0..15
: GPIOB_MODER_MODER1 ( %bb -- x addr ) 2 lshift GPIOB_MODER ; \ GPIOB_MODER_MODER1, Port x configuration bits y =  0..15
: GPIOB_MODER_MODER0 ( %bb -- x addr ) GPIOB_MODER ; \ GPIOB_MODER_MODER0, Port x configuration bits y =  0..15
[then]

execute-defined? use-GPIOB defined? GPIOB_OTYPER_OT15 not and [if]
\ GPIOB_OTYPER (read-write) Reset:0x00000000
: GPIOB_OTYPER_OT15 ( -- x addr ) 15 bit GPIOB_OTYPER ; \ GPIOB_OTYPER_OT15, Port x configuration bits y =  0..15
: GPIOB_OTYPER_OT14 ( -- x addr ) 14 bit GPIOB_OTYPER ; \ GPIOB_OTYPER_OT14, Port x configuration bits y =  0..15
: GPIOB_OTYPER_OT13 ( -- x addr ) 13 bit GPIOB_OTYPER ; \ GPIOB_OTYPER_OT13, Port x configuration bits y =  0..15
: GPIOB_OTYPER_OT12 ( -- x addr ) 12 bit GPIOB_OTYPER ; \ GPIOB_OTYPER_OT12, Port x configuration bits y =  0..15
: GPIOB_OTYPER_OT11 ( -- x addr ) 11 bit GPIOB_OTYPER ; \ GPIOB_OTYPER_OT11, Port x configuration bits y =  0..15
: GPIOB_OTYPER_OT10 ( -- x addr ) 10 bit GPIOB_OTYPER ; \ GPIOB_OTYPER_OT10, Port x configuration bits y =  0..15
: GPIOB_OTYPER_OT9 ( -- x addr ) 9 bit GPIOB_OTYPER ; \ GPIOB_OTYPER_OT9, Port x configuration bits y =  0..15
: GPIOB_OTYPER_OT8 ( -- x addr ) 8 bit GPIOB_OTYPER ; \ GPIOB_OTYPER_OT8, Port x configuration bits y =  0..15
: GPIOB_OTYPER_OT7 ( -- x addr ) 7 bit GPIOB_OTYPER ; \ GPIOB_OTYPER_OT7, Port x configuration bits y =  0..15
: GPIOB_OTYPER_OT6 ( -- x addr ) 6 bit GPIOB_OTYPER ; \ GPIOB_OTYPER_OT6, Port x configuration bits y =  0..15
: GPIOB_OTYPER_OT5 ( -- x addr ) 5 bit GPIOB_OTYPER ; \ GPIOB_OTYPER_OT5, Port x configuration bits y =  0..15
: GPIOB_OTYPER_OT4 ( -- x addr ) 4 bit GPIOB_OTYPER ; \ GPIOB_OTYPER_OT4, Port x configuration bits y =  0..15
: GPIOB_OTYPER_OT3 ( -- x addr ) 3 bit GPIOB_OTYPER ; \ GPIOB_OTYPER_OT3, Port x configuration bits y =  0..15
: GPIOB_OTYPER_OT2 ( -- x addr ) 2 bit GPIOB_OTYPER ; \ GPIOB_OTYPER_OT2, Port x configuration bits y =  0..15
: GPIOB_OTYPER_OT1 ( -- x addr ) 1 bit GPIOB_OTYPER ; \ GPIOB_OTYPER_OT1, Port x configuration bits y =  0..15
: GPIOB_OTYPER_OT0 ( -- x addr ) 0 bit GPIOB_OTYPER ; \ GPIOB_OTYPER_OT0, Port x configuration bits y =  0..15
[then]

defined? use-GPIOB defined? GPIOB_OSPEEDR_OSPEEDR15 not and [if]
\ GPIOB_OSPEEDR (read-write) Reset:0x000000C0
: GPIOB_OSPEEDR_OSPEEDR15 ( %bb -- x addr ) 30 lshift GPIOB_OSPEEDR ; \ GPIOB_OSPEEDR_OSPEEDR15, Port x configuration bits y =  0..15
: GPIOB_OSPEEDR_OSPEEDR14 ( %bb -- x addr ) 28 lshift GPIOB_OSPEEDR ; \ GPIOB_OSPEEDR_OSPEEDR14, Port x configuration bits y =  0..15
: GPIOB_OSPEEDR_OSPEEDR13 ( %bb -- x addr ) 26 lshift GPIOB_OSPEEDR ; \ GPIOB_OSPEEDR_OSPEEDR13, Port x configuration bits y =  0..15
: GPIOB_OSPEEDR_OSPEEDR12 ( %bb -- x addr ) 24 lshift GPIOB_OSPEEDR ; \ GPIOB_OSPEEDR_OSPEEDR12, Port x configuration bits y =  0..15
: GPIOB_OSPEEDR_OSPEEDR11 ( %bb -- x addr ) 22 lshift GPIOB_OSPEEDR ; \ GPIOB_OSPEEDR_OSPEEDR11, Port x configuration bits y =  0..15
: GPIOB_OSPEEDR_OSPEEDR10 ( %bb -- x addr ) 20 lshift GPIOB_OSPEEDR ; \ GPIOB_OSPEEDR_OSPEEDR10, Port x configuration bits y =  0..15
: GPIOB_OSPEEDR_OSPEEDR9 ( %bb -- x addr ) 18 lshift GPIOB_OSPEEDR ; \ GPIOB_OSPEEDR_OSPEEDR9, Port x configuration bits y =  0..15
: GPIOB_OSPEEDR_OSPEEDR8 ( %bb -- x addr ) 16 lshift GPIOB_OSPEEDR ; \ GPIOB_OSPEEDR_OSPEEDR8, Port x configuration bits y =  0..15
: GPIOB_OSPEEDR_OSPEEDR7 ( %bb -- x addr ) 14 lshift GPIOB_OSPEEDR ; \ GPIOB_OSPEEDR_OSPEEDR7, Port x configuration bits y =  0..15
: GPIOB_OSPEEDR_OSPEEDR6 ( %bb -- x addr ) 12 lshift GPIOB_OSPEEDR ; \ GPIOB_OSPEEDR_OSPEEDR6, Port x configuration bits y =  0..15
: GPIOB_OSPEEDR_OSPEEDR5 ( %bb -- x addr ) 10 lshift GPIOB_OSPEEDR ; \ GPIOB_OSPEEDR_OSPEEDR5, Port x configuration bits y =  0..15
: GPIOB_OSPEEDR_OSPEEDR4 ( %bb -- x addr ) 8 lshift GPIOB_OSPEEDR ; \ GPIOB_OSPEEDR_OSPEEDR4, Port x configuration bits y =  0..15
: GPIOB_OSPEEDR_OSPEEDR3 ( %bb -- x addr ) 6 lshift GPIOB_OSPEEDR ; \ GPIOB_OSPEEDR_OSPEEDR3, Port x configuration bits y =  0..15
: GPIOB_OSPEEDR_OSPEEDR2 ( %bb -- x addr ) 4 lshift GPIOB_OSPEEDR ; \ GPIOB_OSPEEDR_OSPEEDR2, Port x configuration bits y =  0..15
: GPIOB_OSPEEDR_OSPEEDR1 ( %bb -- x addr ) 2 lshift GPIOB_OSPEEDR ; \ GPIOB_OSPEEDR_OSPEEDR1, Port x configuration bits y =  0..15
: GPIOB_OSPEEDR_OSPEEDR0 ( %bb -- x addr ) GPIOB_OSPEEDR ; \ GPIOB_OSPEEDR_OSPEEDR0, Port x configuration bits y =  0..15
[then]

execute-defined? use-GPIOB defined? GPIOB_PUPDR_PUPDR15 not and [if]
\ GPIOB_PUPDR (read-write) Reset:0x00000100
: GPIOB_PUPDR_PUPDR15 ( %bb -- x addr ) 30 lshift GPIOB_PUPDR ; \ GPIOB_PUPDR_PUPDR15, Port x configuration bits y =  0..15
: GPIOB_PUPDR_PUPDR14 ( %bb -- x addr ) 28 lshift GPIOB_PUPDR ; \ GPIOB_PUPDR_PUPDR14, Port x configuration bits y =  0..15
: GPIOB_PUPDR_PUPDR13 ( %bb -- x addr ) 26 lshift GPIOB_PUPDR ; \ GPIOB_PUPDR_PUPDR13, Port x configuration bits y =  0..15
: GPIOB_PUPDR_PUPDR12 ( %bb -- x addr ) 24 lshift GPIOB_PUPDR ; \ GPIOB_PUPDR_PUPDR12, Port x configuration bits y =  0..15
: GPIOB_PUPDR_PUPDR11 ( %bb -- x addr ) 22 lshift GPIOB_PUPDR ; \ GPIOB_PUPDR_PUPDR11, Port x configuration bits y =  0..15
: GPIOB_PUPDR_PUPDR10 ( %bb -- x addr ) 20 lshift GPIOB_PUPDR ; \ GPIOB_PUPDR_PUPDR10, Port x configuration bits y =  0..15
: GPIOB_PUPDR_PUPDR9 ( %bb -- x addr ) 18 lshift GPIOB_PUPDR ; \ GPIOB_PUPDR_PUPDR9, Port x configuration bits y =  0..15
: GPIOB_PUPDR_PUPDR8 ( %bb -- x addr ) 16 lshift GPIOB_PUPDR ; \ GPIOB_PUPDR_PUPDR8, Port x configuration bits y =  0..15
: GPIOB_PUPDR_PUPDR7 ( %bb -- x addr ) 14 lshift GPIOB_PUPDR ; \ GPIOB_PUPDR_PUPDR7, Port x configuration bits y =  0..15
: GPIOB_PUPDR_PUPDR6 ( %bb -- x addr ) 12 lshift GPIOB_PUPDR ; \ GPIOB_PUPDR_PUPDR6, Port x configuration bits y =  0..15
: GPIOB_PUPDR_PUPDR5 ( %bb -- x addr ) 10 lshift GPIOB_PUPDR ; \ GPIOB_PUPDR_PUPDR5, Port x configuration bits y =  0..15
: GPIOB_PUPDR_PUPDR4 ( %bb -- x addr ) 8 lshift GPIOB_PUPDR ; \ GPIOB_PUPDR_PUPDR4, Port x configuration bits y =  0..15
: GPIOB_PUPDR_PUPDR3 ( %bb -- x addr ) 6 lshift GPIOB_PUPDR ; \ GPIOB_PUPDR_PUPDR3, Port x configuration bits y =  0..15
: GPIOB_PUPDR_PUPDR2 ( %bb -- x addr ) 4 lshift GPIOB_PUPDR ; \ GPIOB_PUPDR_PUPDR2, Port x configuration bits y =  0..15
: GPIOB_PUPDR_PUPDR1 ( %bb -- x addr ) 2 lshift GPIOB_PUPDR ; \ GPIOB_PUPDR_PUPDR1, Port x configuration bits y =  0..15
: GPIOB_PUPDR_PUPDR0 ( %bb -- x addr ) GPIOB_PUPDR ; \ GPIOB_PUPDR_PUPDR0, Port x configuration bits y =  0..15
[then]

defined? use-GPIOB defined? GPIOB_IDR_IDR15? not and [if]
\ GPIOB_IDR (read-only) Reset:0x00000000
: GPIOB_IDR_IDR15? ( --  1|0 ) 15 bit GPIOB_IDR bit@ ; \ GPIOB_IDR_IDR15, Port input data y =  0..15
: GPIOB_IDR_IDR14? ( --  1|0 ) 14 bit GPIOB_IDR bit@ ; \ GPIOB_IDR_IDR14, Port input data y =  0..15
: GPIOB_IDR_IDR13? ( --  1|0 ) 13 bit GPIOB_IDR bit@ ; \ GPIOB_IDR_IDR13, Port input data y =  0..15
: GPIOB_IDR_IDR12? ( --  1|0 ) 12 bit GPIOB_IDR bit@ ; \ GPIOB_IDR_IDR12, Port input data y =  0..15
: GPIOB_IDR_IDR11? ( --  1|0 ) 11 bit GPIOB_IDR bit@ ; \ GPIOB_IDR_IDR11, Port input data y =  0..15
: GPIOB_IDR_IDR10? ( --  1|0 ) 10 bit GPIOB_IDR bit@ ; \ GPIOB_IDR_IDR10, Port input data y =  0..15
: GPIOB_IDR_IDR9? ( --  1|0 ) 9 bit GPIOB_IDR bit@ ; \ GPIOB_IDR_IDR9, Port input data y =  0..15
: GPIOB_IDR_IDR8? ( --  1|0 ) 8 bit GPIOB_IDR bit@ ; \ GPIOB_IDR_IDR8, Port input data y =  0..15
: GPIOB_IDR_IDR7? ( --  1|0 ) 7 bit GPIOB_IDR bit@ ; \ GPIOB_IDR_IDR7, Port input data y =  0..15
: GPIOB_IDR_IDR6? ( --  1|0 ) 6 bit GPIOB_IDR bit@ ; \ GPIOB_IDR_IDR6, Port input data y =  0..15
: GPIOB_IDR_IDR5? ( --  1|0 ) 5 bit GPIOB_IDR bit@ ; \ GPIOB_IDR_IDR5, Port input data y =  0..15
: GPIOB_IDR_IDR4? ( --  1|0 ) 4 bit GPIOB_IDR bit@ ; \ GPIOB_IDR_IDR4, Port input data y =  0..15
: GPIOB_IDR_IDR3? ( --  1|0 ) 3 bit GPIOB_IDR bit@ ; \ GPIOB_IDR_IDR3, Port input data y =  0..15
: GPIOB_IDR_IDR2? ( --  1|0 ) 2 bit GPIOB_IDR bit@ ; \ GPIOB_IDR_IDR2, Port input data y =  0..15
: GPIOB_IDR_IDR1? ( --  1|0 ) 1 bit GPIOB_IDR bit@ ; \ GPIOB_IDR_IDR1, Port input data y =  0..15
: GPIOB_IDR_IDR0? ( --  1|0 ) 0 bit GPIOB_IDR bit@ ; \ GPIOB_IDR_IDR0, Port input data y =  0..15
[then]

execute-defined? use-GPIOB defined? GPIOB_ODR_ODR15 not and [if]
\ GPIOB_ODR (read-write) Reset:0x00000000
: GPIOB_ODR_ODR15 ( -- x addr ) 15 bit GPIOB_ODR ; \ GPIOB_ODR_ODR15, Port output data y =  0..15
: GPIOB_ODR_ODR14 ( -- x addr ) 14 bit GPIOB_ODR ; \ GPIOB_ODR_ODR14, Port output data y =  0..15
: GPIOB_ODR_ODR13 ( -- x addr ) 13 bit GPIOB_ODR ; \ GPIOB_ODR_ODR13, Port output data y =  0..15
: GPIOB_ODR_ODR12 ( -- x addr ) 12 bit GPIOB_ODR ; \ GPIOB_ODR_ODR12, Port output data y =  0..15
: GPIOB_ODR_ODR11 ( -- x addr ) 11 bit GPIOB_ODR ; \ GPIOB_ODR_ODR11, Port output data y =  0..15
: GPIOB_ODR_ODR10 ( -- x addr ) 10 bit GPIOB_ODR ; \ GPIOB_ODR_ODR10, Port output data y =  0..15
: GPIOB_ODR_ODR9 ( -- x addr ) 9 bit GPIOB_ODR ; \ GPIOB_ODR_ODR9, Port output data y =  0..15
: GPIOB_ODR_ODR8 ( -- x addr ) 8 bit GPIOB_ODR ; \ GPIOB_ODR_ODR8, Port output data y =  0..15
: GPIOB_ODR_ODR7 ( -- x addr ) 7 bit GPIOB_ODR ; \ GPIOB_ODR_ODR7, Port output data y =  0..15
: GPIOB_ODR_ODR6 ( -- x addr ) 6 bit GPIOB_ODR ; \ GPIOB_ODR_ODR6, Port output data y =  0..15
: GPIOB_ODR_ODR5 ( -- x addr ) 5 bit GPIOB_ODR ; \ GPIOB_ODR_ODR5, Port output data y =  0..15
: GPIOB_ODR_ODR4 ( -- x addr ) 4 bit GPIOB_ODR ; \ GPIOB_ODR_ODR4, Port output data y =  0..15
: GPIOB_ODR_ODR3 ( -- x addr ) 3 bit GPIOB_ODR ; \ GPIOB_ODR_ODR3, Port output data y =  0..15
: GPIOB_ODR_ODR2 ( -- x addr ) 2 bit GPIOB_ODR ; \ GPIOB_ODR_ODR2, Port output data y =  0..15
: GPIOB_ODR_ODR1 ( -- x addr ) 1 bit GPIOB_ODR ; \ GPIOB_ODR_ODR1, Port output data y =  0..15
: GPIOB_ODR_ODR0 ( -- x addr ) 0 bit GPIOB_ODR ; \ GPIOB_ODR_ODR0, Port output data y =  0..15
[then]

defined? use-GPIOB defined? GPIOB_BSRR_BR15 not and [if]
\ GPIOB_BSRR (write-only) Reset:0x00000000
: GPIOB_BSRR_BR15 ( -- ) 31 bit GPIOB_BSRR ! ; \ GPIOB_BSRR_BR15, Port x reset bit y y =  0..15
: GPIOB_BSRR_BR14 ( -- ) 30 bit GPIOB_BSRR ! ; \ GPIOB_BSRR_BR14, Port x reset bit y y =  0..15
: GPIOB_BSRR_BR13 ( -- ) 29 bit GPIOB_BSRR ! ; \ GPIOB_BSRR_BR13, Port x reset bit y y =  0..15
: GPIOB_BSRR_BR12 ( -- ) 28 bit GPIOB_BSRR ! ; \ GPIOB_BSRR_BR12, Port x reset bit y y =  0..15
: GPIOB_BSRR_BR11 ( -- ) 27 bit GPIOB_BSRR ! ; \ GPIOB_BSRR_BR11, Port x reset bit y y =  0..15
: GPIOB_BSRR_BR10 ( -- ) 26 bit GPIOB_BSRR ! ; \ GPIOB_BSRR_BR10, Port x reset bit y y =  0..15
: GPIOB_BSRR_BR9 ( -- ) 25 bit GPIOB_BSRR ! ; \ GPIOB_BSRR_BR9, Port x reset bit y y =  0..15
: GPIOB_BSRR_BR8 ( -- ) 24 bit GPIOB_BSRR ! ; \ GPIOB_BSRR_BR8, Port x reset bit y y =  0..15
: GPIOB_BSRR_BR7 ( -- ) 23 bit GPIOB_BSRR ! ; \ GPIOB_BSRR_BR7, Port x reset bit y y =  0..15
: GPIOB_BSRR_BR6 ( -- ) 22 bit GPIOB_BSRR ! ; \ GPIOB_BSRR_BR6, Port x reset bit y y =  0..15
: GPIOB_BSRR_BR5 ( -- ) 21 bit GPIOB_BSRR ! ; \ GPIOB_BSRR_BR5, Port x reset bit y y =  0..15
: GPIOB_BSRR_BR4 ( -- ) 20 bit GPIOB_BSRR ! ; \ GPIOB_BSRR_BR4, Port x reset bit y y =  0..15
: GPIOB_BSRR_BR3 ( -- ) 19 bit GPIOB_BSRR ! ; \ GPIOB_BSRR_BR3, Port x reset bit y y =  0..15
: GPIOB_BSRR_BR2 ( -- ) 18 bit GPIOB_BSRR ! ; \ GPIOB_BSRR_BR2, Port x reset bit y y =  0..15
: GPIOB_BSRR_BR1 ( -- ) 17 bit GPIOB_BSRR ! ; \ GPIOB_BSRR_BR1, Port x reset bit y y =  0..15
: GPIOB_BSRR_BR0 ( -- ) 16 bit GPIOB_BSRR ! ; \ GPIOB_BSRR_BR0, Port x set bit y y=  0..15
: GPIOB_BSRR_BS15 ( -- ) 15 bit GPIOB_BSRR ! ; \ GPIOB_BSRR_BS15, Port x set bit y y=  0..15
: GPIOB_BSRR_BS14 ( -- ) 14 bit GPIOB_BSRR ! ; \ GPIOB_BSRR_BS14, Port x set bit y y=  0..15
: GPIOB_BSRR_BS13 ( -- ) 13 bit GPIOB_BSRR ! ; \ GPIOB_BSRR_BS13, Port x set bit y y=  0..15
: GPIOB_BSRR_BS12 ( -- ) 12 bit GPIOB_BSRR ! ; \ GPIOB_BSRR_BS12, Port x set bit y y=  0..15
: GPIOB_BSRR_BS11 ( -- ) 11 bit GPIOB_BSRR ! ; \ GPIOB_BSRR_BS11, Port x set bit y y=  0..15
: GPIOB_BSRR_BS10 ( -- ) 10 bit GPIOB_BSRR ! ; \ GPIOB_BSRR_BS10, Port x set bit y y=  0..15
: GPIOB_BSRR_BS9 ( -- ) 9 bit GPIOB_BSRR ! ; \ GPIOB_BSRR_BS9, Port x set bit y y=  0..15
: GPIOB_BSRR_BS8 ( -- ) 8 bit GPIOB_BSRR ! ; \ GPIOB_BSRR_BS8, Port x set bit y y=  0..15
: GPIOB_BSRR_BS7 ( -- ) 7 bit GPIOB_BSRR ! ; \ GPIOB_BSRR_BS7, Port x set bit y y=  0..15
: GPIOB_BSRR_BS6 ( -- ) 6 bit GPIOB_BSRR ! ; \ GPIOB_BSRR_BS6, Port x set bit y y=  0..15
: GPIOB_BSRR_BS5 ( -- ) 5 bit GPIOB_BSRR ! ; \ GPIOB_BSRR_BS5, Port x set bit y y=  0..15
: GPIOB_BSRR_BS4 ( -- ) 4 bit GPIOB_BSRR ! ; \ GPIOB_BSRR_BS4, Port x set bit y y=  0..15
: GPIOB_BSRR_BS3 ( -- ) 3 bit GPIOB_BSRR ! ; \ GPIOB_BSRR_BS3, Port x set bit y y=  0..15
: GPIOB_BSRR_BS2 ( -- ) 2 bit GPIOB_BSRR ! ; \ GPIOB_BSRR_BS2, Port x set bit y y=  0..15
: GPIOB_BSRR_BS1 ( -- ) 1 bit GPIOB_BSRR ! ; \ GPIOB_BSRR_BS1, Port x set bit y y=  0..15
: GPIOB_BSRR_BS0 ( -- ) 0 bit GPIOB_BSRR ! ; \ GPIOB_BSRR_BS0, Port x set bit y y=  0..15
[then]

execute-defined? use-GPIOB defined? GPIOB_LCKR_LCKK not and [if]
\ GPIOB_LCKR (read-write) Reset:0x00000000
: GPIOB_LCKR_LCKK ( -- x addr ) 16 bit GPIOB_LCKR ; \ GPIOB_LCKR_LCKK, Port x lock bit y y=  0..15
: GPIOB_LCKR_LCK15 ( -- x addr ) 15 bit GPIOB_LCKR ; \ GPIOB_LCKR_LCK15, Port x lock bit y y=  0..15
: GPIOB_LCKR_LCK14 ( -- x addr ) 14 bit GPIOB_LCKR ; \ GPIOB_LCKR_LCK14, Port x lock bit y y=  0..15
: GPIOB_LCKR_LCK13 ( -- x addr ) 13 bit GPIOB_LCKR ; \ GPIOB_LCKR_LCK13, Port x lock bit y y=  0..15
: GPIOB_LCKR_LCK12 ( -- x addr ) 12 bit GPIOB_LCKR ; \ GPIOB_LCKR_LCK12, Port x lock bit y y=  0..15
: GPIOB_LCKR_LCK11 ( -- x addr ) 11 bit GPIOB_LCKR ; \ GPIOB_LCKR_LCK11, Port x lock bit y y=  0..15
: GPIOB_LCKR_LCK10 ( -- x addr ) 10 bit GPIOB_LCKR ; \ GPIOB_LCKR_LCK10, Port x lock bit y y=  0..15
: GPIOB_LCKR_LCK9 ( -- x addr ) 9 bit GPIOB_LCKR ; \ GPIOB_LCKR_LCK9, Port x lock bit y y=  0..15
: GPIOB_LCKR_LCK8 ( -- x addr ) 8 bit GPIOB_LCKR ; \ GPIOB_LCKR_LCK8, Port x lock bit y y=  0..15
: GPIOB_LCKR_LCK7 ( -- x addr ) 7 bit GPIOB_LCKR ; \ GPIOB_LCKR_LCK7, Port x lock bit y y=  0..15
: GPIOB_LCKR_LCK6 ( -- x addr ) 6 bit GPIOB_LCKR ; \ GPIOB_LCKR_LCK6, Port x lock bit y y=  0..15
: GPIOB_LCKR_LCK5 ( -- x addr ) 5 bit GPIOB_LCKR ; \ GPIOB_LCKR_LCK5, Port x lock bit y y=  0..15
: GPIOB_LCKR_LCK4 ( -- x addr ) 4 bit GPIOB_LCKR ; \ GPIOB_LCKR_LCK4, Port x lock bit y y=  0..15
: GPIOB_LCKR_LCK3 ( -- x addr ) 3 bit GPIOB_LCKR ; \ GPIOB_LCKR_LCK3, Port x lock bit y y=  0..15
: GPIOB_LCKR_LCK2 ( -- x addr ) 2 bit GPIOB_LCKR ; \ GPIOB_LCKR_LCK2, Port x lock bit y y=  0..15
: GPIOB_LCKR_LCK1 ( -- x addr ) 1 bit GPIOB_LCKR ; \ GPIOB_LCKR_LCK1, Port x lock bit y y=  0..15
: GPIOB_LCKR_LCK0 ( -- x addr ) 0 bit GPIOB_LCKR ; \ GPIOB_LCKR_LCK0, Port x lock bit y y=  0..15
[then]

defined? use-GPIOB defined? GPIOB_AFRL_AFRL7 not and [if]
\ GPIOB_AFRL (read-write) Reset:0x00000000
: GPIOB_AFRL_AFRL7 ( %bbbb -- x addr ) 28 lshift GPIOB_AFRL ; \ GPIOB_AFRL_AFRL7, Alternate function selection for port x  bit y y = 0..7
: GPIOB_AFRL_AFRL6 ( %bbbb -- x addr ) 24 lshift GPIOB_AFRL ; \ GPIOB_AFRL_AFRL6, Alternate function selection for port x  bit y y = 0..7
: GPIOB_AFRL_AFRL5 ( %bbbb -- x addr ) 20 lshift GPIOB_AFRL ; \ GPIOB_AFRL_AFRL5, Alternate function selection for port x  bit y y = 0..7
: GPIOB_AFRL_AFRL4 ( %bbbb -- x addr ) 16 lshift GPIOB_AFRL ; \ GPIOB_AFRL_AFRL4, Alternate function selection for port x  bit y y = 0..7
: GPIOB_AFRL_AFRL3 ( %bbbb -- x addr ) 12 lshift GPIOB_AFRL ; \ GPIOB_AFRL_AFRL3, Alternate function selection for port x  bit y y = 0..7
: GPIOB_AFRL_AFRL2 ( %bbbb -- x addr ) 8 lshift GPIOB_AFRL ; \ GPIOB_AFRL_AFRL2, Alternate function selection for port x  bit y y = 0..7
: GPIOB_AFRL_AFRL1 ( %bbbb -- x addr ) 4 lshift GPIOB_AFRL ; \ GPIOB_AFRL_AFRL1, Alternate function selection for port x  bit y y = 0..7
: GPIOB_AFRL_AFRL0 ( %bbbb -- x addr ) GPIOB_AFRL ; \ GPIOB_AFRL_AFRL0, Alternate function selection for port x  bit y y = 0..7
[then]

execute-defined? use-GPIOB defined? GPIOB_AFRH_AFRH15 not and [if]
\ GPIOB_AFRH (read-write) Reset:0x00000000
: GPIOB_AFRH_AFRH15 ( %bbbb -- x addr ) 28 lshift GPIOB_AFRH ; \ GPIOB_AFRH_AFRH15, Alternate function selection for port x  bit y y = 8..15
: GPIOB_AFRH_AFRH14 ( %bbbb -- x addr ) 24 lshift GPIOB_AFRH ; \ GPIOB_AFRH_AFRH14, Alternate function selection for port x  bit y y = 8..15
: GPIOB_AFRH_AFRH13 ( %bbbb -- x addr ) 20 lshift GPIOB_AFRH ; \ GPIOB_AFRH_AFRH13, Alternate function selection for port x  bit y y = 8..15
: GPIOB_AFRH_AFRH12 ( %bbbb -- x addr ) 16 lshift GPIOB_AFRH ; \ GPIOB_AFRH_AFRH12, Alternate function selection for port x  bit y y = 8..15
: GPIOB_AFRH_AFRH11 ( %bbbb -- x addr ) 12 lshift GPIOB_AFRH ; \ GPIOB_AFRH_AFRH11, Alternate function selection for port x  bit y y = 8..15
: GPIOB_AFRH_AFRH10 ( %bbbb -- x addr ) 8 lshift GPIOB_AFRH ; \ GPIOB_AFRH_AFRH10, Alternate function selection for port x  bit y y = 8..15
: GPIOB_AFRH_AFRH9 ( %bbbb -- x addr ) 4 lshift GPIOB_AFRH ; \ GPIOB_AFRH_AFRH9, Alternate function selection for port x  bit y y = 8..15
: GPIOB_AFRH_AFRH8 ( %bbbb -- x addr ) GPIOB_AFRH ; \ GPIOB_AFRH_AFRH8, Alternate function selection for port x  bit y y = 8..15
[then]

defined? use-GPIOC defined? GPIOC_MODER_MODER15 not and [if]
\ GPIOC_MODER (read-write) Reset:0x00000000
: GPIOC_MODER_MODER15 ( %bb -- x addr ) 30 lshift GPIOC_MODER ; \ GPIOC_MODER_MODER15, Port x configuration bits y =  0..15
: GPIOC_MODER_MODER14 ( %bb -- x addr ) 28 lshift GPIOC_MODER ; \ GPIOC_MODER_MODER14, Port x configuration bits y =  0..15
: GPIOC_MODER_MODER13 ( %bb -- x addr ) 26 lshift GPIOC_MODER ; \ GPIOC_MODER_MODER13, Port x configuration bits y =  0..15
: GPIOC_MODER_MODER12 ( %bb -- x addr ) 24 lshift GPIOC_MODER ; \ GPIOC_MODER_MODER12, Port x configuration bits y =  0..15
: GPIOC_MODER_MODER11 ( %bb -- x addr ) 22 lshift GPIOC_MODER ; \ GPIOC_MODER_MODER11, Port x configuration bits y =  0..15
: GPIOC_MODER_MODER10 ( %bb -- x addr ) 20 lshift GPIOC_MODER ; \ GPIOC_MODER_MODER10, Port x configuration bits y =  0..15
: GPIOC_MODER_MODER9 ( %bb -- x addr ) 18 lshift GPIOC_MODER ; \ GPIOC_MODER_MODER9, Port x configuration bits y =  0..15
: GPIOC_MODER_MODER8 ( %bb -- x addr ) 16 lshift GPIOC_MODER ; \ GPIOC_MODER_MODER8, Port x configuration bits y =  0..15
: GPIOC_MODER_MODER7 ( %bb -- x addr ) 14 lshift GPIOC_MODER ; \ GPIOC_MODER_MODER7, Port x configuration bits y =  0..15
: GPIOC_MODER_MODER6 ( %bb -- x addr ) 12 lshift GPIOC_MODER ; \ GPIOC_MODER_MODER6, Port x configuration bits y =  0..15
: GPIOC_MODER_MODER5 ( %bb -- x addr ) 10 lshift GPIOC_MODER ; \ GPIOC_MODER_MODER5, Port x configuration bits y =  0..15
: GPIOC_MODER_MODER4 ( %bb -- x addr ) 8 lshift GPIOC_MODER ; \ GPIOC_MODER_MODER4, Port x configuration bits y =  0..15
: GPIOC_MODER_MODER3 ( %bb -- x addr ) 6 lshift GPIOC_MODER ; \ GPIOC_MODER_MODER3, Port x configuration bits y =  0..15
: GPIOC_MODER_MODER2 ( %bb -- x addr ) 4 lshift GPIOC_MODER ; \ GPIOC_MODER_MODER2, Port x configuration bits y =  0..15
: GPIOC_MODER_MODER1 ( %bb -- x addr ) 2 lshift GPIOC_MODER ; \ GPIOC_MODER_MODER1, Port x configuration bits y =  0..15
: GPIOC_MODER_MODER0 ( %bb -- x addr ) GPIOC_MODER ; \ GPIOC_MODER_MODER0, Port x configuration bits y =  0..15
[then]

execute-defined? use-GPIOC defined? GPIOC_OTYPER_OT15 not and [if]
\ GPIOC_OTYPER (read-write) Reset:0x00000000
: GPIOC_OTYPER_OT15 ( -- x addr ) 15 bit GPIOC_OTYPER ; \ GPIOC_OTYPER_OT15, Port x configuration bits y =  0..15
: GPIOC_OTYPER_OT14 ( -- x addr ) 14 bit GPIOC_OTYPER ; \ GPIOC_OTYPER_OT14, Port x configuration bits y =  0..15
: GPIOC_OTYPER_OT13 ( -- x addr ) 13 bit GPIOC_OTYPER ; \ GPIOC_OTYPER_OT13, Port x configuration bits y =  0..15
: GPIOC_OTYPER_OT12 ( -- x addr ) 12 bit GPIOC_OTYPER ; \ GPIOC_OTYPER_OT12, Port x configuration bits y =  0..15
: GPIOC_OTYPER_OT11 ( -- x addr ) 11 bit GPIOC_OTYPER ; \ GPIOC_OTYPER_OT11, Port x configuration bits y =  0..15
: GPIOC_OTYPER_OT10 ( -- x addr ) 10 bit GPIOC_OTYPER ; \ GPIOC_OTYPER_OT10, Port x configuration bits y =  0..15
: GPIOC_OTYPER_OT9 ( -- x addr ) 9 bit GPIOC_OTYPER ; \ GPIOC_OTYPER_OT9, Port x configuration bits y =  0..15
: GPIOC_OTYPER_OT8 ( -- x addr ) 8 bit GPIOC_OTYPER ; \ GPIOC_OTYPER_OT8, Port x configuration bits y =  0..15
: GPIOC_OTYPER_OT7 ( -- x addr ) 7 bit GPIOC_OTYPER ; \ GPIOC_OTYPER_OT7, Port x configuration bits y =  0..15
: GPIOC_OTYPER_OT6 ( -- x addr ) 6 bit GPIOC_OTYPER ; \ GPIOC_OTYPER_OT6, Port x configuration bits y =  0..15
: GPIOC_OTYPER_OT5 ( -- x addr ) 5 bit GPIOC_OTYPER ; \ GPIOC_OTYPER_OT5, Port x configuration bits y =  0..15
: GPIOC_OTYPER_OT4 ( -- x addr ) 4 bit GPIOC_OTYPER ; \ GPIOC_OTYPER_OT4, Port x configuration bits y =  0..15
: GPIOC_OTYPER_OT3 ( -- x addr ) 3 bit GPIOC_OTYPER ; \ GPIOC_OTYPER_OT3, Port x configuration bits y =  0..15
: GPIOC_OTYPER_OT2 ( -- x addr ) 2 bit GPIOC_OTYPER ; \ GPIOC_OTYPER_OT2, Port x configuration bits y =  0..15
: GPIOC_OTYPER_OT1 ( -- x addr ) 1 bit GPIOC_OTYPER ; \ GPIOC_OTYPER_OT1, Port x configuration bits y =  0..15
: GPIOC_OTYPER_OT0 ( -- x addr ) 0 bit GPIOC_OTYPER ; \ GPIOC_OTYPER_OT0, Port x configuration bits y =  0..15
[then]

defined? use-GPIOC defined? GPIOC_OSPEEDR_OSPEEDR15 not and [if]
\ GPIOC_OSPEEDR (read-write) Reset:0x00000000
: GPIOC_OSPEEDR_OSPEEDR15 ( %bb -- x addr ) 30 lshift GPIOC_OSPEEDR ; \ GPIOC_OSPEEDR_OSPEEDR15, Port x configuration bits y =  0..15
: GPIOC_OSPEEDR_OSPEEDR14 ( %bb -- x addr ) 28 lshift GPIOC_OSPEEDR ; \ GPIOC_OSPEEDR_OSPEEDR14, Port x configuration bits y =  0..15
: GPIOC_OSPEEDR_OSPEEDR13 ( %bb -- x addr ) 26 lshift GPIOC_OSPEEDR ; \ GPIOC_OSPEEDR_OSPEEDR13, Port x configuration bits y =  0..15
: GPIOC_OSPEEDR_OSPEEDR12 ( %bb -- x addr ) 24 lshift GPIOC_OSPEEDR ; \ GPIOC_OSPEEDR_OSPEEDR12, Port x configuration bits y =  0..15
: GPIOC_OSPEEDR_OSPEEDR11 ( %bb -- x addr ) 22 lshift GPIOC_OSPEEDR ; \ GPIOC_OSPEEDR_OSPEEDR11, Port x configuration bits y =  0..15
: GPIOC_OSPEEDR_OSPEEDR10 ( %bb -- x addr ) 20 lshift GPIOC_OSPEEDR ; \ GPIOC_OSPEEDR_OSPEEDR10, Port x configuration bits y =  0..15
: GPIOC_OSPEEDR_OSPEEDR9 ( %bb -- x addr ) 18 lshift GPIOC_OSPEEDR ; \ GPIOC_OSPEEDR_OSPEEDR9, Port x configuration bits y =  0..15
: GPIOC_OSPEEDR_OSPEEDR8 ( %bb -- x addr ) 16 lshift GPIOC_OSPEEDR ; \ GPIOC_OSPEEDR_OSPEEDR8, Port x configuration bits y =  0..15
: GPIOC_OSPEEDR_OSPEEDR7 ( %bb -- x addr ) 14 lshift GPIOC_OSPEEDR ; \ GPIOC_OSPEEDR_OSPEEDR7, Port x configuration bits y =  0..15
: GPIOC_OSPEEDR_OSPEEDR6 ( %bb -- x addr ) 12 lshift GPIOC_OSPEEDR ; \ GPIOC_OSPEEDR_OSPEEDR6, Port x configuration bits y =  0..15
: GPIOC_OSPEEDR_OSPEEDR5 ( %bb -- x addr ) 10 lshift GPIOC_OSPEEDR ; \ GPIOC_OSPEEDR_OSPEEDR5, Port x configuration bits y =  0..15
: GPIOC_OSPEEDR_OSPEEDR4 ( %bb -- x addr ) 8 lshift GPIOC_OSPEEDR ; \ GPIOC_OSPEEDR_OSPEEDR4, Port x configuration bits y =  0..15
: GPIOC_OSPEEDR_OSPEEDR3 ( %bb -- x addr ) 6 lshift GPIOC_OSPEEDR ; \ GPIOC_OSPEEDR_OSPEEDR3, Port x configuration bits y =  0..15
: GPIOC_OSPEEDR_OSPEEDR2 ( %bb -- x addr ) 4 lshift GPIOC_OSPEEDR ; \ GPIOC_OSPEEDR_OSPEEDR2, Port x configuration bits y =  0..15
: GPIOC_OSPEEDR_OSPEEDR1 ( %bb -- x addr ) 2 lshift GPIOC_OSPEEDR ; \ GPIOC_OSPEEDR_OSPEEDR1, Port x configuration bits y =  0..15
: GPIOC_OSPEEDR_OSPEEDR0 ( %bb -- x addr ) GPIOC_OSPEEDR ; \ GPIOC_OSPEEDR_OSPEEDR0, Port x configuration bits y =  0..15
[then]

execute-defined? use-GPIOC defined? GPIOC_PUPDR_PUPDR15 not and [if]
\ GPIOC_PUPDR (read-write) Reset:0x00000000
: GPIOC_PUPDR_PUPDR15 ( %bb -- x addr ) 30 lshift GPIOC_PUPDR ; \ GPIOC_PUPDR_PUPDR15, Port x configuration bits y =  0..15
: GPIOC_PUPDR_PUPDR14 ( %bb -- x addr ) 28 lshift GPIOC_PUPDR ; \ GPIOC_PUPDR_PUPDR14, Port x configuration bits y =  0..15
: GPIOC_PUPDR_PUPDR13 ( %bb -- x addr ) 26 lshift GPIOC_PUPDR ; \ GPIOC_PUPDR_PUPDR13, Port x configuration bits y =  0..15
: GPIOC_PUPDR_PUPDR12 ( %bb -- x addr ) 24 lshift GPIOC_PUPDR ; \ GPIOC_PUPDR_PUPDR12, Port x configuration bits y =  0..15
: GPIOC_PUPDR_PUPDR11 ( %bb -- x addr ) 22 lshift GPIOC_PUPDR ; \ GPIOC_PUPDR_PUPDR11, Port x configuration bits y =  0..15
: GPIOC_PUPDR_PUPDR10 ( %bb -- x addr ) 20 lshift GPIOC_PUPDR ; \ GPIOC_PUPDR_PUPDR10, Port x configuration bits y =  0..15
: GPIOC_PUPDR_PUPDR9 ( %bb -- x addr ) 18 lshift GPIOC_PUPDR ; \ GPIOC_PUPDR_PUPDR9, Port x configuration bits y =  0..15
: GPIOC_PUPDR_PUPDR8 ( %bb -- x addr ) 16 lshift GPIOC_PUPDR ; \ GPIOC_PUPDR_PUPDR8, Port x configuration bits y =  0..15
: GPIOC_PUPDR_PUPDR7 ( %bb -- x addr ) 14 lshift GPIOC_PUPDR ; \ GPIOC_PUPDR_PUPDR7, Port x configuration bits y =  0..15
: GPIOC_PUPDR_PUPDR6 ( %bb -- x addr ) 12 lshift GPIOC_PUPDR ; \ GPIOC_PUPDR_PUPDR6, Port x configuration bits y =  0..15
: GPIOC_PUPDR_PUPDR5 ( %bb -- x addr ) 10 lshift GPIOC_PUPDR ; \ GPIOC_PUPDR_PUPDR5, Port x configuration bits y =  0..15
: GPIOC_PUPDR_PUPDR4 ( %bb -- x addr ) 8 lshift GPIOC_PUPDR ; \ GPIOC_PUPDR_PUPDR4, Port x configuration bits y =  0..15
: GPIOC_PUPDR_PUPDR3 ( %bb -- x addr ) 6 lshift GPIOC_PUPDR ; \ GPIOC_PUPDR_PUPDR3, Port x configuration bits y =  0..15
: GPIOC_PUPDR_PUPDR2 ( %bb -- x addr ) 4 lshift GPIOC_PUPDR ; \ GPIOC_PUPDR_PUPDR2, Port x configuration bits y =  0..15
: GPIOC_PUPDR_PUPDR1 ( %bb -- x addr ) 2 lshift GPIOC_PUPDR ; \ GPIOC_PUPDR_PUPDR1, Port x configuration bits y =  0..15
: GPIOC_PUPDR_PUPDR0 ( %bb -- x addr ) GPIOC_PUPDR ; \ GPIOC_PUPDR_PUPDR0, Port x configuration bits y =  0..15
[then]

defined? use-GPIOC defined? GPIOC_IDR_IDR15? not and [if]
\ GPIOC_IDR (read-only) Reset:0x00000000
: GPIOC_IDR_IDR15? ( --  1|0 ) 15 bit GPIOC_IDR bit@ ; \ GPIOC_IDR_IDR15, Port input data y =  0..15
: GPIOC_IDR_IDR14? ( --  1|0 ) 14 bit GPIOC_IDR bit@ ; \ GPIOC_IDR_IDR14, Port input data y =  0..15
: GPIOC_IDR_IDR13? ( --  1|0 ) 13 bit GPIOC_IDR bit@ ; \ GPIOC_IDR_IDR13, Port input data y =  0..15
: GPIOC_IDR_IDR12? ( --  1|0 ) 12 bit GPIOC_IDR bit@ ; \ GPIOC_IDR_IDR12, Port input data y =  0..15
: GPIOC_IDR_IDR11? ( --  1|0 ) 11 bit GPIOC_IDR bit@ ; \ GPIOC_IDR_IDR11, Port input data y =  0..15
: GPIOC_IDR_IDR10? ( --  1|0 ) 10 bit GPIOC_IDR bit@ ; \ GPIOC_IDR_IDR10, Port input data y =  0..15
: GPIOC_IDR_IDR9? ( --  1|0 ) 9 bit GPIOC_IDR bit@ ; \ GPIOC_IDR_IDR9, Port input data y =  0..15
: GPIOC_IDR_IDR8? ( --  1|0 ) 8 bit GPIOC_IDR bit@ ; \ GPIOC_IDR_IDR8, Port input data y =  0..15
: GPIOC_IDR_IDR7? ( --  1|0 ) 7 bit GPIOC_IDR bit@ ; \ GPIOC_IDR_IDR7, Port input data y =  0..15
: GPIOC_IDR_IDR6? ( --  1|0 ) 6 bit GPIOC_IDR bit@ ; \ GPIOC_IDR_IDR6, Port input data y =  0..15
: GPIOC_IDR_IDR5? ( --  1|0 ) 5 bit GPIOC_IDR bit@ ; \ GPIOC_IDR_IDR5, Port input data y =  0..15
: GPIOC_IDR_IDR4? ( --  1|0 ) 4 bit GPIOC_IDR bit@ ; \ GPIOC_IDR_IDR4, Port input data y =  0..15
: GPIOC_IDR_IDR3? ( --  1|0 ) 3 bit GPIOC_IDR bit@ ; \ GPIOC_IDR_IDR3, Port input data y =  0..15
: GPIOC_IDR_IDR2? ( --  1|0 ) 2 bit GPIOC_IDR bit@ ; \ GPIOC_IDR_IDR2, Port input data y =  0..15
: GPIOC_IDR_IDR1? ( --  1|0 ) 1 bit GPIOC_IDR bit@ ; \ GPIOC_IDR_IDR1, Port input data y =  0..15
: GPIOC_IDR_IDR0? ( --  1|0 ) 0 bit GPIOC_IDR bit@ ; \ GPIOC_IDR_IDR0, Port input data y =  0..15
[then]

execute-defined? use-GPIOC defined? GPIOC_ODR_ODR15 not and [if]
\ GPIOC_ODR (read-write) Reset:0x00000000
: GPIOC_ODR_ODR15 ( -- x addr ) 15 bit GPIOC_ODR ; \ GPIOC_ODR_ODR15, Port output data y =  0..15
: GPIOC_ODR_ODR14 ( -- x addr ) 14 bit GPIOC_ODR ; \ GPIOC_ODR_ODR14, Port output data y =  0..15
: GPIOC_ODR_ODR13 ( -- x addr ) 13 bit GPIOC_ODR ; \ GPIOC_ODR_ODR13, Port output data y =  0..15
: GPIOC_ODR_ODR12 ( -- x addr ) 12 bit GPIOC_ODR ; \ GPIOC_ODR_ODR12, Port output data y =  0..15
: GPIOC_ODR_ODR11 ( -- x addr ) 11 bit GPIOC_ODR ; \ GPIOC_ODR_ODR11, Port output data y =  0..15
: GPIOC_ODR_ODR10 ( -- x addr ) 10 bit GPIOC_ODR ; \ GPIOC_ODR_ODR10, Port output data y =  0..15
: GPIOC_ODR_ODR9 ( -- x addr ) 9 bit GPIOC_ODR ; \ GPIOC_ODR_ODR9, Port output data y =  0..15
: GPIOC_ODR_ODR8 ( -- x addr ) 8 bit GPIOC_ODR ; \ GPIOC_ODR_ODR8, Port output data y =  0..15
: GPIOC_ODR_ODR7 ( -- x addr ) 7 bit GPIOC_ODR ; \ GPIOC_ODR_ODR7, Port output data y =  0..15
: GPIOC_ODR_ODR6 ( -- x addr ) 6 bit GPIOC_ODR ; \ GPIOC_ODR_ODR6, Port output data y =  0..15
: GPIOC_ODR_ODR5 ( -- x addr ) 5 bit GPIOC_ODR ; \ GPIOC_ODR_ODR5, Port output data y =  0..15
: GPIOC_ODR_ODR4 ( -- x addr ) 4 bit GPIOC_ODR ; \ GPIOC_ODR_ODR4, Port output data y =  0..15
: GPIOC_ODR_ODR3 ( -- x addr ) 3 bit GPIOC_ODR ; \ GPIOC_ODR_ODR3, Port output data y =  0..15
: GPIOC_ODR_ODR2 ( -- x addr ) 2 bit GPIOC_ODR ; \ GPIOC_ODR_ODR2, Port output data y =  0..15
: GPIOC_ODR_ODR1 ( -- x addr ) 1 bit GPIOC_ODR ; \ GPIOC_ODR_ODR1, Port output data y =  0..15
: GPIOC_ODR_ODR0 ( -- x addr ) 0 bit GPIOC_ODR ; \ GPIOC_ODR_ODR0, Port output data y =  0..15
[then]

defined? use-GPIOC defined? GPIOC_BSRR_BR15 not and [if]
\ GPIOC_BSRR (write-only) Reset:0x00000000
: GPIOC_BSRR_BR15 ( -- ) 31 bit GPIOC_BSRR ! ; \ GPIOC_BSRR_BR15, Port x reset bit y y =  0..15
: GPIOC_BSRR_BR14 ( -- ) 30 bit GPIOC_BSRR ! ; \ GPIOC_BSRR_BR14, Port x reset bit y y =  0..15
: GPIOC_BSRR_BR13 ( -- ) 29 bit GPIOC_BSRR ! ; \ GPIOC_BSRR_BR13, Port x reset bit y y =  0..15
: GPIOC_BSRR_BR12 ( -- ) 28 bit GPIOC_BSRR ! ; \ GPIOC_BSRR_BR12, Port x reset bit y y =  0..15
: GPIOC_BSRR_BR11 ( -- ) 27 bit GPIOC_BSRR ! ; \ GPIOC_BSRR_BR11, Port x reset bit y y =  0..15
: GPIOC_BSRR_BR10 ( -- ) 26 bit GPIOC_BSRR ! ; \ GPIOC_BSRR_BR10, Port x reset bit y y =  0..15
: GPIOC_BSRR_BR9 ( -- ) 25 bit GPIOC_BSRR ! ; \ GPIOC_BSRR_BR9, Port x reset bit y y =  0..15
: GPIOC_BSRR_BR8 ( -- ) 24 bit GPIOC_BSRR ! ; \ GPIOC_BSRR_BR8, Port x reset bit y y =  0..15
: GPIOC_BSRR_BR7 ( -- ) 23 bit GPIOC_BSRR ! ; \ GPIOC_BSRR_BR7, Port x reset bit y y =  0..15
: GPIOC_BSRR_BR6 ( -- ) 22 bit GPIOC_BSRR ! ; \ GPIOC_BSRR_BR6, Port x reset bit y y =  0..15
: GPIOC_BSRR_BR5 ( -- ) 21 bit GPIOC_BSRR ! ; \ GPIOC_BSRR_BR5, Port x reset bit y y =  0..15
: GPIOC_BSRR_BR4 ( -- ) 20 bit GPIOC_BSRR ! ; \ GPIOC_BSRR_BR4, Port x reset bit y y =  0..15
: GPIOC_BSRR_BR3 ( -- ) 19 bit GPIOC_BSRR ! ; \ GPIOC_BSRR_BR3, Port x reset bit y y =  0..15
: GPIOC_BSRR_BR2 ( -- ) 18 bit GPIOC_BSRR ! ; \ GPIOC_BSRR_BR2, Port x reset bit y y =  0..15
: GPIOC_BSRR_BR1 ( -- ) 17 bit GPIOC_BSRR ! ; \ GPIOC_BSRR_BR1, Port x reset bit y y =  0..15
: GPIOC_BSRR_BR0 ( -- ) 16 bit GPIOC_BSRR ! ; \ GPIOC_BSRR_BR0, Port x set bit y y=  0..15
: GPIOC_BSRR_BS15 ( -- ) 15 bit GPIOC_BSRR ! ; \ GPIOC_BSRR_BS15, Port x set bit y y=  0..15
: GPIOC_BSRR_BS14 ( -- ) 14 bit GPIOC_BSRR ! ; \ GPIOC_BSRR_BS14, Port x set bit y y=  0..15
: GPIOC_BSRR_BS13 ( -- ) 13 bit GPIOC_BSRR ! ; \ GPIOC_BSRR_BS13, Port x set bit y y=  0..15
: GPIOC_BSRR_BS12 ( -- ) 12 bit GPIOC_BSRR ! ; \ GPIOC_BSRR_BS12, Port x set bit y y=  0..15
: GPIOC_BSRR_BS11 ( -- ) 11 bit GPIOC_BSRR ! ; \ GPIOC_BSRR_BS11, Port x set bit y y=  0..15
: GPIOC_BSRR_BS10 ( -- ) 10 bit GPIOC_BSRR ! ; \ GPIOC_BSRR_BS10, Port x set bit y y=  0..15
: GPIOC_BSRR_BS9 ( -- ) 9 bit GPIOC_BSRR ! ; \ GPIOC_BSRR_BS9, Port x set bit y y=  0..15
: GPIOC_BSRR_BS8 ( -- ) 8 bit GPIOC_BSRR ! ; \ GPIOC_BSRR_BS8, Port x set bit y y=  0..15
: GPIOC_BSRR_BS7 ( -- ) 7 bit GPIOC_BSRR ! ; \ GPIOC_BSRR_BS7, Port x set bit y y=  0..15
: GPIOC_BSRR_BS6 ( -- ) 6 bit GPIOC_BSRR ! ; \ GPIOC_BSRR_BS6, Port x set bit y y=  0..15
: GPIOC_BSRR_BS5 ( -- ) 5 bit GPIOC_BSRR ! ; \ GPIOC_BSRR_BS5, Port x set bit y y=  0..15
: GPIOC_BSRR_BS4 ( -- ) 4 bit GPIOC_BSRR ! ; \ GPIOC_BSRR_BS4, Port x set bit y y=  0..15
: GPIOC_BSRR_BS3 ( -- ) 3 bit GPIOC_BSRR ! ; \ GPIOC_BSRR_BS3, Port x set bit y y=  0..15
: GPIOC_BSRR_BS2 ( -- ) 2 bit GPIOC_BSRR ! ; \ GPIOC_BSRR_BS2, Port x set bit y y=  0..15
: GPIOC_BSRR_BS1 ( -- ) 1 bit GPIOC_BSRR ! ; \ GPIOC_BSRR_BS1, Port x set bit y y=  0..15
: GPIOC_BSRR_BS0 ( -- ) 0 bit GPIOC_BSRR ! ; \ GPIOC_BSRR_BS0, Port x set bit y y=  0..15
[then]

execute-defined? use-GPIOC defined? GPIOC_LCKR_LCKK not and [if]
\ GPIOC_LCKR (read-write) Reset:0x00000000
: GPIOC_LCKR_LCKK ( -- x addr ) 16 bit GPIOC_LCKR ; \ GPIOC_LCKR_LCKK, Port x lock bit y y=  0..15
: GPIOC_LCKR_LCK15 ( -- x addr ) 15 bit GPIOC_LCKR ; \ GPIOC_LCKR_LCK15, Port x lock bit y y=  0..15
: GPIOC_LCKR_LCK14 ( -- x addr ) 14 bit GPIOC_LCKR ; \ GPIOC_LCKR_LCK14, Port x lock bit y y=  0..15
: GPIOC_LCKR_LCK13 ( -- x addr ) 13 bit GPIOC_LCKR ; \ GPIOC_LCKR_LCK13, Port x lock bit y y=  0..15
: GPIOC_LCKR_LCK12 ( -- x addr ) 12 bit GPIOC_LCKR ; \ GPIOC_LCKR_LCK12, Port x lock bit y y=  0..15
: GPIOC_LCKR_LCK11 ( -- x addr ) 11 bit GPIOC_LCKR ; \ GPIOC_LCKR_LCK11, Port x lock bit y y=  0..15
: GPIOC_LCKR_LCK10 ( -- x addr ) 10 bit GPIOC_LCKR ; \ GPIOC_LCKR_LCK10, Port x lock bit y y=  0..15
: GPIOC_LCKR_LCK9 ( -- x addr ) 9 bit GPIOC_LCKR ; \ GPIOC_LCKR_LCK9, Port x lock bit y y=  0..15
: GPIOC_LCKR_LCK8 ( -- x addr ) 8 bit GPIOC_LCKR ; \ GPIOC_LCKR_LCK8, Port x lock bit y y=  0..15
: GPIOC_LCKR_LCK7 ( -- x addr ) 7 bit GPIOC_LCKR ; \ GPIOC_LCKR_LCK7, Port x lock bit y y=  0..15
: GPIOC_LCKR_LCK6 ( -- x addr ) 6 bit GPIOC_LCKR ; \ GPIOC_LCKR_LCK6, Port x lock bit y y=  0..15
: GPIOC_LCKR_LCK5 ( -- x addr ) 5 bit GPIOC_LCKR ; \ GPIOC_LCKR_LCK5, Port x lock bit y y=  0..15
: GPIOC_LCKR_LCK4 ( -- x addr ) 4 bit GPIOC_LCKR ; \ GPIOC_LCKR_LCK4, Port x lock bit y y=  0..15
: GPIOC_LCKR_LCK3 ( -- x addr ) 3 bit GPIOC_LCKR ; \ GPIOC_LCKR_LCK3, Port x lock bit y y=  0..15
: GPIOC_LCKR_LCK2 ( -- x addr ) 2 bit GPIOC_LCKR ; \ GPIOC_LCKR_LCK2, Port x lock bit y y=  0..15
: GPIOC_LCKR_LCK1 ( -- x addr ) 1 bit GPIOC_LCKR ; \ GPIOC_LCKR_LCK1, Port x lock bit y y=  0..15
: GPIOC_LCKR_LCK0 ( -- x addr ) 0 bit GPIOC_LCKR ; \ GPIOC_LCKR_LCK0, Port x lock bit y y=  0..15
[then]

defined? use-GPIOC defined? GPIOC_AFRL_AFRL7 not and [if]
\ GPIOC_AFRL (read-write) Reset:0x00000000
: GPIOC_AFRL_AFRL7 ( %bbbb -- x addr ) 28 lshift GPIOC_AFRL ; \ GPIOC_AFRL_AFRL7, Alternate function selection for port x  bit y y = 0..7
: GPIOC_AFRL_AFRL6 ( %bbbb -- x addr ) 24 lshift GPIOC_AFRL ; \ GPIOC_AFRL_AFRL6, Alternate function selection for port x  bit y y = 0..7
: GPIOC_AFRL_AFRL5 ( %bbbb -- x addr ) 20 lshift GPIOC_AFRL ; \ GPIOC_AFRL_AFRL5, Alternate function selection for port x  bit y y = 0..7
: GPIOC_AFRL_AFRL4 ( %bbbb -- x addr ) 16 lshift GPIOC_AFRL ; \ GPIOC_AFRL_AFRL4, Alternate function selection for port x  bit y y = 0..7
: GPIOC_AFRL_AFRL3 ( %bbbb -- x addr ) 12 lshift GPIOC_AFRL ; \ GPIOC_AFRL_AFRL3, Alternate function selection for port x  bit y y = 0..7
: GPIOC_AFRL_AFRL2 ( %bbbb -- x addr ) 8 lshift GPIOC_AFRL ; \ GPIOC_AFRL_AFRL2, Alternate function selection for port x  bit y y = 0..7
: GPIOC_AFRL_AFRL1 ( %bbbb -- x addr ) 4 lshift GPIOC_AFRL ; \ GPIOC_AFRL_AFRL1, Alternate function selection for port x  bit y y = 0..7
: GPIOC_AFRL_AFRL0 ( %bbbb -- x addr ) GPIOC_AFRL ; \ GPIOC_AFRL_AFRL0, Alternate function selection for port x  bit y y = 0..7
[then]

execute-defined? use-GPIOC defined? GPIOC_AFRH_AFRH15 not and [if]
\ GPIOC_AFRH (read-write) Reset:0x00000000
: GPIOC_AFRH_AFRH15 ( %bbbb -- x addr ) 28 lshift GPIOC_AFRH ; \ GPIOC_AFRH_AFRH15, Alternate function selection for port x  bit y y = 8..15
: GPIOC_AFRH_AFRH14 ( %bbbb -- x addr ) 24 lshift GPIOC_AFRH ; \ GPIOC_AFRH_AFRH14, Alternate function selection for port x  bit y y = 8..15
: GPIOC_AFRH_AFRH13 ( %bbbb -- x addr ) 20 lshift GPIOC_AFRH ; \ GPIOC_AFRH_AFRH13, Alternate function selection for port x  bit y y = 8..15
: GPIOC_AFRH_AFRH12 ( %bbbb -- x addr ) 16 lshift GPIOC_AFRH ; \ GPIOC_AFRH_AFRH12, Alternate function selection for port x  bit y y = 8..15
: GPIOC_AFRH_AFRH11 ( %bbbb -- x addr ) 12 lshift GPIOC_AFRH ; \ GPIOC_AFRH_AFRH11, Alternate function selection for port x  bit y y = 8..15
: GPIOC_AFRH_AFRH10 ( %bbbb -- x addr ) 8 lshift GPIOC_AFRH ; \ GPIOC_AFRH_AFRH10, Alternate function selection for port x  bit y y = 8..15
: GPIOC_AFRH_AFRH9 ( %bbbb -- x addr ) 4 lshift GPIOC_AFRH ; \ GPIOC_AFRH_AFRH9, Alternate function selection for port x  bit y y = 8..15
: GPIOC_AFRH_AFRH8 ( %bbbb -- x addr ) GPIOC_AFRH ; \ GPIOC_AFRH_AFRH8, Alternate function selection for port x  bit y y = 8..15
[then]

defined? use-GPIOD defined? GPIOD_MODER_MODER15 not and [if]
\ GPIOD_MODER (read-write) Reset:0x00000000
: GPIOD_MODER_MODER15 ( %bb -- x addr ) 30 lshift GPIOD_MODER ; \ GPIOD_MODER_MODER15, Port x configuration bits y =  0..15
: GPIOD_MODER_MODER14 ( %bb -- x addr ) 28 lshift GPIOD_MODER ; \ GPIOD_MODER_MODER14, Port x configuration bits y =  0..15
: GPIOD_MODER_MODER13 ( %bb -- x addr ) 26 lshift GPIOD_MODER ; \ GPIOD_MODER_MODER13, Port x configuration bits y =  0..15
: GPIOD_MODER_MODER12 ( %bb -- x addr ) 24 lshift GPIOD_MODER ; \ GPIOD_MODER_MODER12, Port x configuration bits y =  0..15
: GPIOD_MODER_MODER11 ( %bb -- x addr ) 22 lshift GPIOD_MODER ; \ GPIOD_MODER_MODER11, Port x configuration bits y =  0..15
: GPIOD_MODER_MODER10 ( %bb -- x addr ) 20 lshift GPIOD_MODER ; \ GPIOD_MODER_MODER10, Port x configuration bits y =  0..15
: GPIOD_MODER_MODER9 ( %bb -- x addr ) 18 lshift GPIOD_MODER ; \ GPIOD_MODER_MODER9, Port x configuration bits y =  0..15
: GPIOD_MODER_MODER8 ( %bb -- x addr ) 16 lshift GPIOD_MODER ; \ GPIOD_MODER_MODER8, Port x configuration bits y =  0..15
: GPIOD_MODER_MODER7 ( %bb -- x addr ) 14 lshift GPIOD_MODER ; \ GPIOD_MODER_MODER7, Port x configuration bits y =  0..15
: GPIOD_MODER_MODER6 ( %bb -- x addr ) 12 lshift GPIOD_MODER ; \ GPIOD_MODER_MODER6, Port x configuration bits y =  0..15
: GPIOD_MODER_MODER5 ( %bb -- x addr ) 10 lshift GPIOD_MODER ; \ GPIOD_MODER_MODER5, Port x configuration bits y =  0..15
: GPIOD_MODER_MODER4 ( %bb -- x addr ) 8 lshift GPIOD_MODER ; \ GPIOD_MODER_MODER4, Port x configuration bits y =  0..15
: GPIOD_MODER_MODER3 ( %bb -- x addr ) 6 lshift GPIOD_MODER ; \ GPIOD_MODER_MODER3, Port x configuration bits y =  0..15
: GPIOD_MODER_MODER2 ( %bb -- x addr ) 4 lshift GPIOD_MODER ; \ GPIOD_MODER_MODER2, Port x configuration bits y =  0..15
: GPIOD_MODER_MODER1 ( %bb -- x addr ) 2 lshift GPIOD_MODER ; \ GPIOD_MODER_MODER1, Port x configuration bits y =  0..15
: GPIOD_MODER_MODER0 ( %bb -- x addr ) GPIOD_MODER ; \ GPIOD_MODER_MODER0, Port x configuration bits y =  0..15
[then]

execute-defined? use-GPIOD defined? GPIOD_OTYPER_OT15 not and [if]
\ GPIOD_OTYPER (read-write) Reset:0x00000000
: GPIOD_OTYPER_OT15 ( -- x addr ) 15 bit GPIOD_OTYPER ; \ GPIOD_OTYPER_OT15, Port x configuration bits y =  0..15
: GPIOD_OTYPER_OT14 ( -- x addr ) 14 bit GPIOD_OTYPER ; \ GPIOD_OTYPER_OT14, Port x configuration bits y =  0..15
: GPIOD_OTYPER_OT13 ( -- x addr ) 13 bit GPIOD_OTYPER ; \ GPIOD_OTYPER_OT13, Port x configuration bits y =  0..15
: GPIOD_OTYPER_OT12 ( -- x addr ) 12 bit GPIOD_OTYPER ; \ GPIOD_OTYPER_OT12, Port x configuration bits y =  0..15
: GPIOD_OTYPER_OT11 ( -- x addr ) 11 bit GPIOD_OTYPER ; \ GPIOD_OTYPER_OT11, Port x configuration bits y =  0..15
: GPIOD_OTYPER_OT10 ( -- x addr ) 10 bit GPIOD_OTYPER ; \ GPIOD_OTYPER_OT10, Port x configuration bits y =  0..15
: GPIOD_OTYPER_OT9 ( -- x addr ) 9 bit GPIOD_OTYPER ; \ GPIOD_OTYPER_OT9, Port x configuration bits y =  0..15
: GPIOD_OTYPER_OT8 ( -- x addr ) 8 bit GPIOD_OTYPER ; \ GPIOD_OTYPER_OT8, Port x configuration bits y =  0..15
: GPIOD_OTYPER_OT7 ( -- x addr ) 7 bit GPIOD_OTYPER ; \ GPIOD_OTYPER_OT7, Port x configuration bits y =  0..15
: GPIOD_OTYPER_OT6 ( -- x addr ) 6 bit GPIOD_OTYPER ; \ GPIOD_OTYPER_OT6, Port x configuration bits y =  0..15
: GPIOD_OTYPER_OT5 ( -- x addr ) 5 bit GPIOD_OTYPER ; \ GPIOD_OTYPER_OT5, Port x configuration bits y =  0..15
: GPIOD_OTYPER_OT4 ( -- x addr ) 4 bit GPIOD_OTYPER ; \ GPIOD_OTYPER_OT4, Port x configuration bits y =  0..15
: GPIOD_OTYPER_OT3 ( -- x addr ) 3 bit GPIOD_OTYPER ; \ GPIOD_OTYPER_OT3, Port x configuration bits y =  0..15
: GPIOD_OTYPER_OT2 ( -- x addr ) 2 bit GPIOD_OTYPER ; \ GPIOD_OTYPER_OT2, Port x configuration bits y =  0..15
: GPIOD_OTYPER_OT1 ( -- x addr ) 1 bit GPIOD_OTYPER ; \ GPIOD_OTYPER_OT1, Port x configuration bits y =  0..15
: GPIOD_OTYPER_OT0 ( -- x addr ) 0 bit GPIOD_OTYPER ; \ GPIOD_OTYPER_OT0, Port x configuration bits y =  0..15
[then]

defined? use-GPIOD defined? GPIOD_OSPEEDR_OSPEEDR15 not and [if]
\ GPIOD_OSPEEDR (read-write) Reset:0x00000000
: GPIOD_OSPEEDR_OSPEEDR15 ( %bb -- x addr ) 30 lshift GPIOD_OSPEEDR ; \ GPIOD_OSPEEDR_OSPEEDR15, Port x configuration bits y =  0..15
: GPIOD_OSPEEDR_OSPEEDR14 ( %bb -- x addr ) 28 lshift GPIOD_OSPEEDR ; \ GPIOD_OSPEEDR_OSPEEDR14, Port x configuration bits y =  0..15
: GPIOD_OSPEEDR_OSPEEDR13 ( %bb -- x addr ) 26 lshift GPIOD_OSPEEDR ; \ GPIOD_OSPEEDR_OSPEEDR13, Port x configuration bits y =  0..15
: GPIOD_OSPEEDR_OSPEEDR12 ( %bb -- x addr ) 24 lshift GPIOD_OSPEEDR ; \ GPIOD_OSPEEDR_OSPEEDR12, Port x configuration bits y =  0..15
: GPIOD_OSPEEDR_OSPEEDR11 ( %bb -- x addr ) 22 lshift GPIOD_OSPEEDR ; \ GPIOD_OSPEEDR_OSPEEDR11, Port x configuration bits y =  0..15
: GPIOD_OSPEEDR_OSPEEDR10 ( %bb -- x addr ) 20 lshift GPIOD_OSPEEDR ; \ GPIOD_OSPEEDR_OSPEEDR10, Port x configuration bits y =  0..15
: GPIOD_OSPEEDR_OSPEEDR9 ( %bb -- x addr ) 18 lshift GPIOD_OSPEEDR ; \ GPIOD_OSPEEDR_OSPEEDR9, Port x configuration bits y =  0..15
: GPIOD_OSPEEDR_OSPEEDR8 ( %bb -- x addr ) 16 lshift GPIOD_OSPEEDR ; \ GPIOD_OSPEEDR_OSPEEDR8, Port x configuration bits y =  0..15
: GPIOD_OSPEEDR_OSPEEDR7 ( %bb -- x addr ) 14 lshift GPIOD_OSPEEDR ; \ GPIOD_OSPEEDR_OSPEEDR7, Port x configuration bits y =  0..15
: GPIOD_OSPEEDR_OSPEEDR6 ( %bb -- x addr ) 12 lshift GPIOD_OSPEEDR ; \ GPIOD_OSPEEDR_OSPEEDR6, Port x configuration bits y =  0..15
: GPIOD_OSPEEDR_OSPEEDR5 ( %bb -- x addr ) 10 lshift GPIOD_OSPEEDR ; \ GPIOD_OSPEEDR_OSPEEDR5, Port x configuration bits y =  0..15
: GPIOD_OSPEEDR_OSPEEDR4 ( %bb -- x addr ) 8 lshift GPIOD_OSPEEDR ; \ GPIOD_OSPEEDR_OSPEEDR4, Port x configuration bits y =  0..15
: GPIOD_OSPEEDR_OSPEEDR3 ( %bb -- x addr ) 6 lshift GPIOD_OSPEEDR ; \ GPIOD_OSPEEDR_OSPEEDR3, Port x configuration bits y =  0..15
: GPIOD_OSPEEDR_OSPEEDR2 ( %bb -- x addr ) 4 lshift GPIOD_OSPEEDR ; \ GPIOD_OSPEEDR_OSPEEDR2, Port x configuration bits y =  0..15
: GPIOD_OSPEEDR_OSPEEDR1 ( %bb -- x addr ) 2 lshift GPIOD_OSPEEDR ; \ GPIOD_OSPEEDR_OSPEEDR1, Port x configuration bits y =  0..15
: GPIOD_OSPEEDR_OSPEEDR0 ( %bb -- x addr ) GPIOD_OSPEEDR ; \ GPIOD_OSPEEDR_OSPEEDR0, Port x configuration bits y =  0..15
[then]

execute-defined? use-GPIOD defined? GPIOD_PUPDR_PUPDR15 not and [if]
\ GPIOD_PUPDR (read-write) Reset:0x00000000
: GPIOD_PUPDR_PUPDR15 ( %bb -- x addr ) 30 lshift GPIOD_PUPDR ; \ GPIOD_PUPDR_PUPDR15, Port x configuration bits y =  0..15
: GPIOD_PUPDR_PUPDR14 ( %bb -- x addr ) 28 lshift GPIOD_PUPDR ; \ GPIOD_PUPDR_PUPDR14, Port x configuration bits y =  0..15
: GPIOD_PUPDR_PUPDR13 ( %bb -- x addr ) 26 lshift GPIOD_PUPDR ; \ GPIOD_PUPDR_PUPDR13, Port x configuration bits y =  0..15
: GPIOD_PUPDR_PUPDR12 ( %bb -- x addr ) 24 lshift GPIOD_PUPDR ; \ GPIOD_PUPDR_PUPDR12, Port x configuration bits y =  0..15
: GPIOD_PUPDR_PUPDR11 ( %bb -- x addr ) 22 lshift GPIOD_PUPDR ; \ GPIOD_PUPDR_PUPDR11, Port x configuration bits y =  0..15
: GPIOD_PUPDR_PUPDR10 ( %bb -- x addr ) 20 lshift GPIOD_PUPDR ; \ GPIOD_PUPDR_PUPDR10, Port x configuration bits y =  0..15
: GPIOD_PUPDR_PUPDR9 ( %bb -- x addr ) 18 lshift GPIOD_PUPDR ; \ GPIOD_PUPDR_PUPDR9, Port x configuration bits y =  0..15
: GPIOD_PUPDR_PUPDR8 ( %bb -- x addr ) 16 lshift GPIOD_PUPDR ; \ GPIOD_PUPDR_PUPDR8, Port x configuration bits y =  0..15
: GPIOD_PUPDR_PUPDR7 ( %bb -- x addr ) 14 lshift GPIOD_PUPDR ; \ GPIOD_PUPDR_PUPDR7, Port x configuration bits y =  0..15
: GPIOD_PUPDR_PUPDR6 ( %bb -- x addr ) 12 lshift GPIOD_PUPDR ; \ GPIOD_PUPDR_PUPDR6, Port x configuration bits y =  0..15
: GPIOD_PUPDR_PUPDR5 ( %bb -- x addr ) 10 lshift GPIOD_PUPDR ; \ GPIOD_PUPDR_PUPDR5, Port x configuration bits y =  0..15
: GPIOD_PUPDR_PUPDR4 ( %bb -- x addr ) 8 lshift GPIOD_PUPDR ; \ GPIOD_PUPDR_PUPDR4, Port x configuration bits y =  0..15
: GPIOD_PUPDR_PUPDR3 ( %bb -- x addr ) 6 lshift GPIOD_PUPDR ; \ GPIOD_PUPDR_PUPDR3, Port x configuration bits y =  0..15
: GPIOD_PUPDR_PUPDR2 ( %bb -- x addr ) 4 lshift GPIOD_PUPDR ; \ GPIOD_PUPDR_PUPDR2, Port x configuration bits y =  0..15
: GPIOD_PUPDR_PUPDR1 ( %bb -- x addr ) 2 lshift GPIOD_PUPDR ; \ GPIOD_PUPDR_PUPDR1, Port x configuration bits y =  0..15
: GPIOD_PUPDR_PUPDR0 ( %bb -- x addr ) GPIOD_PUPDR ; \ GPIOD_PUPDR_PUPDR0, Port x configuration bits y =  0..15
[then]

defined? use-GPIOD defined? GPIOD_IDR_IDR15? not and [if]
\ GPIOD_IDR (read-only) Reset:0x00000000
: GPIOD_IDR_IDR15? ( --  1|0 ) 15 bit GPIOD_IDR bit@ ; \ GPIOD_IDR_IDR15, Port input data y =  0..15
: GPIOD_IDR_IDR14? ( --  1|0 ) 14 bit GPIOD_IDR bit@ ; \ GPIOD_IDR_IDR14, Port input data y =  0..15
: GPIOD_IDR_IDR13? ( --  1|0 ) 13 bit GPIOD_IDR bit@ ; \ GPIOD_IDR_IDR13, Port input data y =  0..15
: GPIOD_IDR_IDR12? ( --  1|0 ) 12 bit GPIOD_IDR bit@ ; \ GPIOD_IDR_IDR12, Port input data y =  0..15
: GPIOD_IDR_IDR11? ( --  1|0 ) 11 bit GPIOD_IDR bit@ ; \ GPIOD_IDR_IDR11, Port input data y =  0..15
: GPIOD_IDR_IDR10? ( --  1|0 ) 10 bit GPIOD_IDR bit@ ; \ GPIOD_IDR_IDR10, Port input data y =  0..15
: GPIOD_IDR_IDR9? ( --  1|0 ) 9 bit GPIOD_IDR bit@ ; \ GPIOD_IDR_IDR9, Port input data y =  0..15
: GPIOD_IDR_IDR8? ( --  1|0 ) 8 bit GPIOD_IDR bit@ ; \ GPIOD_IDR_IDR8, Port input data y =  0..15
: GPIOD_IDR_IDR7? ( --  1|0 ) 7 bit GPIOD_IDR bit@ ; \ GPIOD_IDR_IDR7, Port input data y =  0..15
: GPIOD_IDR_IDR6? ( --  1|0 ) 6 bit GPIOD_IDR bit@ ; \ GPIOD_IDR_IDR6, Port input data y =  0..15
: GPIOD_IDR_IDR5? ( --  1|0 ) 5 bit GPIOD_IDR bit@ ; \ GPIOD_IDR_IDR5, Port input data y =  0..15
: GPIOD_IDR_IDR4? ( --  1|0 ) 4 bit GPIOD_IDR bit@ ; \ GPIOD_IDR_IDR4, Port input data y =  0..15
: GPIOD_IDR_IDR3? ( --  1|0 ) 3 bit GPIOD_IDR bit@ ; \ GPIOD_IDR_IDR3, Port input data y =  0..15
: GPIOD_IDR_IDR2? ( --  1|0 ) 2 bit GPIOD_IDR bit@ ; \ GPIOD_IDR_IDR2, Port input data y =  0..15
: GPIOD_IDR_IDR1? ( --  1|0 ) 1 bit GPIOD_IDR bit@ ; \ GPIOD_IDR_IDR1, Port input data y =  0..15
: GPIOD_IDR_IDR0? ( --  1|0 ) 0 bit GPIOD_IDR bit@ ; \ GPIOD_IDR_IDR0, Port input data y =  0..15
[then]

execute-defined? use-GPIOD defined? GPIOD_ODR_ODR15 not and [if]
\ GPIOD_ODR (read-write) Reset:0x00000000
: GPIOD_ODR_ODR15 ( -- x addr ) 15 bit GPIOD_ODR ; \ GPIOD_ODR_ODR15, Port output data y =  0..15
: GPIOD_ODR_ODR14 ( -- x addr ) 14 bit GPIOD_ODR ; \ GPIOD_ODR_ODR14, Port output data y =  0..15
: GPIOD_ODR_ODR13 ( -- x addr ) 13 bit GPIOD_ODR ; \ GPIOD_ODR_ODR13, Port output data y =  0..15
: GPIOD_ODR_ODR12 ( -- x addr ) 12 bit GPIOD_ODR ; \ GPIOD_ODR_ODR12, Port output data y =  0..15
: GPIOD_ODR_ODR11 ( -- x addr ) 11 bit GPIOD_ODR ; \ GPIOD_ODR_ODR11, Port output data y =  0..15
: GPIOD_ODR_ODR10 ( -- x addr ) 10 bit GPIOD_ODR ; \ GPIOD_ODR_ODR10, Port output data y =  0..15
: GPIOD_ODR_ODR9 ( -- x addr ) 9 bit GPIOD_ODR ; \ GPIOD_ODR_ODR9, Port output data y =  0..15
: GPIOD_ODR_ODR8 ( -- x addr ) 8 bit GPIOD_ODR ; \ GPIOD_ODR_ODR8, Port output data y =  0..15
: GPIOD_ODR_ODR7 ( -- x addr ) 7 bit GPIOD_ODR ; \ GPIOD_ODR_ODR7, Port output data y =  0..15
: GPIOD_ODR_ODR6 ( -- x addr ) 6 bit GPIOD_ODR ; \ GPIOD_ODR_ODR6, Port output data y =  0..15
: GPIOD_ODR_ODR5 ( -- x addr ) 5 bit GPIOD_ODR ; \ GPIOD_ODR_ODR5, Port output data y =  0..15
: GPIOD_ODR_ODR4 ( -- x addr ) 4 bit GPIOD_ODR ; \ GPIOD_ODR_ODR4, Port output data y =  0..15
: GPIOD_ODR_ODR3 ( -- x addr ) 3 bit GPIOD_ODR ; \ GPIOD_ODR_ODR3, Port output data y =  0..15
: GPIOD_ODR_ODR2 ( -- x addr ) 2 bit GPIOD_ODR ; \ GPIOD_ODR_ODR2, Port output data y =  0..15
: GPIOD_ODR_ODR1 ( -- x addr ) 1 bit GPIOD_ODR ; \ GPIOD_ODR_ODR1, Port output data y =  0..15
: GPIOD_ODR_ODR0 ( -- x addr ) 0 bit GPIOD_ODR ; \ GPIOD_ODR_ODR0, Port output data y =  0..15
[then]

defined? use-GPIOD defined? GPIOD_BSRR_BR15 not and [if]
\ GPIOD_BSRR (write-only) Reset:0x00000000
: GPIOD_BSRR_BR15 ( -- ) 31 bit GPIOD_BSRR ! ; \ GPIOD_BSRR_BR15, Port x reset bit y y =  0..15
: GPIOD_BSRR_BR14 ( -- ) 30 bit GPIOD_BSRR ! ; \ GPIOD_BSRR_BR14, Port x reset bit y y =  0..15
: GPIOD_BSRR_BR13 ( -- ) 29 bit GPIOD_BSRR ! ; \ GPIOD_BSRR_BR13, Port x reset bit y y =  0..15
: GPIOD_BSRR_BR12 ( -- ) 28 bit GPIOD_BSRR ! ; \ GPIOD_BSRR_BR12, Port x reset bit y y =  0..15
: GPIOD_BSRR_BR11 ( -- ) 27 bit GPIOD_BSRR ! ; \ GPIOD_BSRR_BR11, Port x reset bit y y =  0..15
: GPIOD_BSRR_BR10 ( -- ) 26 bit GPIOD_BSRR ! ; \ GPIOD_BSRR_BR10, Port x reset bit y y =  0..15
: GPIOD_BSRR_BR9 ( -- ) 25 bit GPIOD_BSRR ! ; \ GPIOD_BSRR_BR9, Port x reset bit y y =  0..15
: GPIOD_BSRR_BR8 ( -- ) 24 bit GPIOD_BSRR ! ; \ GPIOD_BSRR_BR8, Port x reset bit y y =  0..15
: GPIOD_BSRR_BR7 ( -- ) 23 bit GPIOD_BSRR ! ; \ GPIOD_BSRR_BR7, Port x reset bit y y =  0..15
: GPIOD_BSRR_BR6 ( -- ) 22 bit GPIOD_BSRR ! ; \ GPIOD_BSRR_BR6, Port x reset bit y y =  0..15
: GPIOD_BSRR_BR5 ( -- ) 21 bit GPIOD_BSRR ! ; \ GPIOD_BSRR_BR5, Port x reset bit y y =  0..15
: GPIOD_BSRR_BR4 ( -- ) 20 bit GPIOD_BSRR ! ; \ GPIOD_BSRR_BR4, Port x reset bit y y =  0..15
: GPIOD_BSRR_BR3 ( -- ) 19 bit GPIOD_BSRR ! ; \ GPIOD_BSRR_BR3, Port x reset bit y y =  0..15
: GPIOD_BSRR_BR2 ( -- ) 18 bit GPIOD_BSRR ! ; \ GPIOD_BSRR_BR2, Port x reset bit y y =  0..15
: GPIOD_BSRR_BR1 ( -- ) 17 bit GPIOD_BSRR ! ; \ GPIOD_BSRR_BR1, Port x reset bit y y =  0..15
: GPIOD_BSRR_BR0 ( -- ) 16 bit GPIOD_BSRR ! ; \ GPIOD_BSRR_BR0, Port x set bit y y=  0..15
: GPIOD_BSRR_BS15 ( -- ) 15 bit GPIOD_BSRR ! ; \ GPIOD_BSRR_BS15, Port x set bit y y=  0..15
: GPIOD_BSRR_BS14 ( -- ) 14 bit GPIOD_BSRR ! ; \ GPIOD_BSRR_BS14, Port x set bit y y=  0..15
: GPIOD_BSRR_BS13 ( -- ) 13 bit GPIOD_BSRR ! ; \ GPIOD_BSRR_BS13, Port x set bit y y=  0..15
: GPIOD_BSRR_BS12 ( -- ) 12 bit GPIOD_BSRR ! ; \ GPIOD_BSRR_BS12, Port x set bit y y=  0..15
: GPIOD_BSRR_BS11 ( -- ) 11 bit GPIOD_BSRR ! ; \ GPIOD_BSRR_BS11, Port x set bit y y=  0..15
: GPIOD_BSRR_BS10 ( -- ) 10 bit GPIOD_BSRR ! ; \ GPIOD_BSRR_BS10, Port x set bit y y=  0..15
: GPIOD_BSRR_BS9 ( -- ) 9 bit GPIOD_BSRR ! ; \ GPIOD_BSRR_BS9, Port x set bit y y=  0..15
: GPIOD_BSRR_BS8 ( -- ) 8 bit GPIOD_BSRR ! ; \ GPIOD_BSRR_BS8, Port x set bit y y=  0..15
: GPIOD_BSRR_BS7 ( -- ) 7 bit GPIOD_BSRR ! ; \ GPIOD_BSRR_BS7, Port x set bit y y=  0..15
: GPIOD_BSRR_BS6 ( -- ) 6 bit GPIOD_BSRR ! ; \ GPIOD_BSRR_BS6, Port x set bit y y=  0..15
: GPIOD_BSRR_BS5 ( -- ) 5 bit GPIOD_BSRR ! ; \ GPIOD_BSRR_BS5, Port x set bit y y=  0..15
: GPIOD_BSRR_BS4 ( -- ) 4 bit GPIOD_BSRR ! ; \ GPIOD_BSRR_BS4, Port x set bit y y=  0..15
: GPIOD_BSRR_BS3 ( -- ) 3 bit GPIOD_BSRR ! ; \ GPIOD_BSRR_BS3, Port x set bit y y=  0..15
: GPIOD_BSRR_BS2 ( -- ) 2 bit GPIOD_BSRR ! ; \ GPIOD_BSRR_BS2, Port x set bit y y=  0..15
: GPIOD_BSRR_BS1 ( -- ) 1 bit GPIOD_BSRR ! ; \ GPIOD_BSRR_BS1, Port x set bit y y=  0..15
: GPIOD_BSRR_BS0 ( -- ) 0 bit GPIOD_BSRR ! ; \ GPIOD_BSRR_BS0, Port x set bit y y=  0..15
[then]

execute-defined? use-GPIOD defined? GPIOD_LCKR_LCKK not and [if]
\ GPIOD_LCKR (read-write) Reset:0x00000000
: GPIOD_LCKR_LCKK ( -- x addr ) 16 bit GPIOD_LCKR ; \ GPIOD_LCKR_LCKK, Port x lock bit y y=  0..15
: GPIOD_LCKR_LCK15 ( -- x addr ) 15 bit GPIOD_LCKR ; \ GPIOD_LCKR_LCK15, Port x lock bit y y=  0..15
: GPIOD_LCKR_LCK14 ( -- x addr ) 14 bit GPIOD_LCKR ; \ GPIOD_LCKR_LCK14, Port x lock bit y y=  0..15
: GPIOD_LCKR_LCK13 ( -- x addr ) 13 bit GPIOD_LCKR ; \ GPIOD_LCKR_LCK13, Port x lock bit y y=  0..15
: GPIOD_LCKR_LCK12 ( -- x addr ) 12 bit GPIOD_LCKR ; \ GPIOD_LCKR_LCK12, Port x lock bit y y=  0..15
: GPIOD_LCKR_LCK11 ( -- x addr ) 11 bit GPIOD_LCKR ; \ GPIOD_LCKR_LCK11, Port x lock bit y y=  0..15
: GPIOD_LCKR_LCK10 ( -- x addr ) 10 bit GPIOD_LCKR ; \ GPIOD_LCKR_LCK10, Port x lock bit y y=  0..15
: GPIOD_LCKR_LCK9 ( -- x addr ) 9 bit GPIOD_LCKR ; \ GPIOD_LCKR_LCK9, Port x lock bit y y=  0..15
: GPIOD_LCKR_LCK8 ( -- x addr ) 8 bit GPIOD_LCKR ; \ GPIOD_LCKR_LCK8, Port x lock bit y y=  0..15
: GPIOD_LCKR_LCK7 ( -- x addr ) 7 bit GPIOD_LCKR ; \ GPIOD_LCKR_LCK7, Port x lock bit y y=  0..15
: GPIOD_LCKR_LCK6 ( -- x addr ) 6 bit GPIOD_LCKR ; \ GPIOD_LCKR_LCK6, Port x lock bit y y=  0..15
: GPIOD_LCKR_LCK5 ( -- x addr ) 5 bit GPIOD_LCKR ; \ GPIOD_LCKR_LCK5, Port x lock bit y y=  0..15
: GPIOD_LCKR_LCK4 ( -- x addr ) 4 bit GPIOD_LCKR ; \ GPIOD_LCKR_LCK4, Port x lock bit y y=  0..15
: GPIOD_LCKR_LCK3 ( -- x addr ) 3 bit GPIOD_LCKR ; \ GPIOD_LCKR_LCK3, Port x lock bit y y=  0..15
: GPIOD_LCKR_LCK2 ( -- x addr ) 2 bit GPIOD_LCKR ; \ GPIOD_LCKR_LCK2, Port x lock bit y y=  0..15
: GPIOD_LCKR_LCK1 ( -- x addr ) 1 bit GPIOD_LCKR ; \ GPIOD_LCKR_LCK1, Port x lock bit y y=  0..15
: GPIOD_LCKR_LCK0 ( -- x addr ) 0 bit GPIOD_LCKR ; \ GPIOD_LCKR_LCK0, Port x lock bit y y=  0..15
[then]

defined? use-GPIOD defined? GPIOD_AFRL_AFRL7 not and [if]
\ GPIOD_AFRL (read-write) Reset:0x00000000
: GPIOD_AFRL_AFRL7 ( %bbbb -- x addr ) 28 lshift GPIOD_AFRL ; \ GPIOD_AFRL_AFRL7, Alternate function selection for port x  bit y y = 0..7
: GPIOD_AFRL_AFRL6 ( %bbbb -- x addr ) 24 lshift GPIOD_AFRL ; \ GPIOD_AFRL_AFRL6, Alternate function selection for port x  bit y y = 0..7
: GPIOD_AFRL_AFRL5 ( %bbbb -- x addr ) 20 lshift GPIOD_AFRL ; \ GPIOD_AFRL_AFRL5, Alternate function selection for port x  bit y y = 0..7
: GPIOD_AFRL_AFRL4 ( %bbbb -- x addr ) 16 lshift GPIOD_AFRL ; \ GPIOD_AFRL_AFRL4, Alternate function selection for port x  bit y y = 0..7
: GPIOD_AFRL_AFRL3 ( %bbbb -- x addr ) 12 lshift GPIOD_AFRL ; \ GPIOD_AFRL_AFRL3, Alternate function selection for port x  bit y y = 0..7
: GPIOD_AFRL_AFRL2 ( %bbbb -- x addr ) 8 lshift GPIOD_AFRL ; \ GPIOD_AFRL_AFRL2, Alternate function selection for port x  bit y y = 0..7
: GPIOD_AFRL_AFRL1 ( %bbbb -- x addr ) 4 lshift GPIOD_AFRL ; \ GPIOD_AFRL_AFRL1, Alternate function selection for port x  bit y y = 0..7
: GPIOD_AFRL_AFRL0 ( %bbbb -- x addr ) GPIOD_AFRL ; \ GPIOD_AFRL_AFRL0, Alternate function selection for port x  bit y y = 0..7
[then]

execute-defined? use-GPIOD defined? GPIOD_AFRH_AFRH15 not and [if]
\ GPIOD_AFRH (read-write) Reset:0x00000000
: GPIOD_AFRH_AFRH15 ( %bbbb -- x addr ) 28 lshift GPIOD_AFRH ; \ GPIOD_AFRH_AFRH15, Alternate function selection for port x  bit y y = 8..15
: GPIOD_AFRH_AFRH14 ( %bbbb -- x addr ) 24 lshift GPIOD_AFRH ; \ GPIOD_AFRH_AFRH14, Alternate function selection for port x  bit y y = 8..15
: GPIOD_AFRH_AFRH13 ( %bbbb -- x addr ) 20 lshift GPIOD_AFRH ; \ GPIOD_AFRH_AFRH13, Alternate function selection for port x  bit y y = 8..15
: GPIOD_AFRH_AFRH12 ( %bbbb -- x addr ) 16 lshift GPIOD_AFRH ; \ GPIOD_AFRH_AFRH12, Alternate function selection for port x  bit y y = 8..15
: GPIOD_AFRH_AFRH11 ( %bbbb -- x addr ) 12 lshift GPIOD_AFRH ; \ GPIOD_AFRH_AFRH11, Alternate function selection for port x  bit y y = 8..15
: GPIOD_AFRH_AFRH10 ( %bbbb -- x addr ) 8 lshift GPIOD_AFRH ; \ GPIOD_AFRH_AFRH10, Alternate function selection for port x  bit y y = 8..15
: GPIOD_AFRH_AFRH9 ( %bbbb -- x addr ) 4 lshift GPIOD_AFRH ; \ GPIOD_AFRH_AFRH9, Alternate function selection for port x  bit y y = 8..15
: GPIOD_AFRH_AFRH8 ( %bbbb -- x addr ) GPIOD_AFRH ; \ GPIOD_AFRH_AFRH8, Alternate function selection for port x  bit y y = 8..15
[then]

defined? use-GPIOE defined? GPIOE_MODER_MODER15 not and [if]
\ GPIOE_MODER (read-write) Reset:0x00000000
: GPIOE_MODER_MODER15 ( %bb -- x addr ) 30 lshift GPIOE_MODER ; \ GPIOE_MODER_MODER15, Port x configuration bits y =  0..15
: GPIOE_MODER_MODER14 ( %bb -- x addr ) 28 lshift GPIOE_MODER ; \ GPIOE_MODER_MODER14, Port x configuration bits y =  0..15
: GPIOE_MODER_MODER13 ( %bb -- x addr ) 26 lshift GPIOE_MODER ; \ GPIOE_MODER_MODER13, Port x configuration bits y =  0..15
: GPIOE_MODER_MODER12 ( %bb -- x addr ) 24 lshift GPIOE_MODER ; \ GPIOE_MODER_MODER12, Port x configuration bits y =  0..15
: GPIOE_MODER_MODER11 ( %bb -- x addr ) 22 lshift GPIOE_MODER ; \ GPIOE_MODER_MODER11, Port x configuration bits y =  0..15
: GPIOE_MODER_MODER10 ( %bb -- x addr ) 20 lshift GPIOE_MODER ; \ GPIOE_MODER_MODER10, Port x configuration bits y =  0..15
: GPIOE_MODER_MODER9 ( %bb -- x addr ) 18 lshift GPIOE_MODER ; \ GPIOE_MODER_MODER9, Port x configuration bits y =  0..15
: GPIOE_MODER_MODER8 ( %bb -- x addr ) 16 lshift GPIOE_MODER ; \ GPIOE_MODER_MODER8, Port x configuration bits y =  0..15
: GPIOE_MODER_MODER7 ( %bb -- x addr ) 14 lshift GPIOE_MODER ; \ GPIOE_MODER_MODER7, Port x configuration bits y =  0..15
: GPIOE_MODER_MODER6 ( %bb -- x addr ) 12 lshift GPIOE_MODER ; \ GPIOE_MODER_MODER6, Port x configuration bits y =  0..15
: GPIOE_MODER_MODER5 ( %bb -- x addr ) 10 lshift GPIOE_MODER ; \ GPIOE_MODER_MODER5, Port x configuration bits y =  0..15
: GPIOE_MODER_MODER4 ( %bb -- x addr ) 8 lshift GPIOE_MODER ; \ GPIOE_MODER_MODER4, Port x configuration bits y =  0..15
: GPIOE_MODER_MODER3 ( %bb -- x addr ) 6 lshift GPIOE_MODER ; \ GPIOE_MODER_MODER3, Port x configuration bits y =  0..15
: GPIOE_MODER_MODER2 ( %bb -- x addr ) 4 lshift GPIOE_MODER ; \ GPIOE_MODER_MODER2, Port x configuration bits y =  0..15
: GPIOE_MODER_MODER1 ( %bb -- x addr ) 2 lshift GPIOE_MODER ; \ GPIOE_MODER_MODER1, Port x configuration bits y =  0..15
: GPIOE_MODER_MODER0 ( %bb -- x addr ) GPIOE_MODER ; \ GPIOE_MODER_MODER0, Port x configuration bits y =  0..15
[then]

execute-defined? use-GPIOE defined? GPIOE_OTYPER_OT15 not and [if]
\ GPIOE_OTYPER (read-write) Reset:0x00000000
: GPIOE_OTYPER_OT15 ( -- x addr ) 15 bit GPIOE_OTYPER ; \ GPIOE_OTYPER_OT15, Port x configuration bits y =  0..15
: GPIOE_OTYPER_OT14 ( -- x addr ) 14 bit GPIOE_OTYPER ; \ GPIOE_OTYPER_OT14, Port x configuration bits y =  0..15
: GPIOE_OTYPER_OT13 ( -- x addr ) 13 bit GPIOE_OTYPER ; \ GPIOE_OTYPER_OT13, Port x configuration bits y =  0..15
: GPIOE_OTYPER_OT12 ( -- x addr ) 12 bit GPIOE_OTYPER ; \ GPIOE_OTYPER_OT12, Port x configuration bits y =  0..15
: GPIOE_OTYPER_OT11 ( -- x addr ) 11 bit GPIOE_OTYPER ; \ GPIOE_OTYPER_OT11, Port x configuration bits y =  0..15
: GPIOE_OTYPER_OT10 ( -- x addr ) 10 bit GPIOE_OTYPER ; \ GPIOE_OTYPER_OT10, Port x configuration bits y =  0..15
: GPIOE_OTYPER_OT9 ( -- x addr ) 9 bit GPIOE_OTYPER ; \ GPIOE_OTYPER_OT9, Port x configuration bits y =  0..15
: GPIOE_OTYPER_OT8 ( -- x addr ) 8 bit GPIOE_OTYPER ; \ GPIOE_OTYPER_OT8, Port x configuration bits y =  0..15
: GPIOE_OTYPER_OT7 ( -- x addr ) 7 bit GPIOE_OTYPER ; \ GPIOE_OTYPER_OT7, Port x configuration bits y =  0..15
: GPIOE_OTYPER_OT6 ( -- x addr ) 6 bit GPIOE_OTYPER ; \ GPIOE_OTYPER_OT6, Port x configuration bits y =  0..15
: GPIOE_OTYPER_OT5 ( -- x addr ) 5 bit GPIOE_OTYPER ; \ GPIOE_OTYPER_OT5, Port x configuration bits y =  0..15
: GPIOE_OTYPER_OT4 ( -- x addr ) 4 bit GPIOE_OTYPER ; \ GPIOE_OTYPER_OT4, Port x configuration bits y =  0..15
: GPIOE_OTYPER_OT3 ( -- x addr ) 3 bit GPIOE_OTYPER ; \ GPIOE_OTYPER_OT3, Port x configuration bits y =  0..15
: GPIOE_OTYPER_OT2 ( -- x addr ) 2 bit GPIOE_OTYPER ; \ GPIOE_OTYPER_OT2, Port x configuration bits y =  0..15
: GPIOE_OTYPER_OT1 ( -- x addr ) 1 bit GPIOE_OTYPER ; \ GPIOE_OTYPER_OT1, Port x configuration bits y =  0..15
: GPIOE_OTYPER_OT0 ( -- x addr ) 0 bit GPIOE_OTYPER ; \ GPIOE_OTYPER_OT0, Port x configuration bits y =  0..15
[then]

defined? use-GPIOE defined? GPIOE_OSPEEDR_OSPEEDR15 not and [if]
\ GPIOE_OSPEEDR (read-write) Reset:0x00000000
: GPIOE_OSPEEDR_OSPEEDR15 ( %bb -- x addr ) 30 lshift GPIOE_OSPEEDR ; \ GPIOE_OSPEEDR_OSPEEDR15, Port x configuration bits y =  0..15
: GPIOE_OSPEEDR_OSPEEDR14 ( %bb -- x addr ) 28 lshift GPIOE_OSPEEDR ; \ GPIOE_OSPEEDR_OSPEEDR14, Port x configuration bits y =  0..15
: GPIOE_OSPEEDR_OSPEEDR13 ( %bb -- x addr ) 26 lshift GPIOE_OSPEEDR ; \ GPIOE_OSPEEDR_OSPEEDR13, Port x configuration bits y =  0..15
: GPIOE_OSPEEDR_OSPEEDR12 ( %bb -- x addr ) 24 lshift GPIOE_OSPEEDR ; \ GPIOE_OSPEEDR_OSPEEDR12, Port x configuration bits y =  0..15
: GPIOE_OSPEEDR_OSPEEDR11 ( %bb -- x addr ) 22 lshift GPIOE_OSPEEDR ; \ GPIOE_OSPEEDR_OSPEEDR11, Port x configuration bits y =  0..15
: GPIOE_OSPEEDR_OSPEEDR10 ( %bb -- x addr ) 20 lshift GPIOE_OSPEEDR ; \ GPIOE_OSPEEDR_OSPEEDR10, Port x configuration bits y =  0..15
: GPIOE_OSPEEDR_OSPEEDR9 ( %bb -- x addr ) 18 lshift GPIOE_OSPEEDR ; \ GPIOE_OSPEEDR_OSPEEDR9, Port x configuration bits y =  0..15
: GPIOE_OSPEEDR_OSPEEDR8 ( %bb -- x addr ) 16 lshift GPIOE_OSPEEDR ; \ GPIOE_OSPEEDR_OSPEEDR8, Port x configuration bits y =  0..15
: GPIOE_OSPEEDR_OSPEEDR7 ( %bb -- x addr ) 14 lshift GPIOE_OSPEEDR ; \ GPIOE_OSPEEDR_OSPEEDR7, Port x configuration bits y =  0..15
: GPIOE_OSPEEDR_OSPEEDR6 ( %bb -- x addr ) 12 lshift GPIOE_OSPEEDR ; \ GPIOE_OSPEEDR_OSPEEDR6, Port x configuration bits y =  0..15
: GPIOE_OSPEEDR_OSPEEDR5 ( %bb -- x addr ) 10 lshift GPIOE_OSPEEDR ; \ GPIOE_OSPEEDR_OSPEEDR5, Port x configuration bits y =  0..15
: GPIOE_OSPEEDR_OSPEEDR4 ( %bb -- x addr ) 8 lshift GPIOE_OSPEEDR ; \ GPIOE_OSPEEDR_OSPEEDR4, Port x configuration bits y =  0..15
: GPIOE_OSPEEDR_OSPEEDR3 ( %bb -- x addr ) 6 lshift GPIOE_OSPEEDR ; \ GPIOE_OSPEEDR_OSPEEDR3, Port x configuration bits y =  0..15
: GPIOE_OSPEEDR_OSPEEDR2 ( %bb -- x addr ) 4 lshift GPIOE_OSPEEDR ; \ GPIOE_OSPEEDR_OSPEEDR2, Port x configuration bits y =  0..15
: GPIOE_OSPEEDR_OSPEEDR1 ( %bb -- x addr ) 2 lshift GPIOE_OSPEEDR ; \ GPIOE_OSPEEDR_OSPEEDR1, Port x configuration bits y =  0..15
: GPIOE_OSPEEDR_OSPEEDR0 ( %bb -- x addr ) GPIOE_OSPEEDR ; \ GPIOE_OSPEEDR_OSPEEDR0, Port x configuration bits y =  0..15
[then]

execute-defined? use-GPIOE defined? GPIOE_PUPDR_PUPDR15 not and [if]
\ GPIOE_PUPDR (read-write) Reset:0x00000000
: GPIOE_PUPDR_PUPDR15 ( %bb -- x addr ) 30 lshift GPIOE_PUPDR ; \ GPIOE_PUPDR_PUPDR15, Port x configuration bits y =  0..15
: GPIOE_PUPDR_PUPDR14 ( %bb -- x addr ) 28 lshift GPIOE_PUPDR ; \ GPIOE_PUPDR_PUPDR14, Port x configuration bits y =  0..15
: GPIOE_PUPDR_PUPDR13 ( %bb -- x addr ) 26 lshift GPIOE_PUPDR ; \ GPIOE_PUPDR_PUPDR13, Port x configuration bits y =  0..15
: GPIOE_PUPDR_PUPDR12 ( %bb -- x addr ) 24 lshift GPIOE_PUPDR ; \ GPIOE_PUPDR_PUPDR12, Port x configuration bits y =  0..15
: GPIOE_PUPDR_PUPDR11 ( %bb -- x addr ) 22 lshift GPIOE_PUPDR ; \ GPIOE_PUPDR_PUPDR11, Port x configuration bits y =  0..15
: GPIOE_PUPDR_PUPDR10 ( %bb -- x addr ) 20 lshift GPIOE_PUPDR ; \ GPIOE_PUPDR_PUPDR10, Port x configuration bits y =  0..15
: GPIOE_PUPDR_PUPDR9 ( %bb -- x addr ) 18 lshift GPIOE_PUPDR ; \ GPIOE_PUPDR_PUPDR9, Port x configuration bits y =  0..15
: GPIOE_PUPDR_PUPDR8 ( %bb -- x addr ) 16 lshift GPIOE_PUPDR ; \ GPIOE_PUPDR_PUPDR8, Port x configuration bits y =  0..15
: GPIOE_PUPDR_PUPDR7 ( %bb -- x addr ) 14 lshift GPIOE_PUPDR ; \ GPIOE_PUPDR_PUPDR7, Port x configuration bits y =  0..15
: GPIOE_PUPDR_PUPDR6 ( %bb -- x addr ) 12 lshift GPIOE_PUPDR ; \ GPIOE_PUPDR_PUPDR6, Port x configuration bits y =  0..15
: GPIOE_PUPDR_PUPDR5 ( %bb -- x addr ) 10 lshift GPIOE_PUPDR ; \ GPIOE_PUPDR_PUPDR5, Port x configuration bits y =  0..15
: GPIOE_PUPDR_PUPDR4 ( %bb -- x addr ) 8 lshift GPIOE_PUPDR ; \ GPIOE_PUPDR_PUPDR4, Port x configuration bits y =  0..15
: GPIOE_PUPDR_PUPDR3 ( %bb -- x addr ) 6 lshift GPIOE_PUPDR ; \ GPIOE_PUPDR_PUPDR3, Port x configuration bits y =  0..15
: GPIOE_PUPDR_PUPDR2 ( %bb -- x addr ) 4 lshift GPIOE_PUPDR ; \ GPIOE_PUPDR_PUPDR2, Port x configuration bits y =  0..15
: GPIOE_PUPDR_PUPDR1 ( %bb -- x addr ) 2 lshift GPIOE_PUPDR ; \ GPIOE_PUPDR_PUPDR1, Port x configuration bits y =  0..15
: GPIOE_PUPDR_PUPDR0 ( %bb -- x addr ) GPIOE_PUPDR ; \ GPIOE_PUPDR_PUPDR0, Port x configuration bits y =  0..15
[then]

defined? use-GPIOE defined? GPIOE_IDR_IDR15? not and [if]
\ GPIOE_IDR (read-only) Reset:0x00000000
: GPIOE_IDR_IDR15? ( --  1|0 ) 15 bit GPIOE_IDR bit@ ; \ GPIOE_IDR_IDR15, Port input data y =  0..15
: GPIOE_IDR_IDR14? ( --  1|0 ) 14 bit GPIOE_IDR bit@ ; \ GPIOE_IDR_IDR14, Port input data y =  0..15
: GPIOE_IDR_IDR13? ( --  1|0 ) 13 bit GPIOE_IDR bit@ ; \ GPIOE_IDR_IDR13, Port input data y =  0..15
: GPIOE_IDR_IDR12? ( --  1|0 ) 12 bit GPIOE_IDR bit@ ; \ GPIOE_IDR_IDR12, Port input data y =  0..15
: GPIOE_IDR_IDR11? ( --  1|0 ) 11 bit GPIOE_IDR bit@ ; \ GPIOE_IDR_IDR11, Port input data y =  0..15
: GPIOE_IDR_IDR10? ( --  1|0 ) 10 bit GPIOE_IDR bit@ ; \ GPIOE_IDR_IDR10, Port input data y =  0..15
: GPIOE_IDR_IDR9? ( --  1|0 ) 9 bit GPIOE_IDR bit@ ; \ GPIOE_IDR_IDR9, Port input data y =  0..15
: GPIOE_IDR_IDR8? ( --  1|0 ) 8 bit GPIOE_IDR bit@ ; \ GPIOE_IDR_IDR8, Port input data y =  0..15
: GPIOE_IDR_IDR7? ( --  1|0 ) 7 bit GPIOE_IDR bit@ ; \ GPIOE_IDR_IDR7, Port input data y =  0..15
: GPIOE_IDR_IDR6? ( --  1|0 ) 6 bit GPIOE_IDR bit@ ; \ GPIOE_IDR_IDR6, Port input data y =  0..15
: GPIOE_IDR_IDR5? ( --  1|0 ) 5 bit GPIOE_IDR bit@ ; \ GPIOE_IDR_IDR5, Port input data y =  0..15
: GPIOE_IDR_IDR4? ( --  1|0 ) 4 bit GPIOE_IDR bit@ ; \ GPIOE_IDR_IDR4, Port input data y =  0..15
: GPIOE_IDR_IDR3? ( --  1|0 ) 3 bit GPIOE_IDR bit@ ; \ GPIOE_IDR_IDR3, Port input data y =  0..15
: GPIOE_IDR_IDR2? ( --  1|0 ) 2 bit GPIOE_IDR bit@ ; \ GPIOE_IDR_IDR2, Port input data y =  0..15
: GPIOE_IDR_IDR1? ( --  1|0 ) 1 bit GPIOE_IDR bit@ ; \ GPIOE_IDR_IDR1, Port input data y =  0..15
: GPIOE_IDR_IDR0? ( --  1|0 ) 0 bit GPIOE_IDR bit@ ; \ GPIOE_IDR_IDR0, Port input data y =  0..15
[then]

execute-defined? use-GPIOE defined? GPIOE_ODR_ODR15 not and [if]
\ GPIOE_ODR (read-write) Reset:0x00000000
: GPIOE_ODR_ODR15 ( -- x addr ) 15 bit GPIOE_ODR ; \ GPIOE_ODR_ODR15, Port output data y =  0..15
: GPIOE_ODR_ODR14 ( -- x addr ) 14 bit GPIOE_ODR ; \ GPIOE_ODR_ODR14, Port output data y =  0..15
: GPIOE_ODR_ODR13 ( -- x addr ) 13 bit GPIOE_ODR ; \ GPIOE_ODR_ODR13, Port output data y =  0..15
: GPIOE_ODR_ODR12 ( -- x addr ) 12 bit GPIOE_ODR ; \ GPIOE_ODR_ODR12, Port output data y =  0..15
: GPIOE_ODR_ODR11 ( -- x addr ) 11 bit GPIOE_ODR ; \ GPIOE_ODR_ODR11, Port output data y =  0..15
: GPIOE_ODR_ODR10 ( -- x addr ) 10 bit GPIOE_ODR ; \ GPIOE_ODR_ODR10, Port output data y =  0..15
: GPIOE_ODR_ODR9 ( -- x addr ) 9 bit GPIOE_ODR ; \ GPIOE_ODR_ODR9, Port output data y =  0..15
: GPIOE_ODR_ODR8 ( -- x addr ) 8 bit GPIOE_ODR ; \ GPIOE_ODR_ODR8, Port output data y =  0..15
: GPIOE_ODR_ODR7 ( -- x addr ) 7 bit GPIOE_ODR ; \ GPIOE_ODR_ODR7, Port output data y =  0..15
: GPIOE_ODR_ODR6 ( -- x addr ) 6 bit GPIOE_ODR ; \ GPIOE_ODR_ODR6, Port output data y =  0..15
: GPIOE_ODR_ODR5 ( -- x addr ) 5 bit GPIOE_ODR ; \ GPIOE_ODR_ODR5, Port output data y =  0..15
: GPIOE_ODR_ODR4 ( -- x addr ) 4 bit GPIOE_ODR ; \ GPIOE_ODR_ODR4, Port output data y =  0..15
: GPIOE_ODR_ODR3 ( -- x addr ) 3 bit GPIOE_ODR ; \ GPIOE_ODR_ODR3, Port output data y =  0..15
: GPIOE_ODR_ODR2 ( -- x addr ) 2 bit GPIOE_ODR ; \ GPIOE_ODR_ODR2, Port output data y =  0..15
: GPIOE_ODR_ODR1 ( -- x addr ) 1 bit GPIOE_ODR ; \ GPIOE_ODR_ODR1, Port output data y =  0..15
: GPIOE_ODR_ODR0 ( -- x addr ) 0 bit GPIOE_ODR ; \ GPIOE_ODR_ODR0, Port output data y =  0..15
[then]

defined? use-GPIOE defined? GPIOE_BSRR_BR15 not and [if]
\ GPIOE_BSRR (write-only) Reset:0x00000000
: GPIOE_BSRR_BR15 ( -- ) 31 bit GPIOE_BSRR ! ; \ GPIOE_BSRR_BR15, Port x reset bit y y =  0..15
: GPIOE_BSRR_BR14 ( -- ) 30 bit GPIOE_BSRR ! ; \ GPIOE_BSRR_BR14, Port x reset bit y y =  0..15
: GPIOE_BSRR_BR13 ( -- ) 29 bit GPIOE_BSRR ! ; \ GPIOE_BSRR_BR13, Port x reset bit y y =  0..15
: GPIOE_BSRR_BR12 ( -- ) 28 bit GPIOE_BSRR ! ; \ GPIOE_BSRR_BR12, Port x reset bit y y =  0..15
: GPIOE_BSRR_BR11 ( -- ) 27 bit GPIOE_BSRR ! ; \ GPIOE_BSRR_BR11, Port x reset bit y y =  0..15
: GPIOE_BSRR_BR10 ( -- ) 26 bit GPIOE_BSRR ! ; \ GPIOE_BSRR_BR10, Port x reset bit y y =  0..15
: GPIOE_BSRR_BR9 ( -- ) 25 bit GPIOE_BSRR ! ; \ GPIOE_BSRR_BR9, Port x reset bit y y =  0..15
: GPIOE_BSRR_BR8 ( -- ) 24 bit GPIOE_BSRR ! ; \ GPIOE_BSRR_BR8, Port x reset bit y y =  0..15
: GPIOE_BSRR_BR7 ( -- ) 23 bit GPIOE_BSRR ! ; \ GPIOE_BSRR_BR7, Port x reset bit y y =  0..15
: GPIOE_BSRR_BR6 ( -- ) 22 bit GPIOE_BSRR ! ; \ GPIOE_BSRR_BR6, Port x reset bit y y =  0..15
: GPIOE_BSRR_BR5 ( -- ) 21 bit GPIOE_BSRR ! ; \ GPIOE_BSRR_BR5, Port x reset bit y y =  0..15
: GPIOE_BSRR_BR4 ( -- ) 20 bit GPIOE_BSRR ! ; \ GPIOE_BSRR_BR4, Port x reset bit y y =  0..15
: GPIOE_BSRR_BR3 ( -- ) 19 bit GPIOE_BSRR ! ; \ GPIOE_BSRR_BR3, Port x reset bit y y =  0..15
: GPIOE_BSRR_BR2 ( -- ) 18 bit GPIOE_BSRR ! ; \ GPIOE_BSRR_BR2, Port x reset bit y y =  0..15
: GPIOE_BSRR_BR1 ( -- ) 17 bit GPIOE_BSRR ! ; \ GPIOE_BSRR_BR1, Port x reset bit y y =  0..15
: GPIOE_BSRR_BR0 ( -- ) 16 bit GPIOE_BSRR ! ; \ GPIOE_BSRR_BR0, Port x set bit y y=  0..15
: GPIOE_BSRR_BS15 ( -- ) 15 bit GPIOE_BSRR ! ; \ GPIOE_BSRR_BS15, Port x set bit y y=  0..15
: GPIOE_BSRR_BS14 ( -- ) 14 bit GPIOE_BSRR ! ; \ GPIOE_BSRR_BS14, Port x set bit y y=  0..15
: GPIOE_BSRR_BS13 ( -- ) 13 bit GPIOE_BSRR ! ; \ GPIOE_BSRR_BS13, Port x set bit y y=  0..15
: GPIOE_BSRR_BS12 ( -- ) 12 bit GPIOE_BSRR ! ; \ GPIOE_BSRR_BS12, Port x set bit y y=  0..15
: GPIOE_BSRR_BS11 ( -- ) 11 bit GPIOE_BSRR ! ; \ GPIOE_BSRR_BS11, Port x set bit y y=  0..15
: GPIOE_BSRR_BS10 ( -- ) 10 bit GPIOE_BSRR ! ; \ GPIOE_BSRR_BS10, Port x set bit y y=  0..15
: GPIOE_BSRR_BS9 ( -- ) 9 bit GPIOE_BSRR ! ; \ GPIOE_BSRR_BS9, Port x set bit y y=  0..15
: GPIOE_BSRR_BS8 ( -- ) 8 bit GPIOE_BSRR ! ; \ GPIOE_BSRR_BS8, Port x set bit y y=  0..15
: GPIOE_BSRR_BS7 ( -- ) 7 bit GPIOE_BSRR ! ; \ GPIOE_BSRR_BS7, Port x set bit y y=  0..15
: GPIOE_BSRR_BS6 ( -- ) 6 bit GPIOE_BSRR ! ; \ GPIOE_BSRR_BS6, Port x set bit y y=  0..15
: GPIOE_BSRR_BS5 ( -- ) 5 bit GPIOE_BSRR ! ; \ GPIOE_BSRR_BS5, Port x set bit y y=  0..15
: GPIOE_BSRR_BS4 ( -- ) 4 bit GPIOE_BSRR ! ; \ GPIOE_BSRR_BS4, Port x set bit y y=  0..15
: GPIOE_BSRR_BS3 ( -- ) 3 bit GPIOE_BSRR ! ; \ GPIOE_BSRR_BS3, Port x set bit y y=  0..15
: GPIOE_BSRR_BS2 ( -- ) 2 bit GPIOE_BSRR ! ; \ GPIOE_BSRR_BS2, Port x set bit y y=  0..15
: GPIOE_BSRR_BS1 ( -- ) 1 bit GPIOE_BSRR ! ; \ GPIOE_BSRR_BS1, Port x set bit y y=  0..15
: GPIOE_BSRR_BS0 ( -- ) 0 bit GPIOE_BSRR ! ; \ GPIOE_BSRR_BS0, Port x set bit y y=  0..15
[then]

execute-defined? use-GPIOE defined? GPIOE_LCKR_LCKK not and [if]
\ GPIOE_LCKR (read-write) Reset:0x00000000
: GPIOE_LCKR_LCKK ( -- x addr ) 16 bit GPIOE_LCKR ; \ GPIOE_LCKR_LCKK, Port x lock bit y y=  0..15
: GPIOE_LCKR_LCK15 ( -- x addr ) 15 bit GPIOE_LCKR ; \ GPIOE_LCKR_LCK15, Port x lock bit y y=  0..15
: GPIOE_LCKR_LCK14 ( -- x addr ) 14 bit GPIOE_LCKR ; \ GPIOE_LCKR_LCK14, Port x lock bit y y=  0..15
: GPIOE_LCKR_LCK13 ( -- x addr ) 13 bit GPIOE_LCKR ; \ GPIOE_LCKR_LCK13, Port x lock bit y y=  0..15
: GPIOE_LCKR_LCK12 ( -- x addr ) 12 bit GPIOE_LCKR ; \ GPIOE_LCKR_LCK12, Port x lock bit y y=  0..15
: GPIOE_LCKR_LCK11 ( -- x addr ) 11 bit GPIOE_LCKR ; \ GPIOE_LCKR_LCK11, Port x lock bit y y=  0..15
: GPIOE_LCKR_LCK10 ( -- x addr ) 10 bit GPIOE_LCKR ; \ GPIOE_LCKR_LCK10, Port x lock bit y y=  0..15
: GPIOE_LCKR_LCK9 ( -- x addr ) 9 bit GPIOE_LCKR ; \ GPIOE_LCKR_LCK9, Port x lock bit y y=  0..15
: GPIOE_LCKR_LCK8 ( -- x addr ) 8 bit GPIOE_LCKR ; \ GPIOE_LCKR_LCK8, Port x lock bit y y=  0..15
: GPIOE_LCKR_LCK7 ( -- x addr ) 7 bit GPIOE_LCKR ; \ GPIOE_LCKR_LCK7, Port x lock bit y y=  0..15
: GPIOE_LCKR_LCK6 ( -- x addr ) 6 bit GPIOE_LCKR ; \ GPIOE_LCKR_LCK6, Port x lock bit y y=  0..15
: GPIOE_LCKR_LCK5 ( -- x addr ) 5 bit GPIOE_LCKR ; \ GPIOE_LCKR_LCK5, Port x lock bit y y=  0..15
: GPIOE_LCKR_LCK4 ( -- x addr ) 4 bit GPIOE_LCKR ; \ GPIOE_LCKR_LCK4, Port x lock bit y y=  0..15
: GPIOE_LCKR_LCK3 ( -- x addr ) 3 bit GPIOE_LCKR ; \ GPIOE_LCKR_LCK3, Port x lock bit y y=  0..15
: GPIOE_LCKR_LCK2 ( -- x addr ) 2 bit GPIOE_LCKR ; \ GPIOE_LCKR_LCK2, Port x lock bit y y=  0..15
: GPIOE_LCKR_LCK1 ( -- x addr ) 1 bit GPIOE_LCKR ; \ GPIOE_LCKR_LCK1, Port x lock bit y y=  0..15
: GPIOE_LCKR_LCK0 ( -- x addr ) 0 bit GPIOE_LCKR ; \ GPIOE_LCKR_LCK0, Port x lock bit y y=  0..15
[then]

defined? use-GPIOE defined? GPIOE_AFRL_AFRL7 not and [if]
\ GPIOE_AFRL (read-write) Reset:0x00000000
: GPIOE_AFRL_AFRL7 ( %bbbb -- x addr ) 28 lshift GPIOE_AFRL ; \ GPIOE_AFRL_AFRL7, Alternate function selection for port x  bit y y = 0..7
: GPIOE_AFRL_AFRL6 ( %bbbb -- x addr ) 24 lshift GPIOE_AFRL ; \ GPIOE_AFRL_AFRL6, Alternate function selection for port x  bit y y = 0..7
: GPIOE_AFRL_AFRL5 ( %bbbb -- x addr ) 20 lshift GPIOE_AFRL ; \ GPIOE_AFRL_AFRL5, Alternate function selection for port x  bit y y = 0..7
: GPIOE_AFRL_AFRL4 ( %bbbb -- x addr ) 16 lshift GPIOE_AFRL ; \ GPIOE_AFRL_AFRL4, Alternate function selection for port x  bit y y = 0..7
: GPIOE_AFRL_AFRL3 ( %bbbb -- x addr ) 12 lshift GPIOE_AFRL ; \ GPIOE_AFRL_AFRL3, Alternate function selection for port x  bit y y = 0..7
: GPIOE_AFRL_AFRL2 ( %bbbb -- x addr ) 8 lshift GPIOE_AFRL ; \ GPIOE_AFRL_AFRL2, Alternate function selection for port x  bit y y = 0..7
: GPIOE_AFRL_AFRL1 ( %bbbb -- x addr ) 4 lshift GPIOE_AFRL ; \ GPIOE_AFRL_AFRL1, Alternate function selection for port x  bit y y = 0..7
: GPIOE_AFRL_AFRL0 ( %bbbb -- x addr ) GPIOE_AFRL ; \ GPIOE_AFRL_AFRL0, Alternate function selection for port x  bit y y = 0..7
[then]

execute-defined? use-GPIOE defined? GPIOE_AFRH_AFRH15 not and [if]
\ GPIOE_AFRH (read-write) Reset:0x00000000
: GPIOE_AFRH_AFRH15 ( %bbbb -- x addr ) 28 lshift GPIOE_AFRH ; \ GPIOE_AFRH_AFRH15, Alternate function selection for port x  bit y y = 8..15
: GPIOE_AFRH_AFRH14 ( %bbbb -- x addr ) 24 lshift GPIOE_AFRH ; \ GPIOE_AFRH_AFRH14, Alternate function selection for port x  bit y y = 8..15
: GPIOE_AFRH_AFRH13 ( %bbbb -- x addr ) 20 lshift GPIOE_AFRH ; \ GPIOE_AFRH_AFRH13, Alternate function selection for port x  bit y y = 8..15
: GPIOE_AFRH_AFRH12 ( %bbbb -- x addr ) 16 lshift GPIOE_AFRH ; \ GPIOE_AFRH_AFRH12, Alternate function selection for port x  bit y y = 8..15
: GPIOE_AFRH_AFRH11 ( %bbbb -- x addr ) 12 lshift GPIOE_AFRH ; \ GPIOE_AFRH_AFRH11, Alternate function selection for port x  bit y y = 8..15
: GPIOE_AFRH_AFRH10 ( %bbbb -- x addr ) 8 lshift GPIOE_AFRH ; \ GPIOE_AFRH_AFRH10, Alternate function selection for port x  bit y y = 8..15
: GPIOE_AFRH_AFRH9 ( %bbbb -- x addr ) 4 lshift GPIOE_AFRH ; \ GPIOE_AFRH_AFRH9, Alternate function selection for port x  bit y y = 8..15
: GPIOE_AFRH_AFRH8 ( %bbbb -- x addr ) GPIOE_AFRH ; \ GPIOE_AFRH_AFRH8, Alternate function selection for port x  bit y y = 8..15
[then]

defined? use-GPIOF defined? GPIOF_MODER_MODER15 not and [if]
\ GPIOF_MODER (read-write) Reset:0x00000000
: GPIOF_MODER_MODER15 ( %bb -- x addr ) 30 lshift GPIOF_MODER ; \ GPIOF_MODER_MODER15, Port x configuration bits y =  0..15
: GPIOF_MODER_MODER14 ( %bb -- x addr ) 28 lshift GPIOF_MODER ; \ GPIOF_MODER_MODER14, Port x configuration bits y =  0..15
: GPIOF_MODER_MODER13 ( %bb -- x addr ) 26 lshift GPIOF_MODER ; \ GPIOF_MODER_MODER13, Port x configuration bits y =  0..15
: GPIOF_MODER_MODER12 ( %bb -- x addr ) 24 lshift GPIOF_MODER ; \ GPIOF_MODER_MODER12, Port x configuration bits y =  0..15
: GPIOF_MODER_MODER11 ( %bb -- x addr ) 22 lshift GPIOF_MODER ; \ GPIOF_MODER_MODER11, Port x configuration bits y =  0..15
: GPIOF_MODER_MODER10 ( %bb -- x addr ) 20 lshift GPIOF_MODER ; \ GPIOF_MODER_MODER10, Port x configuration bits y =  0..15
: GPIOF_MODER_MODER9 ( %bb -- x addr ) 18 lshift GPIOF_MODER ; \ GPIOF_MODER_MODER9, Port x configuration bits y =  0..15
: GPIOF_MODER_MODER8 ( %bb -- x addr ) 16 lshift GPIOF_MODER ; \ GPIOF_MODER_MODER8, Port x configuration bits y =  0..15
: GPIOF_MODER_MODER7 ( %bb -- x addr ) 14 lshift GPIOF_MODER ; \ GPIOF_MODER_MODER7, Port x configuration bits y =  0..15
: GPIOF_MODER_MODER6 ( %bb -- x addr ) 12 lshift GPIOF_MODER ; \ GPIOF_MODER_MODER6, Port x configuration bits y =  0..15
: GPIOF_MODER_MODER5 ( %bb -- x addr ) 10 lshift GPIOF_MODER ; \ GPIOF_MODER_MODER5, Port x configuration bits y =  0..15
: GPIOF_MODER_MODER4 ( %bb -- x addr ) 8 lshift GPIOF_MODER ; \ GPIOF_MODER_MODER4, Port x configuration bits y =  0..15
: GPIOF_MODER_MODER3 ( %bb -- x addr ) 6 lshift GPIOF_MODER ; \ GPIOF_MODER_MODER3, Port x configuration bits y =  0..15
: GPIOF_MODER_MODER2 ( %bb -- x addr ) 4 lshift GPIOF_MODER ; \ GPIOF_MODER_MODER2, Port x configuration bits y =  0..15
: GPIOF_MODER_MODER1 ( %bb -- x addr ) 2 lshift GPIOF_MODER ; \ GPIOF_MODER_MODER1, Port x configuration bits y =  0..15
: GPIOF_MODER_MODER0 ( %bb -- x addr ) GPIOF_MODER ; \ GPIOF_MODER_MODER0, Port x configuration bits y =  0..15
[then]

execute-defined? use-GPIOF defined? GPIOF_OTYPER_OT15 not and [if]
\ GPIOF_OTYPER (read-write) Reset:0x00000000
: GPIOF_OTYPER_OT15 ( -- x addr ) 15 bit GPIOF_OTYPER ; \ GPIOF_OTYPER_OT15, Port x configuration bits y =  0..15
: GPIOF_OTYPER_OT14 ( -- x addr ) 14 bit GPIOF_OTYPER ; \ GPIOF_OTYPER_OT14, Port x configuration bits y =  0..15
: GPIOF_OTYPER_OT13 ( -- x addr ) 13 bit GPIOF_OTYPER ; \ GPIOF_OTYPER_OT13, Port x configuration bits y =  0..15
: GPIOF_OTYPER_OT12 ( -- x addr ) 12 bit GPIOF_OTYPER ; \ GPIOF_OTYPER_OT12, Port x configuration bits y =  0..15
: GPIOF_OTYPER_OT11 ( -- x addr ) 11 bit GPIOF_OTYPER ; \ GPIOF_OTYPER_OT11, Port x configuration bits y =  0..15
: GPIOF_OTYPER_OT10 ( -- x addr ) 10 bit GPIOF_OTYPER ; \ GPIOF_OTYPER_OT10, Port x configuration bits y =  0..15
: GPIOF_OTYPER_OT9 ( -- x addr ) 9 bit GPIOF_OTYPER ; \ GPIOF_OTYPER_OT9, Port x configuration bits y =  0..15
: GPIOF_OTYPER_OT8 ( -- x addr ) 8 bit GPIOF_OTYPER ; \ GPIOF_OTYPER_OT8, Port x configuration bits y =  0..15
: GPIOF_OTYPER_OT7 ( -- x addr ) 7 bit GPIOF_OTYPER ; \ GPIOF_OTYPER_OT7, Port x configuration bits y =  0..15
: GPIOF_OTYPER_OT6 ( -- x addr ) 6 bit GPIOF_OTYPER ; \ GPIOF_OTYPER_OT6, Port x configuration bits y =  0..15
: GPIOF_OTYPER_OT5 ( -- x addr ) 5 bit GPIOF_OTYPER ; \ GPIOF_OTYPER_OT5, Port x configuration bits y =  0..15
: GPIOF_OTYPER_OT4 ( -- x addr ) 4 bit GPIOF_OTYPER ; \ GPIOF_OTYPER_OT4, Port x configuration bits y =  0..15
: GPIOF_OTYPER_OT3 ( -- x addr ) 3 bit GPIOF_OTYPER ; \ GPIOF_OTYPER_OT3, Port x configuration bits y =  0..15
: GPIOF_OTYPER_OT2 ( -- x addr ) 2 bit GPIOF_OTYPER ; \ GPIOF_OTYPER_OT2, Port x configuration bits y =  0..15
: GPIOF_OTYPER_OT1 ( -- x addr ) 1 bit GPIOF_OTYPER ; \ GPIOF_OTYPER_OT1, Port x configuration bits y =  0..15
: GPIOF_OTYPER_OT0 ( -- x addr ) 0 bit GPIOF_OTYPER ; \ GPIOF_OTYPER_OT0, Port x configuration bits y =  0..15
[then]

defined? use-GPIOF defined? GPIOF_OSPEEDR_OSPEEDR15 not and [if]
\ GPIOF_OSPEEDR (read-write) Reset:0x00000000
: GPIOF_OSPEEDR_OSPEEDR15 ( %bb -- x addr ) 30 lshift GPIOF_OSPEEDR ; \ GPIOF_OSPEEDR_OSPEEDR15, Port x configuration bits y =  0..15
: GPIOF_OSPEEDR_OSPEEDR14 ( %bb -- x addr ) 28 lshift GPIOF_OSPEEDR ; \ GPIOF_OSPEEDR_OSPEEDR14, Port x configuration bits y =  0..15
: GPIOF_OSPEEDR_OSPEEDR13 ( %bb -- x addr ) 26 lshift GPIOF_OSPEEDR ; \ GPIOF_OSPEEDR_OSPEEDR13, Port x configuration bits y =  0..15
: GPIOF_OSPEEDR_OSPEEDR12 ( %bb -- x addr ) 24 lshift GPIOF_OSPEEDR ; \ GPIOF_OSPEEDR_OSPEEDR12, Port x configuration bits y =  0..15
: GPIOF_OSPEEDR_OSPEEDR11 ( %bb -- x addr ) 22 lshift GPIOF_OSPEEDR ; \ GPIOF_OSPEEDR_OSPEEDR11, Port x configuration bits y =  0..15
: GPIOF_OSPEEDR_OSPEEDR10 ( %bb -- x addr ) 20 lshift GPIOF_OSPEEDR ; \ GPIOF_OSPEEDR_OSPEEDR10, Port x configuration bits y =  0..15
: GPIOF_OSPEEDR_OSPEEDR9 ( %bb -- x addr ) 18 lshift GPIOF_OSPEEDR ; \ GPIOF_OSPEEDR_OSPEEDR9, Port x configuration bits y =  0..15
: GPIOF_OSPEEDR_OSPEEDR8 ( %bb -- x addr ) 16 lshift GPIOF_OSPEEDR ; \ GPIOF_OSPEEDR_OSPEEDR8, Port x configuration bits y =  0..15
: GPIOF_OSPEEDR_OSPEEDR7 ( %bb -- x addr ) 14 lshift GPIOF_OSPEEDR ; \ GPIOF_OSPEEDR_OSPEEDR7, Port x configuration bits y =  0..15
: GPIOF_OSPEEDR_OSPEEDR6 ( %bb -- x addr ) 12 lshift GPIOF_OSPEEDR ; \ GPIOF_OSPEEDR_OSPEEDR6, Port x configuration bits y =  0..15
: GPIOF_OSPEEDR_OSPEEDR5 ( %bb -- x addr ) 10 lshift GPIOF_OSPEEDR ; \ GPIOF_OSPEEDR_OSPEEDR5, Port x configuration bits y =  0..15
: GPIOF_OSPEEDR_OSPEEDR4 ( %bb -- x addr ) 8 lshift GPIOF_OSPEEDR ; \ GPIOF_OSPEEDR_OSPEEDR4, Port x configuration bits y =  0..15
: GPIOF_OSPEEDR_OSPEEDR3 ( %bb -- x addr ) 6 lshift GPIOF_OSPEEDR ; \ GPIOF_OSPEEDR_OSPEEDR3, Port x configuration bits y =  0..15
: GPIOF_OSPEEDR_OSPEEDR2 ( %bb -- x addr ) 4 lshift GPIOF_OSPEEDR ; \ GPIOF_OSPEEDR_OSPEEDR2, Port x configuration bits y =  0..15
: GPIOF_OSPEEDR_OSPEEDR1 ( %bb -- x addr ) 2 lshift GPIOF_OSPEEDR ; \ GPIOF_OSPEEDR_OSPEEDR1, Port x configuration bits y =  0..15
: GPIOF_OSPEEDR_OSPEEDR0 ( %bb -- x addr ) GPIOF_OSPEEDR ; \ GPIOF_OSPEEDR_OSPEEDR0, Port x configuration bits y =  0..15
[then]

execute-defined? use-GPIOF defined? GPIOF_PUPDR_PUPDR15 not and [if]
\ GPIOF_PUPDR (read-write) Reset:0x00000000
: GPIOF_PUPDR_PUPDR15 ( %bb -- x addr ) 30 lshift GPIOF_PUPDR ; \ GPIOF_PUPDR_PUPDR15, Port x configuration bits y =  0..15
: GPIOF_PUPDR_PUPDR14 ( %bb -- x addr ) 28 lshift GPIOF_PUPDR ; \ GPIOF_PUPDR_PUPDR14, Port x configuration bits y =  0..15
: GPIOF_PUPDR_PUPDR13 ( %bb -- x addr ) 26 lshift GPIOF_PUPDR ; \ GPIOF_PUPDR_PUPDR13, Port x configuration bits y =  0..15
: GPIOF_PUPDR_PUPDR12 ( %bb -- x addr ) 24 lshift GPIOF_PUPDR ; \ GPIOF_PUPDR_PUPDR12, Port x configuration bits y =  0..15
: GPIOF_PUPDR_PUPDR11 ( %bb -- x addr ) 22 lshift GPIOF_PUPDR ; \ GPIOF_PUPDR_PUPDR11, Port x configuration bits y =  0..15
: GPIOF_PUPDR_PUPDR10 ( %bb -- x addr ) 20 lshift GPIOF_PUPDR ; \ GPIOF_PUPDR_PUPDR10, Port x configuration bits y =  0..15
: GPIOF_PUPDR_PUPDR9 ( %bb -- x addr ) 18 lshift GPIOF_PUPDR ; \ GPIOF_PUPDR_PUPDR9, Port x configuration bits y =  0..15
: GPIOF_PUPDR_PUPDR8 ( %bb -- x addr ) 16 lshift GPIOF_PUPDR ; \ GPIOF_PUPDR_PUPDR8, Port x configuration bits y =  0..15
: GPIOF_PUPDR_PUPDR7 ( %bb -- x addr ) 14 lshift GPIOF_PUPDR ; \ GPIOF_PUPDR_PUPDR7, Port x configuration bits y =  0..15
: GPIOF_PUPDR_PUPDR6 ( %bb -- x addr ) 12 lshift GPIOF_PUPDR ; \ GPIOF_PUPDR_PUPDR6, Port x configuration bits y =  0..15
: GPIOF_PUPDR_PUPDR5 ( %bb -- x addr ) 10 lshift GPIOF_PUPDR ; \ GPIOF_PUPDR_PUPDR5, Port x configuration bits y =  0..15
: GPIOF_PUPDR_PUPDR4 ( %bb -- x addr ) 8 lshift GPIOF_PUPDR ; \ GPIOF_PUPDR_PUPDR4, Port x configuration bits y =  0..15
: GPIOF_PUPDR_PUPDR3 ( %bb -- x addr ) 6 lshift GPIOF_PUPDR ; \ GPIOF_PUPDR_PUPDR3, Port x configuration bits y =  0..15
: GPIOF_PUPDR_PUPDR2 ( %bb -- x addr ) 4 lshift GPIOF_PUPDR ; \ GPIOF_PUPDR_PUPDR2, Port x configuration bits y =  0..15
: GPIOF_PUPDR_PUPDR1 ( %bb -- x addr ) 2 lshift GPIOF_PUPDR ; \ GPIOF_PUPDR_PUPDR1, Port x configuration bits y =  0..15
: GPIOF_PUPDR_PUPDR0 ( %bb -- x addr ) GPIOF_PUPDR ; \ GPIOF_PUPDR_PUPDR0, Port x configuration bits y =  0..15
[then]

defined? use-GPIOF defined? GPIOF_IDR_IDR15? not and [if]
\ GPIOF_IDR (read-only) Reset:0x00000000
: GPIOF_IDR_IDR15? ( --  1|0 ) 15 bit GPIOF_IDR bit@ ; \ GPIOF_IDR_IDR15, Port input data y =  0..15
: GPIOF_IDR_IDR14? ( --  1|0 ) 14 bit GPIOF_IDR bit@ ; \ GPIOF_IDR_IDR14, Port input data y =  0..15
: GPIOF_IDR_IDR13? ( --  1|0 ) 13 bit GPIOF_IDR bit@ ; \ GPIOF_IDR_IDR13, Port input data y =  0..15
: GPIOF_IDR_IDR12? ( --  1|0 ) 12 bit GPIOF_IDR bit@ ; \ GPIOF_IDR_IDR12, Port input data y =  0..15
: GPIOF_IDR_IDR11? ( --  1|0 ) 11 bit GPIOF_IDR bit@ ; \ GPIOF_IDR_IDR11, Port input data y =  0..15
: GPIOF_IDR_IDR10? ( --  1|0 ) 10 bit GPIOF_IDR bit@ ; \ GPIOF_IDR_IDR10, Port input data y =  0..15
: GPIOF_IDR_IDR9? ( --  1|0 ) 9 bit GPIOF_IDR bit@ ; \ GPIOF_IDR_IDR9, Port input data y =  0..15
: GPIOF_IDR_IDR8? ( --  1|0 ) 8 bit GPIOF_IDR bit@ ; \ GPIOF_IDR_IDR8, Port input data y =  0..15
: GPIOF_IDR_IDR7? ( --  1|0 ) 7 bit GPIOF_IDR bit@ ; \ GPIOF_IDR_IDR7, Port input data y =  0..15
: GPIOF_IDR_IDR6? ( --  1|0 ) 6 bit GPIOF_IDR bit@ ; \ GPIOF_IDR_IDR6, Port input data y =  0..15
: GPIOF_IDR_IDR5? ( --  1|0 ) 5 bit GPIOF_IDR bit@ ; \ GPIOF_IDR_IDR5, Port input data y =  0..15
: GPIOF_IDR_IDR4? ( --  1|0 ) 4 bit GPIOF_IDR bit@ ; \ GPIOF_IDR_IDR4, Port input data y =  0..15
: GPIOF_IDR_IDR3? ( --  1|0 ) 3 bit GPIOF_IDR bit@ ; \ GPIOF_IDR_IDR3, Port input data y =  0..15
: GPIOF_IDR_IDR2? ( --  1|0 ) 2 bit GPIOF_IDR bit@ ; \ GPIOF_IDR_IDR2, Port input data y =  0..15
: GPIOF_IDR_IDR1? ( --  1|0 ) 1 bit GPIOF_IDR bit@ ; \ GPIOF_IDR_IDR1, Port input data y =  0..15
: GPIOF_IDR_IDR0? ( --  1|0 ) 0 bit GPIOF_IDR bit@ ; \ GPIOF_IDR_IDR0, Port input data y =  0..15
[then]

execute-defined? use-GPIOF defined? GPIOF_ODR_ODR15 not and [if]
\ GPIOF_ODR (read-write) Reset:0x00000000
: GPIOF_ODR_ODR15 ( -- x addr ) 15 bit GPIOF_ODR ; \ GPIOF_ODR_ODR15, Port output data y =  0..15
: GPIOF_ODR_ODR14 ( -- x addr ) 14 bit GPIOF_ODR ; \ GPIOF_ODR_ODR14, Port output data y =  0..15
: GPIOF_ODR_ODR13 ( -- x addr ) 13 bit GPIOF_ODR ; \ GPIOF_ODR_ODR13, Port output data y =  0..15
: GPIOF_ODR_ODR12 ( -- x addr ) 12 bit GPIOF_ODR ; \ GPIOF_ODR_ODR12, Port output data y =  0..15
: GPIOF_ODR_ODR11 ( -- x addr ) 11 bit GPIOF_ODR ; \ GPIOF_ODR_ODR11, Port output data y =  0..15
: GPIOF_ODR_ODR10 ( -- x addr ) 10 bit GPIOF_ODR ; \ GPIOF_ODR_ODR10, Port output data y =  0..15
: GPIOF_ODR_ODR9 ( -- x addr ) 9 bit GPIOF_ODR ; \ GPIOF_ODR_ODR9, Port output data y =  0..15
: GPIOF_ODR_ODR8 ( -- x addr ) 8 bit GPIOF_ODR ; \ GPIOF_ODR_ODR8, Port output data y =  0..15
: GPIOF_ODR_ODR7 ( -- x addr ) 7 bit GPIOF_ODR ; \ GPIOF_ODR_ODR7, Port output data y =  0..15
: GPIOF_ODR_ODR6 ( -- x addr ) 6 bit GPIOF_ODR ; \ GPIOF_ODR_ODR6, Port output data y =  0..15
: GPIOF_ODR_ODR5 ( -- x addr ) 5 bit GPIOF_ODR ; \ GPIOF_ODR_ODR5, Port output data y =  0..15
: GPIOF_ODR_ODR4 ( -- x addr ) 4 bit GPIOF_ODR ; \ GPIOF_ODR_ODR4, Port output data y =  0..15
: GPIOF_ODR_ODR3 ( -- x addr ) 3 bit GPIOF_ODR ; \ GPIOF_ODR_ODR3, Port output data y =  0..15
: GPIOF_ODR_ODR2 ( -- x addr ) 2 bit GPIOF_ODR ; \ GPIOF_ODR_ODR2, Port output data y =  0..15
: GPIOF_ODR_ODR1 ( -- x addr ) 1 bit GPIOF_ODR ; \ GPIOF_ODR_ODR1, Port output data y =  0..15
: GPIOF_ODR_ODR0 ( -- x addr ) 0 bit GPIOF_ODR ; \ GPIOF_ODR_ODR0, Port output data y =  0..15
[then]

defined? use-GPIOF defined? GPIOF_BSRR_BR15 not and [if]
\ GPIOF_BSRR (write-only) Reset:0x00000000
: GPIOF_BSRR_BR15 ( -- ) 31 bit GPIOF_BSRR ! ; \ GPIOF_BSRR_BR15, Port x reset bit y y =  0..15
: GPIOF_BSRR_BR14 ( -- ) 30 bit GPIOF_BSRR ! ; \ GPIOF_BSRR_BR14, Port x reset bit y y =  0..15
: GPIOF_BSRR_BR13 ( -- ) 29 bit GPIOF_BSRR ! ; \ GPIOF_BSRR_BR13, Port x reset bit y y =  0..15
: GPIOF_BSRR_BR12 ( -- ) 28 bit GPIOF_BSRR ! ; \ GPIOF_BSRR_BR12, Port x reset bit y y =  0..15
: GPIOF_BSRR_BR11 ( -- ) 27 bit GPIOF_BSRR ! ; \ GPIOF_BSRR_BR11, Port x reset bit y y =  0..15
: GPIOF_BSRR_BR10 ( -- ) 26 bit GPIOF_BSRR ! ; \ GPIOF_BSRR_BR10, Port x reset bit y y =  0..15
: GPIOF_BSRR_BR9 ( -- ) 25 bit GPIOF_BSRR ! ; \ GPIOF_BSRR_BR9, Port x reset bit y y =  0..15
: GPIOF_BSRR_BR8 ( -- ) 24 bit GPIOF_BSRR ! ; \ GPIOF_BSRR_BR8, Port x reset bit y y =  0..15
: GPIOF_BSRR_BR7 ( -- ) 23 bit GPIOF_BSRR ! ; \ GPIOF_BSRR_BR7, Port x reset bit y y =  0..15
: GPIOF_BSRR_BR6 ( -- ) 22 bit GPIOF_BSRR ! ; \ GPIOF_BSRR_BR6, Port x reset bit y y =  0..15
: GPIOF_BSRR_BR5 ( -- ) 21 bit GPIOF_BSRR ! ; \ GPIOF_BSRR_BR5, Port x reset bit y y =  0..15
: GPIOF_BSRR_BR4 ( -- ) 20 bit GPIOF_BSRR ! ; \ GPIOF_BSRR_BR4, Port x reset bit y y =  0..15
: GPIOF_BSRR_BR3 ( -- ) 19 bit GPIOF_BSRR ! ; \ GPIOF_BSRR_BR3, Port x reset bit y y =  0..15
: GPIOF_BSRR_BR2 ( -- ) 18 bit GPIOF_BSRR ! ; \ GPIOF_BSRR_BR2, Port x reset bit y y =  0..15
: GPIOF_BSRR_BR1 ( -- ) 17 bit GPIOF_BSRR ! ; \ GPIOF_BSRR_BR1, Port x reset bit y y =  0..15
: GPIOF_BSRR_BR0 ( -- ) 16 bit GPIOF_BSRR ! ; \ GPIOF_BSRR_BR0, Port x set bit y y=  0..15
: GPIOF_BSRR_BS15 ( -- ) 15 bit GPIOF_BSRR ! ; \ GPIOF_BSRR_BS15, Port x set bit y y=  0..15
: GPIOF_BSRR_BS14 ( -- ) 14 bit GPIOF_BSRR ! ; \ GPIOF_BSRR_BS14, Port x set bit y y=  0..15
: GPIOF_BSRR_BS13 ( -- ) 13 bit GPIOF_BSRR ! ; \ GPIOF_BSRR_BS13, Port x set bit y y=  0..15
: GPIOF_BSRR_BS12 ( -- ) 12 bit GPIOF_BSRR ! ; \ GPIOF_BSRR_BS12, Port x set bit y y=  0..15
: GPIOF_BSRR_BS11 ( -- ) 11 bit GPIOF_BSRR ! ; \ GPIOF_BSRR_BS11, Port x set bit y y=  0..15
: GPIOF_BSRR_BS10 ( -- ) 10 bit GPIOF_BSRR ! ; \ GPIOF_BSRR_BS10, Port x set bit y y=  0..15
: GPIOF_BSRR_BS9 ( -- ) 9 bit GPIOF_BSRR ! ; \ GPIOF_BSRR_BS9, Port x set bit y y=  0..15
: GPIOF_BSRR_BS8 ( -- ) 8 bit GPIOF_BSRR ! ; \ GPIOF_BSRR_BS8, Port x set bit y y=  0..15
: GPIOF_BSRR_BS7 ( -- ) 7 bit GPIOF_BSRR ! ; \ GPIOF_BSRR_BS7, Port x set bit y y=  0..15
: GPIOF_BSRR_BS6 ( -- ) 6 bit GPIOF_BSRR ! ; \ GPIOF_BSRR_BS6, Port x set bit y y=  0..15
: GPIOF_BSRR_BS5 ( -- ) 5 bit GPIOF_BSRR ! ; \ GPIOF_BSRR_BS5, Port x set bit y y=  0..15
: GPIOF_BSRR_BS4 ( -- ) 4 bit GPIOF_BSRR ! ; \ GPIOF_BSRR_BS4, Port x set bit y y=  0..15
: GPIOF_BSRR_BS3 ( -- ) 3 bit GPIOF_BSRR ! ; \ GPIOF_BSRR_BS3, Port x set bit y y=  0..15
: GPIOF_BSRR_BS2 ( -- ) 2 bit GPIOF_BSRR ! ; \ GPIOF_BSRR_BS2, Port x set bit y y=  0..15
: GPIOF_BSRR_BS1 ( -- ) 1 bit GPIOF_BSRR ! ; \ GPIOF_BSRR_BS1, Port x set bit y y=  0..15
: GPIOF_BSRR_BS0 ( -- ) 0 bit GPIOF_BSRR ! ; \ GPIOF_BSRR_BS0, Port x set bit y y=  0..15
[then]

execute-defined? use-GPIOF defined? GPIOF_LCKR_LCKK not and [if]
\ GPIOF_LCKR (read-write) Reset:0x00000000
: GPIOF_LCKR_LCKK ( -- x addr ) 16 bit GPIOF_LCKR ; \ GPIOF_LCKR_LCKK, Port x lock bit y y=  0..15
: GPIOF_LCKR_LCK15 ( -- x addr ) 15 bit GPIOF_LCKR ; \ GPIOF_LCKR_LCK15, Port x lock bit y y=  0..15
: GPIOF_LCKR_LCK14 ( -- x addr ) 14 bit GPIOF_LCKR ; \ GPIOF_LCKR_LCK14, Port x lock bit y y=  0..15
: GPIOF_LCKR_LCK13 ( -- x addr ) 13 bit GPIOF_LCKR ; \ GPIOF_LCKR_LCK13, Port x lock bit y y=  0..15
: GPIOF_LCKR_LCK12 ( -- x addr ) 12 bit GPIOF_LCKR ; \ GPIOF_LCKR_LCK12, Port x lock bit y y=  0..15
: GPIOF_LCKR_LCK11 ( -- x addr ) 11 bit GPIOF_LCKR ; \ GPIOF_LCKR_LCK11, Port x lock bit y y=  0..15
: GPIOF_LCKR_LCK10 ( -- x addr ) 10 bit GPIOF_LCKR ; \ GPIOF_LCKR_LCK10, Port x lock bit y y=  0..15
: GPIOF_LCKR_LCK9 ( -- x addr ) 9 bit GPIOF_LCKR ; \ GPIOF_LCKR_LCK9, Port x lock bit y y=  0..15
: GPIOF_LCKR_LCK8 ( -- x addr ) 8 bit GPIOF_LCKR ; \ GPIOF_LCKR_LCK8, Port x lock bit y y=  0..15
: GPIOF_LCKR_LCK7 ( -- x addr ) 7 bit GPIOF_LCKR ; \ GPIOF_LCKR_LCK7, Port x lock bit y y=  0..15
: GPIOF_LCKR_LCK6 ( -- x addr ) 6 bit GPIOF_LCKR ; \ GPIOF_LCKR_LCK6, Port x lock bit y y=  0..15
: GPIOF_LCKR_LCK5 ( -- x addr ) 5 bit GPIOF_LCKR ; \ GPIOF_LCKR_LCK5, Port x lock bit y y=  0..15
: GPIOF_LCKR_LCK4 ( -- x addr ) 4 bit GPIOF_LCKR ; \ GPIOF_LCKR_LCK4, Port x lock bit y y=  0..15
: GPIOF_LCKR_LCK3 ( -- x addr ) 3 bit GPIOF_LCKR ; \ GPIOF_LCKR_LCK3, Port x lock bit y y=  0..15
: GPIOF_LCKR_LCK2 ( -- x addr ) 2 bit GPIOF_LCKR ; \ GPIOF_LCKR_LCK2, Port x lock bit y y=  0..15
: GPIOF_LCKR_LCK1 ( -- x addr ) 1 bit GPIOF_LCKR ; \ GPIOF_LCKR_LCK1, Port x lock bit y y=  0..15
: GPIOF_LCKR_LCK0 ( -- x addr ) 0 bit GPIOF_LCKR ; \ GPIOF_LCKR_LCK0, Port x lock bit y y=  0..15
[then]

defined? use-GPIOF defined? GPIOF_AFRL_AFRL7 not and [if]
\ GPIOF_AFRL (read-write) Reset:0x00000000
: GPIOF_AFRL_AFRL7 ( %bbbb -- x addr ) 28 lshift GPIOF_AFRL ; \ GPIOF_AFRL_AFRL7, Alternate function selection for port x  bit y y = 0..7
: GPIOF_AFRL_AFRL6 ( %bbbb -- x addr ) 24 lshift GPIOF_AFRL ; \ GPIOF_AFRL_AFRL6, Alternate function selection for port x  bit y y = 0..7
: GPIOF_AFRL_AFRL5 ( %bbbb -- x addr ) 20 lshift GPIOF_AFRL ; \ GPIOF_AFRL_AFRL5, Alternate function selection for port x  bit y y = 0..7
: GPIOF_AFRL_AFRL4 ( %bbbb -- x addr ) 16 lshift GPIOF_AFRL ; \ GPIOF_AFRL_AFRL4, Alternate function selection for port x  bit y y = 0..7
: GPIOF_AFRL_AFRL3 ( %bbbb -- x addr ) 12 lshift GPIOF_AFRL ; \ GPIOF_AFRL_AFRL3, Alternate function selection for port x  bit y y = 0..7
: GPIOF_AFRL_AFRL2 ( %bbbb -- x addr ) 8 lshift GPIOF_AFRL ; \ GPIOF_AFRL_AFRL2, Alternate function selection for port x  bit y y = 0..7
: GPIOF_AFRL_AFRL1 ( %bbbb -- x addr ) 4 lshift GPIOF_AFRL ; \ GPIOF_AFRL_AFRL1, Alternate function selection for port x  bit y y = 0..7
: GPIOF_AFRL_AFRL0 ( %bbbb -- x addr ) GPIOF_AFRL ; \ GPIOF_AFRL_AFRL0, Alternate function selection for port x  bit y y = 0..7
[then]

execute-defined? use-GPIOF defined? GPIOF_AFRH_AFRH15 not and [if]
\ GPIOF_AFRH (read-write) Reset:0x00000000
: GPIOF_AFRH_AFRH15 ( %bbbb -- x addr ) 28 lshift GPIOF_AFRH ; \ GPIOF_AFRH_AFRH15, Alternate function selection for port x  bit y y = 8..15
: GPIOF_AFRH_AFRH14 ( %bbbb -- x addr ) 24 lshift GPIOF_AFRH ; \ GPIOF_AFRH_AFRH14, Alternate function selection for port x  bit y y = 8..15
: GPIOF_AFRH_AFRH13 ( %bbbb -- x addr ) 20 lshift GPIOF_AFRH ; \ GPIOF_AFRH_AFRH13, Alternate function selection for port x  bit y y = 8..15
: GPIOF_AFRH_AFRH12 ( %bbbb -- x addr ) 16 lshift GPIOF_AFRH ; \ GPIOF_AFRH_AFRH12, Alternate function selection for port x  bit y y = 8..15
: GPIOF_AFRH_AFRH11 ( %bbbb -- x addr ) 12 lshift GPIOF_AFRH ; \ GPIOF_AFRH_AFRH11, Alternate function selection for port x  bit y y = 8..15
: GPIOF_AFRH_AFRH10 ( %bbbb -- x addr ) 8 lshift GPIOF_AFRH ; \ GPIOF_AFRH_AFRH10, Alternate function selection for port x  bit y y = 8..15
: GPIOF_AFRH_AFRH9 ( %bbbb -- x addr ) 4 lshift GPIOF_AFRH ; \ GPIOF_AFRH_AFRH9, Alternate function selection for port x  bit y y = 8..15
: GPIOF_AFRH_AFRH8 ( %bbbb -- x addr ) GPIOF_AFRH ; \ GPIOF_AFRH_AFRH8, Alternate function selection for port x  bit y y = 8..15
[then]

defined? use-GPIOG defined? GPIOG_MODER_MODER15 not and [if]
\ GPIOG_MODER (read-write) Reset:0x00000000
: GPIOG_MODER_MODER15 ( %bb -- x addr ) 30 lshift GPIOG_MODER ; \ GPIOG_MODER_MODER15, Port x configuration bits y =  0..15
: GPIOG_MODER_MODER14 ( %bb -- x addr ) 28 lshift GPIOG_MODER ; \ GPIOG_MODER_MODER14, Port x configuration bits y =  0..15
: GPIOG_MODER_MODER13 ( %bb -- x addr ) 26 lshift GPIOG_MODER ; \ GPIOG_MODER_MODER13, Port x configuration bits y =  0..15
: GPIOG_MODER_MODER12 ( %bb -- x addr ) 24 lshift GPIOG_MODER ; \ GPIOG_MODER_MODER12, Port x configuration bits y =  0..15
: GPIOG_MODER_MODER11 ( %bb -- x addr ) 22 lshift GPIOG_MODER ; \ GPIOG_MODER_MODER11, Port x configuration bits y =  0..15
: GPIOG_MODER_MODER10 ( %bb -- x addr ) 20 lshift GPIOG_MODER ; \ GPIOG_MODER_MODER10, Port x configuration bits y =  0..15
: GPIOG_MODER_MODER9 ( %bb -- x addr ) 18 lshift GPIOG_MODER ; \ GPIOG_MODER_MODER9, Port x configuration bits y =  0..15
: GPIOG_MODER_MODER8 ( %bb -- x addr ) 16 lshift GPIOG_MODER ; \ GPIOG_MODER_MODER8, Port x configuration bits y =  0..15
: GPIOG_MODER_MODER7 ( %bb -- x addr ) 14 lshift GPIOG_MODER ; \ GPIOG_MODER_MODER7, Port x configuration bits y =  0..15
: GPIOG_MODER_MODER6 ( %bb -- x addr ) 12 lshift GPIOG_MODER ; \ GPIOG_MODER_MODER6, Port x configuration bits y =  0..15
: GPIOG_MODER_MODER5 ( %bb -- x addr ) 10 lshift GPIOG_MODER ; \ GPIOG_MODER_MODER5, Port x configuration bits y =  0..15
: GPIOG_MODER_MODER4 ( %bb -- x addr ) 8 lshift GPIOG_MODER ; \ GPIOG_MODER_MODER4, Port x configuration bits y =  0..15
: GPIOG_MODER_MODER3 ( %bb -- x addr ) 6 lshift GPIOG_MODER ; \ GPIOG_MODER_MODER3, Port x configuration bits y =  0..15
: GPIOG_MODER_MODER2 ( %bb -- x addr ) 4 lshift GPIOG_MODER ; \ GPIOG_MODER_MODER2, Port x configuration bits y =  0..15
: GPIOG_MODER_MODER1 ( %bb -- x addr ) 2 lshift GPIOG_MODER ; \ GPIOG_MODER_MODER1, Port x configuration bits y =  0..15
: GPIOG_MODER_MODER0 ( %bb -- x addr ) GPIOG_MODER ; \ GPIOG_MODER_MODER0, Port x configuration bits y =  0..15
[then]

execute-defined? use-GPIOG defined? GPIOG_OTYPER_OT15 not and [if]
\ GPIOG_OTYPER (read-write) Reset:0x00000000
: GPIOG_OTYPER_OT15 ( -- x addr ) 15 bit GPIOG_OTYPER ; \ GPIOG_OTYPER_OT15, Port x configuration bits y =  0..15
: GPIOG_OTYPER_OT14 ( -- x addr ) 14 bit GPIOG_OTYPER ; \ GPIOG_OTYPER_OT14, Port x configuration bits y =  0..15
: GPIOG_OTYPER_OT13 ( -- x addr ) 13 bit GPIOG_OTYPER ; \ GPIOG_OTYPER_OT13, Port x configuration bits y =  0..15
: GPIOG_OTYPER_OT12 ( -- x addr ) 12 bit GPIOG_OTYPER ; \ GPIOG_OTYPER_OT12, Port x configuration bits y =  0..15
: GPIOG_OTYPER_OT11 ( -- x addr ) 11 bit GPIOG_OTYPER ; \ GPIOG_OTYPER_OT11, Port x configuration bits y =  0..15
: GPIOG_OTYPER_OT10 ( -- x addr ) 10 bit GPIOG_OTYPER ; \ GPIOG_OTYPER_OT10, Port x configuration bits y =  0..15
: GPIOG_OTYPER_OT9 ( -- x addr ) 9 bit GPIOG_OTYPER ; \ GPIOG_OTYPER_OT9, Port x configuration bits y =  0..15
: GPIOG_OTYPER_OT8 ( -- x addr ) 8 bit GPIOG_OTYPER ; \ GPIOG_OTYPER_OT8, Port x configuration bits y =  0..15
: GPIOG_OTYPER_OT7 ( -- x addr ) 7 bit GPIOG_OTYPER ; \ GPIOG_OTYPER_OT7, Port x configuration bits y =  0..15
: GPIOG_OTYPER_OT6 ( -- x addr ) 6 bit GPIOG_OTYPER ; \ GPIOG_OTYPER_OT6, Port x configuration bits y =  0..15
: GPIOG_OTYPER_OT5 ( -- x addr ) 5 bit GPIOG_OTYPER ; \ GPIOG_OTYPER_OT5, Port x configuration bits y =  0..15
: GPIOG_OTYPER_OT4 ( -- x addr ) 4 bit GPIOG_OTYPER ; \ GPIOG_OTYPER_OT4, Port x configuration bits y =  0..15
: GPIOG_OTYPER_OT3 ( -- x addr ) 3 bit GPIOG_OTYPER ; \ GPIOG_OTYPER_OT3, Port x configuration bits y =  0..15
: GPIOG_OTYPER_OT2 ( -- x addr ) 2 bit GPIOG_OTYPER ; \ GPIOG_OTYPER_OT2, Port x configuration bits y =  0..15
: GPIOG_OTYPER_OT1 ( -- x addr ) 1 bit GPIOG_OTYPER ; \ GPIOG_OTYPER_OT1, Port x configuration bits y =  0..15
: GPIOG_OTYPER_OT0 ( -- x addr ) 0 bit GPIOG_OTYPER ; \ GPIOG_OTYPER_OT0, Port x configuration bits y =  0..15
[then]

defined? use-GPIOG defined? GPIOG_OSPEEDR_OSPEEDR15 not and [if]
\ GPIOG_OSPEEDR (read-write) Reset:0x00000000
: GPIOG_OSPEEDR_OSPEEDR15 ( %bb -- x addr ) 30 lshift GPIOG_OSPEEDR ; \ GPIOG_OSPEEDR_OSPEEDR15, Port x configuration bits y =  0..15
: GPIOG_OSPEEDR_OSPEEDR14 ( %bb -- x addr ) 28 lshift GPIOG_OSPEEDR ; \ GPIOG_OSPEEDR_OSPEEDR14, Port x configuration bits y =  0..15
: GPIOG_OSPEEDR_OSPEEDR13 ( %bb -- x addr ) 26 lshift GPIOG_OSPEEDR ; \ GPIOG_OSPEEDR_OSPEEDR13, Port x configuration bits y =  0..15
: GPIOG_OSPEEDR_OSPEEDR12 ( %bb -- x addr ) 24 lshift GPIOG_OSPEEDR ; \ GPIOG_OSPEEDR_OSPEEDR12, Port x configuration bits y =  0..15
: GPIOG_OSPEEDR_OSPEEDR11 ( %bb -- x addr ) 22 lshift GPIOG_OSPEEDR ; \ GPIOG_OSPEEDR_OSPEEDR11, Port x configuration bits y =  0..15
: GPIOG_OSPEEDR_OSPEEDR10 ( %bb -- x addr ) 20 lshift GPIOG_OSPEEDR ; \ GPIOG_OSPEEDR_OSPEEDR10, Port x configuration bits y =  0..15
: GPIOG_OSPEEDR_OSPEEDR9 ( %bb -- x addr ) 18 lshift GPIOG_OSPEEDR ; \ GPIOG_OSPEEDR_OSPEEDR9, Port x configuration bits y =  0..15
: GPIOG_OSPEEDR_OSPEEDR8 ( %bb -- x addr ) 16 lshift GPIOG_OSPEEDR ; \ GPIOG_OSPEEDR_OSPEEDR8, Port x configuration bits y =  0..15
: GPIOG_OSPEEDR_OSPEEDR7 ( %bb -- x addr ) 14 lshift GPIOG_OSPEEDR ; \ GPIOG_OSPEEDR_OSPEEDR7, Port x configuration bits y =  0..15
: GPIOG_OSPEEDR_OSPEEDR6 ( %bb -- x addr ) 12 lshift GPIOG_OSPEEDR ; \ GPIOG_OSPEEDR_OSPEEDR6, Port x configuration bits y =  0..15
: GPIOG_OSPEEDR_OSPEEDR5 ( %bb -- x addr ) 10 lshift GPIOG_OSPEEDR ; \ GPIOG_OSPEEDR_OSPEEDR5, Port x configuration bits y =  0..15
: GPIOG_OSPEEDR_OSPEEDR4 ( %bb -- x addr ) 8 lshift GPIOG_OSPEEDR ; \ GPIOG_OSPEEDR_OSPEEDR4, Port x configuration bits y =  0..15
: GPIOG_OSPEEDR_OSPEEDR3 ( %bb -- x addr ) 6 lshift GPIOG_OSPEEDR ; \ GPIOG_OSPEEDR_OSPEEDR3, Port x configuration bits y =  0..15
: GPIOG_OSPEEDR_OSPEEDR2 ( %bb -- x addr ) 4 lshift GPIOG_OSPEEDR ; \ GPIOG_OSPEEDR_OSPEEDR2, Port x configuration bits y =  0..15
: GPIOG_OSPEEDR_OSPEEDR1 ( %bb -- x addr ) 2 lshift GPIOG_OSPEEDR ; \ GPIOG_OSPEEDR_OSPEEDR1, Port x configuration bits y =  0..15
: GPIOG_OSPEEDR_OSPEEDR0 ( %bb -- x addr ) GPIOG_OSPEEDR ; \ GPIOG_OSPEEDR_OSPEEDR0, Port x configuration bits y =  0..15
[then]

execute-defined? use-GPIOG defined? GPIOG_PUPDR_PUPDR15 not and [if]
\ GPIOG_PUPDR (read-write) Reset:0x00000000
: GPIOG_PUPDR_PUPDR15 ( %bb -- x addr ) 30 lshift GPIOG_PUPDR ; \ GPIOG_PUPDR_PUPDR15, Port x configuration bits y =  0..15
: GPIOG_PUPDR_PUPDR14 ( %bb -- x addr ) 28 lshift GPIOG_PUPDR ; \ GPIOG_PUPDR_PUPDR14, Port x configuration bits y =  0..15
: GPIOG_PUPDR_PUPDR13 ( %bb -- x addr ) 26 lshift GPIOG_PUPDR ; \ GPIOG_PUPDR_PUPDR13, Port x configuration bits y =  0..15
: GPIOG_PUPDR_PUPDR12 ( %bb -- x addr ) 24 lshift GPIOG_PUPDR ; \ GPIOG_PUPDR_PUPDR12, Port x configuration bits y =  0..15
: GPIOG_PUPDR_PUPDR11 ( %bb -- x addr ) 22 lshift GPIOG_PUPDR ; \ GPIOG_PUPDR_PUPDR11, Port x configuration bits y =  0..15
: GPIOG_PUPDR_PUPDR10 ( %bb -- x addr ) 20 lshift GPIOG_PUPDR ; \ GPIOG_PUPDR_PUPDR10, Port x configuration bits y =  0..15
: GPIOG_PUPDR_PUPDR9 ( %bb -- x addr ) 18 lshift GPIOG_PUPDR ; \ GPIOG_PUPDR_PUPDR9, Port x configuration bits y =  0..15
: GPIOG_PUPDR_PUPDR8 ( %bb -- x addr ) 16 lshift GPIOG_PUPDR ; \ GPIOG_PUPDR_PUPDR8, Port x configuration bits y =  0..15
: GPIOG_PUPDR_PUPDR7 ( %bb -- x addr ) 14 lshift GPIOG_PUPDR ; \ GPIOG_PUPDR_PUPDR7, Port x configuration bits y =  0..15
: GPIOG_PUPDR_PUPDR6 ( %bb -- x addr ) 12 lshift GPIOG_PUPDR ; \ GPIOG_PUPDR_PUPDR6, Port x configuration bits y =  0..15
: GPIOG_PUPDR_PUPDR5 ( %bb -- x addr ) 10 lshift GPIOG_PUPDR ; \ GPIOG_PUPDR_PUPDR5, Port x configuration bits y =  0..15
: GPIOG_PUPDR_PUPDR4 ( %bb -- x addr ) 8 lshift GPIOG_PUPDR ; \ GPIOG_PUPDR_PUPDR4, Port x configuration bits y =  0..15
: GPIOG_PUPDR_PUPDR3 ( %bb -- x addr ) 6 lshift GPIOG_PUPDR ; \ GPIOG_PUPDR_PUPDR3, Port x configuration bits y =  0..15
: GPIOG_PUPDR_PUPDR2 ( %bb -- x addr ) 4 lshift GPIOG_PUPDR ; \ GPIOG_PUPDR_PUPDR2, Port x configuration bits y =  0..15
: GPIOG_PUPDR_PUPDR1 ( %bb -- x addr ) 2 lshift GPIOG_PUPDR ; \ GPIOG_PUPDR_PUPDR1, Port x configuration bits y =  0..15
: GPIOG_PUPDR_PUPDR0 ( %bb -- x addr ) GPIOG_PUPDR ; \ GPIOG_PUPDR_PUPDR0, Port x configuration bits y =  0..15
[then]

defined? use-GPIOG defined? GPIOG_IDR_IDR15? not and [if]
\ GPIOG_IDR (read-only) Reset:0x00000000
: GPIOG_IDR_IDR15? ( --  1|0 ) 15 bit GPIOG_IDR bit@ ; \ GPIOG_IDR_IDR15, Port input data y =  0..15
: GPIOG_IDR_IDR14? ( --  1|0 ) 14 bit GPIOG_IDR bit@ ; \ GPIOG_IDR_IDR14, Port input data y =  0..15
: GPIOG_IDR_IDR13? ( --  1|0 ) 13 bit GPIOG_IDR bit@ ; \ GPIOG_IDR_IDR13, Port input data y =  0..15
: GPIOG_IDR_IDR12? ( --  1|0 ) 12 bit GPIOG_IDR bit@ ; \ GPIOG_IDR_IDR12, Port input data y =  0..15
: GPIOG_IDR_IDR11? ( --  1|0 ) 11 bit GPIOG_IDR bit@ ; \ GPIOG_IDR_IDR11, Port input data y =  0..15
: GPIOG_IDR_IDR10? ( --  1|0 ) 10 bit GPIOG_IDR bit@ ; \ GPIOG_IDR_IDR10, Port input data y =  0..15
: GPIOG_IDR_IDR9? ( --  1|0 ) 9 bit GPIOG_IDR bit@ ; \ GPIOG_IDR_IDR9, Port input data y =  0..15
: GPIOG_IDR_IDR8? ( --  1|0 ) 8 bit GPIOG_IDR bit@ ; \ GPIOG_IDR_IDR8, Port input data y =  0..15
: GPIOG_IDR_IDR7? ( --  1|0 ) 7 bit GPIOG_IDR bit@ ; \ GPIOG_IDR_IDR7, Port input data y =  0..15
: GPIOG_IDR_IDR6? ( --  1|0 ) 6 bit GPIOG_IDR bit@ ; \ GPIOG_IDR_IDR6, Port input data y =  0..15
: GPIOG_IDR_IDR5? ( --  1|0 ) 5 bit GPIOG_IDR bit@ ; \ GPIOG_IDR_IDR5, Port input data y =  0..15
: GPIOG_IDR_IDR4? ( --  1|0 ) 4 bit GPIOG_IDR bit@ ; \ GPIOG_IDR_IDR4, Port input data y =  0..15
: GPIOG_IDR_IDR3? ( --  1|0 ) 3 bit GPIOG_IDR bit@ ; \ GPIOG_IDR_IDR3, Port input data y =  0..15
: GPIOG_IDR_IDR2? ( --  1|0 ) 2 bit GPIOG_IDR bit@ ; \ GPIOG_IDR_IDR2, Port input data y =  0..15
: GPIOG_IDR_IDR1? ( --  1|0 ) 1 bit GPIOG_IDR bit@ ; \ GPIOG_IDR_IDR1, Port input data y =  0..15
: GPIOG_IDR_IDR0? ( --  1|0 ) 0 bit GPIOG_IDR bit@ ; \ GPIOG_IDR_IDR0, Port input data y =  0..15
[then]

execute-defined? use-GPIOG defined? GPIOG_ODR_ODR15 not and [if]
\ GPIOG_ODR (read-write) Reset:0x00000000
: GPIOG_ODR_ODR15 ( -- x addr ) 15 bit GPIOG_ODR ; \ GPIOG_ODR_ODR15, Port output data y =  0..15
: GPIOG_ODR_ODR14 ( -- x addr ) 14 bit GPIOG_ODR ; \ GPIOG_ODR_ODR14, Port output data y =  0..15
: GPIOG_ODR_ODR13 ( -- x addr ) 13 bit GPIOG_ODR ; \ GPIOG_ODR_ODR13, Port output data y =  0..15
: GPIOG_ODR_ODR12 ( -- x addr ) 12 bit GPIOG_ODR ; \ GPIOG_ODR_ODR12, Port output data y =  0..15
: GPIOG_ODR_ODR11 ( -- x addr ) 11 bit GPIOG_ODR ; \ GPIOG_ODR_ODR11, Port output data y =  0..15
: GPIOG_ODR_ODR10 ( -- x addr ) 10 bit GPIOG_ODR ; \ GPIOG_ODR_ODR10, Port output data y =  0..15
: GPIOG_ODR_ODR9 ( -- x addr ) 9 bit GPIOG_ODR ; \ GPIOG_ODR_ODR9, Port output data y =  0..15
: GPIOG_ODR_ODR8 ( -- x addr ) 8 bit GPIOG_ODR ; \ GPIOG_ODR_ODR8, Port output data y =  0..15
: GPIOG_ODR_ODR7 ( -- x addr ) 7 bit GPIOG_ODR ; \ GPIOG_ODR_ODR7, Port output data y =  0..15
: GPIOG_ODR_ODR6 ( -- x addr ) 6 bit GPIOG_ODR ; \ GPIOG_ODR_ODR6, Port output data y =  0..15
: GPIOG_ODR_ODR5 ( -- x addr ) 5 bit GPIOG_ODR ; \ GPIOG_ODR_ODR5, Port output data y =  0..15
: GPIOG_ODR_ODR4 ( -- x addr ) 4 bit GPIOG_ODR ; \ GPIOG_ODR_ODR4, Port output data y =  0..15
: GPIOG_ODR_ODR3 ( -- x addr ) 3 bit GPIOG_ODR ; \ GPIOG_ODR_ODR3, Port output data y =  0..15
: GPIOG_ODR_ODR2 ( -- x addr ) 2 bit GPIOG_ODR ; \ GPIOG_ODR_ODR2, Port output data y =  0..15
: GPIOG_ODR_ODR1 ( -- x addr ) 1 bit GPIOG_ODR ; \ GPIOG_ODR_ODR1, Port output data y =  0..15
: GPIOG_ODR_ODR0 ( -- x addr ) 0 bit GPIOG_ODR ; \ GPIOG_ODR_ODR0, Port output data y =  0..15
[then]

defined? use-GPIOG defined? GPIOG_BSRR_BR15 not and [if]
\ GPIOG_BSRR (write-only) Reset:0x00000000
: GPIOG_BSRR_BR15 ( -- ) 31 bit GPIOG_BSRR ! ; \ GPIOG_BSRR_BR15, Port x reset bit y y =  0..15
: GPIOG_BSRR_BR14 ( -- ) 30 bit GPIOG_BSRR ! ; \ GPIOG_BSRR_BR14, Port x reset bit y y =  0..15
: GPIOG_BSRR_BR13 ( -- ) 29 bit GPIOG_BSRR ! ; \ GPIOG_BSRR_BR13, Port x reset bit y y =  0..15
: GPIOG_BSRR_BR12 ( -- ) 28 bit GPIOG_BSRR ! ; \ GPIOG_BSRR_BR12, Port x reset bit y y =  0..15
: GPIOG_BSRR_BR11 ( -- ) 27 bit GPIOG_BSRR ! ; \ GPIOG_BSRR_BR11, Port x reset bit y y =  0..15
: GPIOG_BSRR_BR10 ( -- ) 26 bit GPIOG_BSRR ! ; \ GPIOG_BSRR_BR10, Port x reset bit y y =  0..15
: GPIOG_BSRR_BR9 ( -- ) 25 bit GPIOG_BSRR ! ; \ GPIOG_BSRR_BR9, Port x reset bit y y =  0..15
: GPIOG_BSRR_BR8 ( -- ) 24 bit GPIOG_BSRR ! ; \ GPIOG_BSRR_BR8, Port x reset bit y y =  0..15
: GPIOG_BSRR_BR7 ( -- ) 23 bit GPIOG_BSRR ! ; \ GPIOG_BSRR_BR7, Port x reset bit y y =  0..15
: GPIOG_BSRR_BR6 ( -- ) 22 bit GPIOG_BSRR ! ; \ GPIOG_BSRR_BR6, Port x reset bit y y =  0..15
: GPIOG_BSRR_BR5 ( -- ) 21 bit GPIOG_BSRR ! ; \ GPIOG_BSRR_BR5, Port x reset bit y y =  0..15
: GPIOG_BSRR_BR4 ( -- ) 20 bit GPIOG_BSRR ! ; \ GPIOG_BSRR_BR4, Port x reset bit y y =  0..15
: GPIOG_BSRR_BR3 ( -- ) 19 bit GPIOG_BSRR ! ; \ GPIOG_BSRR_BR3, Port x reset bit y y =  0..15
: GPIOG_BSRR_BR2 ( -- ) 18 bit GPIOG_BSRR ! ; \ GPIOG_BSRR_BR2, Port x reset bit y y =  0..15
: GPIOG_BSRR_BR1 ( -- ) 17 bit GPIOG_BSRR ! ; \ GPIOG_BSRR_BR1, Port x reset bit y y =  0..15
: GPIOG_BSRR_BR0 ( -- ) 16 bit GPIOG_BSRR ! ; \ GPIOG_BSRR_BR0, Port x set bit y y=  0..15
: GPIOG_BSRR_BS15 ( -- ) 15 bit GPIOG_BSRR ! ; \ GPIOG_BSRR_BS15, Port x set bit y y=  0..15
: GPIOG_BSRR_BS14 ( -- ) 14 bit GPIOG_BSRR ! ; \ GPIOG_BSRR_BS14, Port x set bit y y=  0..15
: GPIOG_BSRR_BS13 ( -- ) 13 bit GPIOG_BSRR ! ; \ GPIOG_BSRR_BS13, Port x set bit y y=  0..15
: GPIOG_BSRR_BS12 ( -- ) 12 bit GPIOG_BSRR ! ; \ GPIOG_BSRR_BS12, Port x set bit y y=  0..15
: GPIOG_BSRR_BS11 ( -- ) 11 bit GPIOG_BSRR ! ; \ GPIOG_BSRR_BS11, Port x set bit y y=  0..15
: GPIOG_BSRR_BS10 ( -- ) 10 bit GPIOG_BSRR ! ; \ GPIOG_BSRR_BS10, Port x set bit y y=  0..15
: GPIOG_BSRR_BS9 ( -- ) 9 bit GPIOG_BSRR ! ; \ GPIOG_BSRR_BS9, Port x set bit y y=  0..15
: GPIOG_BSRR_BS8 ( -- ) 8 bit GPIOG_BSRR ! ; \ GPIOG_BSRR_BS8, Port x set bit y y=  0..15
: GPIOG_BSRR_BS7 ( -- ) 7 bit GPIOG_BSRR ! ; \ GPIOG_BSRR_BS7, Port x set bit y y=  0..15
: GPIOG_BSRR_BS6 ( -- ) 6 bit GPIOG_BSRR ! ; \ GPIOG_BSRR_BS6, Port x set bit y y=  0..15
: GPIOG_BSRR_BS5 ( -- ) 5 bit GPIOG_BSRR ! ; \ GPIOG_BSRR_BS5, Port x set bit y y=  0..15
: GPIOG_BSRR_BS4 ( -- ) 4 bit GPIOG_BSRR ! ; \ GPIOG_BSRR_BS4, Port x set bit y y=  0..15
: GPIOG_BSRR_BS3 ( -- ) 3 bit GPIOG_BSRR ! ; \ GPIOG_BSRR_BS3, Port x set bit y y=  0..15
: GPIOG_BSRR_BS2 ( -- ) 2 bit GPIOG_BSRR ! ; \ GPIOG_BSRR_BS2, Port x set bit y y=  0..15
: GPIOG_BSRR_BS1 ( -- ) 1 bit GPIOG_BSRR ! ; \ GPIOG_BSRR_BS1, Port x set bit y y=  0..15
: GPIOG_BSRR_BS0 ( -- ) 0 bit GPIOG_BSRR ! ; \ GPIOG_BSRR_BS0, Port x set bit y y=  0..15
[then]

execute-defined? use-GPIOG defined? GPIOG_LCKR_LCKK not and [if]
\ GPIOG_LCKR (read-write) Reset:0x00000000
: GPIOG_LCKR_LCKK ( -- x addr ) 16 bit GPIOG_LCKR ; \ GPIOG_LCKR_LCKK, Port x lock bit y y=  0..15
: GPIOG_LCKR_LCK15 ( -- x addr ) 15 bit GPIOG_LCKR ; \ GPIOG_LCKR_LCK15, Port x lock bit y y=  0..15
: GPIOG_LCKR_LCK14 ( -- x addr ) 14 bit GPIOG_LCKR ; \ GPIOG_LCKR_LCK14, Port x lock bit y y=  0..15
: GPIOG_LCKR_LCK13 ( -- x addr ) 13 bit GPIOG_LCKR ; \ GPIOG_LCKR_LCK13, Port x lock bit y y=  0..15
: GPIOG_LCKR_LCK12 ( -- x addr ) 12 bit GPIOG_LCKR ; \ GPIOG_LCKR_LCK12, Port x lock bit y y=  0..15
: GPIOG_LCKR_LCK11 ( -- x addr ) 11 bit GPIOG_LCKR ; \ GPIOG_LCKR_LCK11, Port x lock bit y y=  0..15
: GPIOG_LCKR_LCK10 ( -- x addr ) 10 bit GPIOG_LCKR ; \ GPIOG_LCKR_LCK10, Port x lock bit y y=  0..15
: GPIOG_LCKR_LCK9 ( -- x addr ) 9 bit GPIOG_LCKR ; \ GPIOG_LCKR_LCK9, Port x lock bit y y=  0..15
: GPIOG_LCKR_LCK8 ( -- x addr ) 8 bit GPIOG_LCKR ; \ GPIOG_LCKR_LCK8, Port x lock bit y y=  0..15
: GPIOG_LCKR_LCK7 ( -- x addr ) 7 bit GPIOG_LCKR ; \ GPIOG_LCKR_LCK7, Port x lock bit y y=  0..15
: GPIOG_LCKR_LCK6 ( -- x addr ) 6 bit GPIOG_LCKR ; \ GPIOG_LCKR_LCK6, Port x lock bit y y=  0..15
: GPIOG_LCKR_LCK5 ( -- x addr ) 5 bit GPIOG_LCKR ; \ GPIOG_LCKR_LCK5, Port x lock bit y y=  0..15
: GPIOG_LCKR_LCK4 ( -- x addr ) 4 bit GPIOG_LCKR ; \ GPIOG_LCKR_LCK4, Port x lock bit y y=  0..15
: GPIOG_LCKR_LCK3 ( -- x addr ) 3 bit GPIOG_LCKR ; \ GPIOG_LCKR_LCK3, Port x lock bit y y=  0..15
: GPIOG_LCKR_LCK2 ( -- x addr ) 2 bit GPIOG_LCKR ; \ GPIOG_LCKR_LCK2, Port x lock bit y y=  0..15
: GPIOG_LCKR_LCK1 ( -- x addr ) 1 bit GPIOG_LCKR ; \ GPIOG_LCKR_LCK1, Port x lock bit y y=  0..15
: GPIOG_LCKR_LCK0 ( -- x addr ) 0 bit GPIOG_LCKR ; \ GPIOG_LCKR_LCK0, Port x lock bit y y=  0..15
[then]

defined? use-GPIOG defined? GPIOG_AFRL_AFRL7 not and [if]
\ GPIOG_AFRL (read-write) Reset:0x00000000
: GPIOG_AFRL_AFRL7 ( %bbbb -- x addr ) 28 lshift GPIOG_AFRL ; \ GPIOG_AFRL_AFRL7, Alternate function selection for port x  bit y y = 0..7
: GPIOG_AFRL_AFRL6 ( %bbbb -- x addr ) 24 lshift GPIOG_AFRL ; \ GPIOG_AFRL_AFRL6, Alternate function selection for port x  bit y y = 0..7
: GPIOG_AFRL_AFRL5 ( %bbbb -- x addr ) 20 lshift GPIOG_AFRL ; \ GPIOG_AFRL_AFRL5, Alternate function selection for port x  bit y y = 0..7
: GPIOG_AFRL_AFRL4 ( %bbbb -- x addr ) 16 lshift GPIOG_AFRL ; \ GPIOG_AFRL_AFRL4, Alternate function selection for port x  bit y y = 0..7
: GPIOG_AFRL_AFRL3 ( %bbbb -- x addr ) 12 lshift GPIOG_AFRL ; \ GPIOG_AFRL_AFRL3, Alternate function selection for port x  bit y y = 0..7
: GPIOG_AFRL_AFRL2 ( %bbbb -- x addr ) 8 lshift GPIOG_AFRL ; \ GPIOG_AFRL_AFRL2, Alternate function selection for port x  bit y y = 0..7
: GPIOG_AFRL_AFRL1 ( %bbbb -- x addr ) 4 lshift GPIOG_AFRL ; \ GPIOG_AFRL_AFRL1, Alternate function selection for port x  bit y y = 0..7
: GPIOG_AFRL_AFRL0 ( %bbbb -- x addr ) GPIOG_AFRL ; \ GPIOG_AFRL_AFRL0, Alternate function selection for port x  bit y y = 0..7
[then]

execute-defined? use-GPIOG defined? GPIOG_AFRH_AFRH15 not and [if]
\ GPIOG_AFRH (read-write) Reset:0x00000000
: GPIOG_AFRH_AFRH15 ( %bbbb -- x addr ) 28 lshift GPIOG_AFRH ; \ GPIOG_AFRH_AFRH15, Alternate function selection for port x  bit y y = 8..15
: GPIOG_AFRH_AFRH14 ( %bbbb -- x addr ) 24 lshift GPIOG_AFRH ; \ GPIOG_AFRH_AFRH14, Alternate function selection for port x  bit y y = 8..15
: GPIOG_AFRH_AFRH13 ( %bbbb -- x addr ) 20 lshift GPIOG_AFRH ; \ GPIOG_AFRH_AFRH13, Alternate function selection for port x  bit y y = 8..15
: GPIOG_AFRH_AFRH12 ( %bbbb -- x addr ) 16 lshift GPIOG_AFRH ; \ GPIOG_AFRH_AFRH12, Alternate function selection for port x  bit y y = 8..15
: GPIOG_AFRH_AFRH11 ( %bbbb -- x addr ) 12 lshift GPIOG_AFRH ; \ GPIOG_AFRH_AFRH11, Alternate function selection for port x  bit y y = 8..15
: GPIOG_AFRH_AFRH10 ( %bbbb -- x addr ) 8 lshift GPIOG_AFRH ; \ GPIOG_AFRH_AFRH10, Alternate function selection for port x  bit y y = 8..15
: GPIOG_AFRH_AFRH9 ( %bbbb -- x addr ) 4 lshift GPIOG_AFRH ; \ GPIOG_AFRH_AFRH9, Alternate function selection for port x  bit y y = 8..15
: GPIOG_AFRH_AFRH8 ( %bbbb -- x addr ) GPIOG_AFRH ; \ GPIOG_AFRH_AFRH8, Alternate function selection for port x  bit y y = 8..15
[then]

defined? use-GPIOH defined? GPIOH_MODER_MODER15 not and [if]
\ GPIOH_MODER (read-write) Reset:0x00000000
: GPIOH_MODER_MODER15 ( %bb -- x addr ) 30 lshift GPIOH_MODER ; \ GPIOH_MODER_MODER15, Port x configuration bits y =  0..15
: GPIOH_MODER_MODER14 ( %bb -- x addr ) 28 lshift GPIOH_MODER ; \ GPIOH_MODER_MODER14, Port x configuration bits y =  0..15
: GPIOH_MODER_MODER13 ( %bb -- x addr ) 26 lshift GPIOH_MODER ; \ GPIOH_MODER_MODER13, Port x configuration bits y =  0..15
: GPIOH_MODER_MODER12 ( %bb -- x addr ) 24 lshift GPIOH_MODER ; \ GPIOH_MODER_MODER12, Port x configuration bits y =  0..15
: GPIOH_MODER_MODER11 ( %bb -- x addr ) 22 lshift GPIOH_MODER ; \ GPIOH_MODER_MODER11, Port x configuration bits y =  0..15
: GPIOH_MODER_MODER10 ( %bb -- x addr ) 20 lshift GPIOH_MODER ; \ GPIOH_MODER_MODER10, Port x configuration bits y =  0..15
: GPIOH_MODER_MODER9 ( %bb -- x addr ) 18 lshift GPIOH_MODER ; \ GPIOH_MODER_MODER9, Port x configuration bits y =  0..15
: GPIOH_MODER_MODER8 ( %bb -- x addr ) 16 lshift GPIOH_MODER ; \ GPIOH_MODER_MODER8, Port x configuration bits y =  0..15
: GPIOH_MODER_MODER7 ( %bb -- x addr ) 14 lshift GPIOH_MODER ; \ GPIOH_MODER_MODER7, Port x configuration bits y =  0..15
: GPIOH_MODER_MODER6 ( %bb -- x addr ) 12 lshift GPIOH_MODER ; \ GPIOH_MODER_MODER6, Port x configuration bits y =  0..15
: GPIOH_MODER_MODER5 ( %bb -- x addr ) 10 lshift GPIOH_MODER ; \ GPIOH_MODER_MODER5, Port x configuration bits y =  0..15
: GPIOH_MODER_MODER4 ( %bb -- x addr ) 8 lshift GPIOH_MODER ; \ GPIOH_MODER_MODER4, Port x configuration bits y =  0..15
: GPIOH_MODER_MODER3 ( %bb -- x addr ) 6 lshift GPIOH_MODER ; \ GPIOH_MODER_MODER3, Port x configuration bits y =  0..15
: GPIOH_MODER_MODER2 ( %bb -- x addr ) 4 lshift GPIOH_MODER ; \ GPIOH_MODER_MODER2, Port x configuration bits y =  0..15
: GPIOH_MODER_MODER1 ( %bb -- x addr ) 2 lshift GPIOH_MODER ; \ GPIOH_MODER_MODER1, Port x configuration bits y =  0..15
: GPIOH_MODER_MODER0 ( %bb -- x addr ) GPIOH_MODER ; \ GPIOH_MODER_MODER0, Port x configuration bits y =  0..15
[then]

execute-defined? use-GPIOH defined? GPIOH_OTYPER_OT15 not and [if]
\ GPIOH_OTYPER (read-write) Reset:0x00000000
: GPIOH_OTYPER_OT15 ( -- x addr ) 15 bit GPIOH_OTYPER ; \ GPIOH_OTYPER_OT15, Port x configuration bits y =  0..15
: GPIOH_OTYPER_OT14 ( -- x addr ) 14 bit GPIOH_OTYPER ; \ GPIOH_OTYPER_OT14, Port x configuration bits y =  0..15
: GPIOH_OTYPER_OT13 ( -- x addr ) 13 bit GPIOH_OTYPER ; \ GPIOH_OTYPER_OT13, Port x configuration bits y =  0..15
: GPIOH_OTYPER_OT12 ( -- x addr ) 12 bit GPIOH_OTYPER ; \ GPIOH_OTYPER_OT12, Port x configuration bits y =  0..15
: GPIOH_OTYPER_OT11 ( -- x addr ) 11 bit GPIOH_OTYPER ; \ GPIOH_OTYPER_OT11, Port x configuration bits y =  0..15
: GPIOH_OTYPER_OT10 ( -- x addr ) 10 bit GPIOH_OTYPER ; \ GPIOH_OTYPER_OT10, Port x configuration bits y =  0..15
: GPIOH_OTYPER_OT9 ( -- x addr ) 9 bit GPIOH_OTYPER ; \ GPIOH_OTYPER_OT9, Port x configuration bits y =  0..15
: GPIOH_OTYPER_OT8 ( -- x addr ) 8 bit GPIOH_OTYPER ; \ GPIOH_OTYPER_OT8, Port x configuration bits y =  0..15
: GPIOH_OTYPER_OT7 ( -- x addr ) 7 bit GPIOH_OTYPER ; \ GPIOH_OTYPER_OT7, Port x configuration bits y =  0..15
: GPIOH_OTYPER_OT6 ( -- x addr ) 6 bit GPIOH_OTYPER ; \ GPIOH_OTYPER_OT6, Port x configuration bits y =  0..15
: GPIOH_OTYPER_OT5 ( -- x addr ) 5 bit GPIOH_OTYPER ; \ GPIOH_OTYPER_OT5, Port x configuration bits y =  0..15
: GPIOH_OTYPER_OT4 ( -- x addr ) 4 bit GPIOH_OTYPER ; \ GPIOH_OTYPER_OT4, Port x configuration bits y =  0..15
: GPIOH_OTYPER_OT3 ( -- x addr ) 3 bit GPIOH_OTYPER ; \ GPIOH_OTYPER_OT3, Port x configuration bits y =  0..15
: GPIOH_OTYPER_OT2 ( -- x addr ) 2 bit GPIOH_OTYPER ; \ GPIOH_OTYPER_OT2, Port x configuration bits y =  0..15
: GPIOH_OTYPER_OT1 ( -- x addr ) 1 bit GPIOH_OTYPER ; \ GPIOH_OTYPER_OT1, Port x configuration bits y =  0..15
: GPIOH_OTYPER_OT0 ( -- x addr ) 0 bit GPIOH_OTYPER ; \ GPIOH_OTYPER_OT0, Port x configuration bits y =  0..15
[then]

defined? use-GPIOH defined? GPIOH_OSPEEDR_OSPEEDR15 not and [if]
\ GPIOH_OSPEEDR (read-write) Reset:0x00000000
: GPIOH_OSPEEDR_OSPEEDR15 ( %bb -- x addr ) 30 lshift GPIOH_OSPEEDR ; \ GPIOH_OSPEEDR_OSPEEDR15, Port x configuration bits y =  0..15
: GPIOH_OSPEEDR_OSPEEDR14 ( %bb -- x addr ) 28 lshift GPIOH_OSPEEDR ; \ GPIOH_OSPEEDR_OSPEEDR14, Port x configuration bits y =  0..15
: GPIOH_OSPEEDR_OSPEEDR13 ( %bb -- x addr ) 26 lshift GPIOH_OSPEEDR ; \ GPIOH_OSPEEDR_OSPEEDR13, Port x configuration bits y =  0..15
: GPIOH_OSPEEDR_OSPEEDR12 ( %bb -- x addr ) 24 lshift GPIOH_OSPEEDR ; \ GPIOH_OSPEEDR_OSPEEDR12, Port x configuration bits y =  0..15
: GPIOH_OSPEEDR_OSPEEDR11 ( %bb -- x addr ) 22 lshift GPIOH_OSPEEDR ; \ GPIOH_OSPEEDR_OSPEEDR11, Port x configuration bits y =  0..15
: GPIOH_OSPEEDR_OSPEEDR10 ( %bb -- x addr ) 20 lshift GPIOH_OSPEEDR ; \ GPIOH_OSPEEDR_OSPEEDR10, Port x configuration bits y =  0..15
: GPIOH_OSPEEDR_OSPEEDR9 ( %bb -- x addr ) 18 lshift GPIOH_OSPEEDR ; \ GPIOH_OSPEEDR_OSPEEDR9, Port x configuration bits y =  0..15
: GPIOH_OSPEEDR_OSPEEDR8 ( %bb -- x addr ) 16 lshift GPIOH_OSPEEDR ; \ GPIOH_OSPEEDR_OSPEEDR8, Port x configuration bits y =  0..15
: GPIOH_OSPEEDR_OSPEEDR7 ( %bb -- x addr ) 14 lshift GPIOH_OSPEEDR ; \ GPIOH_OSPEEDR_OSPEEDR7, Port x configuration bits y =  0..15
: GPIOH_OSPEEDR_OSPEEDR6 ( %bb -- x addr ) 12 lshift GPIOH_OSPEEDR ; \ GPIOH_OSPEEDR_OSPEEDR6, Port x configuration bits y =  0..15
: GPIOH_OSPEEDR_OSPEEDR5 ( %bb -- x addr ) 10 lshift GPIOH_OSPEEDR ; \ GPIOH_OSPEEDR_OSPEEDR5, Port x configuration bits y =  0..15
: GPIOH_OSPEEDR_OSPEEDR4 ( %bb -- x addr ) 8 lshift GPIOH_OSPEEDR ; \ GPIOH_OSPEEDR_OSPEEDR4, Port x configuration bits y =  0..15
: GPIOH_OSPEEDR_OSPEEDR3 ( %bb -- x addr ) 6 lshift GPIOH_OSPEEDR ; \ GPIOH_OSPEEDR_OSPEEDR3, Port x configuration bits y =  0..15
: GPIOH_OSPEEDR_OSPEEDR2 ( %bb -- x addr ) 4 lshift GPIOH_OSPEEDR ; \ GPIOH_OSPEEDR_OSPEEDR2, Port x configuration bits y =  0..15
: GPIOH_OSPEEDR_OSPEEDR1 ( %bb -- x addr ) 2 lshift GPIOH_OSPEEDR ; \ GPIOH_OSPEEDR_OSPEEDR1, Port x configuration bits y =  0..15
: GPIOH_OSPEEDR_OSPEEDR0 ( %bb -- x addr ) GPIOH_OSPEEDR ; \ GPIOH_OSPEEDR_OSPEEDR0, Port x configuration bits y =  0..15
[then]

execute-defined? use-GPIOH defined? GPIOH_PUPDR_PUPDR15 not and [if]
\ GPIOH_PUPDR (read-write) Reset:0x00000000
: GPIOH_PUPDR_PUPDR15 ( %bb -- x addr ) 30 lshift GPIOH_PUPDR ; \ GPIOH_PUPDR_PUPDR15, Port x configuration bits y =  0..15
: GPIOH_PUPDR_PUPDR14 ( %bb -- x addr ) 28 lshift GPIOH_PUPDR ; \ GPIOH_PUPDR_PUPDR14, Port x configuration bits y =  0..15
: GPIOH_PUPDR_PUPDR13 ( %bb -- x addr ) 26 lshift GPIOH_PUPDR ; \ GPIOH_PUPDR_PUPDR13, Port x configuration bits y =  0..15
: GPIOH_PUPDR_PUPDR12 ( %bb -- x addr ) 24 lshift GPIOH_PUPDR ; \ GPIOH_PUPDR_PUPDR12, Port x configuration bits y =  0..15
: GPIOH_PUPDR_PUPDR11 ( %bb -- x addr ) 22 lshift GPIOH_PUPDR ; \ GPIOH_PUPDR_PUPDR11, Port x configuration bits y =  0..15
: GPIOH_PUPDR_PUPDR10 ( %bb -- x addr ) 20 lshift GPIOH_PUPDR ; \ GPIOH_PUPDR_PUPDR10, Port x configuration bits y =  0..15
: GPIOH_PUPDR_PUPDR9 ( %bb -- x addr ) 18 lshift GPIOH_PUPDR ; \ GPIOH_PUPDR_PUPDR9, Port x configuration bits y =  0..15
: GPIOH_PUPDR_PUPDR8 ( %bb -- x addr ) 16 lshift GPIOH_PUPDR ; \ GPIOH_PUPDR_PUPDR8, Port x configuration bits y =  0..15
: GPIOH_PUPDR_PUPDR7 ( %bb -- x addr ) 14 lshift GPIOH_PUPDR ; \ GPIOH_PUPDR_PUPDR7, Port x configuration bits y =  0..15
: GPIOH_PUPDR_PUPDR6 ( %bb -- x addr ) 12 lshift GPIOH_PUPDR ; \ GPIOH_PUPDR_PUPDR6, Port x configuration bits y =  0..15
: GPIOH_PUPDR_PUPDR5 ( %bb -- x addr ) 10 lshift GPIOH_PUPDR ; \ GPIOH_PUPDR_PUPDR5, Port x configuration bits y =  0..15
: GPIOH_PUPDR_PUPDR4 ( %bb -- x addr ) 8 lshift GPIOH_PUPDR ; \ GPIOH_PUPDR_PUPDR4, Port x configuration bits y =  0..15
: GPIOH_PUPDR_PUPDR3 ( %bb -- x addr ) 6 lshift GPIOH_PUPDR ; \ GPIOH_PUPDR_PUPDR3, Port x configuration bits y =  0..15
: GPIOH_PUPDR_PUPDR2 ( %bb -- x addr ) 4 lshift GPIOH_PUPDR ; \ GPIOH_PUPDR_PUPDR2, Port x configuration bits y =  0..15
: GPIOH_PUPDR_PUPDR1 ( %bb -- x addr ) 2 lshift GPIOH_PUPDR ; \ GPIOH_PUPDR_PUPDR1, Port x configuration bits y =  0..15
: GPIOH_PUPDR_PUPDR0 ( %bb -- x addr ) GPIOH_PUPDR ; \ GPIOH_PUPDR_PUPDR0, Port x configuration bits y =  0..15
[then]

defined? use-GPIOH defined? GPIOH_IDR_IDR15? not and [if]
\ GPIOH_IDR (read-only) Reset:0x00000000
: GPIOH_IDR_IDR15? ( --  1|0 ) 15 bit GPIOH_IDR bit@ ; \ GPIOH_IDR_IDR15, Port input data y =  0..15
: GPIOH_IDR_IDR14? ( --  1|0 ) 14 bit GPIOH_IDR bit@ ; \ GPIOH_IDR_IDR14, Port input data y =  0..15
: GPIOH_IDR_IDR13? ( --  1|0 ) 13 bit GPIOH_IDR bit@ ; \ GPIOH_IDR_IDR13, Port input data y =  0..15
: GPIOH_IDR_IDR12? ( --  1|0 ) 12 bit GPIOH_IDR bit@ ; \ GPIOH_IDR_IDR12, Port input data y =  0..15
: GPIOH_IDR_IDR11? ( --  1|0 ) 11 bit GPIOH_IDR bit@ ; \ GPIOH_IDR_IDR11, Port input data y =  0..15
: GPIOH_IDR_IDR10? ( --  1|0 ) 10 bit GPIOH_IDR bit@ ; \ GPIOH_IDR_IDR10, Port input data y =  0..15
: GPIOH_IDR_IDR9? ( --  1|0 ) 9 bit GPIOH_IDR bit@ ; \ GPIOH_IDR_IDR9, Port input data y =  0..15
: GPIOH_IDR_IDR8? ( --  1|0 ) 8 bit GPIOH_IDR bit@ ; \ GPIOH_IDR_IDR8, Port input data y =  0..15
: GPIOH_IDR_IDR7? ( --  1|0 ) 7 bit GPIOH_IDR bit@ ; \ GPIOH_IDR_IDR7, Port input data y =  0..15
: GPIOH_IDR_IDR6? ( --  1|0 ) 6 bit GPIOH_IDR bit@ ; \ GPIOH_IDR_IDR6, Port input data y =  0..15
: GPIOH_IDR_IDR5? ( --  1|0 ) 5 bit GPIOH_IDR bit@ ; \ GPIOH_IDR_IDR5, Port input data y =  0..15
: GPIOH_IDR_IDR4? ( --  1|0 ) 4 bit GPIOH_IDR bit@ ; \ GPIOH_IDR_IDR4, Port input data y =  0..15
: GPIOH_IDR_IDR3? ( --  1|0 ) 3 bit GPIOH_IDR bit@ ; \ GPIOH_IDR_IDR3, Port input data y =  0..15
: GPIOH_IDR_IDR2? ( --  1|0 ) 2 bit GPIOH_IDR bit@ ; \ GPIOH_IDR_IDR2, Port input data y =  0..15
: GPIOH_IDR_IDR1? ( --  1|0 ) 1 bit GPIOH_IDR bit@ ; \ GPIOH_IDR_IDR1, Port input data y =  0..15
: GPIOH_IDR_IDR0? ( --  1|0 ) 0 bit GPIOH_IDR bit@ ; \ GPIOH_IDR_IDR0, Port input data y =  0..15
[then]

execute-defined? use-GPIOH defined? GPIOH_ODR_ODR15 not and [if]
\ GPIOH_ODR (read-write) Reset:0x00000000
: GPIOH_ODR_ODR15 ( -- x addr ) 15 bit GPIOH_ODR ; \ GPIOH_ODR_ODR15, Port output data y =  0..15
: GPIOH_ODR_ODR14 ( -- x addr ) 14 bit GPIOH_ODR ; \ GPIOH_ODR_ODR14, Port output data y =  0..15
: GPIOH_ODR_ODR13 ( -- x addr ) 13 bit GPIOH_ODR ; \ GPIOH_ODR_ODR13, Port output data y =  0..15
: GPIOH_ODR_ODR12 ( -- x addr ) 12 bit GPIOH_ODR ; \ GPIOH_ODR_ODR12, Port output data y =  0..15
: GPIOH_ODR_ODR11 ( -- x addr ) 11 bit GPIOH_ODR ; \ GPIOH_ODR_ODR11, Port output data y =  0..15
: GPIOH_ODR_ODR10 ( -- x addr ) 10 bit GPIOH_ODR ; \ GPIOH_ODR_ODR10, Port output data y =  0..15
: GPIOH_ODR_ODR9 ( -- x addr ) 9 bit GPIOH_ODR ; \ GPIOH_ODR_ODR9, Port output data y =  0..15
: GPIOH_ODR_ODR8 ( -- x addr ) 8 bit GPIOH_ODR ; \ GPIOH_ODR_ODR8, Port output data y =  0..15
: GPIOH_ODR_ODR7 ( -- x addr ) 7 bit GPIOH_ODR ; \ GPIOH_ODR_ODR7, Port output data y =  0..15
: GPIOH_ODR_ODR6 ( -- x addr ) 6 bit GPIOH_ODR ; \ GPIOH_ODR_ODR6, Port output data y =  0..15
: GPIOH_ODR_ODR5 ( -- x addr ) 5 bit GPIOH_ODR ; \ GPIOH_ODR_ODR5, Port output data y =  0..15
: GPIOH_ODR_ODR4 ( -- x addr ) 4 bit GPIOH_ODR ; \ GPIOH_ODR_ODR4, Port output data y =  0..15
: GPIOH_ODR_ODR3 ( -- x addr ) 3 bit GPIOH_ODR ; \ GPIOH_ODR_ODR3, Port output data y =  0..15
: GPIOH_ODR_ODR2 ( -- x addr ) 2 bit GPIOH_ODR ; \ GPIOH_ODR_ODR2, Port output data y =  0..15
: GPIOH_ODR_ODR1 ( -- x addr ) 1 bit GPIOH_ODR ; \ GPIOH_ODR_ODR1, Port output data y =  0..15
: GPIOH_ODR_ODR0 ( -- x addr ) 0 bit GPIOH_ODR ; \ GPIOH_ODR_ODR0, Port output data y =  0..15
[then]

defined? use-GPIOH defined? GPIOH_BSRR_BR15 not and [if]
\ GPIOH_BSRR (write-only) Reset:0x00000000
: GPIOH_BSRR_BR15 ( -- ) 31 bit GPIOH_BSRR ! ; \ GPIOH_BSRR_BR15, Port x reset bit y y =  0..15
: GPIOH_BSRR_BR14 ( -- ) 30 bit GPIOH_BSRR ! ; \ GPIOH_BSRR_BR14, Port x reset bit y y =  0..15
: GPIOH_BSRR_BR13 ( -- ) 29 bit GPIOH_BSRR ! ; \ GPIOH_BSRR_BR13, Port x reset bit y y =  0..15
: GPIOH_BSRR_BR12 ( -- ) 28 bit GPIOH_BSRR ! ; \ GPIOH_BSRR_BR12, Port x reset bit y y =  0..15
: GPIOH_BSRR_BR11 ( -- ) 27 bit GPIOH_BSRR ! ; \ GPIOH_BSRR_BR11, Port x reset bit y y =  0..15
: GPIOH_BSRR_BR10 ( -- ) 26 bit GPIOH_BSRR ! ; \ GPIOH_BSRR_BR10, Port x reset bit y y =  0..15
: GPIOH_BSRR_BR9 ( -- ) 25 bit GPIOH_BSRR ! ; \ GPIOH_BSRR_BR9, Port x reset bit y y =  0..15
: GPIOH_BSRR_BR8 ( -- ) 24 bit GPIOH_BSRR ! ; \ GPIOH_BSRR_BR8, Port x reset bit y y =  0..15
: GPIOH_BSRR_BR7 ( -- ) 23 bit GPIOH_BSRR ! ; \ GPIOH_BSRR_BR7, Port x reset bit y y =  0..15
: GPIOH_BSRR_BR6 ( -- ) 22 bit GPIOH_BSRR ! ; \ GPIOH_BSRR_BR6, Port x reset bit y y =  0..15
: GPIOH_BSRR_BR5 ( -- ) 21 bit GPIOH_BSRR ! ; \ GPIOH_BSRR_BR5, Port x reset bit y y =  0..15
: GPIOH_BSRR_BR4 ( -- ) 20 bit GPIOH_BSRR ! ; \ GPIOH_BSRR_BR4, Port x reset bit y y =  0..15
: GPIOH_BSRR_BR3 ( -- ) 19 bit GPIOH_BSRR ! ; \ GPIOH_BSRR_BR3, Port x reset bit y y =  0..15
: GPIOH_BSRR_BR2 ( -- ) 18 bit GPIOH_BSRR ! ; \ GPIOH_BSRR_BR2, Port x reset bit y y =  0..15
: GPIOH_BSRR_BR1 ( -- ) 17 bit GPIOH_BSRR ! ; \ GPIOH_BSRR_BR1, Port x reset bit y y =  0..15
: GPIOH_BSRR_BR0 ( -- ) 16 bit GPIOH_BSRR ! ; \ GPIOH_BSRR_BR0, Port x set bit y y=  0..15
: GPIOH_BSRR_BS15 ( -- ) 15 bit GPIOH_BSRR ! ; \ GPIOH_BSRR_BS15, Port x set bit y y=  0..15
: GPIOH_BSRR_BS14 ( -- ) 14 bit GPIOH_BSRR ! ; \ GPIOH_BSRR_BS14, Port x set bit y y=  0..15
: GPIOH_BSRR_BS13 ( -- ) 13 bit GPIOH_BSRR ! ; \ GPIOH_BSRR_BS13, Port x set bit y y=  0..15
: GPIOH_BSRR_BS12 ( -- ) 12 bit GPIOH_BSRR ! ; \ GPIOH_BSRR_BS12, Port x set bit y y=  0..15
: GPIOH_BSRR_BS11 ( -- ) 11 bit GPIOH_BSRR ! ; \ GPIOH_BSRR_BS11, Port x set bit y y=  0..15
: GPIOH_BSRR_BS10 ( -- ) 10 bit GPIOH_BSRR ! ; \ GPIOH_BSRR_BS10, Port x set bit y y=  0..15
: GPIOH_BSRR_BS9 ( -- ) 9 bit GPIOH_BSRR ! ; \ GPIOH_BSRR_BS9, Port x set bit y y=  0..15
: GPIOH_BSRR_BS8 ( -- ) 8 bit GPIOH_BSRR ! ; \ GPIOH_BSRR_BS8, Port x set bit y y=  0..15
: GPIOH_BSRR_BS7 ( -- ) 7 bit GPIOH_BSRR ! ; \ GPIOH_BSRR_BS7, Port x set bit y y=  0..15
: GPIOH_BSRR_BS6 ( -- ) 6 bit GPIOH_BSRR ! ; \ GPIOH_BSRR_BS6, Port x set bit y y=  0..15
: GPIOH_BSRR_BS5 ( -- ) 5 bit GPIOH_BSRR ! ; \ GPIOH_BSRR_BS5, Port x set bit y y=  0..15
: GPIOH_BSRR_BS4 ( -- ) 4 bit GPIOH_BSRR ! ; \ GPIOH_BSRR_BS4, Port x set bit y y=  0..15
: GPIOH_BSRR_BS3 ( -- ) 3 bit GPIOH_BSRR ! ; \ GPIOH_BSRR_BS3, Port x set bit y y=  0..15
: GPIOH_BSRR_BS2 ( -- ) 2 bit GPIOH_BSRR ! ; \ GPIOH_BSRR_BS2, Port x set bit y y=  0..15
: GPIOH_BSRR_BS1 ( -- ) 1 bit GPIOH_BSRR ! ; \ GPIOH_BSRR_BS1, Port x set bit y y=  0..15
: GPIOH_BSRR_BS0 ( -- ) 0 bit GPIOH_BSRR ! ; \ GPIOH_BSRR_BS0, Port x set bit y y=  0..15
[then]

execute-defined? use-GPIOH defined? GPIOH_LCKR_LCKK not and [if]
\ GPIOH_LCKR (read-write) Reset:0x00000000
: GPIOH_LCKR_LCKK ( -- x addr ) 16 bit GPIOH_LCKR ; \ GPIOH_LCKR_LCKK, Port x lock bit y y=  0..15
: GPIOH_LCKR_LCK15 ( -- x addr ) 15 bit GPIOH_LCKR ; \ GPIOH_LCKR_LCK15, Port x lock bit y y=  0..15
: GPIOH_LCKR_LCK14 ( -- x addr ) 14 bit GPIOH_LCKR ; \ GPIOH_LCKR_LCK14, Port x lock bit y y=  0..15
: GPIOH_LCKR_LCK13 ( -- x addr ) 13 bit GPIOH_LCKR ; \ GPIOH_LCKR_LCK13, Port x lock bit y y=  0..15
: GPIOH_LCKR_LCK12 ( -- x addr ) 12 bit GPIOH_LCKR ; \ GPIOH_LCKR_LCK12, Port x lock bit y y=  0..15
: GPIOH_LCKR_LCK11 ( -- x addr ) 11 bit GPIOH_LCKR ; \ GPIOH_LCKR_LCK11, Port x lock bit y y=  0..15
: GPIOH_LCKR_LCK10 ( -- x addr ) 10 bit GPIOH_LCKR ; \ GPIOH_LCKR_LCK10, Port x lock bit y y=  0..15
: GPIOH_LCKR_LCK9 ( -- x addr ) 9 bit GPIOH_LCKR ; \ GPIOH_LCKR_LCK9, Port x lock bit y y=  0..15
: GPIOH_LCKR_LCK8 ( -- x addr ) 8 bit GPIOH_LCKR ; \ GPIOH_LCKR_LCK8, Port x lock bit y y=  0..15
: GPIOH_LCKR_LCK7 ( -- x addr ) 7 bit GPIOH_LCKR ; \ GPIOH_LCKR_LCK7, Port x lock bit y y=  0..15
: GPIOH_LCKR_LCK6 ( -- x addr ) 6 bit GPIOH_LCKR ; \ GPIOH_LCKR_LCK6, Port x lock bit y y=  0..15
: GPIOH_LCKR_LCK5 ( -- x addr ) 5 bit GPIOH_LCKR ; \ GPIOH_LCKR_LCK5, Port x lock bit y y=  0..15
: GPIOH_LCKR_LCK4 ( -- x addr ) 4 bit GPIOH_LCKR ; \ GPIOH_LCKR_LCK4, Port x lock bit y y=  0..15
: GPIOH_LCKR_LCK3 ( -- x addr ) 3 bit GPIOH_LCKR ; \ GPIOH_LCKR_LCK3, Port x lock bit y y=  0..15
: GPIOH_LCKR_LCK2 ( -- x addr ) 2 bit GPIOH_LCKR ; \ GPIOH_LCKR_LCK2, Port x lock bit y y=  0..15
: GPIOH_LCKR_LCK1 ( -- x addr ) 1 bit GPIOH_LCKR ; \ GPIOH_LCKR_LCK1, Port x lock bit y y=  0..15
: GPIOH_LCKR_LCK0 ( -- x addr ) 0 bit GPIOH_LCKR ; \ GPIOH_LCKR_LCK0, Port x lock bit y y=  0..15
[then]

defined? use-GPIOH defined? GPIOH_AFRL_AFRL7 not and [if]
\ GPIOH_AFRL (read-write) Reset:0x00000000
: GPIOH_AFRL_AFRL7 ( %bbbb -- x addr ) 28 lshift GPIOH_AFRL ; \ GPIOH_AFRL_AFRL7, Alternate function selection for port x  bit y y = 0..7
: GPIOH_AFRL_AFRL6 ( %bbbb -- x addr ) 24 lshift GPIOH_AFRL ; \ GPIOH_AFRL_AFRL6, Alternate function selection for port x  bit y y = 0..7
: GPIOH_AFRL_AFRL5 ( %bbbb -- x addr ) 20 lshift GPIOH_AFRL ; \ GPIOH_AFRL_AFRL5, Alternate function selection for port x  bit y y = 0..7
: GPIOH_AFRL_AFRL4 ( %bbbb -- x addr ) 16 lshift GPIOH_AFRL ; \ GPIOH_AFRL_AFRL4, Alternate function selection for port x  bit y y = 0..7
: GPIOH_AFRL_AFRL3 ( %bbbb -- x addr ) 12 lshift GPIOH_AFRL ; \ GPIOH_AFRL_AFRL3, Alternate function selection for port x  bit y y = 0..7
: GPIOH_AFRL_AFRL2 ( %bbbb -- x addr ) 8 lshift GPIOH_AFRL ; \ GPIOH_AFRL_AFRL2, Alternate function selection for port x  bit y y = 0..7
: GPIOH_AFRL_AFRL1 ( %bbbb -- x addr ) 4 lshift GPIOH_AFRL ; \ GPIOH_AFRL_AFRL1, Alternate function selection for port x  bit y y = 0..7
: GPIOH_AFRL_AFRL0 ( %bbbb -- x addr ) GPIOH_AFRL ; \ GPIOH_AFRL_AFRL0, Alternate function selection for port x  bit y y = 0..7
[then]

execute-defined? use-GPIOH defined? GPIOH_AFRH_AFRH15 not and [if]
\ GPIOH_AFRH (read-write) Reset:0x00000000
: GPIOH_AFRH_AFRH15 ( %bbbb -- x addr ) 28 lshift GPIOH_AFRH ; \ GPIOH_AFRH_AFRH15, Alternate function selection for port x  bit y y = 8..15
: GPIOH_AFRH_AFRH14 ( %bbbb -- x addr ) 24 lshift GPIOH_AFRH ; \ GPIOH_AFRH_AFRH14, Alternate function selection for port x  bit y y = 8..15
: GPIOH_AFRH_AFRH13 ( %bbbb -- x addr ) 20 lshift GPIOH_AFRH ; \ GPIOH_AFRH_AFRH13, Alternate function selection for port x  bit y y = 8..15
: GPIOH_AFRH_AFRH12 ( %bbbb -- x addr ) 16 lshift GPIOH_AFRH ; \ GPIOH_AFRH_AFRH12, Alternate function selection for port x  bit y y = 8..15
: GPIOH_AFRH_AFRH11 ( %bbbb -- x addr ) 12 lshift GPIOH_AFRH ; \ GPIOH_AFRH_AFRH11, Alternate function selection for port x  bit y y = 8..15
: GPIOH_AFRH_AFRH10 ( %bbbb -- x addr ) 8 lshift GPIOH_AFRH ; \ GPIOH_AFRH_AFRH10, Alternate function selection for port x  bit y y = 8..15
: GPIOH_AFRH_AFRH9 ( %bbbb -- x addr ) 4 lshift GPIOH_AFRH ; \ GPIOH_AFRH_AFRH9, Alternate function selection for port x  bit y y = 8..15
: GPIOH_AFRH_AFRH8 ( %bbbb -- x addr ) GPIOH_AFRH ; \ GPIOH_AFRH_AFRH8, Alternate function selection for port x  bit y y = 8..15
[then]

defined? use-TIM2 defined? TIM2_CR1_CKD not and [if]
\ TIM2_CR1 (read-write) Reset:0x0000
: TIM2_CR1_CKD ( %bb -- x addr ) 8 lshift TIM2_CR1 ; \ TIM2_CR1_CKD, Clock division
: TIM2_CR1_ARPE ( -- x addr ) 7 bit TIM2_CR1 ; \ TIM2_CR1_ARPE, Auto-reload preload enable
: TIM2_CR1_CMS ( %bb -- x addr ) 5 lshift TIM2_CR1 ; \ TIM2_CR1_CMS, Center-aligned mode  selection
: TIM2_CR1_DIR ( -- x addr ) 4 bit TIM2_CR1 ; \ TIM2_CR1_DIR, Direction
: TIM2_CR1_OPM ( -- x addr ) 3 bit TIM2_CR1 ; \ TIM2_CR1_OPM, One-pulse mode
: TIM2_CR1_URS ( -- x addr ) 2 bit TIM2_CR1 ; \ TIM2_CR1_URS, Update request source
: TIM2_CR1_UDIS ( -- x addr ) 1 bit TIM2_CR1 ; \ TIM2_CR1_UDIS, Update disable
: TIM2_CR1_CEN ( -- x addr ) 0 bit TIM2_CR1 ; \ TIM2_CR1_CEN, Counter enable
[then]

execute-defined? use-TIM2 defined? TIM2_CR2_TI1S not and [if]
\ TIM2_CR2 (read-write) Reset:0x0000
: TIM2_CR2_TI1S ( -- x addr ) 7 bit TIM2_CR2 ; \ TIM2_CR2_TI1S, TI1 selection
: TIM2_CR2_MMS ( %bbb -- x addr ) 4 lshift TIM2_CR2 ; \ TIM2_CR2_MMS, Master mode selection
: TIM2_CR2_CCDS ( -- x addr ) 3 bit TIM2_CR2 ; \ TIM2_CR2_CCDS, Capture/compare DMA  selection
[then]

defined? use-TIM2 defined? TIM2_SMCR_ETP not and [if]
\ TIM2_SMCR (read-write) Reset:0x0000
: TIM2_SMCR_ETP ( -- x addr ) 15 bit TIM2_SMCR ; \ TIM2_SMCR_ETP, External trigger polarity
: TIM2_SMCR_ECE ( -- x addr ) 14 bit TIM2_SMCR ; \ TIM2_SMCR_ECE, External clock enable
: TIM2_SMCR_ETPS ( %bb -- x addr ) 12 lshift TIM2_SMCR ; \ TIM2_SMCR_ETPS, External trigger prescaler
: TIM2_SMCR_ETF ( %bbbb -- x addr ) 8 lshift TIM2_SMCR ; \ TIM2_SMCR_ETF, External trigger filter
: TIM2_SMCR_MSM ( -- x addr ) 7 bit TIM2_SMCR ; \ TIM2_SMCR_MSM, Master/Slave mode
: TIM2_SMCR_TS ( %bbb -- x addr ) 4 lshift TIM2_SMCR ; \ TIM2_SMCR_TS, Trigger selection
: TIM2_SMCR_SMS ( %bbb -- x addr ) TIM2_SMCR ; \ TIM2_SMCR_SMS, Slave mode selection
[then]

execute-defined? use-TIM2 defined? TIM2_DIER_TDE not and [if]
\ TIM2_DIER (read-write) Reset:0x0000
: TIM2_DIER_TDE ( -- x addr ) 14 bit TIM2_DIER ; \ TIM2_DIER_TDE, Trigger DMA request enable
: TIM2_DIER_COMDE ( -- x addr ) 13 bit TIM2_DIER ; \ TIM2_DIER_COMDE, Reserved
: TIM2_DIER_CC4DE ( -- x addr ) 12 bit TIM2_DIER ; \ TIM2_DIER_CC4DE, Capture/Compare 4 DMA request  enable
: TIM2_DIER_CC3DE ( -- x addr ) 11 bit TIM2_DIER ; \ TIM2_DIER_CC3DE, Capture/Compare 3 DMA request  enable
: TIM2_DIER_CC2DE ( -- x addr ) 10 bit TIM2_DIER ; \ TIM2_DIER_CC2DE, Capture/Compare 2 DMA request  enable
: TIM2_DIER_CC1DE ( -- x addr ) 9 bit TIM2_DIER ; \ TIM2_DIER_CC1DE, Capture/Compare 1 DMA request  enable
: TIM2_DIER_UDE ( -- x addr ) 8 bit TIM2_DIER ; \ TIM2_DIER_UDE, Update DMA request enable
: TIM2_DIER_TIE ( -- x addr ) 6 bit TIM2_DIER ; \ TIM2_DIER_TIE, Trigger interrupt enable
: TIM2_DIER_CC4IE ( -- x addr ) 4 bit TIM2_DIER ; \ TIM2_DIER_CC4IE, Capture/Compare 4 interrupt  enable
: TIM2_DIER_CC3IE ( -- x addr ) 3 bit TIM2_DIER ; \ TIM2_DIER_CC3IE, Capture/Compare 3 interrupt  enable
: TIM2_DIER_CC2IE ( -- x addr ) 2 bit TIM2_DIER ; \ TIM2_DIER_CC2IE, Capture/Compare 2 interrupt  enable
: TIM2_DIER_CC1IE ( -- x addr ) 1 bit TIM2_DIER ; \ TIM2_DIER_CC1IE, Capture/Compare 1 interrupt  enable
: TIM2_DIER_UIE ( -- x addr ) 0 bit TIM2_DIER ; \ TIM2_DIER_UIE, Update interrupt enable
[then]

defined? use-TIM2 defined? TIM2_SR_CC4OF not and [if]
\ TIM2_SR (read-write) Reset:0x0000
: TIM2_SR_CC4OF ( -- x addr ) 12 bit TIM2_SR ; \ TIM2_SR_CC4OF, Capture/Compare 4 overcapture  flag
: TIM2_SR_CC3OF ( -- x addr ) 11 bit TIM2_SR ; \ TIM2_SR_CC3OF, Capture/Compare 3 overcapture  flag
: TIM2_SR_CC2OF ( -- x addr ) 10 bit TIM2_SR ; \ TIM2_SR_CC2OF, Capture/compare 2 overcapture  flag
: TIM2_SR_CC1OF ( -- x addr ) 9 bit TIM2_SR ; \ TIM2_SR_CC1OF, Capture/Compare 1 overcapture  flag
: TIM2_SR_TIF ( -- x addr ) 6 bit TIM2_SR ; \ TIM2_SR_TIF, Trigger interrupt flag
: TIM2_SR_CC4IF ( -- x addr ) 4 bit TIM2_SR ; \ TIM2_SR_CC4IF, Capture/Compare 4 interrupt  flag
: TIM2_SR_CC3IF ( -- x addr ) 3 bit TIM2_SR ; \ TIM2_SR_CC3IF, Capture/Compare 3 interrupt  flag
: TIM2_SR_CC2IF ( -- x addr ) 2 bit TIM2_SR ; \ TIM2_SR_CC2IF, Capture/Compare 2 interrupt  flag
: TIM2_SR_CC1IF ( -- x addr ) 1 bit TIM2_SR ; \ TIM2_SR_CC1IF, Capture/compare 1 interrupt  flag
: TIM2_SR_UIF ( -- x addr ) 0 bit TIM2_SR ; \ TIM2_SR_UIF, Update interrupt flag
[then]

execute-defined? use-TIM2 defined? TIM2_EGR_TG not and [if]
\ TIM2_EGR (write-only) Reset:0x0000
: TIM2_EGR_TG ( -- x addr ) 6 bit TIM2_EGR ; \ TIM2_EGR_TG, Trigger generation
: TIM2_EGR_CC4G ( -- x addr ) 4 bit TIM2_EGR ; \ TIM2_EGR_CC4G, Capture/compare 4  generation
: TIM2_EGR_CC3G ( -- x addr ) 3 bit TIM2_EGR ; \ TIM2_EGR_CC3G, Capture/compare 3  generation
: TIM2_EGR_CC2G ( -- x addr ) 2 bit TIM2_EGR ; \ TIM2_EGR_CC2G, Capture/compare 2  generation
: TIM2_EGR_CC1G ( -- x addr ) 1 bit TIM2_EGR ; \ TIM2_EGR_CC1G, Capture/compare 1  generation
: TIM2_EGR_UG ( -- x addr ) 0 bit TIM2_EGR ; \ TIM2_EGR_UG, Update generation
[then]

defined? use-TIM2 defined? TIM2_CCMR1_Output_OC2CE not and [if]
\ TIM2_CCMR1_Output (read-write) Reset:0x00000000
: TIM2_CCMR1_Output_OC2CE ( -- x addr ) 15 bit TIM2_CCMR1_Output ; \ TIM2_CCMR1_Output_OC2CE, Output compare 2 clear  enable
: TIM2_CCMR1_Output_OC2M ( %bbb -- x addr ) 12 lshift TIM2_CCMR1_Output ; \ TIM2_CCMR1_Output_OC2M, Output compare 2 mode
: TIM2_CCMR1_Output_OC2PE ( -- x addr ) 11 bit TIM2_CCMR1_Output ; \ TIM2_CCMR1_Output_OC2PE, Output compare 2 preload  enable
: TIM2_CCMR1_Output_OC2FE ( -- x addr ) 10 bit TIM2_CCMR1_Output ; \ TIM2_CCMR1_Output_OC2FE, Output compare 2 fast  enable
: TIM2_CCMR1_Output_CC2S ( %bb -- x addr ) 8 lshift TIM2_CCMR1_Output ; \ TIM2_CCMR1_Output_CC2S, Capture/Compare 2  selection
: TIM2_CCMR1_Output_OC1CE ( -- x addr ) 7 bit TIM2_CCMR1_Output ; \ TIM2_CCMR1_Output_OC1CE, Output compare 1 clear  enable
: TIM2_CCMR1_Output_OC1M ( %bbb -- x addr ) 4 lshift TIM2_CCMR1_Output ; \ TIM2_CCMR1_Output_OC1M, Output compare 1 mode
: TIM2_CCMR1_Output_OC1PE ( -- x addr ) 3 bit TIM2_CCMR1_Output ; \ TIM2_CCMR1_Output_OC1PE, Output compare 1 preload  enable
: TIM2_CCMR1_Output_OC1FE ( -- x addr ) 2 bit TIM2_CCMR1_Output ; \ TIM2_CCMR1_Output_OC1FE, Output compare 1 fast  enable
: TIM2_CCMR1_Output_CC1S ( %bb -- x addr ) TIM2_CCMR1_Output ; \ TIM2_CCMR1_Output_CC1S, Capture/Compare 1  selection
[then]

execute-defined? use-TIM2 defined? TIM2_CCMR1_Input_IC2F not and [if]
\ TIM2_CCMR1_Input (read-write) Reset:0x00000000
: TIM2_CCMR1_Input_IC2F ( %bbbb -- x addr ) 12 lshift TIM2_CCMR1_Input ; \ TIM2_CCMR1_Input_IC2F, Input capture 2 filter
: TIM2_CCMR1_Input_IC2PSC ( %bb -- x addr ) 10 lshift TIM2_CCMR1_Input ; \ TIM2_CCMR1_Input_IC2PSC, Input capture 2 prescaler
: TIM2_CCMR1_Input_CC2S ( %bb -- x addr ) 8 lshift TIM2_CCMR1_Input ; \ TIM2_CCMR1_Input_CC2S, Capture/compare 2  selection
: TIM2_CCMR1_Input_IC1F ( %bbbb -- x addr ) 4 lshift TIM2_CCMR1_Input ; \ TIM2_CCMR1_Input_IC1F, Input capture 1 filter
: TIM2_CCMR1_Input_IC1PSC ( %bb -- x addr ) 2 lshift TIM2_CCMR1_Input ; \ TIM2_CCMR1_Input_IC1PSC, Input capture 1 prescaler
: TIM2_CCMR1_Input_CC1S ( %bb -- x addr ) TIM2_CCMR1_Input ; \ TIM2_CCMR1_Input_CC1S, Capture/Compare 1  selection
[then]

defined? use-TIM2 defined? TIM2_CCMR2_Output_OC4CE not and [if]
\ TIM2_CCMR2_Output (read-write) Reset:0x00000000
: TIM2_CCMR2_Output_OC4CE ( -- x addr ) 15 bit TIM2_CCMR2_Output ; \ TIM2_CCMR2_Output_OC4CE, Output compare 4 clear  enable
: TIM2_CCMR2_Output_OC4M ( %bbb -- x addr ) 12 lshift TIM2_CCMR2_Output ; \ TIM2_CCMR2_Output_OC4M, Output compare 4 mode
: TIM2_CCMR2_Output_OC4PE ( -- x addr ) 11 bit TIM2_CCMR2_Output ; \ TIM2_CCMR2_Output_OC4PE, Output compare 4 preload  enable
: TIM2_CCMR2_Output_OC4FE ( -- x addr ) 10 bit TIM2_CCMR2_Output ; \ TIM2_CCMR2_Output_OC4FE, Output compare 4 fast  enable
: TIM2_CCMR2_Output_CC4S ( %bb -- x addr ) 8 lshift TIM2_CCMR2_Output ; \ TIM2_CCMR2_Output_CC4S, Capture/Compare 4  selection
: TIM2_CCMR2_Output_OC3CE ( -- x addr ) 7 bit TIM2_CCMR2_Output ; \ TIM2_CCMR2_Output_OC3CE, Output compare 3 clear  enable
: TIM2_CCMR2_Output_OC3M ( %bbb -- x addr ) 4 lshift TIM2_CCMR2_Output ; \ TIM2_CCMR2_Output_OC3M, Output compare 3 mode
: TIM2_CCMR2_Output_OC3PE ( -- x addr ) 3 bit TIM2_CCMR2_Output ; \ TIM2_CCMR2_Output_OC3PE, Output compare 3 preload  enable
: TIM2_CCMR2_Output_OC3FE ( -- x addr ) 2 bit TIM2_CCMR2_Output ; \ TIM2_CCMR2_Output_OC3FE, Output compare 3 fast  enable
: TIM2_CCMR2_Output_CC3S ( %bb -- x addr ) TIM2_CCMR2_Output ; \ TIM2_CCMR2_Output_CC3S, Capture/Compare 3  selection
[then]

execute-defined? use-TIM2 defined? TIM2_CCMR2_Input_IC4F not and [if]
\ TIM2_CCMR2_Input (read-write) Reset:0x00000000
: TIM2_CCMR2_Input_IC4F ( %bbbb -- x addr ) 12 lshift TIM2_CCMR2_Input ; \ TIM2_CCMR2_Input_IC4F, Input capture 4 filter
: TIM2_CCMR2_Input_IC4PSC ( %bb -- x addr ) 10 lshift TIM2_CCMR2_Input ; \ TIM2_CCMR2_Input_IC4PSC, Input capture 4 prescaler
: TIM2_CCMR2_Input_CC4S ( %bb -- x addr ) 8 lshift TIM2_CCMR2_Input ; \ TIM2_CCMR2_Input_CC4S, Capture/Compare 4  selection
: TIM2_CCMR2_Input_IC3F ( %bbbb -- x addr ) 4 lshift TIM2_CCMR2_Input ; \ TIM2_CCMR2_Input_IC3F, Input capture 3 filter
: TIM2_CCMR2_Input_IC3PSC ( %bb -- x addr ) 2 lshift TIM2_CCMR2_Input ; \ TIM2_CCMR2_Input_IC3PSC, Input capture 3 prescaler
: TIM2_CCMR2_Input_CC3S ( %bb -- x addr ) TIM2_CCMR2_Input ; \ TIM2_CCMR2_Input_CC3S, Capture/Compare 3  selection
[then]

defined? use-TIM2 defined? TIM2_CCER_CC4NP not and [if]
\ TIM2_CCER (read-write) Reset:0x0000
: TIM2_CCER_CC4NP ( -- x addr ) 15 bit TIM2_CCER ; \ TIM2_CCER_CC4NP, Capture/Compare 4 output  Polarity
: TIM2_CCER_CC4P ( -- x addr ) 13 bit TIM2_CCER ; \ TIM2_CCER_CC4P, Capture/Compare 3 output  Polarity
: TIM2_CCER_CC4E ( -- x addr ) 12 bit TIM2_CCER ; \ TIM2_CCER_CC4E, Capture/Compare 4 output  enable
: TIM2_CCER_CC3NP ( -- x addr ) 11 bit TIM2_CCER ; \ TIM2_CCER_CC3NP, Capture/Compare 3 output  Polarity
: TIM2_CCER_CC3P ( -- x addr ) 9 bit TIM2_CCER ; \ TIM2_CCER_CC3P, Capture/Compare 3 output  Polarity
: TIM2_CCER_CC3E ( -- x addr ) 8 bit TIM2_CCER ; \ TIM2_CCER_CC3E, Capture/Compare 3 output  enable
: TIM2_CCER_CC2NP ( -- x addr ) 7 bit TIM2_CCER ; \ TIM2_CCER_CC2NP, Capture/Compare 2 output  Polarity
: TIM2_CCER_CC2P ( -- x addr ) 5 bit TIM2_CCER ; \ TIM2_CCER_CC2P, Capture/Compare 2 output  Polarity
: TIM2_CCER_CC2E ( -- x addr ) 4 bit TIM2_CCER ; \ TIM2_CCER_CC2E, Capture/Compare 2 output  enable
: TIM2_CCER_CC1NP ( -- x addr ) 3 bit TIM2_CCER ; \ TIM2_CCER_CC1NP, Capture/Compare 1 output  Polarity
: TIM2_CCER_CC1P ( -- x addr ) 1 bit TIM2_CCER ; \ TIM2_CCER_CC1P, Capture/Compare 1 output  Polarity
: TIM2_CCER_CC1E ( -- x addr ) 0 bit TIM2_CCER ; \ TIM2_CCER_CC1E, Capture/Compare 1 output  enable
[then]

execute-defined? use-TIM2 defined? TIM2_CNT_CNT_H not and [if]
\ TIM2_CNT (read-write) Reset:0x00000000
: TIM2_CNT_CNT_H ( %bbbbbbbbbbbbbbbb -- x addr ) 16 lshift TIM2_CNT ; \ TIM2_CNT_CNT_H, High counter value TIM2  only
: TIM2_CNT_CNT_L ( %bbbbbbbbbbbbbbbb -- x addr ) TIM2_CNT ; \ TIM2_CNT_CNT_L, Low counter value
[then]

defined? use-TIM2 defined? TIM2_PSC_PSC not and [if]
\ TIM2_PSC (read-write) Reset:0x0000
: TIM2_PSC_PSC ( %bbbbbbbbbbbbbbbb -- x addr ) TIM2_PSC ; \ TIM2_PSC_PSC, Prescaler value
[then]

execute-defined? use-TIM2 defined? TIM2_ARR_ARR_H not and [if]
\ TIM2_ARR (read-write) Reset:0x00000000
: TIM2_ARR_ARR_H ( %bbbbbbbbbbbbbbbb -- x addr ) 16 lshift TIM2_ARR ; \ TIM2_ARR_ARR_H, High Auto-reload value TIM2  only
: TIM2_ARR_ARR_L ( %bbbbbbbbbbbbbbbb -- x addr ) TIM2_ARR ; \ TIM2_ARR_ARR_L, Low Auto-reload value
[then]

defined? use-TIM2 defined? TIM2_CCR1_CCR1_H not and [if]
\ TIM2_CCR1 (read-write) Reset:0x00000000
: TIM2_CCR1_CCR1_H ( %bbbbbbbbbbbbbbbb -- x addr ) 16 lshift TIM2_CCR1 ; \ TIM2_CCR1_CCR1_H, High Capture/Compare 1 value TIM2  only
: TIM2_CCR1_CCR1_L ( %bbbbbbbbbbbbbbbb -- x addr ) TIM2_CCR1 ; \ TIM2_CCR1_CCR1_L, Low Capture/Compare 1  value
[then]

execute-defined? use-TIM2 defined? TIM2_CCR2_CCR2_H not and [if]
\ TIM2_CCR2 (read-write) Reset:0x00000000
: TIM2_CCR2_CCR2_H ( %bbbbbbbbbbbbbbbb -- x addr ) 16 lshift TIM2_CCR2 ; \ TIM2_CCR2_CCR2_H, High Capture/Compare 2 value TIM2  only
: TIM2_CCR2_CCR2_L ( %bbbbbbbbbbbbbbbb -- x addr ) TIM2_CCR2 ; \ TIM2_CCR2_CCR2_L, Low Capture/Compare 2  value
[then]

defined? use-TIM2 defined? TIM2_CCR3_CCR3_H not and [if]
\ TIM2_CCR3 (read-write) Reset:0x00000000
: TIM2_CCR3_CCR3_H ( %bbbbbbbbbbbbbbbb -- x addr ) 16 lshift TIM2_CCR3 ; \ TIM2_CCR3_CCR3_H, High Capture/Compare value TIM2  only
: TIM2_CCR3_CCR3_L ( %bbbbbbbbbbbbbbbb -- x addr ) TIM2_CCR3 ; \ TIM2_CCR3_CCR3_L, Low Capture/Compare value
[then]

execute-defined? use-TIM2 defined? TIM2_CCR4_CCR4_H not and [if]
\ TIM2_CCR4 (read-write) Reset:0x00000000
: TIM2_CCR4_CCR4_H ( %bbbbbbbbbbbbbbbb -- x addr ) 16 lshift TIM2_CCR4 ; \ TIM2_CCR4_CCR4_H, High Capture/Compare value TIM2  only
: TIM2_CCR4_CCR4_L ( %bbbbbbbbbbbbbbbb -- x addr ) TIM2_CCR4 ; \ TIM2_CCR4_CCR4_L, Low Capture/Compare value
[then]

defined? use-TIM2 defined? TIM2_DCR_DBL not and [if]
\ TIM2_DCR (read-write) Reset:0x0000
: TIM2_DCR_DBL ( %bbbbb -- x addr ) 8 lshift TIM2_DCR ; \ TIM2_DCR_DBL, DMA burst length
: TIM2_DCR_DBA ( %bbbbb -- x addr ) TIM2_DCR ; \ TIM2_DCR_DBA, DMA base address
[then]

execute-defined? use-TIM2 defined? TIM2_DMAR_DMAB not and [if]
\ TIM2_DMAR (read-write) Reset:0x0000
: TIM2_DMAR_DMAB ( %bbbbbbbbbbbbbbbb -- x addr ) TIM2_DMAR ; \ TIM2_DMAR_DMAB, DMA register for burst  accesses
[then]

defined? use-TIM2 defined? TIM2_OR_ETR_RMP not and [if]
\ TIM2_OR (read-write) Reset:0x0000
: TIM2_OR_ETR_RMP ( %bbb -- x addr ) TIM2_OR ; \ TIM2_OR_ETR_RMP, Timer2 ETR remap
: TIM2_OR_TI4_RMP ( %bb -- x addr ) 3 lshift TIM2_OR ; \ TIM2_OR_TI4_RMP, Internal trigger
[then]

execute-defined? use-TIM3 defined? TIM3_CR1_CKD not and [if]
\ TIM3_CR1 (read-write) Reset:0x0000
: TIM3_CR1_CKD ( %bb -- x addr ) 8 lshift TIM3_CR1 ; \ TIM3_CR1_CKD, Clock division
: TIM3_CR1_ARPE ( -- x addr ) 7 bit TIM3_CR1 ; \ TIM3_CR1_ARPE, Auto-reload preload enable
: TIM3_CR1_CMS ( %bb -- x addr ) 5 lshift TIM3_CR1 ; \ TIM3_CR1_CMS, Center-aligned mode  selection
: TIM3_CR1_DIR ( -- x addr ) 4 bit TIM3_CR1 ; \ TIM3_CR1_DIR, Direction
: TIM3_CR1_OPM ( -- x addr ) 3 bit TIM3_CR1 ; \ TIM3_CR1_OPM, One-pulse mode
: TIM3_CR1_URS ( -- x addr ) 2 bit TIM3_CR1 ; \ TIM3_CR1_URS, Update request source
: TIM3_CR1_UDIS ( -- x addr ) 1 bit TIM3_CR1 ; \ TIM3_CR1_UDIS, Update disable
: TIM3_CR1_CEN ( -- x addr ) 0 bit TIM3_CR1 ; \ TIM3_CR1_CEN, Counter enable
[then]

defined? use-TIM3 defined? TIM3_CR2_TI1S not and [if]
\ TIM3_CR2 (read-write) Reset:0x0000
: TIM3_CR2_TI1S ( -- x addr ) 7 bit TIM3_CR2 ; \ TIM3_CR2_TI1S, TI1 selection
: TIM3_CR2_MMS ( %bbb -- x addr ) 4 lshift TIM3_CR2 ; \ TIM3_CR2_MMS, Master mode selection
: TIM3_CR2_CCDS ( -- x addr ) 3 bit TIM3_CR2 ; \ TIM3_CR2_CCDS, Capture/compare DMA  selection
[then]

execute-defined? use-TIM3 defined? TIM3_SMCR_ETP not and [if]
\ TIM3_SMCR (read-write) Reset:0x0000
: TIM3_SMCR_ETP ( -- x addr ) 15 bit TIM3_SMCR ; \ TIM3_SMCR_ETP, External trigger polarity
: TIM3_SMCR_ECE ( -- x addr ) 14 bit TIM3_SMCR ; \ TIM3_SMCR_ECE, External clock enable
: TIM3_SMCR_ETPS ( %bb -- x addr ) 12 lshift TIM3_SMCR ; \ TIM3_SMCR_ETPS, External trigger prescaler
: TIM3_SMCR_ETF ( %bbbb -- x addr ) 8 lshift TIM3_SMCR ; \ TIM3_SMCR_ETF, External trigger filter
: TIM3_SMCR_MSM ( -- x addr ) 7 bit TIM3_SMCR ; \ TIM3_SMCR_MSM, Master/Slave mode
: TIM3_SMCR_TS ( %bbb -- x addr ) 4 lshift TIM3_SMCR ; \ TIM3_SMCR_TS, Trigger selection
: TIM3_SMCR_SMS ( %bbb -- x addr ) TIM3_SMCR ; \ TIM3_SMCR_SMS, Slave mode selection
[then]

defined? use-TIM3 defined? TIM3_DIER_TDE not and [if]
\ TIM3_DIER (read-write) Reset:0x0000
: TIM3_DIER_TDE ( -- x addr ) 14 bit TIM3_DIER ; \ TIM3_DIER_TDE, Trigger DMA request enable
: TIM3_DIER_COMDE ( -- x addr ) 13 bit TIM3_DIER ; \ TIM3_DIER_COMDE, Reserved
: TIM3_DIER_CC4DE ( -- x addr ) 12 bit TIM3_DIER ; \ TIM3_DIER_CC4DE, Capture/Compare 4 DMA request  enable
: TIM3_DIER_CC3DE ( -- x addr ) 11 bit TIM3_DIER ; \ TIM3_DIER_CC3DE, Capture/Compare 3 DMA request  enable
: TIM3_DIER_CC2DE ( -- x addr ) 10 bit TIM3_DIER ; \ TIM3_DIER_CC2DE, Capture/Compare 2 DMA request  enable
: TIM3_DIER_CC1DE ( -- x addr ) 9 bit TIM3_DIER ; \ TIM3_DIER_CC1DE, Capture/Compare 1 DMA request  enable
: TIM3_DIER_UDE ( -- x addr ) 8 bit TIM3_DIER ; \ TIM3_DIER_UDE, Update DMA request enable
: TIM3_DIER_TIE ( -- x addr ) 6 bit TIM3_DIER ; \ TIM3_DIER_TIE, Trigger interrupt enable
: TIM3_DIER_CC4IE ( -- x addr ) 4 bit TIM3_DIER ; \ TIM3_DIER_CC4IE, Capture/Compare 4 interrupt  enable
: TIM3_DIER_CC3IE ( -- x addr ) 3 bit TIM3_DIER ; \ TIM3_DIER_CC3IE, Capture/Compare 3 interrupt  enable
: TIM3_DIER_CC2IE ( -- x addr ) 2 bit TIM3_DIER ; \ TIM3_DIER_CC2IE, Capture/Compare 2 interrupt  enable
: TIM3_DIER_CC1IE ( -- x addr ) 1 bit TIM3_DIER ; \ TIM3_DIER_CC1IE, Capture/Compare 1 interrupt  enable
: TIM3_DIER_UIE ( -- x addr ) 0 bit TIM3_DIER ; \ TIM3_DIER_UIE, Update interrupt enable
[then]

execute-defined? use-TIM3 defined? TIM3_SR_CC4OF not and [if]
\ TIM3_SR (read-write) Reset:0x0000
: TIM3_SR_CC4OF ( -- x addr ) 12 bit TIM3_SR ; \ TIM3_SR_CC4OF, Capture/Compare 4 overcapture  flag
: TIM3_SR_CC3OF ( -- x addr ) 11 bit TIM3_SR ; \ TIM3_SR_CC3OF, Capture/Compare 3 overcapture  flag
: TIM3_SR_CC2OF ( -- x addr ) 10 bit TIM3_SR ; \ TIM3_SR_CC2OF, Capture/compare 2 overcapture  flag
: TIM3_SR_CC1OF ( -- x addr ) 9 bit TIM3_SR ; \ TIM3_SR_CC1OF, Capture/Compare 1 overcapture  flag
: TIM3_SR_TIF ( -- x addr ) 6 bit TIM3_SR ; \ TIM3_SR_TIF, Trigger interrupt flag
: TIM3_SR_CC4IF ( -- x addr ) 4 bit TIM3_SR ; \ TIM3_SR_CC4IF, Capture/Compare 4 interrupt  flag
: TIM3_SR_CC3IF ( -- x addr ) 3 bit TIM3_SR ; \ TIM3_SR_CC3IF, Capture/Compare 3 interrupt  flag
: TIM3_SR_CC2IF ( -- x addr ) 2 bit TIM3_SR ; \ TIM3_SR_CC2IF, Capture/Compare 2 interrupt  flag
: TIM3_SR_CC1IF ( -- x addr ) 1 bit TIM3_SR ; \ TIM3_SR_CC1IF, Capture/compare 1 interrupt  flag
: TIM3_SR_UIF ( -- x addr ) 0 bit TIM3_SR ; \ TIM3_SR_UIF, Update interrupt flag
[then]

defined? use-TIM3 defined? TIM3_EGR_TG not and [if]
\ TIM3_EGR (write-only) Reset:0x0000
: TIM3_EGR_TG ( -- x addr ) 6 bit TIM3_EGR ; \ TIM3_EGR_TG, Trigger generation
: TIM3_EGR_CC4G ( -- x addr ) 4 bit TIM3_EGR ; \ TIM3_EGR_CC4G, Capture/compare 4  generation
: TIM3_EGR_CC3G ( -- x addr ) 3 bit TIM3_EGR ; \ TIM3_EGR_CC3G, Capture/compare 3  generation
: TIM3_EGR_CC2G ( -- x addr ) 2 bit TIM3_EGR ; \ TIM3_EGR_CC2G, Capture/compare 2  generation
: TIM3_EGR_CC1G ( -- x addr ) 1 bit TIM3_EGR ; \ TIM3_EGR_CC1G, Capture/compare 1  generation
: TIM3_EGR_UG ( -- x addr ) 0 bit TIM3_EGR ; \ TIM3_EGR_UG, Update generation
[then]

execute-defined? use-TIM3 defined? TIM3_CCMR1_Output_OC2CE not and [if]
\ TIM3_CCMR1_Output (read-write) Reset:0x00000000
: TIM3_CCMR1_Output_OC2CE ( -- x addr ) 15 bit TIM3_CCMR1_Output ; \ TIM3_CCMR1_Output_OC2CE, Output compare 2 clear  enable
: TIM3_CCMR1_Output_OC2M ( %bbb -- x addr ) 12 lshift TIM3_CCMR1_Output ; \ TIM3_CCMR1_Output_OC2M, Output compare 2 mode
: TIM3_CCMR1_Output_OC2PE ( -- x addr ) 11 bit TIM3_CCMR1_Output ; \ TIM3_CCMR1_Output_OC2PE, Output compare 2 preload  enable
: TIM3_CCMR1_Output_OC2FE ( -- x addr ) 10 bit TIM3_CCMR1_Output ; \ TIM3_CCMR1_Output_OC2FE, Output compare 2 fast  enable
: TIM3_CCMR1_Output_CC2S ( %bb -- x addr ) 8 lshift TIM3_CCMR1_Output ; \ TIM3_CCMR1_Output_CC2S, Capture/Compare 2  selection
: TIM3_CCMR1_Output_OC1CE ( -- x addr ) 7 bit TIM3_CCMR1_Output ; \ TIM3_CCMR1_Output_OC1CE, Output compare 1 clear  enable
: TIM3_CCMR1_Output_OC1M ( %bbb -- x addr ) 4 lshift TIM3_CCMR1_Output ; \ TIM3_CCMR1_Output_OC1M, Output compare 1 mode
: TIM3_CCMR1_Output_OC1PE ( -- x addr ) 3 bit TIM3_CCMR1_Output ; \ TIM3_CCMR1_Output_OC1PE, Output compare 1 preload  enable
: TIM3_CCMR1_Output_OC1FE ( -- x addr ) 2 bit TIM3_CCMR1_Output ; \ TIM3_CCMR1_Output_OC1FE, Output compare 1 fast  enable
: TIM3_CCMR1_Output_CC1S ( %bb -- x addr ) TIM3_CCMR1_Output ; \ TIM3_CCMR1_Output_CC1S, Capture/Compare 1  selection
[then]

defined? use-TIM3 defined? TIM3_CCMR1_Input_IC2F not and [if]
\ TIM3_CCMR1_Input (read-write) Reset:0x00000000
: TIM3_CCMR1_Input_IC2F ( %bbbb -- x addr ) 12 lshift TIM3_CCMR1_Input ; \ TIM3_CCMR1_Input_IC2F, Input capture 2 filter
: TIM3_CCMR1_Input_IC2PSC ( %bb -- x addr ) 10 lshift TIM3_CCMR1_Input ; \ TIM3_CCMR1_Input_IC2PSC, Input capture 2 prescaler
: TIM3_CCMR1_Input_CC2S ( %bb -- x addr ) 8 lshift TIM3_CCMR1_Input ; \ TIM3_CCMR1_Input_CC2S, Capture/compare 2  selection
: TIM3_CCMR1_Input_IC1F ( %bbbb -- x addr ) 4 lshift TIM3_CCMR1_Input ; \ TIM3_CCMR1_Input_IC1F, Input capture 1 filter
: TIM3_CCMR1_Input_IC1PSC ( %bb -- x addr ) 2 lshift TIM3_CCMR1_Input ; \ TIM3_CCMR1_Input_IC1PSC, Input capture 1 prescaler
: TIM3_CCMR1_Input_CC1S ( %bb -- x addr ) TIM3_CCMR1_Input ; \ TIM3_CCMR1_Input_CC1S, Capture/Compare 1  selection
[then]

execute-defined? use-TIM3 defined? TIM3_CCMR2_Output_OC4CE not and [if]
\ TIM3_CCMR2_Output (read-write) Reset:0x00000000
: TIM3_CCMR2_Output_OC4CE ( -- x addr ) 15 bit TIM3_CCMR2_Output ; \ TIM3_CCMR2_Output_OC4CE, Output compare 4 clear  enable
: TIM3_CCMR2_Output_OC4M ( %bbb -- x addr ) 12 lshift TIM3_CCMR2_Output ; \ TIM3_CCMR2_Output_OC4M, Output compare 4 mode
: TIM3_CCMR2_Output_OC4PE ( -- x addr ) 11 bit TIM3_CCMR2_Output ; \ TIM3_CCMR2_Output_OC4PE, Output compare 4 preload  enable
: TIM3_CCMR2_Output_OC4FE ( -- x addr ) 10 bit TIM3_CCMR2_Output ; \ TIM3_CCMR2_Output_OC4FE, Output compare 4 fast  enable
: TIM3_CCMR2_Output_CC4S ( %bb -- x addr ) 8 lshift TIM3_CCMR2_Output ; \ TIM3_CCMR2_Output_CC4S, Capture/Compare 4  selection
: TIM3_CCMR2_Output_OC3CE ( -- x addr ) 7 bit TIM3_CCMR2_Output ; \ TIM3_CCMR2_Output_OC3CE, Output compare 3 clear  enable
: TIM3_CCMR2_Output_OC3M ( %bbb -- x addr ) 4 lshift TIM3_CCMR2_Output ; \ TIM3_CCMR2_Output_OC3M, Output compare 3 mode
: TIM3_CCMR2_Output_OC3PE ( -- x addr ) 3 bit TIM3_CCMR2_Output ; \ TIM3_CCMR2_Output_OC3PE, Output compare 3 preload  enable
: TIM3_CCMR2_Output_OC3FE ( -- x addr ) 2 bit TIM3_CCMR2_Output ; \ TIM3_CCMR2_Output_OC3FE, Output compare 3 fast  enable
: TIM3_CCMR2_Output_CC3S ( %bb -- x addr ) TIM3_CCMR2_Output ; \ TIM3_CCMR2_Output_CC3S, Capture/Compare 3  selection
[then]

defined? use-TIM3 defined? TIM3_CCMR2_Input_IC4F not and [if]
\ TIM3_CCMR2_Input (read-write) Reset:0x00000000
: TIM3_CCMR2_Input_IC4F ( %bbbb -- x addr ) 12 lshift TIM3_CCMR2_Input ; \ TIM3_CCMR2_Input_IC4F, Input capture 4 filter
: TIM3_CCMR2_Input_IC4PSC ( %bb -- x addr ) 10 lshift TIM3_CCMR2_Input ; \ TIM3_CCMR2_Input_IC4PSC, Input capture 4 prescaler
: TIM3_CCMR2_Input_CC4S ( %bb -- x addr ) 8 lshift TIM3_CCMR2_Input ; \ TIM3_CCMR2_Input_CC4S, Capture/Compare 4  selection
: TIM3_CCMR2_Input_IC3F ( %bbbb -- x addr ) 4 lshift TIM3_CCMR2_Input ; \ TIM3_CCMR2_Input_IC3F, Input capture 3 filter
: TIM3_CCMR2_Input_IC3PSC ( %bb -- x addr ) 2 lshift TIM3_CCMR2_Input ; \ TIM3_CCMR2_Input_IC3PSC, Input capture 3 prescaler
: TIM3_CCMR2_Input_CC3S ( %bb -- x addr ) TIM3_CCMR2_Input ; \ TIM3_CCMR2_Input_CC3S, Capture/Compare 3  selection
[then]

execute-defined? use-TIM3 defined? TIM3_CCER_CC4NP not and [if]
\ TIM3_CCER (read-write) Reset:0x0000
: TIM3_CCER_CC4NP ( -- x addr ) 15 bit TIM3_CCER ; \ TIM3_CCER_CC4NP, Capture/Compare 4 output  Polarity
: TIM3_CCER_CC4P ( -- x addr ) 13 bit TIM3_CCER ; \ TIM3_CCER_CC4P, Capture/Compare 3 output  Polarity
: TIM3_CCER_CC4E ( -- x addr ) 12 bit TIM3_CCER ; \ TIM3_CCER_CC4E, Capture/Compare 4 output  enable
: TIM3_CCER_CC3NP ( -- x addr ) 11 bit TIM3_CCER ; \ TIM3_CCER_CC3NP, Capture/Compare 3 output  Polarity
: TIM3_CCER_CC3P ( -- x addr ) 9 bit TIM3_CCER ; \ TIM3_CCER_CC3P, Capture/Compare 3 output  Polarity
: TIM3_CCER_CC3E ( -- x addr ) 8 bit TIM3_CCER ; \ TIM3_CCER_CC3E, Capture/Compare 3 output  enable
: TIM3_CCER_CC2NP ( -- x addr ) 7 bit TIM3_CCER ; \ TIM3_CCER_CC2NP, Capture/Compare 2 output  Polarity
: TIM3_CCER_CC2P ( -- x addr ) 5 bit TIM3_CCER ; \ TIM3_CCER_CC2P, Capture/Compare 2 output  Polarity
: TIM3_CCER_CC2E ( -- x addr ) 4 bit TIM3_CCER ; \ TIM3_CCER_CC2E, Capture/Compare 2 output  enable
: TIM3_CCER_CC1NP ( -- x addr ) 3 bit TIM3_CCER ; \ TIM3_CCER_CC1NP, Capture/Compare 1 output  Polarity
: TIM3_CCER_CC1P ( -- x addr ) 1 bit TIM3_CCER ; \ TIM3_CCER_CC1P, Capture/Compare 1 output  Polarity
: TIM3_CCER_CC1E ( -- x addr ) 0 bit TIM3_CCER ; \ TIM3_CCER_CC1E, Capture/Compare 1 output  enable
[then]

defined? use-TIM3 defined? TIM3_CNT_CNT_H not and [if]
\ TIM3_CNT (read-write) Reset:0x00000000
: TIM3_CNT_CNT_H ( %bbbbbbbbbbbbbbbb -- x addr ) 16 lshift TIM3_CNT ; \ TIM3_CNT_CNT_H, High counter value TIM2  only
: TIM3_CNT_CNT_L ( %bbbbbbbbbbbbbbbb -- x addr ) TIM3_CNT ; \ TIM3_CNT_CNT_L, Low counter value
[then]

execute-defined? use-TIM3 defined? TIM3_PSC_PSC not and [if]
\ TIM3_PSC (read-write) Reset:0x0000
: TIM3_PSC_PSC ( %bbbbbbbbbbbbbbbb -- x addr ) TIM3_PSC ; \ TIM3_PSC_PSC, Prescaler value
[then]

defined? use-TIM3 defined? TIM3_ARR_ARR_H not and [if]
\ TIM3_ARR (read-write) Reset:0x00000000
: TIM3_ARR_ARR_H ( %bbbbbbbbbbbbbbbb -- x addr ) 16 lshift TIM3_ARR ; \ TIM3_ARR_ARR_H, High Auto-reload value TIM2  only
: TIM3_ARR_ARR_L ( %bbbbbbbbbbbbbbbb -- x addr ) TIM3_ARR ; \ TIM3_ARR_ARR_L, Low Auto-reload value
[then]

execute-defined? use-TIM3 defined? TIM3_CCR1_CCR1_H not and [if]
\ TIM3_CCR1 (read-write) Reset:0x00000000
: TIM3_CCR1_CCR1_H ( %bbbbbbbbbbbbbbbb -- x addr ) 16 lshift TIM3_CCR1 ; \ TIM3_CCR1_CCR1_H, High Capture/Compare 1 value TIM2  only
: TIM3_CCR1_CCR1_L ( %bbbbbbbbbbbbbbbb -- x addr ) TIM3_CCR1 ; \ TIM3_CCR1_CCR1_L, Low Capture/Compare 1  value
[then]

defined? use-TIM3 defined? TIM3_CCR2_CCR2_H not and [if]
\ TIM3_CCR2 (read-write) Reset:0x00000000
: TIM3_CCR2_CCR2_H ( %bbbbbbbbbbbbbbbb -- x addr ) 16 lshift TIM3_CCR2 ; \ TIM3_CCR2_CCR2_H, High Capture/Compare 2 value TIM2  only
: TIM3_CCR2_CCR2_L ( %bbbbbbbbbbbbbbbb -- x addr ) TIM3_CCR2 ; \ TIM3_CCR2_CCR2_L, Low Capture/Compare 2  value
[then]

execute-defined? use-TIM3 defined? TIM3_CCR3_CCR3_H not and [if]
\ TIM3_CCR3 (read-write) Reset:0x00000000
: TIM3_CCR3_CCR3_H ( %bbbbbbbbbbbbbbbb -- x addr ) 16 lshift TIM3_CCR3 ; \ TIM3_CCR3_CCR3_H, High Capture/Compare value TIM2  only
: TIM3_CCR3_CCR3_L ( %bbbbbbbbbbbbbbbb -- x addr ) TIM3_CCR3 ; \ TIM3_CCR3_CCR3_L, Low Capture/Compare value
[then]

defined? use-TIM3 defined? TIM3_CCR4_CCR4_H not and [if]
\ TIM3_CCR4 (read-write) Reset:0x00000000
: TIM3_CCR4_CCR4_H ( %bbbbbbbbbbbbbbbb -- x addr ) 16 lshift TIM3_CCR4 ; \ TIM3_CCR4_CCR4_H, High Capture/Compare value TIM2  only
: TIM3_CCR4_CCR4_L ( %bbbbbbbbbbbbbbbb -- x addr ) TIM3_CCR4 ; \ TIM3_CCR4_CCR4_L, Low Capture/Compare value
[then]

execute-defined? use-TIM3 defined? TIM3_DCR_DBL not and [if]
\ TIM3_DCR (read-write) Reset:0x0000
: TIM3_DCR_DBL ( %bbbbb -- x addr ) 8 lshift TIM3_DCR ; \ TIM3_DCR_DBL, DMA burst length
: TIM3_DCR_DBA ( %bbbbb -- x addr ) TIM3_DCR ; \ TIM3_DCR_DBA, DMA base address
[then]

defined? use-TIM3 defined? TIM3_DMAR_DMAB not and [if]
\ TIM3_DMAR (read-write) Reset:0x0000
: TIM3_DMAR_DMAB ( %bbbbbbbbbbbbbbbb -- x addr ) TIM3_DMAR ; \ TIM3_DMAR_DMAB, DMA register for burst  accesses
[then]

execute-defined? use-TIM3 defined? TIM3_OR_ETR_RMP not and [if]
\ TIM3_OR (read-write) Reset:0x0000
: TIM3_OR_ETR_RMP ( %bbb -- x addr ) TIM3_OR ; \ TIM3_OR_ETR_RMP, Timer2 ETR remap
: TIM3_OR_TI4_RMP ( %bb -- x addr ) 3 lshift TIM3_OR ; \ TIM3_OR_TI4_RMP, Internal trigger
[then]

defined? use-TIM4 defined? TIM4_CR1_CKD not and [if]
\ TIM4_CR1 (read-write) Reset:0x0000
: TIM4_CR1_CKD ( %bb -- x addr ) 8 lshift TIM4_CR1 ; \ TIM4_CR1_CKD, Clock division
: TIM4_CR1_ARPE ( -- x addr ) 7 bit TIM4_CR1 ; \ TIM4_CR1_ARPE, Auto-reload preload enable
: TIM4_CR1_CMS ( %bb -- x addr ) 5 lshift TIM4_CR1 ; \ TIM4_CR1_CMS, Center-aligned mode  selection
: TIM4_CR1_DIR ( -- x addr ) 4 bit TIM4_CR1 ; \ TIM4_CR1_DIR, Direction
: TIM4_CR1_OPM ( -- x addr ) 3 bit TIM4_CR1 ; \ TIM4_CR1_OPM, One-pulse mode
: TIM4_CR1_URS ( -- x addr ) 2 bit TIM4_CR1 ; \ TIM4_CR1_URS, Update request source
: TIM4_CR1_UDIS ( -- x addr ) 1 bit TIM4_CR1 ; \ TIM4_CR1_UDIS, Update disable
: TIM4_CR1_CEN ( -- x addr ) 0 bit TIM4_CR1 ; \ TIM4_CR1_CEN, Counter enable
[then]

execute-defined? use-TIM4 defined? TIM4_CR2_TI1S not and [if]
\ TIM4_CR2 (read-write) Reset:0x0000
: TIM4_CR2_TI1S ( -- x addr ) 7 bit TIM4_CR2 ; \ TIM4_CR2_TI1S, TI1 selection
: TIM4_CR2_MMS ( %bbb -- x addr ) 4 lshift TIM4_CR2 ; \ TIM4_CR2_MMS, Master mode selection
: TIM4_CR2_CCDS ( -- x addr ) 3 bit TIM4_CR2 ; \ TIM4_CR2_CCDS, Capture/compare DMA  selection
[then]

defined? use-TIM4 defined? TIM4_SMCR_ETP not and [if]
\ TIM4_SMCR (read-write) Reset:0x0000
: TIM4_SMCR_ETP ( -- x addr ) 15 bit TIM4_SMCR ; \ TIM4_SMCR_ETP, External trigger polarity
: TIM4_SMCR_ECE ( -- x addr ) 14 bit TIM4_SMCR ; \ TIM4_SMCR_ECE, External clock enable
: TIM4_SMCR_ETPS ( %bb -- x addr ) 12 lshift TIM4_SMCR ; \ TIM4_SMCR_ETPS, External trigger prescaler
: TIM4_SMCR_ETF ( %bbbb -- x addr ) 8 lshift TIM4_SMCR ; \ TIM4_SMCR_ETF, External trigger filter
: TIM4_SMCR_MSM ( -- x addr ) 7 bit TIM4_SMCR ; \ TIM4_SMCR_MSM, Master/Slave mode
: TIM4_SMCR_TS ( %bbb -- x addr ) 4 lshift TIM4_SMCR ; \ TIM4_SMCR_TS, Trigger selection
: TIM4_SMCR_SMS ( %bbb -- x addr ) TIM4_SMCR ; \ TIM4_SMCR_SMS, Slave mode selection
[then]

execute-defined? use-TIM4 defined? TIM4_DIER_TDE not and [if]
\ TIM4_DIER (read-write) Reset:0x0000
: TIM4_DIER_TDE ( -- x addr ) 14 bit TIM4_DIER ; \ TIM4_DIER_TDE, Trigger DMA request enable
: TIM4_DIER_COMDE ( -- x addr ) 13 bit TIM4_DIER ; \ TIM4_DIER_COMDE, Reserved
: TIM4_DIER_CC4DE ( -- x addr ) 12 bit TIM4_DIER ; \ TIM4_DIER_CC4DE, Capture/Compare 4 DMA request  enable
: TIM4_DIER_CC3DE ( -- x addr ) 11 bit TIM4_DIER ; \ TIM4_DIER_CC3DE, Capture/Compare 3 DMA request  enable
: TIM4_DIER_CC2DE ( -- x addr ) 10 bit TIM4_DIER ; \ TIM4_DIER_CC2DE, Capture/Compare 2 DMA request  enable
: TIM4_DIER_CC1DE ( -- x addr ) 9 bit TIM4_DIER ; \ TIM4_DIER_CC1DE, Capture/Compare 1 DMA request  enable
: TIM4_DIER_UDE ( -- x addr ) 8 bit TIM4_DIER ; \ TIM4_DIER_UDE, Update DMA request enable
: TIM4_DIER_TIE ( -- x addr ) 6 bit TIM4_DIER ; \ TIM4_DIER_TIE, Trigger interrupt enable
: TIM4_DIER_CC4IE ( -- x addr ) 4 bit TIM4_DIER ; \ TIM4_DIER_CC4IE, Capture/Compare 4 interrupt  enable
: TIM4_DIER_CC3IE ( -- x addr ) 3 bit TIM4_DIER ; \ TIM4_DIER_CC3IE, Capture/Compare 3 interrupt  enable
: TIM4_DIER_CC2IE ( -- x addr ) 2 bit TIM4_DIER ; \ TIM4_DIER_CC2IE, Capture/Compare 2 interrupt  enable
: TIM4_DIER_CC1IE ( -- x addr ) 1 bit TIM4_DIER ; \ TIM4_DIER_CC1IE, Capture/Compare 1 interrupt  enable
: TIM4_DIER_UIE ( -- x addr ) 0 bit TIM4_DIER ; \ TIM4_DIER_UIE, Update interrupt enable
[then]

defined? use-TIM4 defined? TIM4_SR_CC4OF not and [if]
\ TIM4_SR (read-write) Reset:0x0000
: TIM4_SR_CC4OF ( -- x addr ) 12 bit TIM4_SR ; \ TIM4_SR_CC4OF, Capture/Compare 4 overcapture  flag
: TIM4_SR_CC3OF ( -- x addr ) 11 bit TIM4_SR ; \ TIM4_SR_CC3OF, Capture/Compare 3 overcapture  flag
: TIM4_SR_CC2OF ( -- x addr ) 10 bit TIM4_SR ; \ TIM4_SR_CC2OF, Capture/compare 2 overcapture  flag
: TIM4_SR_CC1OF ( -- x addr ) 9 bit TIM4_SR ; \ TIM4_SR_CC1OF, Capture/Compare 1 overcapture  flag
: TIM4_SR_TIF ( -- x addr ) 6 bit TIM4_SR ; \ TIM4_SR_TIF, Trigger interrupt flag
: TIM4_SR_CC4IF ( -- x addr ) 4 bit TIM4_SR ; \ TIM4_SR_CC4IF, Capture/Compare 4 interrupt  flag
: TIM4_SR_CC3IF ( -- x addr ) 3 bit TIM4_SR ; \ TIM4_SR_CC3IF, Capture/Compare 3 interrupt  flag
: TIM4_SR_CC2IF ( -- x addr ) 2 bit TIM4_SR ; \ TIM4_SR_CC2IF, Capture/Compare 2 interrupt  flag
: TIM4_SR_CC1IF ( -- x addr ) 1 bit TIM4_SR ; \ TIM4_SR_CC1IF, Capture/compare 1 interrupt  flag
: TIM4_SR_UIF ( -- x addr ) 0 bit TIM4_SR ; \ TIM4_SR_UIF, Update interrupt flag
[then]

execute-defined? use-TIM4 defined? TIM4_EGR_TG not and [if]
\ TIM4_EGR (write-only) Reset:0x0000
: TIM4_EGR_TG ( -- x addr ) 6 bit TIM4_EGR ; \ TIM4_EGR_TG, Trigger generation
: TIM4_EGR_CC4G ( -- x addr ) 4 bit TIM4_EGR ; \ TIM4_EGR_CC4G, Capture/compare 4  generation
: TIM4_EGR_CC3G ( -- x addr ) 3 bit TIM4_EGR ; \ TIM4_EGR_CC3G, Capture/compare 3  generation
: TIM4_EGR_CC2G ( -- x addr ) 2 bit TIM4_EGR ; \ TIM4_EGR_CC2G, Capture/compare 2  generation
: TIM4_EGR_CC1G ( -- x addr ) 1 bit TIM4_EGR ; \ TIM4_EGR_CC1G, Capture/compare 1  generation
: TIM4_EGR_UG ( -- x addr ) 0 bit TIM4_EGR ; \ TIM4_EGR_UG, Update generation
[then]

defined? use-TIM4 defined? TIM4_CCMR1_Output_OC2CE not and [if]
\ TIM4_CCMR1_Output (read-write) Reset:0x00000000
: TIM4_CCMR1_Output_OC2CE ( -- x addr ) 15 bit TIM4_CCMR1_Output ; \ TIM4_CCMR1_Output_OC2CE, Output compare 2 clear  enable
: TIM4_CCMR1_Output_OC2M ( %bbb -- x addr ) 12 lshift TIM4_CCMR1_Output ; \ TIM4_CCMR1_Output_OC2M, Output compare 2 mode
: TIM4_CCMR1_Output_OC2PE ( -- x addr ) 11 bit TIM4_CCMR1_Output ; \ TIM4_CCMR1_Output_OC2PE, Output compare 2 preload  enable
: TIM4_CCMR1_Output_OC2FE ( -- x addr ) 10 bit TIM4_CCMR1_Output ; \ TIM4_CCMR1_Output_OC2FE, Output compare 2 fast  enable
: TIM4_CCMR1_Output_CC2S ( %bb -- x addr ) 8 lshift TIM4_CCMR1_Output ; \ TIM4_CCMR1_Output_CC2S, Capture/Compare 2  selection
: TIM4_CCMR1_Output_OC1CE ( -- x addr ) 7 bit TIM4_CCMR1_Output ; \ TIM4_CCMR1_Output_OC1CE, Output compare 1 clear  enable
: TIM4_CCMR1_Output_OC1M ( %bbb -- x addr ) 4 lshift TIM4_CCMR1_Output ; \ TIM4_CCMR1_Output_OC1M, Output compare 1 mode
: TIM4_CCMR1_Output_OC1PE ( -- x addr ) 3 bit TIM4_CCMR1_Output ; \ TIM4_CCMR1_Output_OC1PE, Output compare 1 preload  enable
: TIM4_CCMR1_Output_OC1FE ( -- x addr ) 2 bit TIM4_CCMR1_Output ; \ TIM4_CCMR1_Output_OC1FE, Output compare 1 fast  enable
: TIM4_CCMR1_Output_CC1S ( %bb -- x addr ) TIM4_CCMR1_Output ; \ TIM4_CCMR1_Output_CC1S, Capture/Compare 1  selection
[then]

execute-defined? use-TIM4 defined? TIM4_CCMR1_Input_IC2F not and [if]
\ TIM4_CCMR1_Input (read-write) Reset:0x00000000
: TIM4_CCMR1_Input_IC2F ( %bbbb -- x addr ) 12 lshift TIM4_CCMR1_Input ; \ TIM4_CCMR1_Input_IC2F, Input capture 2 filter
: TIM4_CCMR1_Input_IC2PSC ( %bb -- x addr ) 10 lshift TIM4_CCMR1_Input ; \ TIM4_CCMR1_Input_IC2PSC, Input capture 2 prescaler
: TIM4_CCMR1_Input_CC2S ( %bb -- x addr ) 8 lshift TIM4_CCMR1_Input ; \ TIM4_CCMR1_Input_CC2S, Capture/compare 2  selection
: TIM4_CCMR1_Input_IC1F ( %bbbb -- x addr ) 4 lshift TIM4_CCMR1_Input ; \ TIM4_CCMR1_Input_IC1F, Input capture 1 filter
: TIM4_CCMR1_Input_IC1PSC ( %bb -- x addr ) 2 lshift TIM4_CCMR1_Input ; \ TIM4_CCMR1_Input_IC1PSC, Input capture 1 prescaler
: TIM4_CCMR1_Input_CC1S ( %bb -- x addr ) TIM4_CCMR1_Input ; \ TIM4_CCMR1_Input_CC1S, Capture/Compare 1  selection
[then]

defined? use-TIM4 defined? TIM4_CCMR2_Output_OC4CE not and [if]
\ TIM4_CCMR2_Output (read-write) Reset:0x00000000
: TIM4_CCMR2_Output_OC4CE ( -- x addr ) 15 bit TIM4_CCMR2_Output ; \ TIM4_CCMR2_Output_OC4CE, Output compare 4 clear  enable
: TIM4_CCMR2_Output_OC4M ( %bbb -- x addr ) 12 lshift TIM4_CCMR2_Output ; \ TIM4_CCMR2_Output_OC4M, Output compare 4 mode
: TIM4_CCMR2_Output_OC4PE ( -- x addr ) 11 bit TIM4_CCMR2_Output ; \ TIM4_CCMR2_Output_OC4PE, Output compare 4 preload  enable
: TIM4_CCMR2_Output_OC4FE ( -- x addr ) 10 bit TIM4_CCMR2_Output ; \ TIM4_CCMR2_Output_OC4FE, Output compare 4 fast  enable
: TIM4_CCMR2_Output_CC4S ( %bb -- x addr ) 8 lshift TIM4_CCMR2_Output ; \ TIM4_CCMR2_Output_CC4S, Capture/Compare 4  selection
: TIM4_CCMR2_Output_OC3CE ( -- x addr ) 7 bit TIM4_CCMR2_Output ; \ TIM4_CCMR2_Output_OC3CE, Output compare 3 clear  enable
: TIM4_CCMR2_Output_OC3M ( %bbb -- x addr ) 4 lshift TIM4_CCMR2_Output ; \ TIM4_CCMR2_Output_OC3M, Output compare 3 mode
: TIM4_CCMR2_Output_OC3PE ( -- x addr ) 3 bit TIM4_CCMR2_Output ; \ TIM4_CCMR2_Output_OC3PE, Output compare 3 preload  enable
: TIM4_CCMR2_Output_OC3FE ( -- x addr ) 2 bit TIM4_CCMR2_Output ; \ TIM4_CCMR2_Output_OC3FE, Output compare 3 fast  enable
: TIM4_CCMR2_Output_CC3S ( %bb -- x addr ) TIM4_CCMR2_Output ; \ TIM4_CCMR2_Output_CC3S, Capture/Compare 3  selection
[then]

execute-defined? use-TIM4 defined? TIM4_CCMR2_Input_IC4F not and [if]
\ TIM4_CCMR2_Input (read-write) Reset:0x00000000
: TIM4_CCMR2_Input_IC4F ( %bbbb -- x addr ) 12 lshift TIM4_CCMR2_Input ; \ TIM4_CCMR2_Input_IC4F, Input capture 4 filter
: TIM4_CCMR2_Input_IC4PSC ( %bb -- x addr ) 10 lshift TIM4_CCMR2_Input ; \ TIM4_CCMR2_Input_IC4PSC, Input capture 4 prescaler
: TIM4_CCMR2_Input_CC4S ( %bb -- x addr ) 8 lshift TIM4_CCMR2_Input ; \ TIM4_CCMR2_Input_CC4S, Capture/Compare 4  selection
: TIM4_CCMR2_Input_IC3F ( %bbbb -- x addr ) 4 lshift TIM4_CCMR2_Input ; \ TIM4_CCMR2_Input_IC3F, Input capture 3 filter
: TIM4_CCMR2_Input_IC3PSC ( %bb -- x addr ) 2 lshift TIM4_CCMR2_Input ; \ TIM4_CCMR2_Input_IC3PSC, Input capture 3 prescaler
: TIM4_CCMR2_Input_CC3S ( %bb -- x addr ) TIM4_CCMR2_Input ; \ TIM4_CCMR2_Input_CC3S, Capture/Compare 3  selection
[then]

defined? use-TIM4 defined? TIM4_CCER_CC4NP not and [if]
\ TIM4_CCER (read-write) Reset:0x0000
: TIM4_CCER_CC4NP ( -- x addr ) 15 bit TIM4_CCER ; \ TIM4_CCER_CC4NP, Capture/Compare 4 output  Polarity
: TIM4_CCER_CC4P ( -- x addr ) 13 bit TIM4_CCER ; \ TIM4_CCER_CC4P, Capture/Compare 3 output  Polarity
: TIM4_CCER_CC4E ( -- x addr ) 12 bit TIM4_CCER ; \ TIM4_CCER_CC4E, Capture/Compare 4 output  enable
: TIM4_CCER_CC3NP ( -- x addr ) 11 bit TIM4_CCER ; \ TIM4_CCER_CC3NP, Capture/Compare 3 output  Polarity
: TIM4_CCER_CC3P ( -- x addr ) 9 bit TIM4_CCER ; \ TIM4_CCER_CC3P, Capture/Compare 3 output  Polarity
: TIM4_CCER_CC3E ( -- x addr ) 8 bit TIM4_CCER ; \ TIM4_CCER_CC3E, Capture/Compare 3 output  enable
: TIM4_CCER_CC2NP ( -- x addr ) 7 bit TIM4_CCER ; \ TIM4_CCER_CC2NP, Capture/Compare 2 output  Polarity
: TIM4_CCER_CC2P ( -- x addr ) 5 bit TIM4_CCER ; \ TIM4_CCER_CC2P, Capture/Compare 2 output  Polarity
: TIM4_CCER_CC2E ( -- x addr ) 4 bit TIM4_CCER ; \ TIM4_CCER_CC2E, Capture/Compare 2 output  enable
: TIM4_CCER_CC1NP ( -- x addr ) 3 bit TIM4_CCER ; \ TIM4_CCER_CC1NP, Capture/Compare 1 output  Polarity
: TIM4_CCER_CC1P ( -- x addr ) 1 bit TIM4_CCER ; \ TIM4_CCER_CC1P, Capture/Compare 1 output  Polarity
: TIM4_CCER_CC1E ( -- x addr ) 0 bit TIM4_CCER ; \ TIM4_CCER_CC1E, Capture/Compare 1 output  enable
[then]

execute-defined? use-TIM4 defined? TIM4_CNT_CNT_H not and [if]
\ TIM4_CNT (read-write) Reset:0x00000000
: TIM4_CNT_CNT_H ( %bbbbbbbbbbbbbbbb -- x addr ) 16 lshift TIM4_CNT ; \ TIM4_CNT_CNT_H, High counter value TIM2  only
: TIM4_CNT_CNT_L ( %bbbbbbbbbbbbbbbb -- x addr ) TIM4_CNT ; \ TIM4_CNT_CNT_L, Low counter value
[then]

defined? use-TIM4 defined? TIM4_PSC_PSC not and [if]
\ TIM4_PSC (read-write) Reset:0x0000
: TIM4_PSC_PSC ( %bbbbbbbbbbbbbbbb -- x addr ) TIM4_PSC ; \ TIM4_PSC_PSC, Prescaler value
[then]

execute-defined? use-TIM4 defined? TIM4_ARR_ARR_H not and [if]
\ TIM4_ARR (read-write) Reset:0x00000000
: TIM4_ARR_ARR_H ( %bbbbbbbbbbbbbbbb -- x addr ) 16 lshift TIM4_ARR ; \ TIM4_ARR_ARR_H, High Auto-reload value TIM2  only
: TIM4_ARR_ARR_L ( %bbbbbbbbbbbbbbbb -- x addr ) TIM4_ARR ; \ TIM4_ARR_ARR_L, Low Auto-reload value
[then]

defined? use-TIM4 defined? TIM4_CCR1_CCR1_H not and [if]
\ TIM4_CCR1 (read-write) Reset:0x00000000
: TIM4_CCR1_CCR1_H ( %bbbbbbbbbbbbbbbb -- x addr ) 16 lshift TIM4_CCR1 ; \ TIM4_CCR1_CCR1_H, High Capture/Compare 1 value TIM2  only
: TIM4_CCR1_CCR1_L ( %bbbbbbbbbbbbbbbb -- x addr ) TIM4_CCR1 ; \ TIM4_CCR1_CCR1_L, Low Capture/Compare 1  value
[then]

execute-defined? use-TIM4 defined? TIM4_CCR2_CCR2_H not and [if]
\ TIM4_CCR2 (read-write) Reset:0x00000000
: TIM4_CCR2_CCR2_H ( %bbbbbbbbbbbbbbbb -- x addr ) 16 lshift TIM4_CCR2 ; \ TIM4_CCR2_CCR2_H, High Capture/Compare 2 value TIM2  only
: TIM4_CCR2_CCR2_L ( %bbbbbbbbbbbbbbbb -- x addr ) TIM4_CCR2 ; \ TIM4_CCR2_CCR2_L, Low Capture/Compare 2  value
[then]

defined? use-TIM4 defined? TIM4_CCR3_CCR3_H not and [if]
\ TIM4_CCR3 (read-write) Reset:0x00000000
: TIM4_CCR3_CCR3_H ( %bbbbbbbbbbbbbbbb -- x addr ) 16 lshift TIM4_CCR3 ; \ TIM4_CCR3_CCR3_H, High Capture/Compare value TIM2  only
: TIM4_CCR3_CCR3_L ( %bbbbbbbbbbbbbbbb -- x addr ) TIM4_CCR3 ; \ TIM4_CCR3_CCR3_L, Low Capture/Compare value
[then]

execute-defined? use-TIM4 defined? TIM4_CCR4_CCR4_H not and [if]
\ TIM4_CCR4 (read-write) Reset:0x00000000
: TIM4_CCR4_CCR4_H ( %bbbbbbbbbbbbbbbb -- x addr ) 16 lshift TIM4_CCR4 ; \ TIM4_CCR4_CCR4_H, High Capture/Compare value TIM2  only
: TIM4_CCR4_CCR4_L ( %bbbbbbbbbbbbbbbb -- x addr ) TIM4_CCR4 ; \ TIM4_CCR4_CCR4_L, Low Capture/Compare value
[then]

defined? use-TIM4 defined? TIM4_DCR_DBL not and [if]
\ TIM4_DCR (read-write) Reset:0x0000
: TIM4_DCR_DBL ( %bbbbb -- x addr ) 8 lshift TIM4_DCR ; \ TIM4_DCR_DBL, DMA burst length
: TIM4_DCR_DBA ( %bbbbb -- x addr ) TIM4_DCR ; \ TIM4_DCR_DBA, DMA base address
[then]

execute-defined? use-TIM4 defined? TIM4_DMAR_DMAB not and [if]
\ TIM4_DMAR (read-write) Reset:0x0000
: TIM4_DMAR_DMAB ( %bbbbbbbbbbbbbbbb -- x addr ) TIM4_DMAR ; \ TIM4_DMAR_DMAB, DMA register for burst  accesses
[then]

defined? use-TIM4 defined? TIM4_OR_ETR_RMP not and [if]
\ TIM4_OR (read-write) Reset:0x0000
: TIM4_OR_ETR_RMP ( %bbb -- x addr ) TIM4_OR ; \ TIM4_OR_ETR_RMP, Timer2 ETR remap
: TIM4_OR_TI4_RMP ( %bb -- x addr ) 3 lshift TIM4_OR ; \ TIM4_OR_TI4_RMP, Internal trigger
[then]

execute-defined? use-TIM5 defined? TIM5_CR1_CKD not and [if]
\ TIM5_CR1 (read-write) Reset:0x0000
: TIM5_CR1_CKD ( %bb -- x addr ) 8 lshift TIM5_CR1 ; \ TIM5_CR1_CKD, Clock division
: TIM5_CR1_ARPE ( -- x addr ) 7 bit TIM5_CR1 ; \ TIM5_CR1_ARPE, Auto-reload preload enable
: TIM5_CR1_CMS ( %bb -- x addr ) 5 lshift TIM5_CR1 ; \ TIM5_CR1_CMS, Center-aligned mode  selection
: TIM5_CR1_DIR ( -- x addr ) 4 bit TIM5_CR1 ; \ TIM5_CR1_DIR, Direction
: TIM5_CR1_OPM ( -- x addr ) 3 bit TIM5_CR1 ; \ TIM5_CR1_OPM, One-pulse mode
: TIM5_CR1_URS ( -- x addr ) 2 bit TIM5_CR1 ; \ TIM5_CR1_URS, Update request source
: TIM5_CR1_UDIS ( -- x addr ) 1 bit TIM5_CR1 ; \ TIM5_CR1_UDIS, Update disable
: TIM5_CR1_CEN ( -- x addr ) 0 bit TIM5_CR1 ; \ TIM5_CR1_CEN, Counter enable
[then]

defined? use-TIM5 defined? TIM5_CR2_TI1S not and [if]
\ TIM5_CR2 (read-write) Reset:0x0000
: TIM5_CR2_TI1S ( -- x addr ) 7 bit TIM5_CR2 ; \ TIM5_CR2_TI1S, TI1 selection
: TIM5_CR2_MMS ( %bbb -- x addr ) 4 lshift TIM5_CR2 ; \ TIM5_CR2_MMS, Master mode selection
: TIM5_CR2_CCDS ( -- x addr ) 3 bit TIM5_CR2 ; \ TIM5_CR2_CCDS, Capture/compare DMA  selection
[then]

execute-defined? use-TIM5 defined? TIM5_SMCR_ETP not and [if]
\ TIM5_SMCR (read-write) Reset:0x0000
: TIM5_SMCR_ETP ( -- x addr ) 15 bit TIM5_SMCR ; \ TIM5_SMCR_ETP, External trigger polarity
: TIM5_SMCR_ECE ( -- x addr ) 14 bit TIM5_SMCR ; \ TIM5_SMCR_ECE, External clock enable
: TIM5_SMCR_ETPS ( %bb -- x addr ) 12 lshift TIM5_SMCR ; \ TIM5_SMCR_ETPS, External trigger prescaler
: TIM5_SMCR_ETF ( %bbbb -- x addr ) 8 lshift TIM5_SMCR ; \ TIM5_SMCR_ETF, External trigger filter
: TIM5_SMCR_MSM ( -- x addr ) 7 bit TIM5_SMCR ; \ TIM5_SMCR_MSM, Master/Slave mode
: TIM5_SMCR_TS ( %bbb -- x addr ) 4 lshift TIM5_SMCR ; \ TIM5_SMCR_TS, Trigger selection
: TIM5_SMCR_SMS ( %bbb -- x addr ) TIM5_SMCR ; \ TIM5_SMCR_SMS, Slave mode selection
[then]

defined? use-TIM5 defined? TIM5_DIER_TDE not and [if]
\ TIM5_DIER (read-write) Reset:0x0000
: TIM5_DIER_TDE ( -- x addr ) 14 bit TIM5_DIER ; \ TIM5_DIER_TDE, Trigger DMA request enable
: TIM5_DIER_COMDE ( -- x addr ) 13 bit TIM5_DIER ; \ TIM5_DIER_COMDE, Reserved
: TIM5_DIER_CC4DE ( -- x addr ) 12 bit TIM5_DIER ; \ TIM5_DIER_CC4DE, Capture/Compare 4 DMA request  enable
: TIM5_DIER_CC3DE ( -- x addr ) 11 bit TIM5_DIER ; \ TIM5_DIER_CC3DE, Capture/Compare 3 DMA request  enable
: TIM5_DIER_CC2DE ( -- x addr ) 10 bit TIM5_DIER ; \ TIM5_DIER_CC2DE, Capture/Compare 2 DMA request  enable
: TIM5_DIER_CC1DE ( -- x addr ) 9 bit TIM5_DIER ; \ TIM5_DIER_CC1DE, Capture/Compare 1 DMA request  enable
: TIM5_DIER_UDE ( -- x addr ) 8 bit TIM5_DIER ; \ TIM5_DIER_UDE, Update DMA request enable
: TIM5_DIER_TIE ( -- x addr ) 6 bit TIM5_DIER ; \ TIM5_DIER_TIE, Trigger interrupt enable
: TIM5_DIER_CC4IE ( -- x addr ) 4 bit TIM5_DIER ; \ TIM5_DIER_CC4IE, Capture/Compare 4 interrupt  enable
: TIM5_DIER_CC3IE ( -- x addr ) 3 bit TIM5_DIER ; \ TIM5_DIER_CC3IE, Capture/Compare 3 interrupt  enable
: TIM5_DIER_CC2IE ( -- x addr ) 2 bit TIM5_DIER ; \ TIM5_DIER_CC2IE, Capture/Compare 2 interrupt  enable
: TIM5_DIER_CC1IE ( -- x addr ) 1 bit TIM5_DIER ; \ TIM5_DIER_CC1IE, Capture/Compare 1 interrupt  enable
: TIM5_DIER_UIE ( -- x addr ) 0 bit TIM5_DIER ; \ TIM5_DIER_UIE, Update interrupt enable
[then]

execute-defined? use-TIM5 defined? TIM5_SR_CC4OF not and [if]
\ TIM5_SR (read-write) Reset:0x0000
: TIM5_SR_CC4OF ( -- x addr ) 12 bit TIM5_SR ; \ TIM5_SR_CC4OF, Capture/Compare 4 overcapture  flag
: TIM5_SR_CC3OF ( -- x addr ) 11 bit TIM5_SR ; \ TIM5_SR_CC3OF, Capture/Compare 3 overcapture  flag
: TIM5_SR_CC2OF ( -- x addr ) 10 bit TIM5_SR ; \ TIM5_SR_CC2OF, Capture/compare 2 overcapture  flag
: TIM5_SR_CC1OF ( -- x addr ) 9 bit TIM5_SR ; \ TIM5_SR_CC1OF, Capture/Compare 1 overcapture  flag
: TIM5_SR_TIF ( -- x addr ) 6 bit TIM5_SR ; \ TIM5_SR_TIF, Trigger interrupt flag
: TIM5_SR_CC4IF ( -- x addr ) 4 bit TIM5_SR ; \ TIM5_SR_CC4IF, Capture/Compare 4 interrupt  flag
: TIM5_SR_CC3IF ( -- x addr ) 3 bit TIM5_SR ; \ TIM5_SR_CC3IF, Capture/Compare 3 interrupt  flag
: TIM5_SR_CC2IF ( -- x addr ) 2 bit TIM5_SR ; \ TIM5_SR_CC2IF, Capture/Compare 2 interrupt  flag
: TIM5_SR_CC1IF ( -- x addr ) 1 bit TIM5_SR ; \ TIM5_SR_CC1IF, Capture/compare 1 interrupt  flag
: TIM5_SR_UIF ( -- x addr ) 0 bit TIM5_SR ; \ TIM5_SR_UIF, Update interrupt flag
[then]

defined? use-TIM5 defined? TIM5_EGR_TG not and [if]
\ TIM5_EGR (write-only) Reset:0x0000
: TIM5_EGR_TG ( -- x addr ) 6 bit TIM5_EGR ; \ TIM5_EGR_TG, Trigger generation
: TIM5_EGR_CC4G ( -- x addr ) 4 bit TIM5_EGR ; \ TIM5_EGR_CC4G, Capture/compare 4  generation
: TIM5_EGR_CC3G ( -- x addr ) 3 bit TIM5_EGR ; \ TIM5_EGR_CC3G, Capture/compare 3  generation
: TIM5_EGR_CC2G ( -- x addr ) 2 bit TIM5_EGR ; \ TIM5_EGR_CC2G, Capture/compare 2  generation
: TIM5_EGR_CC1G ( -- x addr ) 1 bit TIM5_EGR ; \ TIM5_EGR_CC1G, Capture/compare 1  generation
: TIM5_EGR_UG ( -- x addr ) 0 bit TIM5_EGR ; \ TIM5_EGR_UG, Update generation
[then]

execute-defined? use-TIM5 defined? TIM5_CCMR1_Output_OC2CE not and [if]
\ TIM5_CCMR1_Output (read-write) Reset:0x00000000
: TIM5_CCMR1_Output_OC2CE ( -- x addr ) 15 bit TIM5_CCMR1_Output ; \ TIM5_CCMR1_Output_OC2CE, Output compare 2 clear  enable
: TIM5_CCMR1_Output_OC2M ( %bbb -- x addr ) 12 lshift TIM5_CCMR1_Output ; \ TIM5_CCMR1_Output_OC2M, Output compare 2 mode
: TIM5_CCMR1_Output_OC2PE ( -- x addr ) 11 bit TIM5_CCMR1_Output ; \ TIM5_CCMR1_Output_OC2PE, Output compare 2 preload  enable
: TIM5_CCMR1_Output_OC2FE ( -- x addr ) 10 bit TIM5_CCMR1_Output ; \ TIM5_CCMR1_Output_OC2FE, Output compare 2 fast  enable
: TIM5_CCMR1_Output_CC2S ( %bb -- x addr ) 8 lshift TIM5_CCMR1_Output ; \ TIM5_CCMR1_Output_CC2S, Capture/Compare 2  selection
: TIM5_CCMR1_Output_OC1CE ( -- x addr ) 7 bit TIM5_CCMR1_Output ; \ TIM5_CCMR1_Output_OC1CE, Output compare 1 clear  enable
: TIM5_CCMR1_Output_OC1M ( %bbb -- x addr ) 4 lshift TIM5_CCMR1_Output ; \ TIM5_CCMR1_Output_OC1M, Output compare 1 mode
: TIM5_CCMR1_Output_OC1PE ( -- x addr ) 3 bit TIM5_CCMR1_Output ; \ TIM5_CCMR1_Output_OC1PE, Output compare 1 preload  enable
: TIM5_CCMR1_Output_OC1FE ( -- x addr ) 2 bit TIM5_CCMR1_Output ; \ TIM5_CCMR1_Output_OC1FE, Output compare 1 fast  enable
: TIM5_CCMR1_Output_CC1S ( %bb -- x addr ) TIM5_CCMR1_Output ; \ TIM5_CCMR1_Output_CC1S, Capture/Compare 1  selection
[then]

defined? use-TIM5 defined? TIM5_CCMR1_Input_IC2F not and [if]
\ TIM5_CCMR1_Input (read-write) Reset:0x00000000
: TIM5_CCMR1_Input_IC2F ( %bbbb -- x addr ) 12 lshift TIM5_CCMR1_Input ; \ TIM5_CCMR1_Input_IC2F, Input capture 2 filter
: TIM5_CCMR1_Input_IC2PSC ( %bb -- x addr ) 10 lshift TIM5_CCMR1_Input ; \ TIM5_CCMR1_Input_IC2PSC, Input capture 2 prescaler
: TIM5_CCMR1_Input_CC2S ( %bb -- x addr ) 8 lshift TIM5_CCMR1_Input ; \ TIM5_CCMR1_Input_CC2S, Capture/compare 2  selection
: TIM5_CCMR1_Input_IC1F ( %bbbb -- x addr ) 4 lshift TIM5_CCMR1_Input ; \ TIM5_CCMR1_Input_IC1F, Input capture 1 filter
: TIM5_CCMR1_Input_IC1PSC ( %bb -- x addr ) 2 lshift TIM5_CCMR1_Input ; \ TIM5_CCMR1_Input_IC1PSC, Input capture 1 prescaler
: TIM5_CCMR1_Input_CC1S ( %bb -- x addr ) TIM5_CCMR1_Input ; \ TIM5_CCMR1_Input_CC1S, Capture/Compare 1  selection
[then]

execute-defined? use-TIM5 defined? TIM5_CCMR2_Output_OC4CE not and [if]
\ TIM5_CCMR2_Output (read-write) Reset:0x00000000
: TIM5_CCMR2_Output_OC4CE ( -- x addr ) 15 bit TIM5_CCMR2_Output ; \ TIM5_CCMR2_Output_OC4CE, Output compare 4 clear  enable
: TIM5_CCMR2_Output_OC4M ( %bbb -- x addr ) 12 lshift TIM5_CCMR2_Output ; \ TIM5_CCMR2_Output_OC4M, Output compare 4 mode
: TIM5_CCMR2_Output_OC4PE ( -- x addr ) 11 bit TIM5_CCMR2_Output ; \ TIM5_CCMR2_Output_OC4PE, Output compare 4 preload  enable
: TIM5_CCMR2_Output_OC4FE ( -- x addr ) 10 bit TIM5_CCMR2_Output ; \ TIM5_CCMR2_Output_OC4FE, Output compare 4 fast  enable
: TIM5_CCMR2_Output_CC4S ( %bb -- x addr ) 8 lshift TIM5_CCMR2_Output ; \ TIM5_CCMR2_Output_CC4S, Capture/Compare 4  selection
: TIM5_CCMR2_Output_OC3CE ( -- x addr ) 7 bit TIM5_CCMR2_Output ; \ TIM5_CCMR2_Output_OC3CE, Output compare 3 clear  enable
: TIM5_CCMR2_Output_OC3M ( %bbb -- x addr ) 4 lshift TIM5_CCMR2_Output ; \ TIM5_CCMR2_Output_OC3M, Output compare 3 mode
: TIM5_CCMR2_Output_OC3PE ( -- x addr ) 3 bit TIM5_CCMR2_Output ; \ TIM5_CCMR2_Output_OC3PE, Output compare 3 preload  enable
: TIM5_CCMR2_Output_OC3FE ( -- x addr ) 2 bit TIM5_CCMR2_Output ; \ TIM5_CCMR2_Output_OC3FE, Output compare 3 fast  enable
: TIM5_CCMR2_Output_CC3S ( %bb -- x addr ) TIM5_CCMR2_Output ; \ TIM5_CCMR2_Output_CC3S, Capture/Compare 3  selection
[then]

defined? use-TIM5 defined? TIM5_CCMR2_Input_IC4F not and [if]
\ TIM5_CCMR2_Input (read-write) Reset:0x00000000
: TIM5_CCMR2_Input_IC4F ( %bbbb -- x addr ) 12 lshift TIM5_CCMR2_Input ; \ TIM5_CCMR2_Input_IC4F, Input capture 4 filter
: TIM5_CCMR2_Input_IC4PSC ( %bb -- x addr ) 10 lshift TIM5_CCMR2_Input ; \ TIM5_CCMR2_Input_IC4PSC, Input capture 4 prescaler
: TIM5_CCMR2_Input_CC4S ( %bb -- x addr ) 8 lshift TIM5_CCMR2_Input ; \ TIM5_CCMR2_Input_CC4S, Capture/Compare 4  selection
: TIM5_CCMR2_Input_IC3F ( %bbbb -- x addr ) 4 lshift TIM5_CCMR2_Input ; \ TIM5_CCMR2_Input_IC3F, Input capture 3 filter
: TIM5_CCMR2_Input_IC3PSC ( %bb -- x addr ) 2 lshift TIM5_CCMR2_Input ; \ TIM5_CCMR2_Input_IC3PSC, Input capture 3 prescaler
: TIM5_CCMR2_Input_CC3S ( %bb -- x addr ) TIM5_CCMR2_Input ; \ TIM5_CCMR2_Input_CC3S, Capture/Compare 3  selection
[then]

execute-defined? use-TIM5 defined? TIM5_CCER_CC4NP not and [if]
\ TIM5_CCER (read-write) Reset:0x0000
: TIM5_CCER_CC4NP ( -- x addr ) 15 bit TIM5_CCER ; \ TIM5_CCER_CC4NP, Capture/Compare 4 output  Polarity
: TIM5_CCER_CC4P ( -- x addr ) 13 bit TIM5_CCER ; \ TIM5_CCER_CC4P, Capture/Compare 3 output  Polarity
: TIM5_CCER_CC4E ( -- x addr ) 12 bit TIM5_CCER ; \ TIM5_CCER_CC4E, Capture/Compare 4 output  enable
: TIM5_CCER_CC3NP ( -- x addr ) 11 bit TIM5_CCER ; \ TIM5_CCER_CC3NP, Capture/Compare 3 output  Polarity
: TIM5_CCER_CC3P ( -- x addr ) 9 bit TIM5_CCER ; \ TIM5_CCER_CC3P, Capture/Compare 3 output  Polarity
: TIM5_CCER_CC3E ( -- x addr ) 8 bit TIM5_CCER ; \ TIM5_CCER_CC3E, Capture/Compare 3 output  enable
: TIM5_CCER_CC2NP ( -- x addr ) 7 bit TIM5_CCER ; \ TIM5_CCER_CC2NP, Capture/Compare 2 output  Polarity
: TIM5_CCER_CC2P ( -- x addr ) 5 bit TIM5_CCER ; \ TIM5_CCER_CC2P, Capture/Compare 2 output  Polarity
: TIM5_CCER_CC2E ( -- x addr ) 4 bit TIM5_CCER ; \ TIM5_CCER_CC2E, Capture/Compare 2 output  enable
: TIM5_CCER_CC1NP ( -- x addr ) 3 bit TIM5_CCER ; \ TIM5_CCER_CC1NP, Capture/Compare 1 output  Polarity
: TIM5_CCER_CC1P ( -- x addr ) 1 bit TIM5_CCER ; \ TIM5_CCER_CC1P, Capture/Compare 1 output  Polarity
: TIM5_CCER_CC1E ( -- x addr ) 0 bit TIM5_CCER ; \ TIM5_CCER_CC1E, Capture/Compare 1 output  enable
[then]

defined? use-TIM5 defined? TIM5_CNT_CNT_H not and [if]
\ TIM5_CNT (read-write) Reset:0x00000000
: TIM5_CNT_CNT_H ( %bbbbbbbbbbbbbbbb -- x addr ) 16 lshift TIM5_CNT ; \ TIM5_CNT_CNT_H, High counter value TIM2  only
: TIM5_CNT_CNT_L ( %bbbbbbbbbbbbbbbb -- x addr ) TIM5_CNT ; \ TIM5_CNT_CNT_L, Low counter value
[then]

execute-defined? use-TIM5 defined? TIM5_PSC_PSC not and [if]
\ TIM5_PSC (read-write) Reset:0x0000
: TIM5_PSC_PSC ( %bbbbbbbbbbbbbbbb -- x addr ) TIM5_PSC ; \ TIM5_PSC_PSC, Prescaler value
[then]

defined? use-TIM5 defined? TIM5_ARR_ARR_H not and [if]
\ TIM5_ARR (read-write) Reset:0x00000000
: TIM5_ARR_ARR_H ( %bbbbbbbbbbbbbbbb -- x addr ) 16 lshift TIM5_ARR ; \ TIM5_ARR_ARR_H, High Auto-reload value TIM2  only
: TIM5_ARR_ARR_L ( %bbbbbbbbbbbbbbbb -- x addr ) TIM5_ARR ; \ TIM5_ARR_ARR_L, Low Auto-reload value
[then]

execute-defined? use-TIM5 defined? TIM5_CCR1_CCR1_H not and [if]
\ TIM5_CCR1 (read-write) Reset:0x00000000
: TIM5_CCR1_CCR1_H ( %bbbbbbbbbbbbbbbb -- x addr ) 16 lshift TIM5_CCR1 ; \ TIM5_CCR1_CCR1_H, High Capture/Compare 1 value TIM2  only
: TIM5_CCR1_CCR1_L ( %bbbbbbbbbbbbbbbb -- x addr ) TIM5_CCR1 ; \ TIM5_CCR1_CCR1_L, Low Capture/Compare 1  value
[then]

defined? use-TIM5 defined? TIM5_CCR2_CCR2_H not and [if]
\ TIM5_CCR2 (read-write) Reset:0x00000000
: TIM5_CCR2_CCR2_H ( %bbbbbbbbbbbbbbbb -- x addr ) 16 lshift TIM5_CCR2 ; \ TIM5_CCR2_CCR2_H, High Capture/Compare 2 value TIM2  only
: TIM5_CCR2_CCR2_L ( %bbbbbbbbbbbbbbbb -- x addr ) TIM5_CCR2 ; \ TIM5_CCR2_CCR2_L, Low Capture/Compare 2  value
[then]

execute-defined? use-TIM5 defined? TIM5_CCR3_CCR3_H not and [if]
\ TIM5_CCR3 (read-write) Reset:0x00000000
: TIM5_CCR3_CCR3_H ( %bbbbbbbbbbbbbbbb -- x addr ) 16 lshift TIM5_CCR3 ; \ TIM5_CCR3_CCR3_H, High Capture/Compare value TIM2  only
: TIM5_CCR3_CCR3_L ( %bbbbbbbbbbbbbbbb -- x addr ) TIM5_CCR3 ; \ TIM5_CCR3_CCR3_L, Low Capture/Compare value
[then]

defined? use-TIM5 defined? TIM5_CCR4_CCR4_H not and [if]
\ TIM5_CCR4 (read-write) Reset:0x00000000
: TIM5_CCR4_CCR4_H ( %bbbbbbbbbbbbbbbb -- x addr ) 16 lshift TIM5_CCR4 ; \ TIM5_CCR4_CCR4_H, High Capture/Compare value TIM2  only
: TIM5_CCR4_CCR4_L ( %bbbbbbbbbbbbbbbb -- x addr ) TIM5_CCR4 ; \ TIM5_CCR4_CCR4_L, Low Capture/Compare value
[then]

execute-defined? use-TIM5 defined? TIM5_DCR_DBL not and [if]
\ TIM5_DCR (read-write) Reset:0x0000
: TIM5_DCR_DBL ( %bbbbb -- x addr ) 8 lshift TIM5_DCR ; \ TIM5_DCR_DBL, DMA burst length
: TIM5_DCR_DBA ( %bbbbb -- x addr ) TIM5_DCR ; \ TIM5_DCR_DBA, DMA base address
[then]

defined? use-TIM5 defined? TIM5_DMAR_DMAB not and [if]
\ TIM5_DMAR (read-write) Reset:0x0000
: TIM5_DMAR_DMAB ( %bbbbbbbbbbbbbbbb -- x addr ) TIM5_DMAR ; \ TIM5_DMAR_DMAB, DMA register for burst  accesses
[then]

execute-defined? use-TIM5 defined? TIM5_OR_ETR_RMP not and [if]
\ TIM5_OR (read-write) Reset:0x0000
: TIM5_OR_ETR_RMP ( %bbb -- x addr ) TIM5_OR ; \ TIM5_OR_ETR_RMP, Timer2 ETR remap
: TIM5_OR_TI4_RMP ( %bb -- x addr ) 3 lshift TIM5_OR ; \ TIM5_OR_TI4_RMP, Internal trigger
[then]

defined? use-TIM1 defined? TIM1_CR1_CKD not and [if]
\ TIM1_CR1 (read-write) Reset:0x0000
: TIM1_CR1_CKD ( %bb -- x addr ) 8 lshift TIM1_CR1 ; \ TIM1_CR1_CKD, Clock division
: TIM1_CR1_ARPE ( -- x addr ) 7 bit TIM1_CR1 ; \ TIM1_CR1_ARPE, Auto-reload preload enable
: TIM1_CR1_CMS ( %bb -- x addr ) 5 lshift TIM1_CR1 ; \ TIM1_CR1_CMS, Center-aligned mode  selection
: TIM1_CR1_DIR ( -- x addr ) 4 bit TIM1_CR1 ; \ TIM1_CR1_DIR, Direction
: TIM1_CR1_OPM ( -- x addr ) 3 bit TIM1_CR1 ; \ TIM1_CR1_OPM, One-pulse mode
: TIM1_CR1_URS ( -- x addr ) 2 bit TIM1_CR1 ; \ TIM1_CR1_URS, Update request source
: TIM1_CR1_UDIS ( -- x addr ) 1 bit TIM1_CR1 ; \ TIM1_CR1_UDIS, Update disable
: TIM1_CR1_CEN ( -- x addr ) 0 bit TIM1_CR1 ; \ TIM1_CR1_CEN, Counter enable
[then]

execute-defined? use-TIM1 defined? TIM1_CR2_OIS4 not and [if]
\ TIM1_CR2 (read-write) Reset:0x0000
: TIM1_CR2_OIS4 ( -- x addr ) 14 bit TIM1_CR2 ; \ TIM1_CR2_OIS4, Output Idle state 4
: TIM1_CR2_OIS3N ( -- x addr ) 13 bit TIM1_CR2 ; \ TIM1_CR2_OIS3N, Output Idle state 3
: TIM1_CR2_OIS3 ( -- x addr ) 12 bit TIM1_CR2 ; \ TIM1_CR2_OIS3, Output Idle state 3
: TIM1_CR2_OIS2N ( -- x addr ) 11 bit TIM1_CR2 ; \ TIM1_CR2_OIS2N, Output Idle state 2
: TIM1_CR2_OIS2 ( -- x addr ) 10 bit TIM1_CR2 ; \ TIM1_CR2_OIS2, Output Idle state 2
: TIM1_CR2_OIS1N ( -- x addr ) 9 bit TIM1_CR2 ; \ TIM1_CR2_OIS1N, Output Idle state 1
: TIM1_CR2_OIS1 ( -- x addr ) 8 bit TIM1_CR2 ; \ TIM1_CR2_OIS1, Output Idle state 1
: TIM1_CR2_TI1S ( -- x addr ) 7 bit TIM1_CR2 ; \ TIM1_CR2_TI1S, TI1 selection
: TIM1_CR2_MMS ( %bbb -- x addr ) 4 lshift TIM1_CR2 ; \ TIM1_CR2_MMS, Master mode selection
: TIM1_CR2_CCDS ( -- x addr ) 3 bit TIM1_CR2 ; \ TIM1_CR2_CCDS, Capture/compare DMA  selection
: TIM1_CR2_CCUS ( -- x addr ) 2 bit TIM1_CR2 ; \ TIM1_CR2_CCUS, Capture/compare control update  selection
: TIM1_CR2_CCPC ( -- x addr ) 0 bit TIM1_CR2 ; \ TIM1_CR2_CCPC, Capture/compare preloaded  control
[then]

defined? use-TIM1 defined? TIM1_SMCR_ETP not and [if]
\ TIM1_SMCR (read-write) Reset:0x0000
: TIM1_SMCR_ETP ( -- x addr ) 15 bit TIM1_SMCR ; \ TIM1_SMCR_ETP, External trigger polarity
: TIM1_SMCR_ECE ( -- x addr ) 14 bit TIM1_SMCR ; \ TIM1_SMCR_ECE, External clock enable
: TIM1_SMCR_ETPS ( %bb -- x addr ) 12 lshift TIM1_SMCR ; \ TIM1_SMCR_ETPS, External trigger prescaler
: TIM1_SMCR_ETF ( %bbbb -- x addr ) 8 lshift TIM1_SMCR ; \ TIM1_SMCR_ETF, External trigger filter
: TIM1_SMCR_MSM ( -- x addr ) 7 bit TIM1_SMCR ; \ TIM1_SMCR_MSM, Master/Slave mode
: TIM1_SMCR_TS ( %bbb -- x addr ) 4 lshift TIM1_SMCR ; \ TIM1_SMCR_TS, Trigger selection
: TIM1_SMCR_SMS ( %bbb -- x addr ) TIM1_SMCR ; \ TIM1_SMCR_SMS, Slave mode selection
[then]

execute-defined? use-TIM1 defined? TIM1_DIER_TDE not and [if]
\ TIM1_DIER (read-write) Reset:0x0000
: TIM1_DIER_TDE ( -- x addr ) 14 bit TIM1_DIER ; \ TIM1_DIER_TDE, Trigger DMA request enable
: TIM1_DIER_COMDE ( -- x addr ) 13 bit TIM1_DIER ; \ TIM1_DIER_COMDE, COM DMA request enable
: TIM1_DIER_CC4DE ( -- x addr ) 12 bit TIM1_DIER ; \ TIM1_DIER_CC4DE, Capture/Compare 4 DMA request  enable
: TIM1_DIER_CC3DE ( -- x addr ) 11 bit TIM1_DIER ; \ TIM1_DIER_CC3DE, Capture/Compare 3 DMA request  enable
: TIM1_DIER_CC2DE ( -- x addr ) 10 bit TIM1_DIER ; \ TIM1_DIER_CC2DE, Capture/Compare 2 DMA request  enable
: TIM1_DIER_CC1DE ( -- x addr ) 9 bit TIM1_DIER ; \ TIM1_DIER_CC1DE, Capture/Compare 1 DMA request  enable
: TIM1_DIER_UDE ( -- x addr ) 8 bit TIM1_DIER ; \ TIM1_DIER_UDE, Update DMA request enable
: TIM1_DIER_TIE ( -- x addr ) 6 bit TIM1_DIER ; \ TIM1_DIER_TIE, Trigger interrupt enable
: TIM1_DIER_CC4IE ( -- x addr ) 4 bit TIM1_DIER ; \ TIM1_DIER_CC4IE, Capture/Compare 4 interrupt  enable
: TIM1_DIER_CC3IE ( -- x addr ) 3 bit TIM1_DIER ; \ TIM1_DIER_CC3IE, Capture/Compare 3 interrupt  enable
: TIM1_DIER_CC2IE ( -- x addr ) 2 bit TIM1_DIER ; \ TIM1_DIER_CC2IE, Capture/Compare 2 interrupt  enable
: TIM1_DIER_CC1IE ( -- x addr ) 1 bit TIM1_DIER ; \ TIM1_DIER_CC1IE, Capture/Compare 1 interrupt  enable
: TIM1_DIER_UIE ( -- x addr ) 0 bit TIM1_DIER ; \ TIM1_DIER_UIE, Update interrupt enable
: TIM1_DIER_BIE ( -- x addr ) 7 bit TIM1_DIER ; \ TIM1_DIER_BIE, Break interrupt enable
: TIM1_DIER_COMIE ( -- x addr ) 5 bit TIM1_DIER ; \ TIM1_DIER_COMIE, COM interrupt enable
[then]

defined? use-TIM1 defined? TIM1_SR_CC4OF not and [if]
\ TIM1_SR (read-write) Reset:0x0000
: TIM1_SR_CC4OF ( -- x addr ) 12 bit TIM1_SR ; \ TIM1_SR_CC4OF, Capture/Compare 4 overcapture  flag
: TIM1_SR_CC3OF ( -- x addr ) 11 bit TIM1_SR ; \ TIM1_SR_CC3OF, Capture/Compare 3 overcapture  flag
: TIM1_SR_CC2OF ( -- x addr ) 10 bit TIM1_SR ; \ TIM1_SR_CC2OF, Capture/compare 2 overcapture  flag
: TIM1_SR_CC1OF ( -- x addr ) 9 bit TIM1_SR ; \ TIM1_SR_CC1OF, Capture/Compare 1 overcapture  flag
: TIM1_SR_BIF ( -- x addr ) 7 bit TIM1_SR ; \ TIM1_SR_BIF, Break interrupt flag
: TIM1_SR_TIF ( -- x addr ) 6 bit TIM1_SR ; \ TIM1_SR_TIF, Trigger interrupt flag
: TIM1_SR_COMIF ( -- x addr ) 5 bit TIM1_SR ; \ TIM1_SR_COMIF, COM interrupt flag
: TIM1_SR_CC4IF ( -- x addr ) 4 bit TIM1_SR ; \ TIM1_SR_CC4IF, Capture/Compare 4 interrupt  flag
: TIM1_SR_CC3IF ( -- x addr ) 3 bit TIM1_SR ; \ TIM1_SR_CC3IF, Capture/Compare 3 interrupt  flag
: TIM1_SR_CC2IF ( -- x addr ) 2 bit TIM1_SR ; \ TIM1_SR_CC2IF, Capture/Compare 2 interrupt  flag
: TIM1_SR_CC1IF ( -- x addr ) 1 bit TIM1_SR ; \ TIM1_SR_CC1IF, Capture/compare 1 interrupt  flag
: TIM1_SR_UIF ( -- x addr ) 0 bit TIM1_SR ; \ TIM1_SR_UIF, Update interrupt flag
[then]

execute-defined? use-TIM1 defined? TIM1_EGR_BG not and [if]
\ TIM1_EGR (write-only) Reset:0x0000
: TIM1_EGR_BG ( -- x addr ) 7 bit TIM1_EGR ; \ TIM1_EGR_BG, Break generation
: TIM1_EGR_TG ( -- x addr ) 6 bit TIM1_EGR ; \ TIM1_EGR_TG, Trigger generation
: TIM1_EGR_COMG ( -- x addr ) 5 bit TIM1_EGR ; \ TIM1_EGR_COMG, Capture/Compare control update  generation
: TIM1_EGR_CC4G ( -- x addr ) 4 bit TIM1_EGR ; \ TIM1_EGR_CC4G, Capture/compare 4  generation
: TIM1_EGR_CC3G ( -- x addr ) 3 bit TIM1_EGR ; \ TIM1_EGR_CC3G, Capture/compare 3  generation
: TIM1_EGR_CC2G ( -- x addr ) 2 bit TIM1_EGR ; \ TIM1_EGR_CC2G, Capture/compare 2  generation
: TIM1_EGR_CC1G ( -- x addr ) 1 bit TIM1_EGR ; \ TIM1_EGR_CC1G, Capture/compare 1  generation
: TIM1_EGR_UG ( -- x addr ) 0 bit TIM1_EGR ; \ TIM1_EGR_UG, Update generation
[then]

defined? use-TIM1 defined? TIM1_CCMR1_Output_OC2CE not and [if]
\ TIM1_CCMR1_Output (read-write) Reset:0x00000000
: TIM1_CCMR1_Output_OC2CE ( -- x addr ) 15 bit TIM1_CCMR1_Output ; \ TIM1_CCMR1_Output_OC2CE, Output Compare 2 clear  enable
: TIM1_CCMR1_Output_OC2M ( %bbb -- x addr ) 12 lshift TIM1_CCMR1_Output ; \ TIM1_CCMR1_Output_OC2M, Output Compare 2 mode
: TIM1_CCMR1_Output_OC2PE ( -- x addr ) 11 bit TIM1_CCMR1_Output ; \ TIM1_CCMR1_Output_OC2PE, Output Compare 2 preload  enable
: TIM1_CCMR1_Output_OC2FE ( -- x addr ) 10 bit TIM1_CCMR1_Output ; \ TIM1_CCMR1_Output_OC2FE, Output Compare 2 fast  enable
: TIM1_CCMR1_Output_CC2S ( %bb -- x addr ) 8 lshift TIM1_CCMR1_Output ; \ TIM1_CCMR1_Output_CC2S, Capture/Compare 2  selection
: TIM1_CCMR1_Output_OC1CE ( -- x addr ) 7 bit TIM1_CCMR1_Output ; \ TIM1_CCMR1_Output_OC1CE, Output Compare 1 clear  enable
: TIM1_CCMR1_Output_OC1M ( %bbb -- x addr ) 4 lshift TIM1_CCMR1_Output ; \ TIM1_CCMR1_Output_OC1M, Output Compare 1 mode
: TIM1_CCMR1_Output_OC1PE ( -- x addr ) 3 bit TIM1_CCMR1_Output ; \ TIM1_CCMR1_Output_OC1PE, Output Compare 1 preload  enable
: TIM1_CCMR1_Output_OC1FE ( -- x addr ) 2 bit TIM1_CCMR1_Output ; \ TIM1_CCMR1_Output_OC1FE, Output Compare 1 fast  enable
: TIM1_CCMR1_Output_CC1S ( %bb -- x addr ) TIM1_CCMR1_Output ; \ TIM1_CCMR1_Output_CC1S, Capture/Compare 1  selection
[then]

execute-defined? use-TIM1 defined? TIM1_CCMR1_Input_IC2F not and [if]
\ TIM1_CCMR1_Input (read-write) Reset:0x00000000
: TIM1_CCMR1_Input_IC2F ( %bbbb -- x addr ) 12 lshift TIM1_CCMR1_Input ; \ TIM1_CCMR1_Input_IC2F, Input capture 2 filter
: TIM1_CCMR1_Input_IC2PCS ( %bb -- x addr ) 10 lshift TIM1_CCMR1_Input ; \ TIM1_CCMR1_Input_IC2PCS, Input capture 2 prescaler
: TIM1_CCMR1_Input_CC2S ( %bb -- x addr ) 8 lshift TIM1_CCMR1_Input ; \ TIM1_CCMR1_Input_CC2S, Capture/Compare 2  selection
: TIM1_CCMR1_Input_IC1F ( %bbbb -- x addr ) 4 lshift TIM1_CCMR1_Input ; \ TIM1_CCMR1_Input_IC1F, Input capture 1 filter
: TIM1_CCMR1_Input_ICPCS ( %bb -- x addr ) 2 lshift TIM1_CCMR1_Input ; \ TIM1_CCMR1_Input_ICPCS, Input capture 1 prescaler
: TIM1_CCMR1_Input_CC1S ( %bb -- x addr ) TIM1_CCMR1_Input ; \ TIM1_CCMR1_Input_CC1S, Capture/Compare 1  selection
[then]

defined? use-TIM1 defined? TIM1_CCMR2_Output_OC4CE not and [if]
\ TIM1_CCMR2_Output (read-write) Reset:0x00000000
: TIM1_CCMR2_Output_OC4CE ( -- x addr ) 15 bit TIM1_CCMR2_Output ; \ TIM1_CCMR2_Output_OC4CE, Output compare 4 clear  enable
: TIM1_CCMR2_Output_OC4M ( %bbb -- x addr ) 12 lshift TIM1_CCMR2_Output ; \ TIM1_CCMR2_Output_OC4M, Output compare 4 mode
: TIM1_CCMR2_Output_OC4PE ( -- x addr ) 11 bit TIM1_CCMR2_Output ; \ TIM1_CCMR2_Output_OC4PE, Output compare 4 preload  enable
: TIM1_CCMR2_Output_OC4FE ( -- x addr ) 10 bit TIM1_CCMR2_Output ; \ TIM1_CCMR2_Output_OC4FE, Output compare 4 fast  enable
: TIM1_CCMR2_Output_CC4S ( %bb -- x addr ) 8 lshift TIM1_CCMR2_Output ; \ TIM1_CCMR2_Output_CC4S, Capture/Compare 4  selection
: TIM1_CCMR2_Output_OC3CE ( -- x addr ) 7 bit TIM1_CCMR2_Output ; \ TIM1_CCMR2_Output_OC3CE, Output compare 3 clear  enable
: TIM1_CCMR2_Output_OC3M ( %bbb -- x addr ) 4 lshift TIM1_CCMR2_Output ; \ TIM1_CCMR2_Output_OC3M, Output compare 3 mode
: TIM1_CCMR2_Output_OC3PE ( -- x addr ) 3 bit TIM1_CCMR2_Output ; \ TIM1_CCMR2_Output_OC3PE, Output compare 3 preload  enable
: TIM1_CCMR2_Output_OC3FE ( -- x addr ) 2 bit TIM1_CCMR2_Output ; \ TIM1_CCMR2_Output_OC3FE, Output compare 3 fast  enable
: TIM1_CCMR2_Output_CC3S ( %bb -- x addr ) TIM1_CCMR2_Output ; \ TIM1_CCMR2_Output_CC3S, Capture/Compare 3  selection
[then]

execute-defined? use-TIM1 defined? TIM1_CCMR2_Input_IC4F not and [if]
\ TIM1_CCMR2_Input (read-write) Reset:0x00000000
: TIM1_CCMR2_Input_IC4F ( %bbbb -- x addr ) 12 lshift TIM1_CCMR2_Input ; \ TIM1_CCMR2_Input_IC4F, Input capture 4 filter
: TIM1_CCMR2_Input_IC4PSC ( %bb -- x addr ) 10 lshift TIM1_CCMR2_Input ; \ TIM1_CCMR2_Input_IC4PSC, Input capture 4 prescaler
: TIM1_CCMR2_Input_CC4S ( %bb -- x addr ) 8 lshift TIM1_CCMR2_Input ; \ TIM1_CCMR2_Input_CC4S, Capture/Compare 4  selection
: TIM1_CCMR2_Input_IC3F ( %bbbb -- x addr ) 4 lshift TIM1_CCMR2_Input ; \ TIM1_CCMR2_Input_IC3F, Input capture 3 filter
: TIM1_CCMR2_Input_IC3PSC ( %bb -- x addr ) 2 lshift TIM1_CCMR2_Input ; \ TIM1_CCMR2_Input_IC3PSC, Input capture 3 prescaler
: TIM1_CCMR2_Input_CC3S ( %bb -- x addr ) TIM1_CCMR2_Input ; \ TIM1_CCMR2_Input_CC3S, Capture/compare 3  selection
[then]

defined? use-TIM1 defined? TIM1_CCER_CC4P not and [if]
\ TIM1_CCER (read-write) Reset:0x0000
: TIM1_CCER_CC4P ( -- x addr ) 13 bit TIM1_CCER ; \ TIM1_CCER_CC4P, Capture/Compare 3 output  Polarity
: TIM1_CCER_CC4E ( -- x addr ) 12 bit TIM1_CCER ; \ TIM1_CCER_CC4E, Capture/Compare 4 output  enable
: TIM1_CCER_CC3NP ( -- x addr ) 11 bit TIM1_CCER ; \ TIM1_CCER_CC3NP, Capture/Compare 3 output  Polarity
: TIM1_CCER_CC3NE ( -- x addr ) 10 bit TIM1_CCER ; \ TIM1_CCER_CC3NE, Capture/Compare 3 complementary output  enable
: TIM1_CCER_CC3P ( -- x addr ) 9 bit TIM1_CCER ; \ TIM1_CCER_CC3P, Capture/Compare 3 output  Polarity
: TIM1_CCER_CC3E ( -- x addr ) 8 bit TIM1_CCER ; \ TIM1_CCER_CC3E, Capture/Compare 3 output  enable
: TIM1_CCER_CC2NP ( -- x addr ) 7 bit TIM1_CCER ; \ TIM1_CCER_CC2NP, Capture/Compare 2 output  Polarity
: TIM1_CCER_CC2NE ( -- x addr ) 6 bit TIM1_CCER ; \ TIM1_CCER_CC2NE, Capture/Compare 2 complementary output  enable
: TIM1_CCER_CC2P ( -- x addr ) 5 bit TIM1_CCER ; \ TIM1_CCER_CC2P, Capture/Compare 2 output  Polarity
: TIM1_CCER_CC2E ( -- x addr ) 4 bit TIM1_CCER ; \ TIM1_CCER_CC2E, Capture/Compare 2 output  enable
: TIM1_CCER_CC1NP ( -- x addr ) 3 bit TIM1_CCER ; \ TIM1_CCER_CC1NP, Capture/Compare 1 output  Polarity
: TIM1_CCER_CC1NE ( -- x addr ) 2 bit TIM1_CCER ; \ TIM1_CCER_CC1NE, Capture/Compare 1 complementary output  enable
: TIM1_CCER_CC1P ( -- x addr ) 1 bit TIM1_CCER ; \ TIM1_CCER_CC1P, Capture/Compare 1 output  Polarity
: TIM1_CCER_CC1E ( -- x addr ) 0 bit TIM1_CCER ; \ TIM1_CCER_CC1E, Capture/Compare 1 output  enable
[then]

execute-defined? use-TIM1 defined? TIM1_CNT_CNT not and [if]
\ TIM1_CNT (read-write) Reset:0x00000000
: TIM1_CNT_CNT ( %bbbbbbbbbbbbbbbb -- x addr ) TIM1_CNT ; \ TIM1_CNT_CNT, counter value
[then]

defined? use-TIM1 defined? TIM1_PSC_PSC not and [if]
\ TIM1_PSC (read-write) Reset:0x0000
: TIM1_PSC_PSC ( %bbbbbbbbbbbbbbbb -- x addr ) TIM1_PSC ; \ TIM1_PSC_PSC, Prescaler value
[then]

execute-defined? use-TIM1 defined? TIM1_ARR_ARR not and [if]
\ TIM1_ARR (read-write) Reset:0x00000000
: TIM1_ARR_ARR ( %bbbbbbbbbbbbbbbb -- x addr ) TIM1_ARR ; \ TIM1_ARR_ARR, Auto-reload value
[then]

defined? use-TIM1 defined? TIM1_RCR_REP not and [if]
\ TIM1_RCR (read-write) Reset:0x0000
: TIM1_RCR_REP ( %bbbbbbbb -- x addr ) TIM1_RCR ; \ TIM1_RCR_REP, Repetition counter value
[then]

execute-defined? use-TIM1 defined? TIM1_CCR1_CCR1 not and [if]
\ TIM1_CCR1 (read-write) Reset:0x00000000
: TIM1_CCR1_CCR1 ( %bbbbbbbbbbbbbbbb -- x addr ) TIM1_CCR1 ; \ TIM1_CCR1_CCR1, Capture/Compare 1 value
[then]

defined? use-TIM1 defined? TIM1_CCR2_CCR2 not and [if]
\ TIM1_CCR2 (read-write) Reset:0x00000000
: TIM1_CCR2_CCR2 ( %bbbbbbbbbbbbbbbb -- x addr ) TIM1_CCR2 ; \ TIM1_CCR2_CCR2, Capture/Compare 2 value
[then]

execute-defined? use-TIM1 defined? TIM1_CCR3_CCR3 not and [if]
\ TIM1_CCR3 (read-write) Reset:0x00000000
: TIM1_CCR3_CCR3 ( %bbbbbbbbbbbbbbbb -- x addr ) TIM1_CCR3 ; \ TIM1_CCR3_CCR3, Capture/Compare value
[then]

defined? use-TIM1 defined? TIM1_CCR4_CCR4 not and [if]
\ TIM1_CCR4 (read-write) Reset:0x00000000
: TIM1_CCR4_CCR4 ( %bbbbbbbbbbbbbbbb -- x addr ) TIM1_CCR4 ; \ TIM1_CCR4_CCR4, Capture/Compare value
[then]

execute-defined? use-TIM1 defined? TIM1_BDTR_MOE not and [if]
\ TIM1_BDTR (read-write) Reset:0x0000
: TIM1_BDTR_MOE ( -- x addr ) 15 bit TIM1_BDTR ; \ TIM1_BDTR_MOE, Main output enable
: TIM1_BDTR_AOE ( -- x addr ) 14 bit TIM1_BDTR ; \ TIM1_BDTR_AOE, Automatic output enable
: TIM1_BDTR_BKP ( -- x addr ) 13 bit TIM1_BDTR ; \ TIM1_BDTR_BKP, Break polarity
: TIM1_BDTR_BKE ( -- x addr ) 12 bit TIM1_BDTR ; \ TIM1_BDTR_BKE, Break enable
: TIM1_BDTR_OSSR ( -- x addr ) 11 bit TIM1_BDTR ; \ TIM1_BDTR_OSSR, Off-state selection for Run  mode
: TIM1_BDTR_OSSI ( -- x addr ) 10 bit TIM1_BDTR ; \ TIM1_BDTR_OSSI, Off-state selection for Idle  mode
: TIM1_BDTR_LOCK ( %bb -- x addr ) 8 lshift TIM1_BDTR ; \ TIM1_BDTR_LOCK, Lock configuration
: TIM1_BDTR_DTG ( %bbbbbbbb -- x addr ) TIM1_BDTR ; \ TIM1_BDTR_DTG, Dead-time generator setup
[then]

defined? use-TIM1 defined? TIM1_DCR_DBL not and [if]
\ TIM1_DCR (read-write) Reset:0x0000
: TIM1_DCR_DBL ( %bbbbb -- x addr ) 8 lshift TIM1_DCR ; \ TIM1_DCR_DBL, DMA burst length
: TIM1_DCR_DBA ( %bbbbb -- x addr ) TIM1_DCR ; \ TIM1_DCR_DBA, DMA base address
[then]

execute-defined? use-TIM1 defined? TIM1_DMAR_DMAB not and [if]
\ TIM1_DMAR (read-write) Reset:0x0000
: TIM1_DMAR_DMAB ( %bbbbbbbbbbbbbbbb -- x addr ) TIM1_DMAR ; \ TIM1_DMAR_DMAB, DMA register for burst  accesses
[then]

defined? use-TIM1 defined? TIM1_OR1_ETR_ADC1_RMP not and [if]
\ TIM1_OR1 (read-write) Reset:0x0000
: TIM1_OR1_ETR_ADC1_RMP ( %bb -- x addr ) TIM1_OR1 ; \ TIM1_OR1_ETR_ADC1_RMP, External trigger remap on ADC1 analog  watchdog
: TIM1_OR1_ETR_ADC3_RMP ( %bb -- x addr ) 2 lshift TIM1_OR1 ; \ TIM1_OR1_ETR_ADC3_RMP, External trigger remap on ADC3 analog  watchdog
: TIM1_OR1_TI1_RMP ( -- x addr ) 4 bit TIM1_OR1 ; \ TIM1_OR1_TI1_RMP, Input Capture 1 remap
[then]

execute-defined? use-TIM1 defined? TIM1_CCMR3_Output_OC6M_bit3 not and [if]
\ TIM1_CCMR3_Output (read-write) Reset:0x00000000
: TIM1_CCMR3_Output_OC6M_bit3 ( -- x addr ) 24 bit TIM1_CCMR3_Output ; \ TIM1_CCMR3_Output_OC6M_bit3, Output Compare 6 mode bit  3
: TIM1_CCMR3_Output_OC5M_bit3 ( %bbb -- x addr ) 16 lshift TIM1_CCMR3_Output ; \ TIM1_CCMR3_Output_OC5M_bit3, Output Compare 5 mode bit  3
: TIM1_CCMR3_Output_OC6CE ( -- x addr ) 15 bit TIM1_CCMR3_Output ; \ TIM1_CCMR3_Output_OC6CE, Output compare 6 clear  enable
: TIM1_CCMR3_Output_OC6M ( %bbb -- x addr ) 12 lshift TIM1_CCMR3_Output ; \ TIM1_CCMR3_Output_OC6M, Output compare 6 mode
: TIM1_CCMR3_Output_OC6PE ( -- x addr ) 11 bit TIM1_CCMR3_Output ; \ TIM1_CCMR3_Output_OC6PE, Output compare 6 preload  enable
: TIM1_CCMR3_Output_OC6FE ( -- x addr ) 10 bit TIM1_CCMR3_Output ; \ TIM1_CCMR3_Output_OC6FE, Output compare 6 fast  enable
: TIM1_CCMR3_Output_OC5CE ( -- x addr ) 7 bit TIM1_CCMR3_Output ; \ TIM1_CCMR3_Output_OC5CE, Output compare 5 clear  enable
: TIM1_CCMR3_Output_OC5M ( %bbb -- x addr ) 4 lshift TIM1_CCMR3_Output ; \ TIM1_CCMR3_Output_OC5M, Output compare 5 mode
: TIM1_CCMR3_Output_OC5PE ( -- x addr ) 3 bit TIM1_CCMR3_Output ; \ TIM1_CCMR3_Output_OC5PE, Output compare 5 preload  enable
: TIM1_CCMR3_Output_OC5FE ( -- x addr ) 2 bit TIM1_CCMR3_Output ; \ TIM1_CCMR3_Output_OC5FE, Output compare 5 fast  enable
[then]

defined? use-TIM1 defined? TIM1_CCR5_CCR5 not and [if]
\ TIM1_CCR5 (read-write) Reset:0x00000000
: TIM1_CCR5_CCR5 ( %bbbbbbbbbbbbbbbb -- x addr ) TIM1_CCR5 ; \ TIM1_CCR5_CCR5, Capture/Compare value
: TIM1_CCR5_GC5C1 ( -- x addr ) 29 bit TIM1_CCR5 ; \ TIM1_CCR5_GC5C1, Group Channel 5 and Channel  1
: TIM1_CCR5_GC5C2 ( -- x addr ) 30 bit TIM1_CCR5 ; \ TIM1_CCR5_GC5C2, Group Channel 5 and Channel  2
: TIM1_CCR5_GC5C3 ( -- x addr ) 31 bit TIM1_CCR5 ; \ TIM1_CCR5_GC5C3, Group Channel 5 and Channel  3
[then]

execute-defined? use-TIM1 defined? TIM1_CCR6_CCR6 not and [if]
\ TIM1_CCR6 (read-write) Reset:0x00000000
: TIM1_CCR6_CCR6 ( %bbbbbbbbbbbbbbbb -- x addr ) TIM1_CCR6 ; \ TIM1_CCR6_CCR6, Capture/Compare value
[then]

defined? use-TIM1 defined? TIM1_OR2_BKINE not and [if]
\ TIM1_OR2 (read-write) Reset:0x00000001
: TIM1_OR2_BKINE ( -- x addr ) 0 bit TIM1_OR2 ; \ TIM1_OR2_BKINE, BRK BKIN input enable
: TIM1_OR2_BKCMP1E ( -- x addr ) 1 bit TIM1_OR2 ; \ TIM1_OR2_BKCMP1E, BRK COMP1 enable
: TIM1_OR2_BKCMP2E ( -- x addr ) 2 bit TIM1_OR2 ; \ TIM1_OR2_BKCMP2E, BRK COMP2 enable
: TIM1_OR2_BKDFBK0E ( -- x addr ) 8 bit TIM1_OR2 ; \ TIM1_OR2_BKDFBK0E, BRK DFSDM_BREAK0 enable
: TIM1_OR2_BKINP ( -- x addr ) 9 bit TIM1_OR2 ; \ TIM1_OR2_BKINP, BRK BKIN input polarity
: TIM1_OR2_BKCMP1P ( -- x addr ) 10 bit TIM1_OR2 ; \ TIM1_OR2_BKCMP1P, BRK COMP1 input polarity
: TIM1_OR2_BKCMP2P ( -- x addr ) 11 bit TIM1_OR2 ; \ TIM1_OR2_BKCMP2P, BRK COMP2 input polarity
: TIM1_OR2_ETRSEL ( %bbb -- x addr ) 14 lshift TIM1_OR2 ; \ TIM1_OR2_ETRSEL, ETR source selection
[then]

execute-defined? use-TIM1 defined? TIM1_OR3_BK2INE not and [if]
\ TIM1_OR3 (read-write) Reset:0x00000001
: TIM1_OR3_BK2INE ( -- x addr ) 0 bit TIM1_OR3 ; \ TIM1_OR3_BK2INE, BRK2 BKIN input enable
: TIM1_OR3_BK2CMP1E ( -- x addr ) 1 bit TIM1_OR3 ; \ TIM1_OR3_BK2CMP1E, BRK2 COMP1 enable
: TIM1_OR3_BK2CMP2E ( -- x addr ) 2 bit TIM1_OR3 ; \ TIM1_OR3_BK2CMP2E, BRK2 COMP2 enable
: TIM1_OR3_BK2DFBK0E ( -- x addr ) 8 bit TIM1_OR3 ; \ TIM1_OR3_BK2DFBK0E, BRK2 DFSDM_BREAK0 enable
: TIM1_OR3_BK2INP ( -- x addr ) 9 bit TIM1_OR3 ; \ TIM1_OR3_BK2INP, BRK2 BKIN input polarity
: TIM1_OR3_BK2CMP1P ( -- x addr ) 10 bit TIM1_OR3 ; \ TIM1_OR3_BK2CMP1P, BRK2 COMP1 input polarity
: TIM1_OR3_BK2CMP2P ( -- x addr ) 11 bit TIM1_OR3 ; \ TIM1_OR3_BK2CMP2P, BRK2 COMP2 input polarity
[then]

defined? use-TIM8 defined? TIM8_CR1_CKD not and [if]
\ TIM8_CR1 (read-write) Reset:0x0000
: TIM8_CR1_CKD ( %bb -- x addr ) 8 lshift TIM8_CR1 ; \ TIM8_CR1_CKD, Clock division
: TIM8_CR1_ARPE ( -- x addr ) 7 bit TIM8_CR1 ; \ TIM8_CR1_ARPE, Auto-reload preload enable
: TIM8_CR1_CMS ( %bb -- x addr ) 5 lshift TIM8_CR1 ; \ TIM8_CR1_CMS, Center-aligned mode  selection
: TIM8_CR1_DIR ( -- x addr ) 4 bit TIM8_CR1 ; \ TIM8_CR1_DIR, Direction
: TIM8_CR1_OPM ( -- x addr ) 3 bit TIM8_CR1 ; \ TIM8_CR1_OPM, One-pulse mode
: TIM8_CR1_URS ( -- x addr ) 2 bit TIM8_CR1 ; \ TIM8_CR1_URS, Update request source
: TIM8_CR1_UDIS ( -- x addr ) 1 bit TIM8_CR1 ; \ TIM8_CR1_UDIS, Update disable
: TIM8_CR1_CEN ( -- x addr ) 0 bit TIM8_CR1 ; \ TIM8_CR1_CEN, Counter enable
[then]

execute-defined? use-TIM8 defined? TIM8_CR2_OIS4 not and [if]
\ TIM8_CR2 (read-write) Reset:0x0000
: TIM8_CR2_OIS4 ( -- x addr ) 14 bit TIM8_CR2 ; \ TIM8_CR2_OIS4, Output Idle state 4
: TIM8_CR2_OIS3N ( -- x addr ) 13 bit TIM8_CR2 ; \ TIM8_CR2_OIS3N, Output Idle state 3
: TIM8_CR2_OIS3 ( -- x addr ) 12 bit TIM8_CR2 ; \ TIM8_CR2_OIS3, Output Idle state 3
: TIM8_CR2_OIS2N ( -- x addr ) 11 bit TIM8_CR2 ; \ TIM8_CR2_OIS2N, Output Idle state 2
: TIM8_CR2_OIS2 ( -- x addr ) 10 bit TIM8_CR2 ; \ TIM8_CR2_OIS2, Output Idle state 2
: TIM8_CR2_OIS1N ( -- x addr ) 9 bit TIM8_CR2 ; \ TIM8_CR2_OIS1N, Output Idle state 1
: TIM8_CR2_OIS1 ( -- x addr ) 8 bit TIM8_CR2 ; \ TIM8_CR2_OIS1, Output Idle state 1
: TIM8_CR2_TI1S ( -- x addr ) 7 bit TIM8_CR2 ; \ TIM8_CR2_TI1S, TI1 selection
: TIM8_CR2_MMS ( %bbb -- x addr ) 4 lshift TIM8_CR2 ; \ TIM8_CR2_MMS, Master mode selection
: TIM8_CR2_CCDS ( -- x addr ) 3 bit TIM8_CR2 ; \ TIM8_CR2_CCDS, Capture/compare DMA  selection
: TIM8_CR2_CCUS ( -- x addr ) 2 bit TIM8_CR2 ; \ TIM8_CR2_CCUS, Capture/compare control update  selection
: TIM8_CR2_CCPC ( -- x addr ) 0 bit TIM8_CR2 ; \ TIM8_CR2_CCPC, Capture/compare preloaded  control
[then]

defined? use-TIM8 defined? TIM8_SMCR_ETP not and [if]
\ TIM8_SMCR (read-write) Reset:0x0000
: TIM8_SMCR_ETP ( -- x addr ) 15 bit TIM8_SMCR ; \ TIM8_SMCR_ETP, External trigger polarity
: TIM8_SMCR_ECE ( -- x addr ) 14 bit TIM8_SMCR ; \ TIM8_SMCR_ECE, External clock enable
: TIM8_SMCR_ETPS ( %bb -- x addr ) 12 lshift TIM8_SMCR ; \ TIM8_SMCR_ETPS, External trigger prescaler
: TIM8_SMCR_ETF ( %bbbb -- x addr ) 8 lshift TIM8_SMCR ; \ TIM8_SMCR_ETF, External trigger filter
: TIM8_SMCR_MSM ( -- x addr ) 7 bit TIM8_SMCR ; \ TIM8_SMCR_MSM, Master/Slave mode
: TIM8_SMCR_TS ( %bbb -- x addr ) 4 lshift TIM8_SMCR ; \ TIM8_SMCR_TS, Trigger selection
: TIM8_SMCR_SMS ( %bbb -- x addr ) TIM8_SMCR ; \ TIM8_SMCR_SMS, Slave mode selection
[then]

execute-defined? use-TIM8 defined? TIM8_DIER_TDE not and [if]
\ TIM8_DIER (read-write) Reset:0x0000
: TIM8_DIER_TDE ( -- x addr ) 14 bit TIM8_DIER ; \ TIM8_DIER_TDE, Trigger DMA request enable
: TIM8_DIER_COMDE ( -- x addr ) 13 bit TIM8_DIER ; \ TIM8_DIER_COMDE, COM DMA request enable
: TIM8_DIER_CC4DE ( -- x addr ) 12 bit TIM8_DIER ; \ TIM8_DIER_CC4DE, Capture/Compare 4 DMA request  enable
: TIM8_DIER_CC3DE ( -- x addr ) 11 bit TIM8_DIER ; \ TIM8_DIER_CC3DE, Capture/Compare 3 DMA request  enable
: TIM8_DIER_CC2DE ( -- x addr ) 10 bit TIM8_DIER ; \ TIM8_DIER_CC2DE, Capture/Compare 2 DMA request  enable
: TIM8_DIER_CC1DE ( -- x addr ) 9 bit TIM8_DIER ; \ TIM8_DIER_CC1DE, Capture/Compare 1 DMA request  enable
: TIM8_DIER_UDE ( -- x addr ) 8 bit TIM8_DIER ; \ TIM8_DIER_UDE, Update DMA request enable
: TIM8_DIER_TIE ( -- x addr ) 6 bit TIM8_DIER ; \ TIM8_DIER_TIE, Trigger interrupt enable
: TIM8_DIER_CC4IE ( -- x addr ) 4 bit TIM8_DIER ; \ TIM8_DIER_CC4IE, Capture/Compare 4 interrupt  enable
: TIM8_DIER_CC3IE ( -- x addr ) 3 bit TIM8_DIER ; \ TIM8_DIER_CC3IE, Capture/Compare 3 interrupt  enable
: TIM8_DIER_CC2IE ( -- x addr ) 2 bit TIM8_DIER ; \ TIM8_DIER_CC2IE, Capture/Compare 2 interrupt  enable
: TIM8_DIER_CC1IE ( -- x addr ) 1 bit TIM8_DIER ; \ TIM8_DIER_CC1IE, Capture/Compare 1 interrupt  enable
: TIM8_DIER_UIE ( -- x addr ) 0 bit TIM8_DIER ; \ TIM8_DIER_UIE, Update interrupt enable
: TIM8_DIER_BIE ( -- x addr ) 7 bit TIM8_DIER ; \ TIM8_DIER_BIE, Break interrupt enable
: TIM8_DIER_COMIE ( -- x addr ) 5 bit TIM8_DIER ; \ TIM8_DIER_COMIE, COM interrupt enable
[then]

defined? use-TIM8 defined? TIM8_SR_CC4OF not and [if]
\ TIM8_SR (read-write) Reset:0x0000
: TIM8_SR_CC4OF ( -- x addr ) 12 bit TIM8_SR ; \ TIM8_SR_CC4OF, Capture/Compare 4 overcapture  flag
: TIM8_SR_CC3OF ( -- x addr ) 11 bit TIM8_SR ; \ TIM8_SR_CC3OF, Capture/Compare 3 overcapture  flag
: TIM8_SR_CC2OF ( -- x addr ) 10 bit TIM8_SR ; \ TIM8_SR_CC2OF, Capture/compare 2 overcapture  flag
: TIM8_SR_CC1OF ( -- x addr ) 9 bit TIM8_SR ; \ TIM8_SR_CC1OF, Capture/Compare 1 overcapture  flag
: TIM8_SR_BIF ( -- x addr ) 7 bit TIM8_SR ; \ TIM8_SR_BIF, Break interrupt flag
: TIM8_SR_TIF ( -- x addr ) 6 bit TIM8_SR ; \ TIM8_SR_TIF, Trigger interrupt flag
: TIM8_SR_COMIF ( -- x addr ) 5 bit TIM8_SR ; \ TIM8_SR_COMIF, COM interrupt flag
: TIM8_SR_CC4IF ( -- x addr ) 4 bit TIM8_SR ; \ TIM8_SR_CC4IF, Capture/Compare 4 interrupt  flag
: TIM8_SR_CC3IF ( -- x addr ) 3 bit TIM8_SR ; \ TIM8_SR_CC3IF, Capture/Compare 3 interrupt  flag
: TIM8_SR_CC2IF ( -- x addr ) 2 bit TIM8_SR ; \ TIM8_SR_CC2IF, Capture/Compare 2 interrupt  flag
: TIM8_SR_CC1IF ( -- x addr ) 1 bit TIM8_SR ; \ TIM8_SR_CC1IF, Capture/compare 1 interrupt  flag
: TIM8_SR_UIF ( -- x addr ) 0 bit TIM8_SR ; \ TIM8_SR_UIF, Update interrupt flag
[then]

execute-defined? use-TIM8 defined? TIM8_EGR_BG not and [if]
\ TIM8_EGR (write-only) Reset:0x0000
: TIM8_EGR_BG ( -- x addr ) 7 bit TIM8_EGR ; \ TIM8_EGR_BG, Break generation
: TIM8_EGR_TG ( -- x addr ) 6 bit TIM8_EGR ; \ TIM8_EGR_TG, Trigger generation
: TIM8_EGR_COMG ( -- x addr ) 5 bit TIM8_EGR ; \ TIM8_EGR_COMG, Capture/Compare control update  generation
: TIM8_EGR_CC4G ( -- x addr ) 4 bit TIM8_EGR ; \ TIM8_EGR_CC4G, Capture/compare 4  generation
: TIM8_EGR_CC3G ( -- x addr ) 3 bit TIM8_EGR ; \ TIM8_EGR_CC3G, Capture/compare 3  generation
: TIM8_EGR_CC2G ( -- x addr ) 2 bit TIM8_EGR ; \ TIM8_EGR_CC2G, Capture/compare 2  generation
: TIM8_EGR_CC1G ( -- x addr ) 1 bit TIM8_EGR ; \ TIM8_EGR_CC1G, Capture/compare 1  generation
: TIM8_EGR_UG ( -- x addr ) 0 bit TIM8_EGR ; \ TIM8_EGR_UG, Update generation
[then]

defined? use-TIM8 defined? TIM8_CCMR1_Output_OC2CE not and [if]
\ TIM8_CCMR1_Output (read-write) Reset:0x00000000
: TIM8_CCMR1_Output_OC2CE ( -- x addr ) 15 bit TIM8_CCMR1_Output ; \ TIM8_CCMR1_Output_OC2CE, Output Compare 2 clear  enable
: TIM8_CCMR1_Output_OC2M ( %bbb -- x addr ) 12 lshift TIM8_CCMR1_Output ; \ TIM8_CCMR1_Output_OC2M, Output Compare 2 mode
: TIM8_CCMR1_Output_OC2PE ( -- x addr ) 11 bit TIM8_CCMR1_Output ; \ TIM8_CCMR1_Output_OC2PE, Output Compare 2 preload  enable
: TIM8_CCMR1_Output_OC2FE ( -- x addr ) 10 bit TIM8_CCMR1_Output ; \ TIM8_CCMR1_Output_OC2FE, Output Compare 2 fast  enable
: TIM8_CCMR1_Output_CC2S ( %bb -- x addr ) 8 lshift TIM8_CCMR1_Output ; \ TIM8_CCMR1_Output_CC2S, Capture/Compare 2  selection
: TIM8_CCMR1_Output_OC1CE ( -- x addr ) 7 bit TIM8_CCMR1_Output ; \ TIM8_CCMR1_Output_OC1CE, Output Compare 1 clear  enable
: TIM8_CCMR1_Output_OC1M ( %bbb -- x addr ) 4 lshift TIM8_CCMR1_Output ; \ TIM8_CCMR1_Output_OC1M, Output Compare 1 mode
: TIM8_CCMR1_Output_OC1PE ( -- x addr ) 3 bit TIM8_CCMR1_Output ; \ TIM8_CCMR1_Output_OC1PE, Output Compare 1 preload  enable
: TIM8_CCMR1_Output_OC1FE ( -- x addr ) 2 bit TIM8_CCMR1_Output ; \ TIM8_CCMR1_Output_OC1FE, Output Compare 1 fast  enable
: TIM8_CCMR1_Output_CC1S ( %bb -- x addr ) TIM8_CCMR1_Output ; \ TIM8_CCMR1_Output_CC1S, Capture/Compare 1  selection
[then]

execute-defined? use-TIM8 defined? TIM8_CCMR1_Input_IC2F not and [if]
\ TIM8_CCMR1_Input (read-write) Reset:0x00000000
: TIM8_CCMR1_Input_IC2F ( %bbbb -- x addr ) 12 lshift TIM8_CCMR1_Input ; \ TIM8_CCMR1_Input_IC2F, Input capture 2 filter
: TIM8_CCMR1_Input_IC2PCS ( %bb -- x addr ) 10 lshift TIM8_CCMR1_Input ; \ TIM8_CCMR1_Input_IC2PCS, Input capture 2 prescaler
: TIM8_CCMR1_Input_CC2S ( %bb -- x addr ) 8 lshift TIM8_CCMR1_Input ; \ TIM8_CCMR1_Input_CC2S, Capture/Compare 2  selection
: TIM8_CCMR1_Input_IC1F ( %bbbb -- x addr ) 4 lshift TIM8_CCMR1_Input ; \ TIM8_CCMR1_Input_IC1F, Input capture 1 filter
: TIM8_CCMR1_Input_ICPCS ( %bb -- x addr ) 2 lshift TIM8_CCMR1_Input ; \ TIM8_CCMR1_Input_ICPCS, Input capture 1 prescaler
: TIM8_CCMR1_Input_CC1S ( %bb -- x addr ) TIM8_CCMR1_Input ; \ TIM8_CCMR1_Input_CC1S, Capture/Compare 1  selection
[then]

defined? use-TIM8 defined? TIM8_CCMR2_Output_OC4CE not and [if]
\ TIM8_CCMR2_Output (read-write) Reset:0x00000000
: TIM8_CCMR2_Output_OC4CE ( -- x addr ) 15 bit TIM8_CCMR2_Output ; \ TIM8_CCMR2_Output_OC4CE, Output compare 4 clear  enable
: TIM8_CCMR2_Output_OC4M ( %bbb -- x addr ) 12 lshift TIM8_CCMR2_Output ; \ TIM8_CCMR2_Output_OC4M, Output compare 4 mode
: TIM8_CCMR2_Output_OC4PE ( -- x addr ) 11 bit TIM8_CCMR2_Output ; \ TIM8_CCMR2_Output_OC4PE, Output compare 4 preload  enable
: TIM8_CCMR2_Output_OC4FE ( -- x addr ) 10 bit TIM8_CCMR2_Output ; \ TIM8_CCMR2_Output_OC4FE, Output compare 4 fast  enable
: TIM8_CCMR2_Output_CC4S ( %bb -- x addr ) 8 lshift TIM8_CCMR2_Output ; \ TIM8_CCMR2_Output_CC4S, Capture/Compare 4  selection
: TIM8_CCMR2_Output_OC3CE ( -- x addr ) 7 bit TIM8_CCMR2_Output ; \ TIM8_CCMR2_Output_OC3CE, Output compare 3 clear  enable
: TIM8_CCMR2_Output_OC3M ( %bbb -- x addr ) 4 lshift TIM8_CCMR2_Output ; \ TIM8_CCMR2_Output_OC3M, Output compare 3 mode
: TIM8_CCMR2_Output_OC3PE ( -- x addr ) 3 bit TIM8_CCMR2_Output ; \ TIM8_CCMR2_Output_OC3PE, Output compare 3 preload  enable
: TIM8_CCMR2_Output_OC3FE ( -- x addr ) 2 bit TIM8_CCMR2_Output ; \ TIM8_CCMR2_Output_OC3FE, Output compare 3 fast  enable
: TIM8_CCMR2_Output_CC3S ( %bb -- x addr ) TIM8_CCMR2_Output ; \ TIM8_CCMR2_Output_CC3S, Capture/Compare 3  selection
[then]

execute-defined? use-TIM8 defined? TIM8_CCMR2_Input_IC4F not and [if]
\ TIM8_CCMR2_Input (read-write) Reset:0x00000000
: TIM8_CCMR2_Input_IC4F ( %bbbb -- x addr ) 12 lshift TIM8_CCMR2_Input ; \ TIM8_CCMR2_Input_IC4F, Input capture 4 filter
: TIM8_CCMR2_Input_IC4PSC ( %bb -- x addr ) 10 lshift TIM8_CCMR2_Input ; \ TIM8_CCMR2_Input_IC4PSC, Input capture 4 prescaler
: TIM8_CCMR2_Input_CC4S ( %bb -- x addr ) 8 lshift TIM8_CCMR2_Input ; \ TIM8_CCMR2_Input_CC4S, Capture/Compare 4  selection
: TIM8_CCMR2_Input_IC3F ( %bbbb -- x addr ) 4 lshift TIM8_CCMR2_Input ; \ TIM8_CCMR2_Input_IC3F, Input capture 3 filter
: TIM8_CCMR2_Input_IC3PSC ( %bb -- x addr ) 2 lshift TIM8_CCMR2_Input ; \ TIM8_CCMR2_Input_IC3PSC, Input capture 3 prescaler
: TIM8_CCMR2_Input_CC3S ( %bb -- x addr ) TIM8_CCMR2_Input ; \ TIM8_CCMR2_Input_CC3S, Capture/compare 3  selection
[then]

defined? use-TIM8 defined? TIM8_CCER_CC4P not and [if]
\ TIM8_CCER (read-write) Reset:0x0000
: TIM8_CCER_CC4P ( -- x addr ) 13 bit TIM8_CCER ; \ TIM8_CCER_CC4P, Capture/Compare 3 output  Polarity
: TIM8_CCER_CC4E ( -- x addr ) 12 bit TIM8_CCER ; \ TIM8_CCER_CC4E, Capture/Compare 4 output  enable
: TIM8_CCER_CC3NP ( -- x addr ) 11 bit TIM8_CCER ; \ TIM8_CCER_CC3NP, Capture/Compare 3 output  Polarity
: TIM8_CCER_CC3NE ( -- x addr ) 10 bit TIM8_CCER ; \ TIM8_CCER_CC3NE, Capture/Compare 3 complementary output  enable
: TIM8_CCER_CC3P ( -- x addr ) 9 bit TIM8_CCER ; \ TIM8_CCER_CC3P, Capture/Compare 3 output  Polarity
: TIM8_CCER_CC3E ( -- x addr ) 8 bit TIM8_CCER ; \ TIM8_CCER_CC3E, Capture/Compare 3 output  enable
: TIM8_CCER_CC2NP ( -- x addr ) 7 bit TIM8_CCER ; \ TIM8_CCER_CC2NP, Capture/Compare 2 output  Polarity
: TIM8_CCER_CC2NE ( -- x addr ) 6 bit TIM8_CCER ; \ TIM8_CCER_CC2NE, Capture/Compare 2 complementary output  enable
: TIM8_CCER_CC2P ( -- x addr ) 5 bit TIM8_CCER ; \ TIM8_CCER_CC2P, Capture/Compare 2 output  Polarity
: TIM8_CCER_CC2E ( -- x addr ) 4 bit TIM8_CCER ; \ TIM8_CCER_CC2E, Capture/Compare 2 output  enable
: TIM8_CCER_CC1NP ( -- x addr ) 3 bit TIM8_CCER ; \ TIM8_CCER_CC1NP, Capture/Compare 1 output  Polarity
: TIM8_CCER_CC1NE ( -- x addr ) 2 bit TIM8_CCER ; \ TIM8_CCER_CC1NE, Capture/Compare 1 complementary output  enable
: TIM8_CCER_CC1P ( -- x addr ) 1 bit TIM8_CCER ; \ TIM8_CCER_CC1P, Capture/Compare 1 output  Polarity
: TIM8_CCER_CC1E ( -- x addr ) 0 bit TIM8_CCER ; \ TIM8_CCER_CC1E, Capture/Compare 1 output  enable
[then]

execute-defined? use-TIM8 defined? TIM8_CNT_CNT not and [if]
\ TIM8_CNT (read-write) Reset:0x00000000
: TIM8_CNT_CNT ( %bbbbbbbbbbbbbbbb -- x addr ) TIM8_CNT ; \ TIM8_CNT_CNT, counter value
[then]

defined? use-TIM8 defined? TIM8_PSC_PSC not and [if]
\ TIM8_PSC (read-write) Reset:0x0000
: TIM8_PSC_PSC ( %bbbbbbbbbbbbbbbb -- x addr ) TIM8_PSC ; \ TIM8_PSC_PSC, Prescaler value
[then]

execute-defined? use-TIM8 defined? TIM8_ARR_ARR not and [if]
\ TIM8_ARR (read-write) Reset:0x00000000
: TIM8_ARR_ARR ( %bbbbbbbbbbbbbbbb -- x addr ) TIM8_ARR ; \ TIM8_ARR_ARR, Auto-reload value
[then]

defined? use-TIM8 defined? TIM8_RCR_REP not and [if]
\ TIM8_RCR (read-write) Reset:0x0000
: TIM8_RCR_REP ( %bbbbbbbb -- x addr ) TIM8_RCR ; \ TIM8_RCR_REP, Repetition counter value
[then]

execute-defined? use-TIM8 defined? TIM8_CCR1_CCR1 not and [if]
\ TIM8_CCR1 (read-write) Reset:0x00000000
: TIM8_CCR1_CCR1 ( %bbbbbbbbbbbbbbbb -- x addr ) TIM8_CCR1 ; \ TIM8_CCR1_CCR1, Capture/Compare 1 value
[then]

defined? use-TIM8 defined? TIM8_CCR2_CCR2 not and [if]
\ TIM8_CCR2 (read-write) Reset:0x00000000
: TIM8_CCR2_CCR2 ( %bbbbbbbbbbbbbbbb -- x addr ) TIM8_CCR2 ; \ TIM8_CCR2_CCR2, Capture/Compare 2 value
[then]

execute-defined? use-TIM8 defined? TIM8_CCR3_CCR3 not and [if]
\ TIM8_CCR3 (read-write) Reset:0x00000000
: TIM8_CCR3_CCR3 ( %bbbbbbbbbbbbbbbb -- x addr ) TIM8_CCR3 ; \ TIM8_CCR3_CCR3, Capture/Compare value
[then]

defined? use-TIM8 defined? TIM8_CCR4_CCR4 not and [if]
\ TIM8_CCR4 (read-write) Reset:0x00000000
: TIM8_CCR4_CCR4 ( %bbbbbbbbbbbbbbbb -- x addr ) TIM8_CCR4 ; \ TIM8_CCR4_CCR4, Capture/Compare value
[then]

execute-defined? use-TIM8 defined? TIM8_BDTR_MOE not and [if]
\ TIM8_BDTR (read-write) Reset:0x0000
: TIM8_BDTR_MOE ( -- x addr ) 15 bit TIM8_BDTR ; \ TIM8_BDTR_MOE, Main output enable
: TIM8_BDTR_AOE ( -- x addr ) 14 bit TIM8_BDTR ; \ TIM8_BDTR_AOE, Automatic output enable
: TIM8_BDTR_BKP ( -- x addr ) 13 bit TIM8_BDTR ; \ TIM8_BDTR_BKP, Break polarity
: TIM8_BDTR_BKE ( -- x addr ) 12 bit TIM8_BDTR ; \ TIM8_BDTR_BKE, Break enable
: TIM8_BDTR_OSSR ( -- x addr ) 11 bit TIM8_BDTR ; \ TIM8_BDTR_OSSR, Off-state selection for Run  mode
: TIM8_BDTR_OSSI ( -- x addr ) 10 bit TIM8_BDTR ; \ TIM8_BDTR_OSSI, Off-state selection for Idle  mode
: TIM8_BDTR_LOCK ( %bb -- x addr ) 8 lshift TIM8_BDTR ; \ TIM8_BDTR_LOCK, Lock configuration
: TIM8_BDTR_DTG ( %bbbbbbbb -- x addr ) TIM8_BDTR ; \ TIM8_BDTR_DTG, Dead-time generator setup
[then]

defined? use-TIM8 defined? TIM8_DCR_DBL not and [if]
\ TIM8_DCR (read-write) Reset:0x0000
: TIM8_DCR_DBL ( %bbbbb -- x addr ) 8 lshift TIM8_DCR ; \ TIM8_DCR_DBL, DMA burst length
: TIM8_DCR_DBA ( %bbbbb -- x addr ) TIM8_DCR ; \ TIM8_DCR_DBA, DMA base address
[then]

execute-defined? use-TIM8 defined? TIM8_DMAR_DMAB not and [if]
\ TIM8_DMAR (read-write) Reset:0x0000
: TIM8_DMAR_DMAB ( %bbbbbbbbbbbbbbbb -- x addr ) TIM8_DMAR ; \ TIM8_DMAR_DMAB, DMA register for burst  accesses
[then]

defined? use-TIM8 defined? TIM8_OR1_ETR_ADC2_RMP not and [if]
\ TIM8_OR1 (read-write) Reset:0x0000
: TIM8_OR1_ETR_ADC2_RMP ( %bb -- x addr ) TIM8_OR1 ; \ TIM8_OR1_ETR_ADC2_RMP, External trigger remap on ADC2 analog  watchdog
: TIM8_OR1_ETR_ADC3_RMP ( %bb -- x addr ) 2 lshift TIM8_OR1 ; \ TIM8_OR1_ETR_ADC3_RMP, External trigger remap on ADC3 analog  watchdog
: TIM8_OR1_TI1_RMP ( -- x addr ) 4 bit TIM8_OR1 ; \ TIM8_OR1_TI1_RMP, Input Capture 1 remap
[then]

execute-defined? use-TIM8 defined? TIM8_CCMR3_Output_OC6M_bit3 not and [if]
\ TIM8_CCMR3_Output (read-write) Reset:0x00000000
: TIM8_CCMR3_Output_OC6M_bit3 ( -- x addr ) 24 bit TIM8_CCMR3_Output ; \ TIM8_CCMR3_Output_OC6M_bit3, Output Compare 6 mode bit  3
: TIM8_CCMR3_Output_OC5M_bit3 ( %bbb -- x addr ) 16 lshift TIM8_CCMR3_Output ; \ TIM8_CCMR3_Output_OC5M_bit3, Output Compare 5 mode bit  3
: TIM8_CCMR3_Output_OC6CE ( -- x addr ) 15 bit TIM8_CCMR3_Output ; \ TIM8_CCMR3_Output_OC6CE, Output compare 6 clear  enable
: TIM8_CCMR3_Output_OC6M ( %bbb -- x addr ) 12 lshift TIM8_CCMR3_Output ; \ TIM8_CCMR3_Output_OC6M, Output compare 6 mode
: TIM8_CCMR3_Output_OC6PE ( -- x addr ) 11 bit TIM8_CCMR3_Output ; \ TIM8_CCMR3_Output_OC6PE, Output compare 6 preload  enable
: TIM8_CCMR3_Output_OC6FE ( -- x addr ) 10 bit TIM8_CCMR3_Output ; \ TIM8_CCMR3_Output_OC6FE, Output compare 6 fast  enable
: TIM8_CCMR3_Output_OC5CE ( -- x addr ) 7 bit TIM8_CCMR3_Output ; \ TIM8_CCMR3_Output_OC5CE, Output compare 5 clear  enable
: TIM8_CCMR3_Output_OC5M ( %bbb -- x addr ) 4 lshift TIM8_CCMR3_Output ; \ TIM8_CCMR3_Output_OC5M, Output compare 5 mode
: TIM8_CCMR3_Output_OC5PE ( -- x addr ) 3 bit TIM8_CCMR3_Output ; \ TIM8_CCMR3_Output_OC5PE, Output compare 5 preload  enable
: TIM8_CCMR3_Output_OC5FE ( -- x addr ) 2 bit TIM8_CCMR3_Output ; \ TIM8_CCMR3_Output_OC5FE, Output compare 5 fast  enable
[then]

defined? use-TIM8 defined? TIM8_CCR5_CCR5 not and [if]
\ TIM8_CCR5 (read-write) Reset:0x00000000
: TIM8_CCR5_CCR5 ( %bbbbbbbbbbbbbbbb -- x addr ) TIM8_CCR5 ; \ TIM8_CCR5_CCR5, Capture/Compare value
: TIM8_CCR5_GC5C1 ( -- x addr ) 29 bit TIM8_CCR5 ; \ TIM8_CCR5_GC5C1, Group Channel 5 and Channel  1
: TIM8_CCR5_GC5C2 ( -- x addr ) 30 bit TIM8_CCR5 ; \ TIM8_CCR5_GC5C2, Group Channel 5 and Channel  2
: TIM8_CCR5_GC5C3 ( -- x addr ) 31 bit TIM8_CCR5 ; \ TIM8_CCR5_GC5C3, Group Channel 5 and Channel  3
[then]

execute-defined? use-TIM8 defined? TIM8_CCR6_CCR6 not and [if]
\ TIM8_CCR6 (read-write) Reset:0x00000000
: TIM8_CCR6_CCR6 ( %bbbbbbbbbbbbbbbb -- x addr ) TIM8_CCR6 ; \ TIM8_CCR6_CCR6, Capture/Compare value
[then]

defined? use-TIM8 defined? TIM8_OR2_BKINE not and [if]
\ TIM8_OR2 (read-write) Reset:0x00000001
: TIM8_OR2_BKINE ( -- x addr ) 0 bit TIM8_OR2 ; \ TIM8_OR2_BKINE, BRK BKIN input enable
: TIM8_OR2_BKCMP1E ( -- x addr ) 1 bit TIM8_OR2 ; \ TIM8_OR2_BKCMP1E, BRK COMP1 enable
: TIM8_OR2_BKCMP2E ( -- x addr ) 2 bit TIM8_OR2 ; \ TIM8_OR2_BKCMP2E, BRK COMP2 enable
: TIM8_OR2_BKDFBK2E ( -- x addr ) 8 bit TIM8_OR2 ; \ TIM8_OR2_BKDFBK2E, BRK DFSDM_BREAK2 enable
: TIM8_OR2_BKINP ( -- x addr ) 9 bit TIM8_OR2 ; \ TIM8_OR2_BKINP, BRK BKIN input polarity
: TIM8_OR2_BKCMP1P ( -- x addr ) 10 bit TIM8_OR2 ; \ TIM8_OR2_BKCMP1P, BRK COMP1 input polarity
: TIM8_OR2_BKCMP2P ( -- x addr ) 11 bit TIM8_OR2 ; \ TIM8_OR2_BKCMP2P, BRK COMP2 input polarity
: TIM8_OR2_ETRSEL ( %bbb -- x addr ) 14 lshift TIM8_OR2 ; \ TIM8_OR2_ETRSEL, ETR source selection
[then]

execute-defined? use-TIM8 defined? TIM8_OR3_BK2INE not and [if]
\ TIM8_OR3 (read-write) Reset:0x00000001
: TIM8_OR3_BK2INE ( -- x addr ) 0 bit TIM8_OR3 ; \ TIM8_OR3_BK2INE, BRK2 BKIN input enable
: TIM8_OR3_BK2CMP1E ( -- x addr ) 1 bit TIM8_OR3 ; \ TIM8_OR3_BK2CMP1E, BRK2 COMP1 enable
: TIM8_OR3_BK2CMP2E ( -- x addr ) 2 bit TIM8_OR3 ; \ TIM8_OR3_BK2CMP2E, BRK2 COMP2 enable
: TIM8_OR3_BK2DFBK3E ( -- x addr ) 8 bit TIM8_OR3 ; \ TIM8_OR3_BK2DFBK3E, BRK2 DFSDM_BREAK3 enable
: TIM8_OR3_BK2INP ( -- x addr ) 9 bit TIM8_OR3 ; \ TIM8_OR3_BK2INP, BRK2 BKIN input polarity
: TIM8_OR3_BK2CMP1P ( -- x addr ) 10 bit TIM8_OR3 ; \ TIM8_OR3_BK2CMP1P, BRK2 COMP1 input polarity
: TIM8_OR3_BK2CMP2P ( -- x addr ) 11 bit TIM8_OR3 ; \ TIM8_OR3_BK2CMP2P, BRK2 COMP2 input polarity
[then]

defined? use-TIM6 defined? TIM6_CR1_ARPE not and [if]
\ TIM6_CR1 (read-write) Reset:0x0000
: TIM6_CR1_ARPE ( -- x addr ) 7 bit TIM6_CR1 ; \ TIM6_CR1_ARPE, Auto-reload preload enable
: TIM6_CR1_OPM ( -- x addr ) 3 bit TIM6_CR1 ; \ TIM6_CR1_OPM, One-pulse mode
: TIM6_CR1_URS ( -- x addr ) 2 bit TIM6_CR1 ; \ TIM6_CR1_URS, Update request source
: TIM6_CR1_UDIS ( -- x addr ) 1 bit TIM6_CR1 ; \ TIM6_CR1_UDIS, Update disable
: TIM6_CR1_CEN ( -- x addr ) 0 bit TIM6_CR1 ; \ TIM6_CR1_CEN, Counter enable
[then]

execute-defined? use-TIM6 defined? TIM6_CR2_MMS not and [if]
\ TIM6_CR2 (read-write) Reset:0x0000
: TIM6_CR2_MMS ( %bbb -- x addr ) 4 lshift TIM6_CR2 ; \ TIM6_CR2_MMS, Master mode selection
[then]

defined? use-TIM6 defined? TIM6_DIER_UDE not and [if]
\ TIM6_DIER (read-write) Reset:0x0000
: TIM6_DIER_UDE ( -- x addr ) 8 bit TIM6_DIER ; \ TIM6_DIER_UDE, Update DMA request enable
: TIM6_DIER_UIE ( -- x addr ) 0 bit TIM6_DIER ; \ TIM6_DIER_UIE, Update interrupt enable
[then]

execute-defined? use-TIM6 defined? TIM6_SR_UIF not and [if]
\ TIM6_SR (read-write) Reset:0x0000
: TIM6_SR_UIF ( -- x addr ) 0 bit TIM6_SR ; \ TIM6_SR_UIF, Update interrupt flag
[then]

defined? use-TIM6 defined? TIM6_EGR_UG not and [if]
\ TIM6_EGR (write-only) Reset:0x0000
: TIM6_EGR_UG ( -- x addr ) 0 bit TIM6_EGR ; \ TIM6_EGR_UG, Update generation
[then]

execute-defined? use-TIM6 defined? TIM6_CNT_CNT not and [if]
\ TIM6_CNT (read-write) Reset:0x00000000
: TIM6_CNT_CNT ( %bbbbbbbbbbbbbbbb -- x addr ) TIM6_CNT ; \ TIM6_CNT_CNT, Low counter value
[then]

defined? use-TIM6 defined? TIM6_PSC_PSC not and [if]
\ TIM6_PSC (read-write) Reset:0x0000
: TIM6_PSC_PSC ( %bbbbbbbbbbbbbbbb -- x addr ) TIM6_PSC ; \ TIM6_PSC_PSC, Prescaler value
[then]

execute-defined? use-TIM6 defined? TIM6_ARR_ARR not and [if]
\ TIM6_ARR (read-write) Reset:0x00000000
: TIM6_ARR_ARR ( %bbbbbbbbbbbbbbbb -- x addr ) TIM6_ARR ; \ TIM6_ARR_ARR, Low Auto-reload value
[then]

defined? use-TIM7 defined? TIM7_CR1_ARPE not and [if]
\ TIM7_CR1 (read-write) Reset:0x0000
: TIM7_CR1_ARPE ( -- x addr ) 7 bit TIM7_CR1 ; \ TIM7_CR1_ARPE, Auto-reload preload enable
: TIM7_CR1_OPM ( -- x addr ) 3 bit TIM7_CR1 ; \ TIM7_CR1_OPM, One-pulse mode
: TIM7_CR1_URS ( -- x addr ) 2 bit TIM7_CR1 ; \ TIM7_CR1_URS, Update request source
: TIM7_CR1_UDIS ( -- x addr ) 1 bit TIM7_CR1 ; \ TIM7_CR1_UDIS, Update disable
: TIM7_CR1_CEN ( -- x addr ) 0 bit TIM7_CR1 ; \ TIM7_CR1_CEN, Counter enable
[then]

execute-defined? use-TIM7 defined? TIM7_CR2_MMS not and [if]
\ TIM7_CR2 (read-write) Reset:0x0000
: TIM7_CR2_MMS ( %bbb -- x addr ) 4 lshift TIM7_CR2 ; \ TIM7_CR2_MMS, Master mode selection
[then]

defined? use-TIM7 defined? TIM7_DIER_UDE not and [if]
\ TIM7_DIER (read-write) Reset:0x0000
: TIM7_DIER_UDE ( -- x addr ) 8 bit TIM7_DIER ; \ TIM7_DIER_UDE, Update DMA request enable
: TIM7_DIER_UIE ( -- x addr ) 0 bit TIM7_DIER ; \ TIM7_DIER_UIE, Update interrupt enable
[then]

execute-defined? use-TIM7 defined? TIM7_SR_UIF not and [if]
\ TIM7_SR (read-write) Reset:0x0000
: TIM7_SR_UIF ( -- x addr ) 0 bit TIM7_SR ; \ TIM7_SR_UIF, Update interrupt flag
[then]

defined? use-TIM7 defined? TIM7_EGR_UG not and [if]
\ TIM7_EGR (write-only) Reset:0x0000
: TIM7_EGR_UG ( -- x addr ) 0 bit TIM7_EGR ; \ TIM7_EGR_UG, Update generation
[then]

execute-defined? use-TIM7 defined? TIM7_CNT_CNT not and [if]
\ TIM7_CNT (read-write) Reset:0x00000000
: TIM7_CNT_CNT ( %bbbbbbbbbbbbbbbb -- x addr ) TIM7_CNT ; \ TIM7_CNT_CNT, Low counter value
[then]

defined? use-TIM7 defined? TIM7_PSC_PSC not and [if]
\ TIM7_PSC (read-write) Reset:0x0000
: TIM7_PSC_PSC ( %bbbbbbbbbbbbbbbb -- x addr ) TIM7_PSC ; \ TIM7_PSC_PSC, Prescaler value
[then]

execute-defined? use-TIM7 defined? TIM7_ARR_ARR not and [if]
\ TIM7_ARR (read-write) Reset:0x00000000
: TIM7_ARR_ARR ( %bbbbbbbbbbbbbbbb -- x addr ) TIM7_ARR ; \ TIM7_ARR_ARR, Low Auto-reload value
[then]

defined? use-USART1 defined? USART1_CR1_M1 not and [if]
\ USART1_CR1 (read-write) Reset:0x0000
: USART1_CR1_M1 ( -- x addr ) 28 bit USART1_CR1 ; \ USART1_CR1_M1, Word length
: USART1_CR1_EOBIE ( -- x addr ) 27 bit USART1_CR1 ; \ USART1_CR1_EOBIE, End of Block interrupt  enable
: USART1_CR1_RTOIE ( -- x addr ) 26 bit USART1_CR1 ; \ USART1_CR1_RTOIE, Receiver timeout interrupt  enable
: USART1_CR1_DEAT4 ( -- x addr ) 25 bit USART1_CR1 ; \ USART1_CR1_DEAT4, Driver Enable assertion  time
: USART1_CR1_DEAT3 ( -- x addr ) 24 bit USART1_CR1 ; \ USART1_CR1_DEAT3, DEAT3
: USART1_CR1_DEAT2 ( -- x addr ) 23 bit USART1_CR1 ; \ USART1_CR1_DEAT2, DEAT2
: USART1_CR1_DEAT1 ( -- x addr ) 22 bit USART1_CR1 ; \ USART1_CR1_DEAT1, DEAT1
: USART1_CR1_DEAT0 ( -- x addr ) 21 bit USART1_CR1 ; \ USART1_CR1_DEAT0, DEAT0
: USART1_CR1_DEDT4 ( -- x addr ) 20 bit USART1_CR1 ; \ USART1_CR1_DEDT4, Driver Enable de-assertion  time
: USART1_CR1_DEDT3 ( -- x addr ) 19 bit USART1_CR1 ; \ USART1_CR1_DEDT3, DEDT3
: USART1_CR1_DEDT2 ( -- x addr ) 18 bit USART1_CR1 ; \ USART1_CR1_DEDT2, DEDT2
: USART1_CR1_DEDT1 ( -- x addr ) 17 bit USART1_CR1 ; \ USART1_CR1_DEDT1, DEDT1
: USART1_CR1_DEDT0 ( -- x addr ) 16 bit USART1_CR1 ; \ USART1_CR1_DEDT0, DEDT0
: USART1_CR1_OVER8 ( -- x addr ) 15 bit USART1_CR1 ; \ USART1_CR1_OVER8, Oversampling mode
: USART1_CR1_CMIE ( -- x addr ) 14 bit USART1_CR1 ; \ USART1_CR1_CMIE, Character match interrupt  enable
: USART1_CR1_MME ( -- x addr ) 13 bit USART1_CR1 ; \ USART1_CR1_MME, Mute mode enable
: USART1_CR1_M0 ( -- x addr ) 12 bit USART1_CR1 ; \ USART1_CR1_M0, Word length
: USART1_CR1_WAKE ( -- x addr ) 11 bit USART1_CR1 ; \ USART1_CR1_WAKE, Receiver wakeup method
: USART1_CR1_PCE ( -- x addr ) 10 bit USART1_CR1 ; \ USART1_CR1_PCE, Parity control enable
: USART1_CR1_PS ( -- x addr ) 9 bit USART1_CR1 ; \ USART1_CR1_PS, Parity selection
: USART1_CR1_PEIE ( -- x addr ) 8 bit USART1_CR1 ; \ USART1_CR1_PEIE, PE interrupt enable
: USART1_CR1_TXEIE ( -- x addr ) 7 bit USART1_CR1 ; \ USART1_CR1_TXEIE, interrupt enable
: USART1_CR1_TCIE ( -- x addr ) 6 bit USART1_CR1 ; \ USART1_CR1_TCIE, Transmission complete interrupt  enable
: USART1_CR1_RXNEIE ( -- x addr ) 5 bit USART1_CR1 ; \ USART1_CR1_RXNEIE, RXNE interrupt enable
: USART1_CR1_IDLEIE ( -- x addr ) 4 bit USART1_CR1 ; \ USART1_CR1_IDLEIE, IDLE interrupt enable
: USART1_CR1_TE ( -- x addr ) 3 bit USART1_CR1 ; \ USART1_CR1_TE, Transmitter enable
: USART1_CR1_RE ( -- x addr ) 2 bit USART1_CR1 ; \ USART1_CR1_RE, Receiver enable
: USART1_CR1_UESM ( -- x addr ) 1 bit USART1_CR1 ; \ USART1_CR1_UESM, USART enable in Stop mode
: USART1_CR1_UE ( -- x addr ) 0 bit USART1_CR1 ; \ USART1_CR1_UE, USART enable
[then]

execute-defined? use-USART1 defined? USART1_CR2_ADD4_7 not and [if]
\ USART1_CR2 (read-write) Reset:0x0000
: USART1_CR2_ADD4_7 ( %bbbb -- x addr ) 28 lshift USART1_CR2 ; \ USART1_CR2_ADD4_7, Address of the USART node
: USART1_CR2_ADD0_3 ( %bbbb -- x addr ) 24 lshift USART1_CR2 ; \ USART1_CR2_ADD0_3, Address of the USART node
: USART1_CR2_RTOEN ( -- x addr ) 23 bit USART1_CR2 ; \ USART1_CR2_RTOEN, Receiver timeout enable
: USART1_CR2_ABRMOD1 ( -- x addr ) 22 bit USART1_CR2 ; \ USART1_CR2_ABRMOD1, Auto baud rate mode
: USART1_CR2_ABRMOD0 ( -- x addr ) 21 bit USART1_CR2 ; \ USART1_CR2_ABRMOD0, ABRMOD0
: USART1_CR2_ABREN ( -- x addr ) 20 bit USART1_CR2 ; \ USART1_CR2_ABREN, Auto baud rate enable
: USART1_CR2_MSBFIRST ( -- x addr ) 19 bit USART1_CR2 ; \ USART1_CR2_MSBFIRST, Most significant bit first
: USART1_CR2_TAINV ( -- x addr ) 18 bit USART1_CR2 ; \ USART1_CR2_TAINV, Binary data inversion
: USART1_CR2_TXINV ( -- x addr ) 17 bit USART1_CR2 ; \ USART1_CR2_TXINV, TX pin active level  inversion
: USART1_CR2_RXINV ( -- x addr ) 16 bit USART1_CR2 ; \ USART1_CR2_RXINV, RX pin active level  inversion
: USART1_CR2_SWAP ( -- x addr ) 15 bit USART1_CR2 ; \ USART1_CR2_SWAP, Swap TX/RX pins
: USART1_CR2_LINEN ( -- x addr ) 14 bit USART1_CR2 ; \ USART1_CR2_LINEN, LIN mode enable
: USART1_CR2_STOP ( %bb -- x addr ) 12 lshift USART1_CR2 ; \ USART1_CR2_STOP, STOP bits
: USART1_CR2_CLKEN ( -- x addr ) 11 bit USART1_CR2 ; \ USART1_CR2_CLKEN, Clock enable
: USART1_CR2_CPOL ( -- x addr ) 10 bit USART1_CR2 ; \ USART1_CR2_CPOL, Clock polarity
: USART1_CR2_CPHA ( -- x addr ) 9 bit USART1_CR2 ; \ USART1_CR2_CPHA, Clock phase
: USART1_CR2_LBCL ( -- x addr ) 8 bit USART1_CR2 ; \ USART1_CR2_LBCL, Last bit clock pulse
: USART1_CR2_LBDIE ( -- x addr ) 6 bit USART1_CR2 ; \ USART1_CR2_LBDIE, LIN break detection interrupt  enable
: USART1_CR2_LBDL ( -- x addr ) 5 bit USART1_CR2 ; \ USART1_CR2_LBDL, LIN break detection length
: USART1_CR2_ADDM7 ( -- x addr ) 4 bit USART1_CR2 ; \ USART1_CR2_ADDM7, 7-bit Address Detection/4-bit Address  Detection
[then]

defined? use-USART1 defined? USART1_CR3_WUFIE not and [if]
\ USART1_CR3 (read-write) Reset:0x0000
: USART1_CR3_WUFIE ( -- x addr ) 22 bit USART1_CR3 ; \ USART1_CR3_WUFIE, Wakeup from Stop mode interrupt  enable
: USART1_CR3_WUS ( %bb -- x addr ) 20 lshift USART1_CR3 ; \ USART1_CR3_WUS, Wakeup from Stop mode interrupt flag  selection
: USART1_CR3_SCARCNT ( %bbb -- x addr ) 17 lshift USART1_CR3 ; \ USART1_CR3_SCARCNT, Smartcard auto-retry count
: USART1_CR3_DEP ( -- x addr ) 15 bit USART1_CR3 ; \ USART1_CR3_DEP, Driver enable polarity  selection
: USART1_CR3_DEM ( -- x addr ) 14 bit USART1_CR3 ; \ USART1_CR3_DEM, Driver enable mode
: USART1_CR3_DDRE ( -- x addr ) 13 bit USART1_CR3 ; \ USART1_CR3_DDRE, DMA Disable on Reception  Error
: USART1_CR3_OVRDIS ( -- x addr ) 12 bit USART1_CR3 ; \ USART1_CR3_OVRDIS, Overrun Disable
: USART1_CR3_ONEBIT ( -- x addr ) 11 bit USART1_CR3 ; \ USART1_CR3_ONEBIT, One sample bit method  enable
: USART1_CR3_CTSIE ( -- x addr ) 10 bit USART1_CR3 ; \ USART1_CR3_CTSIE, CTS interrupt enable
: USART1_CR3_CTSE ( -- x addr ) 9 bit USART1_CR3 ; \ USART1_CR3_CTSE, CTS enable
: USART1_CR3_RTSE ( -- x addr ) 8 bit USART1_CR3 ; \ USART1_CR3_RTSE, RTS enable
: USART1_CR3_DMAT ( -- x addr ) 7 bit USART1_CR3 ; \ USART1_CR3_DMAT, DMA enable transmitter
: USART1_CR3_DMAR ( -- x addr ) 6 bit USART1_CR3 ; \ USART1_CR3_DMAR, DMA enable receiver
: USART1_CR3_SCEN ( -- x addr ) 5 bit USART1_CR3 ; \ USART1_CR3_SCEN, Smartcard mode enable
: USART1_CR3_NACK ( -- x addr ) 4 bit USART1_CR3 ; \ USART1_CR3_NACK, Smartcard NACK enable
: USART1_CR3_HDSEL ( -- x addr ) 3 bit USART1_CR3 ; \ USART1_CR3_HDSEL, Half-duplex selection
: USART1_CR3_IRLP ( -- x addr ) 2 bit USART1_CR3 ; \ USART1_CR3_IRLP, Ir low-power
: USART1_CR3_IREN ( -- x addr ) 1 bit USART1_CR3 ; \ USART1_CR3_IREN, Ir mode enable
: USART1_CR3_EIE ( -- x addr ) 0 bit USART1_CR3 ; \ USART1_CR3_EIE, Error interrupt enable
[then]

execute-defined? use-USART1 defined? USART1_BRR_DIV_Mantissa not and [if]
\ USART1_BRR (read-write) Reset:0x0000
: USART1_BRR_DIV_Mantissa ( %bbbbbbbbbbb -- x addr ) 4 lshift USART1_BRR ; \ USART1_BRR_DIV_Mantissa, DIV_Mantissa
: USART1_BRR_DIV_Fraction ( %bbbb -- x addr ) USART1_BRR ; \ USART1_BRR_DIV_Fraction, DIV_Fraction
[then]

defined? use-USART1 defined? USART1_GTPR_GT not and [if]
\ USART1_GTPR (read-write) Reset:0x0000
: USART1_GTPR_GT ( %bbbbbbbb -- x addr ) 8 lshift USART1_GTPR ; \ USART1_GTPR_GT, Guard time value
: USART1_GTPR_PSC ( %bbbbbbbb -- x addr ) USART1_GTPR ; \ USART1_GTPR_PSC, Prescaler value
[then]

execute-defined? use-USART1 defined? USART1_RTOR_BLEN not and [if]
\ USART1_RTOR (read-write) Reset:0x0000
: USART1_RTOR_BLEN ( %bbbbbbbb -- x addr ) 24 lshift USART1_RTOR ; \ USART1_RTOR_BLEN, Block Length
: USART1_RTOR_RTO ( %bbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) USART1_RTOR ; \ USART1_RTOR_RTO, Receiver timeout value
[then]

defined? use-USART1 defined? USART1_RQR_TXFRQ not and [if]
\ USART1_RQR (write-only) Reset:0x0000
: USART1_RQR_TXFRQ ( -- x addr ) 4 bit USART1_RQR ; \ USART1_RQR_TXFRQ, Transmit data flush  request
: USART1_RQR_RXFRQ ( -- x addr ) 3 bit USART1_RQR ; \ USART1_RQR_RXFRQ, Receive data flush request
: USART1_RQR_MMRQ ( -- x addr ) 2 bit USART1_RQR ; \ USART1_RQR_MMRQ, Mute mode request
: USART1_RQR_SBKRQ ( -- x addr ) 1 bit USART1_RQR ; \ USART1_RQR_SBKRQ, Send break request
: USART1_RQR_ABRRQ ( -- x addr ) 0 bit USART1_RQR ; \ USART1_RQR_ABRRQ, Auto baud rate request
[then]

execute-defined? use-USART1 defined? USART1_ISR_REACK? not and [if]
\ USART1_ISR (read-only) Reset:0x00C0
: USART1_ISR_REACK? ( --  1|0 ) 22 bit USART1_ISR bit@ ; \ USART1_ISR_REACK, REACK
: USART1_ISR_TEACK? ( --  1|0 ) 21 bit USART1_ISR bit@ ; \ USART1_ISR_TEACK, TEACK
: USART1_ISR_WUF? ( --  1|0 ) 20 bit USART1_ISR bit@ ; \ USART1_ISR_WUF, WUF
: USART1_ISR_RWU? ( --  1|0 ) 19 bit USART1_ISR bit@ ; \ USART1_ISR_RWU, RWU
: USART1_ISR_SBKF? ( --  1|0 ) 18 bit USART1_ISR bit@ ; \ USART1_ISR_SBKF, SBKF
: USART1_ISR_CMF? ( --  1|0 ) 17 bit USART1_ISR bit@ ; \ USART1_ISR_CMF, CMF
: USART1_ISR_BUSY? ( --  1|0 ) 16 bit USART1_ISR bit@ ; \ USART1_ISR_BUSY, BUSY
: USART1_ISR_ABRF? ( --  1|0 ) 15 bit USART1_ISR bit@ ; \ USART1_ISR_ABRF, ABRF
: USART1_ISR_ABRE? ( --  1|0 ) 14 bit USART1_ISR bit@ ; \ USART1_ISR_ABRE, ABRE
: USART1_ISR_EOBF? ( --  1|0 ) 12 bit USART1_ISR bit@ ; \ USART1_ISR_EOBF, EOBF
: USART1_ISR_RTOF? ( --  1|0 ) 11 bit USART1_ISR bit@ ; \ USART1_ISR_RTOF, RTOF
: USART1_ISR_CTS? ( --  1|0 ) 10 bit USART1_ISR bit@ ; \ USART1_ISR_CTS, CTS
: USART1_ISR_CTSIF? ( --  1|0 ) 9 bit USART1_ISR bit@ ; \ USART1_ISR_CTSIF, CTSIF
: USART1_ISR_LBDF? ( --  1|0 ) 8 bit USART1_ISR bit@ ; \ USART1_ISR_LBDF, LBDF
: USART1_ISR_TXE? ( --  1|0 ) 7 bit USART1_ISR bit@ ; \ USART1_ISR_TXE, TXE
: USART1_ISR_TC? ( --  1|0 ) 6 bit USART1_ISR bit@ ; \ USART1_ISR_TC, TC
: USART1_ISR_RXNE? ( --  1|0 ) 5 bit USART1_ISR bit@ ; \ USART1_ISR_RXNE, RXNE
: USART1_ISR_IDLE? ( --  1|0 ) 4 bit USART1_ISR bit@ ; \ USART1_ISR_IDLE, IDLE
: USART1_ISR_ORE? ( --  1|0 ) 3 bit USART1_ISR bit@ ; \ USART1_ISR_ORE, ORE
: USART1_ISR_NF? ( --  1|0 ) 2 bit USART1_ISR bit@ ; \ USART1_ISR_NF, NF
: USART1_ISR_FE? ( --  1|0 ) 1 bit USART1_ISR bit@ ; \ USART1_ISR_FE, FE
: USART1_ISR_PE? ( --  1|0 ) 0 bit USART1_ISR bit@ ; \ USART1_ISR_PE, PE
[then]

defined? use-USART1 defined? USART1_ICR_WUCF not and [if]
\ USART1_ICR (write-only) Reset:0x0000
: USART1_ICR_WUCF ( -- x addr ) 20 bit USART1_ICR ; \ USART1_ICR_WUCF, Wakeup from Stop mode clear  flag
: USART1_ICR_CMCF ( -- x addr ) 17 bit USART1_ICR ; \ USART1_ICR_CMCF, Character match clear flag
: USART1_ICR_EOBCF ( -- x addr ) 12 bit USART1_ICR ; \ USART1_ICR_EOBCF, End of block clear flag
: USART1_ICR_RTOCF ( -- x addr ) 11 bit USART1_ICR ; \ USART1_ICR_RTOCF, Receiver timeout clear  flag
: USART1_ICR_CTSCF ( -- x addr ) 9 bit USART1_ICR ; \ USART1_ICR_CTSCF, CTS clear flag
: USART1_ICR_LBDCF ( -- x addr ) 8 bit USART1_ICR ; \ USART1_ICR_LBDCF, LIN break detection clear  flag
: USART1_ICR_TCCF ( -- x addr ) 6 bit USART1_ICR ; \ USART1_ICR_TCCF, Transmission complete clear  flag
: USART1_ICR_IDLECF ( -- x addr ) 4 bit USART1_ICR ; \ USART1_ICR_IDLECF, Idle line detected clear  flag
: USART1_ICR_ORECF ( -- x addr ) 3 bit USART1_ICR ; \ USART1_ICR_ORECF, Overrun error clear flag
: USART1_ICR_NCF ( -- x addr ) 2 bit USART1_ICR ; \ USART1_ICR_NCF, Noise detected clear flag
: USART1_ICR_FECF ( -- x addr ) 1 bit USART1_ICR ; \ USART1_ICR_FECF, Framing error clear flag
: USART1_ICR_PECF ( -- x addr ) 0 bit USART1_ICR ; \ USART1_ICR_PECF, Parity error clear flag
[then]

execute-defined? use-USART1 defined? USART1_RDR_RDR? not and [if]
\ USART1_RDR (read-only) Reset:0x0000
: USART1_RDR_RDR? ( --  x ) USART1_RDR @ ; \ USART1_RDR_RDR, Receive data value
[then]

defined? use-USART1 defined? USART1_TDR_TDR not and [if]
\ USART1_TDR (read-write) Reset:0x0000
: USART1_TDR_TDR ( %bbbbbbbbb -- x addr ) USART1_TDR ; \ USART1_TDR_TDR, Transmit data value
[then]

execute-defined? use-USART2 defined? USART2_CR1_M1 not and [if]
\ USART2_CR1 (read-write) Reset:0x0000
: USART2_CR1_M1 ( -- x addr ) 28 bit USART2_CR1 ; \ USART2_CR1_M1, Word length
: USART2_CR1_EOBIE ( -- x addr ) 27 bit USART2_CR1 ; \ USART2_CR1_EOBIE, End of Block interrupt  enable
: USART2_CR1_RTOIE ( -- x addr ) 26 bit USART2_CR1 ; \ USART2_CR1_RTOIE, Receiver timeout interrupt  enable
: USART2_CR1_DEAT4 ( -- x addr ) 25 bit USART2_CR1 ; \ USART2_CR1_DEAT4, Driver Enable assertion  time
: USART2_CR1_DEAT3 ( -- x addr ) 24 bit USART2_CR1 ; \ USART2_CR1_DEAT3, DEAT3
: USART2_CR1_DEAT2 ( -- x addr ) 23 bit USART2_CR1 ; \ USART2_CR1_DEAT2, DEAT2
: USART2_CR1_DEAT1 ( -- x addr ) 22 bit USART2_CR1 ; \ USART2_CR1_DEAT1, DEAT1
: USART2_CR1_DEAT0 ( -- x addr ) 21 bit USART2_CR1 ; \ USART2_CR1_DEAT0, DEAT0
: USART2_CR1_DEDT4 ( -- x addr ) 20 bit USART2_CR1 ; \ USART2_CR1_DEDT4, Driver Enable de-assertion  time
: USART2_CR1_DEDT3 ( -- x addr ) 19 bit USART2_CR1 ; \ USART2_CR1_DEDT3, DEDT3
: USART2_CR1_DEDT2 ( -- x addr ) 18 bit USART2_CR1 ; \ USART2_CR1_DEDT2, DEDT2
: USART2_CR1_DEDT1 ( -- x addr ) 17 bit USART2_CR1 ; \ USART2_CR1_DEDT1, DEDT1
: USART2_CR1_DEDT0 ( -- x addr ) 16 bit USART2_CR1 ; \ USART2_CR1_DEDT0, DEDT0
: USART2_CR1_OVER8 ( -- x addr ) 15 bit USART2_CR1 ; \ USART2_CR1_OVER8, Oversampling mode
: USART2_CR1_CMIE ( -- x addr ) 14 bit USART2_CR1 ; \ USART2_CR1_CMIE, Character match interrupt  enable
: USART2_CR1_MME ( -- x addr ) 13 bit USART2_CR1 ; \ USART2_CR1_MME, Mute mode enable
: USART2_CR1_M0 ( -- x addr ) 12 bit USART2_CR1 ; \ USART2_CR1_M0, Word length
: USART2_CR1_WAKE ( -- x addr ) 11 bit USART2_CR1 ; \ USART2_CR1_WAKE, Receiver wakeup method
: USART2_CR1_PCE ( -- x addr ) 10 bit USART2_CR1 ; \ USART2_CR1_PCE, Parity control enable
: USART2_CR1_PS ( -- x addr ) 9 bit USART2_CR1 ; \ USART2_CR1_PS, Parity selection
: USART2_CR1_PEIE ( -- x addr ) 8 bit USART2_CR1 ; \ USART2_CR1_PEIE, PE interrupt enable
: USART2_CR1_TXEIE ( -- x addr ) 7 bit USART2_CR1 ; \ USART2_CR1_TXEIE, interrupt enable
: USART2_CR1_TCIE ( -- x addr ) 6 bit USART2_CR1 ; \ USART2_CR1_TCIE, Transmission complete interrupt  enable
: USART2_CR1_RXNEIE ( -- x addr ) 5 bit USART2_CR1 ; \ USART2_CR1_RXNEIE, RXNE interrupt enable
: USART2_CR1_IDLEIE ( -- x addr ) 4 bit USART2_CR1 ; \ USART2_CR1_IDLEIE, IDLE interrupt enable
: USART2_CR1_TE ( -- x addr ) 3 bit USART2_CR1 ; \ USART2_CR1_TE, Transmitter enable
: USART2_CR1_RE ( -- x addr ) 2 bit USART2_CR1 ; \ USART2_CR1_RE, Receiver enable
: USART2_CR1_UESM ( -- x addr ) 1 bit USART2_CR1 ; \ USART2_CR1_UESM, USART enable in Stop mode
: USART2_CR1_UE ( -- x addr ) 0 bit USART2_CR1 ; \ USART2_CR1_UE, USART enable
[then]

defined? use-USART2 defined? USART2_CR2_ADD4_7 not and [if]
\ USART2_CR2 (read-write) Reset:0x0000
: USART2_CR2_ADD4_7 ( %bbbb -- x addr ) 28 lshift USART2_CR2 ; \ USART2_CR2_ADD4_7, Address of the USART node
: USART2_CR2_ADD0_3 ( %bbbb -- x addr ) 24 lshift USART2_CR2 ; \ USART2_CR2_ADD0_3, Address of the USART node
: USART2_CR2_RTOEN ( -- x addr ) 23 bit USART2_CR2 ; \ USART2_CR2_RTOEN, Receiver timeout enable
: USART2_CR2_ABRMOD1 ( -- x addr ) 22 bit USART2_CR2 ; \ USART2_CR2_ABRMOD1, Auto baud rate mode
: USART2_CR2_ABRMOD0 ( -- x addr ) 21 bit USART2_CR2 ; \ USART2_CR2_ABRMOD0, ABRMOD0
: USART2_CR2_ABREN ( -- x addr ) 20 bit USART2_CR2 ; \ USART2_CR2_ABREN, Auto baud rate enable
: USART2_CR2_MSBFIRST ( -- x addr ) 19 bit USART2_CR2 ; \ USART2_CR2_MSBFIRST, Most significant bit first
: USART2_CR2_TAINV ( -- x addr ) 18 bit USART2_CR2 ; \ USART2_CR2_TAINV, Binary data inversion
: USART2_CR2_TXINV ( -- x addr ) 17 bit USART2_CR2 ; \ USART2_CR2_TXINV, TX pin active level  inversion
: USART2_CR2_RXINV ( -- x addr ) 16 bit USART2_CR2 ; \ USART2_CR2_RXINV, RX pin active level  inversion
: USART2_CR2_SWAP ( -- x addr ) 15 bit USART2_CR2 ; \ USART2_CR2_SWAP, Swap TX/RX pins
: USART2_CR2_LINEN ( -- x addr ) 14 bit USART2_CR2 ; \ USART2_CR2_LINEN, LIN mode enable
: USART2_CR2_STOP ( %bb -- x addr ) 12 lshift USART2_CR2 ; \ USART2_CR2_STOP, STOP bits
: USART2_CR2_CLKEN ( -- x addr ) 11 bit USART2_CR2 ; \ USART2_CR2_CLKEN, Clock enable
: USART2_CR2_CPOL ( -- x addr ) 10 bit USART2_CR2 ; \ USART2_CR2_CPOL, Clock polarity
: USART2_CR2_CPHA ( -- x addr ) 9 bit USART2_CR2 ; \ USART2_CR2_CPHA, Clock phase
: USART2_CR2_LBCL ( -- x addr ) 8 bit USART2_CR2 ; \ USART2_CR2_LBCL, Last bit clock pulse
: USART2_CR2_LBDIE ( -- x addr ) 6 bit USART2_CR2 ; \ USART2_CR2_LBDIE, LIN break detection interrupt  enable
: USART2_CR2_LBDL ( -- x addr ) 5 bit USART2_CR2 ; \ USART2_CR2_LBDL, LIN break detection length
: USART2_CR2_ADDM7 ( -- x addr ) 4 bit USART2_CR2 ; \ USART2_CR2_ADDM7, 7-bit Address Detection/4-bit Address  Detection
[then]

execute-defined? use-USART2 defined? USART2_CR3_WUFIE not and [if]
\ USART2_CR3 (read-write) Reset:0x0000
: USART2_CR3_WUFIE ( -- x addr ) 22 bit USART2_CR3 ; \ USART2_CR3_WUFIE, Wakeup from Stop mode interrupt  enable
: USART2_CR3_WUS ( %bb -- x addr ) 20 lshift USART2_CR3 ; \ USART2_CR3_WUS, Wakeup from Stop mode interrupt flag  selection
: USART2_CR3_SCARCNT ( %bbb -- x addr ) 17 lshift USART2_CR3 ; \ USART2_CR3_SCARCNT, Smartcard auto-retry count
: USART2_CR3_DEP ( -- x addr ) 15 bit USART2_CR3 ; \ USART2_CR3_DEP, Driver enable polarity  selection
: USART2_CR3_DEM ( -- x addr ) 14 bit USART2_CR3 ; \ USART2_CR3_DEM, Driver enable mode
: USART2_CR3_DDRE ( -- x addr ) 13 bit USART2_CR3 ; \ USART2_CR3_DDRE, DMA Disable on Reception  Error
: USART2_CR3_OVRDIS ( -- x addr ) 12 bit USART2_CR3 ; \ USART2_CR3_OVRDIS, Overrun Disable
: USART2_CR3_ONEBIT ( -- x addr ) 11 bit USART2_CR3 ; \ USART2_CR3_ONEBIT, One sample bit method  enable
: USART2_CR3_CTSIE ( -- x addr ) 10 bit USART2_CR3 ; \ USART2_CR3_CTSIE, CTS interrupt enable
: USART2_CR3_CTSE ( -- x addr ) 9 bit USART2_CR3 ; \ USART2_CR3_CTSE, CTS enable
: USART2_CR3_RTSE ( -- x addr ) 8 bit USART2_CR3 ; \ USART2_CR3_RTSE, RTS enable
: USART2_CR3_DMAT ( -- x addr ) 7 bit USART2_CR3 ; \ USART2_CR3_DMAT, DMA enable transmitter
: USART2_CR3_DMAR ( -- x addr ) 6 bit USART2_CR3 ; \ USART2_CR3_DMAR, DMA enable receiver
: USART2_CR3_SCEN ( -- x addr ) 5 bit USART2_CR3 ; \ USART2_CR3_SCEN, Smartcard mode enable
: USART2_CR3_NACK ( -- x addr ) 4 bit USART2_CR3 ; \ USART2_CR3_NACK, Smartcard NACK enable
: USART2_CR3_HDSEL ( -- x addr ) 3 bit USART2_CR3 ; \ USART2_CR3_HDSEL, Half-duplex selection
: USART2_CR3_IRLP ( -- x addr ) 2 bit USART2_CR3 ; \ USART2_CR3_IRLP, Ir low-power
: USART2_CR3_IREN ( -- x addr ) 1 bit USART2_CR3 ; \ USART2_CR3_IREN, Ir mode enable
: USART2_CR3_EIE ( -- x addr ) 0 bit USART2_CR3 ; \ USART2_CR3_EIE, Error interrupt enable
[then]

defined? use-USART2 defined? USART2_BRR_DIV_Mantissa not and [if]
\ USART2_BRR (read-write) Reset:0x0000
: USART2_BRR_DIV_Mantissa ( %bbbbbbbbbbb -- x addr ) 4 lshift USART2_BRR ; \ USART2_BRR_DIV_Mantissa, DIV_Mantissa
: USART2_BRR_DIV_Fraction ( %bbbb -- x addr ) USART2_BRR ; \ USART2_BRR_DIV_Fraction, DIV_Fraction
[then]

execute-defined? use-USART2 defined? USART2_GTPR_GT not and [if]
\ USART2_GTPR (read-write) Reset:0x0000
: USART2_GTPR_GT ( %bbbbbbbb -- x addr ) 8 lshift USART2_GTPR ; \ USART2_GTPR_GT, Guard time value
: USART2_GTPR_PSC ( %bbbbbbbb -- x addr ) USART2_GTPR ; \ USART2_GTPR_PSC, Prescaler value
[then]

defined? use-USART2 defined? USART2_RTOR_BLEN not and [if]
\ USART2_RTOR (read-write) Reset:0x0000
: USART2_RTOR_BLEN ( %bbbbbbbb -- x addr ) 24 lshift USART2_RTOR ; \ USART2_RTOR_BLEN, Block Length
: USART2_RTOR_RTO ( %bbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) USART2_RTOR ; \ USART2_RTOR_RTO, Receiver timeout value
[then]

execute-defined? use-USART2 defined? USART2_RQR_TXFRQ not and [if]
\ USART2_RQR (write-only) Reset:0x0000
: USART2_RQR_TXFRQ ( -- x addr ) 4 bit USART2_RQR ; \ USART2_RQR_TXFRQ, Transmit data flush  request
: USART2_RQR_RXFRQ ( -- x addr ) 3 bit USART2_RQR ; \ USART2_RQR_RXFRQ, Receive data flush request
: USART2_RQR_MMRQ ( -- x addr ) 2 bit USART2_RQR ; \ USART2_RQR_MMRQ, Mute mode request
: USART2_RQR_SBKRQ ( -- x addr ) 1 bit USART2_RQR ; \ USART2_RQR_SBKRQ, Send break request
: USART2_RQR_ABRRQ ( -- x addr ) 0 bit USART2_RQR ; \ USART2_RQR_ABRRQ, Auto baud rate request
[then]

defined? use-USART2 defined? USART2_ISR_REACK? not and [if]
\ USART2_ISR (read-only) Reset:0x00C0
: USART2_ISR_REACK? ( --  1|0 ) 22 bit USART2_ISR bit@ ; \ USART2_ISR_REACK, REACK
: USART2_ISR_TEACK? ( --  1|0 ) 21 bit USART2_ISR bit@ ; \ USART2_ISR_TEACK, TEACK
: USART2_ISR_WUF? ( --  1|0 ) 20 bit USART2_ISR bit@ ; \ USART2_ISR_WUF, WUF
: USART2_ISR_RWU? ( --  1|0 ) 19 bit USART2_ISR bit@ ; \ USART2_ISR_RWU, RWU
: USART2_ISR_SBKF? ( --  1|0 ) 18 bit USART2_ISR bit@ ; \ USART2_ISR_SBKF, SBKF
: USART2_ISR_CMF? ( --  1|0 ) 17 bit USART2_ISR bit@ ; \ USART2_ISR_CMF, CMF
: USART2_ISR_BUSY? ( --  1|0 ) 16 bit USART2_ISR bit@ ; \ USART2_ISR_BUSY, BUSY
: USART2_ISR_ABRF? ( --  1|0 ) 15 bit USART2_ISR bit@ ; \ USART2_ISR_ABRF, ABRF
: USART2_ISR_ABRE? ( --  1|0 ) 14 bit USART2_ISR bit@ ; \ USART2_ISR_ABRE, ABRE
: USART2_ISR_EOBF? ( --  1|0 ) 12 bit USART2_ISR bit@ ; \ USART2_ISR_EOBF, EOBF
: USART2_ISR_RTOF? ( --  1|0 ) 11 bit USART2_ISR bit@ ; \ USART2_ISR_RTOF, RTOF
: USART2_ISR_CTS? ( --  1|0 ) 10 bit USART2_ISR bit@ ; \ USART2_ISR_CTS, CTS
: USART2_ISR_CTSIF? ( --  1|0 ) 9 bit USART2_ISR bit@ ; \ USART2_ISR_CTSIF, CTSIF
: USART2_ISR_LBDF? ( --  1|0 ) 8 bit USART2_ISR bit@ ; \ USART2_ISR_LBDF, LBDF
: USART2_ISR_TXE? ( --  1|0 ) 7 bit USART2_ISR bit@ ; \ USART2_ISR_TXE, TXE
: USART2_ISR_TC? ( --  1|0 ) 6 bit USART2_ISR bit@ ; \ USART2_ISR_TC, TC
: USART2_ISR_RXNE? ( --  1|0 ) 5 bit USART2_ISR bit@ ; \ USART2_ISR_RXNE, RXNE
: USART2_ISR_IDLE? ( --  1|0 ) 4 bit USART2_ISR bit@ ; \ USART2_ISR_IDLE, IDLE
: USART2_ISR_ORE? ( --  1|0 ) 3 bit USART2_ISR bit@ ; \ USART2_ISR_ORE, ORE
: USART2_ISR_NF? ( --  1|0 ) 2 bit USART2_ISR bit@ ; \ USART2_ISR_NF, NF
: USART2_ISR_FE? ( --  1|0 ) 1 bit USART2_ISR bit@ ; \ USART2_ISR_FE, FE
: USART2_ISR_PE? ( --  1|0 ) 0 bit USART2_ISR bit@ ; \ USART2_ISR_PE, PE
[then]

execute-defined? use-USART2 defined? USART2_ICR_WUCF not and [if]
\ USART2_ICR (write-only) Reset:0x0000
: USART2_ICR_WUCF ( -- x addr ) 20 bit USART2_ICR ; \ USART2_ICR_WUCF, Wakeup from Stop mode clear  flag
: USART2_ICR_CMCF ( -- x addr ) 17 bit USART2_ICR ; \ USART2_ICR_CMCF, Character match clear flag
: USART2_ICR_EOBCF ( -- x addr ) 12 bit USART2_ICR ; \ USART2_ICR_EOBCF, End of block clear flag
: USART2_ICR_RTOCF ( -- x addr ) 11 bit USART2_ICR ; \ USART2_ICR_RTOCF, Receiver timeout clear  flag
: USART2_ICR_CTSCF ( -- x addr ) 9 bit USART2_ICR ; \ USART2_ICR_CTSCF, CTS clear flag
: USART2_ICR_LBDCF ( -- x addr ) 8 bit USART2_ICR ; \ USART2_ICR_LBDCF, LIN break detection clear  flag
: USART2_ICR_TCCF ( -- x addr ) 6 bit USART2_ICR ; \ USART2_ICR_TCCF, Transmission complete clear  flag
: USART2_ICR_IDLECF ( -- x addr ) 4 bit USART2_ICR ; \ USART2_ICR_IDLECF, Idle line detected clear  flag
: USART2_ICR_ORECF ( -- x addr ) 3 bit USART2_ICR ; \ USART2_ICR_ORECF, Overrun error clear flag
: USART2_ICR_NCF ( -- x addr ) 2 bit USART2_ICR ; \ USART2_ICR_NCF, Noise detected clear flag
: USART2_ICR_FECF ( -- x addr ) 1 bit USART2_ICR ; \ USART2_ICR_FECF, Framing error clear flag
: USART2_ICR_PECF ( -- x addr ) 0 bit USART2_ICR ; \ USART2_ICR_PECF, Parity error clear flag
[then]

defined? use-USART2 defined? USART2_RDR_RDR? not and [if]
\ USART2_RDR (read-only) Reset:0x0000
: USART2_RDR_RDR? ( --  x ) USART2_RDR @ ; \ USART2_RDR_RDR, Receive data value
[then]

execute-defined? use-USART2 defined? USART2_TDR_TDR not and [if]
\ USART2_TDR (read-write) Reset:0x0000
: USART2_TDR_TDR ( %bbbbbbbbb -- x addr ) USART2_TDR ; \ USART2_TDR_TDR, Transmit data value
[then]

defined? use-USART3 defined? USART3_CR1_M1 not and [if]
\ USART3_CR1 (read-write) Reset:0x0000
: USART3_CR1_M1 ( -- x addr ) 28 bit USART3_CR1 ; \ USART3_CR1_M1, Word length
: USART3_CR1_EOBIE ( -- x addr ) 27 bit USART3_CR1 ; \ USART3_CR1_EOBIE, End of Block interrupt  enable
: USART3_CR1_RTOIE ( -- x addr ) 26 bit USART3_CR1 ; \ USART3_CR1_RTOIE, Receiver timeout interrupt  enable
: USART3_CR1_DEAT4 ( -- x addr ) 25 bit USART3_CR1 ; \ USART3_CR1_DEAT4, Driver Enable assertion  time
: USART3_CR1_DEAT3 ( -- x addr ) 24 bit USART3_CR1 ; \ USART3_CR1_DEAT3, DEAT3
: USART3_CR1_DEAT2 ( -- x addr ) 23 bit USART3_CR1 ; \ USART3_CR1_DEAT2, DEAT2
: USART3_CR1_DEAT1 ( -- x addr ) 22 bit USART3_CR1 ; \ USART3_CR1_DEAT1, DEAT1
: USART3_CR1_DEAT0 ( -- x addr ) 21 bit USART3_CR1 ; \ USART3_CR1_DEAT0, DEAT0
: USART3_CR1_DEDT4 ( -- x addr ) 20 bit USART3_CR1 ; \ USART3_CR1_DEDT4, Driver Enable de-assertion  time
: USART3_CR1_DEDT3 ( -- x addr ) 19 bit USART3_CR1 ; \ USART3_CR1_DEDT3, DEDT3
: USART3_CR1_DEDT2 ( -- x addr ) 18 bit USART3_CR1 ; \ USART3_CR1_DEDT2, DEDT2
: USART3_CR1_DEDT1 ( -- x addr ) 17 bit USART3_CR1 ; \ USART3_CR1_DEDT1, DEDT1
: USART3_CR1_DEDT0 ( -- x addr ) 16 bit USART3_CR1 ; \ USART3_CR1_DEDT0, DEDT0
: USART3_CR1_OVER8 ( -- x addr ) 15 bit USART3_CR1 ; \ USART3_CR1_OVER8, Oversampling mode
: USART3_CR1_CMIE ( -- x addr ) 14 bit USART3_CR1 ; \ USART3_CR1_CMIE, Character match interrupt  enable
: USART3_CR1_MME ( -- x addr ) 13 bit USART3_CR1 ; \ USART3_CR1_MME, Mute mode enable
: USART3_CR1_M0 ( -- x addr ) 12 bit USART3_CR1 ; \ USART3_CR1_M0, Word length
: USART3_CR1_WAKE ( -- x addr ) 11 bit USART3_CR1 ; \ USART3_CR1_WAKE, Receiver wakeup method
: USART3_CR1_PCE ( -- x addr ) 10 bit USART3_CR1 ; \ USART3_CR1_PCE, Parity control enable
: USART3_CR1_PS ( -- x addr ) 9 bit USART3_CR1 ; \ USART3_CR1_PS, Parity selection
: USART3_CR1_PEIE ( -- x addr ) 8 bit USART3_CR1 ; \ USART3_CR1_PEIE, PE interrupt enable
: USART3_CR1_TXEIE ( -- x addr ) 7 bit USART3_CR1 ; \ USART3_CR1_TXEIE, interrupt enable
: USART3_CR1_TCIE ( -- x addr ) 6 bit USART3_CR1 ; \ USART3_CR1_TCIE, Transmission complete interrupt  enable
: USART3_CR1_RXNEIE ( -- x addr ) 5 bit USART3_CR1 ; \ USART3_CR1_RXNEIE, RXNE interrupt enable
: USART3_CR1_IDLEIE ( -- x addr ) 4 bit USART3_CR1 ; \ USART3_CR1_IDLEIE, IDLE interrupt enable
: USART3_CR1_TE ( -- x addr ) 3 bit USART3_CR1 ; \ USART3_CR1_TE, Transmitter enable
: USART3_CR1_RE ( -- x addr ) 2 bit USART3_CR1 ; \ USART3_CR1_RE, Receiver enable
: USART3_CR1_UESM ( -- x addr ) 1 bit USART3_CR1 ; \ USART3_CR1_UESM, USART enable in Stop mode
: USART3_CR1_UE ( -- x addr ) 0 bit USART3_CR1 ; \ USART3_CR1_UE, USART enable
[then]

execute-defined? use-USART3 defined? USART3_CR2_ADD4_7 not and [if]
\ USART3_CR2 (read-write) Reset:0x0000
: USART3_CR2_ADD4_7 ( %bbbb -- x addr ) 28 lshift USART3_CR2 ; \ USART3_CR2_ADD4_7, Address of the USART node
: USART3_CR2_ADD0_3 ( %bbbb -- x addr ) 24 lshift USART3_CR2 ; \ USART3_CR2_ADD0_3, Address of the USART node
: USART3_CR2_RTOEN ( -- x addr ) 23 bit USART3_CR2 ; \ USART3_CR2_RTOEN, Receiver timeout enable
: USART3_CR2_ABRMOD1 ( -- x addr ) 22 bit USART3_CR2 ; \ USART3_CR2_ABRMOD1, Auto baud rate mode
: USART3_CR2_ABRMOD0 ( -- x addr ) 21 bit USART3_CR2 ; \ USART3_CR2_ABRMOD0, ABRMOD0
: USART3_CR2_ABREN ( -- x addr ) 20 bit USART3_CR2 ; \ USART3_CR2_ABREN, Auto baud rate enable
: USART3_CR2_MSBFIRST ( -- x addr ) 19 bit USART3_CR2 ; \ USART3_CR2_MSBFIRST, Most significant bit first
: USART3_CR2_TAINV ( -- x addr ) 18 bit USART3_CR2 ; \ USART3_CR2_TAINV, Binary data inversion
: USART3_CR2_TXINV ( -- x addr ) 17 bit USART3_CR2 ; \ USART3_CR2_TXINV, TX pin active level  inversion
: USART3_CR2_RXINV ( -- x addr ) 16 bit USART3_CR2 ; \ USART3_CR2_RXINV, RX pin active level  inversion
: USART3_CR2_SWAP ( -- x addr ) 15 bit USART3_CR2 ; \ USART3_CR2_SWAP, Swap TX/RX pins
: USART3_CR2_LINEN ( -- x addr ) 14 bit USART3_CR2 ; \ USART3_CR2_LINEN, LIN mode enable
: USART3_CR2_STOP ( %bb -- x addr ) 12 lshift USART3_CR2 ; \ USART3_CR2_STOP, STOP bits
: USART3_CR2_CLKEN ( -- x addr ) 11 bit USART3_CR2 ; \ USART3_CR2_CLKEN, Clock enable
: USART3_CR2_CPOL ( -- x addr ) 10 bit USART3_CR2 ; \ USART3_CR2_CPOL, Clock polarity
: USART3_CR2_CPHA ( -- x addr ) 9 bit USART3_CR2 ; \ USART3_CR2_CPHA, Clock phase
: USART3_CR2_LBCL ( -- x addr ) 8 bit USART3_CR2 ; \ USART3_CR2_LBCL, Last bit clock pulse
: USART3_CR2_LBDIE ( -- x addr ) 6 bit USART3_CR2 ; \ USART3_CR2_LBDIE, LIN break detection interrupt  enable
: USART3_CR2_LBDL ( -- x addr ) 5 bit USART3_CR2 ; \ USART3_CR2_LBDL, LIN break detection length
: USART3_CR2_ADDM7 ( -- x addr ) 4 bit USART3_CR2 ; \ USART3_CR2_ADDM7, 7-bit Address Detection/4-bit Address  Detection
[then]

defined? use-USART3 defined? USART3_CR3_WUFIE not and [if]
\ USART3_CR3 (read-write) Reset:0x0000
: USART3_CR3_WUFIE ( -- x addr ) 22 bit USART3_CR3 ; \ USART3_CR3_WUFIE, Wakeup from Stop mode interrupt  enable
: USART3_CR3_WUS ( %bb -- x addr ) 20 lshift USART3_CR3 ; \ USART3_CR3_WUS, Wakeup from Stop mode interrupt flag  selection
: USART3_CR3_SCARCNT ( %bbb -- x addr ) 17 lshift USART3_CR3 ; \ USART3_CR3_SCARCNT, Smartcard auto-retry count
: USART3_CR3_DEP ( -- x addr ) 15 bit USART3_CR3 ; \ USART3_CR3_DEP, Driver enable polarity  selection
: USART3_CR3_DEM ( -- x addr ) 14 bit USART3_CR3 ; \ USART3_CR3_DEM, Driver enable mode
: USART3_CR3_DDRE ( -- x addr ) 13 bit USART3_CR3 ; \ USART3_CR3_DDRE, DMA Disable on Reception  Error
: USART3_CR3_OVRDIS ( -- x addr ) 12 bit USART3_CR3 ; \ USART3_CR3_OVRDIS, Overrun Disable
: USART3_CR3_ONEBIT ( -- x addr ) 11 bit USART3_CR3 ; \ USART3_CR3_ONEBIT, One sample bit method  enable
: USART3_CR3_CTSIE ( -- x addr ) 10 bit USART3_CR3 ; \ USART3_CR3_CTSIE, CTS interrupt enable
: USART3_CR3_CTSE ( -- x addr ) 9 bit USART3_CR3 ; \ USART3_CR3_CTSE, CTS enable
: USART3_CR3_RTSE ( -- x addr ) 8 bit USART3_CR3 ; \ USART3_CR3_RTSE, RTS enable
: USART3_CR3_DMAT ( -- x addr ) 7 bit USART3_CR3 ; \ USART3_CR3_DMAT, DMA enable transmitter
: USART3_CR3_DMAR ( -- x addr ) 6 bit USART3_CR3 ; \ USART3_CR3_DMAR, DMA enable receiver
: USART3_CR3_SCEN ( -- x addr ) 5 bit USART3_CR3 ; \ USART3_CR3_SCEN, Smartcard mode enable
: USART3_CR3_NACK ( -- x addr ) 4 bit USART3_CR3 ; \ USART3_CR3_NACK, Smartcard NACK enable
: USART3_CR3_HDSEL ( -- x addr ) 3 bit USART3_CR3 ; \ USART3_CR3_HDSEL, Half-duplex selection
: USART3_CR3_IRLP ( -- x addr ) 2 bit USART3_CR3 ; \ USART3_CR3_IRLP, Ir low-power
: USART3_CR3_IREN ( -- x addr ) 1 bit USART3_CR3 ; \ USART3_CR3_IREN, Ir mode enable
: USART3_CR3_EIE ( -- x addr ) 0 bit USART3_CR3 ; \ USART3_CR3_EIE, Error interrupt enable
[then]

execute-defined? use-USART3 defined? USART3_BRR_DIV_Mantissa not and [if]
\ USART3_BRR (read-write) Reset:0x0000
: USART3_BRR_DIV_Mantissa ( %bbbbbbbbbbb -- x addr ) 4 lshift USART3_BRR ; \ USART3_BRR_DIV_Mantissa, DIV_Mantissa
: USART3_BRR_DIV_Fraction ( %bbbb -- x addr ) USART3_BRR ; \ USART3_BRR_DIV_Fraction, DIV_Fraction
[then]

defined? use-USART3 defined? USART3_GTPR_GT not and [if]
\ USART3_GTPR (read-write) Reset:0x0000
: USART3_GTPR_GT ( %bbbbbbbb -- x addr ) 8 lshift USART3_GTPR ; \ USART3_GTPR_GT, Guard time value
: USART3_GTPR_PSC ( %bbbbbbbb -- x addr ) USART3_GTPR ; \ USART3_GTPR_PSC, Prescaler value
[then]

execute-defined? use-USART3 defined? USART3_RTOR_BLEN not and [if]
\ USART3_RTOR (read-write) Reset:0x0000
: USART3_RTOR_BLEN ( %bbbbbbbb -- x addr ) 24 lshift USART3_RTOR ; \ USART3_RTOR_BLEN, Block Length
: USART3_RTOR_RTO ( %bbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) USART3_RTOR ; \ USART3_RTOR_RTO, Receiver timeout value
[then]

defined? use-USART3 defined? USART3_RQR_TXFRQ not and [if]
\ USART3_RQR (write-only) Reset:0x0000
: USART3_RQR_TXFRQ ( -- x addr ) 4 bit USART3_RQR ; \ USART3_RQR_TXFRQ, Transmit data flush  request
: USART3_RQR_RXFRQ ( -- x addr ) 3 bit USART3_RQR ; \ USART3_RQR_RXFRQ, Receive data flush request
: USART3_RQR_MMRQ ( -- x addr ) 2 bit USART3_RQR ; \ USART3_RQR_MMRQ, Mute mode request
: USART3_RQR_SBKRQ ( -- x addr ) 1 bit USART3_RQR ; \ USART3_RQR_SBKRQ, Send break request
: USART3_RQR_ABRRQ ( -- x addr ) 0 bit USART3_RQR ; \ USART3_RQR_ABRRQ, Auto baud rate request
[then]

execute-defined? use-USART3 defined? USART3_ISR_REACK? not and [if]
\ USART3_ISR (read-only) Reset:0x00C0
: USART3_ISR_REACK? ( --  1|0 ) 22 bit USART3_ISR bit@ ; \ USART3_ISR_REACK, REACK
: USART3_ISR_TEACK? ( --  1|0 ) 21 bit USART3_ISR bit@ ; \ USART3_ISR_TEACK, TEACK
: USART3_ISR_WUF? ( --  1|0 ) 20 bit USART3_ISR bit@ ; \ USART3_ISR_WUF, WUF
: USART3_ISR_RWU? ( --  1|0 ) 19 bit USART3_ISR bit@ ; \ USART3_ISR_RWU, RWU
: USART3_ISR_SBKF? ( --  1|0 ) 18 bit USART3_ISR bit@ ; \ USART3_ISR_SBKF, SBKF
: USART3_ISR_CMF? ( --  1|0 ) 17 bit USART3_ISR bit@ ; \ USART3_ISR_CMF, CMF
: USART3_ISR_BUSY? ( --  1|0 ) 16 bit USART3_ISR bit@ ; \ USART3_ISR_BUSY, BUSY
: USART3_ISR_ABRF? ( --  1|0 ) 15 bit USART3_ISR bit@ ; \ USART3_ISR_ABRF, ABRF
: USART3_ISR_ABRE? ( --  1|0 ) 14 bit USART3_ISR bit@ ; \ USART3_ISR_ABRE, ABRE
: USART3_ISR_EOBF? ( --  1|0 ) 12 bit USART3_ISR bit@ ; \ USART3_ISR_EOBF, EOBF
: USART3_ISR_RTOF? ( --  1|0 ) 11 bit USART3_ISR bit@ ; \ USART3_ISR_RTOF, RTOF
: USART3_ISR_CTS? ( --  1|0 ) 10 bit USART3_ISR bit@ ; \ USART3_ISR_CTS, CTS
: USART3_ISR_CTSIF? ( --  1|0 ) 9 bit USART3_ISR bit@ ; \ USART3_ISR_CTSIF, CTSIF
: USART3_ISR_LBDF? ( --  1|0 ) 8 bit USART3_ISR bit@ ; \ USART3_ISR_LBDF, LBDF
: USART3_ISR_TXE? ( --  1|0 ) 7 bit USART3_ISR bit@ ; \ USART3_ISR_TXE, TXE
: USART3_ISR_TC? ( --  1|0 ) 6 bit USART3_ISR bit@ ; \ USART3_ISR_TC, TC
: USART3_ISR_RXNE? ( --  1|0 ) 5 bit USART3_ISR bit@ ; \ USART3_ISR_RXNE, RXNE
: USART3_ISR_IDLE? ( --  1|0 ) 4 bit USART3_ISR bit@ ; \ USART3_ISR_IDLE, IDLE
: USART3_ISR_ORE? ( --  1|0 ) 3 bit USART3_ISR bit@ ; \ USART3_ISR_ORE, ORE
: USART3_ISR_NF? ( --  1|0 ) 2 bit USART3_ISR bit@ ; \ USART3_ISR_NF, NF
: USART3_ISR_FE? ( --  1|0 ) 1 bit USART3_ISR bit@ ; \ USART3_ISR_FE, FE
: USART3_ISR_PE? ( --  1|0 ) 0 bit USART3_ISR bit@ ; \ USART3_ISR_PE, PE
[then]

defined? use-USART3 defined? USART3_ICR_WUCF not and [if]
\ USART3_ICR (write-only) Reset:0x0000
: USART3_ICR_WUCF ( -- x addr ) 20 bit USART3_ICR ; \ USART3_ICR_WUCF, Wakeup from Stop mode clear  flag
: USART3_ICR_CMCF ( -- x addr ) 17 bit USART3_ICR ; \ USART3_ICR_CMCF, Character match clear flag
: USART3_ICR_EOBCF ( -- x addr ) 12 bit USART3_ICR ; \ USART3_ICR_EOBCF, End of block clear flag
: USART3_ICR_RTOCF ( -- x addr ) 11 bit USART3_ICR ; \ USART3_ICR_RTOCF, Receiver timeout clear  flag
: USART3_ICR_CTSCF ( -- x addr ) 9 bit USART3_ICR ; \ USART3_ICR_CTSCF, CTS clear flag
: USART3_ICR_LBDCF ( -- x addr ) 8 bit USART3_ICR ; \ USART3_ICR_LBDCF, LIN break detection clear  flag
: USART3_ICR_TCCF ( -- x addr ) 6 bit USART3_ICR ; \ USART3_ICR_TCCF, Transmission complete clear  flag
: USART3_ICR_IDLECF ( -- x addr ) 4 bit USART3_ICR ; \ USART3_ICR_IDLECF, Idle line detected clear  flag
: USART3_ICR_ORECF ( -- x addr ) 3 bit USART3_ICR ; \ USART3_ICR_ORECF, Overrun error clear flag
: USART3_ICR_NCF ( -- x addr ) 2 bit USART3_ICR ; \ USART3_ICR_NCF, Noise detected clear flag
: USART3_ICR_FECF ( -- x addr ) 1 bit USART3_ICR ; \ USART3_ICR_FECF, Framing error clear flag
: USART3_ICR_PECF ( -- x addr ) 0 bit USART3_ICR ; \ USART3_ICR_PECF, Parity error clear flag
[then]

execute-defined? use-USART3 defined? USART3_RDR_RDR? not and [if]
\ USART3_RDR (read-only) Reset:0x0000
: USART3_RDR_RDR? ( --  x ) USART3_RDR @ ; \ USART3_RDR_RDR, Receive data value
[then]

defined? use-USART3 defined? USART3_TDR_TDR not and [if]
\ USART3_TDR (read-write) Reset:0x0000
: USART3_TDR_TDR ( %bbbbbbbbb -- x addr ) USART3_TDR ; \ USART3_TDR_TDR, Transmit data value
[then]

execute-defined? use-UART4 defined? UART4_CR1_M1 not and [if]
\ UART4_CR1 (read-write) Reset:0x0000
: UART4_CR1_M1 ( -- x addr ) 28 bit UART4_CR1 ; \ UART4_CR1_M1, Word length
: UART4_CR1_EOBIE ( -- x addr ) 27 bit UART4_CR1 ; \ UART4_CR1_EOBIE, End of Block interrupt  enable
: UART4_CR1_RTOIE ( -- x addr ) 26 bit UART4_CR1 ; \ UART4_CR1_RTOIE, Receiver timeout interrupt  enable
: UART4_CR1_DEAT4 ( -- x addr ) 25 bit UART4_CR1 ; \ UART4_CR1_DEAT4, Driver Enable assertion  time
: UART4_CR1_DEAT3 ( -- x addr ) 24 bit UART4_CR1 ; \ UART4_CR1_DEAT3, DEAT3
: UART4_CR1_DEAT2 ( -- x addr ) 23 bit UART4_CR1 ; \ UART4_CR1_DEAT2, DEAT2
: UART4_CR1_DEAT1 ( -- x addr ) 22 bit UART4_CR1 ; \ UART4_CR1_DEAT1, DEAT1
: UART4_CR1_DEAT0 ( -- x addr ) 21 bit UART4_CR1 ; \ UART4_CR1_DEAT0, DEAT0
: UART4_CR1_DEDT4 ( -- x addr ) 20 bit UART4_CR1 ; \ UART4_CR1_DEDT4, Driver Enable de-assertion  time
: UART4_CR1_DEDT3 ( -- x addr ) 19 bit UART4_CR1 ; \ UART4_CR1_DEDT3, DEDT3
: UART4_CR1_DEDT2 ( -- x addr ) 18 bit UART4_CR1 ; \ UART4_CR1_DEDT2, DEDT2
: UART4_CR1_DEDT1 ( -- x addr ) 17 bit UART4_CR1 ; \ UART4_CR1_DEDT1, DEDT1
: UART4_CR1_DEDT0 ( -- x addr ) 16 bit UART4_CR1 ; \ UART4_CR1_DEDT0, DEDT0
: UART4_CR1_OVER8 ( -- x addr ) 15 bit UART4_CR1 ; \ UART4_CR1_OVER8, Oversampling mode
: UART4_CR1_CMIE ( -- x addr ) 14 bit UART4_CR1 ; \ UART4_CR1_CMIE, Character match interrupt  enable
: UART4_CR1_MME ( -- x addr ) 13 bit UART4_CR1 ; \ UART4_CR1_MME, Mute mode enable
: UART4_CR1_M0 ( -- x addr ) 12 bit UART4_CR1 ; \ UART4_CR1_M0, Word length
: UART4_CR1_WAKE ( -- x addr ) 11 bit UART4_CR1 ; \ UART4_CR1_WAKE, Receiver wakeup method
: UART4_CR1_PCE ( -- x addr ) 10 bit UART4_CR1 ; \ UART4_CR1_PCE, Parity control enable
: UART4_CR1_PS ( -- x addr ) 9 bit UART4_CR1 ; \ UART4_CR1_PS, Parity selection
: UART4_CR1_PEIE ( -- x addr ) 8 bit UART4_CR1 ; \ UART4_CR1_PEIE, PE interrupt enable
: UART4_CR1_TXEIE ( -- x addr ) 7 bit UART4_CR1 ; \ UART4_CR1_TXEIE, interrupt enable
: UART4_CR1_TCIE ( -- x addr ) 6 bit UART4_CR1 ; \ UART4_CR1_TCIE, Transmission complete interrupt  enable
: UART4_CR1_RXNEIE ( -- x addr ) 5 bit UART4_CR1 ; \ UART4_CR1_RXNEIE, RXNE interrupt enable
: UART4_CR1_IDLEIE ( -- x addr ) 4 bit UART4_CR1 ; \ UART4_CR1_IDLEIE, IDLE interrupt enable
: UART4_CR1_TE ( -- x addr ) 3 bit UART4_CR1 ; \ UART4_CR1_TE, Transmitter enable
: UART4_CR1_RE ( -- x addr ) 2 bit UART4_CR1 ; \ UART4_CR1_RE, Receiver enable
: UART4_CR1_UESM ( -- x addr ) 1 bit UART4_CR1 ; \ UART4_CR1_UESM, USART enable in Stop mode
: UART4_CR1_UE ( -- x addr ) 0 bit UART4_CR1 ; \ UART4_CR1_UE, USART enable
[then]

defined? use-UART4 defined? UART4_CR2_ADD4_7 not and [if]
\ UART4_CR2 (read-write) Reset:0x0000
: UART4_CR2_ADD4_7 ( %bbbb -- x addr ) 28 lshift UART4_CR2 ; \ UART4_CR2_ADD4_7, Address of the USART node
: UART4_CR2_ADD0_3 ( %bbbb -- x addr ) 24 lshift UART4_CR2 ; \ UART4_CR2_ADD0_3, Address of the USART node
: UART4_CR2_RTOEN ( -- x addr ) 23 bit UART4_CR2 ; \ UART4_CR2_RTOEN, Receiver timeout enable
: UART4_CR2_ABRMOD1 ( -- x addr ) 22 bit UART4_CR2 ; \ UART4_CR2_ABRMOD1, Auto baud rate mode
: UART4_CR2_ABRMOD0 ( -- x addr ) 21 bit UART4_CR2 ; \ UART4_CR2_ABRMOD0, ABRMOD0
: UART4_CR2_ABREN ( -- x addr ) 20 bit UART4_CR2 ; \ UART4_CR2_ABREN, Auto baud rate enable
: UART4_CR2_MSBFIRST ( -- x addr ) 19 bit UART4_CR2 ; \ UART4_CR2_MSBFIRST, Most significant bit first
: UART4_CR2_TAINV ( -- x addr ) 18 bit UART4_CR2 ; \ UART4_CR2_TAINV, Binary data inversion
: UART4_CR2_TXINV ( -- x addr ) 17 bit UART4_CR2 ; \ UART4_CR2_TXINV, TX pin active level  inversion
: UART4_CR2_RXINV ( -- x addr ) 16 bit UART4_CR2 ; \ UART4_CR2_RXINV, RX pin active level  inversion
: UART4_CR2_SWAP ( -- x addr ) 15 bit UART4_CR2 ; \ UART4_CR2_SWAP, Swap TX/RX pins
: UART4_CR2_LINEN ( -- x addr ) 14 bit UART4_CR2 ; \ UART4_CR2_LINEN, LIN mode enable
: UART4_CR2_STOP ( %bb -- x addr ) 12 lshift UART4_CR2 ; \ UART4_CR2_STOP, STOP bits
: UART4_CR2_CLKEN ( -- x addr ) 11 bit UART4_CR2 ; \ UART4_CR2_CLKEN, Clock enable
: UART4_CR2_CPOL ( -- x addr ) 10 bit UART4_CR2 ; \ UART4_CR2_CPOL, Clock polarity
: UART4_CR2_CPHA ( -- x addr ) 9 bit UART4_CR2 ; \ UART4_CR2_CPHA, Clock phase
: UART4_CR2_LBCL ( -- x addr ) 8 bit UART4_CR2 ; \ UART4_CR2_LBCL, Last bit clock pulse
: UART4_CR2_LBDIE ( -- x addr ) 6 bit UART4_CR2 ; \ UART4_CR2_LBDIE, LIN break detection interrupt  enable
: UART4_CR2_LBDL ( -- x addr ) 5 bit UART4_CR2 ; \ UART4_CR2_LBDL, LIN break detection length
: UART4_CR2_ADDM7 ( -- x addr ) 4 bit UART4_CR2 ; \ UART4_CR2_ADDM7, 7-bit Address Detection/4-bit Address  Detection
[then]

execute-defined? use-UART4 defined? UART4_CR3_WUFIE not and [if]
\ UART4_CR3 (read-write) Reset:0x0000
: UART4_CR3_WUFIE ( -- x addr ) 22 bit UART4_CR3 ; \ UART4_CR3_WUFIE, Wakeup from Stop mode interrupt  enable
: UART4_CR3_WUS ( %bb -- x addr ) 20 lshift UART4_CR3 ; \ UART4_CR3_WUS, Wakeup from Stop mode interrupt flag  selection
: UART4_CR3_SCARCNT ( %bbb -- x addr ) 17 lshift UART4_CR3 ; \ UART4_CR3_SCARCNT, Smartcard auto-retry count
: UART4_CR3_DEP ( -- x addr ) 15 bit UART4_CR3 ; \ UART4_CR3_DEP, Driver enable polarity  selection
: UART4_CR3_DEM ( -- x addr ) 14 bit UART4_CR3 ; \ UART4_CR3_DEM, Driver enable mode
: UART4_CR3_DDRE ( -- x addr ) 13 bit UART4_CR3 ; \ UART4_CR3_DDRE, DMA Disable on Reception  Error
: UART4_CR3_OVRDIS ( -- x addr ) 12 bit UART4_CR3 ; \ UART4_CR3_OVRDIS, Overrun Disable
: UART4_CR3_ONEBIT ( -- x addr ) 11 bit UART4_CR3 ; \ UART4_CR3_ONEBIT, One sample bit method  enable
: UART4_CR3_CTSIE ( -- x addr ) 10 bit UART4_CR3 ; \ UART4_CR3_CTSIE, CTS interrupt enable
: UART4_CR3_CTSE ( -- x addr ) 9 bit UART4_CR3 ; \ UART4_CR3_CTSE, CTS enable
: UART4_CR3_RTSE ( -- x addr ) 8 bit UART4_CR3 ; \ UART4_CR3_RTSE, RTS enable
: UART4_CR3_DMAT ( -- x addr ) 7 bit UART4_CR3 ; \ UART4_CR3_DMAT, DMA enable transmitter
: UART4_CR3_DMAR ( -- x addr ) 6 bit UART4_CR3 ; \ UART4_CR3_DMAR, DMA enable receiver
: UART4_CR3_SCEN ( -- x addr ) 5 bit UART4_CR3 ; \ UART4_CR3_SCEN, Smartcard mode enable
: UART4_CR3_NACK ( -- x addr ) 4 bit UART4_CR3 ; \ UART4_CR3_NACK, Smartcard NACK enable
: UART4_CR3_HDSEL ( -- x addr ) 3 bit UART4_CR3 ; \ UART4_CR3_HDSEL, Half-duplex selection
: UART4_CR3_IRLP ( -- x addr ) 2 bit UART4_CR3 ; \ UART4_CR3_IRLP, Ir low-power
: UART4_CR3_IREN ( -- x addr ) 1 bit UART4_CR3 ; \ UART4_CR3_IREN, Ir mode enable
: UART4_CR3_EIE ( -- x addr ) 0 bit UART4_CR3 ; \ UART4_CR3_EIE, Error interrupt enable
[then]

defined? use-UART4 defined? UART4_BRR_DIV_Mantissa not and [if]
\ UART4_BRR (read-write) Reset:0x0000
: UART4_BRR_DIV_Mantissa ( %bbbbbbbbbbb -- x addr ) 4 lshift UART4_BRR ; \ UART4_BRR_DIV_Mantissa, DIV_Mantissa
: UART4_BRR_DIV_Fraction ( %bbbb -- x addr ) UART4_BRR ; \ UART4_BRR_DIV_Fraction, DIV_Fraction
[then]

execute-defined? use-UART4 defined? UART4_GTPR_GT not and [if]
\ UART4_GTPR (read-write) Reset:0x0000
: UART4_GTPR_GT ( %bbbbbbbb -- x addr ) 8 lshift UART4_GTPR ; \ UART4_GTPR_GT, Guard time value
: UART4_GTPR_PSC ( %bbbbbbbb -- x addr ) UART4_GTPR ; \ UART4_GTPR_PSC, Prescaler value
[then]

defined? use-UART4 defined? UART4_RTOR_BLEN not and [if]
\ UART4_RTOR (read-write) Reset:0x0000
: UART4_RTOR_BLEN ( %bbbbbbbb -- x addr ) 24 lshift UART4_RTOR ; \ UART4_RTOR_BLEN, Block Length
: UART4_RTOR_RTO ( %bbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) UART4_RTOR ; \ UART4_RTOR_RTO, Receiver timeout value
[then]

execute-defined? use-UART4 defined? UART4_RQR_TXFRQ not and [if]
\ UART4_RQR (write-only) Reset:0x0000
: UART4_RQR_TXFRQ ( -- x addr ) 4 bit UART4_RQR ; \ UART4_RQR_TXFRQ, Transmit data flush  request
: UART4_RQR_RXFRQ ( -- x addr ) 3 bit UART4_RQR ; \ UART4_RQR_RXFRQ, Receive data flush request
: UART4_RQR_MMRQ ( -- x addr ) 2 bit UART4_RQR ; \ UART4_RQR_MMRQ, Mute mode request
: UART4_RQR_SBKRQ ( -- x addr ) 1 bit UART4_RQR ; \ UART4_RQR_SBKRQ, Send break request
: UART4_RQR_ABRRQ ( -- x addr ) 0 bit UART4_RQR ; \ UART4_RQR_ABRRQ, Auto baud rate request
[then]

defined? use-UART4 defined? UART4_ISR_REACK? not and [if]
\ UART4_ISR (read-only) Reset:0x00C0
: UART4_ISR_REACK? ( --  1|0 ) 22 bit UART4_ISR bit@ ; \ UART4_ISR_REACK, REACK
: UART4_ISR_TEACK? ( --  1|0 ) 21 bit UART4_ISR bit@ ; \ UART4_ISR_TEACK, TEACK
: UART4_ISR_WUF? ( --  1|0 ) 20 bit UART4_ISR bit@ ; \ UART4_ISR_WUF, WUF
: UART4_ISR_RWU? ( --  1|0 ) 19 bit UART4_ISR bit@ ; \ UART4_ISR_RWU, RWU
: UART4_ISR_SBKF? ( --  1|0 ) 18 bit UART4_ISR bit@ ; \ UART4_ISR_SBKF, SBKF
: UART4_ISR_CMF? ( --  1|0 ) 17 bit UART4_ISR bit@ ; \ UART4_ISR_CMF, CMF
: UART4_ISR_BUSY? ( --  1|0 ) 16 bit UART4_ISR bit@ ; \ UART4_ISR_BUSY, BUSY
: UART4_ISR_ABRF? ( --  1|0 ) 15 bit UART4_ISR bit@ ; \ UART4_ISR_ABRF, ABRF
: UART4_ISR_ABRE? ( --  1|0 ) 14 bit UART4_ISR bit@ ; \ UART4_ISR_ABRE, ABRE
: UART4_ISR_EOBF? ( --  1|0 ) 12 bit UART4_ISR bit@ ; \ UART4_ISR_EOBF, EOBF
: UART4_ISR_RTOF? ( --  1|0 ) 11 bit UART4_ISR bit@ ; \ UART4_ISR_RTOF, RTOF
: UART4_ISR_CTS? ( --  1|0 ) 10 bit UART4_ISR bit@ ; \ UART4_ISR_CTS, CTS
: UART4_ISR_CTSIF? ( --  1|0 ) 9 bit UART4_ISR bit@ ; \ UART4_ISR_CTSIF, CTSIF
: UART4_ISR_LBDF? ( --  1|0 ) 8 bit UART4_ISR bit@ ; \ UART4_ISR_LBDF, LBDF
: UART4_ISR_TXE? ( --  1|0 ) 7 bit UART4_ISR bit@ ; \ UART4_ISR_TXE, TXE
: UART4_ISR_TC? ( --  1|0 ) 6 bit UART4_ISR bit@ ; \ UART4_ISR_TC, TC
: UART4_ISR_RXNE? ( --  1|0 ) 5 bit UART4_ISR bit@ ; \ UART4_ISR_RXNE, RXNE
: UART4_ISR_IDLE? ( --  1|0 ) 4 bit UART4_ISR bit@ ; \ UART4_ISR_IDLE, IDLE
: UART4_ISR_ORE? ( --  1|0 ) 3 bit UART4_ISR bit@ ; \ UART4_ISR_ORE, ORE
: UART4_ISR_NF? ( --  1|0 ) 2 bit UART4_ISR bit@ ; \ UART4_ISR_NF, NF
: UART4_ISR_FE? ( --  1|0 ) 1 bit UART4_ISR bit@ ; \ UART4_ISR_FE, FE
: UART4_ISR_PE? ( --  1|0 ) 0 bit UART4_ISR bit@ ; \ UART4_ISR_PE, PE
[then]

execute-defined? use-UART4 defined? UART4_ICR_WUCF not and [if]
\ UART4_ICR (write-only) Reset:0x0000
: UART4_ICR_WUCF ( -- x addr ) 20 bit UART4_ICR ; \ UART4_ICR_WUCF, Wakeup from Stop mode clear  flag
: UART4_ICR_CMCF ( -- x addr ) 17 bit UART4_ICR ; \ UART4_ICR_CMCF, Character match clear flag
: UART4_ICR_EOBCF ( -- x addr ) 12 bit UART4_ICR ; \ UART4_ICR_EOBCF, End of block clear flag
: UART4_ICR_RTOCF ( -- x addr ) 11 bit UART4_ICR ; \ UART4_ICR_RTOCF, Receiver timeout clear  flag
: UART4_ICR_CTSCF ( -- x addr ) 9 bit UART4_ICR ; \ UART4_ICR_CTSCF, CTS clear flag
: UART4_ICR_LBDCF ( -- x addr ) 8 bit UART4_ICR ; \ UART4_ICR_LBDCF, LIN break detection clear  flag
: UART4_ICR_TCCF ( -- x addr ) 6 bit UART4_ICR ; \ UART4_ICR_TCCF, Transmission complete clear  flag
: UART4_ICR_IDLECF ( -- x addr ) 4 bit UART4_ICR ; \ UART4_ICR_IDLECF, Idle line detected clear  flag
: UART4_ICR_ORECF ( -- x addr ) 3 bit UART4_ICR ; \ UART4_ICR_ORECF, Overrun error clear flag
: UART4_ICR_NCF ( -- x addr ) 2 bit UART4_ICR ; \ UART4_ICR_NCF, Noise detected clear flag
: UART4_ICR_FECF ( -- x addr ) 1 bit UART4_ICR ; \ UART4_ICR_FECF, Framing error clear flag
: UART4_ICR_PECF ( -- x addr ) 0 bit UART4_ICR ; \ UART4_ICR_PECF, Parity error clear flag
[then]

defined? use-UART4 defined? UART4_RDR_RDR? not and [if]
\ UART4_RDR (read-only) Reset:0x0000
: UART4_RDR_RDR? ( --  x ) UART4_RDR @ ; \ UART4_RDR_RDR, Receive data value
[then]

execute-defined? use-UART4 defined? UART4_TDR_TDR not and [if]
\ UART4_TDR (read-write) Reset:0x0000
: UART4_TDR_TDR ( %bbbbbbbbb -- x addr ) UART4_TDR ; \ UART4_TDR_TDR, Transmit data value
[then]

defined? use-UART5 defined? UART5_CR1_M1 not and [if]
\ UART5_CR1 (read-write) Reset:0x0000
: UART5_CR1_M1 ( -- x addr ) 28 bit UART5_CR1 ; \ UART5_CR1_M1, Word length
: UART5_CR1_EOBIE ( -- x addr ) 27 bit UART5_CR1 ; \ UART5_CR1_EOBIE, End of Block interrupt  enable
: UART5_CR1_RTOIE ( -- x addr ) 26 bit UART5_CR1 ; \ UART5_CR1_RTOIE, Receiver timeout interrupt  enable
: UART5_CR1_DEAT4 ( -- x addr ) 25 bit UART5_CR1 ; \ UART5_CR1_DEAT4, Driver Enable assertion  time
: UART5_CR1_DEAT3 ( -- x addr ) 24 bit UART5_CR1 ; \ UART5_CR1_DEAT3, DEAT3
: UART5_CR1_DEAT2 ( -- x addr ) 23 bit UART5_CR1 ; \ UART5_CR1_DEAT2, DEAT2
: UART5_CR1_DEAT1 ( -- x addr ) 22 bit UART5_CR1 ; \ UART5_CR1_DEAT1, DEAT1
: UART5_CR1_DEAT0 ( -- x addr ) 21 bit UART5_CR1 ; \ UART5_CR1_DEAT0, DEAT0
: UART5_CR1_DEDT4 ( -- x addr ) 20 bit UART5_CR1 ; \ UART5_CR1_DEDT4, Driver Enable de-assertion  time
: UART5_CR1_DEDT3 ( -- x addr ) 19 bit UART5_CR1 ; \ UART5_CR1_DEDT3, DEDT3
: UART5_CR1_DEDT2 ( -- x addr ) 18 bit UART5_CR1 ; \ UART5_CR1_DEDT2, DEDT2
: UART5_CR1_DEDT1 ( -- x addr ) 17 bit UART5_CR1 ; \ UART5_CR1_DEDT1, DEDT1
: UART5_CR1_DEDT0 ( -- x addr ) 16 bit UART5_CR1 ; \ UART5_CR1_DEDT0, DEDT0
: UART5_CR1_OVER8 ( -- x addr ) 15 bit UART5_CR1 ; \ UART5_CR1_OVER8, Oversampling mode
: UART5_CR1_CMIE ( -- x addr ) 14 bit UART5_CR1 ; \ UART5_CR1_CMIE, Character match interrupt  enable
: UART5_CR1_MME ( -- x addr ) 13 bit UART5_CR1 ; \ UART5_CR1_MME, Mute mode enable
: UART5_CR1_M0 ( -- x addr ) 12 bit UART5_CR1 ; \ UART5_CR1_M0, Word length
: UART5_CR1_WAKE ( -- x addr ) 11 bit UART5_CR1 ; \ UART5_CR1_WAKE, Receiver wakeup method
: UART5_CR1_PCE ( -- x addr ) 10 bit UART5_CR1 ; \ UART5_CR1_PCE, Parity control enable
: UART5_CR1_PS ( -- x addr ) 9 bit UART5_CR1 ; \ UART5_CR1_PS, Parity selection
: UART5_CR1_PEIE ( -- x addr ) 8 bit UART5_CR1 ; \ UART5_CR1_PEIE, PE interrupt enable
: UART5_CR1_TXEIE ( -- x addr ) 7 bit UART5_CR1 ; \ UART5_CR1_TXEIE, interrupt enable
: UART5_CR1_TCIE ( -- x addr ) 6 bit UART5_CR1 ; \ UART5_CR1_TCIE, Transmission complete interrupt  enable
: UART5_CR1_RXNEIE ( -- x addr ) 5 bit UART5_CR1 ; \ UART5_CR1_RXNEIE, RXNE interrupt enable
: UART5_CR1_IDLEIE ( -- x addr ) 4 bit UART5_CR1 ; \ UART5_CR1_IDLEIE, IDLE interrupt enable
: UART5_CR1_TE ( -- x addr ) 3 bit UART5_CR1 ; \ UART5_CR1_TE, Transmitter enable
: UART5_CR1_RE ( -- x addr ) 2 bit UART5_CR1 ; \ UART5_CR1_RE, Receiver enable
: UART5_CR1_UESM ( -- x addr ) 1 bit UART5_CR1 ; \ UART5_CR1_UESM, USART enable in Stop mode
: UART5_CR1_UE ( -- x addr ) 0 bit UART5_CR1 ; \ UART5_CR1_UE, USART enable
[then]

execute-defined? use-UART5 defined? UART5_CR2_ADD4_7 not and [if]
\ UART5_CR2 (read-write) Reset:0x0000
: UART5_CR2_ADD4_7 ( %bbbb -- x addr ) 28 lshift UART5_CR2 ; \ UART5_CR2_ADD4_7, Address of the USART node
: UART5_CR2_ADD0_3 ( %bbbb -- x addr ) 24 lshift UART5_CR2 ; \ UART5_CR2_ADD0_3, Address of the USART node
: UART5_CR2_RTOEN ( -- x addr ) 23 bit UART5_CR2 ; \ UART5_CR2_RTOEN, Receiver timeout enable
: UART5_CR2_ABRMOD1 ( -- x addr ) 22 bit UART5_CR2 ; \ UART5_CR2_ABRMOD1, Auto baud rate mode
: UART5_CR2_ABRMOD0 ( -- x addr ) 21 bit UART5_CR2 ; \ UART5_CR2_ABRMOD0, ABRMOD0
: UART5_CR2_ABREN ( -- x addr ) 20 bit UART5_CR2 ; \ UART5_CR2_ABREN, Auto baud rate enable
: UART5_CR2_MSBFIRST ( -- x addr ) 19 bit UART5_CR2 ; \ UART5_CR2_MSBFIRST, Most significant bit first
: UART5_CR2_TAINV ( -- x addr ) 18 bit UART5_CR2 ; \ UART5_CR2_TAINV, Binary data inversion
: UART5_CR2_TXINV ( -- x addr ) 17 bit UART5_CR2 ; \ UART5_CR2_TXINV, TX pin active level  inversion
: UART5_CR2_RXINV ( -- x addr ) 16 bit UART5_CR2 ; \ UART5_CR2_RXINV, RX pin active level  inversion
: UART5_CR2_SWAP ( -- x addr ) 15 bit UART5_CR2 ; \ UART5_CR2_SWAP, Swap TX/RX pins
: UART5_CR2_LINEN ( -- x addr ) 14 bit UART5_CR2 ; \ UART5_CR2_LINEN, LIN mode enable
: UART5_CR2_STOP ( %bb -- x addr ) 12 lshift UART5_CR2 ; \ UART5_CR2_STOP, STOP bits
: UART5_CR2_CLKEN ( -- x addr ) 11 bit UART5_CR2 ; \ UART5_CR2_CLKEN, Clock enable
: UART5_CR2_CPOL ( -- x addr ) 10 bit UART5_CR2 ; \ UART5_CR2_CPOL, Clock polarity
: UART5_CR2_CPHA ( -- x addr ) 9 bit UART5_CR2 ; \ UART5_CR2_CPHA, Clock phase
: UART5_CR2_LBCL ( -- x addr ) 8 bit UART5_CR2 ; \ UART5_CR2_LBCL, Last bit clock pulse
: UART5_CR2_LBDIE ( -- x addr ) 6 bit UART5_CR2 ; \ UART5_CR2_LBDIE, LIN break detection interrupt  enable
: UART5_CR2_LBDL ( -- x addr ) 5 bit UART5_CR2 ; \ UART5_CR2_LBDL, LIN break detection length
: UART5_CR2_ADDM7 ( -- x addr ) 4 bit UART5_CR2 ; \ UART5_CR2_ADDM7, 7-bit Address Detection/4-bit Address  Detection
[then]

defined? use-UART5 defined? UART5_CR3_WUFIE not and [if]
\ UART5_CR3 (read-write) Reset:0x0000
: UART5_CR3_WUFIE ( -- x addr ) 22 bit UART5_CR3 ; \ UART5_CR3_WUFIE, Wakeup from Stop mode interrupt  enable
: UART5_CR3_WUS ( %bb -- x addr ) 20 lshift UART5_CR3 ; \ UART5_CR3_WUS, Wakeup from Stop mode interrupt flag  selection
: UART5_CR3_SCARCNT ( %bbb -- x addr ) 17 lshift UART5_CR3 ; \ UART5_CR3_SCARCNT, Smartcard auto-retry count
: UART5_CR3_DEP ( -- x addr ) 15 bit UART5_CR3 ; \ UART5_CR3_DEP, Driver enable polarity  selection
: UART5_CR3_DEM ( -- x addr ) 14 bit UART5_CR3 ; \ UART5_CR3_DEM, Driver enable mode
: UART5_CR3_DDRE ( -- x addr ) 13 bit UART5_CR3 ; \ UART5_CR3_DDRE, DMA Disable on Reception  Error
: UART5_CR3_OVRDIS ( -- x addr ) 12 bit UART5_CR3 ; \ UART5_CR3_OVRDIS, Overrun Disable
: UART5_CR3_ONEBIT ( -- x addr ) 11 bit UART5_CR3 ; \ UART5_CR3_ONEBIT, One sample bit method  enable
: UART5_CR3_CTSIE ( -- x addr ) 10 bit UART5_CR3 ; \ UART5_CR3_CTSIE, CTS interrupt enable
: UART5_CR3_CTSE ( -- x addr ) 9 bit UART5_CR3 ; \ UART5_CR3_CTSE, CTS enable
: UART5_CR3_RTSE ( -- x addr ) 8 bit UART5_CR3 ; \ UART5_CR3_RTSE, RTS enable
: UART5_CR3_DMAT ( -- x addr ) 7 bit UART5_CR3 ; \ UART5_CR3_DMAT, DMA enable transmitter
: UART5_CR3_DMAR ( -- x addr ) 6 bit UART5_CR3 ; \ UART5_CR3_DMAR, DMA enable receiver
: UART5_CR3_SCEN ( -- x addr ) 5 bit UART5_CR3 ; \ UART5_CR3_SCEN, Smartcard mode enable
: UART5_CR3_NACK ( -- x addr ) 4 bit UART5_CR3 ; \ UART5_CR3_NACK, Smartcard NACK enable
: UART5_CR3_HDSEL ( -- x addr ) 3 bit UART5_CR3 ; \ UART5_CR3_HDSEL, Half-duplex selection
: UART5_CR3_IRLP ( -- x addr ) 2 bit UART5_CR3 ; \ UART5_CR3_IRLP, Ir low-power
: UART5_CR3_IREN ( -- x addr ) 1 bit UART5_CR3 ; \ UART5_CR3_IREN, Ir mode enable
: UART5_CR3_EIE ( -- x addr ) 0 bit UART5_CR3 ; \ UART5_CR3_EIE, Error interrupt enable
[then]

execute-defined? use-UART5 defined? UART5_BRR_DIV_Mantissa not and [if]
\ UART5_BRR (read-write) Reset:0x0000
: UART5_BRR_DIV_Mantissa ( %bbbbbbbbbbb -- x addr ) 4 lshift UART5_BRR ; \ UART5_BRR_DIV_Mantissa, DIV_Mantissa
: UART5_BRR_DIV_Fraction ( %bbbb -- x addr ) UART5_BRR ; \ UART5_BRR_DIV_Fraction, DIV_Fraction
[then]

defined? use-UART5 defined? UART5_GTPR_GT not and [if]
\ UART5_GTPR (read-write) Reset:0x0000
: UART5_GTPR_GT ( %bbbbbbbb -- x addr ) 8 lshift UART5_GTPR ; \ UART5_GTPR_GT, Guard time value
: UART5_GTPR_PSC ( %bbbbbbbb -- x addr ) UART5_GTPR ; \ UART5_GTPR_PSC, Prescaler value
[then]

execute-defined? use-UART5 defined? UART5_RTOR_BLEN not and [if]
\ UART5_RTOR (read-write) Reset:0x0000
: UART5_RTOR_BLEN ( %bbbbbbbb -- x addr ) 24 lshift UART5_RTOR ; \ UART5_RTOR_BLEN, Block Length
: UART5_RTOR_RTO ( %bbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) UART5_RTOR ; \ UART5_RTOR_RTO, Receiver timeout value
[then]

defined? use-UART5 defined? UART5_RQR_TXFRQ not and [if]
\ UART5_RQR (write-only) Reset:0x0000
: UART5_RQR_TXFRQ ( -- x addr ) 4 bit UART5_RQR ; \ UART5_RQR_TXFRQ, Transmit data flush  request
: UART5_RQR_RXFRQ ( -- x addr ) 3 bit UART5_RQR ; \ UART5_RQR_RXFRQ, Receive data flush request
: UART5_RQR_MMRQ ( -- x addr ) 2 bit UART5_RQR ; \ UART5_RQR_MMRQ, Mute mode request
: UART5_RQR_SBKRQ ( -- x addr ) 1 bit UART5_RQR ; \ UART5_RQR_SBKRQ, Send break request
: UART5_RQR_ABRRQ ( -- x addr ) 0 bit UART5_RQR ; \ UART5_RQR_ABRRQ, Auto baud rate request
[then]

execute-defined? use-UART5 defined? UART5_ISR_REACK? not and [if]
\ UART5_ISR (read-only) Reset:0x00C0
: UART5_ISR_REACK? ( --  1|0 ) 22 bit UART5_ISR bit@ ; \ UART5_ISR_REACK, REACK
: UART5_ISR_TEACK? ( --  1|0 ) 21 bit UART5_ISR bit@ ; \ UART5_ISR_TEACK, TEACK
: UART5_ISR_WUF? ( --  1|0 ) 20 bit UART5_ISR bit@ ; \ UART5_ISR_WUF, WUF
: UART5_ISR_RWU? ( --  1|0 ) 19 bit UART5_ISR bit@ ; \ UART5_ISR_RWU, RWU
: UART5_ISR_SBKF? ( --  1|0 ) 18 bit UART5_ISR bit@ ; \ UART5_ISR_SBKF, SBKF
: UART5_ISR_CMF? ( --  1|0 ) 17 bit UART5_ISR bit@ ; \ UART5_ISR_CMF, CMF
: UART5_ISR_BUSY? ( --  1|0 ) 16 bit UART5_ISR bit@ ; \ UART5_ISR_BUSY, BUSY
: UART5_ISR_ABRF? ( --  1|0 ) 15 bit UART5_ISR bit@ ; \ UART5_ISR_ABRF, ABRF
: UART5_ISR_ABRE? ( --  1|0 ) 14 bit UART5_ISR bit@ ; \ UART5_ISR_ABRE, ABRE
: UART5_ISR_EOBF? ( --  1|0 ) 12 bit UART5_ISR bit@ ; \ UART5_ISR_EOBF, EOBF
: UART5_ISR_RTOF? ( --  1|0 ) 11 bit UART5_ISR bit@ ; \ UART5_ISR_RTOF, RTOF
: UART5_ISR_CTS? ( --  1|0 ) 10 bit UART5_ISR bit@ ; \ UART5_ISR_CTS, CTS
: UART5_ISR_CTSIF? ( --  1|0 ) 9 bit UART5_ISR bit@ ; \ UART5_ISR_CTSIF, CTSIF
: UART5_ISR_LBDF? ( --  1|0 ) 8 bit UART5_ISR bit@ ; \ UART5_ISR_LBDF, LBDF
: UART5_ISR_TXE? ( --  1|0 ) 7 bit UART5_ISR bit@ ; \ UART5_ISR_TXE, TXE
: UART5_ISR_TC? ( --  1|0 ) 6 bit UART5_ISR bit@ ; \ UART5_ISR_TC, TC
: UART5_ISR_RXNE? ( --  1|0 ) 5 bit UART5_ISR bit@ ; \ UART5_ISR_RXNE, RXNE
: UART5_ISR_IDLE? ( --  1|0 ) 4 bit UART5_ISR bit@ ; \ UART5_ISR_IDLE, IDLE
: UART5_ISR_ORE? ( --  1|0 ) 3 bit UART5_ISR bit@ ; \ UART5_ISR_ORE, ORE
: UART5_ISR_NF? ( --  1|0 ) 2 bit UART5_ISR bit@ ; \ UART5_ISR_NF, NF
: UART5_ISR_FE? ( --  1|0 ) 1 bit UART5_ISR bit@ ; \ UART5_ISR_FE, FE
: UART5_ISR_PE? ( --  1|0 ) 0 bit UART5_ISR bit@ ; \ UART5_ISR_PE, PE
[then]

defined? use-UART5 defined? UART5_ICR_WUCF not and [if]
\ UART5_ICR (write-only) Reset:0x0000
: UART5_ICR_WUCF ( -- x addr ) 20 bit UART5_ICR ; \ UART5_ICR_WUCF, Wakeup from Stop mode clear  flag
: UART5_ICR_CMCF ( -- x addr ) 17 bit UART5_ICR ; \ UART5_ICR_CMCF, Character match clear flag
: UART5_ICR_EOBCF ( -- x addr ) 12 bit UART5_ICR ; \ UART5_ICR_EOBCF, End of block clear flag
: UART5_ICR_RTOCF ( -- x addr ) 11 bit UART5_ICR ; \ UART5_ICR_RTOCF, Receiver timeout clear  flag
: UART5_ICR_CTSCF ( -- x addr ) 9 bit UART5_ICR ; \ UART5_ICR_CTSCF, CTS clear flag
: UART5_ICR_LBDCF ( -- x addr ) 8 bit UART5_ICR ; \ UART5_ICR_LBDCF, LIN break detection clear  flag
: UART5_ICR_TCCF ( -- x addr ) 6 bit UART5_ICR ; \ UART5_ICR_TCCF, Transmission complete clear  flag
: UART5_ICR_IDLECF ( -- x addr ) 4 bit UART5_ICR ; \ UART5_ICR_IDLECF, Idle line detected clear  flag
: UART5_ICR_ORECF ( -- x addr ) 3 bit UART5_ICR ; \ UART5_ICR_ORECF, Overrun error clear flag
: UART5_ICR_NCF ( -- x addr ) 2 bit UART5_ICR ; \ UART5_ICR_NCF, Noise detected clear flag
: UART5_ICR_FECF ( -- x addr ) 1 bit UART5_ICR ; \ UART5_ICR_FECF, Framing error clear flag
: UART5_ICR_PECF ( -- x addr ) 0 bit UART5_ICR ; \ UART5_ICR_PECF, Parity error clear flag
[then]

execute-defined? use-UART5 defined? UART5_RDR_RDR? not and [if]
\ UART5_RDR (read-only) Reset:0x0000
: UART5_RDR_RDR? ( --  x ) UART5_RDR @ ; \ UART5_RDR_RDR, Receive data value
[then]

defined? use-UART5 defined? UART5_TDR_TDR not and [if]
\ UART5_TDR (read-write) Reset:0x0000
: UART5_TDR_TDR ( %bbbbbbbbb -- x addr ) UART5_TDR ; \ UART5_TDR_TDR, Transmit data value
[then]

execute-defined? use-SPI1 defined? SPI1_CR1_BIDIMODE not and [if]
\ SPI1_CR1 (read-write) Reset:0x0000
: SPI1_CR1_BIDIMODE ( -- x addr ) 15 bit SPI1_CR1 ; \ SPI1_CR1_BIDIMODE, Bidirectional data mode  enable
: SPI1_CR1_BIDIOE ( -- x addr ) 14 bit SPI1_CR1 ; \ SPI1_CR1_BIDIOE, Output enable in bidirectional  mode
: SPI1_CR1_CRCEN ( -- x addr ) 13 bit SPI1_CR1 ; \ SPI1_CR1_CRCEN, Hardware CRC calculation  enable
: SPI1_CR1_CRCNEXT ( -- x addr ) 12 bit SPI1_CR1 ; \ SPI1_CR1_CRCNEXT, CRC transfer next
: SPI1_CR1_DFF ( -- x addr ) 11 bit SPI1_CR1 ; \ SPI1_CR1_DFF, Data frame format
: SPI1_CR1_RXONLY ( -- x addr ) 10 bit SPI1_CR1 ; \ SPI1_CR1_RXONLY, Receive only
: SPI1_CR1_SSM ( -- x addr ) 9 bit SPI1_CR1 ; \ SPI1_CR1_SSM, Software slave management
: SPI1_CR1_SSI ( -- x addr ) 8 bit SPI1_CR1 ; \ SPI1_CR1_SSI, Internal slave select
: SPI1_CR1_LSBFIRST ( -- x addr ) 7 bit SPI1_CR1 ; \ SPI1_CR1_LSBFIRST, Frame format
: SPI1_CR1_SPE ( -- x addr ) 6 bit SPI1_CR1 ; \ SPI1_CR1_SPE, SPI enable
: SPI1_CR1_BR ( %bbb -- x addr ) 3 lshift SPI1_CR1 ; \ SPI1_CR1_BR, Baud rate control
: SPI1_CR1_MSTR ( -- x addr ) 2 bit SPI1_CR1 ; \ SPI1_CR1_MSTR, Master selection
: SPI1_CR1_CPOL ( -- x addr ) 1 bit SPI1_CR1 ; \ SPI1_CR1_CPOL, Clock polarity
: SPI1_CR1_CPHA ( -- x addr ) 0 bit SPI1_CR1 ; \ SPI1_CR1_CPHA, Clock phase
[then]

defined? use-SPI1 defined? SPI1_CR2_RXDMAEN not and [if]
\ SPI1_CR2 (read-write) Reset:0x0000
: SPI1_CR2_RXDMAEN ( -- x addr ) 0 bit SPI1_CR2 ; \ SPI1_CR2_RXDMAEN, Rx buffer DMA enable
: SPI1_CR2_TXDMAEN ( -- x addr ) 1 bit SPI1_CR2 ; \ SPI1_CR2_TXDMAEN, Tx buffer DMA enable
: SPI1_CR2_SSOE ( -- x addr ) 2 bit SPI1_CR2 ; \ SPI1_CR2_SSOE, SS output enable
: SPI1_CR2_NSSP ( -- x addr ) 3 bit SPI1_CR2 ; \ SPI1_CR2_NSSP, NSS pulse management
: SPI1_CR2_FRF ( -- x addr ) 4 bit SPI1_CR2 ; \ SPI1_CR2_FRF, Frame format
: SPI1_CR2_ERRIE ( -- x addr ) 5 bit SPI1_CR2 ; \ SPI1_CR2_ERRIE, Error interrupt enable
: SPI1_CR2_RXNEIE ( -- x addr ) 6 bit SPI1_CR2 ; \ SPI1_CR2_RXNEIE, RX buffer not empty interrupt  enable
: SPI1_CR2_TXEIE ( -- x addr ) 7 bit SPI1_CR2 ; \ SPI1_CR2_TXEIE, Tx buffer empty interrupt  enable
: SPI1_CR2_DS ( %bbbb -- x addr ) 8 lshift SPI1_CR2 ; \ SPI1_CR2_DS, Data size
: SPI1_CR2_FRXTH ( -- x addr ) 12 bit SPI1_CR2 ; \ SPI1_CR2_FRXTH, FIFO reception threshold
: SPI1_CR2_LDMA_RX ( -- x addr ) 13 bit SPI1_CR2 ; \ SPI1_CR2_LDMA_RX, Last DMA transfer for  reception
: SPI1_CR2_LDMA_TX ( -- x addr ) 14 bit SPI1_CR2 ; \ SPI1_CR2_LDMA_TX, Last DMA transfer for  transmission
[then]

execute-defined? use-SPI1 defined? SPI1_SR_RXNE not and [if]
\ SPI1_SR (multiple-access)  Reset:0x0002
: SPI1_SR_RXNE ( -- x addr ) 0 bit SPI1_SR ; \ SPI1_SR_RXNE, Receive buffer not empty
: SPI1_SR_TXE ( -- x addr ) 1 bit SPI1_SR ; \ SPI1_SR_TXE, Transmit buffer empty
: SPI1_SR_CRCERR? ( -- 1|0 ) 4 bit SPI1_SR bit@ ; \ SPI1_SR_CRCERR, CRC error flag
: SPI1_SR_MODF ( -- x addr ) 5 bit SPI1_SR ; \ SPI1_SR_MODF, Mode fault
: SPI1_SR_OVR? ( -- 1|0 ) 6 bit SPI1_SR bit@ ; \ SPI1_SR_OVR, Overrun flag
: SPI1_SR_BSY? ( -- 1|0 ) 7 bit SPI1_SR bit@ ; \ SPI1_SR_BSY, Busy flag
: SPI1_SR_TIFRFE ( -- x addr ) 8 bit SPI1_SR ; \ SPI1_SR_TIFRFE, TI frame format error
: SPI1_SR_FRLVL ( %bb -- x addr ) 9 lshift SPI1_SR ; \ SPI1_SR_FRLVL, FIFO reception level
: SPI1_SR_FTLVL ( %bb -- x addr ) 11 lshift SPI1_SR ; \ SPI1_SR_FTLVL, FIFO transmission level
[then]

defined? use-SPI1 defined? SPI1_DR_DR not and [if]
\ SPI1_DR (read-write) Reset:0x0000
: SPI1_DR_DR ( %bbbbbbbbbbbbbbbb -- x addr ) SPI1_DR ; \ SPI1_DR_DR, Data register
[then]

execute-defined? use-SPI1 defined? SPI1_CRCPR_CRCPOLY not and [if]
\ SPI1_CRCPR (read-write) Reset:0x0007
: SPI1_CRCPR_CRCPOLY ( %bbbbbbbbbbbbbbbb -- x addr ) SPI1_CRCPR ; \ SPI1_CRCPR_CRCPOLY, CRC polynomial register
[then]

defined? use-SPI1 defined? SPI1_RXCRCR_RxCRC? not and [if]
\ SPI1_RXCRCR (read-only) Reset:0x0000
: SPI1_RXCRCR_RxCRC? ( --  x ) SPI1_RXCRCR @ ; \ SPI1_RXCRCR_RxCRC, Rx CRC register
[then]

execute-defined? use-SPI1 defined? SPI1_TXCRCR_TxCRC? not and [if]
\ SPI1_TXCRCR (read-only) Reset:0x0000
: SPI1_TXCRCR_TxCRC? ( --  x ) SPI1_TXCRCR @ ; \ SPI1_TXCRCR_TxCRC, Tx CRC register
[then]

defined? use-SPI2 defined? SPI2_CR1_BIDIMODE not and [if]
\ SPI2_CR1 (read-write) Reset:0x0000
: SPI2_CR1_BIDIMODE ( -- x addr ) 15 bit SPI2_CR1 ; \ SPI2_CR1_BIDIMODE, Bidirectional data mode  enable
: SPI2_CR1_BIDIOE ( -- x addr ) 14 bit SPI2_CR1 ; \ SPI2_CR1_BIDIOE, Output enable in bidirectional  mode
: SPI2_CR1_CRCEN ( -- x addr ) 13 bit SPI2_CR1 ; \ SPI2_CR1_CRCEN, Hardware CRC calculation  enable
: SPI2_CR1_CRCNEXT ( -- x addr ) 12 bit SPI2_CR1 ; \ SPI2_CR1_CRCNEXT, CRC transfer next
: SPI2_CR1_DFF ( -- x addr ) 11 bit SPI2_CR1 ; \ SPI2_CR1_DFF, Data frame format
: SPI2_CR1_RXONLY ( -- x addr ) 10 bit SPI2_CR1 ; \ SPI2_CR1_RXONLY, Receive only
: SPI2_CR1_SSM ( -- x addr ) 9 bit SPI2_CR1 ; \ SPI2_CR1_SSM, Software slave management
: SPI2_CR1_SSI ( -- x addr ) 8 bit SPI2_CR1 ; \ SPI2_CR1_SSI, Internal slave select
: SPI2_CR1_LSBFIRST ( -- x addr ) 7 bit SPI2_CR1 ; \ SPI2_CR1_LSBFIRST, Frame format
: SPI2_CR1_SPE ( -- x addr ) 6 bit SPI2_CR1 ; \ SPI2_CR1_SPE, SPI enable
: SPI2_CR1_BR ( %bbb -- x addr ) 3 lshift SPI2_CR1 ; \ SPI2_CR1_BR, Baud rate control
: SPI2_CR1_MSTR ( -- x addr ) 2 bit SPI2_CR1 ; \ SPI2_CR1_MSTR, Master selection
: SPI2_CR1_CPOL ( -- x addr ) 1 bit SPI2_CR1 ; \ SPI2_CR1_CPOL, Clock polarity
: SPI2_CR1_CPHA ( -- x addr ) 0 bit SPI2_CR1 ; \ SPI2_CR1_CPHA, Clock phase
[then]

execute-defined? use-SPI2 defined? SPI2_CR2_RXDMAEN not and [if]
\ SPI2_CR2 (read-write) Reset:0x0000
: SPI2_CR2_RXDMAEN ( -- x addr ) 0 bit SPI2_CR2 ; \ SPI2_CR2_RXDMAEN, Rx buffer DMA enable
: SPI2_CR2_TXDMAEN ( -- x addr ) 1 bit SPI2_CR2 ; \ SPI2_CR2_TXDMAEN, Tx buffer DMA enable
: SPI2_CR2_SSOE ( -- x addr ) 2 bit SPI2_CR2 ; \ SPI2_CR2_SSOE, SS output enable
: SPI2_CR2_NSSP ( -- x addr ) 3 bit SPI2_CR2 ; \ SPI2_CR2_NSSP, NSS pulse management
: SPI2_CR2_FRF ( -- x addr ) 4 bit SPI2_CR2 ; \ SPI2_CR2_FRF, Frame format
: SPI2_CR2_ERRIE ( -- x addr ) 5 bit SPI2_CR2 ; \ SPI2_CR2_ERRIE, Error interrupt enable
: SPI2_CR2_RXNEIE ( -- x addr ) 6 bit SPI2_CR2 ; \ SPI2_CR2_RXNEIE, RX buffer not empty interrupt  enable
: SPI2_CR2_TXEIE ( -- x addr ) 7 bit SPI2_CR2 ; \ SPI2_CR2_TXEIE, Tx buffer empty interrupt  enable
: SPI2_CR2_DS ( %bbbb -- x addr ) 8 lshift SPI2_CR2 ; \ SPI2_CR2_DS, Data size
: SPI2_CR2_FRXTH ( -- x addr ) 12 bit SPI2_CR2 ; \ SPI2_CR2_FRXTH, FIFO reception threshold
: SPI2_CR2_LDMA_RX ( -- x addr ) 13 bit SPI2_CR2 ; \ SPI2_CR2_LDMA_RX, Last DMA transfer for  reception
: SPI2_CR2_LDMA_TX ( -- x addr ) 14 bit SPI2_CR2 ; \ SPI2_CR2_LDMA_TX, Last DMA transfer for  transmission
[then]

defined? use-SPI2 defined? SPI2_SR_RXNE not and [if]
\ SPI2_SR (multiple-access)  Reset:0x0002
: SPI2_SR_RXNE ( -- x addr ) 0 bit SPI2_SR ; \ SPI2_SR_RXNE, Receive buffer not empty
: SPI2_SR_TXE ( -- x addr ) 1 bit SPI2_SR ; \ SPI2_SR_TXE, Transmit buffer empty
: SPI2_SR_CRCERR? ( -- 1|0 ) 4 bit SPI2_SR bit@ ; \ SPI2_SR_CRCERR, CRC error flag
: SPI2_SR_MODF ( -- x addr ) 5 bit SPI2_SR ; \ SPI2_SR_MODF, Mode fault
: SPI2_SR_OVR? ( -- 1|0 ) 6 bit SPI2_SR bit@ ; \ SPI2_SR_OVR, Overrun flag
: SPI2_SR_BSY? ( -- 1|0 ) 7 bit SPI2_SR bit@ ; \ SPI2_SR_BSY, Busy flag
: SPI2_SR_TIFRFE ( -- x addr ) 8 bit SPI2_SR ; \ SPI2_SR_TIFRFE, TI frame format error
: SPI2_SR_FRLVL ( %bb -- x addr ) 9 lshift SPI2_SR ; \ SPI2_SR_FRLVL, FIFO reception level
: SPI2_SR_FTLVL ( %bb -- x addr ) 11 lshift SPI2_SR ; \ SPI2_SR_FTLVL, FIFO transmission level
[then]

execute-defined? use-SPI2 defined? SPI2_DR_DR not and [if]
\ SPI2_DR (read-write) Reset:0x0000
: SPI2_DR_DR ( %bbbbbbbbbbbbbbbb -- x addr ) SPI2_DR ; \ SPI2_DR_DR, Data register
[then]

defined? use-SPI2 defined? SPI2_CRCPR_CRCPOLY not and [if]
\ SPI2_CRCPR (read-write) Reset:0x0007
: SPI2_CRCPR_CRCPOLY ( %bbbbbbbbbbbbbbbb -- x addr ) SPI2_CRCPR ; \ SPI2_CRCPR_CRCPOLY, CRC polynomial register
[then]

execute-defined? use-SPI2 defined? SPI2_RXCRCR_RxCRC? not and [if]
\ SPI2_RXCRCR (read-only) Reset:0x0000
: SPI2_RXCRCR_RxCRC? ( --  x ) SPI2_RXCRCR @ ; \ SPI2_RXCRCR_RxCRC, Rx CRC register
[then]

defined? use-SPI2 defined? SPI2_TXCRCR_TxCRC? not and [if]
\ SPI2_TXCRCR (read-only) Reset:0x0000
: SPI2_TXCRCR_TxCRC? ( --  x ) SPI2_TXCRCR @ ; \ SPI2_TXCRCR_TxCRC, Tx CRC register
[then]

execute-defined? use-SPI3 defined? SPI3_CR1_BIDIMODE not and [if]
\ SPI3_CR1 (read-write) Reset:0x0000
: SPI3_CR1_BIDIMODE ( -- x addr ) 15 bit SPI3_CR1 ; \ SPI3_CR1_BIDIMODE, Bidirectional data mode  enable
: SPI3_CR1_BIDIOE ( -- x addr ) 14 bit SPI3_CR1 ; \ SPI3_CR1_BIDIOE, Output enable in bidirectional  mode
: SPI3_CR1_CRCEN ( -- x addr ) 13 bit SPI3_CR1 ; \ SPI3_CR1_CRCEN, Hardware CRC calculation  enable
: SPI3_CR1_CRCNEXT ( -- x addr ) 12 bit SPI3_CR1 ; \ SPI3_CR1_CRCNEXT, CRC transfer next
: SPI3_CR1_DFF ( -- x addr ) 11 bit SPI3_CR1 ; \ SPI3_CR1_DFF, Data frame format
: SPI3_CR1_RXONLY ( -- x addr ) 10 bit SPI3_CR1 ; \ SPI3_CR1_RXONLY, Receive only
: SPI3_CR1_SSM ( -- x addr ) 9 bit SPI3_CR1 ; \ SPI3_CR1_SSM, Software slave management
: SPI3_CR1_SSI ( -- x addr ) 8 bit SPI3_CR1 ; \ SPI3_CR1_SSI, Internal slave select
: SPI3_CR1_LSBFIRST ( -- x addr ) 7 bit SPI3_CR1 ; \ SPI3_CR1_LSBFIRST, Frame format
: SPI3_CR1_SPE ( -- x addr ) 6 bit SPI3_CR1 ; \ SPI3_CR1_SPE, SPI enable
: SPI3_CR1_BR ( %bbb -- x addr ) 3 lshift SPI3_CR1 ; \ SPI3_CR1_BR, Baud rate control
: SPI3_CR1_MSTR ( -- x addr ) 2 bit SPI3_CR1 ; \ SPI3_CR1_MSTR, Master selection
: SPI3_CR1_CPOL ( -- x addr ) 1 bit SPI3_CR1 ; \ SPI3_CR1_CPOL, Clock polarity
: SPI3_CR1_CPHA ( -- x addr ) 0 bit SPI3_CR1 ; \ SPI3_CR1_CPHA, Clock phase
[then]

defined? use-SPI3 defined? SPI3_CR2_RXDMAEN not and [if]
\ SPI3_CR2 (read-write) Reset:0x0000
: SPI3_CR2_RXDMAEN ( -- x addr ) 0 bit SPI3_CR2 ; \ SPI3_CR2_RXDMAEN, Rx buffer DMA enable
: SPI3_CR2_TXDMAEN ( -- x addr ) 1 bit SPI3_CR2 ; \ SPI3_CR2_TXDMAEN, Tx buffer DMA enable
: SPI3_CR2_SSOE ( -- x addr ) 2 bit SPI3_CR2 ; \ SPI3_CR2_SSOE, SS output enable
: SPI3_CR2_NSSP ( -- x addr ) 3 bit SPI3_CR2 ; \ SPI3_CR2_NSSP, NSS pulse management
: SPI3_CR2_FRF ( -- x addr ) 4 bit SPI3_CR2 ; \ SPI3_CR2_FRF, Frame format
: SPI3_CR2_ERRIE ( -- x addr ) 5 bit SPI3_CR2 ; \ SPI3_CR2_ERRIE, Error interrupt enable
: SPI3_CR2_RXNEIE ( -- x addr ) 6 bit SPI3_CR2 ; \ SPI3_CR2_RXNEIE, RX buffer not empty interrupt  enable
: SPI3_CR2_TXEIE ( -- x addr ) 7 bit SPI3_CR2 ; \ SPI3_CR2_TXEIE, Tx buffer empty interrupt  enable
: SPI3_CR2_DS ( %bbbb -- x addr ) 8 lshift SPI3_CR2 ; \ SPI3_CR2_DS, Data size
: SPI3_CR2_FRXTH ( -- x addr ) 12 bit SPI3_CR2 ; \ SPI3_CR2_FRXTH, FIFO reception threshold
: SPI3_CR2_LDMA_RX ( -- x addr ) 13 bit SPI3_CR2 ; \ SPI3_CR2_LDMA_RX, Last DMA transfer for  reception
: SPI3_CR2_LDMA_TX ( -- x addr ) 14 bit SPI3_CR2 ; \ SPI3_CR2_LDMA_TX, Last DMA transfer for  transmission
[then]

execute-defined? use-SPI3 defined? SPI3_SR_RXNE not and [if]
\ SPI3_SR (multiple-access)  Reset:0x0002
: SPI3_SR_RXNE ( -- x addr ) 0 bit SPI3_SR ; \ SPI3_SR_RXNE, Receive buffer not empty
: SPI3_SR_TXE ( -- x addr ) 1 bit SPI3_SR ; \ SPI3_SR_TXE, Transmit buffer empty
: SPI3_SR_CRCERR? ( -- 1|0 ) 4 bit SPI3_SR bit@ ; \ SPI3_SR_CRCERR, CRC error flag
: SPI3_SR_MODF ( -- x addr ) 5 bit SPI3_SR ; \ SPI3_SR_MODF, Mode fault
: SPI3_SR_OVR? ( -- 1|0 ) 6 bit SPI3_SR bit@ ; \ SPI3_SR_OVR, Overrun flag
: SPI3_SR_BSY? ( -- 1|0 ) 7 bit SPI3_SR bit@ ; \ SPI3_SR_BSY, Busy flag
: SPI3_SR_TIFRFE ( -- x addr ) 8 bit SPI3_SR ; \ SPI3_SR_TIFRFE, TI frame format error
: SPI3_SR_FRLVL ( %bb -- x addr ) 9 lshift SPI3_SR ; \ SPI3_SR_FRLVL, FIFO reception level
: SPI3_SR_FTLVL ( %bb -- x addr ) 11 lshift SPI3_SR ; \ SPI3_SR_FTLVL, FIFO transmission level
[then]

defined? use-SPI3 defined? SPI3_DR_DR not and [if]
\ SPI3_DR (read-write) Reset:0x0000
: SPI3_DR_DR ( %bbbbbbbbbbbbbbbb -- x addr ) SPI3_DR ; \ SPI3_DR_DR, Data register
[then]

execute-defined? use-SPI3 defined? SPI3_CRCPR_CRCPOLY not and [if]
\ SPI3_CRCPR (read-write) Reset:0x0007
: SPI3_CRCPR_CRCPOLY ( %bbbbbbbbbbbbbbbb -- x addr ) SPI3_CRCPR ; \ SPI3_CRCPR_CRCPOLY, CRC polynomial register
[then]

defined? use-SPI3 defined? SPI3_RXCRCR_RxCRC? not and [if]
\ SPI3_RXCRCR (read-only) Reset:0x0000
: SPI3_RXCRCR_RxCRC? ( --  x ) SPI3_RXCRCR @ ; \ SPI3_RXCRCR_RxCRC, Rx CRC register
[then]

execute-defined? use-SPI3 defined? SPI3_TXCRCR_TxCRC? not and [if]
\ SPI3_TXCRCR (read-only) Reset:0x0000
: SPI3_TXCRCR_TxCRC? ( --  x ) SPI3_TXCRCR @ ; \ SPI3_TXCRCR_TxCRC, Tx CRC register
[then]

defined? use-EXTI defined? EXTI_IMR1_MR0 not and [if]
\ EXTI_IMR1 (read-write) Reset:0xFF820000
: EXTI_IMR1_MR0 ( -- x addr ) 0 bit EXTI_IMR1 ; \ EXTI_IMR1_MR0, Interrupt Mask on line 0
: EXTI_IMR1_MR1 ( -- x addr ) 1 bit EXTI_IMR1 ; \ EXTI_IMR1_MR1, Interrupt Mask on line 1
: EXTI_IMR1_MR2 ( -- x addr ) 2 bit EXTI_IMR1 ; \ EXTI_IMR1_MR2, Interrupt Mask on line 2
: EXTI_IMR1_MR3 ( -- x addr ) 3 bit EXTI_IMR1 ; \ EXTI_IMR1_MR3, Interrupt Mask on line 3
: EXTI_IMR1_MR4 ( -- x addr ) 4 bit EXTI_IMR1 ; \ EXTI_IMR1_MR4, Interrupt Mask on line 4
: EXTI_IMR1_MR5 ( -- x addr ) 5 bit EXTI_IMR1 ; \ EXTI_IMR1_MR5, Interrupt Mask on line 5
: EXTI_IMR1_MR6 ( -- x addr ) 6 bit EXTI_IMR1 ; \ EXTI_IMR1_MR6, Interrupt Mask on line 6
: EXTI_IMR1_MR7 ( -- x addr ) 7 bit EXTI_IMR1 ; \ EXTI_IMR1_MR7, Interrupt Mask on line 7
: EXTI_IMR1_MR8 ( -- x addr ) 8 bit EXTI_IMR1 ; \ EXTI_IMR1_MR8, Interrupt Mask on line 8
: EXTI_IMR1_MR9 ( -- x addr ) 9 bit EXTI_IMR1 ; \ EXTI_IMR1_MR9, Interrupt Mask on line 9
: EXTI_IMR1_MR10 ( -- x addr ) 10 bit EXTI_IMR1 ; \ EXTI_IMR1_MR10, Interrupt Mask on line 10
: EXTI_IMR1_MR11 ( -- x addr ) 11 bit EXTI_IMR1 ; \ EXTI_IMR1_MR11, Interrupt Mask on line 11
: EXTI_IMR1_MR12 ( -- x addr ) 12 bit EXTI_IMR1 ; \ EXTI_IMR1_MR12, Interrupt Mask on line 12
: EXTI_IMR1_MR13 ( -- x addr ) 13 bit EXTI_IMR1 ; \ EXTI_IMR1_MR13, Interrupt Mask on line 13
: EXTI_IMR1_MR14 ( -- x addr ) 14 bit EXTI_IMR1 ; \ EXTI_IMR1_MR14, Interrupt Mask on line 14
: EXTI_IMR1_MR15 ( -- x addr ) 15 bit EXTI_IMR1 ; \ EXTI_IMR1_MR15, Interrupt Mask on line 15
: EXTI_IMR1_MR16 ( -- x addr ) 16 bit EXTI_IMR1 ; \ EXTI_IMR1_MR16, Interrupt Mask on line 16
: EXTI_IMR1_MR17 ( -- x addr ) 17 bit EXTI_IMR1 ; \ EXTI_IMR1_MR17, Interrupt Mask on line 17
: EXTI_IMR1_MR18 ( -- x addr ) 18 bit EXTI_IMR1 ; \ EXTI_IMR1_MR18, Interrupt Mask on line 18
: EXTI_IMR1_MR19 ( -- x addr ) 19 bit EXTI_IMR1 ; \ EXTI_IMR1_MR19, Interrupt Mask on line 19
: EXTI_IMR1_MR20 ( -- x addr ) 20 bit EXTI_IMR1 ; \ EXTI_IMR1_MR20, Interrupt Mask on line 20
: EXTI_IMR1_MR21 ( -- x addr ) 21 bit EXTI_IMR1 ; \ EXTI_IMR1_MR21, Interrupt Mask on line 21
: EXTI_IMR1_MR22 ( -- x addr ) 22 bit EXTI_IMR1 ; \ EXTI_IMR1_MR22, Interrupt Mask on line 22
: EXTI_IMR1_MR23 ( -- x addr ) 23 bit EXTI_IMR1 ; \ EXTI_IMR1_MR23, Interrupt Mask on line 23
: EXTI_IMR1_MR24 ( -- x addr ) 24 bit EXTI_IMR1 ; \ EXTI_IMR1_MR24, Interrupt Mask on line 24
: EXTI_IMR1_MR25 ( -- x addr ) 25 bit EXTI_IMR1 ; \ EXTI_IMR1_MR25, Interrupt Mask on line 25
: EXTI_IMR1_MR26 ( -- x addr ) 26 bit EXTI_IMR1 ; \ EXTI_IMR1_MR26, Interrupt Mask on line 26
: EXTI_IMR1_MR27 ( -- x addr ) 27 bit EXTI_IMR1 ; \ EXTI_IMR1_MR27, Interrupt Mask on line 27
: EXTI_IMR1_MR28 ( -- x addr ) 28 bit EXTI_IMR1 ; \ EXTI_IMR1_MR28, Interrupt Mask on line 28
: EXTI_IMR1_MR29 ( -- x addr ) 29 bit EXTI_IMR1 ; \ EXTI_IMR1_MR29, Interrupt Mask on line 29
: EXTI_IMR1_MR30 ( -- x addr ) 30 bit EXTI_IMR1 ; \ EXTI_IMR1_MR30, Interrupt Mask on line 30
: EXTI_IMR1_MR31 ( -- x addr ) 31 bit EXTI_IMR1 ; \ EXTI_IMR1_MR31, Interrupt Mask on line 31
[then]

execute-defined? use-EXTI defined? EXTI_EMR1_MR0 not and [if]
\ EXTI_EMR1 (read-write) Reset:0x00000000
: EXTI_EMR1_MR0 ( -- x addr ) 0 bit EXTI_EMR1 ; \ EXTI_EMR1_MR0, Event Mask on line 0
: EXTI_EMR1_MR1 ( -- x addr ) 1 bit EXTI_EMR1 ; \ EXTI_EMR1_MR1, Event Mask on line 1
: EXTI_EMR1_MR2 ( -- x addr ) 2 bit EXTI_EMR1 ; \ EXTI_EMR1_MR2, Event Mask on line 2
: EXTI_EMR1_MR3 ( -- x addr ) 3 bit EXTI_EMR1 ; \ EXTI_EMR1_MR3, Event Mask on line 3
: EXTI_EMR1_MR4 ( -- x addr ) 4 bit EXTI_EMR1 ; \ EXTI_EMR1_MR4, Event Mask on line 4
: EXTI_EMR1_MR5 ( -- x addr ) 5 bit EXTI_EMR1 ; \ EXTI_EMR1_MR5, Event Mask on line 5
: EXTI_EMR1_MR6 ( -- x addr ) 6 bit EXTI_EMR1 ; \ EXTI_EMR1_MR6, Event Mask on line 6
: EXTI_EMR1_MR7 ( -- x addr ) 7 bit EXTI_EMR1 ; \ EXTI_EMR1_MR7, Event Mask on line 7
: EXTI_EMR1_MR8 ( -- x addr ) 8 bit EXTI_EMR1 ; \ EXTI_EMR1_MR8, Event Mask on line 8
: EXTI_EMR1_MR9 ( -- x addr ) 9 bit EXTI_EMR1 ; \ EXTI_EMR1_MR9, Event Mask on line 9
: EXTI_EMR1_MR10 ( -- x addr ) 10 bit EXTI_EMR1 ; \ EXTI_EMR1_MR10, Event Mask on line 10
: EXTI_EMR1_MR11 ( -- x addr ) 11 bit EXTI_EMR1 ; \ EXTI_EMR1_MR11, Event Mask on line 11
: EXTI_EMR1_MR12 ( -- x addr ) 12 bit EXTI_EMR1 ; \ EXTI_EMR1_MR12, Event Mask on line 12
: EXTI_EMR1_MR13 ( -- x addr ) 13 bit EXTI_EMR1 ; \ EXTI_EMR1_MR13, Event Mask on line 13
: EXTI_EMR1_MR14 ( -- x addr ) 14 bit EXTI_EMR1 ; \ EXTI_EMR1_MR14, Event Mask on line 14
: EXTI_EMR1_MR15 ( -- x addr ) 15 bit EXTI_EMR1 ; \ EXTI_EMR1_MR15, Event Mask on line 15
: EXTI_EMR1_MR16 ( -- x addr ) 16 bit EXTI_EMR1 ; \ EXTI_EMR1_MR16, Event Mask on line 16
: EXTI_EMR1_MR17 ( -- x addr ) 17 bit EXTI_EMR1 ; \ EXTI_EMR1_MR17, Event Mask on line 17
: EXTI_EMR1_MR18 ( -- x addr ) 18 bit EXTI_EMR1 ; \ EXTI_EMR1_MR18, Event Mask on line 18
: EXTI_EMR1_MR19 ( -- x addr ) 19 bit EXTI_EMR1 ; \ EXTI_EMR1_MR19, Event Mask on line 19
: EXTI_EMR1_MR20 ( -- x addr ) 20 bit EXTI_EMR1 ; \ EXTI_EMR1_MR20, Event Mask on line 20
: EXTI_EMR1_MR21 ( -- x addr ) 21 bit EXTI_EMR1 ; \ EXTI_EMR1_MR21, Event Mask on line 21
: EXTI_EMR1_MR22 ( -- x addr ) 22 bit EXTI_EMR1 ; \ EXTI_EMR1_MR22, Event Mask on line 22
: EXTI_EMR1_MR23 ( -- x addr ) 23 bit EXTI_EMR1 ; \ EXTI_EMR1_MR23, Event Mask on line 23
: EXTI_EMR1_MR24 ( -- x addr ) 24 bit EXTI_EMR1 ; \ EXTI_EMR1_MR24, Event Mask on line 24
: EXTI_EMR1_MR25 ( -- x addr ) 25 bit EXTI_EMR1 ; \ EXTI_EMR1_MR25, Event Mask on line 25
: EXTI_EMR1_MR26 ( -- x addr ) 26 bit EXTI_EMR1 ; \ EXTI_EMR1_MR26, Event Mask on line 26
: EXTI_EMR1_MR27 ( -- x addr ) 27 bit EXTI_EMR1 ; \ EXTI_EMR1_MR27, Event Mask on line 27
: EXTI_EMR1_MR28 ( -- x addr ) 28 bit EXTI_EMR1 ; \ EXTI_EMR1_MR28, Event Mask on line 28
: EXTI_EMR1_MR29 ( -- x addr ) 29 bit EXTI_EMR1 ; \ EXTI_EMR1_MR29, Event Mask on line 29
: EXTI_EMR1_MR30 ( -- x addr ) 30 bit EXTI_EMR1 ; \ EXTI_EMR1_MR30, Event Mask on line 30
: EXTI_EMR1_MR31 ( -- x addr ) 31 bit EXTI_EMR1 ; \ EXTI_EMR1_MR31, Event Mask on line 31
[then]

defined? use-EXTI defined? EXTI_RTSR1_TR0 not and [if]
\ EXTI_RTSR1 (read-write) Reset:0x00000000
: EXTI_RTSR1_TR0 ( -- x addr ) 0 bit EXTI_RTSR1 ; \ EXTI_RTSR1_TR0, Rising trigger event configuration of  line 0
: EXTI_RTSR1_TR1 ( -- x addr ) 1 bit EXTI_RTSR1 ; \ EXTI_RTSR1_TR1, Rising trigger event configuration of  line 1
: EXTI_RTSR1_TR2 ( -- x addr ) 2 bit EXTI_RTSR1 ; \ EXTI_RTSR1_TR2, Rising trigger event configuration of  line 2
: EXTI_RTSR1_TR3 ( -- x addr ) 3 bit EXTI_RTSR1 ; \ EXTI_RTSR1_TR3, Rising trigger event configuration of  line 3
: EXTI_RTSR1_TR4 ( -- x addr ) 4 bit EXTI_RTSR1 ; \ EXTI_RTSR1_TR4, Rising trigger event configuration of  line 4
: EXTI_RTSR1_TR5 ( -- x addr ) 5 bit EXTI_RTSR1 ; \ EXTI_RTSR1_TR5, Rising trigger event configuration of  line 5
: EXTI_RTSR1_TR6 ( -- x addr ) 6 bit EXTI_RTSR1 ; \ EXTI_RTSR1_TR6, Rising trigger event configuration of  line 6
: EXTI_RTSR1_TR7 ( -- x addr ) 7 bit EXTI_RTSR1 ; \ EXTI_RTSR1_TR7, Rising trigger event configuration of  line 7
: EXTI_RTSR1_TR8 ( -- x addr ) 8 bit EXTI_RTSR1 ; \ EXTI_RTSR1_TR8, Rising trigger event configuration of  line 8
: EXTI_RTSR1_TR9 ( -- x addr ) 9 bit EXTI_RTSR1 ; \ EXTI_RTSR1_TR9, Rising trigger event configuration of  line 9
: EXTI_RTSR1_TR10 ( -- x addr ) 10 bit EXTI_RTSR1 ; \ EXTI_RTSR1_TR10, Rising trigger event configuration of  line 10
: EXTI_RTSR1_TR11 ( -- x addr ) 11 bit EXTI_RTSR1 ; \ EXTI_RTSR1_TR11, Rising trigger event configuration of  line 11
: EXTI_RTSR1_TR12 ( -- x addr ) 12 bit EXTI_RTSR1 ; \ EXTI_RTSR1_TR12, Rising trigger event configuration of  line 12
: EXTI_RTSR1_TR13 ( -- x addr ) 13 bit EXTI_RTSR1 ; \ EXTI_RTSR1_TR13, Rising trigger event configuration of  line 13
: EXTI_RTSR1_TR14 ( -- x addr ) 14 bit EXTI_RTSR1 ; \ EXTI_RTSR1_TR14, Rising trigger event configuration of  line 14
: EXTI_RTSR1_TR15 ( -- x addr ) 15 bit EXTI_RTSR1 ; \ EXTI_RTSR1_TR15, Rising trigger event configuration of  line 15
: EXTI_RTSR1_TR16 ( -- x addr ) 16 bit EXTI_RTSR1 ; \ EXTI_RTSR1_TR16, Rising trigger event configuration of  line 16
: EXTI_RTSR1_TR18 ( -- x addr ) 18 bit EXTI_RTSR1 ; \ EXTI_RTSR1_TR18, Rising trigger event configuration of  line 18
: EXTI_RTSR1_TR19 ( -- x addr ) 19 bit EXTI_RTSR1 ; \ EXTI_RTSR1_TR19, Rising trigger event configuration of  line 19
: EXTI_RTSR1_TR20 ( -- x addr ) 20 bit EXTI_RTSR1 ; \ EXTI_RTSR1_TR20, Rising trigger event configuration of  line 20
: EXTI_RTSR1_TR21 ( -- x addr ) 21 bit EXTI_RTSR1 ; \ EXTI_RTSR1_TR21, Rising trigger event configuration of  line 21
: EXTI_RTSR1_TR22 ( -- x addr ) 22 bit EXTI_RTSR1 ; \ EXTI_RTSR1_TR22, Rising trigger event configuration of  line 22
[then]

execute-defined? use-EXTI defined? EXTI_FTSR1_TR0 not and [if]
\ EXTI_FTSR1 (read-write) Reset:0x00000000
: EXTI_FTSR1_TR0 ( -- x addr ) 0 bit EXTI_FTSR1 ; \ EXTI_FTSR1_TR0, Falling trigger event configuration of  line 0
: EXTI_FTSR1_TR1 ( -- x addr ) 1 bit EXTI_FTSR1 ; \ EXTI_FTSR1_TR1, Falling trigger event configuration of  line 1
: EXTI_FTSR1_TR2 ( -- x addr ) 2 bit EXTI_FTSR1 ; \ EXTI_FTSR1_TR2, Falling trigger event configuration of  line 2
: EXTI_FTSR1_TR3 ( -- x addr ) 3 bit EXTI_FTSR1 ; \ EXTI_FTSR1_TR3, Falling trigger event configuration of  line 3
: EXTI_FTSR1_TR4 ( -- x addr ) 4 bit EXTI_FTSR1 ; \ EXTI_FTSR1_TR4, Falling trigger event configuration of  line 4
: EXTI_FTSR1_TR5 ( -- x addr ) 5 bit EXTI_FTSR1 ; \ EXTI_FTSR1_TR5, Falling trigger event configuration of  line 5
: EXTI_FTSR1_TR6 ( -- x addr ) 6 bit EXTI_FTSR1 ; \ EXTI_FTSR1_TR6, Falling trigger event configuration of  line 6
: EXTI_FTSR1_TR7 ( -- x addr ) 7 bit EXTI_FTSR1 ; \ EXTI_FTSR1_TR7, Falling trigger event configuration of  line 7
: EXTI_FTSR1_TR8 ( -- x addr ) 8 bit EXTI_FTSR1 ; \ EXTI_FTSR1_TR8, Falling trigger event configuration of  line 8
: EXTI_FTSR1_TR9 ( -- x addr ) 9 bit EXTI_FTSR1 ; \ EXTI_FTSR1_TR9, Falling trigger event configuration of  line 9
: EXTI_FTSR1_TR10 ( -- x addr ) 10 bit EXTI_FTSR1 ; \ EXTI_FTSR1_TR10, Falling trigger event configuration of  line 10
: EXTI_FTSR1_TR11 ( -- x addr ) 11 bit EXTI_FTSR1 ; \ EXTI_FTSR1_TR11, Falling trigger event configuration of  line 11
: EXTI_FTSR1_TR12 ( -- x addr ) 12 bit EXTI_FTSR1 ; \ EXTI_FTSR1_TR12, Falling trigger event configuration of  line 12
: EXTI_FTSR1_TR13 ( -- x addr ) 13 bit EXTI_FTSR1 ; \ EXTI_FTSR1_TR13, Falling trigger event configuration of  line 13
: EXTI_FTSR1_TR14 ( -- x addr ) 14 bit EXTI_FTSR1 ; \ EXTI_FTSR1_TR14, Falling trigger event configuration of  line 14
: EXTI_FTSR1_TR15 ( -- x addr ) 15 bit EXTI_FTSR1 ; \ EXTI_FTSR1_TR15, Falling trigger event configuration of  line 15
: EXTI_FTSR1_TR16 ( -- x addr ) 16 bit EXTI_FTSR1 ; \ EXTI_FTSR1_TR16, Falling trigger event configuration of  line 16
: EXTI_FTSR1_TR18 ( -- x addr ) 18 bit EXTI_FTSR1 ; \ EXTI_FTSR1_TR18, Falling trigger event configuration of  line 18
: EXTI_FTSR1_TR19 ( -- x addr ) 19 bit EXTI_FTSR1 ; \ EXTI_FTSR1_TR19, Falling trigger event configuration of  line 19
: EXTI_FTSR1_TR20 ( -- x addr ) 20 bit EXTI_FTSR1 ; \ EXTI_FTSR1_TR20, Falling trigger event configuration of  line 20
: EXTI_FTSR1_TR21 ( -- x addr ) 21 bit EXTI_FTSR1 ; \ EXTI_FTSR1_TR21, Falling trigger event configuration of  line 21
: EXTI_FTSR1_TR22 ( -- x addr ) 22 bit EXTI_FTSR1 ; \ EXTI_FTSR1_TR22, Falling trigger event configuration of  line 22
[then]

defined? use-EXTI defined? EXTI_SWIER1_SWIER0 not and [if]
\ EXTI_SWIER1 (read-write) Reset:0x00000000
: EXTI_SWIER1_SWIER0 ( -- x addr ) 0 bit EXTI_SWIER1 ; \ EXTI_SWIER1_SWIER0, Software Interrupt on line  0
: EXTI_SWIER1_SWIER1 ( -- x addr ) 1 bit EXTI_SWIER1 ; \ EXTI_SWIER1_SWIER1, Software Interrupt on line  1
: EXTI_SWIER1_SWIER2 ( -- x addr ) 2 bit EXTI_SWIER1 ; \ EXTI_SWIER1_SWIER2, Software Interrupt on line  2
: EXTI_SWIER1_SWIER3 ( -- x addr ) 3 bit EXTI_SWIER1 ; \ EXTI_SWIER1_SWIER3, Software Interrupt on line  3
: EXTI_SWIER1_SWIER4 ( -- x addr ) 4 bit EXTI_SWIER1 ; \ EXTI_SWIER1_SWIER4, Software Interrupt on line  4
: EXTI_SWIER1_SWIER5 ( -- x addr ) 5 bit EXTI_SWIER1 ; \ EXTI_SWIER1_SWIER5, Software Interrupt on line  5
: EXTI_SWIER1_SWIER6 ( -- x addr ) 6 bit EXTI_SWIER1 ; \ EXTI_SWIER1_SWIER6, Software Interrupt on line  6
: EXTI_SWIER1_SWIER7 ( -- x addr ) 7 bit EXTI_SWIER1 ; \ EXTI_SWIER1_SWIER7, Software Interrupt on line  7
: EXTI_SWIER1_SWIER8 ( -- x addr ) 8 bit EXTI_SWIER1 ; \ EXTI_SWIER1_SWIER8, Software Interrupt on line  8
: EXTI_SWIER1_SWIER9 ( -- x addr ) 9 bit EXTI_SWIER1 ; \ EXTI_SWIER1_SWIER9, Software Interrupt on line  9
: EXTI_SWIER1_SWIER10 ( -- x addr ) 10 bit EXTI_SWIER1 ; \ EXTI_SWIER1_SWIER10, Software Interrupt on line  10
: EXTI_SWIER1_SWIER11 ( -- x addr ) 11 bit EXTI_SWIER1 ; \ EXTI_SWIER1_SWIER11, Software Interrupt on line  11
: EXTI_SWIER1_SWIER12 ( -- x addr ) 12 bit EXTI_SWIER1 ; \ EXTI_SWIER1_SWIER12, Software Interrupt on line  12
: EXTI_SWIER1_SWIER13 ( -- x addr ) 13 bit EXTI_SWIER1 ; \ EXTI_SWIER1_SWIER13, Software Interrupt on line  13
: EXTI_SWIER1_SWIER14 ( -- x addr ) 14 bit EXTI_SWIER1 ; \ EXTI_SWIER1_SWIER14, Software Interrupt on line  14
: EXTI_SWIER1_SWIER15 ( -- x addr ) 15 bit EXTI_SWIER1 ; \ EXTI_SWIER1_SWIER15, Software Interrupt on line  15
: EXTI_SWIER1_SWIER16 ( -- x addr ) 16 bit EXTI_SWIER1 ; \ EXTI_SWIER1_SWIER16, Software Interrupt on line  16
: EXTI_SWIER1_SWIER18 ( -- x addr ) 18 bit EXTI_SWIER1 ; \ EXTI_SWIER1_SWIER18, Software Interrupt on line  18
: EXTI_SWIER1_SWIER19 ( -- x addr ) 19 bit EXTI_SWIER1 ; \ EXTI_SWIER1_SWIER19, Software Interrupt on line  19
: EXTI_SWIER1_SWIER20 ( -- x addr ) 20 bit EXTI_SWIER1 ; \ EXTI_SWIER1_SWIER20, Software Interrupt on line  20
: EXTI_SWIER1_SWIER21 ( -- x addr ) 21 bit EXTI_SWIER1 ; \ EXTI_SWIER1_SWIER21, Software Interrupt on line  21
: EXTI_SWIER1_SWIER22 ( -- x addr ) 22 bit EXTI_SWIER1 ; \ EXTI_SWIER1_SWIER22, Software Interrupt on line  22
[then]

execute-defined? use-EXTI defined? EXTI_PR1_PR0 not and [if]
\ EXTI_PR1 (read-write) Reset:0x00000000
: EXTI_PR1_PR0 ( -- x addr ) 0 bit EXTI_PR1 ; \ EXTI_PR1_PR0, Pending bit 0
: EXTI_PR1_PR1 ( -- x addr ) 1 bit EXTI_PR1 ; \ EXTI_PR1_PR1, Pending bit 1
: EXTI_PR1_PR2 ( -- x addr ) 2 bit EXTI_PR1 ; \ EXTI_PR1_PR2, Pending bit 2
: EXTI_PR1_PR3 ( -- x addr ) 3 bit EXTI_PR1 ; \ EXTI_PR1_PR3, Pending bit 3
: EXTI_PR1_PR4 ( -- x addr ) 4 bit EXTI_PR1 ; \ EXTI_PR1_PR4, Pending bit 4
: EXTI_PR1_PR5 ( -- x addr ) 5 bit EXTI_PR1 ; \ EXTI_PR1_PR5, Pending bit 5
: EXTI_PR1_PR6 ( -- x addr ) 6 bit EXTI_PR1 ; \ EXTI_PR1_PR6, Pending bit 6
: EXTI_PR1_PR7 ( -- x addr ) 7 bit EXTI_PR1 ; \ EXTI_PR1_PR7, Pending bit 7
: EXTI_PR1_PR8 ( -- x addr ) 8 bit EXTI_PR1 ; \ EXTI_PR1_PR8, Pending bit 8
: EXTI_PR1_PR9 ( -- x addr ) 9 bit EXTI_PR1 ; \ EXTI_PR1_PR9, Pending bit 9
: EXTI_PR1_PR10 ( -- x addr ) 10 bit EXTI_PR1 ; \ EXTI_PR1_PR10, Pending bit 10
: EXTI_PR1_PR11 ( -- x addr ) 11 bit EXTI_PR1 ; \ EXTI_PR1_PR11, Pending bit 11
: EXTI_PR1_PR12 ( -- x addr ) 12 bit EXTI_PR1 ; \ EXTI_PR1_PR12, Pending bit 12
: EXTI_PR1_PR13 ( -- x addr ) 13 bit EXTI_PR1 ; \ EXTI_PR1_PR13, Pending bit 13
: EXTI_PR1_PR14 ( -- x addr ) 14 bit EXTI_PR1 ; \ EXTI_PR1_PR14, Pending bit 14
: EXTI_PR1_PR15 ( -- x addr ) 15 bit EXTI_PR1 ; \ EXTI_PR1_PR15, Pending bit 15
: EXTI_PR1_PR16 ( -- x addr ) 16 bit EXTI_PR1 ; \ EXTI_PR1_PR16, Pending bit 16
: EXTI_PR1_PR18 ( -- x addr ) 18 bit EXTI_PR1 ; \ EXTI_PR1_PR18, Pending bit 18
: EXTI_PR1_PR19 ( -- x addr ) 19 bit EXTI_PR1 ; \ EXTI_PR1_PR19, Pending bit 19
: EXTI_PR1_PR20 ( -- x addr ) 20 bit EXTI_PR1 ; \ EXTI_PR1_PR20, Pending bit 20
: EXTI_PR1_PR21 ( -- x addr ) 21 bit EXTI_PR1 ; \ EXTI_PR1_PR21, Pending bit 21
: EXTI_PR1_PR22 ( -- x addr ) 22 bit EXTI_PR1 ; \ EXTI_PR1_PR22, Pending bit 22
[then]

defined? use-EXTI defined? EXTI_IMR2_MR32 not and [if]
\ EXTI_IMR2 (read-write) Reset:0xFFFFFF87
: EXTI_IMR2_MR32 ( -- x addr ) 0 bit EXTI_IMR2 ; \ EXTI_IMR2_MR32, Interrupt Mask on external/internal line  32
: EXTI_IMR2_MR33 ( -- x addr ) 1 bit EXTI_IMR2 ; \ EXTI_IMR2_MR33, Interrupt Mask on external/internal line  33
: EXTI_IMR2_MR34 ( -- x addr ) 2 bit EXTI_IMR2 ; \ EXTI_IMR2_MR34, Interrupt Mask on external/internal line  34
: EXTI_IMR2_MR35 ( -- x addr ) 3 bit EXTI_IMR2 ; \ EXTI_IMR2_MR35, Interrupt Mask on external/internal line  35
: EXTI_IMR2_MR36 ( -- x addr ) 4 bit EXTI_IMR2 ; \ EXTI_IMR2_MR36, Interrupt Mask on external/internal line  36
: EXTI_IMR2_MR37 ( -- x addr ) 5 bit EXTI_IMR2 ; \ EXTI_IMR2_MR37, Interrupt Mask on external/internal line  37
: EXTI_IMR2_MR38 ( -- x addr ) 6 bit EXTI_IMR2 ; \ EXTI_IMR2_MR38, Interrupt Mask on external/internal line  38
: EXTI_IMR2_MR39 ( -- x addr ) 7 bit EXTI_IMR2 ; \ EXTI_IMR2_MR39, Interrupt Mask on external/internal line  39
[then]

execute-defined? use-EXTI defined? EXTI_EMR2_MR32 not and [if]
\ EXTI_EMR2 (read-write) Reset:0x00000000
: EXTI_EMR2_MR32 ( -- x addr ) 0 bit EXTI_EMR2 ; \ EXTI_EMR2_MR32, Event mask on external/internal line  32
: EXTI_EMR2_MR33 ( -- x addr ) 1 bit EXTI_EMR2 ; \ EXTI_EMR2_MR33, Event mask on external/internal line  33
: EXTI_EMR2_MR34 ( -- x addr ) 2 bit EXTI_EMR2 ; \ EXTI_EMR2_MR34, Event mask on external/internal line  34
: EXTI_EMR2_MR35 ( -- x addr ) 3 bit EXTI_EMR2 ; \ EXTI_EMR2_MR35, Event mask on external/internal line  35
: EXTI_EMR2_MR36 ( -- x addr ) 4 bit EXTI_EMR2 ; \ EXTI_EMR2_MR36, Event mask on external/internal line  36
: EXTI_EMR2_MR37 ( -- x addr ) 5 bit EXTI_EMR2 ; \ EXTI_EMR2_MR37, Event mask on external/internal line  37
: EXTI_EMR2_MR38 ( -- x addr ) 6 bit EXTI_EMR2 ; \ EXTI_EMR2_MR38, Event mask on external/internal line  38
: EXTI_EMR2_MR39 ( -- x addr ) 7 bit EXTI_EMR2 ; \ EXTI_EMR2_MR39, Event mask on external/internal line  39
[then]

defined? use-EXTI defined? EXTI_RTSR2_RT35 not and [if]
\ EXTI_RTSR2 (read-write) Reset:0x00000000
: EXTI_RTSR2_RT35 ( -- x addr ) 3 bit EXTI_RTSR2 ; \ EXTI_RTSR2_RT35, Rising trigger event configuration bit  of line 35
: EXTI_RTSR2_RT36 ( -- x addr ) 4 bit EXTI_RTSR2 ; \ EXTI_RTSR2_RT36, Rising trigger event configuration bit  of line 36
: EXTI_RTSR2_RT37 ( -- x addr ) 5 bit EXTI_RTSR2 ; \ EXTI_RTSR2_RT37, Rising trigger event configuration bit  of line 37
: EXTI_RTSR2_RT38 ( -- x addr ) 6 bit EXTI_RTSR2 ; \ EXTI_RTSR2_RT38, Rising trigger event configuration bit  of line 38
[then]

execute-defined? use-EXTI defined? EXTI_FTSR2_FT35 not and [if]
\ EXTI_FTSR2 (read-write) Reset:0x00000000
: EXTI_FTSR2_FT35 ( -- x addr ) 3 bit EXTI_FTSR2 ; \ EXTI_FTSR2_FT35, Falling trigger event configuration bit  of line 35
: EXTI_FTSR2_FT36 ( -- x addr ) 4 bit EXTI_FTSR2 ; \ EXTI_FTSR2_FT36, Falling trigger event configuration bit  of line 36
: EXTI_FTSR2_FT37 ( -- x addr ) 5 bit EXTI_FTSR2 ; \ EXTI_FTSR2_FT37, Falling trigger event configuration bit  of line 37
: EXTI_FTSR2_FT38 ( -- x addr ) 6 bit EXTI_FTSR2 ; \ EXTI_FTSR2_FT38, Falling trigger event configuration bit  of line 38
[then]

defined? use-EXTI defined? EXTI_SWIER2_SWI35 not and [if]
\ EXTI_SWIER2 (read-write) Reset:0x00000000
: EXTI_SWIER2_SWI35 ( -- x addr ) 3 bit EXTI_SWIER2 ; \ EXTI_SWIER2_SWI35, Software interrupt on line  35
: EXTI_SWIER2_SWI36 ( -- x addr ) 4 bit EXTI_SWIER2 ; \ EXTI_SWIER2_SWI36, Software interrupt on line  36
: EXTI_SWIER2_SWI37 ( -- x addr ) 5 bit EXTI_SWIER2 ; \ EXTI_SWIER2_SWI37, Software interrupt on line  37
: EXTI_SWIER2_SWI38 ( -- x addr ) 6 bit EXTI_SWIER2 ; \ EXTI_SWIER2_SWI38, Software interrupt on line  38
[then]

execute-defined? use-EXTI defined? EXTI_PR2_PIF35 not and [if]
\ EXTI_PR2 (read-write) Reset:0x00000000
: EXTI_PR2_PIF35 ( -- x addr ) 3 bit EXTI_PR2 ; \ EXTI_PR2_PIF35, Pending interrupt flag on line  35
: EXTI_PR2_PIF36 ( -- x addr ) 4 bit EXTI_PR2 ; \ EXTI_PR2_PIF36, Pending interrupt flag on line  36
: EXTI_PR2_PIF37 ( -- x addr ) 5 bit EXTI_PR2 ; \ EXTI_PR2_PIF37, Pending interrupt flag on line  37
: EXTI_PR2_PIF38 ( -- x addr ) 6 bit EXTI_PR2 ; \ EXTI_PR2_PIF38, Pending interrupt flag on line  38
[then]

defined? use-RTC defined? RTC_TR_PM not and [if]
\ RTC_TR (read-write) Reset:0x00000000
: RTC_TR_PM ( -- x addr ) 22 bit RTC_TR ; \ RTC_TR_PM, AM/PM notation
: RTC_TR_HT ( %bb -- x addr ) 20 lshift RTC_TR ; \ RTC_TR_HT, Hour tens in BCD format
: RTC_TR_HU ( %bbbb -- x addr ) 16 lshift RTC_TR ; \ RTC_TR_HU, Hour units in BCD format
: RTC_TR_MNT ( %bbb -- x addr ) 12 lshift RTC_TR ; \ RTC_TR_MNT, Minute tens in BCD format
: RTC_TR_MNU ( %bbbb -- x addr ) 8 lshift RTC_TR ; \ RTC_TR_MNU, Minute units in BCD format
: RTC_TR_ST ( %bbb -- x addr ) 4 lshift RTC_TR ; \ RTC_TR_ST, Second tens in BCD format
: RTC_TR_SU ( %bbbb -- x addr ) RTC_TR ; \ RTC_TR_SU, Second units in BCD format
[then]

execute-defined? use-RTC defined? RTC_DR_YT not and [if]
\ RTC_DR (read-write) Reset:0x00002101
: RTC_DR_YT ( %bbbb -- x addr ) 20 lshift RTC_DR ; \ RTC_DR_YT, Year tens in BCD format
: RTC_DR_YU ( %bbbb -- x addr ) 16 lshift RTC_DR ; \ RTC_DR_YU, Year units in BCD format
: RTC_DR_WDU ( %bbb -- x addr ) 13 lshift RTC_DR ; \ RTC_DR_WDU, Week day units
: RTC_DR_MT ( -- x addr ) 12 bit RTC_DR ; \ RTC_DR_MT, Month tens in BCD format
: RTC_DR_MU ( %bbbb -- x addr ) 8 lshift RTC_DR ; \ RTC_DR_MU, Month units in BCD format
: RTC_DR_DT ( %bb -- x addr ) 4 lshift RTC_DR ; \ RTC_DR_DT, Date tens in BCD format
: RTC_DR_DU ( %bbbb -- x addr ) RTC_DR ; \ RTC_DR_DU, Date units in BCD format
[then]

defined? use-RTC defined? RTC_CR_WCKSEL not and [if]
\ RTC_CR (read-write) Reset:0x00000000
: RTC_CR_WCKSEL ( %bbb -- x addr ) RTC_CR ; \ RTC_CR_WCKSEL, Wakeup clock selection
: RTC_CR_TSEDGE ( -- x addr ) 3 bit RTC_CR ; \ RTC_CR_TSEDGE, Time-stamp event active  edge
: RTC_CR_REFCKON ( -- x addr ) 4 bit RTC_CR ; \ RTC_CR_REFCKON, Reference clock detection enable 50 or  60 Hz
: RTC_CR_BYPSHAD ( -- x addr ) 5 bit RTC_CR ; \ RTC_CR_BYPSHAD, Bypass the shadow  registers
: RTC_CR_FMT ( -- x addr ) 6 bit RTC_CR ; \ RTC_CR_FMT, Hour format
: RTC_CR_ALRAE ( -- x addr ) 8 bit RTC_CR ; \ RTC_CR_ALRAE, Alarm A enable
: RTC_CR_ALRBE ( -- x addr ) 9 bit RTC_CR ; \ RTC_CR_ALRBE, Alarm B enable
: RTC_CR_WUTE ( -- x addr ) 10 bit RTC_CR ; \ RTC_CR_WUTE, Wakeup timer enable
: RTC_CR_TSE ( -- x addr ) 11 bit RTC_CR ; \ RTC_CR_TSE, Time stamp enable
: RTC_CR_ALRAIE ( -- x addr ) 12 bit RTC_CR ; \ RTC_CR_ALRAIE, Alarm A interrupt enable
: RTC_CR_ALRBIE ( -- x addr ) 13 bit RTC_CR ; \ RTC_CR_ALRBIE, Alarm B interrupt enable
: RTC_CR_WUTIE ( -- x addr ) 14 bit RTC_CR ; \ RTC_CR_WUTIE, Wakeup timer interrupt  enable
: RTC_CR_TSIE ( -- x addr ) 15 bit RTC_CR ; \ RTC_CR_TSIE, Time-stamp interrupt  enable
: RTC_CR_ADD1H ( -- x addr ) 16 bit RTC_CR ; \ RTC_CR_ADD1H, Add 1 hour summer time  change
: RTC_CR_SUB1H ( -- x addr ) 17 bit RTC_CR ; \ RTC_CR_SUB1H, Subtract 1 hour winter time  change
: RTC_CR_BKP ( -- x addr ) 18 bit RTC_CR ; \ RTC_CR_BKP, Backup
: RTC_CR_COSEL ( -- x addr ) 19 bit RTC_CR ; \ RTC_CR_COSEL, Calibration output  selection
: RTC_CR_POL ( -- x addr ) 20 bit RTC_CR ; \ RTC_CR_POL, Output polarity
: RTC_CR_OSEL ( %bb -- x addr ) 21 lshift RTC_CR ; \ RTC_CR_OSEL, Output selection
: RTC_CR_COE ( -- x addr ) 23 bit RTC_CR ; \ RTC_CR_COE, Calibration output enable
: RTC_CR_ITSE ( -- x addr ) 24 bit RTC_CR ; \ RTC_CR_ITSE, timestamp on internal event  enable
[then]

execute-defined? use-RTC defined? RTC_ISR_ALRAWF? not and [if]
\ RTC_ISR (multiple-access)  Reset:0x00000007
: RTC_ISR_ALRAWF? ( -- 1|0 ) 0 bit RTC_ISR bit@ ; \ RTC_ISR_ALRAWF, Alarm A write flag
: RTC_ISR_ALRBWF? ( -- 1|0 ) 1 bit RTC_ISR bit@ ; \ RTC_ISR_ALRBWF, Alarm B write flag
: RTC_ISR_WUTWF? ( -- 1|0 ) 2 bit RTC_ISR bit@ ; \ RTC_ISR_WUTWF, Wakeup timer write flag
: RTC_ISR_SHPF ( -- x addr ) 3 bit RTC_ISR ; \ RTC_ISR_SHPF, Shift operation pending
: RTC_ISR_INITS? ( -- 1|0 ) 4 bit RTC_ISR bit@ ; \ RTC_ISR_INITS, Initialization status flag
: RTC_ISR_RSF? ( -- 1|0 ) 5 bit RTC_ISR bit@ ; \ RTC_ISR_RSF, Registers synchronization  flag
: RTC_ISR_INITF? ( -- 1|0 ) 6 bit RTC_ISR bit@ ; \ RTC_ISR_INITF, Initialization flag
: RTC_ISR_INIT ( -- x addr ) 7 bit RTC_ISR ; \ RTC_ISR_INIT, Initialization mode
: RTC_ISR_ALRAF? ( -- 1|0 ) 8 bit RTC_ISR bit@ ; \ RTC_ISR_ALRAF, Alarm A flag
: RTC_ISR_ALRBF? ( -- 1|0 ) 9 bit RTC_ISR bit@ ; \ RTC_ISR_ALRBF, Alarm B flag
: RTC_ISR_WUTF? ( -- 1|0 ) 10 bit RTC_ISR bit@ ; \ RTC_ISR_WUTF, Wakeup timer flag
: RTC_ISR_TSF? ( -- 1|0 ) 11 bit RTC_ISR bit@ ; \ RTC_ISR_TSF, Time-stamp flag
: RTC_ISR_TSOVF? ( -- 1|0 ) 12 bit RTC_ISR bit@ ; \ RTC_ISR_TSOVF, Time-stamp overflow flag
: RTC_ISR_TAMP1F? ( -- 1|0 ) 13 bit RTC_ISR bit@ ; \ RTC_ISR_TAMP1F, Tamper detection flag
: RTC_ISR_TAMP2F? ( -- 1|0 ) 14 bit RTC_ISR bit@ ; \ RTC_ISR_TAMP2F, RTC_TAMP2 detection flag
: RTC_ISR_TAMP3F? ( -- 1|0 ) 15 bit RTC_ISR bit@ ; \ RTC_ISR_TAMP3F, RTC_TAMP3 detection flag
: RTC_ISR_RECALPF ( -- x addr ) 16 bit RTC_ISR ; \ RTC_ISR_RECALPF, Recalibration pending Flag
[then]

defined? use-RTC defined? RTC_PRER_PREDIV_A not and [if]
\ RTC_PRER (read-write) Reset:0x007F00FF
: RTC_PRER_PREDIV_A ( %bbbbbbb -- x addr ) 16 lshift RTC_PRER ; \ RTC_PRER_PREDIV_A, Asynchronous prescaler  factor
: RTC_PRER_PREDIV_S ( %bbbbbbbbbbbbbbb -- x addr ) RTC_PRER ; \ RTC_PRER_PREDIV_S, Synchronous prescaler  factor
[then]

execute-defined? use-RTC defined? RTC_WUTR_WUT not and [if]
\ RTC_WUTR (read-write) Reset:0x0000FFFF
: RTC_WUTR_WUT ( %bbbbbbbbbbbbbbbb -- x addr ) RTC_WUTR ; \ RTC_WUTR_WUT, Wakeup auto-reload value  bits
[then]

defined? use-RTC defined? RTC_ALRMAR_MSK4 not and [if]
\ RTC_ALRMAR (read-write) Reset:0x00000000
: RTC_ALRMAR_MSK4 ( -- x addr ) 31 bit RTC_ALRMAR ; \ RTC_ALRMAR_MSK4, Alarm A date mask
: RTC_ALRMAR_WDSEL ( -- x addr ) 30 bit RTC_ALRMAR ; \ RTC_ALRMAR_WDSEL, Week day selection
: RTC_ALRMAR_DT ( %bb -- x addr ) 28 lshift RTC_ALRMAR ; \ RTC_ALRMAR_DT, Date tens in BCD format
: RTC_ALRMAR_DU ( %bbbb -- x addr ) 24 lshift RTC_ALRMAR ; \ RTC_ALRMAR_DU, Date units or day in BCD  format
: RTC_ALRMAR_MSK3 ( -- x addr ) 23 bit RTC_ALRMAR ; \ RTC_ALRMAR_MSK3, Alarm A hours mask
: RTC_ALRMAR_PM ( -- x addr ) 22 bit RTC_ALRMAR ; \ RTC_ALRMAR_PM, AM/PM notation
: RTC_ALRMAR_HT ( %bb -- x addr ) 20 lshift RTC_ALRMAR ; \ RTC_ALRMAR_HT, Hour tens in BCD format
: RTC_ALRMAR_HU ( %bbbb -- x addr ) 16 lshift RTC_ALRMAR ; \ RTC_ALRMAR_HU, Hour units in BCD format
: RTC_ALRMAR_MSK2 ( -- x addr ) 15 bit RTC_ALRMAR ; \ RTC_ALRMAR_MSK2, Alarm A minutes mask
: RTC_ALRMAR_MNT ( %bbb -- x addr ) 12 lshift RTC_ALRMAR ; \ RTC_ALRMAR_MNT, Minute tens in BCD format
: RTC_ALRMAR_MNU ( %bbbb -- x addr ) 8 lshift RTC_ALRMAR ; \ RTC_ALRMAR_MNU, Minute units in BCD format
: RTC_ALRMAR_MSK1 ( -- x addr ) 7 bit RTC_ALRMAR ; \ RTC_ALRMAR_MSK1, Alarm A seconds mask
: RTC_ALRMAR_ST ( %bbb -- x addr ) 4 lshift RTC_ALRMAR ; \ RTC_ALRMAR_ST, Second tens in BCD format
: RTC_ALRMAR_SU ( %bbbb -- x addr ) RTC_ALRMAR ; \ RTC_ALRMAR_SU, Second units in BCD format
[then]

execute-defined? use-RTC defined? RTC_ALRMBR_MSK4 not and [if]
\ RTC_ALRMBR (read-write) Reset:0x00000000
: RTC_ALRMBR_MSK4 ( -- x addr ) 31 bit RTC_ALRMBR ; \ RTC_ALRMBR_MSK4, Alarm B date mask
: RTC_ALRMBR_WDSEL ( -- x addr ) 30 bit RTC_ALRMBR ; \ RTC_ALRMBR_WDSEL, Week day selection
: RTC_ALRMBR_DT ( %bb -- x addr ) 28 lshift RTC_ALRMBR ; \ RTC_ALRMBR_DT, Date tens in BCD format
: RTC_ALRMBR_DU ( %bbbb -- x addr ) 24 lshift RTC_ALRMBR ; \ RTC_ALRMBR_DU, Date units or day in BCD  format
: RTC_ALRMBR_MSK3 ( -- x addr ) 23 bit RTC_ALRMBR ; \ RTC_ALRMBR_MSK3, Alarm B hours mask
: RTC_ALRMBR_PM ( -- x addr ) 22 bit RTC_ALRMBR ; \ RTC_ALRMBR_PM, AM/PM notation
: RTC_ALRMBR_HT ( %bb -- x addr ) 20 lshift RTC_ALRMBR ; \ RTC_ALRMBR_HT, Hour tens in BCD format
: RTC_ALRMBR_HU ( %bbbb -- x addr ) 16 lshift RTC_ALRMBR ; \ RTC_ALRMBR_HU, Hour units in BCD format
: RTC_ALRMBR_MSK2 ( -- x addr ) 15 bit RTC_ALRMBR ; \ RTC_ALRMBR_MSK2, Alarm B minutes mask
: RTC_ALRMBR_MNT ( %bbb -- x addr ) 12 lshift RTC_ALRMBR ; \ RTC_ALRMBR_MNT, Minute tens in BCD format
: RTC_ALRMBR_MNU ( %bbbb -- x addr ) 8 lshift RTC_ALRMBR ; \ RTC_ALRMBR_MNU, Minute units in BCD format
: RTC_ALRMBR_MSK1 ( -- x addr ) 7 bit RTC_ALRMBR ; \ RTC_ALRMBR_MSK1, Alarm B seconds mask
: RTC_ALRMBR_ST ( %bbb -- x addr ) 4 lshift RTC_ALRMBR ; \ RTC_ALRMBR_ST, Second tens in BCD format
: RTC_ALRMBR_SU ( %bbbb -- x addr ) RTC_ALRMBR ; \ RTC_ALRMBR_SU, Second units in BCD format
[then]

defined? use-RTC defined? RTC_WPR_KEY not and [if]
\ RTC_WPR (write-only) Reset:0x00000000
: RTC_WPR_KEY ( %bbbbbbbb -- x addr ) RTC_WPR ; \ RTC_WPR_KEY, Write protection key
[then]

execute-defined? use-RTC defined? RTC_SSR_SS? not and [if]
\ RTC_SSR (read-only) Reset:0x00000000
: RTC_SSR_SS? ( --  x ) RTC_SSR @ ; \ RTC_SSR_SS, Sub second value
[then]

defined? use-RTC defined? RTC_SHIFTR_ADD1S not and [if]
\ RTC_SHIFTR (write-only) Reset:0x00000000
: RTC_SHIFTR_ADD1S ( -- x addr ) 31 bit RTC_SHIFTR ; \ RTC_SHIFTR_ADD1S, Add one second
: RTC_SHIFTR_SUBFS ( %bbbbbbbbbbbbbbb -- x addr ) RTC_SHIFTR ; \ RTC_SHIFTR_SUBFS, Subtract a fraction of a  second
[then]

execute-defined? use-RTC defined? RTC_TSTR_SU? not and [if]
\ RTC_TSTR (read-only) Reset:0x00000000
: RTC_TSTR_SU? ( --  x ) RTC_TSTR @ ; \ RTC_TSTR_SU, Second units in BCD format
: RTC_TSTR_ST? ( --  x ) 4 lshift RTC_TSTR @ ; \ RTC_TSTR_ST, Second tens in BCD format
: RTC_TSTR_MNU? ( --  x ) 8 lshift RTC_TSTR @ ; \ RTC_TSTR_MNU, Minute units in BCD format
: RTC_TSTR_MNT? ( --  x ) 12 lshift RTC_TSTR @ ; \ RTC_TSTR_MNT, Minute tens in BCD format
: RTC_TSTR_HU? ( --  x ) 16 lshift RTC_TSTR @ ; \ RTC_TSTR_HU, Hour units in BCD format
: RTC_TSTR_HT? ( --  x ) 20 lshift RTC_TSTR @ ; \ RTC_TSTR_HT, Hour tens in BCD format
: RTC_TSTR_PM? ( --  1|0 ) 22 bit RTC_TSTR bit@ ; \ RTC_TSTR_PM, AM/PM notation
[then]

defined? use-RTC defined? RTC_TSDR_WDU? not and [if]
\ RTC_TSDR (read-only) Reset:0x00000000
: RTC_TSDR_WDU? ( --  x ) 13 lshift RTC_TSDR @ ; \ RTC_TSDR_WDU, Week day units
: RTC_TSDR_MT? ( --  1|0 ) 12 bit RTC_TSDR bit@ ; \ RTC_TSDR_MT, Month tens in BCD format
: RTC_TSDR_MU? ( --  x ) 8 lshift RTC_TSDR @ ; \ RTC_TSDR_MU, Month units in BCD format
: RTC_TSDR_DT? ( --  x ) 4 lshift RTC_TSDR @ ; \ RTC_TSDR_DT, Date tens in BCD format
: RTC_TSDR_DU? ( --  x ) RTC_TSDR @ ; \ RTC_TSDR_DU, Date units in BCD format
[then]

execute-defined? use-RTC defined? RTC_TSSSR_SS? not and [if]
\ RTC_TSSSR (read-only) Reset:0x00000000
: RTC_TSSSR_SS? ( --  x ) RTC_TSSSR @ ; \ RTC_TSSSR_SS, Sub second value
[then]

defined? use-RTC defined? RTC_CALR_CALP not and [if]
\ RTC_CALR (read-write) Reset:0x00000000
: RTC_CALR_CALP ( -- x addr ) 15 bit RTC_CALR ; \ RTC_CALR_CALP, Increase frequency of RTC by 488.5  ppm
: RTC_CALR_CALW8 ( -- x addr ) 14 bit RTC_CALR ; \ RTC_CALR_CALW8, Use an 8-second calibration cycle  period
: RTC_CALR_CALW16 ( -- x addr ) 13 bit RTC_CALR ; \ RTC_CALR_CALW16, Use a 16-second calibration cycle  period
: RTC_CALR_CALM ( %bbbbbbbbb -- x addr ) RTC_CALR ; \ RTC_CALR_CALM, Calibration minus
[then]

execute-defined? use-RTC defined? RTC_TAMPCR_TAMP1E not and [if]
\ RTC_TAMPCR (read-write) Reset:0x00000000
: RTC_TAMPCR_TAMP1E ( -- x addr ) 0 bit RTC_TAMPCR ; \ RTC_TAMPCR_TAMP1E, Tamper 1 detection enable
: RTC_TAMPCR_TAMP1TRG ( -- x addr ) 1 bit RTC_TAMPCR ; \ RTC_TAMPCR_TAMP1TRG, Active level for tamper 1
: RTC_TAMPCR_TAMPIE ( -- x addr ) 2 bit RTC_TAMPCR ; \ RTC_TAMPCR_TAMPIE, Tamper interrupt enable
: RTC_TAMPCR_TAMP2E ( -- x addr ) 3 bit RTC_TAMPCR ; \ RTC_TAMPCR_TAMP2E, Tamper 2 detection enable
: RTC_TAMPCR_TAMP2TRG ( -- x addr ) 4 bit RTC_TAMPCR ; \ RTC_TAMPCR_TAMP2TRG, Active level for tamper 2
: RTC_TAMPCR_TAMP3E ( -- x addr ) 5 bit RTC_TAMPCR ; \ RTC_TAMPCR_TAMP3E, Tamper 3 detection enable
: RTC_TAMPCR_TAMP3TRG ( -- x addr ) 6 bit RTC_TAMPCR ; \ RTC_TAMPCR_TAMP3TRG, Active level for tamper 3
: RTC_TAMPCR_TAMPTS ( -- x addr ) 7 bit RTC_TAMPCR ; \ RTC_TAMPCR_TAMPTS, Activate timestamp on tamper detection  event
: RTC_TAMPCR_TAMPFREQ ( %bbb -- x addr ) 8 lshift RTC_TAMPCR ; \ RTC_TAMPCR_TAMPFREQ, Tamper sampling frequency
: RTC_TAMPCR_TAMPFLT ( %bb -- x addr ) 11 lshift RTC_TAMPCR ; \ RTC_TAMPCR_TAMPFLT, Tamper filter count
: RTC_TAMPCR_TAMPPRCH ( %bb -- x addr ) 13 lshift RTC_TAMPCR ; \ RTC_TAMPCR_TAMPPRCH, Tamper precharge duration
: RTC_TAMPCR_TAMPPUDIS ( -- x addr ) 15 bit RTC_TAMPCR ; \ RTC_TAMPCR_TAMPPUDIS, TAMPER pull-up disable
: RTC_TAMPCR_TAMP1IE ( -- x addr ) 16 bit RTC_TAMPCR ; \ RTC_TAMPCR_TAMP1IE, Tamper 1 interrupt enable
: RTC_TAMPCR_TAMP1NOERASE ( -- x addr ) 17 bit RTC_TAMPCR ; \ RTC_TAMPCR_TAMP1NOERASE, Tamper 1 no erase
: RTC_TAMPCR_TAMP1MF ( -- x addr ) 18 bit RTC_TAMPCR ; \ RTC_TAMPCR_TAMP1MF, Tamper 1 mask flag
: RTC_TAMPCR_TAMP2IE ( -- x addr ) 19 bit RTC_TAMPCR ; \ RTC_TAMPCR_TAMP2IE, Tamper 2 interrupt enable
: RTC_TAMPCR_TAMP2NOERASE ( -- x addr ) 20 bit RTC_TAMPCR ; \ RTC_TAMPCR_TAMP2NOERASE, Tamper 2 no erase
: RTC_TAMPCR_TAMP2MF ( -- x addr ) 21 bit RTC_TAMPCR ; \ RTC_TAMPCR_TAMP2MF, Tamper 2 mask flag
: RTC_TAMPCR_TAMP3IE ( -- x addr ) 22 bit RTC_TAMPCR ; \ RTC_TAMPCR_TAMP3IE, Tamper 3 interrupt enable
: RTC_TAMPCR_TAMP3NOERASE ( -- x addr ) 23 bit RTC_TAMPCR ; \ RTC_TAMPCR_TAMP3NOERASE, Tamper 3 no erase
: RTC_TAMPCR_TAMP3MF ( -- x addr ) 24 bit RTC_TAMPCR ; \ RTC_TAMPCR_TAMP3MF, Tamper 3 mask flag
[then]

defined? use-RTC defined? RTC_ALRMASSR_MASKSS not and [if]
\ RTC_ALRMASSR (read-write) Reset:0x00000000
: RTC_ALRMASSR_MASKSS ( %bbbb -- x addr ) 24 lshift RTC_ALRMASSR ; \ RTC_ALRMASSR_MASKSS, Mask the most-significant bits starting  at this bit
: RTC_ALRMASSR_SS ( %bbbbbbbbbbbbbbb -- x addr ) RTC_ALRMASSR ; \ RTC_ALRMASSR_SS, Sub seconds value
[then]

execute-defined? use-RTC defined? RTC_ALRMBSSR_MASKSS not and [if]
\ RTC_ALRMBSSR (read-write) Reset:0x00000000
: RTC_ALRMBSSR_MASKSS ( %bbbb -- x addr ) 24 lshift RTC_ALRMBSSR ; \ RTC_ALRMBSSR_MASKSS, Mask the most-significant bits starting  at this bit
: RTC_ALRMBSSR_SS ( %bbbbbbbbbbbbbbb -- x addr ) RTC_ALRMBSSR ; \ RTC_ALRMBSSR_SS, Sub seconds value
[then]

defined? use-RTC defined? RTC_OR_RTC_ALARM_TYPE not and [if]
\ RTC_OR (read-write) Reset:0x00000000
: RTC_OR_RTC_ALARM_TYPE ( -- x addr ) 0 bit RTC_OR ; \ RTC_OR_RTC_ALARM_TYPE, RTC_ALARM on PC13 output  type
: RTC_OR_RTC_OUT_RMP ( -- x addr ) 1 bit RTC_OR ; \ RTC_OR_RTC_OUT_RMP, RTC_OUT remap
[then]

execute-defined? use-RTC defined? RTC_BKP0R_BKP not and [if]
\ RTC_BKP0R (read-write) Reset:0x00000000
: RTC_BKP0R_BKP ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) RTC_BKP0R ; \ RTC_BKP0R_BKP, BKP
[then]

defined? use-RTC defined? RTC_BKP1R_BKP not and [if]
\ RTC_BKP1R (read-write) Reset:0x00000000
: RTC_BKP1R_BKP ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) RTC_BKP1R ; \ RTC_BKP1R_BKP, BKP
[then]

execute-defined? use-RTC defined? RTC_BKP2R_BKP not and [if]
\ RTC_BKP2R (read-write) Reset:0x00000000
: RTC_BKP2R_BKP ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) RTC_BKP2R ; \ RTC_BKP2R_BKP, BKP
[then]

defined? use-RTC defined? RTC_BKP3R_BKP not and [if]
\ RTC_BKP3R (read-write) Reset:0x00000000
: RTC_BKP3R_BKP ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) RTC_BKP3R ; \ RTC_BKP3R_BKP, BKP
[then]

execute-defined? use-RTC defined? RTC_BKP4R_BKP not and [if]
\ RTC_BKP4R (read-write) Reset:0x00000000
: RTC_BKP4R_BKP ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) RTC_BKP4R ; \ RTC_BKP4R_BKP, BKP
[then]

defined? use-RTC defined? RTC_BKP5R_BKP not and [if]
\ RTC_BKP5R (read-write) Reset:0x00000000
: RTC_BKP5R_BKP ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) RTC_BKP5R ; \ RTC_BKP5R_BKP, BKP
[then]

execute-defined? use-RTC defined? RTC_BKP6R_BKP not and [if]
\ RTC_BKP6R (read-write) Reset:0x00000000
: RTC_BKP6R_BKP ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) RTC_BKP6R ; \ RTC_BKP6R_BKP, BKP
[then]

defined? use-RTC defined? RTC_BKP7R_BKP not and [if]
\ RTC_BKP7R (read-write) Reset:0x00000000
: RTC_BKP7R_BKP ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) RTC_BKP7R ; \ RTC_BKP7R_BKP, BKP
[then]

execute-defined? use-RTC defined? RTC_BKP8R_BKP not and [if]
\ RTC_BKP8R (read-write) Reset:0x00000000
: RTC_BKP8R_BKP ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) RTC_BKP8R ; \ RTC_BKP8R_BKP, BKP
[then]

defined? use-RTC defined? RTC_BKP9R_BKP not and [if]
\ RTC_BKP9R (read-write) Reset:0x00000000
: RTC_BKP9R_BKP ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) RTC_BKP9R ; \ RTC_BKP9R_BKP, BKP
[then]

execute-defined? use-RTC defined? RTC_BKP10R_BKP not and [if]
\ RTC_BKP10R (read-write) Reset:0x00000000
: RTC_BKP10R_BKP ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) RTC_BKP10R ; \ RTC_BKP10R_BKP, BKP
[then]

defined? use-RTC defined? RTC_BKP11R_BKP not and [if]
\ RTC_BKP11R (read-write) Reset:0x00000000
: RTC_BKP11R_BKP ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) RTC_BKP11R ; \ RTC_BKP11R_BKP, BKP
[then]

execute-defined? use-RTC defined? RTC_BKP12R_BKP not and [if]
\ RTC_BKP12R (read-write) Reset:0x00000000
: RTC_BKP12R_BKP ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) RTC_BKP12R ; \ RTC_BKP12R_BKP, BKP
[then]

defined? use-RTC defined? RTC_BKP13R_BKP not and [if]
\ RTC_BKP13R (read-write) Reset:0x00000000
: RTC_BKP13R_BKP ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) RTC_BKP13R ; \ RTC_BKP13R_BKP, BKP
[then]

execute-defined? use-RTC defined? RTC_BKP14R_BKP not and [if]
\ RTC_BKP14R (read-write) Reset:0x00000000
: RTC_BKP14R_BKP ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) RTC_BKP14R ; \ RTC_BKP14R_BKP, BKP
[then]

defined? use-RTC defined? RTC_BKP15R_BKP not and [if]
\ RTC_BKP15R (read-write) Reset:0x00000000
: RTC_BKP15R_BKP ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) RTC_BKP15R ; \ RTC_BKP15R_BKP, BKP
[then]

execute-defined? use-RTC defined? RTC_BKP16R_BKP not and [if]
\ RTC_BKP16R (read-write) Reset:0x00000000
: RTC_BKP16R_BKP ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) RTC_BKP16R ; \ RTC_BKP16R_BKP, BKP
[then]

defined? use-RTC defined? RTC_BKP17R_BKP not and [if]
\ RTC_BKP17R (read-write) Reset:0x00000000
: RTC_BKP17R_BKP ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) RTC_BKP17R ; \ RTC_BKP17R_BKP, BKP
[then]

execute-defined? use-RTC defined? RTC_BKP18R_BKP not and [if]
\ RTC_BKP18R (read-write) Reset:0x00000000
: RTC_BKP18R_BKP ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) RTC_BKP18R ; \ RTC_BKP18R_BKP, BKP
[then]

defined? use-RTC defined? RTC_BKP19R_BKP not and [if]
\ RTC_BKP19R (read-write) Reset:0x00000000
: RTC_BKP19R_BKP ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) RTC_BKP19R ; \ RTC_BKP19R_BKP, BKP
[then]

execute-defined? use-RTC defined? RTC_BKP20R_BKP not and [if]
\ RTC_BKP20R (read-write) Reset:0x00000000
: RTC_BKP20R_BKP ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) RTC_BKP20R ; \ RTC_BKP20R_BKP, BKP
[then]

defined? use-RTC defined? RTC_BKP21R_BKP not and [if]
\ RTC_BKP21R (read-write) Reset:0x00000000
: RTC_BKP21R_BKP ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) RTC_BKP21R ; \ RTC_BKP21R_BKP, BKP
[then]

execute-defined? use-RTC defined? RTC_BKP22R_BKP not and [if]
\ RTC_BKP22R (read-write) Reset:0x00000000
: RTC_BKP22R_BKP ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) RTC_BKP22R ; \ RTC_BKP22R_BKP, BKP
[then]

defined? use-RTC defined? RTC_BKP23R_BKP not and [if]
\ RTC_BKP23R (read-write) Reset:0x00000000
: RTC_BKP23R_BKP ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) RTC_BKP23R ; \ RTC_BKP23R_BKP, BKP
[then]

execute-defined? use-RTC defined? RTC_BKP24R_BKP not and [if]
\ RTC_BKP24R (read-write) Reset:0x00000000
: RTC_BKP24R_BKP ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) RTC_BKP24R ; \ RTC_BKP24R_BKP, BKP
[then]

defined? use-RTC defined? RTC_BKP25R_BKP not and [if]
\ RTC_BKP25R (read-write) Reset:0x00000000
: RTC_BKP25R_BKP ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) RTC_BKP25R ; \ RTC_BKP25R_BKP, BKP
[then]

execute-defined? use-RTC defined? RTC_BKP26R_BKP not and [if]
\ RTC_BKP26R (read-write) Reset:0x00000000
: RTC_BKP26R_BKP ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) RTC_BKP26R ; \ RTC_BKP26R_BKP, BKP
[then]

defined? use-RTC defined? RTC_BKP27R_BKP not and [if]
\ RTC_BKP27R (read-write) Reset:0x00000000
: RTC_BKP27R_BKP ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) RTC_BKP27R ; \ RTC_BKP27R_BKP, BKP
[then]

execute-defined? use-RTC defined? RTC_BKP28R_BKP not and [if]
\ RTC_BKP28R (read-write) Reset:0x00000000
: RTC_BKP28R_BKP ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) RTC_BKP28R ; \ RTC_BKP28R_BKP, BKP
[then]

defined? use-RTC defined? RTC_BKP29R_BKP not and [if]
\ RTC_BKP29R (read-write) Reset:0x00000000
: RTC_BKP29R_BKP ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) RTC_BKP29R ; \ RTC_BKP29R_BKP, BKP
[then]

execute-defined? use-RTC defined? RTC_BKP30R_BKP not and [if]
\ RTC_BKP30R (read-write) Reset:0x00000000
: RTC_BKP30R_BKP ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) RTC_BKP30R ; \ RTC_BKP30R_BKP, BKP
[then]

defined? use-RTC defined? RTC_BKP31R_BKP not and [if]
\ RTC_BKP31R (read-write) Reset:0x00000000
: RTC_BKP31R_BKP ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) RTC_BKP31R ; \ RTC_BKP31R_BKP, BKP
[then]

execute-defined? use-OTG_FS_GLOBAL defined? OTG_FS_GLOBAL_FS_GOTGCTL_SRQSCS not and [if]
\ OTG_FS_GLOBAL_FS_GOTGCTL (multiple-access)  Reset:0x00000800
: OTG_FS_GLOBAL_FS_GOTGCTL_SRQSCS ( -- x addr ) 0 bit OTG_FS_GLOBAL_FS_GOTGCTL ; \ OTG_FS_GLOBAL_FS_GOTGCTL_SRQSCS, Session request success
: OTG_FS_GLOBAL_FS_GOTGCTL_SRQ ( -- x addr ) 1 bit OTG_FS_GLOBAL_FS_GOTGCTL ; \ OTG_FS_GLOBAL_FS_GOTGCTL_SRQ, Session request
: OTG_FS_GLOBAL_FS_GOTGCTL_HNGSCS ( -- x addr ) 8 bit OTG_FS_GLOBAL_FS_GOTGCTL ; \ OTG_FS_GLOBAL_FS_GOTGCTL_HNGSCS, Host negotiation success
: OTG_FS_GLOBAL_FS_GOTGCTL_HNPRQ ( -- x addr ) 9 bit OTG_FS_GLOBAL_FS_GOTGCTL ; \ OTG_FS_GLOBAL_FS_GOTGCTL_HNPRQ, HNP request
: OTG_FS_GLOBAL_FS_GOTGCTL_HSHNPEN ( -- x addr ) 10 bit OTG_FS_GLOBAL_FS_GOTGCTL ; \ OTG_FS_GLOBAL_FS_GOTGCTL_HSHNPEN, Host set HNP enable
: OTG_FS_GLOBAL_FS_GOTGCTL_DHNPEN ( -- x addr ) 11 bit OTG_FS_GLOBAL_FS_GOTGCTL ; \ OTG_FS_GLOBAL_FS_GOTGCTL_DHNPEN, Device HNP enabled
: OTG_FS_GLOBAL_FS_GOTGCTL_CIDSTS? ( -- 1|0 ) 16 bit OTG_FS_GLOBAL_FS_GOTGCTL bit@ ; \ OTG_FS_GLOBAL_FS_GOTGCTL_CIDSTS, Connector ID status
: OTG_FS_GLOBAL_FS_GOTGCTL_DBCT ( -- x addr ) 17 bit OTG_FS_GLOBAL_FS_GOTGCTL ; \ OTG_FS_GLOBAL_FS_GOTGCTL_DBCT, Long/short debounce time
: OTG_FS_GLOBAL_FS_GOTGCTL_ASVLD ( -- x addr ) 18 bit OTG_FS_GLOBAL_FS_GOTGCTL ; \ OTG_FS_GLOBAL_FS_GOTGCTL_ASVLD, A-session valid
: OTG_FS_GLOBAL_FS_GOTGCTL_BSVLD ( -- x addr ) 19 bit OTG_FS_GLOBAL_FS_GOTGCTL ; \ OTG_FS_GLOBAL_FS_GOTGCTL_BSVLD, B-session valid
[then]

defined? use-OTG_FS_GLOBAL defined? OTG_FS_GLOBAL_FS_GOTGINT_SEDET not and [if]
\ OTG_FS_GLOBAL_FS_GOTGINT (read-write) Reset:0x00000000
: OTG_FS_GLOBAL_FS_GOTGINT_SEDET ( -- x addr ) 2 bit OTG_FS_GLOBAL_FS_GOTGINT ; \ OTG_FS_GLOBAL_FS_GOTGINT_SEDET, Session end detected
: OTG_FS_GLOBAL_FS_GOTGINT_SRSSCHG ( -- x addr ) 8 bit OTG_FS_GLOBAL_FS_GOTGINT ; \ OTG_FS_GLOBAL_FS_GOTGINT_SRSSCHG, Session request success status  change
: OTG_FS_GLOBAL_FS_GOTGINT_HNSSCHG ( -- x addr ) 9 bit OTG_FS_GLOBAL_FS_GOTGINT ; \ OTG_FS_GLOBAL_FS_GOTGINT_HNSSCHG, Host negotiation success status  change
: OTG_FS_GLOBAL_FS_GOTGINT_HNGDET ( -- x addr ) 17 bit OTG_FS_GLOBAL_FS_GOTGINT ; \ OTG_FS_GLOBAL_FS_GOTGINT_HNGDET, Host negotiation detected
: OTG_FS_GLOBAL_FS_GOTGINT_ADTOCHG ( -- x addr ) 18 bit OTG_FS_GLOBAL_FS_GOTGINT ; \ OTG_FS_GLOBAL_FS_GOTGINT_ADTOCHG, A-device timeout change
: OTG_FS_GLOBAL_FS_GOTGINT_DBCDNE ( -- x addr ) 19 bit OTG_FS_GLOBAL_FS_GOTGINT ; \ OTG_FS_GLOBAL_FS_GOTGINT_DBCDNE, Debounce done
[then]

execute-defined? use-OTG_FS_GLOBAL defined? OTG_FS_GLOBAL_FS_GAHBCFG_GINT not and [if]
\ OTG_FS_GLOBAL_FS_GAHBCFG (read-write) Reset:0x00000000
: OTG_FS_GLOBAL_FS_GAHBCFG_GINT ( -- x addr ) 0 bit OTG_FS_GLOBAL_FS_GAHBCFG ; \ OTG_FS_GLOBAL_FS_GAHBCFG_GINT, Global interrupt mask
: OTG_FS_GLOBAL_FS_GAHBCFG_TXFELVL ( -- x addr ) 7 bit OTG_FS_GLOBAL_FS_GAHBCFG ; \ OTG_FS_GLOBAL_FS_GAHBCFG_TXFELVL, TxFIFO empty level
: OTG_FS_GLOBAL_FS_GAHBCFG_PTXFELVL ( -- x addr ) 8 bit OTG_FS_GLOBAL_FS_GAHBCFG ; \ OTG_FS_GLOBAL_FS_GAHBCFG_PTXFELVL, Periodic TxFIFO empty  level
[then]

defined? use-OTG_FS_GLOBAL defined? OTG_FS_GLOBAL_FS_GUSBCFG_TOCAL not and [if]
\ OTG_FS_GLOBAL_FS_GUSBCFG (multiple-access)  Reset:0x00000A00
: OTG_FS_GLOBAL_FS_GUSBCFG_TOCAL ( %bbb -- x addr ) OTG_FS_GLOBAL_FS_GUSBCFG ; \ OTG_FS_GLOBAL_FS_GUSBCFG_TOCAL, FS timeout calibration
: OTG_FS_GLOBAL_FS_GUSBCFG_PHYSEL ( -- x addr ) 6 bit OTG_FS_GLOBAL_FS_GUSBCFG ; \ OTG_FS_GLOBAL_FS_GUSBCFG_PHYSEL, Full Speed serial transceiver  select
: OTG_FS_GLOBAL_FS_GUSBCFG_SRPCAP ( -- x addr ) 8 bit OTG_FS_GLOBAL_FS_GUSBCFG ; \ OTG_FS_GLOBAL_FS_GUSBCFG_SRPCAP, SRP-capable
: OTG_FS_GLOBAL_FS_GUSBCFG_HNPCAP ( -- x addr ) 9 bit OTG_FS_GLOBAL_FS_GUSBCFG ; \ OTG_FS_GLOBAL_FS_GUSBCFG_HNPCAP, HNP-capable
: OTG_FS_GLOBAL_FS_GUSBCFG_TRDT ( %bbbb -- x addr ) 10 lshift OTG_FS_GLOBAL_FS_GUSBCFG ; \ OTG_FS_GLOBAL_FS_GUSBCFG_TRDT, USB turnaround time
: OTG_FS_GLOBAL_FS_GUSBCFG_FHMOD ( -- x addr ) 29 bit OTG_FS_GLOBAL_FS_GUSBCFG ; \ OTG_FS_GLOBAL_FS_GUSBCFG_FHMOD, Force host mode
: OTG_FS_GLOBAL_FS_GUSBCFG_FDMOD ( -- x addr ) 30 bit OTG_FS_GLOBAL_FS_GUSBCFG ; \ OTG_FS_GLOBAL_FS_GUSBCFG_FDMOD, Force device mode
: OTG_FS_GLOBAL_FS_GUSBCFG_CTXPKT ( -- x addr ) 31 bit OTG_FS_GLOBAL_FS_GUSBCFG ; \ OTG_FS_GLOBAL_FS_GUSBCFG_CTXPKT, Corrupt Tx packet
[then]

execute-defined? use-OTG_FS_GLOBAL defined? OTG_FS_GLOBAL_FS_GRSTCTL_CSRST not and [if]
\ OTG_FS_GLOBAL_FS_GRSTCTL (multiple-access)  Reset:0x20000000
: OTG_FS_GLOBAL_FS_GRSTCTL_CSRST ( -- x addr ) 0 bit OTG_FS_GLOBAL_FS_GRSTCTL ; \ OTG_FS_GLOBAL_FS_GRSTCTL_CSRST, Core soft reset
: OTG_FS_GLOBAL_FS_GRSTCTL_HSRST ( -- x addr ) 1 bit OTG_FS_GLOBAL_FS_GRSTCTL ; \ OTG_FS_GLOBAL_FS_GRSTCTL_HSRST, HCLK soft reset
: OTG_FS_GLOBAL_FS_GRSTCTL_FCRST ( -- x addr ) 2 bit OTG_FS_GLOBAL_FS_GRSTCTL ; \ OTG_FS_GLOBAL_FS_GRSTCTL_FCRST, Host frame counter reset
: OTG_FS_GLOBAL_FS_GRSTCTL_RXFFLSH ( -- x addr ) 4 bit OTG_FS_GLOBAL_FS_GRSTCTL ; \ OTG_FS_GLOBAL_FS_GRSTCTL_RXFFLSH, RxFIFO flush
: OTG_FS_GLOBAL_FS_GRSTCTL_TXFFLSH ( -- x addr ) 5 bit OTG_FS_GLOBAL_FS_GRSTCTL ; \ OTG_FS_GLOBAL_FS_GRSTCTL_TXFFLSH, TxFIFO flush
: OTG_FS_GLOBAL_FS_GRSTCTL_TXFNUM ( %bbbbb -- x addr ) 6 lshift OTG_FS_GLOBAL_FS_GRSTCTL ; \ OTG_FS_GLOBAL_FS_GRSTCTL_TXFNUM, TxFIFO number
: OTG_FS_GLOBAL_FS_GRSTCTL_AHBIDL ( -- x addr ) 31 bit OTG_FS_GLOBAL_FS_GRSTCTL ; \ OTG_FS_GLOBAL_FS_GRSTCTL_AHBIDL, AHB master idle
[then]

defined? use-OTG_FS_GLOBAL defined? OTG_FS_GLOBAL_FS_GINTSTS_CMOD not and [if]
\ OTG_FS_GLOBAL_FS_GINTSTS (multiple-access)  Reset:0x04000020
: OTG_FS_GLOBAL_FS_GINTSTS_CMOD ( -- x addr ) 0 bit OTG_FS_GLOBAL_FS_GINTSTS ; \ OTG_FS_GLOBAL_FS_GINTSTS_CMOD, Current mode of operation
: OTG_FS_GLOBAL_FS_GINTSTS_MMIS ( -- x addr ) 1 bit OTG_FS_GLOBAL_FS_GINTSTS ; \ OTG_FS_GLOBAL_FS_GINTSTS_MMIS, Mode mismatch interrupt
: OTG_FS_GLOBAL_FS_GINTSTS_OTGINT ( -- x addr ) 2 bit OTG_FS_GLOBAL_FS_GINTSTS ; \ OTG_FS_GLOBAL_FS_GINTSTS_OTGINT, OTG interrupt
: OTG_FS_GLOBAL_FS_GINTSTS_SOF ( -- x addr ) 3 bit OTG_FS_GLOBAL_FS_GINTSTS ; \ OTG_FS_GLOBAL_FS_GINTSTS_SOF, Start of frame
: OTG_FS_GLOBAL_FS_GINTSTS_RXFLVL ( -- x addr ) 4 bit OTG_FS_GLOBAL_FS_GINTSTS ; \ OTG_FS_GLOBAL_FS_GINTSTS_RXFLVL, RxFIFO non-empty
: OTG_FS_GLOBAL_FS_GINTSTS_NPTXFE ( -- x addr ) 5 bit OTG_FS_GLOBAL_FS_GINTSTS ; \ OTG_FS_GLOBAL_FS_GINTSTS_NPTXFE, Non-periodic TxFIFO empty
: OTG_FS_GLOBAL_FS_GINTSTS_GINAKEFF ( -- x addr ) 6 bit OTG_FS_GLOBAL_FS_GINTSTS ; \ OTG_FS_GLOBAL_FS_GINTSTS_GINAKEFF, Global IN non-periodic NAK  effective
: OTG_FS_GLOBAL_FS_GINTSTS_GOUTNAKEFF ( -- x addr ) 7 bit OTG_FS_GLOBAL_FS_GINTSTS ; \ OTG_FS_GLOBAL_FS_GINTSTS_GOUTNAKEFF, Global OUT NAK effective
: OTG_FS_GLOBAL_FS_GINTSTS_ESUSP ( -- x addr ) 10 bit OTG_FS_GLOBAL_FS_GINTSTS ; \ OTG_FS_GLOBAL_FS_GINTSTS_ESUSP, Early suspend
: OTG_FS_GLOBAL_FS_GINTSTS_USBSUSP ( -- x addr ) 11 bit OTG_FS_GLOBAL_FS_GINTSTS ; \ OTG_FS_GLOBAL_FS_GINTSTS_USBSUSP, USB suspend
: OTG_FS_GLOBAL_FS_GINTSTS_USBRST ( -- x addr ) 12 bit OTG_FS_GLOBAL_FS_GINTSTS ; \ OTG_FS_GLOBAL_FS_GINTSTS_USBRST, USB reset
: OTG_FS_GLOBAL_FS_GINTSTS_ENUMDNE ( -- x addr ) 13 bit OTG_FS_GLOBAL_FS_GINTSTS ; \ OTG_FS_GLOBAL_FS_GINTSTS_ENUMDNE, Enumeration done
: OTG_FS_GLOBAL_FS_GINTSTS_ISOODRP ( -- x addr ) 14 bit OTG_FS_GLOBAL_FS_GINTSTS ; \ OTG_FS_GLOBAL_FS_GINTSTS_ISOODRP, Isochronous OUT packet dropped  interrupt
: OTG_FS_GLOBAL_FS_GINTSTS_EOPF ( -- x addr ) 15 bit OTG_FS_GLOBAL_FS_GINTSTS ; \ OTG_FS_GLOBAL_FS_GINTSTS_EOPF, End of periodic frame  interrupt
: OTG_FS_GLOBAL_FS_GINTSTS_IEPINT ( -- x addr ) 18 bit OTG_FS_GLOBAL_FS_GINTSTS ; \ OTG_FS_GLOBAL_FS_GINTSTS_IEPINT, IN endpoint interrupt
: OTG_FS_GLOBAL_FS_GINTSTS_OEPINT ( -- x addr ) 19 bit OTG_FS_GLOBAL_FS_GINTSTS ; \ OTG_FS_GLOBAL_FS_GINTSTS_OEPINT, OUT endpoint interrupt
: OTG_FS_GLOBAL_FS_GINTSTS_IISOIXFR ( -- x addr ) 20 bit OTG_FS_GLOBAL_FS_GINTSTS ; \ OTG_FS_GLOBAL_FS_GINTSTS_IISOIXFR, Incomplete isochronous IN  transfer
: OTG_FS_GLOBAL_FS_GINTSTS_IPXFR_INCOMPISOOUT ( -- x addr ) 21 bit OTG_FS_GLOBAL_FS_GINTSTS ; \ OTG_FS_GLOBAL_FS_GINTSTS_IPXFR_INCOMPISOOUT, Incomplete periodic transferHost  mode/Incomplete isochronous OUT transferDevice  mode
: OTG_FS_GLOBAL_FS_GINTSTS_HPRTINT ( -- x addr ) 24 bit OTG_FS_GLOBAL_FS_GINTSTS ; \ OTG_FS_GLOBAL_FS_GINTSTS_HPRTINT, Host port interrupt
: OTG_FS_GLOBAL_FS_GINTSTS_HCINT ( -- x addr ) 25 bit OTG_FS_GLOBAL_FS_GINTSTS ; \ OTG_FS_GLOBAL_FS_GINTSTS_HCINT, Host channels interrupt
: OTG_FS_GLOBAL_FS_GINTSTS_PTXFE ( -- x addr ) 26 bit OTG_FS_GLOBAL_FS_GINTSTS ; \ OTG_FS_GLOBAL_FS_GINTSTS_PTXFE, Periodic TxFIFO empty
: OTG_FS_GLOBAL_FS_GINTSTS_CIDSCHG? ( -- 1|0 ) 28 bit OTG_FS_GLOBAL_FS_GINTSTS bit@ ; \ OTG_FS_GLOBAL_FS_GINTSTS_CIDSCHG, Connector ID status change
: OTG_FS_GLOBAL_FS_GINTSTS_DISCINT ( -- x addr ) 29 bit OTG_FS_GLOBAL_FS_GINTSTS ; \ OTG_FS_GLOBAL_FS_GINTSTS_DISCINT, Disconnect detected  interrupt
: OTG_FS_GLOBAL_FS_GINTSTS_SRQINT ( -- x addr ) 30 bit OTG_FS_GLOBAL_FS_GINTSTS ; \ OTG_FS_GLOBAL_FS_GINTSTS_SRQINT, Session request/new session detected  interrupt
: OTG_FS_GLOBAL_FS_GINTSTS_WKUPINT ( -- x addr ) 31 bit OTG_FS_GLOBAL_FS_GINTSTS ; \ OTG_FS_GLOBAL_FS_GINTSTS_WKUPINT, Resume/remote wakeup detected  interrupt
[then]

execute-defined? use-OTG_FS_GLOBAL defined? OTG_FS_GLOBAL_FS_GINTMSK_MMISM not and [if]
\ OTG_FS_GLOBAL_FS_GINTMSK (multiple-access)  Reset:0x00000000
: OTG_FS_GLOBAL_FS_GINTMSK_MMISM ( -- x addr ) 1 bit OTG_FS_GLOBAL_FS_GINTMSK ; \ OTG_FS_GLOBAL_FS_GINTMSK_MMISM, Mode mismatch interrupt  mask
: OTG_FS_GLOBAL_FS_GINTMSK_OTGINT ( -- x addr ) 2 bit OTG_FS_GLOBAL_FS_GINTMSK ; \ OTG_FS_GLOBAL_FS_GINTMSK_OTGINT, OTG interrupt mask
: OTG_FS_GLOBAL_FS_GINTMSK_SOFM ( -- x addr ) 3 bit OTG_FS_GLOBAL_FS_GINTMSK ; \ OTG_FS_GLOBAL_FS_GINTMSK_SOFM, Start of frame mask
: OTG_FS_GLOBAL_FS_GINTMSK_RXFLVLM ( -- x addr ) 4 bit OTG_FS_GLOBAL_FS_GINTMSK ; \ OTG_FS_GLOBAL_FS_GINTMSK_RXFLVLM, Receive FIFO non-empty  mask
: OTG_FS_GLOBAL_FS_GINTMSK_NPTXFEM ( -- x addr ) 5 bit OTG_FS_GLOBAL_FS_GINTMSK ; \ OTG_FS_GLOBAL_FS_GINTMSK_NPTXFEM, Non-periodic TxFIFO empty  mask
: OTG_FS_GLOBAL_FS_GINTMSK_GINAKEFFM ( -- x addr ) 6 bit OTG_FS_GLOBAL_FS_GINTMSK ; \ OTG_FS_GLOBAL_FS_GINTMSK_GINAKEFFM, Global non-periodic IN NAK effective  mask
: OTG_FS_GLOBAL_FS_GINTMSK_GONAKEFFM ( -- x addr ) 7 bit OTG_FS_GLOBAL_FS_GINTMSK ; \ OTG_FS_GLOBAL_FS_GINTMSK_GONAKEFFM, Global OUT NAK effective  mask
: OTG_FS_GLOBAL_FS_GINTMSK_ESUSPM ( -- x addr ) 10 bit OTG_FS_GLOBAL_FS_GINTMSK ; \ OTG_FS_GLOBAL_FS_GINTMSK_ESUSPM, Early suspend mask
: OTG_FS_GLOBAL_FS_GINTMSK_USBSUSPM ( -- x addr ) 11 bit OTG_FS_GLOBAL_FS_GINTMSK ; \ OTG_FS_GLOBAL_FS_GINTMSK_USBSUSPM, USB suspend mask
: OTG_FS_GLOBAL_FS_GINTMSK_USBRST ( -- x addr ) 12 bit OTG_FS_GLOBAL_FS_GINTMSK ; \ OTG_FS_GLOBAL_FS_GINTMSK_USBRST, USB reset mask
: OTG_FS_GLOBAL_FS_GINTMSK_ENUMDNEM ( -- x addr ) 13 bit OTG_FS_GLOBAL_FS_GINTMSK ; \ OTG_FS_GLOBAL_FS_GINTMSK_ENUMDNEM, Enumeration done mask
: OTG_FS_GLOBAL_FS_GINTMSK_ISOODRPM ( -- x addr ) 14 bit OTG_FS_GLOBAL_FS_GINTMSK ; \ OTG_FS_GLOBAL_FS_GINTMSK_ISOODRPM, Isochronous OUT packet dropped interrupt  mask
: OTG_FS_GLOBAL_FS_GINTMSK_EOPFM ( -- x addr ) 15 bit OTG_FS_GLOBAL_FS_GINTMSK ; \ OTG_FS_GLOBAL_FS_GINTMSK_EOPFM, End of periodic frame interrupt  mask
: OTG_FS_GLOBAL_FS_GINTMSK_EPMISM ( -- x addr ) 17 bit OTG_FS_GLOBAL_FS_GINTMSK ; \ OTG_FS_GLOBAL_FS_GINTMSK_EPMISM, Endpoint mismatch interrupt  mask
: OTG_FS_GLOBAL_FS_GINTMSK_IEPINT ( -- x addr ) 18 bit OTG_FS_GLOBAL_FS_GINTMSK ; \ OTG_FS_GLOBAL_FS_GINTMSK_IEPINT, IN endpoints interrupt  mask
: OTG_FS_GLOBAL_FS_GINTMSK_OEPINT ( -- x addr ) 19 bit OTG_FS_GLOBAL_FS_GINTMSK ; \ OTG_FS_GLOBAL_FS_GINTMSK_OEPINT, OUT endpoints interrupt  mask
: OTG_FS_GLOBAL_FS_GINTMSK_IISOIXFRM ( -- x addr ) 20 bit OTG_FS_GLOBAL_FS_GINTMSK ; \ OTG_FS_GLOBAL_FS_GINTMSK_IISOIXFRM, Incomplete isochronous IN transfer  mask
: OTG_FS_GLOBAL_FS_GINTMSK_IPXFRM_IISOOXFRM ( -- x addr ) 21 bit OTG_FS_GLOBAL_FS_GINTMSK ; \ OTG_FS_GLOBAL_FS_GINTMSK_IPXFRM_IISOOXFRM, Incomplete periodic transfer maskHost  mode/Incomplete isochronous OUT transfer maskDevice  mode
: OTG_FS_GLOBAL_FS_GINTMSK_PRTIM ( -- x addr ) 24 bit OTG_FS_GLOBAL_FS_GINTMSK ; \ OTG_FS_GLOBAL_FS_GINTMSK_PRTIM, Host port interrupt mask
: OTG_FS_GLOBAL_FS_GINTMSK_HCIM ( -- x addr ) 25 bit OTG_FS_GLOBAL_FS_GINTMSK ; \ OTG_FS_GLOBAL_FS_GINTMSK_HCIM, Host channels interrupt  mask
: OTG_FS_GLOBAL_FS_GINTMSK_PTXFEM ( -- x addr ) 26 bit OTG_FS_GLOBAL_FS_GINTMSK ; \ OTG_FS_GLOBAL_FS_GINTMSK_PTXFEM, Periodic TxFIFO empty mask
: OTG_FS_GLOBAL_FS_GINTMSK_CIDSCHGM? ( -- 1|0 ) 28 bit OTG_FS_GLOBAL_FS_GINTMSK bit@ ; \ OTG_FS_GLOBAL_FS_GINTMSK_CIDSCHGM, Connector ID status change  mask
: OTG_FS_GLOBAL_FS_GINTMSK_DISCINT ( -- x addr ) 29 bit OTG_FS_GLOBAL_FS_GINTMSK ; \ OTG_FS_GLOBAL_FS_GINTMSK_DISCINT, Disconnect detected interrupt  mask
: OTG_FS_GLOBAL_FS_GINTMSK_SRQIM ( -- x addr ) 30 bit OTG_FS_GLOBAL_FS_GINTMSK ; \ OTG_FS_GLOBAL_FS_GINTMSK_SRQIM, Session request/new session detected  interrupt mask
: OTG_FS_GLOBAL_FS_GINTMSK_WUIM ( -- x addr ) 31 bit OTG_FS_GLOBAL_FS_GINTMSK ; \ OTG_FS_GLOBAL_FS_GINTMSK_WUIM, Resume/remote wakeup detected interrupt  mask
[then]

defined? use-OTG_FS_GLOBAL defined? OTG_FS_GLOBAL_FS_GRXSTSR_Device_EPNUM? not and [if]
\ OTG_FS_GLOBAL_FS_GRXSTSR_Device (read-only) Reset:0x00000000
: OTG_FS_GLOBAL_FS_GRXSTSR_Device_EPNUM? ( --  x ) OTG_FS_GLOBAL_FS_GRXSTSR_Device @ ; \ OTG_FS_GLOBAL_FS_GRXSTSR_Device_EPNUM, Endpoint number
: OTG_FS_GLOBAL_FS_GRXSTSR_Device_BCNT? ( --  x ) 4 lshift OTG_FS_GLOBAL_FS_GRXSTSR_Device @ ; \ OTG_FS_GLOBAL_FS_GRXSTSR_Device_BCNT, Byte count
: OTG_FS_GLOBAL_FS_GRXSTSR_Device_DPID? ( --  x ) 15 lshift OTG_FS_GLOBAL_FS_GRXSTSR_Device @ ; \ OTG_FS_GLOBAL_FS_GRXSTSR_Device_DPID, Data PID
: OTG_FS_GLOBAL_FS_GRXSTSR_Device_PKTSTS? ( --  x ) 17 lshift OTG_FS_GLOBAL_FS_GRXSTSR_Device @ ; \ OTG_FS_GLOBAL_FS_GRXSTSR_Device_PKTSTS, Packet status
: OTG_FS_GLOBAL_FS_GRXSTSR_Device_FRMNUM? ( --  x ) 21 lshift OTG_FS_GLOBAL_FS_GRXSTSR_Device @ ; \ OTG_FS_GLOBAL_FS_GRXSTSR_Device_FRMNUM, Frame number
[then]

execute-defined? use-OTG_FS_GLOBAL defined? OTG_FS_GLOBAL_FS_GRXSTSR_Host_EPNUM? not and [if]
\ OTG_FS_GLOBAL_FS_GRXSTSR_Host (read-only) Reset:0x00000000
: OTG_FS_GLOBAL_FS_GRXSTSR_Host_EPNUM? ( --  x ) OTG_FS_GLOBAL_FS_GRXSTSR_Host @ ; \ OTG_FS_GLOBAL_FS_GRXSTSR_Host_EPNUM, Endpoint number
: OTG_FS_GLOBAL_FS_GRXSTSR_Host_BCNT? ( --  x ) 4 lshift OTG_FS_GLOBAL_FS_GRXSTSR_Host @ ; \ OTG_FS_GLOBAL_FS_GRXSTSR_Host_BCNT, Byte count
: OTG_FS_GLOBAL_FS_GRXSTSR_Host_DPID? ( --  x ) 15 lshift OTG_FS_GLOBAL_FS_GRXSTSR_Host @ ; \ OTG_FS_GLOBAL_FS_GRXSTSR_Host_DPID, Data PID
: OTG_FS_GLOBAL_FS_GRXSTSR_Host_PKTSTS? ( --  x ) 17 lshift OTG_FS_GLOBAL_FS_GRXSTSR_Host @ ; \ OTG_FS_GLOBAL_FS_GRXSTSR_Host_PKTSTS, Packet status
: OTG_FS_GLOBAL_FS_GRXSTSR_Host_FRMNUM? ( --  x ) 21 lshift OTG_FS_GLOBAL_FS_GRXSTSR_Host @ ; \ OTG_FS_GLOBAL_FS_GRXSTSR_Host_FRMNUM, Frame number
[then]

defined? use-OTG_FS_GLOBAL defined? OTG_FS_GLOBAL_FS_GRXFSIZ_RXFD not and [if]
\ OTG_FS_GLOBAL_FS_GRXFSIZ (read-write) Reset:0x00000200
: OTG_FS_GLOBAL_FS_GRXFSIZ_RXFD ( %bbbbbbbbbbbbbbbb -- x addr ) OTG_FS_GLOBAL_FS_GRXFSIZ ; \ OTG_FS_GLOBAL_FS_GRXFSIZ_RXFD, RxFIFO depth
[then]

execute-defined? use-OTG_FS_GLOBAL defined? OTG_FS_GLOBAL_FS_GNPTXFSIZ_Device_TX0FSA not and [if]
\ OTG_FS_GLOBAL_FS_GNPTXFSIZ_Device (read-write) Reset:0x00000200
: OTG_FS_GLOBAL_FS_GNPTXFSIZ_Device_TX0FSA ( %bbbbbbbbbbbbbbbb -- x addr ) OTG_FS_GLOBAL_FS_GNPTXFSIZ_Device ; \ OTG_FS_GLOBAL_FS_GNPTXFSIZ_Device_TX0FSA, Endpoint 0 transmit RAM start  address
: OTG_FS_GLOBAL_FS_GNPTXFSIZ_Device_TX0FD ( %bbbbbbbbbbbbbbbb -- x addr ) 16 lshift OTG_FS_GLOBAL_FS_GNPTXFSIZ_Device ; \ OTG_FS_GLOBAL_FS_GNPTXFSIZ_Device_TX0FD, Endpoint 0 TxFIFO depth
[then]

defined? use-OTG_FS_GLOBAL defined? OTG_FS_GLOBAL_FS_GNPTXFSIZ_Host_NPTXFSA not and [if]
\ OTG_FS_GLOBAL_FS_GNPTXFSIZ_Host (read-write) Reset:0x00000200
: OTG_FS_GLOBAL_FS_GNPTXFSIZ_Host_NPTXFSA ( %bbbbbbbbbbbbbbbb -- x addr ) OTG_FS_GLOBAL_FS_GNPTXFSIZ_Host ; \ OTG_FS_GLOBAL_FS_GNPTXFSIZ_Host_NPTXFSA, Non-periodic transmit RAM start  address
: OTG_FS_GLOBAL_FS_GNPTXFSIZ_Host_NPTXFD ( %bbbbbbbbbbbbbbbb -- x addr ) 16 lshift OTG_FS_GLOBAL_FS_GNPTXFSIZ_Host ; \ OTG_FS_GLOBAL_FS_GNPTXFSIZ_Host_NPTXFD, Non-periodic TxFIFO depth
[then]

execute-defined? use-OTG_FS_GLOBAL defined? OTG_FS_GLOBAL_FS_GNPTXSTS_NPTXFSAV? not and [if]
\ OTG_FS_GLOBAL_FS_GNPTXSTS (read-only) Reset:0x00080200
: OTG_FS_GLOBAL_FS_GNPTXSTS_NPTXFSAV? ( --  x ) OTG_FS_GLOBAL_FS_GNPTXSTS @ ; \ OTG_FS_GLOBAL_FS_GNPTXSTS_NPTXFSAV, Non-periodic TxFIFO space  available
: OTG_FS_GLOBAL_FS_GNPTXSTS_NPTQXSAV? ( --  x ) 16 lshift OTG_FS_GLOBAL_FS_GNPTXSTS @ ; \ OTG_FS_GLOBAL_FS_GNPTXSTS_NPTQXSAV, Non-periodic transmit request queue  space available
: OTG_FS_GLOBAL_FS_GNPTXSTS_NPTXQTOP? ( --  x ) 24 lshift OTG_FS_GLOBAL_FS_GNPTXSTS @ ; \ OTG_FS_GLOBAL_FS_GNPTXSTS_NPTXQTOP, Top of the non-periodic transmit request  queue
[then]

defined? use-OTG_FS_GLOBAL defined? OTG_FS_GLOBAL_FS_GCCFG_PWRDWN not and [if]
\ OTG_FS_GLOBAL_FS_GCCFG (read-write) Reset:0x00000000
: OTG_FS_GLOBAL_FS_GCCFG_PWRDWN ( -- x addr ) 16 bit OTG_FS_GLOBAL_FS_GCCFG ; \ OTG_FS_GLOBAL_FS_GCCFG_PWRDWN, Power down
: OTG_FS_GLOBAL_FS_GCCFG_VBUSASEN ( -- x addr ) 18 bit OTG_FS_GLOBAL_FS_GCCFG ; \ OTG_FS_GLOBAL_FS_GCCFG_VBUSASEN, Enable the VBUS sensing  device
: OTG_FS_GLOBAL_FS_GCCFG_VBUSBSEN ( -- x addr ) 19 bit OTG_FS_GLOBAL_FS_GCCFG ; \ OTG_FS_GLOBAL_FS_GCCFG_VBUSBSEN, Enable the VBUS sensing  device
: OTG_FS_GLOBAL_FS_GCCFG_SOFOUTEN ( -- x addr ) 20 bit OTG_FS_GLOBAL_FS_GCCFG ; \ OTG_FS_GLOBAL_FS_GCCFG_SOFOUTEN, SOF output enable
[then]

execute-defined? use-OTG_FS_GLOBAL defined? OTG_FS_GLOBAL_FS_CID_PRODUCT_ID not and [if]
\ OTG_FS_GLOBAL_FS_CID (read-write) Reset:0x00001000
: OTG_FS_GLOBAL_FS_CID_PRODUCT_ID ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) OTG_FS_GLOBAL_FS_CID ; \ OTG_FS_GLOBAL_FS_CID_PRODUCT_ID, Product ID field
[then]

defined? use-OTG_FS_GLOBAL defined? OTG_FS_GLOBAL_FS_HPTXFSIZ_PTXSA not and [if]
\ OTG_FS_GLOBAL_FS_HPTXFSIZ (read-write) Reset:0x02000600
: OTG_FS_GLOBAL_FS_HPTXFSIZ_PTXSA ( %bbbbbbbbbbbbbbbb -- x addr ) OTG_FS_GLOBAL_FS_HPTXFSIZ ; \ OTG_FS_GLOBAL_FS_HPTXFSIZ_PTXSA, Host periodic TxFIFO start  address
: OTG_FS_GLOBAL_FS_HPTXFSIZ_PTXFSIZ ( %bbbbbbbbbbbbbbbb -- x addr ) 16 lshift OTG_FS_GLOBAL_FS_HPTXFSIZ ; \ OTG_FS_GLOBAL_FS_HPTXFSIZ_PTXFSIZ, Host periodic TxFIFO depth
[then]

execute-defined? use-OTG_FS_GLOBAL defined? OTG_FS_GLOBAL_FS_DIEPTXF1_INEPTXSA not and [if]
\ OTG_FS_GLOBAL_FS_DIEPTXF1 (read-write) Reset:0x02000400
: OTG_FS_GLOBAL_FS_DIEPTXF1_INEPTXSA ( %bbbbbbbbbbbbbbbb -- x addr ) OTG_FS_GLOBAL_FS_DIEPTXF1 ; \ OTG_FS_GLOBAL_FS_DIEPTXF1_INEPTXSA, IN endpoint FIFO2 transmit RAM start  address
: OTG_FS_GLOBAL_FS_DIEPTXF1_INEPTXFD ( %bbbbbbbbbbbbbbbb -- x addr ) 16 lshift OTG_FS_GLOBAL_FS_DIEPTXF1 ; \ OTG_FS_GLOBAL_FS_DIEPTXF1_INEPTXFD, IN endpoint TxFIFO depth
[then]

defined? use-OTG_FS_GLOBAL defined? OTG_FS_GLOBAL_FS_DIEPTXF2_INEPTXSA not and [if]
\ OTG_FS_GLOBAL_FS_DIEPTXF2 (read-write) Reset:0x02000400
: OTG_FS_GLOBAL_FS_DIEPTXF2_INEPTXSA ( %bbbbbbbbbbbbbbbb -- x addr ) OTG_FS_GLOBAL_FS_DIEPTXF2 ; \ OTG_FS_GLOBAL_FS_DIEPTXF2_INEPTXSA, IN endpoint FIFO3 transmit RAM start  address
: OTG_FS_GLOBAL_FS_DIEPTXF2_INEPTXFD ( %bbbbbbbbbbbbbbbb -- x addr ) 16 lshift OTG_FS_GLOBAL_FS_DIEPTXF2 ; \ OTG_FS_GLOBAL_FS_DIEPTXF2_INEPTXFD, IN endpoint TxFIFO depth
[then]

execute-defined? use-OTG_FS_GLOBAL defined? OTG_FS_GLOBAL_FS_DIEPTXF3_INEPTXSA not and [if]
\ OTG_FS_GLOBAL_FS_DIEPTXF3 (read-write) Reset:0x02000400
: OTG_FS_GLOBAL_FS_DIEPTXF3_INEPTXSA ( %bbbbbbbbbbbbbbbb -- x addr ) OTG_FS_GLOBAL_FS_DIEPTXF3 ; \ OTG_FS_GLOBAL_FS_DIEPTXF3_INEPTXSA, IN endpoint FIFO4 transmit RAM start  address
: OTG_FS_GLOBAL_FS_DIEPTXF3_INEPTXFD ( %bbbbbbbbbbbbbbbb -- x addr ) 16 lshift OTG_FS_GLOBAL_FS_DIEPTXF3 ; \ OTG_FS_GLOBAL_FS_DIEPTXF3_INEPTXFD, IN endpoint TxFIFO depth
[then]

defined? use-OTG_FS_HOST defined? OTG_FS_HOST_FS_HCFG_FSLSPCS not and [if]
\ OTG_FS_HOST_FS_HCFG (multiple-access)  Reset:0x00000000
: OTG_FS_HOST_FS_HCFG_FSLSPCS ( %bb -- x addr ) OTG_FS_HOST_FS_HCFG ; \ OTG_FS_HOST_FS_HCFG_FSLSPCS, FS/LS PHY clock select
: OTG_FS_HOST_FS_HCFG_FSLSS ( -- x addr ) 2 bit OTG_FS_HOST_FS_HCFG ; \ OTG_FS_HOST_FS_HCFG_FSLSS, FS- and LS-only support
[then]

execute-defined? use-OTG_FS_HOST defined? OTG_FS_HOST_HFIR_FRIVL not and [if]
\ OTG_FS_HOST_HFIR (read-write) Reset:0x0000EA60
: OTG_FS_HOST_HFIR_FRIVL ( %bbbbbbbbbbbbbbbb -- x addr ) OTG_FS_HOST_HFIR ; \ OTG_FS_HOST_HFIR_FRIVL, Frame interval
[then]

defined? use-OTG_FS_HOST defined? OTG_FS_HOST_FS_HFNUM_FRNUM? not and [if]
\ OTG_FS_HOST_FS_HFNUM (read-only) Reset:0x00003FFF
: OTG_FS_HOST_FS_HFNUM_FRNUM? ( --  x ) OTG_FS_HOST_FS_HFNUM @ ; \ OTG_FS_HOST_FS_HFNUM_FRNUM, Frame number
: OTG_FS_HOST_FS_HFNUM_FTREM? ( --  x ) 16 lshift OTG_FS_HOST_FS_HFNUM @ ; \ OTG_FS_HOST_FS_HFNUM_FTREM, Frame time remaining
[then]

execute-defined? use-OTG_FS_HOST defined? OTG_FS_HOST_FS_HPTXSTS_PTXFSAVL not and [if]
\ OTG_FS_HOST_FS_HPTXSTS (multiple-access)  Reset:0x00080100
: OTG_FS_HOST_FS_HPTXSTS_PTXFSAVL ( %bbbbbbbbbbbbbbbb -- x addr ) OTG_FS_HOST_FS_HPTXSTS ; \ OTG_FS_HOST_FS_HPTXSTS_PTXFSAVL, Periodic transmit data FIFO space  available
: OTG_FS_HOST_FS_HPTXSTS_PTXQSAV ( %bbbbbbbb -- x addr ) 16 lshift OTG_FS_HOST_FS_HPTXSTS ; \ OTG_FS_HOST_FS_HPTXSTS_PTXQSAV, Periodic transmit request queue space  available
: OTG_FS_HOST_FS_HPTXSTS_PTXQTOP ( %bbbbbbbb -- x addr ) 24 lshift OTG_FS_HOST_FS_HPTXSTS ; \ OTG_FS_HOST_FS_HPTXSTS_PTXQTOP, Top of the periodic transmit request  queue
[then]

defined? use-OTG_FS_HOST defined? OTG_FS_HOST_HAINT_HAINT? not and [if]
\ OTG_FS_HOST_HAINT (read-only) Reset:0x00000000
: OTG_FS_HOST_HAINT_HAINT? ( --  x ) OTG_FS_HOST_HAINT @ ; \ OTG_FS_HOST_HAINT_HAINT, Channel interrupts
[then]

execute-defined? use-OTG_FS_HOST defined? OTG_FS_HOST_HAINTMSK_HAINTM not and [if]
\ OTG_FS_HOST_HAINTMSK (read-write) Reset:0x00000000
: OTG_FS_HOST_HAINTMSK_HAINTM ( %bbbbbbbbbbbbbbbb -- x addr ) OTG_FS_HOST_HAINTMSK ; \ OTG_FS_HOST_HAINTMSK_HAINTM, Channel interrupt mask
[then]

defined? use-OTG_FS_HOST defined? OTG_FS_HOST_FS_HPRT_PCSTS? not and [if]
\ OTG_FS_HOST_FS_HPRT (multiple-access)  Reset:0x00000000
: OTG_FS_HOST_FS_HPRT_PCSTS? ( -- 1|0 ) 0 bit OTG_FS_HOST_FS_HPRT bit@ ; \ OTG_FS_HOST_FS_HPRT_PCSTS, Port connect status
: OTG_FS_HOST_FS_HPRT_PCDET ( -- x addr ) 1 bit OTG_FS_HOST_FS_HPRT ; \ OTG_FS_HOST_FS_HPRT_PCDET, Port connect detected
: OTG_FS_HOST_FS_HPRT_PENA ( -- x addr ) 2 bit OTG_FS_HOST_FS_HPRT ; \ OTG_FS_HOST_FS_HPRT_PENA, Port enable
: OTG_FS_HOST_FS_HPRT_PENCHNG ( -- x addr ) 3 bit OTG_FS_HOST_FS_HPRT ; \ OTG_FS_HOST_FS_HPRT_PENCHNG, Port enable/disable change
: OTG_FS_HOST_FS_HPRT_POCA ( -- x addr ) 4 bit OTG_FS_HOST_FS_HPRT ; \ OTG_FS_HOST_FS_HPRT_POCA, Port overcurrent active
: OTG_FS_HOST_FS_HPRT_POCCHNG ( -- x addr ) 5 bit OTG_FS_HOST_FS_HPRT ; \ OTG_FS_HOST_FS_HPRT_POCCHNG, Port overcurrent change
: OTG_FS_HOST_FS_HPRT_PRES ( -- x addr ) 6 bit OTG_FS_HOST_FS_HPRT ; \ OTG_FS_HOST_FS_HPRT_PRES, Port resume
: OTG_FS_HOST_FS_HPRT_PSUSP ( -- x addr ) 7 bit OTG_FS_HOST_FS_HPRT ; \ OTG_FS_HOST_FS_HPRT_PSUSP, Port suspend
: OTG_FS_HOST_FS_HPRT_PRST ( -- x addr ) 8 bit OTG_FS_HOST_FS_HPRT ; \ OTG_FS_HOST_FS_HPRT_PRST, Port reset
: OTG_FS_HOST_FS_HPRT_PLSTS? ( %bb -- 1|0 ) 10 lshift OTG_FS_HOST_FS_HPRT bit@ ; \ OTG_FS_HOST_FS_HPRT_PLSTS, Port line status
: OTG_FS_HOST_FS_HPRT_PPWR ( -- x addr ) 12 bit OTG_FS_HOST_FS_HPRT ; \ OTG_FS_HOST_FS_HPRT_PPWR, Port power
: OTG_FS_HOST_FS_HPRT_PTCTL ( %bbbb -- x addr ) 13 lshift OTG_FS_HOST_FS_HPRT ; \ OTG_FS_HOST_FS_HPRT_PTCTL, Port test control
: OTG_FS_HOST_FS_HPRT_PSPD ( %bb -- x addr ) 17 lshift OTG_FS_HOST_FS_HPRT ; \ OTG_FS_HOST_FS_HPRT_PSPD, Port speed
[then]

execute-defined? use-OTG_FS_HOST defined? OTG_FS_HOST_FS_HCCHAR0_MPSIZ not and [if]
\ OTG_FS_HOST_FS_HCCHAR0 (read-write) Reset:0x00000000
: OTG_FS_HOST_FS_HCCHAR0_MPSIZ x addr ) OTG_FS_HOST_FS_HCCHAR0 ; \ OTG_FS_HOST_FS_HCCHAR0_MPSIZ, Maximum packet size
: OTG_FS_HOST_FS_HCCHAR0_EPNUM ( %bbbb -- x addr ) 11 lshift OTG_FS_HOST_FS_HCCHAR0 ; \ OTG_FS_HOST_FS_HCCHAR0_EPNUM, Endpoint number
: OTG_FS_HOST_FS_HCCHAR0_EPDIR ( -- x addr ) 15 bit OTG_FS_HOST_FS_HCCHAR0 ; \ OTG_FS_HOST_FS_HCCHAR0_EPDIR, Endpoint direction
: OTG_FS_HOST_FS_HCCHAR0_LSDEV ( -- x addr ) 17 bit OTG_FS_HOST_FS_HCCHAR0 ; \ OTG_FS_HOST_FS_HCCHAR0_LSDEV, Low-speed device
: OTG_FS_HOST_FS_HCCHAR0_EPTYP ( %bb -- x addr ) 18 lshift OTG_FS_HOST_FS_HCCHAR0 ; \ OTG_FS_HOST_FS_HCCHAR0_EPTYP, Endpoint type
: OTG_FS_HOST_FS_HCCHAR0_MCNT ( %bb -- x addr ) 20 lshift OTG_FS_HOST_FS_HCCHAR0 ; \ OTG_FS_HOST_FS_HCCHAR0_MCNT, Multicount
: OTG_FS_HOST_FS_HCCHAR0_DAD ( %bbbbbbb -- x addr ) 22 lshift OTG_FS_HOST_FS_HCCHAR0 ; \ OTG_FS_HOST_FS_HCCHAR0_DAD, Device address
: OTG_FS_HOST_FS_HCCHAR0_ODDFRM ( -- x addr ) 29 bit OTG_FS_HOST_FS_HCCHAR0 ; \ OTG_FS_HOST_FS_HCCHAR0_ODDFRM, Odd frame
: OTG_FS_HOST_FS_HCCHAR0_CHDIS ( -- x addr ) 30 bit OTG_FS_HOST_FS_HCCHAR0 ; \ OTG_FS_HOST_FS_HCCHAR0_CHDIS, Channel disable
: OTG_FS_HOST_FS_HCCHAR0_CHENA ( -- x addr ) 31 bit OTG_FS_HOST_FS_HCCHAR0 ; \ OTG_FS_HOST_FS_HCCHAR0_CHENA, Channel enable
[then]

defined? use-OTG_FS_HOST defined? OTG_FS_HOST_FS_HCCHAR1_MPSIZ not and [if]
\ OTG_FS_HOST_FS_HCCHAR1 (read-write) Reset:0x00000000
: OTG_FS_HOST_FS_HCCHAR1_MPSIZ x addr ) OTG_FS_HOST_FS_HCCHAR1 ; \ OTG_FS_HOST_FS_HCCHAR1_MPSIZ, Maximum packet size
: OTG_FS_HOST_FS_HCCHAR1_EPNUM ( %bbbb -- x addr ) 11 lshift OTG_FS_HOST_FS_HCCHAR1 ; \ OTG_FS_HOST_FS_HCCHAR1_EPNUM, Endpoint number
: OTG_FS_HOST_FS_HCCHAR1_EPDIR ( -- x addr ) 15 bit OTG_FS_HOST_FS_HCCHAR1 ; \ OTG_FS_HOST_FS_HCCHAR1_EPDIR, Endpoint direction
: OTG_FS_HOST_FS_HCCHAR1_LSDEV ( -- x addr ) 17 bit OTG_FS_HOST_FS_HCCHAR1 ; \ OTG_FS_HOST_FS_HCCHAR1_LSDEV, Low-speed device
: OTG_FS_HOST_FS_HCCHAR1_EPTYP ( %bb -- x addr ) 18 lshift OTG_FS_HOST_FS_HCCHAR1 ; \ OTG_FS_HOST_FS_HCCHAR1_EPTYP, Endpoint type
: OTG_FS_HOST_FS_HCCHAR1_MCNT ( %bb -- x addr ) 20 lshift OTG_FS_HOST_FS_HCCHAR1 ; \ OTG_FS_HOST_FS_HCCHAR1_MCNT, Multicount
: OTG_FS_HOST_FS_HCCHAR1_DAD ( %bbbbbbb -- x addr ) 22 lshift OTG_FS_HOST_FS_HCCHAR1 ; \ OTG_FS_HOST_FS_HCCHAR1_DAD, Device address
: OTG_FS_HOST_FS_HCCHAR1_ODDFRM ( -- x addr ) 29 bit OTG_FS_HOST_FS_HCCHAR1 ; \ OTG_FS_HOST_FS_HCCHAR1_ODDFRM, Odd frame
: OTG_FS_HOST_FS_HCCHAR1_CHDIS ( -- x addr ) 30 bit OTG_FS_HOST_FS_HCCHAR1 ; \ OTG_FS_HOST_FS_HCCHAR1_CHDIS, Channel disable
: OTG_FS_HOST_FS_HCCHAR1_CHENA ( -- x addr ) 31 bit OTG_FS_HOST_FS_HCCHAR1 ; \ OTG_FS_HOST_FS_HCCHAR1_CHENA, Channel enable
[then]

execute-defined? use-OTG_FS_HOST defined? OTG_FS_HOST_FS_HCCHAR2_MPSIZ not and [if]
\ OTG_FS_HOST_FS_HCCHAR2 (read-write) Reset:0x00000000
: OTG_FS_HOST_FS_HCCHAR2_MPSIZ x addr ) OTG_FS_HOST_FS_HCCHAR2 ; \ OTG_FS_HOST_FS_HCCHAR2_MPSIZ, Maximum packet size
: OTG_FS_HOST_FS_HCCHAR2_EPNUM ( %bbbb -- x addr ) 11 lshift OTG_FS_HOST_FS_HCCHAR2 ; \ OTG_FS_HOST_FS_HCCHAR2_EPNUM, Endpoint number
: OTG_FS_HOST_FS_HCCHAR2_EPDIR ( -- x addr ) 15 bit OTG_FS_HOST_FS_HCCHAR2 ; \ OTG_FS_HOST_FS_HCCHAR2_EPDIR, Endpoint direction
: OTG_FS_HOST_FS_HCCHAR2_LSDEV ( -- x addr ) 17 bit OTG_FS_HOST_FS_HCCHAR2 ; \ OTG_FS_HOST_FS_HCCHAR2_LSDEV, Low-speed device
: OTG_FS_HOST_FS_HCCHAR2_EPTYP ( %bb -- x addr ) 18 lshift OTG_FS_HOST_FS_HCCHAR2 ; \ OTG_FS_HOST_FS_HCCHAR2_EPTYP, Endpoint type
: OTG_FS_HOST_FS_HCCHAR2_MCNT ( %bb -- x addr ) 20 lshift OTG_FS_HOST_FS_HCCHAR2 ; \ OTG_FS_HOST_FS_HCCHAR2_MCNT, Multicount
: OTG_FS_HOST_FS_HCCHAR2_DAD ( %bbbbbbb -- x addr ) 22 lshift OTG_FS_HOST_FS_HCCHAR2 ; \ OTG_FS_HOST_FS_HCCHAR2_DAD, Device address
: OTG_FS_HOST_FS_HCCHAR2_ODDFRM ( -- x addr ) 29 bit OTG_FS_HOST_FS_HCCHAR2 ; \ OTG_FS_HOST_FS_HCCHAR2_ODDFRM, Odd frame
: OTG_FS_HOST_FS_HCCHAR2_CHDIS ( -- x addr ) 30 bit OTG_FS_HOST_FS_HCCHAR2 ; \ OTG_FS_HOST_FS_HCCHAR2_CHDIS, Channel disable
: OTG_FS_HOST_FS_HCCHAR2_CHENA ( -- x addr ) 31 bit OTG_FS_HOST_FS_HCCHAR2 ; \ OTG_FS_HOST_FS_HCCHAR2_CHENA, Channel enable
[then]

defined? use-OTG_FS_HOST defined? OTG_FS_HOST_FS_HCCHAR3_MPSIZ not and [if]
\ OTG_FS_HOST_FS_HCCHAR3 (read-write) Reset:0x00000000
: OTG_FS_HOST_FS_HCCHAR3_MPSIZ x addr ) OTG_FS_HOST_FS_HCCHAR3 ; \ OTG_FS_HOST_FS_HCCHAR3_MPSIZ, Maximum packet size
: OTG_FS_HOST_FS_HCCHAR3_EPNUM ( %bbbb -- x addr ) 11 lshift OTG_FS_HOST_FS_HCCHAR3 ; \ OTG_FS_HOST_FS_HCCHAR3_EPNUM, Endpoint number
: OTG_FS_HOST_FS_HCCHAR3_EPDIR ( -- x addr ) 15 bit OTG_FS_HOST_FS_HCCHAR3 ; \ OTG_FS_HOST_FS_HCCHAR3_EPDIR, Endpoint direction
: OTG_FS_HOST_FS_HCCHAR3_LSDEV ( -- x addr ) 17 bit OTG_FS_HOST_FS_HCCHAR3 ; \ OTG_FS_HOST_FS_HCCHAR3_LSDEV, Low-speed device
: OTG_FS_HOST_FS_HCCHAR3_EPTYP ( %bb -- x addr ) 18 lshift OTG_FS_HOST_FS_HCCHAR3 ; \ OTG_FS_HOST_FS_HCCHAR3_EPTYP, Endpoint type
: OTG_FS_HOST_FS_HCCHAR3_MCNT ( %bb -- x addr ) 20 lshift OTG_FS_HOST_FS_HCCHAR3 ; \ OTG_FS_HOST_FS_HCCHAR3_MCNT, Multicount
: OTG_FS_HOST_FS_HCCHAR3_DAD ( %bbbbbbb -- x addr ) 22 lshift OTG_FS_HOST_FS_HCCHAR3 ; \ OTG_FS_HOST_FS_HCCHAR3_DAD, Device address
: OTG_FS_HOST_FS_HCCHAR3_ODDFRM ( -- x addr ) 29 bit OTG_FS_HOST_FS_HCCHAR3 ; \ OTG_FS_HOST_FS_HCCHAR3_ODDFRM, Odd frame
: OTG_FS_HOST_FS_HCCHAR3_CHDIS ( -- x addr ) 30 bit OTG_FS_HOST_FS_HCCHAR3 ; \ OTG_FS_HOST_FS_HCCHAR3_CHDIS, Channel disable
: OTG_FS_HOST_FS_HCCHAR3_CHENA ( -- x addr ) 31 bit OTG_FS_HOST_FS_HCCHAR3 ; \ OTG_FS_HOST_FS_HCCHAR3_CHENA, Channel enable
[then]

execute-defined? use-OTG_FS_HOST defined? OTG_FS_HOST_FS_HCCHAR4_MPSIZ not and [if]
\ OTG_FS_HOST_FS_HCCHAR4 (read-write) Reset:0x00000000
: OTG_FS_HOST_FS_HCCHAR4_MPSIZ x addr ) OTG_FS_HOST_FS_HCCHAR4 ; \ OTG_FS_HOST_FS_HCCHAR4_MPSIZ, Maximum packet size
: OTG_FS_HOST_FS_HCCHAR4_EPNUM ( %bbbb -- x addr ) 11 lshift OTG_FS_HOST_FS_HCCHAR4 ; \ OTG_FS_HOST_FS_HCCHAR4_EPNUM, Endpoint number
: OTG_FS_HOST_FS_HCCHAR4_EPDIR ( -- x addr ) 15 bit OTG_FS_HOST_FS_HCCHAR4 ; \ OTG_FS_HOST_FS_HCCHAR4_EPDIR, Endpoint direction
: OTG_FS_HOST_FS_HCCHAR4_LSDEV ( -- x addr ) 17 bit OTG_FS_HOST_FS_HCCHAR4 ; \ OTG_FS_HOST_FS_HCCHAR4_LSDEV, Low-speed device
: OTG_FS_HOST_FS_HCCHAR4_EPTYP ( %bb -- x addr ) 18 lshift OTG_FS_HOST_FS_HCCHAR4 ; \ OTG_FS_HOST_FS_HCCHAR4_EPTYP, Endpoint type
: OTG_FS_HOST_FS_HCCHAR4_MCNT ( %bb -- x addr ) 20 lshift OTG_FS_HOST_FS_HCCHAR4 ; \ OTG_FS_HOST_FS_HCCHAR4_MCNT, Multicount
: OTG_FS_HOST_FS_HCCHAR4_DAD ( %bbbbbbb -- x addr ) 22 lshift OTG_FS_HOST_FS_HCCHAR4 ; \ OTG_FS_HOST_FS_HCCHAR4_DAD, Device address
: OTG_FS_HOST_FS_HCCHAR4_ODDFRM ( -- x addr ) 29 bit OTG_FS_HOST_FS_HCCHAR4 ; \ OTG_FS_HOST_FS_HCCHAR4_ODDFRM, Odd frame
: OTG_FS_HOST_FS_HCCHAR4_CHDIS ( -- x addr ) 30 bit OTG_FS_HOST_FS_HCCHAR4 ; \ OTG_FS_HOST_FS_HCCHAR4_CHDIS, Channel disable
: OTG_FS_HOST_FS_HCCHAR4_CHENA ( -- x addr ) 31 bit OTG_FS_HOST_FS_HCCHAR4 ; \ OTG_FS_HOST_FS_HCCHAR4_CHENA, Channel enable
[then]

defined? use-OTG_FS_HOST defined? OTG_FS_HOST_FS_HCCHAR5_MPSIZ not and [if]
\ OTG_FS_HOST_FS_HCCHAR5 (read-write) Reset:0x00000000
: OTG_FS_HOST_FS_HCCHAR5_MPSIZ x addr ) OTG_FS_HOST_FS_HCCHAR5 ; \ OTG_FS_HOST_FS_HCCHAR5_MPSIZ, Maximum packet size
: OTG_FS_HOST_FS_HCCHAR5_EPNUM ( %bbbb -- x addr ) 11 lshift OTG_FS_HOST_FS_HCCHAR5 ; \ OTG_FS_HOST_FS_HCCHAR5_EPNUM, Endpoint number
: OTG_FS_HOST_FS_HCCHAR5_EPDIR ( -- x addr ) 15 bit OTG_FS_HOST_FS_HCCHAR5 ; \ OTG_FS_HOST_FS_HCCHAR5_EPDIR, Endpoint direction
: OTG_FS_HOST_FS_HCCHAR5_LSDEV ( -- x addr ) 17 bit OTG_FS_HOST_FS_HCCHAR5 ; \ OTG_FS_HOST_FS_HCCHAR5_LSDEV, Low-speed device
: OTG_FS_HOST_FS_HCCHAR5_EPTYP ( %bb -- x addr ) 18 lshift OTG_FS_HOST_FS_HCCHAR5 ; \ OTG_FS_HOST_FS_HCCHAR5_EPTYP, Endpoint type
: OTG_FS_HOST_FS_HCCHAR5_MCNT ( %bb -- x addr ) 20 lshift OTG_FS_HOST_FS_HCCHAR5 ; \ OTG_FS_HOST_FS_HCCHAR5_MCNT, Multicount
: OTG_FS_HOST_FS_HCCHAR5_DAD ( %bbbbbbb -- x addr ) 22 lshift OTG_FS_HOST_FS_HCCHAR5 ; \ OTG_FS_HOST_FS_HCCHAR5_DAD, Device address
: OTG_FS_HOST_FS_HCCHAR5_ODDFRM ( -- x addr ) 29 bit OTG_FS_HOST_FS_HCCHAR5 ; \ OTG_FS_HOST_FS_HCCHAR5_ODDFRM, Odd frame
: OTG_FS_HOST_FS_HCCHAR5_CHDIS ( -- x addr ) 30 bit OTG_FS_HOST_FS_HCCHAR5 ; \ OTG_FS_HOST_FS_HCCHAR5_CHDIS, Channel disable
: OTG_FS_HOST_FS_HCCHAR5_CHENA ( -- x addr ) 31 bit OTG_FS_HOST_FS_HCCHAR5 ; \ OTG_FS_HOST_FS_HCCHAR5_CHENA, Channel enable
[then]

execute-defined? use-OTG_FS_HOST defined? OTG_FS_HOST_FS_HCCHAR6_MPSIZ not and [if]
\ OTG_FS_HOST_FS_HCCHAR6 (read-write) Reset:0x00000000
: OTG_FS_HOST_FS_HCCHAR6_MPSIZ x addr ) OTG_FS_HOST_FS_HCCHAR6 ; \ OTG_FS_HOST_FS_HCCHAR6_MPSIZ, Maximum packet size
: OTG_FS_HOST_FS_HCCHAR6_EPNUM ( %bbbb -- x addr ) 11 lshift OTG_FS_HOST_FS_HCCHAR6 ; \ OTG_FS_HOST_FS_HCCHAR6_EPNUM, Endpoint number
: OTG_FS_HOST_FS_HCCHAR6_EPDIR ( -- x addr ) 15 bit OTG_FS_HOST_FS_HCCHAR6 ; \ OTG_FS_HOST_FS_HCCHAR6_EPDIR, Endpoint direction
: OTG_FS_HOST_FS_HCCHAR6_LSDEV ( -- x addr ) 17 bit OTG_FS_HOST_FS_HCCHAR6 ; \ OTG_FS_HOST_FS_HCCHAR6_LSDEV, Low-speed device
: OTG_FS_HOST_FS_HCCHAR6_EPTYP ( %bb -- x addr ) 18 lshift OTG_FS_HOST_FS_HCCHAR6 ; \ OTG_FS_HOST_FS_HCCHAR6_EPTYP, Endpoint type
: OTG_FS_HOST_FS_HCCHAR6_MCNT ( %bb -- x addr ) 20 lshift OTG_FS_HOST_FS_HCCHAR6 ; \ OTG_FS_HOST_FS_HCCHAR6_MCNT, Multicount
: OTG_FS_HOST_FS_HCCHAR6_DAD ( %bbbbbbb -- x addr ) 22 lshift OTG_FS_HOST_FS_HCCHAR6 ; \ OTG_FS_HOST_FS_HCCHAR6_DAD, Device address
: OTG_FS_HOST_FS_HCCHAR6_ODDFRM ( -- x addr ) 29 bit OTG_FS_HOST_FS_HCCHAR6 ; \ OTG_FS_HOST_FS_HCCHAR6_ODDFRM, Odd frame
: OTG_FS_HOST_FS_HCCHAR6_CHDIS ( -- x addr ) 30 bit OTG_FS_HOST_FS_HCCHAR6 ; \ OTG_FS_HOST_FS_HCCHAR6_CHDIS, Channel disable
: OTG_FS_HOST_FS_HCCHAR6_CHENA ( -- x addr ) 31 bit OTG_FS_HOST_FS_HCCHAR6 ; \ OTG_FS_HOST_FS_HCCHAR6_CHENA, Channel enable
[then]

defined? use-OTG_FS_HOST defined? OTG_FS_HOST_FS_HCCHAR7_MPSIZ not and [if]
\ OTG_FS_HOST_FS_HCCHAR7 (read-write) Reset:0x00000000
: OTG_FS_HOST_FS_HCCHAR7_MPSIZ x addr ) OTG_FS_HOST_FS_HCCHAR7 ; \ OTG_FS_HOST_FS_HCCHAR7_MPSIZ, Maximum packet size
: OTG_FS_HOST_FS_HCCHAR7_EPNUM ( %bbbb -- x addr ) 11 lshift OTG_FS_HOST_FS_HCCHAR7 ; \ OTG_FS_HOST_FS_HCCHAR7_EPNUM, Endpoint number
: OTG_FS_HOST_FS_HCCHAR7_EPDIR ( -- x addr ) 15 bit OTG_FS_HOST_FS_HCCHAR7 ; \ OTG_FS_HOST_FS_HCCHAR7_EPDIR, Endpoint direction
: OTG_FS_HOST_FS_HCCHAR7_LSDEV ( -- x addr ) 17 bit OTG_FS_HOST_FS_HCCHAR7 ; \ OTG_FS_HOST_FS_HCCHAR7_LSDEV, Low-speed device
: OTG_FS_HOST_FS_HCCHAR7_EPTYP ( %bb -- x addr ) 18 lshift OTG_FS_HOST_FS_HCCHAR7 ; \ OTG_FS_HOST_FS_HCCHAR7_EPTYP, Endpoint type
: OTG_FS_HOST_FS_HCCHAR7_MCNT ( %bb -- x addr ) 20 lshift OTG_FS_HOST_FS_HCCHAR7 ; \ OTG_FS_HOST_FS_HCCHAR7_MCNT, Multicount
: OTG_FS_HOST_FS_HCCHAR7_DAD ( %bbbbbbb -- x addr ) 22 lshift OTG_FS_HOST_FS_HCCHAR7 ; \ OTG_FS_HOST_FS_HCCHAR7_DAD, Device address
: OTG_FS_HOST_FS_HCCHAR7_ODDFRM ( -- x addr ) 29 bit OTG_FS_HOST_FS_HCCHAR7 ; \ OTG_FS_HOST_FS_HCCHAR7_ODDFRM, Odd frame
: OTG_FS_HOST_FS_HCCHAR7_CHDIS ( -- x addr ) 30 bit OTG_FS_HOST_FS_HCCHAR7 ; \ OTG_FS_HOST_FS_HCCHAR7_CHDIS, Channel disable
: OTG_FS_HOST_FS_HCCHAR7_CHENA ( -- x addr ) 31 bit OTG_FS_HOST_FS_HCCHAR7 ; \ OTG_FS_HOST_FS_HCCHAR7_CHENA, Channel enable
[then]

execute-defined? use-OTG_FS_HOST defined? OTG_FS_HOST_FS_HCINT0_XFRC not and [if]
\ OTG_FS_HOST_FS_HCINT0 (read-write) Reset:0x00000000
: OTG_FS_HOST_FS_HCINT0_XFRC ( -- x addr ) 0 bit OTG_FS_HOST_FS_HCINT0 ; \ OTG_FS_HOST_FS_HCINT0_XFRC, Transfer completed
: OTG_FS_HOST_FS_HCINT0_CHH ( -- x addr ) 1 bit OTG_FS_HOST_FS_HCINT0 ; \ OTG_FS_HOST_FS_HCINT0_CHH, Channel halted
: OTG_FS_HOST_FS_HCINT0_STALL ( -- x addr ) 3 bit OTG_FS_HOST_FS_HCINT0 ; \ OTG_FS_HOST_FS_HCINT0_STALL, STALL response received  interrupt
: OTG_FS_HOST_FS_HCINT0_NAK ( -- x addr ) 4 bit OTG_FS_HOST_FS_HCINT0 ; \ OTG_FS_HOST_FS_HCINT0_NAK, NAK response received  interrupt
: OTG_FS_HOST_FS_HCINT0_ACK ( -- x addr ) 5 bit OTG_FS_HOST_FS_HCINT0 ; \ OTG_FS_HOST_FS_HCINT0_ACK, ACK response received/transmitted  interrupt
: OTG_FS_HOST_FS_HCINT0_TXERR ( -- x addr ) 7 bit OTG_FS_HOST_FS_HCINT0 ; \ OTG_FS_HOST_FS_HCINT0_TXERR, Transaction error
: OTG_FS_HOST_FS_HCINT0_BBERR ( -- x addr ) 8 bit OTG_FS_HOST_FS_HCINT0 ; \ OTG_FS_HOST_FS_HCINT0_BBERR, Babble error
: OTG_FS_HOST_FS_HCINT0_FRMOR ( -- x addr ) 9 bit OTG_FS_HOST_FS_HCINT0 ; \ OTG_FS_HOST_FS_HCINT0_FRMOR, Frame overrun
: OTG_FS_HOST_FS_HCINT0_DTERR ( -- x addr ) 10 bit OTG_FS_HOST_FS_HCINT0 ; \ OTG_FS_HOST_FS_HCINT0_DTERR, Data toggle error
[then]

defined? use-OTG_FS_HOST defined? OTG_FS_HOST_FS_HCINT1_XFRC not and [if]
\ OTG_FS_HOST_FS_HCINT1 (read-write) Reset:0x00000000
: OTG_FS_HOST_FS_HCINT1_XFRC ( -- x addr ) 0 bit OTG_FS_HOST_FS_HCINT1 ; \ OTG_FS_HOST_FS_HCINT1_XFRC, Transfer completed
: OTG_FS_HOST_FS_HCINT1_CHH ( -- x addr ) 1 bit OTG_FS_HOST_FS_HCINT1 ; \ OTG_FS_HOST_FS_HCINT1_CHH, Channel halted
: OTG_FS_HOST_FS_HCINT1_STALL ( -- x addr ) 3 bit OTG_FS_HOST_FS_HCINT1 ; \ OTG_FS_HOST_FS_HCINT1_STALL, STALL response received  interrupt
: OTG_FS_HOST_FS_HCINT1_NAK ( -- x addr ) 4 bit OTG_FS_HOST_FS_HCINT1 ; \ OTG_FS_HOST_FS_HCINT1_NAK, NAK response received  interrupt
: OTG_FS_HOST_FS_HCINT1_ACK ( -- x addr ) 5 bit OTG_FS_HOST_FS_HCINT1 ; \ OTG_FS_HOST_FS_HCINT1_ACK, ACK response received/transmitted  interrupt
: OTG_FS_HOST_FS_HCINT1_TXERR ( -- x addr ) 7 bit OTG_FS_HOST_FS_HCINT1 ; \ OTG_FS_HOST_FS_HCINT1_TXERR, Transaction error
: OTG_FS_HOST_FS_HCINT1_BBERR ( -- x addr ) 8 bit OTG_FS_HOST_FS_HCINT1 ; \ OTG_FS_HOST_FS_HCINT1_BBERR, Babble error
: OTG_FS_HOST_FS_HCINT1_FRMOR ( -- x addr ) 9 bit OTG_FS_HOST_FS_HCINT1 ; \ OTG_FS_HOST_FS_HCINT1_FRMOR, Frame overrun
: OTG_FS_HOST_FS_HCINT1_DTERR ( -- x addr ) 10 bit OTG_FS_HOST_FS_HCINT1 ; \ OTG_FS_HOST_FS_HCINT1_DTERR, Data toggle error
[then]

execute-defined? use-OTG_FS_HOST defined? OTG_FS_HOST_FS_HCINT2_XFRC not and [if]
\ OTG_FS_HOST_FS_HCINT2 (read-write) Reset:0x00000000
: OTG_FS_HOST_FS_HCINT2_XFRC ( -- x addr ) 0 bit OTG_FS_HOST_FS_HCINT2 ; \ OTG_FS_HOST_FS_HCINT2_XFRC, Transfer completed
: OTG_FS_HOST_FS_HCINT2_CHH ( -- x addr ) 1 bit OTG_FS_HOST_FS_HCINT2 ; \ OTG_FS_HOST_FS_HCINT2_CHH, Channel halted
: OTG_FS_HOST_FS_HCINT2_STALL ( -- x addr ) 3 bit OTG_FS_HOST_FS_HCINT2 ; \ OTG_FS_HOST_FS_HCINT2_STALL, STALL response received  interrupt
: OTG_FS_HOST_FS_HCINT2_NAK ( -- x addr ) 4 bit OTG_FS_HOST_FS_HCINT2 ; \ OTG_FS_HOST_FS_HCINT2_NAK, NAK response received  interrupt
: OTG_FS_HOST_FS_HCINT2_ACK ( -- x addr ) 5 bit OTG_FS_HOST_FS_HCINT2 ; \ OTG_FS_HOST_FS_HCINT2_ACK, ACK response received/transmitted  interrupt
: OTG_FS_HOST_FS_HCINT2_TXERR ( -- x addr ) 7 bit OTG_FS_HOST_FS_HCINT2 ; \ OTG_FS_HOST_FS_HCINT2_TXERR, Transaction error
: OTG_FS_HOST_FS_HCINT2_BBERR ( -- x addr ) 8 bit OTG_FS_HOST_FS_HCINT2 ; \ OTG_FS_HOST_FS_HCINT2_BBERR, Babble error
: OTG_FS_HOST_FS_HCINT2_FRMOR ( -- x addr ) 9 bit OTG_FS_HOST_FS_HCINT2 ; \ OTG_FS_HOST_FS_HCINT2_FRMOR, Frame overrun
: OTG_FS_HOST_FS_HCINT2_DTERR ( -- x addr ) 10 bit OTG_FS_HOST_FS_HCINT2 ; \ OTG_FS_HOST_FS_HCINT2_DTERR, Data toggle error
[then]

defined? use-OTG_FS_HOST defined? OTG_FS_HOST_FS_HCINT3_XFRC not and [if]
\ OTG_FS_HOST_FS_HCINT3 (read-write) Reset:0x00000000
: OTG_FS_HOST_FS_HCINT3_XFRC ( -- x addr ) 0 bit OTG_FS_HOST_FS_HCINT3 ; \ OTG_FS_HOST_FS_HCINT3_XFRC, Transfer completed
: OTG_FS_HOST_FS_HCINT3_CHH ( -- x addr ) 1 bit OTG_FS_HOST_FS_HCINT3 ; \ OTG_FS_HOST_FS_HCINT3_CHH, Channel halted
: OTG_FS_HOST_FS_HCINT3_STALL ( -- x addr ) 3 bit OTG_FS_HOST_FS_HCINT3 ; \ OTG_FS_HOST_FS_HCINT3_STALL, STALL response received  interrupt
: OTG_FS_HOST_FS_HCINT3_NAK ( -- x addr ) 4 bit OTG_FS_HOST_FS_HCINT3 ; \ OTG_FS_HOST_FS_HCINT3_NAK, NAK response received  interrupt
: OTG_FS_HOST_FS_HCINT3_ACK ( -- x addr ) 5 bit OTG_FS_HOST_FS_HCINT3 ; \ OTG_FS_HOST_FS_HCINT3_ACK, ACK response received/transmitted  interrupt
: OTG_FS_HOST_FS_HCINT3_TXERR ( -- x addr ) 7 bit OTG_FS_HOST_FS_HCINT3 ; \ OTG_FS_HOST_FS_HCINT3_TXERR, Transaction error
: OTG_FS_HOST_FS_HCINT3_BBERR ( -- x addr ) 8 bit OTG_FS_HOST_FS_HCINT3 ; \ OTG_FS_HOST_FS_HCINT3_BBERR, Babble error
: OTG_FS_HOST_FS_HCINT3_FRMOR ( -- x addr ) 9 bit OTG_FS_HOST_FS_HCINT3 ; \ OTG_FS_HOST_FS_HCINT3_FRMOR, Frame overrun
: OTG_FS_HOST_FS_HCINT3_DTERR ( -- x addr ) 10 bit OTG_FS_HOST_FS_HCINT3 ; \ OTG_FS_HOST_FS_HCINT3_DTERR, Data toggle error
[then]

execute-defined? use-OTG_FS_HOST defined? OTG_FS_HOST_FS_HCINT4_XFRC not and [if]
\ OTG_FS_HOST_FS_HCINT4 (read-write) Reset:0x00000000
: OTG_FS_HOST_FS_HCINT4_XFRC ( -- x addr ) 0 bit OTG_FS_HOST_FS_HCINT4 ; \ OTG_FS_HOST_FS_HCINT4_XFRC, Transfer completed
: OTG_FS_HOST_FS_HCINT4_CHH ( -- x addr ) 1 bit OTG_FS_HOST_FS_HCINT4 ; \ OTG_FS_HOST_FS_HCINT4_CHH, Channel halted
: OTG_FS_HOST_FS_HCINT4_STALL ( -- x addr ) 3 bit OTG_FS_HOST_FS_HCINT4 ; \ OTG_FS_HOST_FS_HCINT4_STALL, STALL response received  interrupt
: OTG_FS_HOST_FS_HCINT4_NAK ( -- x addr ) 4 bit OTG_FS_HOST_FS_HCINT4 ; \ OTG_FS_HOST_FS_HCINT4_NAK, NAK response received  interrupt
: OTG_FS_HOST_FS_HCINT4_ACK ( -- x addr ) 5 bit OTG_FS_HOST_FS_HCINT4 ; \ OTG_FS_HOST_FS_HCINT4_ACK, ACK response received/transmitted  interrupt
: OTG_FS_HOST_FS_HCINT4_TXERR ( -- x addr ) 7 bit OTG_FS_HOST_FS_HCINT4 ; \ OTG_FS_HOST_FS_HCINT4_TXERR, Transaction error
: OTG_FS_HOST_FS_HCINT4_BBERR ( -- x addr ) 8 bit OTG_FS_HOST_FS_HCINT4 ; \ OTG_FS_HOST_FS_HCINT4_BBERR, Babble error
: OTG_FS_HOST_FS_HCINT4_FRMOR ( -- x addr ) 9 bit OTG_FS_HOST_FS_HCINT4 ; \ OTG_FS_HOST_FS_HCINT4_FRMOR, Frame overrun
: OTG_FS_HOST_FS_HCINT4_DTERR ( -- x addr ) 10 bit OTG_FS_HOST_FS_HCINT4 ; \ OTG_FS_HOST_FS_HCINT4_DTERR, Data toggle error
[then]

defined? use-OTG_FS_HOST defined? OTG_FS_HOST_FS_HCINT5_XFRC not and [if]
\ OTG_FS_HOST_FS_HCINT5 (read-write) Reset:0x00000000
: OTG_FS_HOST_FS_HCINT5_XFRC ( -- x addr ) 0 bit OTG_FS_HOST_FS_HCINT5 ; \ OTG_FS_HOST_FS_HCINT5_XFRC, Transfer completed
: OTG_FS_HOST_FS_HCINT5_CHH ( -- x addr ) 1 bit OTG_FS_HOST_FS_HCINT5 ; \ OTG_FS_HOST_FS_HCINT5_CHH, Channel halted
: OTG_FS_HOST_FS_HCINT5_STALL ( -- x addr ) 3 bit OTG_FS_HOST_FS_HCINT5 ; \ OTG_FS_HOST_FS_HCINT5_STALL, STALL response received  interrupt
: OTG_FS_HOST_FS_HCINT5_NAK ( -- x addr ) 4 bit OTG_FS_HOST_FS_HCINT5 ; \ OTG_FS_HOST_FS_HCINT5_NAK, NAK response received  interrupt
: OTG_FS_HOST_FS_HCINT5_ACK ( -- x addr ) 5 bit OTG_FS_HOST_FS_HCINT5 ; \ OTG_FS_HOST_FS_HCINT5_ACK, ACK response received/transmitted  interrupt
: OTG_FS_HOST_FS_HCINT5_TXERR ( -- x addr ) 7 bit OTG_FS_HOST_FS_HCINT5 ; \ OTG_FS_HOST_FS_HCINT5_TXERR, Transaction error
: OTG_FS_HOST_FS_HCINT5_BBERR ( -- x addr ) 8 bit OTG_FS_HOST_FS_HCINT5 ; \ OTG_FS_HOST_FS_HCINT5_BBERR, Babble error
: OTG_FS_HOST_FS_HCINT5_FRMOR ( -- x addr ) 9 bit OTG_FS_HOST_FS_HCINT5 ; \ OTG_FS_HOST_FS_HCINT5_FRMOR, Frame overrun
: OTG_FS_HOST_FS_HCINT5_DTERR ( -- x addr ) 10 bit OTG_FS_HOST_FS_HCINT5 ; \ OTG_FS_HOST_FS_HCINT5_DTERR, Data toggle error
[then]

execute-defined? use-OTG_FS_HOST defined? OTG_FS_HOST_FS_HCINT6_XFRC not and [if]
\ OTG_FS_HOST_FS_HCINT6 (read-write) Reset:0x00000000
: OTG_FS_HOST_FS_HCINT6_XFRC ( -- x addr ) 0 bit OTG_FS_HOST_FS_HCINT6 ; \ OTG_FS_HOST_FS_HCINT6_XFRC, Transfer completed
: OTG_FS_HOST_FS_HCINT6_CHH ( -- x addr ) 1 bit OTG_FS_HOST_FS_HCINT6 ; \ OTG_FS_HOST_FS_HCINT6_CHH, Channel halted
: OTG_FS_HOST_FS_HCINT6_STALL ( -- x addr ) 3 bit OTG_FS_HOST_FS_HCINT6 ; \ OTG_FS_HOST_FS_HCINT6_STALL, STALL response received  interrupt
: OTG_FS_HOST_FS_HCINT6_NAK ( -- x addr ) 4 bit OTG_FS_HOST_FS_HCINT6 ; \ OTG_FS_HOST_FS_HCINT6_NAK, NAK response received  interrupt
: OTG_FS_HOST_FS_HCINT6_ACK ( -- x addr ) 5 bit OTG_FS_HOST_FS_HCINT6 ; \ OTG_FS_HOST_FS_HCINT6_ACK, ACK response received/transmitted  interrupt
: OTG_FS_HOST_FS_HCINT6_TXERR ( -- x addr ) 7 bit OTG_FS_HOST_FS_HCINT6 ; \ OTG_FS_HOST_FS_HCINT6_TXERR, Transaction error
: OTG_FS_HOST_FS_HCINT6_BBERR ( -- x addr ) 8 bit OTG_FS_HOST_FS_HCINT6 ; \ OTG_FS_HOST_FS_HCINT6_BBERR, Babble error
: OTG_FS_HOST_FS_HCINT6_FRMOR ( -- x addr ) 9 bit OTG_FS_HOST_FS_HCINT6 ; \ OTG_FS_HOST_FS_HCINT6_FRMOR, Frame overrun
: OTG_FS_HOST_FS_HCINT6_DTERR ( -- x addr ) 10 bit OTG_FS_HOST_FS_HCINT6 ; \ OTG_FS_HOST_FS_HCINT6_DTERR, Data toggle error
[then]

defined? use-OTG_FS_HOST defined? OTG_FS_HOST_FS_HCINT7_XFRC not and [if]
\ OTG_FS_HOST_FS_HCINT7 (read-write) Reset:0x00000000
: OTG_FS_HOST_FS_HCINT7_XFRC ( -- x addr ) 0 bit OTG_FS_HOST_FS_HCINT7 ; \ OTG_FS_HOST_FS_HCINT7_XFRC, Transfer completed
: OTG_FS_HOST_FS_HCINT7_CHH ( -- x addr ) 1 bit OTG_FS_HOST_FS_HCINT7 ; \ OTG_FS_HOST_FS_HCINT7_CHH, Channel halted
: OTG_FS_HOST_FS_HCINT7_STALL ( -- x addr ) 3 bit OTG_FS_HOST_FS_HCINT7 ; \ OTG_FS_HOST_FS_HCINT7_STALL, STALL response received  interrupt
: OTG_FS_HOST_FS_HCINT7_NAK ( -- x addr ) 4 bit OTG_FS_HOST_FS_HCINT7 ; \ OTG_FS_HOST_FS_HCINT7_NAK, NAK response received  interrupt
: OTG_FS_HOST_FS_HCINT7_ACK ( -- x addr ) 5 bit OTG_FS_HOST_FS_HCINT7 ; \ OTG_FS_HOST_FS_HCINT7_ACK, ACK response received/transmitted  interrupt
: OTG_FS_HOST_FS_HCINT7_TXERR ( -- x addr ) 7 bit OTG_FS_HOST_FS_HCINT7 ; \ OTG_FS_HOST_FS_HCINT7_TXERR, Transaction error
: OTG_FS_HOST_FS_HCINT7_BBERR ( -- x addr ) 8 bit OTG_FS_HOST_FS_HCINT7 ; \ OTG_FS_HOST_FS_HCINT7_BBERR, Babble error
: OTG_FS_HOST_FS_HCINT7_FRMOR ( -- x addr ) 9 bit OTG_FS_HOST_FS_HCINT7 ; \ OTG_FS_HOST_FS_HCINT7_FRMOR, Frame overrun
: OTG_FS_HOST_FS_HCINT7_DTERR ( -- x addr ) 10 bit OTG_FS_HOST_FS_HCINT7 ; \ OTG_FS_HOST_FS_HCINT7_DTERR, Data toggle error
[then]

execute-defined? use-OTG_FS_HOST defined? OTG_FS_HOST_FS_HCINTMSK0_XFRCM not and [if]
\ OTG_FS_HOST_FS_HCINTMSK0 (read-write) Reset:0x00000000
: OTG_FS_HOST_FS_HCINTMSK0_XFRCM ( -- x addr ) 0 bit OTG_FS_HOST_FS_HCINTMSK0 ; \ OTG_FS_HOST_FS_HCINTMSK0_XFRCM, Transfer completed mask
: OTG_FS_HOST_FS_HCINTMSK0_CHHM ( -- x addr ) 1 bit OTG_FS_HOST_FS_HCINTMSK0 ; \ OTG_FS_HOST_FS_HCINTMSK0_CHHM, Channel halted mask
: OTG_FS_HOST_FS_HCINTMSK0_STALLM ( -- x addr ) 3 bit OTG_FS_HOST_FS_HCINTMSK0 ; \ OTG_FS_HOST_FS_HCINTMSK0_STALLM, STALL response received interrupt  mask
: OTG_FS_HOST_FS_HCINTMSK0_NAKM ( -- x addr ) 4 bit OTG_FS_HOST_FS_HCINTMSK0 ; \ OTG_FS_HOST_FS_HCINTMSK0_NAKM, NAK response received interrupt  mask
: OTG_FS_HOST_FS_HCINTMSK0_ACKM ( -- x addr ) 5 bit OTG_FS_HOST_FS_HCINTMSK0 ; \ OTG_FS_HOST_FS_HCINTMSK0_ACKM, ACK response received/transmitted  interrupt mask
: OTG_FS_HOST_FS_HCINTMSK0_NYET ( -- x addr ) 6 bit OTG_FS_HOST_FS_HCINTMSK0 ; \ OTG_FS_HOST_FS_HCINTMSK0_NYET, response received interrupt  mask
: OTG_FS_HOST_FS_HCINTMSK0_TXERRM ( -- x addr ) 7 bit OTG_FS_HOST_FS_HCINTMSK0 ; \ OTG_FS_HOST_FS_HCINTMSK0_TXERRM, Transaction error mask
: OTG_FS_HOST_FS_HCINTMSK0_BBERRM ( -- x addr ) 8 bit OTG_FS_HOST_FS_HCINTMSK0 ; \ OTG_FS_HOST_FS_HCINTMSK0_BBERRM, Babble error mask
: OTG_FS_HOST_FS_HCINTMSK0_FRMORM ( -- x addr ) 9 bit OTG_FS_HOST_FS_HCINTMSK0 ; \ OTG_FS_HOST_FS_HCINTMSK0_FRMORM, Frame overrun mask
: OTG_FS_HOST_FS_HCINTMSK0_DTERRM ( -- x addr ) 10 bit OTG_FS_HOST_FS_HCINTMSK0 ; \ OTG_FS_HOST_FS_HCINTMSK0_DTERRM, Data toggle error mask
[then]

defined? use-OTG_FS_HOST defined? OTG_FS_HOST_FS_HCINTMSK1_XFRCM not and [if]
\ OTG_FS_HOST_FS_HCINTMSK1 (read-write) Reset:0x00000000
: OTG_FS_HOST_FS_HCINTMSK1_XFRCM ( -- x addr ) 0 bit OTG_FS_HOST_FS_HCINTMSK1 ; \ OTG_FS_HOST_FS_HCINTMSK1_XFRCM, Transfer completed mask
: OTG_FS_HOST_FS_HCINTMSK1_CHHM ( -- x addr ) 1 bit OTG_FS_HOST_FS_HCINTMSK1 ; \ OTG_FS_HOST_FS_HCINTMSK1_CHHM, Channel halted mask
: OTG_FS_HOST_FS_HCINTMSK1_STALLM ( -- x addr ) 3 bit OTG_FS_HOST_FS_HCINTMSK1 ; \ OTG_FS_HOST_FS_HCINTMSK1_STALLM, STALL response received interrupt  mask
: OTG_FS_HOST_FS_HCINTMSK1_NAKM ( -- x addr ) 4 bit OTG_FS_HOST_FS_HCINTMSK1 ; \ OTG_FS_HOST_FS_HCINTMSK1_NAKM, NAK response received interrupt  mask
: OTG_FS_HOST_FS_HCINTMSK1_ACKM ( -- x addr ) 5 bit OTG_FS_HOST_FS_HCINTMSK1 ; \ OTG_FS_HOST_FS_HCINTMSK1_ACKM, ACK response received/transmitted  interrupt mask
: OTG_FS_HOST_FS_HCINTMSK1_NYET ( -- x addr ) 6 bit OTG_FS_HOST_FS_HCINTMSK1 ; \ OTG_FS_HOST_FS_HCINTMSK1_NYET, response received interrupt  mask
: OTG_FS_HOST_FS_HCINTMSK1_TXERRM ( -- x addr ) 7 bit OTG_FS_HOST_FS_HCINTMSK1 ; \ OTG_FS_HOST_FS_HCINTMSK1_TXERRM, Transaction error mask
: OTG_FS_HOST_FS_HCINTMSK1_BBERRM ( -- x addr ) 8 bit OTG_FS_HOST_FS_HCINTMSK1 ; \ OTG_FS_HOST_FS_HCINTMSK1_BBERRM, Babble error mask
: OTG_FS_HOST_FS_HCINTMSK1_FRMORM ( -- x addr ) 9 bit OTG_FS_HOST_FS_HCINTMSK1 ; \ OTG_FS_HOST_FS_HCINTMSK1_FRMORM, Frame overrun mask
: OTG_FS_HOST_FS_HCINTMSK1_DTERRM ( -- x addr ) 10 bit OTG_FS_HOST_FS_HCINTMSK1 ; \ OTG_FS_HOST_FS_HCINTMSK1_DTERRM, Data toggle error mask
[then]

execute-defined? use-OTG_FS_HOST defined? OTG_FS_HOST_FS_HCINTMSK2_XFRCM not and [if]
\ OTG_FS_HOST_FS_HCINTMSK2 (read-write) Reset:0x00000000
: OTG_FS_HOST_FS_HCINTMSK2_XFRCM ( -- x addr ) 0 bit OTG_FS_HOST_FS_HCINTMSK2 ; \ OTG_FS_HOST_FS_HCINTMSK2_XFRCM, Transfer completed mask
: OTG_FS_HOST_FS_HCINTMSK2_CHHM ( -- x addr ) 1 bit OTG_FS_HOST_FS_HCINTMSK2 ; \ OTG_FS_HOST_FS_HCINTMSK2_CHHM, Channel halted mask
: OTG_FS_HOST_FS_HCINTMSK2_STALLM ( -- x addr ) 3 bit OTG_FS_HOST_FS_HCINTMSK2 ; \ OTG_FS_HOST_FS_HCINTMSK2_STALLM, STALL response received interrupt  mask
: OTG_FS_HOST_FS_HCINTMSK2_NAKM ( -- x addr ) 4 bit OTG_FS_HOST_FS_HCINTMSK2 ; \ OTG_FS_HOST_FS_HCINTMSK2_NAKM, NAK response received interrupt  mask
: OTG_FS_HOST_FS_HCINTMSK2_ACKM ( -- x addr ) 5 bit OTG_FS_HOST_FS_HCINTMSK2 ; \ OTG_FS_HOST_FS_HCINTMSK2_ACKM, ACK response received/transmitted  interrupt mask
: OTG_FS_HOST_FS_HCINTMSK2_NYET ( -- x addr ) 6 bit OTG_FS_HOST_FS_HCINTMSK2 ; \ OTG_FS_HOST_FS_HCINTMSK2_NYET, response received interrupt  mask
: OTG_FS_HOST_FS_HCINTMSK2_TXERRM ( -- x addr ) 7 bit OTG_FS_HOST_FS_HCINTMSK2 ; \ OTG_FS_HOST_FS_HCINTMSK2_TXERRM, Transaction error mask
: OTG_FS_HOST_FS_HCINTMSK2_BBERRM ( -- x addr ) 8 bit OTG_FS_HOST_FS_HCINTMSK2 ; \ OTG_FS_HOST_FS_HCINTMSK2_BBERRM, Babble error mask
: OTG_FS_HOST_FS_HCINTMSK2_FRMORM ( -- x addr ) 9 bit OTG_FS_HOST_FS_HCINTMSK2 ; \ OTG_FS_HOST_FS_HCINTMSK2_FRMORM, Frame overrun mask
: OTG_FS_HOST_FS_HCINTMSK2_DTERRM ( -- x addr ) 10 bit OTG_FS_HOST_FS_HCINTMSK2 ; \ OTG_FS_HOST_FS_HCINTMSK2_DTERRM, Data toggle error mask
[then]

defined? use-OTG_FS_HOST defined? OTG_FS_HOST_FS_HCINTMSK3_XFRCM not and [if]
\ OTG_FS_HOST_FS_HCINTMSK3 (read-write) Reset:0x00000000
: OTG_FS_HOST_FS_HCINTMSK3_XFRCM ( -- x addr ) 0 bit OTG_FS_HOST_FS_HCINTMSK3 ; \ OTG_FS_HOST_FS_HCINTMSK3_XFRCM, Transfer completed mask
: OTG_FS_HOST_FS_HCINTMSK3_CHHM ( -- x addr ) 1 bit OTG_FS_HOST_FS_HCINTMSK3 ; \ OTG_FS_HOST_FS_HCINTMSK3_CHHM, Channel halted mask
: OTG_FS_HOST_FS_HCINTMSK3_STALLM ( -- x addr ) 3 bit OTG_FS_HOST_FS_HCINTMSK3 ; \ OTG_FS_HOST_FS_HCINTMSK3_STALLM, STALL response received interrupt  mask
: OTG_FS_HOST_FS_HCINTMSK3_NAKM ( -- x addr ) 4 bit OTG_FS_HOST_FS_HCINTMSK3 ; \ OTG_FS_HOST_FS_HCINTMSK3_NAKM, NAK response received interrupt  mask
: OTG_FS_HOST_FS_HCINTMSK3_ACKM ( -- x addr ) 5 bit OTG_FS_HOST_FS_HCINTMSK3 ; \ OTG_FS_HOST_FS_HCINTMSK3_ACKM, ACK response received/transmitted  interrupt mask
: OTG_FS_HOST_FS_HCINTMSK3_NYET ( -- x addr ) 6 bit OTG_FS_HOST_FS_HCINTMSK3 ; \ OTG_FS_HOST_FS_HCINTMSK3_NYET, response received interrupt  mask
: OTG_FS_HOST_FS_HCINTMSK3_TXERRM ( -- x addr ) 7 bit OTG_FS_HOST_FS_HCINTMSK3 ; \ OTG_FS_HOST_FS_HCINTMSK3_TXERRM, Transaction error mask
: OTG_FS_HOST_FS_HCINTMSK3_BBERRM ( -- x addr ) 8 bit OTG_FS_HOST_FS_HCINTMSK3 ; \ OTG_FS_HOST_FS_HCINTMSK3_BBERRM, Babble error mask
: OTG_FS_HOST_FS_HCINTMSK3_FRMORM ( -- x addr ) 9 bit OTG_FS_HOST_FS_HCINTMSK3 ; \ OTG_FS_HOST_FS_HCINTMSK3_FRMORM, Frame overrun mask
: OTG_FS_HOST_FS_HCINTMSK3_DTERRM ( -- x addr ) 10 bit OTG_FS_HOST_FS_HCINTMSK3 ; \ OTG_FS_HOST_FS_HCINTMSK3_DTERRM, Data toggle error mask
[then]

execute-defined? use-OTG_FS_HOST defined? OTG_FS_HOST_FS_HCINTMSK4_XFRCM not and [if]
\ OTG_FS_HOST_FS_HCINTMSK4 (read-write) Reset:0x00000000
: OTG_FS_HOST_FS_HCINTMSK4_XFRCM ( -- x addr ) 0 bit OTG_FS_HOST_FS_HCINTMSK4 ; \ OTG_FS_HOST_FS_HCINTMSK4_XFRCM, Transfer completed mask
: OTG_FS_HOST_FS_HCINTMSK4_CHHM ( -- x addr ) 1 bit OTG_FS_HOST_FS_HCINTMSK4 ; \ OTG_FS_HOST_FS_HCINTMSK4_CHHM, Channel halted mask
: OTG_FS_HOST_FS_HCINTMSK4_STALLM ( -- x addr ) 3 bit OTG_FS_HOST_FS_HCINTMSK4 ; \ OTG_FS_HOST_FS_HCINTMSK4_STALLM, STALL response received interrupt  mask
: OTG_FS_HOST_FS_HCINTMSK4_NAKM ( -- x addr ) 4 bit OTG_FS_HOST_FS_HCINTMSK4 ; \ OTG_FS_HOST_FS_HCINTMSK4_NAKM, NAK response received interrupt  mask
: OTG_FS_HOST_FS_HCINTMSK4_ACKM ( -- x addr ) 5 bit OTG_FS_HOST_FS_HCINTMSK4 ; \ OTG_FS_HOST_FS_HCINTMSK4_ACKM, ACK response received/transmitted  interrupt mask
: OTG_FS_HOST_FS_HCINTMSK4_NYET ( -- x addr ) 6 bit OTG_FS_HOST_FS_HCINTMSK4 ; \ OTG_FS_HOST_FS_HCINTMSK4_NYET, response received interrupt  mask
: OTG_FS_HOST_FS_HCINTMSK4_TXERRM ( -- x addr ) 7 bit OTG_FS_HOST_FS_HCINTMSK4 ; \ OTG_FS_HOST_FS_HCINTMSK4_TXERRM, Transaction error mask
: OTG_FS_HOST_FS_HCINTMSK4_BBERRM ( -- x addr ) 8 bit OTG_FS_HOST_FS_HCINTMSK4 ; \ OTG_FS_HOST_FS_HCINTMSK4_BBERRM, Babble error mask
: OTG_FS_HOST_FS_HCINTMSK4_FRMORM ( -- x addr ) 9 bit OTG_FS_HOST_FS_HCINTMSK4 ; \ OTG_FS_HOST_FS_HCINTMSK4_FRMORM, Frame overrun mask
: OTG_FS_HOST_FS_HCINTMSK4_DTERRM ( -- x addr ) 10 bit OTG_FS_HOST_FS_HCINTMSK4 ; \ OTG_FS_HOST_FS_HCINTMSK4_DTERRM, Data toggle error mask
[then]

defined? use-OTG_FS_HOST defined? OTG_FS_HOST_FS_HCINTMSK5_XFRCM not and [if]
\ OTG_FS_HOST_FS_HCINTMSK5 (read-write) Reset:0x00000000
: OTG_FS_HOST_FS_HCINTMSK5_XFRCM ( -- x addr ) 0 bit OTG_FS_HOST_FS_HCINTMSK5 ; \ OTG_FS_HOST_FS_HCINTMSK5_XFRCM, Transfer completed mask
: OTG_FS_HOST_FS_HCINTMSK5_CHHM ( -- x addr ) 1 bit OTG_FS_HOST_FS_HCINTMSK5 ; \ OTG_FS_HOST_FS_HCINTMSK5_CHHM, Channel halted mask
: OTG_FS_HOST_FS_HCINTMSK5_STALLM ( -- x addr ) 3 bit OTG_FS_HOST_FS_HCINTMSK5 ; \ OTG_FS_HOST_FS_HCINTMSK5_STALLM, STALL response received interrupt  mask
: OTG_FS_HOST_FS_HCINTMSK5_NAKM ( -- x addr ) 4 bit OTG_FS_HOST_FS_HCINTMSK5 ; \ OTG_FS_HOST_FS_HCINTMSK5_NAKM, NAK response received interrupt  mask
: OTG_FS_HOST_FS_HCINTMSK5_ACKM ( -- x addr ) 5 bit OTG_FS_HOST_FS_HCINTMSK5 ; \ OTG_FS_HOST_FS_HCINTMSK5_ACKM, ACK response received/transmitted  interrupt mask
: OTG_FS_HOST_FS_HCINTMSK5_NYET ( -- x addr ) 6 bit OTG_FS_HOST_FS_HCINTMSK5 ; \ OTG_FS_HOST_FS_HCINTMSK5_NYET, response received interrupt  mask
: OTG_FS_HOST_FS_HCINTMSK5_TXERRM ( -- x addr ) 7 bit OTG_FS_HOST_FS_HCINTMSK5 ; \ OTG_FS_HOST_FS_HCINTMSK5_TXERRM, Transaction error mask
: OTG_FS_HOST_FS_HCINTMSK5_BBERRM ( -- x addr ) 8 bit OTG_FS_HOST_FS_HCINTMSK5 ; \ OTG_FS_HOST_FS_HCINTMSK5_BBERRM, Babble error mask
: OTG_FS_HOST_FS_HCINTMSK5_FRMORM ( -- x addr ) 9 bit OTG_FS_HOST_FS_HCINTMSK5 ; \ OTG_FS_HOST_FS_HCINTMSK5_FRMORM, Frame overrun mask
: OTG_FS_HOST_FS_HCINTMSK5_DTERRM ( -- x addr ) 10 bit OTG_FS_HOST_FS_HCINTMSK5 ; \ OTG_FS_HOST_FS_HCINTMSK5_DTERRM, Data toggle error mask
[then]

execute-defined? use-OTG_FS_HOST defined? OTG_FS_HOST_FS_HCINTMSK6_XFRCM not and [if]
\ OTG_FS_HOST_FS_HCINTMSK6 (read-write) Reset:0x00000000
: OTG_FS_HOST_FS_HCINTMSK6_XFRCM ( -- x addr ) 0 bit OTG_FS_HOST_FS_HCINTMSK6 ; \ OTG_FS_HOST_FS_HCINTMSK6_XFRCM, Transfer completed mask
: OTG_FS_HOST_FS_HCINTMSK6_CHHM ( -- x addr ) 1 bit OTG_FS_HOST_FS_HCINTMSK6 ; \ OTG_FS_HOST_FS_HCINTMSK6_CHHM, Channel halted mask
: OTG_FS_HOST_FS_HCINTMSK6_STALLM ( -- x addr ) 3 bit OTG_FS_HOST_FS_HCINTMSK6 ; \ OTG_FS_HOST_FS_HCINTMSK6_STALLM, STALL response received interrupt  mask
: OTG_FS_HOST_FS_HCINTMSK6_NAKM ( -- x addr ) 4 bit OTG_FS_HOST_FS_HCINTMSK6 ; \ OTG_FS_HOST_FS_HCINTMSK6_NAKM, NAK response received interrupt  mask
: OTG_FS_HOST_FS_HCINTMSK6_ACKM ( -- x addr ) 5 bit OTG_FS_HOST_FS_HCINTMSK6 ; \ OTG_FS_HOST_FS_HCINTMSK6_ACKM, ACK response received/transmitted  interrupt mask
: OTG_FS_HOST_FS_HCINTMSK6_NYET ( -- x addr ) 6 bit OTG_FS_HOST_FS_HCINTMSK6 ; \ OTG_FS_HOST_FS_HCINTMSK6_NYET, response received interrupt  mask
: OTG_FS_HOST_FS_HCINTMSK6_TXERRM ( -- x addr ) 7 bit OTG_FS_HOST_FS_HCINTMSK6 ; \ OTG_FS_HOST_FS_HCINTMSK6_TXERRM, Transaction error mask
: OTG_FS_HOST_FS_HCINTMSK6_BBERRM ( -- x addr ) 8 bit OTG_FS_HOST_FS_HCINTMSK6 ; \ OTG_FS_HOST_FS_HCINTMSK6_BBERRM, Babble error mask
: OTG_FS_HOST_FS_HCINTMSK6_FRMORM ( -- x addr ) 9 bit OTG_FS_HOST_FS_HCINTMSK6 ; \ OTG_FS_HOST_FS_HCINTMSK6_FRMORM, Frame overrun mask
: OTG_FS_HOST_FS_HCINTMSK6_DTERRM ( -- x addr ) 10 bit OTG_FS_HOST_FS_HCINTMSK6 ; \ OTG_FS_HOST_FS_HCINTMSK6_DTERRM, Data toggle error mask
[then]

defined? use-OTG_FS_HOST defined? OTG_FS_HOST_FS_HCINTMSK7_XFRCM not and [if]
\ OTG_FS_HOST_FS_HCINTMSK7 (read-write) Reset:0x00000000
: OTG_FS_HOST_FS_HCINTMSK7_XFRCM ( -- x addr ) 0 bit OTG_FS_HOST_FS_HCINTMSK7 ; \ OTG_FS_HOST_FS_HCINTMSK7_XFRCM, Transfer completed mask
: OTG_FS_HOST_FS_HCINTMSK7_CHHM ( -- x addr ) 1 bit OTG_FS_HOST_FS_HCINTMSK7 ; \ OTG_FS_HOST_FS_HCINTMSK7_CHHM, Channel halted mask
: OTG_FS_HOST_FS_HCINTMSK7_STALLM ( -- x addr ) 3 bit OTG_FS_HOST_FS_HCINTMSK7 ; \ OTG_FS_HOST_FS_HCINTMSK7_STALLM, STALL response received interrupt  mask
: OTG_FS_HOST_FS_HCINTMSK7_NAKM ( -- x addr ) 4 bit OTG_FS_HOST_FS_HCINTMSK7 ; \ OTG_FS_HOST_FS_HCINTMSK7_NAKM, NAK response received interrupt  mask
: OTG_FS_HOST_FS_HCINTMSK7_ACKM ( -- x addr ) 5 bit OTG_FS_HOST_FS_HCINTMSK7 ; \ OTG_FS_HOST_FS_HCINTMSK7_ACKM, ACK response received/transmitted  interrupt mask
: OTG_FS_HOST_FS_HCINTMSK7_NYET ( -- x addr ) 6 bit OTG_FS_HOST_FS_HCINTMSK7 ; \ OTG_FS_HOST_FS_HCINTMSK7_NYET, response received interrupt  mask
: OTG_FS_HOST_FS_HCINTMSK7_TXERRM ( -- x addr ) 7 bit OTG_FS_HOST_FS_HCINTMSK7 ; \ OTG_FS_HOST_FS_HCINTMSK7_TXERRM, Transaction error mask
: OTG_FS_HOST_FS_HCINTMSK7_BBERRM ( -- x addr ) 8 bit OTG_FS_HOST_FS_HCINTMSK7 ; \ OTG_FS_HOST_FS_HCINTMSK7_BBERRM, Babble error mask
: OTG_FS_HOST_FS_HCINTMSK7_FRMORM ( -- x addr ) 9 bit OTG_FS_HOST_FS_HCINTMSK7 ; \ OTG_FS_HOST_FS_HCINTMSK7_FRMORM, Frame overrun mask
: OTG_FS_HOST_FS_HCINTMSK7_DTERRM ( -- x addr ) 10 bit OTG_FS_HOST_FS_HCINTMSK7 ; \ OTG_FS_HOST_FS_HCINTMSK7_DTERRM, Data toggle error mask
[then]

execute-defined? use-OTG_FS_HOST defined? OTG_FS_HOST_FS_HCTSIZ0_XFRSIZ not and [if]
\ OTG_FS_HOST_FS_HCTSIZ0 (read-write) Reset:0x00000000
: OTG_FS_HOST_FS_HCTSIZ0_XFRSIZ x addr ) OTG_FS_HOST_FS_HCTSIZ0 ; \ OTG_FS_HOST_FS_HCTSIZ0_XFRSIZ, Transfer size
: OTG_FS_HOST_FS_HCTSIZ0_PKTCNT ( %bbbbbbbbbb -- x addr ) 19 lshift OTG_FS_HOST_FS_HCTSIZ0 ; \ OTG_FS_HOST_FS_HCTSIZ0_PKTCNT, Packet count
: OTG_FS_HOST_FS_HCTSIZ0_DPID ( %bb -- x addr ) 29 lshift OTG_FS_HOST_FS_HCTSIZ0 ; \ OTG_FS_HOST_FS_HCTSIZ0_DPID, Data PID
[then]

defined? use-OTG_FS_HOST defined? OTG_FS_HOST_FS_HCTSIZ1_XFRSIZ not and [if]
\ OTG_FS_HOST_FS_HCTSIZ1 (read-write) Reset:0x00000000
: OTG_FS_HOST_FS_HCTSIZ1_XFRSIZ x addr ) OTG_FS_HOST_FS_HCTSIZ1 ; \ OTG_FS_HOST_FS_HCTSIZ1_XFRSIZ, Transfer size
: OTG_FS_HOST_FS_HCTSIZ1_PKTCNT ( %bbbbbbbbbb -- x addr ) 19 lshift OTG_FS_HOST_FS_HCTSIZ1 ; \ OTG_FS_HOST_FS_HCTSIZ1_PKTCNT, Packet count
: OTG_FS_HOST_FS_HCTSIZ1_DPID ( %bb -- x addr ) 29 lshift OTG_FS_HOST_FS_HCTSIZ1 ; \ OTG_FS_HOST_FS_HCTSIZ1_DPID, Data PID
[then]

execute-defined? use-OTG_FS_HOST defined? OTG_FS_HOST_FS_HCTSIZ2_XFRSIZ not and [if]
\ OTG_FS_HOST_FS_HCTSIZ2 (read-write) Reset:0x00000000
: OTG_FS_HOST_FS_HCTSIZ2_XFRSIZ x addr ) OTG_FS_HOST_FS_HCTSIZ2 ; \ OTG_FS_HOST_FS_HCTSIZ2_XFRSIZ, Transfer size
: OTG_FS_HOST_FS_HCTSIZ2_PKTCNT ( %bbbbbbbbbb -- x addr ) 19 lshift OTG_FS_HOST_FS_HCTSIZ2 ; \ OTG_FS_HOST_FS_HCTSIZ2_PKTCNT, Packet count
: OTG_FS_HOST_FS_HCTSIZ2_DPID ( %bb -- x addr ) 29 lshift OTG_FS_HOST_FS_HCTSIZ2 ; \ OTG_FS_HOST_FS_HCTSIZ2_DPID, Data PID
[then]

defined? use-OTG_FS_HOST defined? OTG_FS_HOST_FS_HCTSIZ3_XFRSIZ not and [if]
\ OTG_FS_HOST_FS_HCTSIZ3 (read-write) Reset:0x00000000
: OTG_FS_HOST_FS_HCTSIZ3_XFRSIZ x addr ) OTG_FS_HOST_FS_HCTSIZ3 ; \ OTG_FS_HOST_FS_HCTSIZ3_XFRSIZ, Transfer size
: OTG_FS_HOST_FS_HCTSIZ3_PKTCNT ( %bbbbbbbbbb -- x addr ) 19 lshift OTG_FS_HOST_FS_HCTSIZ3 ; \ OTG_FS_HOST_FS_HCTSIZ3_PKTCNT, Packet count
: OTG_FS_HOST_FS_HCTSIZ3_DPID ( %bb -- x addr ) 29 lshift OTG_FS_HOST_FS_HCTSIZ3 ; \ OTG_FS_HOST_FS_HCTSIZ3_DPID, Data PID
[then]

execute-defined? use-OTG_FS_HOST defined? OTG_FS_HOST_FS_HCTSIZ4_XFRSIZ not and [if]
\ OTG_FS_HOST_FS_HCTSIZ4 (read-write) Reset:0x00000000
: OTG_FS_HOST_FS_HCTSIZ4_XFRSIZ x addr ) OTG_FS_HOST_FS_HCTSIZ4 ; \ OTG_FS_HOST_FS_HCTSIZ4_XFRSIZ, Transfer size
: OTG_FS_HOST_FS_HCTSIZ4_PKTCNT ( %bbbbbbbbbb -- x addr ) 19 lshift OTG_FS_HOST_FS_HCTSIZ4 ; \ OTG_FS_HOST_FS_HCTSIZ4_PKTCNT, Packet count
: OTG_FS_HOST_FS_HCTSIZ4_DPID ( %bb -- x addr ) 29 lshift OTG_FS_HOST_FS_HCTSIZ4 ; \ OTG_FS_HOST_FS_HCTSIZ4_DPID, Data PID
[then]

defined? use-OTG_FS_HOST defined? OTG_FS_HOST_FS_HCTSIZ5_XFRSIZ not and [if]
\ OTG_FS_HOST_FS_HCTSIZ5 (read-write) Reset:0x00000000
: OTG_FS_HOST_FS_HCTSIZ5_XFRSIZ x addr ) OTG_FS_HOST_FS_HCTSIZ5 ; \ OTG_FS_HOST_FS_HCTSIZ5_XFRSIZ, Transfer size
: OTG_FS_HOST_FS_HCTSIZ5_PKTCNT ( %bbbbbbbbbb -- x addr ) 19 lshift OTG_FS_HOST_FS_HCTSIZ5 ; \ OTG_FS_HOST_FS_HCTSIZ5_PKTCNT, Packet count
: OTG_FS_HOST_FS_HCTSIZ5_DPID ( %bb -- x addr ) 29 lshift OTG_FS_HOST_FS_HCTSIZ5 ; \ OTG_FS_HOST_FS_HCTSIZ5_DPID, Data PID
[then]

execute-defined? use-OTG_FS_HOST defined? OTG_FS_HOST_FS_HCTSIZ6_XFRSIZ not and [if]
\ OTG_FS_HOST_FS_HCTSIZ6 (read-write) Reset:0x00000000
: OTG_FS_HOST_FS_HCTSIZ6_XFRSIZ x addr ) OTG_FS_HOST_FS_HCTSIZ6 ; \ OTG_FS_HOST_FS_HCTSIZ6_XFRSIZ, Transfer size
: OTG_FS_HOST_FS_HCTSIZ6_PKTCNT ( %bbbbbbbbbb -- x addr ) 19 lshift OTG_FS_HOST_FS_HCTSIZ6 ; \ OTG_FS_HOST_FS_HCTSIZ6_PKTCNT, Packet count
: OTG_FS_HOST_FS_HCTSIZ6_DPID ( %bb -- x addr ) 29 lshift OTG_FS_HOST_FS_HCTSIZ6 ; \ OTG_FS_HOST_FS_HCTSIZ6_DPID, Data PID
[then]

defined? use-OTG_FS_HOST defined? OTG_FS_HOST_FS_HCTSIZ7_XFRSIZ not and [if]
\ OTG_FS_HOST_FS_HCTSIZ7 (read-write) Reset:0x00000000
: OTG_FS_HOST_FS_HCTSIZ7_XFRSIZ x addr ) OTG_FS_HOST_FS_HCTSIZ7 ; \ OTG_FS_HOST_FS_HCTSIZ7_XFRSIZ, Transfer size
: OTG_FS_HOST_FS_HCTSIZ7_PKTCNT ( %bbbbbbbbbb -- x addr ) 19 lshift OTG_FS_HOST_FS_HCTSIZ7 ; \ OTG_FS_HOST_FS_HCTSIZ7_PKTCNT, Packet count
: OTG_FS_HOST_FS_HCTSIZ7_DPID ( %bb -- x addr ) 29 lshift OTG_FS_HOST_FS_HCTSIZ7 ; \ OTG_FS_HOST_FS_HCTSIZ7_DPID, Data PID
[then]

execute-defined? use-OTG_FS_DEVICE defined? OTG_FS_DEVICE_FS_DCFG_DSPD not and [if]
\ OTG_FS_DEVICE_FS_DCFG (read-write) Reset:0x02200000
: OTG_FS_DEVICE_FS_DCFG_DSPD ( %bb -- x addr ) OTG_FS_DEVICE_FS_DCFG ; \ OTG_FS_DEVICE_FS_DCFG_DSPD, Device speed
: OTG_FS_DEVICE_FS_DCFG_NZLSOHSK ( -- x addr ) 2 bit OTG_FS_DEVICE_FS_DCFG ; \ OTG_FS_DEVICE_FS_DCFG_NZLSOHSK, Non-zero-length status OUT  handshake
: OTG_FS_DEVICE_FS_DCFG_DAD ( %bbbbbbb -- x addr ) 4 lshift OTG_FS_DEVICE_FS_DCFG ; \ OTG_FS_DEVICE_FS_DCFG_DAD, Device address
: OTG_FS_DEVICE_FS_DCFG_PFIVL ( %bb -- x addr ) 11 lshift OTG_FS_DEVICE_FS_DCFG ; \ OTG_FS_DEVICE_FS_DCFG_PFIVL, Periodic frame interval
[then]

defined? use-OTG_FS_DEVICE defined? OTG_FS_DEVICE_FS_DCTL_RWUSIG not and [if]
\ OTG_FS_DEVICE_FS_DCTL (multiple-access)  Reset:0x00000000
: OTG_FS_DEVICE_FS_DCTL_RWUSIG ( -- x addr ) 0 bit OTG_FS_DEVICE_FS_DCTL ; \ OTG_FS_DEVICE_FS_DCTL_RWUSIG, Remote wakeup signaling
: OTG_FS_DEVICE_FS_DCTL_SDIS ( -- x addr ) 1 bit OTG_FS_DEVICE_FS_DCTL ; \ OTG_FS_DEVICE_FS_DCTL_SDIS, Soft disconnect
: OTG_FS_DEVICE_FS_DCTL_GINSTS? ( -- 1|0 ) 2 bit OTG_FS_DEVICE_FS_DCTL bit@ ; \ OTG_FS_DEVICE_FS_DCTL_GINSTS, Global IN NAK status
: OTG_FS_DEVICE_FS_DCTL_GONSTS? ( -- 1|0 ) 3 bit OTG_FS_DEVICE_FS_DCTL bit@ ; \ OTG_FS_DEVICE_FS_DCTL_GONSTS, Global OUT NAK status
: OTG_FS_DEVICE_FS_DCTL_TCTL ( %bbb -- x addr ) 4 lshift OTG_FS_DEVICE_FS_DCTL ; \ OTG_FS_DEVICE_FS_DCTL_TCTL, Test control
: OTG_FS_DEVICE_FS_DCTL_SGINAK ( -- x addr ) 7 bit OTG_FS_DEVICE_FS_DCTL ; \ OTG_FS_DEVICE_FS_DCTL_SGINAK, Set global IN NAK
: OTG_FS_DEVICE_FS_DCTL_CGINAK ( -- x addr ) 8 bit OTG_FS_DEVICE_FS_DCTL ; \ OTG_FS_DEVICE_FS_DCTL_CGINAK, Clear global IN NAK
: OTG_FS_DEVICE_FS_DCTL_SGONAK ( -- x addr ) 9 bit OTG_FS_DEVICE_FS_DCTL ; \ OTG_FS_DEVICE_FS_DCTL_SGONAK, Set global OUT NAK
: OTG_FS_DEVICE_FS_DCTL_CGONAK ( -- x addr ) 10 bit OTG_FS_DEVICE_FS_DCTL ; \ OTG_FS_DEVICE_FS_DCTL_CGONAK, Clear global OUT NAK
: OTG_FS_DEVICE_FS_DCTL_POPRGDNE ( -- x addr ) 11 bit OTG_FS_DEVICE_FS_DCTL ; \ OTG_FS_DEVICE_FS_DCTL_POPRGDNE, Power-on programming done
[then]

execute-defined? use-OTG_FS_DEVICE defined? OTG_FS_DEVICE_FS_DSTS_SUSPSTS? not and [if]
\ OTG_FS_DEVICE_FS_DSTS (read-only) Reset:0x00000010
: OTG_FS_DEVICE_FS_DSTS_SUSPSTS? ( --  1|0 ) 0 bit OTG_FS_DEVICE_FS_DSTS bit@ ; \ OTG_FS_DEVICE_FS_DSTS_SUSPSTS, Suspend status
: OTG_FS_DEVICE_FS_DSTS_ENUMSPD? ( --  x ) 1 lshift OTG_FS_DEVICE_FS_DSTS @ ; \ OTG_FS_DEVICE_FS_DSTS_ENUMSPD, Enumerated speed
: OTG_FS_DEVICE_FS_DSTS_EERR? ( --  1|0 ) 3 bit OTG_FS_DEVICE_FS_DSTS bit@ ; \ OTG_FS_DEVICE_FS_DSTS_EERR, Erratic error
: OTG_FS_DEVICE_FS_DSTS_FNSOF? ( --  x ) 8 lshift OTG_FS_DEVICE_FS_DSTS @ ; \ OTG_FS_DEVICE_FS_DSTS_FNSOF, Frame number of the received  SOF
[then]

defined? use-OTG_FS_DEVICE defined? OTG_FS_DEVICE_FS_DIEPMSK_XFRCM not and [if]
\ OTG_FS_DEVICE_FS_DIEPMSK (read-write) Reset:0x00000000
: OTG_FS_DEVICE_FS_DIEPMSK_XFRCM ( -- x addr ) 0 bit OTG_FS_DEVICE_FS_DIEPMSK ; \ OTG_FS_DEVICE_FS_DIEPMSK_XFRCM, Transfer completed interrupt  mask
: OTG_FS_DEVICE_FS_DIEPMSK_EPDM ( -- x addr ) 1 bit OTG_FS_DEVICE_FS_DIEPMSK ; \ OTG_FS_DEVICE_FS_DIEPMSK_EPDM, Endpoint disabled interrupt  mask
: OTG_FS_DEVICE_FS_DIEPMSK_TOM ( -- x addr ) 3 bit OTG_FS_DEVICE_FS_DIEPMSK ; \ OTG_FS_DEVICE_FS_DIEPMSK_TOM, Timeout condition mask Non-isochronous  endpoints
: OTG_FS_DEVICE_FS_DIEPMSK_ITTXFEMSK ( -- x addr ) 4 bit OTG_FS_DEVICE_FS_DIEPMSK ; \ OTG_FS_DEVICE_FS_DIEPMSK_ITTXFEMSK, IN token received when TxFIFO empty  mask
: OTG_FS_DEVICE_FS_DIEPMSK_INEPNMM ( -- x addr ) 5 bit OTG_FS_DEVICE_FS_DIEPMSK ; \ OTG_FS_DEVICE_FS_DIEPMSK_INEPNMM, IN token received with EP mismatch  mask
: OTG_FS_DEVICE_FS_DIEPMSK_INEPNEM ( -- x addr ) 6 bit OTG_FS_DEVICE_FS_DIEPMSK ; \ OTG_FS_DEVICE_FS_DIEPMSK_INEPNEM, IN endpoint NAK effective  mask
[then]

execute-defined? use-OTG_FS_DEVICE defined? OTG_FS_DEVICE_FS_DOEPMSK_XFRCM not and [if]
\ OTG_FS_DEVICE_FS_DOEPMSK (read-write) Reset:0x00000000
: OTG_FS_DEVICE_FS_DOEPMSK_XFRCM ( -- x addr ) 0 bit OTG_FS_DEVICE_FS_DOEPMSK ; \ OTG_FS_DEVICE_FS_DOEPMSK_XFRCM, Transfer completed interrupt  mask
: OTG_FS_DEVICE_FS_DOEPMSK_EPDM ( -- x addr ) 1 bit OTG_FS_DEVICE_FS_DOEPMSK ; \ OTG_FS_DEVICE_FS_DOEPMSK_EPDM, Endpoint disabled interrupt  mask
: OTG_FS_DEVICE_FS_DOEPMSK_STUPM ( -- x addr ) 3 bit OTG_FS_DEVICE_FS_DOEPMSK ; \ OTG_FS_DEVICE_FS_DOEPMSK_STUPM, SETUP phase done mask
: OTG_FS_DEVICE_FS_DOEPMSK_OTEPDM ( -- x addr ) 4 bit OTG_FS_DEVICE_FS_DOEPMSK ; \ OTG_FS_DEVICE_FS_DOEPMSK_OTEPDM, OUT token received when endpoint  disabled mask
[then]

defined? use-OTG_FS_DEVICE defined? OTG_FS_DEVICE_FS_DAINT_IEPINT? not and [if]
\ OTG_FS_DEVICE_FS_DAINT (read-only) Reset:0x00000000
: OTG_FS_DEVICE_FS_DAINT_IEPINT? ( --  x ) OTG_FS_DEVICE_FS_DAINT @ ; \ OTG_FS_DEVICE_FS_DAINT_IEPINT, IN endpoint interrupt bits
: OTG_FS_DEVICE_FS_DAINT_OEPINT? ( --  x ) 16 lshift OTG_FS_DEVICE_FS_DAINT @ ; \ OTG_FS_DEVICE_FS_DAINT_OEPINT, OUT endpoint interrupt  bits
[then]

execute-defined? use-OTG_FS_DEVICE defined? OTG_FS_DEVICE_FS_DAINTMSK_IEPM not and [if]
\ OTG_FS_DEVICE_FS_DAINTMSK (read-write) Reset:0x00000000
: OTG_FS_DEVICE_FS_DAINTMSK_IEPM ( %bbbbbbbbbbbbbbbb -- x addr ) OTG_FS_DEVICE_FS_DAINTMSK ; \ OTG_FS_DEVICE_FS_DAINTMSK_IEPM, IN EP interrupt mask bits
: OTG_FS_DEVICE_FS_DAINTMSK_OEPINT ( %bbbbbbbbbbbbbbbb -- x addr ) 16 lshift OTG_FS_DEVICE_FS_DAINTMSK ; \ OTG_FS_DEVICE_FS_DAINTMSK_OEPINT, OUT endpoint interrupt  bits
[then]

defined? use-OTG_FS_DEVICE defined? OTG_FS_DEVICE_DVBUSDIS_VBUSDT not and [if]
\ OTG_FS_DEVICE_DVBUSDIS (read-write) Reset:0x000017D7
: OTG_FS_DEVICE_DVBUSDIS_VBUSDT ( %bbbbbbbbbbbbbbbb -- x addr ) OTG_FS_DEVICE_DVBUSDIS ; \ OTG_FS_DEVICE_DVBUSDIS_VBUSDT, Device VBUS discharge time
[then]

execute-defined? use-OTG_FS_DEVICE defined? OTG_FS_DEVICE_DVBUSPULSE_DVBUSP not and [if]
\ OTG_FS_DEVICE_DVBUSPULSE (read-write) Reset:0x000005B8
: OTG_FS_DEVICE_DVBUSPULSE_DVBUSP ( %bbbbbbbbbbb -- x addr ) OTG_FS_DEVICE_DVBUSPULSE ; \ OTG_FS_DEVICE_DVBUSPULSE_DVBUSP, Device VBUS pulsing time
[then]

defined? use-OTG_FS_DEVICE defined? OTG_FS_DEVICE_DIEPEMPMSK_INEPTXFEM not and [if]
\ OTG_FS_DEVICE_DIEPEMPMSK (read-write) Reset:0x00000000
: OTG_FS_DEVICE_DIEPEMPMSK_INEPTXFEM ( %bbbbbbbbbbbbbbbb -- x addr ) OTG_FS_DEVICE_DIEPEMPMSK ; \ OTG_FS_DEVICE_DIEPEMPMSK_INEPTXFEM, IN EP Tx FIFO empty interrupt mask  bits
[then]

execute-defined? use-OTG_FS_DEVICE defined? OTG_FS_DEVICE_FS_DIEPCTL0_MPSIZ not and [if]
\ OTG_FS_DEVICE_FS_DIEPCTL0 (multiple-access)  Reset:0x00000000
: OTG_FS_DEVICE_FS_DIEPCTL0_MPSIZ ( %bb -- x addr ) OTG_FS_DEVICE_FS_DIEPCTL0 ; \ OTG_FS_DEVICE_FS_DIEPCTL0_MPSIZ, Maximum packet size
: OTG_FS_DEVICE_FS_DIEPCTL0_USBAEP ( -- x addr ) 15 bit OTG_FS_DEVICE_FS_DIEPCTL0 ; \ OTG_FS_DEVICE_FS_DIEPCTL0_USBAEP, USB active endpoint
: OTG_FS_DEVICE_FS_DIEPCTL0_NAKSTS? ( -- 1|0 ) 17 bit OTG_FS_DEVICE_FS_DIEPCTL0 bit@ ; \ OTG_FS_DEVICE_FS_DIEPCTL0_NAKSTS, NAK status
: OTG_FS_DEVICE_FS_DIEPCTL0_EPTYP ( %bb -- x addr ) 18 lshift OTG_FS_DEVICE_FS_DIEPCTL0 ; \ OTG_FS_DEVICE_FS_DIEPCTL0_EPTYP, Endpoint type
: OTG_FS_DEVICE_FS_DIEPCTL0_STALL ( -- x addr ) 21 bit OTG_FS_DEVICE_FS_DIEPCTL0 ; \ OTG_FS_DEVICE_FS_DIEPCTL0_STALL, STALL handshake
: OTG_FS_DEVICE_FS_DIEPCTL0_TXFNUM ( %bbbb -- x addr ) 22 lshift OTG_FS_DEVICE_FS_DIEPCTL0 ; \ OTG_FS_DEVICE_FS_DIEPCTL0_TXFNUM, TxFIFO number
: OTG_FS_DEVICE_FS_DIEPCTL0_CNAK ( -- x addr ) 26 bit OTG_FS_DEVICE_FS_DIEPCTL0 ; \ OTG_FS_DEVICE_FS_DIEPCTL0_CNAK, Clear NAK
: OTG_FS_DEVICE_FS_DIEPCTL0_SNAK ( -- x addr ) 27 bit OTG_FS_DEVICE_FS_DIEPCTL0 ; \ OTG_FS_DEVICE_FS_DIEPCTL0_SNAK, Set NAK
: OTG_FS_DEVICE_FS_DIEPCTL0_EPDIS ( -- x addr ) 30 bit OTG_FS_DEVICE_FS_DIEPCTL0 ; \ OTG_FS_DEVICE_FS_DIEPCTL0_EPDIS, Endpoint disable
: OTG_FS_DEVICE_FS_DIEPCTL0_EPENA ( -- x addr ) 31 bit OTG_FS_DEVICE_FS_DIEPCTL0 ; \ OTG_FS_DEVICE_FS_DIEPCTL0_EPENA, Endpoint enable
[then]

defined? use-OTG_FS_DEVICE defined? OTG_FS_DEVICE_DIEPCTL1_EPENA not and [if]
\ OTG_FS_DEVICE_DIEPCTL1 (multiple-access)  Reset:0x00000000
: OTG_FS_DEVICE_DIEPCTL1_EPENA ( -- x addr ) 31 bit OTG_FS_DEVICE_DIEPCTL1 ; \ OTG_FS_DEVICE_DIEPCTL1_EPENA, EPENA
: OTG_FS_DEVICE_DIEPCTL1_EPDIS ( -- x addr ) 30 bit OTG_FS_DEVICE_DIEPCTL1 ; \ OTG_FS_DEVICE_DIEPCTL1_EPDIS, EPDIS
: OTG_FS_DEVICE_DIEPCTL1_SODDFRM_SD1PID ( -- x addr ) 29 bit OTG_FS_DEVICE_DIEPCTL1 ; \ OTG_FS_DEVICE_DIEPCTL1_SODDFRM_SD1PID, SODDFRM/SD1PID
: OTG_FS_DEVICE_DIEPCTL1_SD0PID_SEVNFRM ( -- x addr ) 28 bit OTG_FS_DEVICE_DIEPCTL1 ; \ OTG_FS_DEVICE_DIEPCTL1_SD0PID_SEVNFRM, SD0PID/SEVNFRM
: OTG_FS_DEVICE_DIEPCTL1_SNAK ( -- x addr ) 27 bit OTG_FS_DEVICE_DIEPCTL1 ; \ OTG_FS_DEVICE_DIEPCTL1_SNAK, SNAK
: OTG_FS_DEVICE_DIEPCTL1_CNAK ( -- x addr ) 26 bit OTG_FS_DEVICE_DIEPCTL1 ; \ OTG_FS_DEVICE_DIEPCTL1_CNAK, CNAK
: OTG_FS_DEVICE_DIEPCTL1_TXFNUM ( %bbbb -- x addr ) 22 lshift OTG_FS_DEVICE_DIEPCTL1 ; \ OTG_FS_DEVICE_DIEPCTL1_TXFNUM, TXFNUM
: OTG_FS_DEVICE_DIEPCTL1_Stall ( -- x addr ) 21 bit OTG_FS_DEVICE_DIEPCTL1 ; \ OTG_FS_DEVICE_DIEPCTL1_Stall, Stall
: OTG_FS_DEVICE_DIEPCTL1_EPTYP ( %bb -- x addr ) 18 lshift OTG_FS_DEVICE_DIEPCTL1 ; \ OTG_FS_DEVICE_DIEPCTL1_EPTYP, EPTYP
: OTG_FS_DEVICE_DIEPCTL1_NAKSTS ( -- x addr ) 17 bit OTG_FS_DEVICE_DIEPCTL1 ; \ OTG_FS_DEVICE_DIEPCTL1_NAKSTS, NAKSTS
: OTG_FS_DEVICE_DIEPCTL1_EONUM_DPID ( -- x addr ) 16 bit OTG_FS_DEVICE_DIEPCTL1 ; \ OTG_FS_DEVICE_DIEPCTL1_EONUM_DPID, EONUM/DPID
: OTG_FS_DEVICE_DIEPCTL1_USBAEP ( -- x addr ) 15 bit OTG_FS_DEVICE_DIEPCTL1 ; \ OTG_FS_DEVICE_DIEPCTL1_USBAEP, USBAEP
: OTG_FS_DEVICE_DIEPCTL1_MPSIZ x addr ) OTG_FS_DEVICE_DIEPCTL1 ; \ OTG_FS_DEVICE_DIEPCTL1_MPSIZ, MPSIZ
[then]

execute-defined? use-OTG_FS_DEVICE defined? OTG_FS_DEVICE_DIEPCTL2_EPENA not and [if]
\ OTG_FS_DEVICE_DIEPCTL2 (multiple-access)  Reset:0x00000000
: OTG_FS_DEVICE_DIEPCTL2_EPENA ( -- x addr ) 31 bit OTG_FS_DEVICE_DIEPCTL2 ; \ OTG_FS_DEVICE_DIEPCTL2_EPENA, EPENA
: OTG_FS_DEVICE_DIEPCTL2_EPDIS ( -- x addr ) 30 bit OTG_FS_DEVICE_DIEPCTL2 ; \ OTG_FS_DEVICE_DIEPCTL2_EPDIS, EPDIS
: OTG_FS_DEVICE_DIEPCTL2_SODDFRM ( -- x addr ) 29 bit OTG_FS_DEVICE_DIEPCTL2 ; \ OTG_FS_DEVICE_DIEPCTL2_SODDFRM, SODDFRM
: OTG_FS_DEVICE_DIEPCTL2_SD0PID_SEVNFRM ( -- x addr ) 28 bit OTG_FS_DEVICE_DIEPCTL2 ; \ OTG_FS_DEVICE_DIEPCTL2_SD0PID_SEVNFRM, SD0PID/SEVNFRM
: OTG_FS_DEVICE_DIEPCTL2_SNAK ( -- x addr ) 27 bit OTG_FS_DEVICE_DIEPCTL2 ; \ OTG_FS_DEVICE_DIEPCTL2_SNAK, SNAK
: OTG_FS_DEVICE_DIEPCTL2_CNAK ( -- x addr ) 26 bit OTG_FS_DEVICE_DIEPCTL2 ; \ OTG_FS_DEVICE_DIEPCTL2_CNAK, CNAK
: OTG_FS_DEVICE_DIEPCTL2_TXFNUM ( %bbbb -- x addr ) 22 lshift OTG_FS_DEVICE_DIEPCTL2 ; \ OTG_FS_DEVICE_DIEPCTL2_TXFNUM, TXFNUM
: OTG_FS_DEVICE_DIEPCTL2_Stall ( -- x addr ) 21 bit OTG_FS_DEVICE_DIEPCTL2 ; \ OTG_FS_DEVICE_DIEPCTL2_Stall, Stall
: OTG_FS_DEVICE_DIEPCTL2_EPTYP ( %bb -- x addr ) 18 lshift OTG_FS_DEVICE_DIEPCTL2 ; \ OTG_FS_DEVICE_DIEPCTL2_EPTYP, EPTYP
: OTG_FS_DEVICE_DIEPCTL2_NAKSTS ( -- x addr ) 17 bit OTG_FS_DEVICE_DIEPCTL2 ; \ OTG_FS_DEVICE_DIEPCTL2_NAKSTS, NAKSTS
: OTG_FS_DEVICE_DIEPCTL2_EONUM_DPID ( -- x addr ) 16 bit OTG_FS_DEVICE_DIEPCTL2 ; \ OTG_FS_DEVICE_DIEPCTL2_EONUM_DPID, EONUM/DPID
: OTG_FS_DEVICE_DIEPCTL2_USBAEP ( -- x addr ) 15 bit OTG_FS_DEVICE_DIEPCTL2 ; \ OTG_FS_DEVICE_DIEPCTL2_USBAEP, USBAEP
: OTG_FS_DEVICE_DIEPCTL2_MPSIZ x addr ) OTG_FS_DEVICE_DIEPCTL2 ; \ OTG_FS_DEVICE_DIEPCTL2_MPSIZ, MPSIZ
[then]

defined? use-OTG_FS_DEVICE defined? OTG_FS_DEVICE_DIEPCTL3_EPENA not and [if]
\ OTG_FS_DEVICE_DIEPCTL3 (multiple-access)  Reset:0x00000000
: OTG_FS_DEVICE_DIEPCTL3_EPENA ( -- x addr ) 31 bit OTG_FS_DEVICE_DIEPCTL3 ; \ OTG_FS_DEVICE_DIEPCTL3_EPENA, EPENA
: OTG_FS_DEVICE_DIEPCTL3_EPDIS ( -- x addr ) 30 bit OTG_FS_DEVICE_DIEPCTL3 ; \ OTG_FS_DEVICE_DIEPCTL3_EPDIS, EPDIS
: OTG_FS_DEVICE_DIEPCTL3_SODDFRM ( -- x addr ) 29 bit OTG_FS_DEVICE_DIEPCTL3 ; \ OTG_FS_DEVICE_DIEPCTL3_SODDFRM, SODDFRM
: OTG_FS_DEVICE_DIEPCTL3_SD0PID_SEVNFRM ( -- x addr ) 28 bit OTG_FS_DEVICE_DIEPCTL3 ; \ OTG_FS_DEVICE_DIEPCTL3_SD0PID_SEVNFRM, SD0PID/SEVNFRM
: OTG_FS_DEVICE_DIEPCTL3_SNAK ( -- x addr ) 27 bit OTG_FS_DEVICE_DIEPCTL3 ; \ OTG_FS_DEVICE_DIEPCTL3_SNAK, SNAK
: OTG_FS_DEVICE_DIEPCTL3_CNAK ( -- x addr ) 26 bit OTG_FS_DEVICE_DIEPCTL3 ; \ OTG_FS_DEVICE_DIEPCTL3_CNAK, CNAK
: OTG_FS_DEVICE_DIEPCTL3_TXFNUM ( %bbbb -- x addr ) 22 lshift OTG_FS_DEVICE_DIEPCTL3 ; \ OTG_FS_DEVICE_DIEPCTL3_TXFNUM, TXFNUM
: OTG_FS_DEVICE_DIEPCTL3_Stall ( -- x addr ) 21 bit OTG_FS_DEVICE_DIEPCTL3 ; \ OTG_FS_DEVICE_DIEPCTL3_Stall, Stall
: OTG_FS_DEVICE_DIEPCTL3_EPTYP ( %bb -- x addr ) 18 lshift OTG_FS_DEVICE_DIEPCTL3 ; \ OTG_FS_DEVICE_DIEPCTL3_EPTYP, EPTYP
: OTG_FS_DEVICE_DIEPCTL3_NAKSTS ( -- x addr ) 17 bit OTG_FS_DEVICE_DIEPCTL3 ; \ OTG_FS_DEVICE_DIEPCTL3_NAKSTS, NAKSTS
: OTG_FS_DEVICE_DIEPCTL3_EONUM_DPID ( -- x addr ) 16 bit OTG_FS_DEVICE_DIEPCTL3 ; \ OTG_FS_DEVICE_DIEPCTL3_EONUM_DPID, EONUM/DPID
: OTG_FS_DEVICE_DIEPCTL3_USBAEP ( -- x addr ) 15 bit OTG_FS_DEVICE_DIEPCTL3 ; \ OTG_FS_DEVICE_DIEPCTL3_USBAEP, USBAEP
: OTG_FS_DEVICE_DIEPCTL3_MPSIZ x addr ) OTG_FS_DEVICE_DIEPCTL3 ; \ OTG_FS_DEVICE_DIEPCTL3_MPSIZ, MPSIZ
[then]

execute-defined? use-OTG_FS_DEVICE defined? OTG_FS_DEVICE_DOEPCTL0_EPENA not and [if]
\ OTG_FS_DEVICE_DOEPCTL0 (multiple-access)  Reset:0x00008000
: OTG_FS_DEVICE_DOEPCTL0_EPENA ( -- x addr ) 31 bit OTG_FS_DEVICE_DOEPCTL0 ; \ OTG_FS_DEVICE_DOEPCTL0_EPENA, EPENA
: OTG_FS_DEVICE_DOEPCTL0_EPDIS ( -- x addr ) 30 bit OTG_FS_DEVICE_DOEPCTL0 ; \ OTG_FS_DEVICE_DOEPCTL0_EPDIS, EPDIS
: OTG_FS_DEVICE_DOEPCTL0_SNAK ( -- x addr ) 27 bit OTG_FS_DEVICE_DOEPCTL0 ; \ OTG_FS_DEVICE_DOEPCTL0_SNAK, SNAK
: OTG_FS_DEVICE_DOEPCTL0_CNAK ( -- x addr ) 26 bit OTG_FS_DEVICE_DOEPCTL0 ; \ OTG_FS_DEVICE_DOEPCTL0_CNAK, CNAK
: OTG_FS_DEVICE_DOEPCTL0_Stall ( -- x addr ) 21 bit OTG_FS_DEVICE_DOEPCTL0 ; \ OTG_FS_DEVICE_DOEPCTL0_Stall, Stall
: OTG_FS_DEVICE_DOEPCTL0_SNPM ( -- x addr ) 20 bit OTG_FS_DEVICE_DOEPCTL0 ; \ OTG_FS_DEVICE_DOEPCTL0_SNPM, SNPM
: OTG_FS_DEVICE_DOEPCTL0_EPTYP ( %bb -- x addr ) 18 lshift OTG_FS_DEVICE_DOEPCTL0 ; \ OTG_FS_DEVICE_DOEPCTL0_EPTYP, EPTYP
: OTG_FS_DEVICE_DOEPCTL0_NAKSTS ( -- x addr ) 17 bit OTG_FS_DEVICE_DOEPCTL0 ; \ OTG_FS_DEVICE_DOEPCTL0_NAKSTS, NAKSTS
: OTG_FS_DEVICE_DOEPCTL0_USBAEP ( -- x addr ) 15 bit OTG_FS_DEVICE_DOEPCTL0 ; \ OTG_FS_DEVICE_DOEPCTL0_USBAEP, USBAEP
: OTG_FS_DEVICE_DOEPCTL0_MPSIZ ( %bb -- x addr ) OTG_FS_DEVICE_DOEPCTL0 ; \ OTG_FS_DEVICE_DOEPCTL0_MPSIZ, MPSIZ
[then]

defined? use-OTG_FS_DEVICE defined? OTG_FS_DEVICE_DOEPCTL1_EPENA not and [if]
\ OTG_FS_DEVICE_DOEPCTL1 (multiple-access)  Reset:0x00000000
: OTG_FS_DEVICE_DOEPCTL1_EPENA ( -- x addr ) 31 bit OTG_FS_DEVICE_DOEPCTL1 ; \ OTG_FS_DEVICE_DOEPCTL1_EPENA, EPENA
: OTG_FS_DEVICE_DOEPCTL1_EPDIS ( -- x addr ) 30 bit OTG_FS_DEVICE_DOEPCTL1 ; \ OTG_FS_DEVICE_DOEPCTL1_EPDIS, EPDIS
: OTG_FS_DEVICE_DOEPCTL1_SODDFRM ( -- x addr ) 29 bit OTG_FS_DEVICE_DOEPCTL1 ; \ OTG_FS_DEVICE_DOEPCTL1_SODDFRM, SODDFRM
: OTG_FS_DEVICE_DOEPCTL1_SD0PID_SEVNFRM ( -- x addr ) 28 bit OTG_FS_DEVICE_DOEPCTL1 ; \ OTG_FS_DEVICE_DOEPCTL1_SD0PID_SEVNFRM, SD0PID/SEVNFRM
: OTG_FS_DEVICE_DOEPCTL1_SNAK ( -- x addr ) 27 bit OTG_FS_DEVICE_DOEPCTL1 ; \ OTG_FS_DEVICE_DOEPCTL1_SNAK, SNAK
: OTG_FS_DEVICE_DOEPCTL1_CNAK ( -- x addr ) 26 bit OTG_FS_DEVICE_DOEPCTL1 ; \ OTG_FS_DEVICE_DOEPCTL1_CNAK, CNAK
: OTG_FS_DEVICE_DOEPCTL1_Stall ( -- x addr ) 21 bit OTG_FS_DEVICE_DOEPCTL1 ; \ OTG_FS_DEVICE_DOEPCTL1_Stall, Stall
: OTG_FS_DEVICE_DOEPCTL1_SNPM ( -- x addr ) 20 bit OTG_FS_DEVICE_DOEPCTL1 ; \ OTG_FS_DEVICE_DOEPCTL1_SNPM, SNPM
: OTG_FS_DEVICE_DOEPCTL1_EPTYP ( %bb -- x addr ) 18 lshift OTG_FS_DEVICE_DOEPCTL1 ; \ OTG_FS_DEVICE_DOEPCTL1_EPTYP, EPTYP
: OTG_FS_DEVICE_DOEPCTL1_NAKSTS ( -- x addr ) 17 bit OTG_FS_DEVICE_DOEPCTL1 ; \ OTG_FS_DEVICE_DOEPCTL1_NAKSTS, NAKSTS
: OTG_FS_DEVICE_DOEPCTL1_EONUM_DPID ( -- x addr ) 16 bit OTG_FS_DEVICE_DOEPCTL1 ; \ OTG_FS_DEVICE_DOEPCTL1_EONUM_DPID, EONUM/DPID
: OTG_FS_DEVICE_DOEPCTL1_USBAEP ( -- x addr ) 15 bit OTG_FS_DEVICE_DOEPCTL1 ; \ OTG_FS_DEVICE_DOEPCTL1_USBAEP, USBAEP
: OTG_FS_DEVICE_DOEPCTL1_MPSIZ x addr ) OTG_FS_DEVICE_DOEPCTL1 ; \ OTG_FS_DEVICE_DOEPCTL1_MPSIZ, MPSIZ
[then]

execute-defined? use-OTG_FS_DEVICE defined? OTG_FS_DEVICE_DOEPCTL2_EPENA not and [if]
\ OTG_FS_DEVICE_DOEPCTL2 (multiple-access)  Reset:0x00000000
: OTG_FS_DEVICE_DOEPCTL2_EPENA ( -- x addr ) 31 bit OTG_FS_DEVICE_DOEPCTL2 ; \ OTG_FS_DEVICE_DOEPCTL2_EPENA, EPENA
: OTG_FS_DEVICE_DOEPCTL2_EPDIS ( -- x addr ) 30 bit OTG_FS_DEVICE_DOEPCTL2 ; \ OTG_FS_DEVICE_DOEPCTL2_EPDIS, EPDIS
: OTG_FS_DEVICE_DOEPCTL2_SODDFRM ( -- x addr ) 29 bit OTG_FS_DEVICE_DOEPCTL2 ; \ OTG_FS_DEVICE_DOEPCTL2_SODDFRM, SODDFRM
: OTG_FS_DEVICE_DOEPCTL2_SD0PID_SEVNFRM ( -- x addr ) 28 bit OTG_FS_DEVICE_DOEPCTL2 ; \ OTG_FS_DEVICE_DOEPCTL2_SD0PID_SEVNFRM, SD0PID/SEVNFRM
: OTG_FS_DEVICE_DOEPCTL2_SNAK ( -- x addr ) 27 bit OTG_FS_DEVICE_DOEPCTL2 ; \ OTG_FS_DEVICE_DOEPCTL2_SNAK, SNAK
: OTG_FS_DEVICE_DOEPCTL2_CNAK ( -- x addr ) 26 bit OTG_FS_DEVICE_DOEPCTL2 ; \ OTG_FS_DEVICE_DOEPCTL2_CNAK, CNAK
: OTG_FS_DEVICE_DOEPCTL2_Stall ( -- x addr ) 21 bit OTG_FS_DEVICE_DOEPCTL2 ; \ OTG_FS_DEVICE_DOEPCTL2_Stall, Stall
: OTG_FS_DEVICE_DOEPCTL2_SNPM ( -- x addr ) 20 bit OTG_FS_DEVICE_DOEPCTL2 ; \ OTG_FS_DEVICE_DOEPCTL2_SNPM, SNPM
: OTG_FS_DEVICE_DOEPCTL2_EPTYP ( %bb -- x addr ) 18 lshift OTG_FS_DEVICE_DOEPCTL2 ; \ OTG_FS_DEVICE_DOEPCTL2_EPTYP, EPTYP
: OTG_FS_DEVICE_DOEPCTL2_NAKSTS ( -- x addr ) 17 bit OTG_FS_DEVICE_DOEPCTL2 ; \ OTG_FS_DEVICE_DOEPCTL2_NAKSTS, NAKSTS
: OTG_FS_DEVICE_DOEPCTL2_EONUM_DPID ( -- x addr ) 16 bit OTG_FS_DEVICE_DOEPCTL2 ; \ OTG_FS_DEVICE_DOEPCTL2_EONUM_DPID, EONUM/DPID
: OTG_FS_DEVICE_DOEPCTL2_USBAEP ( -- x addr ) 15 bit OTG_FS_DEVICE_DOEPCTL2 ; \ OTG_FS_DEVICE_DOEPCTL2_USBAEP, USBAEP
: OTG_FS_DEVICE_DOEPCTL2_MPSIZ x addr ) OTG_FS_DEVICE_DOEPCTL2 ; \ OTG_FS_DEVICE_DOEPCTL2_MPSIZ, MPSIZ
[then]

defined? use-OTG_FS_DEVICE defined? OTG_FS_DEVICE_DOEPCTL3_EPENA not and [if]
\ OTG_FS_DEVICE_DOEPCTL3 (multiple-access)  Reset:0x00000000
: OTG_FS_DEVICE_DOEPCTL3_EPENA ( -- x addr ) 31 bit OTG_FS_DEVICE_DOEPCTL3 ; \ OTG_FS_DEVICE_DOEPCTL3_EPENA, EPENA
: OTG_FS_DEVICE_DOEPCTL3_EPDIS ( -- x addr ) 30 bit OTG_FS_DEVICE_DOEPCTL3 ; \ OTG_FS_DEVICE_DOEPCTL3_EPDIS, EPDIS
: OTG_FS_DEVICE_DOEPCTL3_SODDFRM ( -- x addr ) 29 bit OTG_FS_DEVICE_DOEPCTL3 ; \ OTG_FS_DEVICE_DOEPCTL3_SODDFRM, SODDFRM
: OTG_FS_DEVICE_DOEPCTL3_SD0PID_SEVNFRM ( -- x addr ) 28 bit OTG_FS_DEVICE_DOEPCTL3 ; \ OTG_FS_DEVICE_DOEPCTL3_SD0PID_SEVNFRM, SD0PID/SEVNFRM
: OTG_FS_DEVICE_DOEPCTL3_SNAK ( -- x addr ) 27 bit OTG_FS_DEVICE_DOEPCTL3 ; \ OTG_FS_DEVICE_DOEPCTL3_SNAK, SNAK
: OTG_FS_DEVICE_DOEPCTL3_CNAK ( -- x addr ) 26 bit OTG_FS_DEVICE_DOEPCTL3 ; \ OTG_FS_DEVICE_DOEPCTL3_CNAK, CNAK
: OTG_FS_DEVICE_DOEPCTL3_Stall ( -- x addr ) 21 bit OTG_FS_DEVICE_DOEPCTL3 ; \ OTG_FS_DEVICE_DOEPCTL3_Stall, Stall
: OTG_FS_DEVICE_DOEPCTL3_SNPM ( -- x addr ) 20 bit OTG_FS_DEVICE_DOEPCTL3 ; \ OTG_FS_DEVICE_DOEPCTL3_SNPM, SNPM
: OTG_FS_DEVICE_DOEPCTL3_EPTYP ( %bb -- x addr ) 18 lshift OTG_FS_DEVICE_DOEPCTL3 ; \ OTG_FS_DEVICE_DOEPCTL3_EPTYP, EPTYP
: OTG_FS_DEVICE_DOEPCTL3_NAKSTS ( -- x addr ) 17 bit OTG_FS_DEVICE_DOEPCTL3 ; \ OTG_FS_DEVICE_DOEPCTL3_NAKSTS, NAKSTS
: OTG_FS_DEVICE_DOEPCTL3_EONUM_DPID ( -- x addr ) 16 bit OTG_FS_DEVICE_DOEPCTL3 ; \ OTG_FS_DEVICE_DOEPCTL3_EONUM_DPID, EONUM/DPID
: OTG_FS_DEVICE_DOEPCTL3_USBAEP ( -- x addr ) 15 bit OTG_FS_DEVICE_DOEPCTL3 ; \ OTG_FS_DEVICE_DOEPCTL3_USBAEP, USBAEP
: OTG_FS_DEVICE_DOEPCTL3_MPSIZ x addr ) OTG_FS_DEVICE_DOEPCTL3 ; \ OTG_FS_DEVICE_DOEPCTL3_MPSIZ, MPSIZ
[then]

execute-defined? use-OTG_FS_DEVICE defined? OTG_FS_DEVICE_DIEPINT0_TXFE not and [if]
\ OTG_FS_DEVICE_DIEPINT0 (multiple-access)  Reset:0x00000080
: OTG_FS_DEVICE_DIEPINT0_TXFE ( -- x addr ) 7 bit OTG_FS_DEVICE_DIEPINT0 ; \ OTG_FS_DEVICE_DIEPINT0_TXFE, TXFE
: OTG_FS_DEVICE_DIEPINT0_INEPNE ( -- x addr ) 6 bit OTG_FS_DEVICE_DIEPINT0 ; \ OTG_FS_DEVICE_DIEPINT0_INEPNE, INEPNE
: OTG_FS_DEVICE_DIEPINT0_ITTXFE ( -- x addr ) 4 bit OTG_FS_DEVICE_DIEPINT0 ; \ OTG_FS_DEVICE_DIEPINT0_ITTXFE, ITTXFE
: OTG_FS_DEVICE_DIEPINT0_TOC ( -- x addr ) 3 bit OTG_FS_DEVICE_DIEPINT0 ; \ OTG_FS_DEVICE_DIEPINT0_TOC, TOC
: OTG_FS_DEVICE_DIEPINT0_EPDISD ( -- x addr ) 1 bit OTG_FS_DEVICE_DIEPINT0 ; \ OTG_FS_DEVICE_DIEPINT0_EPDISD, EPDISD
: OTG_FS_DEVICE_DIEPINT0_XFRC ( -- x addr ) 0 bit OTG_FS_DEVICE_DIEPINT0 ; \ OTG_FS_DEVICE_DIEPINT0_XFRC, XFRC
[then]

defined? use-OTG_FS_DEVICE defined? OTG_FS_DEVICE_DIEPINT1_TXFE not and [if]
\ OTG_FS_DEVICE_DIEPINT1 (multiple-access)  Reset:0x00000080
: OTG_FS_DEVICE_DIEPINT1_TXFE ( -- x addr ) 7 bit OTG_FS_DEVICE_DIEPINT1 ; \ OTG_FS_DEVICE_DIEPINT1_TXFE, TXFE
: OTG_FS_DEVICE_DIEPINT1_INEPNE ( -- x addr ) 6 bit OTG_FS_DEVICE_DIEPINT1 ; \ OTG_FS_DEVICE_DIEPINT1_INEPNE, INEPNE
: OTG_FS_DEVICE_DIEPINT1_ITTXFE ( -- x addr ) 4 bit OTG_FS_DEVICE_DIEPINT1 ; \ OTG_FS_DEVICE_DIEPINT1_ITTXFE, ITTXFE
: OTG_FS_DEVICE_DIEPINT1_TOC ( -- x addr ) 3 bit OTG_FS_DEVICE_DIEPINT1 ; \ OTG_FS_DEVICE_DIEPINT1_TOC, TOC
: OTG_FS_DEVICE_DIEPINT1_EPDISD ( -- x addr ) 1 bit OTG_FS_DEVICE_DIEPINT1 ; \ OTG_FS_DEVICE_DIEPINT1_EPDISD, EPDISD
: OTG_FS_DEVICE_DIEPINT1_XFRC ( -- x addr ) 0 bit OTG_FS_DEVICE_DIEPINT1 ; \ OTG_FS_DEVICE_DIEPINT1_XFRC, XFRC
[then]

execute-defined? use-OTG_FS_DEVICE defined? OTG_FS_DEVICE_DIEPINT2_TXFE not and [if]
\ OTG_FS_DEVICE_DIEPINT2 (multiple-access)  Reset:0x00000080
: OTG_FS_DEVICE_DIEPINT2_TXFE ( -- x addr ) 7 bit OTG_FS_DEVICE_DIEPINT2 ; \ OTG_FS_DEVICE_DIEPINT2_TXFE, TXFE
: OTG_FS_DEVICE_DIEPINT2_INEPNE ( -- x addr ) 6 bit OTG_FS_DEVICE_DIEPINT2 ; \ OTG_FS_DEVICE_DIEPINT2_INEPNE, INEPNE
: OTG_FS_DEVICE_DIEPINT2_ITTXFE ( -- x addr ) 4 bit OTG_FS_DEVICE_DIEPINT2 ; \ OTG_FS_DEVICE_DIEPINT2_ITTXFE, ITTXFE
: OTG_FS_DEVICE_DIEPINT2_TOC ( -- x addr ) 3 bit OTG_FS_DEVICE_DIEPINT2 ; \ OTG_FS_DEVICE_DIEPINT2_TOC, TOC
: OTG_FS_DEVICE_DIEPINT2_EPDISD ( -- x addr ) 1 bit OTG_FS_DEVICE_DIEPINT2 ; \ OTG_FS_DEVICE_DIEPINT2_EPDISD, EPDISD
: OTG_FS_DEVICE_DIEPINT2_XFRC ( -- x addr ) 0 bit OTG_FS_DEVICE_DIEPINT2 ; \ OTG_FS_DEVICE_DIEPINT2_XFRC, XFRC
[then]

defined? use-OTG_FS_DEVICE defined? OTG_FS_DEVICE_DIEPINT3_TXFE not and [if]
\ OTG_FS_DEVICE_DIEPINT3 (multiple-access)  Reset:0x00000080
: OTG_FS_DEVICE_DIEPINT3_TXFE ( -- x addr ) 7 bit OTG_FS_DEVICE_DIEPINT3 ; \ OTG_FS_DEVICE_DIEPINT3_TXFE, TXFE
: OTG_FS_DEVICE_DIEPINT3_INEPNE ( -- x addr ) 6 bit OTG_FS_DEVICE_DIEPINT3 ; \ OTG_FS_DEVICE_DIEPINT3_INEPNE, INEPNE
: OTG_FS_DEVICE_DIEPINT3_ITTXFE ( -- x addr ) 4 bit OTG_FS_DEVICE_DIEPINT3 ; \ OTG_FS_DEVICE_DIEPINT3_ITTXFE, ITTXFE
: OTG_FS_DEVICE_DIEPINT3_TOC ( -- x addr ) 3 bit OTG_FS_DEVICE_DIEPINT3 ; \ OTG_FS_DEVICE_DIEPINT3_TOC, TOC
: OTG_FS_DEVICE_DIEPINT3_EPDISD ( -- x addr ) 1 bit OTG_FS_DEVICE_DIEPINT3 ; \ OTG_FS_DEVICE_DIEPINT3_EPDISD, EPDISD
: OTG_FS_DEVICE_DIEPINT3_XFRC ( -- x addr ) 0 bit OTG_FS_DEVICE_DIEPINT3 ; \ OTG_FS_DEVICE_DIEPINT3_XFRC, XFRC
[then]

execute-defined? use-OTG_FS_DEVICE defined? OTG_FS_DEVICE_DOEPINT0_B2BSTUP not and [if]
\ OTG_FS_DEVICE_DOEPINT0 (read-write) Reset:0x00000080
: OTG_FS_DEVICE_DOEPINT0_B2BSTUP ( -- x addr ) 6 bit OTG_FS_DEVICE_DOEPINT0 ; \ OTG_FS_DEVICE_DOEPINT0_B2BSTUP, B2BSTUP
: OTG_FS_DEVICE_DOEPINT0_OTEPDIS ( -- x addr ) 4 bit OTG_FS_DEVICE_DOEPINT0 ; \ OTG_FS_DEVICE_DOEPINT0_OTEPDIS, OTEPDIS
: OTG_FS_DEVICE_DOEPINT0_STUP ( -- x addr ) 3 bit OTG_FS_DEVICE_DOEPINT0 ; \ OTG_FS_DEVICE_DOEPINT0_STUP, STUP
: OTG_FS_DEVICE_DOEPINT0_EPDISD ( -- x addr ) 1 bit OTG_FS_DEVICE_DOEPINT0 ; \ OTG_FS_DEVICE_DOEPINT0_EPDISD, EPDISD
: OTG_FS_DEVICE_DOEPINT0_XFRC ( -- x addr ) 0 bit OTG_FS_DEVICE_DOEPINT0 ; \ OTG_FS_DEVICE_DOEPINT0_XFRC, XFRC
[then]

defined? use-OTG_FS_DEVICE defined? OTG_FS_DEVICE_DOEPINT1_B2BSTUP not and [if]
\ OTG_FS_DEVICE_DOEPINT1 (read-write) Reset:0x00000080
: OTG_FS_DEVICE_DOEPINT1_B2BSTUP ( -- x addr ) 6 bit OTG_FS_DEVICE_DOEPINT1 ; \ OTG_FS_DEVICE_DOEPINT1_B2BSTUP, B2BSTUP
: OTG_FS_DEVICE_DOEPINT1_OTEPDIS ( -- x addr ) 4 bit OTG_FS_DEVICE_DOEPINT1 ; \ OTG_FS_DEVICE_DOEPINT1_OTEPDIS, OTEPDIS
: OTG_FS_DEVICE_DOEPINT1_STUP ( -- x addr ) 3 bit OTG_FS_DEVICE_DOEPINT1 ; \ OTG_FS_DEVICE_DOEPINT1_STUP, STUP
: OTG_FS_DEVICE_DOEPINT1_EPDISD ( -- x addr ) 1 bit OTG_FS_DEVICE_DOEPINT1 ; \ OTG_FS_DEVICE_DOEPINT1_EPDISD, EPDISD
: OTG_FS_DEVICE_DOEPINT1_XFRC ( -- x addr ) 0 bit OTG_FS_DEVICE_DOEPINT1 ; \ OTG_FS_DEVICE_DOEPINT1_XFRC, XFRC
[then]

execute-defined? use-OTG_FS_DEVICE defined? OTG_FS_DEVICE_DOEPINT2_B2BSTUP not and [if]
\ OTG_FS_DEVICE_DOEPINT2 (read-write) Reset:0x00000080
: OTG_FS_DEVICE_DOEPINT2_B2BSTUP ( -- x addr ) 6 bit OTG_FS_DEVICE_DOEPINT2 ; \ OTG_FS_DEVICE_DOEPINT2_B2BSTUP, B2BSTUP
: OTG_FS_DEVICE_DOEPINT2_OTEPDIS ( -- x addr ) 4 bit OTG_FS_DEVICE_DOEPINT2 ; \ OTG_FS_DEVICE_DOEPINT2_OTEPDIS, OTEPDIS
: OTG_FS_DEVICE_DOEPINT2_STUP ( -- x addr ) 3 bit OTG_FS_DEVICE_DOEPINT2 ; \ OTG_FS_DEVICE_DOEPINT2_STUP, STUP
: OTG_FS_DEVICE_DOEPINT2_EPDISD ( -- x addr ) 1 bit OTG_FS_DEVICE_DOEPINT2 ; \ OTG_FS_DEVICE_DOEPINT2_EPDISD, EPDISD
: OTG_FS_DEVICE_DOEPINT2_XFRC ( -- x addr ) 0 bit OTG_FS_DEVICE_DOEPINT2 ; \ OTG_FS_DEVICE_DOEPINT2_XFRC, XFRC
[then]

defined? use-OTG_FS_DEVICE defined? OTG_FS_DEVICE_DOEPINT3_B2BSTUP not and [if]
\ OTG_FS_DEVICE_DOEPINT3 (read-write) Reset:0x00000080
: OTG_FS_DEVICE_DOEPINT3_B2BSTUP ( -- x addr ) 6 bit OTG_FS_DEVICE_DOEPINT3 ; \ OTG_FS_DEVICE_DOEPINT3_B2BSTUP, B2BSTUP
: OTG_FS_DEVICE_DOEPINT3_OTEPDIS ( -- x addr ) 4 bit OTG_FS_DEVICE_DOEPINT3 ; \ OTG_FS_DEVICE_DOEPINT3_OTEPDIS, OTEPDIS
: OTG_FS_DEVICE_DOEPINT3_STUP ( -- x addr ) 3 bit OTG_FS_DEVICE_DOEPINT3 ; \ OTG_FS_DEVICE_DOEPINT3_STUP, STUP
: OTG_FS_DEVICE_DOEPINT3_EPDISD ( -- x addr ) 1 bit OTG_FS_DEVICE_DOEPINT3 ; \ OTG_FS_DEVICE_DOEPINT3_EPDISD, EPDISD
: OTG_FS_DEVICE_DOEPINT3_XFRC ( -- x addr ) 0 bit OTG_FS_DEVICE_DOEPINT3 ; \ OTG_FS_DEVICE_DOEPINT3_XFRC, XFRC
[then]

execute-defined? use-OTG_FS_DEVICE defined? OTG_FS_DEVICE_DIEPTSIZ0_PKTCNT not and [if]
\ OTG_FS_DEVICE_DIEPTSIZ0 (read-write) Reset:0x00000000
: OTG_FS_DEVICE_DIEPTSIZ0_PKTCNT ( %bb -- x addr ) 19 lshift OTG_FS_DEVICE_DIEPTSIZ0 ; \ OTG_FS_DEVICE_DIEPTSIZ0_PKTCNT, Packet count
: OTG_FS_DEVICE_DIEPTSIZ0_XFRSIZ ( %bbbbbbb -- x addr ) OTG_FS_DEVICE_DIEPTSIZ0 ; \ OTG_FS_DEVICE_DIEPTSIZ0_XFRSIZ, Transfer size
[then]

defined? use-OTG_FS_DEVICE defined? OTG_FS_DEVICE_DOEPTSIZ0_STUPCNT not and [if]
\ OTG_FS_DEVICE_DOEPTSIZ0 (read-write) Reset:0x00000000
: OTG_FS_DEVICE_DOEPTSIZ0_STUPCNT ( %bb -- x addr ) 29 lshift OTG_FS_DEVICE_DOEPTSIZ0 ; \ OTG_FS_DEVICE_DOEPTSIZ0_STUPCNT, SETUP packet count
: OTG_FS_DEVICE_DOEPTSIZ0_PKTCNT ( -- x addr ) 19 bit OTG_FS_DEVICE_DOEPTSIZ0 ; \ OTG_FS_DEVICE_DOEPTSIZ0_PKTCNT, Packet count
: OTG_FS_DEVICE_DOEPTSIZ0_XFRSIZ ( %bbbbbbb -- x addr ) OTG_FS_DEVICE_DOEPTSIZ0 ; \ OTG_FS_DEVICE_DOEPTSIZ0_XFRSIZ, Transfer size
[then]

execute-defined? use-OTG_FS_DEVICE defined? OTG_FS_DEVICE_DIEPTSIZ1_MCNT not and [if]
\ OTG_FS_DEVICE_DIEPTSIZ1 (read-write) Reset:0x00000000
: OTG_FS_DEVICE_DIEPTSIZ1_MCNT ( %bb -- x addr ) 29 lshift OTG_FS_DEVICE_DIEPTSIZ1 ; \ OTG_FS_DEVICE_DIEPTSIZ1_MCNT, Multi count
: OTG_FS_DEVICE_DIEPTSIZ1_PKTCNT ( %bbbbbbbbbb -- x addr ) 19 lshift OTG_FS_DEVICE_DIEPTSIZ1 ; \ OTG_FS_DEVICE_DIEPTSIZ1_PKTCNT, Packet count
: OTG_FS_DEVICE_DIEPTSIZ1_XFRSIZ x addr ) OTG_FS_DEVICE_DIEPTSIZ1 ; \ OTG_FS_DEVICE_DIEPTSIZ1_XFRSIZ, Transfer size
[then]

defined? use-OTG_FS_DEVICE defined? OTG_FS_DEVICE_DIEPTSIZ2_MCNT not and [if]
\ OTG_FS_DEVICE_DIEPTSIZ2 (read-write) Reset:0x00000000
: OTG_FS_DEVICE_DIEPTSIZ2_MCNT ( %bb -- x addr ) 29 lshift OTG_FS_DEVICE_DIEPTSIZ2 ; \ OTG_FS_DEVICE_DIEPTSIZ2_MCNT, Multi count
: OTG_FS_DEVICE_DIEPTSIZ2_PKTCNT ( %bbbbbbbbbb -- x addr ) 19 lshift OTG_FS_DEVICE_DIEPTSIZ2 ; \ OTG_FS_DEVICE_DIEPTSIZ2_PKTCNT, Packet count
: OTG_FS_DEVICE_DIEPTSIZ2_XFRSIZ x addr ) OTG_FS_DEVICE_DIEPTSIZ2 ; \ OTG_FS_DEVICE_DIEPTSIZ2_XFRSIZ, Transfer size
[then]

execute-defined? use-OTG_FS_DEVICE defined? OTG_FS_DEVICE_DIEPTSIZ3_MCNT not and [if]
\ OTG_FS_DEVICE_DIEPTSIZ3 (read-write) Reset:0x00000000
: OTG_FS_DEVICE_DIEPTSIZ3_MCNT ( %bb -- x addr ) 29 lshift OTG_FS_DEVICE_DIEPTSIZ3 ; \ OTG_FS_DEVICE_DIEPTSIZ3_MCNT, Multi count
: OTG_FS_DEVICE_DIEPTSIZ3_PKTCNT ( %bbbbbbbbbb -- x addr ) 19 lshift OTG_FS_DEVICE_DIEPTSIZ3 ; \ OTG_FS_DEVICE_DIEPTSIZ3_PKTCNT, Packet count
: OTG_FS_DEVICE_DIEPTSIZ3_XFRSIZ x addr ) OTG_FS_DEVICE_DIEPTSIZ3 ; \ OTG_FS_DEVICE_DIEPTSIZ3_XFRSIZ, Transfer size
[then]

defined? use-OTG_FS_DEVICE defined? OTG_FS_DEVICE_DTXFSTS0_INEPTFSAV? not and [if]
\ OTG_FS_DEVICE_DTXFSTS0 (read-only) Reset:0x00000000
: OTG_FS_DEVICE_DTXFSTS0_INEPTFSAV? ( --  x ) OTG_FS_DEVICE_DTXFSTS0 @ ; \ OTG_FS_DEVICE_DTXFSTS0_INEPTFSAV, IN endpoint TxFIFO space  available
[then]

execute-defined? use-OTG_FS_DEVICE defined? OTG_FS_DEVICE_DTXFSTS1_INEPTFSAV? not and [if]
\ OTG_FS_DEVICE_DTXFSTS1 (read-only) Reset:0x00000000
: OTG_FS_DEVICE_DTXFSTS1_INEPTFSAV? ( --  x ) OTG_FS_DEVICE_DTXFSTS1 @ ; \ OTG_FS_DEVICE_DTXFSTS1_INEPTFSAV, IN endpoint TxFIFO space  available
[then]

defined? use-OTG_FS_DEVICE defined? OTG_FS_DEVICE_DTXFSTS2_INEPTFSAV? not and [if]
\ OTG_FS_DEVICE_DTXFSTS2 (read-only) Reset:0x00000000
: OTG_FS_DEVICE_DTXFSTS2_INEPTFSAV? ( --  x ) OTG_FS_DEVICE_DTXFSTS2 @ ; \ OTG_FS_DEVICE_DTXFSTS2_INEPTFSAV, IN endpoint TxFIFO space  available
[then]

execute-defined? use-OTG_FS_DEVICE defined? OTG_FS_DEVICE_DTXFSTS3_INEPTFSAV? not and [if]
\ OTG_FS_DEVICE_DTXFSTS3 (read-only) Reset:0x00000000
: OTG_FS_DEVICE_DTXFSTS3_INEPTFSAV? ( --  x ) OTG_FS_DEVICE_DTXFSTS3 @ ; \ OTG_FS_DEVICE_DTXFSTS3_INEPTFSAV, IN endpoint TxFIFO space  available
[then]

defined? use-OTG_FS_DEVICE defined? OTG_FS_DEVICE_DOEPTSIZ1_RXDPID_STUPCNT not and [if]
\ OTG_FS_DEVICE_DOEPTSIZ1 (read-write) Reset:0x00000000
: OTG_FS_DEVICE_DOEPTSIZ1_RXDPID_STUPCNT ( %bb -- x addr ) 29 lshift OTG_FS_DEVICE_DOEPTSIZ1 ; \ OTG_FS_DEVICE_DOEPTSIZ1_RXDPID_STUPCNT, Received data PID/SETUP packet  count
: OTG_FS_DEVICE_DOEPTSIZ1_PKTCNT ( %bbbbbbbbbb -- x addr ) 19 lshift OTG_FS_DEVICE_DOEPTSIZ1 ; \ OTG_FS_DEVICE_DOEPTSIZ1_PKTCNT, Packet count
: OTG_FS_DEVICE_DOEPTSIZ1_XFRSIZ x addr ) OTG_FS_DEVICE_DOEPTSIZ1 ; \ OTG_FS_DEVICE_DOEPTSIZ1_XFRSIZ, Transfer size
[then]

execute-defined? use-OTG_FS_DEVICE defined? OTG_FS_DEVICE_DOEPTSIZ2_RXDPID_STUPCNT not and [if]
\ OTG_FS_DEVICE_DOEPTSIZ2 (read-write) Reset:0x00000000
: OTG_FS_DEVICE_DOEPTSIZ2_RXDPID_STUPCNT ( %bb -- x addr ) 29 lshift OTG_FS_DEVICE_DOEPTSIZ2 ; \ OTG_FS_DEVICE_DOEPTSIZ2_RXDPID_STUPCNT, Received data PID/SETUP packet  count
: OTG_FS_DEVICE_DOEPTSIZ2_PKTCNT ( %bbbbbbbbbb -- x addr ) 19 lshift OTG_FS_DEVICE_DOEPTSIZ2 ; \ OTG_FS_DEVICE_DOEPTSIZ2_PKTCNT, Packet count
: OTG_FS_DEVICE_DOEPTSIZ2_XFRSIZ x addr ) OTG_FS_DEVICE_DOEPTSIZ2 ; \ OTG_FS_DEVICE_DOEPTSIZ2_XFRSIZ, Transfer size
[then]

defined? use-OTG_FS_DEVICE defined? OTG_FS_DEVICE_DOEPTSIZ3_RXDPID_STUPCNT not and [if]
\ OTG_FS_DEVICE_DOEPTSIZ3 (read-write) Reset:0x00000000
: OTG_FS_DEVICE_DOEPTSIZ3_RXDPID_STUPCNT ( %bb -- x addr ) 29 lshift OTG_FS_DEVICE_DOEPTSIZ3 ; \ OTG_FS_DEVICE_DOEPTSIZ3_RXDPID_STUPCNT, Received data PID/SETUP packet  count
: OTG_FS_DEVICE_DOEPTSIZ3_PKTCNT ( %bbbbbbbbbb -- x addr ) 19 lshift OTG_FS_DEVICE_DOEPTSIZ3 ; \ OTG_FS_DEVICE_DOEPTSIZ3_PKTCNT, Packet count
: OTG_FS_DEVICE_DOEPTSIZ3_XFRSIZ x addr ) OTG_FS_DEVICE_DOEPTSIZ3 ; \ OTG_FS_DEVICE_DOEPTSIZ3_XFRSIZ, Transfer size
[then]

execute-defined? use-OTG_FS_PWRCLK defined? OTG_FS_PWRCLK_FS_PCGCCTL_STPPCLK not and [if]
\ OTG_FS_PWRCLK_FS_PCGCCTL (read-write) Reset:0x00000000
: OTG_FS_PWRCLK_FS_PCGCCTL_STPPCLK ( -- x addr ) 0 bit OTG_FS_PWRCLK_FS_PCGCCTL ; \ OTG_FS_PWRCLK_FS_PCGCCTL_STPPCLK, Stop PHY clock
: OTG_FS_PWRCLK_FS_PCGCCTL_GATEHCLK ( -- x addr ) 1 bit OTG_FS_PWRCLK_FS_PCGCCTL ; \ OTG_FS_PWRCLK_FS_PCGCCTL_GATEHCLK, Gate HCLK
: OTG_FS_PWRCLK_FS_PCGCCTL_PHYSUSP ( -- x addr ) 4 bit OTG_FS_PWRCLK_FS_PCGCCTL ; \ OTG_FS_PWRCLK_FS_PCGCCTL_PHYSUSP, PHY Suspended
[then]

defined? use-NVIC defined? NVIC_ICTR_INTLINESNUM? not and [if]
\ NVIC_ICTR (read-only) Reset:0x00000000
: NVIC_ICTR_INTLINESNUM? ( --  x ) NVIC_ICTR @ ; \ NVIC_ICTR_INTLINESNUM, Total number of interrupt lines in  groups
[then]

execute-defined? use-NVIC defined? NVIC_STIR_INTID not and [if]
\ NVIC_STIR (write-only) Reset:0x00000000
: NVIC_STIR_INTID ( %bbbbbbbbb -- x addr ) NVIC_STIR ; \ NVIC_STIR_INTID, interrupt to be triggered
[then]

defined? use-NVIC defined? NVIC_ISER0_SETENA not and [if]
\ NVIC_ISER0 (read-write) Reset:0x00000000
: NVIC_ISER0_SETENA ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) NVIC_ISER0 ; \ NVIC_ISER0_SETENA, SETENA
[then]

execute-defined? use-NVIC defined? NVIC_ISER1_SETENA not and [if]
\ NVIC_ISER1 (read-write) Reset:0x00000000
: NVIC_ISER1_SETENA ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) NVIC_ISER1 ; \ NVIC_ISER1_SETENA, SETENA
[then]

defined? use-NVIC defined? NVIC_ISER2_SETENA not and [if]
\ NVIC_ISER2 (read-write) Reset:0x00000000
: NVIC_ISER2_SETENA ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) NVIC_ISER2 ; \ NVIC_ISER2_SETENA, SETENA
[then]

execute-defined? use-NVIC defined? NVIC_ICER0_CLRENA not and [if]
\ NVIC_ICER0 (read-write) Reset:0x00000000
: NVIC_ICER0_CLRENA ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) NVIC_ICER0 ; \ NVIC_ICER0_CLRENA, CLRENA
[then]

defined? use-NVIC defined? NVIC_ICER1_CLRENA not and [if]
\ NVIC_ICER1 (read-write) Reset:0x00000000
: NVIC_ICER1_CLRENA ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) NVIC_ICER1 ; \ NVIC_ICER1_CLRENA, CLRENA
[then]

execute-defined? use-NVIC defined? NVIC_ICER2_CLRENA not and [if]
\ NVIC_ICER2 (read-write) Reset:0x00000000
: NVIC_ICER2_CLRENA ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) NVIC_ICER2 ; \ NVIC_ICER2_CLRENA, CLRENA
[then]

defined? use-NVIC defined? NVIC_ISPR0_SETPEND not and [if]
\ NVIC_ISPR0 (read-write) Reset:0x00000000
: NVIC_ISPR0_SETPEND ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) NVIC_ISPR0 ; \ NVIC_ISPR0_SETPEND, SETPEND
[then]

execute-defined? use-NVIC defined? NVIC_ISPR1_SETPEND not and [if]
\ NVIC_ISPR1 (read-write) Reset:0x00000000
: NVIC_ISPR1_SETPEND ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) NVIC_ISPR1 ; \ NVIC_ISPR1_SETPEND, SETPEND
[then]

defined? use-NVIC defined? NVIC_ISPR2_SETPEND not and [if]
\ NVIC_ISPR2 (read-write) Reset:0x00000000
: NVIC_ISPR2_SETPEND ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) NVIC_ISPR2 ; \ NVIC_ISPR2_SETPEND, SETPEND
[then]

execute-defined? use-NVIC defined? NVIC_ICPR0_CLRPEND not and [if]
\ NVIC_ICPR0 (read-write) Reset:0x00000000
: NVIC_ICPR0_CLRPEND ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) NVIC_ICPR0 ; \ NVIC_ICPR0_CLRPEND, CLRPEND
[then]

defined? use-NVIC defined? NVIC_ICPR1_CLRPEND not and [if]
\ NVIC_ICPR1 (read-write) Reset:0x00000000
: NVIC_ICPR1_CLRPEND ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) NVIC_ICPR1 ; \ NVIC_ICPR1_CLRPEND, CLRPEND
[then]

execute-defined? use-NVIC defined? NVIC_ICPR2_CLRPEND not and [if]
\ NVIC_ICPR2 (read-write) Reset:0x00000000
: NVIC_ICPR2_CLRPEND ( %bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -- x addr ) NVIC_ICPR2 ; \ NVIC_ICPR2_CLRPEND, CLRPEND
[then]

defined? use-NVIC defined? NVIC_IABR0_ACTIVE? not and [if]
\ NVIC_IABR0 (read-only) Reset:0x00000000
: NVIC_IABR0_ACTIVE? ( --  x ) NVIC_IABR0 @ ; \ NVIC_IABR0_ACTIVE, ACTIVE
[then]

execute-defined? use-NVIC defined? NVIC_IABR1_ACTIVE? not and [if]
\ NVIC_IABR1 (read-only) Reset:0x00000000
: NVIC_IABR1_ACTIVE? ( --  x ) NVIC_IABR1 @ ; \ NVIC_IABR1_ACTIVE, ACTIVE
[then]

defined? use-NVIC defined? NVIC_IABR2_ACTIVE? not and [if]
\ NVIC_IABR2 (read-only) Reset:0x00000000
: NVIC_IABR2_ACTIVE? ( --  x ) NVIC_IABR2 @ ; \ NVIC_IABR2_ACTIVE, ACTIVE
[then]

execute-defined? use-NVIC defined? NVIC_IPR0_IPR_N0 not and [if]
\ NVIC_IPR0 (read-write) Reset:0x00000000
: NVIC_IPR0_IPR_N0 ( %bbbbbbbb -- x addr ) NVIC_IPR0 ; \ NVIC_IPR0_IPR_N0, IPR_N0
: NVIC_IPR0_IPR_N1 ( %bbbbbbbb -- x addr ) 8 lshift NVIC_IPR0 ; \ NVIC_IPR0_IPR_N1, IPR_N1
: NVIC_IPR0_IPR_N2 ( %bbbbbbbb -- x addr ) 16 lshift NVIC_IPR0 ; \ NVIC_IPR0_IPR_N2, IPR_N2
: NVIC_IPR0_IPR_N3 ( %bbbbbbbb -- x addr ) 24 lshift NVIC_IPR0 ; \ NVIC_IPR0_IPR_N3, IPR_N3
[then]

defined? use-NVIC defined? NVIC_IPR1_IPR_N0 not and [if]
\ NVIC_IPR1 (read-write) Reset:0x00000000
: NVIC_IPR1_IPR_N0 ( %bbbbbbbb -- x addr ) NVIC_IPR1 ; \ NVIC_IPR1_IPR_N0, IPR_N0
: NVIC_IPR1_IPR_N1 ( %bbbbbbbb -- x addr ) 8 lshift NVIC_IPR1 ; \ NVIC_IPR1_IPR_N1, IPR_N1
: NVIC_IPR1_IPR_N2 ( %bbbbbbbb -- x addr ) 16 lshift NVIC_IPR1 ; \ NVIC_IPR1_IPR_N2, IPR_N2
: NVIC_IPR1_IPR_N3 ( %bbbbbbbb -- x addr ) 24 lshift NVIC_IPR1 ; \ NVIC_IPR1_IPR_N3, IPR_N3
[then]

execute-defined? use-NVIC defined? NVIC_IPR2_IPR_N0 not and [if]
\ NVIC_IPR2 (read-write) Reset:0x00000000
: NVIC_IPR2_IPR_N0 ( %bbbbbbbb -- x addr ) NVIC_IPR2 ; \ NVIC_IPR2_IPR_N0, IPR_N0
: NVIC_IPR2_IPR_N1 ( %bbbbbbbb -- x addr ) 8 lshift NVIC_IPR2 ; \ NVIC_IPR2_IPR_N1, IPR_N1
: NVIC_IPR2_IPR_N2 ( %bbbbbbbb -- x addr ) 16 lshift NVIC_IPR2 ; \ NVIC_IPR2_IPR_N2, IPR_N2
: NVIC_IPR2_IPR_N3 ( %bbbbbbbb -- x addr ) 24 lshift NVIC_IPR2 ; \ NVIC_IPR2_IPR_N3, IPR_N3
[then]

defined? use-NVIC defined? NVIC_IPR3_IPR_N0 not and [if]
\ NVIC_IPR3 (read-write) Reset:0x00000000
: NVIC_IPR3_IPR_N0 ( %bbbbbbbb -- x addr ) NVIC_IPR3 ; \ NVIC_IPR3_IPR_N0, IPR_N0
: NVIC_IPR3_IPR_N1 ( %bbbbbbbb -- x addr ) 8 lshift NVIC_IPR3 ; \ NVIC_IPR3_IPR_N1, IPR_N1
: NVIC_IPR3_IPR_N2 ( %bbbbbbbb -- x addr ) 16 lshift NVIC_IPR3 ; \ NVIC_IPR3_IPR_N2, IPR_N2
: NVIC_IPR3_IPR_N3 ( %bbbbbbbb -- x addr ) 24 lshift NVIC_IPR3 ; \ NVIC_IPR3_IPR_N3, IPR_N3
[then]

execute-defined? use-NVIC defined? NVIC_IPR4_IPR_N0 not and [if]
\ NVIC_IPR4 (read-write) Reset:0x00000000
: NVIC_IPR4_IPR_N0 ( %bbbbbbbb -- x addr ) NVIC_IPR4 ; \ NVIC_IPR4_IPR_N0, IPR_N0
: NVIC_IPR4_IPR_N1 ( %bbbbbbbb -- x addr ) 8 lshift NVIC_IPR4 ; \ NVIC_IPR4_IPR_N1, IPR_N1
: NVIC_IPR4_IPR_N2 ( %bbbbbbbb -- x addr ) 16 lshift NVIC_IPR4 ; \ NVIC_IPR4_IPR_N2, IPR_N2
: NVIC_IPR4_IPR_N3 ( %bbbbbbbb -- x addr ) 24 lshift NVIC_IPR4 ; \ NVIC_IPR4_IPR_N3, IPR_N3
[then]

defined? use-NVIC defined? NVIC_IPR5_IPR_N0 not and [if]
\ NVIC_IPR5 (read-write) Reset:0x00000000
: NVIC_IPR5_IPR_N0 ( %bbbbbbbb -- x addr ) NVIC_IPR5 ; \ NVIC_IPR5_IPR_N0, IPR_N0
: NVIC_IPR5_IPR_N1 ( %bbbbbbbb -- x addr ) 8 lshift NVIC_IPR5 ; \ NVIC_IPR5_IPR_N1, IPR_N1
: NVIC_IPR5_IPR_N2 ( %bbbbbbbb -- x addr ) 16 lshift NVIC_IPR5 ; \ NVIC_IPR5_IPR_N2, IPR_N2
: NVIC_IPR5_IPR_N3 ( %bbbbbbbb -- x addr ) 24 lshift NVIC_IPR5 ; \ NVIC_IPR5_IPR_N3, IPR_N3
[then]

execute-defined? use-NVIC defined? NVIC_IPR6_IPR_N0 not and [if]
\ NVIC_IPR6 (read-write) Reset:0x00000000
: NVIC_IPR6_IPR_N0 ( %bbbbbbbb -- x addr ) NVIC_IPR6 ; \ NVIC_IPR6_IPR_N0, IPR_N0
: NVIC_IPR6_IPR_N1 ( %bbbbbbbb -- x addr ) 8 lshift NVIC_IPR6 ; \ NVIC_IPR6_IPR_N1, IPR_N1
: NVIC_IPR6_IPR_N2 ( %bbbbbbbb -- x addr ) 16 lshift NVIC_IPR6 ; \ NVIC_IPR6_IPR_N2, IPR_N2
: NVIC_IPR6_IPR_N3 ( %bbbbbbbb -- x addr ) 24 lshift NVIC_IPR6 ; \ NVIC_IPR6_IPR_N3, IPR_N3
[then]

defined? use-NVIC defined? NVIC_IPR7_IPR_N0 not and [if]
\ NVIC_IPR7 (read-write) Reset:0x00000000
: NVIC_IPR7_IPR_N0 ( %bbbbbbbb -- x addr ) NVIC_IPR7 ; \ NVIC_IPR7_IPR_N0, IPR_N0
: NVIC_IPR7_IPR_N1 ( %bbbbbbbb -- x addr ) 8 lshift NVIC_IPR7 ; \ NVIC_IPR7_IPR_N1, IPR_N1
: NVIC_IPR7_IPR_N2 ( %bbbbbbbb -- x addr ) 16 lshift NVIC_IPR7 ; \ NVIC_IPR7_IPR_N2, IPR_N2
: NVIC_IPR7_IPR_N3 ( %bbbbbbbb -- x addr ) 24 lshift NVIC_IPR7 ; \ NVIC_IPR7_IPR_N3, IPR_N3
[then]

execute-defined? use-NVIC defined? NVIC_IPR8_IPR_N0 not and [if]
\ NVIC_IPR8 (read-write) Reset:0x00000000
: NVIC_IPR8_IPR_N0 ( %bbbbbbbb -- x addr ) NVIC_IPR8 ; \ NVIC_IPR8_IPR_N0, IPR_N0
: NVIC_IPR8_IPR_N1 ( %bbbbbbbb -- x addr ) 8 lshift NVIC_IPR8 ; \ NVIC_IPR8_IPR_N1, IPR_N1
: NVIC_IPR8_IPR_N2 ( %bbbbbbbb -- x addr ) 16 lshift NVIC_IPR8 ; \ NVIC_IPR8_IPR_N2, IPR_N2
: NVIC_IPR8_IPR_N3 ( %bbbbbbbb -- x addr ) 24 lshift NVIC_IPR8 ; \ NVIC_IPR8_IPR_N3, IPR_N3
[then]

defined? use-NVIC defined? NVIC_IPR9_IPR_N0 not and [if]
\ NVIC_IPR9 (read-write) Reset:0x00000000
: NVIC_IPR9_IPR_N0 ( %bbbbbbbb -- x addr ) NVIC_IPR9 ; \ NVIC_IPR9_IPR_N0, IPR_N0
: NVIC_IPR9_IPR_N1 ( %bbbbbbbb -- x addr ) 8 lshift NVIC_IPR9 ; \ NVIC_IPR9_IPR_N1, IPR_N1
: NVIC_IPR9_IPR_N2 ( %bbbbbbbb -- x addr ) 16 lshift NVIC_IPR9 ; \ NVIC_IPR9_IPR_N2, IPR_N2
: NVIC_IPR9_IPR_N3 ( %bbbbbbbb -- x addr ) 24 lshift NVIC_IPR9 ; \ NVIC_IPR9_IPR_N3, IPR_N3
[then]

execute-defined? use-NVIC defined? NVIC_IPR10_IPR_N0 not and [if]
\ NVIC_IPR10 (read-write) Reset:0x00000000
: NVIC_IPR10_IPR_N0 ( %bbbbbbbb -- x addr ) NVIC_IPR10 ; \ NVIC_IPR10_IPR_N0, IPR_N0
: NVIC_IPR10_IPR_N1 ( %bbbbbbbb -- x addr ) 8 lshift NVIC_IPR10 ; \ NVIC_IPR10_IPR_N1, IPR_N1
: NVIC_IPR10_IPR_N2 ( %bbbbbbbb -- x addr ) 16 lshift NVIC_IPR10 ; \ NVIC_IPR10_IPR_N2, IPR_N2
: NVIC_IPR10_IPR_N3 ( %bbbbbbbb -- x addr ) 24 lshift NVIC_IPR10 ; \ NVIC_IPR10_IPR_N3, IPR_N3
[then]

defined? use-NVIC defined? NVIC_IPR11_IPR_N0 not and [if]
\ NVIC_IPR11 (read-write) Reset:0x00000000
: NVIC_IPR11_IPR_N0 ( %bbbbbbbb -- x addr ) NVIC_IPR11 ; \ NVIC_IPR11_IPR_N0, IPR_N0
: NVIC_IPR11_IPR_N1 ( %bbbbbbbb -- x addr ) 8 lshift NVIC_IPR11 ; \ NVIC_IPR11_IPR_N1, IPR_N1
: NVIC_IPR11_IPR_N2 ( %bbbbbbbb -- x addr ) 16 lshift NVIC_IPR11 ; \ NVIC_IPR11_IPR_N2, IPR_N2
: NVIC_IPR11_IPR_N3 ( %bbbbbbbb -- x addr ) 24 lshift NVIC_IPR11 ; \ NVIC_IPR11_IPR_N3, IPR_N3
[then]

execute-defined? use-NVIC defined? NVIC_IPR12_IPR_N0 not and [if]
\ NVIC_IPR12 (read-write) Reset:0x00000000
: NVIC_IPR12_IPR_N0 ( %bbbbbbbb -- x addr ) NVIC_IPR12 ; \ NVIC_IPR12_IPR_N0, IPR_N0
: NVIC_IPR12_IPR_N1 ( %bbbbbbbb -- x addr ) 8 lshift NVIC_IPR12 ; \ NVIC_IPR12_IPR_N1, IPR_N1
: NVIC_IPR12_IPR_N2 ( %bbbbbbbb -- x addr ) 16 lshift NVIC_IPR12 ; \ NVIC_IPR12_IPR_N2, IPR_N2
: NVIC_IPR12_IPR_N3 ( %bbbbbbbb -- x addr ) 24 lshift NVIC_IPR12 ; \ NVIC_IPR12_IPR_N3, IPR_N3
[then]

defined? use-NVIC defined? NVIC_IPR13_IPR_N0 not and [if]
\ NVIC_IPR13 (read-write) Reset:0x00000000
: NVIC_IPR13_IPR_N0 ( %bbbbbbbb -- x addr ) NVIC_IPR13 ; \ NVIC_IPR13_IPR_N0, IPR_N0
: NVIC_IPR13_IPR_N1 ( %bbbbbbbb -- x addr ) 8 lshift NVIC_IPR13 ; \ NVIC_IPR13_IPR_N1, IPR_N1
: NVIC_IPR13_IPR_N2 ( %bbbbbbbb -- x addr ) 16 lshift NVIC_IPR13 ; \ NVIC_IPR13_IPR_N2, IPR_N2
: NVIC_IPR13_IPR_N3 ( %bbbbbbbb -- x addr ) 24 lshift NVIC_IPR13 ; \ NVIC_IPR13_IPR_N3, IPR_N3
[then]

execute-defined? use-NVIC defined? NVIC_IPR14_IPR_N0 not and [if]
\ NVIC_IPR14 (read-write) Reset:0x00000000
: NVIC_IPR14_IPR_N0 ( %bbbbbbbb -- x addr ) NVIC_IPR14 ; \ NVIC_IPR14_IPR_N0, IPR_N0
: NVIC_IPR14_IPR_N1 ( %bbbbbbbb -- x addr ) 8 lshift NVIC_IPR14 ; \ NVIC_IPR14_IPR_N1, IPR_N1
: NVIC_IPR14_IPR_N2 ( %bbbbbbbb -- x addr ) 16 lshift NVIC_IPR14 ; \ NVIC_IPR14_IPR_N2, IPR_N2
: NVIC_IPR14_IPR_N3 ( %bbbbbbbb -- x addr ) 24 lshift NVIC_IPR14 ; \ NVIC_IPR14_IPR_N3, IPR_N3
[then]

defined? use-NVIC defined? NVIC_IPR15_IPR_N0 not and [if]
\ NVIC_IPR15 (read-write) Reset:0x00000000
: NVIC_IPR15_IPR_N0 ( %bbbbbbbb -- x addr ) NVIC_IPR15 ; \ NVIC_IPR15_IPR_N0, IPR_N0
: NVIC_IPR15_IPR_N1 ( %bbbbbbbb -- x addr ) 8 lshift NVIC_IPR15 ; \ NVIC_IPR15_IPR_N1, IPR_N1
: NVIC_IPR15_IPR_N2 ( %bbbbbbbb -- x addr ) 16 lshift NVIC_IPR15 ; \ NVIC_IPR15_IPR_N2, IPR_N2
: NVIC_IPR15_IPR_N3 ( %bbbbbbbb -- x addr ) 24 lshift NVIC_IPR15 ; \ NVIC_IPR15_IPR_N3, IPR_N3
[then]

execute-defined? use-NVIC defined? NVIC_IPR16_IPR_N0 not and [if]
\ NVIC_IPR16 (read-write) Reset:0x00000000
: NVIC_IPR16_IPR_N0 ( %bbbbbbbb -- x addr ) NVIC_IPR16 ; \ NVIC_IPR16_IPR_N0, IPR_N0
: NVIC_IPR16_IPR_N1 ( %bbbbbbbb -- x addr ) 8 lshift NVIC_IPR16 ; \ NVIC_IPR16_IPR_N1, IPR_N1
: NVIC_IPR16_IPR_N2 ( %bbbbbbbb -- x addr ) 16 lshift NVIC_IPR16 ; \ NVIC_IPR16_IPR_N2, IPR_N2
: NVIC_IPR16_IPR_N3 ( %bbbbbbbb -- x addr ) 24 lshift NVIC_IPR16 ; \ NVIC_IPR16_IPR_N3, IPR_N3
[then]

defined? use-NVIC defined? NVIC_IPR17_IPR_N0 not and [if]
\ NVIC_IPR17 (read-write) Reset:0x00000000
: NVIC_IPR17_IPR_N0 ( %bbbbbbbb -- x addr ) NVIC_IPR17 ; \ NVIC_IPR17_IPR_N0, IPR_N0
: NVIC_IPR17_IPR_N1 ( %bbbbbbbb -- x addr ) 8 lshift NVIC_IPR17 ; \ NVIC_IPR17_IPR_N1, IPR_N1
: NVIC_IPR17_IPR_N2 ( %bbbbbbbb -- x addr ) 16 lshift NVIC_IPR17 ; \ NVIC_IPR17_IPR_N2, IPR_N2
: NVIC_IPR17_IPR_N3 ( %bbbbbbbb -- x addr ) 24 lshift NVIC_IPR17 ; \ NVIC_IPR17_IPR_N3, IPR_N3
[then]

execute-defined? use-NVIC defined? NVIC_IPR18_IPR_N0 not and [if]
\ NVIC_IPR18 (read-write) Reset:0x00000000
: NVIC_IPR18_IPR_N0 ( %bbbbbbbb -- x addr ) NVIC_IPR18 ; \ NVIC_IPR18_IPR_N0, IPR_N0
: NVIC_IPR18_IPR_N1 ( %bbbbbbbb -- x addr ) 8 lshift NVIC_IPR18 ; \ NVIC_IPR18_IPR_N1, IPR_N1
: NVIC_IPR18_IPR_N2 ( %bbbbbbbb -- x addr ) 16 lshift NVIC_IPR18 ; \ NVIC_IPR18_IPR_N2, IPR_N2
: NVIC_IPR18_IPR_N3 ( %bbbbbbbb -- x addr ) 24 lshift NVIC_IPR18 ; \ NVIC_IPR18_IPR_N3, IPR_N3
[then]

defined? use-NVIC defined? NVIC_IPR19_IPR_N0 not and [if]
\ NVIC_IPR19 (read-write) Reset:0x00000000
: NVIC_IPR19_IPR_N0 ( %bbbbbbbb -- x addr ) NVIC_IPR19 ; \ NVIC_IPR19_IPR_N0, IPR_N0
: NVIC_IPR19_IPR_N1 ( %bbbbbbbb -- x addr ) 8 lshift NVIC_IPR19 ; \ NVIC_IPR19_IPR_N1, IPR_N1
: NVIC_IPR19_IPR_N2 ( %bbbbbbbb -- x addr ) 16 lshift NVIC_IPR19 ; \ NVIC_IPR19_IPR_N2, IPR_N2
: NVIC_IPR19_IPR_N3 ( %bbbbbbbb -- x addr ) 24 lshift NVIC_IPR19 ; \ NVIC_IPR19_IPR_N3, IPR_N3
[then]

execute-defined? use-NVIC defined? NVIC_IPR20_IPR_N0 not and [if]
\ NVIC_IPR20 (read-write) Reset:0x00000000
: NVIC_IPR20_IPR_N0 ( %bbbbbbbb -- x addr ) NVIC_IPR20 ; \ NVIC_IPR20_IPR_N0, IPR_N0
: NVIC_IPR20_IPR_N1 ( %bbbbbbbb -- x addr ) 8 lshift NVIC_IPR20 ; \ NVIC_IPR20_IPR_N1, IPR_N1
: NVIC_IPR20_IPR_N2 ( %bbbbbbbb -- x addr ) 16 lshift NVIC_IPR20 ; \ NVIC_IPR20_IPR_N2, IPR_N2
: NVIC_IPR20_IPR_N3 ( %bbbbbbbb -- x addr ) 24 lshift NVIC_IPR20 ; \ NVIC_IPR20_IPR_N3, IPR_N3
[then]

forth-module 1 set-order
forth-module set-current

compile-to-ram
