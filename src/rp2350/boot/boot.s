@ Copyright (c) 2021-2024 Travis Bemann
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

	.syntax unified
	.cpu cortex-m33
	.thumb

        .equ SIO_BASE, 0xD0000000
        .equ IO_BANK0_BASE, 0x40028000
        .equ PADS_BANK0_BASE, 0x40038000
        .equ GPIO_OUT_SET_OFFSET, 0x018
        .equ GPIO_OE_SET_OFFSET, 0x038
        .equ GPIO25, 1 << 25
        .equ GPIO25_CTRL, (25 << 3) + IO_BANK0_BASE + 4
        .equ PAD_BANK0_GPIO25, (25 << 2) + PADS_BANK0_BASE + 4
	.equ RAM_BASE, 0x20000000
	.equ FLASH_IMAGE_BASE, 0x10001000
	.equ IMAGE_SIZE, 0x8000
	.equ PADS_QSPI_BASE, 0x40020000
	.equ VTOR, 0xE000ED08
        .equ RSTACK_TOP, 0x20082000

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
        .equ RESETS_ALL       , 0x1FFFFFFF
        .equ RESETS_EARLY     , RESETS_ALL & ~(1<<RESETS_IO_QSPI) & ~(1<<RESETS_PADS_QSPI) & ~(1<<RESETS_PLL_USB) & ~(1<<RESETS_PLL_SYS)
        .equ RESETS_CLK_GLMUX , RESETS_ALL & ~(1<<RESETS_ADC) & ~(1<<RESETS_SPI0) & ~(1<<RESETS_SPI1) & ~(1<<RESETS_UART0) & ~(1<<RESETS_UART1) & ~(1<<RESETS_USBCTRL)

        .equ XOSC_BASE   , 0x40048000

        .equ XOSC_CTRL   , 0x00 @ Crystal Oscillator Control
        .equ XOSC_STATUS , 0x04 @ Crystal Oscillator Status
        .equ XOSC_DORMANT, 0x08 @ Crystal Oscillator pause control
        .equ XOSC_STARTUP, 0x0c @ Controls the startup delay
        .equ XOSC_COUNT  , 0x1c @ A down counter running at the XOSC frequency which counts to zero and stops.

        .equ XOSC_ENABLE_12MHZ, 0xfabaa0
        .equ XOSC_DELAY       , 47 @ ceil((f_crystal * t_stable) / 256)

        .equ CLOCKS_BASE         , 0x40010000
        .equ CLK_REF_CTRL        , 0x30 @ Clock control, can be changed on-the-fly (except for auxsrc)
        .equ CLK_PERI_CTRL       , 0x48 @ Clock control, can be changed on-the-fly (except for auxsrc)

	@ Pad control constants
	.equ PADS_QSPI_GPIO_QSPI_SCLK_DRIVE_LSB, 4
	.equ PADS_QSPI_GPIO_QSPI_SCLK_SLEWFAST_BITS, 0x01
	.equ PADS_QSPI_GPIO_QSPI_SCLK_OFFSET, 0x04
	.equ PADS_QSPI_GPIO_QSPI_SD0_OFFSET, 0x08
	.equ PADS_QSPI_GPIO_QSPI_SD1_OFFSET, 0x0C
	.equ PADS_QSPI_GPIO_QSPI_SD2_OFFSET, 0x10
	.equ PADS_QSPI_GPIO_QSPI_SD3_OFFSET, 0x14
	.equ PADS_QSPI_GPIO_QSPI_SD0_SCHMITT_BITS, 0x02

        .text
	.word RSTACK_TOP
	.word _handle_reset+1 @ 1: the reset handler
	.word _handle_reset+1 @ 2: the NMI handler
	.word _handle_reset+1 @ 3: the hard fault handler
	.word _handle_reset+1 @ 4: the MPU fault handler
	.word _handle_reset+1 @ 5: the bus fault handler
	.word _handle_reset+1 @ 6: the usage fault handler
	.word _handle_reset+1 @ 7: reserved
	.word _handle_reset+1 @ 8: reserved
	.word _handle_reset+1 @ 9: reserved
	.word _handle_reset+1 @ 10: reserved
	.word _handle_reset+1 @ 11: SVCall handler
	.word _handle_reset+1 @ 12: debug handler
	.word _handle_reset+1 @ 13: reserved
	.word _handle_reset+1 @ 14: the PendSV handler
	.word _handle_reset+1 @ 15: the Systick handler
	.word _handle_reset+1 @ 16
	.word _handle_reset+1 @ 17
	.word _handle_reset+1 @ 18
	.word _handle_reset+1 @ 19
	.word _handle_reset+1 @ 20
	.word _handle_reset+1 @ 21
	.word _handle_reset+1 @ 22
	.word _handle_reset+1 @ 23
	.word _handle_reset+1 @ 24
	.word _handle_reset+1 @ 25
	.word _handle_reset+1 @ 26
	.word _handle_reset+1 @ 27
	.word _handle_reset+1 @ 28
	.word _handle_reset+1 @ 29
	.word _handle_reset+1 @ 30
	.word _handle_reset+1 @ 31
	.word _handle_reset+1 @ 32
	.word _handle_reset+1 @ 33
	.word _handle_reset+1 @ 34
	.word _handle_reset+1 @ 35
	.word _handle_reset+1 @ 36
	.word _handle_reset+1 @ 37
	.word _handle_reset+1 @ 38
	.word _handle_reset+1 @ 39
	.word _handle_reset+1 @ 40
	.word _handle_reset+1 @ 41
	.word _handle_reset+1 @ 42
	.word _handle_reset+1 @ 43
	.word _handle_reset+1 @ 44
	.word _handle_reset+1 @ 45
	.word _handle_reset+1 @ 46
	.word _handle_reset+1 @ 47
	.word _handle_reset+1 @ 48
        .word _handle_reset+1 @ 49
        .word _handle_reset+1 @ 50
        .word _handle_reset+1 @ 51
        .word _handle_reset+1 @ 52
        .word _handle_reset+1 @ 53
        .word _handle_reset+1 @ 54
        .word _handle_reset+1 @ 55
        .word _handle_reset+1 @ 56
        .word _handle_reset+1 @ 57
        .word _handle_reset+1 @ 58
        .word _handle_reset+1 @ 59
        .word _handle_reset+1 @ 60
        .word _handle_reset+1 @ 61
        .word _handle_reset+1 @ 62
        .word _handle_reset+1 @ 63
        .word _handle_reset+1 @ 64
        .word _handle_reset+1 @ 65
        .word _handle_reset+1 @ 66
        .word _handle_reset+1 @ 67

