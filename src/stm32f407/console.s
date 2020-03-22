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

	
	.equ GPIOA_BASE      ,   0x40020000
	.equ GPIOA_MODER     ,   GPIOA_BASE + 0x00
	.equ GPIOA_OTYPER    ,   GPIOA_BASE + 0x04
	.equ GPIOA_OSPEEDR   ,   GPIOA_BASE + 0x08
	.equ GPIOA_PUPDR     ,   GPIOA_BASE + 0x0C
	.equ GPIOA_IDR       ,   GPIOA_BASE + 0x10
	.equ GPIOA_ODR       ,   GPIOA_BASE + 0x14
	.equ GPIOA_BSRR      ,   GPIOA_BASE + 0x18
	.equ GPIOA_LCKR      ,   GPIOA_BASE + 0x1C
	.equ GPIOA_AFRL      ,   GPIOA_BASE + 0x20
	.equ GPIOA_AFRH      ,   GPIOA_BASE + 0x24
	
	.equ RCC_BASE        ,   0x40023800
	.equ RCC_CR          ,   RCC_BASE + 0x00
	.equ RCC_CFGR        ,   RCC_BASE + 0x08
	.equ RCC_AHB1ENR     ,   RCC_BASE + 0x30
	.equ RCC_APB1ENR     ,   RCC_BASE + 0x40
	
	.equ HSERDY          ,   0x020000
	.equ HSEON           ,   0x010000
        
        .equ USART2_BASE     ,   0x40004400

	.equ CONSOLE_BASE    ,   USART2_BASE
        
	.equ CONSOLE_SR      ,   CONSOLE_BASE + 0x00
	.equ CONSOLE_DR      ,   CONSOLE_BASE + 0x04
	.equ CONSOLE_BRR     ,   CONSOLE_BASE + 0x08
	.equ CONSOLE_CR1     ,   CONSOLE_BASE + 0x0c
	.equ CONSOLE_CR2     ,   CONSOLE_BASE + 0x10
	.equ CONSOLE_CR3     ,   CONSOLE_BASE + 0x14
	.equ CONSOLE_GTPR    ,   CONSOLE_BASE + 0x18

        .equ RXNE            ,   0x20
        .equ TC              ,   0x40
        .equ TXE             ,   0x80

	@@ Initialize UART
	define_word "uart-init", visible_flag
_uart_init:
        ldr r1, = RCC_CR
        mov r0, HSEON
        str r0, [r1]            @ turn on the external clock
1:	awaitHSE:
        ldr r0, [r1]
        ands r0, #HSERDY
        beq 1b            @ hang here until external clock is stable

        @ at this point, the HSE is running and stable but I suppose we have not yet
        @ switched Sysclk to use it.

        ldr r1, = RCC_CFGR
        mov r0, # 1
        str r0, [r1]            @ switch to the external clock
        
        @ Turn off the HSION bit
        ldr r1, = RCC_CR
        ldr r0, [r1]
        and r0, 0xFFFFFFFE      @ Zero the 0th bit
        str r0, [r1]

        @ Enable the CCM RAM and all GPIO peripheral clock
        ldr r1, = RCC_AHB1ENR
        ldr r0, = 0x1001FF
        str r0, [r1]

        @ Set PORTA pins in alternate function mode
        ldr r1, = GPIOA_MODER
        ldr r0, [r1]
        orrs r0, #0xA0
        str r0, [r1]

        @ Set alternate function 7 to enable CONSOLE pins on Port A
        ldr r1, = GPIOA_AFRL
        ldr r0, = 0x7700              @ Alternate function 7 for TX and RX pins of CONSOLE on PORTA 
        str r0, [r1]

        @ Enable the CONSOLE peripheral clock by setting bit 17
        ldr r1, = RCC_APB1ENR
        ldr r0, = 0x20000
        str r0, [r1]

	@ Configure BRR by deviding the bus clock with the baud rate
	
	ldr r1, =CONSOLE_BRR
	@ ldr r0, =0x341  @  9600 bps
	@ movs r0, #0xD0  @ 38400 bps
	@ movs r0, #0x45  @ 115200 bps
	movs r0, #0x46  @ 115200 bps, ein ganz kleines bisschen langsamer...
	str r0, [r1]
	
	@ Enable the USART, TX, and RX circuit
	ldr r1, =CONSOLE_CR1
	ldr r0, =0x200C @ USART_CR1_UE | USART_CR1_TE | USART_CR1_RE
	str r0, [r1]

	bx lr

	@@ Emit one character ( c -- )
	define_word "serial-emit", visible_flag
_serial_emit:
	push {lr}
1:	bl _serial_emit_q
	cmp tos, #0
	pull_tos
	beq 1b
	ldr r2, =CONSOLE_DR
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
	ldr r2, =CONSOLE_DR
	ldrb tos, [r2]
	pop {pc}

	@@ Test whether a character may be emitted ( -- flag )
	define_word "serial-emit?", visible_flag
_serial_emit_q:
	push {lr}
	bl _pause
	push_tos
	movs tos, #0
	ldr r0, =CONSOLE_SR
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
	ldr r0, =CONSOLE_SR
	ldr r1, [r0]
	movs r0, #RXNE
	ands r1, r0
	beq 1f
	movs tos, #-1
1:	pop {pc}

	.ltorg
	
