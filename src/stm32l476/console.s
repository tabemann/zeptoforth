@ Copyright (c) 2013 Matthias Koch
@ Copyright (c) 2020 Travis Bemann
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
	
	.equ GPIOD_Base      ,   0x48000C00
	.equ GPIOD_MODER     ,   GPIOD_Base + 0x00
	.equ GPIOD_OTYPER    ,   GPIOD_Base + 0x04
	.equ GPIOD_OSPEEDR   ,   GPIOD_Base + 0x08
	.equ GPIOD_PUPDR     ,   GPIOD_Base + 0x0C
	.equ GPIOD_IDR       ,   GPIOD_Base + 0x10
	.equ GPIOD_ODR       ,   GPIOD_Base + 0x14
	.equ GPIOD_BSRR      ,   GPIOD_Base + 0x18
	.equ GPIOD_LCKR      ,   GPIOD_Base + 0x1C
	.equ GPIOD_AFRL      ,   GPIOD_Base + 0x20
	.equ GPIOD_AFRH      ,   GPIOD_Base + 0x24
	.equ GPIOD_BRR       ,   GPIOD_Base + 0x28
	.equ GPIOD_ASCR      ,   GPIOD_Base + 0x2C
	
	.equ RCC_Base        ,   0x40021000
	.equ RCC_AHB1ENR     ,   RCC_Base + 0x48
	.equ RCC_AHB2ENR     ,   RCC_Base + 0x4C @ gpiod  - b3
	.equ RCC_APB1ENR1    ,   RCC_Base + 0x58 @ usart2 - b17

        @ stm32l476 discovery board uses pd5, pd6 on usart2
        
        .equ USART2_Base     ,   0x40004400
        
        .equ USART2_CR1      ,   USART2_Base + 0x00
        .equ USART2_CR2      ,   USART2_Base + 0x04
        .equ USART2_CR3      ,   USART2_Base + 0x08
        .equ USART2_BRR      ,   USART2_Base + 0x0C
        .equ USART2_GTPR     ,   USART2_Base + 0x10
        .equ USART2_RTOR     ,   USART2_Base + 0x14
        .equ USART2_RQR      ,   USART2_Base + 0x18
        .equ USART2_ISR      ,   USART2_Base + 0x1C
        .equ USART2_ICR      ,   USART2_Base + 0x20
        .equ USART2_RDR      ,   USART2_Base + 0x24
        .equ USART2_TDR      ,   USART2_Base + 0x28

        @ Flags for USART2_ISR register:
        .equ RXNE            ,   0x20
        .equ TC              ,   0x40
        .equ TXE             ,   0x80

	@@ Initialize UART
	define_internal_word "uart-init", visible_flag
_uart_init:
	@@ Enable all GPIO peripheral clocks
	ldr r1, =RCC_AHB2ENR
	ldr r0, =0xFF
	str r0, [r1]
	@@ Enable the USART2 peripher clock
	ldr r1, =RCC_APB1ENR1
	ldr r0, =0x00020000
	str r0, [r1]
	@@ Set PORTD pins 5 and 6 in alternate function mode
	ldr r1, =GPIOD_MODER
	ldr r0, =0xFFFFEBFF
	str r0, [r1]
	@@ Set alternate function 7 to enable USART 2 pins on Port D
	ldr r1, =GPIOD_AFRL
	ldr r0, =0x07700000
	str r0, [r1]
	@@ Configure BRR by dividing the bus clock with the baud rate
	ldr r1, =USART2_BRR
	ldr r0, =(4000000 + (115200/2)) / 115200
	str r0, [r1]
	@@ Disable overrun detection before UE to avoid USART blocking on
	@@ overflow
	ldr r1, =USART2_CR3
	ldr r0, =0x1000
	str r0, [r1]
	@@ Enable the USART, TX, and RX circuit
	ldr r1, =USART2_CR1
	ldr r0, =0x0D
	str r0, [r1]
	bx lr
	end_inlined

	@@ Change USART2 baudrate to 115200 baud for 48 MHz mode
	define_internal_word "serial-115200-48mhz", visible_flag
_serial_115200_48mhz:	
	ldr r0, =USART2_BRR
	ldr r1, =(48000000 + 115200 / 2) / 115200
	str r1, [r0]
	bx  lr
	end_inlined

	@@ Emit one character ( c -- )
	define_internal_word "serial-emit", visible_flag
_serial_emit:
	push {lr}
1:	bl _serial_emit_q
	cmp tos, #0
	pull_tos
	beq 1b
	ldr r2, =USART2_TDR
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
	push_tos
	ldr r2, =USART2_RDR
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
	ldr r0, =USART2_ISR
	ldr r1, [r0]
	movs r0, #TXE
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
	ldr r0, =USART2_ISR
	ldr r1, [r0]
	movs r0, #RXNE
	ands r1, r0
	beq 1f
	movs tos, #-1
1:	pop {pc}
	end_inlined

	.ltorg
	
