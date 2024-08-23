@ Copyright (c) 2021-2024 Travis Bemann
@ Copyright (c) 2021 Jan Bramkamp
@ Copyright (c) 2024 Paul Koning
@
@ Permission is hereby granted, free of charge, to any person obtaining a copy
@ of this software and associated documentation files (the "Software"), to deal
@ in the Software without restriction, including without limitation the rights
@ to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
@ copies of the Software, and to permit persons to whom the Software is
@ furnished to do so, subject to the following conditions:
@ 
@ The above copyright notice and this permission notice shall be included in
@ all copies or substantial portions of the Software.
@ 
@ THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
@ IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
@ FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
@ AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
@ LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
@ OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
@ SOFTWARE.


@ -----------------------------------------------------------------------------
@ Clocks
@ -----------------------------------------------------------------------------

.equ CLOCKS_BASE         , 0x40010000
.equ CLK_CTRL            , 0x00 @ Clock control, can be changed on-the-fly (except for auxsrc)
.equ CLK_DIV             , 0x04 @ Clock divisor, can be changed on-the-fly
.equ CLK_SELECTED        , 0x08 @ Indicates which src is currently selected (one-hot)

.equ CLK_GPOUT0          , 12 * 0
.equ CLK_GPOUT1          , 12 * 1
.equ CLK_GPOUT2          , 12 * 2
.equ CLK_GPOUT3          , 12 * 3
.equ CLK_REF             , 12 * 4
.equ CLK_SYS             , 12 * 5
.equ CLK_PERI            , 12 * 6
.equ CLK_HSTX            , 12 * 7        
.equ CLK_USB             , 12 * 8
.equ CLK_ADC             , 12 * 9

.equ CLK_GPOUT0_CTRL     , 0x00 @ Clock control, can be changed on-the-fly (except for auxsrc)
.equ CLK_GPOUT0_DIV      , 0x04 @ Clock divisor, can be changed on-the-fly
.equ CLK_GPOUT0_SELECTED , 0x08 @ Indicates which src is currently selected (one-hot)

.equ CLK_GPOUT1_CTRL     , 0x0c @ Clock control, can be changed on-the-fly (except for auxsrc)
.equ CLK_GPOUT1_DIV      , 0x10 @ Clock divisor, can be changed on-the-fly
.equ CLK_GPOUT1_SELECTED , 0x14 @ Indicates which src is currently selected (one-hot)

.equ CLK_GPOUT2_CTRL     , 0x18 @ Clock control, can be changed on-the-fly (except for auxsrc)
.equ CLK_GPOUT2_DIV      , 0x1c @ Clock divisor, can be changed on-the-fly
.equ CLK_GPOUT2_SELECTED , 0x20 @ Indicates which src is currently selected (one-hot)

.equ CLK_GPOUT3_CTRL     , 0x24 @ Clock control, can be changed on-the-fly (except for auxsrc)
.equ CLK_GPOUT3_DIV      , 0x28 @ Clock divisor, can be changed on-the-fly
.equ CLK_GPOUT3_SELECTED , 0x2c @ Indicates which src is currently selected (one-hot)

.equ CLK_REF_CTRL        , 0x30 @ Clock control, can be changed on-the-fly (except for auxsrc)
.equ CLK_REF_DIV         , 0x34 @ Clock divisor, can be changed on-the-fly
.equ CLK_REF_SELECTED    , 0x38 @ Indicates which src is currently selected (one-hot)

.equ CLK_SYS_CTRL        , 0x3c @ Clock control, can be changed on-the-fly (except for auxsrc)
.equ CLK_SYS_DIV         , 0x40 @ Clock divisor, can be changed on-the-fly
.equ CLK_SYS_SELECTED    , 0x44 @ Indicates which src is currently selected (one-hot)

