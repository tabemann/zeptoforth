@ Copyright (c) 2021 Travis Bemann
@ Copyright (c) 2021 Jan Bramkamp
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

	.equ UART0_BASE, 0x40034000
	.equ UART1_BASE, 0x40038000
	.equ UART_CONSOLE_BASE, UART0_BASE
	.equ UARTDR_OFFSET, 0x00
	.equ UARTRSR_OFFSET, 0x04
	.equ UARTFR_OFFSET, 0x18
	.equ UARTILPR_OFFSET, 0x20
	.equ UARTIBRD_OFFSET, 0x24 @ Set this to the integer baud rate
	.equ UARTFBRD_OFFSET, 0x28 @ Set this to the fractional baud rate
	.equ UARTLCR_H_OFFSET, 0x2C
	.equ UARTCR_OFFSET, 0x30
	.equ UARTIFLS_OFFSET, 0x34
	.equ UARTIMSC_OFFSET, 0x38
	.equ UARTRIS_OFFSET, 0x3C
	.equ UARTMIS_OFFSET, 0x40
	.equ UARTICR_OFFSET, 0x44
	.equ UARTDMACR_OFFSET, 0x48
	.equ UARTFR_TX_FIFO_FULL, 5
	.equ UARTFR_RX_FIFO_EMPTY, 4
	.equ UARTIBRD_VALUE, 67
	.equ UARTFBRD_VALUE, 52
	.equ UARTLCR_H_VALUE, 0x70
	.equ UART_ENABLE, 0x301

	@@ Initialize UART
	define_internal_word "uart-init", visible_flag
_uart_init:
	@@ Configure the console UART
	ldr r0, =UART_CONSOLE_BASE
	movs r1, #UARTIBRD_VALUE
	str r1, [r0, #UARTIBRD_OFFSET]
	movs r1, #UARTFBRD_VALUE
	str r1, [r0, #UARTFBRD_OFFSET]
	movs r1, #UARTLCR_H_VALUE
	str r1, [r0, #UARTLCR_H_OFFSET]
	
	@@ Enable the console UART
	ldr r1, =UART_ENABLE
	str r1, [r0, #UARTCR_OFFSET]

	ldr r0, =IO_BANK0_BASE
	movs r1, #2
	str r1, [r0, #4]
	str r1, [r0, #12]
	
	bx lr
	end_inlined

	@@ Emit one character ( c -- )
	define_internal_word "serial-emit", visible_flag
_serial_emit:
	push {lr}
1:	bl _serial_emit_q
	movs r0, tos
	pull_tos
	cmp r0, #0
	beq 1b
	ldr r0, =UART_CONSOLE_BASE
	strb tos, [r0, #UARTDR_OFFSET]
	pull_tos
	pop {pc}
	end_inlined

	@@ Receive one character ( -- c )
	define_internal_word "serial-key", visible_flag
_serial_key:
	push {lr}
1:	bl _serial_key_q
	movs r0, tos
	pull_tos
	cmp r0, #0
	beq 1b
	push_tos
	ldr r0, =UART_CONSOLE_BASE
	ldrb tos, [r0, #UARTDR_OFFSET]
	pop {pc}
	end_inlined

	@@ Test whether a character may be emitted ( -- flag )
	define_internal_word "serial-emit?", visible_flag
_serial_emit_q:
	push {lr}
	bl _pause
	ldr r0, =UART_CONSOLE_BASE
	ldr r0, [r0, #UARTFR_OFFSET]
	lsls r0, r0, #(31 - UARTFR_TX_FIFO_FULL)
	asrs r0, r0, #31
	push_tos
	mvns tos, r0
	pop {pc}
	end_inlined

	@@ Test whether a character is ready be received ( -- flag )
	define_internal_word "serial-key?", visible_flag
_serial_key_q:
	push {lr}
	bl _pause
	ldr r0, =UART_CONSOLE_BASE
	ldr r0, [r0, #UARTFR_OFFSET]
	lsls r0, r0, #(31 - UARTFR_RX_FIFO_EMPTY)
	asrs r0, r0, #31
	push_tos
	mvns tos, r0
	pop {pc}
	end_inlined
	
	.ltorg
	
