@ Copyright (c) 2021 Travis Bemann
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
	.cpu cortex-m0plus
	.thumb

	.equ RAM_BASE 0x20000000
	.equ FLASH_IMAGE_BASE 0x10001000
	.equ IMAGE_SIZE 0x8000
	.equ XIP_SSI_BASE 0x18000000
	.equ SSI_CTRLR0_OFFSET 0x00
	.equ SSI_CTRLR1_OFFSET 0x04
	.equ SSI_SSIENR_OFFSET 0x08
	.equ SSI_BAUDR_OFFSET 0x14
	.equ SSI_SPI_CTRLR0_OFFSET 0xF4
	.equ VTOR 0xE0000ED08

	@ Read
	.equ CMD_READ 0x03

	@ Standard 1-bit SPI serial frames (0 << 21)
	@ 32 clocks per data frame (31 << 16)
	@ Send instruction and address, receive data (3 << 8)
	.equ CTRLR0_INIT_XIP ((0 << 21) | (31 << 16) | (3 << 8))

	@ Execute a Read instruction (CMD_READ << 24)
	@ 8 bit command prefix (2 << 8)
	@ 24 address and mode bits (6 << 2)
	@ Command and address are both serial (0 << 0)
	.equ SPI_CTRLR0_INIT_XIP ((CMD_READ << 24) | (2 << 8) | (6 << 2) | (0 << 0))

	.text

	ldr r0, =XIP_SSI_BASE

	@@ Disable SSI
	movs r1, #0
	str r1, [r0, #SSI_SSIENR_OFFSET]

	@@ Set the clock divider
	movs r1, #PICO_FLASH_SPI_CLKDIV
	str r1, [r0, #SSI_BAUDR_OFFSET]

	@@ Configure XIP
	ldr r1, =CTRLR0_INIT_XIP
	str r1, [r0, #SSI_CTRLR0_OFFSET]
	ldr r1, =SPI_CTRLR0_INIT_XIP
	ldr r2, =XIP_SSI_BASE + SSI_SPI_CTRLR0_OFFSET
	str r1, [r2]
	movs r1, #0
	str r1, [r3, #SSI_CTRLR1_OFFSET]

	@@ Enable SSI
	movs r1, #1
	str r1, [r0, #SSI_SSIENR_OFFSET]

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
	