.equ CLK_PERI_CTRL       , 0x48 @ Clock control, can be changed on-the-fly (except for auxsrc)
.equ CLK_PERI_SELECTED   , 0x50 @ Indicates which src is currently selected (one-hot)

.equ CLK_HSTX_CTRL       , 0x54 @ Clock control, can be changed on-the-fly (except for auxsrc)
.equ CLK_HSTX_DIV        , 0x58 @ Clock divisor
.equ CLK_HSTX_SELECTED   , 0x5c @ Indicates which src is currently selected (one-hot)
 
.equ CLK_USB_CTRL        , 0x60 @ Clock control, can be changed on-the-fly (except for auxsrc)
.equ CLK_USB_DIV         , 0x64 @ Clock divisor, can be changed on-the-fly
.equ CLK_USB_SELECTED    , 0x68 @ Indicates which src is currently selected (one-hot)

.equ CLK_ADC_CTRL        , 0x6C @ Clock control, can be changed on-the-fly (except for auxsrc)
.equ CLK_ADC_DIV         , 0x70 @ Clock divisor, can be changed on-the-fly
.equ CLK_ADC_SELECTED    , 0x74 @ Indicates which src is currently selected (one-hot)

.equ CLK_HSTX_CTRL       , 0x6c @ Clock control, can be changed on-the-fly (except for auxsrc)
.equ CLK_HSTX_DIV        , 0x70 @ Clock divisor, can be changed on-the-fly
.equ CLK_HSTX_SELECTED   , 0x74 @ Indicates which src is currently selected (one-hot)

.equ DFTCLK_XOSC_CTRL    , 0x78
.equ DFTCLK_ROSC_CTRL    , 0x7C
.equ DFTCLK_LPOSC_CTRL   , 0x80

.equ CLK_SYS_RESUS_CTRL  , 0x84
.equ CLK_SYS_RESUS_STATUS, 0x88

.equ FC0_REF_KHZ         , 0x8C @ Reference clock frequency in kHz
.equ FC0_MIN_KHZ         , 0x90 @ Minimum pass frequency in kHz. This is optional. Set to 0 if you are not using the pass/fail flags
.equ FC0_MAX_KHZ         , 0x94 @ Maximum pass frequency in kHz. This is optional. Set to 0x1ffffff if you are not using the pass/fail flags
.equ FC0_DELAY           , 0x98 @ Delays the start of frequency counting to allow the mux to settle Delay is measured in multiples of the reference clock period
.equ FC0_INTERVAL        , 0x9C @ The test interval is 0.98us * 2**interval, but let’s call it 1us * 2**interval The default gives a test interval of 250us
.equ FC0_SRC             , 0xA0 @ Clock sent to frequency counter, set to 0 when not required Writing to this register initiates the frequency count
.equ FC0_STATUS          , 0xA4 @ Frequency counter status
.equ FC0_RESULT          , 0xA8 @ Result of frequency measurement, only valid when status_done=1

.equ WAKE_EN0            , 0xAC @ enable clock in wake mode
.equ WAKE_EN1            , 0xB0 @ enable clock in wake mode
.equ SLEEP_EN0           , 0xB4 @ enable clock in sleep mode
.equ SLEEP_EN1           , 0xB8 @ enable clock in sleep mode
.equ ENABLED0            , 0xBC @ indicates the state of the clock enable
.equ ENABLED1            , 0xC0 @ indicates the state of the clock enable
.equ INTR                , 0xC4 @ Raw Interrupts
.equ INTE                , 0xC8 @ Interrupt Enable
.equ INTF                , 0xCC @ Interrupt Force
.equ INTS                , 0xD0 @ Interrupt status after masking & forcing

@ -----------------------------------------------------------------------------
@ Crystal Oscillator
@ -----------------------------------------------------------------------------

.equ XOSC_BASE   , 0x40048000

