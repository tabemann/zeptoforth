@ Copyright (c) 2013 Matthias Koch
@ Copyright (c) 2020 Travis Bemann
@
@ This program is free software: you can redistribute it and/or modify
@ it under the terms of the GNU General Public License as published by
@ the Free Software Foundation, either version 3 of the License, or
@ (at your option) any later version.
@
@ This program is distributed in the hope that it will be useful,
@ but WITHOUT ANY WARRANTY; without even the implied warranty of
@ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
@ GNU General Public License for more details.
@
@ You should have received a copy of the GNU General Public License
@ along with this program.  If not, see <http://www.gnu.org/licenses/>.

	
	.equ GPIOD_BASE      ,   0x48000C00
	.equ GPIOD_MODER     ,   GPIOD_BASE + 0x00
	.equ GPIOD_OTYPER    ,   GPIOD_BASE + 0x04
	.equ GPIOD_OSPEEDR   ,   GPIOD_BASE + 0x08
	.equ GPIOD_PUPDR     ,   GPIOD_BASE + 0x0C
	.equ GPIOD_IDR       ,   GPIOD_BASE + 0x10
	.equ GPIOD_ODR       ,   GPIOD_BASE + 0x14
	.equ GPIOD_BSRR      ,   GPIOD_BASE + 0x18
	.equ GPIOD_LCKR      ,   GPIOD_BASE + 0x1C
	.equ GPIOD_AFRL      ,   GPIOD_BASE + 0x20
	.equ GPIOD_AFRH      ,   GPIOD_BASE + 0x24
	.equ GPIOD_BRR       ,   GPIOD_BASE + 0x28
	.equ GPIOD_ASCR      ,   GPIOD_BASE + 0x2C
	
	.equ RCC_BASE        ,   0x40021000
	.equ RCC_AHB1ENR     ,   RCC_BASE + 0x48
	.equ RCC_AHB2ENR     ,   RCC_BASE + 0x4C @ gpiod  - b3
	.equ RCC_APB1ENR1    ,   RCC_BASE + 0x58 @ usart2 - b17

        @ stm32l476 discovery board uses pd5, pd6 on usart2
        
        .equ USART2_BASE     ,   0x40004400
        
        .equ USART2_CR1      ,   USART2_BASE + 0x00
        .equ USART2_CR2      ,   USART2_BASE + 0x04
        .equ USART2_CR3      ,   USART2_BASE + 0x08
        .equ USART2_BRR      ,   USART2_BASE + 0x0C
        .equ USART2_GTPR     ,   USART2_BASE + 0x10
        .equ USART2_RTOR     ,   USART2_BASE + 0x14
        .equ USART2_RQR      ,   USART2_BASE + 0x18
        .equ USART2_ISR      ,   USART2_BASE + 0x1C
        .equ USART2_ICR      ,   USART2_BASE + 0x20
        .equ USART2_RDR      ,   USART2_BASE + 0x24
        .equ USART2_TDR      ,   USART2_BASE + 0x28

        @ Flags for USART2_ISR register:
        .equ RXNE            ,   0x20
        .equ TC              ,   0x40
        .equ TXE             ,   0x80

	@@ Initialize UART
	define_word "uart-init", visible_flag
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

	@@ Change USART2 baudrate to 115200 baud for 48 MHz mode
	define_word "serial-115200-48mhz", visible_flag
_serial_115200_48mhz:	
	ldr r0, =USART2_BRR
	ldr r1, =(48000000 + 115200 / 2) / 115200
	str r1, [r0]
	bx  lr

	@@ Emit one character ( c -- )
	define_word "serial-emit", visible_flag
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

	@@ Receive one character ( -- c )
	define_word "serial-key", visible_flag
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

	@@ Test whether a character may be emitted ( -- flag )
	define_word "serial-emit?", visible_flag
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

	@@ Test whether a character is ready be received ( -- flag )
	define_word "serial-key?", visible_flag
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

	.ltorg
	
