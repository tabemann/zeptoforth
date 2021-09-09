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
	.equ PADS_QSPI_BASE 0x40020000
	.equ XIP_SSI_BASE 0x18000000
	.equ SSI_CTRLR0_OFFSET 0x00
	.equ SSI_CTRLR1_OFFSET 0x04
	.equ SSI_SSIENR_OFFSET 0x08
	.equ SSI_BAUDR_OFFSET 0x14
	.equ SSI_SR_OFFSET 0x28
	.equ SSI_DR0_OFFSET 0x60 @ 36 words
	.equ SSI_SPI_CTRLR0_OFFSET 0xF4
	.equ VTOR 0xE0000ED08

	@ Commands
	.equ CMD_WRITE_STATUS 0x01
	.equ CMD_READ_DATA_FAST_QUAD_IO 0xEB @
	.equ CMD_READ_STATUS 0x05
	.equ CMD_WRITE_ENABLE 0x06
	.equ CMD_READ_STATUS_2 0x35
	.equ CMD_CONT_READ 0xA0

	@ QSPI Enable state
	.equ QSPI_ENABLE_STATE 0x02

	@ Write busy state
	.equ WRITE_BUSY_STATE 0x01

	@ Wait cycles
	.equ WAIT_CYCLES 4

	@ Pad control constants
	.equ PADS_QSPI_GPIO_QSPI_SCLK_DRIVE_LSB 4
	.equ PADS_QSPI_GPIO_QSPI_SCLK_SLEWFAST_BITS 0x01
	.equ PADS_QSPI_GPIO_QSPI_SCLK_OFFSET 0x04
	.equ PADS_QSPI_GPIO_QSPI_SD0_OFFSET 0x08
	.equ PADS_QSPI_GPIO_QSPI_SD1_OFFSET 0x0C
	.equ PADS_QSPI_GPIO_QSPI_SD2_OFFSET 0x10
	.equ PADS_QSPI_GPIO_QSPI_SD3_OFFSET 0x14
	.equ PADS_QSPI_GPIO_QSPI_SD0_SCHMITT_BITS 0x02

	@ Mode to send commands
	@ Standard SPI mode (0 << 21)
	@ 8 clocks per data frame (7 << 16)
	@ Tx and Rx (0 << 8)
	.equ CTRLR0_SEND_CMD ((0 << 21) | (7 << 16) | (0 << 8))
	
	@ Quad SPI mode (2 << 21)
	@ 32 clocks per data frame (31 << 16)
	@ Send instruction and address, receive data (3 << 8)
	.equ CTRLR0_INIT_XIP ((2 << 21) | (31 << 16) | (3 << 8))

	@ 4 wait cycles (WAIT_CYCLES << 11)
	@ 8 bit command prefix (2 << 8)
	@ 32 address and mode bits (8 << 2)
	@ Command is SPI, address is QSPI (1 << 0)
	.equ SPI_CTRLR0_INIT_XIP ((WAIT_CYCLES << 11) | (2 << 8) | (8 << 2) | (1 << 0))

	@ Continually send reads
	@ Send CMD_CONT_READ (CMD_CONT_READ << 24)
	@ 4 wait cycles (WAIT_CYCLES << 11)
	@ 0 bit command prefix (0 << 8)
	@ 32 address and mode bits (8 << 2)
	@ Command is QSPI, address is QSPI (2 << 0)
	.equ SPI_CTRLR0_CONT_XIP ((CMD_CONT_READ << 24) | (WAIT_CYCLES << 11) | (0 << 8) | (9 << 2) | (2 << 0))

	.text

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

	ldr r0, =XIP_SSI_BASE

	@@ Disable SSI
	movs r1, #0
	str r1, [r0, #SSI_SSIENR_OFFSET]

	@@ Set the clock divider
	movs r1, #PICO_FLASH_SPI_CLKDIV
	str r1, [r0, #SSI_BAUDR_OFFSET]

	@ Set up sending commands
	ldr r1, =CTRLR0_SEND_CMD
	str r1, [r0, #SSI_CTRLR0_OFFSET]

	@@ Enable SSI
	movs r1, #1
	str r1, [r0, #SSI_SSIENR_OFFSET]

	@@ Read the status register
	movs r1, #CMD_READ_STATUS_2
	str r1, [r0, #SSI_DR0_OFFSET]
	str r1, [r0, #SSI_DR0_OFFSET]
	bl _wait_ssi_busy
	ldr r1, [r0, #SSI_DR0_OFFSET]
	ldr r1, [r0, #SSI_DR0_OFFSET]

	@@ Check for whether QSPI is enabled and skip initialization if so
	movs r2, #QSPI_ENABLE_STATE
	cmp r1, r2
	beq 1f

	@@ Enable writing
	movs r1, #CMD_WRITE_ENABLE
	str r1, [r0, #SSI_DR0_OFFSET]
	bl _wait_ssi_busy
	ldr r1, [r0, #SSI_DR0_OFFSET]

	@@ Enable QSPI
	movs r1, #CMD_WRITE_STATUS
	str r1, [r0, #SSI_DR0_OFFSET]
	movs r3, #0
	str r3, [r0, #SSI_DR0_OFFSET]
	str r2, [r0, #SSI_DR0_OFFSET]
	bl _wait_ssi_busy
	ldr r3, [r0, #SSI_DR0_OFFSET]
	ldr r3, [r0, #SSI_DR0_OFFSET]
	ldr r3, [r0, #SSI_DR0_OFFSET]

	@@ Wait for writing to finish
2:	movs r1, #CMD_READ_STATUS
	str r1, [r0, #SSI_DR0_OFFSET]
	str r1, [r0, #SSI_DR0_OFFSET]
	bl _wait_ssi_busy
	ldr r1, [r0, #SSI_DR0_OFFSET]
	ldr r1, [r0, #SSI_DR0_OFFSET]
	movs r2, #WRITE_BUSY_STATE
	tst r1, r2
	bne 2b

	@@ Actually set up XIP
	@@ First disable the SSI
1:	movs r1, #0
	str r1, [r0, #SSI_SSIENR_OFFSET]
	
	@@ Set up the registers
	ldr r1, =CTRLR0_INIT_XIP
	str r1, [r0, #SSI_CTRLR0_OFFSET]
	movs r1, #0
	str r1, [r0, #SSI_CTRLR1_OFFSET]
	ldr r1, =SPI_CTRLR0_INIT_XIP
	ldr r2, =XIP_SSI_BASE + SSI_SPI_CTRLR0_OFFSET
	str r1, [r2]

	@@ Enable SSI
	movs r1, #1
	str r1, [r0, #SSI_SSIENR_OFFSET]

	@@ Prime XIP
	movs r1, #CMD_READ_DATA_FAST_QUAD_IO
	str r1, [r0, #SSI_DR0_OFFSET]
	movs r1, #CMD_CONT_READ
	str r1, [r0, #SSI_DR0_OFFSET]
	bl _wait_ssi_busy

	@@ Disable SSI
	movs r1, #0
	str r1, [r0, #SSI_SSIENR_OFFSET]

	@@ Set up continuing XIP automatically
	ldr r1, =SPI_CTRLR0_CONT_XIP
	ldr r2, =XIP_SSI_BASE + SSI_SPI_CTRLR0_OFFSET
	str r1, [r2]

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

	@ Wait for SSI to become ready
_wait_ssi_busy:
	push {r0, r1, r2, r3, lr}
	ldr r0, =XIP_SSI_BASE + SSI_SR_OFFSET
	movs r1, #1 @ BUSY
	movs r2, #4 @ TX FIFO empty
1:	ldr r3, [r0]
	tst r3, r2
	beq 1b
	tst r3, r1
	bne 1b
	pop {r0, r1, r2, r3, pc}