.equ XOSC_CTRL   , 0x00 @ Crystal Oscillator Control
.equ XOSC_STATUS , 0x04 @ Crystal Oscillator Status
.equ XOSC_DORMANT, 0x08 @ Crystal Oscillator pause control
.equ XOSC_STARTUP, 0x0c @ Controls the startup delay
.equ XOSC_COUNT  , 0x1c @ A down counter running at the XOSC frequency which counts to zero and stops.

.equ XOSC_ENABLE_12MHZ, 0xfabaa0
.equ XOSC_DELAY       , 47 @ ceil((f_crystal * t_stable) / 256)

@ -----------------------------------------------------------------------------
@ PLLs
@ -----------------------------------------------------------------------------

.equ PLL_SYS_BASE, 0x40050000
.equ PLL_USB_BASE, 0x40058000
.equ PLL_CS      , 0x0
.equ PLL_PWR     , 0x4
.equ PLL_FBDIV_INT   , 0x8
.equ PLL_PRIM    , 0xc
.equ PLL_VCOPD   , 5
.equ PLL_PD      , 0
.equ PLL_POSTDIV1, 16
.equ PLL_POSTDIV2, 12
.equ PLL_CS_LOCK, 1 << 31
.equ PLL_START   , (1<<PLL_VCOPD) | (1<<PLL_PD)
.equ PLL_SYS_DIV , (5<<PLL_POSTDIV1) | (2<<PLL_POSTDIV2)
.equ PLL_USB_DIV , (5<<PLL_POSTDIV1) | (4<<PLL_POSTDIV2)

@ -----------------------------------------------------------------------------
@ UARTs
@ -----------------------------------------------------------------------------

.equ UART0_BASE, 0x40070000
.equ UART1_BASE, 0x40078000

.equ UARTDR   , 0x00 @ Data Register, UARTDR
.equ UARTRSR  , 0x04 @ Receive Status Register/Error Clear Register, UARTRSR/UARTECR
.equ UARTFR   , 0x18 @ Flag Register, UARTFR
.equ UARTILPR , 0x20 @ IrDA Low-Power Counter Register, UARTILPR
.equ UARTIBRD , 0x24 @ Integer Baud Rate Register, UARTIBRD
.equ UARTFBRD , 0x28 @ Fractional Baud Rate Register, UARTFBRD
.equ UARTLCR_H, 0x2c @ Line Control Register, UARTLCR_H
.equ UARTCR   , 0x30 @ Control Register, UARTCR
.equ UARTIFLS , 0x34 @ Interrupt FIFO Level Select Register, UARTIFLS
.equ UARTIMSC , 0x38 @ Interrupt Mask Set/Clear Register, UARTIMSC
.equ UARTRIS  , 0x3c @ Raw Interrupt Status Register, UARTRIS
.equ UARTMIS  , 0x40 @ Masked Interrupt Status Register, UARTMIS
.equ UARTICR  , 0x44 @ Interrupt Clear Register, UARTICR
.equ UARTDMACR, 0x48 @ DMA Control Register, UARTDMACR

.equ UART_8N1, 3 << 5
.equ UART_FIFO, 1 << 4
.equ UART_ENABLE, 1<<9|1<<8|1<<0
.equ UART_DMA, 1<<2|1<<1

.equ UART0_IBAUD, 81
.equ UART0_FBAUD, 24

@ -----------------------------------------------------------------------------

.equ IO_BANK0_BASE  , 0x40028000
.equ PADS_BANK0_BASE, 0x40038000
.equ SIO_BASE       , 0xd0000000

.equ PSM_BASE       , 0x40018000
.equ PSM_FRCE_OFF   , PSM_BASE + 0x4
.equ PSM_FRCE_OFF_PROC1, 1 << 24	

.equ WAKE_EN0, CLOCKS_BASE + 0x000000AC

