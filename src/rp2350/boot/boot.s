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

	.equ RAM_BASE, 0x20000000
	.equ FLASH_IMAGE_BASE, 0x10001000
	.equ IMAGE_SIZE, 0x8000
	.equ PADS_QSPI_BASE, 0x40020000
	.equ VTOR, 0xE000ED08
        .equ RSTACK_TOP, 0x20082000

	@ SPI clock divider
	.equ PICO_FLASH_SPI_CLKDIV, 4

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
	.word _handle_reset+1 	@ 1: the reset handler
	.word _handle_reset+1	@ 2: the NMI handler
	.word _handle_reset+1    @ 3: the hard fault handler
	.word _handle_reset+1  @ 4: the MPU fault handler
	.word _handle_reset+1  @ 5: the bus fault handler
	.word _handle_reset+1  @ 6: the usage fault handler
	.word 0               @ 7: reserved
	.word 0               @ 8: reserved
	.word 0               @ 9: reserved
	.word 0               @ 10: reserved
	.word _handle_reset+1   @ 11: SVCall handler
	.word _handle_reset+1   @ 12: debug handler
	.word 0               @ 13: reserved
	.word _handle_reset+1   @ 14: the PendSV handler
	.word _handle_reset+1  @ 15: the Systick handler
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
	@ Set stack pointer to the end of SRAM5 temporarily
	ldr r3, =RSTACK_TOP
	mov sp, r3

	@ Set up QSPI pads
	ldr r3, =PADS_QSPI_BASE
	movs r0, #(2 << PADS_QSPI_GPIO_QSPI_SCLK_DRIVE_LSB) | PADS_QSPI_GPIO_QSPI_SCLK_SLEWFAST_BITS
	str r0, [r3, #PADS_QSPI_GPIO_QSPI_SCLK_OFFSET]
	ldr r0, [r3, #PADS_QSPI_GPIO_QSPI_SD0_OFFSET]
	movs r1, #PADS_QSPI_GPIO_QSPI_SD0_SCHMITT_BITS
	bics r0, r1
	str r0, [r3, #PADS_QSPI_GPIO_QSPI_SD0_OFFSET]
	str r0, [r3, #PADS_QSPI_GPIO_QSPI_SD1_OFFSET]
	str r0, [r3, #PADS_QSPI_GPIO_QSPI_SD2_OFFSET]
	str r0, [r3, #PADS_QSPI_GPIO_QSPI_SD3_OFFSET]

	@@ Set the clock divider
	movs r1, #PICO_FLASH_SPI_CLKDIV
	str r1, [r0, #SSI_BAUDR_OFFSET]

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

	@ Wait for SSI to become ready
_wait_ssi_busy:
	push {r0, r1, r2, r3, lr}
	ldr r0, =XIP_SSI_BASE + SSI_SR_OFFSET
	movs r2, #4 @ TX FIFO empty
1:	ldr r3, [r0]
	tst r3, r2
	beq 1b
	tst r3, r5 @ BUSY
	bne 1b
	pop {r0, r1, r2, r3, pc}