_handle_reset:

@        	// Reset as much as possible.
@	// * We have to keep the QSPI flash XIP working
@	// * We have to leave the PLLs feeding into glitching muxes running
@	ldr  r1, =RESETS_BASE|ALIAS_SET
@	ldr  r0, =RESETS_EARLY
@	str  r0, [r1, #RESET]
@
@	// Start everything that's clocked by clk_sys and clk_ref clocks.
@	// These clocks contain glitchfree muxes allowing us to switch clock sources
@	// without stopping everything clocked by them.
@	ldr  r1, =RESETS_BASE|ALIAS_CLR
@	ldr  r0, =RESETS_EARLY
@	str  r0, [r1, #RESET]
@
@	// Wait for the peripherals to return from reset
@	ldr  r1, =RESETS_BASE
@1:	ldr  r2, [r1, #RESET_DONE]
@	mvns r2, r2
@	ands r2, r0
@	bne  1b
@
@
@

        ldr r0, =RESETS_BASE
        movs r1, #0
        str r1, [r0, #RESET]

        ldr r6, =XOSC_BASE
        @ Activate XOSC

        ldr r5, =XOSC_DELAY
        str  r5, [r6, #XOSC_STARTUP]

        ldr r3, =XOSC_ENABLE_12MHZ
        str  r3, [r6, #XOSC_CTRL] @ r3 = XOSC_ENABLE_12MHZ

1:      ldr  r0, [r6, #XOSC_STATUS] @ Wait for stable flag (in MSB)
        asrs r0, r0, 31
        bpl  1b

        @ Select XOSC as source for clk_ref, which is the clock source of everything in reset configuration

        ldr r4, =CLOCKS_BASE
        movs r1, #2
        str  r1, [r4, #CLK_REF_CTRL] @ r1 = 2, r4 = CLOCKS_BASE

        @ Light up the LED to signal we got this far

        ldr r0, =PAD_BANK0_GPIO25
        ldr r1, [r0]
        ldr r2, =1 << 8
        bics r1, r2
        str r1, [r0]
        
@        ldr r0, =GPIO25_CTRL
@        movs r1, #5 @ SIO
@        str r1, [r0]
@        ldr r0, =SIO_BASE
@        ldr r1, =GPIO25
@        str r1, [r0, #GPIO_OE_SET_OFFSET]
@        str r1, [r0, #GPIO_OUT_SET_OFFSET]

        @ Pause so the user can see the LED
@        ldr r0, =0x00FFFFFF
@1:      subs r0, #1
@        cmp r0, #0
@        bne 1b

	@@ Copy the image from flash into RAM
	ldr r0, =RAM_BASE
	ldr r1, =FLASH_IMAGE_BASE
	ldr r2, =IMAGE_SIZE
1:	subs r2, #4
	ldr r3, [r1, r2]
	str r3, [r0, r2]
	bne 1b

	@@ Set up interrupts
	ldr r1, =VTOR
	str r0, [r1]

	@ Set up the return stack
	ldr r1, [r0]
	mov sp, r1

	@ Start zeptoforth
	ldr r1, [r0, #4]
	bx r1

        @ IMAGE_DEF
        .p2align 2
        .word 0xFFFFDED3
        .word 0x10210142
        .word 0x000001FF
        .word 0x00000000
        .word 0xAB123579