.equ GPIO_0_STATUS,  IO_BANK0_BASE + (8 *  0)
.equ GPIO_0_CTRL,    IO_BANK0_BASE + (8 *  0) + 4
.equ GPIO_1_STATUS,  IO_BANK0_BASE + (8 *  1)
.equ GPIO_1_CTRL,    IO_BANK0_BASE + (8 *  1) + 4
.equ GPIO_25_STATUS, IO_BANK0_BASE + (8 * 25)
.equ GPIO_25_CTRL,   IO_BANK0_BASE + (8 * 25) + 4

@ .equ GPIO_25_PAD,     PADS_BANK0_BASE + 0x68 @ Darin kann ich elektrische Eigenschaften einstellen

.equ CPUID          , 0x000 @ Processor core identifier
.equ GPIO_IN        , 0x004 @ Input value for GPIO pins
.equ GPIO_HI_IN     , 0x008 @ Input value for QSPI pins
.equ GPIO_OUT       , 0x010 @ GPIO output value
.equ GPIO_HI_OUT    , 0x014 @ GPIO output value for GPIO's > 31
.equ GPIO_OUT_SET   , 0x018 @ GPIO output value set
.equ GPIO_HI_OUT_SET, 0x01C @ GPIO output value set for GPIO's > 31
.equ GPIO_OUT_CLR   , 0x020 @ GPIO output value clear
.equ GPIO_HI_OUT_CLR, 0x024 @ GPIO output value clear for GPIO's > 31
.equ GPIO_OUT_XOR   , 0x028 @ GPIO output value XOR
.equ GPIO_HI_OUT_XOR, 0x02C @ GPIO output value XOR for GPIO's > 31
.equ GPIO_OE        , 0x030 @ GPIO output enable
.equ GPIO_HI_OE     , 0x034 @ GPIO output enable for GPIO's > 31
.equ GPIO_OE_SET    , 0x038 @ GPIO output enable set
.equ GPIO_HI_OE_SET , 0x03C @ GPIO output enable set for GPIO's > 31
.equ GPIO_OE_CLR    , 0x040 @ GPIO output enable clear
.equ GPIO_HI_OE_CLR , 0x044 @ GPIO output enable clear for GPIO's > 31
.equ GPIO_OE_XOR    , 0x048 @ GPIO output enable XOR
.equ GPIO_HI_OE_XOR , 0x04C @ GPIO output enable XOR for GPIO's > 31

.equ RESETS_BASE, 0x40020000
.equ RESET      , 0
.equ WDSEL      , 4
.equ RESET_DONE , 8

.equ ALIAS_RW , 0<<12
.equ ALIAS_XOR, 1<<12
.equ ALIAS_SET, 2<<12
.equ ALIAS_CLR, 3<<12

.equ RESETS_USBCTRL   , 28
.equ RESETS_UART1     , 27
.equ RESETS_UART0     , 26
.equ RESETS_TRNG      , 25        
.equ RESETS_TIMER1    , 24
.equ RESETS_TIMER0    , 23
.equ RESETS_TBMAN     , 22
.equ RESETS_SYSINFO   , 21
.equ RESETS_SYSCFG    , 20
.equ RESETS_SPI1      , 19
.equ RESETS_SPI0      , 18
.equ RESETS_SHA256    , 17
.equ RESETS_PWM       , 16
.equ RESETS_PLL_USB   , 15
.equ RESETS_PLL_SYS   , 14
.equ RESETS_PIO2      , 13
.equ RESETS_PIO1      , 12
.equ RESETS_PIO0      , 11
.equ RESETS_PADS_QSPI , 10
.equ RESETS_PADS_BANK0, 9
.equ RESETS_JTAG      , 8
.equ RESETS_IO_QSPI   , 7
.equ RESETS_IO_BANK0  , 6
.equ RESETS_I2C1      , 5
.equ RESETS_I2C0      , 4
.equ RESETS_HSTX      , 3
.equ RESETS_DMA       , 2
.equ RESETS_BUSCTRL   , 1
.equ RESETS_ADC       , 0
.equ RESETS_ALL       , 0x1FFFFFFF & ~(1<<RESETS_HSTX)
.equ RESETS_EARLY     , RESETS_ALL & ~(1<<RESETS_IO_QSPI) & ~(1<<RESETS_PADS_QSPI) & ~(1<<RESETS_PLL_USB) & ~(1<<RESETS_PLL_SYS)
.equ RESETS_CLK_GLMUX , RESETS_ALL & ~(1<<RESETS_ADC) & ~(1<<RESETS_SPI0) & ~(1<<RESETS_SPI1) & ~(1<<RESETS_UART0) & ~(1<<RESETS_UART1) & ~(1<<RESETS_USBCTRL)
.equ RESETS_PLLS      , (1<<RESETS_PLL_USB) | (1<<RESETS_PLL_SYS)

