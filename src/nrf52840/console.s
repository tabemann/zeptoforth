@ Copyright (c) 2013 Matthias Koch
@ Copyright (C) 2018 juju2013@github
@ Copyright (c) 2020-2022 Travis Bemann
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
	
	@--- GPIO related registers
	.equ NRF_GPIO               ,	0x50000000
	.equ NRF_GPIO_PIN_CNF       ,	NRF_GPIO + 0x700
	.equ NRF_GPIO_OUTSET        ,	NRF_GPIO + 0x508
	
	@--- UART related registers
	.equ UART                   , 0x40002000
	.equ UART_TASK_STARTRX      ,	UART + 0x000
	.equ UART_TASK_STOPRX       ,	UART + 0x004
	.equ UART_TASK_STARTTX      ,	UART + 0x008
	.equ UART_TASK_STOPTX       ,	UART + 0x00C
	.equ UART_EVENT_RXDRDY      ,	UART + 0x108
	.equ UART_EVENT_TXDRDY      ,	UART + 0x11C
	.equ UART_ENABLE            ,	UART + 0x500
	.equ UART_PSELTXD           ,	UART + 0x50C
	.equ UART_PSELRXD           ,	UART + 0x514
	.equ UART_RXD               ,	UART + 0x518
	.equ UART_TXD               ,	UART + 0x51C
	.equ UART_BAUDRATE          ,	UART + 0x524
	.equ UART_CONFIG            ,	UART + 0x56C
	
	@--- constants
	.equ BAUD115200             ,	0x01D7E000
	.equ RX_PIN_NUMBER          ,	3
	.equ TX_PIN_NUMBER          ,	2
	
	@@ Initialize UART
	define_internal_word "uart-init", visible_flag
_uart_init:
	@----------------------- setup UART IO PIN ---------------------------
	ldr   r1, =UART_ENABLE
	movs  r0, #0
	str   r0, [r1]                    @ disable UART before configuration
	
	@--- TX PIN
	ldr   r1, =NRF_GPIO_OUTSET
	ldr   r0, =(1<<TX_PIN_NUMBER)
	str   r0, [r1]
	ldr   r1, =NRF_GPIO_PIN_CNF+4*TX_PIN_NUMBER
	movs  r0, #1
	str   r0, [r1]                    @ as output
	
	ldr   r1, =UART_PSELTXD
	movs  r0, #TX_PIN_NUMBER
	str   r0, [r1]                    @ set TX PIN
	
	
	@--- TX PIN
	ldr   r1, =NRF_GPIO_PIN_CNF+4*RX_PIN_NUMBER
	movs  r0, #0xC                    @ PULL UP
	str   r0, [r1]
	
	ldr   r1, =UART_PSELRXD
	movs  r0, #RX_PIN_NUMBER
	str   r0, [r1]                    @ set RX PIN
	
	@--- BAUD rate  
	ldr r1, =UART_BAUDRATE
	ldr r0, =BAUD115200
	str r0, [r1]
	
	@--- Enable UART
	ldr   r1, =UART_ENABLE
	movs  r0, #4
	str   r0, [r1]
	
	@--- start TX and RX tasks  
	movs  r0, #1
	ldr   r1, =UART_TASK_STARTTX
	str   r0, [r1]
	ldr   r1, =UART_TASK_STARTRX
	str   r0, [r1]
	
	@--- clear RX flag
	movs  r0, #0
	ldr   r1, =UART_EVENT_RXDRDY
	str   r0, [r1]  
	@NRF_UART0->EVENTS_RXDRDY    = 0;
	
	@--- DEBUG
	ldr   r1, =UART_TXD
	
	movs  r0, #68               @ D
	str r0, [r1]
	movs  r0, #69               @ E
	str r0, [r1]
	movs  r0, #66               @ B
	str r0, [r1]
	movs  r0, #85               @ U
	str r0, [r1]
	movs  r0, #71               @ G
	str r0, [r1]
	
	bx lr
	end_inlined


	@@ Emit one character ( c -- )
	define_internal_word "serial-emit", visible_flag
_serial_emit:
	push {lr}
1:	bl _serial_emit_q
	cmp tos, #0
	pull_tos
	beq 1b
	ldr r2, =UART_EVENT_TXDRDY
	movs r0, #0
	str r0, [r2]
	ldr r2, =UART_TXD
	strb tos, [r2]         @ Output the character
	pull_tos
	pop {pc}
	end_inlined

	@@ Receive one character ( -- c )
	define_internal_word "serial-key", visible_flag
_serial_key:
	push {lr}
1:	bl _serial_key_q
	cmp tos, #0
	pull_tos
	beq 1b
	ldr r2, =UART_EVENT_RXDRDY
	movs r0, #0
	str r0, [r2]
	push_tos
	ldr r2, =UART_RXD
	ldrb tos, [r2]
	pop {pc}
	end_inlined

	@@ Test whether a character may be emitted ( -- flag )
	define_internal_word "serial-emit?", visible_flag
_serial_emit_q:
	push {lr}
	bl _pause
	push_tos
	movs tos, #0
	ldr r0, =UART_EVENT_TXDRDY
	ldr r1, [r0]
	movs r0, #1
	ands r1, r0
	beq 1f
	movs tos, #-1
1:	pop {pc}
	end_inlined

	@@ Test whether a character is ready be received ( -- flag )
	define_internal_word "serial-key?", visible_flag
_serial_key_q:
	push {lr}
	bl _pause
	push_tos
	movs tos, #0
	ldr r0, =UART_EVENT_RXDRDY
	ldr r1, [r0]
	movs r0, #RXNE
	ands r1, r0
	beq 1f
	movs tos, #-1
1:	pop {pc}
	end_inlined


	@@ Time multiplier
	define_internal_word "time-multiplier", visible_flag
_time_multiplier:
	push_tos
	movs tos, #1
	bx lr
	end_inlined

	@@ Time divisor
	define_internal_word "time-divisor", visible_flag
_time_divisor:
	push_tos
	movs tos, #1
	bx lr
	end_inlined

	@@ Divisor to get ms from systicks
	define_internal_word "systick-divisor", visible_flag
_systick_divisor:
	push_tos
	movs tos, #10
	bx lr
	end_inlined
	
	.ltorg
	
