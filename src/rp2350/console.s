@ Copyright (c) 2021-2024 Travis Bemann
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

	.equ UARTFR_TX_FIFO_FULL, 5
	.equ UARTFR_RX_FIFO_EMPTY, 4
        .equ CTRL_C, 3
        
_uart_test:
	ldr r0, =UART0_BASE
	movs r2, #0x21
	movs r3, #1 << UARTFR_TX_FIFO_FULL
1:	ldr r1, [r0, #UARTFR]
	tst r1, r3
	bne 1b
	strb r2, [r0, #UARTDR]
	b _uart_test

	@@ Emit one character ( c -- )
	define_internal_word "serial-emit", visible_flag
_serial_emit:
	push {lr}
1:	bl _serial_emit_q
	movs r0, tos
	pull_tos
	cmp r0, #0
	beq 1b
	ldr r0, =UART0_BASE
	strb tos, [r0, #UARTDR]
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
	ldr r0, =UART0_BASE
	ldrb tos, [r0, #UARTDR]
        ldr r0, =uart_special_enabled
        ldr r0, [r0]
        cmp r0, #0
        beq 2f
        cmp tos, #CTRL_C
        bne 2f
        bl _reboot
2:      pop {pc}
	end_inlined

	@@ Test whether a character may be emitted ( -- flag )
	define_internal_word "serial-emit?", visible_flag
_serial_emit_q:
	push {lr}
	bl _pause
	ldr r0, =UART0_BASE
	ldr r0, [r0, #UARTFR]
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

@        @ Debugging LED display
@        ldr r0, =SIO_BASE
@        ldr r1, =1 << 25
@        str r1, [r0, #GPIO_OE_SET]
@        str r1, [r0, #GPIO_OUT_SET]

        ldr r0, =UART0_BASE
	ldr r0, [r0, #UARTFR]
	lsls r0, r0, #(31 - UARTFR_RX_FIFO_EMPTY)
	asrs r0, r0, #31
	push_tos
	mvns tos, r0

@        @ Debugging LED display
@        ldr r0, =SIO_BASE
@        ldr r1, =1 << 25
@        str r1, [r0, #GPIO_OE_SET]
@        str r1, [r0, #GPIO_OUT_CLR]

        movs r0, #-1
        cmp tos, r0
        bne 1f        
1:      pop {pc}
	end_inlined
	
	.ltorg
	