.equ XOSC_MHZ         , 12
.equ WATCHDOG_BASE    , 0x400D8000
.equ WATCHDOG_CTRL    , 0x00
.equ WATCHDOG_LOAD    , 0x04
.equ WATCHDOG_REASON  , 0x08
.equ WATCHDOG_SCRATCH0, 0x0c
.equ WATCHDOG_SCRATCH1, 0x10
.equ WATCHDOG_SCRATCH2, 0x14
.equ WATCHDOG_SCRATCH3, 0x18
.equ WATCHDOG_SCRATCH4, 0x1c
.equ WATCHDOG_SCRATCH5, 0x20
.equ WATCHDOG_SCRATCH6, 0x24
.equ WATCHDOG_SCRATCH7, 0x28

        .equ TICKS_BASE,            0x40108000
        .equ TICKS_PROC0_CTRL,      0x00
        .equ TICKS_PROC0_CYCLES,    0x04
        .equ TICKS_PROC0_COUNT,     0x08
        .equ TICKS_PROC1_CTRL,      0x0C
        .equ TICKS_PROC1_CYCLES,    0x10
        .equ TICKS_PROC1_COUNT,     0x14
        .equ TICKS_TIMER0_CTRL,     0x18
        .equ TICKS_TIMER0_CYCLES,   0x1C
        .equ TICKS_TIMER0_COUNT,    0x20
        .equ TICKS_TIMER1_CTRL,     0x24
        .equ TICKS_TIMER1_CYCLES,   0x28
        .equ TICKS_TIMER1_COUNT,    0x2C
        .equ TICKS_WATCHDOG_CTRL,   0x30
        .equ TICKS_WATCHDOG_CYCLES, 0x34
        .equ TICKS_WATCHDOG_COUNT,  0x38

        .equ TICKS_CTRL_ENABLE,     1 << 0
        .equ TICKS_CTRL_RUNNING,    1 << 1
        .equ INIT_TICKS_CYCLES,     12
        
@ -----------------------------------------------------------------------------
_init_hardware: @ Many thanks to Jan Bramkamp
@ -----------------------------------------------------------------------------

	// Reset as much as possible.
	// * We have to keep the QSPI flash XIP working
	// * We have to leave the PLLs feeding into glitching muxes running
	ldr  r1, =RESETS_BASE|ALIAS_SET
	ldr  r0, =RESETS_EARLY
	str  r0, [r1, #RESET]

	// Start everything that's clocked by clk_sys and clk_ref clocks.
	// These clocks contain glitchfree muxes allowing us to switch clock sources
	// without stopping everything clocked by them.
	ldr  r1, =RESETS_BASE|ALIAS_CLR
	ldr  r0, =RESETS_CLK_GLMUX
	str  r0, [r1, #RESET]

	// Wait for the peripherals to return from reset
	ldr  r1, =RESETS_BASE
1:	ldr  r2, [r1, #RESET_DONE]
	mvns r2, r2
	ands r2, r0
	bne  1b

Disable_Resus:
	movs r0, 0
	ldr	 r1, =CLOCKS_BASE
	str  r0, [r1, #CLK_SYS_RESUS_CTRL]

XOSC_Init:
	ldr  r1, =XOSC_BASE
	movs r0, XOSC_DELAY
	str  r0, [r1, #XOSC_STARTUP]

	ldr  r0, =XOSC_ENABLE_12MHZ
	str  r0, [r1, #XOSC_CTRL]

1:	ldr  r0, [r1, #XOSC_STATUS] @ Wait for stable flag (in MSB)
	adds r0, #0
	bpl  1b

Switch_From_Aux:
	ldr  r1, =CLOCKS_BASE
	ldr  r2, =CLOCKS_BASE|ALIAS_CLR

	movs r0, 1
	str  r0, [r2, #CLK_SYS_CTRL]
1:	ldr  r0, [r1, #CLK_SYS_SELECTED]
	cmp  r0, #1
	bne  1b

	movs r0, 3
	str  r0, [r2, #CLK_REF_CTRL]
1:	ldr  r0, [r1, #CLK_REF_SELECTED]
	cmp  r0, #1
	bne  1b

Reset_PLLs:
	ldr  r1, =RESETS_BASE|ALIAS_SET
	ldr  r0, =RESETS_PLLS
	str  r0, [r1, #RESET]
	ldr  r1, =RESETS_BASE|ALIAS_CLR
	str  r0, [r1, #RESET]
	ldr  r1, =RESETS_BASE
1:	ldr	 r2, [r1, #RESET_DONE]
	ands r2, r0
	cmp  r2, r0
	bne	 1b

Init_PLLs:
	// Disable PLLs
	movs r1, #0
	subs r0, r1, #1
	ldr  r2, =PLL_SYS_BASE
	ldr  r3, =PLL_USB_BASE
	str  r0, [r2, #PLL_PWR]
	str  r0, [r3, #PLL_PWR]
	str  r1, [r2, #PLL_FBDIV_INT]
	str  r1, [r3, #PLL_FBDIV_INT]

	// Don't divide the crystal frequency
	movs r0, #1
	str  r0, [r2, #PLL_CS]
	str  r0, [r3, #PLL_CS]

	// SYS: VCO = 12MHz * 125 = 1500MHz
	// USB: VCO = 12MHz *  80 =  960MHz
	movs r0, #125
	str  r0, [r2, #PLL_FBDIV_INT]
	movs r0, #80
	str  r0, [r3, #PLL_FBDIV_INT]

	// Start PLLs
	movs r0, #PLL_START
	mvns r0, r0
	str  r0, [r2, #PLL_PWR]
	str  r0, [r3, #PLL_PWR]

	// Wait until both PLLs are locked
1:	ldr  r0, [r2, #PLL_CS]
	ldr  r1, [r3, #PLL_CS]
	ands r0, r1
	bpl  1b

	// Set the PLL post dividers
	ldr  r0, =PLL_SYS_DIV
	ldr  r1, =PLL_USB_DIV
	str  r0, [r2, #PLL_PRIM]
	str  r1, [r3, #PLL_PRIM]
	movs r0, #0
	str  r0, [r2, #PLL_PWR]
	str  r0, [r3, #PLL_PWR]

Init_Clk_Ref:
	// Switch the glitchless mux to the XOSC source.
	ldr  r0, =CLOCKS_BASE
	ldr  r1, [r0, #CLK_REF_CTRL]
	movs r2, #2
	orrs r1, r2
	subs r2, #4
	ands r1, r2
	str  r1, [r0, #CLK_REF_CTRL]

	// Wait for the clock to switch to the selected source
1:	ldr  r1, [r0, #CLK_REF_SELECTED]
	cmp  r1, #1<<2
	bne  1b

	// Don't divide the reference clock
	lsls r1, #6
	str  r1, [r0, #CLK_REF_DIV]

Init_Clk_Sys:
	// Switch clk_sys to clk_ref (only clock not routed through the glitching mux)
	ldr  r1, =CLOCKS_BASE|ALIAS_CLR
	movs r2, #1
	str  r2, [r1, #CLK_SYS_CTRL]

	// Wait for the clock to switch
1:	ldr  r2, [r0, #CLK_SYS_SELECTED]
	cmp  r2, #1<<0
	bne  1b

	// Switch the glitching mux to the system PLL
	movs r2, #0
	str  r2, [r0, #CLK_SYS_CTRL]
	movs r2, #1
	str  r2, [r0, #CLK_SYS_CTRL]

	// Don't divide the system clock
	lsls r2, #8
	str  r2, [r0, #CLK_SYS_DIV]

Init_Clk_USB:
	// Disable the USB clock
	lsls r2, #11-8
	str  r2, [r1, #CLK_USB_CTRL]

	// Wait for the clock to stop
	movs r3, #4 @ ceil(150 MHz / 48 MHz)
1:	subs r3, 1
	bne  1b

	// Select the USB PLL as auxiliary clock source (reuses the zero in r3)
	str  r3, [r0, #CLK_USB_CTRL]

	// (Re-)start the USB clock (only the enable flag is set)
	str  r2, [r0, #CLK_USB_CTRL]

	// Don't divide the USB clock
	lsrs r2, #11-8
	str  r2, [r0, #CLK_USB_DIV]

Init_Clk_ADC:
	// Stop the ADC clock
	lsls r2, #11-8
	str  r2, [r1, #CLK_ADC_CTRL]

	// Wait for the clock to stop
	movs r3, #4 @ ceil(150 MHz / 48 MHz)
1:	subs r3, 1
	bne  1b

	// Select the USB PLL as auxiliary clock source (reuses the zero in r3)
	str  r3, [r0, #CLK_ADC_CTRL]

	// (Re-)start the ADC clock (only the enable flag is set)
	str  r2, [r0, #CLK_ADC_CTRL]

	// Don't divide the ADC clock
	lsrs r2, #11-8
	str  r2, [r0, #CLK_ADC_DIV]

Init_Clk_Peri:
	ldr  r2, =1<<11
	str  r2, [r0, #CLK_PERI_CTRL]

Unreset_All:
	// We did the clock dance for a reason
        movs r3, #0
	ldr  r1, =RESETS_BASE
	str  r3, [r1, #RESET]

enable_tick_generators:
        ldr r0, =TICKS_BASE | ALIAS_CLR
        ldr r1, =TICKS_CTRL_ENABLE
        str r1, [r0, #TICKS_PROC0_CTRL]
@        str r1, [r0, #TICKS_PROC1_CTRL]
        str r1, [r0, #TICKS_TIMER0_CTRL]
        str r1, [r0, #TICKS_TIMER1_CTRL]
        str r1, [r0, #TICKS_WATCHDOG_CTRL]
        
        ldr r0, =TICKS_BASE
        ldr r1, =TICKS_CTRL_RUNNING
1:      ldr r2, [r0, #TICKS_PROC0_CTRL]
        tst r1, r2
        bne 1b
@        ldr r2, [r0, #TICKS_PROC1_CTRL]
@        tst r1, r2
@        bne 1b
        ldr r2, [r0, #TICKS_TIMER0_CTRL]
        tst r1, r2
        bne 1b
        ldr r2, [r0, #TICKS_TIMER1_CTRL]
        tst r1, r2
        bne 1b
        ldr r2, [r0, #TICKS_WATCHDOG_CTRL]
        tst r1, r2
        bne 1b
        
        ldr r0, =TICKS_BASE
        ldr r1, =INIT_TICKS_CYCLES
        str r1, [r0, #TICKS_PROC0_CYCLES]
@        str r1, [r0, #TICKS_PROC1_CYCLES]
        str r1, [r0, #TICKS_TIMER0_CYCLES]
        str r1, [r0, #TICKS_TIMER1_CYCLES]
        str r1, [r0, #TICKS_WATCHDOG_CYCLES]

        ldr r0, =TICKS_BASE | ALIAS_SET
        ldr r1, =TICKS_CTRL_ENABLE
        str r1, [r0, #TICKS_PROC0_CTRL]
@        str r1, [r0, #TICKS_PROC1_CTRL]
        str r1, [r0, #TICKS_TIMER0_CTRL]
        str r1, [r0, #TICKS_TIMER1_CTRL]
        str r1, [r0, #TICKS_WATCHDOG_CTRL]
        
        ldr r0, =TICKS_BASE
        ldr r1, =TICKS_CTRL_RUNNING
1:      ldr r2, [r0, #TICKS_PROC0_CTRL]
        tst r1, r2
        beq 1b
@        ldr r2, [r0, #TICKS_PROC1_CTRL]
@        tst r1, r2
@        bne 1b
        ldr r2, [r0, #TICKS_TIMER0_CTRL]
        tst r1, r2
        beq 1b
        ldr r2, [r0, #TICKS_TIMER1_CTRL]
        tst r1, r2
        beq 1b
        ldr r2, [r0, #TICKS_WATCHDOG_CTRL]
        tst r1, r2
        beq 1b

UART_Baudrate:
	ldr  r0, =UART0_BASE
	movs r1, #UART0_IBAUD
	str  r1, [r0, #UARTIBRD]
	movs r1, #UART0_FBAUD
	str  r1, [r0, #UARTFBRD]
	movs r1, #UART_8N1 | UART_FIFO
	str  r1, [r0, #UARTLCR_H]

UART_Enable:
	ldr  r1, =UART_ENABLE
	str  r1, [r0, #UARTCR]
//	movs r1, #UART_DMA
//	str  r1, [r0, #UARTDMACR]

UART_Function:
	movs r0, 2
	ldr  r1, =IO_BANK0_BASE
	str  r0, [r1, #4+0]
	str  r0, [r1, #4+8]

Enable_GPIO:

        ldr  r0, =SIO_BASE
        movs r1, #0 @ All pins inputs
        str  r1, [r0, #GPIO_OE]
        ldr  r1, =0xFFFF
        ldr  r2, [r0, #GPIO_HI_OE]
        bics r2, r1
        str  r2, [r0, #GPIO_HI_OE]

        @ Turn off isolation for all GPIO's
        ldr r0, =PADS_BANK0_BASE + 4
        ldr r1, =(PADS_BANK0_BASE + 4) + (48 * 4)
        ldr r2, =1 << 8
1:      ldr r3, [r0]
        bics r3, r2
        str r3, [r0]
        adds r0, #4
        cmp r0, r1
        bne 1b

        @ Set all GPIO's except 0 and 1 to SIO function (5)
        ldr r0, =IO_BANK0_BASE + 4 + 16
        ldr r1, =(IO_BANK0_BASE + 4) + (48 * 8)
        movs r2, #5
1:      str r2, [r0]
        adds r0, #8
        cmp r0, r1
        bne 1b

        bx lr

	@@ Time multiplier
	define_word "time-multiplier", visible_flag
_time_multiplier:
	push_tos
	movs tos, #1
	bx lr
	end_inlined

	@@ Time divisor
	define_word "time-divisor", visible_flag
_time_divisor:
	push_tos
	movs tos, #1
	bx lr
	end_inlined

	@@ Systick divisor
	define_word "systick-divisor", visible_flag
_systick_divisor:
	push_tos
	movs tos, #10
	bx lr
	end_inlined

	@@ Crystal oscillator frequency
	define_word "xosc-frequency", visible_flag
_xosc_freq:
	push_tos
	ldr tos, =XOSC_MHZ * 1000000
	bx lr
	end_inlined

	.ltorg
	
